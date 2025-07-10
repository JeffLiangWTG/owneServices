using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderProvider))]
sealed class CusAuthorisationHeaderProviderTest : EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
{
	protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

	protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Poland;

	protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);

	protected override bool ExpectedShowCustomsCode => true;

	public void TestGetRuleValueFromFieldType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("This value should inherit from EU", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("if the RuleCode is LOC in Poland, the rule value should be entered by users", nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorisationRule));
		});
	}

	public void TestCPR_ValueFromMaxLength()
	{
		const int MaxLengthForLOC = 17;
		CombineAssertions(() =>
		{
			AssertEquals("The max length should inherit from EU if there is no special instructions", CusAuthorisationRule.Schema.CPR_ValueFromMaxLength, authorisationRule.CPR_ValueFromInfo.MaxLength);

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			AssertEquals("The max length should be changed if the RuleCode is LOC", MaxLengthForLOC, authorisationRule.CPR_ValueFromInfo.MaxLength);
		});
	}

	protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("OTH", "Other PL");
			result.AddPair("SAS", "Self-Assessment");
			result.AddPair("DPO", "Deferred");
			result.Sort();
			return result;
		}
	}

	protected override void SetUp()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, null, grouping);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, "AUTH", "OTH", "Other PL", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();
		base.SetUp();
	}
}
