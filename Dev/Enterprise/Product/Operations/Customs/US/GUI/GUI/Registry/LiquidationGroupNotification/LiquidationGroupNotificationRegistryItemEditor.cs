using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public class LiquidationGroupNotificationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public LiquidationGroupNotificationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected sealed override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new LiquidationGroupNotificationControl();
		}

		protected sealed override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
