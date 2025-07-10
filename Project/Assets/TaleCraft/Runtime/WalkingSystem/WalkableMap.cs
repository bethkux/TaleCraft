using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaleCraft.Movement
{
    /// <summary>
    /// Manages the logic behind the map in a given scene.
    /// This includes finding the shortest path between start and end points.
    /// <para>Inspiration: <see href="https://www.groebelsloot.com/2016/03/13/pathfinding-part-2/">HERE</see></para>
    /// </summary>
    [ExecuteAlways]
    public class WalkableMap : MonoBehaviour
    {
        #region VARIABLES MEMBRES

        public List<Polygon> obstaclePolygons;

        [SerializeField] private Polygon[] polygons;
        [SerializeField] private Polygon mainWalkablePolygon;
        [Tooltip("Constrains the start points to remain inside the main walkable area a")]
        [SerializeField] private bool StartMain = true;
        [Tooltip("Constrains the end points to remain inside the main walkable area ")]
        [SerializeField] private bool EndMain = true;
        [Tooltip("Constrains the start points to remain outside the obstacles.")]
        [SerializeField] private bool StartObstacles = true;
        [Tooltip("Constrains the end points to remain outside the obstacles.")]
        [SerializeField] private bool EndObstacles = true;
        [SerializeField] private bool Graph = true;
        [SerializeField] private bool Path = true;
        [SerializeField] private Color graphColor = Color.gray;
        [SerializeField] private Color pathColor = Color.green;

        public Polygon MainWalkablePolygon => mainWalkablePolygon;
        public Polygon[] Polygons => polygons;
        public bool ConstrainStartMain => StartMain;
        public bool ConstrainEndMain => EndMain;
        public bool ConstrainStartObstacles => StartObstacles;
        public bool ConstrainEndObstacles => EndObstacles;
        public bool DrawGraph => Graph;
        public bool DrawPath => Path;
        public Color GraphColor => graphColor;
        public Color PathColor => pathColor;

        #endregion

        private void Awake()
        {
            if (Application.IsPlaying(gameObject))
                ListChildrenWithComponent();
            RemoveNullPolygons();
        }

        private void Update()
        {
            if (!Application.IsPlaying(gameObject))
                RemoveNullPolygons();
        }

        private void OnEnable()
        {
            ListChildrenWithComponent();
        }

        /// <summary>
        /// Removes null polygons.
        /// </summary>
        public void RemoveNullPolygons()
        {
            int cnt = obstaclePolygons.Count;
            for (int i = obstaclePolygons.Count - 1; i >= 0; --i)
            {
                if (obstaclePolygons[i] == null)
                    obstaclePolygons.RemoveAt(i);
            }

            if (cnt != obstaclePolygons.Count || polygons.Length != cnt + 1)
                polygons = new Polygon[] { mainWalkablePolygon }.Concat(obstaclePolygons).ToArray();
        }

        /// <summary>
        /// Lists all child objects of the current GameObject that have the specified component.
        /// </summary>
        public void ListChildrenWithComponent()
        {
            obstaclePolygons = new();
            // Iterate over all child objects
            foreach (Transform child in transform.Find("Obstacles"))
            {
                if (child.GetComponent<Polygon>() != null)
                    obstaclePolygons.Add(child.GetComponent<Polygon>());
            }
        }
    }
}