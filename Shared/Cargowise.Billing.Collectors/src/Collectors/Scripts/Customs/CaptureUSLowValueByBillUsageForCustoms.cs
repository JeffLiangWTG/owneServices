namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class CaptureUSLowValueByBillUsageForCustoms : CaptureUSLowValueByBillUsage
	{
		public override string FeatureCode => "ULC";
		public override string FeatureName => "Ecommerce - Customs US Low Value Entries";
		public override string WhereClause => "ulh.ULH_UseCode <> 'HVL' AND ce.CE_EntryNum <> ''";
		public override bool UsedInBilling => true;
	}

	#endregion
}
