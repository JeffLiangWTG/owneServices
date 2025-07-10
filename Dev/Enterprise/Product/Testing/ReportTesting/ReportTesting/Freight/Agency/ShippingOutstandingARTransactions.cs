using Enterprise.Freight.Agency.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Shipping Outstanding AR Transactions")]
	public class ShippingOutstandingARTransactions : TemplateTestCase
	{
	}

	public class OutstandingReceivablesByPrincipalTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return
					"This report lists all outstanding Receivable Invoices & Credit Note transactions recorded against shipping jobs " +
					"(Bills of Lading, Voyage Accounting, Sundry Charges & Container Detention Jobs) by Principal. The report " +
					"identifies outstanding Receivable transactions by Vessel-Voyage and Receivable account." +
					"";
			}
		}

		public override string MenuName
		{
			get { return "Outstanding Receivables by Principal"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ShippingOutstandingARTransactions();
		}
	}
}
