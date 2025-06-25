namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class AgencyBooking : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "BKG";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Liner and Agency";
		public override string FunctionName => "General Liner/Ships Agent Engine";
		public override string FeatureName => "Booking/Bill of Lading (Booking)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jh.JH_SystemCreateTimeUtc";
		public override string CreatingUserCode => "jh.JH_SystemCreateUser";
		public override string GuidReference => "jh.JH_PK";
		public override string BillingReference1 => "jh.JH_JobNum";
		public override string BillingReference2 => "js.JS_UniqueConsignRef";
		public override string FromClause => @"
					JobHeader jh
					INNER JOIN dbo.JobShipment js ON js.JS_PK = jh.JH_ParentID
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jh.JH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					js.JS_IsForwardRegistered = 0
					AND js.JS_IsShipping = 1
					AND js.JS_ShipmentStatus in ('WEB', 'BKD', 'WTL')";
		public override string MinCW1Version => "22.11.8.377";
	}
	#endregion
}
