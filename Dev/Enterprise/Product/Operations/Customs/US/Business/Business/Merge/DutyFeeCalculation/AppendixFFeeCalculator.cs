using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class AppendixFFeeCalculator : AppendixFCalculator
	{
		public AppendixFFeeCalculator(IFeeCalculationDataProvider dutyData, ZString feeCode, BusinessObjectFactory factory)
			: base(dutyData, factory)
		{
			this.feeCode = feeCode;
		}

		readonly ZString feeCode;

		protected override IDutyResult Calculate()
		{
			IDutyResult result;

			var feeDataProvider = (IFeeCalculationDataProvider)dutyData;
			var overriddenTaxRate = feeDataProvider.OverriddenTaxRate;
			var overriddenTaxRateUQ = feeDataProvider.OverriddenTaxRateUQ;
			var taxRateQuantity = feeDataProvider.TaxRateQuantity;
			var taxComputationCode = feeDataProvider.TaxComputationCode;

			if (feeCode == feeDataProvider.TaxCode && overriddenTaxRate.HasValue)
			{
				DutyResult dutyResult = new DutyResult();

				if (taxComputationCode == ComputationCodeList.Codes.SpecificRateFirstQuantity)
				{
					dutyResult.PerUnitAmount = overriddenTaxRate.Value;
					dutyResult.PerUnitUQ = dutyData.UQ1;

					var quantity = GetQuantityForOverriddenTaxCalculation(dutyData.Quantity1, dutyData.UQ1, overriddenTaxRateUQ);
					dutyResult.TotalAmount = GetLocalAmount(overriddenTaxRate.Value * quantity);
				}
				else if (taxComputationCode == ComputationCodeList.Codes.SpecificRateSecondQuantity)
				{
					dutyResult.PerUnitAmount = overriddenTaxRate.Value;
					dutyResult.PerUnitUQ = dutyData.UQ2;

					var quantity = GetQuantityForOverriddenTaxCalculation(dutyData.Quantity2, dutyData.UQ2, overriddenTaxRateUQ);
					dutyResult.TotalAmount = GetLocalAmount(overriddenTaxRate.Value * quantity);
				}
				else if (taxComputationCode == ComputationCodeList.Codes.AdValorem)
				{
					dutyResult.PercentOfValue = overriddenTaxRate.Value;
					dutyResult.TotalAmount = GetLocalAmount(overriddenTaxRate.Value * GetCustomsValueForTaxFeeCalculation(dutyData));
				}
				else if (taxComputationCode == ComputationCodeList.CustomComputationCodeForCalculatingIRTax)
				{
					dutyResult.PerUnitUQ = overriddenTaxRateUQ;
					dutyResult.TotalAmount = GetLocalAmount(overriddenTaxRate.Value * taxRateQuantity);
				}

				result = dutyResult;
			}
			else
			{
				IFeeWrapper wrapper = DutyRateWrapper.GetFeeWrapper(dutyData, feeCode);
				if (wrapper != null)
				{
					ZString computationCode = wrapper.ComputationCode;
					result = Calculate(wrapper, computationCode);
				}
				else
				{
					result = new DutyResult();
				}
			}

			return result;
		}

		ZDecimal GetQuantityForOverriddenTaxCalculation(ZDecimal quantity, ZString customsUQ, ZString overriddenTaxRateUQ)
		{
			var result = quantity;

			if (overriddenTaxRateUQ == AppendixBTaxRateList.Fifty)
			{
				switch (customsUQ)
				{
					case ABIUnitOfMeasureList.Codes.Thousand:
						result = quantity * 20m;
						break;

					case ABIUnitOfMeasureList.Codes.Number:
						result = quantity / 50m;
						break;

					case AppendixBTaxRateList.Fifty:
						result = quantity;
						break;

					default:
						result = 0m;
						break;
				}
			}

			return result;
		}

		ZDecimal GetCustomsValueForTaxFeeCalculation(IDutyData lineDutyData)
		{
			var result = lineDutyData.GetEffectiveCustomsValue();

			if (lineDutyData.IsCombinedLine())
			{
				var normalTariffLine = lineDutyData.CombineAllLines.FirstOrDefault(x => x.IsNormalTariffLine());
				if (normalTariffLine != null)
				{
					result = normalTariffLine.GetEffectiveCustomsValue();
				}
			}
			else
			{
				var parentDutyData = lineDutyData.ParentTariffLine;
				if (parentDutyData != null && parentDutyData.ImportTariff != null && parentDutyData.ImportTariff.Applies(TariffRuleList.Codes.AdditionalTariffs, lineDutyData.DateForDutyCalculation))
				{
					result = parentDutyData.GetEffectiveCustomsValue();
				}
			}

			return result;
		}
	}
}
