using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	public class CustomsDSBCreditorOverrideRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CustomsDSBCreditorOverrideRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CustomsDSBCreditorOverrideUserControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
