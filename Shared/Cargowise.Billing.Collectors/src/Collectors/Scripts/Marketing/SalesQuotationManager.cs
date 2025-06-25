namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion

	public class SalesQuotationManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "SQM";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Quotation Manager";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "th.TH_SystemCreateTimeUtc";
		public override string BillingReference1 => "'Header: ' + th.TH_QuoteNumber";
		public override string GuidReference => "th.TH_PK";
		public override string CreatingUserCode => "th.TH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			RatingHeader th
			INNER JOIN dbo.GlbCompany gc ON th.TH_GC = gc.GC_PK";
		public override string WhereClause => @"
			th.TH_RateType = 'QTE'
			AND th.TH_IsCancelled = 0
			AND th.TH_OneTimeQuote = 0";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.11.29.231";
	}

	#endregion
}
