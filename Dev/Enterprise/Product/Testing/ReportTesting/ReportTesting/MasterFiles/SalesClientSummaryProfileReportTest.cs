namespace Enterprise.ReportTesting.MasterFiles
{
	using Enterprise.ReportTesting;

	[TemplateName("Sales Client Summary Profile Report")]
	public class TestSalesClientSummaryProfileTemplate : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestSalesClientSummaryProfileReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Sales Client Summary"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Organization – Sales Client Summary Report lists all Organizations with " + "\"Sales\"" + @" ticked on their main Details Tab. By Organization, this report lists all information currently entered on the Sales Client Summary Tab.

This report is designed for use to preview or email an XLS file to yourself.  As the volume of the information entered on the organization profile does not suit printing, it is best to use Excel’s data-sort and auto-filter functions.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestSalesClientSummaryProfileTemplate();
		}
	}
}
