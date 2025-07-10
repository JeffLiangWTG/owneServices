using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.US.Business
{
	class DerivedDutyCalculator
	{
		public IDutyResult Calculate(IEntryLineOrInvoiceLineDutyData parentLine)
		{
			var derivedSetsNormalLine = CalculateDutyForSetsHelper.GetDerivedSetsNormalLine(parentLine);
			if (derivedSetsNormalLine != null)
			{
				parentLine = parentLine.ParentLine ?? parentLine;
			}

			var childLines = new List<IEntryLineOrInvoiceLineDutyData>(parentLine.SecondaryLines);
			if (derivedSetsNormalLine != null)
			{
				childLines = childLines.Where(x => !x.HasBothSupTariffAndNormalTariff()).ToList();
			}

			IEntryLineOrInvoiceLineDutyData childLineToKeep = null;
			AppendixFDutyCalculator calculator = null;

			var providerData = new DerivedDutyCalculatorProviderData(parentLine);
			providerData.AddLines(childLines);

			GetLineAndCalculatorWithMaxDuty(childLines, providerData, out childLineToKeep, out calculator);

			IDutyResult result = new DutyResult();

			if (childLineToKeep != null)
			{
				foreach (IEntryLineOrInvoiceLineDutyData childLine in childLines)
				{
					//do actions only for CusEntryLine
					childLine.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(childLineToKeep.PK);

					childLine.StoreAdjustedDerivedCustomsValue(0m);
				}

				if (derivedSetsNormalLine != null)
				{
					derivedSetsNormalLine.StoreAdjustedDerivedCustomsValue(providerData.CustomsValue);
				}
				else
				{
					parentLine.StoreAdjustedDerivedCustomsValue(providerData.CustomsValue);
				}

				result = calculator.DutyResult;
			}

			return result;
		}

		void GetLineAndCalculatorWithMaxDuty(List<IEntryLineOrInvoiceLineDutyData> childLines, DerivedDutyCalculatorProviderData providerData, out IEntryLineOrInvoiceLineDutyData childLineWithMaxDuty, out AppendixFDutyCalculator calculator)
		{
			childLineWithMaxDuty = null;
			calculator = null;

			foreach (IEntryLineOrInvoiceLineDutyData childLine in childLines)
			{
				var dutyData = new DutyDataProxy(childLine);
				dutyData.Quantity1 = providerData.Quantity1;
				dutyData.Quantity2 = providerData.Quantity2;
				dutyData.Quantity3 = providerData.Quantity3;
				dutyData.CustomsValue = providerData.CustomsValue;

				AppendixFDutyCalculator potentialCalculator = new AppendixFDutyCalculator(dutyData, dutyData.Factory);

				if (calculator == null || potentialCalculator.DutyResult.TotalAmount.Amount > calculator.DutyResult.TotalAmount.Amount)
				{
					calculator = potentialCalculator;
					childLineWithMaxDuty = childLine;
				}
			}
		}
	}
}
