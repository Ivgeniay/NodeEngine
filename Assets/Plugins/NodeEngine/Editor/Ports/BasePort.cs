using UnityEditor.Experimental.GraphView;
using NodeEngine.Manipulations;
using UnityEngine.UIElements;
using NodeEngine.Window;
using NodeEngine.Edges;
using System.Reflection;
using UnityEngine;
using System;
using NodeEngine.Nodes;
using System.Linq;
using NodeEngine.Database.Save;
using UnityEditor;

namespace NodeEngine.Ports
{
    public class BasePort : Port
    {
        public string ID { get; set; }
        public bool IsFunctions { get; set; }
        public BasePort IfPortSource { get; set; }
        public object Value { get; private set; }
        public Type Type { get => portType; private set => portType = value; }
        public string Name { get => portName; private set => portName = value; } 
        public UnityEngine.Object AssetSource { get; set; }

        public DSGraphView GrathView { get; internal set; }

        public string Anchor = string.Empty;

        public Type[] AvailableTypes;
        public PortSide PortSide;
        public bool IsAnchorable = false;

        private Color defaultColor;
        
        public BasePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type) { }
        public static BasePort CreateBasePort<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type) where TEdge : DSEdge, new()
        {
            Type edgeConnectorType = typeof(CustomEdgeConnector<>).MakeGenericType(typeof(TEdge));
            Type outerType = typeof(Port);
            Type innerType = outerType.GetNestedType("DefaultEdgeConnectorListener", BindingFlags.NonPublic);

            object listenerInstance = Activator.CreateInstance(innerType);
            object edgeConnectorInstance = Activator.CreateInstance(edgeConnectorType, listenerInstance);

            BasePort port = new BasePort(orientation, direction, capacity, type);

            Type Type = typeof(BasePort);
            FieldInfo edgeConnectorField = Type.GetField("m_EdgeConnector", BindingFlags.NonPublic | BindingFlags.Instance);

            if (edgeConnectorField != null)
                edgeConnectorField.SetValue(port, edgeConnectorInstance);

            port.AddManipulators();
            port.defaultColor = port.portColor;
            EditorApplication.update += port.Update;
            return port;
        }

        private void Update() { }
        internal void OnDistroy()
        {
            EditorApplication.update -= Update;
            BasePortManager.UnRegister(this);
            if (!string.IsNullOrWhiteSpace(Anchor))
            {
                GrathView?.RemoveAnchor(this);
                BaseNode node = this.node as BaseNode;
                DSPortModel model = null;
                switch (PortSide)
                {
                    case PortSide.Input:
                        model = node.Model.Inputs.FirstOrDefault(e => e.PortID == ID);
                        break;
                    case PortSide.Output:
                        model = node.Model.Outputs.FirstOrDefault(e => e.PortID == ID);
                        break;
                }
                if (model != null) model.Anchor = string.Empty;
            }
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            if (edge.output != null && edge.input == this)
            {
                BasePort other = edge.output as BasePort;
                if (Type == other.Type) return;
                if (!BasePortManager.HaveCommonTypes(other.Type, AvailableTypes))
                {
                    Disconnect(edge);
                    edge.output.portCapLit = false;
                    edge.input.portCapLit = false;
                }
            }
        }
        public override void Disconnect(Edge edge)
        {
            if (edge != null)
            {
                GraphView graphView = GetFirstAncestorOfType<GraphView>();
                if (graphView != null)
                {
                    var t = new GraphViewChange();
                    t.elementsToRemove = new() { edge };
                    graphView.graphViewChanged?.Invoke(t);
                    graphView.RemoveElement(edge);
                }
            }
            base.Disconnect(edge);
        }

        public override void DisconnectAll()
        {
            var connect = connections.ToList();
            for (int i = 0; i < connect.Count(); i++)
            {
                BasePort inp = connect[i].input as BasePort;
                BasePort outp = connect[i].output as BasePort;
                inp.Disconnect(connect[i]);
                outp.Disconnect(connect[i]);
            }

            base.DisconnectAll();
        }

        public override void OnStartEdgeDragging()
        {
            if (this.m_EdgeConnector?.edgeDragHelper?.draggedPort == this)
                BasePortManager.CallStartDrag(this);
            base.OnStartEdgeDragging();
        }
        public override void OnStopEdgeDragging()
        {
            base.OnStopEdgeDragging();
            
            if (this.m_EdgeConnector?.edgeDragHelper?.draggedPort == this)
                BasePortManager.CallStopDrag(this);
        }

        private void AddManipulators()
        {
            this.AddManipulator(this.m_EdgeConnector);
            StartDragManipulator startDrag = new StartDragManipulator(this);
            this.AddManipulator(startDrag);

            this.AddManipulator(CreateContextualMenu());
        }
        internal void AddOrUpdateAnchor(string anchorName)
        {
            Anchor = anchorName;
            BaseNode node = this.node as BaseNode;
            DSPortModel model = null;
            switch (PortSide)
            {
                case PortSide.Input:
                    model = node.Model.Inputs.FirstOrDefault(e => e.PortID == ID);
                    break;
                case PortSide.Output:
                    model = node.Model.Outputs.FirstOrDefault(e => e.PortID == ID);
                    break;
            }
            if (model != null) model.Anchor = anchorName;
            GrathView?.AddOrUpdateAnchor(this, anchorName);
            if (string.IsNullOrWhiteSpace(Anchor))
            {
                this.portColor = defaultColor;
                this.tooltip = null;
            }
            else
            {
                if (ColorUtility.TryParseHtmlString("#FDD057", out Color col)) this.portColor = col;
                else this.portColor = Color.yellow;
                this.tooltip = Anchor;
            }
        }

        internal void SetValue(object value)
        {
            if (value is UnityEngine.Object uObj) AssetSource = uObj;
            Value = value;
        }
        internal void SetPortType(Type type) => Type = type;
        internal void ChangeName(string name) => Name = name;
        

        private IManipulator CreateContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new(e =>
            {
                if (IsAnchorable)
                {
                    e.menu.AppendAction("Anchor", a => { DSAnchorWindow.OpenWindow(this); });
                }
            });
            return contextualMenuManipulator;
        }

    }
}
