using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ComTracMessage))]
	sealed class ComTracMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			ComTracMessage newMessage = (ComTracMessage)GetNewBusinessObject();
			AssertEquals(ApplicationCodeList.Codes.ComTrac, newMessage.EM_ApplicationCode);
			AssertEquals(ApplicationCodeList.Codes.ComTrac, newMessage.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, newMessage.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, newMessage.EM_Status);
		}
	}
}
