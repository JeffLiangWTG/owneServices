using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(BarcodeParsingRuleComponentInfo))]
	class BarcodeParsingRuleComponentInfoTest : DataObjectInfoTestCase<BarcodeParsingRuleComponentInfo>
	{
		public void TestConstructor()
		{
			AssertEquals("", new BarcodeParsingRuleComponentInfo().ApplicationIdentifier);
			AssertEquals("", new BarcodeParsingRuleComponentInfo().TargetField);

			var helper = new BarcodeParsingTestHelper(Factory);
			var rule = helper.CreateRule();
			var component = helper.CreateRuleComponent(rule);
			component.BRC_ApplicationID = "|";
			component.BRC_Delimiter = 44;
			component.BRC_Format = nameof(FormatType.ANY);
			component.BRC_MaxLength = 2;
			component.BRC_MinLength = 1;
			component.BRC_Sequence = 3;
			component.BRC_TargetField = nameof(WarehouseTargetField.PRC);
			component.IsDelimiterMultiComponent = true;

			var ruleComponentInfo = new BarcodeParsingRuleComponentInfo(component);
			AssertEquals("ruleComponentInfo.ApplicationIdentifier", "|", ruleComponentInfo.ApplicationIdentifier);
			AssertEquals("ruleComponentInfo.Delimiter", (byte)44, ruleComponentInfo.Delimiter);
			AssertEquals("ruleComponentInfo.Format", (int)FormatType.ANY, ruleComponentInfo.FormatEnumValue);
			AssertEquals("ruleComponentInfo.MaxLength", (short)2, ruleComponentInfo.MaxLength);
			AssertEquals("ruleComponentInfo.MinLength", (short)1, ruleComponentInfo.MinLength);
			AssertEquals("ruleComponentInfo.Sequence", (short)3, ruleComponentInfo.Sequence);
			AssertEquals("ruleComponentInfo.TargetField", "PRC", ruleComponentInfo.TargetField);
			AssertEquals("ruleComponentInfo.IsDelimiterMultiComponent", true, ruleComponentInfo.IsDelimiterMultiComponent);
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new BarcodeParsingRuleComponentInfo();
		}
	}
}
