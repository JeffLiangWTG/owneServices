using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCFXUpliftConfiguration))]
	sealed class AccCFXUpliftConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCFXUplifConfigurationDeletionWhenFactoryDoesNotHavePermittedToDeleteCFXUpliftConfigContext()
		{
			var factoryWithoutContext = new BusinessObjectFactory();
			Assert(!factoryWithoutContext.HasContext(BusinessContext.PermittedToDeleteCFXUpliftConfig));

			var companyInFactoryWithoutContext = factoryWithoutContext.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var cfxConfigInFactoryWithoutContext = CreateCompanyLevelCfxConfig(companyInFactoryWithoutContext);
			Assert("Company level CFX uplift not in database", !cfxConfigInFactoryWithoutContext.IsInDatabase);

			ErrorReporter.Clear();
			cfxConfigInFactoryWithoutContext.Delete();
			Assert("Should allow deletion because CFX uplift is not in database", cfxConfigInFactoryWithoutContext.IsDeleted);
			AssertEquals("Should not report error because CFX uplift is not in database", string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals("Should not report error because CFX uplift is not in database", string.Empty, ErrorReporter.LastMessageReported);

			cfxConfigInFactoryWithoutContext = CreateCompanyLevelCfxConfig(companyInFactoryWithoutContext);
			factoryWithoutContext.Save();
			Assert("Company level CFX uplift is in database", cfxConfigInFactoryWithoutContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				cfxConfigInFactoryWithoutContext.Delete();
				Assert("Should allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is off", cfxConfigInFactoryWithoutContext.IsDeleted);
				AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastMessageReported);
			}

			cfxConfigInFactoryWithoutContext = CreateCompanyLevelCfxConfig(companyInFactoryWithoutContext);
			factoryWithoutContext.Save();
			Assert("Company level CFX uplift is in database", cfxConfigInFactoryWithoutContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				try
				{
					cfxConfigInFactoryWithoutContext.Delete();
					Fail("Should not reach this line because deleting CFX config when PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on, must throw exception");
				}
				catch (CannotDeleteException ex)
				{
					Assert("Should not allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on", !cfxConfigInFactoryWithoutContext.IsDeleted);
					AssertEquals("Unable to delete Company level CFX Configuration for SHP-ALL-ALL-AFA: CFX 5% 0.1 Min. Please report this error to CargoWise Support.", ex.Message);
					AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals("Should not report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is off", string.Empty, ErrorReporter.LastMessageReported);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				cfxConfigInFactoryWithoutContext.Delete();
				Assert("Should allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is off", cfxConfigInFactoryWithoutContext.IsDeleted);
				AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "DeletingCFXConfiguration", ErrorReporter.LastKeyReported);
				AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "Trying to delete Company level CFX Configuration for SHP-ALL-ALL-AFA: CFX 5% 0.1 Min.", ErrorReporter.LastMessageReported);
			}

			cfxConfigInFactoryWithoutContext = CreateCompanyLevelCfxConfig(companyInFactoryWithoutContext);
			factoryWithoutContext.Save();
			Assert("Company level CFX uplift is in database", cfxConfigInFactoryWithoutContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				try
				{
					cfxConfigInFactoryWithoutContext.Delete();
					Fail("Should not reach this line because deleting CFX config when PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on, must throw exception");
				}
				catch (CannotDeleteException ex)
				{
					Assert("Should not allow deletion because PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations registry is on", !cfxConfigInFactoryWithoutContext.IsDeleted);
					AssertEquals("Unable to delete Company level CFX Configuration for SHP-ALL-ALL-AFA: CFX 5% 0.1 Min. Please report this error to CargoWise Support.", ex.Message);
					AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "DeletingCFXConfiguration", ErrorReporter.LastKeyReported);
					AssertEquals("Should report error because ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations registry is on", "Trying to delete Company level CFX Configuration for SHP-ALL-ALL-AFA: CFX 5% 0.1 Min.", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestCFXUplifConfigurationDeletionWhenFactoryHasPermittedToDeleteCFXUpliftConfigContext()
		{
			var factoryWithContext = new BusinessObjectFactory();
			factoryWithContext.SetContext(BusinessContext.PermittedToDeleteCFXUpliftConfig);
			Assert(factoryWithContext.HasContext(BusinessContext.PermittedToDeleteCFXUpliftConfig));
			var companyInFactoryWithContext = factoryWithContext.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var cfxConfigInFactoryWithContext = CreateCompanyLevelCfxConfig(companyInFactoryWithContext);
			factoryWithContext.Save();

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				cfxConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteCFXUpliftConfig context", cfxConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}

			cfxConfigInFactoryWithContext = CreateCompanyLevelCfxConfig(companyInFactoryWithContext);
			factoryWithContext.Save();
			Assert("Company level CFX uplift is in database", cfxConfigInFactoryWithContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ErrorReporter.Clear();
				cfxConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteCFXUpliftConfig context", cfxConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}

			cfxConfigInFactoryWithContext = CreateCompanyLevelCfxConfig(companyInFactoryWithContext);
			factoryWithContext.Save();
			Assert("Company level CFX uplift is in database", cfxConfigInFactoryWithContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				cfxConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteCFXUpliftConfig context", cfxConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}

			cfxConfigInFactoryWithContext = CreateCompanyLevelCfxConfig(companyInFactoryWithContext);
			factoryWithContext.Save();
			Assert("Company level CFX uplift is in database", cfxConfigInFactoryWithContext.IsInDatabase);

			using (AccountingMasterFilesRegistry.Instance.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				cfxConfigInFactoryWithContext.Delete();
				Assert("Should allow deletion because factory has PermittedToDeleteCFXUpliftConfig context", cfxConfigInFactoryWithContext.IsDeleted);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Should not report error because factory has PermittedToDeleteCFXUpliftConfig context", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		AccCFXUpliftConfiguration CreateCompanyLevelCfxConfig(GlbCompany company)
		{
			company.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, Core.Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);
			var cfxConfig = company.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault();
			AssertNotNull("Company level CFX uplift", cfxConfig);
			return cfxConfig;
		}

		public void TestAutoLoggingEnabled() => Assert("Auto Log Creation must be enabled.", Factory.New<AccCFXUpliftConfiguration>().IsAutoAdminBusinessObjectLoggerEnabled);

		public void TestLogReferences()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK.ToGuid()));
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK.ToGuid()));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "QWERTY";
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = org.PK;
			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			var companyLevelCFX1 = CreateCfxUplift(company.AccCFXConfigurations, JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, "", "", Core.Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);
			var companyLevelCFX2 = CreateCfxUplift(company.AccCFXConfigurations, JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, "AU", "NZ", Core.Constants.CurrencyCodes.Afghanistan, 5m, 0.1m, startDate: new DateTime(2025, 1, 1), expiryDate: new DateTime(2025, 2, 1));
			var branchLevelCFX = CreateCfxUplift(branch.AccCFXConfigurations, JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, "", "", Core.Constants.CurrencyCodes.Afghanistan, 5m, 0.1m);
			var orgLevelCFX = CreateCfxUplift(orgCompanyData.AccCFXConfigurations, JobInvoicingConsumerTypes.TransitDispatch.Code, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, "", "", "", 5m, 0.1m, startDate: new DateTime(2025, 1, 1), expiryDate: new DateTime(2025, 2, 1));

			Factory.Save();

			AssertNotNull("Company level CFX uplift 1", companyLevelCFX1);
			AssertNotNull("Company level CFX uplift 2", companyLevelCFX2);
			AssertNotNull("Branch level CFX uplift", branchLevelCFX);
			AssertNotNull("Organisation level CFX uplift", orgLevelCFX);

			AssertAddLogReference(companyLevelCFX1, "Added Company EDI CFX config for SHP-ALL-ALL-AFA--: CFX 5% 0.1 Min.");
			AssertAddLogReference(companyLevelCFX2, "Added Company EDI CFX config for SHP-ALL-ALL-AFA-AU-NZ: CFX 5% 0.1 Min. From 01/01/25 to 01/02/25");
			AssertAddLogReference(branchLevelCFX, "Added Branch BNE CFX config for SHP-IMP-AIR-AFA: CFX 5% 0.1 Min.");
			AssertAddLogReference(orgLevelCFX, "Added Organization QWERTY CFX config for TDC-EXP-AIR-: CFX 5% 0.1 Min. From 01/01/25 to 01/02/25");

			foreach (var cfxConfig in new[] { companyLevelCFX1, companyLevelCFX2, branchLevelCFX, orgLevelCFX })
			{
				cfxConfig.JCF_TransportMode = Core.Constants.TransportModes.Sea;
				cfxConfig.JCF_CFXPercentage = 10m;
			}

			companyLevelCFX1.JCF_RN_NKDestinationCountry = "AU";
			companyLevelCFX1.JCF_StartDate = new ZDate(2025, 1, 5);
			companyLevelCFX1.JCF_ExpiryDate = new ZDate(2025, 1, 9);
			companyLevelCFX2.JCF_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Domestic;
			companyLevelCFX2.JCF_RX_NKCurrency = "";
			companyLevelCFX2.JCF_StartDate = new ZDate(2025, 1, 5);
			companyLevelCFX2.JCF_ExpiryDate = new ZDate(2025, 1, 9);
			branchLevelCFX.JCF_StartDate = new ZDate(2025, 1, 5);
			branchLevelCFX.JCF_ExpiryDate = new ZDate(2025, 1, 9);
			orgLevelCFX.JCF_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			orgLevelCFX.JCF_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.All;
			orgLevelCFX.JCF_RN_NKOriginCountry = "NZ";
			orgLevelCFX.JCF_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;
			orgLevelCFX.JCF_StartDate = orgLevelCFX.JCF_ExpiryDate = ZDate.Empty;
			Factory.Save();

			AssertEditLogReference(companyLevelCFX1, "Changed Company EDI CFX config for SHP-ALL-ALL-AFA--: CFX 5% 0.1 Min. changed to SHP-ALL-SEA-AFA--AU: CFX 10% 0.1 Min. From 05/01/25 to 09/01/25");
			AssertEditLogReference(companyLevelCFX2, "Changed Company EDI CFX config for SHP-ALL-ALL-AFA-AU-NZ: CFX 5% 0.1 Min. From 01/01/25 to 01/02/25 changed to SHP-DOM-SEA-: CFX 10% 0.1 Min. From 05/01/25 to 09/01/25");
			AssertEditLogReference(branchLevelCFX, "Changed Branch BNE CFX config for SHP-IMP-AIR-AFA: CFX 5% 0.1 Min. changed to SHP-IMP-SEA-AFA: CFX 10% 0.1 Min. From 05/01/25 to 09/01/25");
			AssertEditLogReference(orgLevelCFX, "Changed Organization QWERTY CFX config for TDC-EXP-AIR-: CFX 5% 0.1 Min. From 01/01/25 to 01/02/25 changed to BRK-ALL-SEA-NZD-NZ-: CFX 10% 0.1 Min.");

			foreach (var cfxConfig in new[] { companyLevelCFX1, companyLevelCFX2, branchLevelCFX, orgLevelCFX })
			{
				cfxConfig.Delete();
			}
			Factory.Save();

			AssertDeleteLogReference(companyLevelCFX1.PK, "Deleted Company EDI CFX config for SHP-ALL-SEA-AFA--AU: CFX 10% 0.1 Min. From 05/01/25 to 09/01/25");
			AssertDeleteLogReference(companyLevelCFX2.PK, "Deleted Company EDI CFX config for SHP-DOM-SEA-: CFX 10% 0.1 Min. From 05/01/25 to 09/01/25");
			AssertDeleteLogReference(branchLevelCFX.PK, "Deleted Branch BNE CFX config for SHP-IMP-SEA-AFA: CFX 10% 0.1 Min. From 05/01/25 to 09/01/25");
			AssertDeleteLogReference(orgLevelCFX.PK, "Deleted Organization QWERTY CFX config for BRK-ALL-SEA-NZD-NZ-: CFX 10% 0.1 Min.");

			AccCFXUpliftConfiguration CreateCfxUplift(AccCFXUpliftConfigurationCollection cfx, string jobType, string direction, string transportMode, string origin, string destination, string currency, decimal cfxPercent, decimal cfxMin, DateTime? startDate = null, DateTime? expiryDate = null)
			{
				var cfxUplift = cfx.AddNew();
				cfxUplift.JCF_JobType = jobType;
				cfxUplift.JCF_ServiceDirection = direction;
				cfxUplift.JCF_TransportMode = transportMode;
				cfxUplift.JCF_RN_NKOriginCountry = origin;
				cfxUplift.JCF_RN_NKDestinationCountry = destination;
				cfxUplift.JCF_RX_NKCurrency = currency;
				cfxUplift.JCF_StartDate = startDate.HasValue ? new ZDate(startDate.Value) : ZDate.Empty;
				cfxUplift.JCF_ExpiryDate = expiryDate.HasValue ? new ZDate(expiryDate.Value) : ZDate.Empty;
				cfxUplift.JCF_CFXPercentage = cfxPercent;
				cfxUplift.JCF_CFXMinimum = cfxMin;
				return cfxUplift;
			}

			void AssertAddLogReference(AccCFXUpliftConfiguration cfxConfig, string expectedAddLogReference) => AssertLogReference(cfxConfig, AutoEvents.AddedARecordToTheSystem, expectedAddLogReference);

			void AssertEditLogReference(AccCFXUpliftConfiguration cfxConfig, string expectedEditLogReference) => AssertLogReference(cfxConfig, AutoEvents.EditedARecord, expectedEditLogReference);

			void AssertLogReference(AccCFXUpliftConfiguration cfxConfig, Event eventToAssert, string expectedLogReference)
			{
				var log = ((IAutoAdminLogTarget)cfxConfig).Logs.MostRecentLogByEventTime(eventToAssert);
				AssertNotNull(log);
				AssertEquals(expectedLogReference, log.SL_Reference);
			}

			void AssertDeleteLogReference(ZGuid cfxConfigPK, string expectedDeleteLogReference)
			{
				var deleteLog = Factory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, cfxConfigPK).AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeletedARecordInTheSystem.Code));
				AssertNotNull(deleteLog);
				AssertEquals(expectedDeleteLogReference, deleteLog.SL_Reference);
			}
		}

		public void TestSetDefaultsCorrectly()
		{
			var cfxUpliftConfig = Factory.New<AccCFXUpliftConfiguration>();

			AssertEquals("CFX", cfxUpliftConfig.JCF_ConfigType);
			AssertEquals("AR", cfxUpliftConfig.JCF_Ledger);
			AssertEquals("ALL", cfxUpliftConfig.JCF_JobType);
			AssertEquals(ZString.Empty, cfxUpliftConfig.JCF_RX_NKCurrency);
			AssertEquals(ZString.Empty, cfxUpliftConfig.JCF_RN_NKOriginCountry);
			AssertEquals(ZString.Empty, cfxUpliftConfig.JCF_RN_NKDestinationCountry);
			AssertEquals(ZDate.Empty, cfxUpliftConfig.JCF_StartDate);
			AssertEquals(ZDate.Empty, cfxUpliftConfig.JCF_ExpiryDate);
		}

		public void TestDeterminesDuplicateCorrectly()
		{
			var fakeCompanyGuid = ZGuid.NewZGuid();

			var cfxUpliftConfig = (AccCFXUpliftConfiguration)GetNewBusinessObject();
			var cfxUpliftConfigDup = (AccCFXUpliftConfiguration)GetNewBusinessObject();

			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_GC = v, b => b.JCF_GC, cfxUpliftConfig, cfxUpliftConfigDup, ZGuid.NewZGuid());
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_ParentTableCode = v, b => b.JCF_ParentTableCode, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("OH"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_ParentID = v, b => b.JCF_ParentID, cfxUpliftConfig, cfxUpliftConfigDup, ZGuid.NewZGuid());
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_JobType = v, b => b.JCF_JobType, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("JOB"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_RX_NKCurrency = v, b => b.JCF_RX_NKCurrency, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("USD"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_ServiceDirection = v, b => b.JCF_ServiceDirection, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("DOM"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_TransportMode = v, b => b.JCF_TransportMode, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("TRN"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_RN_NKOriginCountry = v, b => b.JCF_RN_NKOriginCountry, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("AU"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_RN_NKDestinationCountry = v, b => b.JCF_RN_NKDestinationCountry, cfxUpliftConfig, cfxUpliftConfigDup, new ZString("AU"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_StartDate = v, b => b.JCF_StartDate, cfxUpliftConfig, cfxUpliftConfigDup, new ZDate("2025-01-01"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.JCF_ExpiryDate = v, b => b.JCF_ExpiryDate, cfxUpliftConfig, cfxUpliftConfigDup, new ZDate("2025-01-01"));
		}

		void TestPropertyIsConsideredInCheckForDuplication<PropType>(Action<AccCFXUpliftConfiguration, PropType> setter, Func<AccCFXUpliftConfiguration, PropType> getter, AccCFXUpliftConfiguration origBizo, AccCFXUpliftConfiguration dupBizo, PropType randomValue)
		{
			Assert(origBizo.IsDuplicateOf(dupBizo));
			AssertEquals(getter(origBizo), getter(dupBizo));
			setter(dupBizo, randomValue);
			Assert(!origBizo.IsDuplicateOf(dupBizo));
			setter(dupBizo, getter(origBizo));
		}

		public void TestOverlapsWith()
		{
			var startDate = new ZDate(2025, 1, 12);
			var expiryDate = new ZDate(2025, 1, 15);

			var initialCfxConfig = CreateCfxConfig();

			AssertOverlapsWith(cfx =>
			{
				cfx.JCF_StartDate = startDate;
				cfx.JCF_ExpiryDate = expiryDate;
			});

			AssertOverlapsWith(cfx =>
			{
				cfx.JCF_StartDate = new ZDate(2025, 1, 11);
				cfx.JCF_ExpiryDate = new ZDate(2025, 1, 12);
			});

			AssertOverlapsWith(cfx =>
			{
				cfx.JCF_StartDate = new ZDate(2025, 1, 13);
				cfx.JCF_ExpiryDate = new ZDate(2025, 1, 14);
			});

			AssertOverlapsWith(cfx =>
			{
				cfx.JCF_StartDate = new ZDate(2025, 1, 15);
				cfx.JCF_ExpiryDate = new ZDate(2025, 1, 16);
			});

			AssertOverlapsWith(cfx =>
			{
				cfx.JCF_StartDate = new ZDate(2025, 1, 11);
				cfx.JCF_ExpiryDate = new ZDate(2025, 1, 16);
			});

			AssertNoOverlapWith(cfx =>
			{
				cfx.JCF_StartDate = new ZDate(2025, 1, 16);
				cfx.JCF_ExpiryDate = new ZDate(2025, 1, 17);
			});

			AssertNoOverlapWith(cfx => cfx.JCF_GC = ZGuid.NewZGuid());
			AssertNoOverlapWith(cfx => cfx.JCF_ParentTableCode = new ZString("OH"));
			AssertNoOverlapWith(cfx => cfx.JCF_ParentID = ZGuid.NewZGuid());
			AssertNoOverlapWith(cfx => cfx.JCF_JobType = new ZString("JOB"));
			AssertNoOverlapWith(cfx => cfx.JCF_ServiceDirection = new ZString("DOM"));
			AssertNoOverlapWith(cfx => cfx.JCF_TransportMode = new ZString("TRN"));
			AssertNoOverlapWith(cfx => cfx.JCF_RN_NKOriginCountry = new ZString("AU"));
			AssertNoOverlapWith(cfx => cfx.JCF_RN_NKDestinationCountry = new ZString("AU"));
			AssertNoOverlapWith(cfx => cfx.JCF_RX_NKCurrency = new ZString("USD"));

			void AssertOverlapsWith(Action<AccCFXUpliftConfiguration> setupCfxConfig)
			{
				var newCfxConfig = (AccCFXUpliftConfiguration)GetNewBusinessObject();
				setupCfxConfig(newCfxConfig);
				Assert(newCfxConfig.OverlapsWith(initialCfxConfig));
			}

			void AssertNoOverlapWith(Action<AccCFXUpliftConfiguration> setupCfxConfig)
			{
				var newCfxConfig = CreateCfxConfig();
				setupCfxConfig(newCfxConfig);
				Assert(!newCfxConfig.OverlapsWith(initialCfxConfig));
			}

			AccCFXUpliftConfiguration CreateCfxConfig()
			{
				var newCfxConfig = (AccCFXUpliftConfiguration)GetNewBusinessObject();
				newCfxConfig.JCF_StartDate = startDate;
				newCfxConfig.JCF_ExpiryDate = expiryDate;
				return newCfxConfig;
			}
		}

		public void TestLevelIsCalculatedProperly()
		{
			var cfxUpliftConfig = (AccCFXUpliftConfiguration)GetNewBusinessObject();

			AssertEquals(AccCFXConfigurationLevelEnum.Company, cfxUpliftConfig.Level);
			AssertEquals("Company", cfxUpliftConfig.LevelName);

			cfxUpliftConfig.JCF_ParentTableCode = GlbBranchSchema.Constants.Prefix;

			AssertEquals(AccCFXConfigurationLevelEnum.Branch, cfxUpliftConfig.Level);
			AssertEquals("Branch", cfxUpliftConfig.LevelName);

			cfxUpliftConfig.JCF_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			AssertEquals(AccCFXConfigurationLevelEnum.Organisation, cfxUpliftConfig.Level);
			AssertEquals("Organization", cfxUpliftConfig.LevelName);
		}

		public void TestCalculatesReadOnlyProperly()
		{
			var testCases = new[]
			{
				new { bizoLevel = AccCFXConfigurationLevelEnum.Company, colLevel = AccCFXConfigurationLevelEnum.Company, expected = false },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Company, colLevel = AccCFXConfigurationLevelEnum.Branch, expected = true },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Company, colLevel = AccCFXConfigurationLevelEnum.Organisation, expected = true },

				new { bizoLevel = AccCFXConfigurationLevelEnum.Branch, colLevel = AccCFXConfigurationLevelEnum.Company, expected = true },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Branch, colLevel = AccCFXConfigurationLevelEnum.Branch, expected = false },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Branch, colLevel = AccCFXConfigurationLevelEnum.Organisation, expected = true },

				new { bizoLevel = AccCFXConfigurationLevelEnum.Organisation, colLevel = AccCFXConfigurationLevelEnum.Company, expected = true },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Organisation, colLevel = AccCFXConfigurationLevelEnum.Branch, expected = true },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Organisation, colLevel = AccCFXConfigurationLevelEnum.Organisation, expected = false },
			};

			var cfxUpliftConfig = (AccCFXUpliftConfiguration)GetNewBusinessObject();

			Assert(!cfxUpliftConfig.ReadOnly);

			foreach (var testCase in testCases)
			{
				AccCFXUpliftConfigurationCollection col;

				switch (testCase.colLevel)
				{
					case AccCFXConfigurationLevelEnum.Company:
						col = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
						break;
					case AccCFXConfigurationLevelEnum.Branch:
						col = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK);
						break;
					case AccCFXConfigurationLevelEnum.Organisation:
						col = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);
						break;
					default:
						throw new InvalidOperationException("Unsupported test case");
				}

				cfxUpliftConfig.JCF_ParentTableCode = testCase.bizoLevel.ToTablePrefix();
				col.Add(cfxUpliftConfig);
				AssertEquals($"bizo: {testCase.bizoLevel}, col: {testCase.colLevel}", testCase.expected, cfxUpliftConfig.ReadOnly);
				col.Remove(cfxUpliftConfig);
			}

			Env.Security.CompaniesModifyCurrencyCFXUplift.IsAllowed = false;
			var readonlyCollection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			cfxUpliftConfig.JCF_ParentTableCode = readonlyCollection.Level.ToTablePrefix();
			Assert(readonlyCollection.ReadOnly);
			readonlyCollection.Add(cfxUpliftConfig);
			Assert(cfxUpliftConfig.ReadOnly);
		}

		public void TestCanDeleteWhenExistDifferentLevelParentCollections()
		{
			var config = (AccCFXUpliftConfiguration)GetNewBusinessObject();
			AssertEquals(AccCFXConfigurationLevelEnum.Company, config.Level);

			var collection1 = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			collection1.Load();
			Assert(collection1.Contains(config));

			var collection2 = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK);
			collection2.Load();
			Assert(collection2.Contains(config));

			AssertEquals(2, ((IBusinessObjectInternals)config).ParentCollections.OfType<AccCFXUpliftConfigurationCollection>().Count());

			Assert(!collection1.ReadOnly);
			Assert(!collection2.ReadOnly);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, collection1.Level);
			AssertEquals(AccCFXConfigurationLevelEnum.Branch, collection2.Level);
			AssertReadOnlyAndCanDelete(true, config);
		}

		public void TestCanNotDeleteWhenExistReadOnlyParentCollections()
		{
			var config = (AccCFXUpliftConfiguration)GetNewBusinessObject();

			AssertEquals(AccCFXConfigurationLevelEnum.Company, config.Level);

			var collection1 = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			collection1.Load();
			Assert(collection1.Contains(config));

			var collection2 = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			collection2.Load();
			Assert(collection2.Contains(config));

			AssertEquals(2, ((IBusinessObjectInternals)config).ParentCollections.OfType<AccCFXUpliftConfigurationCollection>().Count());

			collection2.SetReadOnlyIncludingChildren(true);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, collection1.Level);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, collection2.Level);
			Assert(!collection1.ReadOnly);
			Assert(collection2.ReadOnly);
			AssertReadOnlyAndCanDelete(true, config);
		}

		public void TestCanDeleteWhenExistMultipleParentCollections()
		{
			var config = (AccCFXUpliftConfiguration)GetNewBusinessObject();
			AssertEquals(AccCFXConfigurationLevelEnum.Company, config.Level);

			var collection1 = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			collection1.Load();
			Assert(collection1.Contains(config));

			var collection2 = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
			collection2.Load();
			Assert(collection2.Contains(config));

			AssertEquals(2, ((IBusinessObjectInternals)config).ParentCollections.OfType<AccCFXUpliftConfigurationCollection>().Count());

			Assert(!collection1.ReadOnly);
			Assert(!collection1.ReadOnly);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, collection1.Level);
			AssertEquals(AccCFXConfigurationLevelEnum.Company, collection2.Level);
			AssertReadOnlyAndCanDelete(false, config);
		}

		public void TestCanDelete()
		{
			var testCases = new[]
			{
				new { bizoLevel = AccCFXConfigurationLevelEnum.Company, colLevel = AccCFXConfigurationLevelEnum.Company },

				new { bizoLevel = AccCFXConfigurationLevelEnum.Company, colLevel = AccCFXConfigurationLevelEnum.Branch },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Branch, colLevel = AccCFXConfigurationLevelEnum.Branch },

				new { bizoLevel = AccCFXConfigurationLevelEnum.Company, colLevel = AccCFXConfigurationLevelEnum.Organisation },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Branch, colLevel = AccCFXConfigurationLevelEnum.Organisation },
				new { bizoLevel = AccCFXConfigurationLevelEnum.Organisation, colLevel = AccCFXConfigurationLevelEnum.Organisation },
			};

			foreach (var testCase in testCases)
			{
				IBusinessObjectCollection collection;

				switch (testCase.colLevel)
				{
					case AccCFXConfigurationLevelEnum.Company:
						collection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK);
						break;
					case AccCFXConfigurationLevelEnum.Branch:
						collection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK);
						break;
					case AccCFXConfigurationLevelEnum.Organisation:
						collection = new AccCFXUpliftConfigurationCollection(Factory, Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentCompany.OrganisationPK);
						break;
					default:
						throw new InvalidOperationException("Unsupported test case");
				}

				var config = (AccCFXUpliftConfiguration)GetNewBusinessObject();
				config.JCF_ParentTableCode = testCase.bizoLevel.ToTablePrefix();
				collection.Add(config);
				AssertEquals(testCase.bizoLevel, config.Level);
				AssertEquals(testCase.colLevel, (collection as AccCFXUpliftConfigurationCollection).Level);
				AssertReadOnlyAndCanDelete(testCase.bizoLevel != testCase.colLevel, config);
			}
		}

		void AssertReadOnlyAndCanDelete(bool isReadOnly, AccCFXUpliftConfiguration config)
		{
			var expectedMessage = "is read only and can not be deleted";
			AssertEquals(isReadOnly, config.ReadOnly);
			AssertEquals(!isReadOnly, config.CanDelete);
			AssertContains(isReadOnly ? expectedMessage : string.Empty, config.ReasonForNotAbleToDelete);
		}

		public void TestHasCorrectLookupAttributes()
		{
			Func<string, string> getLookupRef = s => typeof(AccCFXUpliftConfiguration).GetProperty(s).GetAttribute<ListAttribute>().ListDataSourceMember;

			AssertEquals("Lookups.JobTypesList", getLookupRef(nameof(AccCFXUpliftConfiguration.JCF_JobType)));
			AssertEquals("Lookups.DirectionsList", getLookupRef(nameof(AccCFXUpliftConfiguration.JCF_ServiceDirection)));
			AssertEquals("Lookups.TransportModesList", getLookupRef(nameof(AccCFXUpliftConfiguration.JCF_TransportMode)));
		}

		public void TestCFXPercentageCorrectDecimalPlaces()
		{
			var decimalPlacesOnPercentage = typeof(AccCFXUpliftConfiguration).GetProperty(nameof(AccCFXUpliftConfiguration.JCF_CFXPercentage)).GetAttribute<DecimalPlacesAttribute>().DecimalPlaces;
			AssertEquals(2, decimalPlacesOnPercentage);
		}

		public void TestMinimumHasCorrectDecimalPlaces()
		{
			var decimalMemberOnMinimum = typeof(AccCFXUpliftConfiguration).GetProperty(nameof(AccCFXUpliftConfiguration.JCF_CFXMinimum)).GetAttribute<DecimalPlacesAttribute>().DecimalPlacesMember;
			AssertEquals(nameof(AccCFXUpliftConfiguration.LocalCurrencyDecimals), decimalMemberOnMinimum);

			var cfxUpliftConfig = (AccCFXUpliftConfiguration)GetNewBusinessObject();

			cfxUpliftConfig.JCF_GC = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Decimals, cfxUpliftConfig.LocalCurrencyDecimals);

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_RX_NKLocalCurrency = "IDR";
			AssertEquals(0, otherCompany.LocalCurrency.Decimals);

			cfxUpliftConfig.JCF_GC = otherCompany.PK;
			AssertEquals(otherCompany.LocalCurrency.Decimals, cfxUpliftConfig.LocalCurrencyDecimals);

			cfxUpliftConfig.JCF_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Decimals, cfxUpliftConfig.LocalCurrencyDecimals);
		}

		public void TestIJobConfiguration()
		{
			var bizo = (IJobConfiguration)Factory.New<AccCFXUpliftConfiguration>();
			Assert(bizo.IncludeOptionsForAllJobTypes);
		}

		public void TestOriginCountry()
		{
			var config = (AccCFXUpliftConfiguration)GetNewBusinessObject();
			foreach (var jobType in new[] { JobInvoicingConsumerTypes.Brokerage.Code, JobInvoicingConsumerTypes.Shipment.Code })
			{
				config.JCF_JobType = jobType;
				config.JCF_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.All;
				Assert($"Should be enabled when job type is {jobType} and direction is ALL", !config.JCF_RN_NKOriginCountryInfo.ReadOnly);

				config.JCF_RN_NKOriginCountry = "AU";
				config.JCF_JobType = "ALL";
				Assert($"Should be readonly when job type is changed from {jobType} to ALL", config.JCF_RN_NKOriginCountryInfo.ReadOnly);
				Assert($"Should be cleared when job type is changed from {jobType} to ALL", config.JCF_RN_NKOriginCountry.IsEmpty);

				config.JCF_JobType = jobType;
				Assert($"Should be re-enabled when job type is {jobType} and direction is ALL", !config.JCF_RN_NKOriginCountryInfo.ReadOnly);
				Assert($"Should be empty when job type is {jobType} and direction is ALL", config.JCF_RN_NKOriginCountry.IsEmpty);

				config.JCF_RN_NKOriginCountry = "AU";
				config.JCF_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
				Assert($"Should be readonly when job type is {jobType} and direction is not ALL", config.JCF_RN_NKOriginCountryInfo.ReadOnly);
				Assert($"Should be cleared when job type is {jobType} and direction is not ALL", config.JCF_RN_NKOriginCountry.IsEmpty);
			}
		}

		public void TestDestinationCountry()
		{
			var config = (AccCFXUpliftConfiguration)GetNewBusinessObject();
			foreach (var jobType in new[] { JobInvoicingConsumerTypes.Brokerage.Code, JobInvoicingConsumerTypes.Shipment.Code })
			{
				config.JCF_JobType = jobType;
				config.JCF_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.All;
				Assert($"Should be enabled when job type is {jobType} and direction is ALL", !config.JCF_RN_NKDestinationCountryInfo.ReadOnly);

				config.JCF_RN_NKDestinationCountry = "AU";
				config.JCF_JobType = "ALL";
				Assert($"Should be readonly when job type is changed from {jobType} to ALL", config.JCF_RN_NKDestinationCountryInfo.ReadOnly);
				Assert($"Should be cleared when job type is changed from {jobType} to ALL", config.JCF_RN_NKDestinationCountry.IsEmpty);

				config.JCF_JobType = jobType;
				Assert($"Should be re-enabled when job type is {jobType} and direction is ALL", !config.JCF_RN_NKDestinationCountryInfo.ReadOnly);
				Assert($"Should be empty when job type is {jobType} and direction is ALL", config.JCF_RN_NKDestinationCountry.IsEmpty);

				config.JCF_RN_NKDestinationCountry = "AU";
				config.JCF_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
				Assert($"Should be readonly when job type is {jobType} and direction is not ALL", config.JCF_RN_NKDestinationCountryInfo.ReadOnly);
				Assert($"Should be cleared when job type is {jobType} and direction is not ALL", config.JCF_RN_NKDestinationCountry.IsEmpty);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cfxUpliftConfig = (AccCFXUpliftConfiguration)base.GetNewBusinessObject();

			cfxUpliftConfig.JCF_GC = Env.CurrentCompanyPK;
			cfxUpliftConfig.JCF_ServiceDirection = "IMP";
			cfxUpliftConfig.JCF_TransportMode = "SEA";

			return cfxUpliftConfig;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
