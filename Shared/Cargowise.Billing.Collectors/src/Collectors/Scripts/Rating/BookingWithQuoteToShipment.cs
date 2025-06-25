namespace CargoWise.Billing.Collectors.Rating
{
	#region SuppressResourceStringsCheckRegion

	public class BookingWithQuoteToShipment : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "OTS";
		public override string RoleName => "Rating";
		public override string ModuleName => "One Off Quote";
		public override string FunctionName => "Booking with Qoute Converted to Shipment";
		public override string FeatureName => "Booking with Qoute to Shipment Count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => string.Empty;
		public override string TransactionDateUtc => "js.JS_SystemCreateTimeUtc";
		public override string BillingReference1 => "js.JS_UniqueConsignRef";
		public override string GuidReference => "js.JS_PK";
		public override string CreatingUserCode => "js.JS_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			[JobShipment] js
			join ([RatingHeader] th
					left JOIN dbo.GlbCompany gc on gc.GC_PK = TH.TH_GC
				) on js.JS_TH_OneTimeQuote = th.TH_PK";

		public override string WhereClause => @"
			js.JS_IsBooking = 1 And
			js.JS_IsForwardRegistered = 1 And
			js.JS_TH_OneTimeQuote is not null And
			js.JS_PK not in (Select JN_JS from [JobConShipLink])";

		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
