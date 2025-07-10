using System;
using UnityEngine;

namespace TaleCraft.Movement
{
    /// <summary>
    /// Manages scaling of an object for perspective.
    /// </summary>
    [ExecuteAlways]
    public class SpriteScaler : MonoBehaviour
    {
        [SerializeField] private ScalerType scalerType = ScalerType.None;
        [SerializeField] private PerspectiveType perspectiveType = PerspectiveType.Linear;
        [Range(0.1f, 5.0f)]
        [SerializeField] private float perspectiveFactor = 1.0f;

        // Custom scaling
        [SerializeField] private Polygon mainWalkablePolygon;
        [SerializeField] private float baseScale = 1f;
        private float newScale = 1f;
        [HideInInspector] public float Ratio => newScale / baseScale;

        // X/Y scaling
        [SerializeField] private float upperScale = 0.4f;
        [SerializeField] private float lowerScale = 0.8f;

        public enum ScalerType
        {
            None,
            XScaling,
            YScaling,
            Custom
        }

        public enum PerspectiveType
        {
            Linear,
            Hyperbolic
        }

        private void Update()
        {
            // Choose scaling method
            switch (scalerType)
            {
                case ScalerType.None:
                    newScale = baseScale;
                    break;
                case ScalerType.YScaling:
                    newScale = CalculateYScale();
                    break;
                case ScalerType.XScaling:
                    newScale = CalculateXScale();
                    break;
                case ScalerType.Custom:
                    newScale = CalculateCustomScale();
                    break;
            }

            transform.localScale = new(newScale, newScale, 1f);
        }

        /// <summary>
        /// Calculates scaling by modifying <c>scale</c> variable in <c>Point</c> class.
        /// </summary>
        private float CalculateCustomScale()
        {
            var node = new GraphNode(transform.position);

            double sum = 0;
            double exps = 0;

            if (mainWalkablePolygon == null || mainWalkablePolygon.Vertices == null)
                return baseScale;

            foreach (var main_vertex in mainWalkablePolygon.Vertices)
            {
                if (main_vertex.P == null || main_vertex.P.scale == -1)
                { continue; }

                var dist = Graph.Distance(main_vertex, node);
                var exp = Math.Exp(dist * (-1));
                exps += exp;
                var x = exp * main_vertex.P.scale;
                sum += x;
            }
            float result = (float)sum / (float)exps;
            result *= baseScale;
            return result;
        }


        /// <summary>
        /// Calculates scaling by choosing <c>lowerScale</c> and <c>upperScale</c> within the main walkable area.
        /// </summary>
        private float CalculateYScale()
        {
            if (mainWalkablePolygon == null || mainWalkablePolygon.Vertices == null)
                return baseScale;

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            // Find lowest and highest Y values for all Vertices in the main walkable area
            for (int i = 0; i < mainWalkablePolygon.Vertices.Count; i++)
            {
                var y = mainWalkablePolygon.Vertices[i].Y;
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            if (minY >= maxY)
                return baseScale;

            var areaHeight = Mathf.Abs(maxY - minY);
            var scaleDifference = lowerScale - upperScale;

            return perspectiveType switch
            {
                PerspectiveType.Linear => ClampScale(PerspeciveYScaling(minY, areaHeight, 1f), scaleDifference),
                PerspectiveType.Hyperbolic => ClampScale(PerspeciveYScaling(minY, areaHeight, perspectiveFactor), scaleDifference),
                _ => baseScale,
            };
        }

        /// <summary>
        /// Calculates scaling by choosing <c>lowerScale</c> and <c>upperScale</c> within the main walkable area.
        /// </summary>
        private float CalculateXScale()
        {
            if (mainWalkablePolygon == null || mainWalkablePolygon.Vertices == null)
                return baseScale;

            float minX = float.MaxValue;
            float maxX = float.MinValue;

            // Find lowest and highest Y values for all Vertices in the main walkable area
            for (int i = 0; i < mainWalkablePolygon.Vertices.Count; i++)
            {
                var x = mainWalkablePolygon.Vertices[i].X;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
            }

            if (minX >= maxX)
                return baseScale;

            var areaWidth = Mathf.Abs(maxX - minX);
            var scaleDifference = lowerScale - upperScale;

            return perspectiveType switch
            {
                PerspectiveType.Linear => ClampScale(PerspeciveXScaling(minX, areaWidth, 1f), scaleDifference),
                PerspectiveType.Hyperbolic => ClampScale(PerspeciveXScaling(minX, areaWidth, perspectiveFactor), scaleDifference),
                _ => baseScale,
            };
        }

        /// <summary>
        /// Clamps the input scale between 0 and 1, then calculates a scaled value
        /// adjusted by a given difference and base parameters.
        /// </summary>
        private float ClampScale(float newPosScale, float scaleDifference)
        {
            // Clamp the new position scale between 0 and 1
            newPosScale = Mathf.Clamp01(newPosScale);
            return (lowerScale - scaleDifference * newPosScale) * baseScale;
        }

        private float PerspeciveYScaling(float minY, float areaHeight, float perspFactor)
        {
            return Mathf.Pow((transform.position.y - minY) / areaHeight, perspFactor);
        }

        private float PerspeciveXScaling(float minX, float areaHeight, float perspFactor)
        {
            return Mathf.Pow((transform.position.x - minX) / areaHeight, perspFactor);
        }
    }
}