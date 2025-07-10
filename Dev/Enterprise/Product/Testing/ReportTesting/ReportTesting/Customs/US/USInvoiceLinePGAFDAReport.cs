namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Import Invoice Line PGA Report - FDA")]
	public class TestUSInvoiceLinePGAFDAReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");

			var testHelper = new Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>();
			((BusinessObject)declaration).FillWithValidTestData();
			var entry = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			((BusinessObject)entry).FillWithValidTestData();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = "ENS";
			Factory.Save();
		}
	}

	public class TestUSInvoiceLinePGAReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();
		public override string MenuName => "Import Invoice Line PGA Report - FDA";
		public override string Hint => "Import Invoice Line PGA Report lists detailed PGA data related with Import Invoice Lines";
		protected override TemplateTestCase GetTemplateTestCase() => new TestUSInvoiceLinePGAFDAReport();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override bool ExpectedIsPublished => true;
	}
}
