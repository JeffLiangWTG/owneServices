namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class AgencyContainerOceanTracking : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "OCK";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Liner and Agency";
		public override string FunctionName => "Container Management";
		public override string FeatureName => "Ocean Container Tracking";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "em.EM_SystemCreateTimeUtc";
		public override string CreatingUserCode => "em.EM_SystemCreateUser";
		public override string GuidReference => "em.EM_PK";
		public override string BillingReference1 => "'MSG#: ' + em.EM_MessageNum";
		public override string BillingReference2 => "jcm.E9_MovementType";
		public override string BillingReference3 => "jcm.E9_MovementDate";
		public override string BillingReference4 => "rcs.R6_ContainerNum";
		public override string FromClause => @"
					JobContainerMove jcm
					INNER JOIN dbo.EDIMessage em ON em.EM_LinkUniqueID = jcm.E9_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = em.EM_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					INNER JOIN dbo.RefContainerStock rcs ON rcs.R6_PK = jcm.E9_R6";
		public override string WhereClause => @"
					em.EM_ApplicationCode = 'CMG' 
					AND em.EM_Status = 'RKN'
					AND em.EM_ReceiveTransmit = 'RCV'
					AND em.EM_IsActive = 1";
		public override string MinCW1Version => "22.11.8.377";
	}
	#endregion
}
