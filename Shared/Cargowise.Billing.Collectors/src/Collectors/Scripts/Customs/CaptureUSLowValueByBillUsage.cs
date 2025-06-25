namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	abstract public class CaptureUSLowValueByBillUsage : RefStlScriptWithDefaults
	{
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Ecommerce US Low Value Entries";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string GuidReference => "ulb.ULB_PK";
		public override string BillingReference1 => "ulh.ULH_JobNumber";
		public override string BillingReference2 => "ce.CE_EntryNum";
		public override string BillingReference3 => "ulb.ULB_HouseBill";
		public override string BillingReference4 => "ulh.ULH_UseCode";
		public override string CreatingUserCode => "ce.CE_SystemCreateUser";
		public override string FromClause => @"
				CusUSLVConsignment ulb
				INNER JOIN dbo.CusUSLVClearance ulh ON ulh.ULH_PK = ulb.ULB_ULH
				INNER JOIN dbo.CusEntryNum ce ON ce.CE_ParentID = ulb.ULB_PK AND ce.CE_EntryType = 'ENS' AND ce.CE_RN_NKCountryCode = 'US' AND ce.CE_Category = 'CUS'
				INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ulh.ULH_GB
				INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
	}

	#endregion
}
