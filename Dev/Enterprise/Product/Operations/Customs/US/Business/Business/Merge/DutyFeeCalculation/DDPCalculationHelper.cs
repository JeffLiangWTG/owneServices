using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public static class DDPCalculationHelper
	{
		public static FeeCalculationInternalData GetFeeCalculationInternalDataFromIDutyResult(IDutyResult dutyResult)
		{
			var result = new FeeCalculationInternalData();
			if (dutyResult.TotalAmount.Amount > 0)
			{
				if (dutyResult.PercentOfValue > 0)
				{
					result.PercentOfRate = dutyResult.PercentOfValue;
					result.NoneCustomsValueAmount = dutyResult.NoneCustomsValueAmount;
				}
				else
				{
					result.NoneCustomsValueAmount = dutyResult.TotalAmount.Amount;
				}
			}

			return result;
		}

		public static FeeCalculationInternalData CombineFeeCalculationInternalDatas(FeeCalculationInternalData data1, FeeCalculationInternalData data2)
		{
			var result = new FeeCalculationInternalData();

			if (data1 != null && data2 != null)
			{
				result.PercentOfRate = data1.PercentOfRate + data2.PercentOfRate;
				result.NoneCustomsValueAmount = data1.NoneCustomsValueAmount + data2.NoneCustomsValueAmount;
			}
			return result;
		}

		public static bool HasUserEnteredDisbursementCharge(IChargeHolder chargeHolder)
		{
			bool result = false;

			foreach (Customs.Business.BaseJobComInvHeaderCharge charge in chargeHolder.Charges)
			{
				if (charge.J7_ChargeType == USCustomsChargeTypeList.Codes.DisbursementCharge && !charge.J7_IsSystem && (!charge.J7_IsDutiable || charge.J7_AdjustedCharge))
				{
					result = true;
					break;
				}
			}

			if (!result)
			{
				IChargeHolder parentChargeHolder = chargeHolder.ImmediateChargeHolderParent;

				if (parentChargeHolder != null)
				{
					result = HasUserEnteredDisbursementCharge(parentChargeHolder);
				}
			}

			return result;
		}

		public static bool IsDisbursementChargeToBeAutoCalculated(JobComInvoiceHeader invoice)
		{
			bool result = false;

			if (invoice != null && (invoice.JZ_IncoTerm == TermsOfDeliveryList.Codes.DDP || invoice.JZ_IncoTerm == TermsOfDeliveryList.Codes.EXQ))
			{
				result = true;

				if (HasUserEnteredDisbursementCharge(invoice))
				{
					result = false;
				}
			}

			return result;
		}
	}
}
