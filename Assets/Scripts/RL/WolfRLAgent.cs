using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// RL-волк для сравнения с GOAP. Требует SimpleHunger (IHunger), FoodBehaviour на сцене, тег Food.
/// На префабе: Behavior Parameters (WolfRL, 4 obs, 2 continuous actions), Decision Requester.
/// Отключите WolfBrain, AgentBehaviour, AgentMovement2D — они для GOAP-версии.
/// </summary>
[RequireComponent(typeof(SimpleHunger))]
public class WolfRLAgent : Agent
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;

    [Header("Arena (нормализация наблюдений)")]
    [SerializeField] Vector2 arenaHalfExtents = new(10f, 10f);
    [SerializeField] float maxFoodDistance = 20f;

    [Header("Rewards")]
    [SerializeField] float foodReward = 5f;
    [SerializeField] float hungerStepPenalty = 0.01f;
    [SerializeField] float deathPenalty = 5f;

    const int DeathHungerLevel = 100;

    IHunger hunger;
    int lastHungerLevel;
    FoodBehaviour[] foods = System.Array.Empty<FoodBehaviour>();

    public override void Initialize()
    {
        hunger = GetComponent<IHunger>();
        if (hunger == null)
            Debug.LogError($"{name}: WolfRLAgent требует компонент IHunger (например SimpleHunger).", this);
    }

    public override void OnEpisodeBegin()
    {
        hunger?.Eat();
        lastHungerLevel = 0;

        transform.position = new Vector3(
            Random.Range(-arenaHalfExtents.x, arenaHalfExtents.x),
            Random.Range(-arenaHalfExtents.y, arenaHalfExtents.y),
            transform.position.z);

        ResetAllFood();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        RefreshFoodCache();

        var hungerNorm = hunger != null ? hunger.HungerLevel / (float)DeathHungerLevel : 0f;
        sensor.AddObservation(hungerNorm);

        var pos = transform.position;
        sensor.AddObservation(pos.x / arenaHalfExtents.x);
        sensor.AddObservation(pos.y / arenaHalfExtents.y);

        var distance = GetDistanceToClosestFood();
        sensor.AddObservation(Mathf.Clamp01(distance / maxFoodDistance));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        var moveY = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        var delta = new Vector3(moveX, moveY, 0f) * (moveSpeed * Time.deltaTime);
        var next = transform.position + delta;
        next.x = Mathf.Clamp(next.x, -arenaHalfExtents.x, arenaHalfExtents.x);
        next.y = Mathf.Clamp(next.y, -arenaHalfExtents.y, arenaHalfExtents.y);
        transform.position = next;

        ApplyHungerPenalties();

        if (hunger != null && hunger.HungerLevel >= DeathHungerLevel)
        {
            AddReward(-deathPenalty);
            EndEpisode();
        }
    }

    void ApplyHungerPenalties()
    {
        if (hunger == null)
            return;

        var current = hunger.HungerLevel;
        var hungerDelta = current - lastHungerLevel;
        if (hungerDelta > 0)
            AddReward(-hungerStepPenalty * hungerDelta);

        lastHungerLevel = current;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Food"))
            return;

        var food = other.GetComponent<FoodBehaviour>();
        if (food == null || !food.IsAvailable)
            return;

        hunger?.Eat();
        food.Consume();
        AddReward(foodReward);
        lastHungerLevel = 0;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxisRaw("Horizontal");
        continuous[1] = Input.GetAxisRaw("Vertical");
    }

    void RefreshFoodCache()
    {
        foods = FindObjectsByType<FoodBehaviour>(FindObjectsSortMode.None);
    }

    void ResetAllFood()
    {
        RefreshFoodCache();
        foreach (var food in foods)
        {
            if (food != null)
                food.ResetAvailability();
        }
    }

    float GetDistanceToClosestFood()
    {
        var position = transform.position;
        var closestDistance = maxFoodDistance;

        foreach (var food in foods)
        {
            if (food == null || !food.IsAvailable)
                continue;

            var distance = Vector3.Distance(position, food.transform.position);
            if (distance < closestDistance)
                closestDistance = distance;
        }

        return closestDistance;
    }
}
