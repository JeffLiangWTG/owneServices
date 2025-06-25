namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class LandTransportKilogramsMoved : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "LTK";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Port Transport";
		public override string FeatureName => "Local Transport Engine (Kilograms Moved)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jj.JJ_SystemCreateTimeUtc";
		public override string BillingReference1 => "jj.JJ_ConsignmentID";
		public override string GuidReference => "jj.JJ_PK";
		public override string CreatingUserCode => "jj.JJ_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobCartage jj
					INNER JOIN dbo.JobBookedCtgMove ew ON ew.EW_JJ = jj.JJ_PK
					INNER JOIN dbo.JobContainer jc ON jc.JC_PK = ew.EW_JC_Container
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jj.JJ_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					CROSS APPLY dbo.ConvertWeight(jc.JC_GrossWeight - jc.JC_TareWeight, jc.JC_GrossWeightUQ, 'KG') WeightKg";
		public override string WhereClause => @"
					jc.JC_GrossWeight > jc.JC_TareWeight
					AND WeightKg.Value <= 2147483647";
		public override string TransactionCount => "WeightKg.Value";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.29.221";
	}

	#endregion
}
