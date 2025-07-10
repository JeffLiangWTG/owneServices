using Enterprise.Freight.Forwarding.Module;

namespace Enterprise.ReportTesting.Freight.Forwarding
{
	[TemplateName("Shipments with No Jobs Headers AndOr No Invoiced Revenue")]
	public class TestShipmentsandDeclarationswithnoInvoicingOrNoInvoicedRevenueReport : TemplateTestCase
	{
	}

	public class TestShipmentsWithoutInvoicingorRevenues : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Shipments With No Job Headers And / Or No Invoiced Revenue"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report has two optional templates. Both layouts assist you to identify freight jobs in your system that have not or cannot have invoices issued against them.

Use the 'No Accounting Job Header' optional template to identify Shipments and Standalone Declarations with NO ACCOUNTING JOB HEADER in your login company. Identifying records of this type is important because freight records without an accounting job header in your login company cannot issue invoices have costs applied against them.

Use the 'Jobs with No REV Transactions' optional template to identify Shipments and Standalone Declarations with Accounting Job Headers in your login company, but with NO Revenue (Invoiced) Transaction lines posted against them. This layout will assist you to identify those jobs where invoicing has yet to be started.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentsandDeclarationswithnoInvoicingOrNoInvoicedRevenueReport();
		}
	}
}
