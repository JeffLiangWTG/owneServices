using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class BondedWarehouseIntegrationEndToEndTest : BondedWarehouseIntegrationEndToEndTest<JobDeclaration, JobComInvoiceLine, OrgSupplierPart, CusClassification, CusClassPartPivot>
	{
		#region Change Of Ownership
		protected override bool ShouldTestChangeOfOwnership => true;

		protected override void AssertAdditionalChangeOfOwnershipInwardOnlyFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
		{
			AssertAdditionalChangeOfOwnershipFieldsValidation(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
		}

		protected override void AssertAdditionalChangeOfOwnershipOutwardOnlyFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
		{
			var outwardEntry = (CusEntryHeader)changeOfOwnershipJob;
			var entryInstruction = outwardEntry.EntryInstruction;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			changeOfOwnershipInvoiceLine.JI_BondedWhsQuantity = 0m;
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)changeOfOwnershipDeclaration.MessageInitiator;
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(changeOfOwnershipDeclaration);
			var messageSendingObject = objectParent.SendingObjectsCollection[0];
			var notification = new MessageNotificationCollector_ForTest();
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			messageSendingObject.ShouldSend = true;
			new MessageManager(objectParent, notification).SendMessages();
			var bondedWarehouseMessage = string.Format("Inventory recording/Bonded Warehouse Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to withdraw the stock that will be declared for Entry ({0}).", outwardEntry.EntryHeaderDescriptiveMenuItemText);
			AssertContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
			entryInstruction.CEI_OA_Warehouse = Helper.WhsWarehouse.WW_OA_WarehouseAddress;
			messageInitiator.InvalidOperationText = null;
			new MessageManager(objectParent, notification).SendMessages();
			AssertNotContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
		}

		protected override void AssertAdditionalChangeOfOwnershipFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var entryInstruction = entry.EntryInstruction;
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			changeOfOwnershipInvoiceLine.JI_BondedWhsQuantity = 0m;
			Factory.Save();
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)changeOfOwnershipDeclaration.MessageInitiator;
			messageInitiator.InvalidOperationText = null;
			var objectParent = new JobDeclarationMessageSendingObjectParent(changeOfOwnershipDeclaration);
			new MessageManager(objectParent, notification).SendMessages();
			var bondedWarehouseMessage = string.Format("Inventory recording/Bonded Warehouse Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared for Entry ({0}).", entry.EntryHeaderDescriptiveMenuItemText);
			AssertContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
			entryInstruction.CEI_OA_Warehouse2 = Helper.WhsWarehouse.WW_OA_WarehouseAddress;
			messageInitiator.InvalidOperationText = null;
			objectParent = new JobDeclarationMessageSendingObjectParent(changeOfOwnershipDeclaration);
			new MessageManager(objectParent, notification).SendMessages();
			AssertNotContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			Factory.Save();
			messageInitiator.InvalidOperationText = null;
			new MessageManager(objectParent, notification).SendMessages();
			AssertNotContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertNotContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
		}

		protected override void AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, ZString inwardBondedEntryKey, ZString inwardBondedEntryKey2, ZString changeOfOwnershipBondedEntryKey, ZString changeOfOwnershipBondedEntryKey2)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			if (entry.IsChangeOfOwnershipWarehousing)
			{
				AssertAdditionalBeforeOriginalResponseAutomationIsDisabled(changeOfOwnershipJob, originalMessage);
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending, entry.CH_WarehouseTransactionStatus);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
			}
			else if (entry.IsIntoWarehouseWarehousing)
			{
				AssertAdditionalForInwardBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage);
			}
			else if (entry.IsOutOfWarehouseWarehousing)
			{
				AssertAdditionalForOutwardBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2);
			}

			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
		}

		protected override void AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, ZString inwardBondedEntryKey, ZString inwardBondedEntryKey2, ZString changeOfOwnershipBondedEntryKey, ZString changeOfOwnershipBondedEntryKey2)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			if (entry.IsChangeOfOwnershipWarehousing)
			{
				AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				AssertAdditionalBeforeAmendmentResponseAutomationIsDisabled(changeOfOwnershipJob, amendmentMessage);
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending, entry.CH_WarehouseTransactionStatus);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
			}
			else if (entry.IsIntoWarehouseWarehousing)
			{
				AssertAdditionalForInwardBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage);
			}
			else if (entry.IsOutOfWarehouseWarehousing)
			{
				AssertAdditionalForOutwardBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2);
			}
		}

		protected override void AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, ZString inwardBondedEntryKey, ZString inwardBondedEntryKey2, ZString changeOfOwnershipBondedEntryKey, ZString changeOfOwnershipBondedEntryKey2)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			if (entry.IsChangeOfOwnershipWarehousing)
			{
				AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				AssertAdditionalBeforeWithdrawalResponseAutomationIsDisabled(changeOfOwnershipJob, withdrawalMessage);
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding, entry.CH_WarehouseTransactionStatus);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
			}
			else if (entry.IsIntoWarehouseWarehousing)
			{
				AssertAdditionalForInwardBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage);
			}
			else if (entry.IsOutOfWarehouseWarehousing)
			{
				AssertAdditionalForOutwardBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2);
			}
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + changeOfOwnershipDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				if (entry.IsChangeOfOwnershipBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Bonded Warehouse Change Of Ownership has been canceled.", email.Body);
				}
				else if (entry.IsOutwardBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Stock Release has been canceled. (WHS Order: ", email.Body);
				}
				else
				{
					AssertNotContains("Email Body", "(WHS ", email.Body);
				}
			});
		}

		protected override EDIMessage CreateChangeOfOwnershipOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess)
		{
			return CreateOriginalCUSRESEDIMessage(originalMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override EDIMessage SendChangeOfOwnershipOriginalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration)
		{
			return SendMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration, MessageSubTypeCodes.Codes.Original);
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "1", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '1'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 1 - Release', " + changeOfOwnershipDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				if (entry.IsChangeOfOwnershipBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Bonded Warehouse Change Of Ownership has been created.", email.Body);
				}
				else if (entry.IsOutwardBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Stock Release can be finalized. (WHS Order: ", email.Body);
				}
				else
				{
					AssertContains("Email Body", "Stock Levels have been updated. (WHS Receipt: ", email.Body);
				}
			});
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Amendment Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + changeOfOwnershipDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				if (entry.IsChangeOfOwnershipBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Previous Bonded Warehouse Change Of Ownership has been restored.", email.Body);
				}
				else if (entry.IsOutwardBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Previous Stock Release has been restored. (WHS Order: ", email.Body);
				}
				else
				{
					AssertContains("Email Body", "Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
				}
			});
		}

		protected override EDIMessage CreateChangeOfOwnershipAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess)
		{
			return CreateAmendmentCUSRESEDIMessage(amendmentMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override EDIMessage SendChangeOfOwnershipAmendmentMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration)
		{
			return SendMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration, MessageSubTypeCodes.Codes.Change);
		}

		protected override void SetupChangeOfOwnershipOriginalClearState(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
		{
			var outwardEntry = (CusEntryHeader)changeOfOwnershipJob;
			outwardEntry.EntryNumber = ZAWhsDataTestHelper.EntryNumber2;
			outwardEntry.CH_EntryStatus = "1";
			outwardEntry.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage("1000", "1", ZAWhsDataTestHelper.EntryNumber2, outwardEntry.CH_BGMReference); //Response 1 RELEASE
			originalCUSRESMessage.EM_LinkedObject = outwardEntry;
			originalCUSRESMessage.EM_Status = CUSRESEDIMessage.Status.ProcessedOK;
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Amendment Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "27", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '27'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 27 - Amendment granted', " + changeOfOwnershipDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				if (entry.IsChangeOfOwnershipBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Bonded Warehouse Change Of Ownership has been updated.", email.Body);
				}
				else if (entry.IsOutwardBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Stock Release has been updated. (WHS Order: ", email.Body);
				}
				else
				{
					AssertContains("Email Body", "Stock Levels have been updated. (WHS Receipt: ", email.Body);
				}
			});
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + changeOfOwnershipDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				if (entry.IsChangeOfOwnershipBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Previous Bonded Warehouse Change Of Ownership has been restored.", email.Body);
				}
				else if (entry.IsOutwardBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Stock Release can be finalized. (WHS Order: ", email.Body);
				}
				else
				{
					AssertContains("Email Body", "Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
				}
			});
		}

		protected override EDIMessage CreateChangeOfOwnershipWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess)
		{
			return CreateWithdrawalCUSRESEDIMessage(withdrawalMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override EDIMessage SendChangeOfOwnershipWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration)
		{
			return SendMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration, MessageSubTypeCodes.Codes.Cancellation);
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "28", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '28'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 28 - Cancellation granted', " + changeOfOwnershipDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				if (entry.IsChangeOfOwnershipBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Bonded Warehouse Change Of Ownership has been canceled.", email.Body);
				}
				else if (entry.IsOutwardBondedWarehousingEnabled)
				{
					AssertContains("Email Body", "Stock Release has been canceled. (WHS Order: ", email.Body);
				}
				else
				{
					AssertContains("Email Body", "Stock Levels Update has been canceled.", email.Body);
				}
			});
		}

		protected override IWarehouseIntegrationSupporter GetNewChangeOfOwnershipJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity1, ZDecimal quantity2)
		{
			var changeOfOwnershipCusProcedure = Helper.ChangeOfOwnershipCusProcedure;
			Helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, jobReference, changeOfOwnershipCusProcedure.ZZ6_ProcedureCode, inwardEntryKey, changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode, quantity1);
			var instruction = entry.EntryInstruction;
			instruction.CEI_OH_Owner = Helper.Owner.PK;
			var invoice = entry.RandomHeader;
			var invoiceLine1 = entry.RandomEntryLine.RandomLine;
			invoiceLine1.JI_NewOwnerPartNo = Helper.OwnerPart.OP_PartNum;
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, entry.CH_CEI_Instruction, changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode, inwardEntryKey, 2);
			invoiceLine2.JI_NewOwnerPartNo = Helper.OwnerPart2.OP_PartNum;
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			var declaration = entry.Declaration;
			declaration.DoMerge();
			return declaration.ActiveEntryHeaders[0];
		}

		protected override OrgHeader GetNewOwner(IWarehouseIntegrationSupporter changeOfOwnershipJob)
		{
			var entry = (CusEntryHeader)changeOfOwnershipJob;
			return entry.EntryInstruction.Owner;
		}
		#endregion

		#region Inward Testing
		protected override IWarehouseIntegrationSupporter GetNewInwardJob(ZString jobReference, ZDecimal quantity, ZDecimal quantity2)
		{
			var inwardCusProcedure = Helper.InwardCusProcedure;
			var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, jobReference, inwardCusProcedure.ZZ6_ProcedureCode, ZString.Empty, inwardCusProcedure.ZZ6_PreviousProcedureCode, quantity);
			var invoice = entry.RandomHeader;
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, entry.CH_CEI_Instruction, inwardCusProcedure.ZZ6_PreviousProcedureCode, ZString.Empty, ZShort.Zero);
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			var declaration = entry.Declaration;
			declaration.DoMerge();
			return declaration.ActiveEntryHeaders[0];
		}

		protected override EDIMessage SendInwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter iwnardJob, JobDeclaration inwardDeclaration)
		{
			return SendMessageViaMenu(iwnardJob, inwardDeclaration, MessageSubTypeCodes.Codes.Original);
		}

		protected override void AssertAdditionalInwardFieldsValidation(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2)
		{
			var entry = (CusEntryHeader)inwardJob;
			var entryInstruction = entry.EntryInstruction;
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			inwardInvoiceLine.JI_BondedWhsQuantity = 0m;
			Factory.Save();
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)inwardDeclaration.MessageInitiator;
			messageInitiator.InvalidOperationText = null;
			var objectParent = new JobDeclarationMessageSendingObjectParent(inwardDeclaration);
			new MessageManager(objectParent, notification).SendMessages();
			var bondedWarehouseMessage = string.Format("Inventory recording/Bonded Warehouse Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to receive the stock that will be declared for Entry ({0}).", entry.EntryHeaderDescriptiveMenuItemText);
			AssertContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
			entryInstruction.CEI_OA_Warehouse2 = Helper.WhsWarehouse.WW_OA_WarehouseAddress;
			messageInitiator.InvalidOperationText = null;
			objectParent = new JobDeclarationMessageSendingObjectParent(inwardDeclaration);
			new MessageManager(objectParent, notification).SendMessages();
			AssertNotContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			Factory.Save();
			messageInitiator.InvalidOperationText = null;
			new MessageManager(objectParent, notification).SendMessages();
			AssertNotContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertNotContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
		}

		public void TestHandleInwardReplacementMessage()
		{
			var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZString.Empty, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var entryInstruction = entry.EntryInstruction;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var inwardDeclaration = entry.Declaration;
			inwardDeclaration.DoMerge();
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(inwardDeclaration);
			var messageSendingObject = objectParent.SendingObjectsCollection[0];
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Replace;
			messageSendingObject.ShouldSend = true;
			messageSendingObject.LocalReferenceNumber = "00505655JSA20170228004185";
			messageSendingObject.MovementReferenceNumber = "JSA201702285000610";
			new MessageManager(objectParent, notification).SendMessages();
			var cusdecMessage = entry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
			AssertEquals("entry.CH_WarehouseTransactionStatus", ZString.Empty, entry.CH_WarehouseTransactionStatus);
			AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
		}

		public void TestHandleInwardOnOriginalControlError()
		{
			var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZString.Empty, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var inwardDeclaration = entry.Declaration;
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(inwardDeclaration);
			var messageSendingObject = objectParent.SendingObjectsCollection[0];
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			messageSendingObject.ShouldSend = true;
			new MessageManager(objectParent, notification).SendMessages();
			var cusdecMessage = entry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
			AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);
			AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
			Factory.Save();
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Control Rejected", () =>
			{
				contrlMessage.Reload();
				AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
				AssertEquals("contrlMessage.EM_LinkUniqueID", entry.PK, contrlMessage.EM_LinkUniqueID);
				entry.Reload();
				AssertEquals("entry.CH_WarehouseTransactionStatus", ZString.Empty, entry.CH_WarehouseTransactionStatus);
				AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Error, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertEquals("No email", 0, Env.AllEmailsCreated.Count());
			});
		}

		protected override void AssertAdditionalForInwardBeforeOriginalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage originalInwardMessage)
		{
			AssertAdditionalBeforeOriginalResponseAutomationIsDisabled(inwardJob, originalInwardMessage);
		}

		protected override void AssertAdditionalForInwardBeforeOriginalResponse(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage)
		{
			AssertAdditionalForInwardBeforeOriginalResponseAutomationIsDisabled(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalMessage);
			var entry = (CusEntryHeader)inwardJob;
			AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, entry.CH_WarehouseTransactionStatus);
		}

		protected override EDIMessage CreateInwardOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess)
		{
			return CreateOriginalCUSRESEDIMessage(originalMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber);
		}

		CUSRESEDIMessage CreateOriginalCUSRESEDIMessage(EDIMessage originalMessage, bool isSuccess, ZString entyNumber)
		{
			return Helper.CreateCUSRESEDIMessage(originalMessage.EM_MessageNum, isSuccess ? "1" : "6", entyNumber, (originalMessage.EM_LinkedObject as CusEntryHeader)?.CH_BGMReference ?? ZString.Empty); //Response 6 REJECT, 1 RELEASE
		}

		protected override void ProcessResponseAndAssertInwardOnOriginalError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + inwardDeclaration.JE_DeclarationReference;
				AssertNotNull("Email created", Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject));
			});
		}

		protected override void ProcessResponseAndAssertInwardOnOriginalClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "1", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '1'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 1 - Release', " + inwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Levels have been updated. (WHS Receipt: ", email.Body);
				var usxml = entry.GetLastClearedUniversalShipment(UniversalDataBuss.Integration.RecipientRoleType.BWI);
				var xmlEntry = usxml.EntryHeaderCollection[0];
				AssertEquals("XML has been updated with latest response data before being pushed to warehouse: status is '1'", "1", xmlEntry.EntryStatus.Code);
				AssertEquals("XML has been updated with latest response data before being pushed to warehouse: release date is 'today'", ZDateTime.Now.Date, xmlEntry.EntryReleaseDate);
			});
		}

		public void TestHandleInwardOnAmendmentControlError()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				var inwardDeclaration = entry.Declaration;
				Factory.Save();
				entry.CH_EntryStatus = "1";
				entry.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
				Factory.Save();
				entry.PublishShipmentForWHSInward(false);
				entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, entry.CH_WarehouseTransactionStatus);
				var bondedEntryKey = ZAWhsDataTestHelper.EntryNumber + "-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				var invoiceLine = inwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_BondedWhsQuantity = 200m;
				inwardDeclaration.DoMerge();
				var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage("1000", "1", ZAWhsDataTestHelper.EntryNumber, entry.CH_BGMReference); //Response 1 RELEASE
				originalCUSRESMessage.EM_LinkedObject = entry;
				originalCUSRESMessage.EM_Status = CUSRESEDIMessage.Status.ProcessedOK;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(inwardDeclaration);
				var messageSendingObject = objectParent.SendingObjectsCollection[0];
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
				messageSendingObject.ShouldSend = true;
				new MessageManager(objectParent, notification).SendMessages();
				var cusdecMessage = entry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, entry.CH_WarehouseTransactionStatus);
				AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 0m);
				var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
				Factory.Save();
				var logger = new LoggingInformationForTesting();
				Env.ClearAllEmailsCreated();
				new ZACIncomingMessageProcessor(logger).ExecuteBatch();
				CombineAssertions("Control Rejected", () =>
				{
					contrlMessage.Reload();
					AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
					AssertEquals("contrlMessage.EM_LinkUniqueID", entry.PK, contrlMessage.EM_LinkUniqueID);
					entry.Reload();
					AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
					AssertEquals("entry.CH_EntryStatus", "1", entry.CH_EntryStatus);
					AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Error, entry.CH_Status);
					AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
					AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", entry.CH_BGMReference), logger.LogMessages.ToString());
					var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Stock Levels Update for Entry: " + entry.EntryHeaderDescriptiveMenuItemText);
					AssertContains("Email Body", "Stock Levels have been updated. (WHS Receipt:", email.Body);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 200m);
				});
			}
		}

		protected override void SetupInwardOriginalClearState(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, bool createOriginalResponseMessage = true)
		{
			SetupOriginalClearState(inwardJob, ZAWhsDataTestHelper.EntryNumber);
		}

		protected override EDIMessage SendInwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter job, JobDeclaration inwardDeclaration)
		{
			return SendMessageViaMenu(job, inwardDeclaration, MessageSubTypeCodes.Codes.Change);
		}

		protected override void AssertAdditionalForInwardBeforeAmendmentResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage amendmentInwardMessage)
		{
			AssertAdditionalBeforeAmendmentResponseAutomationIsDisabled(inwardJob, amendmentInwardMessage);
		}

		void AssertAdditionalBeforeAmendmentResponseAutomationIsDisabled(IWarehouseIntegrationSupporter job, EDIMessage amendmentMessage)
		{
			var entry = (CusEntryHeader)job;
			var entryStatus = entry.CH_EntryStatus;
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			var contrlMessage = Helper.CreateCONTRLEDIMessage(amendmentMessage.EM_MessageNum, "7"); //Response 7 - Message Acknowledged
			Factory.Save();
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Control Acknowledged", () =>
			{
				contrlMessage.Reload();
				AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
				AssertEquals("contrlMessage.EM_LinkUniqueID", entry.PK, contrlMessage.EM_LinkUniqueID);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", entryStatus, entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ACK'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertEquals("No email", 0, Env.AllEmailsCreated.Count());
			});
		}

		protected override void AssertAdditionalForInwardBeforeAmendmentResponse(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
			AssertAdditionalForInwardBeforeAmendmentResponseAutomationIsDisabled(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentMessage);
			AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, entry.CH_WarehouseTransactionStatus);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(GetEntryKey(entry) + "-1", 0m);
		}

		protected override EDIMessage CreateInwardAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess)
		{
			return CreateAmendmentCUSRESEDIMessage(amendmentMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber);
		}

		CUSRESEDIMessage CreateAmendmentCUSRESEDIMessage(EDIMessage originalMessage, bool isSuccess, ZString entryNumber)
		{
			return Helper.CreateCUSRESEDIMessage(originalMessage.EM_MessageNum, isSuccess ? "27" : "6", entryNumber, (originalMessage.EM_LinkedObject as CusEntryHeader)?.CH_BGMReference ?? ZString.Empty); //Response 6 REJECT, 27 AMENDMENT GRANT
		}

		protected override void ProcessResponseAndAssertInwardOnAmendmentError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Amendment Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + inwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
			});
		}

		protected override void ProcessResponseAndAssertInwardOnAmendmentClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Amendment Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "27", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '27'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 27 - Amendment granted', " + inwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Levels have been updated. (WHS Receipt: ", email.Body);
			});
		}

		public void TestHandleInwardOnWithdrawalControlError()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				var inwardDeclaration = entry.Declaration;
				entry.CH_EntryStatus = "1";
				entry.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
				Factory.Save();
				entry.PublishShipmentForWHSInward(false);
				entry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, entry.CH_WarehouseTransactionStatus);
				var bondedEntryKey = ZAWhsDataTestHelper.EntryNumber + "-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage("1000", "1", ZAWhsDataTestHelper.EntryNumber, entry.CH_BGMReference); //Response 1 RELEASE
				originalCUSRESMessage.EM_LinkedObject = entry;
				originalCUSRESMessage.EM_Status = CUSRESEDIMessage.Status.ProcessedOK;
				var invoiceLine = inwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_BondedWhsQuantity = 200m; // withdrawal should not change inventory quantity
				inwardDeclaration.DoMerge();
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(inwardDeclaration);
				var messageSendingObject = objectParent.SendingObjectsCollection[0];
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				messageSendingObject.ShouldSend = true;
				new MessageManager(objectParent, notification).SendMessages();
				var cusdecMessage = entry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
				AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, entry.CH_WarehouseTransactionStatus);
				AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 0m);
				var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
				Factory.Save();
				var logger = new LoggingInformationForTesting();
				Env.ClearAllEmailsCreated();
				new ZACIncomingMessageProcessor(logger).ExecuteBatch();
				CombineAssertions("Control Rejected", () =>
				{
					contrlMessage.Reload();
					AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
					AssertEquals("contrlMessage.EM_LinkUniqueID", entry.PK, contrlMessage.EM_LinkUniqueID);
					entry.Reload();
					AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, entry.CH_WarehouseTransactionStatus);
					AssertEquals("entry.CH_EntryStatus", "1", entry.CH_EntryStatus);
					AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Error, entry.CH_Status);
					AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
					AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", entry.CH_BGMReference), logger.LogMessages.ToString());
					var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Restored Previous Stock Levels for Entry: " + entry.EntryHeaderDescriptiveMenuItemText);
					AssertContains("Email Body", "Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				});
			}
		}

		public void TestProcessWithdrawalMessageWhereBondedWarehouseAutomationWasChangedAfterOriginalMessage()
		{
			Helper.UniversalTariffHelper.CreateCustomsStatusCusCodeEntry("8");
			Factory.Save();
			var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var inwardDeclaration = entry.Declaration;
			var importer = Helper.Importer;
			importer.CompanyData.OB_IMUsedBondedWhs = false;
			var warehouse = Helper.WhsWarehouse.WarehouseAddress.Header;
			warehouse.CompanyData.OB_IMUsedBondedWhs = false;
			Factory.Save();
			var originalCUSDECMessage = SendInwardOriginalMessageViaMenu(entry, inwardDeclaration);
			AssertEquals("entry.CH_WarehouseTransactionStatus", ZString.Empty, entry.CH_WarehouseTransactionStatus);
			AssertNull("No DEX for inward original", entry.Logs.MostRecentLogByEventTime(Events.DataExport));
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(ZAWhsDataTestHelper.EntryNumber, 1, 0m);
			AssertNull(entry.GetLastHoldUniversalShipmentFromNote());
			var originalCONTRLMessage = Helper.CreateCONTRLEDIMessage(originalCUSDECMessage.EM_MessageNum, "7"); //Response 7 Ready For Cash Payment
			Factory.Save();
			var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage(originalCUSDECMessage.EM_MessageNum, "8", ZAWhsDataTestHelper.EntryNumber, entry.CH_BGMReference); //Response 8 Proceed to Border (SACU clearances)
			Factory.Save();
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original", () =>
			{
				originalCONTRLMessage.Reload();
				AssertEquals("originalCONTRLMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, originalCONTRLMessage.EM_Status);
				AssertEquals("originalCONTRLMessage.EM_LinkUniqueID", entry.PK, originalCONTRLMessage.EM_LinkUniqueID);
				originalCUSRESMessage.Reload();
				AssertEquals("originalCUSRESMessage.EM_Status", CUSRESEDIMessage.Status.ProcessedOK, originalCUSRESMessage.EM_Status);
				AssertEquals("originalCUSRESMessage.EM_LinkUniqueID", entry.PK, originalCUSRESMessage.EM_LinkUniqueID);
				entry.Reload();
				AssertEquals("entry.CH_WarehouseTransactionStatus", ZString.Empty, entry.CH_WarehouseTransactionStatus);
				AssertEquals("entry.CH_EntryStatus", "8", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(ZAWhsDataTestHelper.EntryNumber, 1, 0m);
				AssertNull(entry.GetLastHoldUniversalShipmentFromNote());
				AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", originalCONTRLMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ACK'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", originalCUSRESMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '8'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 8 - Proceed to Border (SACU clearances)', " + inwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertNotContains("Email Body", "(WHS ", email.Body);
			});
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			Factory.Save();
			var withdrawalCUSDECMessage = SendInwardWithdrawalMessageViaMenu(entry, inwardDeclaration);
			CombineAssertions("Withdrawal Message", () =>
			{
				AssertEquals("entry.CH_WarehouseTransactionStatus", ZString.Empty, entry.CH_WarehouseTransactionStatus);
				AssertEquals("entry.CH_EntryStatus", "", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
				AssertNull("No DEX for inward original", entry.Logs.MostRecentLogByEventTime(Events.DataExport));
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(ZAWhsDataTestHelper.EntryNumber, 1, 0m);
				AssertNull(entry.GetLastHoldUniversalShipmentFromNote());
			});
			var withdrawalCONTRLMessage = Helper.CreateCONTRLEDIMessage(withdrawalCUSDECMessage.EM_MessageNum, "7"); //Response 7 Ready For Cash Payment
			Factory.Save();
			var withdrawalCUSRESMessage = Helper.CreateCUSRESEDIMessage(withdrawalCUSDECMessage.EM_MessageNum, "6", ZAWhsDataTestHelper.EntryNumber, entry.CH_BGMReference); //Response 6 Reject To Clearer
			Factory.Save();
			logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal", () =>
			{
				withdrawalCONTRLMessage.Reload();
				AssertEquals("withdrawalCONTRLMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, withdrawalCONTRLMessage.EM_Status);
				AssertEquals("withdrawalCONTRLMessage.EM_LinkUniqueID", entry.PK, withdrawalCONTRLMessage.EM_LinkUniqueID);
				withdrawalCUSRESMessage.Reload();
				AssertEquals("withdrawalCUSRESMessage.EM_Status", CUSRESEDIMessage.Status.ProcessedOK, withdrawalCUSRESMessage.EM_Status);
				AssertEquals("withdrawalCUSRESMessage.EM_LinkUniqueID", entry.PK, withdrawalCUSRESMessage.EM_LinkUniqueID);
				entry.Reload();
				AssertEquals("entry.CH_WarehouseTransactionStatus", ZString.Empty, entry.CH_WarehouseTransactionStatus);
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertNull("No DEX for inward original", entry.Logs.MostRecentLogByEventTime(Events.DataExport));
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(ZAWhsDataTestHelper.EntryNumber, 1, 0m);
				AssertNull(entry.GetLastHoldUniversalShipmentFromNote());
				AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", withdrawalCONTRLMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ACK'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", withdrawalCUSRESMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject2 = "Entry Notification: 'Code 6 - Reject To Clearer', " + inwardDeclaration.JE_DeclarationReference;
				var email2 = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject2);
				AssertNotContains("Email Body", "(WHS ", email2.Body);
			});
		}

		protected override EDIMessage SendInwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration)
		{
			return SendMessageViaMenu(inwardJob, inwardDeclaration, MessageSubTypeCodes.Codes.Cancellation);
		}

		protected override void AssertAdditionalForInwardBeforeWithdrawalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalInwardMessage)
		{
			AssertAdditionalBeforeWithdrawalResponseAutomationIsDisabled(inwardJob, withdrawalInwardMessage);
		}

		void AssertAdditionalBeforeWithdrawalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, EDIMessage withdrawalMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var entryStatus = entry.CH_EntryStatus;
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			var contrlMessage = Helper.CreateCONTRLEDIMessage(withdrawalMessage.EM_MessageNum, "7"); //Response 7 - Message Acknowledged
			Factory.Save();
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Control Acknowledged", () =>
			{
				contrlMessage.Reload();
				AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
				AssertEquals("contrlMessage.EM_LinkUniqueID", entry.PK, contrlMessage.EM_LinkUniqueID);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", entryStatus, entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ACK'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertEquals("No email", 0, Env.AllEmailsCreated.Count());
			});
		}

		protected override void AssertAdditionalForInwardBeforeWithdrawalResponse(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
			AssertAdditionalForInwardBeforeWithdrawalResponseAutomationIsDisabled(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalMessage);
			AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, entry.CH_WarehouseTransactionStatus);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(GetEntryKey(entry) + "-1", 0m);
		}

		protected override EDIMessage CreateInwardWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess)
		{
			return CreateWithdrawalCUSRESEDIMessage(withdrawalMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber);
		}

		CUSRESEDIMessage CreateWithdrawalCUSRESEDIMessage(EDIMessage originalMessage, bool isSuccess, ZString entryNumber)
		{
			return Helper.CreateCUSRESEDIMessage(originalMessage.EM_MessageNum, isSuccess ? "28" : "6", entryNumber, (originalMessage.EM_LinkedObject as CusEntryHeader)?.CH_BGMReference ?? ZString.Empty); //Response 6 REJECT, 28 CANCELLATION GRANTED
		}

		protected override void ProcessResponseAndAssertInwardOnWithdrawalError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "6", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + inwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Previous Stock Levels have been restored. (WHS Receipt: ", email.Body);
			});
		}

		protected override void ProcessResponseAndAssertInwardOnWithdrawalClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			var entry = (CusEntryHeader)inwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", "28", entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '28'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 28 - Cancellation granted', " + inwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Levels Update has been canceled.", email.Body);
			});
		}
		#endregion

		#region Outward Testing
		protected override IWarehouseIntegrationSupporter GetNewOutwardJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity, ZDecimal quantity2)
		{
			var outwardCusProcedure = Helper.OutwardCusProcedure;
			var entry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, jobReference, outwardCusProcedure.ZZ6_ProcedureCode, inwardEntryKey, outwardCusProcedure.ZZ6_PreviousProcedureCode, quantity);
			var invoice = entry.RandomHeader;
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, entry.CH_CEI_Instruction, outwardCusProcedure.ZZ6_PreviousProcedureCode, inwardEntryKey, 2);
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			var declaration = entry.Declaration;
			declaration.DoMerge();
			return declaration.ActiveEntryHeaders[0];
		}

		protected override EDIMessage SendOutwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration)
		{
			return SendMessageViaMenu(outwardJob, outwardDeclaration, MessageSubTypeCodes.Codes.Original);
		}

		protected override void AssertAdditionalOutwardFieldsValidation(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var entryInstruction = outwardEntry.EntryInstruction;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			outwardInvoiceLine.JI_BondedWhsQuantity = 0m;
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)outwardDeclaration.MessageInitiator;
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(outwardDeclaration);
			var messageSendingObject = objectParent.SendingObjectsCollection[0];
			var notification = new MessageNotificationCollector_ForTest();
			messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
			messageSendingObject.ShouldSend = true;
			new MessageManager(objectParent, notification).SendMessages();
			var bondedWarehouseMessage = string.Format("Inventory recording/Bonded Warehouse Integration is active for Importer 'IMP'. Please enter a Bonded Warehouse that will be used to withdraw the stock that will be declared for Entry ({0}).", outwardEntry.EntryHeaderDescriptiveMenuItemText);
			AssertContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
			entryInstruction.CEI_OA_Warehouse = Helper.WhsWarehouse.WW_OA_WarehouseAddress;
			messageInitiator.InvalidOperationText = null;
			new MessageManager(objectParent, notification).SendMessages();
			AssertNotContains(bondedWarehouseMessage, messageInitiator.InvalidOperationText);
			AssertContains("An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.", messageInitiator.InvalidOperationText);
		}

		public void TestHandleOutwardOnControlError()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (Helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				var inwardDeclaration = inwardEntry.Declaration;
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("inwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardEntry.CH_WarehouseTransactionStatus);
				var bondedEntryKey = ZAWhsDataTestHelper.EntryNumber + "-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				var outwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001240", Helper.OutwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 60m);
				var outwardDeclaration = outwardEntry.Declaration;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(outwardDeclaration);
				var messageSendingObject = objectParent.SendingObjectsCollection[0];
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
				messageSendingObject.ShouldSend = true;
				new MessageManager(objectParent, notification).SendMessages();
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				var cusdecMessage = outwardEntry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
				AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, outwardEntry.CH_Status);
				var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
				Factory.Save();
				var logger = new LoggingInformationForTesting();
				Env.ClearAllEmailsCreated();
				new ZACIncomingMessageProcessor(logger).ExecuteBatch();
				CombineAssertions("Control Rejected", () =>
				{
					contrlMessage.Reload();
					AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
					AssertEquals("contrlMessage.EM_LinkUniqueID", outwardEntry.PK, contrlMessage.EM_LinkUniqueID);
					outwardEntry.Reload();
					AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
					AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Error, outwardEntry.CH_Status);
					AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Canceled Stock Release for Entry: " + outwardEntry.EntryHeaderDescriptiveMenuItemText);
					AssertContains("Email Body", "Stock Release has been canceled. (WHS Order: ", email.Body);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				});
			}
		}

		public void TestHandleOutwardOnControlError_HasNoDataContext_InvalidOperationException()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (Helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				var inwardDeclaration = inwardEntry.Declaration;
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("inwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardEntry.CH_WarehouseTransactionStatus);
				var bondedEntryKey = ZAWhsDataTestHelper.EntryNumber + "-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				var outwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001240", Helper.OutwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 60m);
				var outwardDeclaration = outwardEntry.Declaration;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(outwardDeclaration);
				var messageSendingObject = objectParent.SendingObjectsCollection[0];
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;
				messageSendingObject.ShouldSend = true;
				new MessageManager(objectParent, notification).SendMessages();
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				var cusdecMessage = outwardEntry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
				AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, outwardEntry.CH_Status);
				var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
				cusdecMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
				Factory.Save();
				var logger = new LoggingInformationForTesting();
				Env.ClearAllEmailsCreated();
				new ZACIncomingMessageProcessor(logger).ExecuteBatch();
				CombineAssertions("Control Rejected", () =>
				{
					AssertEquals("Logger should indicate reason for failure", expected: false, logger.Logs.Any(x => x.Message.Contains("Data Context for the WarehouseCustomsEntry data object was empty. The Data Context should always be created by the top level data object.")));
					AssertEquals("Message retry", 0, logger.Logs.Count(x => System.Text.RegularExpressions.Regex.IsMatch(x.Message, @"Data Context for the WarehouseCustomsEntry data object was empty\.")));
					contrlMessage.Reload();
					AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
					AssertEquals("contrlMessage.EM_LinkUniqueID", outwardEntry.PK, contrlMessage.EM_LinkUniqueID);
					outwardEntry.Reload();
					AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
					AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Error, outwardEntry.CH_Status);
					AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Updated Stock Release for Entry: " + outwardEntry.EntryHeaderDescriptiveMenuItemText);
					AssertContains("Email Body", "Updated Stock Release for Entry: ", email.Body);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				});

				var changeCONTRLMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "7"); //Response 7 Ready For Cash Payment
				Factory.Save();
				var changeCUSRESMessage = Helper.CreateCUSRESEDIMessage(cusdecMessage.EM_MessageNum, "6", ZAWhsDataTestHelper.EntryNumber, outwardEntry.CH_BGMReference);//Response 6 Rejection
				Factory.Save();
				var caseClosedCUSRESMessage = Helper.CreateCUSRESEDIMessage(cusdecMessage.EM_MessageNum, "34", ZAWhsDataTestHelper.EntryNumber, outwardEntry.CH_BGMReference);//Response 34 Case Closed
				Factory.Save();
				Env.ClearAllEmailsCreated();
				AssertNoExceptionThrown("Data Context for the WarehouseCustomsEntry data object was empty. The Data Context should always be created by the top level data object", () => new ZACIncomingMessageProcessor(logger).ExecuteBatch());
				CombineAssertions("Change", () =>
				{
					AssertEquals("Logger should indicate reason for failure", expected: false, logger.Logs.Any(x => x.Message.Contains("Data Context for the WarehouseCustomsEntry data object was empty. The Data Context should always be created by the top level data object.")));
					AssertEquals("Message retry", 0, logger.Logs.Count(x => System.Text.RegularExpressions.Regex.IsMatch(x.Message, @"Data Context for the WarehouseCustomsEntry data object was empty\.")));
					changeCONTRLMessage.Reload();
					AssertEquals("chgCONTRLMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, changeCONTRLMessage.EM_Status);
					AssertEquals("chgCONTRLMessage.EM_LinkUniqueID", outwardEntry.PK, changeCONTRLMessage.EM_LinkUniqueID);
					changeCUSRESMessage.Reload();
					AssertEquals("chgCUSRESMessage.EM_Status", CUSRESEDIMessage.Status.ProcessedOK, changeCUSRESMessage.EM_Status);
					AssertEquals("chgCUSRESMessage.EM_LinkUniqueID", outwardEntry.PK, changeCUSRESMessage.EM_LinkUniqueID);
					outwardEntry.Reload();
					AssertEquals("entry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("entry.CH_EntryStatus", "34", outwardEntry.CH_EntryStatus);
					AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					AssertNull(outwardEntry.GetLastHoldUniversalShipmentFromNote());
				});
			}
		}

		protected override void AssertAdditionalForOutwardBeforeOriginalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage originalOutwardMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
			AssertAdditionalBeforeOriginalResponseAutomationIsDisabled(outwardJob, originalOutwardMessage);
		}

		void AssertAdditionalBeforeOriginalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter job, EDIMessage originalMessage)
		{
			var entry = (CusEntryHeader)job;
			AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
			AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
			var contrlMessage = Helper.CreateCONTRLEDIMessage(originalMessage.EM_MessageNum, "7"); //Response 7 - Message Acknowledged
			Factory.Save();
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Control Acknowledged", () =>
			{
				contrlMessage.Reload();
				AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
				AssertEquals("contrlMessage.EM_LinkUniqueID", entry.PK, contrlMessage.EM_LinkUniqueID);
				entry.Reload();
				AssertEquals("entry.CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				AssertEquals("entry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPre", string.Format("Information: 	Message Status of job:{0} has been updated to 'ACK'.", entry.CH_BGMReference), logger.LogMessages.ToString());
				AssertEquals("No email", 0, Env.AllEmailsCreated.Count());
			});
		}

		protected override void AssertAdditionalForOutwardBeforeOriginalResponse(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
			AssertAdditionalForOutwardBeforeOriginalResponseAutomationIsDisabled(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalMessage, bondedEntryKey, bondedEntryKey2);
			var outwardEntry = (CusEntryHeader)outwardJob;
			AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardEntry.CH_WarehouseTransactionStatus);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
		}

		protected override EDIMessage CreateOutwardOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess)
		{
			return CreateOriginalCUSRESEDIMessage(originalMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override void ProcessResponseAndAssertOutwardOnOriginalError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				outwardEntry.Reload();
				AssertEquals("outwardEntry.CH_EntryStatus", "6", outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + outwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Release has been canceled. (WHS Order: ", email.Body);
			});
		}

		protected override void ProcessResponseAndAssertOutwardOnOriginalClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Original Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				outwardEntry.Reload();
				AssertEquals("outwardEntry.CH_EntryStatus", "1", outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '1'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 1 - Release', " + outwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Release can be finalized. (WHS Order: ", email.Body);
			});
		}

		public void TestHandleOutwardOnAmendmentControlError()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (Helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				var inwardDeclaration = inwardEntry.Declaration;
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("inwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardEntry.CH_WarehouseTransactionStatus);
				var bondedEntryKey = ZAWhsDataTestHelper.EntryNumber + "-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				var outwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001240", Helper.OutwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 60m);
				var outwardDeclaration = outwardEntry.Declaration;
				Factory.Save();
				outwardEntry.CH_EntryStatus = "1";
				outwardEntry.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
				Factory.Save();
				outwardEntry.PublishShipmentForWHSOutward(false);
				outwardEntry.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_BondedWhsQuantity = 70m;
				outwardDeclaration.DoMerge();
				var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage("1000", "1", ZAWhsDataTestHelper.EntryNumber2, outwardEntry.CH_BGMReference); //Response 1 RELEASE
				originalCUSRESMessage.EM_LinkedObject = outwardEntry;
				originalCUSRESMessage.EM_Status = CUSRESEDIMessage.Status.ProcessedOK;
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(outwardDeclaration);
				var messageSendingObject = objectParent.SendingObjectsCollection[0];
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
				messageSendingObject.ShouldSend = true;
				new MessageManager(objectParent, notification).SendMessages();
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 30m);
				var cusdecMessage = outwardEntry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
				AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardEntry.CH_WarehouseTransactionStatus);
				AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, outwardEntry.CH_Status);
				var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
				Factory.Save();
				var logger = new LoggingInformationForTesting();
				Env.ClearAllEmailsCreated();
				new ZACIncomingMessageProcessor(logger).ExecuteBatch();
				CombineAssertions("Control Rejected", () =>
				{
					contrlMessage.Reload();
					AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
					AssertEquals("contrlMessage.EM_LinkUniqueID", outwardEntry.PK, contrlMessage.EM_LinkUniqueID);
					outwardEntry.Reload();
					AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("outwardEntry.CH_EntryStatus", "1", outwardEntry.CH_EntryStatus);
					AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Error, outwardEntry.CH_Status);
					AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Updated Stock Release for Entry: " + outwardEntry.EntryHeaderDescriptiveMenuItemText);
					AssertContains("Email Body", "Stock Release has been updated. (WHS Order: ", email.Body);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 30m);//40m
				});
			}
		}

		protected override EDIMessage CreateOutwardAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess)
		{
			return CreateAmendmentCUSRESEDIMessage(amendmentMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override void SetupOutwardOriginalClearState(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, bool createOriginalResponseMessage = true)
		{
			SetupOriginalClearState(outwardJob, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override EDIMessage SendOutwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration)
		{
			return SendMessageViaMenu(outwardJob, outwardDeclaration, MessageSubTypeCodes.Codes.Change);
		}

		protected override void AssertAdditionalForOutwardBeforeAmendmentResponseAutomationIsDisabled(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage amendmentOutwardMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
			AssertAdditionalBeforeAmendmentResponseAutomationIsDisabled(outwardJob, amendmentOutwardMessage);
		}

		protected override void AssertAdditionalForOutwardBeforeAmendmentResponse(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
			AssertAdditionalForOutwardBeforeAmendmentResponseAutomationIsDisabled(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentMessage, bondedEntryKey, bondedEntryKey2);
			AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardEntry.CH_WarehouseTransactionStatus);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 30m);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
		}

		protected override void ProcessResponseAndAssertOutwardOnAmendmentError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Amendment Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				outwardEntry.Reload();
				AssertEquals("outwardEntry.CH_EntryStatus", "6", outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + outwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Previous Stock Release has been restored. (WHS Order: ", email.Body);
			});
		}

		protected override void ProcessResponseAndAssertOutwardOnAmendmentClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Amendment Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				outwardEntry.Reload();
				AssertEquals("outwardEntry.CH_EntryStatus", "27", outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '27'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 27 - Amendment granted', " + outwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Release has been updated. (WHS Order: ", email.Body);
			});
		}

		public void TestHandleOutwardOnWithdrawalControlError()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (Helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				var inwardDeclaration = inwardEntry.Declaration;
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("inwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardEntry.CH_WarehouseTransactionStatus);
				var bondedEntryKey = ZAWhsDataTestHelper.EntryNumber + "-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				var outwardEntry = Helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001240", Helper.OutwardCusProcedure.ZZ6_ProcedureCode, ZAWhsDataTestHelper.EntryNumber, Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 60m);
				var outwardDeclaration = outwardEntry.Declaration;
				outwardEntry.CH_EntryStatus = "1";
				outwardEntry.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
				Factory.Save();
				outwardEntry.PublishShipmentForWHSOutward(false);
				outwardEntry.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage("1000", "1", ZAWhsDataTestHelper.EntryNumber2, outwardEntry.CH_BGMReference); //Response 1 RELEASE
				originalCUSRESMessage.EM_LinkedObject = outwardEntry;
				originalCUSRESMessage.EM_Status = CUSRESEDIMessage.Status.ProcessedOK;
				var invoiceLine = outwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_BondedWhsQuantity = 30m; // withdrawal should not change inventory quantity
				outwardDeclaration.DoMerge();
				Factory.Save();
				var objectParent = new JobDeclarationMessageSendingObjectParent(outwardDeclaration);
				var messageSendingObject = objectParent.SendingObjectsCollection[0];
				messageSendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				messageSendingObject.ShouldSend = true;
				new MessageManager(objectParent, notification).SendMessages();
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				var cusdecMessage = outwardEntry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
				AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardEntry.CH_WarehouseTransactionStatus);
				AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.AwaitingResponse, outwardEntry.CH_Status);
				var contrlMessage = Helper.CreateCONTRLEDIMessage(cusdecMessage.EM_MessageNum, "4"); //Response 4 - Message Rejected
				Factory.Save();
				var logger = new LoggingInformationForTesting();
				Env.ClearAllEmailsCreated();
				new ZACIncomingMessageProcessor(logger).ExecuteBatch();
				CombineAssertions("Control Rejected", () =>
				{
					contrlMessage.Reload();
					AssertEquals("contrlMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, contrlMessage.EM_Status);
					AssertEquals("contrlMessage.EM_LinkUniqueID", outwardEntry.PK, contrlMessage.EM_LinkUniqueID);
					outwardEntry.Reload();
					AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("outwardEntry.CH_EntryStatus", "1", outwardEntry.CH_EntryStatus);
					AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Error, outwardEntry.CH_Status);
					AssertContains("LogPre", string.Format("Information: 	Linking CONTRL Message: #1/{0} to job: {1}", contrlMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					AssertContains("LogPro", string.Format("Information: 	Message Status of job:{0} has been updated to 'ERR'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
					var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == "Finalized Stock Release for Entry: " + outwardEntry.EntryHeaderDescriptiveMenuItemText);
					AssertContains("Email Body", "Stock Release can be finalized. (WHS Order: ", email.Body);
					ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
				});
			}
		}

		protected override EDIMessage SendOutwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration)
		{
			return SendMessageViaMenu(outwardJob, outwardDeclaration, MessageSubTypeCodes.Codes.Cancellation);
		}

		protected override void AssertAdditionalForOutwardBeforeWithdrawalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalOutwardMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
			AssertAdditionalBeforeWithdrawalResponseAutomationIsDisabled(outwardJob, withdrawalOutwardMessage);
		}

		protected override void AssertAdditionalForOutwardBeforeWithdrawalResponse(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			AssertEquals("outwardEntry.CH_EntryStatus", ZString.Empty, outwardEntry.CH_EntryStatus);
			AssertAdditionalForOutwardBeforeWithdrawalResponseAutomationIsDisabled(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalMessage, bondedEntryKey, bondedEntryKey2);
			AssertEquals("outwardEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardEntry.CH_WarehouseTransactionStatus);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
			ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
		}

		protected override EDIMessage CreateOutwardWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess)
		{
			return CreateWithdrawalCUSRESEDIMessage(withdrawalMessage, isSuccess, ZAWhsDataTestHelper.EntryNumber2);
		}

		protected override void ProcessResponseAndAssertOutwardOnWithdrawalError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal Rejected", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				outwardEntry.Reload();
				AssertEquals("outwardEntry.CH_EntryStatus", "6", outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '6'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 6 - Reject To Clearer', " + outwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Release can be finalized. (WHS Order: ", email.Body);
			});
		}

		protected override void ProcessResponseAndAssertOutwardOnWithdrawalClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage)
		{
			var outwardEntry = (CusEntryHeader)outwardJob;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions("Withdrawal Clear", () =>
			{
				responseMessage.Reload();
				AssertEquals("responseMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, responseMessage.EM_Status);
				outwardEntry.Reload();
				AssertEquals("outwardEntry.CH_EntryStatus", "28", outwardEntry.CH_EntryStatus);
				AssertEquals("outwardEntry.CH_Status", ZAMessageStatusList.Codes.Acknowledged, outwardEntry.CH_Status);
				AssertContains("LogPre", string.Format("Information: 	Linking CUSRES Message: #1/{0} to job: {1}", responseMessage.Interchange.EI_InterchangeNum, outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("LogPro", string.Format("Information: 	Entry Status of Entry:{0} has been updated to '28'.", outwardEntry.CH_BGMReference), logger.LogMessages.ToString());
				var subject = "Entry Notification: 'Code 28 - Cancellation granted', " + outwardDeclaration.JE_DeclarationReference;
				var email = Env.AllEmailsCreated.FirstOrDefault(x => x.Subject == subject);
				AssertContains("Email Body", "Stock Release has been canceled. (WHS Order: ", email.Body);
			});
		}
		#endregion

		#region Implementation
		ZAWhsDataTestHelper Helper => (ZAWhsDataTestHelper)helper;

		protected override void GetInvoiceLines(IWarehouseIntegrationSupporter job, out JobComInvoiceLine invoiceLine, out JobComInvoiceLine invoiceLine2)
		{
			var entry = (CusEntryHeader)job;
			var invoiceLines = entry.InvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ToArray();
			invoiceLine = invoiceLines[0];
			invoiceLine2 = invoiceLines[1];
		}

		protected override WhsDataTestHelper<JobDeclaration, OrgSupplierPart, CusClassification, CusClassPartPivot> CreateNewHelper()
		{
			return new ZAWhsDataTestHelper(Factory);
		}

		MessageNotificationCollector_ForTest notification;
		protected override void SetUp()
		{
			base.SetUp();
			notification = new MessageNotificationCollector_ForTest();
			Helper.SetupCustomsData();
		}

		protected override void ProcessResponse(IWarehouseIntegrationSupporter job, JobDeclaration declaration, JobComInvoiceLine invoiceLine, JobComInvoiceLine invoiceLine2, EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			var entry = (CusEntryHeader)job;
			var logger = new LoggingInformationForTesting();
			Env.ClearAllEmailsCreated();
			new ZACIncomingMessageProcessor(logger).ExecuteBatch();
			CombineAssertions(() =>
			{
				incomingMessage.Reload();
				AssertEquals("incomingMessage.EM_Status", CONTRLEDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			});
		}

		EDIMessage SendMessageViaMenu(IWarehouseIntegrationSupporter job, JobDeclaration declaration, ZString messageType)
		{
			var entry = (CusEntryHeader)job;
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageSendingObject = objectParent.SendingObjectsCollection[0];
			messageSendingObject.MessageType = messageType;
			messageSendingObject.ShouldSend = true;
			new MessageManager(objectParent, notification).SendMessages();
			return entry.Messages.OfType<CUSDECEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
		}

		void SetupOriginalClearState(IWarehouseIntegrationSupporter job, string entryNumber)
		{
			var entry = (CusEntryHeader)job;
			entry.EntryNumber = entryNumber;
			entry.CH_EntryStatus = "1";
			entry.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			var originalCUSRESMessage = Helper.CreateCUSRESEDIMessage("1000", "1", entryNumber, entry.CH_BGMReference); //Response 1 RELEASE
			originalCUSRESMessage.EM_LinkedObject = entry;
			originalCUSRESMessage.EM_Status = CUSRESEDIMessage.Status.ProcessedOK;
		}
		#endregion
	}
}
