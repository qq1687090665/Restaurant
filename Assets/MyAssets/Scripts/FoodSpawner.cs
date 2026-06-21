using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public Food[] food;
    public Transform[] foodSpawnPoint, foodTakingPoint;

    private void Start()
    {
        SpawnFood(0);
        SpawnFood(1);
    }

    public void SpawnFood(int foodIndex)
    {
        GameObject foodObj = Instantiate(food[foodIndex].gameObject, foodSpawnPoint[foodIndex].position, transform.rotation);
        foodObj.GetComponent<Food>().foodIndex = foodIndex;
        foodObj.transform.parent = this.transform;
    }
}
