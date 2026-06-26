using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasUiManager : MonoBehaviour
{
    public static CanvasUiManager Instance { get; private set; }

    [Header("金钱")]
    public Text collectedMoney;
    public Transform moneyUiTarget;

    [Header("按钮")]
    public Button resetButton;
    public Button reloadButton;
    public Button addMoneyButton;
    public Button soundToggleButton;
    public Image audioBtnImage;
    public Sprite audioOn, audioOff;

    [Header("面板")]
    public BuyWaiterPanel buyWaiterPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        resetButton.onClick.AddListener(() => GameManager.Instance.ResetGame());
        reloadButton.onClick.AddListener(() => GameManager.Instance.ReloadScene());
        addMoneyButton.onClick.AddListener(() => GameManager.Instance.AddFiveHundred());
        soundToggleButton.onClick.AddListener(OnSoundToggle);

        // 初始化声音图标
        RefreshSoundIcon();
    }

    private void OnSoundToggle()
    {
        AudioManager.Instance.ToggleMute();
        RefreshSoundIcon();
    }

    private void RefreshSoundIcon()
    {
        audioBtnImage.sprite = AudioManager.Instance.IsMuted ? audioOff : audioOn;
    }

    public void SetMoneyText(int amount)
    {
        collectedMoney.text = "$" + amount.ToString();
    }

    public void ShowBuyWaiterPanel(WaiterSpawner spawner)
    {
        buyWaiterPanel.Show(spawner);
    }

    public void HideBuyWaiterPanel()
    {
        buyWaiterPanel.Hide();
    }
}
