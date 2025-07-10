using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentValueObjectDataAdapter : ShipmentValueObjectDataAdapter<ForwardingShipment>
	{
		#region Ctor

		public ForwardingShipmentValueObjectDataAdapter()
		{
		}

		public ForwardingShipmentValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		public ForwardingShipmentValueObjectDataAdapter(ForwardingConsol existingConsol)
			: this(existingConsol, EventsWithSourceType.Empty)
		{
		}

		public ForwardingShipmentValueObjectDataAdapter(ForwardingConsol existingConsol, EventsWithSourceType triggeredByEvents)
			: base(existingConsol, triggeredByEvents)
		{
		}

		public ForwardingShipmentValueObjectDataAdapter(Order orderToExportFilter)
			: this(orderToExportFilter, EventsWithSourceType.Empty)
		{
		}

		public ForwardingShipmentValueObjectDataAdapter(Order orderToExportFilter, EventsWithSourceType triggeredByEvents)
			: this(triggeredByEvents)
		{
			this.orderToExportFilter = orderToExportFilter;
		}

		public ForwardingShipmentValueObjectDataAdapter(ForwardingConsol existingConsol, Order orderToExportFilter)
			: this(existingConsol, orderToExportFilter, EventsWithSourceType.Empty)
		{
		}

		public ForwardingShipmentValueObjectDataAdapter(ForwardingConsol existingConsol, Order orderToExportFilter, EventsWithSourceType triggeredByEvents)
			: this(existingConsol, triggeredByEvents)
		{
			this.orderToExportFilter = orderToExportFilter;
		}

		#endregion

		#region Properties

		readonly Order orderToExportFilter;

		#endregion

		#region Import

		protected override void AddImportEvent(ForwardingShipment shipment, Xsd.Shipment shipmentValue)
		{
			if (shipmentValue.ShipmentDetails.DeclarationStyle == "SAC")
			{
				AddImportEvent(shipment, (NoResString)"AU Declaration Style: SAC"); // Event reference should not be localized?
			}
			else
			{
				base.AddImportEvent(shipment, shipmentValue);
			}
		}

		protected override void OnAfterImportFromValueObjectCore(ForwardingShipment shipment, Xsd.Shipment value, IValueObjectImportContext context)
		{
			if (value.ShipmentDetails != null)
			{
				GetShipmentOrdersDataAdapterHelper().ImportOrdersAndOrderReferences(new ShipmentOrdersDataAdapterHelper.ImportArgs()
				{
					Shipment = shipment,
					ShipmentValue = value,
					Context = context
				});
			}
			base.OnAfterImportFromValueObjectCore(shipment, value, context);
		}

		protected virtual ShipmentOrdersDataAdapterHelper GetShipmentOrdersDataAdapterHelper(IValueObjectDataAdapter orderValueObjectDataAdapter = null)
		{
			return new ShipmentOrdersDataAdapterHelper(TriggeredByEvents, orderValueObjectDataAdapter);
		}

		protected override void ImportOuterPackagesCore(PackLine packline, Xsd.Package package, IValueObjectImportContext context)
		{
			new ForwardingPackLineValueObjectDataAdapter<ForwardingPackLine, Xsd.Package>().ImportFromValueObject(packline as ForwardingPackLine, package, context);
		}

		#endregion

		#region Export

		protected override void OnAfterExportFromValueObjectCore(ForwardingShipment shipment, Xsd.Shipment value, IValueObjectExportContext context)
		{
			ExportPreAdviceIdentifier(shipment, value);

			var ordersDataAdapterHelper = GetShipmentOrdersDataAdapterHelper(GetOrderValueObjectDataAdapter(TriggeredByEvents));
			ordersDataAdapterHelper.ExportOrdersAndOrderReferences(new ShipmentOrdersDataAdapterHelper.ExportArgs()
			{
				Shipment = shipment,
				ShipmentValue = value,
				Context = context,
				OrderFilter = orderToExportFilter
			});

			ExportARInvoices(shipment, value, context);
			ExportAWBHeader(shipment, value, context);

			base.OnAfterExportFromValueObjectCore(shipment, value, context);
		}

		void ExportAWBHeader(ForwardingShipment shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (IsSendXMLWithAWB && shipment.IsAir)
			{
				ExportAWBHeaderValueObjectDataAdapter aWBHeaderExporter = new ExportAWBHeaderValueObjectDataAdapter(shipment);
				shipment.PopulateAWB();
				shipmentValue.AWBHeaders.Add(aWBHeaderExporter.ExportToValueObject(shipment.AWBHeader, context));
			}
		}

		bool IsSendXMLWithAWB
		{
			get
			{
				return
				this.TriggeredByEvents != null &&
				this.TriggeredByEvents.TriggerAction != null &&
				WorkflowTriggerActionTypeConstants.IsSendXMLWithAWB(this.TriggeredByEvents.TriggerAction.PQ_TriggerType);
			}
		}

		protected virtual void ExportARInvoices(ForwardingShipment shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext context)
		{
			if (SystemRegistry.IncludeConsolOrShipmentARInvoices.Value)
			{
				ForwardingJobInvoicesExporter invoiceExporter = new ForwardingJobInvoicesExporter(shipment.Factory);
				shipmentValue.ARInvoices = invoiceExporter.PopulateInvoicesToXSD(shipment.JS_UniqueConsignRef, context, false);
			}
		}

		void ExportPreAdviceIdentifier(ForwardingShipment shipment, Xsd.Shipment shipmentValue)
		{
			if (shipment.PreAdvice != null)
			{
				Xsd.ShipmentIdentifier identifier = shipmentValue.ShipmentIdentifier.AddNew();
				identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.PreadviceIdentifier;
				identifier.Value = shipment.PreAdvice.EF_PreshipID;
			}
		}

		protected virtual IValueObjectDataAdapter GetOrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			return new OrderValueObjectDataAdapter(triggeredByEvents);
		}

		protected override Xsd.Package ExportOuterPackagesCore(PackLine packline, IValueObjectExportContext context)
		{
			return new ForwardingPackLineValueObjectDataAdapter<ForwardingPackLine, Xsd.Package>().ExportToValueObject(packline as ForwardingPackLine, context);
		}

		#endregion
	}
}

