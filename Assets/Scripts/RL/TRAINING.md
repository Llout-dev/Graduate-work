# Обучение WolfRL (ML-Agents 4.x)

## Настройка сцены (Unity)

1. **Дубликат волка для RL** (отдельный префаб или копия GOAP-волка):
   - `SimpleHunger` — общий с GOAP (голод 0–100).
   - `WolfRLAgent` + `Behavior Parameters` + `Decision Requester`.
   - `Collider2D` (Is Trigger) + `Rigidbody2D` (Kinematic, если нужно).
   - Тег **Food** на объектах с `FoodBehaviour`.

2. **Отключите GOAP-компоненты** на RL-волке:
   - `WolfBrain`, `AgentBehaviour`, `AgentMovement2D`.

3. **Behavior Parameters**:
   | Поле | Значение |
   |------|----------|
   | Behavior Name | `WolfRL` (как в YAML) |
   | Vector Observation → Space Size | **4** |
   | Actions → Continuous Actions | **2** |
   | Behavior Type | Default (обучение) / Heuristic Only (ручной тест) |

4. **Decision Requester**: Decision Period = 5 (подберите под FPS).

5. **Max Step** на `WolfRLAgent`: например 5000 (таймаут эпизода).

6. Поля **Arena Half Extents** / **Max Food Distance** в `WolfRLAgent` согласуйте с размером арены.

## Python (3.10)

```powershell
python -m venv .venv-mlagents
.\.venv-mlagents\Scripts\Activate.ps1
pip install --upgrade pip
pip install mlagents==1.1.0 torch
```

Для Unity Package `com.unity.ml-agents` **4.0.3** используйте Python-пакет **mlagents 1.1.0** ([таблица совместимости](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Migrating.md)).

## Запуск обучения

Из корня проекта (где лежит `Assets`):

```powershell
cd "C:\Users\Ксюша\Yandex.Disk\Unity\Диплом и практика"
mlagents-learn "Assets\Scripts\RL\WolfRL.yaml" --run-id=WolfRL_01
```

Дождитесь строки *Press the Play button in the Unity Editor*.

1. Откройте сцену с RL-волком.
2. **Play** в Unity.
3. В консоли Python появятся шаги и TensorBoard-логи в `results/WolfRL_01`.

Остановка: Ctrl+C в терминале, затем Stop в Unity.

## Продолжение / резюме

```powershell
mlagents-learn "Assets\Scripts\RL\WolfRL.yaml" --run-id=WolfRL_01 --resume
```

## TensorBoard

```powershell
tensorboard --logdir results
```

Откройте http://localhost:6006 — смотрите `Environment/Cumulative Reward`.

## Inference (после обучения)

1. Экспорт модели: `results/WolfRL_01/WolfRL.onnx` (появится после чекпоинта).
2. Перетащите `.onnx` в `Assets/MLModels/`.
3. На `Behavior Parameters`: Behavior Type = **Inference Only**, Model = ваш ONNX.

## Наблюдения и награды

| Наблюдение | Описание |
|------------|----------|
| 0 | Голод / 100 |
| 1–2 | Позиция X, Y / полуразмер арены |
| 3 | Расстояние до ближайшей еды / maxFoodDistance |

| Событие | Награда |
|---------|---------|
| Съел еду | +5 |
| Рост голода (на 1 пункт шкалы 0–100) | −0.01 |
| Голод = 100 (смерть) | −5, конец эпизода |

## Сравнение с GOAP

- Одинаковые: `SimpleHunger`, `FoodBehaviour`, тег Food, скорость ~2.
- GOAP: планировщик + `EatTargetSensor`; RL: политика PPO по вектору наблюдений.
