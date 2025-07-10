using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("GB CCSUK Bond Check")]
	public class TestGBCCSUKBondCheckReportTemplate : TemplateTestCase
	{
	}

	public class TestGBCCSUKBondCheckReportReport : GBReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.Customs.EU.GB.GbCcsukReports;

		public override string MenuName
		{
			get { return "GB CCSUK Bond Check"; }
		}

		public override string Hint
		{
			get
			{
				return @"Shows consignments that should (still) be in the shed. Shows only those records where NPR > 0 and where the sum of the number of pieces released and delivered is less than NPR. It excludes, therefore, those records not fully received (NPX>NPR) but where everything received has been released. Gives details of warehouse receipts and locations.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGBCCSUKBondCheckReportTemplate();
		}
	}
}
