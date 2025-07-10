using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("GB CCSUK Unentered Consignments")]
	public class TestGBCCSUKUnenteredConsignmentsReportTemplate : TemplateTestCase
	{
	}

	public class TestGBCCSUKUnenteredConsignmentsReportReport : GBReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.Customs.EU.GB.GbCcsukReports;

		public override string MenuName
		{
			get { return "GB CCSUK Unentered Consignments"; }
		}

		public override string Hint
		{
			get
			{
				return @"Shows all unentered consignments since the given date. 'Unentered' is defined as having a blank or CX customs action code.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGBCCSUKUnenteredConsignmentsReportTemplate();
		}
	}
}
