using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportsSetupCategoryCollection))]
	sealed class ComplianceReportsSetupCategoryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceReportsSetupCategoryCollection>
	{
		public void TestAddDefaultsReportCategories()
		{
			list = new ComplianceReportsSetupCategoryCollection("TT0", Constants.CountryCodes.China);
			AssertEquals(5, list.Count);

			list = new ComplianceReportsSetupCategoryCollection();
			list.AddDefaultsReportCategories(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, Constants.CountryCodes.China);
			AssertEquals(82, list.Count);
			AssertEquals("D01, D02, D03, D04, D05, D06, D07, D08, D09, D10, D11, D12, D13, D14, D20, D21, D22, D23, D24, D25, D26, D27, D28, D30, D32, D33, D34, D35, D36, D37, D38, D39, D40, D41, D42, D43, D44, D45, D46, H01, H02, H03, H04, H05, H06, H07, H08, H09, H10, H11, H12, H13, H14, H15, H16, H30, H31, H32, H33, H34, H35, H36, H37, H38, H41, H42, H43, H44, H45, H46, H47, H48, H49, H50, H51, H52, H53, H54, H55, H57, H59, XXX", list.CodesAsString);
			AssertEquals(BalanceSheet_China.Codes.D01, list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[81].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss, Constants.CountryCodes.China);
			AssertEquals(33, list.Count);
			AssertEquals("D02, D03, D05, D06, D07, D08, D09, D10, D11, D12, D13, D14, D15, D16, D17, D18, D19, D20, H22, H23, H24, H25, H26, H27, H28, H29, H30, H32, H33, H35, H38, H39, XXX", list.CodesAsString);
			AssertEquals("D02", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[32].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ChartOfAccount, Constants.CountryCodes.China);
			AssertEquals(7, list.Count);
			AssertEquals("BSH, P&L, PLA, SPA, SSE, CFS, XXX", list.CodesAsString);
			AssertEquals("BSH", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[6].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly, Constants.CountryCodes.China);
			AssertEquals(15, list.Count);
			AssertEquals("D01, D04, D05, D11, D14, D15, D16, D19, D22, D23, D25, D28, D29, D30, XXX", list.CodesAsString);
			AssertEquals("D01", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[14].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision, Constants.CountryCodes.China);
			AssertEquals(15, list.Count);
			AssertEquals("P02, P03, P05, P06, P08, P09, P11, P12, P14, P15, P17, P18, P19, P20, XXX", list.CodesAsString);
			AssertEquals("P02", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[14].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed, Constants.CountryCodes.China);
			AssertEquals(11, list.Count);
			AssertEquals("V02, V03, V04, V05, V08, V09, V10, V11, V12, V18, XXX", list.CodesAsString);
			AssertEquals("V02", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[10].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation, Constants.CountryCodes.China);
			AssertEquals(13, list.Count);
			AssertEquals("A02, A03, A04, A05, A06, A07, A08, A09, A10, A11, A12, A13, XXX", list.CodesAsString);
			AssertEquals("A02", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[12].Category);

			list = new ComplianceReportsSetupCategoryCollection(AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement, Constants.CountryCodes.China);
			AssertEquals(29, list.Count);
			AssertEquals("B03, B04, B05, B06, B18, B19, B20, B21, B22, B23, B30, B41, B49, B50, B51, B52, B53, B54, B56, B57, B58, B59, B64, B65, B66, B69, B72, B79, XXX", list.CodesAsString);
			AssertEquals("B03", list[0].Category);
			AssertEquals(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, list[28].Category);
		}

		public void TestCodesAsString()
		{
			list = new ComplianceReportsSetupCategoryCollection();
			AssertEquals(ZString.Empty, list.CodesAsString);

			ComplianceReportsSetupCategory complianceReportType = list.AddNew();
			complianceReportType.Category = "ABC";
			AssertEquals("ABC", list.CodesAsString);

			ComplianceReportsSetupCategory complianceReportType1 = list.AddNew();
			complianceReportType1.Category = "EFG";
			AssertEquals("ABC, EFG", list.CodesAsString);

			list = new ComplianceReportsSetupCategoryCollection("TT0", Constants.CountryCodes.China);
			AssertEquals("A01, A02, A03, A04, XXX", list.CodesAsString);
		}

		public void TestGetDescriptionFromCode()
		{
			list = new ComplianceReportsSetupCategoryCollection("TT0", Constants.CountryCodes.China);
			AssertEquals("Balance Sheet1", list.GetDescriptionFromCode("A01"));
		}

		public void TestContainsCategory()
		{
			list = new ComplianceReportsSetupCategoryCollection("TT0", Constants.CountryCodes.China);
			Assert(list.ContainsCategory("A03"));
		}

		public void TestDefaultsReportCategoriesEqualsScript()
		{
			AssertReportCategoriesEquals("BSH");
			AssertReportCategoriesEquals("PLM");
			AssertReportCategoriesEquals("P&L");
			AssertReportCategoriesEquals("PLA");
		}

		void AssertReportCategoriesEquals(string reportType)
		{
			var table = DataUtils.GetDataTableFromQuery(Db.Connection, $@"SELECT Category, CategoryDescription FROM GetCategoryDetailsFromRegistry('{reportType}')");
			var dictFromScript = table.Rows.Cast<DataRow>().ToDictionary(x => x["Category"].ToString(), x => x["CategoryDescription"].ToString());
			dictFromScript.Add("XXX", "Undefined Category");

			var collection = new ComplianceReportsSetupCategoryCollection();
			collection.AddDefaultsReportCategories(reportType, Constants.CountryCodes.China);
			var dictDefault = collection.Cast<ComplianceReportsSetupCategory>().ToDictionary(x => x.Category.ToString(), x => x.CategoryDescription.ToString());

			AssertEquals($"{reportType}: Count is not equal", dictDefault.Count, dictFromScript.Count);

			foreach (var item in dictFromScript)
			{
				Assert($"{reportType}: Category changes", dictDefault.ContainsKey(item.Key));
				AssertEquals($"{reportType}: Category Description changes", dictDefault[item.Key], item.Value);
			}
		}

		public void TestAllowSort()
		{
			var testAllowSort = new ComplianceReportsSetupCategoryCollectionForTestAllowSort();
			Assert(!testAllowSort.AllowSortForTest);
		}

		class ComplianceReportsSetupCategoryCollectionForTestAllowSort : ComplianceReportsSetupCategoryCollection
		{
			public bool AllowSortForTest
			{
				get { return AllowSort; }
			}
		}

		#region Implementation
		ComplianceReportsSetupCategoryCollection list;
		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ComplianceReportsSetupCategoryCollection GetCollectionToTest()
		{
			return new ComplianceReportsSetupCategoryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceReportsSetupCategory();
		}

		#endregion
	}
}
