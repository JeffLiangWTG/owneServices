using System.Collections.Generic;

namespace Enterprise.ReportTesting.Freight.Booking
{
	[TemplateName("Consignor Booking Status Report")]
	public class TestConsignorBookingStatusReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Booking Status Report"
				};
			}
		}
	}

	[TemplateName("Client - Booking Summary")]
	public class TestClientBookingListingReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "Booking Summary" }; }
		}
	}
}
