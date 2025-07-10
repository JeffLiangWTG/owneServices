namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.Customs
{
	public class PNTSManifestScript : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "TSD";

		public override string FeatureName => "EU Presentation Notification and Temporary Storage";

		public override string TransactionDateUtc => "AMA_SystemCreateTimeUtc";

		public override string GuidReference => "AMA_PK";

		public override string FromClause => @"AsycudaManifestHeader  
					inner join GlbBranch on GB_PK = AMA_GB 
					inner join GlbCompany on GC_PK = GB_GC
					left join AsycudaBill m 
					on m.ABL_AMA = AMA_PK 
					and m.ABL_BolType = 'BOL'
					";

		public override string RoleName => "Customs & Country Specific Integrations";

		public override string ModuleName => "Customs";

		public override string FunctionName => "Customs & Other Government Communication";

		public override string CompanyCode => "GC_Code";

		public override string BranchCode => "GB_Code";

		public override string BillingReference1 => "'Job Reference:' + AMA_JobReference";
		public override string BillingReference2 => "'Master Bill:' + ABL_BillNumber";

		public override string WhereClause => @"AMA_ApplicationCode = 'STO'";
	}
}
