using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	[TestedType(typeof(WDFEDIMessage))]
	public class WDFEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("Application Code", EDIMessage.ApplicationCodes.WarehouseDocket, Message.EM_ApplicationCode);
			AssertEquals("Receive Transmit", EDIMessage.Direction.Receive, Message.EM_ReceiveTransmit);
			AssertEquals("Status", EDIMessage.Status.Received, Message.EM_Status);
			AssertEquals("TestMessage", false, Message.EM_IsTestMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Message = Factory.New<WDFEDIMessage>();
		}

		WDFEDIMessage Message;
	}
}
