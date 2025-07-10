using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.KR
{
	[TemplateName("KR Export Entries Report")]
	public class TestKRExportEntriesReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("KR");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1111111111111";
			Factory.Save();
		}
	}

	public class TestKRExportEntriesReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "KR Export Entries Report"; }
		}

		public override string Hint
		{
			get { return "This report makes export entries available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestKRExportEntriesReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("KR");
		}
	}
}
