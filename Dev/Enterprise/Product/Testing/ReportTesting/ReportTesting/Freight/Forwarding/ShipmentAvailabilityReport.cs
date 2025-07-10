namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Shipment Availability Report")]
	public class TestShipmentAvailabilityReport : TemplateTestCase
	{
	}

	public class TestShipmentAvailabilityReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Import Shipments Availability Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The 'Import Shipments Availability Report' will produce a list of shipments and summary cargo details by arrival and/or availability of cargo.

Using the cargo availability dates and times recorded on the schedule, this report will generate a list of shipments detailing: arrival and availability dates and time, shipment number, destination, consignee name, goods handling instructions, hazardous cargo handling instructions, house number, weight, volume, packages, goods description, consol number, vessel/voyage/flight/journey, discharge port, ATA, number of days from FCL to LCL Availability and from ATA to LCL Availability.

Report filters include availability date and time, transport mode (e.g. sea, rail), discharge port, consol type (e.g. FCL, Groupage), consignee, CFS, CTO.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentAvailabilityReport();
		}
	}
}
