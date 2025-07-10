using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using TaleCraft.Movement;
using TaleCraft.Inventory;

namespace TaleCraft.Commands
{
    /// <summary>
    /// Applied to a non-UI GameObject that can be interacted with by the user.
    /// </summary>
    public class WorldObject : Interactable
    {
        public WorldItem Item;
        [SerializeField] private bool showTag;
        [SerializeField] private Vector2[] goToPoints;
        [Tooltip("The point position is either local (true) or global (false). In other words: It determines if the position of the points is moved together with the object that this script is attached to.")]
        [SerializeField] private bool moveWithObject = true;
        [SerializeField] private float pointSize = 0.3f;
        [SerializeField] private Color pointColor = Color.white;

        public UnityEvent OnCursorEnter;
        public UnityEvent OnCursorExit;

        public Vector2[] GoToPoints => goToPoints;
        public float PointSize => pointSize;
        public Color PointColor => pointColor;
        public bool MoveWithObject { get => moveWithObject; set => moveWithObject = value; }


        public void OnMouseEnter()
        {
            if (showTag)
                TagManager.Instance.TagDesc.GetComponent<TextMeshProUGUI>().text = Item.Name;

            OnCursorEnter?.Invoke();
            CommandManager.Instance.ActionTemp = new(Item, this);
        }

        public void OnMouseExit()
        {
            // Remove the description tag when the mouse exits the Item slot.
            if (showTag && TagManager.Instance.TagDesc != null)
                TagManager.Instance.TagDesc.GetComponent<TextMeshProUGUI>().text = "";

            OnCursorExit?.Invoke();
            CommandManager.Instance.ActionTemp = null;
        }

        /// <summary>
        /// Logic when moving closer to the EnvironmentObject.
        /// </summary>
        protected async override Task<bool> MoveCloser(Vector3 mousePos)
        {
            CharacterMovement movement = CommandManager.Instance.Player;

            if (!movement.enabled)
                return false;

            var start = movement.gameObject.transform.position;
            bool finished;

            // If there are any go-to-points defined, find the closest one
            if (goToPoints.Length > 0)
            {
                PathLength[] paths = new PathLength[goToPoints.Length];
                var shortestPath = float.MaxValue;
                var shortestIdx = -1;

                // Run through all of them and find the one that's the closest from the current Position of the player.
                for (int i = 0; i < goToPoints.Length; i++)
                {
                    var length = 0f;
                    var pos = (Vector3)goToPoints[i];
                    if (moveWithObject)
                        pos += transform.position;
                    var path = movement.PathFinder.Run(start, pos);
                    paths[i] = path;
                    foreach (var l in path.Length)
                    {
                        length += l;
                    }
                    if (length < shortestPath)  // Update shortest distance.
                    {
                        shortestPath = length;
                        shortestIdx = i;
                    }
                }

                if (shortestIdx == -1) Debug.Log("ERROR");
                finished = await movement.Move(paths[shortestIdx]);
            }
            // Otherwise find the closest point to the Interactable on the walking map
            else
                finished = await movement.Move(mousePos);

            return finished;
        }
    }
}