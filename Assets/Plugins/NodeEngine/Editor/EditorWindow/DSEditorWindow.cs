using NodeEngine.Toolbars;
using NodeEngine.Utilities;
using UnityEditor;
using UnityEngine.UIElements;

namespace NodeEngine.Window
{
    public class DSEditorWindow : EditorWindow
    {
        public StyleSheet StyleSheet;
        private string stylesLink = "Assets/Plugins/NodeEngine/Resources/Front/NodeEngineVariables.uss";
        DSGraphView grathView;


        [MenuItem("NodeEngine/OpenWindow")]
        public static void OpenWindow()
        {
            GetWindow<DSEditorWindow>("Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }


        #region Elements Addition
        private void AddGraphView()
        {
            grathView = new DSGraphView(this);
            grathView.StretchToParentSize();

            rootVisualElement.Add(grathView);
        }
        private void AddToolbar()
        {
            DSToolbar toolbar = new(grathView);
            toolbar.Initialize("Toolbar", "Filename: ");
            rootVisualElement.Add(toolbar);
        }
        #endregion

        #region Styles
        private void AddStyles()
        {
            if (StyleSheet == null) rootVisualElement.LoadAndAddStyleSheets(stylesLink);
            else rootVisualElement.LoadAndAddStyleSheets(StyleSheet);
        }
        #endregion

    }
}
