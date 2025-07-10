using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCommissionRuleRate))]
	sealed class AccCommissionRuleRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesAccCommissionRuleRate()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			var comList = new List<string>
			{
				nameof(rate.ACT_CommissionAmount)
			};

			var percentList = new List<string>
			{
				nameof(rate.ACT_CommissionPercentage)
			};

			var tester = new DecimalPlacesAttributeTester(rate);
			tester.CheckNonLocalCurrency(comList, nameof(rate.CommissionAmountDecimalPlaces), nameof(rate.ACT_RX_NKCommissionCurrency), rate);
			tester.CheckConstant(percentList, nameof(rate.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		public void TestACT_CommissionType_ResetsUnusedValues()
		{
			var rate = Factory.New<AccCommissionRuleRate>();
			rate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			rate.ACT_RX_NKCommissionCurrency = "AUD";
			rate.ACT_CommissionAmount = 100d;

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals("", rate.ACT_RX_NKCommissionCurrency);
			AssertEquals(ZDecimal.Zero, rate.ACT_CommissionAmount);

			rate.ACT_CommissionPercentage = 10d;
			rate.ACT_CommissionType = "";
			AssertEquals(ZDecimal.Zero, rate.ACT_CommissionPercentage);
		}

		public void TestACT_CommissionType_DefaultCommissionCurrency()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_RX_NKLocalCurrency = "USD";
			var companySpecificTeam = Factory.New<SalesTeam>();
			companySpecificTeam.GG_GC = anotherCompany.PK;
			var globalTeam = Factory.New<SalesTeam>();
			globalTeam.GG_GC = ZGuid.Empty;

			var globalTeamRule = globalTeam.CommissionRules.AddNew();
			var globalTeamRuleRate = globalTeamRule.Rates.AddNew();
			globalTeamRuleRate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals("AUD", globalTeamRuleRate.ACT_RX_NKCommissionCurrency);

			var companySpecificTeamRule = companySpecificTeam.CommissionRules.AddNew();
			var companySpecificTeamRuleRate = companySpecificTeamRule.Rates.AddNew();
			companySpecificTeamRuleRate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals("USD", companySpecificTeamRuleRate.ACT_RX_NKCommissionCurrency);
		}

		public void TestACT_CommissionPercentage_ReadOnly()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = "";
			AssertEquals(true, rate.ACT_CommissionPercentageInfo.ReadOnly);

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals(false, rate.ACT_CommissionPercentageInfo.ReadOnly);

			rate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals(true, rate.ACT_CommissionPercentageInfo.ReadOnly);

			rate.ACT_CommissionType = "XXX";
			AssertEquals(true, rate.ACT_CommissionPercentageInfo.ReadOnly);
		}

		public void TestACT_RX_NKCommissionCurrency_ReadOnly()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = "";
			AssertEquals(true, rate.ACT_RX_NKCommissionCurrencyInfo.ReadOnly);

			rate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals(false, rate.ACT_RX_NKCommissionCurrencyInfo.ReadOnly);

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals(true, rate.ACT_RX_NKCommissionCurrencyInfo.ReadOnly);

			rate.ACT_CommissionType = "XXX";
			AssertEquals(true, rate.ACT_RX_NKCommissionCurrencyInfo.ReadOnly);
		}

		public void TestACT_CommissionAmount_ReadOnly()
		{
			var rate = Factory.New<AccCommissionRuleRate>();

			rate.ACT_CommissionType = "";
			AssertEquals(true, rate.ACT_CommissionAmountInfo.ReadOnly);

			rate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			AssertEquals(false, rate.ACT_CommissionAmountInfo.ReadOnly);

			rate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			AssertEquals(true, rate.ACT_CommissionAmountInfo.ReadOnly);

			rate.ACT_CommissionType = "XXX";
			AssertEquals(true, rate.ACT_CommissionAmountInfo.ReadOnly);
		}

		public void TestCommissionAmountDecimalPlaces()
		{
			var rate = Factory.New<AccCommissionRuleRate>();
			rate.ACT_RX_NKCommissionCurrency = "";
			AssertEquals(2, rate.CommissionAmountDecimalPlaces);

			rate.ACT_RX_NKCommissionCurrency = "IDR";
			AssertEquals(0, rate.CommissionAmountDecimalPlaces);

			rate.ACT_RX_NKCommissionCurrency = "USD";
			AssertEquals(2, rate.CommissionAmountDecimalPlaces);
		}

		public void TestPropertyMaxLength()
		{
			AssertEquals(OrgCommissionAgreementRecipientRate.Schema.CAT_CommissionPeriodMaxLength, AccCommissionRuleRate.Schema.ACT_CommissionPeriodMaxLength);
			AssertEquals(OrgCommissionAgreementRecipient.Schema.CAR_CommissionTypeMaxLength, AccCommissionRuleRate.Schema.ACT_CommissionTypeMaxLength);
			AssertEquals(OrgCommissionAgreementRecipientRate.Schema.CAT_RX_NKCommissionCurrencyMaxLength, AccCommissionRuleRate.Schema.ACT_RX_NKCommissionCurrencyMaxLength);
		}
	}
}
