namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Export Shipments Cut Off Report")]
	public class TestExportShipmentsCutOffReport : TemplateTestCase
	{
	}

	public class TestExportShipmentsCutOffReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Export Shipments Cut Off Report"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Export Shipments by Cut Off Date Report will produce a list of shipments and summary cargo details by load port cut off date and time.

Using the cargo cut off dates and times recorded on the schedule, this report will generate a list of shipments detailing: cargo cutoff date and time, shipment number, destination, consignor name, goods handling instructions, hazardous cargo handling instructions, house number, weight, packages, goods description, consol number, vessel/voyage/flight/journey, load port, ETD.

Report filters include cutoff date and time, transport mode (e.g. sea, rail), load port, consol type (e.g. FCL, Groupage), consignor, CFS, CTO.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestExportShipmentsCutOffReport();
		}
	}
}
