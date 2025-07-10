using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.GUI
{
	public class FTPSettingsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public FTPSettingsRegistryItemEditor(Integration.IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new FTPSettingsRegistryItemUserControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
