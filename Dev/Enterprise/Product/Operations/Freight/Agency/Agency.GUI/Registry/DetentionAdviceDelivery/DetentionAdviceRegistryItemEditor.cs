using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.GUI
{
	public class DetentionAdviceDeliveryRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DetentionAdviceDeliveryRegistryItemEditor(DetentionAdviceDeliveryRegistryDataType type, FallbackLevel fallback, BusinessObjectFactory factory)
			: base(type, fallback, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new DetentionAdviceDeliveryControl();
		}
	}
}


