namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Reconciliation Line Report")]
	public class TestReconciliationLineReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			var declaration = Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			((BusinessObject)declaration).FillWithValidTestData();
			var entry = Factory.New<Enterprise.Integration.Customs.US.ICusEntryHeader>();
			((BusinessObject)entry).FillWithValidTestData();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = "ENS";

			Factory.Save();
		}
	}

	public class TestReconciliationLineReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Reconciliation Line Report";

		public override string Hint => "This report makes reconciliation lines information available.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestReconciliationLineReport();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
