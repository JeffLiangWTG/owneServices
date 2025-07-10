using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRuleCollection))]
	sealed class CusGuaranteeRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusGuaranteeRuleCollection>
	{
		protected override CusGuaranteeRuleCollection GetCollectionToTest()
		{
			return new CusGuaranteeRuleCollection<CusGuaranteeRule>(Factory.New<BaseCusGuaranteeHeader>());
		}

		public void TestTwoConstructor()
		{
			var header = Factory.New<BaseCusGuaranteeHeader>();
			var plainRule = Factory.New<CusGuaranteeRule>();
			var accessRule = Factory.New<CusGuaranteeRule>();
			var defaultPin = Factory.New<CusGuaranteeRule>();
			plainRule.CPR_RuleCode = "XXX";
			accessRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			defaultPin.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
			plainRule.CPR_CPH_PermitHeader = header.PK;
			accessRule.CPR_CPH_PermitHeader = header.PK;
			defaultPin.CPR_CPH_PermitHeader = header.PK;

			var plainCollection = new CusGuaranteeRuleCollection<CusGuaranteeRule>(header);
			var accessCollection = new CusGuaranteeRuleCollection<CusGuaranteeRule>(header, PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber);
			var mainCollection = new CusGuaranteeRuleCollection<CusGuaranteeRule>(header, PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin);

			AssertContainsExactElementsInAnyOrder(new[] { plainRule }, plainCollection);
			AssertContainsExactElementsInAnyOrder(new[] { accessRule }, accessCollection);
			AssertContainsExactElementsInAnyOrder(new[] { defaultPin }, mainCollection);
		}
	}
}
