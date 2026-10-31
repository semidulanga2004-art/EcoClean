using UnityEngine; 
using TMPro; 
public class SortingManager : MonoBehaviour 
{    
    public static SortingManager Instance { get; private set; }    
    public TMP_Text scoreText;    
    public TMP_Text feedbackText;    
    private WasteItem selectedWaste;

     private int score;   
      private void Awake()    
      {        if (Instance != null && Instance != this)        
      {            Destroy(gameObject);           
       return;        
      }        
      Instance = this;   
     }    
     private void Start()    
     {        UpdateScore();       
      if (feedbackText != null)            
      feedbackText.text = "Select waste, then select a bin.";    
      }    
      public void SelectWaste(WasteItem waste)   
       {       
         selectedWaste = waste;        
         if (feedbackText != null)            
         
         feedbackText.text = "Selected: " + waste.category;    
         }    
         public void TrySort(SortingBin bin)    
         {        if (selectedWaste == null)        
         {            if (feedbackText != null)                
         feedbackText.text = "Select a waste item first!";            
         return;        
         }        
         if (selectedWaste.category == bin.acceptsCategory)       
          {            score += 10;            
          Destroy(selectedWaste.gameObject);            
          if (feedbackText != null)                
          feedbackText.text = "Correct! +10 points";        
          }        
          else        {            
            score -= 5;            
            if (feedbackText != null)                
            feedbackText.text = "Wrong bin! -5 points";       
             }        
             selectedWaste = null;        
             UpdateScore();    
             }    
             private void UpdateScore()    
             {        if (scoreText != null)            
             scoreText.text = "Score: " + score;    
             } 
             }