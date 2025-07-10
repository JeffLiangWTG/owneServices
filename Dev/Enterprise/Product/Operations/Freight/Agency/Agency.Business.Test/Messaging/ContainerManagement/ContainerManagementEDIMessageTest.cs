using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerManagementEDIMessage))]
	class ContainerManagementMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<ContainerManagementEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.ContainerManagement, message.EM_ApplicationCode);
			AssertEquals(ContainerManagementEDIMessage.ContainerManagementMessageType, message.EM_MessageType);
		}

		[ExpectNoExceptions]
		public void CanSaveReceiveMessage()
		{
			var message = Factory.New<ContainerManagementEDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			Factory.Save();
		}

		public void CanGenerateNumberOnReceiveMessage()
		{
			var message = Factory.New<ContainerManagementEDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.MessageNumberStrategy = new ReceivingContainerManagementNumberStrategy(Factory, "IronMan");

			Factory.Save();

			AssertEquals("1", message.EM_MessageNum);
		}
	}
}
