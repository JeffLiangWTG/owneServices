namespace Enterprise.ReportTesting.Freight.Agency
{
	using Enterprise.Freight.Agency.Module;
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("Agency Container Detentions By Month Report")]
	internal sealed class ContainerDetentionsByMonthReport : TemplateTestCase
	{
	}

	internal sealed class ContainerDetentionsByMonthReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return @"This report lists collected and collectable detention amounts by calendar month for a given date range.
The date used is the date the billing was created on the detention job.

This report supports a summary and two detail templates. The 'Summary' report lists detention totals in local currency by month.  
'Details In Local Currency' template provides the list of individual detentions grouped by month. The 'Details With Container Info' template extends the 'Details In Local Currency' template with information about individual containers covered by the detention jobs.";
			}
		}

		public override string MenuName
		{
			get { return "Container Detentions By Month"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ContainerDetentionsByMonthReport();
		}
	}
}
