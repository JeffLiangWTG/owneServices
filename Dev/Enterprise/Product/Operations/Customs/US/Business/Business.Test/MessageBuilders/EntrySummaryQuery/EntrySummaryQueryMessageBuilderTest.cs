using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntrySummaryQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEndToEndTestQuery()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "9999";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "32377722";

			var entryHeaderMessageSendingAction = new EntryHeaderMessageSendingAction(entryHeader, ImportMessageStatusList.MessageType.EntrySummary, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.EntrySummaryQuery));
			entryHeaderMessageSendingAction.US_CollectionBillInformationCode = CollectionBillInformationCodesList.Codes._3;

			var entrySummaryQueryMessageBuilder = new EntrySummaryQueryMessageBuilder(entryHeaderMessageSendingAction);
			var message = entrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("B01    XJ5JI                                               <<MSGNO PLACEHOLDER>>J1    XJ5 32377722                                                3             Y      XJ5JI00001", message.EM_MessageText);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryQuery, message.EM_MessageSubType);
			AssertCollectionContains(message, entryHeader.Messages);
			AssertNoExceptionThrown(() => Factory.Save());

			entrySummaryQueryMessageBuilder = new EntrySummaryQueryMessageBuilder(entryHeader, CollectionBillInformationCodesList.Codes._3);
			message = entrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("B01    XJ5JI                                               <<MSGNO PLACEHOLDER>>J1    XJ5 32377722                                                3             Y      XJ5JI00001", message.EM_MessageText);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryQuery, message.EM_MessageSubType);
			AssertCollectionContains(message, entryHeader.Messages);
		}

		public void TestBBlock_WhenRLF()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "9999";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "6666";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "32377722";

			var entryHeaderMessageSendingAction = new EntryHeaderMessageSendingAction(entryHeader, ImportMessageStatusList.MessageType.EntrySummary, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.EntrySummaryQuery));
			entryHeaderMessageSendingAction.US_CollectionBillInformationCode = CollectionBillInformationCodesList.Codes._3;

			var entrySummaryQueryMessageBuilder = new EntrySummaryQueryMessageBuilder(entryHeaderMessageSendingAction);
			var message = entrySummaryQueryMessageBuilder.PopulateMessage();

			AssertEquals("B016666XJ5JI                                               <<MSGNO PLACEHOLDER>>J1    XJ5 32377722                                                3             Y  6666XJ5JI00001", message.EM_MessageText);
		}
	}
}
