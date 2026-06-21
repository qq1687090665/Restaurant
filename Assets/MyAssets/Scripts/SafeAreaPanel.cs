using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaPanel : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;
    private Vector2 lastScreenSize = Vector2.zero;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Refresh();
    }

    void Update()
    {
        // 业内标准优化：只有当屏幕尺寸、安全区、或方向发生改变时才刷新，严禁每帧盲目计算
        if (lastSafeArea != Screen.safeArea || 
            lastScreenSize.x != Screen.width || 
            lastScreenSize.y != Screen.height)
        {
            Refresh();
        }
    }

    void Refresh()
    {
        lastSafeArea = Screen.safeArea;
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        if (Screen.width == 0 || Screen.height == 0) return;

        // 1. 获取安全区域的像素边界坐标
        Vector2 anchorMin = lastSafeArea.position;
        Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;

        // 2. 将绝对像素坐标，转换为 UGUI 顶层所需的 0~1 的归一化比例（Anchor 坐标）
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // 3. 安全钳制：防止某些安卓模拟器返回越界数据导致 UI 崩溃
        if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x <= 1 && anchorMax.y <= 1)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}