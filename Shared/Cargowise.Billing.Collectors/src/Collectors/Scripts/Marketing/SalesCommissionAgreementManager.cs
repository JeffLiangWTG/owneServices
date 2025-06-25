namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesCommissionAgreementManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "CAC";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "CommissionAgreement: Create - Success";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "ca.CA0_SystemCreateTimeUtc";
		public override string CreatingUserCode => "ca.CA0_SystemCreateUser";
		public override string GuidReference => "ca.CA0_PK";
		public override string BillingReference1 => "p8.P8_OpportunityID";
		public override string BillingReference2 => "ca.CA0_Name";
		public override string BillingReference3 => @"
				concat('ItemCount=', caStats.ItemCount,
					'|ModeCount=', caStats.ModeCount)";
		public override string BillingReference4 => @"
				concat('CATrigger=', ca.CA0_CommissionTriggerType,
					'|WolfPackCount=', caStats.WolfPackCount,
					'|RatesCount=', caStats.RatesCount)";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				OrgCommissionAgreement ca
				INNER JOIN dbo.OrgOpportunity p8 ON p8.P8_PK = ca.CA0_P8
				INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = p8.P8_GC
				INNER JOIN (
					SELECT ca.CA0_PK as AgreementPK, count(distinct cai.CAI_PK) as ItemCount, count(distinct cic.CIC_PK) as ModeCount, count(distinct car.CAR_PK) as WolfPackCount, count(distinct cat.CAT_PK) as RatesCount
					FROM dbo.OrgCommissionAgreement ca
					LEFT JOIN dbo.OrgCommissionAgreementItem cai ON cai.CAI_ParentID = ca.CA0_PK AND cai.CAI_ParentTableCode = 'CA0'
					LEFT JOIN dbo.OrgCommissionAgreementItemCondition cic ON cic.CIC_CAI = cai.CAI_PK
					LEFT JOIN dbo.OrgCommissionAgreementRecipient car ON car.CAR_CA0 = ca.CA0_PK
					LEFT JOIN dbo.OrgCommissionAgreementRecipientRate cat ON cat.CAT_CAR = car.CAR_PK
					GROUP BY ca.CA0_PK
				) caStats ON caStats.AgreementPK = ca.CA0_PK";

		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
	}
	#endregion // SuppressResourceStringsCheckRegion
}
