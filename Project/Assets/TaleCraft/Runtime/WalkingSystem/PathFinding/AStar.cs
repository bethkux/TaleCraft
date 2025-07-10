using System;
using System.Collections.Generic;
using System.Linq;

namespace TaleCraft.Movement
{
    // Inspired by pseudocode from https://en.wikipedia.org/wiki/A*_search_algorithm
    /// <summary>
    /// Implements A* algorithm for finding the shortest path in the <c>Graph</c> class.
    /// </summary>
    public class AStar
    {
        /// <value>Represents the graph of all <c>GraphNode</c>s connected by edges.</value>
        private readonly Graph graph;
        /// <value>The set of discovered nodes that may need to be (re-)expanded. Initially, only the start node is known.</value>
        private List<int> openSet;
        /// <value>Determines the node immediately preceding it on the cheapest path from the start to <c>n</c> currently known.</value>
        private int[] cameFrom;
        /// <value>The cost of the cheapest path from start to <c>n</c> currently known.</value>
        private float[] gScore;
        /// <value>Represents our current best guess as to how cheap a path could be from start to finish if it goes through <c>n</c>. Definition: <c>fScore[n] := gScore[n] + h(n)</c></value>
        private float[] fScore;

        private const int StartIdx = 0;     // Start node is always first
        private const int EndIdx = 1;       // End node is always second

        public AStar(Graph graph) => this.graph = graph;

        /// <summary>
        /// Heuristic function that estimates the cost to reach goal from node n.
        /// In this case, the value is computed by determining the euclidian distance from the given node to the end node.
        /// </summary>
        private float Heuristic(GraphNode g)
        {
            return Graph.Distance(g, graph.End);
        }

        /// <summary>
        /// Finds a node with lowest value in <c>fScore</c>.
        /// </summary>
        private int LowestNode()
        {
            int result = openSet[0];
            foreach (int i in openSet)
            {
                if (fScore[i] < fScore[result])
                    result = i;
            }
            return result;
        }

        /// <summary>
        /// Constructs a path from the Data in <c>cameFrom</c>.
        /// </summary>
        /// <returns>
        /// A list of indices representing a <c>GraphNode</c> edges from the starting node to the end.
        /// </returns>
        private List<int> ConstructPath(int current)
        {
            List<int> total_path = new() { current };
            while (current != StartIdx)
            {
                current = cameFrom[current];
                total_path.Add(current);
            }
            total_path.Reverse();
            return total_path;
        }

        /// <summary>
        /// Finds the shortest path.
        /// </summary>
        /// <returns>
        /// A list of indices representing a <c>GraphNode</c> edges from the starting node to the end.
        /// </returns>
        public List<int> FindShortestPath()
        {
            // Initilize a list and arrays
            openSet = new List<int> { StartIdx };
            cameFrom = Enumerable.Repeat(-1, graph.GraphNodes.Count).ToArray();

            gScore = new float[graph.GraphNodes.Count];
            Array.Fill(gScore, float.MaxValue);
            gScore[StartIdx] = 0;

            fScore = new float[graph.GraphNodes.Count];
            Array.Fill(fScore, float.MaxValue);
            fScore[StartIdx] = Heuristic(graph.Start);

            while (openSet.Count != 0)  // while openSet not empty
            {
                int current = LowestNode(); // find a node with the lowest fScore
                if (current == EndIdx)      // path found, construct path
                    return ConstructPath(current);

                openSet.Remove(current);

                foreach (var neighbor in graph.Neighbors[current])  // go through neighbors of the current node
                {
                    // tentative_gScore is the distance from start to the neighbor through current
                    var tentative_gScore = gScore[current] +
                        Graph.Distance(graph.GraphNodes[current], graph.GraphNodes[neighbor]);

                    if (tentative_gScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = tentative_gScore + Heuristic(graph.GraphNodes[neighbor]);
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return new List<int>(); // openSet is empty but goal was never reached
        }
    }
}