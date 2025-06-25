namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVItemPlusUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "HVP";
		public override string RoleName => "Ecommerce Plus";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Standard & Security Functions";
		public override string FeatureName => "Ecommerce Plus";
		public override string CompanyCode => "item.HXU_GC_NKCompany";
		public override string BranchCode => "item.HXU_BranchCode";
		public override string TransactionDateUtc => "item.HXU_UsageTimeUtc";
		public override string BillingReference1 => "item.HVI_ItemId";
		public override string BillingReference2 => "item.HVI_CurrentBarcode";
		public override string BillingReference3 => "'V2'";
		public override string BillingReference4 => "item.HVI_ShipperReference";
		public override string GuidReference => "item.HVI_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"(SELECT HVLVItem.HVI_ItemId,
								HVLVItem.HVI_CurrentBarcode,
								HVLVItem.HVI_ShipperReference,
								HVLVItem.HVI_PK,
								HVLVConsignmentHeader.HCH_UsageType,	
								HVLVUsage.HXU_GC_NKCompany,
								HVLVUsage.HXU_BranchCode,
								HVLVUsage.HXU_Code,
								HVLVUsage.HXU_Category,
								HVLVUsage.HXU_UsageTimeUtc
								FROM HVLVItem
								INNER JOIN HVLVConsignmentHeader ON HVLVConsignmentHeader.HCH_ClusterKey = HVLVItem.HVI_ClusterKey
								INNER JOIN HVLVUsage ON HVLVUsage.HXU_HVI_ParentItem = HVLVItem.HVI_PK) AS item";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => "((HXU_Category ='CW1' and HXU_Code = 'CWU') or HXU_Code = 'GLS' or HXU_Code = 'GLC') AND item.HCH_UsageType = 'P'";
		public override string MinCW1Version => "24.4.19.213";
	}

	#endregion
}
