using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class OperationalActionBulkAESTIRMessageSenderTest : TestCaseWithFactory
	{
		public void TestMergeAndSend()
		{
			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Export;
			job.JE_DeclarationReference = "B00001234";
			job.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			job.US_EnableENS = true;

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkAESTIRMessageSender(job);
			sender.OperationalActionSendMessage(false, log);
			AssertContains("Entry Filer ID has not been set for this branch. Please set it in the Registry > Customs > United States of America > Export > AES > Entry Filer ID.", log.MessagesString());

			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "364331434";
			filer.EntryFilerIDType = "E";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filer);
			sender.OperationalActionSendMessage(false, log);
			Assert(log.MessagesString().Contains("Job [HL B00001234]: There is no entry to send the message."));

			var invoice = job.Invoices.AddNew();
			var invoiceLine = job.InvoiceLines.AddNew();
			log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			AssertEquals(ZString.Empty, job.MergeManager.InvalidOperationText);
			AssertNotNull(job.ActiveEntryHeaders);
			AssertEquals("should send message", job.ActiveEntryHeaders[0].Messages.Count, 1);
			AssertEquals("entry status should be set to Add", job.ActiveEntryHeaders[0].CH_Status, AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse);
		}
	}
}
