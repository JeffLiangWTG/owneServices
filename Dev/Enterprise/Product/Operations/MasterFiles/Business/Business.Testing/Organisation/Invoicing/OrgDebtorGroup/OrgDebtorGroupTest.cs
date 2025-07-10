using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDebtorGroup))]
	public class OrgDebtorGroupTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgDebtorGroup debtorGroup = factory.New<OrgDebtorGroup>();
			AccBankAccount account = factory.NewWithValidTestData<AccBankAccount>();

			debtorGroup.OJ_Code = "XYZ";
			debtorGroup.OJ_Desc = "Desc";
			debtorGroup.DefaultBankAccountPK = account.PK;

			return debtorGroup;
		}

		public void TestBusinessObjectsWithRelatedEventsCore()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.Load();
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			debtorGroup.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.Domestic, Core.Constants.TransportModes.Air, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var systemLevelExRateConfig = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);
			AssertNotNull("System level Ex Rate Config", systemLevelExRateConfig);
			var companyLevelExRateConfig = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level Ex Rate Config", companyLevelExRateConfig);
			var debtorGroupLevelExRateConfig = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup);
			AssertNotNull("Debtor group level Ex Rate Config", debtorGroupLevelExRateConfig);

			var bizObjsWithRelatedEvents = debtorGroup.BusinessObjectsWithRelatedEvents;

			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain system level Ex Rate config.", systemLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain company level Ex Rate config.", companyLevelExRateConfig, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain debtor group level Ex Rate config.", debtorGroupLevelExRateConfig, bizObjsWithRelatedEvents);
		}

		public void TestAccExchangeRateConfigurations()
		{
			var orgDebtGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			AssertNotNull(orgDebtGroup.AccExchangeRateConfigurations);
			AssertEquals(AccExRateConfigurationLevelEnum.DebtorGroup, orgDebtGroup.AccExchangeRateConfigurations.Level);
		}

		public void TestAccountFeeSettings()
		{
			var debtorGroup = Factory.New<OrgDebtorGroup>();
			debtorGroup.AccountFeeSettings.OverrideSettings = true;
			debtorGroup.AccountFeeSettings.AAF_FeeAmount = -25m;
			debtorGroup.RunPreSaveValidation();
			AssertHasErrorContaining(debtorGroup.AccountFeeSettings.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");
		}

		[ExpectNoExceptions()]
		public void TestDontCreateEmptyChildWhenGettingDebtorGroupBankDefault()
		{
			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();

			AssertNull(debtorGroup.DefaultBankAccount);
			Factory.Save();
		}

		public void TestDeleteDebtorGroupWithMultipleBankDefaults()
		{
			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();

			debtorGroup.OJ_Code = "XYZ";
			debtorGroup.OJ_Desc = "Desc";
			debtorGroup.DefaultBankAccountPK = account.PK;
			OrgDebtorGroupBankDefault currentCompanyBankDefault = debtorGroup.DebtorGroupBankDefault;

			OrgDebtorGroupBankDefault otherCompanyBankDefault = Factory.New<OrgDebtorGroupBankDefault>();
			otherCompanyBankDefault.P6_AB = account.PK; // usually bank account would belong to a different company, but in this case, doesn't matter.
			otherCompanyBankDefault.P6_OJ = debtorGroup.PK;
			otherCompanyBankDefault.P6_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			debtorGroup.Delete();

			Assert("First debtor group bank default should be deleted", currentCompanyBankDefault.IsDeleted);
			Assert("Second Company debtor group bank default should be deleted", otherCompanyBankDefault.IsDeleted);
		}

		public void TestCompanySpecificDebtorGroupBankDefaultCreation()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch newCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompanyBranch.GB_GC = newCompany.PK;

			GlbCompany secondNewCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch secondNewCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			secondNewCompanyBranch.GB_GC = secondNewCompany.PK;

			AccBankAccount newCompanyBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			newCompanyBankAccount.AB_GC = newCompany.PK;

			AccBankAccount secondNewCompanyBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			secondNewCompanyBankAccount.AB_GC = secondNewCompany.PK;

			Factory.Save();

			BusinessObjectFactory factoryForGroupCreation = new BusinessObjectFactory();
			OrgDebtorGroup newGroup = factoryForGroupCreation.New<OrgDebtorGroup>();
			newGroup.OJ_Code = "AVS";
			newGroup.OJ_Desc = "AVS DESC";
			factoryForGroupCreation.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory newCompanyFactory = new BusinessObjectFactory();
				OrgDebtorGroup groupInNewCompanyFactory = newCompanyFactory.Load<OrgDebtorGroup>(newGroup.PK);
				groupInNewCompanyFactory.DefaultBankAccountPK = newCompanyBankAccount.PK;
				newCompanyFactory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, secondNewCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				BusinessObjectFactory secondNewCompanyFactory = new BusinessObjectFactory();
				OrgDebtorGroup groupInSecondNewCompanyFactory = secondNewCompanyFactory.Load<OrgDebtorGroup>(newGroup.PK);
				groupInSecondNewCompanyFactory.DefaultBankAccountPK = secondNewCompanyBankAccount.PK;
				secondNewCompanyFactory.Save();

				BusinessObjectFactory loadFactory = new BusinessObjectFactory();
				OrgDebtorGroup reloadedGroup = loadFactory.Load<OrgDebtorGroup>(newGroup.PK);
				OrgDebtorGroupBankDefaultCollection bankDefaults = new OrgDebtorGroupBankDefaultCollection(reloadedGroup, null);
				bankDefaults.Load();
				AssertEquals("Should have 2 bank defaults", 2, bankDefaults.Count);
				AssertEquals("Should have 1 new company bank default", 1, bankDefaults.Find(new ZQuery(OrgDebtorGroupBankDefaultSchema.P6_GC, newCompany.PK)).Length);
				AssertEquals("Should have 1 second new company bank default", 1, bankDefaults.Find(new ZQuery(OrgDebtorGroupBankDefaultSchema.P6_GC, secondNewCompany.PK)).Length);
			}
		}

		public void TestDefaultBankAccount()
		{
			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();

			debtorGroup.DefaultBankAccountPK = account.PK;
			AssertEquals("The DebtorGroupBankAccount's account field is set", account.PK, debtorGroup.DebtorGroupBankDefault.P6_AB);

			debtorGroup.DefaultBankAccountPK = ZGuid.Empty;
			AssertEquals("Should be debtor groups defaults in the collection", 1, debtorGroup.BankDefaults.Count);
			AssertEquals("The DebtorGroupBankAccount's account field is set", ZGuid.Empty, debtorGroup.DebtorGroupBankDefault.P6_AB);

			debtorGroup.DefaultBankAccountPK = account.PK;
			AssertEquals("The DebtorGroupBankAccount's account field is set", account.PK, debtorGroup.DebtorGroupBankDefault.P6_AB);

			debtorGroup.DefaultBankAccountPK = ZGuid.Invalid;
			AssertEquals("Should be debtor groups defaults in the collection", 1, debtorGroup.BankDefaults.Count);
			AssertEquals("The DebtorGroupBankAccount's account field is set", ZGuid.Invalid, debtorGroup.DebtorGroupBankDefault.P6_AB);
		}

		public void TestGetBankAccountByCurrency()
		{
			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();
			RefCurrency currency1 = Factory.LoadTop1<RefCurrency>(new ZQuery());
			RefCurrency currency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.PK, SQLComparisonOperator.NotEqual, currency1.PK));
			debtorGroup.DebtorGroupBankDefault.P6_OverrideRegistryCurrencyToBankSetting = true;
			debtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.AddNew();
			debtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency = currency1.RX_Code;
			debtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_AB = account.PK;
			AssertEquals("Returned Bank Account Must be null", debtorGroup.GetBankAccountByCurrency(currency2.RX_Code), null);

			debtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.AddNew();
			debtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[1].PB_RX_NKCurrency = currency2.RX_Code;
			debtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[1].PB_AB = account.PK;
			AssertEquals("Returned Bank Account Must be equal setted bank accaunt", debtorGroup.GetBankAccountByCurrency(currency2.RX_Code), account);
		}

		public void TestOverrideRegistryCurrencyToBankSetting()
		{
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			collection.AddNew();
			collection[0].Currency = "USD";
			collection[0].BankAccount = account.PK;
			OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			OrgDebtorGroup orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.DefaultBankAccountPK = ZGuid.Empty;
			orgDebtorGroup.OverrideRegistryCurrencyToBankSetting = false;
			AssertEquals("Currency must be USD", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency, "USD");
			AssertEquals("Bank account must be the same as in registry", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_AB, account.PK);
			AssertEquals("Grid must be readonly", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.ReadOnly, true);
			AssertNoErrors("Must be no errors when DefaultBankAccount is blank", orgDebtorGroup.DefaultBankAccountPKInfo);

			orgDebtorGroup.OverrideRegistryCurrencyToBankSetting = true;
			AssertEquals("Currency must be USD", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency, "USD");
			AssertEquals("Bank account must be the same as in registry", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_AB, account.PK);
			AssertEquals("Grid must be editable", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.ReadOnly, false);
			AssertHasErrors("Must be errors when DefaultBankAccount is blank", orgDebtorGroup.DefaultBankAccountPKInfo);

			orgDebtorGroup.DefaultBankAccountPK = account.PK;
			orgDebtorGroup.OverrideRegistryCurrencyToBankSetting = true;
			AssertNoErrors("Must be no errors when DefaultBankAccount filled", orgDebtorGroup.DefaultBankAccountPKInfo);
		}

		public void TestOrgDebtorGroupBankCurrentOverrideCollection()
		{
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			collection.AddNew();
			collection[0].Currency = "USD";
			collection[0].BankAccount = account.PK;
			OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			OrgDebtorGroup orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.DefaultBankAccountPK = account.PK;
			AssertEquals("Currency must be USD", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency, "USD");
			AssertEquals("Bank account must be the same as in registry", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_AB, account.PK);
			AssertEquals("Grid must be readonly", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.ReadOnly, true);
			orgDebtorGroup.OverrideRegistryCurrencyToBankSetting = true;
			orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency = "AUD";

			Factory.Save();
			orgDebtorGroup = Factory.Load<OrgDebtorGroup>(orgDebtorGroup.PK);
			AssertEquals("Currency must be AUD", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_RX_NKCurrency, "AUD");
			AssertEquals("Bank account must be the same as in registry", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection[0].PB_AB, account.PK);
			AssertEquals("Grid must be editable", orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.ReadOnly, false);
		}

		public void TestOnFactorySavingAndOnFactorySaved()
		{
			AccBankAccount account = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			collection.AddNew();
			collection[0].Currency = "USD";
			collection[0].BankAccount = account.PK;
			OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			OrgDebtorGroup orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "001";
			orgDebtorGroup.DefaultBankAccountPK = account.PK;
			AssertEquals("DebtorGroupBankDefault must exist", false, orgDebtorGroup.DebtorGroupBankDefault.IsDeleted);
			AssertEquals("Collection must have one row from registry", 1, orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.Count);
			Factory.Save();
			AssertEquals("DebtorGroupBankDefault must exist", false, orgDebtorGroup.DebtorGroupBankDefault.IsDeleted);
			AssertEquals("Collection must have one row from registry", 1, orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.Count);
			AssertNoErrors("Saving must be perfect", orgDebtorGroup);

			orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "002";
			orgDebtorGroup.DefaultBankAccountPK = ZGuid.Empty;
			AssertEquals("DebtorGroupBankDefault must exist", false, orgDebtorGroup.DebtorGroupBankDefault.IsDeleted);
			AssertEquals("Collection must have one row from registry", 1, orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.Count);
			Factory.Save();
			AssertEquals("DebtorGroupBankDefault must exist", false, orgDebtorGroup.DebtorGroupBankDefault.IsDeleted);
			AssertEquals("Collection must have one row from registry", 1, orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.Count);
			AssertNoErrors("Saving must be perfect", orgDebtorGroup);

			orgDebtorGroup = Factory.New<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "003";
			orgDebtorGroup.DefaultBankAccountPK = account.PK;
			orgDebtorGroup.OverrideRegistryCurrencyToBankSetting = true;
			orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("DebtorGroupBankDefault must exist", false, orgDebtorGroup.DebtorGroupBankDefault.IsDeleted);
			AssertEquals("Collection must don't have rows because it's loading from empty database", 0, orgDebtorGroup.OrgDebtorGroupBankCurrentOverrideCollection.Count);
			AssertNoErrors("Saving must be perfect", orgDebtorGroup);
		}

		public void TestCascadeDeleteExchangeRateConfigurations()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var company = Factory.New<GlbCompany>();
			company.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			Factory.Save();
			debtorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var anotherDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			Factory.Save();
			anotherDebtorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			Factory.Save();
			creditorGroup.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			var orgHeader = testObjectCreator.CreateOrgHeader("DMO", true, true);
			orgHeader.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			orgHeader.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);

			Factory.Save();

			var systemLevelPK = systemLevelExchangeRateConfigs.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.System).PK;
			var companyLevelPK = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Company).PK;
			var debtorGroupPK = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup).PK;
			var anotherDebtorGroupPK = anotherDebtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup).PK;
			var creditorGroupPK = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup).PK;
			var orgHeaderARPK = orgHeader.CompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Debtor).PK;
			var orgHeaderAPPK = orgHeader.CompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().First(x => x.Level == AccExRateConfigurationLevelEnum.Creditor).PK;

			var queryForExRateConfig = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, debtorGroupPK, creditorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherDebtorGroupPK }));
			var exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfig);
			AssertEquals(7, exRateConfigCount.Length);

			debtorGroup.AccExchangeRateConfigurations.Reload(true);
			debtorGroup.Delete();
			Factory.Save();

			var queryForExRateConfigExceptDebtorLevel = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration))
										.AddToFilter(new ZQuery(AccExchangeRateConfigurationViewSchema.PK,
														new List<ZGuid>() { systemLevelPK, companyLevelPK, creditorGroupPK, orgHeaderARPK, orgHeaderAPPK, anotherDebtorGroupPK }));
			exRateConfigCount = Factory.Load<AccExchangeRateConfiguration>(queryForExRateConfigExceptDebtorLevel);
			AssertEquals(true, Factory.Exists(typeof(AccExchangeRateConfiguration), queryForExRateConfigExceptDebtorLevel, true));
			AssertEquals(6, exRateConfigCount.Length);
		}

		public void TestNoAuditLogs()
		{
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, debtorGroup.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				debtorGroup.OJ_Desc = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				debtorGroup.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}
	}
}
