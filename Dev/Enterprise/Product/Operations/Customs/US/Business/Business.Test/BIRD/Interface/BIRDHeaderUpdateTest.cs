using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	public abstract class BIRDHeaderUpdateTest : TestCaseWithFactory
	{
		public void TestUpdate()
		{
			SetUpData();

			IBIRDHeaderRecord[] headerRecords = GetPopulatedHeaderRecords();

			foreach (IBIRDHeaderRecord headerRecord in headerRecords)
			{
				AssertSerialisationAndMessaging(headerRecord, "");
			}
		}

		public void TestPopulateUnusedFieldsAndReport()
		{
			BIRDUpdateTestToolForUntestedFields.TestUnpopulatedFields(
				GetTypeOfMessageBlock(),
				GetFieldNameToExcludeForTesting(),
				GetPopulatedHeaderRecords());
		}

		void AssertSerialisationAndMessaging(IBIRDRecord record, string message)
		{
			IBIRDHeaderRecord headerRecord = (IBIRDHeaderRecord)record;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_NoDutyCalc = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			PrepareDeclaration(declaration, headerRecord);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			NotificationBuffer notifications = new NotificationBuffer();
			headerRecord.Update(declaration, notifications);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			Assert(!notifications.HasWarnings);
			Assert(!notifications.HasErrors);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> builder = GetMessageBuilder(declaration, headerRecord);
			ZString messageTextToCompare = builder.PopulateMessage().EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, declaration.ImportEntryNumber);

			AssertContains(message, headerRecord.Serialise(), messageTextToCompare);
		}

		protected abstract IBIRDHeaderRecord[] GetPopulatedHeaderRecords();
		protected abstract Type GetTypeOfMessageBlock();
		protected abstract EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord);

		protected virtual string[] GetFieldNameToExcludeForTesting()
		{
			return Array.Empty<string>();
		}

		protected virtual void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
		}

		protected virtual void SetUpData()
		{
		}
	}
}
