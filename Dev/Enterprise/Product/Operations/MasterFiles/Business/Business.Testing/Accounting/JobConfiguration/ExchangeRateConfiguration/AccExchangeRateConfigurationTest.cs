using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.Testing.OrgCompanyDataTest;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccExchangeRateConfiguration))]
	sealed class AccExchangeRateConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExchangeRateConfigurationDeletionWhenFactoryDoesNotHavePermittedToDeleteJobExchangeRateConfigContext()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var factoryWithoutContext = new BusinessObjectFactory();
			Assert(!factoryWithoutContext.HasContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig));

			var companyInFactoryWithoutContext = factoryWithoutContext.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var exRateConfigInFactoryWithoutContext = CreateCompanyLevelExRateConfig(companyInFactoryWithoutContext);
			Assert("Company level exchange rate config is not in database", !exRateConfigInFactoryWithoutContext.IsInDatabase);

			ErrorReporter.Clear();
			exRateConfigInFactoryWithoutContext.Delete();
			Assert("Should allow deletion because exchange rate config is not in database", exRateConfigInFactoryWithoutContext.IsDeleted);
			AssertEquals("Should not report error because exchange rate config is not in database", string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("Should not report error because exchange rate config is not in database", string.Empty, ErrorReporter.LastMessageReported);

			exRateConfigInFactoryWithoutContext = CreateCompanyLevelExRateConfig(companyInFactoryWithoutContext);
			factoryWithoutContext.Save();
			Assert("Company level exchange rate config is in database", exRateConfigInFactoryWithoutContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				exRateConfigInFactoryWithoutContext.Delete();
				Assert("Should allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is off", exRateConfigInFactoryWithoutContext.IsDeleted);
				AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastMessageReported);
			}

			exRateConfigInFactoryWithoutContext = CreateCompanyLevelExRateConfig(companyInFactoryWithoutContext);
			factoryWithoutContext.Save();
			Assert("Company level exchange rate config is in database", exRateConfigInFactoryWithoutContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				try
				{
					exRateConfigInFactoryWithoutContext.Delete();
					Fail("Should not reach this line because deleting exchange rate config when PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on, must throw exception");
				}
				catch (CannotDeleteException ex)
				{
					Assert("Should not allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on", !exRateConfigInFactoryWithoutContext.IsDeleted);
					AssertEquals("Unable to delete Company level Job Billing Exchange Rate Configuration for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: AIR, InvoiceCurrencyType: , Preference: TDR. Please report this error to CargoWise Support.", ex.Message);
					AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastMessageReported);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				exRateConfigInFactoryWithoutContext.Delete();
				Assert("Should allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is off", exRateConfigInFactoryWithoutContext.IsDeleted);
				AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "DeletingJobBillingExchangeRateConfiguration", ErrorReporter.LastKeyReported);
				AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "Trying to delete Company level Job Billing Exchange Rate Configuration for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: AIR, InvoiceCurrencyType: , Preference: TDR.", ErrorReporter.LastMessageReported);
			}

			exRateConfigInFactoryWithoutContext = CreateCompanyLevelExRateConfig(companyInFactoryWithoutContext);
			factoryWithoutContext.Save();
			Assert("Company level exchange rate config is in database", exRateConfigInFactoryWithoutContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				try
				{
					exRateConfigInFactoryWithoutContext.Delete();
					Fail("Should not reach this line because deleting exchange rate config when PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on, must throw exception");
				}
				catch (CannotDeleteException ex)
				{
					Assert("Should not allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on", !exRateConfigInFactoryWithoutContext.IsDeleted);
					AssertEquals("Unable to delete Company level Job Billing Exchange Rate Configuration for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: AIR, InvoiceCurrencyType: , Preference: TDR. Please report this error to CargoWise Support.", ex.Message);
					AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "DeletingJobBillingExchangeRateConfiguration", ErrorReporter.LastKeyReported);
					AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "Trying to delete Company level Job Billing Exchange Rate Configuration for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: AIR, InvoiceCurrencyType: , Preference: TDR.", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestExchangeRateConfigurationDeletionWhenFactoryHasPermittedToDeleteJobExchangeRateConfigContext()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var factoryWithContext = new BusinessObjectFactory();
			factoryWithContext.SetContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig);
			Assert(factoryWithContext.HasContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig));

			var companyInFactoryWithContext = factoryWithContext.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var exRateConfigInFactoryWithContext = CreateCompanyLevelExRateConfig(companyInFactoryWithContext);
			factoryWithContext.Save();

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				exRateConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteJobExchangeRateConfig context", exRateConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}

			exRateConfigInFactoryWithContext = CreateCompanyLevelExRateConfig(companyInFactoryWithContext);
			factoryWithContext.Save();
			Assert("Company level exchange rate config is in database", exRateConfigInFactoryWithContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				exRateConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteJobExchangeRateConfig context", exRateConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}

			exRateConfigInFactoryWithContext = CreateCompanyLevelExRateConfig(companyInFactoryWithContext);
			factoryWithContext.Save();
			Assert("Company level exchange rate config is in database", exRateConfigInFactoryWithContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				exRateConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteJobExchangeRateConfig context", exRateConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}

			exRateConfigInFactoryWithContext = CreateCompanyLevelExRateConfig(companyInFactoryWithContext);
			factoryWithContext.Save();
			Assert("Company level exchange rate config is in database", exRateConfigInFactoryWithContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				exRateConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteJobExchangeRateConfig context", exRateConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteJobExchangeRateConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		AccExchangeRateConfiguration CreateCompanyLevelExRateConfig(GlbCompany company)
		{
			company.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			var exRateConfig = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level Ex Rate Config", exRateConfig);
			return exRateConfig;
		}

		public void TestAutoLoggingEnabled() => Assert("Auto Log Creation must be enabled.", Factory.New<AccExchangeRateConfiguration>().IsAutoAdminBusinessObjectLoggerEnabled);

		public void TestLogReferences()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "QWERTY";
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = org.PK;
			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			creditorGroup.OG_Code = "CCC";
			orgCompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			debtorGroup.OJ_Code = "DDD";
			orgCompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var systemLevelExchangeRateConfigs = new AccExchangeRateConfigurationCollection(Factory);
			systemLevelExchangeRateConfigs.Load();
			systemLevelExchangeRateConfigs.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, invoiceCurrencyType: Constants.InvoicePostingExchangeRateCurrencyType.Code.Local);
			company.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, invoiceCurrencyType: Constants.InvoicePostingExchangeRateCurrencyType.Code.Local);
			creditorGroup.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, invoiceCurrencyType: Constants.InvoicePostingExchangeRateCurrencyType.Code.Local);
			debtorGroup.AccExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Domestic, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, invoiceCurrencyType: Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign);
			orgCompanyData.AccAPExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Air, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate, invoiceCurrencyType: Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign);
			orgCompanyData.AccARExchangeRateConfigurations.SetExRate(LedgerTypes.AccountsReceivable, JobInvoicingConsumerTypes.Shipment.Code, Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Sea, Constants.ExchangeRateTypes.Code.BuyRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			Factory.Save();

			var systemLevelExRateConfig = systemLevelExchangeRateConfigs.Cast<AccExchangeRateConfiguration>().FirstOrDefault();
			AssertNotNull("System level Ex Rate Config", systemLevelExRateConfig);
			var companyLevelExRateConfig = company.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Company);
			AssertNotNull("Company level Ex Rate Config", companyLevelExRateConfig);
			var creditorGroupLevelExRateConfig = creditorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup);
			AssertNotNull("Creditor Group level Ex Rate Config", creditorGroupLevelExRateConfig);
			var debtorGroupLevelExRateConfig = debtorGroup.AccExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup);
			AssertNotNull("Debtor group level Ex Rate Config", debtorGroupLevelExRateConfig);
			var orgLevelAPExRateConfig = orgCompanyData.AccAPExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Creditor);
			AssertNotNull("Organisation level AP Ex Rate Config", orgLevelAPExRateConfig);
			var orgLevelARExRateConfig = orgCompanyData.AccARExchangeRateConfigurations.Cast<AccExchangeRateConfiguration>().FirstOrDefault(x => x.Level == AccExRateConfigurationLevelEnum.Debtor);
			AssertNotNull("Organisation level AR Ex Rate Config", orgLevelARExRateConfig);

			AssertAddLogReference(systemLevelExRateConfig, "Added System Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: ALL, TransportMode: ALL, InvoiceCurrencyType: LOC, Preference: TDR.");
			AssertAddLogReference(companyLevelExRateConfig, "Added Company EDI Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: AIR, InvoiceCurrencyType: LOC, Preference: TDR.");
			AssertAddLogReference(creditorGroupLevelExRateConfig, "Added Creditor Group CCC Ex. Rate config for Ledger: AP, JobType: SHP, ServiceDirection: EXP, TransportMode: AIR, InvoiceCurrencyType: LOC, Preference: TDR.");
			AssertAddLogReference(debtorGroupLevelExRateConfig, "Added Debtor Group DDD Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: DOM, TransportMode: AIR, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertAddLogReference(orgLevelAPExRateConfig, "Added Creditor QWERTY Ex. Rate config for Ledger: AP, JobType: SHP, ServiceDirection: OTH, TransportMode: AIR, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertAddLogReference(orgLevelARExRateConfig, "Added Debtor QWERTY Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: OTH, TransportMode: SEA, InvoiceCurrencyType: , Preference: TDR.");

			foreach (var exRateConfig in new[] { systemLevelExRateConfig, companyLevelExRateConfig, creditorGroupLevelExRateConfig, debtorGroupLevelExRateConfig, orgLevelAPExRateConfig, orgLevelARExRateConfig })
			{
				exRateConfig.JCE_TransportMode = Constants.TransportModes.Sea;
				exRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.C01Rate;
				exRateConfig.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
			}
			Factory.Save();

			AssertEditLogReference(systemLevelExRateConfig, "Changed System Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: ALL, TransportMode: ALL, InvoiceCurrencyType: LOC, Preference: TDR changed to Ledger: AR, JobType: SHP, ServiceDirection: ALL, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertEditLogReference(companyLevelExRateConfig, "Changed Company EDI Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: AIR, InvoiceCurrencyType: LOC, Preference: TDR changed to Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertEditLogReference(creditorGroupLevelExRateConfig, "Changed Creditor Group CCC Ex. Rate config for Ledger: AP, JobType: SHP, ServiceDirection: EXP, TransportMode: AIR, InvoiceCurrencyType: LOC, Preference: TDR changed to Ledger: AP, JobType: SHP, ServiceDirection: EXP, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertEditLogReference(debtorGroupLevelExRateConfig, "Changed Debtor Group DDD Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: DOM, TransportMode: AIR, InvoiceCurrencyType: FOR, Preference: TDR changed to Ledger: AR, JobType: SHP, ServiceDirection: DOM, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertEditLogReference(orgLevelAPExRateConfig, "Changed Creditor QWERTY Ex. Rate config for Ledger: AP, JobType: SHP, ServiceDirection: OTH, TransportMode: AIR, InvoiceCurrencyType: FOR, Preference: TDR changed to Ledger: AP, JobType: SHP, ServiceDirection: OTH, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertEditLogReference(orgLevelARExRateConfig, "Changed Debtor QWERTY Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: OTH, TransportMode: SEA, InvoiceCurrencyType: , Preference: TDR changed to Ledger: AR, JobType: SHP, ServiceDirection: OTH, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");

			foreach (var exRateConfig in new[] { systemLevelExRateConfig, companyLevelExRateConfig, creditorGroupLevelExRateConfig, debtorGroupLevelExRateConfig, orgLevelAPExRateConfig, orgLevelARExRateConfig })
			{
				exRateConfig.Delete();
			}
			Factory.Save();

			AssertDeleteLogReference(systemLevelExRateConfig.PK, "Deleted System Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: ALL, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertDeleteLogReference(companyLevelExRateConfig.PK, "Deleted Company EDI Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: IMP, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertDeleteLogReference(creditorGroupLevelExRateConfig.PK, "Deleted Creditor Group CCC Ex. Rate config for Ledger: AP, JobType: SHP, ServiceDirection: EXP, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertDeleteLogReference(debtorGroupLevelExRateConfig.PK, "Deleted Debtor Group DDD Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: DOM, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertDeleteLogReference(orgLevelAPExRateConfig.PK, "Deleted Creditor QWERTY Ex. Rate config for Ledger: AP, JobType: SHP, ServiceDirection: OTH, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");
			AssertDeleteLogReference(orgLevelARExRateConfig.PK, "Deleted Debtor QWERTY Ex. Rate config for Ledger: AR, JobType: SHP, ServiceDirection: OTH, TransportMode: SEA, InvoiceCurrencyType: FOR, Preference: TDR.");

			void AssertAddLogReference(AccExchangeRateConfiguration exRateConfig, string expectedAddLogReference) => AssertLogReference(exRateConfig, AutoEvents.AddedARecordToTheSystem, expectedAddLogReference);

			void AssertEditLogReference(AccExchangeRateConfiguration exRateConfig, string expectedEditLogReference) => AssertLogReference(exRateConfig, AutoEvents.EditedARecord, expectedEditLogReference);

			void AssertLogReference(AccExchangeRateConfiguration exRateConfig, Event eventToAssert, string expectedLogReference)
			{
				var log = ((IAutoAdminLogTarget)exRateConfig).Logs.MostRecentLogByEventTime(eventToAssert);
				AssertNotNull(log);
				AssertEquals(expectedLogReference, log.SL_Reference);
			}

			void AssertDeleteLogReference(ZGuid exRateConfigPK, string expectedDeleteLogReference)
			{
				var deleteLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, exRateConfigPK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeletedARecordInTheSystem.Code));
				AssertNotNull(deleteLog);
				AssertEquals(expectedDeleteLogReference, deleteLog.SL_Reference);
			}
		}

		public void TestSetDefaultValues()
		{
			var exRateConfig = Factory.New<AccExchangeRateConfiguration>();
			var currencyConfig = exRateConfig.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>();

			AssertEquals("Configuration Type", "ERT", exRateConfig.JCE_ConfigType);
			Assert("Company", exRateConfig.JCE_GC.IsEmpty);
			Assert("Ledger", exRateConfig.JCE_Ledger.IsEmpty);
			Assert("Parent Table Code", exRateConfig.JCE_ParentTableCode.IsEmpty);
			Assert("Parent ID", exRateConfig.JCE_ParentID.IsEmpty);
			AssertEquals("Job Type", "ALL", exRateConfig.JCE_JobType);
			AssertEquals("Transport Mode", "ALL", exRateConfig.JCE_TransportMode);
			AssertEquals("Service Direction", "ALL", exRateConfig.JCE_ServiceDirection);
			AssertEquals("Invoice Currency Type", "", exRateConfig.JCE_InvoiceCurrencyType);
			AssertEquals("Currency Type", "ALL", exRateConfig.JCE_Calc_CurrencyType);
			AssertEquals("Have one currency config", 1, currencyConfig.Count());
			AssertEquals("Empty Currency Code", "", currencyConfig.First().JCT_Code);
			AssertEquals("Exchange Rate Type", "BUY", currencyConfig.First().JCT_ExRateType);
		}

		public void TestCannotDeleteSystemDefault()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var sysDefExRateConfig = Factory.New<AccExchangeRateConfiguration>();

			Assert("Can delete if not in DB", sysDefExRateConfig.CanDelete);
			Assert("Can edit properties if not in DB", !sysDefExRateConfig.JCE_JobTypeInfo.ReadOnly);
			Assert("Can edit properties if not in DB", !sysDefExRateConfig.JCE_ServiceDirectionInfo.ReadOnly);
			Assert("Can edit properties if not in DB", !sysDefExRateConfig.JCE_TransportModeInfo.ReadOnly);

			Factory.Save();

			Assert("Cannot delete if in DB", !sysDefExRateConfig.CanDelete);
			Assert("Cannot edit properties if in DB", sysDefExRateConfig.JCE_JobTypeInfo.ReadOnly);
			Assert("Cannot edit properties if in DB", sysDefExRateConfig.JCE_ServiceDirectionInfo.ReadOnly);
			Assert("Cannot edit properties if in DB", sysDefExRateConfig.JCE_TransportModeInfo.ReadOnly);
		}

		public void TestJCE_LedgerReadOnly()
		{
			Func<string, IDictionary<string, string>> getReadOnlyRefs = s => typeof(AccExchangeRateConfiguration).GetProperty(s).GetAttribute<ReadOnlyMemberAttribute>().ReferencedMembers;

			Assert(getReadOnlyRefs(nameof(AccExchangeRateConfiguration.JCE_Ledger)).ContainsKey("JCE_Ledger_ReadOnly"));

			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();

			exRateConfig.JCE_GC = ZGuid.Empty;
			Assert("No parent table", exRateConfig.JCE_ParentTableCode.IsEmpty);
			Assert("No parent", exRateConfig.JCE_ParentID.IsEmpty);
			Assert("Ledger is editable", !exRateConfig.JCE_LedgerInfo.ReadOnly);

			exRateConfig.JCE_GC = Env.CurrentCompanyPK;
			Assert("No parent table", exRateConfig.JCE_ParentTableCode.IsEmpty);
			Assert("No parent", exRateConfig.JCE_ParentID.IsEmpty);
			Assert("Ledger is editable", !exRateConfig.JCE_LedgerInfo.ReadOnly);

			exRateConfig.JCE_ParentID = ZGuid.NewZGuid();
			foreach (var prefix in new[] { OrgDebtorGroupSchema.Constants.Prefix, OrgCreditorGroupSchema.Constants.Prefix, OrgHeaderSchema.Constants.Prefix })
			{
				exRateConfig.JCE_ParentTableCode = prefix;
				Assert("Ledger is Read Only", exRateConfig.JCE_LedgerInfo.ReadOnly);
			}
		}

		public void TestJCE_InvoiceCurrencyTypeReadOnly()
		{
			Action<Action<AccExchangeRateConfiguration>, bool, string> validate = (setup, isReadOnly, message) =>
			{
				var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();

				// default setup to be editable
				exRateConfig.JCE_GC = Env.CurrentCompanyPK;
				exRateConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;

				setup(exRateConfig);

				Assert(message, exRateConfig.JCE_InvoiceCurrencyTypeInfo.ReadOnly == isReadOnly);
			};

			validate(cfg => { }, false, "Invoice Currency Type is editable");
			validate(cfg => cfg.JCE_GC = ZGuid.Empty, true, "Invoice Currency Type is read only for system level");
			validate(cfg => cfg.JCE_Ledger = LedgerTypes.AccountsPayable, true, "Invoice Currency Type is read only for AP ledger");
			validate(cfg => cfg.JCE_Ledger = string.Empty, true, "Invoice Currency Type requires AR ledger for editing");
		}

		public void TestJCE_InvoiceCurrencyType_ResetField()
		{
			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();

			AssertEquals(LedgerTypes.AccountsReceivable, exRateConfig.JCE_Ledger);
			Assert(!exRateConfig.JCE_InvoiceCurrencyTypeInfo.ReadOnly);
			exRateConfig.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;

			exRateConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			Assert(exRateConfig.JCE_InvoiceCurrencyTypeInfo.ReadOnly);
			AssertEquals("Invoice Currency Type field value is reset when ledger is changed to something different from AR", string.Empty, exRateConfig.JCE_InvoiceCurrencyType);

			exRateConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			exRateConfig.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
			exRateConfig.JCE_Ledger = string.Empty;
			AssertEquals(string.Empty, exRateConfig.JCE_InvoiceCurrencyType);
		}

		public void TestJCE_PromptReadOnly()
		{
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();

			var testCases = new List<Tuple<Func<AccExchangeRateConfiguration, bool>, AccExchangeRateConfigurationCollection>>
			{
				{ IsPromptReadOnlyForSystemAndComapnyLevel, GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ZGuid.Empty) },
				{ IsPromptReadOnlyForSystemAndComapnyLevel, GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.Company, ZGuid.Empty, ZGuid.Empty) },
				{ (config) => true, GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroup.PK, ZGuid.Empty) },
				{ (config) => true, GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.CreditorGroup, ZGuid.Empty, creditorGroup.PK) },
				{ (config) => true, GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.Debtor, debtorGroup.PK, ZGuid.Empty) },
				{ (config) => true, GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.Creditor, ZGuid.Empty, creditorGroup.PK) },
			};

			foreach (var testCase in testCases)
			{
				var exRateConfig = testCase.Item2.AddNew();
				AssertEquals(testCase.Item2.Level, exRateConfig.Level);

				foreach (CodeDescriptionPair jobType in exRateConfig.Lookups.JobTypeList)
				{
					exRateConfig.JCE_JobType = jobType.Code;
					foreach (CodeDescriptionPair preference in exRateConfig.Lookups.PreferenceList)
					{
						exRateConfig.JCE_Preference = preference.Code;

						exRateConfig.JCE_Ledger = ZString.Empty;
						AssertEquals(testCase.Item1(exRateConfig), exRateConfig.JCE_PromptInfo.ReadOnly);

						exRateConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
						AssertEquals(true, exRateConfig.JCE_PromptInfo.ReadOnly);

						exRateConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
						AssertEquals(true, exRateConfig.JCE_PromptInfo.ReadOnly);
					}
				}

				exRateConfig.Delete();
			}

			bool IsPromptReadOnlyForSystemAndComapnyLevel(AccExchangeRateConfiguration exRateConfig)
			{
				var isPromptEnabled = exRateConfig.JCE_JobType == JobInvoicingConsumerTypes.ShipmentCode
					&& (exRateConfig.JCE_Preference == JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate
					|| exRateConfig.JCE_Preference == JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate
					|| exRateConfig.JCE_Preference == JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate
					|| exRateConfig.JCE_Preference == JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);

				return !isPromptEnabled;
			}
		}

		public void TestClearPromptOptionIfNecessary()
		{
			var collection = GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ZGuid.Empty);
			var config1 = collection.AddNew();
			var config2 = collection.AddNew();
			var config3 = collection.AddNew();
			var config4 = collection.AddNew();

			config1.JCE_JobType = config2.JCE_JobType = config3.JCE_JobType = config4.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			config1.JCE_Prompt = config2.JCE_Prompt = config3.JCE_Prompt = config4.JCE_Prompt = true;
			config1.JCE_Preference = JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate;
			config2.JCE_Preference = JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate;
			config3.JCE_Preference = JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate;
			config4.JCE_Preference = JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate;

			config1.JCE_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			AssertEquals(false, config1.JCE_Prompt);

			config2.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(false, config2.JCE_Prompt);

			config3.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;
			AssertEquals(false, config3.JCE_Prompt);

			config4.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate;
			AssertEquals(true, config4.JCE_Prompt);
		}

		public void TestLookupAttributes()
		{
			Func<string, string> getLookupRef = s => typeof(AccExchangeRateConfiguration).GetProperty(s).GetAttribute<ListAttribute>().ListDataSourceMember;

			AssertEquals("Lookups.LedgerList", getLookupRef(nameof(AccExchangeRateConfiguration.JCE_Ledger)));
			AssertEquals("Lookups.JobTypeList", getLookupRef(nameof(AccExchangeRateConfiguration.JCE_JobType)));
			AssertEquals("Lookups.DirectionList", getLookupRef(nameof(AccExchangeRateConfiguration.JCE_ServiceDirection)));
			AssertEquals("Lookups.TransportModeList", getLookupRef(nameof(AccExchangeRateConfiguration.JCE_TransportMode)));
			AssertEquals("Lookups.PreferenceList", getLookupRef(nameof(AccExchangeRateConfiguration.JCE_Preference)));
			AssertEquals("Lookups.InvoiceCurrencyTypeList", getLookupRef(nameof(AccExchangeRateConfiguration.JCE_InvoiceCurrencyType)));
		}

		public void TestIsDuplicateOf()
		{
			var fakeCompanyGuid = ZGuid.NewZGuid();

			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();
			var exRateConfigDup = (AccExchangeRateConfiguration)GetNewBusinessObject();

			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_GC = v, b => b.JCE_GC, exRateConfig, exRateConfigDup, ZGuid.NewZGuid());
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_ParentTableCode = v, b => b.JCE_ParentTableCode, exRateConfig, exRateConfigDup, new ZString("OH"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_ParentID = v, b => b.JCE_ParentID, exRateConfig, exRateConfigDup, ZGuid.NewZGuid());
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_Ledger = v, b => b.JCE_Ledger, exRateConfig, exRateConfigDup, new ZString("UA"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_JobType = v, b => b.JCE_JobType, exRateConfig, exRateConfigDup, new ZString("JOB"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_ServiceDirection = v, b => b.JCE_ServiceDirection, exRateConfig, exRateConfigDup, new ZString("DOM"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_TransportMode = v, b => b.JCE_TransportMode, exRateConfig, exRateConfigDup, new ZString("TRN"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_InvoiceCurrencyType = v, b => b.JCE_InvoiceCurrencyType, exRateConfig, exRateConfigDup, new ZString("FOR"));
			AssertPropertyIsConsideredInCheckForDuplication((b, v) => b.JCE_Calc_CurrencyType = v, b => b.JCE_Calc_CurrencyType, exRateConfig, exRateConfigDup, new ZString("CUR"));
		}

		public void TestIsDuplicateOf_ShouldOnlyCheckForCurrencyTypeALL()
		{
			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();
			var exRateConfigDup = (AccExchangeRateConfiguration)GetNewBusinessObject();

			AssertEquals("PreCondition", AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL, exRateConfig.JCE_Calc_CurrencyType);
			AssertEquals(true, exRateConfig.IsDuplicateOf(exRateConfigDup));

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			exRateConfigDup.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			AssertEquals(false, exRateConfig.IsDuplicateOf(exRateConfigDup));
		}

		public void TestIsSameGroupConfigForCurrency()
		{
			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();
			var exRateConfigDup = (AccExchangeRateConfiguration)GetNewBusinessObject();
			AssertEquals("PreCondition", true, exRateConfig.IsDuplicateOf(exRateConfigDup));

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			exRateConfigDup.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			AssertEquals("PreCondition", true, exRateConfig.IsTheSameConfigWithCURCurrencyType(exRateConfigDup));

			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_GCInfo, exRateConfig, exRateConfigDup, new ZGuid("2B91C765-A425-4E83-83B6-B263F86E350E"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_ParentTableCodeInfo, exRateConfig, exRateConfigDup, new ZString("OH"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_ParentIDInfo, exRateConfig, exRateConfigDup, new ZGuid("3D37737A-64B8-41A4-85CF-A7FA8E2E688A"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_LedgerInfo, exRateConfig, exRateConfigDup, new ZString("UA"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_JobTypeInfo, exRateConfig, exRateConfigDup, new ZString("JOB"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_ServiceDirectionInfo, exRateConfig, exRateConfigDup, new ZString("DOM"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_TransportModeInfo, exRateConfig, exRateConfigDup, new ZString("TRN"));
			AssertIsSameGroupConfigForCurrency(exRateConfig.JCE_InvoiceCurrencyTypeInfo, exRateConfig, exRateConfigDup, new ZString("FOR"));

			void AssertIsSameGroupConfigForCurrency(ZPropertyInfo propInfo, AccExchangeRateConfiguration origBizo, AccExchangeRateConfiguration dupBizo, IZType newValue)
			{
				var originalValue = propInfo.Value;

				propInfo.Value = newValue;
				Assert(!origBizo.IsTheSameConfigWithCURCurrencyType(dupBizo));

				propInfo.Value = originalValue;
				Assert(origBizo.IsTheSameConfigWithCURCurrencyType(dupBizo));
			}
		}

		public void TestCanNotDeleteWhenExistDifferentLevelParentCollections()
		{
			var config = (AccExchangeRateConfiguration)GetNewBusinessObject();

			config.JCE_GC = Guid.Empty;
			AssertEquals(AccExRateConfigurationLevelEnum.System, config.Level);

			var collection1 = new AccExchangeRateConfigurationCollection(Factory);
			collection1.Load();
			Assert(collection1.Contains(config));

			var collection2 = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK);
			collection2.Load();
			Assert(collection2.Contains(config));

			AssertEquals(2, ((IBusinessObjectInternals)config).ParentCollections.OfType<AccExchangeRateConfigurationCollection>().Count());

			Assert(!collection1.ReadOnly);
			Assert(!collection2.ReadOnly);
			AssertEquals(AccExRateConfigurationLevelEnum.System, collection1.Level);
			AssertEquals(AccExRateConfigurationLevelEnum.Company, collection2.Level);
			AssertReadOnlyAndCanDelete(true, config);
		}

		public void TestCanNotDeleteWhenExistReadOnlyParentCollections()
		{
			var config = (AccExchangeRateConfiguration)GetNewBusinessObject();

			config.JCE_GC = Guid.Empty;
			AssertEquals(AccExRateConfigurationLevelEnum.System, config.Level);

			var collection1 = new AccExchangeRateConfigurationCollection(Factory);
			collection1.Load();
			Assert(collection1.Contains(config));

			var collection2 = new AccExchangeRateConfigurationCollection(Factory);
			collection2.Load();
			Assert(collection2.Contains(config));

			AssertEquals(2, ((IBusinessObjectInternals)config).ParentCollections.OfType<AccExchangeRateConfigurationCollection>().Count());

			collection2.SetReadOnlyIncludingChildren(true);
			AssertEquals(AccExRateConfigurationLevelEnum.System, collection1.Level);
			AssertEquals(AccExRateConfigurationLevelEnum.System, collection2.Level);
			Assert(!collection1.ReadOnly);
			Assert(collection2.ReadOnly);
			AssertReadOnlyAndCanDelete(true, config);
		}

		public void TestCanDeleteWhenExistMultipleParentCollections()
		{
			var config = (AccExchangeRateConfiguration)GetNewBusinessObject();

			config.JCE_GC = Guid.Empty;
			AssertEquals(AccExRateConfigurationLevelEnum.System, config.Level);

			var collection1 = new AccExchangeRateConfigurationCollection(Factory);
			collection1.Load();
			Assert(collection1.Contains(config));

			var collection2 = new AccExchangeRateConfigurationCollection(Factory);
			collection2.Load();
			Assert(collection2.Contains(config));

			AssertEquals(2, ((IBusinessObjectInternals)config).ParentCollections.OfType<AccExchangeRateConfigurationCollection>().Count());

			Assert(!collection1.ReadOnly);
			Assert(!collection1.ReadOnly);
			AssertEquals(AccExRateConfigurationLevelEnum.System, collection1.Level);
			AssertEquals(AccExRateConfigurationLevelEnum.System, collection2.Level);
			AssertReadOnlyAndCanDelete(false, config);
		}

		public void TestCurrencyConfigurations()
		{
			var parent1 = (AccExchangeRateConfiguration)GetNewBusinessObject();
			var parent2 = TestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, LedgerTypes.AccountsReceivable, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Sea);

			parent1.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			parent2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			var currencyConfig1 = parent1.CurrencyConfigurations.AddNew();
			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			var currencyConfig2 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(parent2);
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;
			parent1.CurrencyConfigurations.Load();
			parent2.CurrencyConfigurations.Load();

			Factory.Save();

			var collection = parent1.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>();
			AssertEquals("Only rows of Parent1 is loaded", 1, collection.Count());
			AssertNotNull(collection.Single(x => x.PK == currencyConfig1.PK));
		}

		public void TestUpdateJCE_Calc_CurrencyType()
		{
			var parent = (AccExchangeRateConfiguration)GetNewBusinessObject();
			AssertEquals("Default value", AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL, parent.JCE_Calc_CurrencyType);
			AssertEquals("Precondition", false, parent.CurrencyConfigurations.ReadOnly);

			parent.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			AssertEquals("Changing from ALL to CUR will clear currency collection ", 0, parent.CurrencyConfigurations.Count);
			AssertEquals(false, parent.CurrencyConfigurations.ReadOnly);

			parent.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;

			AssertEquals("Changing from CUR to ALL will create a default currency config", 1, parent.CurrencyConfigurations.Count);
			AssertEquals("With empty currency code", "", parent.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().First().JCT_Code);
			AssertEquals("With ready only currency code", true, parent.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().First().JCT_CodeInfo.ReadOnly);
			AssertEquals("Ex Rate Type should be BUY", "BUY", parent.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().First().JCT_ExRateType);
			AssertEquals(false, parent.CurrencyConfigurations.ReadOnly);
		}

		public void TestExRateConfigInitilize_ChangeJCE_Calc_CurrencyType_DoNotDeleteExistingCurrencyConfig()
		{
			var exRateConfigGUID = Guid.NewGuid();
			var curConfigGUID = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($"insert into {AccExchangeRateConfigurationViewSchema.Constants.TableName}(JCE_PK, JCE_ConfigType, JCE_JobType, JCE_Ledger, JCE_ServiceDirection, JCE_TransportMode, JCE_Preference)values('{exRateConfigGUID}', 'ERT', 'SHP', 'AP', 'EXP', 'SEA', 'TDR')");
			Db.Connection.ExecuteNonQuery($"insert into {AccJobConfigPivotSchema.Constants.TableName}(JCT_PK, JCT_JCF_JobConfig, JCT_Code, JCT_ExRateType, JCT_SystemCreateTimeUtc, JCT_SystemCreateUser, JCT_SystemLastEditTimeUtc, JCT_SystemLastEditUser)values('{curConfigGUID}', '{exRateConfigGUID}', 'USD', 'SEL', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			var exRateConfig = Factory.Load<AccExchangeRateConfiguration>(exRateConfigGUID);
			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			AssertEquals("When exchange rate config is initialized, set JCE_Calc_CurrencyType will not reset currency code", "USD",
				exRateConfig.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().First().JCT_Code);
			AssertEquals("When exchange rate config is initialized, set JCE_Calc_CurrencyType will not reset currency exRateType", "SEL",
				exRateConfig.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().First().JCT_ExRateType);
		}

		public void TestJCE_Calc_CurrencyTypeReadOnly()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var parent = (AccExchangeRateConfiguration)GetNewBusinessObject();
			parent.JCE_ParentTableCode = string.Empty;
			parent.JCE_GC = ZGuid.Empty;
			parent.JCE_ConfigType = JobConfiguration.TypeCodes.ExchangeRate;
			parent.JCE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			parent.JCE_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			parent.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
			parent.JCE_ServiceDirection = Constants.FreightShipmentDirection.Code.All;
			parent.JCE_Ledger = string.Empty;
			parent.JCE_InvoiceCurrencyType = string.Empty;

			AssertEquals("Pre-condition", false, parent.IsSystemDefaultSavedInDB);

			AssertEquals(false, parent.JCE_Calc_CurrencyTypeInfo.ReadOnly);

			Factory.Save();

			AssertEquals("Pre-condition", true, parent.IsSystemDefaultSavedInDB);

			AssertEquals(true, parent.JCE_Calc_CurrencyTypeInfo.ReadOnly);
		}

		public void TestCalculateJCE_Calc_CurrencyType_ALL()
		{
			var parent = (AccExchangeRateConfiguration)GetNewBusinessObject();
			parent.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var parentReload = newFactory.Load<AccExchangeRateConfiguration>(parent.PK);
			AssertEquals("JCE_Calc_CurrencyType should be calculated properly", AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL, parentReload.JCE_Calc_CurrencyType);
			AssertEquals(1, parentReload.CurrencyConfigurations.Count);
			AssertEquals(true, parentReload.CurrencyConfigurations[0].JCT_Code.IsEmpty);
			AssertEquals(true, parentReload.CurrencyConfigurations[0].JCT_CodeInfo.ReadOnly);
		}

		public void TestCalculateJCE_Calc_CurrencyType_CUR()
		{
			var parent = (AccExchangeRateConfiguration)GetNewBusinessObject();
			parent.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			var currencyConfig = parent.CurrencyConfigurations.AddNew();
			currencyConfig.JCT_Code = CurrencyCodes.NewZealand;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var parentReload = newFactory.Load<AccExchangeRateConfiguration>(parent.PK);
			AssertEquals("JCE_Calc_CurrencyType should be calculated properly", AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR, parentReload.JCE_Calc_CurrencyType);
			AssertEquals(1, parentReload.CurrencyConfigurations.Count);
			AssertEquals(currencyConfig.PK, parentReload.CurrencyConfigurations[0].PK);
		}

		public void TestGetCurrencyConfiguration()
		{
			var exRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig = exRateConfig.CurrencyConfigurations.AddNew();
			currencyConfig.JCT_Code = CurrencyCodes.Australia;

			AssertEquals("When currency type is ALL, Should get empty currency config", ZString.Empty, exRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_Code);
			AssertEquals("When currency type is ALL, Passing currency code will only result in empty currency config", ZString.Empty, exRateConfig.GetCurrencyConfig(currencyConfig.JCT_Code, ZDate.Empty).JCT_Code);

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			currencyConfig = exRateConfig.CurrencyConfigurations.AddNew();
			currencyConfig.JCT_Code = CurrencyCodes.Japan;

			AssertEquals("When currency type is CUR, passing currency in the collection will return currency config", currencyConfig.JCT_Code, exRateConfig.GetCurrencyConfig(currencyConfig.JCT_Code, ZDate.Empty).JCT_Code);
			AssertEquals("When currency type is CUR, passing currency not in the collection will return null", null, exRateConfig.GetCurrencyConfig(CurrencyCodes.Australia, ZDate.Empty));
		}

		public void TestGetCurrencyConfiguration_WithDateRange()
		{
			var exRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig = exRateConfig.CurrencyConfigurations.AddNew();
			currencyConfig.JCT_Code = CurrencyCodes.Australia;
			var defaultCurrencyConfig = exRateConfig.CurrencyConfigurations.GetRecord(ZString.Empty, ZDate.Empty);
			defaultCurrencyConfig.JCT_StartDate = new ZDate(2025, 3, 1);
			defaultCurrencyConfig.JCT_ExpiryDate = new ZDate(2025, 3, 3);

			AssertEquals("When currency type is ALL, Should get empty currency config within date range", ZString.Empty, exRateConfig.GetCurrencyConfig(ZString.Empty, new ZDate(2025, 3, 2)).JCT_Code);
			AssertEquals("When currency type is ALL, Passing currency code within date range will result in empty currency config", ZString.Empty, exRateConfig.GetCurrencyConfig(CurrencyCodes.Australia, new ZDate(2025, 3, 2)).JCT_Code);
			AssertEquals("When currency type is ALL, Passing currency code outside date range will result in null value", null, exRateConfig.GetCurrencyConfig(CurrencyCodes.Australia, new ZDate(2025, 3, 10)));

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			currencyConfig = exRateConfig.CurrencyConfigurations.AddNew();
			currencyConfig.JCT_Code = CurrencyCodes.Japan;
			currencyConfig.JCT_StartDate = new ZDate(2025, 3, 1);
			currencyConfig.JCT_ExpiryDate = new ZDate(2025, 3, 3);

			AssertEquals("When currency type is CUR, passing currency in the collection will return currency config within date range", currencyConfig.JCT_Code, exRateConfig.GetCurrencyConfig(CurrencyCodes.Japan, new ZDate(2025, 3, 2)).JCT_Code);
			AssertEquals("When currency type is CUR and no currency within date range, should return null", null, exRateConfig.GetCurrencyConfig(CurrencyCodes.Japan, new ZDate(2025, 3, 10)));
		}

		public void TestOnSaving_ExRateConfigMustHaveCurrencyConfig()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			AssertEquals("Have one currency config by default", 1, accExRateConfig.CurrencyConfigurations.Count);
			accExRateConfig.CurrencyConfigurations.RemoveAndDeleteAll();

			AssertExceptionThrown(typeof(Exception), "Exchange Rate Configuration must have Currency Configuration", () => Factory.Save());
		}

		public void TestUniqueIndexFailureHandlerWhenHasDuplicateCurrencyCode()
		{
			var accExRateConfig1 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			accExRateConfig1.JCE_JobType = JobInvoicingConsumerTypes.AgencyBookingCode;
			var currencyConfig1 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(accExRateConfig1);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var testObjectCreator2 = new AccountingTestObjectCreator(newFactory);
			var accExRateConfig2 = newFactory.NewWithValidTestData<AccExchangeRateConfiguration>();
			accExRateConfig2.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			var currencyConfig2 = testObjectCreator2.CreateExchangeRateCurrencyConfiguration(accExRateConfig2);

			newFactory.Save();

			AssertEquals("Precondition", true, Globals.IsUserInteractive);
			AssertEquals(currencyConfig1.JCT_Code, currencyConfig2.JCT_Code);

			accExRateConfig1.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;

			var notification = new NotificationHandlerForTest();
			var handler = ((IBusinessObjectInternals)accExRateConfig1).UniqueIndexFailureHandlers.Single(x => x.HandledUniqueIndexNames.Single() == "NR_UC__vw_ExchangeRateCurrencyConfiguration");

			AssertExceptionThrown("Should have ZSaveException due to duplicate currency code", typeof(ZSaveException), () => Factory.Save());

			handler.NotifyUserAndAttemptToResolve(notification, handler.HandledUniqueIndexNames.Single());
			AssertContains(@"The same Currency Code already exists in the Job Billing Exchange Rate configuration.
Please check values for each parameter, make sure the currency lists don't overlap when you save multiple Job Exchange Rate Configurations with the same attributes (job type, transport mode, ledger, direction, and currency type).", notification.Message);
			AssertEquals(1, notification.ReportErrorCount);
		}

		public void TestDeleteCurrencyConfigurations()
		{
			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();
			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			var currencyConfig1 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(exRateConfig, CurrencyCodes.Australia);
			var currencyConfig2 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(exRateConfig, CurrencyCodes.NewZealand);

			exRateConfig.CurrencyConfigurations.Add(currencyConfig1);
			exRateConfig.CurrencyConfigurations.Add(currencyConfig2);

			AssertEquals("Should have currency elements", 2, exRateConfig.CurrencyConfigurations.Count);

			exRateConfig.Delete();

			AssertEquals("Should automatically remove all currency elements", 0, exRateConfig.CurrencyConfigurations.Count);
			AssertEquals(true, currencyConfig1.IsDeleted);
			AssertEquals(true, currencyConfig2.IsDeleted);
		}

		public void TestCanDelete()
		{
			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();

			var testCases = new[]
			{
				new { bizoLevel = AccExRateConfigurationLevelEnum.None, colLevel = AccExRateConfigurationLevelEnum.System },
				new { bizoLevel = AccExRateConfigurationLevelEnum.System, colLevel = AccExRateConfigurationLevelEnum.System },

				new { bizoLevel = AccExRateConfigurationLevelEnum.None, colLevel = AccExRateConfigurationLevelEnum.Company },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Company, colLevel = AccExRateConfigurationLevelEnum.Company },

				new { bizoLevel = AccExRateConfigurationLevelEnum.None, colLevel = AccExRateConfigurationLevelEnum.DebtorGroup },
				new { bizoLevel = AccExRateConfigurationLevelEnum.System, colLevel = AccExRateConfigurationLevelEnum.DebtorGroup },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Company, colLevel = AccExRateConfigurationLevelEnum.DebtorGroup },
				new { bizoLevel = AccExRateConfigurationLevelEnum.DebtorGroup, colLevel = AccExRateConfigurationLevelEnum.DebtorGroup },

				new { bizoLevel = AccExRateConfigurationLevelEnum.None, colLevel = AccExRateConfigurationLevelEnum.CreditorGroup },
				new { bizoLevel = AccExRateConfigurationLevelEnum.System, colLevel = AccExRateConfigurationLevelEnum.CreditorGroup },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Company, colLevel = AccExRateConfigurationLevelEnum.CreditorGroup },
				new { bizoLevel = AccExRateConfigurationLevelEnum.CreditorGroup, colLevel = AccExRateConfigurationLevelEnum.CreditorGroup },

				new { bizoLevel = AccExRateConfigurationLevelEnum.None, colLevel = AccExRateConfigurationLevelEnum.Debtor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.System, colLevel = AccExRateConfigurationLevelEnum.Debtor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Company, colLevel = AccExRateConfigurationLevelEnum.Debtor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.DebtorGroup, colLevel = AccExRateConfigurationLevelEnum.Debtor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Debtor, colLevel = AccExRateConfigurationLevelEnum.Debtor },

				new { bizoLevel = AccExRateConfigurationLevelEnum.None, colLevel = AccExRateConfigurationLevelEnum.Creditor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.System, colLevel = AccExRateConfigurationLevelEnum.Creditor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Company, colLevel = AccExRateConfigurationLevelEnum.Creditor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.CreditorGroup, colLevel = AccExRateConfigurationLevelEnum.Creditor },
				new { bizoLevel = AccExRateConfigurationLevelEnum.Creditor, colLevel = AccExRateConfigurationLevelEnum.Creditor },
			};

			foreach (var testCase in testCases)
			{
				var config = Factory.New<AccExchangeRateConfiguration>();
				IBusinessObjectCollection collection = GetAccExchangeRateConfigurationCollection(testCase.colLevel, debtorGroup.PK, creditorGroup.PK);

				switch (testCase.bizoLevel)
				{
					case AccExRateConfigurationLevelEnum.None:
						config.JCE_ParentTableCode = "zzz";
						break;
					case AccExRateConfigurationLevelEnum.System:
						config.JCE_ParentTableCode = ZString.Empty;
						break;
					case AccExRateConfigurationLevelEnum.Company:
						config.JCE_GC = ZGuid.NewZGuid();
						break;
					case AccExRateConfigurationLevelEnum.DebtorGroup:
						config.JCE_ParentTableCode = testCase.bizoLevel.ToTablePrefix();
						break;
					case AccExRateConfigurationLevelEnum.CreditorGroup:
						config.JCE_ParentTableCode = testCase.bizoLevel.ToTablePrefix();
						break;
					case AccExRateConfigurationLevelEnum.Debtor:
						config.JCE_ParentTableCode = testCase.bizoLevel.ToTablePrefix();
						config.JCE_Ledger = LedgerTypes.AccountsReceivable;
						break;
					case AccExRateConfigurationLevelEnum.Creditor:
						config.JCE_ParentTableCode = testCase.bizoLevel.ToTablePrefix();
						config.JCE_Ledger = LedgerTypes.AccountsPayable;
						break;
					default:
						throw new InvalidOperationException("Unsupported test case");
				}

				collection.Add(config);
				AssertEquals(testCase.bizoLevel, config.Level);
				AssertEquals(testCase.colLevel, (collection as AccExchangeRateConfigurationCollection).Level);
				AssertReadOnlyAndCanDelete(testCase.bizoLevel != testCase.colLevel, config);
			}
		}

		void AssertReadOnlyAndCanDelete(bool isReadOnly, AccExchangeRateConfiguration config)
		{
			var expectedMessage = "is read only and can not be deleted";
			AssertEquals(isReadOnly, config.ReadOnly);
			AssertEquals(!isReadOnly, config.CanDelete);
			AssertContains(isReadOnly ? expectedMessage : string.Empty, config.ReasonForNotAbleToDelete);
		}

		void AssertPropertyIsConsideredInCheckForDuplication<PropType>(Action<AccExchangeRateConfiguration, PropType> setter, Func<AccExchangeRateConfiguration, PropType> getter, AccExchangeRateConfiguration origBizo, AccExchangeRateConfiguration dupBizo, PropType randomValue)
		{
			Assert(origBizo.IsDuplicateOf(dupBizo));
			AssertEquals(getter(origBizo), getter(dupBizo));
			setter(dupBizo, randomValue);
			Assert(!origBizo.IsDuplicateOf(dupBizo));
			setter(dupBizo, getter(origBizo));
		}

		public void TestLevelAndLevelName()
		{
			CombineAssertions(() =>
			{
				AssertLevelPerLedger(LedgerTypes.AccountsReceivable);
				AssertLevelPerLedger(LedgerTypes.AccountsPayable);
			});
		}

		void AssertLevelPerLedger(ZString ledger)
		{
			var exRateConfig = (AccExchangeRateConfiguration)GetNewBusinessObject();

			exRateConfig.JCE_Ledger = ledger;
			exRateConfig.JCE_GC = ZGuid.Empty;
			exRateConfig.JCE_ParentID = ZGuid.Empty;

			exRateConfig.JCE_ParentTableCode = "ZZZ";
			AssertEquals(AccExRateConfigurationLevelEnum.None, exRateConfig.Level);
			AssertEquals("None", exRateConfig.LevelName);

			exRateConfig.JCE_ParentTableCode = ZString.Empty;
			AssertEquals(AccExRateConfigurationLevelEnum.System, exRateConfig.Level);
			AssertEquals("System", exRateConfig.LevelName);

			exRateConfig.JCE_GC = ZGuid.NewZGuid();
			AssertEquals(AccExRateConfigurationLevelEnum.Company, exRateConfig.Level);
			AssertEquals("Company", exRateConfig.LevelName);

			exRateConfig.JCE_ParentID = ZGuid.NewZGuid();

			if (ledger == LedgerTypes.AccountsReceivable)
			{
				exRateConfig.JCE_ParentTableCode = OrgDebtorGroupSchema.Constants.Prefix;
				AssertEquals(AccExRateConfigurationLevelEnum.DebtorGroup, exRateConfig.Level);
				AssertEquals("Debtor Group", exRateConfig.LevelName);

				exRateConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
				AssertEquals(AccExRateConfigurationLevelEnum.Debtor, exRateConfig.Level);
				AssertEquals("Debtor", exRateConfig.LevelName);
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				exRateConfig.JCE_ParentTableCode = OrgCreditorGroupSchema.Constants.Prefix;
				AssertEquals(AccExRateConfigurationLevelEnum.CreditorGroup, exRateConfig.Level);
				AssertEquals("Creditor Group", exRateConfig.LevelName);

				exRateConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
				AssertEquals(AccExRateConfigurationLevelEnum.Creditor, exRateConfig.Level);
				AssertEquals("Creditor", exRateConfig.LevelName);
			}
			else
			{
				Fail($"Unexpected Ledger {ledger}");
			}
		}

		public void TestFetchHintForExchangeRateCurrencyConfiguration()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			TestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA", currencyCodes: new[] { CurrencyCodes.UnitedStates });
			TestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA", currencyCodes: new[] { CurrencyCodes.EuropeanUnion });
			TestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, string.Empty, "ALL", "IMP", "SEA", currencyCodes: new[] { CurrencyCodes.Japan });

			AssertEquals("PreCondition", 0, Factory.GetTableHitCount(ExchangeRateCurrencyConfiguration.Schema.TableName));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var configs = newFactory.Load<AccExchangeRateConfiguration>(new ZQuery()).ToList();

			AssertEquals("PreCondition", 3, configs.Count);
			AssertEquals("PreCondition", 0, newFactory.GetTableHitCount(ExchangeRateCurrencyConfiguration.Schema.TableName));

			foreach (var config in configs)
			{
				AssertEquals(1, config.CurrencyConfigurations.Count);
			}

			AssertEquals("We should only hit table once.", 1, newFactory.GetTableHitCount(ExchangeRateCurrencyConfiguration.Schema.TableName));
		}

		#region IJobConfiguration test

		public void TestIJobConfiguration()
		{
			var exRateConfig = (AccExchangeRateConfiguration)BusinessObject;
			var jobConfiguration = BusinessObject as IJobConfiguration;
			AssertNotNull("IJobConfiguration", jobConfiguration);
			AssertEquals("IncludeOptionsForAllJobTypes", false, jobConfiguration.IncludeOptionsForAllJobTypes);

			foreach (var jobType in JobConfigurationLookupsExtensions.GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				exRateConfig.JCE_JobType = jobType;
				AssertEquals($"JobType: {jobType}", jobType, jobConfiguration.JobType);

				foreach (var transportMode in jobConfiguration.GetTransportModeList().Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					exRateConfig.JCE_TransportMode = transportMode;
					AssertEquals($"JobType: {jobType},TransportMode: {transportMode}", transportMode, jobConfiguration.TransportMode);

					foreach (var serviceDirection in jobConfiguration.GetDirectionList().Cast<CodeDescriptionPair>().Select(x => x.Code))
					{
						exRateConfig.JCE_ServiceDirection = serviceDirection;
						AssertEquals($"JobType: {jobType},TransportMode: {transportMode}, ServiceDirection: {serviceDirection}", serviceDirection, jobConfiguration.ServiceDirection);
					}
				}
			}
		}

		AccExchangeRateConfigurationCollection GetAccExchangeRateConfigurationCollection(AccExRateConfigurationLevelEnum level, ZGuid debtorGrouppPK, ZGuid creditorGroupPK)
		{
			switch (level)
			{
				case AccExRateConfigurationLevelEnum.System:
					return new AccExchangeRateConfigurationCollection(Factory);
				case AccExRateConfigurationLevelEnum.Company:
					return new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK);
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					return new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, debtorGrouppPK);
				case AccExRateConfigurationLevelEnum.CreditorGroup:
					return new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, creditorGroupPK);
				case AccExRateConfigurationLevelEnum.Debtor:
					return new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, debtorGrouppPK, Env.CurrentCompany.OrganisationPK);
				case AccExRateConfigurationLevelEnum.Creditor:
					return new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, creditorGroupPK, Env.CurrentCompany.OrganisationPK);
				default:
					throw new InvalidOperationException("Unsupported test case");
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (AccExchangeRateConfiguration)base.GetNewBusinessObject();

			result.JCE_GC = Env.CurrentCompanyPK;
			result.JCE_Ledger = LedgerTypes.AccountsReceivable;
			result.JCE_ServiceDirection = Constants.FreightShipmentDirection.Code.Import;
			result.JCE_TransportMode = Constants.TransportModes.Sea;
			result.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;

			return result;
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		#endregion

	}
}
