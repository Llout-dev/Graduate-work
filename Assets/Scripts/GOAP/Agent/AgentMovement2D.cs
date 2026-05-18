using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;

/// <summary>
/// Простое движение к цели действия (2D). Подключите к волку вместе с AgentBehaviour.
/// </summary>
[RequireComponent(typeof(AgentBehaviour))]
public class AgentMovement2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private AgentBehaviour agent;
    private ITarget currentTarget;
    private bool shouldMove;

    private void Awake()
    {
        agent = GetComponent<AgentBehaviour>();
    }

    private void OnEnable()
    {
        agent.Events.OnTargetInRange += OnTargetInRange;
        agent.Events.OnTargetNotInRange += OnTargetNotInRange;
        agent.Events.OnTargetChanged += OnTargetChanged;
        agent.Events.OnTargetLost += OnTargetLost;
        agent.Events.OnMove += OnMove;
    }

    private void OnDisable()
    {
        agent.Events.OnTargetInRange -= OnTargetInRange;
        agent.Events.OnTargetNotInRange -= OnTargetNotInRange;
        agent.Events.OnTargetChanged -= OnTargetChanged;
        agent.Events.OnTargetLost -= OnTargetLost;
        agent.Events.OnMove -= OnMove;
    }

    private void OnTargetInRange(ITarget target) => shouldMove = false;

    private void OnTargetNotInRange(ITarget target)
    {
        currentTarget = target;
        shouldMove = true;
    }

    private void OnTargetChanged(ITarget target, bool inRange)
    {
        currentTarget = target;
        shouldMove = !inRange;
    }

    private void OnMove(ITarget target)
    {
        currentTarget = target;
        shouldMove = true;
    }

    private void OnTargetLost() => shouldMove = false;

    private void Update()
    {
        if (!shouldMove || currentTarget == null)
            return;

        var destination = currentTarget.GetValidPosition();
        if (destination == null)
            return;

        var current = transform.position;
        var next = Vector3.MoveTowards(current, destination.Value, moveSpeed * Time.deltaTime);
        transform.position = next;
    }
}
