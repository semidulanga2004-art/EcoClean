using UnityEngine;

namespace EcoClean.Environment
{
    // A lightweight recirculating fountain: no gameplay or player dependencies.
    public sealed class ParkFountain : MonoBehaviour
    {
        public Transform[] ripples;
        void Update()
        {
            if (ripples == null) return;
            for (int i = 0; i < ripples.Length; i++)
            {
                if (!ripples[i]) continue;
                float t = Mathf.Repeat(Time.time * .23f + i / (float)ripples.Length, 1);
                float scale = Mathf.Lerp(.25f, 4.3f, t);
                ripples[i].localScale = new Vector3(scale, .012f, scale);
            }
        }
    }
}
