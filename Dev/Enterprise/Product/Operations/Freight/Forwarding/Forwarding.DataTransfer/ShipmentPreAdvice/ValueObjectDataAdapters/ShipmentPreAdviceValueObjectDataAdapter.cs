using System;
using System.Xml.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ShipmentPreAdviceValueObjectDataAdapter : FreightValueObjectDataAdapter<JobShipmentPreplanning, Xsd.Consol>
	{
		#region Constructors

		public ShipmentPreAdviceValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		public ShipmentPreAdviceValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base()
		{
			TriggeredByEvents = triggeredByEvents;
		}

		internal ShipmentPreAdviceValueObjectDataAdapter(Order orderToExportFilter, EventsWithSourceType triggeredByEvents)
			: this(triggeredByEvents)
		{
			this.orderToExportFilter = orderToExportFilter;
		}
		readonly Order orderToExportFilter;
		readonly EventsWithSourceType TriggeredByEvents;

		#endregion

		#region Overrides

		protected override bool RegistryDefaultForImporting
		{
			get { throw new NotSupportedException("RegistryDefaultForImporting - Not implemented in class ShipmentPreAdviceValueObjectDataAdapter"); }
		}

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Consols"; } // Hard-coded constant.
		}

		public override string RootElementName
		{
			get { return (NoResString)"Consol"; } // Hard-coded constant.
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleConsolSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ConsolsSchema; }
		}

		#endregion

		#region ImportFromValueObject

		protected override void ImportFromValueObjectCore(JobShipmentPreplanning bizObj, Xsd.Consol value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region ExportToValueObject

		protected override void ExportToValueObjectCore(JobShipmentPreplanning preAdvice, Xsd.Consol xsdPreAdvice, IValueObjectExportContext context)
		{
			string errorContext = (NoResString)"Shipment Pre Advice " + preAdvice.EF_PreshipID; // Hard-coded error context constant.

			xsdPreAdvice.Events = StmALogValueObjectDataAdapter.New(preAdvice, errorContext, TriggeredByEvents).ToXmlCollectionValueObject(context);
			xsdPreAdvice.Notes = new NoteValueObjectDataAdapter().ExportToXmlValueObjectCollection(preAdvice.Notes, context);

			ExportConsolDetails(preAdvice, xsdPreAdvice, context);
			ExportShipmentDetails(preAdvice, xsdPreAdvice, context, errorContext);

			AddExportEvent(xsdPreAdvice, preAdvice, context);
		}

		void ExportConsolDetails(JobShipmentPreplanning preAdvice, Xsd.Consol xsdPreAdvice, IValueObjectExportContext context)
		{
			Xsd.ConsolIdentifier masterBill = xsdPreAdvice.ConsolIdentifier.AddNew();
			masterBill.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			masterBill.ConsolIdentifierTypeSpecified = true;
			masterBill.Value = preAdvice.EF_MasterBill;

			xsdPreAdvice.ConsolDetail.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(preAdvice.Factory, preAdvice.EF_RL_NKPortLoad, ZDateTime.Empty, ZDateTime.Empty);
			xsdPreAdvice.ConsolDetail.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(preAdvice.Factory, preAdvice.EF_RL_NKPortDisch, ZDateTime.Empty, ZDateTime.Empty);
			xsdPreAdvice.ConsolDetail.Carrier = new OrganisationValueObjectDataAdapter(OrganisationTypes.Carrier).ExportToValueObject(preAdvice.Carrier, context);
			xsdPreAdvice.ConsolDetail.SendingAgent = new OrganisationValueObjectDataAdapter(OrganisationTypes.Forwarder).ExportToValueObject(preAdvice.SendingAgent, context);
			xsdPreAdvice.ConsolDetail.ReceivingAgent = new OrganisationValueObjectDataAdapter(OrganisationTypes.Forwarder).ExportToValueObject(preAdvice.ReceivingAgent, context);

			ExportContainers(preAdvice, xsdPreAdvice, context);
		}

		void ExportShipmentDetails(JobShipmentPreplanning preAdvice, Xsd.Consol xsdPreAdvice, IValueObjectExportContext context, string errorContext)
		{
			if (preAdvice.Shipment != null)
			{
				ForwardingShipmentValueObjectDataAdapter shipmentAdapter = new ForwardingShipmentValueObjectDataAdapter(TriggeredByEvents);
				Xsd.Shipment xsdShipment = shipmentAdapter.ExportToValueObject(preAdvice.Shipment, context);
				xsdPreAdvice.Shipments.Add(xsdShipment);
				xsdShipment.DocData.IsSpecified = false;
			}
			else
			{
				Xsd.Shipment xsdShipment = xsdPreAdvice.Shipments.AddNew();
				ExportShipmentIdentifiers(preAdvice, xsdShipment);

				xsdShipment.ShipmentDetailsSpecified = true;
				xsdShipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
				xsdShipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(preAdvice.EF_ActualWeight, preAdvice.EF_UnitOfWeight);
				xsdShipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(preAdvice.EF_ActualVolume, preAdvice.EF_UnitOfVolume);
				xsdShipment.ShipmentDetails.TotalOuterPacksQty = Xsd.DimensionValue.FromAmountAndUnit(preAdvice.EF_Packs, preAdvice.EF_F3_NKPackType);
				xsdShipment.ShipmentDetails.ChargeableWeight.IsSpecified = true;
				xsdShipment.ShipmentDetails.TotalInnerPacksQty.IsSpecified = true;
				xsdShipment.ShipmentDetails.Consignee = new OrganisationValueObjectDataAdapter(OrganisationTypes.Consignee).ExportToValueObject(preAdvice.Buyer, context);

				ExportTransportLegs(preAdvice, xsdPreAdvice, context, errorContext);
				if (orderToExportFilter == null)
				{
					ExportOrders(preAdvice.Orders, xsdShipment, context);
				}
				else
				{
					ExportOrder(orderToExportFilter, xsdShipment, context);
				}
				xsdShipment.DocData.IsSpecified = false;
			}
		}

		void ExportShipmentIdentifiers(JobShipmentPreplanning preAdvice, Xsd.Shipment xsdShipment)
		{
			Xsd.ShipmentIdentifier xsdHouseBill = xsdShipment.ShipmentIdentifier.AddNew();
			xsdHouseBill.Value = preAdvice.EF_HouseBill;
			xsdHouseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			Xsd.ShipmentIdentifier xsdPreadviceIdentifier = xsdShipment.ShipmentIdentifier.AddNew();
			xsdPreadviceIdentifier.Value = preAdvice.EF_PreshipID;
			xsdPreadviceIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.PreadviceIdentifier;
		}

		void ExportTransportLegs(JobShipmentPreplanning preAdvice, Xsd.Consol xsdConsol, IValueObjectExportContext context, string errorContext)
		{
			foreach (Transport transport in preAdvice.PreAdviceTransports)
			{
				Xsd.PlannedLeg xsdPlannedLeg = xsdConsol.ConsolDetail.PlannedLegs.AddNew();
				XsdPlannedLegObjectHelper.ExportPlannedLeg(xsdPlannedLeg, transport, context, errorContext);
			}
		}

		void ExportOrders(OrderCollection orders, Xsd.Shipment xsdShipment, IValueObjectExportContext context)
		{
			if (orders.Count > 0)
			{
				OrderValueObjectDataAdapter orderDataAdapter = new OrderValueObjectDataAdapter(TriggeredByEvents);
				foreach (Order order in orders)
				{
					Xsd.Order xsdOrder = orderDataAdapter.ExportToValueObject(order, context);
					xsdShipment.Orders.Add(xsdOrder);
				}
			}
		}

		void ExportOrder(Order order, Xsd.Shipment xsdShipment, IValueObjectExportContext context)
		{
			OrderValueObjectDataAdapter orderDataAdapter = new OrderValueObjectDataAdapter(TriggeredByEvents);
			Xsd.Order xsdOrder = orderDataAdapter.ExportToValueObject(order, context);
			xsdShipment.Orders.Add(xsdOrder);
		}

		void ExportContainers(JobShipmentPreplanning preAdvice, Xsd.Consol xsdPreAdvice, IValueObjectExportContext context)
		{
			foreach (OrderContainer container in preAdvice.Containers)
			{
				Xsd.Container xsdContainer = xsdPreAdvice.ConsolDetail.Containers.AddNew();
				xsdContainer.ContainerNumber = container.J1_ContainerCount != 1 ? (container.J1_ContainerCount + " CONTAINERS") : (string)container.J1_ContainerNumber;
				ContainerTypeValueObjectDataAdapter containerTypeAdapter = new ContainerTypeValueObjectDataAdapter();
				xsdContainer.ContainerType = containerTypeAdapter.ExportToValueObject(container.Container, context);
			}
		}

		#endregion
	}
}
