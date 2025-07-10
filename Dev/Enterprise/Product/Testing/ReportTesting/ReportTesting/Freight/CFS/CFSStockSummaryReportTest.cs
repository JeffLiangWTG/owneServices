namespace Enterprise.ReportTesting.Freight.CFS
{
	using System.Collections.Generic;
	using Enterprise.Freight.CFS.Module;

	[TemplateName("CFS Stock Summary Report")]
	public class TestCFSStockSummaryReportTest : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Template"
				};
			}
		}
	}

	public class TestCFSStockSummaryReportTestMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CFSCTOReports(); }
		}

		public override string MenuName
		{
			get { return "Stock Summary"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Stock Summary Report shows export shipments not packed or gate passed.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCFSStockSummaryReportTest();
		}
	}
}
