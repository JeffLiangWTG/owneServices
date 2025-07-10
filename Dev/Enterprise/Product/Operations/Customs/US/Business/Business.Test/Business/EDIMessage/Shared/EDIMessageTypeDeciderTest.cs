using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			AssertGetTypeForLoad<MQEDIMessage, Enterprise.Messaging.Business.EDIMessage>(string.Empty, "EMP20190101");
			AssertGetTypeForLoad<AESTIREDIMessage, AESTIREDIMessage>(EDIMessage.ApplicationCodes.USCustomsExport, "USE20190101");
			AssertGetTypeForLoad<MQEDIMessage, MQEDIMessage>(EDIMessage.ApplicationCodes.USCustomsImport, "USI20190101");
			AssertGetTypeForLoad<EBondEDIMessage, EBondEDIMessage>(EDIMessage.ApplicationCodes.USeBond, "UXB20190101");
		}

		public void TestGetTypeForAMS()
		{
			var typeDecider = new EDIMessageTypeDecider();
			var message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			AssertNoExceptionThrown(() => _ = message.EM_MessageInterpretation);
			var row = ((INeedRow)message).Row;
			var amsTypeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.US.USAMS.IEDIMessageTypeDecider>();
			AssertEquals("Status Notification message", amsTypeDecider.GetTypeForLoad(row, Factory), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestMessageForInBondStatusNotification()
		{
			var message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "ACR          RC12062421122616241                                                M01AAAD30DEINGOLSTADT             001A      000001                              R01AAAD3901INGOLSTADT             001A 000001120624                             J01AAAD                                                                         R02CW1001      69000000001261333210150      1206242112             1            R023901    3909                                                                 B04SNPOTT1                                                                      R03BILL ON FILE                                                                 R05NC                                                                           ZCR          RC                   00008";
			var row = ((INeedRow)message).Row;
			var amsTypeDecider = (TypeDecider)ObjectFactory.Get<Integration.Customs.US.USAMS.IEDIMessageTypeDecider>();
			var type = amsTypeDecider.GetTypeForLoad(row, Factory);
			var factory2 = new BusinessObjectFactory();
			AssertNoExceptionThrown(() => factory2.Load(type, message.PK));
		}

		void AssertGetTypeForLoad<T, M>(string applicationCode, string messageNumber)
		{
			var factory = NewFactory();
			var ediMessage = factory.NewWithValidTestData<EDIMessage>();
			ediMessage.EM_ApplicationCode = applicationCode;
			ediMessage.MessageNumberStrategy = new TestMessageNumberStrategy(messageNumber);
			var typeDecider = new EDIMessageTypeDecider();
			var expectedType = typeof(T);
			var actualType = typeDecider.GetTypeForLoad(((INeedRow)ediMessage).Row, factory);
			var message = $@"The type should be {expectedType.Name} when the application code is ""{applicationCode}"" for getting type from EDIMessageTypeDecider.";
			AssertEquals(message, expectedType, actualType);
			factory.Save();
			var newFactory = NewFactory();
			var messageInNewFactory = newFactory.Load<Enterprise.Messaging.Business.EDIMessage>(ediMessage.PK);
			expectedType = typeof(M);
			actualType = messageInNewFactory.GetType();
			message = $@"The type should be {expectedType.Name} when the application code is ""{applicationCode}"" for loading in a new facotry.";
			AssertEquals(message, expectedType, actualType);
		}

		sealed class TestMessageNumberStrategy : IMessageNumberStrategy
		{
			internal TestMessageNumberStrategy(string messageNumber)
			{
				this.messageNumber = messageNumber;
			}

			string IMessageNumberStrategy.GetMessageReferenceNumber() => messageNumber;

			readonly string messageNumber;
		}
	}
}
