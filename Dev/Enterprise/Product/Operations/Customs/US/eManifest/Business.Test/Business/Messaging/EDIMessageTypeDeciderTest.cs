using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new EDIMessageTypeDecider();
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			AssertMessageTypeDecided(typeDecider, message, MessageTypes.Codes.SyntaxError, typeof(SyntaxErrorMessage));
			AssertMessageTypeDecided(typeDecider, message, string.Empty, typeof(EDIMessage));
		}

		void AssertMessageTypeDecided(EDIMessageTypeDecider typeDecider, EDIMessage message, string messageType, Type expectedType)
		{
			message.EM_MessageType = messageType;
			AssertEquals(expectedType, typeDecider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			Factory.Save();
			var reLoadedMessage = new BusinessObjectFactory().Load<Enterprise.Messaging.Business.EDIMessage>(message.PK);
			AssertEquals(expectedType, reLoadedMessage.GetType());
		}
	}
}
