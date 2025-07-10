namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	public class TestClientShipmentListingReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Client - Shipment & Declaration Listing Report"; }
		}

		public override string Hint
		{
			get
			{
				return
@"This is a Client facing report that generates a list of Forwarding Shipments and Declarations for a single client.
For the nominated client the report selects all shipments and standalone declarations where that Organization is either the Consignee or Consignor.
Additional filter options allow you to limit the report to particular freight dates, ports, transport modes etc. The report also includes Pickup and Delivery date filter options. This report supports documenting more than 135 columns worth of information for a shipment/declaration.
Use the Column Customization controls to format a report that provides your client only the tracking information they require. Use this report to update your client on the latest status of freight handled on their behalf.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Freight.Forwarding.TestClientShipmentListingReport();
		}
	}
}
