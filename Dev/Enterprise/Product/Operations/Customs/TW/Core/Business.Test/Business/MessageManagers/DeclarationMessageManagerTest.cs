using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class DeclarationMessageManagerTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			AssertEquals(messageSender, messageManager.BusinessObject);
		}

		public void TestCanSendOriginal()
		{
			AssertEquals(true, messageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			AssertEquals(true, messageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			var heard = messageSender.Header;
			heard.CH_EntryStatus = EntryStatusCodeList.Codes.IEM;
			Assert(!heard.IsWaitingForResponse);
			Assert(!messageManager.IsWaitingForResponse);
			heard.CH_Status = JobDeclarationMessageStatusList.Codes.AWC;
			Assert(heard.IsWaitingForResponse);
			Assert(messageManager.IsWaitingForResponse);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", messageSender.FriendlyNameForMessageManager, messageManager.MessageFriendlyName);
		}

		public void TestGenerateMessage()
		{
			var entry = messageSender.Header;
			var declaration = entry?.Declaration;
			messageSender.SupportingDocuments.AddNew();
			messageSender.ShouldSend = true;
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			var message = messages[0];
			AssertEquals(true, message.EM_IsTestMessage);
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			message = messages[0];
			AssertEquals(false, message.EM_IsTestMessage);
			AssertEquals(1, message.MessageAttachments.Count);
			var attachment = message.MessageAttachments[0];
			CombineAssertions("Test Goods Examination Application Message Properties", () =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", messageSender.MessageType, message.EM_MessageType);
				AssertEquals("EM_LinkUniqueID", entry.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("EM_ApplicationReference", declaration.JE_CustomsProfile, message.EM_ApplicationReference);
				AssertEquals("EM_IsTestMessage", false, message.EM_IsTestMessage);
				AssertEquals("EM_MessageOwner", messageSender.GetMessageOwner(), message.EM_MessageOwner);
				AssertEquals("Message Attachment Count", 1, message.MessageAttachments.Count);
				AssertEquals("EG_StorageDocsGuid", doc.UniqueKey, attachment.EG_StorageDocsGuid);
				AssertEquals("EG_FileName", doc.FileName, attachment.EG_FileName);
				AssertEquals("EG_EdiMsgDocType", doc.DocType, attachment.EG_EdiMsgDocType);
				AssertEquals("EG_EM", message.PK, attachment.EG_EM);
			}

			);
			var docLine = messageSender.SupportingDocuments.AddNew();
			docLine.EDoc = doc1.UniqueKey;
			messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			message = messages[0];
			AssertEquals(2, message.MessageAttachments.Count);
			Assert(message.MessageAttachments.Cast<EDIMessageAttach>().Any(x => x.EG_StorageDocsGuid == doc.UniqueKey));
			Assert(message.MessageAttachments.Cast<EDIMessageAttach>().Any(x => x.EG_StorageDocsGuid == doc1.UniqueKey));
		}

		public void TestSendWithErrorWithMessageErrorOnDeclaration()
		{
			var entry = messageSender.Header;
			var declaration = entry?.Declaration;
			messageSender.SupportingDocuments.AddNew();
			messageSender.ShouldSend = true;
			var messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			var message = messages[0];
			AssertEquals("EM_SendWithMessageErrors should be false", false, message.EM_SendWithMessageErrors);
			declaration.Validation.ValidateAll();
			messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			message = messages[0];
			AssertEquals("EM_SendWithMessageErrors should be true", true, message.EM_SendWithMessageErrors);
		}

		public void TestSendWithErrorWithMessageErrorOnEntryHeader()
		{
			var entry = messageSender.Header;
			var declaration = entry?.Declaration;
			messageSender.SupportingDocuments.AddNew();
			messageSender.ShouldSend = true;
			var messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			var message = messages[0];
			AssertEquals("EM_SendWithMessageErrors should be false", false, message.EM_SendWithMessageErrors);
			declaration.CusEntryInstruction.Validation.ValidateAll();
			messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			message = messages[0];
			AssertEquals("EM_SendWithMessageErrors should be true", true, message.EM_SendWithMessageErrors);
		}

		public void TestSendWithErrorWithMessageErrorOnSendingObject()
		{
			var entry = messageSender.Header;
			var declaration = entry?.Declaration;
			messageSender.SupportingDocuments.AddNew();
			messageSender.ShouldSend = true;
			var messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			var message = messages[0];
			AssertEquals("EM_SendWithMessageErrors should be false", false, message.EM_SendWithMessageErrors);
			messageSender.Action = ActionCodeList.Codes.Update;
			Assert(!entry.HasBeenLodgedAtCustoms);
			messageSender.Validation.ValidateAll();
			messages = messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("Messages count", 1, messages.Length);
			message = messages[0];
			AssertEquals("EM_SendWithMessageErrors should be true", true, message.EM_SendWithMessageErrors);
		}

		public void TestCH_Status()
		{
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			messageSender.ShouldSend = true;
			messageSender.MessageType = MessageTypeList.Codes.IEA;
			messageSender.Action = "9";
			messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("CH_Status", "AWG", messageSender.Header.CH_Status);
			messageSender.Action = "5";
			messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals("CH_Status", "AWG", messageSender.Header.CH_Status);
		}

		[TestDate(2021, 07, 14)]
		public void TestEntrySubmittedDate()
		{
			var entryHeader = messageSender.Header;
			var declaration = entryHeader.Declaration;
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			messageSender.ShouldSend = true;
			messageSender.MessageType = MessageTypeList.Codes.IEA;
			messageManager.GenerateOriginalMessages(messageSender);
			Assert(entryHeader.CH_EntrySubmittedDate.IsEmpty);
			Assert(declaration.JE_EntrySubmittedDate.IsEmpty);
			messageSender.MessageType = MessageTypeList.Codes.ECD;
			var expectedDate = new ZDateTime(2021, 7, 14);
			messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals(expectedDate, entryHeader.CH_EntrySubmittedDate);
			AssertEquals(expectedDate, declaration.JE_EntrySubmittedDate);
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			Assert(entryHeader.CH_EntrySubmittedDate.IsEmpty);
			Assert(declaration.JE_EntrySubmittedDate.IsEmpty);
			messageSender.MessageType = MessageTypeList.Codes.ICD;
			messageManager.GenerateOriginalMessages(messageSender);
			AssertEquals(expectedDate, entryHeader.CH_EntrySubmittedDate);
			AssertEquals(expectedDate, declaration.JE_EntrySubmittedDate);
		}

		public void TestShouldWaitUntilResponded()
		{
			var messageManager = new DeclarationMessageManagerForTest(messageSender);
			Assert(!messageManager.ShouldWaitUntilRespondedForTest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_JE = declaration.PK;
			cusHead1.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			messageSender = new DeclarationMessageSendingObject(cusHead1);
			messageManager = new DeclarationMessageManager(messageSender);
			doc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var docLine = messageSender.SupportingDocuments.AddNew();
			docLine.EDoc = doc.UniqueKey;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			declaration.JE_JS = shipment.PK;
			doc1 = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
		}

		IeDoc doc;
		IeDoc doc1;
		DeclarationMessageSendingObject messageSender;
		DeclarationMessageManager messageManager;
		sealed class DeclarationMessageManagerForTest : DeclarationMessageManager
		{
			public DeclarationMessageManagerForTest(MessageSendingObject messageSender) : base(messageSender)
			{
			}

			internal bool ShouldWaitUntilRespondedForTest => ShouldWaitUntilResponded;
		}
	}
}
