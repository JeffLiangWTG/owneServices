using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingEventCollection : NonPersistentBusinessObjectCollection<SterlingEvent>
	{
		public SterlingEventCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingEvent();
		}

		public void UpdateCollection()
		{
			foreach (Xsd.Event eventToAdd in Master.Shipment.Events.Event)
			{
				if (Master.Interchange.Source.Purpose != "EVT" || (Master.Interchange.Source.Purpose == "EVT" && eventToAdd.TriggeredBy))
				{
					SterlingEvent @event = this.AddNew();
					@event.SourceEvent = eventToAdd;
				}
			}
		}

		readonly SterlingCommerceConsolAndShipmentExporter Master;
	}
}

