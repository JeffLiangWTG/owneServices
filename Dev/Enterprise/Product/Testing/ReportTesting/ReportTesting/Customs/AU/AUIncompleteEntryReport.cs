namespace Enterprise.ReportTesting.Customs.AU
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("Incomplete Entry Report")]
	public class AUIncompleteEntryReportTemplateTest : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			var result = base.GetExcludedSheetNames();
			result.Add("Template");
			return result;
		}
	}

	public class AUIncompleteEntryReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Incomplete Entry Report";

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "CTY=AU");

		public override string Hint => @"The entries that their Customs Status is not Finalised are printed by this report.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AUIncompleteEntryReportTemplateTest();
		}
	}
}
