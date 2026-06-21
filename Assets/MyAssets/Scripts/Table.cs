using UnityEngine;

public class Table : MonoBehaviour
{
   // [HideInInspector]
    public string orderedFoodName;
  //  [HideInInspector]
    public bool isFoodDelivered, isFull, isCustomerSit;
    public Transform chairTransform, tableTopTramsform;
    //[HideInInspector]
    public Customer _Customer;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Unlocked"))
            this.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food"))
        {

            isFoodDelivered = true;
            GameEvents.current.FoodDelivered();
        }
    }
}
