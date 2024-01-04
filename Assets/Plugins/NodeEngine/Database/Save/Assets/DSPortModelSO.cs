using NodeEngine.Ports;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NodeEngine.Database.Save
{
    [Serializable]
    public class DSPortModelSO : ScriptableObject
    {
        [SerializeField] public List<NodePortModelSO> NodeIDs;
        [SerializeField] public string PortID;
        [SerializeField] public string Value;
        [SerializeField] public string PortText;
        [SerializeField] public bool IsSingle;
        [SerializeField] public bool IsInput;
        [SerializeField] public bool IsIfPort;
        [SerializeField] public string IfPortSourceId;
        [SerializeField] public bool PlusIf;
        [SerializeField] public bool Cross;
        [SerializeField] public bool IsField;
        [SerializeField] public bool IsFunction;
        [SerializeField] public string Type;
        [SerializeField] public string[] AvailableTypes;
        [SerializeField] public PortSide PortSide;
        [SerializeField] public string Anchor;
        [SerializeField] public bool IsAnchorable;
        [SerializeField] public UnityEngine.Object AssetSource;

        internal void Init(DSPortModel dSPortModel, UnityEngine.Object parent)
        {
            NodeIDs = new List<NodePortModelSO>();

            PortID = dSPortModel.PortID;
            Value = dSPortModel.Value;
            PortText = dSPortModel.PortText;
            IsSingle = dSPortModel.IsSingle;
            IsInput = dSPortModel.IsInput;
            IsIfPort = dSPortModel.IsIfPort;
            IfPortSourceId = dSPortModel.IfPortSourceId;
            PlusIf = dSPortModel.PlusIf;
            Cross = dSPortModel.Cross;
            IsField = dSPortModel.IsField;
            IsFunction = dSPortModel.IsFunction;
            Type = dSPortModel.Type.ToString();
            AvailableTypes = dSPortModel.AvailableTypes;
            PortSide = dSPortModel.PortSide;
            IsAnchorable = dSPortModel.IsAnchorable;
            Anchor = dSPortModel.Anchor;
            AssetSource = dSPortModel.AssetSource;

            if (dSPortModel.NodeIDs != null)
            {
                foreach (var o in dSPortModel.NodeIDs)
                {
                    if (o != null)
                    {
                        NodePortModelSO dSNodePortModelSO = CreateInstance<NodePortModelSO>();
                        AssetDatabase.AddObjectToAsset(dSNodePortModelSO, parent);

                        dSNodePortModelSO.name = $"(PortModel){PortID}{o.NodeID}";
                        dSNodePortModelSO.Init(o);
                        NodeIDs.Add(dSNodePortModelSO);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    if (o == null)
                    {

                    }
                }
            }
        }
    }
}
