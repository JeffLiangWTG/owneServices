using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class NZCMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeForLoad()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			var message1 = Factory.New<NZCMessage>();
			message1.EM_MessageType = NZCMessage.MessageTypes.OutwardReport.MessageType;
			message1.EM_MessageNum = "1";
			message1.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();
			EDIMessage messageToTest = Factory.Load<EDIMessage>(message1.PK);
			AssertEquals("CUSMOD Outward Report message", typeof(OutwardReportMessage), messageToTest.GetType());

			var message2 = Factory.New<TSWMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.OCR;
			message2.EM_MessageNum = "2";
			message2.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();
			messageToTest = Factory.Load<EDIMessage>(message2.PK);
			AssertEquals("Trade Single Window OCR message", typeof(TSWMessage), messageToTest.GetType());

			var message3 = Factory.New<TSWMessage>();
			message3.EM_MessageType = MessageTypeList.Codes.I10;
			message3.EM_MessageNum = "3";
			message3.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();
			messageToTest = Factory.Load<EDIMessage>(message3.PK);
			AssertEquals("Trade Single Window Import (I10) message", typeof(TSWMessage), messageToTest.GetType());

			var message5 = Factory.New<TSWMessage>();
			message5.EM_MessageType = MessageTypeList.Codes.E40;
			message5.EM_MessageNum = "5";
			message5.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();
			messageToTest = Factory.Load<EDIMessage>(message5.PK);
			AssertEquals("Trade Single Window Export (E40) message", typeof(TSWMessage), messageToTest.GetType());

			var message6 = Factory.New<OutwardReportMessage>();
			message6.EM_MessageType = NZCMessage.MessageTypes.OutwardReport.MessageType;
			message6.EM_MessageNum = "6";
			message6.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();
			messageToTest = Factory.Load<EDIMessage>(message6.PK);
			AssertEquals("Legacy Outward Report message", typeof(OutwardReportMessage), messageToTest.GetType());

			var message7 = Factory.New<NZCMessage>();
			message7.EM_MessageType = NZCMessage.MessageTypes.FormalEntry.MessageType;
			message7.EM_MessageNum = "1";
			message7.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();
			messageToTest = Factory.Load<EDIMessage>(message7.PK);
			AssertEquals("CUSMOD message", typeof(NZCMessage), messageToTest.GetType());
		}
	}
}
