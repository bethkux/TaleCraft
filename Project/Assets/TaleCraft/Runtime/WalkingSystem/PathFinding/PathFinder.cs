using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaleCraft.Movement
{
    /// <summary>
    /// Finds the shortest path from starting Node to the ending Node.
    /// </summary>
    public class PathFinder
    {
        private readonly WalkableMap walkableMap;
        private List<int> path = new();
        private Graph walkGraph;

        public PathFinder(WalkableMap wMap)
        {
            walkableMap = wMap;
        }


        /// <summary>
        /// Takes an array of points and an idx of the point to be examined and calculates if the vertex is concave (more than 180°) or not.
        /// </summary>
        private static bool IsVertexConcave(List<GraphNode> vertices, int vertex_idx)
        {
            GraphNode current = vertices[vertex_idx];
            GraphNode next = vertices[(vertex_idx + 1) % vertices.Count];
            GraphNode previous = vertices[vertex_idx == 0 ? vertices.Count - 1 : vertex_idx - 1];

            var left = new System.Numerics.Vector2(current.X - previous.X, current.Y - previous.Y);
            var right = new System.Numerics.Vector2(next.X - current.X, next.Y - current.Y);
            float cross = (left.X * right.Y) - (left.Y * right.X);
            return cross < 0;
        }


        /// <summary>
        /// Determines whether two lines ab and cd cross.
        /// </summary>
        public static bool LineSegmentsCross(GraphNode a, GraphNode b, GraphNode c, GraphNode d)
        {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));

            if (denominator == 0)
                return false;

            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            if (numerator1 == 0 || numerator2 == 0)
                return false;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r > 0 && r < 1) && (s > 0 && s < 1);
        }


        /// <summary>
        /// Determines whether two GraphNodes are in line of sight (LOS).
        /// This is true iff they are both located inside both the main polygon and outside the obstacles
        /// and there is no line segment that stands in their way.
        /// </summary>
        private bool IsInLineOfSight(GraphNode start, GraphNode end)
        {
            float epsilon = 0.005f;

            // Not in LOS if any of the ends is outside the polygon
            if (!walkableMap.Polygons[0].IsInside(start) || !walkableMap.Polygons[0].IsInside(end))
                return false;

            // In LOS if it's the same start and end location
            if (Graph.Distance(start, end) < epsilon)
                return true;

            // Not in LOS if any edge is intersected by the start-end line segment 
            foreach (var polygon in walkableMap.Polygons)
            {
                if (polygon == null || !polygon.gameObject.activeInHierarchy) continue;

                for (int i = 0; i < polygon.Vertices.Count; i++)
                {
                    var v1 = polygon.Vertices[i];
                    var v2 = polygon.Vertices[(i + 1) % polygon.Vertices.Count];

                    if (LineSegmentsCross(start, end, v1, v2))
                    {
                        // In some cases a 'snapped' endpoint is just a little over the line due to rounding errors. So a margin is used to tackle those cases. 
                        if (Graph.DistanceToSegment(start, v1, v2) > epsilon && Graph.DistanceToSegment(end, v1, v2) > epsilon)
                        {
                            return false;
                        }
                    }
                }
            }

            // The middle point in the segment determines if in LOS or not
            GraphNode new_node = new((start.X + end.X) / 2, (start.Y + end.Y) / 2);
            bool inside = walkableMap.Polygons[0].IsInside(new_node);
            for (int i = 1; i < walkableMap.Polygons.Length; i++)
            {
                if (walkableMap.Polygons[i] == null || !walkableMap.Polygons[i].gameObject.activeInHierarchy) continue;

                if (walkableMap.Polygons[i].IsInside(new_node, false))
                {
                    inside = false;
                    break;
                }
            }

            return inside;
        }


        /// <summary>
        /// Determines whether two GraphNodes are in line of sight (LOS).
        /// This is true iff they are both located inside both the main polygon and outside the obstacles
        /// and there is no line segment that stands in their way.
        /// </summary>
        /// <returns>
        /// A list of GraphNode that makes up the shortest path from start point to end point 
        /// and a list of distances between two neighboring nodes.
        /// </returns>
        public PathLength GetPath()
        {
            var nodePath = new List<GraphNode>() { walkGraph.GraphNodes[0] };
            var lengths = new List<float>();

            for (int i = 1; i < path.Count; ++i)
            {
                nodePath.Add(walkGraph.GraphNodes[path[i]]);
                lengths.Add(walkGraph.FindEdge(path[i - 1], path[i]).Length);
            }
            return new(nodePath, lengths);
        }


        /// <summary>
        /// Executes function Run(Vector2 vStart, Vector2 vEnd) with parameter vStart as both vStart and vEnd.
        /// </summary>
        /// <returns>
        public PathLength Run(Vector2 vStart)
        {
            return Run(vStart, vStart);
        }


        /// <summary>
        /// Executes the main algorithm for finding the shortest path between two points.
        /// </summary>
        /// <returns>
        /// Result of GetPath()
        /// </returns>
        public PathLength Run(Vector2 vStart, Vector2 vEnd)
        {
            GraphNode nStart = new(vStart);
            GraphNode nEnd = new(vEnd);
            walkGraph = new(nStart, nEnd);
            walkGraph.AddNode(nStart);  // First in list
            walkGraph.AddNode(nEnd);    // Second in list

            bool mainWalkableArea = false;   // first one is main walkable area
            foreach (var polygon in walkableMap.Polygons)
            {
                if (polygon == null || !polygon.gameObject.activeInHierarchy) continue;

                if (polygon != null && polygon.Vertices != null && polygon.Vertices.Count > 2)     // Valid polygon
                {
                    if (!polygon.IsOrientationClockwise())  // must be clockwise
                        polygon.ReverseVertices();

                    for (int i = 0; i < polygon.Vertices.Count; i++)
                    {
                        if (IsVertexConcave(polygon.Vertices, i) == mainWalkableArea)
                            walkGraph.AddNode(polygon.Vertices[i]);
                    }
                }
                mainWalkableArea = true;
            }

            MakeConstraints(nStart, nEnd);

            // Check if every two nodes are in LOS
            for (int c1_index = 0; c1_index < walkGraph.GraphNodes.Count; c1_index++)
            {
                for (int c2_index = c1_index + 1; c2_index < walkGraph.GraphNodes.Count; c2_index++)
                {
                    if (IsInLineOfSight(walkGraph.GraphNodes[c1_index], walkGraph.GraphNodes[c2_index]))
                        walkGraph.AddEdge(c1_index, c2_index);
                }
            }

            path = new AStar(walkGraph).FindShortestPath();

            return GetPath();
        }

        /// <summary>
        /// Constrains the start and/or the end points to remain inside the main walkable area and outside the obstacles.
        /// </summary>
        private Tuple<GraphNode, GraphNode> MakeConstraints(GraphNode nStart, GraphNode nEnd)
        {
            // Constraints for main walkable area
            if (walkableMap.ConstrainStartMain && !walkableMap.MainWalkablePolygon.IsInside(nStart))
                UpdateStart(walkableMap.Polygons[0].GetClosestPointOnEdge(nStart));

            if (walkableMap.ConstrainEndMain && !walkableMap.MainWalkablePolygon.IsInside(nEnd))
                UpdateEnd(walkableMap.Polygons[0].GetClosestPointOnEdge(nEnd));

            // Constraints for obstacles
            for (int i = 0; i < walkableMap.obstaclePolygons.Count; ++i)
            {
                var polygon = walkableMap.obstaclePolygons[i];

                if (nStart == null || polygon == null || !polygon.gameObject.activeInHierarchy)
                    continue;

                if (walkableMap.ConstrainStartObstacles && polygon.IsInside(nStart))
                    UpdateStart(polygon.GetClosestPointOnEdge(nStart));

                if (walkableMap.ConstrainEndObstacles && polygon.IsInside(nEnd))
                    UpdateEnd(polygon.GetClosestPointOnEdge(nEnd));
            }

            return new Tuple<GraphNode, GraphNode>(nStart, nEnd);
        }

        /// <summary>
        /// Updates the Position of the start point and the graph node.
        /// </summary>
        private GraphNode UpdateStart(Vector3 pos)
        {
            //start.transform.Position = pos;
            GraphNode nStart = new(pos);
            walkGraph.GraphNodes[0] = nStart;
            walkGraph.Start = nStart;
            return nStart;
        }

        /// <summary>
        /// Updates the Position of the end point and the graph node.
        /// </summary>
        private GraphNode UpdateEnd(Vector3 pos)
        {
            //end.transform.Position = pos;
            GraphNode nEnd = new(pos);
            walkGraph.GraphNodes[1] = nEnd;
            walkGraph.End = nEnd;
            return nEnd;
        }

        public void DrawGizmos()
        {
            if (!walkableMap.DrawGraph && !walkableMap.DrawPath) return;
            foreach (var edges in walkGraph.GraphEdges.Values)
            {
                foreach (var edge in edges)
                {
                    Gizmos.color = walkableMap.GraphColor;

                    if (walkableMap.DrawPath && path.Contains(edge.N1) && path.Contains(edge.N2) && Math.Abs(path.IndexOf(edge.N1) - path.IndexOf(edge.N2)) <= 1)
                        Gizmos.color = walkableMap.PathColor;
                    else if (!walkableMap.DrawGraph)
                        continue;

                    var e = walkGraph.GetVectorEdge(edge);
                    Gizmos.DrawLine(e[0], e[1]);
                }
            }
        }
    }

    public class PathLength
    {
        public List<GraphNode> NodeList = new ();
        public List<float> Length = new ();

        public PathLength(List<GraphNode> n, List<float> f) 
        { 
            NodeList.AddRange(n);
            Length.AddRange(f);
        }
    }
}