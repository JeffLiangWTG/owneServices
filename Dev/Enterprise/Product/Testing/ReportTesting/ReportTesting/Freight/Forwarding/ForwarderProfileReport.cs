namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.ReportTesting;

	[TemplateName("Forwarder Profile Report")]
	public class TestForwarderProfileTemplate : TemplateTestCase
	{
	}

	public class TestForwarderProfileReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Forwarder Profile"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Organization – Forwarder Profile Report lists all Organizations with ""Forwarder"" ticked on their main Details Tab. By Organization, this report lists all information currently entered on the Forwarder Tab.

This report is designed for use to preview or email an XLS file to yourself.  As the volume of the information entered on the organization profile does not suit printing, it is best to use Excel’s data-sort and auto-filter functions.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestForwarderProfileTemplate();
		}
	}
}
