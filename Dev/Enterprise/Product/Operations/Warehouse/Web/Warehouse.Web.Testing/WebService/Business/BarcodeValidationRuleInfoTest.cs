using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(BarcodeValidationRuleInfo))]
	class BarcodeValidationRuleInfoTest : DataObjectInfoTestCase<BarcodeValidationRuleInfo>
	{
		public void TestConstructor()
		{
			var helper = new BarcodeParsingTestHelper(Factory);
			var rule = helper.CreateValidationRule();
			rule.BVR_Prefix = "Test";
			rule.BVR_Format = nameof(FormatType.ANY);
			rule.BVR_MinLength = 1;
			rule.BVR_MaxLength = 2;
			rule.BVR_TargetField = nameof(WarehouseTargetField.PRC);

			var ruleInfo = new BarcodeValidationRuleInfo(rule);
			AssertEquals("ruleInfo.Prefix", "Test", ruleInfo.Prefix);
			AssertEquals("ruleInfo.Format", (int)FormatType.ANY, ruleInfo.FormatEnumValue);
			AssertEquals("ruleInfo.MinLength", (short)1, ruleInfo.MinLength);
			AssertEquals("ruleInfo.MaxLength", (short)2, ruleInfo.MaxLength);
			AssertEquals("ruleInfo.TargetField", "PRC", ruleInfo.TargetField);
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new BarcodeValidationRuleInfo();
		}
	}
}
