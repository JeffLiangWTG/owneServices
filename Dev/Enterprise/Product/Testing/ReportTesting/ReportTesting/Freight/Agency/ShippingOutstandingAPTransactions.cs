using Enterprise.Freight.Agency.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Shipping Outstanding AP Transactions")]
	internal sealed class ShippingOutstandingAPTransactions : TemplateTestCase
	{
	}

	internal sealed class OutstandingPayablesByPrincipalTest : ReportTestCase
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
					"This report lists all outstanding Payable Invoices & Credit Note transactions recorded against shipping jobs " +
					"(Bills of Lading, Voyage Accounting, Sundry Charges & Container Detention Jobs) by Principal. The report " +
					"identifies outstanding Payable transactions by Vessel-Voyage and Payable account." +
					"";
			}
		}

		public override string MenuName
		{
			get { return "Outstanding Payables by Principal"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ShippingOutstandingAPTransactions();
		}
	}
}
