using System.Collections.Generic;
using Enterprise.Freight.Agency.Module;

namespace Enterprise.ReportTesting.Freight.Agency
{
	[TemplateName("Vessel Manifest")]
	public class TestVesselManifestReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get { return new string[] { "Template" }; }
		}
	}

	public class VesselManifestTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get { return "For a nominated Vessel / Voyage and principal this report shows Manifest details including consignor, consignee, notifies, marks and numbers, goods description, container and pack details, weight, volume. Further filters include load, discharge, origin and destination ports, which can be filtered by UNLOCO, country/region or zone. Additionally there is an option to include or exclude freight & charges"; }
		}

		public override string MenuName
		{
			get { return "Vessel Voyage Manifest"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVesselManifestReport();
		}
	}
}
