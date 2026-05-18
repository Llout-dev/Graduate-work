using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

[GoapId("HungerSensor")]
public class HungerSensor : LocalWorldSensorBase
{
    public override void Created() { }

    public override void Update() { }

    public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
    {
        var hunger = references.GetCachedComponent<IHunger>();
        return hunger == null ? 0 : hunger.HungerLevel;
    }
}
