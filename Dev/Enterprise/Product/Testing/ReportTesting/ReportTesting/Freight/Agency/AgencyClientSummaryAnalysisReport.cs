using Enterprise.Freight.Agency.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Agency Client Summary Analysis Report")]
	public class TestAgencyClientSummaryAnalysisReport : TemplateTestCase
	{
	}

	public class AgencyClientSummaryAnalysisReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string MenuName
		{
			get { return "Client Summary Volume & Charges Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report identifies Jobs, Volumes and Charges totals by Client.
It supports analysis of various cargo types, TEU, KG, M3, M2, number of jobs and profit by client.

For the nominated date range, the report selects all jobs that meet the filter criteria.
By default 'Client' is the Local Client on the Accounting Job Header (billing tab) of each Job.
Alternatively, you can elect to run this analysis by Consignee (imports), Consignor (exports).
Nominate a Staff Grouping option to measure client performance by staff member.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAgencyClientSummaryAnalysisReport();
		}
	}
}
