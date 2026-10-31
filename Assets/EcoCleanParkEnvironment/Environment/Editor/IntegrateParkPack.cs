using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;
[InitializeOnLoad] public static class IntegrateParkPack {
 const string Pack="Assets/EcoCleanParkEnvironment/ThirdParty/Palmov Island/Low Poly Environment Park";
 static IntegrateParkPack(){EditorApplication.delayCall+=TryRun;}
 static void CleanImportedInstances(){
  if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
  var root=GameObject.Find("EcoClean Park Environment");if(!root)return;
  int removed=0;
  foreach(var t in root.GetComponentsInChildren<Transform>(true))removed+=GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
  if(removed>0)EditorSceneManager.SaveScene(root.scene);
  File.WriteAllText("ParkPreviews/missing-script-check.txt","Missing components removed from scene instances: "+removed+". Imported source files retained.");
 }
 static void TryRun(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=TryRun;return;}
  if(!File.Exists("ParkPreviews/INTEGRATE_REQUEST"))return;
  File.Delete("ParkPreviews/INTEGRATE_REQUEST");Run();
 }
 static Bounds BoundsOf(GameObject g){var b=new Bounds(g.transform.position,Vector3.zero);bool first=true;foreach(var r in g.GetComponentsInChildren<Renderer>()){if(first){b=r.bounds;first=false;}else b.Encapsulate(r.bounds);}return b;}
 [MenuItem("EcoClean/Integrate Imported Park Pack")]
 public static void Run(){
  if(EditorApplication.isPlaying)return;
  var root=GameObject.Find("EcoClean Park Environment");if(!root)return;
  if(Enumerable.Range(0,root.transform.childCount).Any(i=>root.transform.GetChild(i).name.StartsWith("Palmov "))){Debug.Log("Park pack is already integrated.");return;}
  var scene=root.scene;
  EditorSceneManager.SaveScene(scene,"Assets/EcoCleanParkEnvironment/Environment/Scenes/Park_CustomBackup.unity",true);
  var sourceMat=AssetDatabase.LoadAssetAtPath<Material>(Pack+"/Materials/mat main.mat");
  if(!sourceMat)throw new System.Exception("Pack material not imported.");
  string mp="Assets/EcoCleanParkEnvironment/Environment/Materials/Palmov Park URP.mat";
  var material=AssetDatabase.LoadAssetAtPath<Material>(mp);
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,mp);}
  material.SetTexture("_BaseMap",sourceMat.GetTexture("_MainTex"));material.color=Color.white;material.SetFloat("_Smoothness",.05f);material.enableInstancing=true;
  int count=0;
  var oldObjects=Enumerable.Range(0,root.transform.childCount).Select(i=>root.transform.GetChild(i).gameObject).ToArray();
  foreach(var old in oldObjects){
   string rel=null;
   if(old.name=="TreeOak")rel="Trees/tree oak";
   if(old.name=="TreeLime")rel="Trees/spherical tree";
   if(old.name=="TreeDark")rel="Trees/deciduous tree";
   if(old.name=="Bench")rel="Environment/bench 1";
   if(old.name=="Rock")rel="Stones/stone 1";
   if(rel==null)continue;
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Pack+"/Prefabs/"+rel+".prefab");if(!asset)throw new System.Exception("Missing pack prefab "+rel);
   var priorBounds=BoundsOf(old);var p=old.transform.position;
   var source=PrefabUtility.GetCorrespondingObjectFromSource(old);
   var yaw=source?old.transform.rotation*Quaternion.Inverse(source.transform.rotation):Quaternion.identity;
   var obj=(GameObject)PrefabUtility.InstantiatePrefab(asset);obj.transform.SetParent(root.transform);obj.transform.position=p;obj.transform.rotation=yaw*asset.transform.rotation;
   var b=BoundsOf(obj);float factor=old.name=="Bench"?2.25f/Mathf.Max(.01f,b.size.x):priorBounds.size.y/Mathf.Max(.01f,b.size.y);
   obj.transform.localScale*=factor;b=BoundsOf(obj);obj.transform.position+=new Vector3(p.x-b.center.x,p.y-b.min.y,p.z-b.center.z);
   foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>material).ToArray();
   foreach(var f in obj.GetComponentsInChildren<MeshFilter>()){if(!f.GetComponent<Collider>()){var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;}}
   obj.isStatic=true;obj.name="Palmov "+asset.name;Object.DestroyImmediate(old);count++;
  }
  // Reuse the pack's grass around the planting borders and lawn.
  var grass=AssetDatabase.LoadAssetAtPath<GameObject>(Pack+"/Prefabs/Grass/grass 1.prefab");
  var random=new System.Random(93);
  for(int i=0;i<90;i++){
   float a=(float)random.NextDouble()*Mathf.PI*2;float radius=i<30?8.8f:17+(float)random.NextDouble()*4;
   Vector3 p=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
   if(Mathf.Abs(p.x)<3||Mathf.Abs(p.z)<2||(p.z>8&&p.z<18&&Mathf.Abs(p.x)>9))continue;
   var g=(GameObject)PrefabUtility.InstantiatePrefab(grass);g.transform.SetParent(root.transform);g.transform.position=p;g.transform.Rotate(0,(float)random.NextDouble()*360,0);
   var b=BoundsOf(g);g.transform.localScale*=.3f/Mathf.Max(.01f,b.size.y);b=BoundsOf(g);g.transform.position+=Vector3.up*(-b.min.y);
   foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>material).ToArray();g.isStatic=true;count++;
  }
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,"Assets/EcoCleanParkEnvironment/Environment/Scenes/Park.unity");
  File.WriteAllText("ParkPreviews/pack-integration.txt",count+" park-pack scenery instances integrated. Source pack material retained; compatible URP material created separately. Custom-only scene backed up as Park_CustomBackup.unity.");
  var cam=Camera.main;ParkBuilder.Capture(cam,"park-pack-overview",new Vector3(0,35,-45),new Vector3(0,0,1));ParkBuilder.Capture(cam,"park-pack-entrance",new Vector3(0,2.5f,-21),new Vector3(0,1,1));cam.transform.position=new Vector3(0,35,-45);cam.transform.LookAt(new Vector3(0,0,1));EditorSceneManager.SaveScene(scene);
 }
}
