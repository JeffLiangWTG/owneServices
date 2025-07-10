namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using NUnit.Framework;

	[TemplateName("US Export Invoice Line Report")]
	public class TestUSExportInvoiceLinesReport : TemplateTestCase
	{
		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList>  FROM USExportInvoiceLines(<CurrentCompany>, <Country / Region Of Destination>, <Tariff Starts With 1>, <Tariff Starts With 2>, <Tariff Starts With 3>, <Tariff Starts With 4>, <Tariff Starts With 5>, <Job Registered On.FromDateUtc>, <Job Registered On.ToNextDateUtc>, <Show ITAR>, <Routed Transaction>, <Product Code Starts With 1>, <Product Code Starts With 2>, <Product Code Starts With 3>, <Product Code Starts With 4>, <Product Code Starts With 5>)",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}

		[ExpectNoExceptions]
		public void TestReportLicenseTypeEXP()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["License Type"]).ValueAsStringForSerialisation = "EXP";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestProductStartsWith1()
		{
			AssertTextFilter("Product Code Starts With 1");
		}

		[ExpectNoExceptions]
		public void TestProductStartsWith2()
		{
			AssertTextFilter("Product Code Starts With 2");
		}

		[ExpectNoExceptions]
		public void TestProductStartsWith3()
		{
			AssertTextFilter("Product Code Starts With 3");
		}

		[ExpectNoExceptions]
		public void TestProductStartsWith4()
		{
			AssertTextFilter("Product Code Starts With 4");
		}

		[ExpectNoExceptions]
		public void TestProductStartsWith5()
		{
			AssertTextFilter("Product Code Starts With 5");
		}

		[ExpectNoExceptions]
		public void TestECCN()
		{
			AssertTextFilter("ECCN");
		}

		[ExpectNoExceptions]
		public void TestLicenseNumber()
		{
			AssertTextFilter("License Number");
		}

		void AssertTextFilter(ZString filterName)
		{
			PrepareReportForRender();
			var filter = (TextField)Report.FilterCollection[filterName];
			filter.Value = "1";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestLicenseType()
		{
			PrepareReportForRender();
			var filter = (CodeLookupField)Report.FilterCollection["License Type"];
			filter.Value = "1";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestITARExemptionNumber()
		{
			PrepareReportForRender();
			var filter = (CodeListMultipleChoice)Report.FilterCollection["ITAR Exemption Number"];
			filter.Value = "1";
			RunReport();
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
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, "DESC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, "C30", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			Factory.New<BaseJobDeclaration>();
			var part = Factory.New<Enterprise.Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "part";
			Factory.Save();
		}
	}

	public class TestUSExportInvoiceLineReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Export Invoice Line Report"; }
		}

		public override string Hint
		{
			get { return "This report makes export declaration and invoice line information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSExportInvoiceLinesReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
