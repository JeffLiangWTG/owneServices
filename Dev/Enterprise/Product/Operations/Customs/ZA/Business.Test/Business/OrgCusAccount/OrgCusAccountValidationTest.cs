using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class OrgCusAccountValidationTest : TestCaseWithFactory
	{
		public void TestCheckCZ_Account()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var testAgentForFAN = Factory.New<OrgHeader>();
			testAgentForFAN.OH_FullName = "buyer";
			testAgentForFAN.OH_Code = "IMP#@$43";
			testAgentForFAN.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testAgentForFAN2 = Factory.New<OrgHeader>();
			testAgentForFAN2.OH_FullName = "buyer";
			testAgentForFAN2.OH_Code = "IMP#@$44";
			testAgentForFAN2.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testAgentForFAN3 = Factory.New<OrgHeader>();
			testAgentForFAN3.OH_FullName = "buyer";
			testAgentForFAN3.OH_Code = "IMP#@$45";
			testAgentForFAN3.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditor1 = Factory.New<OrgHeader>();
			testCreditor1.OH_FullName = "buyer";
			testCreditor1.OH_Code = "IMP#@$55";
			testCreditor1.OH_IsCreditor = true;
			var testCreditor2 = Factory.New<OrgHeader>();
			testCreditor2.OH_FullName = "buyer";
			testCreditor2.OH_Code = "IMP#@$56";
			testCreditor2.OH_IsCreditor = true;
			var testCreditor3 = Factory.New<OrgHeader>();
			testCreditor3.OH_FullName = "buyer";
			testCreditor3.OH_Code = "IMP#@$57";
			testCreditor3.OH_IsCreditor = true;
			Factory.Save();
			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			var mapping1 = agentOfficeToCreditorMappings.AddNew();
			mapping1.OrganizationPK = testAgentForFAN.PK;
			mapping1.CustomsOfficeCode = "JHB";
			mapping1.FinancialAccountNumber = "1234567890";
			mapping1.CreditorPK = testCreditor1.PK;
			mapping1.ImporterPays = false;
			mapping1.AccountStartDay = 1;
			var mapping2 = agentOfficeToCreditorMappings.AddNew();
			mapping2.OrganizationPK = testAgentForFAN2.PK;
			mapping2.CustomsOfficeCode = "BBR";
			mapping2.FinancialAccountNumber = "1234567899";
			mapping2.CreditorPK = testCreditor2.PK;
			mapping2.ImporterPays = false;
			mapping2.AccountStartDay = 1;
			var mapping3 = agentOfficeToCreditorMappings.AddNew();
			mapping3.OrganizationPK = testAgentForFAN3.PK;
			mapping3.CustomsOfficeCode = "BBR";
			mapping3.FinancialAccountNumber = "1234567800";
			mapping3.CreditorPK = testCreditor3.PK;
			mapping3.ImporterPays = false;
			mapping3.AccountStartDay = 1;
			using (ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings))
			{
				var wrapper = OrgHeaderWrapper.New(testAgentForFAN);
				var fan1 = wrapper.FinancialAccountNumbers.AddNew();
				ValidationTestHelper.AssertErrorIfNotEntered(fan1.CZ_AccountInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(fan1.CZ_AccountInfo, "XX", "1234567890");
				fan1.CZ_Account = "1234567890";
				var fan2 = wrapper.FinancialAccountNumbers.AddNew();
				fan2.CZ_Account = "1234567890";
				AssertHasError(fan2.CZ_AccountInfo, "This number is duplicated. Only one occurrence of each FAN is allowed.");
				fan2.CZ_Account = "1234567899";
				AssertNoErrors(fan2.CZ_AccountInfo);
				var fan3 = wrapper.FinancialAccountNumbers.AddNew();
				fan3.CZ_Account = "1234567800";
				AssertHasError(fan3.CZ_AccountInfo, "Cannot have two (or more) FAN Numbers against an organization for the same Customs office.");
			}
		}

		public void TestCZ_Password()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(orgHeader);
			var orgCusAccount = wrapper.FinancialAccountNumbers.AddNew();
			AssertEquals("Collection sets Default Value so that Provider is determined at an early stage", Core.Constants.CountryCodes.SouthAfrica, orgCusAccount.CZ_RN_NKCountryCode);
			orgCusAccount.RunPreSaveValidation();
			AssertNoErrors(orgCusAccount.CZ_CodeInfo);
		}

		public void TestCZ_Code()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(orgHeader);
			var orgCusAccount = wrapper.FinancialAccountNumbers.AddNew();
			orgCusAccount.CZ_Code = "XXX";
			AssertHasErrorContaining(orgCusAccount.CZ_CodeInfo, ListValidation.InvalidCodeError);
			orgCusAccount.CZ_Code = "FAN";
			AssertNoErrorContaining(orgCusAccount.CZ_CodeInfo, ListValidation.InvalidCodeError);
		}
	}
}
