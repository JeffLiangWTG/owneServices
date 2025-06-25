namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVItemUsageByCategory : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "HUC";
		public override string RoleName => "Ecommerce Usage Categories";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Usage Counts";
		public override string FeatureName => "HVLV Item Usage";
		public override string GuidReference => "HVI_PK";
		public override string BillingReference1 => "HXU_Category";
		public override string BillingReference2 => "HXU_Code";
		public override string BillingReference3 => "HVI_CurrentBarcode";
		public override string BillingReference4 => "HVC_RN_NKConsigneeCountryCode";
		public override string TransactionDateUtc => "HXU_UsageTimeUtc";
		public override string BranchCode => "HXU_BranchCode";
		public override string CompanyCode => "HXU_GC_NKCompany";
		public override string FromClause => @"dbo.HVLVUsage INNER JOIN dbo.HVLVItem ON HVLVUsage.HXU_HVI_ParentItem = HVLVItem.HVI_PK INNER JOIN dbo.HVLVConsignment ON HVLVItem.HVI_HVC_Consignment = HVLVConsignment.HVC_PK";
		public override string WhereClause => "";
		public override bool UsedInBilling => false;
		public override string MinCW1Version => "23.7.28.391";
	}

	#endregion
}
