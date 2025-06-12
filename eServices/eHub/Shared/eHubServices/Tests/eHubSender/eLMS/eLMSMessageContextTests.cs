using System;
using System.IO;
using CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.eLMS
{
	[TestClass]
	public class eLMSMessageContextTests : BaseTest
	{
		[TestMethod]
		public void eLMSCreateContextTest()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.eLMS.TestFiles.MailStatement.xml")).ReadToEnd();
			var context = new eLMSMessageContext(messageString, "Sender1", "Recepient1");
			context.Load();
			Assert.AreEqual("S00045873", context.GetValue("ShipmentNumber"));
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Invalid Xml. Can't find Shipment Number.")]
		public void eLMSCreateContextEmptyShipmentNumberTest()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.eLMS.TestFiles.MailStatementMissingShipmentNumber.xml")).ReadToEnd();
            var context = new eLMSMessageContext(messageString, "Sender1", "Recepient1");
			context.Load();
		}
	}
}
