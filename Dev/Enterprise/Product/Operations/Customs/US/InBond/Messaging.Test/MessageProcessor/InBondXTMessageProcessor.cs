using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;

namespace Enterprise.Customs.US.InBond.Messaging.MessageProcessor.Testing
{
	sealed class InBondXTMessageProcessorTest : ABIProcessorTest<InBondXTMessageProcessor, APLA, APLB, APLY>
	{
		public void TestRejectedReponses()
		{
			request.EM_MessageText = @"B013901SV9QX                                               115359               10A61333209203   TOWE5301     0023232335-215404600                              20AZ  40                         AZ636       3901120211                         30 Y        05564654214                                                         Y  3901SV9QX00003";
			response.EM_MessageText = @"B013901SV9XT                                               115359               10A61333209203   TOWE5301     0023232335-215404600                              20AZ  40                         AZ636       3901120211                         9501030INVALID FLIGHT NUMBER                                                    Y  3901SV9XT00003                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("DATA ACCEPTANCE", sentMail.Body);
			AssertContains("Air In-bond Departure Response (Failure) for INB00000~~ / 3332092~~", sentMail.Subject);
		}

		public void TestChangeFromPendingToQueWhenCleared()
		{
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			response.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			response.EM_MessageText = @"B013901SV9XT                                               115360               10A61333209214   TOWE5301     0023232335-215404600                              20AZ  40                         636         3901120211                         30 Y        05564654214                                                         9502358DATA ACCEPTANCE                                                          Y  3901SV9XT00003                                                               ";

			var moveHeader2 = (CusInBondMoveHeader)moveHeader;
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;

			var pendingMessage = Factory.New<MQEDIMessage>();
			pendingMessage.EM_LinkedObject = moveHeader2;
			pendingMessage.EM_Status = EDIMessage.Status.Pending;
			pendingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			pendingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			pendingMessage.EM_MessageText = "A " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			pendingMessage.Reload();
			moveHeader2.Reload();

			AssertEquals("Linked to the entry", moveHeader2, response.EM_LinkedObject);
			AssertEquals("Pending message status should be changed to QUE", EDIMessage.Status.Queued, pendingMessage.EM_Status);
			AssertEquals("Status of entry", ImportMessageStatusList.Codes.AwaitingDepartureOriginal, moveHeader2.BM_CustomsStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains(InBondPendingMessageManager.InBondPendingMessagesQueuedBody));
		}

		public void TestChangeFromPendingToCancelledWhenNotCleared()
		{
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			response.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			response.EM_MessageText = @"B013901SV9XT                                               115359               10A61333209203   TOWE5301     0023232335-215404600                              20AZ  40                         AZ636       3901120211                         9501030INVALID FLIGHT NUMBER                                                    Y  3901SV9XT00003                                                               ";

			var moveHeader2 = (CusInBondMoveHeader)moveHeader;
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;

			var pendingMessage = Factory.New<MQEDIMessage>();
			pendingMessage.EM_LinkedObject = moveHeader2;
			pendingMessage.EM_Status = EDIMessage.Status.Pending;
			pendingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			pendingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			pendingMessage.EM_MessageText = "A " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			pendingMessage.Reload();
			moveHeader2.Reload();

			AssertEquals("Linked to the entry", moveHeader2, response.EM_LinkedObject);
			AssertEquals("Pending message status should be changed to QUE", EDIMessage.Status.Cancelled, pendingMessage.EM_Status);
			AssertEquals("Status of entry", ImportMessageStatusList.Codes.ErrorDepartureWithdraw, moveHeader2.BM_CustomsStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains(InBondPendingMessageManager.InBondPendingMessagesCancelledBody));
		}

		protected override void EndToEndCore()
		{
			request.EM_MessageText = @"B013901SV9QX                                               115360               10A61333209214   TOWE5301     0023232335-215404600                              20AZ  40                         636         3901120211                         30 Y        05564654214                                                         Y  3901SV9QX00003";
			response.EM_MessageText = @"B013901SV9XT                                               115360               10A61333209214   TOWE5301     0023232335-215404600                              20AZ  40                         636         3901120211                         30 Y        05564654214                                                         9502358DATA ACCEPTANCE                                                          Y  3901SV9XT00003                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("DATA ACCEPTANCE", sentMail.Body);
			AssertContains("Air In-bond Departure Response for INB00000~~ / 3332092~~", sentMail.Subject);
		}

		MQEDIMessage request;
		MQEDIMessage response;
		Integration.Customs.US.InBond.ICusInBondHeader header;
		Integration.Customs.US.InBond.ICusInBondMoveHeader moveHeader;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			header.BH_JobReference = "INB00000~~";
			moveHeader.InBondNumber = "3332092~~";

			var mock = Factory.NewMoq<MQEDIMessage>();

			request = mock.Object;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbond;
			request.EM_MessageNum = "545011";
			request.EM_LinkedObject = moveHeader as BusinessObject;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.AirInbondResponse;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "545011";
		}
	}
}
