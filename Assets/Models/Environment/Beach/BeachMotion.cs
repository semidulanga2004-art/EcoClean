using UnityEngine;

namespace EcoClean.Environment
{
    public sealed class BeachMotion : MonoBehaviour
    {
        [System.Serializable]
        public struct Gull
        {
            public Transform body, leftWing, rightWing;
        }

        [SerializeField] private Transform[] foam;
        [SerializeField] private Gull[] flock;
        private Vector3[] foamOrigins;

        private void Awake()
        {
            foamOrigins = new Vector3[foam.Length];
            for (int i = 0; i < foam.Length; i++) foamOrigins[i] = foam[i].localPosition;
        }

        private void Update()
        {
            float time = Time.time;
            for (int i = 0; i < foam.Length; i++)
            {
                float motion = Mathf.Sin(time * 1.08f - i * .85f);
                foam[i].localPosition = foamOrigins[i] + new Vector3(.38f * motion, .004f * motion, 0);
            }

            for (int i = 0; i < flock.Length; i++)
            {
                Gull gull = flock[i];
                float angle = time * (.23f + i * .011f) + i * .58f;
                gull.body.localPosition = new Vector3(5 + 18 * Mathf.Cos(angle),
                    7.1f + i * .49f + .42f * Mathf.Sin(angle * 3), 7 + 20 * Mathf.Sin(angle));
                gull.body.localRotation = Quaternion.LookRotation(new Vector3(-18 * Mathf.Sin(angle), 0, 20 * Mathf.Cos(angle)));
                float flap = Mathf.Sin(time * 7.3f + i * .68f) * (18 + 10 * Mathf.Sin(time * 1.2f + i));
                gull.leftWing.localRotation = Quaternion.Euler(0, 0, flap);
                gull.rightWing.localRotation = Quaternion.Euler(0, 0, -flap);
            }
        }
    }
}
