using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

[GoapId("EatGoal")]
public class EatGoal : GoalBase
{
    public override float GetCost(IActionReceiver agent, IComponentReference references)
    {
        var hunger = references.GetCachedComponent<IHunger>();

        if (hunger != null && hunger.IsHungry())
            return 10f;

        return float.MaxValue;
    }
}