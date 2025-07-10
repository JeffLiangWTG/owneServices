using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("CCSUK Cleared or Released Short Deliveries")]
	public class TestCCSUKClearedOrReleasedShortDeliveriesReportTemplate : TemplateTestCase
	{
	}

	public class TestCCSUKClearedOrReleasedShortDeliveriesReportReport : GBReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.Customs.EU.GB.GbCcsukReports;

		public override string MenuName
		{
			get { return "CCSUK Cleared or Released Short Deliveries"; }
		}

		public override string Hint
		{
			get
			{
				return @"A report showing all consignments where the number of pieces delivered is not equal to the number of pieces received. It therefore shows short deliveries or those consignments not yet fully delivered.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCCSUKClearedOrReleasedShortDeliveriesReportTemplate();
		}
	}
}
