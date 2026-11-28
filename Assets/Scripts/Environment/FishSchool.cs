using UnityEngine;

// Swims its child fish together along a slow looping path around where the school was placed.
public sealed class FishSchool : MonoBehaviour
{
    [SerializeField, Min(0f)] private float pathRadius = 8f;
    [SerializeField, Min(0f)] private float pathHeight = 1.2f;
    [SerializeField] private float loopSpeed = 0.18f;
    [SerializeField, Min(0f)] private float wiggle = 12f;

    private Transform[] fish;
    private Vector3[] offsets;
    private Vector3 anchor;
    private float phase;

    private void Awake()
    {
        anchor = transform.position;
        phase = Random.value * Mathf.PI * 2f;
        fish = new Transform[transform.childCount];
        offsets = new Vector3[fish.Length];
        for (int i = 0; i < fish.Length; i++)
        {
            fish[i] = transform.GetChild(i);
            offsets[i] = fish[i].localPosition;
        }
    }

    private void Update()
    {
        float t = Time.time * loopSpeed + phase;
        Vector3 center = anchor + new Vector3(
            Mathf.Cos(t) * pathRadius,
            Mathf.Sin(t * 1.7f) * pathHeight,
            Mathf.Sin(t) * pathRadius * 0.6f);
        // Direction of travel along the path (its derivative).
        Vector3 heading = new Vector3(-Mathf.Sin(t), 0f, Mathf.Cos(t) * 0.6f) * Mathf.Sign(loopSpeed);
        Quaternion facing = Quaternion.LookRotation(heading, Vector3.up);

        for (int i = 0; i < fish.Length; i++)
        {
            float wave = Time.time * 3f + i * 0.9f;
            fish[i].position = center + facing * offsets[i] + new Vector3(0f, Mathf.Sin(wave * 0.5f) * 0.15f, 0f);
            fish[i].rotation = facing * Quaternion.Euler(0f, Mathf.Sin(wave * 2.5f) * wiggle, 0f);
        }
    }
}
