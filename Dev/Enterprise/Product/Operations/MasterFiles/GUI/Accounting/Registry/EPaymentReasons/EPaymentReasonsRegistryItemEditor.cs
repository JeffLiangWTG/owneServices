using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class EPaymentReasonsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public EPaymentReasonsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{ }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new EPaymentReasonsControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
