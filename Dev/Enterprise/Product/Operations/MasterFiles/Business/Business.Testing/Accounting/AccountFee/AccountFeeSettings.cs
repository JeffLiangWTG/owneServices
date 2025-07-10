using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccountFeeSettings))]
	sealed class AccountFeeSettingsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccountFeeSettings(GlbCompany.CurrentCompany.PK, GlbCompany.CurrentCompany);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccountFeeSettings()
		{
			var settings = GlbCompany.CurrentCompany.AccountFeeSettings;

			var feeList = new List<string>
			{
				nameof(settings.AAF_FeeAmount)
			};

			var tester = new DecimalPlacesAttributeTester(settings, settings.Company);
			tester.CheckNonLocalCurrency(feeList, nameof(settings.AAF_FeeAmountDecimalPlaces), nameof(settings.AAF_RX_NKFeeCurrency), settings);
		}

		public void TestAccountFeeForACompany()
		{
			SetUpData();

			var settings = GlbCompany.CurrentCompany.AccountFeeSettings;
			settings.PopulateFields();

			AssertEquals("GLAccount", ZGuid.Empty, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, settings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, settings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, settings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = true;
			settings.AAF_AG_GLAccount = GLHeader.PK;
			settings.AAF_FeeAmount = 500m;
			settings.AAF_Rule = "TCR";
			settings.AAF_RX_NKFeeCurrency = currency.RX_Code;
			GlbCompany.CurrentCompany.Factory.Save();

			var company = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			AssertEquals("GLAccount", GLHeader.PK, company.AccountFeeSettings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 500m, company.AccountFeeSettings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", true, company.AccountFeeSettings.OverrideSettings);
			AssertEquals("Rule", "TCR", company.AccountFeeSettings.AAF_Rule);
			AssertEquals("Currency", currency.RX_Code, company.AccountFeeSettings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = false;
			GlbCompany.CurrentCompany.Factory.Save();

			company = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			AssertEquals("GLAccount", ZGuid.Empty, company.AccountFeeSettings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, company.AccountFeeSettings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, company.AccountFeeSettings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, company.AccountFeeSettings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, company.AccountFeeSettings.AAF_RX_NKFeeCurrency);
		}

		public void TestAccountFeeForAOrg()
		{
			SetUpData();

			var settings = org1.CompanyData.AccountFeeSettings;
			settings.PopulateFields();

			AssertEquals("GLAccount", ZGuid.Empty, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, settings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, settings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, settings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = true;
			settings.AAF_AG_GLAccount = GLHeader.PK;
			settings.AAF_FeeAmount = 500m;
			settings.AAF_Rule = "TCR";
			settings.AAF_RX_NKFeeCurrency = currency.RX_Code;
			Factory.Save();

			var org = new BusinessObjectFactory().Load<OrgHeader>(org1.PK);
			AssertEquals("GLAccount", GLHeader.PK, org.CompanyData.AccountFeeSettings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 500m, org.CompanyData.AccountFeeSettings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", true, org.CompanyData.AccountFeeSettings.OverrideSettings);
			AssertEquals("Rule", "TCR", org.CompanyData.AccountFeeSettings.AAF_Rule);
			AssertEquals("Currency", currency.RX_Code, org.CompanyData.AccountFeeSettings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = false;
			Factory.Save();

			org = new BusinessObjectFactory().Load<OrgHeader>(org1.PK);
			AssertEquals("GLAccount", ZGuid.Empty, org.CompanyData.AccountFeeSettings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, org.CompanyData.AccountFeeSettings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, org.CompanyData.AccountFeeSettings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, org.CompanyData.AccountFeeSettings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, org.CompanyData.AccountFeeSettings.AAF_RX_NKFeeCurrency);
		}

		public void TestAccountFeeForADebtorGroup()
		{
			SetUpData();

			var settings = debtorGroup.AccountFeeSettings;
			settings.PopulateFields();

			AssertEquals("GLAccount", ZGuid.Empty, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, settings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, settings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, settings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = true;
			settings.AAF_AG_GLAccount = GLHeader.PK;
			settings.AAF_FeeAmount = 500m;
			settings.AAF_Rule = "TCR";
			settings.AAF_RX_NKFeeCurrency = currency.RX_Code;
			Factory.Save();

			var debtGrp = new BusinessObjectFactory().Load<OrgDebtorGroup>(debtorGroup.PK);
			AssertEquals("GLAccount", GLHeader.PK, debtGrp.AccountFeeSettings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 500m, debtGrp.AccountFeeSettings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", true, debtGrp.AccountFeeSettings.OverrideSettings);
			AssertEquals("Rule", "TCR", debtGrp.AccountFeeSettings.AAF_Rule);
			AssertEquals("Currency", currency.RX_Code, debtGrp.AccountFeeSettings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = false;
			Factory.Save();

			debtGrp = new BusinessObjectFactory().Load<OrgDebtorGroup>(debtorGroup.PK);
			AssertEquals("GLAccount", ZGuid.Empty, debtGrp.AccountFeeSettings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, debtGrp.AccountFeeSettings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, debtGrp.AccountFeeSettings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, debtGrp.AccountFeeSettings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, debtGrp.AccountFeeSettings.AAF_RX_NKFeeCurrency);
		}

		public void TestWhenOverrideIsTickedOffFallBackValueIsLoaded()
		{
			SetUpData();

			var settings = debtorGroup.AccountFeeSettings;
			settings.PopulateFields();

			AssertEquals("GLAccount", ZGuid.Empty, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 0m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, settings.OverrideSettings);
			AssertEquals("Rule", ZString.Empty, settings.AAF_Rule);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, settings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = true;
			settings.AAF_AG_GLAccount = GLHeader.PK;
			settings.AAF_FeeAmount = 500m;
			settings.AAF_Rule = "TCR";
			settings.AAF_RX_NKFeeCurrency = currency.RX_Code;
			Factory.Save();

			AssertEquals("GLAccount", GLHeader.PK, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 500m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", true, settings.OverrideSettings);
			AssertEquals("Rule", "TCR", settings.AAF_Rule);
			AssertEquals("Currency", currency.RX_Code, settings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = false;

			AssertEquals("GLAccount (As there is no Fallback Value for this Debtor group)", ZGuid.Empty, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount (As there is no Fallback Value for this Debtor group)", 0m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings (As there is no Fallback Value for this Debtor group)", false, settings.OverrideSettings);
			AssertEquals("Rule (As there is no Fallback Value for this Debtor group)", ZString.Empty, settings.AAF_Rule);
			AssertEquals("Currency (As there is no Fallback Value for this Debtor group)", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, settings.AAF_RX_NKFeeCurrency);

			settings.OverrideSettings = true;

			//Setup in Company Level
			var ovrSettings = GlbCompany.CurrentCompany.AccountFeeSettings;
			ovrSettings.PopulateFields();
			ovrSettings.OverrideSettings = true;
			ovrSettings.AAF_AG_GLAccount = GLHeader2.PK;
			ovrSettings.AAF_FeeAmount = 1500m;
			ovrSettings.AAF_Rule = "TCB";
			ovrSettings.AAF_RX_NKFeeCurrency = currency.RX_Code;
			GlbCompany.CurrentCompany.Factory.Save();

			settings.OverrideSettings = false;

			AssertEquals("GLAccount", GLHeader2.PK, settings.AAF_AG_GLAccount);
			AssertEquals("Fee Amount", 1500m, settings.AAF_FeeAmount);
			AssertEquals("OverrideSettings", false, settings.OverrideSettings);
			AssertEquals("Rule", "TCB", settings.AAF_Rule);
			AssertEquals("Currency", currency.RX_Code, settings.AAF_RX_NKFeeCurrency);
		}

		public void TestAccFeeRuleNONSetsAmountToZero()
		{
			var settings = GlbCompany.CurrentCompany.AccountFeeSettings;
			settings.PopulateFields();

			settings.AAF_Rule = AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted;
			settings.AAF_FeeAmount = 250m;

			AssertEquals("AAF Rule (before)", AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, settings.AAF_Rule);
			AssertEquals("AAF Fee Amount (before)", 250m, settings.AAF_FeeAmount);

			settings.AAF_Rule = AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee;

			AssertEquals("AAF Rule (After)", AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee, settings.AAF_Rule);
			AssertEquals("AAF Fee Amount (After)", 0m, settings.AAF_FeeAmount);
		}

		public void TestHasAccountFeeSettingsAtCompanyLevel()
		{
			try
			{
				SetUpData();

				AssertEquals("There should be no AccountFeeSettings", false, AccountFeeSettings.HasAccountFeeSettings(GlbCompany.CurrentCompany.PK));

				var settings = GlbCompany.CurrentCompany.AccountFeeSettings;
				settings.OverrideSettings = true;
				settings.AAF_Rule = AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted;
				settings.AAF_FeeAmount = 250m;
				settings.AAF_AG_GLAccount = GLHeader.PK;
				settings.AAF_RX_NKFeeCurrency = currency.RX_Code;
				GlbCompany.CurrentCompany.Factory.Save();

				AssertEquals("There should be no AccountFeeSettings", true, AccountFeeSettings.HasAccountFeeSettings(GlbCompany.CurrentCompany.PK));
			}
			finally
			{
				var settings = GlbCompany.CurrentCompany.AccountFeeSettings;
				settings.OverrideSettings = false;
				GlbCompany.CurrentCompany.Factory.Save();
			}
		}

		public void TestAccountFeeSettings_ReadOnly()
		{
			var accountFeeSettings = GlbCompany.CurrentCompany.AccountFeeSettings;
			var initialValue = Env.Security.OrgReceivablesModifyAccountFee.IsAllowed;
			try
			{
				Env.Security.OrgReceivablesModifyAccountFee.IsAllowed = true;

				CombineAssertions("ReadOnly should be false.", () =>
				{
					AssertEquals(false, accountFeeSettings.OverrideSettingsInfo.ReadOnly);
					AssertEquals(false, accountFeeSettings.AAF_AG_GLAccountInfo.ReadOnly);
					AssertEquals(false, accountFeeSettings.AAF_RuleInfo.ReadOnly);
					AssertEquals(false, accountFeeSettings.AAF_FeeAmountInfo.ReadOnly);
				});

				Env.Security.OrgReceivablesModifyAccountFee.IsAllowed = false;
				CombineAssertions("ReadOnly should be true.", () =>
				{
					AssertEquals(true, accountFeeSettings.OverrideSettingsInfo.ReadOnly);
					AssertEquals(true, accountFeeSettings.AAF_AG_GLAccountInfo.ReadOnly);
					AssertEquals(true, accountFeeSettings.AAF_RuleInfo.ReadOnly);
					AssertEquals(true, accountFeeSettings.AAF_FeeAmountInfo.ReadOnly);
				});
			}
			finally
			{
				Env.Security.OrgReceivablesModifyAccountFee.IsAllowed = initialValue;
			}
		}

		void SetUpData()
		{
			GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			org1 = Factory.NewWithValidTestData<OrgHeader>();
			debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			currency = Factory.NewWithValidTestData<RefCurrency>();
			GLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();

			Factory.Save();
		}

		OrgHeader org1;
		AccGLHeader GLHeader;
		AccGLHeader GLHeader2;
		OrgDebtorGroup debtorGroup;
		RefCurrency currency;
	}
}

