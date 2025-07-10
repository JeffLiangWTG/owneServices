using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new EDIMessageTypeDecider();
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.AMS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var row = ((INeedRow)message).Row;
			AssertEquals("Default Message", typeof(AMSEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = "";
			AssertEquals("Message", typeof(AMSEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reLoadedMessage = newFactory.Load(typeof(EDIMessage), message.PK);
			AssertEquals(typeof(AMSEDIMessage), reLoadedMessage.GetType());
		}
	}
}
