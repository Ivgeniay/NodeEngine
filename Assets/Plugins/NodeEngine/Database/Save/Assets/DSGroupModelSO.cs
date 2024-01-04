using UnityEngine;
using System;

namespace NodeEngine.Database.Save
{
    [Serializable]
    public class DSGroupModelSO : ScriptableObject
    {
        [SerializeField] public string ID;
        [SerializeField] public string Type;
        [SerializeField] public string GroupName;
        [SerializeField] public Vector2 Position;

        public void Init(DSGroupModel model)
        {
            ID = model.ID;
            Type = model.Type;
            GroupName = model.GroupName;
            Position = model.Position;
        }
    }
}
