# Ghost Mode

Асинхронное соревнование с предыдущим заездом. После финиша заезд сохраняется, при следующем запуске полупрозрачный призрак воспроизводит его маршрут.

## Файлы

### `GhostData.cs` — структуры данных

Два класса для хранения заезда:
- `GhostFrame` — один снимок: время (`float time`), позиция (`Vector3`), поворот (`Quaternion`)
- `GhostData` — полный заезд: список кадров + итоговое время. Сериализуется в JSON через `JsonUtility`

### `GhostRecorder.cs` — запись заезда

Attach на `[GhostSystem]`. Требует ссылку на `Transform` каяка игрока.

- Каждые `recordEveryNFrames` (по умолчанию 2) тиков `FixedUpdate` добавляет `GhostFrame` в список
- `StartRecording()` — очищает список и запускает запись
- `StopRecording(finalTime)` — останавливает запись, возвращает `GhostData`
- `ResetRecording()` — сбрасывает без сохранения

### `GhostPlayback.cs` — воспроизведение призрака

Attach на `[Ghost Kayak]` (дубликат каяка без физики). Объект должен быть **неактивен** по умолчанию.

- `StartPlayback(GhostData)` — если данные есть, активирует объект и начинает воспроизведение. Если данных нет — оставляет объект неактивным
- В `Update` интерполирует позицию и поворот между кадрами через `Vector3.Lerp` / `Quaternion.Slerp`
- `StopPlayback()` — деактивирует объект

### `GhostManager.cs` — синглтон-оркестратор

Attach на `[GhostSystem]`. Хранит ссылки на `GhostRecorder` и `GhostPlayback`.

- При `Awake` загружает файл заезда для текущей сцены
- `StartRace()` — запускает запись + воспроизведение предыдущего заезда
- `FinishRace(finalTime)` — останавливает запись, сохраняет заезд (всегда перезаписывает последним)
- `ResetRace()` — сбрасывает запись и скрывает призрак

**Сохранение:** файл называется по имени сцены — `ghost_LakesAndRivers.json`, `ghost_SampleScene.json` и т.д.

| Режим | Путь |
|---|---|
| Редактор | `Assets/GhostData/ghost_<SceneName>.json` |
| Билд | `Application.persistentDataPath/ghost_<SceneName>.json` |

Переключение между путями происходит автоматически через `#if UNITY_EDITOR`.

## Изменения в существующих скриптах

**`MenuManager.cs`**
- `OnStartGameButton()` — добавлен вызов `GhostManager.Instance.StartRace()`
- `OnRestartButton()` — добавлен вызов `GhostManager.Instance.ResetRace()` перед перезагрузкой сцены

**`CheckpointManager.cs`**
- `OnFinish()` — добавлен вызов `GhostManager.Instance.FinishRace(dashboard.CurrentTime)`

## Объекты на сцене

| GameObject | Компоненты | Назначение |
|---|---|---|
| `[GhostSystem]` | `GhostManager`, `GhostRecorder` | Логика записи и управления |
| `[Ghost Kayak]` | `GhostPlayback` + иерархия меша каяка | Визуальный призрак |

`[Ghost Kayak]` — дубликат каяка без `Rigidbody`, коллайдеров и скриптов управления. По умолчанию **неактивен**. Материал — полупрозрачный (`URP/Lit`, Surface Type: Transparent).
