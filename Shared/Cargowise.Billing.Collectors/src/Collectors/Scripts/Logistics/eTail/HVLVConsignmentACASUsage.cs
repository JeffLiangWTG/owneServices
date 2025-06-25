namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVConsignmentACASUsage : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "HAS";
		public override string RoleName => "Ecommerce Air Cargo Advance Screening";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Air Cargo Advance Screening";
		public override string FeatureName => "HVLV Air Cargo Advance Screening";
		public override string GuidReference => "c.HVC_PK";
		public override string BillingReference1 => "c.HVC_ConsignmentId";
		public override string BillingReference2 => "c.HVC_WaybillNumber";
		public override string BranchCode => "c.BranchCode";
		public override string TransactionDateUtc => "c.MessageSendTime";
		public override string FromClause => @"(SELECT DISTINCT
										HVLVConsignment.HVC_PK,
										HVLVConsignment.HVC_ConsignmentId,
										HVLVConsignment.HVC_WaybillNumber,
										HVLVConsignment.HVC_ACASMessageStatus,
										HVLVUsage.HXU_UsageTimeUtc AS MessageSendTime,
										HVLVUsage.HXU_GC_NKCompany AS CompanyCode,
										HVLVUsage.HXU_BranchCode AS BranchCode
									FROM dbo.HVLVConsignment
									JOIN dbo.HVLVItem ON HVI_HVC_Consignment = HVC_PK
									JOIN dbo.HVLVUsage ON HXU_HVI_ParentItem = HVI_PK
									WHERE HXU_Code = 'ACA') AS c";
		public override string WhereClause => $@"c.HVC_ACASMessageStatus <> ''";
		public override bool UsedInBilling => false;
		public override string CompanyCode => string.Empty;
		public override string MinCW1Version => "23.7.28.391";
	}

	#endregion
}
