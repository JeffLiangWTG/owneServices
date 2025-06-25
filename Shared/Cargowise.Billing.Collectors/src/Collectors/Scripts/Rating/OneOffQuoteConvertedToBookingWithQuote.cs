namespace CargoWise.Billing.Collectors.Rating
{
	#region SuppressResourceStringsCheckRegion

	public class OneOffQuoteConvertedToBookingWithQuote : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "OOB";
		public override string RoleName => "Rating";
		public override string ModuleName => "One Off Quote";
		public override string FunctionName => "One off quote to Booking with quote Count";
		public override string FeatureName => "One off quote to Booking with quote Count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => string.Empty;
		public override string TransactionDateUtc => $"DATEFROMPARTS(YEAR({Constants.EndDateTimeExclusiveParamName}), MONTH({Constants.EndDateTimeExclusiveParamName}), DAY({Constants.EndDateTimeExclusiveParamName}))";
		public override string BillingReference1 => string.Empty;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.EndDateTimeExclusiveParamName})*10000) + (DATEPART(month, {Constants.EndDateTimeExclusiveParamName})*100) + DATEPART(day, {Constants.EndDateTimeExclusiveParamName})as varbinary(16)) as uniqueidentifier)";
		public override string ActiveOn => "ALL";
		/*
			When we create a booking with a quote (BWQ), we add one record in the RatingHeader table and one record in the JobShipment table.
			For one-off quotes (OOQ), we only add one record in the RatingHeader table. We already have stats for (OOQ + BWQ) and standalone OOQ.
			Later, if a client converts an OOQ to a BWQ, we add a new record in the JobShipment table.
			So, if a client creates an OOQ and converts it to a BWQ before the service task runs,
			we populate them in the OneOffQuoteCounter.cs (OBW - One Off Quote/Booking With Quote Count) and this function returns zero.
			If we create some OOQ and, after running the service task, convert them to BWQ, the next time the service task runs, this function will return the number of OOQs converted to BWQ.

			For example, if a client creates 1000 OOQs and 500 BWQs and then converts 100 OOQs to BWQs before the service task runs, we report:
			OBW - One Off Quote / Booking With Quote Count = 1500
			OOB - One off quote to Booking with quote Count = 0 (we cannot differentiate between OOQs converted to BWQs and BWQs)
			Then, after the service task runs, the client converts 300 OOQs to BWQs. The next time the service task runs, we report:
			OBW - One Off Quote / Booking With Quote Count = 0 (nothing new created)
			OOB - One off quote to Booking with quote Count = 300
		*/
		public override string PreparationScript => @"
WITH BookingWithQuoteOnly AS
(
    (
        SELECT
            th.*
        FROM
            dbo.RatingHeader th
            JOIN dbo.JobShipment js ON th.TH_PK = js.JS_TH_OneTimeQuote
        WHERE
            th.TH_RateType = 'QTE'
            AND th.TH_OneTimeQuote = 1
            AND js.JS_SystemCreateTimeUtc >= @StartDateTimeInclusive
            AND js.JS_SystemCreateTimeUtc < @EndDateTimeExclusive
    )
    EXCEPT
    (
        SELECT
            th.*
        FROM
            dbo.RatingHeader th
        WHERE 
            th.TH_RateType = 'QTE'
            AND th.TH_OneTimeQuote = 1
            AND th.TH_SystemCreateTimeUTC >= @StartDateTimeInclusive
            AND th.TH_SystemCreateTimeUTC < @EndDateTimeExclusive
    )
)";

		public override string FromClause => @"
			(BookingWithQuoteOnly th
			left JOIN dbo.GlbCompany gc on gc.GC_PK = TH.TH_GC)";

		public override string WhereClause => $@"th.TH_IsCancelled  = 0 GROUP BY gc.gc_code";

		public override string TransactionCount => "COUNT(DISTINCT th.TH_PK)";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
