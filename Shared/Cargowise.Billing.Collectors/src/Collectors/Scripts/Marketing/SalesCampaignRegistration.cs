namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesCampaignRegistration : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CMR";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Campaign Manager - Campaign Registration";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "g0.G0_SystemCreateTimeUtc";
		public override string BillingReference1 => "g0.G0_CampaignName";
		public override string GuidReference => "g0.G0_PK";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT
					CampaignType = g0.G0_BroadcastVoteSurveyExam,
					CampaignStage = g0.G0_Stage,
					MediaCategory = g0.G0_Category,
					TouchCount = case when g0.G0_BroadcastVoteSurveyExam in ('DRM', 'INS') then (select convert(varchar(5), count(*)) from dbo.GlbCompanyCampaign touch where touch.G0_G0_Master = g0.G0_PK) else 'N/A' end,
					MediaType = g0.G0_Type,
					MailSender = g0.G0_EmailSenderOption,
					HasCustomFields = case when exists (select null from dbo.GenCustomAddOnValue where XV_ParentTableCode = 'G0' and XV_ParentID = g0.G0_PK) then 'Y' else 'N' end,
					eDocs = case when G0_AttachmentList <> '' then 'Y' else 'N' end,
					HasBudget = case when exists (select null from dbo.GlbCompanyCampaignBudgetItem where G9_G0 = g0.G0_PK) then 'Y' else 'N' end
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string CreatingUserCode => "g0.G0_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					GlbCompanyCampaign g0
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = g0.G0_GC";
		public override string WhereClause => @"
					g0.G0_BroadcastVoteSurveyExam != 'LCT'
					AND g0.G0_G0_Master IS NULL";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.11.29.231";
	}
	#endregion
}
