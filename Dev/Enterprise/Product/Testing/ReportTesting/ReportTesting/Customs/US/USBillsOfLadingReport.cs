namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.Types;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Import Bill Of Lading Report")]
	public class TestUSBillsOfLadingReport : TemplateTestCase
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
			Factory.Save();
		}

		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList> FROM USBillsOfLading(<CurrentCompany>, <Lowest Bills>, <Job Registered On.FromDateForSQLParameter>, <Job Registered On.ToNextDateForSQLParameter>, <Import Date.FromDateForSQLParameter>, <Import Date.ToNextDateForSQLParameter>, <Arrival Date.FromDateForSQLParameter>, <Arrival Date.ToNextDateForSQLParameter>, <Importer>, <Entry Port>)",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}
	}

	public class TestUSBillsOfLadingReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Import Bill Of Lading Report"; }
		}

		public override string Hint
		{
			get { return "This report makes bill information and related to bill import (IMP, IMX, MSC) declaration  information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSBillsOfLadingReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
