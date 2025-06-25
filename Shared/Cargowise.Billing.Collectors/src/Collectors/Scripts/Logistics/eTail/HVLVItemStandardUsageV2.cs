namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVItemStandardUsageV2 : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HD2";
		public override string RoleName => "Ecommerce Standard";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Standard Functions";
		public override string FeatureName => "Ecommerce Standard V2";
		public override string CompanyCode => "item.HXU_GC_NKCompany";
		public override string BranchCode => "item.HXU_BranchCode";
		public override string TransactionDateUtc => "item.HXU_UsageTimeUtc";
		public override string BillingReference1 => "item.HVI_ItemId";
		public override string BillingReference2 => "item.HVI_CurrentBarcode";
		public override string BillingReference4 => "item.HVI_ShipperReference";
		public override string GuidReference => "item.HVI_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"(SELECT HVLVUsage.HXU_UsageTimeUtc,
								HVLVItem.HVI_ItemId,
								HVLVItem.HVI_CurrentBarcode,
								HVLVItem.HVI_ShipperReference,
								HVLVItem.HVI_PK,
								HVLVItem.HVI_UsageType,
								HVLVUsage.HXU_GC_NKCompany,
								HVLVUsage.HXU_Category,
								HVLVUsage.HXU_BranchCode,
								HVLVUsage.HXU_Code,
								HVLVConsignmentHeader.HCH_UsageType
								FROM HVLVItem
								INNER JOIN HVLVUsage ON HVLVUsage.HXU_HVI_ParentItem = HVLVItem.HVI_PK
								INNER JOIN HVLVConsignmentHeader ON HVLVConsignmentHeader.HCH_ClusterKey = HVLVItem.HVI_ClusterKey) AS item";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => "((HXU_Category = 'CW1' and HXU_Code = 'CWU') OR HXU_Code = 'GLS' OR HXU_Code = 'GLC') AND HCH_UsageType = 'S'";
		public override string MinCW1Version => "23.7.28.391";
	}

	#endregion

}
