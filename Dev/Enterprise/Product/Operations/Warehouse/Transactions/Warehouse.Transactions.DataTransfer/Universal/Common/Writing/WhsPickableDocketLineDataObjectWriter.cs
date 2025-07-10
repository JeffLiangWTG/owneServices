using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsPickableDocketLineDataObjectWriter<T> : WhsDocketLineDataObjectWriter<T>
		where T : WhsPickableDocketLine
	{
		public WhsPickableDocketLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override sealed void PopulateDataObject(OrderLine orderLineDataObject, T orderLineBO)
		{
			orderLineDataObject.ExtendedLinePrice = orderLineBO.WE_ExtendedLinePrice;
			orderLineDataObject.QuantityMet = orderLineBO.SumOfUnitsMet;
			orderLineDataObject.UnitPriceRecommended = orderLineBO.WE_RecommendedUnitPrice;
			orderLineDataObject.UnitPriceDiscountAmount = orderLineBO.WE_UnitDiscountAmount;
			orderLineDataObject.UnitPriceDiscountPercent = orderLineBO.WE_UnitDiscountPercent;
			orderLineDataObject.UnitPriceAfterDiscount = orderLineBO.WE_UnitPriceAfterDiscount;
			orderLineDataObject.UnitPriceCurrency = ListHelper.GetWithDescription<Currency>(orderLineBO.WE_RX_NKUnitPriceCurrency, orderLineBO.Lookups.UnitPriceCurrencies);

			PopulateDataObjectCore(orderLineDataObject, orderLineBO);
		}

		protected virtual void PopulateDataObjectCore(OrderLine orderLineDataObject, T orderLineBO)
		{
		}
	}
}
