using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class AutoHotUpdateBuilder
{
    // 基础配置：热更资源存放的目标根目录
    private static readonly string TargetRootFolder = "Assets/MyAssets/HotUpdate/CodeAsset";

    [MenuItem("Tools/热更新/一键编译并热更全部DLL")]
    public static void CompileAndCopyAllDLLs()
    {
        // 1. 获取当前激活平台
        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"<color=yellow>[AutoBuilder]</color> 开始编译目标平台 <color=cyan>{activeTarget}</color> 的代码...");

        // 2. 执行 HybridCLR 编译
        HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(activeTarget);

        // 3. 计算路径
        string projectRoot = Path.Combine(Application.dataPath, "..");
        string hotUpdateDllSrcDir = Path.Combine(projectRoot, "HybridCLRData", "HotUpdateDlls", activeTarget.ToString());

        // 安全检查与目录创建（合规的 Unity 做法）
        CheckAndCreateFolder(TargetRootFolder);

        int copyCount = 0;

        // 4. 【核心优化】：动态获取 HybridCLR 配置中所有的热更新 DLL 名称，杜绝硬编码
        // 注：不同版本 HybridCLR 的 API 稍有差异，以下为较新版本的标准获取方式
        List<string> hotUpdateAssemblies = HybridCLR.Editor.SettingsUtil.AOTAssemblyNames;

        foreach (var dllName in hotUpdateAssemblies)
        {
            string sourcePath = Path.Combine(hotUpdateDllSrcDir, dllName);
            string targetPath = Path.Combine(TargetRootFolder, dllName + ".bytes"); // 自动加 .bytes 后缀

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, targetPath, true);
                Debug.Log($"<color=white>[AutoBuilder]</color> 已同步热更程序集: <color=green>{dllName}.bytes</color>");
                copyCount++;
            }
            else
            {
                Debug.LogError($"<color=red>[AutoBuilder]</color> 未在产物目录找到配置的 Dll: {sourcePath}");
            }
        }

        // 5. 刷新资源数据库
        if (copyCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>[AutoBuilder] 大成功！</color> 共成功同步 {copyCount} 个热更 Dll，YooAsset 已可识别！");
        }
    }

    [MenuItem("Tools/热更新/收集Shader变体并配置到GraphicsSettings")]
    public static void CollectShaderVariantsAndSetup()
    {
        const string packageName = "DefaultPackage";
        string savePath = ShaderVariantCollectorSetting.GeFileSavePath(packageName);
        int processCapacity = ShaderVariantCollectorSetting.GeProcessCapacity(packageName);

        Debug.Log($"<color=yellow>[ShaderVariant]</color> 开始收集 <color=cyan>{packageName}</color> 包的 Shader 变体...");
        Debug.Log($"<color=yellow>[ShaderVariant]</color> 保存路径: {savePath}");

        ShaderVariantCollector.Run(savePath, packageName, processCapacity, () =>
        {
            // 收集完成后，自动添加到 GraphicsSettings 的 Preloaded Shaders
            AddSVCToPreloadedShaders(savePath);
        });
    }

    /// <summary>
    /// 将 ShaderVariantCollection 添加到 GraphicsSettings 的 Preloaded Shaders 列表
    /// </summary>
    private static void AddSVCToPreloadedShaders(string svcPath)
    {
        ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(svcPath);
        if (svc == null)
        {
            Debug.LogError($"<color=red>[ShaderVariant]</color> 未找到 SVC 文件: {svcPath}");
            return;
        }

        // 通过 SerializedObject 修改 GraphicsSettings 的 m_PreloadedShaders
        var graphicsSettingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettingsAssets == null || graphicsSettingsAssets.Length == 0)
        {
            Debug.LogError("<color=red>[ShaderVariant]</color> 找不到 GraphicsSettings.asset");
            return;
        }

        var graphicsSettingsObj = graphicsSettingsAssets[0];
        using (var so = new SerializedObject(graphicsSettingsObj))
        {
            var preloadedShadersProp = so.FindProperty("m_PreloadedShaders");
            if (preloadedShadersProp == null)
            {
                Debug.LogError("<color=red>[ShaderVariant]</color> 找不到 m_PreloadedShaders 属性");
                return;
            }

            // 检查是否已经存在
            for (int i = 0; i < preloadedShadersProp.arraySize; i++)
            {
                var element = preloadedShadersProp.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == svc)
                {
                    Debug.Log($"<color=green>[ShaderVariant]</color> {svc.name} 已经在 Preloaded Shaders 列表中，跳过添加。");
                    return;
                }
            }

            // 添加新项
            int index = preloadedShadersProp.arraySize;
            preloadedShadersProp.InsertArrayElementAtIndex(index);
            var newElement = preloadedShadersProp.GetArrayElementAtIndex(index);
            newElement.objectReferenceValue = svc;

            so.ApplyModifiedProperties();
            Debug.Log($"<color=green>[ShaderVariant]</color> 已将 {svc.name} 添加到 Preloaded Shaders！");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[ShaderVariant] 大成功！</color> Shader 变体已收集并已配置到 Graphics Settings。现在重新运行场景应该不会再变紫了！");
    }

    /// <summary>
    /// 确保 Assets 目录下的文件夹安全创建，防止 Meta 文件丢失
    /// </summary>
    private static void CheckAndCreateFolder(string assetPath)
    {
        if (Directory.Exists(assetPath)) return;

        string[] folders = assetPath.Split('/');
        string currentPath = folders[0]; // 应该是 "Assets"

        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = nextPath;
        }
    }
}