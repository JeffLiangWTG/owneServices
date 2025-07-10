using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateItem))]
	sealed class AccOrgTaxConfigurationTemplateItemTest : AccOrgTaxConfigurationTest
	{
		public override void TestGetParentCompany()
		{
			var configForTemplate = Factory.New<AccOrgTaxConfigurationTemplateItem>();
			AssertEquals("Should be null if it's a standalone tax config for template", null, configForTemplate.GetParentCompany());

			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_GC_Company = GlbCompany.CurrentCompany.PK;
			configForTemplate = template.AccOrgTaxConfigurations.AddNew();
			AssertEquals("Should be template company", GlbCompany.CurrentCompany.PK, configForTemplate.GetParentCompany().PK);

			template.OCT_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			AssertEquals("Should be template company", TestObjectCreator.NonCurrentCompany.PK, configForTemplate.GetParentCompany().PK);
		}

		public void TestPropertyGettersForAPTemplate()
		{
			AssertPropertyGettersCore(false);
		}

		public void TestPropertyGettersForARTemplate()
		{
			AssertPropertyGettersCore(true);
		}

		void AssertPropertyGettersCore(bool isReceivable)
		{
			var ledger = isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			var taxSystem = TestObjectCreator.CreateTaxSystem("TTS", registrationLevel: AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Branch.Code);
			var taxAuthority = TestObjectCreator.CreateTaxAuthority("TTA");
			var code_TTA = new CodeDescriptionPair(taxAuthority.Code.ToString(), taxAuthority.Name.ToString());

			TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper().WithGetTaxSystem(Factory, taxSystem).WithGetTaxSystems(taxSystem)
																					.WithGetTaxAuthorities(Core.Constants.CountryCodes.Australia, null, new CodeDescriptionPairList { code_TTA });

			var taxConfig = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, ledger);
			taxConfig.ETC_Code = "TestCode";
			taxConfig.ETC_Description = "TestDesc";
			Factory.Save();

			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = isReceivable;
			Factory.Save();

			var configForTemplate = template.AccOrgTaxConfigurations.AddNew();

			AssertEquals("Should be default values", string.Empty, configForTemplate.OTC_Code);
			AssertEquals(string.Empty, configForTemplate.OTC_TaxSystem);
			AssertEquals(string.Empty, configForTemplate.OTC_SuperType);
			AssertEquals(string.Empty, configForTemplate.OTC_BranchCode);
			AssertEquals(string.Empty, configForTemplate.OTC_TaxAuthority);
			AssertEquals(string.Empty, configForTemplate.OTC_Ledger);
			AssertNotNull(configForTemplate.OrgTaxConfigurationTemplate);
			AssertEquals("Value is from parent template", ledger, configForTemplate.Ledger);

			configForTemplate.OTC_ETC = taxConfig.PK;

			AssertEquals("Should be new tax config values", taxConfig.ETC_Code, configForTemplate.OTC_Code);
			AssertEquals(taxSystem.Code, configForTemplate.OTC_TaxSystem);
			AssertEquals(taxSystem.TaxSuperType, configForTemplate.OTC_SuperType);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, configForTemplate.OTC_BranchCode);
			AssertEquals(taxAuthority.Code, configForTemplate.OTC_TaxAuthority);
			AssertEquals(ledger, configForTemplate.OTC_Ledger);
			AssertNotNull(configForTemplate.OrgTaxConfigurationTemplate);
		}

		public void TestLookupAPTaxConfigurations()
		{
			AssertLookupTaxConfigurationsCore(false);
		}

		public void TestLookupARTaxConfigurations()
		{
			AssertLookupTaxConfigurationsCore(true);
		}

		void AssertLookupTaxConfigurationsCore(bool isReceivable)
		{
			var ledger = isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			var reversedLedger = !isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			var taxSystem = TestObjectCreator.CreateTaxSystem("TTS");
			var taxAuthority = TestObjectCreator.CreateTaxAuthority("TTA");
			Factory.Save();

			var config1 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, ledger);
			var config2 = TestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, ledger);
			var config3 = TestObjectCreator.CreateTaxConfiguration(TestObjectCreator.NonCurrentCompany, taxAuthority, taxSystem, ledger);
			var config4 = TestObjectCreator.CreateTaxConfiguration(TestObjectCreator.NonCurrentCompanyBranch, taxAuthority, taxSystem, ledger);
			var config5 = TestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, ledger, active: false);
			var configR = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, reversedLedger);
			config1.ETC_Code = "TT1";
			config2.ETC_Code = "TT2";
			config3.ETC_Code = "TT3";
			config4.ETC_Code = "TT4";
			config5.ETC_Code = "TT5";
			configR.ETC_Code = "TT6";
			Factory.Save();

			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_GC_Company = GlbCompany.CurrentCompany.PK;
			template.OCT_IsReceivable = isReceivable;
			var configForTemplate = template.AccOrgTaxConfigurations.AddNew();

			var configs = configForTemplate.Lookups.TaxConfigurations;
			AssertEquals(3, configs.Count);
			AssertArrayEqualsByElements("Only configs relative to current company and branch, active or not, should be included", new ZGuid[] { config1.PK, config2.PK, config5.PK }, configs.Select(x => x.PK).ToArray());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplateItem>();

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		#endregion
	}
}
