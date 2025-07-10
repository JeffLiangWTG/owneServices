namespace Enterprise.ReportTesting.Customs.SG
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("SG Product Expired Tariff Report")]
	public class TestSGProductExpiredTariffReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("SG");
		}
	}

	public class TestCAProductExpiredTariffReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=SG"); }
		}

		public override string MenuName
		{
			get { return "Product Expired Tariff Report"; }
		}

		public override string Hint
		{
			get { return "The Product Expired Tariff Report will show products which have active customs information where the tariff number of SG is not valid as of the selected date."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestSGProductExpiredTariffReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("SG");
		}
	}
}
