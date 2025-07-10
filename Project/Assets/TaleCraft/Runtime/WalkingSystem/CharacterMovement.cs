using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace TaleCraft.Movement
{
    /// <summary>
    /// Manages the movement of an object, primarly a character.
    /// </summary>
    public class CharacterMovement : MonoBehaviour
    {
        public WalkableMap WalkableMap;
        public PathFinder PathFinder;
        [SerializeField] private GameObject endingPoint;
        [SerializeField] private float baseSpeed = 1f;
        private bool running = false;
        private IEnumerator coroutine;
        private Vector3 end;

        void Start()
        {
            if (endingPoint != null)
                endingPoint.transform.position = transform.position;
            else end = transform.position;

            PathFinder = new(WalkableMap);
            PathFinder.Run(transform.position);
        }

        private void OnDrawGizmos()
        {
            if (WalkableMap == null)
                return;

            PathFinder ??= new(WalkableMap);

            if (endingPoint != null)
                end = endingPoint.transform.position;
            else if (!Application.isPlaying) end = transform.position;

            if (!Application.isPlaying)
                PathFinder.Run(transform.position, end);   // needed in edit mode but not in game mode

            PathFinder.DrawGizmos();
        }

        /// <summary>
        /// Find path and move object to the position.
        /// </summary>
        public async Task<bool> Move(Vector3 world_Position)
        {
            var path = PathFinder.Run(transform.position, world_Position);

            bool result = await Move(path);
            return result;
        }

        /// <summary>
        /// Move object to the position.
        /// </summary>
        public async Task<bool> Move(PathLength path)
        {
            Vector2 end = path.NodeList[path.NodeList.Count - 1].GetLocation();
            this.end = end;

            if (endingPoint != null)
                endingPoint.transform.position = end;

            // If the starting point is still running, stop and move toward a new goal
            if (running)
                StopCoroutine(coroutine);
            running = true;

            TaskCompletionSource<bool> tcs = new();
            coroutine = LerpPosition(path, baseSpeed, tcs);
            StartCoroutine(coroutine);

            // Await the completion of the task, which will be signaled by the coroutine
            bool result = await tcs.Task;
            return result;
        }

        /// <summary>
        /// Manages the transition along the path with given speed.
        /// </summary>
        IEnumerator LerpPosition(PathLength path, float baseSpeed, TaskCompletionSource<bool> tcs)
        {
            for (int i = 1; i < path.NodeList.Count; i++)
            {
                float time = 0;
                Vector3 startPosition = new(path.NodeList[i - 1].X, path.NodeList[i - 1].Y, 0);
                Vector3 stopPosition = new(path.NodeList[i].X, path.NodeList[i].Y, 0);
                float segmentDistance = path.Length[i - 1];

                while (time < 1f)
                {
                    // Adjust speed based on current scale
                    float currentScale = GetCurrentScale();
                    float adjustedSpeed = baseSpeed * currentScale;
                    float delta = (adjustedSpeed * Time.deltaTime) / segmentDistance;
                    time += delta;

                    transform.position = Vector3.Lerp(startPosition, stopPosition, Mathf.Clamp01(time));
                    yield return null;
                }
            }

            running = false;
            tcs.SetResult(true);
        }

        /// <summary>
        /// Returns the ratio between the current scale and the default scale.
        /// </summary>
        private float GetCurrentScale()
        {
            if (!TryGetComponent<SpriteScaler>(out var s))
                return 1f;

            return s.Ratio;
        }

        /// <summary>
        /// Move character to a position.
        /// </summary>
        public async void MoveTo(GameObject newPos)
        {
            await Run(newPos);
        }

        private async Task<bool> Run(GameObject newPos)
        {
            var path = PathFinder.Run(transform.position, newPos.transform.position);
            var fin = await Move(path);
            return fin;
        }
    }
}