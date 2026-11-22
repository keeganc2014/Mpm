# Gtag-style map test (offline)

Open this folder in Unity Hub with **Unity 6 (6000.0 LTS)** (add the Android Build Support module if you want a Quest build).

1. First open: Unity downloads the XR packages and creates `Assets/Scenes/Main.unity` by itself.
2. Press Play. The scene builds itself at runtime: map, lighting, gorilla rig, menu. Nothing to drag into the scene.
   - No headset: right-mouse-drag = look, WASD = move, Space = jump, Shift = fly, Q/E = platforms, Tab = menu (aim at a button + left click).
3. Quest build (local): menu **Gtag Test > Configure for Quest (Android)** (also enables the Oculus XR loader), then File > Build Settings > Build.
   Or headless: `Unity -batchmode -quit -projectPath . -executeMethod BuildSetup.BuildAndroid` -> `Builds/GtagMapTest.apk`.

## Unity Build Automation (Cloud Build) -> APK
1. Push this folder's *contents* to the root of a Git repo (or set "Project subdirectory" in the build target). `.gitignore` is included.
2. Create a build target: platform **Android**, Unity version **6000.0.23f1** (or the closest 6000.0 LTS offered).
3. Advanced settings -> **Pre-export method name**: `BuildSetup.PreExport`
   (creates the scene, applies IL2CPP/ARM64/Vulkan/Linear color space, assigns the Oculus loader).
4. Build. Download the APK and sideload it to the Quest (`adb install GtagMapTest.apk`).
   No keystore is needed for sideloading; add one in the target's credentials for a Store/release build.

## In VR
- Left X button: open/close the **Main Menu** on your left hand. Poke a button with your right hand + pull right trigger.
  - **Mods**: Speed, Gravity, **Long Arms** (1.5x/2x/3x), Fly, Noclip, Platforms, **Invincible**, **Freeze Monster**, Flashlight, Darkness, Random Spot, Save/Load Pos, Reset Mods
  - **Settings**: Turn speed, Monster on/off, Monster speed, Reset Board
  - **Player**: Name and Color (shown on your gorilla and on the leaderboard)
  - Go to Stump, Close
- Right thumbstick: smooth turn.
- **Monster**: a shadow monster floats after you. If it touches you: "YOU DIED", you are reset to the stump, your time is saved.
- **Leaderboard**: shown on the map's board (the object named scoreboard/board); longest times alive, with name + color.
- Spawn is the stump (around the map's computer object).

## Player rig (your Blender model)
Unity Cloud has no Blender, so convert once on a PC with Blender:
`blender -b _Originals/Gorilla_IK_Rig.blend -P Tools/export_rig_fbx.py` -> writes `Assets/Resources/Rig/GorillaRig.fbx`.
The game loads that FBX automatically. Until it exists, a built-in gorilla (body + stretchy arms) is used.

## Files
- `Assets/Scripts/GorillaLocomotion/` - your Player.cs + Surface.cs (unchanged)
- `Assets/Scripts/GtagBootstrap.cs` - builds the scene, loads the map, spawns the rig with the original prefab's tuning values
- `Assets/Editor/BuildSetup.cs` - Quest/XR build configuration used by the menu, the batch build and Cloud Build
- `Assets/Scripts/XRRigInput.cs` - headset/controller input (+ desktop fallback)
- `Assets/Scripts/MainMenu.cs` - combined main menu (mods + settings + player)
- `Assets/Scripts/Monster.cs`, `Leaderboard.cs`, `PlayerRigModel.cs`, `PlayerProfile.cs` - monster, board, gorilla model, saved settings
- `Tools/export_rig_fbx.py` - Blender -> FBX export for the rig
- `Assets/Resources/Map/` - map FBX + textures (30 textures in your zip; the FBX references a few more that weren't included, those materials stay plain)
- `_Originals/` - your original unitypackage (outside Assets, ignored by Unity)

Poison_Menu.dll is intentionally not in the project: it hooks into the real game's code and can't load in a standalone Unity project.
