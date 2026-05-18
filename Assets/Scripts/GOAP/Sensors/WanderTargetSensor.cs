// WanderTargetSensor.cs
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

[GoapId("WanderTargetSensor")]
public class WanderTargetSensor : LocalTargetSensorBase
{
    public override void Created() { }
    public override void Update() { }

    public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
    {
        // Случайная позиция в радиусе 10 единиц
        Vector3 randomPos = agent.Transform.position + new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(-5f, 5f),
            0
        );
        return new PositionTarget(randomPos);
    }
}