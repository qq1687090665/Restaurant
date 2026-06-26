using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [HideInInspector]
    public List<Food> collectedFood;
    public float cashCollectAnimDuration, moneySpriteAnimSpeed, moneySpriteScaleAnimDuration;
    public int moneyUiVibrate;
    public Vector3 scaleValue;
    public GameObject moneySprite;
    private bool canAnimate = true;

    private void Start()
    {
        Instance = this;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Table"))
        {
            Table table = other.GetComponent<Table>();
            print("Check");
            if (table.isFull)
            {
                int i = -1;
                for (int k = collectedFood.Count - 1; k >= 0; k--)
                {
                    if (!table.isFoodDelivered)
                    {
                        i++;
                        if (collectedFood[k].foodName == table.orderedFoodName)
                        {
                            collectedFood[k].DeliverToTable(table.tableTopTramsform);
                            AudioManager.Instance.Play("FoodServe");

                            table.isFoodDelivered = true;

                            collectedFood.Remove(collectedFood[k]);

                            for (int j = k; j < collectedFood.Count; j++)
                            {
                                collectedFood[j].DownYPose();
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    Tweener tweener;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BuyPoint"))
        {
            print("BuyPointEnter");
            other.GetComponent<BuyPoint>().StartSpend();
        }

        if (other.gameObject.CompareTag("Money"))
        {
            other.gameObject.tag = "Untagged";
            AudioManager.Instance.Play("MoneyCollect");

            other.transform.DOKill();
            other.transform.DOMove(transform.position, cashCollectAnimDuration)
            .OnComplete(() =>
            {
                Destroy(other.gameObject);

                Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

                GameObject moneySp = Instantiate(moneySprite, pos, Quaternion.Euler(0, 0, 0), CanvasUiManager.Instance.transform);

                tweener = moneySp.transform.DOMove(CanvasUiManager.Instance.moneyUiTarget.position, moneySpriteAnimSpeed)
                .OnComplete(() =>
                {
                    GameManager.Instance.AddMoney(2);
                    print("Money");
                    if (canAnimate)
                    {
                        canAnimate = false;
                        CanvasUiManager.Instance.moneyUiTarget.DOPunchScale(scaleValue, moneySpriteScaleAnimDuration, moneyUiVibrate)
                        .OnComplete(() =>
                        {
                            canAnimate = true;

                        });
                    }

                    Destroy(moneySp);
                });
            });
        }

        if (other.gameObject.CompareTag("WaiterSpawner"))
        {
            WaiterSpawner waiterSpawner = other.GetComponent<WaiterSpawner>();
            CanvasUiManager.Instance.ShowBuyWaiterPanel(waiterSpawner);
            waiterSpawner.IsPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BuyPoint"))
            other.GetComponent<BuyPoint>().StopSpend();

        if (other.gameObject.CompareTag("WaiterSpawner"))
        {
            WaiterSpawner waiterSpawner = other.GetComponent<WaiterSpawner>();
            CanvasUiManager.Instance.HideBuyWaiterPanel();
            waiterSpawner.IsPlayerNearby = false;
        }
    }
}
