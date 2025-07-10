using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_RuleCode_Mandatory()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_RuleCode = ZString.Empty;
				AssertHasErrorContaining("Empty", cusAuthorisationRule.CPR_RuleCodeInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertNoErrorContaining("Entered", cusAuthorisationRule.CPR_RuleCodeInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCPR_RuleCode_List()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_RuleCode = "XYZ";
				AssertHasErrorContaining("Invalid", cusAuthorisationRule.CPR_RuleCodeInfo, ListValidation.InvalidCodeError);

				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertNoErrorContaining("Valid", cusAuthorisationRule.CPR_RuleCodeInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckCPR_ValueFrom_Mandatory()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = ZString.Empty;
				AssertHasErrorContaining("Empty", cusAuthorisationRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationRule.CPR_ValueFrom = "A003";
				AssertNoErrorContaining("Entered", cusAuthorisationRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCPR_ValueFrom_Location_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "A003", "ZA Facility", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			CombineAssertions(() =>
			{
				cusAuthorisationRule.CPR_ValueFrom = "B045";
				AssertHasError("Invalid", cusAuthorisationRule.CPR_ValueFromInfo, "Enter a valid Value.");

				cusAuthorisationRule.CPR_ValueFrom = "A003";
				AssertNoError("Valid", cusAuthorisationRule.CPR_ValueFromInfo, "Enter a valid Value.");
			});
		}

		public void TestCheckCPR_ValueFrom_AdditionalValidatorOnValue()
		{
			var mockedCusAuthorizationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			var authorizationHeader = cusAuthorisationRule.AuthorisationHeader;
			authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorizationHeaderProviderType))
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				cusAuthorisationRule.CPR_RuleCode = "AAA";
				cusAuthorisationRule.Validation.ValidateCPR_ValueFrom();
				AssertHasErrorContaining("HasError", cusAuthorisationRule.CPR_ValueFromInfo, "Error");

				cusAuthorisationRule.CPR_RuleCode = "BBB";
				cusAuthorisationRule.Validation.ValidateCPR_ValueFrom();
				AssertHasMessageErrorContaining("HasMessageError", cusAuthorisationRule.CPR_ValueFromInfo, "MessageError");

				cusAuthorisationRule.CPR_RuleCode = "CCC";
				cusAuthorisationRule.Validation.ValidateCPR_ValueFrom();
				AssertHasWarningContaining("HasWarning", cusAuthorisationRule.CPR_ValueFromInfo, "Warning");

				cusAuthorisationRule.CPR_RuleCode = "DDD";
				cusAuthorisationRule.Validation.ValidateCPR_ValueFrom();
				AssertNoErrorContaining("NoError", cusAuthorisationRule.CPR_ValueFromInfo, "Error");

				cusAuthorisationRule.CPR_RuleCode = "MLT";
				cusAuthorisationRule.Validation.ValidateCPR_ValueFrom();
				AssertHasMessageErrorContaining("HasMessageError", cusAuthorisationRule.CPR_ValueFromInfo, "MessageError");
				AssertHasErrorContaining("HasError", cusAuthorisationRule.CPR_ValueFromInfo, "Error");
			}
		}

		public void TestCheckCPR_RuleCode_AuthorisationRuleRepetitions()
		{
			var mockedCusAuthorizationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			var authorizationHeader = cusAuthorisationRule.AuthorisationHeader;
			authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorizationHeaderProviderType))
			{
				const string youAreAllowedToHaveAMaximumOf = "You are allowed to have a maximum of";
				CombineAssertions(() =>
				{
					authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument;
					cusAuthorisationRule.Validation.ValidateCPR_RuleCode();
					AssertNoErrorContaining("No rule XXX", cusAuthorisationRule.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);

					cusAuthorisationRule.CPR_RuleCode = "XXX";
					AssertNoErrorContaining("Added rule XXX", cusAuthorisationRule.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);

					var rule2 = authorizationHeader.CusAuthorisationRules.AddNew();
					rule2.CPR_RuleCode = "XXX";
					AssertHasErrorContaining("More than MaxAllowed", rule2.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);

					authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
					rule2.Validation.ValidateCPR_RuleCode();
					AssertNoErrorContaining("MaxAllowed 0", rule2.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);
				});
			}
		}

		public void TestCheckCPR_RuleCode_LinkedAuthorisationRuleRepetitions_LessThanMinRequired()
		{
			var mockedCusAuthorizationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorizationHeaderProviderType))
			{
				const string youAreRequiredToHaveAMinimumOf = "You are required to have a minimum of";
				CombineAssertions(() =>
				{
					cusAuthorisationRule.CPR_RuleCode = "LOC";
					AssertHasErrorContaining("Less than MinRequired", cusAuthorisationRule.CPR_RuleCodeInfo, youAreRequiredToHaveAMinimumOf);

					var rule1 = cusAuthorisationRule.LinkedCusAuthorisationRules.AddNew();
					rule1.CPR_RuleCode = "XXX";
					cusAuthorisationRule.Validation.ValidateCPR_RuleCode();
					AssertNoErrorContaining("Added linked rule XXX", cusAuthorisationRule.CPR_RuleCodeInfo, youAreRequiredToHaveAMinimumOf);
				});
			}
		}

		public void TestCheckCPR_RuleCode_LinkedAuthorisationRuleRepetitions_MinRequiredSameAsMaxAllowed()
		{
			var mockedCusAuthorizationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorizationHeaderProviderType))
			{
				const string youAreRequiredToHave = "You are required to have 1 linked rule(s)";
				CombineAssertions(() =>
				{
					cusAuthorisationRule.CPR_RuleCode = "USE";
					AssertHasErrorContaining("Less than MinRequired", cusAuthorisationRule.CPR_RuleCodeInfo, youAreRequiredToHave);

					var rule1 = cusAuthorisationRule.LinkedCusAuthorisationRules.AddNew();
					rule1.CPR_RuleCode = "XXX";
					cusAuthorisationRule.Validation.ValidateCPR_RuleCode();
					AssertNoErrorContaining("Added linked rule XXX", cusAuthorisationRule.CPR_RuleCodeInfo, youAreRequiredToHave);
				});
			}
		}

		public void TestCheckCPR_RuleCode_LinkedAuthorisationRuleRepetitions_MoreThanMaxAllowed()
		{
			var mockedCusAuthorizationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorizationHeaderProviderType))
			{
				const string youAreAllowedToHaveAMaximumOf = "You are allowed to have a maximum of";
				CombineAssertions(() =>
				{
					cusAuthorisationRule.CPR_RuleCode = "USE";
					var rule1 = cusAuthorisationRule.LinkedCusAuthorisationRules.AddNew();
					rule1.CPR_RuleCode = "XXX";
					cusAuthorisationRule.Validation.ValidateCPR_RuleCode();
					AssertNoErrorContaining("Less than MaxAllowed", cusAuthorisationRule.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);

					var rule2 = cusAuthorisationRule.LinkedCusAuthorisationRules.AddNew();
					rule2.CPR_RuleCode = "XXX";
					cusAuthorisationRule.Validation.ValidateCPR_RuleCode();
					AssertHasErrorContaining("More than MaxAllowed", cusAuthorisationRule.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);

					cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
					AssertNoErrorContaining("MaxAllowed 0", cusAuthorisationRule.CPR_RuleCodeInfo, youAreAllowedToHaveAMaximumOf);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
		}
		CusAuthorisationRule cusAuthorisationRule;
	}
}
