using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class DefaultEPaymentReferenceRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DefaultEPaymentReferenceRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new DefaultEPaymentReferenceControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
