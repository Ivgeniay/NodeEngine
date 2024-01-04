using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEngine.Database.Save
{
    [Serializable]
    public class NodePortModelSO : ScriptableObject
    {
        [SerializeField] public string NodeID;
        [SerializeField] public List<string> PortIDs;

        internal void Init(NodePortModel o)
        {
            NodeID = o.NodeID;
            PortIDs = new List<string>();
            if (o.PortIDs != null || o.PortIDs.Count > 0)
            {
                foreach (string id in o.PortIDs) PortIDs.Add(id);
            }
            else
            {

            }
        }
    }
}
