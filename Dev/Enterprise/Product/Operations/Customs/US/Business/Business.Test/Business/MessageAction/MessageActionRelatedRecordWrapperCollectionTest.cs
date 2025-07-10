using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MessageActionRelatedRecordWrapperCollection))]
	sealed class MessageActionRelatedRecordWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageActionRelatedRecordWrapperCollection>
	{
		public void TestSynchroniseWithRelatedRecordsCountChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			CusContainer container = declaration.CusContainers.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("RelatedRecords", 0, declaration.InBondRelatedRecords.Count);
			BlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			container.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			bill1.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			entry.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			Factory.Save();
			AssertEquals("RelatedRecords", 3, declaration.InBondRelatedRecords.Count);
			AssertNotNull(declaration.InBondRelatedRecords.GetElementWrapping(bill1));
			AssertNotNull(declaration.InBondRelatedRecords.GetElementWrapping(container));
			AssertNotNull(declaration.InBondRelatedRecords.GetElementWrapping(entry));
			Bill bill2 = declaration.Bills.AddNew();
			AssertEquals("RelatedRecords", 3, declaration.InBondRelatedRecords.Count);
			AssertNull(declaration.InBondRelatedRecords.GetElementWrapping(bill2));
			declaration.Bills.RemoveAndDelete(bill1);
			AssertEquals("RelatedRecords", 2, declaration.InBondRelatedRecords.Count);
			AssertNull(declaration.InBondRelatedRecords.GetElementWrapping(bill1));
		}

		public void TestAllowNew()
		{
			MessageActionRelatedRecordWrapperCollection coll = GetCollectionToTest();
			AssertEquals("AllowNew", false, coll.AllowNew);
		}

		public void TestAllowRemove()
		{
			MessageActionRelatedRecordWrapperCollection coll = new MessageActionRelatedRecordWrapperCollection(Header, delegate
			{
				return Header.MessageAttachees;
			});
			AssertEquals(false, coll.AllowRemove);
		}

		public void TestReadonly()
		{
			MessageActionRelatedRecordWrapperCollection coll = new MessageActionRelatedRecordWrapperCollection(Header, delegate
			{
				return Header.MessageAttachees;
			});
			AssertEquals(true, coll.ReadOnly);
		}

		public void TestPopulateOnConstruction()
		{
			DummyIInBondWP relatedRecord2 = Factory.New<DummyIInBondWP>();
			relatedRecord2.HumanFriendlyReference = "1";
			RelatedRecord.HumanFriendlyReference = "2";
			DummyIInBondWP relatedRecord3 = Factory.New<DummyIInBondWP>();
			relatedRecord3.HumanFriendlyReference = "3";
			Header.MessageAttachees = new IMessageAttacheeInDeclaration[] { relatedRecord2, RelatedRecord, relatedRecord3 };
			MessageActionRelatedRecordWrapperCollection coll = new MessageActionRelatedRecordWrapperCollection(Header, delegate
			{
				return Header.MessageAttachees;
			});
			AssertEquals("There should be 3 items", 3, coll.Count);
		}

		public void TestSorting()
		{
			DummyIInBondWP containerRecord = Factory.New<DummyIInBondWP>();
			containerRecord.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.Container;
			DummyIInBondWP electronicInvoiceRecord = Factory.New<DummyIInBondWP>();
			electronicInvoiceRecord.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.ElectronicInvoice;
			DummyIInBondWP entry = Factory.New<DummyIInBondWP>();
			entry.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.Entry;
			DummyIInBondWP billRecord = Factory.New<DummyIInBondWP>();
			billRecord.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.Bill;
			DummyIInBondWP cargoRelease = Factory.New<DummyIInBondWP>();
			cargoRelease.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.CargoRelease;
			DummyIInBondWP declarationRecord = Factory.New<DummyIInBondWP>();
			declarationRecord.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.Declaration;
			DummyIInBondWP inbondRecord1 = Factory.New<DummyIInBondWP>();
			inbondRecord1.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.InBond;
			inbondRecord1.HumanFriendlyReference = "B";
			DummyIInBondWP inbondRecord2 = Factory.New<DummyIInBondWP>();
			inbondRecord2.RecordTypeDescription = MessageAttacheeRecordTypeDescriptions.InBond;
			inbondRecord2.HumanFriendlyReference = "A";
			Header.MessageAttachees = new IMessageAttacheeInDeclaration[] { containerRecord, electronicInvoiceRecord, entry, billRecord, cargoRelease, declarationRecord, inbondRecord1, inbondRecord2 };
			MessageActionRelatedRecordWrapperCollection coll = new MessageActionRelatedRecordWrapperCollection(Header, delegate
			{
				return Header.MessageAttachees;
			});
			AssertEquals(8, coll.Count);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.CargoRelease, coll[0].relatedRecord.RecordTypeDescription);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Entry, coll[1].relatedRecord.RecordTypeDescription);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.InBond, coll[2].relatedRecord.RecordTypeDescription);
			AssertEquals("A", coll[2].relatedRecord.HumanFriendlyReference);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.InBond, coll[3].relatedRecord.RecordTypeDescription);
			AssertEquals("B", coll[3].relatedRecord.HumanFriendlyReference);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.ElectronicInvoice, coll[4].relatedRecord.RecordTypeDescription);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Bill, coll[5].relatedRecord.RecordTypeDescription);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Container, coll[6].relatedRecord.RecordTypeDescription);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Declaration, coll[7].relatedRecord.RecordTypeDescription);
		}

		public void TestCreateMessageActionRelatedRecords()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;
			declaration.US_EntryFilerCode = "XJ5";
			AssertEquals("0 elements, because no messages attached", 0, declaration.InBondRelatedRecords.Count);
			CusContainer container = declaration.CusContainers.AddNew();
			BlockControlGenerator generator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			container.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			Factory.Save();
			AssertEquals("1 element", 1, declaration.InBondRelatedRecords.Count);
			AssertNotNull("A record exists wrapping container", declaration.InBondRelatedRecords.GetElementWrapping(container));
			Bill bill = declaration.Bills.AddNew();
			bill.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			Factory.Save();
			AssertEquals("2 elements", 2, declaration.InBondRelatedRecords.Count);
			AssertNotNull("A record exists wrapping bill", declaration.InBondRelatedRecords.GetElementWrapping(bill));
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			invoice.Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			Factory.Save();
			AssertEquals("3 elements", 3, declaration.InBondRelatedRecords.Count);
			AssertNotNull("A record exists wrapping invoice", declaration.InBondRelatedRecords.GetElementWrapping(invoice));
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("3 elements", 3, declaration.InBondRelatedRecords.Count);
			declaration.CustomsEntryHeaders[0].Messages.Add(generator.CreateMessage<MQEDIMessage>(Factory));
			Factory.Save();
			AssertEquals("4 elements", 4, declaration.InBondRelatedRecords.Count);
			AssertNotNull("A record exists wrapping invoice", declaration.InBondRelatedRecords.GetElementWrapping(declaration.CustomsEntryHeaders[0]));
			container.Delete();
			AssertEquals("3 elements", 3, declaration.InBondRelatedRecords.Count);
			AssertNull("A record should not exist wrapping container", declaration.InBondRelatedRecords.GetElementWrapping(container));
			bill.Delete();
			AssertEquals("2 elements", 2, declaration.InBondRelatedRecords.Count);
			AssertNull("A record should not exist wrapping bill", declaration.InBondRelatedRecords.GetElementWrapping(bill));
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("2 elements should have been constructed", 2, decLoaded.InBondRelatedRecords.Count);
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
			entry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
			entry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;
			entry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
			declaration.US_ITDate = ZDateTime.Today;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			Assert("ShouldBeMerged OK", declaration.DoMerge());
			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			Assert(!entry.IsActive);
			Assert(!entry.IsDeleted);
			declaration.InBondRelatedRecords.ReBuild(MessagesToShowCollection.MessagesStatus.InactiveOnly);
			AssertEquals(1, declaration.InBondRelatedRecords.Count);
			AssertEquals(1, declaration.InBondRelatedRecords[0].MessagesToShow.Count);
			declaration.InBondRelatedRecords.ReBuild(MessagesToShowCollection.MessagesStatus.ActiveOnly);
			AssertEquals(0, declaration.InBondRelatedRecords.Count);
		}

		public override void TestAddNew()
		{
			Assert("AllowNew is false for this collection and system adds elements for users", true);
		}

		public override void TestTypedAddNew()
		{
			Assert("AllowNew is false for this collection and system adds elements for users", true);
		}

		protected override MessageActionRelatedRecordWrapperCollection GetCollectionToTest()
		{
			return new MessageActionRelatedRecordWrapperCollection(Header, delegate
			{
				return Header.MessageAttachees;
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageActionRelatedRecordWrapper(RelatedRecord, MessagesToShowCollection.MessagesStatus.ActiveOnly);

		DummyIInBondWP header;
		DummyIInBondWP Header => header ?? (header = Factory.New<DummyIInBondWP>());

		DummyIInBondWP relatedRecord;
		DummyIInBondWP RelatedRecord => relatedRecord ?? (relatedRecord = Factory.New<DummyIInBondWP>());
	}
}
