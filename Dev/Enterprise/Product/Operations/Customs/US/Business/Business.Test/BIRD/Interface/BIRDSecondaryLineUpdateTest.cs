using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	public abstract class BIRDSecondaryLineUpdateTest : TestCaseWithFactory
	{
		public void TestUpdateLine()
		{
			IBIRDSecondaryLineRecord[] lineRecords = GetPopulatedSecondaryLineRecords();

			foreach (IBIRDSecondaryLineRecord lineRecord in lineRecords)
			{
				AssertSerialisationAndMessaging(lineRecord, "");
			}
		}

		public void TestPopulateUnusedFieldsAndReport()
		{
			BIRDUpdateTestToolForUntestedFields.TestUnpopulatedFields(
				GetTypeOfMessageBlock(),
				GetFieldNameToExcludeForTesting(),
				GetPopulatedSecondaryLineRecords());
		}

		void AssertSerialisationAndMessaging(IBIRDRecord record, string message)
		{
			IBIRDSecondaryLineRecord lineRecord = (IBIRDSecondaryLineRecord)record;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_NoDutyCalc = true;
			declaration.ImportEntryNumber = "12345678";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();

			PrepareData(declaration, invoice, secondaryLine, lineRecord);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			NotificationBuffer notifications = new NotificationBuffer();
			lineRecord.Update(secondaryLine, notifications);

			Assert(!notifications.HasWarnings);
			Assert(!notifications.HasErrors);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> builder = GetMessageBuilder(declaration);
			ZString messageTextToCompare = builder.PopulateMessage().EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, declaration.ImportEntryNumber);

			AssertContains(message, lineRecord.Serialise(), messageTextToCompare);
		}

		protected virtual void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine secondaryLine, IBIRDSecondaryLineRecord lineRecord)
		{
		}

		protected virtual string[] GetFieldNameToExcludeForTesting()
		{
			return Array.Empty<string>();
		}

		protected abstract IBIRDSecondaryLineRecord[] GetPopulatedSecondaryLineRecords();
		protected abstract Type GetTypeOfMessageBlock();
		protected abstract EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration);
	}
}
