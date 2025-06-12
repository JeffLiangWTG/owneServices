using System;
using System.IO;
using System.ServiceModel;
using CargoWise.eHub.Share.eHubServices.eHubSender;
using CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.Tests.eHubSender.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.USDIS
{
	[TestClass]
	public class USDISSendTests : BaseTest
	{
		[TestMethod]
        [Ignore]
		//This is integration test. US DIS service should be available.
		public void USDIS_SendSubmission_Integration()
		{
            var protocol = ServicePointManager.SecurityProtocol;
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmissionToSend.xml")).ReadToEnd();

			var sendToService = new SendToService();
            sendToService.Send("HYEDAUIKB", "USDIS", messageString);
            Assert.AreEqual(protocol, ServicePointManager.SecurityProtocol);
		}

		[TestMethod]
		public void USDIS_SendSubmission_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmission.xml")).ReadToEnd();
			string messageToSend = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmissionToSend.xml")).ReadToEnd();
            const string replyText = "<iws:ResponseMessage xmlns:iws='http://iws.cbp.dhs.gov/ITDSServices/IWS'><ReturnCode>SUCCESS</ReturnCode><ReturnRemark>Message received successfully by CBP</ReturnRemark></iws:ResponseMessage>";

            var protocol = ServicePointManager.SecurityProtocol;
			
            var serviceProvider = new USDISServiceProviderTest(Logger, "HYEDAUIKB", "USDIS", messageString, replyText);
			var service = new SendToServiceTest(serviceProvider);
			service.Send("HYEDAUIKB", "USDIS", messageString);

			Assert.AreEqual(messageToSend, serviceProvider.Message);
			Assert.AreEqual("DocumentSubmission", serviceProvider.MessageType);
			Assert.AreEqual(null, serviceProvider.ReplyToBiztalk);
            Assert.AreEqual(protocol, ServicePointManager.SecurityProtocol);
		}

        [TestMethod]
        public void USDIS_SendSubmission_Error()
        {
            string messageString = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmission.xml")).ReadToEnd();
            string messageToSend = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmissionToSend.xml")).ReadToEnd();
            const string replyText = "<iws:ResponseMessage xmlns:iws='http://iws.cbp.dhs.gov/ITDSServices/IWS'><ReturnCode>ERROR</ReturnCode><ReturnRemark>Message not received successfully by CBP</ReturnRemark><ErrorDetailsList><ErrorDetails><ErrorCode>1000</ErrorCode><ErrorDescription>Unable parse incoming message</ErrorDescription></ErrorDetails></ErrorDetailsList></iws:ResponseMessage>";

            var protocol = ServicePointManager.SecurityProtocol;

            var serviceProvider = new USDISServiceProviderTest(Logger, "HYEDAUIKB", "USDIS", messageString, replyText);
            var service = new SendToServiceTest(serviceProvider);

            try
            {
                service.Send("HYEDAUIKB", "USDIS", messageString);
            }
            catch (FaultException ex)
            {
                Assert.AreEqual("USDIS submission errors. Error code 1000 - Unable parse incoming message. ", ex.Message);
                Assert.AreEqual(protocol, ServicePointManager.SecurityProtocol);
            }
        }

		[TestMethod]
		public void USDIS_TestRouting()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmission.xml")).ReadToEnd();
			string messageToSend = new StreamReader(GetEmbeddedResource("eHubSender.USDIS.TestFiles.USDISSubmissionToSend.xml")).ReadToEnd();
			const string replyText = "<iws:ResponseMessage xmlns:iws='http://iws.cbp.dhs.gov/ITDSServices/IWS'><ReturnCode>SUCCESS</ReturnCode><ReturnRemark>Message received successfully by CBP</ReturnRemark></iws:ResponseMessage>";

			{
				var serviceProvider = new USDISServiceProviderTest(Logger, "HYEDAUIKB", "USDIS", messageString, replyText);
				var service = new SendToServiceTest(serviceProvider);
				service.Send("HYEDAUIKB", "USDIS", messageString);
				Assert.AreEqual("USDISTest", serviceProvider.Context_Exposed.eHubRecipientId);
				Assert.AreEqual("https://testserver:2933/", serviceProvider.endpointAddress_Exposed);
			}

			{
				var serviceProvider = new USDISServiceProviderTest(Logger, "HYEDAPROD", "USDIS", messageString, replyText);
				var service = new SendToServiceTest(serviceProvider);
				service.Send("HYEDAPROD", "USDIS", messageString);
				Assert.AreEqual("USDIS", serviceProvider.Context_Exposed.eHubRecipientId);
				Assert.AreEqual("https://prodserver:2933/", serviceProvider.endpointAddress_Exposed);
			}
		}
	}
}
