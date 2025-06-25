namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class BookingConsolidationPrintedAdvice : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "BKA";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Booking";
		public override string FunctionName => "Unversal Booking  System (Transport Booking)";
		public override string FeatureName => "Inward eBooking / Inbound Shipment / Consignment or Job Registration (Prt)";
		public override string TransactionDateUtc => "km.KM_SystemCreateTimeUtc";
		public override string BillingReference1 => "km.KM_JobID";
		public override string GuidReference => "km.KM_PK";
		public override string CreatingUserCode => "km.KM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					DtbBooking km
					INNER JOIN dbo.DtbBookingConsolidation kb ON kb.KB_PK = km.KM_KB_Booking";
		public override string WhereClause => @"
					kb.KB_JobType = 'BKG'
					AND kb.KB_IsOverridden = 0";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
