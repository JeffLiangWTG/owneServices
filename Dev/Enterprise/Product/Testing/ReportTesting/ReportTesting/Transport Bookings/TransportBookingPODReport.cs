namespace Enterprise.ReportTesting.TransportBookings
{
	[TemplateName("Transport Booking POD")]
	public class TestTransportBookingPODReport : TemplateTestCase
	{
	}

	public class TestTransportBookingPODReportReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.TransportBookings.Module.ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Transport Booking POD"; }
		}

		public override string Hint
		{
			get { return "Transport Booking Proof Of Delivery Report"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestTransportBookingPODReport();
		}
	}
}
