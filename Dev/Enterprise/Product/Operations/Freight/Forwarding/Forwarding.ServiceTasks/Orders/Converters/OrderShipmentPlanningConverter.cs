using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	class OrderShipmentPlanningConverter
	{
		public ForwardingShipment ConvertToShipment(OrderShipmentPlanning shipmentPlanning)
		{
			var shipment = shipmentPlanning.Shipment ?? FillShipment(shipmentPlanning, shipmentPlanning.Factory.New<ForwardingShipment>());
			shipment.ForceHouseBillGeneration = true;

			foreach (var planningLine in shipmentPlanning.OrderShipmentPlanningLines)
			{
				new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);
			}

			shipment.UpdateShipmentFromOuterPackLines();
			shipment.JS_GoodsValue = shipment.OuterPackLines.OfType<ForwardingPackLine>().Sum(packLine => packLine.JL_LinePrice);

			return shipment;
		}

		ForwardingShipment FillShipment(OrderShipmentPlanning shipmentPlanning, ForwardingShipment shipment)
		{
			using (new DisposableAction(() => shipment.SuppressPackLinesUpdate = true, () => shipment.SuppressPackLinesUpdate = false))
			{
				var supplierBooking = shipmentPlanning.SupplierBooking;
				shipmentPlanning.OPS_JS_Shipment = shipment.PK;
				shipment.JS_TransportMode = shipmentPlanning.OPS_TransportMode;
				shipment.JS_PackingMode = shipmentPlanning.OPS_ContainerMode;
				shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

				// Make sure transport mode, packing mode, consignor and consignee are assigned before other properties.
				// Their change would trigger the match process of buyer supplier link mode which would assign matched properties to related properties of shipment. Especially the change of consignor and consignee, it would reset origin port and destination port directly according to their location.
				PopulateShipmentAddresses(shipmentPlanning, shipment, supplierBooking);

				shipment.JS_INCO = supplierBooking.JSB_IncoTerm;
				shipment.JS_GoodsDescription = shipmentPlanning.OPS_GoodsDescription;
				shipment.JS_E_DEP = shipmentPlanning.OPS_ETD.ToZDateTime();
				shipment.JS_E_ARV = shipmentPlanning.OPS_ETA.ToZDateTime();
				shipment.AddSPTRefNumber(shipmentPlanning.OPS_ShipmentName);
				shipment.DetailedGoodsDescriptionNoteText = supplierBooking.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).FirstOrDefault()?.ST_NoteText ?? ZString.Empty;
				shipment.SetMarksAndNumbers(supplierBooking.JSB_MarksAndNumbers);
				shipment.JS_RL_NKOrigin = shipmentPlanning.OPS_RL_NKOrigin;
				shipment.JS_RL_NKDestination = shipmentPlanning.OPS_RL_NKDestination;

				return shipment;
			}
		}

		void PopulateShipmentAddresses(OrderShipmentPlanning shipmentPlanning, ForwardingShipment shipment, JobSupplierBooking booking)
		{
			PopulateShipmentConsignee(shipmentPlanning, shipment);
			CopyDocAddressIfNotEmpty(booking.SupplierAddress, shipment.ConsignorDocumentaryAddress);
			CopyDocAddressIfNotEmpty(booking.ControllingCustomerAddress, shipment.ControllingCustomerAddress);
			CopyDocAddressIfNotEmpty(booking.NotifyPartyDocAddress, shipment.NotifyPartyDocumentaryAddress);
			CopyDocAddressIfNotEmpty(booking.NotifyParty2DocAddress, shipment.NotifyParty2DocumentaryAddress);
			CopyDocAddressIfNotEmpty(booking.NotifyParty3DocAddress, shipment.NotifyParty3DocumentaryAddress);

			PopulateShipmentAddress(shipmentPlanning, DocAddressType.Manufacturer, shipment.ManufacturerDocAddress);
			PopulateShipmentAddress(shipmentPlanning, DocAddressType.GoodsAvailableAt, shipment.ConsignorPickupAddress);
			PopulateShipmentAddress(shipmentPlanning, DocAddressType.GoodsDeliveredTo, shipment.ConsigneeDeliveryAddress);
		}

		void CopyDocAddressIfNotEmpty(JobDocAddress from, JobDocAddress to)
		{
			if (from != null && !from.IsEmpty && to != null)
			{
				to.CopyDocAddressFrom(from);
			}
		}

		void PopulateShipmentConsignee(OrderShipmentPlanning shipmentPlanning, ForwardingShipment shipment)
		{
			var booking = shipmentPlanning.SupplierBooking;
			if (!booking.ConsigneeDocumentaryAddress.E2_AddressOverride && booking.ConsigneeDocumentaryAddress.Address != null)
			{
				CopyDocAddressIfNotEmpty(booking.ConsigneeDocumentaryAddress, shipment.ConsigneeDocumentaryAddress);
			}
			else
			{
				PopulateShipmentAddress(shipmentPlanning, DocAddressType.ConsigneeDocumentaryAddress, shipment.ConsigneeDocumentaryAddress);
			}

			if (shipment.ConsigneeDocumentaryAddress.E2_OA_Address.IsEmpty && shipmentPlanning.OrderShipmentPlanningLines.Any())
			{
				if (AreAddressesExistAndConsistent(shipmentPlanning.OrderShipmentPlanningLines.Select(planningLine => planningLine.SupplierBookingLine.OrderLine.Order.BuyerAddress)))
				{
					var order = shipmentPlanning.OrderShipmentPlanningLines.First().SupplierBookingLine.OrderLine.Order;
					shipment.ConsigneeDocumentaryAddress.OrganisationPK = order.BuyerAddress.OA_OH;
					shipment.ConsigneeDocumentaryAddress.E2_OA_Address = order.BuyerAddress.PK;
					shipment.ConsigneeDocumentaryAddress.ContactPK = order.JD_OC_BuyerContact;
				}
			}
		}

		static void PopulateShipmentAddress(OrderShipmentPlanning shipmentPlanning, DocAddressType addressType, JobDocAddress toDocAddress)
		{
			if (AreAddressesExistAndConsistent(shipmentPlanning.OrderShipmentPlanningLines.Select(planningLine => planningLine.SupplierBookingLine.OrderLine.GetValidAddressWithFallbackToOrder(addressType))))
			{
				toDocAddress.CopyDocAddressFrom(shipmentPlanning.OrderShipmentPlanningLines[0].SupplierBookingLine.OrderLine.GetValidAddressWithFallbackToOrder(addressType));
			}
		}

		static bool AreAddressesExistAndConsistent(IEnumerable<JobDocAddress> docAddresses)
		{
			return docAddresses.Where(docAddress => docAddress != null && !docAddress.E2_AddressOverride && !docAddress.E2_OA_Address.IsEmpty).Select(docAddress => docAddress.E2_OA_Address).Distinct().Count() == 1;
		}

		static bool AreAddressesExistAndConsistent(IEnumerable<OrgAddress> orgAddresses)
		{
			return orgAddresses.Where(orgAddress => orgAddress != null).Select(orgAddress => orgAddress.PK).Distinct().Count() == 1;
		}
	}
}
