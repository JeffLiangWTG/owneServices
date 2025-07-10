// please don't edit your tests in this file, move them to other solutions instead.
using CargoWise.Definitions;

namespace Enterprise.ReportTesting.Common
{
	[TemplateName("Bank Statement Transactions Listing")]
	public class TestBankStatementTransactionsListingReport : TemplateTestCase
	{
	}

	[TemplateName("3rd Party Software Report")]
	public class Test3rdPartySoftwareReport : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode
		{
			get { return Clients.EDI; }
		}
	}

	[TemplateName("Budget Trial Balance Report")]
	public class TestBudgetTrialBalanceReport : TemplateTestCase
	{
	}

	[TemplateName("GL Balance Sheet Multilingual Report")]
	public class TestGLBalanceSheetMultilingualReport : TemplateTestCase
	{
	}

	[TemplateName("Job Summary Selected Client Report")]
	public class TestJobSummarySelectedClientReport : TemplateTestCase
	{
	}

	[TemplateName("Product Listing")]
	public class TestProductListing : TemplateTestCase
	{
	}

	[TemplateName("Tariff Lookup Listing")]
	public class TestTariffLookupListing : TemplateTestCase
	{
	}

	[TemplateName("Train List Report")]
	public class TestTrainListReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Transport Movement Report")]
	public class TestTransportMovementReport : TemplateTestCase
	{
	}
}
