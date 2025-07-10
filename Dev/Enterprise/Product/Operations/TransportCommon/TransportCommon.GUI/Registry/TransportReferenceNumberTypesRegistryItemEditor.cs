using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public class TransportReferenceNumberTypesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public TransportReferenceNumberTypesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new TransportReferenceNumberTypesRegistryControl();
		}

		protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
		{
			((TransportReferenceNumberTypesRegistryControl)editorPane).ReadOnly = !enabled;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
