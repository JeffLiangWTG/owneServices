using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ModuleShipmentCollectionForTest : ModuleShipmentCollection
	{
		public ModuleShipmentCollectionForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void CallAddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
		}
	}
}
