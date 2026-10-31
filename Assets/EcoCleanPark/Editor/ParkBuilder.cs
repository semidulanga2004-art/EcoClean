using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using EcoClean.Environment;

[InitializeOnLoad]
public static class ParkBuilder
{
    const string Root = "Assets/EcoCleanPark";
    static Transform environment;
    static Dictionary<string, Material> mats = new Dictionary<string, Material>();
    static System.Random rng;
    static ParkBuilder() { EditorApplication.delayCall += AutoBuild; }
    static void AutoBuild()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall += AutoBuild; return; }
        if (!File.Exists(Root + "/BUILD_REQUEST")) return;
        File.Delete(Root + "/BUILD_REQUEST");
        try { Build(); } catch (System.Exception e) { File.WriteAllText(Root + "/build-error.txt", e.ToString()); Debug.LogException(e); }
    }
    static Material M(string name, Color c)
    {
        if (mats.TryGetValue(name, out var found)) return found;
        string path = Root + "/Materials/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.color = c; m.SetFloat("_Smoothness", .12f); mats[name] = m; return m;
    }
    static float R(float a, float b) => Mathf.Lerp(a,b,(float)rng.NextDouble());
    static GameObject Empty(string name, Transform parent = null)
    { var g = new GameObject(name); g.transform.SetParent(parent ? parent : environment); return g; }
    static GameObject Box(string n, Vector3 p, Vector3 s, Material m, bool collide = true)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(environment);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;
        if(!collide) Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Prop(string name, Vector3 p, float yaw=0, float scale=1, bool collider=true)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+".fbx");
        if(!asset) throw new System.Exception("Missing model: "+name);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.name=name;g.transform.SetParent(environment);g.transform.position=p;g.transform.rotation=Quaternion.Euler(0,yaw,0)*asset.transform.localRotation;g.transform.localScale=asset.transform.localScale*scale;
        foreach(var ren in g.GetComponentsInChildren<Renderer>())
        {
            ren.sharedMaterials=ren.sharedMaterials.Select(m=>m ? MaterialByName(m.name) : M("stone",new Color(.64f,.66f,.59f))).ToArray();
        }
        if(collider) foreach(var f in g.GetComponentsInChildren<MeshFilter>()) { var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh; }
        g.isStatic=true;return g;
    }
    static Material MaterialByName(string name)
    {
        string key=name.Split('.')[0];
        var colors=new Dictionary<string,Color>{
          {"wood",new Color(.32f,.16f,.065f)},{"woodlight",new Color(.56f,.31f,.12f)},
          {"iron",new Color(.12f,.17f,.16f)},{"stone",new Color(.64f,.66f,.59f)},{"water",new Color(.15f,.58f,.72f)},
          {"leaf",new Color(.28f,.48f,.09f)},{"leaflight",new Color(.43f,.61f,.13f)},{"leafdark",new Color(.14f,.32f,.09f)},
          {"trunk",new Color(.27f,.14f,.06f)},{"blue",new Color(.055f,.28f,.7f)},{"yellow",new Color(.95f,.62f,.055f)},
          {"grey",new Color(.26f,.32f,.34f)},{"white",new Color(.92f,.91f,.77f)},{"red",new Color(.75f,.26f,.12f)},{"bottle",new Color(.2f,.65f,.77f)}};
        return M(key,colors.TryGetValue(key,out var c)?c:Color.gray);
    }
    static GameObject Ring(string name,float inner,float outer,float y,Material mat,int count=96)
    {
        var verts=new Vector3[count*2];var tris=new int[count*6];
        for(int i=0;i<count;i++) {float a=i*Mathf.PI*2/count;verts[i*2]=new Vector3(Mathf.Cos(a)*inner,y,Mathf.Sin(a)*inner);verts[i*2+1]=new Vector3(Mathf.Cos(a)*outer,y,Mathf.Sin(a)*outer);int j=(i+1)%count;int t=i*6;tris[t]=i*2;tris[t+1]=j*2;tris[t+2]=i*2+1;tris[t+3]=i*2+1;tris[t+4]=j*2;tris[t+5]=j*2+1;}
        var mesh=new Mesh{name=name};mesh.vertices=verts;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var path=Root+"/Meshes/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
        var g=Empty(name);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
    }
    static void Label(string text,Vector3 p,float size,Color color)
    {var g=Empty(text);g.transform.position=p;var t=g.AddComponent<TextMesh>();t.text=text;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.characterSize=size;t.fontSize=64;t.color=color;}
    [MenuItem("EcoClean/Build Park Environment")]
    public static void Build()
    {
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        foreach(string folder in new[]{"Materials","Meshes","Scenes","Prefabs","Audio"}) Directory.CreateDirectory(Root+"/"+folder);
        rng=new System.Random(31);mats.Clear();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        environment=new GameObject("EcoClean Park Environment").transform;
        var grass=M("Grass",new Color(.36f,.51f,.16f));var path=M("Sandstone paths",new Color(.76f,.64f,.43f));var curb=M("Path edging",new Color(.66f,.66f,.55f));
        Box("Ground",new Vector3(0,-.20f,0),new Vector3(50,.4f,50),grass);
        Box("Landscape surround",new Vector3(0,-.28f,0),new Vector3(140,.15f,140),M("Distant meadow",new Color(.40f,.53f,.20f)),false);
        Ring("Circular walking path",11.5f,14.5f,.025f,path);Ring("Inner curb",11.35f,11.5f,.04f,curb);Ring("Outer curb",14.5f,14.65f,.04f,curb);
        Ring("Fountain plaza",0,6.0f,.035f,path);Ring("Plaza edging",6,6.15f,.045f,curb);
        Box("South main walk",new Vector3(0,.025f,-15),new Vector3(4,.05f,20),path);
        Box("North main walk",new Vector3(0,.025f,15),new Vector3(4,.05f,20),path);
        Box("East crossing",new Vector3(9,.025f,0),new Vector3(10,.05f,3),path);
        Box("West crossing",new Vector3(-9,.025f,0),new Vector3(10,.05f,3),path);
        Box("Picnic terrace",new Vector3(-15,.025f,13),new Vector3(10,.05f,7),path);
        Box("Sorting terrace",new Vector3(15,.025f,13),new Vector3(8,.05f,6),path);
        // Four benches face the centre, with room to circulate behind them.
        for(int i=0;i<4;i++){float a=(45+i*90)*Mathf.Deg2Rad;Vector3 p=new Vector3(Mathf.Sin(a)*7.7f,.06f,Mathf.Cos(a)*7.7f);Prop("Bench",p,45+i*90);}
        Prop("PicnicTable",new Vector3(-16,.06f,14),-8);Prop("PicnicTable",new Vector3(-13,.06f,11),12);
        for(int i=0;i<3;i++){
            Prop(new[]{"BinPaper","BinPlastic","BinMetal"}[i],new Vector3(13+i*1.7f,.06f,14));
            Label(new[]{"PAPER","PLASTIC","METAL"}[i],new Vector3(13+i*1.7f,1.62f,13.55f),.055f,new Color(.16f,.23f,.18f));
        }
        Prop("Sign",new Vector3(3.5f,.06f,-22));Label("ECOCLEAN\nCOMMUNITY PARK",new Vector3(3.5f,1.65f,-22.09f),.042f,new Color(.94f,.93f,.80f));
        Prop("PlantingBed",new Vector3(15,.04f,-12));
        for(int i=0;i<9;i++)Prop("Flower",new Vector3(14+ i%3,.24f,-12.5f+i/3*.5f),R(0,360),1.2f,false);
        // Boundary fence leaves an entrance gap and a clear north exit.
        for(int i=-11;i<=11;i++){
            if(Mathf.Abs(i)>1){Prop("Fence",new Vector3(i*2,.04f,-24));Prop("Fence",new Vector3(i*2,.04f,24));}
            Prop("Fence",new Vector3(-24,.04f,i*2),90);Prop("Fence",new Vector3(24,.04f,i*2),90);
        }
        for(int i=0;i<52;i++){
            int edge=i%4;float along=R(-22,22);Vector3 p=edge==0?new Vector3(-21+R(-1,1),0,along):edge==1?new Vector3(21+R(-1,1),0,along):edge==2?new Vector3(along,0,-21+R(-1,1)):new Vector3(along,0,21+R(-1,1));
            if(Mathf.Abs(p.x)<4 && Mathf.Abs(p.z)>19)continue;
            Prop(new[]{"TreeOak","TreeLime","TreeDark"}[i%3],p,R(0,360),R(.9f,1.45f));
        }
        for(int i=0;i<110;i++){
            float a=R(0,Mathf.PI*2),r=R(15.3f,22);Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);
            if(Mathf.Abs(p.x)<3 || (p.z>8 && p.z<18 && Mathf.Abs(p.x)>9 && Mathf.Abs(p.x)<20))continue;
            Prop(i%2==0?"Bush":"BushLight",p,R(0,360),R(.65f,1.15f),false);
            if(i%4==0)Prop("Rock",p+new Vector3(.8f,0,.5f),R(0,360),R(.5f,1),false);
            if(i%2==0)Prop("Flower",p+new Vector3(.7f,.02f,-.3f),R(0,360),R(.7f,1.3f),false);
        }
        for(int i=0;i<20;i++){
            float a=i*Mathf.PI*2/20;Vector3 p=new Vector3(Mathf.Cos(a)*8.8f,0,Mathf.Sin(a)*8.8f);
            if(Mathf.Abs(p.x)<3||Mathf.Abs(p.z)<2)continue;Prop("Bush",p,R(0,360),.6f,false);
            Prop("Flower",p+new Vector3(.6f,.03f,0),0,.9f,false);
        }
        for(int i=0;i<15;i++){var g=Prop(new[]{"LitterBottle","LitterCup","LitterPaper"}[i%3],new Vector3(R(-19,-10),.07f,R(9,17)),R(0,360),1.3f,false);g.name="Litter placement "+(i+1);}
        var fountain=Prop("Fountain",Vector3.zero);fountain.isStatic=false;AddWater(fountain);
        PrefabUtility.SaveAsPrefabAsset(fountain,Root+"/Prefabs/WorkingFountain.prefab");
        var marker=Empty("PlayerSpawn - integration marker only");marker.transform.position=new Vector3(0,.1f,-21);
        for(int i=0;i<4;i++){
            float x=i==2?-25.4f:25.4f;var b=Box("Invisible boundary",new Vector3(i<2?0:x,2,i<2?(i==0?-25.4f:25.4f):0),i<2?new Vector3(52,4,.5f):new Vector3(.5f,4,52),grass);b.GetComponent<Renderer>().enabled=false;
        }
        var light=new GameObject("Afternoon sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.7f;light.color=new Color(1,.95f,.82f);light.transform.rotation=Quaternion.Euler(48,-35,0);light.shadows=LightShadows.Soft;
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.60f,.73f,.88f);RenderSettings.ambientEquatorColor=new Color(.46f,.53f,.37f);RenderSettings.ambientGroundColor=new Color(.28f,.31f,.19f);
        RenderSettings.fog=true;RenderSettings.fogColor=new Color(.72f,.80f,.66f);RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=95;RenderSettings.fogEndDistance=220;
        var cam=new GameObject("Park overview - environment preview").AddComponent<Camera>();cam.tag="MainCamera";cam.transform.position=new Vector3(0,35,-45);cam.transform.LookAt(new Vector3(0,0,1));cam.fieldOfView=49;cam.farClipPlane=220;cam.backgroundColor=new Color(.70f,.81f,.90f);cam.clearFlags=CameraClearFlags.SolidColor;cam.gameObject.AddComponent<AudioListener>();
        EditorSceneManager.SaveScene(scene,Root+"/Scenes/Park.unity");AssetDatabase.SaveAssets();
        Capture(cam,"park-overview",new Vector3(0,35,-45),new Vector3(0,0,1));
        Capture(cam,"park-entrance",new Vector3(0,2.5f,-21),new Vector3(0,1,1));
        Capture(cam,"park-picnic",new Vector3(-18,3,8),new Vector3(7,1,9));
        cam.transform.position=new Vector3(0,35,-45);cam.transform.LookAt(new Vector3(0,0,1));EditorSceneManager.SaveScene(scene);
        File.WriteAllText(Root+"/build-complete.txt","Park built, saved and three previews rendered. Models: "+AssetDatabase.FindAssets("t:Model",new[]{Root+"/Models"}).Length);
        Debug.Log("EcoClean park environment saved successfully.");
    }
    static void AddWater(GameObject fountain)
    {
        var fx=fountain.AddComponent<ParkFountain>();
        // FBX roots retain their unit-conversion scale. Keep effect units in metres.
        var effectRoot=new GameObject("Fountain water effects");effectRoot.transform.SetParent(fountain.transform);effectRoot.transform.rotation=Quaternion.identity;effectRoot.transform.localScale=Vector3.one/fountain.transform.localScale.x;
        Material water=M("Fountain spray",new Color(.46f,.83f,.94f));
        void Spray(string name,Vector3 p,float speed,float life,float rate,float angle,float size)
        {
            var g=new GameObject(name);g.transform.SetParent(effectRoot.transform);g.transform.localPosition=p;g.transform.localRotation=Quaternion.Euler(-90,0,0);
            var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.duration=5;main.loop=true;main.prewarm=true;main.startLifetime=life;main.startSpeed=speed;main.startSize=size;main.startColor=new Color(.62f,.88f,.96f,.8f);main.gravityModifier=1;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=600;
            var emission=ps.emission;emission.rateOverTime=rate;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=angle;shape.radius=.045f;
            var rend=ps.GetComponent<ParticleSystemRenderer>();rend.sharedMaterial=water;rend.renderMode=ParticleSystemRenderMode.Stretch;rend.lengthScale=.3f;rend.velocityScale=.08f;
            ps.Play();
        }
        Spray("Top water jet",new Vector3(0,2.88f,0),3,.65f,100,6,.05f);
        for(int i=0;i<12;i++) {float a=i*Mathf.PI*2/12;Spray("Bowl cascade "+i,new Vector3(Mathf.Cos(a)*1.08f,1.89f,Mathf.Sin(a)*1.08f),.2f,.53f,18,20,.035f);}
        for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Spray("Upper bowl spill "+i,new Vector3(Mathf.Cos(a)*.53f,2.65f,Mathf.Sin(a)*.53f),.15f,.4f,16,12,.03f);}
        fx.ripples=new Transform[3];
        for(int i=0;i<3;i++){
            var g=Ring("Ripple "+i,.49f,.5f,0,water,64);g.transform.SetParent(effectRoot.transform);g.transform.localPosition=new Vector3(0,.39f+i*.002f,0);fx.ripples[i]=g.transform;
        }
        string audioPath=Root+"/Audio/fountain-water.wav";
        if(!File.Exists(audioPath)){
            int rate=22050,n=rate*6;var random=new System.Random(8);using(var w=new BinaryWriter(File.Create(audioPath))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);float filter=0;for(int i=0;i<n;i++){filter=.84f*filter+.16f*((float)random.NextDouble()*2-1);float v=filter*.5f;float fade=Mathf.Min(1,Mathf.Min(i,n-1-i)/220f);w.Write((short)(v*fade*32767));}}
            AssetDatabase.ImportAsset(audioPath);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(audioPath,ImportAssetOptions.ForceSynchronousImport);
        var audio=fountain.AddComponent<AudioSource>();audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);audio.loop=true;audio.playOnAwake=true;audio.spatialBlend=1;audio.minDistance=3;audio.maxDistance=24;audio.volume=.4f;audio.rolloffMode=AudioRolloffMode.Linear;
        if(!audio.clip) throw new System.Exception("Fountain audio failed to import.");
    }
    public static void Capture(Camera cam,string name,Vector3 pos,Vector3 target)
    {
        cam.transform.position=pos;cam.transform.LookAt(target);var rt=new RenderTexture(1600,1000,24);cam.targetTexture=rt;cam.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();Directory.CreateDirectory("ParkPreviews");File.WriteAllBytes("ParkPreviews/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=prior;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
    }
}
