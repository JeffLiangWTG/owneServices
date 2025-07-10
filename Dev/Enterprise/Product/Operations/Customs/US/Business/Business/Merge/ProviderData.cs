using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class DerivedDutyCalculatorProviderData
	{
		public DerivedDutyCalculatorProviderData(IEntryLineOrInvoiceLineDutyData parentLine)
		{
			this.ParentLine = parentLine;
		}
		public ZDecimal Quantity1;
		public ZDecimal Quantity2;
		public ZDecimal Quantity3;
		public ZDecimal CustomsValue;
		public IEntryLineOrInvoiceLineDutyData ParentLine;

		internal void AddLines(List<IEntryLineOrInvoiceLineDutyData> childLines)
		{
			if (ParentLine.IsRecon)
			{
				CustomsValue = ParentLine.CustomsValue; //We adjusted the customs value in original duty calculator, 'parentLine.StoreAdjustedDerivedCustomsValue(providerData.CustomsValue);'
			}

			foreach (IEntryLineOrInvoiceLineDutyData childLine in childLines)
			{
				Quantity1 += childLine.Quantity1;
				Quantity2 += childLine.Quantity2;
				Quantity3 += childLine.Quantity3;
				CustomsValue += childLine.CustomsValue;
			}
		}
	}
}
