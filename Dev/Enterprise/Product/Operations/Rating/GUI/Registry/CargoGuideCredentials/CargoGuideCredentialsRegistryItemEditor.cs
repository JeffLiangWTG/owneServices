using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI
{
	class CargoGuideCredentialsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CargoGuideCredentialsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CargoGuideCredentialsControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}

