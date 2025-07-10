namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("US Product Listing")]
	public class TestUSProductListingReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		public void TestProvProgAdditionalTariffColumns()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var headings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			Assert(headings.Contains("Prov Add. Tariff 1"));
			Assert(headings.Contains("Prov Add. Tariff 2"));
			Assert(headings.Contains("Prov Add. Tariff 3"));
			Assert(headings.Contains("Prov Add. Tariff 4"));
			Assert(headings.Contains("Prov Add. Tariff 5"));
		}
	}

	public class TestUSProductListingMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter
		{
			get { return new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=US"); }
		}

		public override string MenuName
		{
			get { return "Product Listing"; }
		}

		public override string Hint
		{
			get { return @"The Product Listing Report lists the Customs Products set up in your CargoWise system.  It shows the part number, descriptions, product numbers and references. 
    
    NB: When entering a value into a text box on the filter form, CargoWise searches for fields that start with that value, unless otherwise specified.	"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSProductListingReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
