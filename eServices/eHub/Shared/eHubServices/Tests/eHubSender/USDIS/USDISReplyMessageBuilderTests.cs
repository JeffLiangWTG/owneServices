using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.eLMS
{
	[TestClass]
	public class USDISReplyMessageBuilderTests : BaseTest
	{
		[TestMethod]
		public void USDISReplyMessageBuilder_Success()
		{
			var builder = new USDISReplyMessageBuilder(null, new ServiceReply(ServiceReply.Action.Success, "<iws:ResponseMessage xmlns:iws='http://iws.cbp.dhs.gov/ITDSServices/IWS'><ReturnCode>SUCCESS</ReturnCode><ReturnRemark>Message received successfully by CBP</ReturnRemark></iws:ResponseMessage>"));
			var result = builder.GetReply();
			Assert.AreEqual(null, result);
		}

		[TestMethod]
		public void USDISReplyMessageBuilder_Error()
		{
			var builder = new USDISReplyMessageBuilder(null, new ServiceReply(ServiceReply.Action.Success, "<iws:ResponseMessage xmlns:iws='http://iws.cbp.dhs.gov/ITDSServices/IWS'><ReturnCode>ERROR</ReturnCode><ReturnRemark>Message not received successfully by CBP</ReturnRemark><ErrorDetailsList><ErrorDetails><ErrorCode>1000</ErrorCode><ErrorDescription>Unable parse incoming message</ErrorDescription></ErrorDetails></ErrorDetailsList></iws:ResponseMessage>"));
			var result = builder.GetReply();
			Assert.AreEqual("USDIS submission errors. Error code 1000 - Unable parse incoming message. ", result);
		}
	}
}
