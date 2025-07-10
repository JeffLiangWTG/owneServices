using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(BarcodeParsingRuleInfo))]
	class BarcodeParsingRuleInfoTest : DataObjectInfoTestCase<BarcodeParsingRuleInfo>
	{
		public void TestConstructor()
		{
			AssertEquals("", new BarcodeParsingRuleInfo().Terminator);
			AssertNotNull(new BarcodeParsingRuleInfo().Components);

			var helper = new BarcodeParsingTestHelper(Factory);
			var rule1 = helper.CreateRule();
			rule1.IsPartialRule = true;
			rule1.BRU_RuleNumber = 2;
			rule1.BRU_Name = "ABC";
			rule1.BRU_Terminator = "|";

			var component = helper.CreateRuleComponent(rule1, fixedLength: 2);
			var ruleInfo1 = new BarcodeParsingRuleInfo(rule1);
			AssertEquals("ruleInfo.IsPartialRule", true, ruleInfo1.IsPartialRule);
			AssertEquals("ruleInfo.RuleNumber", (short)2, ruleInfo1.RuleNumber);
			AssertEquals("ruleInfo.RuleName", "ABC", ruleInfo1.RuleName);
			AssertEquals("ruleInfo.Terminator", "|", ruleInfo1.Terminator);
			AssertEquals("ruleInfo.TerminatorCharacterCode", 0, ruleInfo1.TerminatorCharacterCode);
			AssertEquals("ruleInfo.Components.Count", 1, ruleInfo1.Components.Count);

			var componentInfo = ruleInfo1.Components[0];
			AssertEquals((short)2, componentInfo.MinLength);
			AssertEquals((short)2, componentInfo.MaxLength);

			var rule2 = helper.CreateRule();
			rule2.TerminatorType = TerminatorTypes.Codes.GS1;
			var ruleInfo2 = new BarcodeParsingRuleInfo(rule2);
			// We cannot send character '29' (GS1 Separator) over webservice as it crashes when used in XML.
			AssertEquals("ruleInfo.Terminator", "", ruleInfo2.Terminator);
			AssertEquals("ruleInfo.TerminatorCharacterCode", 29, ruleInfo2.TerminatorCharacterCode);
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new BarcodeParsingRuleInfo();
		}
	}
}
