using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

// Не MonoBehaviour — на объект НЕ вешать. Подключается через WolfAgentTypeFactory.
public class WolfCapabilityFactory : CapabilityFactoryBase
{
    public override ICapabilityConfig Create()
    {
        var capability = new CapabilityBuilder("Wolf");

        capability.AddGoal<EatGoal>()
            .SetBaseCost(10)
            .AddCondition<HungerLevel>(Comparison.SmallerThanOrEqual, 25);

        capability.AddGoal<WanderGoal>()
            .SetBaseCost(5);

        capability.AddAction<EatAction>()
            .SetTarget<EatTargetKey>()
            .SetRequiresTarget(true)
            .SetStoppingDistance(0.5f)
            .SetMoveMode(ActionMoveMode.MoveBeforePerforming)
            .AddCondition<HungerLevel>(Comparison.GreaterThanOrEqual, 25)
            .AddEffect<HungerLevel>(EffectType.Decrease);

        capability.AddAction<WanderAction>()
            .SetTarget<WanderTargetKey>()
            .SetRequiresTarget(true)
            .SetStoppingDistance(0.3f)
            .SetMoveMode(ActionMoveMode.PerformWhileMoving);

        capability.AddWorldSensor<HungerSensor>()
            .SetKey<HungerLevel>();

        capability.AddTargetSensor<WanderTargetSensor>()
            .SetTarget<WanderTargetKey>();

        capability.AddTargetSensor<EatTargetSensor>()
            .SetTarget<EatTargetKey>();

        return capability.Build();
    }
}
