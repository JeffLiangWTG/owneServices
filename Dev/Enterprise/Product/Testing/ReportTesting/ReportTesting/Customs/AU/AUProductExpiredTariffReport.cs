namespace Enterprise.ReportTesting.Customs.AU
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("AU Product Expired Tariff Report")]
	public class TestAUProductExpiredTariffReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
		}
	}

	public class TestAUProductExpiredTariffReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		public override string MenuName
		{
			get { return "Product Expired Tariff Report"; }
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=AU"); }
		}

		public override string Hint
		{
			get { return "The Product Expired Tariff Report will show products which have active customs information where the tariff number of AU is not valid as of the selected date."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAUProductExpiredTariffReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}
}
