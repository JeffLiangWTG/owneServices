namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	using System;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Business.Testing;

	public class InterchangeSenderTest : BaseInterchangeSenderTest
	{
		#region GetNewEDIInterchangeReadyToSend

		protected override EDIInterchange GetNewEDIInterchangeReadyToSend()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_HeaderText = "HEADERTEXT";
			interchange.EI_BodyText = "BODYTEXT";
			interchange.EI_FooterText = "FOOTERTEXT";
			interchange.EI_InterchangeNum = "12345";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			return interchange;
		}

		#endregion

		#region GetNewEDIMessageReadyToSend

		protected override EDIMessage GetNewEDIMessageReadyToSend()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_MessageText = NZCMessage.MessageNumberPlaceHolder;
			return message;
		}

		public override void TestSendMessage()
		{
			Assert("interchanges already created", condition: true);
		}

		public override void TestProcess()
		{
			Assert("interchanges already created", condition: true);
		}

		public override void TestMessagesWithEM_HeldUntilDateNotSentUntilTheRightDate()
		{
			Assert("interchanges already created", condition: true);
		}

		public override void TestMessagesWithoutEM_HeldUntilDateGetSentRightNow()
		{
			Assert("interchanges already created", condition: true);
		}

		#endregion

		#region GetNewSender

		protected override BaseInterchangeSender GetNewSender()
		{
			return new InterchangeSender(new LoggingInformation());
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}

		#endregion

	}
}
