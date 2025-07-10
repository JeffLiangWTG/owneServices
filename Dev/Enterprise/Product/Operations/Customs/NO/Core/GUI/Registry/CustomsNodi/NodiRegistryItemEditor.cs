using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry.GUI
{
	public class NodiRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NodiRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new NodiRegistryItemControl();
	}
}
