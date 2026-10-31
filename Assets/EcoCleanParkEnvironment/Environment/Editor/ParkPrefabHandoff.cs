using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Validate the handoff once after the editor imports this folder. Never changes the open scene.
[InitializeOnLoad]
public static class ParkPrefabHandoff
{
    const string Folder = "Assets/EcoCleanParkEnvironment";
    const string ScenePath = Folder + "/Environment/Scenes/Park.unity";
    const string Main = Folder + "/EcoCleanPark.prefab";
    const string Pending = Folder + "/VALIDATE_ON_IMPORT.txt";
    static ParkPrefabHandoff() { EditorApplication.delayCall += TryValidate; }
    static void TryValidate()
    {
        if (!File.Exists(Pending)) return;
        if (EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += TryValidate; return; }
        File.Delete(Pending);
        try { RebuildAndValidate(); }
        catch (Exception e)
        {
            File.WriteAllText(Folder + "/UNITY_VALIDATION.txt", "FAILED\n" + e);
            Debug.LogException(e);
        }
    }
    [MenuItem("EcoClean/Rebuild And Validate Environment Prefabs")]
    public static void RebuildAndValidate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var preview = EditorSceneManager.OpenPreviewScene(ScenePath);
        GameObject check = null;
        Scene checkScene = default;
        try
        {
            var source = preview.GetRootGameObjects().Single(g => g.name == "EcoClean Park Environment");
            var copy = UnityEngine.Object.Instantiate(source);
            SceneManager.MoveGameObjectToScene(copy, preview);
            copy.name = "EcoCleanPark";
            copy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            copy.transform.localScale = Vector3.one;
            PrefabUtility.SaveAsPrefabAsset(copy, Main, out bool saved);
            if (!saved) throw new Exception("Could not save the environment prefab.");
            UnityEngine.Object.DestroyImmediate(copy);

            // Standalone reusable props; the main prefab already contains their scene placements.
            foreach (var pair in new[] { new[] { "Fountain", "WorkingFountain" }, new[] { "PicnicTable", "PicnicTable" }, new[] { "BinPaper", "RecyclingBinPaper" } })
            {
                var prop = source.GetComponentsInChildren<Transform>(true).First(t => t.name == pair[0]);
                var item = UnityEngine.Object.Instantiate(prop.gameObject);
                SceneManager.MoveGameObjectToScene(item, preview);
                if (PrefabUtility.IsPartOfPrefabInstance(item))
                    PrefabUtility.UnpackPrefabInstance(item, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                item.name = pair[1]; item.transform.position = Vector3.zero;
                // Keep imported unit scale and model-axis rotation.
                PrefabUtility.SaveAsPrefabAsset(item, Folder + "/Prefabs/" + pair[1] + ".prefab", out bool propSaved);
                UnityEngine.Object.DestroyImmediate(item);
                if (!propSaved) throw new Exception("Could not save " + pair[1]);
            }

            checkScene = EditorSceneManager.NewPreviewScene();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Main);
            check = (GameObject)PrefabUtility.InstantiatePrefab(asset, checkScene);
            var all = check.GetComponentsInChildren<Transform>(true);
            int missing = all.Sum(t => t.GetComponents<Component>().Count(c => c == null));
            int badMeshes = check.GetComponentsInChildren<MeshFilter>(true).Count(m => !m.sharedMesh);
            int badMaterials = check.GetComponentsInChildren<Renderer>(true).Count(r => r.sharedMaterials.Any(m => !m || !m.shader || m.shader.name.Contains("InternalError")));
            var audio = check.GetComponentsInChildren<AudioSource>(true);
            var particles = check.GetComponentsInChildren<ParticleSystem>(true);
            var fountains = check.GetComponentsInChildren<EcoClean.Environment.ParkFountain>(true);
            var outside = AssetDatabase.GetDependencies(Main, true).Where(p => p.StartsWith("Assets/") && !p.StartsWith(Folder + "/")).ToArray();
            if (missing != 0 || badMeshes != 0 || badMaterials != 0 || outside.Length != 0 || audio.Length != 1 || !audio[0].clip || !audio[0].loop || !audio[0].playOnAwake || particles.Length != 19 || fountains.Length != 1 || fountains[0].ripples.Any(r => !r))
                throw new Exception($"Invalid handoff: missing scripts={missing}, meshes={badMeshes}, materials={badMaterials}, audio sources={audio.Length}, particles={particles.Length}, outside dependencies={string.Join(",", outside)}");
            if (check.GetComponentsInChildren<Camera>(true).Length != 0 || check.GetComponentsInChildren<AudioListener>(true).Length != 0 || check.GetComponentsInChildren<Light>(true).Length != 0)
                throw new Exception("The environment must not introduce a player camera, listener, or sun.");
            if (all.Length != source.GetComponentsInChildren<Transform>(true).Length)
                throw new Exception("Prefab hierarchy differs from the saved source scene.");
            AssetDatabase.SaveAssets();
            File.WriteAllText(Folder + "/UNITY_VALIDATION.txt", $"PASS - Unity {Application.unityVersion}\nCreated using Unity PrefabUtility and instantiated in an isolated preview scene.\nTransforms: {all.Length}\nColliders: {check.GetComponentsInChildren<Collider>(true).Length}\nWater particle systems: {particles.Length}\nFountain audio: assigned, loop enabled, play on awake enabled\nMissing scripts / meshes / materials: 0 / 0 / 0\nAsset dependencies outside handoff folder: 0\nPreview cameras / audio listeners / lights: 0 / 0 / 0\nThis checks prefab import and references, not gameplay or runtime performance.\n");
            Debug.Log("EcoClean park prefab handoff validated. See " + Folder + "/UNITY_VALIDATION.txt");
        }
        finally
        {
            if (checkScene.IsValid()) EditorSceneManager.ClosePreviewScene(checkScene);
            EditorSceneManager.ClosePreviewScene(preview);
        }
    }
}
