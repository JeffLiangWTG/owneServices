using System;
using System.Linq;
using CargoWise.eServices.eHub.Common;
using CargoWise.eServices.eHub.Common.EntityModel;
using CargoWise.eServices.eHub.Common.TestCase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Service.TestCase
{
	[TestClass]
	public class eHubServiceTest : TestCaseWithConnection
	{
		[TestMethod]
		public void Send()
		{
			var service = new eHubService();

			string senderID = Guid.NewGuid().ToString();
			string recipientID = Guid.NewGuid().ToString();

			var request = new SendRequest();
			request.RequestID = "1000001";
			request.Envelope.InterchangeDetails.InterchangeID = "1";
			request.Envelope.InterchangeDetails.InterchangeVersion = Constants.InterchangeVersion;
			request.Envelope.InterchangeDetails.SenderApplicaitonVersion = Constants.DefaultApplicationVersion;
			request.Envelope.InterchangeDetails.SenderID = senderID;
			request.Envelope.InterchangeDetails.RecipientID = recipientID;
			request.Envelope.Document.DocumentType = "AAA";
			request.Envelope.Document.DocumentContent = "BLAH";

			var response = service.Send(request);
			Assert.IsTrue(response.HasError);
			Assert.IsTrue(response.ErrorDescription.Contains("Unable to find Sender with ID"));

			Factory.AddToeHubClient(eHubClient.CreateeHubClient(Guid.NewGuid(), senderID, Guid.NewGuid()));
			Factory.AddToeHubClient(eHubClient.CreateeHubClient(Guid.NewGuid(), recipientID, Guid.NewGuid()));
			Factory.SaveChanges();

			response = service.Send(request);
			Assert.IsFalse(response.HasError);
			Assert.IsTrue(string.IsNullOrEmpty(response.ErrorDescription.Trim()));
		}

		[TestMethod]
		public void Retrieve()
		{
			var service = new eHubService();

			string recipientID = Guid.NewGuid().ToString();

			var retrieveRequest = new RetrieveRequest();
			retrieveRequest.RecipientID = recipientID;
			retrieveRequest.RequestID = "1020304";

			var response = service.Retrieve(retrieveRequest);
			Assert.IsTrue(response.HasError);
			Assert.IsTrue(response.ErrorDescription.Contains("Unable to find Recipient with ID"));
			Assert.AreEqual(0, response.Envelopes.Count());

			var recipient = eHubClient.CreateeHubClient(Guid.NewGuid(), recipientID, Guid.NewGuid());
			Factory.AddToeHubClient(recipient);

			var outbox = eHubOutboxEnvelope.CreateeHubOutboxEnvelope(Guid.NewGuid(), Guid.NewGuid().ToString());
			outbox.Recipient = recipient;
			outbox.Document = eHubDocument.CreateeHubDocument(Guid.NewGuid(), "<Document DocumentType=\"AAA\">BLAH</Document>");
			Factory.AddToeHubOutboxEnvelope(outbox);
			Factory.SaveChanges();

			response = service.Retrieve(retrieveRequest);
			Assert.IsFalse(response.HasError);
			Assert.IsTrue(string.IsNullOrEmpty(response.ErrorDescription.Trim()));
			Assert.AreEqual(1, response.Envelopes.Count());
		}

		protected override string ConnectionString
		{
			get { return ConfigHelper.DbServerConnectionString("eHubTransactions"); }
		}

		protected override EntityFactory Factory
		{
			get { return factory ?? (factory = new EntityFactory(ConnectionString)); }
		}
		EntityFactory factory;
	}
}
