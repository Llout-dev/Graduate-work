using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

/// <summary>
/// Связывает волка с типом "Wolf" и запрашивает цели у планировщика.
/// </summary>
[RequireComponent(typeof(GoapActionProvider))]
[RequireComponent(typeof(AgentBehaviour))]
public class WolfBrain : MonoBehaviour
{
    private GoapActionProvider provider;

    private void Awake()
    {
        provider = GetComponent<GoapActionProvider>();

        // При настройке через код (WolfAgentTypeFactory) поле Agent Type Behaviour оставляют пустым.
        if (provider.AgentTypeBehaviour == null)
        {
            var goap = FindFirstObjectByType<GoapBehaviour>();
            if (goap != null)
                provider.AgentType = goap.GetAgentType("Wolf");
        }
    }

    private void Start()
    {
        provider.RequestGoal<EatGoal, WanderGoal>();
    }
}
