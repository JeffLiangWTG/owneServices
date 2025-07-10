namespace Enterprise.ReportTesting.TransportBookings
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("Transport Booking HVLV Shipment Details")]
	public class TestHVLVLineDetailsReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return true; }
		}

		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "With Totals" }; }
		}
	}

	public class TestHVLVLineDetailsReportReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.TransportBookings.Module.ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Transport Booking HVLV Shipment Details"; }
		}

		public override string Hint
		{
			get { return ""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestHVLVLineDetailsReport();
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "HasHVLVClearance=Y");
	}
}
