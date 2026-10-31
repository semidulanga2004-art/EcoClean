# EcoClean park environment

Drag **EcoCleanPark.prefab** into your game's Hierarchy once. Keep its position at (0, 0, 0), rotation at (0, 0, 0), and scale at (1, 1, 1) when using the supplied spawn marker. Add your player, follow camera, and gameplay trash separately.

The main prefab contains the entire park layout: paths, ground, trees, vegetation, furniture, sorting area, colliders, boundary, fountain particles, ripples, and positional water audio. Existing decorative litter remains scenery; it has no waste collection logic. The main prefab already includes the fountain; do not also add WorkingFountain to the same location.

## Folder contents

- `EcoCleanPark.prefab`: the assembled environment.
- `Prefabs/WorkingFountain.prefab`: a separately reusable fountain.
- `Environment/`: custom models, textures, materials, meshes, audio, runtime script, saved scenes and editor tools.
- `ThirdParty/Palmov Island/`: the imported Low Poly Environment Park asset pack and its original materials, textures and models.
- `Environment/Scenes/Park.unity`: the saved ground-level preview with its camera, sun and scene lighting settings.
- `UNITY_VALIDATION.txt`: native Unity validation result, generated at the next editor import.

The first Unity refresh also generates standalone PicnicTable and RecyclingBinPaper prefabs. The main prefab already includes those objects before that refresh.

## Team integration

1. Pull this branch into the team's Unity **6000.3.27f1** project, with **URP 17.3.0** available.
2. Keep this entire folder **and all `.meta` files**, including `Assets/EcoCleanParkEnvironment.meta`. Asset GUIDs were preserved during reorganization so existing scenes retain their links.
3. Let Unity import and compile. Read `UNITY_VALIDATION.txt`; it should start with `PASS`. If it says `FAILED`, inspect the message before integration. You can rerun **EcoClean > Rebuild And Validate Environment Prefabs**.
4. Drag `EcoCleanPark.prefab` into the gameplay scene. Use the teammate's player camera and an AudioListener. Place trash objects as separate gameplay objects on the park.

Copying the prefab file alone is insufficient: its models, textures, materials, script and sound are in this folder. No scene camera, AudioListener or sun is included in the main prefab. Lighting, fog and skybox are scene settings; use the preview scene as a visual reference or copy its settings into the main game scene. The same lighting will not automatically transfer by dragging a prefab.

## Sources and editing

Editable Blender sources remain at `SourceArt/Park/` at repository root. They are not required for the prefab to run, but should stay in the GitHub submission. The Blender layout is the earlier custom layout; the final landscaping and imported pack integration were finished in Unity.

Third-party attribution: **Low Poly Environment Park**, **Palmov Island**, Unity Asset Store package **242702**. Keep the original pack documentation and comply with its license. Share these project assets with your authorized project team; this folder is not an independent asset-library release.

The editor scene generators were updated to the new paths. **Build Park Environment** regenerates the earlier custom scene and can replace the polished park. Use the existing main prefab for integration. If you intentionally edit and save the preview scene, rerun **Rebuild And Validate Environment Prefabs** to update the handoff; it replaces the generated prefab contents from that saved scene.

## GitHub

Stage the new folder, the removal of the old `Assets/EcoCleanPark` and `Assets/Palmov Island` locations, and the updated SourceArt scripts/docs together. Git can recognize the moves. Keep `.meta` files. Do not commit Library, Temp or Logs. No commit or push was performed by this packaging step.
