namespace Enterprise.ReportTesting.BufferManagement
{
	using ZArchitecture.Modules;

	[TemplateName("Containment Barrier Outcomes")]
	public class ContainmentBarrierOutcomesTest : TemplateTestCase
	{
	}

	public class ContainmentBarrierOutcomesReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.BMReports(); }
		}

		public override string MenuName
		{
			get { return "Containment Barrier Outcomes"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ContainmentBarrierOutcomesTest();
		}
	}
}
