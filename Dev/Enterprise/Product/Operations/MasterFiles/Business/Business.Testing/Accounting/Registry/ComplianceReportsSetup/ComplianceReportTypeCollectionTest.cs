using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportTypeCollection))]
	sealed class ComplianceReportTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceReportTypeCollection>
	{
		public void TestAddDefaultReportTypes()
		{
			list = new ComplianceReportTypeCollection("Test0");
			AssertEquals("Test0", list.CountryCode);

			list = new ComplianceReportTypeCollection();
			list.AddDefaultReportTypes(Constants.CountryCodes.China);
			AssertEquals(Constants.CountryCodes.China, list.CountryCode);
			AssertEquals(8, list.Count);
			AssertEquals("BSH.D01, BSH.D02, BSH.D03, BSH.D04, BSH.D05, BSH.D06, BSH.D07, BSH.D08, BSH.D09, BSH.D10, BSH.D11, BSH.D12, BSH.D13, BSH.D14, BSH.D20, BSH.D21, BSH.D22, BSH.D23, BSH.D24, BSH.D25, BSH.D26, BSH.D27, BSH.D28, BSH.D30, BSH.D32, BSH.D33, BSH.D34, BSH.D35, BSH.D36, BSH.D37, BSH.D38, BSH.D39, BSH.D40, BSH.D41, BSH.D42, BSH.D43, BSH.D44, BSH.D45, BSH.D46," +
						" BSH.H01, BSH.H02, BSH.H03, BSH.H04, BSH.H05, BSH.H06, BSH.H07, BSH.H08, BSH.H09, BSH.H10, BSH.H11, BSH.H12, BSH.H13, BSH.H14, BSH.H15, BSH.H16, BSH.H30, BSH.H31, BSH.H32, BSH.H33, BSH.H34, BSH.H35, BSH.H36, BSH.H37, BSH.H38, BSH.H41, BSH.H42, BSH.H43, BSH.H44, BSH.H45, BSH.H46, BSH.H47, BSH.H48, BSH.H49, BSH.H50, BSH.H51, BSH.H52, BSH.H53, BSH.H54, BSH.H55, BSH.H57, BSH.H59, BSH.XXX," +
						" P&L.D02, P&L.D03, P&L.D05, P&L.D06, P&L.D07, P&L.D08, P&L.D09, P&L.D10, P&L.D11, P&L.D12, P&L.D13, P&L.D14, P&L.D15, P&L.D16, P&L.D17, P&L.D18, P&L.D19, P&L.D20, P&L.H22, P&L.H23, P&L.H24, P&L.H25, P&L.H26, P&L.H27, P&L.H28, P&L.H29, P&L.H30, P&L.H32, P&L.H33, P&L.H35, P&L.H38, P&L.H39, P&L.XXX," +
						" PLM.D01, PLM.D04, PLM.D05, PLM.D11, PLM.D14, PLM.D15, PLM.D16, PLM.D19, PLM.D22, PLM.D23, PLM.D25, PLM.D28, PLM.D29, PLM.D30, PLM.XXX," +
						" VAT.V02, VAT.V03, VAT.V04, VAT.V05, VAT.V08, VAT.V09, VAT.V10, VAT.V11, VAT.V12, VAT.V18, VAT.XXX," +
						" SPA.P02, SPA.P03, SPA.P05, SPA.P06, SPA.P08, SPA.P09, SPA.P11, SPA.P12, SPA.P14, SPA.P15, SPA.P17, SPA.P18, SPA.P19, SPA.P20, SPA.XXX," +
						" PLA.A02, PLA.A03, PLA.A04, PLA.A05, PLA.A06, PLA.A07, PLA.A08, PLA.A09, PLA.A10, PLA.A11, PLA.A12, PLA.A13, PLA.XXX," +
						" SSE.B03, SSE.B04, SSE.B05, SSE.B06, SSE.B18, SSE.B19, SSE.B20, SSE.B21, SSE.B22, SSE.B23, SSE.B30, SSE.B41, SSE.B49, SSE.B50, SSE.B51, SSE.B52, SSE.B53, SSE.B54, SSE.B56, SSE.B57, SSE.B58, SSE.B59, SSE.B64, SSE.B65, SSE.B66, SSE.B69, SSE.B72, SSE.B79, SSE.XXX," +
						" TT0.A01, TT0.A02, TT0.A03, TT0.A04, TT0.XXX", list.CodesAsString);
		}

		public void TestAddNewAndGetElement()
		{
			list = new ComplianceReportTypeCollection("Test0");
			ComplianceReportType complianceReportType = list.AddNew();
			complianceReportType.ReportType = "ABC";
			complianceReportType.CountryCode = "Test0";
			AssertEquals(2, list.Count);
			AssertEquals(complianceReportType, list[1]);
			AssertEquals(complianceReportType, list["ABC"]);
		}

		public void TestCodesAsString()
		{
			list = new ComplianceReportTypeCollection();
			AssertEquals(ZString.Empty, list.CodesAsString);
			ComplianceReportType complianceReportType = list.AddNew();
			complianceReportType.ReportType = "ABC";
			AssertEquals("ABC.XXX", list.CodesAsString);
			ComplianceReportType complianceReportType1 = list.AddNew();
			complianceReportType1.ReportType = "EFG";

			AssertEquals("ABC.XXX, EFG.XXX", list.CodesAsString);

			list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
			AssertEquals("BSH.D01, BSH.D02, BSH.D03, BSH.D04, BSH.D05, BSH.D06, BSH.D07, BSH.D08, BSH.D09, BSH.D10, BSH.D11, BSH.D12, BSH.D13, BSH.D14, BSH.D20, BSH.D21, BSH.D22, BSH.D23, BSH.D24, BSH.D25, BSH.D26, BSH.D27, BSH.D28, BSH.D30, BSH.D32, BSH.D33, BSH.D34, BSH.D35, BSH.D36, BSH.D37, BSH.D38, BSH.D39, BSH.D40, BSH.D41, BSH.D42, BSH.D43, BSH.D44, BSH.D45, BSH.D46," +
							" BSH.H01, BSH.H02, BSH.H03, BSH.H04, BSH.H05, BSH.H06, BSH.H07, BSH.H08, BSH.H09, BSH.H10, BSH.H11, BSH.H12, BSH.H13, BSH.H14, BSH.H15, BSH.H16, BSH.H30, BSH.H31, BSH.H32, BSH.H33, BSH.H34, BSH.H35, BSH.H36, BSH.H37, BSH.H38, BSH.H41, BSH.H42, BSH.H43, BSH.H44, BSH.H45, BSH.H46, BSH.H47, BSH.H48, BSH.H49, BSH.H50, BSH.H51, BSH.H52, BSH.H53, BSH.H54, BSH.H55, BSH.H57, BSH.H59, BSH.XXX," +
							" P&L.D02, P&L.D03, P&L.D05, P&L.D06, P&L.D07, P&L.D08, P&L.D09, P&L.D10, P&L.D11, P&L.D12, P&L.D13, P&L.D14, P&L.D15, P&L.D16, P&L.D17, P&L.D18, P&L.D19, P&L.D20, P&L.H22, P&L.H23, P&L.H24, P&L.H25, P&L.H26, P&L.H27, P&L.H28, P&L.H29, P&L.H30, P&L.H32, P&L.H33, P&L.H35, P&L.H38, P&L.H39, P&L.XXX," +
							" PLM.D01, PLM.D04, PLM.D05, PLM.D11, PLM.D14, PLM.D15, PLM.D16, PLM.D19, PLM.D22, PLM.D23, PLM.D25, PLM.D28, PLM.D29, PLM.D30, PLM.XXX," +
							" VAT.V02, VAT.V03, VAT.V04, VAT.V05, VAT.V08, VAT.V09, VAT.V10, VAT.V11, VAT.V12, VAT.V18, VAT.XXX," +
							" SPA.P02, SPA.P03, SPA.P05, SPA.P06, SPA.P08, SPA.P09, SPA.P11, SPA.P12, SPA.P14, SPA.P15, SPA.P17, SPA.P18, SPA.P19, SPA.P20, SPA.XXX," +
							" PLA.A02, PLA.A03, PLA.A04, PLA.A05, PLA.A06, PLA.A07, PLA.A08, PLA.A09, PLA.A10, PLA.A11, PLA.A12, PLA.A13, PLA.XXX," +
							" SSE.B03, SSE.B04, SSE.B05, SSE.B06, SSE.B18, SSE.B19, SSE.B20, SSE.B21, SSE.B22, SSE.B23, SSE.B30, SSE.B41, SSE.B49, SSE.B50, SSE.B51, SSE.B52, SSE.B53, SSE.B54, SSE.B56, SSE.B57, SSE.B58, SSE.B59, SSE.B64, SSE.B65, SSE.B66, SSE.B69, SSE.B72, SSE.B79, SSE.XXX," +
							" TT0.A01, TT0.A02, TT0.A03, TT0.A04, TT0.XXX", list.CodesAsString);
		}

		public void TestDefaultReportTypes()
		{
			var defaultReportTypes = new Dictionary<string, string>();
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.BalanceSheet);
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.ProfitAndLoss);
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.ProfitAndLossMonthly);
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.VATDetailed);
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.AssetProvision);
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.P_LAppropriation);
			defaultReportTypes.Add(AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement, AccountingMasterFilesConstants.ReportDescriptionsOfLocalReport.EquityMovement);
			defaultReportTypes.Add("TT0", "Test Type 0");

			list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);

			AssertEquals(8, list.DefaultReportTypes.Count);
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet));
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss));
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly));
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed));
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision));
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation));
			Assert(list.DefaultReportTypes.ContainsKey(AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement));
			Assert(list.DefaultReportTypes.ContainsKey("TT0"));
		}

		public void DefaultReportTypesAsString()
		{
			list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);

			AssertEquals("BSH, P&L, PLM, VAT, SPA, PLA, SSE, TT0", list.DefaultReportTypesAsString);
		}

		public void TestGetDescriptionFromCode()
		{
			list = new ComplianceReportTypeCollection("Test0");
			AssertEquals("Balance Sheet", list.GetDescriptionFromCode("BSH"));
		}

		public void TestGetReportTypeCategoriesFromCode()
		{
			list = new ComplianceReportTypeCollection("Test0");
			AssertEquals(4, list.GetReportTypeCategoriesFromCode("BSH").Count);
		}
		#region Implementation
		ComplianceReportTypeCollection list;
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

		protected override ComplianceReportTypeCollection GetCollectionToTest()
		{
			return new ComplianceReportTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceReportType();
		}

		#endregion
	}
}
