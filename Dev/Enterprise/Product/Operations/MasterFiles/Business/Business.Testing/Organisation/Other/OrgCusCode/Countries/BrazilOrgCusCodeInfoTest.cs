using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BrazilOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Brazil;

		public void TestGetCodesCannotCoexistInParentCountry()
		{
			var conflictingCodes = new List<ZString> { BrazilOrgCusCodeInfo.OrgCusCodes.RSN, BrazilOrgCusCodeInfo.OrgCusCodes.RLR, BrazilOrgCusCodeInfo.OrgCusCodes.RLP, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ };
			var notConflictingCode = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;

			var addCusCode1 = Factory.NewWithValidTestData<OrgCusCode>();
			var addCusCode2 = Factory.NewWithValidTestData<OrgCusCode>();

			addCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			addCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;

			foreach (var code1 in conflictingCodes)
			{
				addCusCode1.OK_CodeType = code1;
				AssertNoErrors(addCusCode1.OK_CodeTypeInfo);

				foreach (var code2 in conflictingCodes)
				{
					addCusCode2.OK_CodeType = code2;
					AssertHasErrors("Expected Error message", addCusCode2.OK_CodeTypeInfo);

					addCusCode2.OK_CodeType = notConflictingCode;
					AssertNoErrors(addCusCode2.OK_CodeTypeInfo);
				}
			}
		}

		public void TestCNPJValidation()
		{
			var orgCusCode = CreateOrgCusCode();
			orgCusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;

			orgCusCode.OK_CustomsRegNo = "45116822000190";
			AssertHasError(orgCusCode.OK_CustomsRegNoInfo, "The entered CNPJ is not valid.\r\n\r\nA CNPJ is a specially provided number built with a checksum to ensure its validity. An error on this field indicates that the given number is DEFINITELY INVALID.");

			orgCusCode.OK_CustomsRegNo = "27.383.104/0001-89";
			AssertNoErrors(orgCusCode.OK_CustomsRegNoInfo);
		}

		public void TestMunicipalRegistrationTaxAuthorityValidation()
		{
			var orgCusCode1 = CreateOrgCusCode();
			orgCusCode1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority;

			orgCusCode1.OK_CustomsRegNo = "12345678";
			AssertNull("Precondition:", OrgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration, CountryCodes.Brazil));
			AssertHasWarning(orgCusCode1.OK_CustomsRegNoInfo, "IMM codes should only be recorded when there is an IMF code.");

			var orgCusCode2 = OrgHeader.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = CountryCodes.Brazil;
			orgCusCode2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration;
			AssertNotNull("Precondition:", OrgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration, CountryCodes.Brazil));

			orgCusCode1.OK_CustomsRegNo = "44448888";
			AssertNoWarnings(orgCusCode1.OK_CustomsRegNoInfo);
		}

		public void TestCustomsRegNoLookupList_WhenIMM()
		{
			var expected_BRMunicipalTaxAuthorityLookupList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("BRM1", "BR MUNICIPAL - MUN1"),
				new CodeDescriptionPair("BRM2", "BR MUNICIPAL - MUN2")
			};

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.GetTaxAuthorities(It.IsAny<ZString>(), It.IsAny<ZString?>())).Returns(expected_BRMunicipalTaxAuthorityLookupList);

			var orgCusCode = CreateOrgCusCode();
			AssertNotEquals("Precondition:", BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, orgCusCode.OK_CodeType);
			AssertNotEquals("When OrgCusCodeType Is NOT MunicipalRegistrationTaxAuthority (IMM)", expected_BRMunicipalTaxAuthorityLookupList, orgCusCode.CustomsRegNoLookupList);

			orgCusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority;
			AssertEquals("Precondition:", BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, orgCusCode.OK_CodeType);
			AssertEquals("When OrgCusCodeType Is MunicipalRegistrationTaxAuthority (IMM)", expected_BRMunicipalTaxAuthorityLookupList, orgCusCode.CustomsRegNoLookupList);
		}

		public void TestCustomsRegNoLookupListInvokesGetTaxAuthorities_WhenIMM()
		{
			var orgCusCode = CreateOrgCusCode();
			AssertNotEquals("Precondition:", BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, orgCusCode.OK_CodeType);

			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			_ = orgCusCode.CustomsRegNoLookupList;
			mockITaxFrameworkConfigurationHelper.Verify(x => x.GetTaxAuthorities(It.IsAny<ZString>(), It.IsAny<ZString?>()), Times.Never);

			orgCusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority;
			AssertEquals("Precondition:", BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, orgCusCode.OK_CodeType);

			_ = orgCusCode.CustomsRegNoLookupList;
			mockITaxFrameworkConfigurationHelper.Verify(x => x.GetTaxAuthorities(CountryCodes.Brazil, AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Municipal.Code));
		}

		public void TestCustomsRegNoFieldType_WhenIMM()
		{
			var orgCusCode = CreateOrgCusCode();
			AssertNotEquals("Precondition:", BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, orgCusCode.OK_CodeType);
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.Text), orgCusCode.CustomsRegNoFieldType);

			orgCusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority;
			AssertEquals("Precondition:", BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalRegistrationTaxAuthority, orgCusCode.OK_CodeType);
			AssertEquals("CustomsRegNoFieldType", nameof(FieldType.TextDropEdit), orgCusCode.CustomsRegNoFieldType);
		}

		OrgCusCode CreateOrgCusCode()
		{
			OrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = OrgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;

			return orgCusCode;
		}
		OrgHeader OrgHeader;
	}
}
