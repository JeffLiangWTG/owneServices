namespace CargoWise.Billing.Collectors.Rating
{
	#region SuppressResourceStringsCheckRegion

	public class OneOffQuoteLinkedToBookingWithQuote : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "OBQ";
		public override string RoleName => "Rating";
		public override string ModuleName => "One Off Quote";
		public override string FunctionName => "Linked to Booking With Quote Count";
		public override string FeatureName => "Linked to Booking With Quote Count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => string.Empty;
		public override string TransactionDateUtc => "th.TH_SystemLastEditTimeUtc";
		public override string BillingReference1 => "'Header: ' + th.TH_QuoteNumber";
		public override string GuidReference => "th.TH_PK";
		public override string CreatingUserCode => "th.TH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			([RatingHeader] th
				left JOIN dbo.GlbCompany gc on gc.GC_PK = TH.TH_GC)
			join [JobHeader] jh on th.TH_QuoteNumber = jh.JH_TH_NKQuoteNumber
			join [JobShipment] js on jh.JH_ParentID = js.JS_TH_OneTimeQuote";

		public override string WhereClause => @"
			th.TH_IsOneOffQuoteConsumed = 1 And
			jh.JH_ParentTableCode ='TH' And
			th.TH_PK not in (Select JS_TH_OneTimeQuote from [JobShipment] where JS_TH_OneTimeQuote is not null)";

		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
