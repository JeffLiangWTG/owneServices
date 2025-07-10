using Enterprise.Freight.Agency.Module;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Agency Reconcile Container Movements By Voyage")]
	public class ContainerMovementsReconciliationByVoyageReport : TemplateTestCase
	{
	}

	public class ContainerMovementsReconciliationByVoyageReportTest : ReportTestCase
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
					"This report reconciles the container movements attached to a voyage with the containers entered against bills on the same voyage, " +
					"highlighting movements reported for containers that are not on the specified voyage and containers said to be on a voyage but have " +
					"not had any movements reported relating to the voyage.";
			}
		}

		public override string MenuName
		{
			get { return "Container Movements Reconciliation by Voyage"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ContainerMovementsReconciliationByVoyageReport();
		}
	}
}
