using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMCBPShipmentDescription : IAIMCBPShipmentDescription
	{
		public static AIMCBPShipmentDescription CreateIfHasGoodsValue(AsycudaBill bill)
		{
			return bill.ABL_GoodsValue > 0m && !bill.ABL_RX_NKGoodsValueCurrency.IsEmpty ? new AIMCBPShipmentDescription(bill) : null;
		}

		protected AIMCBPShipmentDescription(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			goodsValueInUSD = bill.GoodsValueInUSD;
		}

		readonly AsycudaBill bill;
		readonly ZDecimal goodsValueInUSD;

		public ZString OriginOfGoods => bill.GoodsOrigin;

		public ZDecimal DeclaredValue => !goodsValueInUSD.IsEmpty ? goodsValueInUSD : bill.ABL_GoodsValue;

		public ZString ISOCurrencyCode => !goodsValueInUSD.IsEmpty ? (ZString)Core.Constants.CurrencyCodes.UnitedStates : bill.ABL_RX_NKGoodsValueCurrency;

		public ZString HarmonizedCommodityCode => bill.ABL_Tariff;
	}
}
