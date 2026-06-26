using System.Collections;
using UnityEngine;
using YooAsset;

public class PlayerSkinController : MonoBehaviour
{
    [Header("绑定 Player 的渲染器")]
    public SkinnedMeshRenderer PlayerRenderer;

    private AssetHandle _skinHandle;

    // 由 YooAssetBoot 的 EnterGame() 触发，或者等待初始化完毕后调用
    public void LoadSkin()
    {
        StartCoroutine(LoadSkinCO());
    }

    private IEnumerator LoadSkinCO()
    {
        // 工业界标准：使用 AddressByFileName 配置后，直接通过字符串寻址
        _skinHandle = YooAssets.LoadAssetAsync<Material>("PlayerSkin");
        yield return _skinHandle;

        if (_skinHandle.Status == EOperationStatus.Succeed)
            {
            Material mat = _skinHandle.AssetObject as Material;
            PlayerRenderer.material = mat;
            Debug.Log("皮肤 Material 加载成功！");
        }
        else
            {
            Debug.LogError($"皮肤加载失败");
        }
    }

    private void OnDestroy()
    {
        // 【面试加分项】工业界极端注重内存管理，句柄使用完或物体销毁时必须释放，否则导致内存泄漏
        if (_skinHandle != null)
            {
            _skinHandle.Release();
            Debug.Log("释放皮肤资源句柄");
        }
    }
}