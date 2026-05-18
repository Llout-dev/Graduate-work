using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

[GoapId("EatTargetSensor")]
public class EatTargetSensor : LocalTargetSensorBase
{
    private FoodBehaviour[] foods = System.Array.Empty<FoodBehaviour>();

    public override void Created() { }

    public override void Update()
    {
        foods = Object.FindObjectsByType<FoodBehaviour>(FindObjectsSortMode.None);
    }

    public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
    {
        FoodBehaviour closest = null;
        var closestDistance = float.MaxValue;
        var position = agent.Transform.position;

        foreach (var food in foods)
        {
            if (food == null || !food.IsAvailable)
                continue;

            var distance = Vector3.Distance(position, food.transform.position);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closest = food;
        }

        return closest == null ? null : new TransformTarget(closest.transform);
    }
}
