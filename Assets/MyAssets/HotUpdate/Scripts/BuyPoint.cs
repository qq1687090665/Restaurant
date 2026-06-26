using UnityEngine;
using RDG;
using TMPro;
using DG.Tweening;

public class BuyPoint : MonoBehaviour
{
    public int srNo, purchaseAmount;
    private int waitersCount;
    private float countAnimSpeed = 0.1f;
    private float animDuration = 0.5f;
    private TextMeshPro moneyAmountText;
    public GameObject objectToUnlock;
    private PlayerController _PlayerController;
    public bool areaBuyPoint, tableBuyPoint, waiterSpawnerBuyPoint;

    private void Awake()
    {
        _PlayerController = FindObjectOfType<PlayerController>();
        waitersCount = PlayerPrefs.GetInt(srNo + "WaitersCount", 0);

        if (PlayerPrefs.HasKey(srNo+"Unlocked"))
        {
            if (objectToUnlock.GetComponent<Table>())
                objectToUnlock.GetComponent<BoxCollider>().enabled = true;

            UnlockObject();
        }

        purchaseAmount = PlayerPrefs.GetInt(srNo+"PurchaseAmount", purchaseAmount);

        moneyAmountText = GetComponentInChildren<TextMeshPro>();

        ShowPurchaseAmount();
    }

    private void ShowPurchaseAmount()
    {
        moneyAmountText.text = purchaseAmount.ToString();
    }

    public void StartSpend()
    {
        print("StartSpend");

        if (purchaseAmount > 500)
            countAnimSpeed = 0.01f;
         else if (purchaseAmount > 100)
            countAnimSpeed = 0.05f;

        InvokeRepeating("Spend", countAnimSpeed, countAnimSpeed);
    }

    private void Spend()
    {
        print("TestSpend");

        if (GameManager.Instance.collectedMoney > 0)
        {
            AudioManager.Instance.Play("BuyPoint");

            Vibration.Vibrate(30);
            purchaseAmount--;
            PlayerPrefs.SetInt(srNo + "PurchaseAmount", purchaseAmount);

            GameManager.Instance.LessMoney();
            ShowPurchaseAmount();

            if (purchaseAmount == 0)
            {
                PlayerPrefs.SetString(srNo + "Unlocked", "True");

                if (tableBuyPoint)
                {
                    _PlayerController.SidePos();
                    objectToUnlock.transform.DOPunchScale(new Vector3(0.1f, 1, 0.1f), animDuration, 7).OnComplete(() => objectToUnlock.GetComponent<BoxCollider>().enabled = true/*Destroy(this.gameObject)*/);
                    UnlockObject();

                    if (transform.parent.transform.GetComponentInChildren<CustomerSpawner>())
                        transform.parent.transform.GetComponentInChildren<CustomerSpawner>().SpawnCustomer();
                    else
                        transform.parent.transform.parent.transform.GetComponentInChildren<CustomerSpawner>().SpawnCustomer();

                }
                else if (areaBuyPoint)
                {
                    UnlockObject();
                }
                else if (waiterSpawnerBuyPoint)
                {
                    _PlayerController.SidePos();
                    objectToUnlock.transform.DOPunchScale(new Vector3(0.1f, 1, 0.1f), animDuration, 7).OnComplete(() => Destroy(this.gameObject)); ;
                    UnlockObject();
                }

                AudioManager.Instance.Play("Unlock");
                objectToUnlock.GetComponentInChildren<ParticleSystem>().Play();
            }
        }
    }

    private void UnlockObject()
    {


        objectToUnlock.SetActive(true);
        DOTween.Kill(this.gameObject);
        Destroy(this.gameObject);
    }

    public void StopSpend()
    {
        CancelInvoke("Spend");
    }
}
