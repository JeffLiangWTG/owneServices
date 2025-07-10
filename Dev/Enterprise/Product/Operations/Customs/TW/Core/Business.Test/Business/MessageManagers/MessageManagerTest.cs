using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;
using MessageManager = Enterprise.Customs.TW.Business.MessageManagers.MessageManager;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class MessageManagerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConcurrencyErrorWhenFactorySave()
		{
			var declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			var messageType = "ICD";
			var parent = SendAllMessage(declaration, messageType);
			var sendingObject = parent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
			var entryHeader = sendingObject.Header;
			AssertEquals(ZString.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("AWO", entryHeader.CH_Status);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			newEntryHeader.CH_EntryStatus = "F88";
			newEntryHeader.CH_EntryReleaseDate = ZDateTime.Now;
			newFactory.Save();
			messageType = MessageTypeList.Codes.ADM;
			var testDoc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var additionalDocumentMessageSendingObjectParent = new AdditionalDocumentMessageSendingObjectParent(declaration, messageType);
			var supportDocment = additionalDocumentMessageSendingObjectParent.SendingObjectsCollection[0].SupportingDocuments.AddNew();
			supportDocment.EDoc = testDoc.UniqueKey;
			sendingObject = additionalDocumentMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
			sendingObject.ShouldSend = true;
			var manager = new MessageManager(additionalDocumentMessageSendingObjectParent, notification);
			AssertNoExceptionThrown("--- Save Aborted Due to Concurrency Check ---", () =>
			{
				manager.SendMessages(declaration.MessageInitiator);
			}

			);
		}

		public void TestLogCustomsCommencedForECM()
		{
			var declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Export, entryNmber);
			var messageType = "ADM";
			SendAllMessage(declaration, messageType);
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCommenced.Code));
			AssertEquals("ECM (Export Customs Commenced) Log Entry event should not be created for ADM message", false, logEntries.Any());
			messageType = MessageTypeList.Codes.ECD;
			SendAllMessage(declaration, messageType);
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCommenced.Code));
			AssertEquals("ECM (Export Customs Commenced) Log Entry event should have been created", true, logEntries.Any());
		}

		public void TestLogCustomsCommencedForCCC()
		{
			var declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			var messageType = "ADM";
			SendAllMessage(declaration, messageType);
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			AssertEquals("CCC (Customs Commenced) Log Entry event should not be created for ADM message", false, logEntries.Any());
			messageType = MessageTypeList.Codes.ICD;
			SendAllMessage(declaration, messageType);
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			AssertEquals("CCC (Customs Commenced) Log Entry event should have been created", true, logEntries.Any());
		}

		[TestDate(2020, 08, 07)]
		public void TestSendMessages()
		{
			var declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Export, entryNmber);
			var messageType = "ECD";
			var parent = SendAllMessage(declaration, messageType);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is " + messageType, messageType, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusEntryHeader", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + declaration.JE_CustomsProfile, declaration.JE_CustomsProfile, message.EM_ApplicationReference);
				AssertContains("<ID>NO1</ID>", message.EM_MessageText);
			}

			declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			messageType = "ICD";
			parent = SendAllMessage(declaration, messageType);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is " + messageType, messageType, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusEntryHeader", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + declaration.JE_CustomsProfile, declaration.JE_CustomsProfile, message.EM_ApplicationReference);
				AssertContains("<ID>NO1</ID>", message.EM_MessageText);
			}

			declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Import, "");
			messageType = "ICD";
			parent = SendAllMessage(declaration, messageType);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertContains("<ID>BAB10923480001</ID>", message.EM_MessageText);
			}

			declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Import, "");
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = ZString.Empty;
			parent = SendAllMessage(declaration, messageType);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 0, entry.Messages.Count);
				AssertContains("Generate entry number error.", notification.LastMessage);
			}

			declaration = CreateNewJob(Customs.Business.JobMessageTypeList.Codes.Import, "");
			declaration.EntryNumber = "BAB10923480008";
			parent = SendAllMessage(declaration, messageType);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertContains("<ID>BAB10923480008</ID>", message.EM_MessageText);
			}
		}

		JobDeclaration CreateNewJob(ZString messageType, ZString entryNmber)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = messageType;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
			cusHead1.EntryNumber = entryNmber;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "B1";
			entryInstruction.CEI_CustomsOffice = "BA";
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			return declaration;
		}

		JobDeclarationMessageSendingObjectParent SendAllMessage(JobDeclaration declaration, ZString messageType)
		{
			var parent = new JobDeclarationMessageSendingObjectParent(declaration, messageType);
			parent.MenuCaption = "test menu caption";
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				if (messageType == MessageTypeList.Codes.ICD)
				{
					action.MessageType = MessageTypeList.Codes.ICD;
				}

				action.ShouldSend = true;
			}

			notification.Clear();
			var manager = new MessageManager(parent, notification);
			manager.SendMessages(declaration.MessageInitiator);
			return parent;
		}

		public void TestSendIEAMessage()
		{
			InitData(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			var messageType = MessageTypeList.Codes.IEA;
			cusHead1.CH_EntryStatus = EntryStatusCodeList.Codes.IEM;
			SendAllMessageWithoutSaving(messageType);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is " + messageType, messageType, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusEntryHeader", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + declaration.JE_CustomsProfile, declaration.JE_CustomsProfile, message.EM_ApplicationReference);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendECDMessage()
		{
			InitData(Customs.Business.JobMessageTypeList.Codes.Export, entryNmber);
			var messageType = MessageTypeList.Codes.ECD;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			cusHead1.CH_EntryStatus = "ADD";
			var objectParent = new AdditionalDocumentMessageSendingObjectParent(declaration, messageType);
			var supportDocment = objectParent.SendingObjectsCollection[0].SupportingDocuments.AddNew();
			supportDocment.EDoc = doc1.UniqueKey;
			SendAllMessageWithoutSaving(messageType, objectParent);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is " + messageType, messageType, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusEntryHeader", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + declaration.JE_CustomsProfile, declaration.JE_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("message.MessageAttachments.Count", 1, message.MessageAttachments.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendICDMessage()
		{
			InitData(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			var messageType = MessageTypeList.Codes.ICD;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			cusHead1.CH_EntryStatus = "ADD";
			var objectParent = new AdditionalDocumentMessageSendingObjectParent(declaration, messageType);
			var supportDocment = objectParent.SendingObjectsCollection[0].SupportingDocuments.AddNew();
			supportDocment.EDoc = doc1.UniqueKey;
			SendAllMessageWithoutSaving(messageType, objectParent);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is " + messageType, messageType, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusEntryHeader", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + declaration.JE_CustomsProfile, declaration.JE_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("message.MessageAttachments.Count", 1, message.MessageAttachments.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendADMMessage()
		{
			InitData(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			var messageType = MessageTypeList.Codes.ADM;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			cusHead1.CH_EntryStatus = "ADD";
			var objectParent = new AdditionalDocumentMessageSendingObjectParent(declaration, messageType);
			var supportDocment = objectParent.SendingObjectsCollection[0].SupportingDocuments.AddNew();
			supportDocment.EDoc = doc1.UniqueKey;
			SendAllMessageWithoutSaving(messageType, objectParent);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType is " + messageType, messageType, message.EM_MessageType);
				AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("message.EM_LinkTable is CusEntryHeader", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("message.EM_ApplicationReference is " + declaration.JE_CustomsProfile, declaration.JE_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("message.MessageAttachments.Count", 1, message.MessageAttachments.Count);
			}
		}

		public void TestCheckDaysDelayedDeclaration()
		{
			InitData(Customs.Business.JobMessageTypeList.Codes.Import, entryNmber);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2019, 03, 01);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 03, 18);
			entryInstruction.CEI_DaysOfDelayedDeclaration = 3;
			parent = new JobDeclarationMessageSendingObjectParent(declaration, MessageTypeList.Codes.ICD);
			AssertEquals("coll has 1", 1, parent.SendingObjectsCollection.Count);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				action.Action = ActionCodeList.Codes.Create;
				action.ShouldSend = true;
			}
			var manager = new MessageManager(parent, notification);
			notification.Clear();
			notification.NextTextAnswer = DaysOfDelayAnswersList.Codes.Update;
			manager.CheckDaysDelayedDeclaration();
			var message = @"These declarations will be subject to delay fees.
The calculated and entered days of delay for each one are:
	NO1 - Calculated 2, Entered 3
Please select one of the actions below.";
			CombineAssertions(() =>
			{
				AssertContains(message, notification.LastMessage);
				AssertEquals(2, entryInstruction.CEI_DaysOfDelayedDeclaration);
			}

			);
			cusHead1.Messages.RemoveAll();
			notification.Clear();
			notification.NextTextAnswer = DaysOfDelayAnswersList.Codes.Continue;
			entryInstruction.CEI_DaysOfDelayedDeclaration = 3;
			manager.CheckDaysDelayedDeclaration();
			AssertEquals(3, entryInstruction.CEI_DaysOfDelayedDeclaration);
		}

		MessageManager SendAllMessageWithoutSaving(ZString messageType)
		{
			return SendAllMessageWithoutSaving(messageType, new JobDeclarationMessageSendingObjectParent(declaration, messageType));
		}

		MessageManager SendAllMessageWithoutSaving(ZString messageType, JobDeclarationMessageSendingObjectParent sendingObjectParent)
		{
			parent = sendingObjectParent;
			AssertEquals("coll has 1", 1, parent.SendingObjectsCollection.Count);
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				if (messageType == MessageTypeList.Codes.ICD)
				{
					action.MessageType = MessageTypeList.Codes.ICD;
				}

				action.ShouldSend = true;
			}

			var manager = new MessageManager(parent, notification);
			manager.SendMessagesWithoutSaving(declaration.MessageInitiator);
			return manager;
		}

		void InitData(ZString messageType, ZString entryNmber)
		{
			declaration.JE_MessageType = messageType;
			entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "B1";
			cusHead1.Messages.RemoveAll();
			cusHead1.CH_Status = ZString.Empty;
			cusHead1.EntryNumber = entryNmber;
			Factory.Save();
		}

		public void TestCheckEntries()
		{
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.EntryNumber = "AA";
			notification.ShowInformation(ZString.Empty, ZString.Empty);
			CombineAssertions("Already has entry header", () =>
			{
				Assert(MessageManager.CheckEntries(declaration, notification));
				AssertEquals(ZString.Empty, notification.LastMessage);
			});
			var newDeclaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			newDeclaration1.MessageInitiator = messageInitiator;
			messageInitiator.AnswerToContinueWithAction = false;
			CombineAssertions("Do not Generate entries (Merge)", () =>
			{
				Assert(!MessageManager.CheckEntries(newDeclaration1, notification));
				AssertEquals(ZString.Empty, notification.LastMessage);
			});

			messageInitiator.AnswerToContinueWithAction = true;
			newDeclaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			newDeclaration1.Invoices.AddNew().InvoiceLines.AddNew();
			CombineAssertions("Generate entries (Merge), but Submit Type is not 'BLT'", () =>
			{
				Assert(!MessageManager.CheckEntries(newDeclaration1, notification));
				AssertEquals(@"Cannot Generate Entries (Merge) when Submit Type is not 'BLT'. Please change the Submit Type to 'BLT'. 

If you cannot see the Submit Type, configure the Submit Type to 'BLT' or 'BTH' in Registry > Customs > Integration > Local Country Customs Interface.", notification.LastMessage);
			});

			notification.ShowError(ZString.Empty, ZString.Empty);
			newDeclaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			CombineAssertions("Generate entries (Merge) successfully.", () =>
			{
				Assert(!MessageManager.CheckEntries(newDeclaration1, notification));
				AssertEquals("Generate Entry(Merge) successful, please save and send again.", notification.LastMessage);
			});

			notification.ShowError(ZString.Empty, ZString.Empty);
			var newDeclaration2 = Factory.New<JobDeclaration>();
			newDeclaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			newDeclaration2.MessageInitiator = messageInitiator;
			CombineAssertions("Generate entries (Merge) failed", () =>
			{
				Assert(!MessageManager.CheckEntries(newDeclaration2, notification));
				AssertEquals(ZString.Empty, notification.LastMessage);
			});
		}

		public void TestCheckDeclarationNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_BoxNumber = "123";
			Assert(!MessageManager.CheckDeclarationNumber(declaration, notification));
			AssertEquals("Please generate entries first.", notification.LastMessage);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = "";
			Assert(!MessageManager.CheckDeclarationNumber(declaration, notification));
			AssertEquals("The generator rules could not be found for the job.", notification.LastMessage);
			entryInstruction.CEI_Style = "G1";
			notification.ShowError("", "");
			Assert(MessageManager.CheckDeclarationNumber(declaration, notification));
			AssertEquals("", notification.LastMessage);
		}

		public void TestSendMessages_DeclarationEvent()
		{
			AssertSendMessages_DeclarationEvent("ECD", "9", Events.ExportCustomsCommenced.Code, "test menu caption, SendingObjectsCollectionAction='9'");
			AssertSendMessages_DeclarationEvent("ICD", "9", Events.CustomsCommenced.Code, "test menu caption, SendingObjectsCollectionAction='9'");
			AssertSendMessages_DeclarationEvent("ICD", "5", Events.DeclarationAmendmentSent.Code, "test menu caption, SendingObjectsCollectionAction='5'");
			AssertSendMessages_DeclarationEvent("ICD", "17", Events.DeclarationAmendmentSent.Code, "test menu caption, SendingObjectsCollectionAction='17'");
			AssertSendMessages_DeclarationEvent("ICD", "18", Events.DeclarationAmendmentSent.Code, "test menu caption, SendingObjectsCollectionAction='18'");
		}

		void AssertSendMessages_DeclarationEvent(string messageType, string actionCode, string eventCode, string expectReference)
		{
			var declaration = CreateNewJob(messageType == "ICD" ? SharedJobMessageTypeList.Codes.Import : SharedJobMessageTypeList.Codes.Export, entryNmber);
			var parent = new JobDeclarationMessageSendingObjectParent(declaration, messageType);
			parent.MenuCaption = "test menu caption";
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				action.Action = actionCode;
			}
			notification.Clear();
			var manager = new MessageManagerForTest(parent, notification);
			manager.LogDeclarationEventExpose();

			Factory.Save();
			var log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code)).Single();
			AssertEquals("should always log a MSN event", expectReference, log.SL_Reference);

			if (actionCode == "9")
			{
				log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode)).Single();
				AssertEquals(expectReference, log.SL_Reference);
			}
		}

		public void TestSendMessages_DeclarationEventForAircraftParts()
		{
			var declaration = CreateNewJob(SharedJobMessageTypeList.Codes.Import, entryNmber);
			var parent = new AdditionalDocumentMessageSendingObjectParent(declaration, "CAA");
			parent.MenuCaption = "Send Import Customs Declaration (Aircraft Parts)";
			foreach (MessageSendingObject action in parent.SendingObjectsCollection)
			{
				action.Action = "9";
			}
			notification.Clear();
			var manager = new MessageManagerForTest(parent, notification);
			manager.LogDeclarationEventExpose();

			Factory.Save();
			var expectReference = "Send Import Customs Declaration (Aircraft Parts), SendingObjectsCollectionAction='9'";
			var log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code)).Single();
			AssertEquals("should always log a MSN event", expectReference, log.SL_Reference);

			log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code)).Single();
			AssertEquals(expectReference, log.SL_Reference);
		}

		class MessageManagerForTest : MessageManager
		{
			public MessageManagerForTest(JobDeclarationMessageSendingObjectParent declarationWrapper, IMessageNotificationCollector notification, bool shoulSendAll = false) : base(declarationWrapper, notification, shoulSendAll)
			{
			}

			public void LogDeclarationEventExpose() => LogDeclarationEvent();
		}

		readonly MessageNotificationCollector_ForTest notification = new MessageNotificationCollector_ForTest();
		JobDeclaration declaration;
		JobDeclarationMessageSendingObjectParent parent;
		CusEntryHeader cusHead1;
		CusEntryInstruction entryInstruction;
		readonly ZString entryNmber = "NO1";
		OrgHeader orgHeader;
		protected override void SetUp()
		{
			base.SetUp();
			this.orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = this.orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = this.orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = this.orgHeader.MainAddress.PK;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = this.orgHeader.MainAddress.PK;
			cusHead1 = declaration.ActiveEntryHeaders.AddNew();
			entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "B1";
			entryInstruction.CEI_CustomsOffice = "BA";
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "orgProxy01";
			orgProxy.OH_RL_NKClosestPort = "TW";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			Factory.Save();
		}
	}
}
