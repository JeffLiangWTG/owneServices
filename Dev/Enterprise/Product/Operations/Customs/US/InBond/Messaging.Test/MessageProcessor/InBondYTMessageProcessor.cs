using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;

namespace Enterprise.Customs.US.InBond.Messaging.MessageProcessor.Testing
{
	sealed class InBondYTMessageProcessorTest : ABIProcessorTest<InBondYTMessageProcessor, APLA, APLB, APLY>
	{
		public void TestRejectedReponses()
		{
			request.EM_MessageText = @"B013901SV9WX                                               115373               1013332092~~                                                                    201112051805005301                                          AZ 63A  12031140    Y  3901SV9WX00002";
			response.EM_MessageText = @"B013901SV9YT                                               115373               1013332092~~                                                                    201112051805005301                                          AZ 63A  12031140    9501030INVALID FLIGHT NUMBER                                                    Y  3901SV9YT00002                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("DATA ACCEPTANCE", sentMail.Body);
			AssertContains("Air In-bond Arrival/Exportation Response (Failure) for INB00000~~ / 3332092~~", sentMail.Subject);
		}

		public void TestNarrativeNotes()
		{
			request.EM_MessageText = @"B013901SV9WX                                               115373               1013332092~~                                                                    201112051805005301                                          AZ 63A  12031140    Y  3901SV9WX00002";
			response.EM_MessageText = @"B011512DSVYT                                               DFDEWRPRD_623427     101053234171                                                                    201305161114002402                                          LH 446  05121340    9502358DATA ACCEPTANCE                         -UPDTD #BILLS: 00001             Y  1512DSVYT00002";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>DATA ACCEPTANCE</td><td>-UPDTD #BILLS: 00001</td></tr>", sentMail.Body);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "545011";
			response.EM_MessageText = @"B011512DSVYT                                               DFDEWRPRD_696840     101053243190                                                                    201308061504002402                                          LH 8202 08041340    9501463INBOND ALREADY ARRIVED                  0205905023500MUC0021098          Y  1512DSVYT00002";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>INBOND ALREADY ARRIVED</td><td>0205905023500MUC0021098</td>", sentMail.Body);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "545011";
			response.EM_MessageText = @"B013801791YT                                03             ROBMSPPRD_95763      101533793713                                                                    201305270000002304                                          UA 059  06041340    9501462INVALID ACTUAL ARRIVAL/EXPORT DATETIME  01634221681129268692WAW          Y  3801791YT00002";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>INVALID ACTUAL ARRIVAL/EXPORT DATETIME</td><td>01634221681129268692WAW</td>", sentMail.Body);
		}

		protected override void EndToEndCore()
		{
			request.EM_MessageText = @"B013901SV9WX                                               115361               1013332092~~                                                                    201112010122005301                                          AZ 636  12031140    Y  3901SV9WX00002";
			response.EM_MessageText = @"B013901SV9YT                                               115361               1013332092~~                                                                    201112010122005301                                          AZ 636  12031140    9502358DATA ACCEPTANCE                                                          Y  3901SV9YT00002                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("DATA ACCEPTANCE", sentMail.Body);
			AssertContains("Air In-bond Arrival/Exportation Response for INB00000~~ / 3332092~~", sentMail.Subject);
		}

		MQEDIMessage request;
		MQEDIMessage response;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			header.BH_JobReference = "INB00000~~";
			moveHeader.InBondNumber = "3332092~~";

			var mock = Factory.NewMoq<MQEDIMessage>();

			request = mock.Object;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNotice;
			request.EM_MessageNum = "545011";
			request.EM_LinkedObject = moveHeader as BusinessObject;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondArrival;

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "545011";
			response.EM_LinkedObject = moveHeader as BusinessObject;
		}
	}
}
