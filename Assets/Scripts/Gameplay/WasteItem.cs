using UnityEngine; public class WasteItem : MonoBehaviour 
{    
    public WasteCategory category;   
     private void OnMouseDown()    
    {        if (SortingManager.Instance != null)           
     SortingManager.Instance.SelectWaste(this);   
    } 
}