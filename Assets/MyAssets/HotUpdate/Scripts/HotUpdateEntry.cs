using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YooAsset;

public static class HotUpdateEntry
{
    private static SceneHandle _currentSceneHandle; // 💡 业内常备：保留句柄防止内存泄漏

    public static void StartGame(Slider progressBar, TMP_Text progressText)
    {
        Debug.Log("======== 进入热更业务生命周期 ========");
        // 开启协程加载场景 (创建临时 MonoBehaviour 驱动协程)
        var runnerGo = new GameObject("HotUpdateCoroutineRunner");
        Object.DontDestroyOnLoad(runnerGo);
        var runner = runnerGo.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(LoadGameScene(progressBar, progressText, runnerGo));
    }

    /// <summary>
    /// 临时协程驱动 MonoBehaviour（用于静态类中启动协程）
    /// </summary>
    private class CoroutineRunner : MonoBehaviour { }

    private static IEnumerator LoadGameScene(Slider progressBar, TMP_Text progressText, GameObject runnerGo)
    {
        var package = YooAssets.GetPackage("DefaultPackage");

        // 🚨 避坑核心：如果你的 AddressRule 是 AddressByAssetPath，请在这里写全路径！
        // 示例："Assets/MyAssets/HotUpdate/Scenes/Scene_Game.unity"
        string sceneLocation = "Assets/MyAssets/HotUpdate/Scenes/Scene_Game";

        Debug.Log($"正在异步加载热更场景: {sceneLocation}");
        _currentSceneHandle = package.LoadSceneAsync(sceneLocation, UnityEngine.SceneManagement.LoadSceneMode.Single);

        while (!_currentSceneHandle.IsDone)
        {
            // 在旧场景被销毁前，安全刷新 UI
            if (progressBar != null) progressBar.value = _currentSceneHandle.Progress;
            if (progressText != null) progressText.text = $"正在进入游戏世界... {(_currentSceneHandle.Progress * 100):F0}%";
            yield return null;
        }

        // 清理临时协程驱动器
        if (runnerGo != null) Object.Destroy(runnerGo);

        if (_currentSceneHandle.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"热更场景加载失败: {_currentSceneHandle.LastError}");
            yield break;
        }

        Debug.Log("======== 新场景加载完毕，开始初始化业务组件 ========");

        // 💡 此时新场景已加载，热更包内的类型已完全解冻，可以直接正常调用和查找了
        PlayerSkinController skinController = Object.FindObjectOfType<PlayerSkinController>();
        if (skinController != null)
        {
            skinController.LoadSkin();
        }
        else
        {
            Debug.LogWarning("未在新场景中找到 PlayerSkinController，请检查预制体挂载。");
        }
    }

    // 💡 留给未来切换场景时调用，防止长线运行内存暴涨
    public static void UnloadCurrentScene()
    {
        _currentSceneHandle?.UnloadAsync();
    }
}