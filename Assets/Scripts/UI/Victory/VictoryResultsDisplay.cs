using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EcoClean.Victory
{
    /// <summary>
    /// Shows final score, cleaned count, total count and percentage.
    ///
    /// HOW THE PROJECT LEAD USES IT
    /// Option A (different scenes, recommended):
    ///   VictoryResultsHandoff.Set(finalScore, cleanedCount, totalCount, percentage);
    ///   SceneManager.LoadScene("Victory");
    /// Option B (same scene, direct call):
    ///   display.ShowResults(finalScore, cleanedCount, totalCount, percentage);
    ///
    /// percentage is 0-100. Preview values are used ONLY in the Unity Editor
    /// and only when no real data was handed over.
    /// </summary>
    public class VictoryResultsDisplay : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text cleanedText;
        [SerializeField] private TMP_Text totalText;
        [SerializeField] private TMP_Text percentageText;

        [Header("Text formats")]
        [SerializeField] private string scoreFormat = "Final Score: {0}";
        [SerializeField] private string cleanedFormat = "Items Cleaned: {0}";
        [SerializeField] private string totalFormat = "Total Items: {0}";
        [SerializeField] private string percentageFormat = "Park Cleaned: {0:0}%";

        [Header("Optional extras (leave empty to skip)")]
        [Tooltip("An Image with Image Type = Filled (Horizontal).")]
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text ratingText;
        [SerializeField] private bool animateCountUp = true;
        [SerializeField] private float countUpDuration = 1.5f;

        [Header("Rating wording (percentage thresholds)")]
        [SerializeField] private string ratingFull = "Eco Hero!";
        [SerializeField] private string ratingHigh = "Great Cleaner!";
        [SerializeField] private string ratingMid = "Good Start!";
        [SerializeField] private string ratingLow = "Keep Going!";

        [Header("EDITOR PREVIEW ONLY (not real data)")]
        [SerializeField] private bool showPreviewInEditor = true;
        [SerializeField] private VictoryResultsData previewValues = new VictoryResultsData(1250, 18, 20, 90f);

        private Coroutine running;

        private void Start()
        {
            if (VictoryResultsHandoff.HasPending)
            {
                ShowResults(VictoryResultsHandoff.Pending);
                VictoryResultsHandoff.Clear();
            }
#if UNITY_EDITOR
            else if (showPreviewInEditor)
            {
                ShowResults(previewValues);
            }
#endif
        }

        public void ShowResults(int finalScore, int cleanedCount, int totalCount, float percentage)
        {
            ShowResults(new VictoryResultsData(finalScore, cleanedCount, totalCount, percentage));
        }

        public void ShowResults(VictoryResultsData data)
        {
            if (running != null) StopCoroutine(running);

            if (animateCountUp && countUpDuration > 0f && Application.isPlaying)
                running = StartCoroutine(Animate(data));
            else
                Apply(data, 1f);
        }

        private IEnumerator Animate(VictoryResultsData data)
        {
            float t = 0f;
            while (t < countUpDuration)
            {
                t += Time.unscaledDeltaTime;
                Apply(data, Mathf.Clamp01(t / countUpDuration));
                yield return null;
            }
            Apply(data, 1f);
            running = null;
        }

        // progress: 0..1 (how far the count-up has run)
        private void Apply(VictoryResultsData data, float progress)
        {
            float pct = Mathf.Clamp(data.percentage, 0f, 100f);

            if (scoreText != null) scoreText.text = string.Format(scoreFormat, Mathf.RoundToInt(data.finalScore * progress));
            if (cleanedText != null) cleanedText.text = string.Format(cleanedFormat, Mathf.RoundToInt(data.cleanedCount * progress));
            if (totalText != null) totalText.text = string.Format(totalFormat, data.totalCount);
            if (percentageText != null) percentageText.text = string.Format(percentageFormat, pct * progress);
            if (progressFill != null) progressFill.fillAmount = (pct * progress) / 100f;

            if (ratingText != null)
                ratingText.text = progress >= 1f ? GetRating(pct) : string.Empty;
        }

        private string GetRating(float pct)
        {
            if (pct >= 100f) return ratingFull;
            if (pct >= 70f) return ratingHigh;
            if (pct >= 40f) return ratingMid;
            return ratingLow;
        }

        [ContextMenu("Preview Values")]
        private void PreviewNow()
        {
            ShowResults(previewValues);
        }
    }
}