using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IFTZWhsOrderLineData
	{
		ZString ProductCode { get; }
		ZString ProductDesc { get; }
		ZDecimal Quantity { get; }
		ZString QuantityUnit { get; }
		ZString Tariff { get; }
		ZString CountryOfOriginCode { get; }
		ZString PrimaryPreference { get; }
		ZDecimal TotalReceiveQuantity { get; }
		ZDecimal TotalReceiveValueForDuty { get; }
		ZDecimal TotalReceiveCustomsQty { get; }
		ZString ReceiveCustomsQtyUnit { get; }
		ZDecimal TotalReceiveCustomsSecondQty { get; }
		ZString ReceiveCustomsSecondQtyUnit { get; }
		ZDecimal TotalReceiveCustomsThirdQty { get; }
		ZString ReceiveCustomsThirdQtyUnit { get; }
		ZString ReceiveCustomsAddInfo { get; }
	}
}
