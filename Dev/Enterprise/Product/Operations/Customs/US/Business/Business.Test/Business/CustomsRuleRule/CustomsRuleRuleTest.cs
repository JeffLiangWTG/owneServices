using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
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
			AssertEquals("Default From", nameof(FieldType.Text), rule.CPR_ValueFrom_FieldType);
			AssertEquals("Default To", nameof(FieldType.Text), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertEquals("SingleTransactionBondAmount From", nameof(FieldType.Decimal), rule.CPR_ValueFrom_FieldType);
			AssertEquals("SingleTransactionBondAmount To", nameof(FieldType.Decimal), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals("TariffNumber From", nameof(FieldType.Text), rule.CPR_ValueFrom_FieldType);
			AssertEquals("TariffNumber To", nameof(FieldType.Text), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			AssertEquals("ADCEligible From", nameof(FieldType.TextDropEdit), rule.CPR_ValueFrom_FieldType);
			AssertEquals("ADCEligible To", nameof(FieldType.TextDropEdit), rule.CPR_ValueTo_FieldType);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals("ADCEligible From", nameof(FieldType.Decimal), rule.CPR_ValueFrom_FieldType);
			AssertEquals("ADCEligible To", nameof(FieldType.Decimal), rule.CPR_ValueTo_FieldType);
		}

		public void TestReadOnlyAttributes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			AssertEquals(false, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertEquals(true, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals(false, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			AssertEquals(false, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(true, rule.CPR_ValueToInfo.ReadOnly);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals(true, rule.CPR_ValueFromInfo.ReadOnly);
			AssertEquals(false, rule.CPR_ValueToInfo.ReadOnly);
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

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertEquals(CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount, rule.CPR_RuleCode);
			AssertEquals("0", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals(CustomsRuleRuleCodeList.Codes.TariffNumber, rule.CPR_RuleCode);
			AssertEquals("", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);

			rule.CPR_ValueFrom = "A";
			rule.CPR_ValueTo = "1";
			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			AssertEquals(CustomsRuleRuleCodeList.Codes.ADCEligible, rule.CPR_RuleCode);
			AssertEquals("", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);

			rule.CPR_ValueFrom = "A";
			rule.CPR_ValueTo = "1";
			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals(CustomsRuleRuleCodeList.Codes.TotalDuty, rule.CPR_RuleCode);
			AssertEquals("0", rule.CPR_ValueFrom);
			AssertEquals("", rule.CPR_ValueTo);
		}

		public void TestValidationType()
		{
			AssertEquals(typeof(CustomsRuleRuleValidation), Factory.New<CustomsRuleRule>().Validation.GetType());
		}

		public void TestRuleCodeDescription()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			AssertEquals(ZString.Empty, rule.RuleCodeDescription);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertEquals(CustomsRuleRuleCodeList.Descriptions.SingleTransactionBondAmount, rule.RuleCodeDescription.ToString());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals(CustomsRuleRuleCodeList.Descriptions.TariffNumber, rule.RuleCodeDescription.ToString());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			AssertEquals(CustomsRuleRuleCodeList.Descriptions.ADCEligible, rule.RuleCodeDescription.ToString());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals(CustomsRuleRuleCodeList.Descriptions.TotalDuty, rule.RuleCodeDescription.ToString());
		}

		#region Override

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as CustomsRuleRule;
			result.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			result.CPR_ValueFrom = "0";
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject() as CustomsRuleRule;
			var customsRule = Factory.New<CustomsRule>();
			customsRule.Rules.Add(result);

			result.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			result.CPR_ValueFrom = "0";
			return result;
		}

		IDisposable countryDisposition;
		protected override void SetUp()
		{
			base.SetUp();
			countryDisposition = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		protected override void TearDown()
		{
			base.TearDown();
			countryDisposition.Dispose();
		}
		#endregion
	}
}
