using UnityEngine; public class SortingBin : MonoBehaviour 
{    
    public WasteCategory acceptsCategory;    
    private void OnMouseDown()   
     {        
        if (SortingManager.Instance != null)            
        SortingManager.Instance.TrySort(this);    
        } 
        }