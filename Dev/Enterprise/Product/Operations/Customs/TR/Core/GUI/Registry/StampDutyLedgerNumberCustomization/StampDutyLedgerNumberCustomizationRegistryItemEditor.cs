using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.GUI
{
	public class StampDutyLedgerNumberCustomizationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public StampDutyLedgerNumberCustomizationRegistryItemEditor(Integration.IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new StampDutyLedgerNumberCustomizationRegistryItemUserControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;
	}
}
