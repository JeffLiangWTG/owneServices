using Enterprise.Freight.Agency.Module;
using Enterprise.Freight.Business;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Shipments - Profile by Vessel-Voyage")]
	public class TestShipmentsProfileByVesselVoyage : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			Factory.Save();
		}
	}

	public class ShipmentsProfileByVesselVoyageTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return @"Shipment Profile Report provides summary and detailed information about every export (Booking and/or Bill of Lading) and import (Bill of Lading) shipment in a single convenient report.

For given vessel-voyage/s or a date range, with break ups by ports, the report lists jobs with cargo type, weight, commodity, organizations, container information (including TEU count by type), charges (prepaid and collect amounts in the local currency with subtotals by chosen port), etc.

Filter options allow generation of reports profiling shipments for a selected port, principal, trade lane, customer, confirmed and/or active status, cargo type, with optional charges section and container summary group.

The layout is compatible with Excel’s sort, auto filter and subtotal functions.

Note. If one container is entered on more than one Bill of Lading (the same container number and container type), this container will only participate once in the calculation of total counts.";
			}
		}

		public override string MenuName
		{
			get { return "Shipment Profile Report"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestShipmentsProfileByVesselVoyage();
		}
	}
}
