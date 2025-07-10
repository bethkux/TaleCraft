using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace TaleCraft.Dialogue.Editor
{
    /// <summary>
    /// Data for group of nodes.
    /// </summary>
    public class BaseGroup : Group
    {
        public string GroupGuid;

        public BaseGroup() 
        {
            GroupGuid = GUID.Generate().ToString();
        }
    }
}