namespace Enterprise.ReportTesting.Customs.US
{
	using System;
	using System.Data;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using NUnit.Framework;

	[TemplateName("Import Invoice Line Report")]
	public class TestUSInvoiceLinesReport : TemplateTestCase
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

		public void TestCBMAColumns()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var headings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			AssertEquals(true, headings.Contains("CBMA TTB Rate Designation Code"));
			AssertEquals(true, headings.Contains("CBMA Tax Amount (for claim purposes)"));
			AssertEquals(true, headings.Contains("CBMA Tax Rate (for claim purposes)"));
		}

		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList> FROM USInvoiceLines(<CurrentCompany>, <Country/Region Of Origin>, <Release Status>, <ENS Status>, <Product Code Starts With 1>, <Product Code Starts With 2>, <Product Code Starts With 3>, <Product Code Starts With 4>, <Product Code Starts With 5>, <Tariff Starts With 1>, <Tariff Starts With 2>, <Tariff Starts With 3>, <Tariff Starts With 4>, <Tariff Starts With 5>, <Prov/Prog. Tariff Starts With 1>, <Prov/Prog. Tariff Starts With 2>, <Prov/Prog. Tariff Starts With 3>, <Prov/Prog. Tariff Starts With 4>, <Prov/Prog. Tariff Starts With 5>, <Job Registered On.FromDateUtc>, <Job Registered On.ToNextDateUtc>, <Import Date.FromDateForSQLParameter>, <Import Date.ToNextDate>, <Arrival Date.FromDateForSQLParameter>, <Arrival Date.ToNextDate>, <Importer>, <Entry Port>, <PGA Entry Status>,<PGA Expedited Release>)",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override string TemplateFileType => @".xlsx";

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			Factory.New<BaseJobDeclaration>();
			var part = Factory.New<Enterprise.Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "part";
			Factory.Save();

			var monthlyStatemnetPK = Guid.NewGuid();
			var monthlySql = @"insert into dbo.CusStatementHeader (B2_PK, B2_GC, B2_B2_PeriodicStatement, B2_IsMonthlyStatement, B2_EntryFilerCode, B2_SystemCreateTimeUtc, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser) values
	(@PK, @companyPK, null, 1, 'SV9', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(monthlySql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, monthlyStatemnetPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			var dailyStatemnetPK = Guid.NewGuid();
			var dailySql = @"insert into dbo.CusStatementHeader (B2_PK, B2_GC, B2_B2_PeriodicStatement, B2_IsMonthlyStatement, B2_EntryFilerCode, B2_SystemCreateTimeUtc, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser) values
	(@PK, @companyPK, @parentPK, 0, 'SV9', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(dailySql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, dailyStatemnetPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, monthlyStatemnetPK);
				command.ExecuteNonQuery();
			}

			var statementLinePK = Guid.NewGuid();
			var statementLinePKSql = @"insert into dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryFilerCode, B3_SystemCreateTimeUtc, B3_SystemCreateUser, B3_SystemLastEditTimeUtc, B3_SystemLastEditUser) values(@PK, @dailyPK, 'EntryNum1', 'SV9', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = Db.Connection.Command(statementLinePKSql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, statementLinePK);
				command.AddParameter("@dailyPK", SqlDbType.UniqueIdentifier, dailyStatemnetPK);
				command.ExecuteNonQuery();
			}
		}
	}

	public class TestUSInvoiceLineReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Import Invoice Line Report"; }
		}

		public override string Hint
		{
			get { return "This report makes import (IMP, IMX, MSC, FTZ) declaration and invoice line information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSInvoiceLinesReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
