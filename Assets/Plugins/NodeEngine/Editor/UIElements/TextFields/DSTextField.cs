using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.UIElements;
using NodeEngine.Window;
using NodeEngine.Ports;
using System.Linq;
using System;

namespace NodeEngine.UIElement
{
    public class DSTextField : TextField
    {
        public Type Type { get => this.GetType();}
        public object Value { get => text; set => text = value.ToString(); }
        public bool IsFunctions { get; set; } = false;
        public string Name { get; set; } = "Text";

        private DSGraphView graphView;
        internal ObservableDictionary<BasePort, string> anchors = new();
        TextElement anchorsTElement;

        public void Initialize(DSGraphView graphView)
        {
            this.graphView = graphView;

            AddManipulators();

            anchorsTElement = new TextElement();
            this.Add(anchorsTElement);
            this.RegisterValueChangedCallback(OnTextFieldValueChange);

            graphView.Anchors.OnDictionaryChangedEvent += OnGraphAnchorsChangedHandler;
            anchors.OnDictionaryChangedEvent += OnOwnAnchorsDictionaryChanged;
            CheckStartValue(graphView.Anchors);
        }
        internal void OnDistroy() {
            graphView.Anchors.OnDictionaryChangedEvent -= OnGraphAnchorsChangedHandler;
        }

        private void AddManipulators()
        {
            this.AddManipulator(CreateContextualMenuAnchors());
        }
        private IManipulator CreateContextualMenuAnchors()
        {
            ContextualMenuManipulator contextualMenuManipulator = new(e =>
            {
                if (graphView.Anchors.Count > 0)
                {
                    e.menu.ClearItems();
                    foreach (var anchor in graphView.Anchors)
                    {
                        if (!anchors.Contains(anchor))
                        {
                            e.menu.AppendAction($"Anchor:{anchor.Value}", a => 
                            {
                                anchors.Add(anchor.Key, anchor.Value);
                                this.value = this.value.Insert(this.cursorIndex, $"{{{anchor.Value}}}");
                            });
                        }
                    }
                }
            });
            return contextualMenuManipulator;
        }

        private void CheckStartValue(ObservableDictionary<BasePort, string> anchors)
        {
            foreach (var anchor in anchors)
            {
                if (this.text.Contains("{" + anchor.Value + "}"))
                    this.anchors.Add(anchor.Key, anchor.Value);
            }
        }

        private void OnTextFieldValueChange(ChangeEvent<string> evt)
        {
            DSTextField target = evt.target as DSTextField;
            Value = evt.newValue;

            if (target != null)
            {
                List<BasePort> presentAnchors = anchors
                    .Where(kv => !evt.newValue.Contains(kv.Value))
                    .Select(kv => kv.Key)
                    .ToList();

                if (presentAnchors.Count > 0)
                {
                    foreach (var anchor in presentAnchors)
                        anchors.Remove(anchor);
                }
            }
        }
        private void OnOwnAnchorsDictionaryChanged(object sender, DictionaryChangedEventArgs<BasePort, string> e)
        {
            List<TextElement> childs = anchorsTElement.GetElementsByType<TextElement>(e => e != anchorsTElement);
            foreach (var child in childs) anchorsTElement.Remove(child);
                
            if (anchors.Count > 0)
            {
                foreach (var item in anchors)
                {
                    TextElement newChild = new TextElement();
                    newChild.text = $"<color=#FDD057>{item.Value}</color>";
                    newChild.AddManipulator(new Clickable(() => OnAnchorClickEvent(newChild), 0, 0));
                    anchorsTElement.Add(newChild);
                }
            }
            else anchorsTElement.text = string.Empty;
        }
        private void OnAnchorClickEvent(TextElement textElement)
        {
            Regex regex = new Regex(@"<color=[^>]+>([^<]+)</color>");
            Match match = regex.Match(textElement.text);
            string data = "d_a_t_a";
            if (match.Success) data = match.Groups[1].Value;

            BasePort port = anchors.FirstOrDefault(e => e.Value.Contains($"{data}")).Key;
            if (port != null)
            {
                graphView.AddToSelection(port.node);
                graphView.FrameSelection();
            }
        }
        private void OnGraphAnchorsChangedHandler(object sender, DictionaryChangedEventArgs<BasePort, string> e)
        {
            if (e.ChangeType == DictionaryChangeType.Update) OnAnchorUpdate(e.Key, e.Value);
            else if (e.ChangeType == DictionaryChangeType.Remove) OnAnchorDelete(e.Key);
            else if (e.ChangeType == DictionaryChangeType.Add)
            {
                if (this.anchors.TryGetValue(e.Key, out string value)) { }
                else
                {
                    if (text.Contains("{" + e.Value + "}")) 
                        anchors.Add(e.Key, e.Value);
                }
            }
        }
        public void OnAnchorUpdate(BasePort port, string newRegex)
        {
            if (anchors.TryGetValue(port, out string regex))
            {
                this.text = this.text.Replace("{" + regex + "}", "{" + newRegex + "}");
                anchors[port] = newRegex;
            }
        }
        public void OnAnchorDelete(BasePort port)
        {
            if (anchors.TryGetValue(port, out string regex))
            {
                this.text = this.text.Replace(regex, port.Value != null ? port.Value.ToString() : "");
                anchors.Remove(port);
            }
        }


    }
}
