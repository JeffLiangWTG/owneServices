using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ZACustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		public ZACustomsQuantityConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		public override ZDecimal CalculateFromNetWeightToCustomsQtyCore()
		{
			var customsQty = Core.Constants.Weight.Convert(InvoiceLine.JI_NetWeight, InvoiceLine.JI_NetWeightUQ, (ZString)customsUnitOfQuantityInfo.Value);
			if (customsQty < MinimumReapportionedCustomsQty)
			{
				customsQty = MinimumReapportionedCustomsQty;
			}
			return customsQty;
		}

		const decimal MinimumReapportionedCustomsQty = 0.01m;
	}
}
