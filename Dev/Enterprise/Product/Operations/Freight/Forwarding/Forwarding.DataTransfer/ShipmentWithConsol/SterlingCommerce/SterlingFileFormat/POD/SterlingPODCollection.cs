using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingPODCollection : NonPersistentBusinessObjectCollection<SterlingPOD>
	{
		public SterlingPODCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCollection();
		}
		readonly SterlingCommerceConsolAndShipmentExporter Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingPOD();
		}

		public void UpdateCollection()
		{
			foreach (Xsd.ContainerLeg legToAdd in Master.Shipment.ShipmentDetails.Deliver.DeliveryLegs)
			{
				SterlingPOD pOD = this.AddNew();
				pOD.Source = legToAdd;
			}
		}
	}
}

