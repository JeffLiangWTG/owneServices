using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class OrgCusAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var fan = Factory.New<MasterFiles.Business.OrgCusAccount>();
			fan.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(1, fan.Lookups.CodeList.Count);
			AssertEquals(OrgCusAccountProvider.FANCode, fan.Lookups.CodeList[0].Code);
			AssertEquals(OrgCusAccountProvider.FANDesc, fan.Lookups.CodeList[0].Description);
		}

		public void TestFinancialAccountNumberList()
		{
			var orgheader = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			orgheader.CompanyData.OB_IsCreditor = true;
			orgheader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.CodeTypes.AgentCode, "AST11", Core.Constants.CountryCodes.SouthAfrica);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("ABC");
			testHelper.CreateCustomsOfficeCusCodeEntry("CDE", "Office CDE");
			testHelper.CreateCustomsOfficeCusCodeEntry("XYZ", "Office XYZ");
			testHelper.CreateCustomsOfficeCusCodeEntry("ZZZ", "Office ZZZ");
			Factory.Save();
			var maps = new FinancialAccountNumberPortMapCollection();
			var fansForTesting = new string[][]
			{
				new string[] { "0000000001", "ABC", "ABC" },
				new string[] { "0000000002", "CDE", "Office CDE" },
				new string[] { "0000000003", "XYZ", "Office XYZ" },
				new string[] { "0000000004", "ZZZ", "Office ZZZ" }
			};
			for (int mappingIndex = 0; mappingIndex < fansForTesting.Length; mappingIndex++)
			{
				var mapping = maps.AddNew();
				mapping.AccountStartDay = 1;
				mapping.ImporterPays = false;
				mapping.OrganizationPK = orgheader.PK;
				mapping.CreditorPK = orgheader.PK;
				mapping.CustomsOfficeCode = fansForTesting[mappingIndex][1];
				mapping.FinancialAccountNumber = fansForTesting[mappingIndex][0];
			}

			using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps))
			{
				var fan = Factory.New<MasterFiles.Business.OrgCusAccount>();
				fan.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				var fanList = ((OrgCusAccountLookups)fan.Lookups).AccountList;
				AssertEquals(4, fanList.Count);
				Assert(fanList.ContainsCode("0000000001"));
				Assert(fanList.ContainsCode("0000000002"));
				Assert(fanList.ContainsCode("0000000003"));
				Assert(fanList.ContainsCode("0000000004"));
			}
		}
	}
}
