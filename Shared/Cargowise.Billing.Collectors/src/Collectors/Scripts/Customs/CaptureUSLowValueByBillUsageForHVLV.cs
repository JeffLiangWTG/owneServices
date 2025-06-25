namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class CaptureUSLowValueByBillUsageForHVLV : CaptureUSLowValueByBillUsage
	{
		public override string FeatureCode => "ULH";
		public override string FeatureName => "Ecommerce - HVLV US Low Value Entries";
		public override string WhereClause => "ulh.ULH_UseCode = 'HVL' AND ce.CE_EntryNum <> ''";
		public override bool UsedInBilling => false;
	}

	#endregion
}
