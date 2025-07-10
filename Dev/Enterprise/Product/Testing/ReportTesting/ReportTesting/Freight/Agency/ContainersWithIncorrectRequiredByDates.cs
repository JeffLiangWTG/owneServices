namespace Enterprise.ReportTesting.Freight.Agency
{
	using Enterprise.Freight.Agency.Module;
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("Agency Containers With Incorrect Required By Dates")]
	internal sealed class ContainersWithIncorrectRequiredByDatesReport : TemplateTestCase
	{
	}

	internal sealed class ContainersWithIncorrectRequiredByDatesReportTest : ReportTestCase
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
					"This report lists all bill containers where the container return required by date does not match the value calculated from the availability date and the applicable detention free days.\r\n" +
					"";
			}
		}

		public override string MenuName
		{
			get { return "Containers - With Incorrect Required By Dates"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ContainersWithIncorrectRequiredByDatesReport();
		}
	}
}
