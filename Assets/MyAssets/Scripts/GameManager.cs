using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector]
    public int collectedMoney;
    public GameObject pcPrefab;
    public GameObject androidPrefab;

    private GameObject hudInstance;
    void Awake()
    {
        Instance = this;

#if UNITY_ANDROID || UNITY_IOS
        hudInstance = Instantiate(androidPrefab);
#else
        hudInstance = Instantiate(pcPrefab);
#endif
    }

    private void Start()
    {
        collectedMoney = PlayerPrefs.GetInt("MoneyAmount", 0);
        CanvasUiManager.Instance.SetMoneyText(collectedMoney);
    }

    public void AddMoney(int amount)
    {
        collectedMoney += amount;
        ShowAndSave();
    }

    public void LessMoney()
    {
        collectedMoney--;
        ShowAndSave();
    }

    public void ShowAndSave()
    {
        CanvasUiManager.Instance.SetMoneyText(collectedMoney);
        PlayerPrefs.SetInt("MoneyAmount", collectedMoney);
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        ReloadScene();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void AddFiveHundred()
    {
        AddMoney(500);
        Debug.Log("Added 500 money. Current money: " + collectedMoney);
    }
}
