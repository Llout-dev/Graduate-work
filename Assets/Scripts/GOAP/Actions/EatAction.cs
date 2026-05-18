using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

[GoapId("EatAction")]
public class EatAction : GoapActionBase<EatAction.Data>
{
    public override void Start(IMonoAgent agent, Data data)
    {
        Debug.Log($"{agent.gameObject.name}: иду к еде");
    }

    public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
    {
        if (!context.IsInRange)
            return ActionRunState.Continue;

        if (!data.HasEaten)
        {
            agent.GetComponent<IHunger>()?.Eat();

            if (data.Target is TransformTarget foodTarget && foodTarget.Transform != null)
                foodTarget.Transform.GetComponent<FoodBehaviour>()?.Consume();

            data.HasEaten = true;
            Debug.Log($"{agent.gameObject.name}: поел");
        }

        return ActionRunState.Completed;
    }

    public override void End(IMonoAgent agent, Data data)
    {
        Debug.Log($"{agent.gameObject.name}: закончил есть");
    }

    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public bool HasEaten { get; set; }
    }
}