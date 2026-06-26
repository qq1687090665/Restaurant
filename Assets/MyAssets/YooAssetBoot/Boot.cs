using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class Boot : MonoBehaviour
{
    [Header("运行模式")]
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    [Header("本地服务器地址")]
    public string BaseServerUrl = "http://localhost:3939";

    private const string PackageName = "DefaultPackage";

    [Header("UI 引用")]
    public Slider ProgressBar;
    public TMP_Text ProgressText;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private IEnumerator Start()
    {
        // 1. 初始化资源系统
        YooAssets.Initialize();
        var package = YooAssets.CreatePackage(PackageName);
        YooAssets.SetDefaultPackage(package);

        // 2. 根据模式配置初始化参数
        InitializationOperation initOperation = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(PackageName);
            var packageRoot = simulateBuildResult.PackageRootDirectory;
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initOperation = package.InitializeAsync(initParameters);
        }
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initOperation = package.InitializeAsync(initParameters);
        }
        else if (PlayMode == EPlayMode.HostPlayMode)
        {
            BaseServerUrl = GetHostServerURL();
            IRemoteServices remoteServices = new CustomRemoteServices(BaseServerUrl);
            var initParameters = new HostPlayModeParameters();
            initParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initOperation = package.InitializeAsync(initParameters);
        }

        yield return initOperation;
        if (initOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"YooAsset 初始化失败: {initOperation.Error}");
            yield break;
        }

        // 3. 获取最新的资源版本号
        var requestVersionOp = package.RequestPackageVersionAsync();
        yield return requestVersionOp;
        if (requestVersionOp.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"获取资源版本失败: {requestVersionOp.Error}");
            yield break;
        }

        // 4. 更新资源清单
        var updateManifestOp = package.UpdatePackageManifestAsync(requestVersionOp.PackageVersion);
        yield return updateManifestOp;
        if (updateManifestOp.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"更新资源清单失败: {updateManifestOp.Error}");
            yield break;
        }

        // 5. 创建下载器并下载资源
        yield return DownloadAssets(package);

        // 6. 核心：加载热更程序集
        yield return LoadHotUpdateAssembly();
    }

    private IEnumerator DownloadAssets(ResourcePackage package)
    {
        var downloader = package.CreateResourceDownloader(10, 3);
        if (downloader.TotalDownloadCount == 0) yield break;

        downloader.BeginDownload();
        while (!downloader.IsDone)
        {
            if (ProgressBar != null) ProgressBar.value = downloader.Progress;
            if (ProgressText != null) ProgressText.text = $"正在下载热更资源... {(downloader.Progress * 100):F0}%";
            yield return null;
        }

        if (downloader.Status != EOperationStatus.Succeed)
        {
            Debug.LogError("资源下载失败！");
            yield break;
        }
    }

    private IEnumerator LoadHotUpdateAssembly()
    {
        var package = YooAssets.GetPackage(PackageName);
        AssetHandle handle = package.LoadAssetAsync<TextAsset>("HotUpdate.dll");
        yield return handle;

        if (handle.Status != EOperationStatus.Succeed)
        {
            Debug.LogError("加载热更 DLL 资源失败！");
            yield break;
        }

        TextAsset dllAsset = handle.AssetObject as TextAsset;
        byte[] dllBytes = dllAsset.bytes;
        Assembly hotUpdateAss = Assembly.Load(dllBytes);
        Debug.Log("======== HybridCLR 代码热更成功载入内存！ ========");
        handle.Release();

        // ==================== 自动化白盒诊断开始 ====================
        Debug.LogWarning($"<color=yellow>[诊断]</color> 成功载入的程序集全称: {hotUpdateAss.FullName}");
        
        System.Type[] allTypes = null;
        try
        {
            allTypes = hotUpdateAss.GetTypes();
            Debug.LogWarning($"<color=yellow>[诊断]</color> 该 DLL 内目前一共包含 {allTypes.Length} 个类。");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[诊断] 读取类列表失败，可能存在依赖缺失: {ex.Message}");
        }

        if (allTypes != null)
        {
            foreach (var t in allTypes)
            {
                // 打印出每一个类在底层的真实全称
                Debug.LogWarning($"<color=white>   -> 发现类: </color> <color=cyan>\"{t.FullName}\"</color> (Namespace: \"{t.Namespace}\" | Name: \"{t.Name}\")");
            }
        }
        // ==================== 自动化白盒诊断结束 ====================

        // 诊断完毕后，再执行原本的获取逻辑
        Type entryType = hotUpdateAss.GetType("HotUpdateEntry");
        if (entryType != null)
        {
            MethodInfo method = entryType.GetMethod("StartGame", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, new object[] { ProgressBar, ProgressText });
            }
            else
            {
                Debug.LogError("致命错误：在 HotUpdateEntry 中找到了类，但没找到 [public static void StartGame] 方法！");
            }
        }
        else
        {
            Debug.LogError("致命错误：未在热更程序集中找到 HotUpdateEntry 类！");
        }
    }

    private string GetHostServerURL()
    {
        string hostServerIP = "http://localhost:3939";
#if      UNITY_STANDALONE_WIN
        return $"{hostServerIP}/cdn/PC";
#elif UNITY_ANDROID
        return $"{hostServerIP}/cdn/Android";
#elif UNITY_IOS
        return $"{hostServerIP}/cdn/iOS";
#else
        return $"{hostServerIP}/cdn/Default";
#endif
    }
}

/// <summary>
/// 远程服务实现类，用于联机模式寻址
/// </summary>
public class CustomRemoteServices : IRemoteServices
{
    private readonly string _baseUrl;

    public CustomRemoteServices(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public string GetRemoteMainURL(string fileName)
    {
        return $"{_baseUrl}/{fileName}";
    }

    public string GetRemoteFallbackURL(string fileName)
    {
        return $"{_baseUrl}/{fileName}";
    }
}