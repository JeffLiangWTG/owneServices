using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.eTail.Business.Testing
{
	sealed class HVLVEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.AirCargoAdvanceScreening;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageType = "XDC";
			message.EM_MessageSubType = "XUS";

			var row = ((INeedRow)message).Row;
			var typeDecider = new HVLVEDIMessageTypeDecider();
			AssertEquals(typeof(XmlEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
