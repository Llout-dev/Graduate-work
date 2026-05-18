using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

[GoapId("WanderAction")]
public class WanderAction : GoapActionBase<WanderAction.Data>
{
    public override void Start(IMonoAgent agent, Data data)
    {
        // Тут можно запустить анимацию или установить триггер
        Debug.Log($"{agent.gameObject.name}: Начинаю бродить");
    }

    public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
    {
        // Выполняем действие 2 секунды
        if (data.Timer == null)
        {
            data.Timer = ActionRunState.Wait(2f);
        }

        // Логика движения будет здесь
        Debug.Log($"{agent.gameObject.name}: Иду в точку {data.Target}");

        return data.Timer;
    }

    public override void End(IMonoAgent agent, Data data)
    {
        Debug.Log($"{agent.gameObject.name}: Закончил бродить");
    }

    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public IActionRunState Timer { get; set; }
    }
}