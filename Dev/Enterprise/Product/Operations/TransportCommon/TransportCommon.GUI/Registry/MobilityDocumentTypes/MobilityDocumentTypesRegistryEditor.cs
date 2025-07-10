using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public class MobilityDocumentTypesRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public MobilityDocumentTypesRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new MobilityDocumentTypesControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
