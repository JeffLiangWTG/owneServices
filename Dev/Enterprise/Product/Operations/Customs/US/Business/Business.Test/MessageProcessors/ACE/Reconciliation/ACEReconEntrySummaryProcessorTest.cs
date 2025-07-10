using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACEReconEntrySummaryProcessorTest : ABIProcessorTest<ACEReconEntrySummaryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDeclaration = new ReconDeclaration(declaration);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText =
"B  2501REXRE                                                                    " +
"10ASV9  70042864 0401B00167372 X 58 - 12345678920 - 150745600891 CE3USN         " +
"11QING(SUNNIE) YU + 18473111111   SUNNIE.YU @WISETECHGLOBAL.COM                 " +
"20SV9  70042849                                                                 " +
"2100100000000000 49900000000000 10700000000000 01600000000000                   " +
"509801001015HK   081916                                                         " +
"519102117030 9801001015 9101291030 9801001015                                   " +
"519102192040                                                                    " +
"52SV9  70042849  1                                                              " +
"539801001029 0000001000 00000000000                                             " +
"549102117030 0000000900 00000000000 9801001029                                  " +
"549101291030 0000001900 00000000000 9801001029                                  " +
"549102192040 0000002000 00000000000                                             " +
"5500100000000000 499            107            016                              " +
"901        12                                                                   " +
"9100100000000000 49900000000000 10700000000000 01600000000000                   " +
"9200100000000000 49900000000000 10700000000000 01600000000000                   " +
"Y  0401REXRE00016";

			var reconEntry = reconDeclaration.ReconEntry.GetEntry();

			outgoingmessage.EM_LinkedObject = reconEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			message.EM_MessageText =
"B  2501REXRX                                                                    " +
"E0 RECONS 000001 REF ID: REX 22100055 001577581                                 " +
"E1A 995   RECON HAS BEEN ADDED                    REX  22100055     001577581   " +
"Y  2501REXRX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.Reload();
			reconEntry.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(reconEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", ReconMessageStatusList.Codes.ClearReconOriginal, declaration.JE_MessageStatus);
			AssertEquals("Status changed", ReconMessageStatusList.Codes.ClearReconOriginal, reconEntry.CH_Status);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>995</td><td>RECON HAS BEEN ADDED</td>", email.Body);
		}

		public void TestMessageRejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDeclaration = new ReconDeclaration(declaration);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummary;
			outgoingmessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText =
"B  2501REXRE                                                                    " +
"10ASV9  70042864 0401B00167372 X 58 - 12345678920 - 150745600891 CE3USN         " +
"11QING(SUNNIE) YU + 18473111111   SUNNIE.YU @WISETECHGLOBAL.COM                 " +
"20SV9  70042849                                                                 " +
"2100100000000000 49900000000000 10700000000000 01600000000000                   " +
"509801001015HK   081916                                                         " +
"519102117030 9801001015 9101291030 9801001015                                   " +
"519102192040                                                                    " +
"52SV9  70042849  1                                                              " +
"539801001029 0000001000 00000000000                                             " +
"549102117030 0000000900 00000000000 9801001029                                  " +
"549101291030 0000001900 00000000000 9801001029                                  " +
"549102192040 0000002000 00000000000                                             " +
"5500100000000000 499            107            016                              " +
"901        12                                                                   " +
"9100100000000000 49900000000000 10700000000000 01600000000000                   " +
"9200100000000000 49900000000000 10700000000000 01600000000000                   " +
"Y  0401REXRE00016";

			var reconEntry = reconDeclaration.ReconEntry.GetEntry();
			outgoingmessage.EM_LinkedObject = reconEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "~15000";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			message.EM_MessageText =
"B  2501REXRX                                                                    " +
"E0 RECONS 000001 REF ID: REX 22100055 001577581                                 " +
"E1RF995   TRANSACTION DATA REJECTED               REX  22100055     001577581   " +
"Y  2501REXRX00002";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			declaration.Reload();
			reconEntry.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(reconEntry, message.EM_LinkedObject);
			AssertEquals("Status changed", ReconMessageStatusList.Codes.ErrorReconOriginal, declaration.JE_MessageStatus);
			AssertEquals("Status changed", ReconMessageStatusList.Codes.ErrorReconOriginal, reconEntry.CH_Status);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<tr><td>Rejected</td><td>995</td><td>TRANSACTION DATA REJECTED</td></tr>", email.Body);
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
