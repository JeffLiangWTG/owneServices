using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class PGACorrectionMessageProcessorTest : ABIProcessorTest<PGACorrectionMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9CC                                               HYEDUSCMT_165024     " +
"CA10RSV971007882                                                                " +
"CA4000001                                                                       " +
"CA608471704065                                                                  " +
"CA9002   DATA ACCEPTED                                                          " +
"OI        TEST PGA CORRECTION RESPONSE                                          " +
"PG01001FDAFOOFEE                                                                " +
"PG02PFDP                                                                        " +
"PG06262FR                                                                       " +
"PG07TEST FOOD                                                                   " +
"PG10                   SOMETHING                                                " +
"PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    " +
"PG20ALBION  QLD                                                  AU4010         " +
"PG25          1                                                                 " +
"OI        PLYWOOD                                                               " +
"PG01001APHAPL                                                                   " +
"PG02P                                                                           " +
"PG10                   MORE PLYWOOD                                             " +
"PG04 CONST 1 NAME                                       000000100000M3   0500000" +
"PG05GENUS                 SPECIES                                               " +
"PG06HRVHK                                                                       " +
"PG04 CONST 2 NAME                                       000000250000M3   0500000" +
"PG05GENUS2                SPECIES2                                              " +
"PG06HRVCN                                                                       " +
"PG22             IM AP6 Y06242016                                               " +
"PG21IM FIRST LASTNAME         2156661212     FIRST.LASTNAME@TTSMAIL.COM         " +
"PG25                                                    000000010000            " +
"Y  3901SV9CC";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>DATA ACCEPTED</td>", email.Body);
			AssertEquals(PGACorrectionStatusList.Codes.ClearPGADataCorrection, declaration.US_PGACorrectionStatus);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "SV9";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "71007882";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoing = mock.Object;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_Status = "SNT";
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PGADataCorrection;
			outgoing.EM_MessageNum = "HYEDUSCMT_165024";
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(outgoing);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse;
			incomingMessage.EM_MessageNum = "HYEDUSCMT_165024";
		}
		MQEDIMessage incomingMessage;
		MQEDIMessage outgoing;
		JobDeclaration declaration;
	}
}
