using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BuyWaiterPanel : MonoBehaviour
{
    [Header("自己的子控件")]
    public Text buyWaiterAmount;
    public Button buyButton;

    [Header("动画")]
    private RectTransform rectTransform;
    private Vector2 targetPos;
    public Vector2 offset;

    private WaiterSpawner currentSpawner;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    /// <summary> 显示面板并绑定到指定 WaiterSpawner </summary>
    public void Show(WaiterSpawner spawner)
    {
        // 绑定数据源
        currentSpawner = spawner;
        currentSpawner.OnDataChanged += RefreshUI;

        // 显示 + 滑入动画
        gameObject.SetActive(true);
        rectTransform.anchoredPosition = targetPos + offset;
        rectTransform.DOAnchorPos(targetPos, 0.3f);

        RefreshUI();
    }

    /// <summary> 隐藏面板，解绑数据 </summary>
    public void Hide()
    {
        // 解绑
        if (currentSpawner != null)
        {
            currentSpawner.OnDataChanged -= RefreshUI;
            currentSpawner = null;
        }

        // 滑出动画，完成后 SetActive(false)
        rectTransform.DOAnchorPos(targetPos + offset, 0.3f)
            .OnComplete(() => gameObject.SetActive(false));
    }

    /// <summary> 刷新金额文本和按钮状态 </summary>
    public void RefreshUI()
    {
        if (currentSpawner == null) return;

        buyWaiterAmount.text = currentSpawner.buyAmount.ToString();

        buyButton.interactable = GameManager.Instance != null && GameManager.Instance.collectedMoney >= currentSpawner.buyAmount;
    }

    /// <summary> 购买按钮点击回调（已在 Awake 中通过 AddListener 绑定）</summary>
    public void OnBuyButtonClick()
    {
        if (currentSpawner != null)
            currentSpawner.SpawnWaiter();
    }
}
