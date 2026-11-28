using System.Collections.Generic;
using UnityEngine;

// Marks a body of water. The top of the box collider is the water surface.
[RequireComponent(typeof(BoxCollider))]
public sealed class WaterZone : MonoBehaviour
{
    private static readonly List<WaterZone> Active = new List<WaterZone>();

    private BoxCollider box;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnEnable() => Active.Add(this);
    private void OnDisable() => Active.Remove(this);

    // True when the point's XZ lies inside a water zone and it is below that zone's surface + margin.
    public static bool TryGetSurface(Vector3 point, out float surfaceY)
    {
        foreach (WaterZone zone in Active)
        {
            Bounds bounds = zone.box.bounds;
            if (point.x >= bounds.min.x && point.x <= bounds.max.x &&
                point.z >= bounds.min.z && point.z <= bounds.max.z &&
                point.y >= bounds.min.y && point.y <= bounds.max.y + 2f)
            {
                surfaceY = bounds.max.y;
                return true;
            }
        }

        surfaceY = 0f;
        return false;
    }
}
