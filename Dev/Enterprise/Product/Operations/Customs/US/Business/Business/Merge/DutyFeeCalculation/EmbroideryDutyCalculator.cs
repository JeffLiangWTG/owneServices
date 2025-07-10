using System.Linq;

namespace Enterprise.Customs.US.Business
{
	public class EmbroideryDutyCalculator
	{
		public IDutyResult Calculate(IEntryLineOrInvoiceLineDutyData line)
		{
			IDutyResult result = new DutyResult();

			var parentLine = line.ParentLine ?? line;
			var isParentLineSupTariffLine = parentLine.IsSupTariffLine();
			if (line.IsSupTariffLine())
			{
				if (isParentLineSupTariffLine)
				{
					if (line.PK == parentLine.PK || (parentLine.IsCombinedLine() && parentLine.CombineParentLine == null && !line.IsCombineSecondaryTariffLine))
					{
						result = CalculateDutyResult(line, parentLine);
					}
				}
				else
				{
					var firstSupTariffLine = parentLine.SecondaryLines.FirstOrDefault(x => x.IsSupTariffLine()) ?? line;
					if (firstSupTariffLine.PK == line.PK)
					{
						result = CalculateDutyResult(firstSupTariffLine, parentLine);
					}
				}
			}
			else
			{
				var normalTariffLine = !isParentLineSupTariffLine ? parentLine : parentLine.SecondaryLines.FirstOrDefault(x => x.IsNormalTariffLine()) ?? line;
				if (normalTariffLine.PK == line.PK)
				{
					result = CalculateDutyResult(line, parentLine);

					foreach (var secondaryLine in parentLine.SecondaryLines)
					{
						if (!secondaryLine.IsSupTariffLine() && secondaryLine.PK != line.PK)
						{
							var dutyResultForSecondary = CalculateDutyResult(secondaryLine, parentLine);
							if (result == null || result.TotalAmount.Amount < dutyResultForSecondary.TotalAmount.Amount)
							{
								result = dutyResultForSecondary;
							}
						}
					}
				}
			}

			return result;
		}

		IDutyResult CalculateDutyResult(IEntryLineOrInvoiceLineDutyData lineToCalculate, IEntryLineOrInvoiceLineDutyData parentLine)
		{
			var dutyData = new DutyDataProxy(lineToCalculate) { CustomsValue = parentLine.TotalCustomsValueIncludingSecondaryLines };
			var dutyCalculator = new AppendixFDutyCalculator(dutyData, dutyData.Factory);
			return dutyCalculator.DutyResult;
		}
	}
}
