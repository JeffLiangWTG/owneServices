using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	// Tested by Test93 (EntrySummaryMessageBuilder)
	class RepairsDutyCalculator
	{
		public RepairsDutyCalculator(IEnumerable<IEntryLineOrInvoiceLineDutyData> lines, ZString repairTariff)
		{
			this.lines = lines;
			this.repairTariff = repairTariff;
		}
		readonly IEnumerable<IEntryLineOrInvoiceLineDutyData> lines;
		readonly ZString repairTariff;

		bool ShouldDeriveAdValoremRate
		{
			get
			{
				if (!shouldDeriveAdValoremRate.HasValue)
				{
					shouldDeriveAdValoremRate = true;
					if (lines.Count() == 2)
					{
						var secondaryLine = lines.ElementAt(1);
						if (secondaryLine.ImportTariff != null && secondaryLine.ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.AdValorem)
						{
							shouldDeriveAdValoremRate = false;
						}
					}
				}
				return shouldDeriveAdValoremRate.Value;
			}
		}
		bool? shouldDeriveAdValoremRate;

		public void Execute()
		{
			if (ShouldDeriveAdValoremRate)
			{
				CalculateValueAndNormalDuty();
			}

			RecordAdValoremDuty();
		}

		ZDecimal derivedAdvaloremRate;

		void CalculateValueAndNormalDuty()
		{
			var totalDuty = ZDecimal.Zero;
			var totalValue = ZDecimal.Zero;
			ZDecimal repairCustomsValue = ZDecimal.Zero;

			foreach (IEntryLineOrInvoiceLineDutyData line in lines)
			{
				if (!line.IsCombineSecondaryTariffLine && (line.Tariff == repairTariff || line.IsNormalTariffLine()))
				{
					totalValue += line.CustomsValue;

					if (line.Tariff != repairTariff)
					{
						var dutyData = new DutyDataProxy(line);
						dutyData.CustomsValue = repairCustomsValue + line.CustomsValue;
						var childLineCalculator = new AppendixFDutyCalculator(dutyData, line.Factory);
						totalDuty += childLineCalculator.DutyResult.TotalAmount.Amount.Round(2);
					}
					else
					{
						repairCustomsValue = line.CustomsValue;
					}
				}
			}

			if (totalValue != 0)
			{
				ZDecimal advaloremRate = totalDuty / totalValue;
				derivedAdvaloremRate = advaloremRate.Truncate(5);
			}
		}

		void RecordAdValoremDuty()
		{
			var customsValue = ZDecimal.Zero;

			foreach (IEntryLineOrInvoiceLineDutyData line in lines)
			{
				var adjustLine = line;
				if (adjustLine.IsNormalTariffLine())
				{
					if (adjustLine.IsCombineSecondaryTariffLine)
					{
						customsValue = adjustLine.CustomsValue;
					}

					if (adjustLine.Tariff != repairTariff)
					{
						if (customsValue != 0)
						{
							adjustLine = new LineDutyData(line, customsValue);
						}

						if (ShouldDeriveAdValoremRate)
						{
							DutyResult dutyResult = new DutyResult();

							ZDecimal amount = derivedAdvaloremRate * adjustLine.CustomsValue;
							amount = amount.Round(2);

							dutyResult.PercentOfValue = derivedAdvaloremRate * 100;
							dutyResult.PerUnitAmount = 0m;
							dutyResult.PerUnitUQ = ZString.Empty;
							dutyResult.TotalAmount = new Money(amount, JobDeclaration.GetLocalCurrency());

							if (dutyResult.PercentOfValue > 0m)
							{
								dutyResult.RateString = dutyResult.PercentOfValue.ToStringTrimZeros() + "%";
							}

							adjustLine.SetDutyResult(dutyResult);
						}
						else
						{
							var calculator = new AppendixFDutyCalculator(adjustLine, adjustLine.Factory);
							var dutyResult = calculator.DutyResult;

							adjustLine.SetDutyResult(dutyResult);
						}
					}
				}
			}
		}
	}
}
