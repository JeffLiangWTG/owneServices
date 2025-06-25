namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationSpecialEntryLandedCosting : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "LCH";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string FeatureName => "Landed Costing";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "lt.LT_DateOfEntry";
		public override string BillingReference1 => "lt.LT_LandedCostType";
		public override string BillingReference2 => "CASE WHEN je.JE_DeclarationReference is null THEN 'Ord.' + jd.JD_OrderNumber ELSE 'Dec.' + je.JE_DeclarationReference END";
		public override string GuidReference => "lt.LT_PK";
		public override string CreatingUserCode => "COALESCE(je.JE_SystemCreateUser, jd.JD_SystemCreateUser)";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					LandedCostHeader lt
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = lt.LT_GC
					LEFT JOIN dbo.JobDeclaration je ON je.JE_PK = lt.LT_ParentID
					LEFT JOIN dbo.JobOrderHeader jd ON jd.JD_PK = lt.LT_ParentID";
		public override string WhereClause => "(je.JE_PK is not null OR jd.JD_PK is not null)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;

		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.11.21.50";
	}
	#endregion
}
