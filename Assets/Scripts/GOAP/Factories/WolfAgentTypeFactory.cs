using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

// Вешать на объект GOAP вместе с GoapBehaviour и ReactiveControllerBehaviour.
public class WolfAgentTypeFactory : AgentTypeFactoryBase
{
    public override IAgentTypeConfig Create()
    {
        var builder = CreateBuilder("Wolf");
        builder.AddCapability<WolfCapabilityFactory>();
        return builder.Build();
    }
}
