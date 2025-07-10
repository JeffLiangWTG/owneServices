using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMOutboundMessageProcessorTest : OutgoingMessageProcessorTest
	{
		public void TestIsBranchFilter()
		{
			AssertEquals("IsBranchFilter", false, uemOutboundMessageProcessorForTest.GetIsBranchFilter());
		}

		public void TestMessageFilter()
		{
			AssertEquals("MessageFilter", @"EM_ApplicationCode = 'UEM' and EM_ReceiveTransmit = 'TRX' and EM_Status = 'QUE' and EM_IsActive = 1", uemOutboundMessageProcessorForTest.GetMessageFilter().LiteralTextADO);
		}

		public void TestCreateNewInterchangeProvider()
		{
			var message = Factory.New<UEMEDIMessage>();
			message.EM_GB = Env.CurrentBranchPK;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageText = "TEST MESSAGE";
			message.EM_Status = EDIMessage.Status.Queued;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message);

			var interchangeProvider = uemOutboundMessageProcessorForTest.CreateNewInterchangeProvider_Exposed(messages);
			AssertEquals("UEMEDIInterchangeProvider", typeof(UEMEDIInterchangeProvider), interchangeProvider.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			uemOutboundMessageProcessorForTest = new UEMOutboundMessageProcessorForTest(new LoggingInformation());
		}

		UEMOutboundMessageProcessorForTest uemOutboundMessageProcessorForTest;
	}

	class UEMOutboundMessageProcessorForTest : UEMOutboundMessageProcessor
	{
		public UEMOutboundMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public bool GetIsBranchFilter()
		{
			return IsBranchFilter;
		}

		public ZQuery GetMessageFilter()
		{
			return MessageFilter;
		}

		public InterchangeProviderBase CreateNewInterchangeProvider_Exposed(NonDependentEDIMessageCollection readyMessages)
		{
			return CreateNewInterchangeProvider(readyMessages);
		}
	}
}
