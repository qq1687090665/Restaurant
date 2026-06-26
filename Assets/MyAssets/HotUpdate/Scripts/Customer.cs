using UnityEngine;
using UnityEngine.AI;

public enum CustomerState
{
    None,
    WalkingToTable,
    Sitting,
    Eating,
    Victory,
    WalkingToExit
}

public class Customer : MonoBehaviour
{
    [Header("配置")]
    public float eatingTime = 5f;
    public float victoryPoseDuration = 3f;

    [Header("引用")]
    public Animator anim;
    public SpriteRenderer orderedFoodSprite;
    public GameObject moneyPrefab;
    [HideInInspector] public Transform exitTransform;
    [HideInInspector] public GameObject food;

    private NavMeshAgent agent;
    private Table table;
    private Camera mainCamera;
    private CustomerState state;
    private float stateTimer;
    private float rotationSpeed;
    private int targetYRotation;
    private CustomerSpawner spawner;

    // ========== Unity 生命周期 ==========

    private void Start()
    {
        spawner = transform.parent.GetComponentInChildren<CustomerSpawner>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        mainCamera = Camera.main;
        table = FindAvailableTable();
        ChangeState(CustomerState.WalkingToTable);
    }

    private void Update()
    {
        // 点餐气泡面向摄像机
        if (orderedFoodSprite)
            orderedFoodSprite.transform.rotation = mainCamera.transform.rotation;

        // 旋转平滑
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.Euler(0, targetYRotation, 0),
            rotationSpeed * Time.deltaTime);

        // 状态计时 + 条件判断
        switch (state)
        {
            case CustomerState.WalkingToTable:
                if (ReachedDestination())
                    ChangeState(CustomerState.Sitting);
                break;
            case CustomerState.Eating:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(CustomerState.Victory);
                break;
            case CustomerState.Victory:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    ChangeState(CustomerState.WalkingToExit);
                break;
            case CustomerState.WalkingToExit:
                if (ReachedDestination())
                    LeaveRestaurant();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExitPoint"))
            LeaveRestaurant();
    }

    private void OnLeaveTable()
    {
        if (table == null) return;
        table.isFoodDelivered = false;
        table.isFull = false;
        table.isCustomerSit = false;
        table._Customer = null;
        table.orderedFoodName = "None";
    }

    // ========== 状态切换 ==========

    private void ChangeState(CustomerState next)
    {
        LeaveState(state);   // 清理旧状态
        state = next;
        EnterState(next);    // 初始化新状态 + 设动画 bool
    }

    private void LeaveState(CustomerState old)
    {
        // 只做逻辑清理，不管动画（Trigger 已自动消费）
        switch (old)
        {
            case CustomerState.Eating:
                Destroy(food);
                break;
        }
    }

    private void EnterState(CustomerState next)
    {
        // Trigger 是一次性的，Animator 消费后自动重置，不会重复触发
        switch (next)
        {
            case CustomerState.WalkingToTable:
                anim.SetTrigger("walk");
                agent.SetDestination(table.chairTransform.position);
                agent.isStopped = false;
                // ===== 模拟热更：我们在代码里硬编码将其寻路速度改为 5.0f =====
                agent.speed = 5.0f; 
                Debug.Log($"[热更验证] 顾客当前寻路速度已被动态修改为: {agent.speed}");
                break;

            case CustomerState.Sitting:
                anim.SetTrigger("sit");
                agent.isStopped = true;
                table.isCustomerSit = true;
                table._Customer = this;
                AdjustSitRotation();
                rotationSpeed = 150f;
                OrderRandomFood();
                break;

            case CustomerState.Eating:
                anim.SetTrigger("eat");
                stateTimer = eatingTime;
                if (orderedFoodSprite)
                    Destroy(orderedFoodSprite.gameObject);
                break;

            case CustomerState.Victory:
                anim.SetTrigger("victory");
                stateTimer = victoryPoseDuration;
                Instantiate(moneyPrefab,
                    new Vector3(transform.position.x, moneyPrefab.transform.position.y, transform.position.z),
                    transform.rotation);
                break;

            case CustomerState.WalkingToExit:
                anim.SetTrigger("walk");
                rotationSpeed = 0f;
                agent.SetDestination(exitTransform.position);
                agent.isStopped = false;
                agent.updateRotation = true;
                OnLeaveTable();
                break;
        }
    }

    // ========== 外部调用 ==========

    public void ServeFood(GameObject foodObj)
    {
        food = foodObj;
        ChangeState(CustomerState.Eating);
    }

    // ========== 工具方法 ==========

    private Table FindAvailableTable()
    {
        Table[] tables = transform.parent.GetComponentsInChildren<Table>();
        int rand = Random.Range(0, tables.Length);
        int attempts = 0;
        while (tables[rand].isFull && attempts < tables.Length)
        {
            rand = (rand + 1) % tables.Length;
            attempts++;
        }
        tables[rand].isFull = true;
        return tables[rand];
    }

    private void AdjustSitRotation()
    {
        float y = table.chairTransform.eulerAngles.y;
        if (Mathf.Approximately(y, 270f))       targetYRotation = 90;
        else if (Mathf.Approximately(y, 0f))    targetYRotation = 180;
        else if (Mathf.Approximately(y, 90f))   targetYRotation = -90;
    }

    private void OrderRandomFood()
    {
        int idx = Random.Range(0, spawner.foodNames.Length);
        table.orderedFoodName = spawner.foodNames[idx];
        orderedFoodSprite.sprite = spawner.orderedFoodSprites[idx];
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0f) return false;
        return true;
    }

    private void LeaveRestaurant()
    {
        spawner.SpawnCustomer();
        Destroy(gameObject);
    }
}
