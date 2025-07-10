using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingRoutingCollection : NonPersistentBusinessObjectCollection<SterlingRouting>
	{
		public SterlingRoutingCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCllection();
		}
		readonly SterlingCommerceConsolAndShipmentExporter Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingRouting();
		}

		public void UpdateCllection()
		{
			foreach (Xsd.PlannedLeg routingToAdd in Master.Shipment.ShipmentDetails.TransportPlan)
			{
				if (this.Count < 20)
				{
					SterlingRouting routing = this.AddNew();
					routing.Source = routingToAdd;
					routing.Level = "S";
				}
			}
			if (Master.Consol != null)
			{
				foreach (Xsd.PlannedLeg routingToAdd in Master.Consol.ConsolDetail.PlannedLegs)
				{
					if (this.Count < 20 && Master.Consol != null)
					{
						SterlingRouting routing = this.AddNew();
						routing.Source = routingToAdd;
						routing.Level = "C";
					}
				}
			}
		}
	}
}

