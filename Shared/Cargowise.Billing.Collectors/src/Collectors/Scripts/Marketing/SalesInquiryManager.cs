namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesInquiryManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "ICS";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Inquiry: Create - Success";
		public override string TransactionDateUtc => "o1.O1_SystemCreateTimeUtc";
		public override string GuidReference => "o1.O1_PK";
		public override string CreatingUserCode => "o1.O1_SystemCreateUser";
		public override string BillingReference1 => "o1.O1_LeadUniqueReference";
		public override string BillingReference2 => "o1.O1_EnquiryType";
		public override string BillingReference3 => "CASE o1.O1_SystemCreateUser WHEN 'ZZ' THEN 'WEB' ELSE 'CW1' END";
		public override string BillingReference4 => "'HasNotes=' + CASE WHEN ST_PK is null THEN 'N' ELSE 'Y' END + '|HasLeadSource=' + CASE WHEN O1_LeadSource <> '' OR O1_OH_SourceOfLead IS NOT NULL OR O1_OpportunitySourceDetails <> '' OR O1_OC_ReferringContact IS NOT NULL THEN 'Y' ELSE 'N' END + '|HasDetails=' + CASE WHEN O1_GS_NKRepAssigned <> '' OR O1_LeadCalledDate IS NOT NULL OR O1_InterestLevel <> '' OR O1_CloseReason <> '' THEN 'Y' ELSE 'N' END ";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				OrgColdCallRegister o1
				left join dbo.stmnote st on ST_ParentID = O1_PK and ST_IsCustomDescription = 0 and ST_Description = 'Blob Details'
			";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
	}
	#endregion // SuppressResourceStringsCheckRegion
}
