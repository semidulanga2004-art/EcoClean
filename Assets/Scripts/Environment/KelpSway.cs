using UnityEngine;

// Gently sways every direct child (kelp stalks, sea grass) as if moved by the current.
public sealed class KelpSway : MonoBehaviour
{
    [SerializeField, Min(0f)] private float angle = 6f;
    [SerializeField, Min(0f)] private float speed = 0.7f;

    private Transform[] stalks;
    private Quaternion[] rest;

    private void Awake()
    {
        stalks = new Transform[transform.childCount];
        rest = new Quaternion[stalks.Length];
        for (int i = 0; i < stalks.Length; i++)
        {
            stalks[i] = transform.GetChild(i);
            rest[i] = stalks[i].localRotation;
        }
    }

    private void Update()
    {
        float t = Time.time * speed;
        for (int i = 0; i < stalks.Length; i++)
        {
            Vector3 p = stalks[i].position;
            float phase = p.x * 0.15f + p.z * 0.11f;
            stalks[i].localRotation = rest[i] * Quaternion.Euler(
                Mathf.Sin(t + phase) * angle, 0f, Mathf.Sin(t * 0.8f + phase * 1.3f) * angle * 0.6f);
        }
    }
}
