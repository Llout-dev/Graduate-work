using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

[GoapId("WanderGoal")]
public class WanderGoal : GoalBase
{
    // ���� "�������" ����� ������ ��������� (5), ���� ����� ���.
    // ���� �������, ��������� ����������� (�����������).
    public override float GetCost(IActionReceiver agent, IComponentReference references)
    {
        var hunger = references.GetCachedComponent<IHunger>();
        if (hunger != null && hunger.IsHungry())
            return float.MaxValue; // �� ��������, ���� �������

        return 5f;
    }
}