namespace Enterprise.Customs.US.Business
{
	static class IDutyDataLineHeaderExtensionscs
	{
		public static void UpdateHMFAccordingToMinimumThresholdRule(this IDutyDataLineHeader entry)
		{
			if (entry.IsHMFDeMinimisApplicable)
			{
				var currentHMF = entry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF);
				if (currentHMF > 0 && currentHMF <= new FeeCalculationHelper(entry.Factory, entry.DateForFeeCalculation).HMFThresholdAmount && currentHMF == entry.FeeAndCharges.GetGrandTotalFee())
				{
					entry.FeeAndCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.HMF, 0m);

					entry.UpdateAfterHMFDeMinimusRuleApplied();
				}
			}
		}
	}
}
