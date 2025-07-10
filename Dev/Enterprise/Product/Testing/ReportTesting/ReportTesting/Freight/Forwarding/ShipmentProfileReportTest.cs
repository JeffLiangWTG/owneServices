namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;

	[TemplateName("Shipment Profile Report")]
	public class TestShipmentProfileReportTest : TemplateTestCase
	{
	}

	public class TestShipmentProfileReportTestMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Shipment Profile Report"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Shipment Profile Report generates a list of Shipments in your CargoWise system. Across 60+ columns, the report summarizes key information for each shipment. Including:

- Key freight details about the shipment e.g. Consignee, Consignor, Ports, ETA/ETD etc.

- Job Profit details drawn from the Invoicing Job attached to a shipment. E.g. Local client, sales rep, branch, department, profit. 

- Summary Consol details drawn from the Consol attached to a shipment.

- TEU and number of containers attached to FCL or BCN Master shipments. In case of sub-shipments, the Containers Number and the TEU Count are blank, because this information is associated with master shipments of these subs.  The Container Numbers and Counts will only be shown against stand alone shipments and top level master shipments.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentProfileReportTest();
		}
	}
}
