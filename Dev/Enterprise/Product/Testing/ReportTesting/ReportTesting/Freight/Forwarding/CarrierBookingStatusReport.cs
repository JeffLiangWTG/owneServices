using Enterprise.Freight.Forwarding.Module;
using Enterprise.ReportTesting.Freight.Booking;

namespace Enterprise.ReportTesting.Freight.Forwarding
{
	public class TestCarrierBookingStatusReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Bookings - Carrier Booking Status Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report shows booking, freight and journey details for Bookings- sorted and grouped by Carrier and Consignor. Columns displayed include Journey, Load and Discharge, Freight details and references, pickup, destination and Consignee details. 
Extensive filters allow the report to be limited to the bookings you need to review, there is an option to include / exclude Consignee details or Consolidated Bookings.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestConsignorBookingStatusReport();
		}
	}
}
