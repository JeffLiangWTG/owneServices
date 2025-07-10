using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingOrderReferenceCollection : NonPersistentBusinessObjectCollection<SterlingOrderReference>
	{
		public SterlingOrderReferenceCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCollection();
		}
		readonly SterlingCommerceConsolAndShipmentExporter Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingOrderReference();
		}

		public void UpdateCollection()
		{
			if (Master.Shipment.ShipmentDetails.OrderReferences != null)
			{
				foreach (ZString refToAdd in Master.Shipment.ShipmentDetails.OrderReferences)
				{
					SterlingOrderReference @ref = this.AddNew();
					@ref.UpdateFields(refToAdd);
				}
			}
		}
	}
}

