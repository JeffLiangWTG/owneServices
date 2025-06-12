using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.eLMS
{
	[TestClass]
	public class eLMSReplyMessageBuilderTests
	{
		[TestMethod]
		public void eLMSReplyMessageBuilderValidInputSuccess()
		{
			var context = new TestMessageContext("", "Sender1", "Recepient1");
			context.AddContext("ShipmentNumber", "Shipment 1");

			var builder = new eLMSReplyMessageBuilder(context, new ServiceReply(ServiceReply.Action.Success, "3400"));
			var result = builder.GetReply();
			Assert.AreEqual("<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>Recepient1</SenderId><RecepientId>Sender1</RecepientId><TargetBOType>ForwardingShipment</TargetBOType><TargetBOKey>Shipment 1</TargetBOKey><EventTypeCode>MSC</EventTypeCode><Reference>Message processing reference - 3400</Reference><ContextCollection /></ClientDeliveryNotification>", result);
		}
	}
}
