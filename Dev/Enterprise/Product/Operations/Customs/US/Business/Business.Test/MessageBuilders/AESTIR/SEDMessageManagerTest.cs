using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SEDMessageManagerTest : DeclarationTestHelper
	{
		public void TestBusinessObject()
		{
			AssertEquals("BusinessObject's type", typeof(CusEntryHeader), manager.BusinessObject.GetType());
			AssertEquals("BusinessObject", entryHeader.PK, manager.BusinessObject.PK);
		}

		public void TestCanSendOriginal()
		{
			entryHeader.EntryNumber = "";
			AssertEquals("PreCondition:IsWaitingForResponse is false", false, manager.IsWaitingForResponse);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);
			entryHeader.EntryNumber = "EN32424";
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);
			AssertEquals("CanSendOriginal", false, manager.CanSendOriginal);
		}

		public void TestGenerateOriginalMessages()
		{
			Enterprise.Messaging.Business.EDIMessage[] messages = manager.GenerateOriginalMessages(entryHeader);
			AssertEquals("messages.length", 1, messages.Length);
			AssertEquals("messages[0].EM_MessageText", entryHeader.SEDString, messages[0].EM_MessageText);
		}

		public void TestIsWaitingForResponse()
		{
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
			AssertEquals("IsWaitingForResponse", true, manager.IsWaitingForResponse);
		}

		public void TestMessageFriendlyName()
		{
			entryHeader.CH_BGMReference = "B00012121";
			AssertEquals("MessageFriendlyName", "SED Message for B00012121", manager.MessageFriendlyName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			declaration = CreateSeaExportDeclaration();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders[0];
			manager = new SEDMessageManager(entryHeader);
		}
		CusEntryHeader entryHeader;
		SEDMessageManager manager;
		JobDeclaration declaration;
	}
}
