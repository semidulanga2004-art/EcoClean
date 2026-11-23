using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SortingManager : MonoBehaviour
{
    public static SortingManager Instance { get; private set; }

    [Header("Sorting UI")]
    public TMP_Text scoreText;
    public TMP_Text feedbackText;

    [Header("Park Cleaning Progress")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFill;

    private readonly HashSet<int> cleanedWasteIds = new HashSet<int>();
    private WasteItem selectedWaste;
    private int score;
    private int totalWaste;
    private int cleanedWaste;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        totalWaste = FindObjectsByType<WasteItem>(FindObjectsSortMode.None).Length;
        UpdateScore();
        UpdateProgress();

        if (feedbackText != null)
        {
            feedbackText.text = "Select waste, then select a bin.";
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SelectWaste(WasteItem waste)
    {
        if (waste == null)
        {
            return;
        }

        selectedWaste = waste;
        if (feedbackText != null)
        {
            feedbackText.text = "Selected: " + waste.category;
        }
    }

    public void TrySort(SortingBin bin)
    {
        if (bin == null)
        {
            return;
        }

        if (selectedWaste == null)
        {
            if (feedbackText != null)
            {
                feedbackText.text = "Select a waste item first!";
            }

            return;
        }

        if (selectedWaste.category == bin.acceptsCategory)
        {
            score += 10;
            if (cleanedWasteIds.Add(selectedWaste.GetInstanceID()))
            {
                cleanedWaste++;
            }

            Destroy(selectedWaste.gameObject);
            if (feedbackText != null)
            {
                feedbackText.text = "Correct! +10 points";
            }
        }
        else
        {
            score -= 5;
            if (feedbackText != null)
            {
                feedbackText.text = "Wrong bin! -5 points";
            }
        }

        selectedWaste = null;
        UpdateScore();
        UpdateProgress();
    }

    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "<size=68%><color=#A9CDB7><b>SCORE</b></color></size>\n<size=140%><color=#EAF8EE><b>" + score + "</b></color></size>";
        }
    }

    private void UpdateProgress()
    {
        int percentage = totalWaste > 0
            ? Mathf.RoundToInt((float)cleanedWaste / totalWaste * 100f)
            : 0;

        if (progressText != null)
        {
            progressText.text = "PARK CLEANED: " + cleanedWaste + " / " + totalWaste
                + "\n<size=85%><color=#B8D7C4>" + percentage + "% COMPLETE</color></size>";
        }

        if (progressFill != null)
        {
            progressFill.fillAmount = totalWaste > 0
                ? Mathf.Clamp01((float)cleanedWaste / totalWaste)
                : 0f;
        }
    }
}
