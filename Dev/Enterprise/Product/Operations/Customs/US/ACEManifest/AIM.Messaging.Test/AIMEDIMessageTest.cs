namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Integration;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AIMEDIMessage))]
	public class AIMEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAIMEDIMessageLoad()
		{
			var testMessage = Factory.New<AIMEDIMessage>();
			testMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = "MessageBuilder";
			testMessage.EM_MessageNum = "TEST11223344";

			AssertEquals(ApplicationCodeList.Codes.USAMA, testMessage.EM_ApplicationCode);

			Factory.Save();

			var loadedMessage = new BusinessObjectFactory().Load<EDIMessage>(testMessage.PK);
			AssertType<AIMEDIMessage>(loadedMessage);
		}

		public void TestResetToQueuedStatus()
		{
			var testMessage = Factory.New<AIMEDIMessage>();
			testMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			testMessage.EM_MessageSubType = "FER";
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			testMessage.EM_Status = EDIMessage.Status.Failed;
			testMessage.EM_MessageText = "FER";
			testMessage.EM_MessageNum = "000001";

			testMessage.ResetToQueuedStatus();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageTypeList.Codes.FHL, testMessage.EM_MessageType);
				AssertEquals("FER", testMessage.EM_MessageSubType);
			});
		}
	}
}
