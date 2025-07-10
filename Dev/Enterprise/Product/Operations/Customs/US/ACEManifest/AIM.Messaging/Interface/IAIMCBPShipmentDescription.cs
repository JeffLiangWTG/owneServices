using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMCBPShipmentDescription
	{
		ZString OriginOfGoods { get; }
		ZDecimal DeclaredValue { get; }
		ZString ISOCurrencyCode { get; }
		ZString HarmonizedCommodityCode { get; }
	}
}
