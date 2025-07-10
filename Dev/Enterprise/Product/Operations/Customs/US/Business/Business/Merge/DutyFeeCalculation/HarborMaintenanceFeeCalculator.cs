using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class HarborMaintenanceFeeCalculator : ILineFeeCalculator
	{
		public HarborMaintenanceFeeCalculator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#region IFeeCalculator Members

		public FeeResult CalculateFee(IFeeCalculationDataProvider line)
		{
			FeeResult result = new FeeResult(ZDecimal.Zero);

			if (!line.IsSetVLine && !IsLineHMFExempt(line))
			{
				var customsValue = GetTotalCustomsValueIncludingSecondary(line);
				var rate = new FeeCalculationHelper(factory, line.DateForDutyCalculation).HMFRatePercentage;
				var amount = customsValue * rate / 100;
				result = new FeeResult(amount, rate);
			}

			return result;
			//.125% ad valorem of merchandise
		}

		bool IsLineHMFExempt(IFeeCalculationDataProvider line)
		{
			return line.IsCombinedLine() ? line.CombineAllLines.Any(x => TariffHasHMFExemptRule(x.Tariff, x.DateForDutyCalculation)) : TariffHasHMFExemptRule(line.Tariff, line.DateForDutyCalculation) || line.SupTariffs.Any(supTariff => TariffHasHMFExemptRule(supTariff, line.DateForDutyCalculation));
		}

		bool TariffHasHMFExemptRule(ZString tariff, ZDate date) => USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, tariff, date, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.HMFExempt98Tariffs) != null;

		ZDecimal GetTotalCustomsValueIncludingSecondary(IFeeCalculationDataProvider line)
		{
			var result = ZDecimal.Zero;
			if (line.IsCombinedLine() && !CalculateDutyForSetsHelper.IsCombinedXLine(line))
			{
				result = line.CustomsValue;
				var combineAllLines = line.CombineAllLines;
				var combine98Line = combineAllLines.FirstOrDefault(x => x.SupTariffs.Any(supTariff => Chapter98Helper.Is98Tariff(supTariff)));
				if (combine98Line != null)
				{
					result += combine98Line.CustomsValue;
				}
			}
			else if (!line.IsSecondaryTariffLine)
			{
				result = line.CustomsValue + line.SecondaryLines.Sum(s => s.CustomsValue);
			}

			return result;
		}

		#endregion
	}
}
