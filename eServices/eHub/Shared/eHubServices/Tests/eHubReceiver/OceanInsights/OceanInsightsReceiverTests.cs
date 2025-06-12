using System;
using System.IO;
using CargoWise.eHub.Share.eHubServices.eHubReceiver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubReceiver
{
    using Common.Logging;

    [TestClass]
	public class OceanInsightsReceiverTests : BaseTest
	{
		[TestMethod]
		public void OceanInsightsReceiverTests_ContainerEvent_Json()
		{
            var logger = new StringLogger();
            var messageLogger = new StringLogger();
			var stream = GetEmbeddedResource("eHubReceiver.OceanInsights.TestFiles.ContainerEvent.txt");
            var receiver = new OceanInsightsReceiverTest(logger, messageLogger);
			receiver.PostContainerEvent(stream);

			var expectedText = GetEmbeddedResourceAsString("eHubReceiver.OceanInsights.TestFiles.ContainerEvent_output.xml");
			Assert.AreEqual(expectedText, receiver.Message);
		}

		[TestMethod]
		public void OceanInsightsReceiverTests_ContainerEvent_Error()
		{
            var logger = new StringLogger();
            var messageLogger = new StringLogger();
			var stream = GetEmbeddedResource("eHubReceiver.OceanInsights.TestFiles.ContainerEvent.txt");
            var receiver = new OceanInsightsReceiverTest(logger, messageLogger) { SendToBiztalkThrownException = true };
			receiver.PostContainerEvent(stream);
            Assert.IsTrue(logger.ToString().Contains("System.Exception: Error during sending to eHub"), "Logger error");
		}
	}

	public class OceanInsightsReceiverTest : PushService
	{
	    public OceanInsightsReceiverTest(ILog logger, ILog messageLogger)
            : base(logger, messageLogger)
	    {
	        
	    }

		public bool SendToBiztalkThrownException { get; set; }
		public string Message { get; set; }
		public bool InvalidUri { get; set; }


		protected override string DateTimeNowString
		{
			get
			{
				return "2011-12-02T09:12:22-07:00";
			}
		}

		protected override void SendToBiztalk(Stream message)
		{
			message.Position = 0;
			Message = new StreamReader(message).ReadToEnd();
			if (SendToBiztalkThrownException) throw new Exception("Error during sending to eHub");
		}

		protected override Uri EndpointAddress
		{
			get
			{
				return new Uri("http://localhost/eHubReceiver/PushService.svc");
			}
		}
	}
}
