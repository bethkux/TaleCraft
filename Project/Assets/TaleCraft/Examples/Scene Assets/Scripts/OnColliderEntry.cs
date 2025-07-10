using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaleCraft.Example
{
    public class OnColliderEntry : MonoBehaviour
    {
        public List<ColliderEvents> Events = new();

        void OnTriggerEnter2D(Collider2D col)
        {
            foreach (var a in Events)
            {
                if (col == a.Collider)
                {
                    a.Event.Invoke();
                }
            }
        }
    }


    [System.Serializable]
    public class ColliderEvents
    {
        public Collider2D Collider;
        public UnityEvent Event;
    }
}