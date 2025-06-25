namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class OceanBookingStatusMessaging : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "INT";
		public override string RoleName => "Transaction and Content Services";
		public override string ModuleName => "Elements";
		public override string FunctionName => "Messaging and Electronic Submission";
		public override string FeatureName => "Ocean Booking/SI/Status Messaging (via INTTRA/GTNexus/Other)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "em.EM_SystemCreateTimeUtc";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "em.EM_MessageNum";
		public override string GuidReference => "em.EM_PK";
		public override string CreatingUserCode => "em.EM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobConsol jk
					INNER JOIN dbo.EDIMessage em on em.EM_LinkUniqueID = jk.JK_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = em.EM_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					em.EM_ApplicationCode = 'INT'
					AND em.EM_ReceiveTransmit = 'TRX'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.13.467";
	}

	#endregion
}
