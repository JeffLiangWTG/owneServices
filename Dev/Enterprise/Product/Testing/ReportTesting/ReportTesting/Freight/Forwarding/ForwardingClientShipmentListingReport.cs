namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Client - Shipment Listing Report")]
	public class TestClientShipmentListingReport : TemplateTestCase
	{
	}

	public class TestClientShipmentListingReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Client - Shipment Listing Report"; }
		}

		public override string Hint
		{
			get
			{
				return
@"This is a Client facing report that allows you to generate a list of Forwarding Shipments for a single client.
For the nominated client the report selects all shipments where Organization is Consignee, Consignor or Billing Client on a shipment.
Additional filter options allow you to limit the report to particular freight dates, ports, transport modes etc. The report also includes Pickup and Delivery date filter options. This report supports documenting more than 135 columns worth of information for a shipment.
Use the Column Customization controls to format a report that provides your client only the tracking information they require. Use this report to update your client on the latest status of forwarding shipments handled on their behalf.

In case of sub-shipments, the Container & TEU Count are blank, because this information is associated with master shipments of these subs.  The Container & TEU Count will only be shown against stand alone shipments and top level master shipments.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestClientShipmentListingReport();
		}
	}
}
