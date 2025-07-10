using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Test
{
	sealed class BookingMessageActionTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			Assert("Ensure database is clean", Factory.Load<CommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000")).IsNullOrEmpty());
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageType = EDIMessage.ApplicationCodes.XMS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.ShipmentBookings;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "00000000000000000101";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				message.EM_MessageText = resourceRetriever.GetString("Enterprise.Freight.QuotedBookings.DataTransfer.Test.MessageAction.TestFiles.ShipmentBookings.xml");
			}
			Factory.Save();
			var processor = new StandardXMLMessageServiceTask();
			processor.ServiceLogger = new DummyLogger();
			processor.RunTask();
			AssertEquals("Booking(Shipment) created from Native XML", true, Factory.Load<CommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000")).Length > 0);
		}
	}
}
