using NodeEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NodeEngine.Database.Save
{
    [Serializable]
    public class DSNodeModelSO : ScriptableObject
    {
        [SerializeField] public List<DSPortModelSO> Outputs;
        [SerializeField] public List<DSPortModelSO> Inputs;
        [SerializeField] public string DialogueType;
        [SerializeField] public Vector2 Position;
        [SerializeField] public string NodeName;
        [SerializeField] public string GroupID;
        [SerializeField] public string Text;
        [SerializeField] public int Minimal;
        [SerializeField] public string ID;

        public void Init(DSNodeModel dSNodeModel, UnityEngine.Object parent)
        {
            Outputs = new List<DSPortModelSO>();
            Inputs = new List<DSPortModelSO>();

            ID = dSNodeModel.ID;
            NodeName = dSNodeModel.NodeName;
            Minimal = dSNodeModel.Minimal;
            GroupID = dSNodeModel.GroupID;
            Text = dSNodeModel.Text;
            DialogueType = dSNodeModel.DialogueType;
            Position = dSNodeModel.Position;

            foreach (var portModel in dSNodeModel.Outputs)
            {
                DSPortModelSO dSPortModelSO = ScriptableObject.CreateInstance<DSPortModelSO>();
                AssetDatabase.AddObjectToAsset(dSPortModelSO, parent);

                dSPortModelSO.name = $"(OutputPort){portModel.PortID}";
                dSPortModelSO.Init(portModel, dSPortModelSO);
                Outputs.Add(dSPortModelSO);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            foreach (var portModel in dSNodeModel.Inputs)
            {
                DSPortModelSO dSPortModelSO = ScriptableObject.CreateInstance<DSPortModelSO>();
                AssetDatabase.AddObjectToAsset(dSPortModelSO, parent);

                dSPortModelSO.name = $"(InputPort){portModel.PortID}";
                dSPortModelSO.Init(portModel, dSPortModelSO);
                Inputs.Add(dSPortModelSO);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public DSNodeModel Deconstruct()
        {
            DSNodeModel Model = new()
            {
                ID = this.ID,
                DialogueType = this.DialogueType,
                Position = this.Position,
                GroupID = this.GroupID,
                Outputs = new(),
                Inputs = new(),
                Minimal = this.Minimal,
                NodeName = this.NodeName,
                Text = this.Text
            };

            DecompForDsPortModelList(Model.Outputs, this.Outputs);
            DecompForDsPortModelList(Model.Inputs, this.Inputs);

            return Model;
        }

        private void DecompForDsPortModelList(List<DSPortModel> dSPortModel, List<DSPortModelSO> dSPortModelSO)
        {
            foreach (DSPortModelSO outputsSO in dSPortModelSO)
            {
                Type[] aTypes = outputsSO.AvailableTypes.Select(x => Type.GetType(x)).ToArray();
                Type type = DSUtilities.GetType(outputsSO.Type);

                if (!string.IsNullOrWhiteSpace(outputsSO.Type) && type == null)
                {
                    Assembly assembly = Assembly.Load(DSConstants.DEFAULT_ASSEMBLY);
                    type = assembly.GetType(outputsSO.Type);
                }

                DSPortModel portModel = new DSPortModel(aTypes, outputsSO.PortSide)
                {
                    PortID = outputsSO.PortID,
                    Cross = outputsSO.Cross,
                    IsField = outputsSO.IsField,
                    IsIfPort = outputsSO.IsIfPort,
                    IfPortSourceId = outputsSO.IfPortSourceId,
                    IsInput = outputsSO.IsInput,
                    IsSingle = outputsSO.IsSingle,
                    PortText = outputsSO.PortText,
                    Type = type,
                    Value = outputsSO.Value?.ToString(),
                    NodeIDs = new(),
                    IsFunction = outputsSO.IsFunction,
                    PlusIf = outputsSO.PlusIf,
                    PortSide = outputsSO.PortSide,
                    Anchor = outputsSO.Anchor,
                    IsAnchorable = outputsSO.IsAnchorable,
                    AvailableTypes = outputsSO.AvailableTypes,
                    AssetSource = outputsSO.AssetSource,
                };

                foreach (NodePortModelSO nodeIds in outputsSO.NodeIDs)
                {
                    var pModel = new NodePortModel()
                    {
                        NodeID = nodeIds.NodeID,
                        PortIDs = new()
                    };
                    foreach (var portConnectionId in nodeIds.PortIDs)
                    {
                        pModel.PortIDs.Add(portConnectionId);
                    }

                    portModel.NodeIDs.Add(pModel);
                }
                dSPortModel.Add(portModel);
            }
        }
    }
}
