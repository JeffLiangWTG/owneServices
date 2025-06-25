namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVCustomsDeclarationUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HCD";
		public override string RoleName => "Ecommerce Customs Declarations";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Forwarding and Customs Functions";
		public override string FeatureName => "HVLV Consignment Customs Declaration";
		public override string CompanyCode => "''";
		public override string BranchCode => "GlbBranch.GB_Code";
		public override string TransactionDateUtc => "DeclaredConsignment.JE_SystemCreateTimeUtc";
		public override string BillingReference1 => "HVC_ConsignmentId";
		public override string GuidReference => "HVC_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"(SELECT HVLVConsignment.HVC_PK, HVLVConsignment.HVC_ConsignmentId, JobDeclaration.JE_SystemCreateTimeUtc, JobDeclaration.JE_GB
	FROM HVLVConsignment
	INNER JOIN dbo.JobDeclaration
		ON JobDeclaration.JE_PK = HVLVConsignment.HVC_JE_ImportDeclaration
	UNION
	SELECT HVLVConsignment.HVC_PK, HVLVConsignment.HVC_ConsignmentId, JobDeclaration.JE_SystemCreateTimeUtc, JobDeclaration.JE_GB
	FROM HVLVConsignment
	INNER JOIN dbo.JobDeclaration
		ON JobDeclaration.JE_PK = HVLVConsignment.HVC_JE_ExportDeclaration) AS DeclaredConsignment
	LEFT JOIN dbo.GlbBranch
	ON DeclaredConsignment.JE_GB = GlbBranch.GB_PK";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "24.8.30.332";
	}

	#endregion
}
