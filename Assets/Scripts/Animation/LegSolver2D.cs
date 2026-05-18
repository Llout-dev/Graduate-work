using UnityEngine;

public class LegSolver2D : MonoBehaviour
{
    [Header("Кости")]
    public Transform shoulder;      // PIVOT_LShoulder
    public Transform elbow;         // PIVOT_LWrist
    public Transform palm;          // PIVOT_LPalm

    [Header("Параметры")]
    public float upperLegLength = 1f;
    public float lowerLegLength = 1f;

    [Header("Настройки сгиба")]
    public bool isLeftLeg = true;   // Левая или правая нога
    public bool isFrontLeg = true;  // Передняя или задняя
    public float bendStrength = 1f; // Сила сгиба (0-1)

    [Header("Поворот ладони")]
    public float maxPalmRotation = 30f;  // Максимальный поворот ладони в сторону цели

    [Header("След")]
    public Transform footPrint;
    public float stepSpeed = 15f;

    [Header("Настройки спрайтов")]
    public float shoulderRotationOffset = 90f;
    public float elbowRotationOffset = 90f;
    public float palmRotationOffset = 90f;

    [Header("Визуализация")]
    public bool drawDebug = true;

    private Vector3 targetPalmPosition;
    private bool flipElbow;
    private float currentPalmAngle = 0f; // Для плавного поворота ладони
    private float currentPalmAngleVelocity = 0f; // Опционально для ещё большей плавности

    private float totalSolveTime = 0f;
    private int solveCount = 0;

    void Start()
    {
        UpdateBendDirection();

        if (shoulder != null && elbow != null)
            upperLegLength = Vector3.Distance(shoulder.position, elbow.position);

        if (elbow != null && palm != null)
            lowerLegLength = Vector3.Distance(elbow.position, palm.position);

        if (footPrint == null)
        {
            GameObject fp = new GameObject("FootPrint");
            footPrint = fp.transform;
            AddFootPrintVisuals();
        }

        footPrint.position = palm.position;
        targetPalmPosition = palm.position;

        // ИНИЦИАЛИЗИРУЕМ ТЕКУЩИЙ УГОЛ
        currentPalmAngle = palmRotationOffset;  // Добавь эту строку

        Debug.Log($"Upper: {upperLegLength}, Lower: {lowerLegLength}, FlipElbow: {flipElbow}");
    }

    void AddFootPrintVisuals()
    {
        // Создаём дочерние объекты для визуализации
        GameObject upArrow = new GameObject("UpArrow");
        upArrow.transform.SetParent(footPrint);
        upArrow.transform.localPosition = Vector3.zero;

        GameObject rightArrow = new GameObject("RightArrow");
        rightArrow.transform.SetParent(footPrint);
        rightArrow.transform.localPosition = Vector3.zero;
    }

    void UpdateBendDirection()
    {
        if (isLeftLeg && isFrontLeg)
            flipElbow = false;
        else if (!isLeftLeg && isFrontLeg)
            flipElbow = false;
        else if (isLeftLeg && !isFrontLeg)
            flipElbow = false;
        else
            flipElbow = true;
    }

    void Update()
    {
        MoveShoulderWithInput();

        if (Input.GetKeyDown(KeyCode.F))
        {
            flipElbow = !flipElbow;
            Debug.Log($"FlipElbow: {flipElbow}");
        }

        Vector3 desiredPosition = footPrint.position;
        Vector3 fromShoulder = desiredPosition - shoulder.position;
        float maxReach = upperLegLength + lowerLegLength;

        if (fromShoulder.magnitude > maxReach)
        {
            desiredPosition = shoulder.position + fromShoulder.normalized * (maxReach - 0.01f);
        }

        targetPalmPosition = Vector3.Lerp(targetPalmPosition, desiredPosition, Time.deltaTime * stepSpeed);
        SolveIK(targetPalmPosition);
        RotatePivots();
        RotatePalmTowardTarget();

        if (drawDebug)
        {
            Debug.DrawLine(shoulder.position, elbow.position, Color.red);
            Debug.DrawLine(elbow.position, palm.position, Color.green);
            Debug.DrawLine(palm.position, footPrint.position,
                Vector3.Distance(palm.position, footPrint.position) < 0.1f ? Color.green : Color.yellow);
        }
    }

    void RotatePalmTowardTarget()
    {
        if (palm == null || footPrint == null) return;

        // Расстояние от плеча до следа
        float shoulderToTargetDist = Vector3.Distance(shoulder.position, footPrint.position);
        float maxLegLength = upperLegLength + lowerLegLength;

        // Фактор натяжения: 0 = достает, 1 = не достает
        float stretchFactor = Mathf.Clamp01((shoulderToTargetDist - maxLegLength) / maxLegLength);

        // Угол плеча (направление от плеча к следу)
        Vector3 shoulderToTarget = footPrint.position - shoulder.position;
        float shoulderAngle = Mathf.Atan2(shoulderToTarget.y, shoulderToTarget.x) * Mathf.Rad2Deg;

        // Когда тянемся - добавляем -90°, чтобы ладонь смотрела вниз (перпендикулярно плечу)
        float targetAngleWhenStretching = shoulderAngle + 90f;

        // Базовый угол для покоя (горизонтально)
        float baseAngle = isLeftLeg ? -90f : 90f;

        // Интерполяция между горизонтальным углом и углом "вниз от плеча"
        float finalAngle = Mathf.Lerp(baseAngle, targetAngleWhenStretching, stretchFactor);

        // Применяем оффсет спрайта
        finalAngle += palmRotationOffset;

        // Плавный поворот
        currentPalmAngle = Mathf.LerpAngle(currentPalmAngle, finalAngle, Time.deltaTime * 12f);
        palm.rotation = Quaternion.Euler(0, 0, currentPalmAngle);

        if (drawDebug)
        {
            Debug.Log($"Stretch: {stretchFactor:F2}, ShoulderAngle: {shoulderAngle:F0}, TargetStretch: {targetAngleWhenStretching:F0}, Final: {finalAngle:F0}");
            Debug.DrawLine(palm.position, palm.position + (Quaternion.Euler(0, 0, targetAngleWhenStretching) * Vector3.right) * 0.5f, Color.cyan);
            Debug.DrawLine(palm.position, palm.position + (Quaternion.Euler(0, 0, finalAngle - palmRotationOffset) * Vector3.right) * 0.5f, Color.magenta);
        }
    }

    void MoveShoulderWithInput()
    {
        float speed = 2f;
        Vector3 move = Vector3.zero;

        move.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        move.y = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        shoulder.position += move;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            shoulder.position = mousePos;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            footPrint.position = mousePos;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
            footPrint.position += Vector3.down * scroll;
    }

    void SolveIK(Vector3 targetPosition)
    {
        float startTime = Time.realtimeSinceStartup;
        Vector3 shoulderToTarget = targetPosition - shoulder.position;
        float distanceToTarget = shoulderToTarget.magnitude;
        float totalLength = upperLegLength + lowerLegLength;

        // Если цель слишком далеко - вытягиваем ногу прямо на цель
        if (distanceToTarget > totalLength - 0.01f)
        {
            Vector3 direction = shoulderToTarget.normalized;
            elbow.position = shoulder.position + direction * upperLegLength;
            palm.position = elbow.position + direction * lowerLegLength;
            return;
        }

        // Вычисляем угол в плече для достижения цели
        float angleAtShoulder = Mathf.Acos(Mathf.Clamp(
            (upperLegLength * upperLegLength + distanceToTarget * distanceToTarget - lowerLegLength * lowerLegLength) /
            (2 * upperLegLength * distanceToTarget), -1f, 1f));

        // Определяем направление сгиба (всегда в одну сторону для левой ноги)
        // Для левой ноги локоть всегда должен быть справа от линии плечо-цель
        // Для правой ноги - слева
        float bendDirection = isLeftLeg ? 1f : -1f;

        // Применяем силу сгиба (bendStrength 0-1, где 1 = максимальный сгиб)
        angleAtShoulder = angleAtShoulder * bendStrength * bendDirection;

        // Поворачиваем плечо к цели
        Quaternion toTargetRot = Quaternion.FromToRotation(Vector3.right, shoulderToTarget);
        Quaternion elbowRot = Quaternion.Euler(0, 0, angleAtShoulder * Mathf.Rad2Deg);

        // Позиция локтя
        Vector3 elbowDir = toTargetRot * (elbowRot * Vector3.right);
        elbow.position = shoulder.position + elbowDir * upperLegLength;

        // ЛАДОНЬ ВСЕГДА ТЯНЕТСЯ ПРЯМО К ЦЕЛИ, а не к позиции локтя
        Vector3 palmDir = (targetPosition - elbow.position).normalized;
        palm.position = elbow.position + palmDir * lowerLegLength;

        // Дополнительная корректировка: если ладонь всё ещё далеко от цели,
        // но цель вне досягаемости, максимально вытягиваем
        float palmToTargetDist = Vector3.Distance(palm.position, targetPosition);
        if (palmToTargetDist > 0.05f && distanceToTarget > totalLength * 0.9f)
        {
            // Почти вытянутая нога - доворачиваем последний сегмент точно на цель
            Vector3 finalDir = (targetPosition - elbow.position).normalized;
            palm.position = elbow.position + finalDir * lowerLegLength;
        }
        float solveTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        totalSolveTime += solveTime;
        solveCount++;
        if (solveCount % 100 == 0)
        {
            Debug.Log($"LegSolver2D: ср. {totalSolveTime / solveCount:F3} мс, макс. не замерялось, выз. {solveCount}");
        }
    }

    void RotatePivots()
    {
        if (shoulder != null)
        {
            Vector2 direction = elbow.position - shoulder.position;
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                shoulder.rotation = Quaternion.Euler(0, 0, angle + shoulderRotationOffset);
            }
        }

        if (elbow != null)
        {
            Vector2 direction = palm.position - elbow.position;
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                elbow.rotation = Quaternion.Euler(0, 0, angle + elbowRotationOffset);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawDebug) return;

        if (shoulder != null)
        {
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(shoulder.position, upperLegLength);
            Gizmos.DrawWireSphere(shoulder.position, upperLegLength + lowerLegLength);
        }

        // Красивое обозначение следа
        if (footPrint != null)
        {
            // Зелёная стрелка вверх (ось Y)
            Gizmos.color = Color.green;
            Vector3 upArrowEnd = footPrint.position + Vector3.up * 0.3f;
            Gizmos.DrawLine(footPrint.position, upArrowEnd);
            // Стрелка
            Vector3 upArrowLeft = upArrowEnd + (Vector3.left + Vector3.down) * 0.1f;
            Vector3 upArrowRight = upArrowEnd + (Vector3.right + Vector3.down) * 0.1f;
            Gizmos.DrawLine(upArrowEnd, upArrowLeft);
            Gizmos.DrawLine(upArrowEnd, upArrowRight);

            // Красная стрелка вправо (ось X)
            Gizmos.color = Color.red;
            Vector3 rightArrowEnd = footPrint.position + Vector3.right * 0.3f;
            Gizmos.DrawLine(footPrint.position, rightArrowEnd);
            // Стрелка
            Vector3 rightArrowUp = rightArrowEnd + (Vector3.up + Vector3.left) * 0.1f;
            Vector3 rightArrowDown = rightArrowEnd + (Vector3.down + Vector3.left) * 0.1f;
            Gizmos.DrawLine(rightArrowEnd, rightArrowUp);
            Gizmos.DrawLine(rightArrowEnd, rightArrowDown);

            // Синий кружок в центре
            Gizmos.color = new Color(0, 0.5f, 1, 0.5f);
            Gizmos.DrawWireSphere(footPrint.position, 0.08f);
        }

        // Текущая позиция ладони
        if (Application.isPlaying && palm != null)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.8f);
            Gizmos.DrawWireSphere(palm.position, 0.1f);
        }
    }
}