using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	/// <summary>
	/// Use to actually export a shipment wrapped in a consol that has been prior determined
	/// </summary>
	public class ForwardingConsolWithShipmentValueObjectDataAdapter : ForwardingConsolValueObjectDataAdapter
	{
		public ForwardingConsolWithShipmentValueObjectDataAdapter(ForwardingShipment shipment)
			: this(shipment, EventsWithSourceType.Empty)
		{
		}

		public ForwardingConsolWithShipmentValueObjectDataAdapter(ForwardingShipment shipment, EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
			this.ShipmentToExport = shipment;
		}

		protected override void ExportShipments(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (ShipmentToExport != null && consol.Shipments.Contains(ShipmentToExport.PK))
			{
				consolValue.Shipments = new Xsd.ShipmentCollection();
				consolValue.Shipments.Add(GetNewShipmentValueObjectDataAdapter(null).ExportToValueObject(ShipmentToExport, context));
			}
		}

		protected override void ExportContainers(ForwardingConsol consol, Xsd.Consol consolValue, IValueObjectExportContext context)
		{
			if (ShipmentToExport != null && consol.Shipments.Contains(ShipmentToExport.PK))
			{
				var adapter = new ConsolContainerValueObjectDataAdapter<CommonContainer, Xsd.Container>(consol);
				consolValue.ConsolDetail.Containers = new Xsd.ContainerCollection();

				foreach (ForwardingContainer container in consol.Containers)
				{
					if (ShipmentToExport.OuterPackLines.OfType<ForwardingPackLine>().SelectMany(pack => pack.Containers).Contains(container))
					{
						consolValue.ConsolDetail.Containers.Add(adapter.ExportToValueObject(container, context));
					}
				}
			}
		}

		/// <summary>
		/// DEX Event is added to the shipment. It must not be added to the consol. 
		/// </summary>
		/// <param name="valueObject"></param>
		/// <param name="consol"></param>
		/// <param name="reference"></param>
		protected override void AddExportEvent(Xsd.Consol valueObject, ForwardingConsol consol, IValueObjectExportContext context, ZString reference)
		{
		}

		readonly ForwardingShipment ShipmentToExport;
	}
}

#region Implementation
#endregion
