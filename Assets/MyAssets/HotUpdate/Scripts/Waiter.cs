using UnityEngine;
using UnityEngine.AI;

public enum WaiterState
{
    None,
    Idle,            // 空闲，定时重试找活
    MovingToFood,    // 去取餐
    MovingToTable    // 去送餐
}

public class Waiter : MonoBehaviour
{
    [Header("引用")]
    public FoodSpawner _FoodSpawner;
    public Animator animator;
    [HideInInspector] public Food collectedFood;
    [HideInInspector] public Table _table;

    private NavMeshAgent agent;
    private WaiterState state;
    private float retryTimer;
    private const float RetryInterval = 2f;
    private Table[] cachedTables;

    // ========== Unity 生命周期 ==========

    private void Start()
    {
        GameEvents.current.onFoodDelivered += OnFoodDelivered;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        cachedTables = transform.parent.GetComponentsInChildren<Table>();
        ChangeState(WaiterState.Idle);
    }

    private void Update()
    {
        switch (state)
        {
            case WaiterState.Idle:
                retryTimer -= Time.deltaTime;
                if (retryTimer <= 0f)
                {
                    if (!TryFindTask())
                        retryTimer = RetryInterval;
                }
                break;

            case WaiterState.MovingToFood:
                if (collectedFood != null)
                {
                    // 捡到食物了，切送餐状态或回 idle 端着等
                    if (TryGoToTableForFood())
                        break;
                    ChangeState(WaiterState.Idle);
                    break;
                }
                if (ReachedDestination())
                {
                    // 到了但食物被别人拿了
                    ChangeState(WaiterState.Idle);
                }
                break;

            case WaiterState.MovingToTable:
                if (collectedFood == null)
                {
                    ChangeState(WaiterState.Idle);
                }
                else if (_table == null || _table.isFoodDelivered || !_table.isCustomerSit)
                {
                    // 目标桌中途失效（已被喂、顾客走了），找新桌
                    if (!TryGoToTableForFood())
                        ChangeState(WaiterState.Idle); // 没人需要这菜了 → carry
                }
                else if (ReachedDestination())
                {
                    ChangeState(WaiterState.Idle);
                }
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Table")) return;
        if (collectedFood == null) return;
        Table triggerTable = other.GetComponent<Table>();
        if (triggerTable == null) return;
        if (!triggerTable.isCustomerSit) return;
        if (collectedFood.foodName != triggerTable.orderedFoodName) return;
        if (triggerTable.isFoodDelivered) return;
        if (triggerTable != _table) return;

        // 条件都满足 → 送餐
        triggerTable.isFoodDelivered = true;
        collectedFood.DeliverToTable(triggerTable.tableTopTramsform);
        collectedFood = null;
    }

    private void OnDestroy()
    {
        if (GameEvents.current != null)
            GameEvents.current.onFoodDelivered -= OnFoodDelivered;
    }

    // ========== 状态切换 ==========

    private void ChangeState(WaiterState next)
    {
        LeaveState(state);
        state = next;
        EnterState(next);
    }

    private void LeaveState(WaiterState old)
    {
        // Waiter 没有需要清理的逻辑
    }

    private void EnterState(WaiterState next)
    {
        switch (next)
        {
            case WaiterState.Idle:
                animator.SetTrigger("carry");
                retryTimer = 0f;
                agent.isStopped = true;
                break;

            case WaiterState.MovingToFood:
            case WaiterState.MovingToTable:
                animator.SetTrigger("jog");
                agent.isStopped = false;
                break;
        }
    }

    // ========== 外部调用 ==========

    public void PickUpFood(Food food)
    {
        collectedFood = food;
        if (!TryGoToTableForFood())
            ChangeState(WaiterState.Idle); // 没有匹配的餐桌，端着菜回carry等
    }

    // ========== 任务查找 ==========

    private bool TryFindTask()
    {
        if (collectedFood != null)
            return TryGoToTableForFood();
        return TryGoToFoodForTable();
    }

    private bool TryGoToFoodForTable()
    {
        ShuffleTables();
        foreach (Table table in cachedTables)
        {
            if (table.isFoodDelivered || !table.isCustomerSit) continue;
            foreach (Food food in _FoodSpawner.food)
            {
                if (food.foodName == table.orderedFoodName)
                {
                    _table = table;
                    GoTo(_FoodSpawner.foodTakingPoint[food.foodIndex].position);
                    ChangeState(WaiterState.MovingToFood);
                    return true;
                }
            }
        }
        return false;
    }

    private bool TryGoToTableForFood()
    {
        if (collectedFood == null) return false;
        ShuffleTables();
        foreach (Table table in cachedTables)
        {
            if (!table.isFoodDelivered && table.isCustomerSit
                && table.orderedFoodName == collectedFood.foodName)
            {
                _table = table;
                GoTo(table.transform.position);
                ChangeState(WaiterState.MovingToTable);
                return true;
            }
        }
        return false;
    }

    // ========== 事件 ==========

    private void OnFoodDelivered()
    {
        if (_table != null && _table.isFoodDelivered)
        {
            if (collectedFood != null && TryGoToTableForFood())
                return;
            _table = null;
            ChangeState(WaiterState.Idle);
        }
    }

    // ========== 工具 ==========

    private void GoTo(Vector3 pos)
    {
        agent.SetDestination(pos);
        agent.isStopped = false;
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0f) return false;
        return true;
    }

    private void ShuffleTables()
    {
        for (int i = 0; i < cachedTables.Length; i++)
        {
            int ri = Random.Range(i, cachedTables.Length);
            (cachedTables[i], cachedTables[ri]) = (cachedTables[ri], cachedTables[i]);
        }
    }
}
