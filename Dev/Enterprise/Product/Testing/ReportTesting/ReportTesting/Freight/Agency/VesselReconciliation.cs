namespace Enterprise.ReportTesting.Freight.Agency
{
	using System.Collections.Generic;
	using Enterprise.Freight.Agency.Module;
	using Enterprise.Freight.Business;

	[TemplateName("Vessel Reconciliation")]
	public class TestVesselReconciliationReport : TemplateTestCase
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
					"Summary by Vessel",
					"Summary by Branch by Vessel"
				};
			}
		}
	}

	public class VesselReconciliationTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return @"This report compares vessel income with expenses, and can be run for vessels with optional filters by Principal, Vessel-Voyage, Branch & Posted Date range with 2 optional templates: Summary by Vessel & Summary by Branch by Vessel.

It can be used by both, Agencies and Shipping Lines, depending on the following Charges Selection filter:

-Agency - Cost amounts where Principal is the Creditor (principal income) vs Sell amounts where Principal is the Debtor (principal expenses). This provides statement on amounts collected or paid on behalf of principals.

-Shipping Line - All Sell amounts (income) vs All Cost amounts (expenses).

-Comparison Agency - Sell amounts from Bills/Bookings (income) vs Sell amounts from Voyage Accounting (expenses), i.e. it compares the total shipments revenue with what is being charged to the principal on Voyage Accounting.

-Comparison Shipping Line - Sell amounts from Bills/Bookings (income) vs Cost amounts from Voyage Accounting (expenses).";
			}
		}

		public override string MenuName
		{
			get { return "Vessel Reconciliation"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVesselReconciliationReport();
		}
	}
}
