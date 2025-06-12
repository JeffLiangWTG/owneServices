using System;
using System.Linq;
using CargoWise.eServices.eHub.Common;
using CargoWise.eServices.eHub.Common.EntityModel;
using CargoWise.eServices.eHub.Common.TestCase;
using CargoWise.eServices.eHub.Common.TestCase.eHubReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Routing.TestCase
{
	[TestClass]
	public class RetrieveMessagesOrchestrationTest : TestCaseWithService
	{
		[TestMethod]
		public void RetrieveMessages()
		{
			var request = new RetrieveRequest();
			request.MaxResponseSize = "100000000";
			request.RequestID = new Guid().ToString();
			request.RecipientID = "BLAHBLAH";

			var response = eHubClient.RetrieveMessages(request);
			Assert.AreEqual(request.RequestID, response.RequestID);
			Assert.IsTrue(response.HasErrors);
			Assert.IsFalse(string.IsNullOrEmpty(response.ErrorMessage));
			Assert.IsNull(response.Interchange);

			string clientID = Guid.NewGuid().ToString();

			var payload = new eHubDocument();
			payload.DC_PK = Guid.NewGuid();
			payload.DC_Content = "<blah>message text</blah>";
			Factory.AddToeHubDocument(payload);

			var outboxEnvelope = new eHubOutboxEnvelope();
			outboxEnvelope.OI_PK = Guid.NewGuid();
			outboxEnvelope.OI_ID = "0000346";
			outboxEnvelope.Recipient = CreateClient(clientID);
			outboxEnvelope.Document = payload;
			Factory.AddToeHubOutboxEnvelope(outboxEnvelope);
			Factory.SaveChanges();

			Factory.SaveChanges(true);

			request.RecipientID = clientID;
			response = eHubClient.RetrieveMessages(request);
			Assert.AreEqual(request.RequestID, response.RequestID);
			Assert.IsFalse(response.HasErrors);
			Assert.AreEqual(1, response.Interchange.Count());
			Assert.AreEqual("0000346", response.Interchange.First().InterchangeHeader.InterchangeID);
			Assert.AreEqual(Constants.InterchangeVersion, response.Interchange.First().InterchangeHeader.InterchangeVersion);
			Assert.AreEqual(Constants.DefaultApplicationVersion, response.Interchange.First().InterchangeHeader.SenderApplicationVersion);
			Assert.AreEqual(clientID, response.Interchange.First().InterchangeHeader.RecipientID);
			Assert.AreEqual("<blah>message text</blah>", response.Interchange.First().Payload.OuterXml);
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
