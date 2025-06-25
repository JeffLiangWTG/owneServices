namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class ForwarderGeneralShipment : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "SHP";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "General Forwarding Engine";
		public override string FeatureName => "Shipment, Consol, Bookings, Spot Quotes (Forwarding)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jh.JH_SystemCreateTimeUtc";
		public override string BillingReference1 => "jh.JH_JobNum";
		public override string GuidReference => "jh.JH_PK";
		public override string CreatingUserCode => "jh.JH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobHeader jh
					INNER JOIN dbo.JobShipment js ON js.JS_PK = jh.JH_ParentID
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jh.JH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "(js.JS_IsForwardRegistered = 1 OR js.JS_IsBooking = 1)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.1.20.174";
	}
	#endregion
}
