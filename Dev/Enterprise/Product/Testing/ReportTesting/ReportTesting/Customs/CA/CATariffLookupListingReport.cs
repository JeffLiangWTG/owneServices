using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Customs.CA
{
	[TemplateName("CA Tariff Lookup Listing")]
	public class CATariffLookupListingTemplateTest : TemplateTestCase
	{
	}

	public class CATariffLookupListingReportTest : ReportTestCase
	{
		public override string MenuName => "Tariff Lookup Listing";

		public override string Hint => string.Empty;

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=CA");

		public override ZEmbeddedModule ModuleToTest => new CustFilesReports();

		protected override TemplateTestCase GetTemplateTestCase() => new CATariffLookupListingTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
		}
	}
}
