using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TaleCraft.Dialogue
{
    /// <summary>
    /// Manages registration of multiple EventContainers to their respective events.
    /// </summary>
    public class EventListener : MonoBehaviour
    {
        public List<EventContainer> Events;

        void OnEnable()
        {
            foreach (EventContainer ev in Events)
            {
                ev.AddListener();
            }
        }

        void OnDisable()
        {
            foreach (EventContainer ev in Events)
            {
                ev.RemoveListener();
            }
        }
    }

    /// <summary>
    /// Handles adding and removing listeners to the UnityEventObject's event.
    /// </summary>
    [System.Serializable]
    public class EventContainer
    {
        public UnityEventObject Event;
        public UnityEvent Response;

        public void AddListener()
        {
            if (Event != null)
                Event.Event.AddListener(OnEventRaised);
        }

        public void RemoveListener()
        {
            if (Event != null)
                Event.Event.RemoveListener(OnEventRaised);
        }

        void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}