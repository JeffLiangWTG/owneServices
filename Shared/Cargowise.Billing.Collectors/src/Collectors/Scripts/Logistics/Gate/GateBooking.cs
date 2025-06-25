namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class GateBooking : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "GTB";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "ContainerFreightStation";
		public override string FunctionName => "Gate Module";
		public override string FeatureName => "Gate Booking";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "[GateBooking].[GTB_SystemCreateTimeUtc]";
		public override string BillingReference1 => "[GateBooking].[GTB_BookingNumber]";
		public override string BillingReference2 => "[GateBooking].[GTB_JobReference]";
		public override string GuidReference => "[GateBooking].[GTB_PK]";
		public override string ActiveOn => "ALL";
		public override string FromClause => "[GateBooking] INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = [GateBooking].[GTB_GB_Branch]	INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC ";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
