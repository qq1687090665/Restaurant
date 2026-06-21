using UnityEngine;
using UnityEngine.InputSystem;

public class WaiterSpawner : MonoBehaviour
{
    public GameObject waiterPrefab;
    public Transform spawnPos;
    public FoodSpawner _FoodSpawner;
    public int srNo;
    private int waiterIncreasePurchaseAmount = 50;
    public int buyAmount = 50;
    private int waitersCount = 0, tempCount = 0;

    [HideInInspector] public bool IsPlayerNearby;

    /// <summary> 数据变化时触发，供 UI 面板订阅刷新 </summary>
    [HideInInspector] public System.Action OnDataChanged;

    private Controls controls;

    private void Start()
    {
        buyAmount = PlayerPrefs.GetInt(srNo + "WaiterSpawnerBuyAmount", buyAmount);
        waitersCount = PlayerPrefs.GetInt(srNo + "WaitersCount", waitersCount);

        if (tempCount < waitersCount)
        {
            InvokeRepeating("Spawn", .5f, .5f);
        }

        controls = new Controls();
        controls.Enable();
        controls.Player.Interact.performed += OnInteract;
    }

    /// <summary> 购买服务员（由购买按钮或按键触发）</summary>
    public void SpawnWaiter()
    {
        if (GameManager.Instance.collectedMoney >= buyAmount)
        {
            AudioManager.Instance.Play("WaiterSpawned");
            Spawn();

            GameManager.Instance.collectedMoney -= buyAmount;
            GameManager.Instance.ShowAndSave();

            buyAmount += waiterIncreasePurchaseAmount;
            PlayerPrefs.SetInt(srNo + "WaiterSpawnerBuyAmount", buyAmount);

            waitersCount++;
            PlayerPrefs.SetInt(srNo + "WaitersCount", waitersCount);

            OnDataChanged?.Invoke();
        }
    }

    private void Spawn()
    {
        tempCount++;

        if (tempCount == waitersCount)
            CancelInvoke("Spawn");

        GameObject waiter = Instantiate(waiterPrefab, spawnPos.position, spawnPos.rotation);
        waiter.GetComponent<Waiter>()._FoodSpawner = _FoodSpawner;
        waiter.transform.parent = transform.parent.transform;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (IsPlayerNearby)
            SpawnWaiter();
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }
}
