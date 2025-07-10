namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("GST Summary Report")]
	public class TestGSTSummaryReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}

	public class TestGSTSummaryPayablesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "GST / VAT Summary"; }
		}

		public override string Hint
		{
			get
			{
				return @"The GST / VAT Summary Report shows a GST transaction summary to assist GST/VAT reporting.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGSTSummaryReport();
		}
	}

	public class TestGSTSummaryReceivablesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "GST / VAT Summary"; }
		}

		public override string Hint
		{
			get
			{
				return @"The GST / VAT Summary Report shows a GST transaction summary to assist GST/VAT reporting.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGSTSummaryReport();
		}
	}
}
