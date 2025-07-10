using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	sealed class CusAuthorisationHeaderProviderTest : CusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;
		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Norway;

		protected override Type ExpectedHeaderValidationType => typeof(CusAuthorisationHeaderValidation);
		protected override CodeDescriptionPairList ExpectedAuthorisationTypeList => new CusAuthorizationHeaderTypeList().SortedList();
		protected override CodeDescriptionPairList ExpectedRuleCodeListForModule => new CusAuthorisationRuleTypeList().SortedList();
		protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);
		protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);
		protected override Type ExpectedLinkedRuleLookupsType => typeof(LinkedCusAuthorisationRuleLookups);
		protected override Type ExpectedLinkedRuleCollectionType => typeof(LinkedCusAuthorisationRuleCollection);
		protected override Type ExpectedLinkedRuleValidationType => typeof(LinkedCusAuthorisationRuleValidation);

		public void TestRuleCodesWithLinkedRules()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			var ruleCodesWithLinkedRules = AuthorisationHeaderProvider.RuleCodesWithLinkedRules(authorisationRule);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { CusAuthorisationRuleTypeList.Codes.Location, CusAuthorisationRuleTypeList.Codes.MainCustomsOffice }, ruleCodesWithLinkedRules);
				AssertSame("Cached", ruleCodesWithLinkedRules, AuthorisationHeaderProvider.RuleCodesWithLinkedRules(authorisationRule));
			});
		}

		public void TestGetRuleDescriptionFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			AssertEquals("RuleCode 'MCO'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleDescriptionFieldType(authorisationRule));
		}

		public void TestGetRuleValueFromFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			AssertEquals("RuleCode 'MCO'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
		}

		public void TestGetLinkedRuleValueFromFieldType()
		{
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			var linkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
			AssertEquals("RuleCode 'VCO'", nameof(FieldType.Text), AuthorisationHeaderProvider.GetLinkedRuleValueFromFieldType(linkedRule));
		}

		public void TestGetValidRepetitionsForAuthorisationRules()
		{
			var validRuleRepetitions = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationHeader.CPH_Type);
			CombineAssertions(() =>
			{
				AssertSame("Cached", validRuleRepetitions, AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationHeader.CPH_Type));
				var ruleRequirementIDE = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration);
				Assert("Must contain requirement rules for IDE", ruleRequirementIDE.Any());
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice, ruleRequirementIDE.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice), 1, 1, false);
				var ruleRequirementEDE = AuthorisationHeaderProvider.GetValidAuthorisationRuleRequirements(authorisationHeader, CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration);
				Assert("Must contain requirement rules for EDE", ruleRequirementEDE.Any());
				AssertRuleRequirement(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice, ruleRequirementEDE.Single(x => x.RuleType == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice), 1, 1, false);
			});
		}

		public void TestGetValidRepetitionsForLinkedAuthorizationRules()
		{
			var validRuleRepetitions = AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory);
			CombineAssertions(() =>
			{
				Assert("Must contain rule for MCO", validRuleRepetitions.ContainsKey(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice));
				AssertSame("Cached", validRuleRepetitions, AuthorisationHeaderProvider.GetValidLinkedAuthorizationRuleRepetitions(Factory));
				var ruleRangesMCO = validRuleRepetitions[CusAuthorisationRuleTypeList.Codes.MainCustomsOffice];
				AssertLinkedRuleRange(LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, ruleRangesMCO.Single(x => x.RuleType == LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices), 1, 0);
			});
		}
	}

	public static class CodeDescriptionPairListExt
	{
		public static CodeDescriptionPairList SortedList(this CodeDescriptionPairList codeList)
		{
			codeList.Sort();
			return codeList;
		}
	}
}
