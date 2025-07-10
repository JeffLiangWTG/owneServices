using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CustomsRuleRule))]
	public class CustomsRuleRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var rule = Factory.New<CustomsRuleRule>();
			AssertEquals("CustomsRuleRuleLookups", typeof(CustomsRuleRuleLookups), rule.Lookups.GetType());
		}

		public void TestFieldTypes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			AssertEquals("Text", nameof(FieldType.Text), rule.CPR_ValueFrom_FieldType);
			AssertEquals("Text", nameof(FieldType.Text), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			AssertEquals("TextDropEdit", nameof(FieldType.TextDropEdit), rule.CPR_ValueFrom_FieldType);
			AssertEquals("TextDropEdit", nameof(FieldType.TextDropEdit), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			AssertEquals("Decimal", nameof(FieldType.Decimal), rule.CPR_ValueFrom_FieldType);
			AssertEquals("Decimal", nameof(FieldType.Decimal), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			AssertEquals("Decimal", nameof(FieldType.Decimal), rule.CPR_ValueFrom_FieldType);
			AssertEquals("Decimal", nameof(FieldType.Decimal), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals("Decimal", nameof(FieldType.Decimal), rule.CPR_ValueFrom_FieldType);
			AssertEquals("Decimal", nameof(FieldType.Decimal), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals("Text", nameof(FieldType.Text), rule.CPR_ValueFrom_FieldType);
			AssertEquals("Text", nameof(FieldType.Text), rule.CPR_ValueTo_FieldType);
		}

		public void TestReadOnlyAttributes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			AssertEquals(false, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			AssertEquals(true, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			AssertEquals(true, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals(true, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals(false, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			AssertEquals(false, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(true, rule.CPR_ValueToInfo.ReadOnly);
		}

		public void TestSetCPR_ValueFromDependsOnCPR_RuleCode()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			AssertEquals(ZString.Empty, rule.CPR_RuleCode);
			AssertEquals(ZString.Empty, rule.CPR_ValueFrom);

			rule.CPR_ValueFrom = "A";
			rule.CPR_ValueTo = "1";
			AssertEquals(ZString.Empty, rule.CPR_RuleCode);
			AssertEquals("A", rule.CPR_ValueFrom);
			AssertEquals("1", rule.CPR_ValueTo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			AssertEquals(CustomsRuleRuleCodeList.Codes.PaymentType, rule.CPR_RuleCode);
			AssertEquals("", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			AssertEquals(CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement, rule.CPR_RuleCode);
			AssertEquals("0", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			rule.CPR_ValueFrom = "A";
			rule.CPR_ValueTo = "";
			AssertEquals(CustomsRuleRuleCodeList.Codes.PaymentType, rule.CPR_RuleCode);
			AssertEquals("A", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			AssertEquals(CustomsRuleRuleCodeList.Codes.TotalCustomsValue, rule.CPR_RuleCode);
			AssertEquals("0", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);
		}

		public void TestValidationType()
		{
			AssertEquals(typeof(CustomsRuleRuleValidation), Factory.New<CustomsRuleRule>().Validation.GetType());
		}

		public void TestRuleCodeDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Afghanistan))
			{
				var customsRule = Factory.New<CustomsRule>();
				var rule = customsRule.Rules.AddNew();
				AssertEquals(ZString.Empty, rule.RuleCodeDescription);

				rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
				AssertEquals(CustomsRuleRuleCodeList.Descriptions.TotalCustomsDisbursement, rule.RuleCodeDescription);

				rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
				AssertEquals(CustomsRuleRuleCodeList.Descriptions.TotalCustomsValue, rule.RuleCodeDescription);

				rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
				AssertEquals(CustomsRuleRuleCodeList.Descriptions.PaymentType, rule.RuleCodeDescription);
			}
		}

		#region Override

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as CustomsRuleRule;
			result.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			result.CPR_ValueFrom = "ValueFrom";
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject() as CustomsRuleRule;
			var customsRule = Factory.New<CustomsRule>();
			customsRule.Rules.Add(result);

			result.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			result.CPR_ValueFrom = "ValueFrom";
			return result;
		}

		#endregion
	}
}
