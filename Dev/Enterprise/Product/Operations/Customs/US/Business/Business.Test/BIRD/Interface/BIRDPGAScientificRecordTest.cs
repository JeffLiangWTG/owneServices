using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	public abstract class BIRDPGAScientificRecordTest : TestCaseWithFactory
	{
		public void TestUpdate()
		{
			IBIRDPGAScientificRecord[] records = GetPopulatedRecords();

			foreach (IBIRDPGAScientificRecord record in records)
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();

				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EnableENS = true;

				declaration.Invoices.AddNew();

				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

				PGA pgaLine = invoiceLine.LaceyActLines.AddNew();
				ConstituentElement constituentElement = pgaLine.PG04ConstituentElements.AddNew();

				PrepareData(declaration, invoiceLine, constituentElement, record);

				NotificationBuffer notifications = new NotificationBuffer();
				record.Update(constituentElement, notifications);

				Assert(!notifications.HasWarnings);
				Assert(!notifications.HasErrors);

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true);
				ZString messageTextToCompare = builder.PopulateMessage().EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, declaration.ImportEntryNumber);
				AssertContains(record.Serialise(), messageTextToCompare);
			}
		}

		public void TestPopulateUnusedFieldsAndReport()
		{
			BIRDUpdateTestToolForUntestedFields.TestUnpopulatedFields(
				GetTypeOfMessageBlock(),
				GetFieldNameToExcludeForTesting(),
				GetPopulatedRecords());
		}

		protected virtual void PrepareData(JobDeclaration declaration, JobComInvoiceLine invoiceLine, ConstituentElement constituentElement, IBIRDPGAScientificRecord lineRecord)
		{
		}

		protected abstract IBIRDPGAScientificRecord[] GetPopulatedRecords();
		protected abstract Type GetTypeOfMessageBlock();
		protected virtual string[] GetFieldNameToExcludeForTesting()
		{
			return Array.Empty<string>();
		}
	}
}
