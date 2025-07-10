using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public static class Extensions
	{
		public static Order With(this Order order,
			string jD_OA_NKDeliverAddress_NI = null,
			string jD_OA_NKPickupAddress_NI = null,
			string jD_RL_NKGoodsDeliveredTo = null,
			string jD_RL_NKGoodsAvailableAt = null,
			string jD_RS_NKServiceLevel_NI = null,
			string jD_RX_NKOrderCurrency = null,
			string jD_ContainerMode = null,
			string jD_F3_NKPackType = null,
			string jD_UnitOfWeight = null,
			string jD_UnitOfVolume = null,
			OrgHeader receivingAgent = null,
			OrgHeader sendingAgent = null,
			OrgHeader supplier = null,
			OrgHeader carrier = null,
			OrgHeader buyer = null,
			decimal? jD_ActualWeight = null,
			decimal? jD_ActualVolume = null,
			int? jD_Packs = null)
		{
			if (jD_OA_NKDeliverAddress_NI != null)
			{
				var query = new ZQuery(OrgAddressSchema.OA_Code, jD_OA_NKDeliverAddress_NI);
				var goodsDeliverToAddress = order.Factory.LoadTop1<OrgAddress>(query);
				order.GoodsDeliveredToAddress.E2_OA_Address = goodsDeliverToAddress?.PK ?? ZGuid.Empty;
			}

			if (jD_OA_NKPickupAddress_NI != null)
			{
				var query = new ZQuery(OrgAddressSchema.OA_Code, jD_OA_NKPickupAddress_NI);
				var goodsAvailableAtAddress = order.Factory.LoadTop1<OrgAddress>(query);
				order.GoodsAvailableAtAddress.E2_OA_Address = goodsAvailableAtAddress?.PK ?? ZGuid.Empty;
			}

			if (jD_RL_NKGoodsDeliveredTo != null)
			{
				order.JD_RL_NKGoodsDeliveredTo = jD_RL_NKGoodsDeliveredTo;
			}

			if (jD_RL_NKGoodsAvailableAt != null)
			{
				order.JD_RL_NKGoodsAvailableAt = jD_RL_NKGoodsAvailableAt;
			}

			if (jD_RS_NKServiceLevel_NI != null)
			{
				order.JD_RS_NKServiceLevel_NI = jD_RS_NKServiceLevel_NI;
			}

			if (jD_RX_NKOrderCurrency != null)
			{
				order.JD_RX_NKOrderCurrency = jD_RX_NKOrderCurrency;
			}

			if (jD_ActualWeight != null)
			{
				order.JD_ActualWeight = jD_ActualWeight.Value;
			}

			if (jD_ActualVolume != null)
			{
				order.JD_ActualVolume = jD_ActualVolume.Value;
			}

			if (receivingAgent != null)
			{
				order.JD_OH_ReceivingAgent = receivingAgent.PK;
			}

			if (jD_F3_NKPackType != null)
			{
				order.JD_F3_NKPackType = jD_F3_NKPackType;
			}

			if (jD_ContainerMode != null)
			{
				order.JD_ContainerMode = jD_ContainerMode;
			}

			if (jD_UnitOfWeight != null)
			{
				order.JD_UnitOfWeight = jD_UnitOfWeight;
			}

			if (jD_UnitOfVolume != null)
			{
				order.JD_UnitOfVolume = jD_UnitOfVolume;
			}

			if (sendingAgent != null)
			{
				order.JD_OH_SendingAgent = sendingAgent.PK;
			}

			if (supplier != null)
			{
				order.SupplierPK = supplier.PK;
			}

			if (jD_Packs != null)
			{
				order.JD_Packs = jD_Packs.Value;
			}

			if (carrier != null)
			{
				order.JD_OH_Carrier = carrier.PK;
			}

			if (buyer != null)
			{
				order.BuyerPK = buyer.PK;
			}

			return order;
		}

		public static OrderContainer With(this OrderContainer container, string containerType = null, short? j1_ContainerCount = null)
		{
			if (j1_ContainerCount != null)
			{
				container.J1_ContainerCount = j1_ContainerCount.Value;
			}

			if (containerType != null)
			{
				container.J1_RC = container.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			}

			return container;
		}

		public static OrderLine With(this OrderLine line, decimal? jO_LinePrice = null)
		{
			if (jO_LinePrice != null)
			{
				line.JO_LinePrice = jO_LinePrice.Value;
			}

			return line;
		}
	}
}
