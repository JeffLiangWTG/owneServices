using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class TaxFeeCalculator : ILineFeeCalculator
	{
		public TaxFeeCalculator(string feeCode, ZBool ignoreTIBExemptionCondition)
			: this(feeCode, false, ignoreTIBExemptionCondition)
		{
		}

		public TaxFeeCalculator(string feeCode, bool disregardAMSFeeExempt, ZBool ignoreTIBExemptionCondition)
		{
			if (feeCode == null)
			{
				throw new ArgumentNullException(nameof(feeCode));
			}

			if (feeCode.Length == 0)
			{
				throw new ArgumentException("feecode");
			}

			this.feeCode = feeCode;
			this.disregardAMSFeeExempt = disregardAMSFeeExempt;
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}

		readonly string feeCode;
		readonly bool disregardAMSFeeExempt;
		readonly ZBool ignoreTIBExemptionCondition;

		#region IFeeCalculator Members

		public FeeResult CalculateFee(IFeeCalculationDataProvider invoiceLine)
		{
			bool isRequired = ShouldHaveFee(invoiceLine.EntryType);
			IDutyResult dutyResult = new DutyResult();
			if (isRequired)
			{
				dutyResult = CalculateFeeAmount(invoiceLine);
			}
			var feeCalculationInternalData = DDPCalculationHelper.GetFeeCalculationInternalDataFromIDutyResult(dutyResult);
			return new FeeResult(dutyResult.TotalAmount.Amount, isRequired, feeCalculationInternalData.PercentOfRate, feeCalculationInternalData.NoneCustomsValueAmount);
		}

		bool ShouldHaveFee(string entryType)
		{
			return ignoreTIBExemptionCondition || entryType != EntryTypeList.Codes.TemporaryImportationBond;
		}

		IDutyResult CalculateFeeAmount(IFeeCalculationDataProvider invoiceLine)
		{
			IDutyResult result = new DutyResult();
			if (disregardAMSFeeExempt || !invoiceLine.IsAMSFeeExempt)
			{
				result = new AppendixFFeeCalculator(invoiceLine, feeCode, invoiceLine.Factory).DutyResult;
			}
			return result;
		}

		#endregion
	}
}
