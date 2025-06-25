namespace CargoWise.Billing.Collectors.Logistics
{
	public class HVLVConsignmentFHLAirlineMessaging : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "FH2";
		public override string RoleName => "HVLV Consignment FHL Airline Messaging";
		public override string ModuleName => "International Logistics";
		public override string FunctionName => "FHL Airline Messaging";
		public override string FeatureName => "HVLV Consignment FHL Airline Messaging";
		public override string GuidReference => "c.HVC_PK";
		public override string BillingReference1 => "c.HVC_ConsignmentId";
		public override string BillingReference2 => "c.HVC_WaybillNumber";
		public override string BranchCode => "c.BranchCode";
		public override string TransactionDateUtc => "c.MessageSendTime";
		public override string FromClause => @"(SELECT DISTINCT
										HVLVConsignment.HVC_PK,
										HVLVConsignment.HVC_ConsignmentId,
										HVLVConsignment.HVC_WaybillNumber,
										HVLVUsage.HXU_UsageTimeUtc AS MessageSendTime,
										HVLVUsage.HXU_GC_NKCompany AS CompanyCode,
										HVLVUsage.HXU_BranchCode AS BranchCode
									FROM dbo.HVLVConsignment
									JOIN dbo.HVLVItem ON HVI_HVC_Consignment = HVC_PK
									JOIN dbo.HVLVUsage ON HXU_HVI_ParentItem = HVI_PK
									WHERE HXU_Code = 'FHL') AS c";
		public override string WhereClause => string.Empty;
		public override string CompanyCode => "c.CompanyCode";
		public override bool UsedInBilling => false;
		public override string MinCW1Version => "23.7.28.391";
	}
}
