using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaleCraft.Movement
{
    /// <summary>
    /// A Data structure for points that make up a polygon.
    /// </summary>
    [ExecuteAlways]
    public class Polygon : MonoBehaviour
    {
        /// <value>The set of all <c>Point</c>s connected by edges. 
        /// Must be clockwise, otherwise the algorithm won't work properly. 
        /// To ensure this, the class <c>WalkableMap</c> calls <c>IsOrientationClockwise()</c> in function class <c>Run()</c>.</value>
        public List<GraphNode> Vertices;   // clockwise!
        [SerializeField] private Color lineColor = Color.yellow;
        [SerializeField] private bool drawEdge = true;

        [Header("Point settings")]
        [SerializeField] private bool drawPoints = true;
        [SerializeField] private bool automaticZooming = true;
        [Range(0.1f, 10.0f)] [SerializeField] private float pointSize = 0.5f;

        public float PointSize => pointSize / 100;
        public bool DrawEdge { get => drawEdge; set => drawEdge = value; }
        public bool DrawPoints { get => drawPoints; set => drawPoints = value; }

        private void Awake()
        {
            if (Application.IsPlaying(gameObject))
                ListChildrenWithComponent();
        }

        private void OnValidate()
        {
            ListChildrenWithComponent();
        }

        private void OnEnable()
        {
            // Ensure this only runs in the editor
            if (!Application.isPlaying)
                ListChildrenWithComponent();
        }

        private void Update()
        {
            if (!Application.IsPlaying(gameObject))
            {
                ListChildrenWithComponent();
            }
        }

        public void ReverseVertices()
        {
            Vertices.Reverse();
        }

        /// <summary>
        /// Determines if the orientation of points is clockwise or not
        /// <para>Source: <see href="https://stackoverflow.com/a/1165943">HERE</see></para>
        /// </summary>
        /// <returns>
        /// Boolean whether the orientation of points is clockwise or not.
        /// </returns>
        public bool IsOrientationClockwise()
        {
            Debug.Assert(Vertices.Count >= 3);

            float sum = 0;
            for (int i = 0; i < Vertices.Count; ++i)
            {
                var a = Vertices[i];
                var b = Vertices[(i + 1) % Vertices.Count];
                sum += (b.X - a.X) * (b.Y + a.Y);
            }

            return sum > 0; // clock or counterclock wise 
        }

        /// <summary>
        /// Determines if given point is inside this polygon.
        /// Continuation of the algorithm from class <c>WalkableMap</c>.
        /// <para>Inspiration: <see href="https://www.david-gouveia.com/pathfinding-on-a-2d-polygonal-map">HERE</see></para>
        /// </summary>
        public bool IsInside(GraphNode point, bool toleranceOnOutside = true)
        {
            const float epsilon = 0.005f;

            bool inside = false;    // initial value

            if (Vertices.Count < 3) return false;  // Must have 3 or more edges to be 2-dimensional

            GraphNode oldPoint = Vertices[^1];  // last one
            float oldSqDist = Graph.DistanceSquared(oldPoint, point);

            for (int i = 0; i < Vertices.Count; i++)
            {
                GraphNode newPoint = Vertices[i];
                float newSqDist = Graph.DistanceSquared(newPoint, point);

                if (oldSqDist + newSqDist + 2.0f * Math.Sqrt(oldSqDist * newSqDist) - Graph.DistanceSquared(newPoint, oldPoint) < epsilon)
                    return toleranceOnOutside;

                // If x-coor is smaller for newPoint then it's the left GraphNode. Otherwise oldPoint.
                GraphNode left = newPoint;
                GraphNode right = oldPoint;
                if (newPoint.X > oldPoint.X)
                {
                    left = oldPoint;
                    right = newPoint;
                }

                if (left.X < point.X && point.X <= right.X && (point.Y - left.Y) * (right.X - left.X) < (right.Y - left.Y) * (point.X - left.X))
                    inside = !inside;

                oldPoint = newPoint;
                oldSqDist = newSqDist;
            }

            return inside;
        }

        /// <summary>
        /// Finds closest point on the edge of the polygon.
        /// <para>Inspiration: <see href="https://github.com/MicUurloon/AdventurePathfinding/blob/master/src/pathfinding/Polygon.hx">HERE</see></para>
        /// </summary>
        public Vector3 GetClosestPointOnEdge(GraphNode p)
        {
            var vi1 = -1;
            var vi2 = -1;
            float mindist = 100000;

            for (int i = 0; i < Vertices.Count; ++i)
            {
                var dist = Graph.DistanceToSegment(p, Vertices[i], Vertices[(i + 1) % Vertices.Count]);

                if (dist < mindist)
                {
                    mindist = dist;
                    vi1 = i;
                    vi2 = (i + 1) % Vertices.Count;
                }
            }

            var p1 = Vertices[vi1];
            var p2 = Vertices[vi2];

            float x1 = p1.X;
            float y1 = p1.Y;
            float x2 = p2.X;
            float y2 = p2.Y;
            float x3 = p.X;
            float y3 = p.Y;

            var u = (((x3 - x1) * (x2 - x1)) + ((y3 - y1) * (y2 - y1))) / (((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            var xu = x1 + u * (x2 - x1);
            var yu = y1 + u * (y2 - y1);

            Vector3 linevector;
            if (u < 0) linevector = new Vector3(x1, y1);
            else if (u > 1) linevector = new Vector3(x2, y2);
            else linevector = new Vector3(xu, yu);
            return linevector;
        }

        /// <summary>
        /// Deletes a point around the location end.
        /// </summary>
        public bool DestroyPoint(Vector3 vec)
        {
            if (Vertices.Count <= 3) return false;    // Cannot have a polygon with 2 or less Vertices

            int closestPoint = -1;
            GraphNode gn = new(vec);
            float minDis = float.MaxValue;
            float epsilon = 0.4f;

            for (int i = 0; i < Vertices.Count; ++i)
            {
                var dis = Graph.Distance(Vertices[i], gn);
                if (Vertices[i].P != null && dis < minDis)
                {
                    closestPoint = i;
                    minDis = dis;
                }
            }

            if (closestPoint != -1 && minDis <= epsilon)
            {
                DestroyImmediate(transform.GetChild(closestPoint).gameObject);
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = lineColor;
            for (int i = 1; i < transform.childCount + 1; ++i)
            {
                Transform lastChild = transform.GetChild(i - 1);
                Transform currentChild = transform.GetChild(i % transform.childCount);

                if (drawEdge)
                    Gizmos.DrawLine(lastChild.position, currentChild.position);

                if (!drawPoints)
                    currentChild.GetComponent<SpriteRenderer>().enabled = false;
                else
                {
                    currentChild.GetComponent<SpriteRenderer>().enabled = true;
                    lastChild.gameObject.GetComponent<SpriteRenderer>().color = lineColor;

                    float handleSize = 3f; // Default size if not in the Editor
                    Vector3 s = new(PointSize, PointSize, 0);
                    // Calculate the handle size only if in the Unity Editor
                    if (!Application.isPlaying && automaticZooming)
                        handleSize = HandleUtility.GetHandleSize(transform.position);
                    currentChild.localScale = handleSize * s;
                }
            }
        }
#endif

        //// This method lists all child objects of the current GameObject that have Point component
        public void ListChildrenWithComponent()
        {
            int cnt = 0;
            if (this == null) return;
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Point>() != null)
                    cnt++;
            }

            if (GetComponent<Polygon>().Vertices == null || cnt != GetComponent<Polygon>().Vertices.Count)
                GetComponent<Polygon>().Vertices = new List<GraphNode>(new GraphNode[cnt]);

            int i = 0;
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Point>() != null)
                {
                    GetComponent<Polygon>().Vertices[i] = (new(child.GetComponent<Point>()));
                    i++;
                }
            }
        }
    }
}