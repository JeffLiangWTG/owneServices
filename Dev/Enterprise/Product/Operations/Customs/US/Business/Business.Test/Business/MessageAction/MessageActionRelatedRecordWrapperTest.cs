using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MessageActionRelatedRecordWrapper))]
	sealed class MessageActionRelatedRecordWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessagesToShow()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = new MessageActionRelatedRecordWrapper(declaration, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(0, wrapper.MessagesToShow.Count);
			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			wrapper = new MessageActionRelatedRecordWrapper(ensEntry, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(0, wrapper.MessagesToShow.Count);
			var ensMessage = Factory.New<MQEDIMessage>();
			ensEntry.Messages.Add(ensMessage);
			wrapper = new MessageActionRelatedRecordWrapper(ensEntry, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(1, wrapper.MessagesToShow.Count);
			var liquidation = Factory.New<CusLiquidation>();
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = liquidation;
			declaration.Liquidations.Add(liquidation);
			wrapper = new MessageActionRelatedRecordWrapper(new LiquidationWithMessagesToShow(declaration.Liquidations), MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(1, wrapper.MessagesToShow.Count);
			var container = declaration.CusContainers.AddNew();
			var cntMessage = Factory.New<MQEDIMessage>();
			container.Messages.Add(cntMessage);
			wrapper = new MessageActionRelatedRecordWrapper(container, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(1, wrapper.MessagesToShow.Count);
			var bill = declaration.Bills.AddNew();
			var billMessage = Factory.New<MQEDIMessage>();
			bill.Messages.Add(billMessage);
			wrapper = new MessageActionRelatedRecordWrapper(bill, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(1, wrapper.MessagesToShow.Count);
			var decMessage = Factory.New<MQEDIMessage>();
			declaration.Messages.Add(decMessage);
			wrapper = new MessageActionRelatedRecordWrapper(declaration, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(1, wrapper.MessagesToShow.Count);
			var invoice = declaration.Invoices.AddNew();
			var invMessage = Factory.New<MQEDIMessage>();
			invoice.Messages.Add(invMessage);
			wrapper = new MessageActionRelatedRecordWrapper(invoice, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(1, wrapper.MessagesToShow.Count);
			var builder = new MessageBuilders.BIRDEntrySummaryQueryMessageBuilder(ensEntry);
			var ensQuery = builder.PopulateMessage();
			Factory.Save();
			wrapper = (MessageActionRelatedRecordWrapper)declaration.InBondRelatedRecords.FirstOrDefault(x => ((MessageActionRelatedRecordWrapper)x).RecordType == MessageAttacheeRecordType.BIRD);
			AssertNotNull(wrapper);
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery, wrapper.MessagesToShow[0].EM_MessageType);
			var seEntry = declaration.CustomsEntryHeaders.AddNew();
			seEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var birdMessageAttachee = new BIRDMessageAttachee(declaration);
			var birdMessage = seEntry.Messages.AddNew(typeof(MQEDIMessage));
			birdMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction;
			var birdWrapper = new MessageActionRelatedRecordWrapper(birdMessageAttachee, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(2, birdWrapper.MessagesToShow.Count);
		}

		public void TestMessagesToShowWithDiscarded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			Assert(declaration.IsBorderMovement);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			var message = new MessageBuilders.BorderCargoReleaseMessageBuilder(entry, UpdateActionCode.Add).PopulateMessage();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
			declaration.US_ITDate = ZDateTime.Today;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			Assert("ShouldBeMerged OK", declaration.DoMerge());
			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			Assert(!entry.IsActive);
			Assert(!entry.IsDeleted);
			var wrapper = new MessageActionRelatedRecordWrapper(entry, MessagesToShowCollection.MessagesStatus.InactiveOnly);
			AssertEquals("discarded Messages only", 1, wrapper.MessagesToShow.Count);
		}

		public void TestTIBProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_TIBExpiryDate = new ZDateTime(2005, 1, 1);
			ensEntry.US_TIBNumOfExtensions = 3;
			var wrapper = new MessageActionRelatedRecordWrapper(ensEntry, MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals("TIB expiry date", new ZDateTime(2005, 1, 1), wrapper.TIBExpiryDate);
			AssertEquals("TIB number of extension", 3, wrapper.TIBNumOfExtensions);
		}

		public void TestHumanFriendlyReference()
		{
			RelatedRecord.RecordType = MessageAttacheeRecordType.MasterBillOfLading;
			RelatedRecord.HumanFriendlyReference = "ASDF";
			AssertEquals("ASDF", wrapper.HumanFriendlyReference);
			RelatedRecord.RecordType = MessageAttacheeRecordType.Container;
			RelatedRecord.HumanFriendlyReference = "ASDF";
			AssertEquals("ASDF", wrapper.HumanFriendlyReference);
		}

		public void TestProxyProperties()
		{
			RelatedRecord.RecordType = MessageAttacheeRecordType.MasterBillOfLading;
			AssertEquals(MessageAttacheeRecordType.MasterBillOfLading, wrapper.RecordType);
			RelatedRecord.MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			AssertEquals("Status", ImportMessageStatusList.Descriptions.AwaitingDepartureAmendment, wrapper.StatusDesc);
		}

		public void TestStatus()
		{
			RelatedRecord.MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			AssertEquals(ImportMessageStatusList.Codes.AwaitingDepartureAmendment, wrapper.Status);
		}

		public void TestEntryStatus()
		{
			RelatedRecord.EntryStatus = ImportEntryStatusList.Codes._03;
			AssertEquals(ImportEntryStatusList.Codes._03, wrapper.EntryStatus);
		}

		public void TestReleaseDate()
		{
			RelatedRecord.ReleaseDate = new ZDateTime(2008, 09, 04);
			AssertEquals(new ZDateTime(2008, 09, 04), wrapper.ReleaseDate);
		}

		public void TestRecordTypeDescription()
		{
			AssertEquals("RecordTypeDescription", wrapper.RecordTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObject() => new MessageActionRelatedRecordWrapper(RelatedRecord, MessagesToShowCollection.MessagesStatus.ActiveOnly);

		MessageActionRelatedRecordWrapper wrapper;
		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new MessageActionRelatedRecordWrapper(RelatedRecord, MessagesToShowCollection.MessagesStatus.ActiveOnly);
		}

		DummyIInBondWP relatedRecord;
		DummyIInBondWP RelatedRecord => relatedRecord ?? (relatedRecord = Factory.New<DummyIInBondWP>());
	}
}
