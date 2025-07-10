using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BIRDEntrySummaryQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEndToEndTestQuery()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "9999";
			declaration.SetExternalBrokerForTesting();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "32377722";

			BIRDEntrySummaryQueryMessageBuilder builder = new BIRDEntrySummaryQueryMessageBuilder(entry);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			AssertContains("AAJI  XJ5", message.EM_FormattedMessageText);
			AssertContains("J1XJ5 323777223", message.EM_FormattedMessageText);
			AssertContains("ZZJI  000000001", message.EM_FormattedMessageText);

			AssertCollectionContains(message, entry.Messages);
			AssertNoExceptionThrown(() => Factory.Save());
			Assert(message.IsBIRDTransaction);
			AssertEquals(EDIMessage.Status.Pending, message.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.BIRDEntrySummaryQueryInput, message.EM_MessageSubType);
		}
	}
}
