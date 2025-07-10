namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Hot Cheques Transaction Listing Report")]
	public class TestHotChequeListingTemplate : TemplateTestCase
	{
	}

	public class TestHotChequeListingReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Hot Checks Transaction Listing"; }
		}

		public override string Hint
		{
			get
			{
				return
@"The Hot Check Transaction Listing Report will list hot checks and their associated details.
By default, this report only lists active hot checks.
Filter options allow you to optionally include Posted and Canceled hot checks in the report.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestHotChequeListingTemplate();
		}
	}
}
