using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class DairyFeeCalculator : ILineFeeCalculator
	{
		FeeResult ILineFeeCalculator.CalculateFee(IFeeCalculationDataProvider invoiceLine)
		{
			var importTariff = invoiceLine.ImportTariff;

			var result = new FeeResult(0m, false);
			if (importTariff != null)
			{
				var dutyRate = importTariff.DutyRates.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.DairyFee);

				if (dutyRate != null)
				{
					var amount = ZDecimal.Zero;
					var percentOfRate = ZDecimal.Zero;
					var noneCustomsValueAmount = ZDecimal.Zero;

					if (dutyRate.UD_TaxFeeComputationCode.IsEmpty || dutyRate.UD_TaxFeeComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable)
					{
						amount = new ZDecimal(invoiceLine.DairyQty * dutyRate.UD_TaxFeeSpecificRate);
						noneCustomsValueAmount = amount;
					}
					else
					{
						var dutyResult = new AppendixFFeeCalculator(invoiceLine, Core.Constants.USCustoms.FeeCodes.DairyFee, invoiceLine.Factory).DutyResult;
						amount = dutyResult.TotalAmount.Amount;

						var feeCalculationInternalData = DDPCalculationHelper.GetFeeCalculationInternalDataFromIDutyResult(dutyResult);
						percentOfRate = feeCalculationInternalData.PercentOfRate;
						noneCustomsValueAmount = feeCalculationInternalData.NoneCustomsValueAmount;
					}

					result = new FeeResult(amount.Round(2), true, percentOfRate, noneCustomsValueAmount);
				}
			}

			return result;
		}
	}
}
