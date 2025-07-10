using System.Collections.Generic;
using Enterprise.Freight.Agency.Module;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Agency Container Statistics Report")]
	public class TestAgencyContainerStatisticsReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Template"
				};
			}
		}
	}

	public class AgencyContainerStatisticsReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get { return "This report provides container statistics per port for a given period of time. The report gives full and empty container count on import and export sailings by vessel, voyage and container type with optional ownership details and filter by principal."; }
		}

		public override string MenuName
		{
			get { return "Container Statistics by Port and Period"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAgencyContainerStatisticsReport();
		}
	}
}
