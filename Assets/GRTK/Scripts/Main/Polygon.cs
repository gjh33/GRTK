using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GRTK
{
    // Class to represent a closed polygon. This is serializable so we can
    // store it in a scriptable object (LevelGeometry) to save our
    // results from compiling geometry
    public class Polygon
    {
        // Polygons have a parent child hierarchy
        // The exterior most polygon has 0 parents and depth of 0
        // it represents the outter most ring of your boundaries
        // its child is an interior polygon. It represents the inner
        // ring of your boundaries, and the outter ring of navigable space
        // its child would be an exterior of another boundary that is internal
        // a polygon can have many children (think 2 seperate islands) but
        // only one parent
        public Polygon parent = null;
        public List<Polygon> children = new List<Polygon>();

        // the raw data that represents these polygons.
        private List<Vector2> verticies = new List<Vector2>();

        public Polygon() { }

        public Polygon(List<Vector2> verticies)
        {
            this.verticies = verticies;
        }

        public Polygon(List<Vector2> verticies, Polygon parent)
        {
            this.verticies = verticies;
            this.parent = parent;
            if (!parent.children.Contains(this))
                parent.children.Add(this);
        }

        // Adds a vertex as a neighbor to the last and first vertex in the loop
        public void AppendVertex(Vector2 vert)
        {
            verticies.Add(vert);
        }

        // Removes a given vertex
        public void RemoveVertex(Vector2 vert)
        {
            verticies.Remove(vert);
        }

        // Sets the parent of this polygon and sets this polygon as a child of the parent
        public void SetParent(Polygon parent)
        {
            this.parent = parent;
            if (!parent.children.Contains(this))
            {
                parent.children.Add(this);
            }
        }

        // Gets the depth of the polygon (how many parents)
        public int GetDepth()
        {
            Polygon cur = this;
            int depth = 0;
            while (cur.parent != null)
            {
                cur = cur.parent;
                depth++;
            }
            return depth;
        }

        // Decides if this is an interior polygon
        // The exterior most polygon will have 0 parents, its
        // interior will have 1, etc etc. Thus odd parents = interior
        public bool IsInterior()
        {
            return GetDepth() % 2 == 1;
        }

        // Decides if this is an exterior polygon
        public bool IsExterior()
        {
            return !IsInterior();
        }

        // Returns this polygon in a way unity can process with meshes
        public Vector2[] GetRaw2()
        {
            return verticies.ToArray();
        }

        public Vector3[] GetRaw3()
        {
            List<Vector3> vec3 = new List<Vector3>();
            foreach (Vector2 vertex in verticies)
            {
                vec3.Add(new Vector3(vertex.x, vertex.y, 0));
            }

            return vec3.ToArray();
        }
    }
}