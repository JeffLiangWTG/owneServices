using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDeciding_XmlMessages()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var messageInNewFactory = newFactory.Load<EDIMessage>(message.PK);
			AssertType<SGXmlEDIMessage>(messageInNewFactory);
		}

		public void TestTypeDeciding_TransmittedMessages()
		{
			Message.EM_MessageType = MessageTypeCodeList.Codes.COO;
			Message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EDIMessage ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is COODECEDIMessage);
			Message.EM_MessageType = "";
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is CUSDECEDIMessage);
		}

		public void TestTypeDeciding_ReceivedMessages()
		{
			Message.EM_MessageType = Aperak09bMessageProcessor.MessageType;
			Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EDIMessage ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is APERAKEDIMessage);
			Message.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is CUSPMTEDIMessage);
			Message.EM_MessageType = Cusres09bMessageProcessor.MessageType;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is CUSRESEDIMessage);
			Message.EM_MessageType = Debadv09bMessageProcessor.MessageType;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is DEBADVEDIMessage);
			Message.EM_MessageType = Tcodec09bMessageProcessor.MessageType;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			ediMessage = Factory.Load<EDIMessage>(Message.PK);
			Assert(ediMessage is COODCIEDIMessage);
		}

		#region Message
		CUSDECEDIMessage Message
		{
			get
			{
				if (message == null)
				{
					message = Factory.New<CUSDECEDIMessage>();
					message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				}

				return message;
			}
		}

		CUSDECEDIMessage message;
		#endregion
	}
}
