using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.GUI
{
	public class CodeDescriptionBoolTreeRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionBoolTreeRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, fallbackLevel, null)
		{
			this.editorInfo = (CodeDescriptionBoolTreeRegistryEditorInfo)editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CodeDescriptionBoolTreeControl(editorInfo);
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected CodeDescriptionBoolTreeRegistryEditorInfo EditorInfo
		{
			get { return editorInfo; }
		}

		readonly CodeDescriptionBoolTreeRegistryEditorInfo editorInfo;
	}
}
