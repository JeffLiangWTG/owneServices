using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccOrgTaxConfigurationTemplateItemValidationTest : AccOrgTaxConfigurationValidationTest
	{
		public void TestAddOrgTaxConfigurationWithDuplicateCodeForAPTemplate()
		{
			AssertAddOrgTaxConfigurationWithDuplicateCodeCore(false);
		}

		public void TestAddOrgTaxConfigurationWithDuplicateCodeForARTemplate()
		{
			AssertAddOrgTaxConfigurationWithDuplicateCodeCore(true);
		}

		void AssertAddOrgTaxConfigurationWithDuplicateCodeCore(bool isReceivable)
		{
			var ledger = isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			var taxSystem = TestObjectCreator.CreateTaxSystem("TTS");
			var taxAuthority = TestObjectCreator.CreateTaxAuthority("TTA");
			var taxConfig1 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, ledger);
			var taxConfig2 = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, ledger);
			taxConfig1.ETC_Code = "TT1";
			taxConfig2.ETC_Code = "TT2";
			Factory.Save();

			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = isReceivable;
			Factory.Save();

			var orgConfig1 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfig1);
			var orgConfig2 = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfig2);
			Factory.Save();

			AssertNoErrors("Pre-condition", orgConfig1.OTC_ETCInfo);
			AssertNoErrors(orgConfig2.OTC_ETCInfo);

			var orgConfig1Duplicate = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfig1);

			AssertNoErrors("Old record does NOT have duplicate error", orgConfig1.OTC_ETCInfo);
			AssertHasError("New record has duplicate error", orgConfig1Duplicate.OTC_ETCInfo, "Same Tax Configuration can only be listed in Template once.");
			AssertNoErrors(orgConfig2.OTC_ETCInfo);

			orgConfig1Duplicate.Delete();

			AssertNoErrors("Duplication removed", orgConfig1.OTC_ETCInfo);
			AssertNoErrors(orgConfig2.OTC_ETCInfo);
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
