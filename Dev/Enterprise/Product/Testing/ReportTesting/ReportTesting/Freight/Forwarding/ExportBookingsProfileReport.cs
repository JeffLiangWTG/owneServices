namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Export Bookings Profile")]
	public class TestExportBookingProfile : TemplateTestCase
	{
	}

	public class TestExportBookingProfileReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Bookings - Export Bookings Profile"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Export Bookings Profile Report produces a detailed table of data profiling each booking in the Export Bookings Module. It is designed to be used as an Excel file, not as a printed document. 

Filter options in the report allow you to generate a report profiling Export Bookings for selected transport modes, pack modes, dates, ports, routing, clients. There is an option to include/ exclude consolidated bookings.

The layout is compatible with Excel’s sort, auto filter and subtotal functions.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestExportBookingProfile();
		}
	}
}
