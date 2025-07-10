using System;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

namespace TaleCraft.Movement
{
    /// <summary>
    /// A Data structure for a set of nodes that make up a cyclic undirected graph connected by <c>GraphEdges</c>.
    /// </summary>
    public class Graph
    {
        /// <value>Nodes that the graph consists of.</value>
        public List<GraphNode> GraphNodes { get; private set; } = new();
        /// <value>Defines the neighboring nodes for a node.</value>
        public List<List<int>> Neighbors { get; private set; } = new();
        /// <value>Dictionary of each node and its edges (only one instance of edge inside).</value>
        public Dictionary<int, List<GraphEdge>> GraphEdges { get; private set; } = new();
        /// <value>Starting node of the graph.</value>
        public GraphNode Start { get; set; }
        /// <value>Ending node of the graph.</value>
        public GraphNode End { get; set; }

        public Graph(GraphNode start, GraphNode end)
        {
            Start = start; End = end;
        }

        /// <summary>
        /// Calculates Euclidian distance of two <c>GraphNode</c>s.
        /// </summary>
        /// <returns>
        /// A float representing the Euclidian distance.
        /// </returns>
        public static float Distance(GraphNode p1, GraphNode p2)
        {
            return (float)System.Math.Sqrt(DistanceSquared(p1, p2));
        }

        /// <summary>
        /// Calculates squared Euclidian distance of two <c>GraphNode</c>s.
        /// </summary>
        /// <returns>
        /// A float representing the squared Euclidian distance.
        /// </returns>
        public static float DistanceSquared(GraphNode p1, GraphNode p2)
        {
            float distanceX = p1.X - p2.X;
            float distanceY = p1.Y - p2.Y;
            return distanceX * distanceX + distanceY * distanceY;
        }

        /// <summary>
        /// Calculates Euclidian shortest distance between a point and a line segment.
        /// </summary>
        /// <returns>
        /// A float representing the Euclidian distance.
        /// </returns>
        public static float DistanceToSegment(GraphNode p, GraphNode q, GraphNode r)
        {
            return (float)Math.Sqrt(DistanceToSegmentSquared(p, q, r));
        }

        /// <summary>
        /// Calculates squared Euclidian shortest distance between a point p and a line segment qr.
        /// </summary>
        /// <returns>
        /// A float representing the squared Euclidian distance.
        /// </returns>
        public static float DistanceToSegmentSquared(GraphNode p, GraphNode q, GraphNode r)
        {
            float l2 = DistanceSquared(q, r);
            if (l2 == 0) return DistanceSquared(p, r);
            float t = ((p.X - q.X) * (r.X - q.X) + (p.Y - q.Y) * (r.Y - q.Y)) / l2;
            if (t < 0) return DistanceSquared(p, q);
            if (t > 1) return DistanceSquared(p, r);
            return DistanceSquared(p, new(q.X + t * (r.X - q.X), q.Y + t * (r.Y - q.Y)));
        }

        /// <summary>
        /// Adds a new graph node from class Point.
        /// </summary>
        public void AddNode(Point point)
        {
            AddNode(new GraphNode(point));
        }

        /// <summary>
        /// Adds a new graph node from class GraphNode.
        /// </summary>
        public void AddNode(GraphNode node)
        {
            GraphNodes.Add(node);
            Neighbors.Add(new List<int>());
        }

        /// <summary>
        /// Adds a new graph edge from two indeces of graph nodes.
        /// </summary>
        public void AddEdge(int n1, int n2)
        {
            AddEdge(new GraphEdge(n1, n2, Distance(GraphNodes[n1], GraphNodes[n2])));
        }

        /// <summary>
        /// Adds a new graph edge to the graph at the Position of lower index.
        /// </summary>
        private void AddEdge(GraphEdge edge)
        {
            var i = FindIdx(edge);
            if (!GraphEdges.ContainsKey(i))
                GraphEdges.Add(i, new());
            GraphEdges[i].Add(edge);
            AddNeigbors(edge);
        }

        /// <summary>
        /// Finds the index of an edge.
        /// </summary>
        private int FindIdx(GraphEdge edge)
        {
            int result = edge.N1 < edge.N2 ? edge.N1 : edge.N2;
            return result;
        }

        /// <summary>
        /// Finds the edge based on node indices.
        /// </summary>
        public GraphEdge FindEdge(int n1, int n2)
        {
            int idx_min = Math.Min(n1, n2);
            int idx_max = Math.Max(n1, n2);
            foreach (GraphEdge e in GraphEdges[idx_min])
            {
                if (e.N2 == idx_max)
                {
                    return e;
                }
            }
            System.Diagnostics.Debug.Assert(false);
            return null;
        }

        /// <summary>
        /// For each vertex of the edge, neighboring Vertices are updated.
        /// </summary>
        private void AddNeigbors(GraphEdge edge)
        {
            Neighbors[edge.N1].Add(edge.N2);
            Neighbors[edge.N2].Add(edge.N1);
        }

        public Vector3[] GetVectorEdge(GraphEdge edge)
        {
            var ar = new Vector3[2];
            var n1 = GraphNodes[edge.N1];
            var n2 = GraphNodes[edge.N2];
            ar[0] = new Vector3(n1.X, n1.Y, 0);
            ar[1] = new Vector3(n2.X, n2.Y, 0);
            return ar;
        }

    }

    /// <summary>
    /// A Data structure for a node that makes up a cyclic undirected graph.
    /// </summary>
    public class GraphNode
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public Point P { get; private set; }


        public GraphNode() { X = 0; Y = 0; }

        public GraphNode(Vector3 vec)
        {
            X = vec.x; Y = vec.y; P = null;
        }

        public GraphNode(float x, float y)
        {
            X = x;
            Y = y;
            P = null;
        }

        public GraphNode(Point p)
        {
            X = p.transform.position.x;
            Y = p.transform.position.y;
            P = p;
        }

        public Vector2 GetLocation()
        {
            return new(X, Y);
        }
    }

    /// <summary>
    /// A Data structure for an edge that makes up a cyclic undirected graph.
    /// It is made up of two indices of nodes and its pre-calculated length.
    /// </summary>
    public class GraphEdge
    {
        public int N1 { get; private set; }
        public int N2 { get; private set; }
        public float Length { get; private set; }

        public GraphEdge(int n1, int n2, float length)
        {
            N1 = Math.Min(n1, n2);
            N2 = Math.Max(n1, n2);
            Length = length;
        }
    }
}