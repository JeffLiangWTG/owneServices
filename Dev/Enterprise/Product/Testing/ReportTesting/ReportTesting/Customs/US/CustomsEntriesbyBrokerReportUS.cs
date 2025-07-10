namespace Enterprise.ReportTesting.Customs.US
{
	using Enterprise.Customs.Module;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Customs Entries by Broker Report US")]
	public class TestCustomsEntriesLodgedbyBroker : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		public void TestReportEventDatesFilter()
		{
			PrepareReportForRender();
			var dateRange = (DateRangeField)Report.FilterCollection["Event Dates"];
			Assert(dateRange.RequireBothFromAndToDates);
		}
	}

	public class TestCustomsEntriesLodgedbyBrokerMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Customs Entries Lodged by Broker"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"This report produces a list of customs declaration entries by Broker.
For a nominated date range and for a nominated Customs Clearance Event Type the report will list customs declaration details by broker by date.
The report includes column customization to allow you to control the content and layout of the report.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCustomsEntriesLodgedbyBroker();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
