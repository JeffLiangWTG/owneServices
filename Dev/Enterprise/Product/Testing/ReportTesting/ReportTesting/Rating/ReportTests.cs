namespace Enterprise.ReportTesting.Rating
{
	public static class SalesManagerReportTests
	{
		[TemplateName("Expiring Rates")]
		public class TestExpiringRates : TemplateTestCase
		{
			protected override bool ReportRequiresColumnHeadings
			{
				get { return false; }
			}
		}

		[TemplateName("Sales Missing Client Rates Report")]
		public class TestSalesMissingClientRatesReport : TemplateTestCase
		{
			protected override bool ReportRequiresColumnHeadings
			{
				get { return false; }
			}
		}

		[TemplateName("Sales Staff Quotations Activity Report")]
		public class TestSalesSatffQuotationActivityReport : TemplateTestCase
		{
			protected override bool ReportRequiresColumnHeadings
			{
				get { return false; }
			}

			protected override string TemplateFileType => ".xlsx";
		}

		[TemplateName("Sales Staff Funnel Activity Report")]
		public class TestSalesStaffFunnelActivityReport : TemplateTestCase
		{
			protected override string TemplateFileType => ".xlsx";
		}
	}
}
