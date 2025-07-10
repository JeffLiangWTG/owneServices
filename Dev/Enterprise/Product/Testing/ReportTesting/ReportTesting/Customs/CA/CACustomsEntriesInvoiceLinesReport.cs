using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Customs.CA
{
	[TemplateName("CA Customs Entries Invoice Lines")]
	public class CACustomsEntriesInvoiceLinesTemplateTest : TemplateTestCase
	{
	}

	public class CACustomsEntriesInvoiceLinesReportTest : ReportTestCase
	{
		public override string MenuName => "CA Customs Entries Invoice Lines";

		public override string Hint => string.Empty;

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "BKRCTY=CA");

		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		protected override TemplateTestCase GetTemplateTestCase() => new CACustomsEntriesInvoiceLinesTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
		}
	}
}
