namespace CargoWise.Billing.Collectors.Rating
{
	#region SuppressResourceStringsCheckRegion

	public class StandaloneOneOffQuoteCounter : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "OOQ";
		public override string RoleName => "Rating";
		public override string ModuleName => "One Off Quote";
		public override string FunctionName => "Un-used One Off Quote Count";
		public override string FeatureName => "Un-used One Off Quote Count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => string.Empty;
		public override string TransactionDateUtc => "th.TH_SystemCreateTimeUtc";
		public override string BillingReference1 => "'Header: ' + th.TH_QuoteNumber";
		public override string GuidReference => "th.TH_PK";
		public override string CreatingUserCode => "th.TH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				([RatingHeader] th
				left JOIN dbo.GlbCompany gc on gc.GC_PK = TH.TH_GC)";

		public override string WhereClause => @"
				th.TH_RateType = 'QTE' And
				th.TH_OneTimeQuote = 1 And
				th.TH_PK not in (SELECT JS_TH_OneTimeQuote FROM [JobShipment] where JS_TH_OneTimeQuote is not null)";

		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
