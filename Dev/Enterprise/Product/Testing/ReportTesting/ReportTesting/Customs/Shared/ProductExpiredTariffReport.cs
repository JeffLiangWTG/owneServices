namespace Enterprise.ReportTesting.Customs.Shared
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("Product Expired Tariff Report")]
	public class TestProductExpiredTariffReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("ZA");
		}
	}

	public class TestProductExpiredTariffReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY!=US=CA=SG=AU"); }
		}

		public override string MenuName
		{
			get { return "Product Expired Tariff Report"; }
		}

		public override string Hint
		{
			get { return "The Product Expired Tariff Report will show products which have active customs information where the tariff number is not valid as of the selected date."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestProductExpiredTariffReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("ZA");
		}
	}
}
