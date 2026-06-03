# Подключение лидерборда к сцене

## Требования

В сцене должны присутствовать:

| Объект | Что делает |
|---|---|
| `RaceManager` | Владеет состоянием гонки, запускает отсчёт, фиксирует финиш |
| `CheckpointManager` | Считает пропущенные чекпоинты, записывает результат в лидерборд |
| `FinishLineTrigger` | Коллайдер на финишной линии, вызывает `RaceManager.OnTriggerFinishLine()` |
| `MenuManager` | Управляет панелями (Panel_Finish / Game Over / Pause) |

Эталон настройки — `LakesAndRivers_copy.unity`.

---

## Шаги

### 1. Добавь префаб на сцену

Перетащи `Assets/Prefabs/LeaderBoard UI/LeaderboardCanvas.prefab` в иерархию сцены.

### 2. Подключи камеру в LeaderboardCanvas

Выдели `LeaderboardCanvas` на сцене → компонент **UIFollowHead** → поле **Head Camera** → назначь камеру VR-персонажа (обычно `Main Camera` внутри `XR Origin (VR)`).

Без этого канвас не будет следовать за головой игрока в VR.

### 3. Подключи в MenuManager

Выдели объект с компонентом `MenuManager` → в инспекторе поле **Leaderboard UI** → назначь `LeaderboardCanvas` со сцены.

`MenuManager.ShowVictory` автоматически вызовет `leaderboardUI.Show(sceneName, playerName)` — лидерборд появится внутри `Panel_Finish` после финиша.

### 3. Убедись что профиль выбран до входа в сцену

Запись в лидерборд происходит только если `PlayerProfile.HasName == true`.  
Профиль выбирается в хабе (`GayHub.unity`) через `ProfileSelectionCanvas`.  
Если игрок попадает в сцену напрямую (минуя хаб) — запись будет пропущена с варнингом в консоль, краша не будет.

---

## Ключ лидерборда

Результаты хранятся по имени сцены: `Application.persistentDataPath/leaderboards/{имя_сцены}.json`

**Важно:** переименование сцены сбрасывает историю результатов.