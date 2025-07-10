using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class OrgCusCodeValidatorTestHelper
	{
		static void SetupStrictEnforcementOfRegistrationNumberFormatsRegistry(ZString codeToTick, bool tickAll = false)
		{
			var collection = OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.Value;

			for (var i = 0; i < collection.Count; i++)
			{
				var item = collection[i];
				item.Bool = tickAll || (item.Code == codeToTick);
			}

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public static void AssertValidation(ZString country, ZString codeType, ZString[] invalidValue, ZString[] validValue, ZString expectedMessage, string registryCodeType = null, bool isOnlyWarning = false)
		{
			if (invalidValue.Length == 0 || validValue.Length == 0)
			{
				Assertion.Fail("Test mast have at least 1 invalid and 1 valid value to make any sense.");
			}

			var factory = new BusinessObjectFactory();
			var orgCusCode = factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = country;
			orgCusCode.OK_CodeType = codeType;

			if (!isOnlyWarning)
			{
				SetupStrictEnforcementOfRegistrationNumberFormatsRegistry(registryCodeType);
				foreach (var currentCode in invalidValue)
				{
					orgCusCode.OK_CustomsRegNo = currentCode;
					TestCaseWithFactory.AssertHasError("OrgCusCode Should be have an Error Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
				}
			}

			SetupStrictEnforcementOfRegistrationNumberFormatsRegistry("", tickAll: isOnlyWarning);
			foreach (var currentCode in invalidValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				TestCaseWithFactory.AssertHasWarning("OrgCusCode Should be have a Warning Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}

			foreach (var currentCode in validValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				TestCaseWithFactory.AssertNoWarnings("Assertion shouldn't have a warning message!", orgCusCode.OK_CustomsRegNoInfo);
				TestCaseWithFactory.AssertNoErrors("Assertion shouldn't have an error message!", orgCusCode.OK_CustomsRegNoInfo);
			}
		}

		public static void AssertValidation_VNVAT(ZString country, ZString codeType, ZString[] invalidValue, ZString[] validValue, ZString expectedMessage, string registryCodeType = null)
		{
			if (invalidValue.Length == 0 || validValue.Length == 0)
			{
				Assertion.Fail("Test mast have at least 1 invalid and 1 valid value to make any sense.");
			}

			var factory = new BusinessObjectFactory();
			var orgCusCode = factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = country;
			orgCusCode.OK_CodeType = codeType;

			SetupStrictEnforcementOfRegistrationNumberFormatsRegistry(registryCodeType);
			foreach (var currentCode in invalidValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				TestCaseWithFactory.AssertHasErrorContaining("OrgCusCode Should be contains an Error Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}

			foreach (var currentCode in validValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				TestCaseWithFactory.AssertNoErrorContaining("Assertion shouldn't contains an error message!", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}

			SetupStrictEnforcementOfRegistrationNumberFormatsRegistry("");
			foreach (var currentCode in invalidValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				TestCaseWithFactory.AssertHasWarningContaining("OrgCusCode Should be have a Warning Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}

			foreach (var currentCode in validValue)
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				TestCaseWithFactory.AssertNoWarningContaining("Assertion shouldn't contains an warning message!", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);
			}
		}
	}
}
