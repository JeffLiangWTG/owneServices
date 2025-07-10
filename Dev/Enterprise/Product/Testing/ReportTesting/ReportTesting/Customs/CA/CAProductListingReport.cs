using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Customs.CA
{
	[TemplateName("CA Product Listing")]
	public class CAProductListingTemplateTest : TemplateTestCase
	{
	}

	public class CAProductListingReportTest : ReportTestCase
	{
		public override string MenuName => "Product Listing";

		public override string Hint => string.Empty;

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=CA");

		public override ZEmbeddedModule ModuleToTest => new CustFilesReports();

		protected override TemplateTestCase GetTemplateTestCase() => new CAProductListingTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
		}
	}
}
