using System;
using System.Linq;
using System.Xml;
using CargoWise.eServices.eHub.Common;
using CargoWise.eServices.eHub.Common.EntityModel;
using CargoWise.eServices.eHub.Common.TestCase;
using CargoWise.eServices.eHub.Common.TestCase.eHubReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Routing.TestCase
{
	[TestClass]
	public class SendMessageOrchestrationTest : TestCaseWithService
	{
		[TestMethod]
		public void SendMessage()
		{
			int eHubInboxEnvelopeCount = Factory.eHubInboxEnvelope.Count();
			int eHubInboxEnvelopeStatusCount = Factory.eHubInboxEnvelopeStatus.Count();
			int eHubPayloadCount = Factory.eHubDocument.Count();
			int eHubOutboxEnvelopeCount = Factory.eHubOutboxEnvelope.Count();
			string clientID = Guid.NewGuid().ToString();

			var message = new Interchange();
			message.InterchangeHeader = new InterchangeInterchangeHeader();
			message.InterchangeHeader.InterchangeID = "1000";
			message.InterchangeHeader.InterchangeVersion = Constants.InterchangeVersion;
			message.InterchangeHeader.SenderApplicationVersion = Constants.DefaultApplicationVersion;
			message.InterchangeHeader.SenderID = clientID;
			message.InterchangeHeader.RecipientID = clientID;
			var xml = new XmlDocument();
			xml.LoadXml("<ns0:Document DocumentType=\"AAA\" xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><DocumentContent>Content_0</DocumentContent></ns0:Document>");
			message.Payload = xml.DocumentElement;

			var response = eHubClient.SendMessage(message);
			Assert.AreEqual("1000", response.InterchangeID);
			Assert.AreEqual(Constants.SendResponseMessageStatus.Failure, response.Status);
			Assert.IsTrue(response.ErrorMessage.Contains("Unable to find Sender with ID"));
			Assert.AreEqual(eHubInboxEnvelopeCount, Factory.eHubInboxEnvelope.Count());
			Assert.AreEqual(eHubInboxEnvelopeStatusCount, Factory.eHubInboxEnvelopeStatus.Count());
			Assert.AreEqual(eHubPayloadCount, Factory.eHubDocument.Count());
			Assert.AreEqual(eHubOutboxEnvelopeCount, Factory.eHubOutboxEnvelope.Count());

			CreateClient(clientID);
			Factory.SaveChanges(true);

			response = eHubClient.SendMessage(message);
			Assert.AreEqual("1000", response.InterchangeID);
			Assert.AreEqual(Constants.SendResponseMessageStatus.OK, response.Status);
			Assert.IsTrue(string.IsNullOrEmpty(response.ErrorMessage.Trim()));
			Assert.AreEqual(eHubInboxEnvelopeCount + 1, Factory.eHubInboxEnvelope.Count());
			Assert.AreEqual(eHubInboxEnvelopeStatusCount + 1, Factory.eHubInboxEnvelopeStatus.Count());
			Assert.AreEqual(eHubPayloadCount + 1, Factory.eHubDocument.Count());
			Assert.AreEqual(eHubOutboxEnvelopeCount + 1, Factory.eHubOutboxEnvelope.Count());
		}

		protected override string ConnectionString
		{
			get { return ConfigHelper.DbServerConnectionString("eHubTransactions"); }
		}

		eHubClient CreateClient(string clientID)
		{
			var client = new eHubClient() { CC_ID = clientID, CC_PK = Guid.NewGuid(), CC_Odyssey_OH = Guid.NewGuid() };
			Factory.AddToeHubClient(client);
			return client;
		}
	}
}
