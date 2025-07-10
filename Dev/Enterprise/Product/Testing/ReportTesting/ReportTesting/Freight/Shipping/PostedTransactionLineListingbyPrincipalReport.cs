using Enterprise.Freight.Agency.Module;

namespace Enterprise.ReportTesting.Freight.Shipping
{
	[TemplateName("ShippingTransactionLinesPostedbyPrincipal")]
	public class TestShippingTransactionLinesPostedbyPrincipalReport : TemplateTestCase
	{
	}

	public class ShippingTransactionLinesPostedbyPrincipalReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return @"For a selected Period or Date Range, this report will itemize the transaction lines of invoices and credit notes posted against Shipping jobs.
By Principal, this report will itemize posted Revenue (REV) and posted Cost (CST) transaction lines.
Optional filters by Vessel/Voyage, Branch, Department.";
			}
		}

		public override string MenuName
		{
			get { return "Posted Transaction Line Listing by Principal"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShippingTransactionLinesPostedbyPrincipalReport();
		}
	}
}
