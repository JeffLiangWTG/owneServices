using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusPermitRule))]
	sealed class BaseCusPermitRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCPR_ValueTo_ReadOnly()
		{
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.Tariff;
			Assert("ValueTo is editable for Tariff", !permitRule.CPR_ValueToInfo.ReadOnly);
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.CountryOfOrigin;
			Assert("ValueTo is readonly for CountryOfOrigin", permitRule.CPR_ValueToInfo.ReadOnly);
		}

		public void TestCPR_ValueFrom_FieldType()
		{
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.Tariff;
			AssertEquals("Text", permitRule.CPR_ValueFrom_FieldType);
			AssertEquals(false, permitRule.CPR_ValueFromIsCodeField);
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.CountryOfOrigin;
			AssertEquals("TextCodeFindBox", permitRule.CPR_ValueFrom_FieldType);
			AssertEquals(true, permitRule.CPR_ValueFromIsCodeField);
		}

		public void TestValueComparer()
		{
			var comparer = permitRule.ValueComparer;
			AssertEquals("Less Than", -1, comparer.Compare(new ZString("012"), new ZString("013")));
			AssertEquals("Less Than", -1, comparer.Compare(new ZString("0123"), new ZString("013")));
			AssertEquals("Greater Than", 1, comparer.Compare(new ZString("014"), new ZString("013")));
			AssertEquals("Greater Than", 1, comparer.Compare(new ZString("0141"), new ZString("013")));
			AssertEquals("Equals", 0, comparer.Compare(new ZString("1234"), new ZString("1234")));
		}

		public void TestValueInRange()
		{
			permitRule.CPR_ValueFrom = "50";
			permitRule.CPR_ValueTo = "99";
			var exception = permitRule.CusPermitRuleExceptions.AddNew();
			exception.CPE_ValueFrom = "60";
			exception.CPE_ValueTo = "70";

			Assert("Doesn't match when LT range", !permitRule.IsValueInRange("40"));
			Assert("Doesn't match when GT rage", !permitRule.IsValueInRange("110"));
			Assert("Match when in range LT exclusion", permitRule.IsValueInRange("55"));
			Assert("Match when in range GT exclusion", permitRule.IsValueInRange("75"));
			Assert("Doesn't match when in exclusion", !permitRule.IsValueInRange("65"));
		}

		public void TestPermitHeader()
		{
			AssertType<BaseCusPermitHeader>(permitRule.PermitHeader);
			AssertEquals(permitRule.CPR_CPH_PermitHeader, permitRule.PermitHeader.PK);
		}

		#region Bindings for Visibility of ValueFrom Edit

		public void TestValueFromFieldIsTextCodeFindBox()
		{
			var permit = Factory.New<BaseCusPermitHeader_ForTest>();
			var rule = permit.CusPermitRules.AddNew();

			rule.CPR_RuleCode = ((int)FieldType.Boolean).ToString();
			AssertEquals(nameof(FieldType.Boolean), rule.CPR_ValueFrom_FieldType);
			AssertEquals(false, rule.ValueFromFieldIsTextCodeFindBox);
			AssertEquals("Default to N", YesNoList.Codes.No, rule.CPR_ValueFrom);
			AssertEquals("Default to empty", ZString.Empty, rule.CPR_ValueTo);

			rule.CPR_RuleCode = ((int)FieldType.TextCodeFindBox).ToString();
			AssertEquals(nameof(FieldType.TextCodeFindBox), rule.CPR_ValueFrom_FieldType);
			AssertEquals(true, rule.ValueFromFieldIsTextCodeFindBox);
			AssertEquals("Default to empty", ZString.Empty, rule.CPR_ValueFrom);
			AssertEquals("Default to empty", ZString.Empty, rule.CPR_ValueTo);
		}

		public void TestValueFromFieldIsOthers()
		{
			var permit = Factory.New<BaseCusPermitHeader_ForTest>();
			var rule = permit.CusPermitRules.AddNew();

			rule.CPR_RuleCode = ((int)FieldType.Boolean).ToString();
			AssertEquals(nameof(FieldType.Boolean), rule.CPR_ValueFrom_FieldType);
			AssertEquals(true, rule.ValueFromFieldIsOthers);

			rule.CPR_RuleCode = ((int)FieldType.TextCodeFindBox).ToString();
			AssertEquals(nameof(FieldType.TextCodeFindBox), rule.CPR_ValueFrom_FieldType);
			AssertEquals(false, rule.ValueFromFieldIsOthers);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			return permitHeader.CusPermitRules.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			permitRule = permitHeader.CusPermitRules.AddNew();
		}

		BaseCusPermitRule permitRule;

		#endregion
	}
}
