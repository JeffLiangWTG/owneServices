namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("US Product Expired Tariff Report")]
	public class TestUSProductExpiredTariffReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}

	public class TestUSProductExpiredTariffReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=US"); }
		}

		public override string MenuName
		{
			get { return "Product Expired Tariff Report"; }
		}

		public override string Hint
		{
			get { return "The Product Expired Tariff Report will show products which have active customs information where the tariff number of US is not valid as of the selected date."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSProductExpiredTariffReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
