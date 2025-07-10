namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using NUnit.Framework;

	[TemplateName("Import Invoice Report")]
	public class TestUSInvoicesReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		[StressTest]
		[TestDate(2006, 12, 25)]
		public void TestReportRunsWithNoExceptionWithAllColumnHeadings()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var headings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in headings)
			{
				heading.Hidden = false;
			}
			RunReport();
		}

		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList> FROM USInvoices(<CurrentCompany>, <Country/Region Of Origin>, <Job Registered On.FromDateUtc>, <Job Registered On.ToNextDateUtc>, <Import Date.FromDateForSQLParameter>, <Import Date.ToNextDate>, <Arrival Date.FromDateForSQLParameter>, <Arrival Date.ToNextDate>, <Importer>, <Entry Port>,<PGA Entry Status>,<PGA Expedited Release>)",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}

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

			Factory.New<BaseJobDeclaration>();
			Factory.Save();
		}
	}

	public class TestUSInvoiceReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Import Invoice Report"; }
		}

		public override string Hint
		{
			get { return "This report makes import (IMP, IMX, MSC, FTZ) declaration and invoice information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSInvoicesReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
