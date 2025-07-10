namespace Enterprise.ReportTesting.Customs.AU
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("AU Customs Entry Payment")]
	public class AUCustomsEntryPaymentTemplateTest : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			var sheetsToExclude = base.GetExcludedSheetNames();
			sheetsToExclude.Add("Payment SUMMARY");
			return sheetsToExclude;
		}
	}

	public class AUCustomsEntryPaymentReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "AU Customs Entry Payment Report";

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "CTY=AU");

		public override string Hint =>
			@"The report produces a listing of payments on AU Declaration Entries.

For a specified date range the report identifies declaration payment details including branch, job, broker, entry type and style, debtor, importer, supplier and various reference and payment details.  Filter options include Branch, Debtor Type, Importer, Supplier and declaration types and a range of journey / transport filters.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AUCustomsEntryPaymentTemplateTest();
		}
	}
}
