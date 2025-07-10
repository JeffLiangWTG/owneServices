using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Duty is reduced in such proportion as the cost of US products bears to the full value of imported components
	/// </summary>
	class USProductsAssemblyDutyCalculator
	{
		public USProductsAssemblyDutyCalculator(IEnumerable<IEntryLineOrInvoiceLineDutyData> lines, ZString assemblyTariff)
		{
			this.lines = lines;
			this.assemblyTariff = assemblyTariff;
			dutyRate = 0m;
		}
		readonly IEnumerable<IEntryLineOrInvoiceLineDutyData> lines;
		readonly ZString assemblyTariff;

		bool ShouldDeriveDutyRate
		{
			get
			{
				if (!shouldDeriveDutyRate.HasValue)
				{
					shouldDeriveDutyRate = lines.FirstOrDefault(x => x.IsCombinedLine() && x.SupTariffs.Any(supTariff => Chapter98Helper.Is98Tariff(supTariff))) == null;
				}
				return shouldDeriveDutyRate.Value;
			}
		}
		bool? shouldDeriveDutyRate;

		public void Execute()
		{
			if (ShouldDeriveDutyRate)
			{
				CalculateValueAndDuty();
			}

			RecordFinalDuty();
		}

		ZDecimal dutyRate;

		void CalculateValueAndDuty()
		{
			ZDecimal totalValue = 0m;
			ZDecimal totalUSValue = 0m;
			ZDecimal usProductValue = 0m;

			//lines are in the order of invoice line no before duty is calculated. LineMerge.AssignLineNumber happens before LineMerger.CalculateDuty
			foreach (IEntryLineOrInvoiceLineDutyData entryLine in lines)
			{
				totalValue += entryLine.CustomsValue;

				if (entryLine.Tariff != assemblyTariff)
				{
					var dutyData = new DutyDataProxy(entryLine);
					dutyData.CustomsValue = usProductValue + entryLine.CustomsValue;
					var childLineCalculator = new AppendixFDutyCalculator(dutyData, entryLine.Factory);

					var dutyResult = childLineCalculator.DutyResult;
					var dutyForLine = dutyResult.TotalAmount.Amount;
					entryLine.SetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, dutyForLine, new FeeCalculationInternalData());
				}
				else
				{
					usProductValue = entryLine.CustomsValue;
					totalUSValue += usProductValue;
				}
			}

			if (totalValue != 0m)
			{
				ZDecimal usComponentsRate = totalUSValue / totalValue;
				dutyRate = 1 - usComponentsRate;
			}
		}

		void RecordFinalDuty()
		{
			foreach (IEntryLineOrInvoiceLineDutyData line in lines)
			{
				if (line.Tariff != assemblyTariff)
				{
					if (ShouldDeriveDutyRate)
					{
						var existingDuty = line.GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);
						ZDecimal dutyAmount = existingDuty * dutyRate;

						var dutyResult = new DutyResult();
						dutyResult.PercentOfValue = 0m;
						dutyResult.PerUnitAmount = 0m;
						dutyResult.PerUnitUQ = ZString.Empty;
						dutyResult.TotalAmount = new Money(dutyAmount.Round(2), JobDeclaration.GetLocalCurrency());

						line.SetDutyResult(dutyResult);
					}
					else
					{
						var calculator = new AppendixFDutyCalculator(line, line.Factory);
						var dutyResult = calculator.DutyResult;
						line.SetDutyResult(dutyResult);
					}
				}
			}
		}
	}
}
