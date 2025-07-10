using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using SimilarOrgDataTable = Enterprise.MasterFiles.ReportTableProviders.SimilarOrganisationsReportDataset.SimilarOrganisationsDatasetDataTable;

namespace Enterprise.MasterFiles.ReportTableProviders.Testing
{
	internal class SimilarOrganisationsTableProviderTest : TestCaseWithFactory
	{
		#region Test Class

		public class TestSimilarOrganisationsTableProvider : SimilarOrganisationsTableProvider
		{
			protected override DataTable GetDataTable()
			{
				return new DataTable();
			}

			public new DataTable GetDataTable(string dataSourceString, CollectionOfIFilter filters, Report report)
			{
				return base.GetDataTable(dataSourceString, filters, report);
			}

			public new FilterOptions ReportFilterSettings
			{
				get { return base.ReportFilterSettings; }
			}

			public new void AddSimilarOrganisationsForOrg(SimilarOrgDataTable reportData, OrgHeader header, Dictionary<ZGuid, object> similarOrgsAlreadyShown)
			{
				base.AddSimilarOrganisationsForOrg(reportData, header, similarOrgsAlreadyShown);
			}

			public new void AddMatchToTempTable(SimilarOrgDataTable reportData, OrgHeader mainOrg, OrgPatternMatch matchToAdd)
			{
				base.AddMatchToTempTable(reportData, mainOrg, matchToAdd);
			}
		}

		#endregion

		public void TestAddSimilarOrganisationsForOrg()
		{
			OrgHeader mainHeader = OrgHeader.New(Factory);
			mainHeader.OH_FullName = "Main Full Name";
			mainHeader.OH_RL_NKClosestPort = "AUSYD";
			mainHeader.OH_Code = "MAIN";
			mainHeader.PrimaryRegistrationNumber.Number = "123456";
			mainHeader.MainAddress.OA_Address1 = "Main Address1";
			mainHeader.MainAddress.OA_City = "Main City";
			OrgAddress newAddress = mainHeader.Addresses.AddNew();
			newAddress.OA_Address1 = "Second Address1";
			newAddress.OA_City = "Second City";

			OrgHeader matchHeader = OrgHeader.New(Factory);
			matchHeader.OH_FullName = "Main Full Name";
			matchHeader.OH_RL_NKClosestPort = "AUBNE";
			matchHeader.OH_Code = "MATCH";
			matchHeader.PrimaryRegistrationNumber.Number = "123456";
			matchHeader.MainAddress.OA_Address1 = "Main Address1";
			matchHeader.MainAddress.OA_City = "Main City";
			newAddress = matchHeader.Addresses.AddNew();
			newAddress.OA_Address1 = "Second Address1";
			newAddress.OA_City = "Second City";

			Factory.Save();
			AssertEquals("Main Header has 2 pattern matches", 2, mainHeader.PatternMatchesForThisOrg.Count);
			AssertEquals("Match Header has 2 pattern matches", 2, matchHeader.PatternMatchesForThisOrg.Count);

			TestSimilarOrganisationsTableProvider provider = new TestSimilarOrganisationsTableProvider();
			SimilarOrgDataTable testTable = new SimilarOrgDataTable();

			provider.AddSimilarOrganisationsForOrg(testTable, mainHeader, new Dictionary<ZGuid, object>());

			AssertEquals("Only 1 record in the table", 1, testTable.Rows.Count);
		}

		public void TestAddMatchToTempTable()
		{
			TestSimilarOrganisationsTableProvider provider = new TestSimilarOrganisationsTableProvider();
			SimilarOrgDataTable testTable = new SimilarOrgDataTable();

			OrgHeader mainHeader = OrgHeader.New(Factory);
			mainHeader.OH_FullName = "Main Full Name";
			mainHeader.OH_RL_NKClosestPort = "AUSYD";
			mainHeader.OH_Code = "MAIN";
			mainHeader.PrimaryRegistrationNumber.Number = "123456";
			mainHeader.MainAddress.OA_Address1 = "Main Address1";
			mainHeader.MainAddress.OA_Address2 = "Main Address2";
			mainHeader.MainAddress.OA_City = "Main City";
			mainHeader.MainAddress.OA_State = "Main State";
			mainHeader.MainAddress.OA_PostCode = "12345";
			mainHeader.MainAddress.OA_Phone = "012345";
			mainHeader.MainAddress.OA_Fax = "4321";
			mainHeader.MainAddress.OA_Email = "main@address.com";
			mainHeader.MainWebURL.PU_URL = "http://www.testmainheader.com.org.biz";

			OrgHeader matchHeader = OrgHeader.New(Factory);
			matchHeader.OH_FullName = "Match Full Name";
			matchHeader.OH_RL_NKClosestPort = "AUBNE";
			matchHeader.OH_Code = "MATCH";
			matchHeader.PrimaryRegistrationNumber.Number = "00110011";
			matchHeader.MainAddress.OA_Address1 = "Match Address1";
			matchHeader.MainAddress.OA_Address2 = "Match  Address2";
			matchHeader.MainAddress.OA_City = "Match City";
			matchHeader.MainAddress.OA_State = "Match State";
			matchHeader.MainAddress.OA_PostCode = "54321";
			matchHeader.MainAddress.OA_Phone = "099876";
			matchHeader.MainAddress.OA_Fax = "768594032";
			matchHeader.MainAddress.OA_Email = "match@address.com";
			matchHeader.MainWebURL.PU_URL = "http://www.matchmatch.net.br.au";

			OrgPatternMatch testMatch = Factory.New<OrgPatternMatch>();
			testMatch.OS_OH = matchHeader.PK;
			testMatch.OS_OA = matchHeader.MainAddress.PK;
			testMatch.OS_BusinessRegNo = matchHeader.PrimaryRegistrationNumber.Number;
			testMatch.OS_Score = 120;

			AssertEquals("Table is empty", 0, testTable.Rows.Count);

			provider.AddMatchToTempTable(testTable, mainHeader, testMatch);
			AssertEquals("One row in table", 1, testTable.Rows.Count);
			AssertEquals("MainOrgCode Set", mainHeader.OH_Code, testTable[0].MainOrgCode);
			AssertEquals("MainOrgName Set", mainHeader.OH_FullName, testTable[0].MainOrgName);
			AssertEquals("MainBusinessRegNo Set", mainHeader.PrimaryRegistrationNumber.Number, testTable[0].MainBusinessRegNo);
			AssertEquals("MainUNLOCO Set", mainHeader.OH_RL_NKClosestPort, testTable[0].MainUNLOCO);
			AssertEquals("MainAddress1 Set", mainHeader.MainAddress.OA_Address1, testTable[0].MainAddress1);
			AssertEquals("MainAddress2 Set", mainHeader.MainAddress.OA_Address2, testTable[0].MainAddress2);
			AssertEquals("MainCity Set", mainHeader.MainAddress.OA_City, testTable[0].MainCity);
			AssertEquals("MainState Set", mainHeader.MainAddress.OA_State, testTable[0].MainState);
			AssertEquals("MainPostCode Set", mainHeader.MainAddress.OA_PostCode, testTable[0].MainPostCode);
			AssertEquals("MainPhone Set", mainHeader.MainAddress.OA_Phone, testTable[0].MainPhone);
			AssertEquals("MainFax Set", mainHeader.MainAddress.OA_Fax, testTable[0].MainFax);
			AssertEquals("MainEmail Set", mainHeader.MainAddress.OA_Email, testTable[0].MainEmail);
			AssertEquals("MainWeb Set", mainHeader.MainWebURL.PU_URL, testTable[0].MainWeb);

			AssertEquals("One row in table", 1, testTable.Rows.Count);
			AssertEquals("MatchOrgCode Set", matchHeader.OH_Code, testTable[0].SimilarOrgCode);
			AssertEquals("MatchOrgName Set", matchHeader.OH_FullName, testTable[0].SimilarOrgName);
			AssertEquals("MatchBusinessRegNo Set", matchHeader.PrimaryRegistrationNumber.Number, testTable[0].SimilarBusinessRegNo);
			AssertEquals("MatchUNLOCO Set", matchHeader.OH_RL_NKClosestPort, testTable[0].SimilarUNLOCO);
			AssertEquals("MatchAddress1 Set", matchHeader.MainAddress.OA_Address1, testTable[0].SimilarAddress1);
			AssertEquals("MainAddress2 Set", matchHeader.MainAddress.OA_Address2, testTable[0].SimilarAddress2);
			AssertEquals("MatchCity Set", matchHeader.MainAddress.OA_City, testTable[0].SimilarCity);
			AssertEquals("MatchState Set", matchHeader.MainAddress.OA_State, testTable[0].SimilarState);
			AssertEquals("MatchPostCode Set", matchHeader.MainAddress.OA_PostCode, testTable[0].SimilarPostCode);
			AssertEquals("MatchPhone Set", matchHeader.MainAddress.OA_Phone, testTable[0].SimilarPhone);
			AssertEquals("MatchFax Set", matchHeader.MainAddress.OA_Fax, testTable[0].SimilarFax);
			AssertEquals("MatchEmail Set", matchHeader.MainAddress.OA_Email, testTable[0].SimilarEmail);
			AssertEquals("MatchWeb Set", matchHeader.MainWebURL.PU_URL, testTable[0].SimilarWeb);
		}

		public void TestReportFilterSettings()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			TestSimilarOrganisationsTableProvider provider = new TestSimilarOrganisationsTableProvider();
			CollectionOfIFilter filters = new CollectionOfIFilter();

			ZGuid mainOrgPK = ZGuid.NewZGuid();
			ZGuid aRGroupPK = ZGuid.NewZGuid();
			ZGuid aPGroupPK = ZGuid.NewZGuid();

			AddParameterToFilter(filters, "MainOrgPK", mainOrgPK.ToGuid());
			AddParameterToFilter(filters, "MainBranchPK", GlbBranch.CurrentBranch.PK.ToGuid());
			AddParameterToFilter(filters, "MainUNLOCOPK", GlbBranch.CurrentBranch.OrgProxy.UNLOCO.PK.ToGuid());
			AddParameterToFilter(filters, "MainCountryPK", GlbBranch.CurrentBranch.OrgProxy.UNLOCO.Country.PK.ToGuid());
			AddParameterToFilter(filters, "IsNational", "Y");
			AddParameterToFilter(filters, "IsTemporary", "Y");
			AddParameterToFilter(filters, "OrgNameStartsWith", "ABB");
			AddParameterToFilter(filters, "MatchLikelihood", "HIG");
			AddParameterToFilter(filters, "ARGroupPK", aRGroupPK.ToGuid());
			AddParameterToFilter(filters, "APGroupPK", aPGroupPK.ToGuid());
			AddParameterToFilter(filters, "AccountType", "AR");
			AddParameterToFilter(filters, "Org. Added Date", new ZDateTime(2008, 1, 1), new ZDateTime(2008, 3, 1));

			provider.GetDataTable("SimilarOrganisations(<MainOrgPK>, <MainBranchPK>, <MainUNLOCOPK>, <MainCountryPK>, <IsNational>, <IsTemporary>, <OrgNameStartsWith>, <MatchLikelihood>, <ARGroupPK>, <APGroupPK>, <AccountType>, <Org. Added Date->FromDate>, <Org. Added Date->ToDate>)", filters, new Report(null, null));

			string expectedFilter = "OH_PK = CONVERT('" + mainOrgPK.ToString() + "', 'System.Guid') and OH_FullName like 'ABB%' and OH_RL_NKClosestPort = 'AUBNE' and OH_RL_NKClosestPort like 'AU%' and OH_IsNationalAccount = 1 and OH_IsTempAccount = 1 and " +
				"(OH_PK IN (SELECT OB_OH FROM dbo.OrgCompanyData WHERE OB_OJ_ARDebtorGroup = CONVERT('" + aRGroupPK.ToString() +
				"', 'System.Guid') and OB_OG_APCreditorGroup = CONVERT('" + aPGroupPK.ToString() + "', 'System.Guid') and OB_IsDebtor = 1 and OB_GB_ControllingBranch = CONVERT('27a55065-ac88-4ec3-8bed-e575e79172cb', 'System.Guid'))) and " +
				"(OH_PK IN (SELECT SL_Parent FROM dbo.StmALog WHERE SL_SE_NKEvent = '" + Events.AddedARecordToTheSystem.Code + "' and SL_PostedTimeUtc >= #2008-01-01 00:00:00.000# and SL_PostedTimeUtc < #2008-03-02 00:00:00.000#))";
			AssertEquals("All details have been successfully set", expectedFilter, provider.ReportFilterSettings.ToZQuery().LiteralTextADO);
		}

		public void TestSystemDefaultUnmatchedOrganizationShouldBeExcluded()
		{
			SystemDefinedOrganisation registry = new UnmatchedOrganisation();
			registry.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			Assert("Pre-condition: registry item use unmatched organization is turned on.", OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled);

			OrgHeader mainHeader = OrgHeader.New(Factory);
			mainHeader.OH_FullName = "Main Full Name";
			mainHeader.OH_RL_NKClosestPort = "AUSYD";
			mainHeader.OH_Code = "MAIN";
			mainHeader.PrimaryRegistrationNumber.Number = "123456";
			mainHeader.MainAddress.OA_Address1 = "Main Address1";
			mainHeader.MainAddress.OA_City = "Main City";
			OrgAddress newAddress = mainHeader.Addresses.AddNew();
			newAddress.OA_Address1 = "Second Address1";
			newAddress.OA_City = "Second City";

			Factory.Save();
			AssertEquals("Main Header has 2 pattern matches", 2, mainHeader.PatternMatchesForThisOrg.Count);

			TestSimilarOrganisationsTableProvider provider = new TestSimilarOrganisationsTableProvider();
			SimilarOrgDataTable testTable = new SimilarOrgDataTable();

			provider.AddSimilarOrganisationsForOrg(testTable, mainHeader, new Dictionary<ZGuid, object>());

			AssertEquals("No record in the table", 0, testTable.Rows.Count);
		}

		void AddParameterToFilter(CollectionOfIFilter filters, string name, object value)
		{
			if (value is Guid)
			{
				LookupField field = new LookupField(Factory);
				field.DisplayName = name;
				field.Value = (Guid)value;
				filters.Add(field);
			}
			else
			{
				TextField field = new TextField(Factory);
				field.DisplayName = name;
				field.Value = value.ToString();
				filters.Add(field);
			}
		}

		void AddParameterToFilter(CollectionOfIFilter filters, string name, ZDateTime fromDate, ZDateTime toDate)
		{
			DateRangeField field = new DateRangeField(Factory);
			field.DisplayName = name;
			field.ValueHigh = toDate;
			field.ValueLow = fromDate;
			filters.Add(field);
		}
	}
}
