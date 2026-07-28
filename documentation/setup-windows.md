# OpenFrame Unity simulator setup (Windows)

## Requirements

- Windows 10 or 11
- Unity Hub
- Unity Editor `6000.5.1f1` (Unity 6)
- Git or GitHub Desktop

No phone, networking, or Bluetooth connection is required for this prototype.

## Open the project

1. Clone the `OpenFrame` repository or fetch the latest branches in GitHub Desktop.
2. Check out `feature/unity-notification-prototype`.
3. In Unity Hub, select **Add > Add project from disk**.
4. Select the repository's `Unitysim` folder, not the repository root.
5. Open the project with Unity `6000.5.1f1`.
6. Open `Assets/OpenFrame/Scenes/OpenFrameSimulator.unity`.

If the generated scene or prefab needs to be rebuilt, select
**OpenFrame > Build Simulator Scene** in the Unity menu.

## Test the simulator

1. Open the `OpenFrameSimulator` scene.
2. Enter Play mode.
3. Press **N** to show the next notification from
   `Assets/OpenFrame/Data/mock-notifications.json`.
4. Continue pressing **N** to cycle through the mock notifications.
5. Press **D** to dismiss the visible notification.
6. Use **WASD** to move and the mouse to look around.
7. Press **Escape** to release the mouse cursor.

Each notification card should show its source application, title, message body,
and timestamp. The card is a reusable prefab at
`Assets/OpenFrame/Prefabs/NotificationCard.prefab`.

## Editor validation

Select **OpenFrame > Validate Simulator**. A successful validation writes
`OPENFRAME_VALIDATION_PASSED` to the Unity Console.

For an automated command-line check from PowerShell:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "<path-to-repository>\Unitysim" `
  -executeMethod OpenFrame.Editor.OpenFrameSimulatorBuilder.BuildAndValidate `
  -logFile "openframe-validation.log"
```

The command succeeds with exit code `0` and the log contains
`OPENFRAME_VALIDATION_PASSED`.
