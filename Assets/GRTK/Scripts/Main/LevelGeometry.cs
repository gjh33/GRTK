using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace GRTK
{
    // Class to hold the data for level geometry
    // No constructor since its a scriptableObject.
    // To instantiate, set its values after calling ScriptableObject.CreateInstance<LevelGeometry>()
    public class LevelGeometry : MonoBehaviour
    {
        // Exterior most polygon. From this you can navigate the level geometry
        [HideInInspector]
        public Polygon Exterior;

        // See editor code for visualization code
        [Tooltip("Highlights exterior polygons as blue, and interior as green. Useful for visualization purposes")]
        public bool visualizeInteriorExteriorPolygons = false;

        /*private void OnDrawGizmosSelected()
        {
            Polygon cur = Exterior;
            while(cur != null && cur.children.Count != 0)
            {
                Vector3[] poly = cur.GetRaw3();
                int i;
                for (i=0; i < poly.Length; i++)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(poly[i], 1);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(poly[i], poly[(i + 1) % poly.Length]);
                }
            }
        }*/
    }
}
