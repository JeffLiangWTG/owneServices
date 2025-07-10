using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateItemCollection))]
	sealed class AccOrgTaxConfigurationTemplateItemCollectionTest : ActiveBusinessObjectCollectionTestCase<AccOrgTaxConfigurationTemplateItemCollection>
	{
		public void TestLoadAPElements()
		{
			AssertLoadElementsCore(false);
		}

		public void TestLoadARElements()
		{
			AssertLoadElementsCore(true);
		}

		void AssertLoadElementsCore(bool isReceivable)
		{
			var ledger = isReceivable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			var taxSystem = TestObjectCreator.CreateTaxSystem("TTS");
			var taxAuthority = TestObjectCreator.CreateTaxAuthority("TTA");
			var taxConfig = TestObjectCreator.CreateTaxConfiguration(GlbBranch.CurrentBranch, taxAuthority, taxSystem, ledger);
			taxConfig.ETC_Code = "TS1";
			Factory.Save();

			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			template.OCT_IsReceivable = isReceivable;
			Factory.Save();

			var templateConfigItem = TestObjectCreator.AddOrgTaxConfigurationForTemplate(template, taxConfig);
			Factory.Save();

			AssertEquals("Pre-condition", ledger, templateConfigItem.Ledger);
			AssertEquals(template.PK, templateConfigItem.OrgTaxConfigurationTemplate.PK);

			var newFactory = Factory.CreateNewFactory();
			var templateReloaded = newFactory.Load<AccOrgTaxConfigurationTemplate>(template.PK);

			AssertEquals("Should load relative tax config", 1, templateReloaded.AccOrgTaxConfigurations.Count);
			AssertEquals(templateConfigItem.PK, templateReloaded.AccOrgTaxConfigurations[0].PK);
		}

		protected override AccOrgTaxConfigurationTemplateItemCollection GetCollectionToTest()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			return new AccOrgTaxConfigurationTemplateItemCollection(template);
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
