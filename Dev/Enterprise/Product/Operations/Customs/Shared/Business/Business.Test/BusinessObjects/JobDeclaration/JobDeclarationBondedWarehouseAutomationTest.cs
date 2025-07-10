using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationBondedWarehouseAutomationTest : TestCaseWithFactory
	{
		public void TestSendMessageWithBondedWarehouseAutomation_ChangeOfRegimeEndToEnd()
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
				var changeOfRegimeEntry = data.Entry;
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

				var sendMessageWasCalled = false;
				var sendMessageResult = false;
				Func<bool> sendMessage = () =>
				{
					sendMessageWasCalled = true;
					return sendMessageResult;
				};

				changeOfRegimeDeclaration.SendMessageWithBondedWarehouseAutomation(changeOfRegimeEntry, changeOfRegimeEntry.GetInventoryAutomationAction(), sendMessage, MessageAction.Original);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				var inwardBondedEntryKey1 = "ENT32342-1";
				var changeOfRegimeBondedEntryKey1 = "ENT4234-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);

				sendMessageWasCalled = false;
				sendMessageResult = true;
				changeOfRegimeDeclaration.SendMessageWithBondedWarehouseAutomation(changeOfRegimeEntry, changeOfRegimeEntry.GetInventoryAutomationAction(), sendMessage, MessageAction.Original);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
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

				sendMessageWasCalled = false;
				sendMessageResult = false;
				changeOfRegimeDeclaration.SendMessageWithBondedWarehouseAutomation(changeOfRegimeEntry, changeOfRegimeEntry.GetInventoryAutomationAction(), sendMessage, MessageAction.Amendment);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 60m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				AssertEquals("CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				sendMessageWasCalled = false;
				sendMessageResult = true;
				changeOfRegimeDeclaration.SendMessageWithBondedWarehouseAutomation(changeOfRegimeEntry, changeOfRegimeEntry.GetInventoryAutomationAction(), sendMessage, MessageAction.Amendment);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				changeOfRegimeInvoiceLine.JI_BondedWhsQuantity = 80m;
				Factory.Save();

				changeOfRegimeEntry.PublishShipmentForWHSChangeOfRegimeFromLastHoldOrLatestDataAndSaveIfNeeded();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 70m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				sendMessageWasCalled = false;
				sendMessageResult = false;
				changeOfRegimeDeclaration.SendMessageWithBondedWarehouseAutomation(changeOfRegimeEntry, changeOfRegimeEntry.GetInventoryAutomationAction(), sendMessage, MessageAction.Withdrawal);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 70m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0m);
				AssertEquals("CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated, changeOfRegimeEntry.CH_WarehouseTransactionStatus);

				sendMessageWasCalled = false;
				sendMessageResult = true;
				changeOfRegimeDeclaration.SendMessageWithBondedWarehouseAutomation(changeOfRegimeEntry, changeOfRegimeEntry.GetInventoryAutomationAction(), sendMessage, MessageAction.Withdrawal);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfRegimeBondedEntryKey1, 0m);
				AssertEquals("changeOfRegimeEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding, changeOfRegimeEntry.CH_WarehouseTransactionStatus);
			}
		}

		public void TestSendMessageWithBondedWarehouseAutomation_IsAutomationDisabled()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, "B00002132", "ENT324", 110m);
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				var entryHeader = declaration.ActiveEntryHeaders[0];
				entryHeader.CH_HasManualWhsUpdate = false;
				Factory.Save();
				AssertEquals("SupportsBondedWarehousing", true, declaration.SupportsBondedWarehousing);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);
				Assert("HasManualWhsUpdate", !((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);

				bool sendMessageWasCalled = false;
				Func<bool> sendMessage = () =>
				{
					sendMessageWasCalled = true;
					return true;
				};

				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Original);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);

				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Amendment);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);

				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Withdrawal);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);

				declaration.WarehouseTransactionStatus = ZString.Empty;
				Factory.Save();
				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Original);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);

				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Factory.Save();
				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Amendment);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);

				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Factory.Save();
				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Withdrawal);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);

				foreach (var code in new[]
				{
						WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending,
						WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding,
						WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending,
						WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending,
						WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding,
						WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending,
						WarehouseTransactionStatusList.Codes.InwardCreatedPending,
						WarehouseTransactionStatusList.Codes.InwardCreationHeld,
						WarehouseTransactionStatusList.Codes.InwardUpdatedPending,
						WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
						WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
						WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
						WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
						WarehouseTransactionStatusList.Codes.OutwardHolding
					})
				{
					AssertWhenTransactionStatusIsPending(code, declaration, sendMessage, sendMessageWasCalled);
				}

				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				entryHeader.CH_HasManualWhsUpdate = true;
				Factory.Save();
				Assert("HasManualWhsUpdate", ((IWarehouseIntegrationSupporter)declaration).HasManualWhsUpdate);
				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Withdrawal);
				AssertEquals("sendMessageCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus is not updated as AutomationIsDisabled due to HasManualWhsUpdate", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);

				declaration.WarehouseTransactionStatus = ZString.Empty;
				Factory.Save();
				sendMessageWasCalled = false;
				declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Original);
				AssertEquals("sendMessageWasCalled", true, sendMessageWasCalled);
				AssertEquals("WarehouseTransactionStatus is not updated as AutomationIsDisabled due to HasManualWhsUpdate", "", declaration.WarehouseTransactionStatus);
			}
		}

		void AssertWhenTransactionStatusIsPending(string code, BaseJobDeclaration declaration, Func<bool> sendMessage, bool sendMessageWasCalled)
		{
			declaration.WarehouseTransactionStatus = code;
			Factory.Save();
			sendMessageWasCalled = false;
			AssertExceptionThrown<ApplicationException>("There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.",
				() => declaration.SendMessageWithBondedWarehouseAutomation(declaration, declaration.GetInventoryAutomationAction(), sendMessage, MessageAction.Original));
			AssertEquals("sendMessageWasNotCalled", false, sendMessageWasCalled);
		}

		public void TestSendMessageWithBondedWarehouseAutomation_SupportModificationState()
		{
			bool sendMessageWasCalled = false;
			Func<bool> sendMessage = () =>
			{
				sendMessageWasCalled = true;
				return true;
			};

			var declaration = Factory.New<BaseJobDeclaration_SupportModificationState>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "B00001000";
			var whsWarehouse1 = PrepareWarehouseData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			Factory.Save();

			declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Original);

			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreationHeld;
			Factory.Save();
			sendMessageWasCalled = false;
			AssertExceptionThrown<ApplicationException>("There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.",
				() => declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Original));
			AssertEquals(false, sendMessageWasCalled);

			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreationHeld;
			Factory.Save();
			sendMessageWasCalled = false;
			declaration.SendMessageWithBondedWarehouseAutomation(sendMessage, MessageAction.Amendment);
			AssertEquals(true, sendMessageWasCalled);
		}

		public void TestShouldProcessWarehouse()
		{
			var declaration = Factory.New<BaseJobDeclaration_SupportModificationState>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var whsWarehouse1 = PrepareWarehouseData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			entry.CH_CEI_Instruction = instruction.PK;

			CombineAssertions(() =>
			{
				var whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Original);
				AssertEquals("Automation enabled", true, whsAutomation.ShouldProcessWarehouse());

				entry.CH_HasManualWhsUpdate = true;
				AssertEquals("Has manual update", false, whsAutomation.ShouldProcessWarehouse());

				entry.CH_HasManualWhsUpdate = false;
				declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				AssertEquals("Automation disabled", false, whsAutomation.ShouldProcessWarehouse());
			});
		}

		public void TestGetValidationMessage()
		{
			var declaration = Factory.New<BaseJobDeclaration_SupportModificationState>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var whsWarehouse1 = PrepareWarehouseData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Original) as IJobDeclarationBondedWarehouseAutomation;

			AssertEquals("No WHS", ZString.Empty, whsAutomation.GetPendingTransactionError());

			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			AssertEquals("No Pending transactions", ZString.Empty, whsAutomation.GetPendingTransactionError());

			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreationHeld;
			AssertContains("Pending transactions", "There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs.", whsAutomation.GetPendingTransactionError());
		}

		public void TestPrepareForProcessing()
		{
			var declaration = Factory.New<BaseJobDeclaration_SupportModificationState>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var whsWarehouse1 = PrepareWarehouseData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			entry.CH_CEI_Instruction = instruction.PK;

			var whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Original) as IJobDeclarationBondedWarehouseAutomation;
			AssertEquals("No PreAction - not bonded warehouse", false, whsAutomation.PrepareForProcessing(InventoryAutomationAction.Inward, false));

			whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Withdrawal);
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreationHeld;
			AssertEquals("WHS PreAction setup", true, whsAutomation.PrepareForProcessing(InventoryAutomationAction.Inward, false));
		}

		public void TestExecutePreAction()
		{
			var declaration = Factory.New<BaseJobDeclaration_SupportModificationState>();

			var actionCalled = false;
			var whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Original)
			{
				preAction = () =>
				{
					actionCalled = true;
					return PublishToUniversalResult.Empty;
				}
			};

			var result = ((IJobDeclarationBondedWarehouseAutomation)whsAutomation).ExecutePreAction();
			AssertEquals("Pre-action called", true, actionCalled);
			AssertEquals("Pre-action success", true, result);
			AssertEquals("Restore should be flagged", true, whsAutomation.needsToRestore);

			actionCalled = false;
			whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Original)
			{
				preAction = () =>
				{
					actionCalled = true;
					return PublishToUniversalResult.New("Some", "Error");
				}
			};

			result = ((IJobDeclarationBondedWarehouseAutomation)whsAutomation).ExecutePreAction();
			AssertEquals("Pre-action called", true, actionCalled);
			AssertEquals("Pre-action error", false, result);
		}

		public void TestExecuteRestoreAction()
		{
			var declaration = Factory.New<BaseJobDeclaration_SupportModificationState>();

			var actionCalled = false;
			var whsAutomation = new JobDeclarationBondedWarehouseAutomation(declaration, MessageAction.Original)
			{
				needsToRestore = true,
				restoreToPreState = () => { actionCalled = true; }
			} as IJobDeclarationBondedWarehouseAutomation;

			whsAutomation.ExecuteRestoreAction();
			AssertEquals("Restore-action called", true, actionCalled);

			actionCalled = false;
			whsAutomation.ExecuteRestoreAction();
			AssertEquals("Restore-action should not be called", false, actionCalled);
		}

		IWhsWarehouse PrepareWarehouseData()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				helper.Importer.MainAddress.OA_PostCode = "1234";
				helper.Importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "FRTVA1234");

				var whsWarehouse1 = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
				whsWarehouse1.WW_IsVirtualWarehouse = false;
				var whsRowA = helper.WhsHelper.CreateRowAndGenerateLocations(whsWarehouse1, "A");
				Factory.Save();

				var locationAPK = helper.WhsHelper.FindLocation(whsWarehouse1.PK, "A").PK;
				var receive = helper.GetNewWhsReceive(whsWarehouse1.PK, helper.Importer.PK);
				receive.WD_CustomsParentReference = "B00001000-EDIDATEDI";
				receive.WD_TotalUnits = 100m;
				var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
				receiveLine.WE_WL = locationAPK;
				var whsInventory = receiveLine.Inventory;
				Factory.Save();

				return whsWarehouse1;
			}
		}
	}
}
