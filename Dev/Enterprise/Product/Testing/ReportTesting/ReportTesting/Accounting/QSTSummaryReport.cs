namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("QST Summary Report")]
	public class TestQSTSummaryReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}
	}

	public class TestQSTSummaryPayablesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "QST Summary Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report summarises QST posted in the Accounts Receivable, Accounts Payable and Cash Books modules in a nominated Accounting Period range. This report is only available to Canadian Login Companies.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestQSTSummaryReport();
		}
	}

	public class TestQSTSummaryReceivablesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "QST Summary Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report summarises QST posted in the Accounts Receivable, Accounts Payable and Cash Books modules in a nominated Accounting Period range. This report is only available to Canadian Login Companies.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestQSTSummaryReport();
		}
	}
}
