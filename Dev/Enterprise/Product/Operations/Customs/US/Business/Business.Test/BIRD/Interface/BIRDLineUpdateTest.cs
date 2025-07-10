using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	public abstract class BIRDLineUpdateTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 12)]
		public virtual void TestUpdateLine()
		{
			IBIRDLineRecord[] lineRecords = GetPopulatedLineRecords();

			foreach (IBIRDLineRecord lineRecord in lineRecords)
			{
				AssertSerialisationAndMessaging(lineRecord, "");
			}
		}

		public void TestPopulateUnusedFieldsAndReport()
		{
			BIRDUpdateTestToolForUntestedFields.TestUnpopulatedFields(
				GetTypeOfMessageBlock(),
				GetFieldNameToExcludeForTesting(),
				GetPopulatedLineRecords());
		}

		void AssertSerialisationAndMessaging(IBIRDRecord record, string message)
		{
			IBIRDLineRecord lineRecord = (IBIRDLineRecord)record;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_NoDutyCalc = true;
			declaration.ImportEntryNumber = "12345678";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			PrepareData(declaration, invoice, invoiceLine, lineRecord);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			NotificationBuffer notifications = new NotificationBuffer();
			lineRecord.Update(invoiceLine, notifications);

			Assert(!notifications.HasWarnings);
			Assert(!notifications.HasErrors);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> builder = GetMessageBuilder(declaration);
			ZString messageTextToCompare = builder.PopulateMessage().EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, declaration.ImportEntryNumber);

			AssertContains(message, lineRecord.Serialise(), messageTextToCompare);
		}

		protected virtual void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
		}

		protected virtual string[] GetFieldNameToExcludeForTesting()
		{
			return new string[] { "MiscellaneousIndicator" };
		}

		protected abstract IBIRDLineRecord[] GetPopulatedLineRecords();
		protected abstract Type GetTypeOfMessageBlock();
		protected abstract EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration);

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
