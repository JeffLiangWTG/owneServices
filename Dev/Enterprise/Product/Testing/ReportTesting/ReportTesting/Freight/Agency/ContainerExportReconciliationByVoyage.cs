using Enterprise.Freight.Agency.Module;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Agency Containers - Export reconciliation by Voyage")]
	public class ContainerExportReconciliationByVoyageReport : TemplateTestCase
	{
	}

	public class ContainerExportReconciliationByVoyageReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return
					"This report compares the containers that were booked for each bill on a vessel-voyage " +
					"against the containers that have been reported to be on the bill.";
			}
		}

		public override string MenuName
		{
			get { return "Containers - Export Reconciliation by Voyage"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ContainerExportReconciliationByVoyageReport();
		}
	}
}
