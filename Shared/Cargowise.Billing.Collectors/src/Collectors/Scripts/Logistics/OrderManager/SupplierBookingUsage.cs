namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class SupplierBookingUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "SBG";
		public override string ModuleName => "Order Manager";
		public override string RoleName => "Order Manager Usage";
		public override string FunctionName => "Supplier Booking Usage Collectors";
		public override string FeatureName => "Order Manager Supplier Booking Usage Collector";
		public override string TransactionDateUtc => "log.SL_PostedTimeUtc";
		public override string BillingReference1 => "supplierBooking.JSB_BookingId";
		public override string GuidReference => "supplierBooking.JSB_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					StmALog log
					JOIN dbo.JobSupplierBooking supplierBooking ON supplierBooking.JSB_PK = log.SL_Parent
					LEFT JOIN dbo.GlbBranch globalBranch ON globalBranch.GB_Code = log.SL_GB_NKBranch
					LEFT JOIN dbo.GlbCompany globalCompany ON globalCompany.GC_PK = globalBranch.GB_GC";
		public override string WhereClause => "log.SL_SE_NKEvent = 'ADD'";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string DateType => RefStlDateType.DateTime;
		public override string CreatingUserCode => "log.SL_GS_NKUser";
		public override string BranchCode => "globalBranch.GB_Code";
		public override string CompanyCode => "globalCompany.GC_Code";
	}

	#endregion
}
