namespace CargoWise.Billing.Collectors.Rating
{
	#region SuppressResourceStringsCheckRegion

	public class RatingHeaders : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "RHC";
		public override string RoleName => "Rating";
		public override string ModuleName => "Rating Headers";
		public override string FunctionName => "Rating Headers Count";
		public override string FeatureName => "Rating Headers Count";
		public override string CompanyCode => "RatingHeaders.CompanyCode";
		public override string BranchCode => string.Empty;
		public override string TransactionDateUtc => $"DATEFROMPARTS(YEAR({Constants.EndDateTimeExclusiveParamName}), MONTH({Constants.EndDateTimeExclusiveParamName}), DAY({Constants.EndDateTimeExclusiveParamName}))";
		public override string BillingReference1 => "RatingHeaders.RateType";
		public override string BillingReference2 => "RatingHeaders.IsAccepted";
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.EndDateTimeExclusiveParamName})*10000) + (DATEPART(month, {Constants.EndDateTimeExclusiveParamName})*100) + DATEPART(day, {Constants.EndDateTimeExclusiveParamName})as varbinary(16)) as uniqueidentifier)";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			(
				SELECT	gc.GC_Code CompanyCode,
						CASE WHEN th.TH_Accepted is not null THEN 'True' ELSE 'False' END IsAccepted,
						th.TH_RateType RateType,
						Count(*) RatingHeadersCount
				FROM dbo.RatingHeader th
				LEFT JOIN dbo.GlbCompany gc on gc.GC_PK = TH.TH_GC
				WHERE th.TH_IsCancelled = 0 AND th.TH_SystemCreateTimeUTC >=  @StartDateTimeInclusive AND th.TH_SystemCreateTimeUTC < @EndDateTimeExclusive
				GROUP BY gc.GC_Code, th.TH_RateType, th.TH_Accepted
			) RatingHeaders";

		public override string TransactionCount => "RatingHeaders.RatingHeadersCount";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
	}

	#endregion
}
