using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public class OrganisationRTUSEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public OrganisationRTUSEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new OrganisationRTUSControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
