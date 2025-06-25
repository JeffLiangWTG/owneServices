namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVConsignmentPreScreeningUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HPS";
		public override string RoleName => "Ecommerce Pre-Screening";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Pre-Screening";
		public override string FeatureName => "HVLV Pre-Screening";
		public override string CompanyCode => "''";
		public override string BranchCode => "''";
		public override string TransactionDateUtc => "StmALog.SL_PostedTimeUtc";
		public override string BillingReference1 => @"CASE WHEN SL_Table = 'HVLVBookingHeader' THEN HVH_BookingReference
WHEN SL_Table = 'JobShipment' THEN JS_UniqueConsignRef END";
		public override string BillingReference2 => "SL_Table";
		public override string GuidReference => "SL_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => "StmALog LEFT JOIN dbo.HVLVBookingHeader ON SL_Parent = HVH_PK LEFT JOIN dbo.JobShipment ON SL_Parent = JS_PK";
		public override string WhereClause => @"(SL_Table = 'HVLVBookingHeader' OR SL_Table = 'JobShipment')
AND CHARINDEX('|RES=Pre-Screened|TTL=', SL_Reference) > 0";
		public override string TransactionCount => "CONVERT(int, SUBSTRING(SL_Reference, CHARINDEX('|TTL=', SL_REFERENCE) + 5, LEN(SL_Reference)))";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
