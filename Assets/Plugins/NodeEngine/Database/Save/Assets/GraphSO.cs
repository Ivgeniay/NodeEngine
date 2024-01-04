using NodeEngine.Database.Save;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace NodeEngine.Save
{
    public class GraphSO : ScriptableObject
    {
        [SerializeField] public string FileName;
        [SerializeField] public List<DSNodeModelSO> NodeModels;
        [SerializeField] public List<DSGroupModelSO> GroupModels;

        public void Init(string fileName, List<DSNodeModel> nodes, List<DSGroupModel> groups, UnityEngine.Object parent, Action<float, float> callback = null)
        {
            FileName = fileName;
            NodeModels = new List<DSNodeModelSO>();
            GroupModels = new List<DSGroupModelSO>();
            int counter = 0;
            int counValues = nodes.Count + groups.Count;

            foreach (DSGroupModel group in groups)
            {
                DSGroupModelSO dSGroupModel = ScriptableObject.CreateInstance<DSGroupModelSO>();
                AssetDatabase.AddObjectToAsset(dSGroupModel, parent);

                dSGroupModel.name = $"{Type.GetType(group.Type).Name}({group.GroupName})";
                dSGroupModel.Init(group);
                this.GroupModels.Add(dSGroupModel);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                counter++;
                callback?.Invoke(counter, counValues);
            }

            foreach (DSNodeModel node in nodes)
            {
                DSNodeModelSO dSNodeModelSO = ScriptableObject.CreateInstance<DSNodeModelSO>();
                AssetDatabase.AddObjectToAsset(dSNodeModelSO, parent);

                dSNodeModelSO.name = $"{Type.GetType(node.DialogueType).Name}({node.NodeName}";
                dSNodeModelSO.Init(node, dSNodeModelSO);
                this.NodeModels.Add(dSNodeModelSO);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                counter++;
                callback?.Invoke(counter, counValues);
            }
        }
    }
}
