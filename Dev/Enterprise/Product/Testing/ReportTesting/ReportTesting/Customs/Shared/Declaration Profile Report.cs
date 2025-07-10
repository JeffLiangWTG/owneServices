namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("Declaration Profile Report")]
	public class TestDeclarationProfileReport : TemplateTestCase
	{
	}

	public class TestDeclarationProfileReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Declaration Profile Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Declaration Profile Report produces a list of all Customs Declarations matching the selection criteria.
This report is similar to the 'Export Grid' functionality available from the Customs Declaration screen.
It differs in that:
a) not all columns from that export grid are included here.
b) job profits (REV, WIP, CST & ACR) totals are included.
c) report is pre-formatted for use in excel.

This report is designed for use in Excel.
It produces a detailed table of data profiling each Declaration. This report is best used as an Excel file, not as a printed  document.
The layout of this report is compatible with using Excel’s sort, auto filter, subtotal functions.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestDeclarationProfileReport();
		}
	}
}
