using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccExchangeRateConfigurationCollection))]
	sealed class AccExchangeRateConfigurationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAPCollectionShouldNotContainAREntityAndViceVersa()
		{
			var company = GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_GC;
			var orgHeader = GlbCompany.CurrentCompany.OrgProxy.CompanyData.OB_OH;

			var initilOrgConfigs = new AccExchangeRateConfigurationCollection(Factory, company, string.Empty, ZGuid.Empty, orgHeader);
			initilOrgConfigs.Load();
			AssertHasNonDefaultConfigs(0, initilOrgConfigs);

			var apCreditorConfig = Factory.New<AccExchangeRateConfiguration>();
			apCreditorConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			apCreditorConfig.JCE_ParentID = GlbCompany.CurrentCompany.OrgProxy.PK;
			apCreditorConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccExRateConfigurationLevelEnum.Creditor, apCreditorConfig.Level);

			var arDebtorConfig = Factory.New<AccExchangeRateConfiguration>();
			arDebtorConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			arDebtorConfig.JCE_ParentID = GlbCompany.CurrentCompany.OrgProxy.PK;
			arDebtorConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccExRateConfigurationLevelEnum.Debtor, arDebtorConfig.Level);

			Factory.Save();

			var arDebtorConfigs = new AccExchangeRateConfigurationCollection(Factory, company, LedgerTypes.AccountsReceivable, ZGuid.Empty, orgHeader);
			arDebtorConfigs.Load();
			AssertHasNonDefaultConfigs(1, arDebtorConfigs);
			Assert(!arDebtorConfigs.Cast<AccExchangeRateConfiguration>().Any(x => x.JCE_Ledger == LedgerTypes.AccountsPayable));

			var apCreditorConfigs = new AccExchangeRateConfigurationCollection(Factory, company, LedgerTypes.AccountsPayable, ZGuid.Empty, orgHeader);
			apCreditorConfigs.Load();
			AssertHasNonDefaultConfigs(1, apCreditorConfigs);
			Assert(!apCreditorConfigs.Cast<AccExchangeRateConfiguration>().Any(x => x.JCE_Ledger == LedgerTypes.AccountsReceivable));

			var apOrgGroup = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery()).PK;
			var arOrgGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery()).PK;

			var creditorGroupConfig = Factory.New<AccExchangeRateConfiguration>();
			creditorGroupConfig.JCE_ParentTableCode = OrgCreditorGroupSchema.Constants.Prefix;
			creditorGroupConfig.JCE_ParentID = apOrgGroup;
			creditorGroupConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccExRateConfigurationLevelEnum.CreditorGroup, creditorGroupConfig.Level);

			var debtorGroupConfig = Factory.New<AccExchangeRateConfiguration>();
			debtorGroupConfig.JCE_ParentTableCode = OrgDebtorGroupSchema.Constants.Prefix;
			debtorGroupConfig.JCE_ParentID = arOrgGroup;
			debtorGroupConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroupConfig.Level);

			Factory.Save();

			var debtorGroupConfigs = new AccExchangeRateConfigurationCollection(Factory, company, LedgerTypes.AccountsReceivable, arOrgGroup);
			debtorGroupConfigs.Load();
			AssertHasNonDefaultConfigs(1, debtorGroupConfigs);
			Assert(!debtorGroupConfigs.Cast<AccExchangeRateConfiguration>().Any(x => x.JCE_Ledger == LedgerTypes.AccountsPayable));

			var creditorGroupConfigs = new AccExchangeRateConfigurationCollection(Factory, company, LedgerTypes.AccountsPayable, apOrgGroup);
			creditorGroupConfigs.Load();
			AssertHasNonDefaultConfigs(1, creditorGroupConfigs);
			Assert(!creditorGroupConfigs.Cast<AccExchangeRateConfiguration>().Any(x => x.JCE_Ledger == LedgerTypes.AccountsReceivable));

			void AssertHasNonDefaultConfigs(int expectedCount, AccExchangeRateConfigurationCollection collection)
			{
				var defaultConfigs = collection.Cast<AccExchangeRateConfiguration>().Any(x => x.JCE_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated) ? 2 : 1;
				AssertEquals(expectedCount, collection.Count - defaultConfigs);
			}
		}

		public void TestSystemLevelCollectionReturnsCorrectSets()
		{
			var configPKs = PrepTestData(LedgerTypes.AccountsReceivable);

			var systemLevelCollection = new AccExchangeRateConfigurationCollection(Factory);

			AssertEquals(AccExRateConfigurationLevelEnum.System, systemLevelCollection.Level);

			systemLevelCollection.Load();

			Assert(systemLevelCollection.IsLoaded);
			AssertEquals(1, systemLevelCollection.Count);

			var sysLevelConfig = systemLevelCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault();

			AssertNotNull(sysLevelConfig);
			AssertEquals(AccExRateConfigurationLevelEnum.System, sysLevelConfig.Level);
			AssertEquals(configPKs.sysLevelPK, sysLevelConfig.PK);

			TestAddNew(systemLevelCollection, ZGuid.Empty);
		}

		public void TestCompanyLevelCollectionReturnsCorrectSets()
		{
			var configPKs = PrepTestData(LedgerTypes.AccountsReceivable);

			var companyLevelCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK);

			AssertEquals(AccExRateConfigurationLevelEnum.Company, companyLevelCollection.Level);

			companyLevelCollection.Load();

			Assert(companyLevelCollection.IsLoaded);
			AssertEquals(2, companyLevelCollection.Count);

			var sysLevelConfig = companyLevelCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);

			AssertNotNull(sysLevelConfig);
			AssertEquals(configPKs.sysLevelPK, sysLevelConfig.PK);

			var exRateConfig = companyLevelCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);

			AssertNotNull(exRateConfig);
			AssertEquals(configPKs.cmpLevelPK, exRateConfig.PK);

			TestAddNew(companyLevelCollection, ZGuid.Empty);
		}

		public void TestDebtorGroupLevelCollectionReturnsCorrectSets()
		{
			var configPKs = PrepTestData(LedgerTypes.AccountsReceivable);

			var debtorGroupCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, configPKs.groupPK);
			AssertEquals(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroupCollection.Level);

			debtorGroupCollection.Load();

			Assert(debtorGroupCollection.IsLoaded);
			AssertEquals(3, debtorGroupCollection.Count);

			var sysLevelConfig = debtorGroupCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);

			AssertNotNull(sysLevelConfig);
			AssertEquals(configPKs.sysLevelPK, sysLevelConfig.PK);

			var cmpLevelConfig = debtorGroupCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);

			AssertNotNull(cmpLevelConfig);
			AssertEquals(configPKs.cmpLevelPK, cmpLevelConfig.PK);

			var debtorGroupConfig = debtorGroupCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(c => c.Level == AccExRateConfigurationLevelEnum.DebtorGroup);

			AssertNotNull(debtorGroupConfig);
			AssertEquals(configPKs.grpLevelPK, debtorGroupConfig.PK);

			TestAddNew(debtorGroupCollection, configPKs.groupPK);
		}

		public void TestCreditorGroupLevelCollectionReturnsCorrectSets()
		{
			var configPKs = PrepTestData(LedgerTypes.AccountsPayable);

			var creditorGroupCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, configPKs.groupPK);
			AssertEquals(AccExRateConfigurationLevelEnum.CreditorGroup, creditorGroupCollection.Level);

			creditorGroupCollection.Load();

			Assert(creditorGroupCollection.IsLoaded);
			AssertEquals(3, creditorGroupCollection.Count);

			var sysLevelConfig = creditorGroupCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);

			AssertNotNull(sysLevelConfig);
			AssertEquals(configPKs.sysLevelPK, sysLevelConfig.PK);

			var cmpLevelConfig = creditorGroupCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);

			AssertNotNull(cmpLevelConfig);
			AssertEquals(configPKs.cmpLevelPK, cmpLevelConfig.PK);

			var creditorGroupConfig = creditorGroupCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(c => c.Level == AccExRateConfigurationLevelEnum.CreditorGroup);

			AssertNotNull(creditorGroupConfig);
			AssertEquals(configPKs.grpLevelPK, creditorGroupConfig.PK);

			TestAddNew(creditorGroupCollection, configPKs.groupPK);
		}

		public void TestDebtorLevelCollectionReturnsCorrectSets()
		{
			var configPKs = PrepTestData(LedgerTypes.AccountsReceivable);

			var debtorCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, configPKs.groupPK, configPKs.orgPK);
			AssertEquals(AccExRateConfigurationLevelEnum.Debtor, debtorCollection.Level);

			debtorCollection.Load();

			Assert(debtorCollection.IsLoaded);
			AssertEquals(4, debtorCollection.Count);

			var sysLevelConfig = debtorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);

			AssertNotNull(sysLevelConfig);
			AssertEquals(configPKs.sysLevelPK, sysLevelConfig.PK);

			var cmpLevelConfig = debtorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);

			AssertNotNull(cmpLevelConfig);
			AssertEquals(configPKs.cmpLevelPK, cmpLevelConfig.PK);

			var debtorGroupConfig = debtorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(c => c.Level == AccExRateConfigurationLevelEnum.DebtorGroup);

			AssertNotNull(debtorGroupConfig);
			AssertEquals(configPKs.grpLevelPK, debtorGroupConfig.PK);

			var debtorConfig = debtorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(c => c.Level == AccExRateConfigurationLevelEnum.Debtor);

			AssertNotNull(debtorConfig);
			AssertEquals(configPKs.orgLevelPK, debtorConfig.PK);

			TestAddNew(debtorCollection, configPKs.orgPK);
		}

		public void TestCreditorLevelCollectionReturnsCorrectSets()
		{
			var configPKs = PrepTestData(LedgerTypes.AccountsPayable);

			var creditorCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, configPKs.groupPK, configPKs.orgPK);
			AssertEquals(AccExRateConfigurationLevelEnum.Creditor, creditorCollection.Level);

			creditorCollection.Load();

			Assert(creditorCollection.IsLoaded);
			AssertEquals(4, creditorCollection.Count);

			var sysLevelConfig = creditorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.System);

			AssertNotNull(sysLevelConfig);
			AssertEquals(configPKs.sysLevelPK, sysLevelConfig.PK);

			var cmpLevelConfig = creditorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);

			AssertNotNull(cmpLevelConfig);
			AssertEquals(configPKs.cmpLevelPK, cmpLevelConfig.PK);

			var creditorGroupConfig = creditorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(c => c.Level == AccExRateConfigurationLevelEnum.CreditorGroup);

			AssertNotNull(creditorGroupConfig);
			AssertEquals(configPKs.grpLevelPK, creditorGroupConfig.PK);

			var creditorConfig = creditorCollection.Cast<AccExchangeRateConfiguration>().FirstOrDefault(c => c.Level == AccExRateConfigurationLevelEnum.Creditor);

			AssertNotNull(creditorConfig);
			AssertEquals(configPKs.orgLevelPK, creditorConfig.PK);

			TestAddNew(creditorCollection, configPKs.orgPK);
		}

		public void TestSecurityRightsAreAppliedCorrectly()
		{
			var systemLevelCollection = new AccExchangeRateConfigurationCollection(Factory);
			var companyLevelCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK);

			Assert(!systemLevelCollection.ReadOnly);
			Assert(!companyLevelCollection.ReadOnly);

			Env.Security.JobExRateSysConfigs.IsAllowed = false;

			Assert(systemLevelCollection.ReadOnly);
			Assert(!companyLevelCollection.ReadOnly);

			Env.Security.JobExRateSysConfigs.IsAllowed = true;
			Env.Security.JobExRateSysConfigsModify.IsAllowed = false;

			Assert(systemLevelCollection.ReadOnly);
			Assert(!companyLevelCollection.ReadOnly);

			Env.Security.CompaniesModifyJobBillingExchangeRates.IsAllowed = false;

			Assert(systemLevelCollection.ReadOnly);
			Assert(companyLevelCollection.ReadOnly);

			var configPKs = PrepTestData(LedgerTypes.AccountsPayable);

			var creditorCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, configPKs.groupPK, configPKs.orgPK);
			AssertEquals(AccExRateConfigurationLevelEnum.Creditor, creditorCollection.Level);

			Assert(!creditorCollection.ReadOnly);

			Env.Security.OrgPayablesModifyExchangeRates.IsAllowed = false;

			Assert(creditorCollection.ReadOnly);

			configPKs = PrepTestData(LedgerTypes.AccountsPayable);

			var debtorCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, configPKs.groupPK, configPKs.orgPK);
			AssertEquals(AccExRateConfigurationLevelEnum.Debtor, debtorCollection.Level);

			Assert(!debtorCollection.ReadOnly);

			Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed = false;

			Assert(debtorCollection.ReadOnly);
		}

		public void TestAdditionalQueryConstructorSetNoneLevel()
		{
			var collection = new AccExchangeRateConfigurationCollection(Factory, new ZQuery());
			AssertEquals(AccExRateConfigurationLevelEnum.None, collection.Level);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccExchangeRateConfigurationCollection(Factory, ZGuid.NewZGuid());
		}

		(ZGuid sysLevelPK, ZGuid cmpLevelPK, ZGuid grpLevelPK, ZGuid orgLevelPK, ZGuid groupPK, ZGuid orgPK) PrepTestData(string ledger)
		{
			var sysLevelPK = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty);
			var cmpLevelPK = CreateExRateConfig(AccExRateConfigurationLevelEnum.Company, ZGuid.Empty);
			var grpLevelPK = ZGuid.Empty;
			var orgLevelPK = ZGuid.Empty;
			var groupPK = ZGuid.Empty;

			var org = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);

			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AutoAccExchangeRateConfigurationView.Schema.TableName);

			if (ledger == LedgerTypes.AccountsReceivable)
			{
				var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
				org.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
				groupPK = debtorGroup.PK;

				Factory.Save();

				grpLevelPK = CreateExRateConfig(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroup.PK);
				orgLevelPK = CreateExRateConfig(AccExRateConfigurationLevelEnum.Debtor, org.PK);
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
				org.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
				groupPK = creditorGroup.PK;

				Factory.Save();

				grpLevelPK = CreateExRateConfig(AccExRateConfigurationLevelEnum.CreditorGroup, creditorGroup.PK);
				orgLevelPK = CreateExRateConfig(AccExRateConfigurationLevelEnum.Creditor, org.PK);
			}
			else
			{
				throw new ArgumentException($"Incorrect Ledger Type {ledger}");
			}

			Factory.Save();

			return (sysLevelPK, cmpLevelPK, grpLevelPK, orgLevelPK, groupPK, org.PK);
		}

		ZGuid CreateExRateConfig(AccExRateConfigurationLevelEnum level, ZGuid parentPK, string preference = "TDR", int offset = 0, bool prompt = false, string invoiceCurrencyType = "")
		{
			var exRateConfig = Factory.New<AccExchangeRateConfiguration>();

			if (level != AccExRateConfigurationLevelEnum.System)
			{
				exRateConfig.JCE_GC = Env.CurrentCompanyPK;
				exRateConfig.JCE_Ledger = level.ToLedgerType();
				exRateConfig.JCE_ParentTableCode = level.ToTablePrefix();
				exRateConfig.JCE_ParentID = parentPK;
				exRateConfig.JCE_InvoiceCurrencyType = invoiceCurrencyType;
			}

			exRateConfig.JCE_ServiceDirection = "IMP";
			exRateConfig.JCE_TransportMode = "SEA";
			exRateConfig.JCE_Preference = preference;
			exRateConfig.JCE_Offset = offset;
			exRateConfig.JCE_Prompt = prompt;

			return exRateConfig.PK;
		}

		void TestAddNew(AccExchangeRateConfigurationCollection col, ZGuid parentPk)
		{
			var bizo = col.AddNew();
			AssertEquals("JCE_GC", col.Level == AccExRateConfigurationLevelEnum.System ? ZGuid.Empty : Env.CurrentCompanyPK, bizo.JCE_GC);
			AssertEquals("Levle", col.Level, bizo.Level);
			AssertEquals("JCE_ParentID", parentPk, bizo.JCE_ParentID);
		}

		public void TestGetRecord()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			var orgDebtor = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			orgDebtor.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var orgCreditor = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			orgCreditor.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			Factory.Save();

			var rateConsumer = new Mock<IAccExchangeRateConfigurationRateConsumer>(MockBehavior.Strict).Object;

			var record1 = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA");
			var record2 = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "SHP", "IMP", "ROA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local);
			var record3 = CreateExRateConfig(AccExRateConfigurationLevelEnum.Company, ZGuid.Empty, string.Empty, "ALL", "ALL", "SEA");
			var record4 = CreateExRateConfig(AccExRateConfigurationLevelEnum.Company, ZGuid.Empty, string.Empty, "SHP", "EXP", "SEA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign);

			var record5 = CreateExRateConfig(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroup.PK, "AR", "SHP", "IMP", "AIR");
			var record6 = CreateExRateConfig(AccExRateConfigurationLevelEnum.Debtor, orgDebtor.PK, "AR", "SHP", "IMP", "SEA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local);

			var record7 = CreateExRateConfig(AccExRateConfigurationLevelEnum.CreditorGroup, creditorGroup.PK, "AP", "SHP", "EXP", "AIR", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign);
			var record8 = CreateExRateConfig(AccExRateConfigurationLevelEnum.Creditor, orgCreditor.PK, "AP", "SHP", "IMP", "SEA");

			Factory.Save();

			var sysCollection = new AccExchangeRateConfigurationCollection(Factory);
			sysCollection.Load();

			var gcCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK);
			gcCollection.Load();

			var dgCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, "AR", debtorGroup.PK);
			dgCollection.Load();

			var cgCollection = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, "AP", creditorGroup.PK);
			cgCollection.Load();

			var debtorCol = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, "AR", debtorGroup.PK, orgDebtor.PK);
			debtorCol.Load();

			var creditorCol = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, "AP", creditorGroup.PK, orgCreditor.PK);
			creditorCol.Load();

			AssertEquals(record5, dgCollection.GetRecord("AR", "SHP", "IMP", "AIR", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable, ZString.Empty, rateConsumer).PK);
			AssertEquals(record4, dgCollection.GetRecord("AR", "SHP", "EXP", "SEA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ZString.Empty, rateConsumer).PK);

			AssertEquals(record7, cgCollection.GetRecord("AP", "SHP", "EXP", "AIR", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ZString.Empty, rateConsumer).PK);
			AssertEquals(record3, cgCollection.GetRecord("AP", "SHP", "IMP", "SEA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable, ZString.Empty, rateConsumer).PK);

			AssertEquals(record6, debtorCol.GetRecord("AR", "SHP", "IMP", "SEA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ZString.Empty, rateConsumer).PK);
			AssertEquals(record5, debtorCol.GetRecord("AR", "SHP", "IMP", "AIR", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable, ZString.Empty, rateConsumer).PK);

			AssertEquals(record8, creditorCol.GetRecord("AP", "SHP", "IMP", "SEA", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable, ZString.Empty, rateConsumer).PK);
			AssertEquals(record7, creditorCol.GetRecord("AP", "SHP", "EXP", "AIR", Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ZString.Empty, rateConsumer).PK);
		}

		public void TestGetRecord_CurrencyHigherPriorityThanLedger()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var config_AllCur_Ledger = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, "AR", "ALL", "IMP", "SEA", currencyCode: string.Empty);
			var config_AllCur_JobType = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "SHP", "IMP", "SEA", currencyCode: string.Empty);
			var config_USD = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA", currencyCode: CurrencyCodes.UnitedStates);
			Factory.Save();

			var sysCollection = new AccExchangeRateConfigurationCollection(Factory);
			sysCollection.Load();

			var rateConsumer = new Mock<IAccExchangeRateConfigurationRateConsumer>(MockBehavior.Strict).Object;

			AssertEquals("Get config_AllCur because of empty currency condition.(JobType has higher priority than Ledger)",
				config_AllCur_JobType,
				sysCollection.GetRecord("AR", "SHP", "IMP", "SEA", string.Empty, string.Empty, rateConsumer).PK
			);
			AssertEquals("Get config_USD because of USD currency condition even its ledger is not specified.",
				config_USD,
				sysCollection.GetRecord("AR", "SHP", "IMP", "SEA", string.Empty, "USD", rateConsumer).PK
			);
		}

		public void TestGetRecordFallBackToDefaultWhenNoRecordExists()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);
			Factory.Save();

			var sysCollection = new AccExchangeRateConfigurationCollection(Factory);
			sysCollection.Load();
			var rateConsumer = new Mock<IAccExchangeRateConfigurationRateConsumer>(MockBehavior.Strict).Object;
			AssertEquals("Expect no records in table", 0, sysCollection.Count);
			var result = sysCollection.GetRecord("AR", "SHP", "IMP", "ROA", ZString.Empty, ZString.Empty, rateConsumer);
			AssertNotNull(result);
			AssertEquals("", result.Ledger);
			AssertEquals("ALL", result.JobType);
			AssertEquals("ALL", result.ServiceDirection);
			AssertEquals("ALL", result.TransportMode);
			AssertEquals("BUY", result.ExchangeRateTypeAsZString);
			AssertEquals("TDR", result.Preference);
		}

		public void TestGetRecord_WithinCurrencyConfigDateRange()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);
			var config_USD_PK = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA", currencyCode: CurrencyCodes.UnitedStates);
			Factory.Save();

			var sysCollection = new AccExchangeRateConfigurationCollection(Factory);
			sysCollection.Load();

			var config_USD = Factory.Load<AccExchangeRateConfiguration>(config_USD_PK);
			config_USD.JCE_Preference = "TST";

			var currencyConfig = config_USD.GetCurrencyConfig("USD", ZDate.Empty);
			currencyConfig.JCT_StartDate = new ZDate(2025, 2, 3);
			currencyConfig.JCT_ExpiryDate = new ZDate(2025, 2, 5);

			var rateConsumer = new Mock<IAccExchangeRateConfigurationRateConsumer>(MockBehavior.Strict).Object;

			var getExchangeRateDateMock = new Mock<IAccExchangeRateConfigurationsHelper>(MockBehavior.Strict);
			getExchangeRateDateMock.Setup(x => x.GetExchangeRateDate(rateConsumer, "TST")).Returns(new ZDate(2025, 2, 4));

			using (ObjectFactory.Substitute(getExchangeRateDateMock.Object))
			{
				AssertEquals("Should get config_USD since the date within the currencyConfig date range",
					config_USD_PK,
					sysCollection.GetRecord("AR", "SHP", "IMP", "SEA", string.Empty, "USD", rateConsumer).PK
				);
			}

			getExchangeRateDateMock.Verify(x => x.GetExchangeRateDate(It.IsAny<IAccExchangeRateConfigurationRateConsumer>(), It.IsAny<ZString>()), Times.Exactly(2));
		}

		public void TestGetRecord_OutsideCurrencyConfigDateRange()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);
			var config_USD_PK = CreateExRateConfig(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA", currencyCode: CurrencyCodes.UnitedStates);
			Factory.Save();

			var sysCollection = new AccExchangeRateConfigurationCollection(Factory);
			sysCollection.Load();

			var config_USD = Factory.Load<AccExchangeRateConfiguration>(config_USD_PK);
			config_USD.JCE_Preference = "TST";

			var currencyConfig = config_USD.GetCurrencyConfig("USD", ZDate.Empty);
			currencyConfig.JCT_StartDate = new ZDate(2025, 2, 3);
			currencyConfig.JCT_ExpiryDate = new ZDate(2025, 2, 5);

			var rateConsumer = new Mock<IAccExchangeRateConfigurationRateConsumer>(MockBehavior.Strict).Object;

			var getExchangeRateDateMock = new Mock<IAccExchangeRateConfigurationsHelper>(MockBehavior.Strict);
			getExchangeRateDateMock.Setup(x => x.GetExchangeRateDate(rateConsumer, "TST")).Returns(new ZDate(2025, 2, 10));
			getExchangeRateDateMock.Setup(x => x.GetExchangeRateDate(rateConsumer, "TDR")).Returns(new ZDate(2025, 2, 10));

			using (ObjectFactory.Substitute(getExchangeRateDateMock.Object))
			{
				var result = sysCollection.GetRecord("AR", "SHP", "IMP", "ROA", ZString.Empty, ZString.Empty, rateConsumer);
				AssertNotEquals("Should found the fall back default config when outside currency config date range", config_USD_PK, result.PK);
				AssertEquals("", result.Ledger);
				AssertEquals("ALL", result.JobType);
				AssertEquals("ALL", result.ServiceDirection);
				AssertEquals("ALL", result.TransportMode);
				AssertEquals("BUY", result.ExchangeRateTypeAsZString);
				AssertEquals("TDR", result.Preference);
			}

			getExchangeRateDateMock.Verify(x => x.GetExchangeRateDate(It.IsAny<IAccExchangeRateConfigurationRateConsumer>(), It.IsAny<ZString>()), Times.Exactly(2));
		}

		#region Test Remove Collection Element

		public void TestRemoveElement_SaveElementsBeforeDelete()
		{
			AssertRemoveElementCore(true);
		}

		public void TestRemoveElement_DoNotSaveElementsBeforeDelete()
		{
			AssertRemoveElementCore(false);
		}

		void AssertRemoveElementCore(bool shouldSaveCurrencyConfigsBeforeDelete)
		{
			var systemCollection = new AccExchangeRateConfigurationCollection(Factory);
			systemCollection.Load();
			AssertEquals("The collection has 2 default elements", 2, systemCollection.Count);

			var exRateConfig = TestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA");
			Factory.Save();

			systemCollection.Load();
			AssertEquals("The collection has one more element", 3, systemCollection.Count);
			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			var currencyConfig1 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(exRateConfig, CurrencyCodes.Australia);
			var currencyConfig2 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(exRateConfig, CurrencyCodes.NewZealand);

			exRateConfig.CurrencyConfigurations.Add(currencyConfig1);
			exRateConfig.CurrencyConfigurations.Add(currencyConfig2);

			if (shouldSaveCurrencyConfigsBeforeDelete)
			{
				Factory.Save();
			}

			AssertEquals("Should have currency elements", 2, exRateConfig.CurrencyConfigurations.Count);

			systemCollection.RemoveAndDelete(exRateConfig);
			Factory.Save();
			exRateConfig.CurrencyConfigurations.Load();
			AssertEquals("Should automatically remove all currency elements", 0, exRateConfig.CurrencyConfigurations.Count);

			systemCollection.Load();
			AssertEquals("The collection should remain 2 default elements", 2, systemCollection.Count);
		}

		#endregion

		ZGuid CreateExRateConfig(AccExRateConfigurationLevelEnum level, ZGuid parentPk, ZString ledger, ZString jobType, ZString serviceDirection, ZString transportMode, string invoiceCurrencyType = "", string currencyCode = "")
		{
			return TestObjectCreator.CreateAccExchangeRateConfiguration(level, parentPk, ledger, jobType, serviceDirection, transportMode, invoiceCurrencyType, new[] { currencyCode }).PK;
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
