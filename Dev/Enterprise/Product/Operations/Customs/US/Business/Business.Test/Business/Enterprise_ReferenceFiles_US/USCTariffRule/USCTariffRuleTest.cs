using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffRule))]
	public class USCTariffRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestA99AndI99MutuallyExclusive()
		{
			var query = new ZQuery(USCTariffRuleSchema.U1_RuleCode, "A99");
			var tariffRuleA99 = Factory.Load<USCTariffRule>(query);

			foreach (var rule in tariffRuleA99)
			{
				var tariffNumber = rule.U1_Tariff;
				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffNumber;
				tariff.UE_DateFrom = rule.U1_DateFrom;
				tariff.UE_DateTo = rule.U1_DateTo;

				if (tariff.TariffRules.Applies("I99", tariffNumber, ZDateTime.Today))
				{
					Assert("Tariff rule A99 and I99 should be mutually exclusive, you need to add tariff " + rule.U1_Tariff + " in USCTariffRuleException for I99.", false);
				}
			}

			query = new ZQuery(USCTariffRuleSchema.U1_RuleCode, "I99");
			var tariffRuleI99 = Factory.Load<USCTariffRule>(query);
			foreach (var rule in tariffRuleI99)
			{
				var tariffNumber = rule.U1_Tariff;
				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffNumber;
				tariff.UE_DateFrom = rule.U1_DateFrom;
				tariff.UE_DateTo = rule.U1_DateTo;

				if (tariff.TariffRules.Applies("A99", tariffNumber, ZDateTime.Today))
				{
					Assert("Tariff rule A99 and I99 should be mutually exclusive, you need to add tariff " + rule.U1_Tariff + " in USCTariffRuleException for I99.", false);
				}
			}

			Assert(true);
		}

		public void TestShouldDeleteSecondaryTariffs()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = "AAA";
			Assert(!tariffRule.ShouldDeleteSecondaryTariffs);

			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			Assert(!tariffRule.ShouldDeleteSecondaryTariffs);

			tariffRule.SecondaryTariffs.AddNew();
			Assert(!tariffRule.ShouldDeleteSecondaryTariffs);

			tariffRule.U1_RuleCode = "AAA";
			Assert(tariffRule.ShouldDeleteSecondaryTariffs);
		}

		public void TestApplies()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = "AAA";
			tariffRule.U1_Tariff = "9902";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception = tariffRule.RuleExceptions.AddNew();
			exception.U2_Tariff = "990215";
			exception.U2_DateFrom = ZDateTime.BrettsBirthday;
			exception.U2_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCTariffRuleException exception2 = tariffRule.RuleExceptions.AddNew();
			exception2.U2_Tariff = "990225";
			exception2.U2_DateFrom = ZDateTime.BrettsBirthday;

			AssertEquals(true, tariffRule.Applies("AAA", "9902000000", ZDateTime.BrettsBirthday));
			AssertEquals(false, tariffRule.Applies("AAA", "9902150000", ZDateTime.BrettsBirthday));
			AssertEquals(true, tariffRule.Applies("AAA", "9902150000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));
			AssertEquals(false, tariffRule.Applies("AAA", "9902250000", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));
		}

		public void TestAppliesForTariffRange()
		{
			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9902.51.1600"));
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "9902511610";
				tariff.UE_ShortDescription = "CASHMERE HAIR FM SBHD 5102";
			}
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var rule = Factory.LoadTop1<USCRule>(new ZQuery(USCRuleSchema.U0_Code, TariffRuleList.Codes.WoolLicenseEligible));
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.WoolLicenseEligible;
			}

			var query = new ZQuery(USCTariffRuleSchema.U1_Tariff, "99025115");
			query.AddToFilter(USCTariffRuleSchema.U1_RuleCode, TariffRuleList.Codes.WoolLicenseEligible);

			var tariffRule = Factory.LoadTop1<USCTariffRule>(query);
			if (tariffRule == null)
			{
				tariffRule = Factory.New<USCTariffRule>();
				tariffRule.U1_RuleCode = TariffRuleList.Codes.WoolLicenseEligible;
				tariffRule.U1_Tariff = "99025115";
				tariffRule.U1_TariffTo = "99025116";
				tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			}
			tariffRule.U1_DateTo = ZDateTime.Empty;

			AssertEquals("Tariff 9902.51.1610 within tariff range for WLE tariff rule",
				tariffRule, tariff.GetTariffRuleIfApplies(TariffRuleList.Codes.WoolLicenseEligible, ZDateTime.Today));
		}

		public void TestRuleCodeDescription()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			AssertEquals("", rule.RuleCodeDescription);

			rule.U1_RuleCode = TariffRuleList.Codes.AssembledAbroadOfUSProducts;
			AssertEquals(TariffRuleList.Descriptions.AssembledAbroadOfUSProducts, rule.RuleCodeDescription);
		}

		public void TestFormattedTariff()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();
			rule.U1_Tariff = "00000000";
			AssertEquals("0000.00.00", rule.FormattedTariff);

			rule.FormattedTariff = "";
			AssertEquals("", rule.U1_Tariff);

			rule.FormattedTariff = "0000.00.11";
			AssertEquals("00000011", rule.U1_Tariff);
		}

		public void TestGetTariffRuleIfApplies()
		{
			USCTariff tariffA = Factory.New<USCTariff>();
			tariffA.UE_Tariff = "99021010";
			tariffA.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffA.UE_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCTariffRule tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = "AAA";
			tariffRule1.U1_Tariff = "9902";
			tariffRule1.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule1.U1_DateTo = ZDateTime.BrettsBirthday.AddYears(10);

			USCTariff tariffB = Factory.New<USCTariff>();
			tariffB.UE_Tariff = "0000112200";
			tariffB.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffB.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariffRule tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = "BBB";
			tariffRule2.U1_Tariff = "00001122";
			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday;

			AssertEquals(tariffRule1, tariffA.GetTariffRuleIfApplies("AAA", ZDateTime.BrettsBirthday));
			AssertEquals(null, tariffA.GetTariffRuleIfApplies("BBB", ZDateTime.BrettsBirthday));
			AssertEquals(null, tariffB.GetTariffRuleIfApplies("AAA", ZDateTime.BrettsBirthday));
			AssertEquals(null, tariffA.GetTariffRuleIfApplies("AAA", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));
			AssertEquals(tariffRule2, tariffB.GetTariffRuleIfApplies("BBB", ZDateTime.BrettsBirthday.AddYears(10).AddDays(1)));
		}

		public void TestEligibleForAssociatedSecondaryTariffs()
		{
			USCTariffRule rule = Factory.New<USCTariffRule>();

			TariffRuleList ruleCodeList = new TariffRuleList();
			foreach (CodeDescriptionPair ruleCode in ruleCodeList)
			{
				rule.U1_RuleCode = ruleCode.Code;

				if (ruleCode.Code == TariffRuleList.Codes.EligibleForSecondaryTariffNumbers)
				{
					Assert(rule.EligibleForAssociatedSecondaryTariffs);
				}
				else
				{
					Assert(!rule.EligibleForAssociatedSecondaryTariffs);
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCTariffRule rule = factory.New<USCTariffRule>();
			rule.U1_DateFrom = ZDateTime.BrettsBirthday;
			rule.RuleExceptions.AddNew();
			return rule;
		}
	}
}
