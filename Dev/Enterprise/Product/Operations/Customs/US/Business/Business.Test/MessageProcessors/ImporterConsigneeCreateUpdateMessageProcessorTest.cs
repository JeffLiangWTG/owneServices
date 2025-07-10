using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ImporterConsigneeCreateUpdateMessageProcessorTest : ABIProcessorTest<ImporterConsigneeCreateUpdateMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_FullName = "IAN TEST IMPORTER 1";
			OrgCusCode cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "85-2574189AB");

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.AddNewCBPFormCBPF5106DatatotheImporterFile;
			outgoingMessage.EM_MessageText = "B  1101SV9TP                                               HYEDUSCMT_199837     T1N            IAN TEST IMPORTER 1             1701 E WOODFIELD RD             CT2                                            SCHAUMBURG           IL60173      TD                                         +18473111111                         TE1RESIDENCE                                                                    TFIAN@ABC.COM                                                                   TLXMARY SMITH                    PRESIDENT                                      Y  1101SV9TP";
			outgoingMessage.EM_LinkedObject = organisation;
			Factory.Save();

			outgoingMessage.EM_MessageNum = "HYEDUSCMT_199837";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageNum = "HYEDUSCMT_199837";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.AddNew5106toImporterFileProcessingResults;
			message.EM_MessageText =
"B003902SV9TT                                               HYEDUSCMT_199837     " +
"E0 IMPACC 000001 REF ID: 193901-14596IANTESTIMPORTER1                           " +
"E1F R04   IMPORTER NUMBER IS VOIDED                                             " +
"Y  3902SV9TT00002";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			organisation.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(organisation, message.EM_LinkedObject);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var expectedBodyMessage = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><th colspan=""4"" align=""left"">&nbsp;</th></tr><tr><td colspan=""4"">Importer Number: <strong>193901-14596</strong> Abbreviated Importer Name: <strong>IANTESTIMPORTER1</strong></td></tr><tr><th>Disposition</th><th>Condition Code</th><th>Text</th></tr><tr><td>Error</td><td>R04</td><td>IMPORTER NUMBER IS VOIDED</td></tr></table>";
			AssertContains(expectedBodyMessage, email.Body);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
