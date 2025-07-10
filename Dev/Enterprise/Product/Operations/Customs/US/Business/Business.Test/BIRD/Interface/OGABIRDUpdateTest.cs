using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	public abstract class OGABIRDUpdateTest : TestCaseWithFactory
	{
		protected abstract IBIRDOGALineRecord[] GetPopulatedOGARecords();
		protected abstract Type GetTypeOfMessageBlock();
		protected abstract IOGALine CreateOGALine(JobComInvoiceLine invoiceLine);
		protected virtual string[] GetFieldNameToExcludeForTesting()
		{
			return Array.Empty<string>();
		}

		protected virtual void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
		}

		public virtual void TestUpdate()
		{
			foreach (IBIRDOGALineRecord ogaRecord in GetPopulatedOGARecords())
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EnableENS = true;

				declaration.Invoices.AddNew();

				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				IOGALine ogaLine = CreateOGALine(invoiceLine);

				PrepareJob(invoiceLine, ogaLine, ogaRecord);

				NotificationCollection notifications = new NotificationCollection();
				ogaRecord.Update(ogaLine, notifications);

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true);
				MQEDIMessage message = builder.PopulateMessage();

				AssertContains(ogaRecord.Serialise(), message.EM_MessageText);
			}
		}

		public void TestPopulateUnusedFieldsAndReport()
		{
			BIRDUpdateTestToolForUntestedFields.TestUnpopulatedFields(
				GetTypeOfMessageBlock(),
				GetFieldNameToExcludeForTesting(),
				GetPopulatedOGARecords());
		}
	}
}
