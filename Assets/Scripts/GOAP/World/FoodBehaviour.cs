using UnityEngine;

public class FoodBehaviour : MonoBehaviour
{
    public bool IsAvailable { get; private set; } = true;

    public void Consume()
    {
        if (!IsAvailable)
            return;

        IsAvailable = false;
        gameObject.SetActive(false);
    }

    /// <summary>Восстанавливает еду после конца эпизода RL (GOAP не вызывает).</summary>
    public void ResetAvailability()
    {
        IsAvailable = true;
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}
