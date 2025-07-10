using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class GoodsShipmentDutyTaxFeeWrapper : IGoodsShipmentDutyTaxFee
	{
		public GoodsShipmentDutyTaxFeeWrapper(ZString chargeType, ZDecimal chargeAmount)
		{
			TypeCode = chargeType;
			AdValoremTaxBaseAmount = chargeAmount;
		}

		public ZDecimal AdValoremTaxBaseAmount { get; }

		public ZString TypeCode { get; }
	}
}
