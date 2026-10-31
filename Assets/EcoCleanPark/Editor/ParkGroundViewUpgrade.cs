using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using System.Collections.Generic;

[InitializeOnLoad] public static class ParkGroundViewUpgrade
{
 const string A="Assets/EcoCleanPark";
 static Transform detail;
 static System.Random rand;
 static ParkGroundViewUpgrade(){EditorApplication.delayCall+=TryRun;EditorApplication.delayCall+=FinishRequested;}
 static void FinishRequested(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=FinishRequested;return;}
  if(!File.Exists("ParkPreviews/GROUND_POLISH_REQUEST")||EditorApplication.isPlayingOrWillChangePlaymode)return;
  File.Delete("ParkPreviews/GROUND_POLISH_REQUEST");FinishPolish();
 }
 static void FinishPolish(){
  var root=GameObject.Find("EcoClean Park Environment");if(!root)return;detail=root.transform.Find("Ground-view landscaping");rand=new System.Random(441);
  var path=AssetDatabase.LoadAssetAtPath<Material>(A+"/Materials/Fine gravel path.mat");
  foreach(string name in new[]{"Circular walking path","Inner curb","Outer curb","Plaza edging"}){
   var g=root.transform.Find(name).gameObject;g.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(A+"/Meshes/"+name+".asset");
   if(name=="Circular walking path"){g.transform.position=Vector3.up*.025f;Planar(g,path);}else g.transform.position=Vector3.zero;
  }
  var plaza=root.transform.Find("Fountain plaza");plaza.position=Vector3.up*.015f;Planar(plaza.gameObject,path);
  foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="South main walk"||t.name=="North main walk")){
   t.localScale=new Vector3(4,.05f,19.3f);t.position=new Vector3(0,.025f,t.position.z>0?15.35f:-15.35f);Planar(t.gameObject,path);
  }
  var blades=detail.Find("Lawn grass detail").GetComponent<MeshFilter>().sharedMesh;blades.normals=Enumerable.Repeat(Vector3.up,blades.vertexCount).ToArray();EditorUtility.SetDirty(blades);
  var green=AssetDatabase.LoadAssetAtPath<Material>(A+"/Materials/Grass blade green.mat");green.color=new Color(.38f,.52f,.18f);
  for(int i=0;i<3;i++){
   var bin=root.transform.Find(new[]{"BinPaper","BinPlastic","BinMetal"}[i]);var src=PrefabUtility.GetCorrespondingObjectFromSource(bin.gameObject);bin.rotation=Quaternion.Euler(0,180,0)*src.transform.rotation;
   var label=root.transform.Find(new[]{"PAPER","PLASTIC","METAL"}[i]);label.GetComponent<TextMesh>().color=new Color(.025f,.07f,.04f);label.GetComponent<TextMesh>().characterSize=.028f;
  }
  foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Litter placement")))t.localScale=Vector3.one*75;
  // Small cloud clusters provide a sky background at walking height.
  var cloud=Mat("Cloud white",new Color(.94f,.96f,.97f));
  for(int i=0;i<(detail.Find("Distant cloud")?0:8);i++){
   float a=i*Mathf.PI/4;Vector3 c=new Vector3(Mathf.Cos(a)*95,R(23,33),Mathf.Sin(a)*95);
   for(int j=0;j<3;j++){
    var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Distant cloud";g.transform.SetParent(detail);g.transform.position=c+new Vector3(j*4,R(-.5f,.5f),0);g.transform.localScale=new Vector3(R(8,12),R(2,3),R(5,8));Object.DestroyImmediate(g.GetComponent<Collider>());var r=g.GetComponent<Renderer>();r.sharedMaterial=cloud;r.shadowCastingMode=ShadowCastingMode.Off;
   }
  }
  var patio=root.transform.Find("Picnic terrace");patio.position=new Vector3(-16,.025f,6);patio.localScale=new Vector3(6.5f,.05f,10);Planar(patio.gameObject,path);
  var tables=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="PicnicTable").ToArray();for(int i=0;i<tables.Length;i++)tables[i].position=new Vector3(i==0?-16:-16.5f,.06f,3+i*4);
  foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Litter placement")))t.position=new Vector3(R(-18.5f,-14.8f),.07f,R(1.5f,10));
  foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Bush")||t.name.StartsWith("Palmov stone")).ToArray()){
   var b=B(t.gameObject);if(b.center.x>-19.5f&&b.center.x<-12.8f&&b.center.z>1&&b.center.z<11)Object.DestroyImmediate(t.gameObject);
  }
  var cam=Camera.main;cam.transform.position=new Vector3(-16,2.6f,-3);cam.transform.LookAt(new Vector3(-5,1.1f,8));AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(root.scene);
  ParkBuilder.Capture(cam,"park-street-picnic",new Vector3(-16,2.6f,-3),new Vector3(-5,1.1f,8));
  ParkBuilder.Capture(cam,"park-street-entrance",new Vector3(0,2.6f,-20),new Vector3(0,1.1f,0));
  ParkBuilder.Capture(cam,"park-street-sorting",new Vector3(5,2.6f,6),new Vector3(1.6f,1.2f,11.5f));
  cam.transform.position=new Vector3(-16,2.6f,-3);cam.transform.LookAt(new Vector3(-5,1.1f,8));EditorSceneManager.SaveScene(root.scene);
 }
 static void TryRun(){
  if(EditorApplication.isUpdating||EditorApplication.isCompiling){EditorApplication.delayCall+=TryRun;return;}
  if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists("ParkPreviews/GROUND_VIEW_REQUEST"))return;
  File.Delete("ParkPreviews/GROUND_VIEW_REQUEST");
  try{Run();}catch(System.Exception e){File.WriteAllText("ParkPreviews/ground-view-error.txt",e.ToString());Debug.LogException(e);}
 }
 static Material Mat(string name,Color color){
  string p=A+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);
  if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
  m.color=color;m.SetFloat("_Smoothness",.15f);return m;
 }
 static float R(float a,float b)=>Mathf.Lerp(a,b,(float)rand.NextDouble());
 static GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool collision=false){
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(detail);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;
  if(!collision)Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
 }
 static Bounds B(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
 static GameObject CloneProp(GameObject source,Vector3 p,float scale=1){
  var g=Object.Instantiate(source,detail);g.name=source.name+" detail";g.transform.position=p;g.transform.localScale=source.transform.localScale*scale;
  var b=B(g);g.transform.position+=new Vector3(p.x-b.center.x,p.y-b.min.y,p.z-b.center.z);
  foreach(var c in g.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);g.isStatic=true;return g;
 }
 static Texture2D Surface(string name,bool grass){
  Directory.CreateDirectory(A+"/Textures");int s=512;var t=new Texture2D(s,s,TextureFormat.RGB24,false);
  for(int y=0;y<s;y++)for(int x=0;x<s;x++){
   float n=Mathf.PerlinNoise(x*.015f,y*.015f),f=Mathf.PerlinNoise(x*.17f+7,y*.17f+19);float speck=(float)rand.NextDouble();
   Color c=grass?Color.Lerp(new Color(.26f,.39f,.105f),new Color(.41f,.53f,.20f),n*.75f+f*.25f):Color.Lerp(new Color(.64f,.52f,.35f),new Color(.79f,.69f,.49f),n*.5f+f*.5f);
   c*=grass?Mathf.Lerp(.90f,1.06f,speck):Mathf.Lerp(.93f,1.05f,speck);t.SetPixel(x,y,c);
  }
  t.Apply();string p=A+"/Textures/"+name+".png";File.WriteAllBytes(p,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
  var ti=(TextureImporter)AssetImporter.GetAtPath(p);ti.wrapMode=TextureWrapMode.Repeat;ti.filterMode=FilterMode.Trilinear;ti.anisoLevel=8;ti.mipmapEnabled=true;ti.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
 }
 static void Planar(GameObject g,Material mat){
  var filter=g.GetComponent<MeshFilter>();if(!filter)return;var mesh=Object.Instantiate(filter.sharedMesh);mesh.name=g.name+" ground UV";
  var uv=mesh.vertices.Select(v=>{var p=g.transform.TransformPoint(v);return new Vector2(p.x*.45f,p.z*.45f);}).ToArray();mesh.uv=uv;
  string path=A+"/Meshes/GroundView_"+g.name.Replace(" ","_")+".asset";
  var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
  filter.sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=mat;
 }
 static void Text(string label,Transform parent,Vector3 p,float size,Color color){
  var go=new GameObject(label);go.transform.SetParent(parent);go.transform.position=p;var t=go.AddComponent<TextMesh>();t.text=label;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.characterSize=size;t.fontSize=64;t.color=color;
 }
 [MenuItem("EcoClean/Improve Park For Ground View")]
 public static void Run(){
  var root=GameObject.Find("EcoClean Park Environment");if(!root)return;
  if(root.transform.Find("Ground-view landscaping")){Debug.Log("Ground-view landscaping is already installed.");return;}
  EditorSceneManager.SaveScene(root.scene,A+"/Scenes/Park_BeforeGroundView.unity",true);
  rand=new System.Random(318);detail=new GameObject("Ground-view landscaping").transform;detail.SetParent(root.transform);
  var wood=Mat("Shelter timber",new Color(.34f,.20f,.10f));var stone=Mat("Warm limestone",new Color(.61f,.61f,.52f));
  var grass=Mat("Detailed meadow",Color.white);grass.SetTexture("_BaseMap",Surface("meadow-detail",true));
  var path=Mat("Fine gravel path",Color.white);path.SetTexture("_BaseMap",Surface("gravel-detail",false));
  // Align the crossing surfaces and move close-up activity spaces into readable views.
  var ground=root.transform.Find("Ground").gameObject;Planar(ground,grass);
  var surround=root.transform.Find("Landscape surround");surround.localScale=new Vector3(600,.15f,600);Planar(surround.gameObject,grass);
  var picnic=root.transform.Find("Picnic terrace");picnic.position=new Vector3(-12,.025f,6.5f);picnic.localScale=new Vector3(6,.05f,9);
  var tables=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="PicnicTable").ToArray();
  for(int i=0;i<tables.Length;i++)tables[i].position=new Vector3(-12,.06f,4+i*4);
  var sorting=root.transform.Find("Sorting terrace");sorting.position=new Vector3(1.6f,.025f,11);sorting.localScale=new Vector3(6.5f,.05f,5);
  string[] bins={"BinPaper","BinPlastic","BinMetal"};string[] words={"PAPER","PLASTIC","METAL"};
  for(int i=0;i<3;i++){
   var g=root.transform.Find(bins[i]);g.position=new Vector3(i*1.6f,.06f,11.5f);
   var label=root.transform.Find(words[i]);if(label){label.position=new Vector3(i*1.6f,.91f,11.07f);label.GetComponent<TextMesh>().characterSize=.039f;}
  }
  // An open shelter frames the sorting destination without hiding its labels.
  foreach(float x in new[]{-1.0f,4.2f})Box("Sorting shelter post",new Vector3(x,1.25f,12),new Vector3(.15f,2.5f,.15f),wood,true);
  Box("Sorting shelter roof",new Vector3(1.6f,2.52f,11.65f),new Vector3(5.65f,.18f,1.65f),wood);
  Box("Sorting shelter header",new Vector3(1.6f,2.23f,11.0f),new Vector3(5.5f,.31f,.13f),wood);
  Text("SORT • RECYCLE • RESTORE",detail,new Vector3(1.6f,2.23f,10.91f),.052f,new Color(.95f,.94f,.83f));
  var children=Enumerable.Range(0,root.transform.childCount).Select(i=>root.transform.GetChild(i).gameObject).ToArray();
  foreach(var g in children){
   if(g.name.Contains("walk")||g.name.Contains("crossing")||g.name.Contains("terrace")||g.name=="Fountain plaza")Planar(g,path);
   if(g.name=="South main walk"){g.transform.position=new Vector3(0,.025f,-15.55f);g.transform.localScale=new Vector3(4,.05f,18.9f);Planar(g,path);}
   if(g.name=="North main walk"){g.transform.position=new Vector3(0,.025f,15.55f);g.transform.localScale=new Vector3(4,.05f,18.9f);Planar(g,path);}
   if(g.name=="East crossing"||g.name=="West crossing"){g.transform.position=new Vector3(g.name=="East crossing"?10.25f:-10.25f,.025f,0);g.transform.localScale=new Vector3(8.2f,.05f,3);Planar(g,path);}
   if(g.name=="Circular walking path"||g.name=="Inner curb"||g.name=="Outer curb"||g.name=="Plaza edging"){
    var f=g.GetComponent<MeshFilter>();var mesh=Object.Instantiate(f.sharedMesh);var tri=mesh.triangles;var verts=mesh.vertices;var keep=new List<int>();
    for(int j=0;j<tri.Length;j+=3){var p=(verts[tri[j]]+verts[tri[j+1]]+verts[tri[j+2]])/3;if(Mathf.Abs(p.x)<2.05f||Mathf.Abs(p.z)<1.55f)continue;keep.AddRange(new[]{tri[j],tri[j+1],tri[j+2]});}
    mesh.triangles=keep.ToArray();mesh.RecalculateNormals();string mp=A+"/Meshes/GroundView_"+g.name.Replace(" ","_")+"_trim.asset";AssetDatabase.CreateAsset(mesh,mp);f.sharedMesh=mesh;
    if(g.name=="Circular walking path"){g.transform.position=Vector3.up*.025f;Planar(g,path);}
   }
  }
  // Reset the relocated litter into the picnic space, ready for gameplay integration.
  foreach(var g in children.Where(g=>g.name.StartsWith("Litter placement")))g.transform.position=new Vector3(R(-14.5f,-9.5f),.07f,R(2.5f,9.5f));
  // Low, continuous planting at the boundary creates a background from walking height.
  var bush=root.GetComponentsInChildren<Transform>().First(t=>t.name=="Bush").gameObject;
  for(int edge=0;edge<4;edge++)for(int i=-13;i<=13;i++){
   float along=i*1.65f;if((edge<2)&&Mathf.Abs(along)<3.1f)continue;
   var p=edge==0?new Vector3(along,0,-23):edge==1?new Vector3(along,0,23):edge==2?new Vector3(-23,0,along):new Vector3(23,0,along);
   CloneProp(bush,p,R(1.05f,1.4f));
  }
  // Behind the bins, a low hedgerow frames labels and separates the outer scenery.
  for(int i=-3;i<=6;i++)CloneProp(bush,new Vector3(i*1.3f,0,14.5f),1.2f);
  var flower=root.GetComponentsInChildren<Transform>().First(t=>t.name=="Flower").gameObject;
  var rock=children.First(g=>g.name.StartsWith("Palmov stone"));
  for(int i=0;i<85;i++){
   float a=R(0,Mathf.PI*2),r=i<45?R(7.9f,10.9f):R(16,21);Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
   if(Mathf.Abs(p.x)<2.8f||Mathf.Abs(p.z)<2.1f||(p.x<-8&&p.z>1&&p.z<12)||(p.x>-2&&p.x<5&&p.z>8))continue;
   CloneProp(flower,p,R(.55f,.9f));if(i%4==0)CloneProp(rock,p+new Vector3(.25f,0,.2f),R(.3f,.6f));
  }
  // Lightweight grass blades are clustered on lawn, never scattered across the paths.
  var tuftMat=Mat("Grass blade green",new Color(.29f,.43f,.13f));var v=new List<Vector3>();var triangles=new List<int>();
  for(int i=0;i<900;i++){
   float x=R(-22,22),z=R(-22,22),r=Mathf.Sqrt(x*x+z*z);
   if(Mathf.Abs(x)<2.5f||Mathf.Abs(z)<1.9f||r<6.4f||(r>11.0f&&r<15.0f)||(x<-8&&z>1&&z<12)||(x>-2&&x<5&&z>8&&z<14))continue;
   for(int b=0;b<3;b++){float h=R(.08f,.25f),w=.035f;Vector3 p=new Vector3(x+R(-.15f,.15f),.015f,z+R(-.15f,.15f));int n=v.Count;v.Add(p+Vector3.left*w);v.Add(p+Vector3.right*w);v.Add(p+new Vector3(R(-.07f,.07f),h,R(-.06f,.06f)));triangles.AddRange(new[]{n,n+2,n+1,n+1,n+2,n});}
  }
  var gm=new Mesh{name="Clustered lawn grass"};gm.SetVertices(v);gm.SetTriangles(triangles,0);gm.RecalculateNormals();AssetDatabase.CreateAsset(gm,A+"/Meshes/GroundView_grass.asset");var gg=new GameObject("Lawn grass detail");gg.transform.SetParent(detail);gg.AddComponent<MeshFilter>().sharedMesh=gm;gg.AddComponent<MeshRenderer>().sharedMaterial=tuftMat;gg.isStatic=true;
  // A second tree layer prevents an empty flat horizon in the street-level camera.
  var trees=children.Where(g=>g.name.StartsWith("Palmov ")&&g.name.Contains("tree")).ToArray();
  for(int i=0;i<55;i++){float a=i*Mathf.PI*2/55;float radius=R(30,48);CloneProp(trees[i%trees.Length],new Vector3(Mathf.Cos(a)*radius,-.1f,Mathf.Sin(a)*radius),R(.8f,1.35f));}
  var sky=new Material(Shader.Find("Skybox/Procedural"));sky.name="Park daylight sky";sky.SetColor("_SkyTint",new Color(.48f,.65f,.85f));sky.SetColor("_GroundColor",new Color(.38f,.48f,.26f));sky.SetFloat("_AtmosphereThickness",.85f);sky.SetFloat("_SunSize",.025f);AssetDatabase.CreateAsset(sky,A+"/Materials/Park daylight sky.mat");RenderSettings.skybox=sky;
  RenderSettings.sun=Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l=>l.type==LightType.Directional);
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.47f,.60f,.74f);RenderSettings.ambientEquatorColor=new Color(.31f,.39f,.25f);RenderSettings.ambientGroundColor=new Color(.20f,.24f,.15f);RenderSettings.sun.intensity=1.5f;
  RenderSettings.fogStartDistance=48;RenderSettings.fogEndDistance=145;RenderSettings.fogColor=new Color(.64f,.76f,.69f);
  var water=AssetDatabase.LoadAssetAtPath<Material>(A+"/Materials/water.mat");water.color=new Color(.24f,.61f,.72f);water.SetFloat("_Smoothness",.65f);
  var cam=Camera.main;cam.name="Park ground-level preview - no character";cam.clearFlags=CameraClearFlags.Skybox;cam.fieldOfView=60;cam.nearClipPlane=.1f;cam.farClipPlane=250;
  SetView(cam,new Vector3(-10,2.6f,-3),new Vector3(-1,1.15f,8));
  foreach(var spec in new[]{("Third-person entrance view",new Vector3(0,2.6f,-20)),("Third-person picnic view",new Vector3(-10,2.6f,-3)),("Third-person sorting view",new Vector3(5,2.6f,6))}){
   var a=new GameObject(spec.Item1);a.transform.SetParent(detail);a.transform.position=spec.Item2;
  }
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(root.scene);
  ParkBuilder.Capture(cam,"park-street-picnic",new Vector3(-10,2.6f,-3),new Vector3(-1,1.15f,8));
  ParkBuilder.Capture(cam,"park-street-entrance",new Vector3(0,2.6f,-20),new Vector3(0,1.1f,0));
  ParkBuilder.Capture(cam,"park-street-sorting",new Vector3(5,2.6f,6),new Vector3(1.6f,1.2f,11.5f));
  SetView(cam,new Vector3(-10,2.6f,-3),new Vector3(-1,1.15f,8));EditorSceneManager.SaveScene(root.scene);
  File.WriteAllText("ParkPreviews/ground-view-complete.txt","Ground-level camera saved; textured surfaces, close activity areas, shelter, planting layers and daylight installed. Pre-upgrade scene saved as Park_BeforeGroundView.unity.");
 }
 static void SetView(Camera camera,Vector3 p,Vector3 target){camera.transform.position=p;camera.transform.LookAt(target);}
}
