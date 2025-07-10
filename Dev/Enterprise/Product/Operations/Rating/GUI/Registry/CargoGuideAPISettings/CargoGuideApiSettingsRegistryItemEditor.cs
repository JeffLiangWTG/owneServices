using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI
{
	class CargoGuideApiSettingsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CargoGuideApiSettingsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CargoGuideApiSettingsControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}

