using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("配置")]
    public float spawnInterval = 1f; // 初始填满时的间隔

    [Header("引用")]
    public GameObject customerPrefab;
    public Transform spawnPos;
    public Transform exitTransform;
    public Sprite[] orderedFoodSprites;
    public string[] foodNames;

    private Table[] tables;

    void Start()
    {
        tables = transform.parent.GetComponentsInChildren<Table>();
        StartCoroutine(FillTablesRoutine());
    }

    /// <summary> 初始填满所有空桌子，每 spawnInterval 秒一个 </summary>
    private IEnumerator FillTablesRoutine()
    {
        while (true)
        {
            if (TrySpawnCustomer())
                yield return new WaitForSeconds(spawnInterval);
            else
                yield break; // 桌子满了，停止
        }
    }

    /// <summary> 尝试生成一个顾客，没有空桌子则返回 false </summary>
    public void SpawnCustomer()
    {
        TrySpawnCustomer();
        // 兼容旧调用方（Customer.OnDestroy → LeaveRestaurant 已改调 SpawnCustomer）
    }

    private bool TrySpawnCustomer()
    {
        if (!HasFreeTable()) return false;

        // Customer.Start 里自己会 FindAvailableTable + 设 isFull，
        // 这里只做闸门判断，不替他预占
        GameObject customer = Instantiate(customerPrefab, spawnPos.position, spawnPos.rotation, transform.parent);
        customer.GetComponent<Customer>().exitTransform = exitTransform;
        return true;
    }

    private bool HasFreeTable()
    {
        foreach (Table table in tables)
            if (!table.isFull)
                return true;
        return false;
    }
}
