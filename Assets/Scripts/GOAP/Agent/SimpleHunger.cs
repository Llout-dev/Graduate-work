using UnityEngine;

public class SimpleHunger : MonoBehaviour, IHunger
{
    private float hungerLevel = 0f;

    void Update()
    {
        hungerLevel += Time.deltaTime * 0.05f; // Медленно голодаем
        hungerLevel = Mathf.Clamp01(hungerLevel);
    }

    public int HungerLevel => Mathf.RoundToInt(hungerLevel * 100f);

    public bool IsHungry() => HungerLevel > 70;

    public void Eat()
    {
        hungerLevel = 0f; // Полностью насытились
        Debug.Log("Уровень голода сброшен!");
    }
}