using UnityEngine;
using RDG;
using DG.Tweening;

public class Food : MonoBehaviour
{
    private Transform playerBack;
    private float cashYvalue = 0.6f;
    private bool goToPlayerBack = true;
    private Waiter _Waiter;
    public float paySpeed, jumpPower;
    private Transform targetPose;
    public string foodName;
    private FoodSpawner _FoodSpawner;
    public int foodIndex;

    private void Start()
    {
        _FoodSpawner = GetComponentInParent<FoodSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (goToPlayerBack)
            {
                playerBack = other.transform.GetChild(1).transform;
                targetPose = playerBack;

                Waiter waiter = other.gameObject.GetComponent<Waiter>();
                if (waiter != null)
                {
                    if (waiter.collectedFood)
                        return;
                    else if (waiter._table != null && waiter._table.orderedFoodName != foodName)
                        return;
                }

                if (other.gameObject.GetComponent<PlayerManager>())
                {
                    Vibration.Vibrate(30);
                    AudioManager.Instance.Play("FoodCollect");
                    transform.parent = PlayerManager.Instance.transform;
                }
                else
                {
                    _Waiter = other.gameObject.GetComponent<Waiter>();
                    transform.parent = _Waiter.transform;
                }

                if (other.gameObject.GetComponent<PlayerManager>())
                    PlayerManager.Instance.collectedFood.Add(this);
                else
                {
                    _Waiter.PickUpFood(this);
                }

                transform.DOLocalJump(targetPose.localPosition, jumpPower, 1, paySpeed)
                .OnComplete(delegate ()
                {

                    this.transform.localPosition = playerBack.localPosition;
                    this.transform.localEulerAngles = Vector3.zero;

                    playerBack.position = new Vector3(playerBack.transform.position.x, playerBack.transform.position.y + cashYvalue, playerBack.transform.position.z);
                    print("Test1");
                    _FoodSpawner.SpawnFood(foodIndex);
                });

                goToPlayerBack = false;
            }
        }

        if (other.CompareTag("Table"))
        {
            Table table = other.GetComponent<Table>();

            if (table._Customer)
            { 
                table._Customer.ServeFood(this.gameObject);
            }
        }
    }

    public void DeliverToTable(Transform targetPos)
    {
        if(transform.parent)
        transform.parent = null;

        targetPose = targetPos;

        transform.DOJump(targetPose.position, 4, 1, .4f);

        playerBack.position = new Vector3(playerBack.transform.position.x, playerBack.transform.position.y - cashYvalue, playerBack.transform.position.z);
    }

    public void DownYPose()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - cashYvalue, transform.position.z);
    }
}
