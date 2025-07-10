using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	public class JobSupplierBookingConverter
	{
		public void ConvertToShipmentForBooking(JobSupplierBooking supplierBooking, ForwardingShipment shipment)
		{
			using (new DisposableAction(() => shipment.SuppressPackLinesUpdate = true, () => shipment.SuppressPackLinesUpdate = false))
			{
				shipment.ForceHouseBillGeneration = true;
				var supplierBookingLines = supplierBooking.SupplierBookingLines;
				var order = supplierBookingLines.FirstOrDefault()?.OrderLine.Order;
				FillCreatedShipmentFromSupplierBooking(shipment, supplierBooking, order, null);

				shipment.BookingPartyDocumentaryAddress.OrganisationPK = supplierBooking.JSB_OH_BookingParty;
				shipment.UpdateShipmentMeasuresFromBookingLines(supplierBookingLines);
				shipment.JS_RX_NKGoodsValueCurr = order?.JD_RX_NKOrderCurrency ?? shipment.JS_RX_NKGoodsValueCurr;

				shipment.UpdateShipmentFromSupplierBooking(supplierBooking);
			}
		}

		public ForwardingShipment ConvertToShipmentForLoadList(JobSupplierBooking booking, Order order, ForwardingConsol consol)
		{
			Argument.NotNull(booking, nameof(booking));
			Argument.NotNull(consol, nameof(consol));

			var shipment = (consol?.Shipments?.AddNew()) ?? throw new InvalidOperationException(nameof(booking));

			using (new DisposableAction(() => shipment.SuppressPackLinesUpdate = true, () => shipment.SuppressPackLinesUpdate = false))
			{
				shipment.ForceHouseBillGeneration = true;
				FillCreatedShipmentFromSupplierBooking(shipment, booking, order, consol);
				return shipment;
			}
		}

		void FillCreatedShipmentFromSupplierBooking(ForwardingShipment shipment, JobSupplierBooking booking, Order order, ForwardingConsol consol)
		{
			Argument.NotNull(shipment, nameof(shipment));
			Argument.NotNull(booking, nameof(booking));

			if (booking.JSB_LoadMode == Core.Constants.SupplierBookingLoadMode.ContainerFreightStation)
			{
				FillCreatedShipmentFromCFSSupplierBooking(shipment, booking, order, consol);
			}
			else
			{
				FillCreatedShipmentFromCYOrLSESupplierBooking(shipment, booking, order, consol);
			}
		}

		void FillCreatedShipmentFromCYOrLSESupplierBooking(ForwardingShipment shipment, JobSupplierBooking booking, Order order, ForwardingConsol relatedConsol)
		{
			var isSuppressPackLinesUpdate = shipment.SuppressPackLinesUpdate;
			shipment.JS_TransportMode = booking.JSB_TransportMode;
			shipment.JS_PackingMode = booking.GetConvertedPackingMode();

			UpdateShipmentCommonProperties(shipment, booking, order);

			if (order != null)
			{
				if (order.GoodsAvailableAtAddress.Address != null)
				{
					shipment.ConsignorPickupAddress.E2_OA_Address = order.GoodsAvailableAtAddress.Address.PK;
				}

				if (order.GoodsDeliveredToAddress.Address != null)
				{
					shipment.ConsigneeDeliveryAddress.E2_OA_Address = order.GoodsDeliveredToAddress.Address.PK;
				}

				if (order.NotifyPartyDocAddress != null && !order.NotifyPartyDocAddress.IsEmpty)
				{
					shipment.NotifyPartyDocumentaryAddress.CopyDocAddressFrom(order.NotifyPartyDocAddress);
				}

				if (order.NotifyParty2DocAddress != null && !order.NotifyParty2DocAddress.IsEmpty)
				{
					shipment.NotifyParty2DocumentaryAddress.CopyDocAddressFrom(order.NotifyParty2DocAddress);
				}

				if (order.NotifyParty3DocAddress != null && !order.NotifyParty3DocAddress.IsEmpty)
				{
					shipment.NotifyParty3DocumentaryAddress.CopyDocAddressFrom(order.NotifyParty3DocAddress);
				}

				if (booking.ControllingCustomerAddress != null && !booking.ControllingCustomerAddress.IsEmpty)
				{
					shipment.ControllingCustomerAddress.CopyDocAddressFrom(booking.ControllingCustomerAddress);
				}

				if (order.ControllingAgentDocAddress != null && !order.ControllingAgentDocAddress.IsEmpty)
				{
					shipment.ControllingAgentDocumentaryAddress.CopyDocAddressFrom(order.ControllingAgentDocAddress);
				}
			}

			if (relatedConsol != null)
			{
				shipment.JS_E_DEP = relatedConsol.MostInterestingTransportForBinding.Cast<ITransport>().FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty;
				shipment.JS_E_ARV = relatedConsol.MostInterestingTransportForBinding.Cast<ITransport>().FirstOrDefault()?.JW_ETA ?? ZDateTime.Empty;
			}
		}

		void FillCreatedShipmentFromCFSSupplierBooking(ForwardingShipment shipment, JobSupplierBooking booking, Order order, ForwardingConsol relatedConsol)
		{
			shipment.JS_TransportMode = relatedConsol.JK_TransportMode;
			shipment.JS_PackingMode = booking.GetConvertedPackingMode();

			shipment.JS_E_DEP = relatedConsol.MostInterestingTransportForBinding?.Cast<ITransport>().FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty;
			shipment.JS_E_ARV = relatedConsol.MostInterestingTransportForBinding?.Cast<ITransport>().FirstOrDefault()?.JW_ETA ?? ZDateTime.Empty;

			if (!booking.ConsigneeDocumentaryAddress.E2_AddressOverride && !booking.ConsigneeDocumentaryAddress.IsEmpty)
			{
				shipment.ConsigneeDeliveryAddress.OrganisationPK = booking.ConsigneeDocumentaryAddress.OrganisationPK;
				shipment.ConsigneeDeliveryAddress.ContactPK = booking.ConsigneeDocumentaryAddress.ContactPK;
				shipment.ConsigneeDeliveryAddress.E2_OA_Address = booking.ConsigneeDocumentaryAddress.E2_OA_Address;
			}
			else if (order != null)
			{
				shipment.ConsigneeDeliveryAddress.OrganisationPK = order.Buyer.PK;
				shipment.ConsigneeDeliveryAddress.ContactPK = order.JD_OC_BuyerContact;
				shipment.ConsigneeDeliveryAddress.E2_OA_Address = order.JD_OA_BuyerAddress;
			}

			shipment.ControllingCustomerAddress.CopyDocAddressFrom(booking.ControllingCustomerAddress);
			shipment.JS_OA_ExportReceivingDepot = booking.CFSAddress?.PK ?? Guid.Empty;

			UpdateShipmentCommonProperties(shipment, booking, order);
		}

		protected static void UpdateShipmentCommonProperties(ForwardingShipment shipment, JobSupplierBooking booking, Order order)
		{
			shipment.ConsignorDocumentaryAddress.OrganisationPK = booking.SupplierAddress.OrganisationPK;
			shipment.ConsignorDocumentaryAddress.ContactPK = booking.SupplierAddress.ContactPK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = booking.SupplierAddress.E2_OA_Address;
			if (!booking.ConsigneeDocumentaryAddress.E2_AddressOverride && booking.ConsigneeDocumentaryAddress.Address != null)
			{
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = booking.ConsigneeDocumentaryAddress.OrganisationPK;
				shipment.ConsigneeDocumentaryAddress.ContactPK = booking.ConsigneeDocumentaryAddress.ContactPK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = booking.ConsigneeDocumentaryAddress.E2_OA_Address;
			}
			else if (order != null)
			{
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = order.Buyer.PK;
				shipment.ConsigneeDocumentaryAddress.ContactPK = order.JD_OC_BuyerContact;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = order.JD_OA_BuyerAddress;
			}
			if (order?.BuyerAddress != null && order.BuyerAddress.PK != (shipment.ConsigneeDocumentaryAddress?.E2_OA_Address ?? ZGuid.Empty))
			{
				shipment.BuyerDocAddress.OrganisationPK = order.BuyerAddress.OA_OH;
				shipment.BuyerDocAddress.ContactPK = order.JD_OC_BuyerContact;
				shipment.BuyerDocAddress.E2_OA_Address = order.JD_OA_BuyerAddress;
			}
			shipment.JS_INCO = booking.JSB_IncoTerm;
			shipment.JS_RL_NKOrigin = booking.JSB_RL_NKOrigin;
			shipment.JS_RL_NKDestination = booking.JSB_RL_NKDestination;
		}
	}
}
