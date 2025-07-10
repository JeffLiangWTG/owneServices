using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI.Registry
{
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AutoratingIntercompanyTariffsForGatewayJobConfigurationItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfigurationControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}


