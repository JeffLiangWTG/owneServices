// -----------------------------------------------------------------------
// <copyright file="EntryLevelFeeCalculator.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Types;

	public static class EntryLevelFeeCalculator
	{
		public static void CalculateAndStoreEntryLevelFees(this IDutyDataLineHeader entry, Action<string, decimal> storeFee)
		{
			var informalFeeAmount = new InformalFeeCalculator().CalculateFee(entry).Amount;
			storeFee(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, informalFeeAmount);

			var dutiableMailCharge = new DutiableMailFeeCalculator().Calculate(entry);
			storeFee(Core.Constants.USCustoms.FeeCodes.DutiableMail, dutiableMailCharge);

			var amount = entry.DoesMPFSurchargeApply ? new FeeCalculationHelper(entry.Factory, entry.DateForMPFCalculation).ManualSurchargeAmount : 0m;
			storeFee(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, amount);
		}

		public static bool IsFeeUserEntered(this IDutyDataLineHeader entry, ZString feeType)
		{
			return feeType == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing && entry.OverridenTotalMPFPayable.HasValue;
		}

		public static IEnumerable<string> GetEntryLevelFeeCodes()
		{
			yield return Core.Constants.USCustoms.FeeCodes.MerchandiseInformal;
			yield return Core.Constants.USCustoms.FeeCodes.DutiableMail;
			yield return Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge;
		}
	}
}
