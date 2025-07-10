namespace Enterprise.ReportTesting.Freight.Agency
{
	using Enterprise.Freight.Agency.Module;
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("Agency Container Movements Out Of Sequence Report")]
	internal sealed class ContainerMovementsOutOfSequenceReport : TemplateTestCase
	{
	}

	internal sealed class ContainerMovementsOutOfSequenceReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return
					"This report lists all container movements that appear to be out of sequence. " +
					"For example, if a wharf gate in movement were to appear after a yard gate in movement. " +
					"This example could indicate that a yard gate out movement is missing.";
			}
		}

		public override string MenuName
		{
			get { return "Container Movements Out of Sequence"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ContainerMovementsOutOfSequenceReport();
		}
	}
}
