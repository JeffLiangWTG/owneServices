using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_FER()
		{
			AssertTypeForLoad(AIMMessageSubTypes.FER, typeof(FERMessage));
		}

		public void TestGetTypeForLoad_FSC()
		{
			AssertTypeForLoad(AIMMessageSubTypes.FSC, typeof(FSCMessage));
		}

		public void TestGetTypeForLoad_FSI()
		{
			AssertTypeForLoad(AIMMessageSubTypes.FSI, typeof(FSIMessage));
		}

		public void TestGetTypeForLoad_FSN()
		{
			AssertTypeForLoad(AIMMessageSubTypes.FSN, typeof(FSNMessage));
		}

		void AssertTypeForLoad(string messageSubTypeCode, Type messageType)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USAMA;
			message.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			message.EM_MessageSubType = messageSubTypeCode;
			var row = ((INeedRow)message).Row;

			var typeDecider = new AIMEDIMessageTypeDecider();
			AssertEquals(messageType, typeDecider.GetTypeForLoad(row, Factory));
			message.MessageNumberStrategy = new TestMessageNumberStrategy(ZDateTime.Now.Millisecond.ToString());
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reLoadedMessage = newFactory.Load<EDIMessage>(message.PK);
			AssertType(messageType, reLoadedMessage);
		}

		class TestMessageNumberStrategy : IMessageNumberStrategy
		{
			internal TestMessageNumberStrategy(string messageNumber)
			{
				this.messageNumber = messageNumber;
			}

			#region Implementation of IMessageNumberStrategy

			public string GetMessageReferenceNumber()
			{
				return messageNumber;
			}

			#endregion

			readonly string messageNumber;
		}
	}
}
