using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CusEntryHeaderLinkerTest : TestCaseWithFactory
	{
		public void TestLinkWithEntryNumber()
		{
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "8282");
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryFilerCode = "535";

			CusEntryHeader entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = "~1";

			CusEntryHeader entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry2.EntryNumber = "~1";

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "646";

			CusEntryHeader entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry3.EntryNumber = "~1";

			MQEDIMessage message = Factory.New<MQEDIMessage>();

			AssertEquals("entry1 is linked", entry1, CusEntryHeaderLinker.Link("~1", "535", message, CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertEquals("entry1 is linked", entry1, message.EM_LinkedObject);

			AssertEquals("entry3 is linked", entry3, CusEntryHeaderLinker.Link("~1", "646", message, CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			AssertEquals("entry3 is linked", entry3, message.EM_LinkedObject);
		}
	}
}
