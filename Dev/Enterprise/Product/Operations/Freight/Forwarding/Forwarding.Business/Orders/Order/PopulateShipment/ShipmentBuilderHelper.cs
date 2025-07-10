using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public static class ShipmentBuilderHelper
	{
		public static ForwardingShipment PopulateShipmentFromOrder(ZGuid orderPK, ForwardingShipment shipment, Action<object, OrderLineToPackLineConversionEventArgs> orderLineToPackLineConversion = null)
		{
			var loadedOrder = shipment.Factory.Load<Order>(orderPK);

			if (loadedOrder != null)
			{
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = loadedOrder.JD_OA_SupplierAddress;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = loadedOrder.JD_OA_BuyerAddress;

				if (loadedOrder.GoodsAvailableAtAddress != null && !loadedOrder.GoodsAvailableAtAddress.IsEmpty)
				{
					shipment.ConsignorPickupAddress.CopyPersistentValuesFrom(loadedOrder.GoodsAvailableAtAddress);
					shipment.ConsignorPickupAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorPickupDeliveryAddress;
				}

				if (loadedOrder.GoodsDeliveredToAddress != null && !loadedOrder.GoodsDeliveredToAddress.IsEmpty)
				{
					shipment.ConsigneeDeliveryAddress.CopyPersistentValuesFrom(loadedOrder.GoodsDeliveredToAddress);
					shipment.ConsigneeDeliveryAddress.E2_AddressType = DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
				}

				if (loadedOrder.NotifyPartyDocAddress != null && !loadedOrder.NotifyPartyDocAddress.IsEmpty)
				{
					shipment.NotifyPartyDocumentaryAddress.CopyPersistentValuesFrom(loadedOrder.NotifyPartyDocAddress);
				}

				if (loadedOrder.NotifyParty2DocAddress != null && !loadedOrder.NotifyParty2DocAddress.IsEmpty)
				{
					shipment.NotifyParty2DocumentaryAddress.CopyPersistentValuesFrom(loadedOrder.NotifyParty2DocAddress);
				}

				if (loadedOrder.NotifyParty3DocAddress != null && !loadedOrder.NotifyParty3DocAddress.IsEmpty)
				{
					shipment.NotifyParty3DocumentaryAddress.CopyPersistentValuesFrom(loadedOrder.NotifyParty3DocAddress);
				}

				shipment.ControllingCustomerAddress.CopyPersistentValuesFrom(loadedOrder.ControllingCustomerDocAddress);
				shipment.ControllingAgentDocumentaryAddress.CopyPersistentValuesFrom(loadedOrder.ControllingAgentDocAddress);

				shipment.JS_INCO = loadedOrder.JD_IncoTerm;
				shipment.JS_TransportMode = loadedOrder.JD_TransportMode;
				shipment.JS_PackingMode = loadedOrder.JD_ContainerMode;

				using (shipment.SuspendSettingConsignorFromOrigin())
				using (shipment.SuspendSettingConsigneeFromDestination())
				{
					shipment.JS_RL_NKOrigin = loadedOrder.JD_RL_NKPortOfLoading;
					shipment.JS_RL_NKDestination = loadedOrder.JD_RL_NKPortOfDischarge;
				}

				shipment.JS_HouseBill = loadedOrder.JD_Waybill.Left(20);
				shipment.JS_RS_NKServiceLevel = loadedOrder.JD_RS_NKServiceLevel_NI;

				shipment.DocsAndCartage.JP_PickupRequiredBy = loadedOrder.JD_ExWorksRequiredBy;
				shipment.DocsAndCartage.JP_DeliveryRequiredBy = loadedOrder.JD_DeliveryRequiredBy;

				MapPackLinesToOrderLines(shipment, loadedOrder, orderLineToPackLineConversion);

				if (shipment.UpdatePackLines && shipment.OuterPackLines.Count > 0)
				{
					using (new DisposableAction(() => shipment.SuppressPackLinesUpdate = true, () => shipment.SuppressPackLinesUpdate = false))
					{
						UpdateShipmentFromOrderPlan(shipment, loadedOrder);
					}
				}
				else
				{
					UpdateShipmentFromOrderPlan(shipment, loadedOrder);
				}

				loadedOrder.JD_JS = shipment.PK;
			}

			return shipment;
		}

		static void UpdateShipmentFromOrderPlan(ForwardingShipment shipment, Order loadedOrder)
		{
			shipment.JS_F3_NKPackType = loadedOrder.JD_F3_NKPackType;
			shipment.JS_OuterPacks = loadedOrder.JD_Packs;
			shipment.JS_ActualVolume = loadedOrder.JD_ActualVolume;
			shipment.JS_UnitOfVolume = loadedOrder.JD_UnitOfVolume;
			shipment.JS_ActualWeight = loadedOrder.JD_ActualWeight;
			shipment.JS_UnitOfWeight = loadedOrder.JD_UnitOfWeight;
			shipment.JS_GoodsDescription = loadedOrder.JD_OrderGoodsDescription;
		}

		static void MapPackLinesToOrderLines(ForwardingShipment shipment, Order order, Action<object, OrderLineToPackLineConversionEventArgs> orderLineToPackLineConversion = null)
		{
			var helper = new OrderLineToPackLineConversionHelper(shipment.Factory, shipment, new Order[] { order });
			var args = new OrderLineToPackLineConversionEventArgs(helper, true);

			if (helper.OrderLines.Count > 0)
			{
				orderLineToPackLineConversion?.Invoke(null, args);
			}

			if (args.ShouldCreatePacklines)
			{
				helper.CreatePackLines();
			}
		}
	}
}
