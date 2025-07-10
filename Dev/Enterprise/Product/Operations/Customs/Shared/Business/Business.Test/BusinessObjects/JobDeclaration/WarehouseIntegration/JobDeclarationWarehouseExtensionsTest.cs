using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.WarehouseExtensions.Testing
{
	sealed class JobDeclarationWarehouseExtensionsTest : WhsDataTestHelper
	{
		public void TestPublishAcceptEventForWHSInwardAndSaveIfNeededWhenIsManualWhsUpdate()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENTE234", 100m);
				Factory.Save();
				var entry = declaration.CustomsEntryHeaders[0];
				AssertEquals("PreCondition", false, ((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);
				AssertEquals("PreCondition", false, ((IWarehouseIntegrationSupporter)entry).HasManualWhsUpdate);

				entry.PublishShipmentForWHSInward(false);
				entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true, false);
				AssertEquals("HasManualWhsUpdate was not changed if PublishAcceptEventForWHSInwardAndSaveIfNeeded is not invoked by manual whs upgrade", false, ((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);
				AssertEquals("HasManualWhsUpdate was not changed if PublishAcceptEventForWHSInwardAndSaveIfNeeded is not invoked by manual whs upgrade", false, ((IWarehouseIntegrationSupporter)entry).HasManualWhsUpdate);

				entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true, true);
				AssertEquals("HasManualWhsUpdate was changed to true if PublishAcceptEventForWHSInwardAndSaveIfNeeded is invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);
				AssertEquals("HasManualWhsUpdate was changed to true if PublishAcceptEventForWHSInwardAndSaveIfNeeded is invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)entry).HasManualWhsUpdate);
			}
		}

		public void TestPublishCancelEventForWHSInwardAndSaveIfNeededWhenIsManualWhsUpdate()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				AssertCancelEventForWHSInward(false, true, "Test1");
				AssertCancelEventForWHSInward(true, false, "Test2");
			}
		}

		void AssertCancelEventForWHSInward(bool isManualWhsUpdate, bool hasManualWhsUpdate, string declarationReference)
		{
			var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, "ENTE234", 100m);
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders[0];
			entry.PublishShipmentForWHSInward(false);
			entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true, true);

			AssertEquals("PreCondition", true, ((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);
			AssertEquals("PreCondition", true, ((IWarehouseIntegrationSupporter)entry).HasManualWhsUpdate);

			entry.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, isManualWhsUpdate: isManualWhsUpdate);
			AssertEquals("HasManualWhsUpdate changed if PublishCancelEventForWHSInwardAndSaveIfNeeded is invoked by manual whs upgrade", hasManualWhsUpdate, ((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);
			AssertEquals("HasManualWhsUpdate changed if PublishCancelEventForWHSInwardAndSaveIfNeeded is not invoked by manual whs upgrade", hasManualWhsUpdate, ((IWarehouseIntegrationSupporter)entry).HasManualWhsUpdate);
		}

		public void TestPublishAcceptEventForWHSOutwardAndSaveIfNeededWhenIsManualWhsUpdate()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				var inwardEntry = inwardDeclaration.CustomsEntryHeaders[0];

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002133", "ENT324", 60m);
				outwardDeclaration.MessageInitiator = messageInitiator;
				var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				Factory.Save();

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders[0];
				AssertEquals("PreCondition", false, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
				AssertEquals("PreCondition", false, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);

				outwardDeclaration.PublishShipmentForWHSOutward();
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true, false);
				AssertEquals("HasManualWhsUpdate was not changed if PublishAcceptEventForWHSOutwardAndSaveIfNeeded is not invoked by manual whs upgrade", false, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
				AssertEquals("HasManualWhsUpdate was not changed if PublishAcceptEventForWHSOutwardAndSaveIfNeeded is not invoked by manual whs upgrade", false, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);

				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true, true);
				AssertEquals("HasManualWhsUpdate was changed to true if PublishAcceptEventForWHSOutwardAndSaveIfNeeded is invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
				AssertEquals("HasManualWhsUpdate was changed to true if PublishAcceptEventForWHSOutwardAndSaveIfNeeded is invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);
			}
		}

		public void TestPublishCancelEventForWHSOutwardAndSaveIfNeededWhenIsManualWhsUpdate()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				AssertCancelEventForWHSOutward(false, true, "ENT324", "B00002132", "B00002134");
				AssertCancelEventForWHSOutward(true, false, "ENT325", "B00002133", "B00002135");
			}
		}

		void AssertCancelEventForWHSOutward(bool isManualWhsUpdate, bool hasManualWhsUpdate, string entryNumber, string declarationReference, string outwardDeclarationReference)
		{
			var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, entryNumber, 110m);
			Factory.Save();
			inwardDeclaration.PublishShipmentForWHSInward(false);
			var inwardEntry = inwardDeclaration.CustomsEntryHeaders[0];

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, outwardDeclarationReference, entryNumber, 60m);
			outwardDeclaration.MessageInitiator = messageInitiator;
			var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
			outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
			Factory.Save();
			var outwardEntry = outwardDeclaration.CustomsEntryHeaders[0];
			outwardDeclaration.PublishShipmentForWHSOutward();
			outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true, true);
			AssertEquals("PreCondition", true, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
			AssertEquals("PreCondition", true, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);

			outwardDeclaration.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true, isManualWhsUpdate);
			AssertEquals("HasManualWhsUpdate changed if PublishCancelEventForWHSOutwardAndSaveIfNeeded is invoked by manual whs upgrade", hasManualWhsUpdate, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
			AssertEquals("HasManualWhsUpdate changed if PublishCancelEventForWHSOutwardAndSaveIfNeeded is invoked by manual whs upgrade", hasManualWhsUpdate, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);

			outwardDeclaration.PublishShipmentForWHSOutward();
			outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true, true);
			AssertEquals("PreCondition when exists canceled", true, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
			AssertEquals("PreCondition when exists canceled", true, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);

			outwardDeclaration.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true, isManualWhsUpdate);
			AssertEquals("HasManualWhsUpdate changed if PublishCancelEventForWHSOutwardAndSaveIfNeeded is invoked by manual whs upgrade when exists canceled", hasManualWhsUpdate, ((IWarehouseIntegrationSupporter)outwardDeclaration).HasManualWhsUpdate);
			AssertEquals("HasManualWhsUpdate changed if PublishCancelEventForWHSOutwardAndSaveIfNeeded is invoked by manual whs upgrade when exists canceled", hasManualWhsUpdate, ((IWarehouseIntegrationSupporter)outwardEntry).HasManualWhsUpdate);
		}

		public void TestPublishShipmentForWHSChangeOfOwnershipWhenIsManualWhsUpdate()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;

				var (changeOfOwnershipDeclaration, changeOfOwnershipEntry) = GetNewChangeOfOwnershipDeclarationAndEntry(helper);

				AssertEquals("PreCondition", false, ((IWarehouseIntegrationSupporter)changeOfOwnershipDeclaration).HasManualWhsUpdate);
				AssertEquals("PreCondition", false, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);

				CombineAssertions(() =>
				{
					changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(false, false);
					AssertEquals("HasManualWhsUpdate was not changed if PublishShipmentForWHSChangeOfOwnership is not invoked by manual whs upgrade", false, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);

					changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(false, true);
					AssertEquals("HasManualWhsUpdate was changed to true if PublishShipmentForWHSChangeOfOwnership is invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);

					changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(true, true);
					AssertEquals("HasManualWhsUpdate was changed to true if PublishShipmentForWHSChangeOfOwnership is invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);
				});
			}
		}

		public void TestPublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeededWhenIsManualWhsUpdate()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				var (changeOfOwnershipDeclaration, changeOfOwnershipEntry) = GetNewChangeOfOwnershipDeclarationAndEntry(helper);

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(false, true);
				AssertEquals("PreCondition", true, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);

				changeOfOwnershipDeclaration.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true, false);
				AssertEquals("HasManualWhsUpdate was not changed if PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded is not invoked by manual whs upgrade", true, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);

				changeOfOwnershipEntry.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true, true);
				AssertEquals("HasManualWhsUpdate was changed to false if PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded is invoked by manual whs upgrade", false, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);
			}
		}

		public void TestPublishAcceptEventForWHSOutwardInADifferentFactory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.JE_GoodsDescription = "HELLO";
			declaration.PublishAcceptEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, declaration.HasChanges);
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised));
			Factory.Save();
			declaration.JE_GoodsDescription = "BYE";
			declaration.PublishAcceptEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, declaration.HasChanges);
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised));
		}

		public void TestChangeOfRegimePublishMethods()
		{
			var helper = new WhsDataTestHelper(Factory) { WarehouseDefaultCountry = Core.Constants.CountryCodes.Eritrea };
			(var disposable, var customsDetails) = WarehouseCustomsDetailsChangeOfRegimeForTesting.Setup(Core.Constants.CountryCodes.Eritrea);
			using (disposable)
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardCusProcedure = helper.InwardCusProcedure;
				var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
				helper.SetupInwardProcessingAreaAndSave(helper.WhsWarehouse2);

				var inwardData = helper.CreateChangeOfRegimeEntryData(JobMessageTypeList.Codes.Import, "BZA0001232", "ENT32342", 100m);
				var inwardEntryInstruction = inwardData.Instruction;
				inwardEntryInstruction.CEI_OA_Warehouse2 = inwardEntryInstruction.CEI_OA_Warehouse;
				inwardEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				var inwardEntry = inwardData.Entry;
				var inwardInvoiceLine = inwardData.InvoiceLine;
				inwardInvoiceLine.JI_Procedure = inwardCusProcedure.ZZ6_ProcedureCode + inwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_BondedWhsQuantity = inwardInvoiceLine.JI_InvoiceQuantity;
				inwardInvoiceLine.JI_BondedWhsUnitQty = inwardInvoiceLine.JI_InvoiceUQ;
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				var warehouse2 = helper.Warehouse2;
				customsDetails.NewWarehouse = new OrganizationAddress { AddressType = AddressTypes.Warehouse1, AddressShortCode = warehouse2.MainAddress.OA_Code };
				((IOrganizationAddress)customsDetails.NewWarehouse).OrganizationCode = warehouse2.OH_Code;
				var data = helper.CreateChangeOfRegimeEntryData();
				var changeOfRegimeDeclaration = data.Declaration;
				changeOfRegimeDeclaration.JE_DeclarationReference = "BZA0005433";
				var changeOfRegimeEntryInstruction = data.Instruction;
				var changeOfRegimeInvoiceLine = data.InvoiceLine;
				changeOfRegimeInvoiceLine.JI_InvoiceQuantity = 60m;
				changeOfRegimeInvoiceLine.JI_InvoiceUQ = "NO";
				changeOfRegimeInvoiceLine.JI_CustomsUnitQty = "KG";
				changeOfRegimeInvoiceLine.JI_CustomsQuantity = 600m;
				changeOfRegimeInvoiceLine.JI_PreviousEntryNumber = "ENT32342";
				changeOfRegimeInvoiceLine.JI_PreviousEntryLineNumber = 1;
				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = changeOfRegimeInvoiceLine.JI_InvoiceQuantity;
				changeOfRegimeInvoiceLine.JI_BondedWhsUnitQty = changeOfRegimeInvoiceLine.JI_InvoiceUQ;
				Factory.Save();
				var inwardBondedEntryKey1 = "ENT32342-1";
				var changeOfRegimeBondedEntryKey1 = "ENT4234-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);

				var changeOfRegimeEntry = data.Entry;
				var result = changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegime(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = 50m;
				Factory.Save();
				changeOfRegimeEntry.PublishCancelEventForWHSChangeOfRegimeAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = 60m;
				Factory.Save();
				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegime(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = 70m;
				changeOfRegimeEntry.EntryNumber = "ENT4234";
				Factory.Save();
				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLastDataAndUpdateEntryDetailsAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 60m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeWithPreAmendmentData();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = 80m;
				Factory.Save();

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 60m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeWithPreAmendmentData();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = 70m;
				Factory.Save();

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLastHoldOrLatestDataAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 80m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 80m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeEntry.PublishCancelEventForWHSChangeOfRegimeAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled, changeOfRegimeEntry.CH_WarehouseTransactionStatus);
			}
		}

		public void TestChangeOfOwnershipPublishMethods()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, helper.InwardCusProcedure.ZZ6_ZZZ_NKDataGrouping);
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, helper.ChangeOfOwnershipCusProcedure.ZZ6_ZZZ_NKDataGrouping);
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;

				var (changeOfOwnershipDeclaration, changeOfOwnershipEntry) = GetNewChangeOfOwnershipDeclarationAndEntry(helper);
				var inwardBondedEntryKey1 = "ENT32342-1";
				var inwardBondedEntryKey2 = "ENT32342-2";
				var changeOfOwnershipBondedEntryKey1 = "ENT4234-1";
				var changeOfOwnershipBondedEntryKey2 = "ENT4234-2";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-2", 0m);

				var result = changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-2", 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				var changeOfOwnershipInvoiceLine1 = changeOfOwnershipDeclaration.InvoiceLines[0];
				var changeOfOwnershipInvoiceLine2 = changeOfOwnershipDeclaration.InvoiceLines[1];
				changeOfOwnershipInvoiceLine1.JI_BondedWhsQuantity = 50m;
				changeOfOwnershipInvoiceLine2.JI_BondedWhsQuantity = 180m;
				Factory.Save();
				changeOfOwnershipEntry.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-2", 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipInvoiceLine1.JI_BondedWhsQuantity = 60m;
				changeOfOwnershipInvoiceLine2.JI_BondedWhsQuantity = 150m;
				Factory.Save();
				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-2", 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipInvoiceLine1.JI_BondedWhsQuantity = 70m;
				changeOfOwnershipInvoiceLine2.JI_BondedWhsQuantity = 120m;
				changeOfOwnershipEntry.EntryNumber = "ENT4234";
				Factory.Save();
				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipFromLastDataAndUpdateEntryDetailsAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 60m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-2", 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipWithPreAmendmentData();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipInvoiceLine1.JI_BondedWhsQuantity = 80m;
				changeOfOwnershipInvoiceLine2.JI_BondedWhsQuantity = 90m;
				Factory.Save();

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 60m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipWithPreAmendmentData();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipInvoiceLine1.JI_BondedWhsQuantity = 70m;
				changeOfOwnershipInvoiceLine2.JI_BondedWhsQuantity = 120m;
				Factory.Save();

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipFromLastHoldOrLatestDataAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 210m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 80m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 90m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 210m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 210m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 80m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 90m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 20m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 210m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);

				changeOfOwnershipEntry.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);
			}
		}

		(BaseJobDeclaration, CusEntryHeader) GetNewChangeOfOwnershipDeclarationAndEntry(WhsDataTestHelper helper)
		{
			var ownerPart2 = helper.CreateProduct(helper.Owner.PK, helper.Part2.OP_PartNum);
			var inwardDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, "BZA0001232", "ENT32342", 100m);
			inwardDeclaration.JE_ApplicationCode = "BLT";
			inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			inwardDeclaration[ZAJobDeclarationSchema.JE_AGTCode.Name] = "ASBSD";
			inwardDeclaration.JE_CustomsOffice = "JHB";
			var inwardEntryInstruction = inwardDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inwardEntryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			inwardEntryInstruction.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var inwardEntry = inwardDeclaration.CustomsEntryHeaders[0];
			inwardEntry.CH_CEI_Instruction = inwardEntryInstruction.PK;
			var inwardInvoiceLine1 = inwardDeclaration.InvoiceLines[0];
			inwardInvoiceLine1.JI_CEI = inwardEntryInstruction.PK;
			inwardInvoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			inwardInvoiceLine1.JI_BondedWhsQuantity = inwardInvoiceLine1.JI_InvoiceQuantity;
			inwardInvoiceLine1.JI_BondedWhsUnitQty = inwardInvoiceLine1.JI_InvoiceUQ;
			var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
			inwardInvoiceLine2.JI_CEI = inwardEntryInstruction.PK;
			inwardInvoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			inwardInvoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
			inwardInvoiceLine2.JI_BondedWhsQuantity = 300m;
			inwardInvoiceLine2.JI_BondedWhsUnitQty = "NO";
			inwardDeclaration.DoMerge();
			Factory.Save();
			inwardEntry.PublishShipmentForWHSInward(false);
			inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

			var changeOfOwnershipDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "BZA0005433", "", 60m);
			changeOfOwnershipDeclaration.JE_ApplicationCode = "BLT";
			changeOfOwnershipDeclaration[ZAJobDeclarationSchema.JE_AGTCode.Name] = "ASBSD";
			changeOfOwnershipDeclaration.JE_CustomsOffice = "JHB";
			var changeOfOwnershipEntryInstruction = changeOfOwnershipDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			changeOfOwnershipEntryInstruction.CEI_Style = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			changeOfOwnershipEntryInstruction.CEI_OH_Owner = helper.Owner.PK;
			changeOfOwnershipEntryInstruction.CEI_OA_Warehouse = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			changeOfOwnershipEntryInstruction.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var changeOfOwnershipEntry = changeOfOwnershipDeclaration.CustomsEntryHeaders[0];
			changeOfOwnershipEntry.CH_CEI_Instruction = changeOfOwnershipEntryInstruction.PK;
			var changeOfOwnershipInvoiceLine1 = changeOfOwnershipDeclaration.InvoiceLines[0];
			changeOfOwnershipInvoiceLine1.JI_CEI = changeOfOwnershipEntryInstruction.PK;
			changeOfOwnershipInvoiceLine1.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			changeOfOwnershipInvoiceLine1.JI_PreviousEntryNumber = "ENT32342";
			changeOfOwnershipInvoiceLine1.JI_PreviousEntryLineNumber = 1;
			changeOfOwnershipInvoiceLine1.JI_BondedWhsQuantity = changeOfOwnershipInvoiceLine1.JI_InvoiceQuantity;
			changeOfOwnershipInvoiceLine1.JI_BondedWhsUnitQty = changeOfOwnershipInvoiceLine1.JI_InvoiceUQ;
			changeOfOwnershipInvoiceLine1[ZAJobComInvoiceLineSchema.JI_NewOwnerPartNo.Name] = helper.OwnerPart.OP_PartNum;
			var changeOfOwnershipInvoiceLine2 = changeOfOwnershipDeclaration.InvoiceLines.AddNew();
			changeOfOwnershipInvoiceLine2.JI_CEI = changeOfOwnershipEntryInstruction.PK;
			changeOfOwnershipInvoiceLine2.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			changeOfOwnershipInvoiceLine2.JI_PreviousEntryNumber = "ENT32342";
			changeOfOwnershipInvoiceLine2.JI_PreviousEntryLineNumber = 2;
			changeOfOwnershipInvoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
			changeOfOwnershipInvoiceLine2.JI_BondedWhsQuantity = 150m;
			changeOfOwnershipInvoiceLine2.JI_BondedWhsUnitQty = "NO";
			changeOfOwnershipInvoiceLine2[ZAJobComInvoiceLineSchema.JI_NewOwnerPartNo.Name] = ownerPart2.OP_PartNum;
			changeOfOwnershipDeclaration.DoMerge();
			Factory.Save();

			return (changeOfOwnershipDeclaration, changeOfOwnershipEntry);
		}

		public void TestHasWHSInwardTransactionAndNotCreatedPending()
		{
			using (CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime()))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				IWarehouseIntegrationSupporter supporter = entry;
				supporter.WarehouseTransactionStatus = ZString.Empty;
				AssertEquals(true, dec.IsWHSUniversalXMLActive);
				AssertEquals(false, supporter.HasWHSInwardTransactionAndNotCreatedPending());
				var list = new WarehouseTransactionStatusList();
				foreach (var code in new[]
				{
					WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
					WarehouseTransactionStatusList.Codes.InwardCreated,
					WarehouseTransactionStatusList.Codes.InwardUpdated,
					WarehouseTransactionStatusList.Codes.InwardUpdatedPending
				})
				{
					list.RemoveCode(code);
					supporter.WarehouseTransactionStatus = code;
					AssertEquals(code, true, supporter.HasWHSInwardTransactionAndNotCreatedPending());
				}
				foreach (CargoWise.Integration.ICodeDescription pair in list)
				{
					supporter.WarehouseTransactionStatus = pair.Code;
					AssertEquals(pair.Code, false, supporter.HasWHSInwardTransactionAndNotCreatedPending());
				}
			}
		}

		public void TestHasWHSOutwardTransactionAndNotCreatedPending()
		{
			using (CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime()))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				IWarehouseIntegrationSupporter supporter = entry;
				supporter.WarehouseTransactionStatus = ZString.Empty;
				AssertEquals(true, dec.IsWHSUniversalXMLActive);
				AssertEquals(false, supporter.HasWHSOutwardTransactionAndNotCreatedPending());
				var list = new WarehouseTransactionStatusList();
				foreach (var code in new[]
				{
					WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
					WarehouseTransactionStatusList.Codes.OutwardCreated,
					WarehouseTransactionStatusList.Codes.OutwardHolding,
					WarehouseTransactionStatusList.Codes.OutwardUpdated,
					WarehouseTransactionStatusList.Codes.OutwardUpdatedPending
				})
				{
					list.RemoveCode(code);
					supporter.WarehouseTransactionStatus = code;
					AssertEquals(code, true, supporter.HasWHSOutwardTransactionAndNotCreatedPending());
				}
				foreach (CargoWise.Integration.ICodeDescription pair in list)
				{
					supporter.WarehouseTransactionStatus = pair.Code;
					AssertEquals(pair.Code, false, supporter.HasWHSOutwardTransactionAndNotCreatedPending());
				}
			}
		}

		public void TestHasWHSChangeOfOwnershipTransactionAndNotCreatedPending()
		{
			using (CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime()))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				IWarehouseIntegrationSupporter supporter = entry;
				supporter.WarehouseTransactionStatus = ZString.Empty;
				AssertEquals(true, dec.IsWHSUniversalXMLActive);
				AssertEquals(false, supporter.HasWHSChangeOfOwnershipTransactionAndNotCreatedPending());
				var list = new WarehouseTransactionStatusList();
				foreach (var code in new[]
				{
					WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated,
					WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding,
					WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated,
					WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending
				})
				{
					list.RemoveCode(code);
					supporter.WarehouseTransactionStatus = code;
					AssertEquals(code, true, supporter.HasWHSChangeOfOwnershipTransactionAndNotCreatedPending());
				}
				foreach (CargoWise.Integration.ICodeDescription pair in list)
				{
					supporter.WarehouseTransactionStatus = pair.Code;
					AssertEquals(pair.Code, false, supporter.HasWHSChangeOfOwnershipTransactionAndNotCreatedPending());
				}
			}
		}

		public void TestHasWHSChangeOfRegimeTransactionAndNotCreatedPending()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			IWarehouseIntegrationSupporter supporter = entry;
			supporter.WarehouseTransactionStatus = ZString.Empty;
			AssertEquals(true, dec.IsWHSUniversalXMLActive);
			AssertEquals(false, supporter.HasWHSChangeOfRegimeTransactionAndNotCreatedPending());
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
				{
					WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreated,
					WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding,
					WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated,
					WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending
				})
			{
				list.RemoveCode(code);
				supporter.WarehouseTransactionStatus = code;
				AssertEquals(code, true, supporter.HasWHSChangeOfRegimeTransactionAndNotCreatedPending());
			}
			foreach (CargoWise.Integration.ICodeDescription pair in list)
			{
				supporter.WarehouseTransactionStatus = pair.Code;
				AssertEquals(pair.Code, false, supporter.HasWHSChangeOfRegimeTransactionAndNotCreatedPending());
			}
		}

		public void TestHasWHSTransaction()
		{
			using (CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime()))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				IWarehouseIntegrationSupporter supporter = entry;
				supporter.WarehouseTransactionStatus = ZString.Empty;
				AssertEquals(true, dec.IsWHSUniversalXMLActive);
				AssertEquals(false, supporter.HasWHSTransaction());
				var list = new WarehouseTransactionStatusList();
				list.RemoveCode(WarehouseTransactionStatusList.Codes.InwardCanceled);
				list.RemoveCode(WarehouseTransactionStatusList.Codes.OutwardCanceled);
				list.RemoveCode(WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled);
				list.RemoveCode(WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled);
				list.RemoveCode(WarehouseTransactionStatusList.Codes.AutomationIsDisabled);
				foreach (CargoWise.Integration.ICodeDescription pair in list)
				{
					supporter.WarehouseTransactionStatus = pair.Code;
					AssertEquals(pair.Code, true, supporter.HasWHSTransaction());
				}
				supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCanceled;
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceled, false, supporter.HasWHSTransaction());
				supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCanceled;
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, false, supporter.HasWHSTransaction());
				supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled;
				AssertEquals(WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled, false, supporter.HasWHSTransaction());
				supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled;
				AssertEquals(WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled, false, supporter.HasWHSTransaction());
				supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				AssertEquals(WarehouseTransactionStatusList.Codes.AutomationIsDisabled, false, supporter.HasWHSTransaction());
			}
		}

		public void TestPublishCancelEventForWHSOutwardInADifferentFactory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.JE_GoodsDescription = "HELLO";
			declaration.PublishCancelEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, declaration.HasChanges);
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));
			Factory.Save();
			declaration.JE_GoodsDescription = "BYE";
			declaration.PublishCancelEventForWHSOutwardInADifferentFactory();
			AssertEquals(true, declaration.HasChanges);
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob));
		}

		public void TestRestoreLatestClearedBondedWarehouseOutwardInADifferentFactory()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002133", "ENT324", 60m);
				outwardDeclaration.MessageInitiator = messageInitiator;
				var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward();
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				var exportLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
				AssertEquals(2, outwardDeclaration.Logs.Find(exportLogQuery).Length);

				outwardInvoiceLine.JI_InvoiceQuantity = 70m;
				outwardInvoiceLine.JI_LinePrice = 7000m;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 40m);
				AssertEquals(3, outwardDeclaration.Logs.Find(exportLogQuery).Length);

				outwardDeclaration.JE_GoodsDescription = "HELLO";
				outwardDeclaration.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory();
				AssertEquals(true, outwardDeclaration.HasChanges);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				outwardInvoiceLine.Reload();
				AssertEquals(70m, outwardInvoiceLine.JI_InvoiceQuantity);
				AssertEquals(5, outwardDeclaration.Logs.Find(exportLogQuery).Length);
			}
		}

		public void TestPublishCancelEventForWHSOutwardAndSaveIfNeeded()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
				Factory.Save();
				inwardDec.PublishShipmentForWHSInward(false);
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B012321312", "ENT865", 60m);
				var outwardInvoiceLine = declaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT865*WRL=1*";
				Factory.Save();
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 110m);
				declaration.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 50m);
				var result = declaration.PublishCancelEventForWHSOutwardAndSaveIfNeeded();
				var cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(false, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, declaration.WarehouseTransactionStatus);
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 110m);

				declaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B0893654555", "ENT865", 60m);
				outwardInvoiceLine = declaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT865*WRL=1*";
				Factory.Save();
				declaration.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 50m);

				var factory = new BusinessObjectFactory();
				declaration = factory.Load<BaseJobDeclaration>(declaration.PK);
				result = declaration.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
				cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(true, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, declaration.WarehouseTransactionStatus);
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 110m);
			}
		}

		public void TestPublishHoldEventForWHSOutwardAndSaveIfNeeded()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
				Factory.Save();
				inwardDec.PublishShipmentForWHSInward(false);
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B012321312", "ENT865", 60m);
				var outwardInvoiceLine = declaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT865*WRL=1*";
				Factory.Save();
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 110m);
				declaration.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 50m);
				var result = declaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded();
				var holdLog = declaration.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
				AssertEquals(false, holdLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, declaration.WarehouseTransactionStatus);

				var factory = new BusinessObjectFactory();
				declaration = factory.Load<BaseJobDeclaration>(declaration.PK);
				result = declaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("ENT865-1", 50m);
				holdLog = declaration.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
				AssertEquals(true, holdLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, declaration.WarehouseTransactionStatus);
			}
		}

		public void TestPublishAcceptEventForWHSOutwardAndSaveIfNeeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B012321312";
			Factory.Save();
			var result = declaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded();
			var acceptLog = declaration.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised);
			AssertEquals(false, acceptLog.IsInDatabase);
			declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);

			var factory = new BusinessObjectFactory();
			factory.Save();
			declaration = factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B0893654455";
			factory.Save();
			result = declaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
			acceptLog = declaration.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised);
			AssertEquals(true, acceptLog.IsInDatabase);
			declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
		}

		public void TestPublishShipmentForWHSOutward()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B012321312";
			Factory.Save();
			var result = declaration.PublishShipmentForWHSOutward(true);
			var exportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertEquals(true, exportLog.IsInDatabase);
			declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
		}

		public void TestPublishShipmentForWHSOutwardWithPreAmendmentData()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				var inwardEntryLine2 = inwardDeclaration.CustomsEntryHeaders[0].MergedLines.AddNew();
				inwardEntryLine2.CL_LineNumber = 2;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine2.JI_PartNo = "~~1";
				inwardInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardInvoiceLine2.JI_LinePrice = 20000m;
				inwardInvoiceLine2.JI_CL = inwardEntryLine2.PK;
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 20000m;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);
				AssertInventoryAvailabilityInActualDatabase("ENT324-2", 200m);

				var outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002134", "ENT324", 60m);
				var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				var outwardEntryLine2 = outwardDeclaration.CustomsEntryHeaders[0].MergedLines.AddNew();
				outwardEntryLine2.CL_LineNumber = 2;
				var outwardInvoiceLine2 = outwardDeclaration.InvoiceLines.AddNew();
				outwardInvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				outwardInvoiceLine2.JI_PartNo = "~~1";
				outwardInvoiceLine2.JI_InvoiceQuantity = 80m;
				outwardInvoiceLine2.JI_InvoiceUQ = "NO";
				outwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				outwardInvoiceLine2.JI_CustomsQuantity = 800m;
				outwardInvoiceLine2.JI_LinePrice = 8000m;
				outwardInvoiceLine2.JI_CL = outwardEntryLine2.PK;
				outwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 8000m;
				outwardInvoiceLine2.JI_AddInfo = "WRN=ENT324*WRL=2*";

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				outwardDeclaration.MessageInitiator = messageInitiator;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutward(true);
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertInventoryAvailabilityInActualDatabase("ENT324-2", 120m);

				outwardInvoiceLine.JI_InvoiceQuantity = 55m;
				outwardInvoiceLine.JI_LinePrice = 550m;
				outwardInvoiceLine2.JI_InvoiceQuantity = 90m;
				outwardInvoiceLine2.JI_LinePrice = 900m;
				outwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount = outwardInvoiceLine.JI_LinePrice + outwardInvoiceLine2.JI_LinePrice;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m); // Should keep previous quantity on hold
				AssertInventoryAvailabilityInActualDatabase("ENT324-2", 110m); // Should add 10 extra to hold
			}
		}

		public void TestNoExceptionInGetLastClearedOutwardUniversalShipmentWhenRecipientCollectionIsNull()
		{
			var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
			Factory.Save();
			inwardDeclaration.PublishShipmentForWHSInward(false);
			AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);
			Factory.Save();

			var exportLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
			var log = inwardDeclaration.Logs.Find(exportLogQuery);
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, log[0].PK);

			var relatedMessagePivot = Factory.LoadTop1<GenPivot>(query);
			var message1 = Factory.Load<EDIMessage>(relatedMessagePivot.XX_Relation2ID);
			var noRecipientRollCollection = @"UPDATE dbo.EDIMessage SET EM_MessageData = dbo.CLRCompressStringAsBytes('<?xml version=""1.0"" encoding=""utf-8""?>  <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">    <Shipment>      <DataContext>        <DataSourceCollection>          <DataSource>            <Type>CustomsDeclaration</Type>            <Key>B00002132</Key>          </DataSource>        </DataSourceCollection>          <Company>          <Code>EDI</Code>          <Country>            <Code>AU</Code>            <Name>Australia</Name>          </Country>          <Name>Eagle Datamation International</Name>        </Company>        <DataProvider>EDIDATEDI</DataProvider>        <EnterpriseID>EDI</EnterpriseID>        <EventBranch>          <Code>BNE</Code>          <Name>BN - AUBNE</Name>        </EventBranch>        <EventDepartment>          <Code>BRN</Code>          <Name>Branch</Name>        </EventDepartment>        <EventType>          <Code></Code>        </EventType>        <EventUser>          <Code>E</Code>          <Name>CargoWise Support</Name>        </EventUser>        <ServerID>DAT</ServerID>        <TriggerCount>1</TriggerCount>        <TriggerDate>2022-04-05T14:34:21.843</TriggerDate>        <TriggerDescription></TriggerDescription>        <TriggerType>Manual</TriggerType>          </DataContext>        <AdditionalTerms></AdditionalTerms>      <AgentsReference></AgentsReference>      <Branch>        <Code>BNE</Code>        <Name>BN - AUBNE</Name>      </Branch>      <CommercialInfo>        <Name>All Invoices</Name>          <AddInfoCollection>          <AddInfo>            <Key>BOMLineExpanded_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>EffectDutyDate_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>ExcisableGoods_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>ImporterToOrder_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>IncADJ_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>IsManualTILV_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>IsNonAQISAEPLine_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>IsPAYRECAck_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>IsSubjectToRedLine_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>NoPermitRequired_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>PrescribedGoods_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>SendZeroDutyOverride_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>SOFAIndicator_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>UPEIndicator_Hidden</Key>            <Value>N</Value>          </AddInfo>          <AddInfo>            <Key>VIS_Hidden</Key>            <Value>N</Value>          </AddInfo>        </AddInfoCollection>          <CommercialInvoiceCollection Content=""Complete"">          <CommercialInvoice>            <InvoiceNumber></InvoiceNumber>            <AdditionalTerms></AdditionalTerms>            <AgreedExchangeRate>1</AgreedExchangeRate>            <BillNumber></BillNumber>            <ExchangeRateType>              <Code></Code>            </ExchangeRateType>            <IncoTerm>              <Code></Code>            </IncoTerm>            <InvoiceAmount>11000</InvoiceAmount>            <InvoiceCurrency>              <Code>AUD</Code>              <Description>Australian Dollar</Description>            </InvoiceCurrency>            <InvoiceDate>2022-04-05T00:00:00</InvoiceDate>            <LandedCostExchangeRate>1</LandedCostExchangeRate>            <MessageStatus>              <Code></Code>            </MessageStatus>            <NetWeight>0</NetWeight>            <NetWeightUQ>              <Code>KG</Code>              <Description>Kilograms</Description>            </NetWeightUQ>            <NoOfPacks>0</NoOfPacks>            <PaymentAmount>0</PaymentAmount>            <PaymentDate></PaymentDate>            <PaymentExchangeRate>1</PaymentExchangeRate>            <PaymentNumber></PaymentNumber>            <RelatedIndicator>              <Code></Code>            </RelatedIndicator>            <ValuationCode>              <Code></Code>            </ValuationCode>            <ValuationDateOverride></ValuationDateOverride>            <Volume>0</Volume>            <VolumeUnit>              <Code></Code>            </VolumeUnit>            <Weight>0</Weight>            <WeightUnit>              <Code>KG</Code>              <Description>Kilograms</Description>            </WeightUnit>              <AddInfoCollection>              <AddInfo>                <Key>BOMLineExpanded_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>EffectDutyDate_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>ExcisableGoods_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>HeaderREL_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>ImporterToOrder_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>IncADJ_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>IsManualTILV_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>IsNonAQISAEPLine_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>IsPAYRECAck_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>IsSubjectToRedLine_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>NoPermitRequired_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>PrescribedGoods_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>SendZeroDutyOverride_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>SOFAIndicator_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>UPEIndicator_Hidden</Key>                <Value>N</Value>              </AddInfo>              <AddInfo>                <Key>VALB_Hidden</Key>                <Value>TV</Value>              </AddInfo>              <AddInfo>                <Key>VIS_Hidden</Key>                <Value>N</Value>              </AddInfo>            </AddInfoCollection>              <AddInfoGroupCollection>            </AddInfoGroupCollection>              <CommercialInvoiceLineCollection>              <CommercialInvoiceLine>                <LineNo>1</LineNo>                <BondedWarehouseQuantity>110</BondedWarehouseQuantity>                <BondedWarehouseQuantityUnit>                  <Code>NO</Code>                  <Description>Number</Description>                </BondedWarehouseQuantityUnit>                <BrandName></BrandName>                <ClassificationCode></ClassificationCode>                <ClassUsageComment></ClassUsageComment>                <Commodity>                  <Code></Code>                </Commodity>                <ConcessionOrder></ConcessionOrder>                <ContainerMode>                  <Code></Code>                </ContainerMode>                <CountryOfExport>                  <Code></Code>                </CountryOfExport>                <CountryOfOrigin>                  <Code></Code>                </CountryOfOrigin>                <CustomsQuantity>1100</CustomsQuantity>                <CustomsQuantityUnit>                  <Code>KG</Code>                </CustomsQuantityUnit>                <CustomsSecondQuantity>0</CustomsSecondQuantity>                <CustomsSecondQuantityUnit>                  <Code></Code>                </CustomsSecondQuantityUnit>                <CustomsThirdQuantity>0</CustomsThirdQuantity>                <CustomsThirdQuantityUnit>                  <Code></Code>                </CustomsThirdQuantityUnit>                <CustomsValue>11000</CustomsValue>                <DataImportMatchingKey></DataImportMatchingKey>                <Description>~~1 DESC</Description>                <EntryLineNumber>1</EntryLineNumber>                <EntryNumber>ENT324</EntryNumber>                <HarmonisedCode></HarmonisedCode>                <HazardousMaterial>                  <Code></Code>                  <CodeType>                    <Code></Code>                  </CodeType>                </HazardousMaterial>                <InvoiceQuantity>110</InvoiceQuantity>                <InvoiceQuantityUnit>                  <Code>NO</Code>                  <Description>Number</Description>                </InvoiceQuantityUnit>                <LinePrice>11000</LinePrice>                <Link>1</Link>                <LocalDescription></LocalDescription>                <Model></Model>                <NetWeight>0</NetWeight>                <NetWeightUnit>                  <Code>KG</Code>                  <Description>Kilograms</Description>                </NetWeightUnit>                <OrderLineLink>0</OrderLineLink>                <OrderNumber></OrderNumber>                <ParentLineNo>0</ParentLineNo>                <PartNo>~~1</PartNo>                <PreviousEntryLineNumber>0</PreviousEntryLineNumber>                <PreviousEntryNumber></PreviousEntryNumber>                <PrimaryPreference></PrimaryPreference>                <Procedure></Procedure>                <RelatedIndicator>                  <Code></Code>                </RelatedIndicator>                <SecondaryPreference></SecondaryPreference>                <StateOfOrigin>                  <Code></Code>                </StateOfOrigin>                <TaxType>                  <Code></Code>                </TaxType>                <UnitPrice>100</UnitPrice>                <ValuationCode>                  <Code></Code>                </ValuationCode>                <ValuationMarkup>0</ValuationMarkup>                <Volume>2.20</Volume>                <VolumeUnit>                  <Code>M3</Code>                  <Description>Cubic Meters</Description>                </VolumeUnit>                <Weight>220.0</Weight>                <WeightUnit>                  <Code>KG</Code>                  <Description>Kilograms</Description>                </WeightUnit>                  <AddInfoCollection>                  <AddInfo>                    <Key>BOMLineExpanded_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>EffectDutyDate_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>ExcisableGoods_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>ImporterToOrder_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>IncADJ_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>IsManualTILV_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>IsNonAQISAEPLine_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>IsPAYRECAck_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>IsSubjectToRedLine_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>NoPermitRequired_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>PrescribedGoods_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>SendZeroDutyOverride_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>SOFAIndicator_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>UPEIndicator_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                  <AddInfo>                    <Key>VIS_Hidden</Key>                    <Value>N</Value>                  </AddInfo>                </AddInfoCollection>                  <AddInfoGroupCollection>                </AddInfoGroupCollection>                  <CustomizedFieldCollection>                  <CustomizedField>                    <DataType>String</DataType>                    <Key>VIN1</Key>                    <Value></Value>                  </CustomizedField>                </CustomizedFieldCollection>              </CommercialInvoiceLine>            </CommercialInvoiceLineCollection>          </CommercialInvoice>        </CommercialInvoiceCollection>      </CommercialInfo>      <ConsolidatedCargoStatus>        <Code></Code>      </ConsolidatedCargoStatus>      <ContainerCount>0</ContainerCount>      <CustomsContainerMode>        <Code>FCL</Code>        <Description>Full Container Load</Description>      </CustomsContainerMode>      <CustomsOffice>        <Code></Code>      </CustomsOffice>      <CustomsProfileIdentifier>        <Type>UserName</Type>        <Value></Value>      </CustomsProfileIdentifier>      <CustomsValuationPort>        <Code></Code>      </CustomsValuationPort>      <DeclarantType>        <Code></Code>      </DeclarantType>      <DefermentAccountNumber></DefermentAccountNumber>      <EFTMode>        <Code></Code>      </EFTMode>      <EntryStatus>        <Code></Code>      </EntryStatus>      <ExportGoodsType>        <Code>OT</Code>        <Description>General Consigned Cargo</Description>      </ExportGoodsType>      <Folio></Folio>      <GoodsDescription></GoodsDescription>      <GoodsOrigin>        <Code></Code>      </GoodsOrigin>      <IsPersonalEffects>false</IsPersonalEffects>      <LloydsIMO></LloydsIMO>      <LocationAtClearance>        <Code></Code>        <Description></Description>      </LocationAtClearance>      <MergeBy>        <Code>NON</Code>        <Description>No Merge</Description>      </MergeBy>      <MessageStatus>        <Code></Code>        <Description>Not Sent</Description>      </MessageStatus>      <MessageSubType>        <Code>FRM</Code>        <Description>Formal Entry</Description>      </MessageSubType>      <MessageType>        <Code>IMP</Code>        <Description>Import</Description>      </MessageType>      <MessagingApplicationCode>        <Code>CMR</Code>        <Description>Send CMR Message</Description>      </MessagingApplicationCode>      <OperationalStatus>        <Code></Code>      </OperationalStatus>      <OuterPacks>0</OuterPacks>      <OuterPacksPackageType>        <Code>PKG</Code>        <Description>Package</Description>      </OuterPacksPackageType>      <OwnerRef></OwnerRef>      <PaidBy>        <Code></Code>      </PaidBy>      <PaymentMethod>        <Code>DEF</Code>        <Description>Default</Description>      </PaymentMethod>      <PortOfDestination>        <Code></Code>      </PortOfDestination>      <PortOfDischarge>        <Code></Code>      </PortOfDischarge>      <PortOfFirstArrival>        <Code></Code>      </PortOfFirstArrival>      <PortOfLoading>        <Code></Code>      </PortOfLoading>      <PortOfOrigin>        <Code></Code>      </PortOfOrigin>      <ScreeningStatus>        <Code>NOT</Code>        <Description>Not Screened</Description>      </ScreeningStatus>      <ServiceLevel>        <Code>STD</Code>        <Description>Standard</Description>      </ServiceLevel>      <ShipmentIncoTerm>        <Code>FOB</Code>        <Description>Free On Board</Description>      </ShipmentIncoTerm>      <SubLocationAtClearance>        <Code></Code>        <Description></Description>      </SubLocationAtClearance>      <TotalNoOfPacksDecimal>0</TotalNoOfPacksDecimal>      <TotalNoOfPieces>0</TotalNoOfPieces>      <TotalNoOfPiecesLanded>0</TotalNoOfPiecesLanded>      <TotalVolume>0</TotalVolume>      <TotalVolumeUnit>        <Code>M3</Code>        <Description>Cubic Meters</Description>      </TotalVolumeUnit>      <TotalWeight>0</TotalWeight>      <TotalWeightUnit>        <Code>KG</Code>        <Description>Kilograms</Description>      </TotalWeightUnit>      <TransportMode>        <Code></Code>      </TransportMode>      <TransportNationality>        <Code></Code>      </TransportNationality>      <VesselName></VesselName>      <VoyageFlightNo></VoyageFlightNo>      <WarehouseReleaseStatus>        <Code></Code>      </WarehouseReleaseStatus>        <LocalProcessing>        <ArrivalCartageRef></ArrivalCartageRef>        <DeliveryCartageAdvised></DeliveryCartageAdvised>        <DeliveryCartageCompleted></DeliveryCartageCompleted>        <DeliveryLabourCharge>0</DeliveryLabourCharge>        <DeliveryLabourTime></DeliveryLabourTime>        <DeliveryRequiredBy></DeliveryRequiredBy>        <DeliveryRequiredFrom></DeliveryRequiredFrom>        <DeliveryTruckWaitCharge>0</DeliveryTruckWaitCharge>        <DeliveryTruckWaitTime></DeliveryTruckWaitTime>        <DemurrageOnDeliveryCharge>0</DemurrageOnDeliveryCharge>        <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>        <DemurrageOnPickupCharge>0</DemurrageOnPickupCharge>        <DemurrageOnPickupTime></DemurrageOnPickupTime>        <EstimatedDelivery></EstimatedDelivery>        <EstimatedPickup></EstimatedPickup>        <ExportStatement>          <Code></Code>        </ExportStatement>        <FCLAvailable></FCLAvailable>        <FCLDeliveryDetentionCharge>0</FCLDeliveryDetentionCharge>        <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>        <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>        <FCLDeliveryEquipmentNeeded>          <Code>WUP</Code>          <Description>Wait for Pack/Unpack</Description>        </FCLDeliveryEquipmentNeeded>        <FCLPickupDetentionCharge>0</FCLPickupDetentionCharge>        <FCLPickupDetentionDays>0</FCLPickupDetentionDays>        <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>        <FCLPickupEquipmentNeeded>          <Code></Code>        </FCLPickupEquipmentNeeded>        <FCLStorageCommences></FCLStorageCommences>        <HasProhibitedPackaging>false</HasProhibitedPackaging>        <InsuranceRequired>false</InsuranceRequired>        <IsContingencyRelease>false</IsContingencyRelease>        <LCLAirStorageCharge>0</LCLAirStorageCharge>        <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>        <LCLAvailable></LCLAvailable>        <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>        <LCLStorageCommences></LCLStorageCommences>        <PickupCartageAdvised></PickupCartageAdvised>        <PickupCartageCompleted></PickupCartageCompleted>        <PickupLabourCharge>0</PickupLabourCharge>        <PickupLabourTime></PickupLabourTime>        <PickupRequiredBy></PickupRequiredBy>        <PickupRequiredFrom></PickupRequiredFrom>        <PickupTruckWaitCharge>0</PickupTruckWaitCharge>        <PickupTruckWaitTime></PickupTruckWaitTime>        <PrintOptionForPackagesOnAWB>          <Code>DEF</Code>          <Description>Default (Dims, fallback to Vol)</Description>        </PrintOptionForPackagesOnAWB>      </LocalProcessing>        <AddInfoCollection>        <AddInfo>          <Key>BOMLineExpanded_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>EffectDutyDate_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>ExcisableGoods_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>ImporterToOrder_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>IncADJ_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>IsManualTILV_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>IsNonAQISAEPLine_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>IsPAYRECAck_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>IsSubjectToRedLine_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>NoPermitRequired_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>PrescribedGoods_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>SendZeroDutyOverride_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>SOFAIndicator_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>UPEIndicator_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>VIS_Hidden</Key>          <Value>N</Value>        </AddInfo>        <AddInfo>          <Key>UseOwnerRefAsQuarantineRef</Key>          <Value>N</Value>        </AddInfo>      </AddInfoCollection>        <AdditionalReferenceCollection>      </AdditionalReferenceCollection>        <ContainerCollection>      </ContainerCollection>        <DateCollection>        <Date>          <Type>Departure</Type>          <IsEstimate>true</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>LoadingDate</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>FirstArrivalInCountry</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>DischargeDate</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>Arrival</Type>          <IsEstimate>true</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>EntrySubmitted</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>EntryAuthorisation</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>WarehouseRelease</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>        <Date>          <Type>EntryDate</Type>          <IsEstimate>false</IsEstimate>          <Value></Value>        </Date>      </DateCollection>        <EntryHeaderCollection>        <EntryHeader>          <Type>            <Code></Code>          </Type>          <Reference>B00002132/1</Reference>          <BondValidToDate></BondValidToDate>          <EntryReleaseDate></EntryReleaseDate>          <EntryStatus>            <Code></Code>          </EntryStatus>          <EntrySubmittedDate></EntrySubmittedDate>          <MessageStatus>            <Code></Code>            <Description>Not Sent</Description>          </MessageStatus>          <TotalAmountPaid>0</TotalAmountPaid>            <AddInfoCollection>            <AddInfo>              <Key>BOMLineExpanded_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>EffectDutyDate_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>ExcisableGoods_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>ImporterToOrder_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>IncADJ_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>IsManualTILV_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>IsNonAQISAEPLine_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>IsPAYRECAck_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>IsSubjectToRedLine_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>NoPermitRequired_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>PrescribedGoods_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>SendZeroDutyOverride_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>SOFAIndicator_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>UPEIndicator_Hidden</Key>              <Value>N</Value>            </AddInfo>            <AddInfo>              <Key>VIS_Hidden</Key>              <Value>N</Value>            </AddInfo>          </AddInfoCollection>            <EntryLineCollection>            <EntryLine>              <LineNumber>1</LineNumber>              <CustomsStatus>                <Code>ACT</Code>                <Description>Active</Description>              </CustomsStatus>              <CustomsValue>0</CustomsValue>              <Description></Description>              <DutyRateFlatAmount>0</DutyRateFlatAmount>              <DutyRateFlatAmountUnit>                <Code></Code>              </DutyRateFlatAmountUnit>              <DutyRatePercent>0</DutyRatePercent>              <HarmonisedCode></HarmonisedCode>                <AddInfoCollection>                <AddInfo>                  <Key>BOMLineExpanded_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>EffectDutyDate_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>ExcisableGoods_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>ImporterToOrder_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>IncADJ_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>IsManualTILV_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>IsNonAQISAEPLine_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>IsPAYRECAck_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>IsSubjectToRedLine_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>NoPermitRequired_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>PrescribedGoods_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>SendZeroDutyOverride_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>SOFAIndicator_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>UPEIndicator_Hidden</Key>                  <Value>N</Value>                </AddInfo>                <AddInfo>                  <Key>VIS_Hidden</Key>                  <Value>N</Value>                </AddInfo>              </AddInfoCollection>            </EntryLine>          </EntryLineCollection>            <EntryNumberCollection>            <EntryNumber>              <Type>                <Code>IMP</Code>                <Description>Import</Description>              </Type>              <Number>ENT324</Number>              <EntryIsSystemGenerated>true</EntryIsSystemGenerated>            </EntryNumber>          </EntryNumberCollection>        </EntryHeader>      </EntryHeaderCollection>        <OrganizationAddressCollection>        <OrganizationAddress>          <AddressType>CustomsWarehouseAddress</AddressType>          <Address1>W1 ADDRESS 1</Address1>          <Address2></Address2>          <AddressOverride>false</AddressOverride>          <AddressShortCode>W1 ADDRESS 1</AddressShortCode>          <City></City>          <CompanyName></CompanyName>          <Country>            <Code>US</Code>            <Name>United States</Name>          </Country>          <Email></Email>          <Fax></Fax>          <OrganizationCode>W1</OrganizationCode>          <Phone></Phone>          <Port>            <Code>AU2CO</Code>            <Name>Tyabb</Name>          </Port>          <Postcode></Postcode>          <ScreeningStatus>            <Code>NOT</Code>            <Description>Not Screened</Description>          </ScreeningStatus>          <State></State>            <RegistrationNumberCollection>            <RegistrationNumber>              <Type>                <Code>CCP</Code>                <Description>Customs Controlled Premises Code</Description>              </Type>              <CountryOfIssue>                <Code>AU</Code>                <Name>Australia</Name>              </CountryOfIssue>              <Value>23423</Value>            </RegistrationNumber>          </RegistrationNumberCollection>        </OrganizationAddress>        <OrganizationAddress>          <AddressType>ImporterDocumentaryAddress</AddressType>          <Address1>IMP ADDRESS 1</Address1>          <Address2></Address2>          <AddressOverride>false</AddressOverride>          <AddressShortCode>IMP ADDRESS 1</AddressShortCode>          <City></City>          <CompanyName>TestImp</CompanyName>          <Country>            <Code>AU</Code>            <Name>Australia</Name>          </Country>          <Email></Email>          <Fax></Fax>          <OrganizationCode>IMP</OrganizationCode>          <Phone></Phone>          <Port>            <Code></Code>          </Port>          <Postcode></Postcode>          <ScreeningStatus>            <Code>NOT</Code>            <Description>Not Screened</Description>          </ScreeningStatus>          <State></State>        </OrganizationAddress>        <OrganizationAddress>          <AddressType>ImporterPickupDeliveryAddress</AddressType>          <Address1>IMP ADDRESS 1</Address1>          <Address2></Address2>          <AddressOverride>false</AddressOverride>          <AddressShortCode>IMP ADDRESS 1</AddressShortCode>          <City></City>          <CompanyName>TestImp</CompanyName>          <Country>            <Code>AU</Code>            <Name>Australia</Name>          </Country>          <Email></Email>          <Fax></Fax>          <OrganizationCode>IMP</OrganizationCode>          <Phone></Phone>          <Port>            <Code></Code>          </Port>          <Postcode></Postcode>          <ScreeningStatus>            <Code>NOT</Code>            <Description>Not Screened</Description>          </ScreeningStatus>          <State></State>        </OrganizationAddress>      </OrganizationAddressCollection>    </Shipment>  </UniversalShipment> ')"
				+ @"WHERE EM_PK  = '" + message1.PK + @"'";

			using (var command = Db.Connection.Command(noRecipientRollCollection))
			{
				command.ExecuteNonQuery();
			}
			Factory.Save();

			AssertNoExceptionThrown(() => AssertNull(inwardDeclaration.GetLastClearedUniversalShipment(RecipientRoleType.BWI)));
		}

		public void TestGetLastClearedOutwardUniversalShipment()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002133", "ENT324", 60m);
				outwardDeclaration.MessageInitiator = messageInitiator;
				var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				Factory.Save();

				outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertNull(outwardDeclaration.GetLastClearedUniversalShipment(RecipientRoleType.BWR));

				outwardInvoiceLine.JI_InvoiceQuantity = 55m;
				outwardInvoiceLine.JI_LinePrice = 550m;
				Factory.Save();

				outwardDeclaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertNull(outwardDeclaration.GetLastClearedUniversalShipment(RecipientRoleType.BWR));

				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				var shipment = outwardDeclaration.GetLastClearedUniversalShipment(RecipientRoleType.BWR);
				AssertNotNull(shipment);
				var commercialInvoiceLine = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
				AssertEquals(60m, commercialInvoiceLine.BondedWarehouseQuantity.GetValueOrDefault());
			}
		}

		public void TestGetLastClearedUniversalShipmentWhenEventAndShipmentHaveSamePostedTimeUtc()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);

				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				var outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002133", "ENT324", 60m);
				outwardDeclaration.MessageInitiator = messageInitiator;
				var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				Factory.Save();

				outwardDeclaration.PublishShipmentForWHSOutward(true);
				var dex1 = outwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				var dex2 = outwardDeclaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				Assert("Should be different events", !object.ReferenceEquals(dex1, dex2));
				var postedTimeUtc = ZDateTime.UtcNow;
				((INeedRow)dex1).Row[StmALog.Schema.SL_PostedTimeUtc] = postedTimeUtc;
				dex1.HasChanges = true;
				((INeedRow)dex2).Row[StmALog.Schema.SL_PostedTimeUtc] = postedTimeUtc;
				dex2.HasChanges = true;
				Factory.Save();

				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				outwardInvoiceLine.JI_InvoiceQuantity = 55m;
				outwardInvoiceLine.JI_LinePrice = 550m;
				Factory.Save();

				outwardDeclaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);

				var testRunCount = 3;
				while (testRunCount-- > 0)
				{
					var newFactory = new BusinessObjectFactory();
					var outwardDeclarationInDiffFactory = newFactory.Load<BaseJobDeclaration>(outwardDeclaration.PK);
					var shipment = outwardDeclarationInDiffFactory.GetLastClearedUniversalShipment(RecipientRoleType.BWR);
					AssertNotNull(shipment);
					var commercialInvoiceLine = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0];
					AssertEquals(60m, commercialInvoiceLine.BondedWarehouseQuantity.GetValueOrDefault());
				}
			}
		}

		public void TestPublishAcceptEventForWHSInwardInADifferentFactory()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.MessageInitiator = messageInitiator;
				declaration.JE_GoodsDescription = "HELLO";
				declaration.PublishAcceptEventForWHSInwardInADifferentFactory();
				AssertEquals(true, declaration.HasChanges);
				AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised));
				Factory.Save();
				declaration.JE_GoodsDescription = "BYE";
				declaration.PublishAcceptEventForWHSInwardInADifferentFactory();
				AssertEquals(true, declaration.HasChanges);
				AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.WarehouseJobCanNowBeFinalised));
			}
		}

		public void TestPublishHoldEventForWHSInwardAndSaveIfNeeded()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B012321312", "ENT324", 110m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);
				Factory.Save();
				var result = declaration.PublishHoldEventForWHSInwardAndSaveIfNeeded();
				var cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
				AssertEquals(false, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);

				declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT325", 110m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				AssertInventoryAvailabilityInActualDatabase("ENT325-1", 110m);
				Factory.Save();
				var factory = new BusinessObjectFactory();
				declaration = factory.Load<BaseJobDeclaration>(declaration.PK);
				result = declaration.PublishHoldEventForWHSInwardAndSaveIfNeeded(true);
				cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.HoldTheWarehouseOrder);
				AssertEquals(true, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);
			}
		}

		public void TestPublishCancelEventForWHSInwardAndSaveIfNeeded()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B012321312", "ENT324", 110m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				Factory.Save();
				var result = declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				var cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(false, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);

				declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				Factory.Save();
				var factory = new BusinessObjectFactory();
				declaration = factory.Load<BaseJobDeclaration>(declaration.PK);
				result = declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true);
				cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(true, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);

				declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654456", "ENT568", 110m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				Factory.Save();
				factory = new BusinessObjectFactory();
				declaration = factory.Load<BaseJobDeclaration>(declaration.PK);
				result = declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
				cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);
				AssertEquals(true, cancelLog.IsInDatabase);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);

				WhsWarehouse.WW_IsVirtualWarehouse = true;
				Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002131", "ENT323", 100m);
				declaration.JE_GoodsDescription = "HELLO WORLD";
				Factory.Save();

				declaration.SendMessageWithBondedWarehouseAutomation(() => true, MessageAction.Original, Factory.Save);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				result = declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				cancelLog = declaration.Logs.MostRecentLogByEventTime(Events.CancelTheWarehouseJob);

				Assert("Event response result type", result.ResultType != UniversalResult.HadErrors);
				AssertNotNull(cancelLog);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);
			}
		}

		public void TestProcessInwardAmendment()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENTE234", 100m);
				Factory.Save();
				var entry = declaration.CustomsEntryHeaders[0];
				entry.PublishShipmentForWHSInward(false);
				entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("ENTE234-1", 100m);
				AssertEquals("CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, entry.CH_WarehouseTransactionStatus);
				var invoiceLine2 = declaration.InvoiceLines.AddNew();
				invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoiceLine2.JI_PartNo = Part2.OP_PartNum;
				invoiceLine2.JI_InvoiceQuantity = 200m;
				invoiceLine2.JI_InvoiceUQ = "NO";
				invoiceLine2.JI_BondedWhsQuantity = 200m;
				invoiceLine2.JI_BondedWhsUnitQty = "NO";
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_CustomsQuantity = 2000m;
				invoiceLine2.JI_LinePrice = 20000m;

				var entryLine2 = entry.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				invoiceLine2.JI_CL = entryLine2.PK;
				entry.ResetTotalsAndCachedValues();
				Factory.Save();
				entry.PublishShipmentForWHSInward(false);
				entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertInventoryAvailabilityInActualDatabase("ENTE234-1", 100m);
				AssertInventoryAvailabilityInActualDatabase("ENTE234-2", 200m);
				AssertEquals("CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
			}
		}

		public void TestPublishShipmentForWHSInward()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				declaration.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWI);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);
			}
		}

		public void TestHandleNotesNotSupportedInWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				declaration.JE_MarksAndNumbers = "MARKS AND NUMBERS";
				Factory.Save();
				var result = declaration.PublishShipmentForWHSInward(false);
				var receive = result.FindJobIfExists();
				var notes = receive.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				AssertEquals(1, notes.Length);
				AssertEquals(ZBool.True, notes[0].ST_IsCustomDescription);
			}
		}

		public void TestMessageHoldFunctionality()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				WhsWarehouse.WW_IsVirtualWarehouse = true;
				Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				var inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002131", "ENT323", 100m);
				inwardDeclaration.JE_GoodsDescription = "HELLO WORLD";
				Factory.Save();
				inwardDeclaration.SendMessageWithBondedWarehouseAutomation(() => false, MessageAction.Original, Factory.Save);
				AssertEquals("WarehouseTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);
				AssertInventoryAvailabilityInActualDatabase("ENT323-1", 0m);
				AssertNotNull(inwardDeclaration.GetLastHoldUniversalShipment(RecipientRoleType.BWI));
				var exportLogs = inwardDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
				AssertEquals("exportLogs.Length", 2, exportLogs.Length);
				AssertNull(inwardDeclaration.GetLastHoldUniversalShipmentFromNote());

				inwardDeclaration.SendMessageWithBondedWarehouseAutomation(() => true, MessageAction.Original, Factory.Save);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, inwardDeclaration.WarehouseTransactionStatus);
				AssertInventoryAvailabilityInActualDatabase("ENT323-1", 0m);
				var shipment = inwardDeclaration.GetLastHoldUniversalShipment(RecipientRoleType.BWI);
				AssertEquals("GoodsDescription", "HELLO WORLD", shipment.GoodsDescription);
				exportLogs = inwardDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
				AssertEquals("exportLogs.Length", 3, exportLogs.Length);
				AssertNotNull(inwardDeclaration.GetLastHoldUniversalShipmentFromNote());
			}
		}

		public void TestWarehouseEndToEndForVirtualWarehouse()
		{
			AssertWarehouseEndToEnd(true);
		}

		public void TestWarehouseEndToEndForRealWarehouse()
		{
			AssertWarehouseEndToEnd(false);
		}

		void AssertWarehouseEndToEnd(bool isVirtualWarehouse)
		{
			var helper = new WhsDataTestHelper(Factory);

			BaseJobDeclaration inwardDeclaration;
			BaseJobDeclaration outwardDeclaration;
			BaseJobComInvoiceLine inwardInvoiceLine;
			BaseJobComInvoiceLine outwardInvoiceLine;
			BaseJobComInvoiceHeader inwardInvoice;
			PublishToUniversalResult result;
			IWhsReceive whsReceive;
			IWhsReceive whsReceive2;
			SendsMessagesToCustomsShutterUpperer messageInitiator;

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				WhsWarehouse.WW_IsVirtualWarehouse = isVirtualWarehouse;
				Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002131", "ENT323", 1m);
				Factory.Save();

				// Force creation of warehouse
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				if (isVirtualWarehouse)
				{
					AssertInventoryAvailabilityInActualDatabase("ENT323-1", 1m);
					inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
					AssertInventoryAvailabilityInActualDatabase("ENT323-1", 0m);
				}
				else
				{
					inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				}

				inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002133", "ENT324", 60m);
				outwardDeclaration.MessageInitiator = messageInitiator;
				outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				Factory.Save();

				// Outward data created for missing inward
				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.HadErrors, result.ResultType);
				AssertContains("Order could not be created for Customs Job B00002133 because there are errors", result.ErrorMessage);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, outwardDeclaration.WarehouseTransactionStatus);

				// Inward data created for cleared message
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				if (isVirtualWarehouse)
				{
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 110m);
				}
				else
				{
					AssertEquals("W00000002", whsReceive.WD_DocketID);
					AssertEquals("ENT", whsReceive.WD_DocketStatus);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m); // Is still pending
					var inventories = GetWhsInventoryFromDatabase("ENT324-1");
					AssertEquals(1, inventories.Length);
					var inventory = inventories[0];
					AssertEquals(110m, inventory.WI_TotalUnits);
				}
				AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);

				// Inward data is updated with increase quantity
				inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				inwardInvoiceLine.JI_InvoiceQuantity = 120m;
				inwardInvoiceLine.JI_CustomsQuantity = 1200m;
				inwardInvoiceLine.JI_LinePrice = 12000m;
				inwardInvoice = inwardInvoiceLine.InvoiceHeader;
				inwardInvoice.JZ_InvoiceAmount = inwardInvoiceLine.JI_LinePrice;
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				if (isVirtualWarehouse)
				{
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 120m);
				}
				else
				{
					AssertEquals("W00000002", whsReceive.WD_DocketID);
					AssertEquals("ENT", whsReceive.WD_DocketStatus);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m); // Is still pending
					var inventories = GetWhsInventoryFromDatabase("ENT324-1");
					AssertEquals(1, inventories.Length);
					var inventory = inventories[0];
					AssertEquals(120m, inventory.WI_TotalUnits);
				}
				AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);

				// Inward data is updated with decrease quantity
				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_CustomsQuantity = 1000m;
				inwardInvoiceLine.JI_LinePrice = 10000m;
				inwardInvoice = inwardInvoiceLine.InvoiceHeader;
				inwardInvoice.JZ_InvoiceAmount = inwardInvoiceLine.JI_LinePrice;
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				if (isVirtualWarehouse)
				{
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 100m);
				}
				else
				{
					AssertEquals("W00000002", whsReceive.WD_DocketID);
					AssertEquals("ENT", whsReceive.WD_DocketStatus);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m); // Is still pending
					var inventories = GetWhsInventoryFromDatabase("ENT324-1");
					AssertEquals(1, inventories.Length);
					var inventory = inventories[0];
					AssertEquals(100m, inventory.WI_TotalUnits);
				}
				AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);

				// Inward data is cancelled
				result = inwardDeclaration.PublishCancelEventForWHSInwardAndSaveIfNeeded();
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive2 = (IWhsReceive)result.FindJobIfExists();
				AssertEquals(whsReceive.PK, whsReceive2.PK);
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000002", whsReceive2.WD_DocketID);
					AssertEquals("CAN", whsReceive2.WD_DocketStatus);
				}
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, inwardDeclaration.WarehouseTransactionStatus);

				// Inward data for outward testing
				inwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002134", "ENT324", 100m);
				Factory.Save();
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotEquals(whsReceive.PK, whsReceive2.PK);
			}

			if (isVirtualWarehouse)
			{
				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);
					inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				}
			}
			else
			{
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 0m); // Is still pending
				AssertEquals("WHSTransactionStatus", ZString.Empty, inwardDeclaration.WarehouseTransactionStatus);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive = Factory.Load<IWhsReceive>(whsReceive.PK);
				whsReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				WhsHelper.FinaliseDocketWithoutUserConfirmation(whsReceive.PK);
				Factory.Save();
			}
			AssertInventoryAvailabilityInActualDatabase("ENT324-1", 100m);
			AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				// Inward data is amended after being finalised
				inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
				inwardInvoiceLine.JI_InvoiceQuantity = 120m;
				inwardInvoiceLine.JI_CustomsQuantity = 1200m;
				inwardInvoiceLine.JI_LinePrice = 12000m;
				inwardInvoice = inwardInvoiceLine.InvoiceHeader;
				inwardInvoice.JZ_InvoiceAmount = inwardInvoiceLine.JI_LinePrice;
				Factory.Save();
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				if (isVirtualWarehouse)
				{
					AssertNotNull(whsReceive);
					AssertEquals(UniversalResult.Internal, result.ResultType);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 120m);
				}
				else
				{
					AssertNull(whsReceive);
					AssertEquals(UniversalResult.HadErrors, result.ResultType);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 100m);
					var inventories = GetWhsInventoryFromDatabase("ENT324-1");
					AssertEquals(2, inventories.Length);
					AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 0.000m, 100.000m }, inventories.Select(x => x.WI_TotalUnits));
				}
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);

				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_CustomsQuantity = 1000m;
				inwardInvoiceLine.JI_LinePrice = 10000m;
				Factory.Save();
				result = inwardDeclaration.PublishShipmentForWHSInward(false);
				whsReceive = (IWhsReceive)result.FindJobIfExists();
				if (isVirtualWarehouse)
				{
					AssertNotNull(whsReceive);
					AssertEquals(UniversalResult.Internal, result.ResultType);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 100m);
				}
				else
				{
					AssertNull(whsReceive);
					AssertEquals(UniversalResult.HadErrors, result.ResultType);
					AssertInventoryAvailabilityInActualDatabase("ENT324-1", 100m);
					var inventories = GetWhsInventoryFromDatabase("ENT324-1");
					AssertEquals(2, inventories.Length);
					AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 0.000m, 100.000m }, inventories.Select(x => x.WI_TotalUnits));
				}
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);

				// Outward data creation
				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 40m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data is set to be finalised
				result = outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 40m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data is updated with increase in quantity
				outwardInvoiceLine.JI_InvoiceQuantity = 75m;
				outwardInvoiceLine.JI_LinePrice = 7500m;
				Factory.Save();
				result = outwardDeclaration.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 25m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data is updated with decrease in quantity
				outwardDeclaration.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory();
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 40m);
				AssertEquals(75m, outwardInvoiceLine.JI_InvoiceQuantity);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data is updated with decrease in quantity
				outwardInvoiceLine.JI_InvoiceQuantity = 50m;
				outwardInvoiceLine.JI_LinePrice = 5000m;
				Factory.Save();
				result = outwardDeclaration.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 40m); // should still keep 60 on hold
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);

				// Outward amendment is cleared; need to publish current shipment follow by accept event
				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);

				result = outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data cancellation failure: Hold the data and send accept when Customs withdrawal is rejected
				result = outwardDeclaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				IWhsOrder whsOrder2 = null;
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder2 = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder2);
				if (!isVirtualWarehouse)
				{
					AssertEquals(whsOrder.PK, whsOrder2.PK);
					AssertEquals("W00000004", whsOrder2.WD_DocketID);
					AssertEquals("ATP", whsOrder2.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data hold is removed
				result = outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals(whsOrder.PK, whsOrder2.PK);
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data cancellation successful: Hold the data and send cancel when Customs withdrawal is accepted
				result = outwardDeclaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder2 = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder2);
				if (!isVirtualWarehouse)
				{
					AssertEquals(whsOrder.PK, whsOrder2.PK);
					AssertEquals("W00000004", whsOrder2.WD_DocketID);
					AssertEquals("ATP", whsOrder2.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);

				// Outward data hold is removed
				result = outwardDeclaration.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000004", whsOrder.WD_DocketID);
					AssertEquals("CAN", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 100m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardDeclaration.WarehouseTransactionStatus);

				// New outward data
				outwardDeclaration = GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "B00002135", "ENT324", 60m);
				outwardDeclaration.MessageInitiator = messageInitiator;
				outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT324*WRL=1*";
				Factory.Save();
				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				if (!isVirtualWarehouse)
				{
					AssertEquals("W00000005", whsOrder.WD_DocketID);
					AssertEquals("ATP", whsOrder.WD_DocketStatus);
				}
				AssertInventoryAvailabilityInActualDatabase("ENT324-1", 40m);
			}
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
