using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	sealed class CusAuthorisationHeaderProviderBaseOnlyTest : CusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		public void TestGetByCountryCode()
		{
			var providerBR = CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.Brazil);
			var providerLV = CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.Latvia);
			var providerDE = CusAuthorisationHeaderProvider.GetByCountryCode(Core.Constants.CountryCodes.Germany);
			AssertType<CusAuthorisationHeaderProvider>("Default provider", providerBR);
			AssertEquals("Enterprise.Customs.EU.Business.CusAuthorisationHeaderProvider", providerLV.GetType().FullName);
			AssertEquals("Enterprise.Customs.DE.Business.CusAuthorisationHeaderProvider", providerDE.GetType().FullName);
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				AssertEquals($"Provider for {country}", "Enterprise.Customs.FR.Business.CusAuthorisationHeaderProvider", CusAuthorisationHeaderProvider.GetByCountryCode(country).GetType().FullName);
			}
		}

		public void TestDefaultEnableAdHoc()
		{
			AssertEquals("EnableAdHoc should be false", false, AuthorisationHeaderProvider.EnableAdHoc);
		}

		public void TestAllowMixedCaseAuthorisationNumbers()
		{
			AssertEquals("AllowMixedCaseAuthorisationNumbers should be false", false, AuthorisationHeaderProvider.AllowMixedCaseAuthorisationNumbers(authorisationHeader));
		}

		public void TestDefaultCPHNumber()
		{
			AssertEquals("DefaultTemporaryAuthorizationNumber should be Empty", ZString.Empty, AuthorisationHeaderProvider.DefaultTemporaryAuthorizationNumber);
		}

		public void TestGetRuleDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", ZString.Empty, AuthorisationHeaderProvider.GetRuleDescription(null));
				authorisationRule.CPR_RuleCode = "XYZ";
				authorisationRule.CPR_ValueFrom = "AA01";
				AssertEquals("No Addition in the Dictionary for rule", ZString.Empty, AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				authorisationRule.CPR_ValueFrom = "AA01";
				AssertEquals("Correct Description", "Loading place AA01", AuthorisationHeaderProvider.GetRuleDescription(authorisationRule));
			});
		}

		public void TestRuleCodesWithLinkedRules()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			var ruleCodesWithLinkedRules = AuthorisationHeaderProvider.RuleCodesWithLinkedRules(authorisationRule);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { CusAuthorisationRuleTypeList.Codes.Location }, ruleCodesWithLinkedRules);
				AssertSame("Cached", ruleCodesWithLinkedRules, AuthorisationHeaderProvider.RuleCodesWithLinkedRules(authorisationRule));
			});
		}

		public void TestGetLinkedRuleDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AA01", "Customs Office AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			var linkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", ZString.Empty, AuthorisationHeaderProvider.GetLinkedRuleDescription(null));
				linkedRule.CPR_RuleCode = "XYZ";
				linkedRule.CPR_ValueFrom = "AA01";
				AssertEquals("No Addition in the Dictionary for rule", ZString.Empty, AuthorisationHeaderProvider.GetLinkedRuleDescription(linkedRule));
				linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				AssertEquals("Correct Description", "Customs Office AA01", AuthorisationHeaderProvider.GetLinkedRuleDescription(linkedRule));
			});
		}

		public void TestGetLinkedRuleValueFromFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			var linkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", nameof(FieldType.Text), AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(null));
				linkedRule.CPR_RuleCode = "ABC";
				AssertEquals("Unknown RuleCode", nameof(FieldType.Text), AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedRule));
				linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				AssertEquals("RuleCode 'CUS'", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedRule));
			});
		}

		public void TestGetRuleValueFromFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(null));
				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("Unknown RuleCode", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("RuleCode 'LOC'", nameof(FieldType.TextCodeFindBox), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
			});
		}

		public void TestGetRuleDescriptionFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("No Rule", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleDescriptionFieldType(null));
				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("Unknown RuleCode", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule));
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("RuleCode 'LOC'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule));
			});
		}

		public void TestGetRuleValueFromMaxLength()
		{
			AssertEquals(CusAuthorisationRule.Schema.CPR_ValueFromMaxLength, AuthorisationHeaderProvider.GetRuleValueFromMaxLength(null));
		}

		public void TestGetValidRepititionsForAuthorisationRules()
		{
			var validRuleRepetitions = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationHeader.CPH_Type);
			CombineAssertions(() =>
			{
				AssertEquals("Count", 0, validRuleRepetitions.Count());
				AssertSame("Cached", validRuleRepetitions, AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationHeader.CPH_Type));
			});
		}

		public void TestAddOrUpdateValidRuleRepetitions()
		{
			var mockedCusAuthorisationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Australia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorisationHeaderProviderType))
			{
				var authorisationHeaderProvider = (CusAuthorisationHeaderProviderForTest)authorisationHeader.Provider;
				var ruleRequirementSDE = authorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration);
				var ruleRequirementETD = authorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument);
				CombineAssertions(() =>
				{
					AssertRuleRequirement("ZZZ", ruleRequirementSDE.Single(x => x.RuleType == "ZZZ"), 2, 2, false);
					AssertRuleRequirement("YYY", ruleRequirementSDE.Single(x => x.RuleType == "YYY"), 1, 1, true);
					AssertRuleRequirement("XXX", ruleRequirementSDE.Single(x => x.RuleType == "XXX"), 1, 0, false);
					AssertRuleRequirement("XXX", ruleRequirementETD.Single(x => x.RuleType == "XXX"), 1, 1, false);
				});
			}
		}

		public void TestAuthorizationTypesNeedAddress()
		{
			var authorizationTypesNeedAddress = AuthorisationHeaderProvider.AuthorizationTypesNeedAddress;
			CombineAssertions(() =>
			{
				AssertEquals("List", "CW1, CW2, CWP", string.Join(", ", authorizationTypesNeedAddress));
				AssertSame("Cached", authorizationTypesNeedAddress, AuthorisationHeaderProvider.AuthorizationTypesNeedAddress);
			});
		}

		public void TestGetValidRepetitionsForLinkedAuthorizationRules()
		{
			var validRuleRepetitions = AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Count", 0, validRuleRepetitions.Count);
				AssertSame("Cached", validRuleRepetitions, AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory));
			});
		}

		public void TestAddOrUpdateValidLinkedRuleRepetitions()
		{
			var mockedCusAuthorisationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Australia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorisationHeaderProviderType))
			{
				var authorisationHeaderProvider = (CusAuthorisationHeaderProviderForTest)authorisationHeader.Provider;
				var validLinkedRuleRepetitions = authorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory);
				var ruleRangesLOC = validLinkedRuleRepetitions[CusAuthorisationRuleTypeList.Codes.Location];
				var ruleRangesUSE = validLinkedRuleRepetitions["USE"];
				CombineAssertions(() =>
				{
					AssertLinkedRuleRange("ZZZ", ruleRangesLOC.Single(x => x.RuleType == "ZZZ"), 2, 2);
					AssertLinkedRuleRange("YYY", ruleRangesLOC.Single(x => x.RuleType == "YYY"), 1, 1);
					AssertLinkedRuleRange("XXX", ruleRangesLOC.Single(x => x.RuleType == "XXX"), 1, 0);
					AssertLinkedRuleRange("XXX", ruleRangesUSE.Single(x => x.RuleType == "XXX"), 1, 1);
				});
			}
		}

		public void TestIsAuthorisationNumberValid()
		{
			Assert(AuthorisationHeaderProvider.IsAuthorisationNumberValid(authorisationHeader));
		}

		public void TestAuthorisationNumberInvalidFormatMessage()
		{
			AssertEquals(ZString.Empty, AuthorisationHeaderProvider.GetAuthorisationNumberInvalidFormatMessage(authorisationHeader));
		}

		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => authorisationHeader.Provider;

		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Australia;
	}

	[TestsSubclassesOf(typeof(CusAuthorisationHeaderProvider))]
	public abstract class CusAuthorisationHeaderProviderAbstractTest<T> : TestCaseWithFactory
		where T : CusAuthorisationHeaderProvider
	{
		public void TestShowCustomsCode()
		{
			AssertEquals(ExpectedShowCustomsCode, AuthorisationHeaderProvider.ShowCustomsCode);
		}

		public void TestNoDuplicateRuleCodeInValidLinkedAuthorizationRuleRepetitions()
		{
			var validRuleRepetitions = AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory);
			if (validRuleRepetitions.Count == 0)
			{
				Assert("No duplicates due to empty Dictionary.", true);
			}
			else
			{
				CombineAssertions(() =>
				{
					foreach (var validRuleRepetition in validRuleRepetitions)
					{
						var rulesCount = validRuleRepetition.Value.Count;
						var distinctCount = validRuleRepetition.Value.Select(x => x.RuleType).Distinct().Count();
						AssertEquals($"{validRuleRepetition.Key}: Duplicate RuleCode in ValidRuleRepetitions", true, rulesCount == distinctCount);
					}
				});
			}
		}

		public void TestNoDuplicateRuleCodeInValidAuthorisationRuleRepititions()
		{
			var authorisationTypes = authorisationHeader.Lookups.AuthorisationTypeList.GetAllCodes().Append("KeyForGeneralRequirement");
			foreach (var authorisationType in authorisationTypes)
			{
				var validRuleRequirements = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationType);
				if (validRuleRequirements.Any())
				{
					var rulesCount = validRuleRequirements.Count();
					var distinctCount = IEnumerableExtensions.DistinctBy(
							validRuleRequirements,
							x => x.RuleType)
						.Count();
					AssertEquals($"{authorisationType}: Duplicate RuleCode in ValidRuleRepetitions", true, rulesCount == distinctCount);
				}
				else
				{
					Assert("No duplicates due to empty requirements.", true);
				}
			}
		}

		public void TestGetRuleCodeListForModule()
		{
			var ruleCodeList = AuthorisationHeaderProvider.GetRuleCodeListForModule(Factory);
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", ruleCodeList.GetAllCodes(), ExpectedRuleCodeListForModule.GetAllCodes());
				AssertSame("Cached", ruleCodeList, AuthorisationHeaderProvider.GetRuleCodeListForModule(Factory));
			});
		}

		public void TestGetAuthorisationTypeList()
		{
			var authorisationTypeList = AuthorisationHeaderProvider.GetAuthorisationTypeList(Factory);
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", authorisationTypeList.GetAllCodes(), ExpectedAuthorisationTypeList.GetAllCodes());
				AssertSame("Cached", authorisationTypeList, AuthorisationHeaderProvider.GetAuthorisationTypeList(Factory));
			});
		}

		protected void AssertRuleRequirement(ZString testCase, CusAuthorisationRuleRequirement ruleRequirement, ZInt expectedMinRequired, ZInt expectedMaxAllowed, ZBool hasAdditionalValidation)
		{
			AssertEquals($"{testCase}: min required", expectedMinRequired, ruleRequirement.MinRequired);
			AssertEquals($"{testCase}: max allowed", expectedMaxAllowed, ruleRequirement.MaxAllowed);
			AssertEquals($"{testCase}: additional Validation", hasAdditionalValidation, ruleRequirement.AdditionalMinAuthorisationCheck != null);
		}

		protected void AssertRuleRequirement(ZString testCase, CusAuthorisationRuleRequirement ruleRequirement, ZInt expectedMinRequired, ZInt expectedMaxAllowed, ZBool hasAdditionalValidation,
			string validValue, string invalidValue, string expectedValidationMessage, CargoWise.ComponentModel.INotificationType expectedNotificationType)
		{
			AssertRuleRequirement(testCase, ruleRequirement, expectedMinRequired, expectedMaxAllowed, hasAdditionalValidation);

			var validator = ruleRequirement.AdditionalValidatorOnValueCollection.First();
			var output = validator.Invoke(invalidValue);
			AssertEquals($"{testCase}: ValidationMessage", expectedValidationMessage, output.ValidationMessage);
			AssertEquals($"{testCase}: NotificationType", expectedNotificationType, output.NotificationType);
			AssertEquals($"{testCase}: valid value", string.Empty, validator.Invoke(validValue).ValidationMessage);
		}

		protected void AssertLinkedRuleRange(ZString testCase, LinkedCusAuthorisationRuleRange ruleRequirement, int expectedMinRequired, int expectedMaxAllowed)
		{
			AssertEquals($"{testCase}: min required", expectedMinRequired, ruleRequirement.MinRequired);
			AssertEquals($"{testCase}: max allowed", expectedMaxAllowed, ruleRequirement.MaxAllowed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_RN_NKCountryCode = AuthorisationHeaderCountryCode;
		}
		protected CusAuthorisationHeader authorisationHeader;

		protected abstract T AuthorisationHeaderProvider { get; }

		protected abstract ZString AuthorisationHeaderCountryCode { get; }

		protected virtual bool ExpectedShowCustomsCode => false;

		protected virtual CodeDescriptionPairList ExpectedRuleCodeListForModule => new CusAuthorisationRuleTypeList();

		protected virtual CodeDescriptionPairList ExpectedAuthorisationTypeList => new CusAuthorizationHeaderTypeList();

		protected virtual Type ExpectedHeaderLookupsType => typeof(CusAuthorisationHeaderLookups);
		protected virtual Type ExpectedHeaderValidationType => typeof(CusAuthorisationHeaderValidation);
		protected virtual Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);
		protected virtual Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);
		protected virtual Type ExpectedLinkedRuleLookupsType => typeof(LinkedCusAuthorisationRuleLookups);
		protected virtual Type ExpectedLinkedRuleValidationType => typeof(LinkedCusAuthorisationRuleValidation);
		protected virtual Type ExpectedLinkedRuleCollectionType => typeof(LinkedCusAuthorisationRuleCollection);

		public void TestCusAuthorisationHeaderLookups()
		{
			AssertEquals("CusAuthorisationHeaderLookups", ExpectedHeaderLookupsType, AuthorisationHeaderProvider.GetNewLookups(authorisationHeader).GetType());
		}

		public void TestCusAuthorisationHeaderValidation()
		{
			AssertEquals("CusAuthorisationHeaderValidation", ExpectedHeaderValidationType, AuthorisationHeaderProvider.GetNewValidation(authorisationHeader).GetType());
		}

		public void TestCusAuthorisationRuleLookups()
		{
			AssertEquals("CusAuthorisationRuleLookups", ExpectedRuleLookupsType, AuthorisationHeaderProvider.GetNewLookups(authorisationHeader.CusAuthorisationRules.AddNew()).GetType());
		}

		public void TestCusAuthorisationRuleValidation()
		{
			AssertEquals("CusAuthorisationRuleValidation", ExpectedRuleValidationType, AuthorisationHeaderProvider.GetNewValidation(authorisationHeader.CusAuthorisationRules.AddNew()).GetType());
		}

		public void TestLinkedCusAuthorisationRuleLookups()
		{
			AssertEquals("LinkedCusAuthorisationRuleLookups", ExpectedLinkedRuleLookupsType, AuthorisationHeaderProvider.GetNewLookups(authorisationHeader.CusAuthorisationRules.AddNew().LinkedCusAuthorisationRules.AddNew()).GetType());
		}

		public void TestLinkedCusAuthorisationRuleValidation()
		{
			AssertEquals("LinkedCusAuthorisationRuleValidation", ExpectedLinkedRuleValidationType, AuthorisationHeaderProvider.GetNewValidation(authorisationHeader.CusAuthorisationRules.AddNew().LinkedCusAuthorisationRules.AddNew()).GetType());
		}

		public void TestLinkedCusAuthorisationRuleCollection()
		{
			AssertEquals("LinkedCusAuthorisationRuleCollection", ExpectedLinkedRuleCollectionType, AuthorisationHeaderProvider.GetLinkedCusAuthorisationRules(authorisationHeader.CusAuthorisationRules.AddNew()).GetType());
		}

		protected virtual void TestLinkedRulesRuleCodeReadOnly()
		{
			AssertEquals("LinkedRulesRuleCodeReadOnly", true, authorisationHeader.Provider.IsLinkedRuleCodeReadOnly(null));
		}
	}

	class CusAuthorisationHeaderProviderForTest : CusAuthorisationHeaderProvider
	{
		public CusAuthorisationHeaderProviderForTest(ZString countryCode) : base(countryCode)
		{
		}

		protected override Dictionary<ZString, List<CusAuthorisationRuleRequirement>> GetValidAuthorisationRuleRequirementsCore(CusAuthorisationHeader cusAuthorisationHeader)
		{
			var rules = base.GetValidAuthorisationRuleRequirementsCore(cusAuthorisationHeader);
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, new List<CusAuthorisationRuleRequirement>()
			{
				new CusAuthorisationRuleRequirement("ZZZ", 1, 1, () => true, null),
				new CusAuthorisationRuleRequirement("YYY", 1, 1, () => true, null),
				new CusAuthorisationRuleRequirement("AAA", 1, 1, () => true, x => "Error", CargoWise.EntityFramework.NotificationType.Error),
				new CusAuthorisationRuleRequirement("BBB", 1, 1, () => true, x => "MessageError", CargoWise.EntityFramework.NotificationType.MessageError),
				new CusAuthorisationRuleRequirement("CCC", 1, 1, () => true, x => "Warning", CargoWise.EntityFramework.NotificationType.Warning),
				new CusAuthorisationRuleRequirement("DDD", 1, 1, () => true, x => ZString.Empty, CargoWise.EntityFramework.NotificationType.Error),
				new CusAuthorisationRuleRequirement("MLT", 1, 1, () => true, null)
				.AddAdditionalValidatorOnValue(v => ("MessageError",CargoWise.EntityFramework.NotificationType.MessageError))
				.AddAdditionalValidatorOnValue(v => ("Error",CargoWise.EntityFramework.NotificationType.Error)),
			});
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument, new List<CusAuthorisationRuleRequirement>()
			{
				new CusAuthorisationRuleRequirement("XXX", 1, 1)
			});
			AddOrUpdateValidAuthorisationRuleRepititions(rules, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, new List<CusAuthorisationRuleRequirement>()
			{
				new CusAuthorisationRuleRequirement("ZZZ", 2, 2),
				new CusAuthorisationRuleRequirement("XXX", 1, 0)
			});
			return rules;
		}

		protected override Dictionary<ZString, List<LinkedCusAuthorisationRuleRange>> GetValidLinkedAuthorizationRuleRepetitionsCore()
		{
			var rules = base.GetValidLinkedAuthorizationRuleRepetitionsCore();
			AddOrUpdateValidLinkedAuthorizationRuleRepetitions(rules, CusAuthorisationRuleTypeList.Codes.Location, new List<LinkedCusAuthorisationRuleRange>()
			{
				new LinkedCusAuthorisationRuleRange("ZZZ", 1, 1),
				new LinkedCusAuthorisationRuleRange("YYY", 1, 1)
			});
			AddOrUpdateValidLinkedAuthorizationRuleRepetitions(rules, "USE", new List<LinkedCusAuthorisationRuleRange>()
			{
				new LinkedCusAuthorisationRuleRange("XXX", 1, 1)
			});
			AddOrUpdateValidLinkedAuthorizationRuleRepetitions(rules, CusAuthorisationRuleTypeList.Codes.Location, new List<LinkedCusAuthorisationRuleRange>()
			{
				new LinkedCusAuthorisationRuleRange("ZZZ", 2, 2),
				new LinkedCusAuthorisationRuleRange("XXX", 1, 0)
			});
			return rules;
		}

		protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
		{
			return Enum.GetValues(typeof(FieldType)).Cast<FieldType>().ToDictionary(x => new ZString(((int)x).ToString()));
		}

		public ZBool IsAuthorisationNumberValidExposed { get; set; }

		protected override ZBool IsAuthorisationNumberValidCore(CusAuthorisationHeader authorisationHeader) => IsAuthorisationNumberValidExposed;

		public ZString AuthorisationNumberInvalidFormatMessageExposed { get; set; }

		protected override ZString GetAuthorisationNumberInvalidFormatMessageCore(CusAuthorisationHeader authorisationHeader) => AuthorisationNumberInvalidFormatMessageExposed;
	}

	class CusAuthorisationHeaderProviderForTestRegistration : ObjectHandle
	{
		public override object GetObject(params object[] arguments) => new CusAuthorisationHeaderProviderForTest(arguments[0].ToString());
	}
}
