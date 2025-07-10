namespace Enterprise.ReportTesting.Freight.Agency
{
	using System.Collections.Generic;
	using Enterprise.Freight.Agency.Module;
	using Enterprise.Freight.Business;

	[TemplateName("Agency Bookings Container Summary Report")]
	public class TestAgencyBookingsContainerSummaryReport : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			Factory.Save();
		}

		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Template"
				};
			}
		}
	}

	public class AgencyBookingsContainerSummaryReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get { return "For a nominated period of time and optional filters by Principal, Trade Lane, Vessel-Voyage and Load Port, or for an individual Vessel-Voyage, this report summarizes a number of containers booked and allocated by container type and status (full/empty) per vessel-voyage with subtotals for each of the load ports."; }
		}

		public override string MenuName
		{
			get { return "Bookings - Container Summary by Vessel Voyage"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAgencyBookingsContainerSummaryReport();
		}
	}
}
