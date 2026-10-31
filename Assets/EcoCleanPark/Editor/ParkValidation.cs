using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
[InitializeOnLoad] public static class ParkValidation
{
 static double start; static int errors; static int frames; static int initialParticles;
 static ParkValidation(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
 static void Log(string c,string s,LogType t){if(EditorApplication.isPlaying && (t==LogType.Error||t==LogType.Exception))errors++;}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||File.Exists("Assets/EcoCleanPark/BUILD_REQUEST"))return;
  if(!EditorApplication.isPlaying){
   if(!File.Exists("ParkPreviews/VALIDATE_REQUEST"))return;
   float duration; if(!float.TryParse(File.ReadAllText("ParkPreviews/VALIDATE_REQUEST"),out duration))duration=120;
   SessionState.SetFloat("ParkValidationDuration",duration);
   File.Delete("ParkPreviews/VALIDATE_REQUEST");SessionState.SetBool("ParkValidationRunning",true);EditorApplication.isPlaying=true;return;
  }
  if(!SessionState.GetBool("ParkValidationRunning",false))return;
  if(start==0){start=EditorApplication.timeSinceStartup;initialParticles=Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length;}
  frames++;
  if(EditorApplication.timeSinceStartup-start<SessionState.GetFloat("ParkValidationDuration",120))return;
  var all=Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);var bad=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.sharedMaterials.Any(m=>!m||!m.shader||m.shader.name.Contains("InternalError"))).Count();
  var audio=Object.FindFirstObjectByType<EcoClean.Environment.ParkFountain>().GetComponent<AudioSource>();
  File.WriteAllText("ParkPreviews/validation.txt",SessionState.GetFloat("ParkValidationDuration",120)+"-second Unity Editor play-mode check\nRuntime errors: "+errors+"\nParticle systems at start/end: "+initialParticles+"/"+all.Length+"\nPlaying systems: "+all.Count(p=>p.isPlaying)+"\nLive water particles: "+all.Sum(p=>p.particleCount)+"\nMissing/error materials: "+bad+"\nFountain audio playing: "+audio.isPlaying+"\nColliders: "+Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length+"\nThis verifies editor playback, not standalone PC performance or audible sound quality.");
  var cam=Camera.main;
  if(GameObject.Find("Ground-view landscaping")){
   ParkBuilder.Capture(cam,"park-street-picnic",new Vector3(-16,2.6f,-3),new Vector3(-5,1.1f,8));
   ParkBuilder.Capture(cam,"park-street-entrance",new Vector3(0,2.6f,-20),new Vector3(0,1.1f,0));
   ParkBuilder.Capture(cam,"park-street-sorting",new Vector3(5,2.6f,6),new Vector3(1.6f,1.2f,11.5f));
   int blocked=0;Physics.SyncTransforms();
   for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;Vector3 p=new Vector3(Mathf.Cos(a)*13,0,Mathf.Sin(a)*13);if(Physics.CheckCapsule(p+Vector3.up*.45f,p+Vector3.up*1.5f,.3f,~0,QueryTriggerInteraction.Ignore))blocked++;}
   File.AppendAllText("ParkPreviews/validation.txt","\nLoop path capsule checks blocked: "+blocked+"/48 (0.6m-wide test capsule).\n");
  }else{
   ParkBuilder.Capture(cam,"park-final-overview",new Vector3(0,35,-45),new Vector3(0,0,1));
   ParkBuilder.Capture(cam,"park-final-entrance",new Vector3(0,2.5f,-21),new Vector3(0,1,1));
   ParkBuilder.Capture(cam,"park-final-fountain",new Vector3(7,4,-8),new Vector3(0,1.3f,0));
  }
  SessionState.SetBool("ParkValidationRunning",false);EditorApplication.isPlaying=false;start=0;
 }
}
