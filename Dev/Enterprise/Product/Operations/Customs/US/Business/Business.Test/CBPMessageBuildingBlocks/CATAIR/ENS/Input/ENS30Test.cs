using System;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS30Test : BIRDHeaderUpdateTest
	{
		public void TestImportTeamNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;
			declaration.US_NoDutyCalc = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var informalRecord = new ENS30();
			informalRecord.ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			informalRecord.SummaryCertificationCode = "0";
			informalRecord.ReleaseCertificationCode = 0;
			informalRecord.PaymentTypeIndicator = PaymentTypeList.Codes.IndividualBasis;
			informalRecord.CarrierCode = "";
			informalRecord.TeamNumber = "999";

			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)informalRecord).Update(declaration, notifications);

			Assert(!notifications.HasErrors);
			Assert(!notifications.HasWarnings);

			AssertEquals("Team number is updated", "999", declaration.US_TeamNo);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			ENS30 exwRecord = new ENS30();
			exwRecord.EntryFilerCodeOfWarehouseEntry = "ABC";
			exwRecord.WarehouseEntryNumber = "12345678";
			exwRecord.DistrictPortCodeOfWarehouseEntry = "8888";
			exwRecord.FinalWarehouseIndicator = "1";
			exwRecord.SummaryCertificationCode = "0";
			exwRecord.ReleaseCertificationCode = 1;
			exwRecord.PeriodicStatementMonth = "07";
			exwRecord.PaymentTypeIndicator = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			exwRecord.PreliminaryStatementPrintDate = new CargoWise.Types.ZDate(2009, 8, 1);
			exwRecord.CarrierCode = "ABCD";

			ENS30 nonEXWRecord = new ENS30();
			nonEXWRecord.SummaryCertificationCode = "0";
			nonEXWRecord.ReleaseCertificationCode = 0;
			nonEXWRecord.PeriodicStatementMonth = "";
			nonEXWRecord.PaymentTypeIndicator = PaymentTypeList.Codes.IndividualBasis;
			nonEXWRecord.CarrierCode = "ABCD";

			ENS30 rlfRecord = new ENS30();
			rlfRecord.SummaryCertificationCode = "1";
			rlfRecord.ReleaseCertificationCode = 0;
			rlfRecord.PeriodicStatementMonth = "";
			rlfRecord.PaymentTypeIndicator = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			rlfRecord.PreliminaryStatementPrintDate = new CargoWise.Types.ZDate(2009, 10, 1);
			rlfRecord.CarrierCode = "";
			rlfRecord.DesignatedExamPort = "3901";

			ENS30 informalRecord = new ENS30();
			informalRecord.ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			informalRecord.SummaryCertificationCode = "0";
			informalRecord.ReleaseCertificationCode = 0;
			informalRecord.PaymentTypeIndicator = PaymentTypeList.Codes.IndividualBasis;
			informalRecord.CarrierCode = "";

			return new IBIRDHeaderRecord[] { exwRecord, nonEXWRecord, rlfRecord, informalRecord };
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);

			ENS30 ens30 = (ENS30)headerRecord;
			if (!ens30.DesignatedExamPort.IsEmpty)
			{
				declaration.US_EntryMode = EntryModeList.Codes.RLF;
			}

			if (!ens30.CarrierCode.IsEmpty)
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			}

			if (!ens30.EntryFilerCodeOfWarehouseEntry.IsEmpty)
			{
				declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			}
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			ENS30 ens30 = (ENS30)headerRecord;

			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry
				, UpdateActionCode.Add
				, ens30.ReleaseCertificationCode == 1);
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS30);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"TeamNumber",//EntrySummaryMessageBuilder for lodging purpose does not populate this field at all! It should import/export the value though
				"SummaryCertificationCode",// This is not relevant for BIRD
			};
		}
	}
}
