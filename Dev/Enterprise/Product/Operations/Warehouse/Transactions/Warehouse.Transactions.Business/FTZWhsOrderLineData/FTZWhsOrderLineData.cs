using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class FTZWhsOrderLineData : IFTZWhsOrderLineData
	{
		#region Constructor

		public FTZWhsOrderLineData(ZString productCode, ZString productDesc, ZDecimal quantity, ZString quantityUnit, ZString tariff,
			ZString countryOfOriginCode, ZString primaryPreference, ZDecimal totalReceiveQuantity, ZDecimal totalReceiveValueForDuty,
			ZDecimal totalReceiveCustomsQty, ZString rceiveCustomsQtyUnit, ZDecimal totalReceiveCustomsSecondQty, ZString receiveCustomsSecondQtyUnit,
			ZDecimal totalReceiveCustomsThirdQty, ZString receiveCustomsThirdQtyUnit, ZString receiveCustomsAddInfo)
		{
			ProductCode = productCode;
			ProductDesc = productDesc;
			Quantity = quantity;
			QuantityUnit = quantityUnit;
			Tariff = tariff;
			CountryOfOriginCode = countryOfOriginCode;
			PrimaryPreference = primaryPreference;
			TotalReceiveQuantity = totalReceiveQuantity;
			TotalReceiveValueForDuty = totalReceiveValueForDuty;
			TotalReceiveCustomsQty = totalReceiveCustomsQty;
			ReceiveCustomsQtyUnit = rceiveCustomsQtyUnit;
			TotalReceiveCustomsSecondQty = totalReceiveCustomsSecondQty;
			ReceiveCustomsSecondQtyUnit = receiveCustomsSecondQtyUnit;
			TotalReceiveCustomsThirdQty = totalReceiveCustomsThirdQty;
			ReceiveCustomsThirdQtyUnit = receiveCustomsThirdQtyUnit;
			ReceiveCustomsAddInfo = receiveCustomsAddInfo;
		}

		#endregion

		#region IFTZWhsOrderLineData Members

		public ZString ProductCode { get; }

		public ZString ProductDesc { get; }

		public ZDecimal Quantity { get; }

		public ZString QuantityUnit { get; }

		public ZString Tariff { get; }

		public ZString CountryOfOriginCode { get; }

		public ZString PrimaryPreference { get; }

		public ZDecimal TotalReceiveValueForDuty { get; }
		public ZDecimal TotalReceiveQuantity { get; }
		public ZDecimal TotalReceiveCustomsQty { get; }
		public ZString ReceiveCustomsQtyUnit { get; }
		public ZDecimal TotalReceiveCustomsSecondQty { get; }
		public ZString ReceiveCustomsSecondQtyUnit { get; }
		public ZDecimal TotalReceiveCustomsThirdQty { get; }
		public ZString ReceiveCustomsThirdQtyUnit { get; }
		public ZString ReceiveCustomsAddInfo { get; }

		#endregion
	}
}
