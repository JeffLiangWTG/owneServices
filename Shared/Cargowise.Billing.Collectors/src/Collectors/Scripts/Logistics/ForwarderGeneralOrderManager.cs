namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class ForwarderGeneralOrderManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "ORM";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "General Forwarding Engine";
		public override string FeatureName => "Order Manager";
		public override string TransactionDateUtc => "joh.JD_SystemCreateTimeUtc";
		public override string BillingReference1 => "joh.JD_OrderNumber";
		public override string GuidReference => "joh.JD_PK";
		public override string CreatingUserCode => "joh.JD_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @" (
SELECT JD_PK,
	JD_SystemCreateTimeUtc,
	JD_OrderNumber,
	JD_SystemCreateUser,
	JD_GB_NKBranch = (SELECT TOP 1 SL_GB_NKBranch
						FROM dbo.StmALog WITH(FORCESEEK, INDEX(NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime))
						WHERE	SL_Parent = JD_PK
								AND SL_SE_NKEvent = 'ADD'
								AND SL_Table = 'JobOrderHeader'
								AND SL_PostedTimeUtc >= DATEADD(MI, -5, @StartDateTimeInclusive)
								AND SL_PostedTimeUtc < DATEADD(MI, 5, @EndDateTimeExclusive))
FROM dbo.JobOrderHeader WITH(FORCESEEK, INDEX(NR_RC__JD_SystemCreateTimeUtc))) joh
LEFT JOIN dbo.GlbBranch branch ON branch.GB_Code = joh.JD_GB_NKBranch
LEFT JOIN dbo.GlbCompany company ON company.GC_PK = GB_GC";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => "company.GC_Code";
		public override string BranchCode => "branch.GB_Code";
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "23.1.20.174";
	}
	#endregion
}
