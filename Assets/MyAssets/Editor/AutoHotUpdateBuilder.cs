using System.IO;
using UnityEditor;
using UnityEngine;

public static class AutoHotUpdateBuilder
{
    //【核心配置】：请确保这里填的是你项目中放置 .bytes 资源的真实文件夹路径
    private static readonly string TargetFolder = "Assets/MyAssets/HotUpdate/CodeAsset";
    private static readonly string DllName = "HotUpdate.dll";
    private static readonly string BytesName = "HotUpdate.dll.bytes";

    // 特性标签：在 Unity 顶部菜单栏生成 [Tools -> 热更新 -> 一键编译并热更DLL] 按钮
    [MenuItem("Tools/热更新/一键编译并热更DLL")]
    public static void CompileAndCopyDLL()
    {
        // 1. 自动获取当前 Unity Build Settings 激活的平台 (如 Android 或 Windows)
        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"<color=yellow>[AutoBuilder]</color> 开始编译目标平台 <color=cyan>{activeTarget}</color> 的热更 DLL...");

        // 2. 核心：通过代码触发 HybridCLR 的原生的 CompileDll 命令
        HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(activeTarget);

        // 3. 计算源 DLL 的绝对物理路径
        // 相当于：项目根目录/HybridCLRData/HotUpdateDlls/平台名/HotUpdate.dll
        string projectRoot = Path.Combine(Application.dataPath, "..");
        string sourceDir = Path.Combine(projectRoot, "HybridCLRData", "HotUpdateDlls", activeTarget.ToString());
        string sourcePath = Path.Combine(sourceDir, DllName);
        string targetPath = Path.Combine(TargetFolder, BytesName);

        // 4. 安全检查：如果目标文件夹不存在，自动创建它
        if (!Directory.Exists(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
        }

        // 5. 核心：执行物理文件复制与强行覆写
        if (File.Exists(sourcePath))
        {
            // 第三个参数为 true 表示如果文件存在则直接覆盖
            File.Copy(sourcePath, targetPath, true);
            Debug.Log($"<color=green>[AutoBuilder] 成功！</color> 已将最新 DLL 复制并重命名至: {targetPath}");

            // 6. 极其重要：强行刷新 Unity 资源数据库
            AssetDatabase.Refresh();
            Debug.Log("<color=green>[AutoBuilder]</color> 资源数据库已刷新，YooAsset 已可识别最新文件！");
        }
        else
        {
            Debug.LogError($"<color=red>[AutoBuilder] 失败！</color> 未在路径找到编译产物，请检查 HybridCLR 菜单是否正常: {sourcePath}");
        }
    }
}