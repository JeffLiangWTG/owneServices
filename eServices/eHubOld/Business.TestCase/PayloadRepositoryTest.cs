using System;
using System.Linq;
using CargoWise.eServices.eHub.Common;
using CargoWise.eServices.eHub.Common.EntityModel;
using CargoWise.eServices.eHub.Common.TestCase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Business.TestCase
{
	[TestClass]
	public class PayloadRepositoryTest : TestCaseWithConnection
	{
		#region Save

		[TestMethod]
		public void SaveInbox()
		{
			int eHubInboxEnvelopeCount = Factory.eHubInboxEnvelope.Count();
			int eHubInboxEnvelopeStatusCount = Factory.eHubInboxEnvelopeStatus.Count();
			eHubClient recipient = null;
			eHubInboxEnvelope inbox = null;

			AssertHasException(() => { Repository.SaveInbox("1", InvalidInterchange, out inbox, out recipient); });
			Assert.AreEqual(eHubInboxEnvelopeCount, Factory.eHubInboxEnvelope.Count());
			Assert.AreEqual(eHubInboxEnvelopeStatusCount, Factory.eHubInboxEnvelopeStatus.Count());
			
			string senderID = Guid.NewGuid().ToString();
			string recipientID = Guid.NewGuid().ToString();
			AssertHasException(() => { Repository.SaveInbox("1", string.Format(ValidInterchange, senderID, recipientID), out inbox, out recipient); });
			Assert.AreEqual(eHubInboxEnvelopeCount, Factory.eHubInboxEnvelope.Count());
			Assert.AreEqual(eHubInboxEnvelopeStatusCount, Factory.eHubInboxEnvelopeStatus.Count());
			
			CreateClient(senderID);
			CreateClient(recipientID);
			Factory.SaveChanges(true);

			AssertHasException(() => { Repository.SaveInbox("1", string.Format(ValidInterchangeWithEmptyPayload, senderID, recipientID), out inbox, out recipient); });
			Assert.AreEqual(eHubInboxEnvelopeCount, Factory.eHubInboxEnvelope.Count());
			Assert.AreEqual(eHubInboxEnvelopeStatusCount, Factory.eHubInboxEnvelopeStatus.Count());

			AssertNoException(() => { Repository.SaveInbox("1", string.Format(ValidInterchange, senderID, recipientID), out inbox, out recipient); });
			Assert.AreEqual(eHubInboxEnvelopeCount + 1, Factory.eHubInboxEnvelope.Count());
			Assert.AreEqual(eHubInboxEnvelopeStatusCount + 1, Factory.eHubInboxEnvelopeStatus.Count());
			Assert.IsNotNull(recipient);
			Assert.AreEqual(recipientID, recipient.CC_ID);
			Assert.IsNotNull(inbox);
		}

		[TestMethod]
		public void SaveOutbox()
		{
			int eHubDocumentCount = Factory.eHubDocument.Count();
			int eHubOutboxEnvelopeCount = Factory.eHubOutboxEnvelope.Count();
			
			var recipient = CreateClient(Guid.NewGuid().ToString());

			var inbox = new eHubInboxEnvelope();
			inbox.IE_PK = Guid.NewGuid();
			inbox.IE_ID = "1";
			inbox.IE_Original = "test case";
			inbox.IE_SenderApplicationVersion = Constants.DefaultApplicationVersion;
			inbox.IE_Version = Constants.InterchangeVersion;
			inbox.Sender = recipient;
			Factory.AddToeHubInboxEnvelope(inbox);
			Factory.SaveChanges(true);

			AssertNoException(() => { Repository.SaveOutbox("DocumentContent", recipient, inbox); });
			Assert.AreEqual(eHubDocumentCount + 1, Factory.eHubDocument.Count());
			Assert.AreEqual(eHubOutboxEnvelopeCount + 1, Factory.eHubOutboxEnvelope.Count());
		}

		#endregion

		#region Retrieve

		[TestMethod]
		public void Retrieve()
		{
			int eHubOutboxEnvelopeCount = Factory.eHubOutboxEnvelope.Count();
			int eHubOutboxEnvelopeArchiveCount = Factory.eHubOutboxEnvelopeArchive.Count();

			string recipientID = Guid.NewGuid().ToString();
			var recipient = CreateClient(recipientID);

			var payload = new eHubDocument();
			payload.DC_PK = Guid.NewGuid();
			payload.DC_Content = "<blah>message text</blah>";
			Factory.AddToeHubDocument(payload);

			var outboxEnvelope = new eHubOutboxEnvelope();
			outboxEnvelope.OI_PK= Guid.NewGuid();
			outboxEnvelope.OI_ID = "0000346";
			outboxEnvelope.Recipient = recipient;
			outboxEnvelope.Document = payload;
			Factory.AddToeHubOutboxEnvelope(outboxEnvelope);
			Factory.SaveChanges();

			Assert.AreEqual(eHubOutboxEnvelopeCount + 1, Factory.eHubOutboxEnvelope.Count());
			Assert.AreEqual(eHubOutboxEnvelopeArchiveCount, Factory.eHubOutboxEnvelopeArchive.Count());
			var resposeXML = Repository.Retrieve("1", string.Format(RequestXml, recipientID));
			Assert.AreEqual(string.Format("<ns0:RetrieveResponse xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><RequestID>1</RequestID><HasErrors>false</HasErrors><ErrorMessage></ErrorMessage><Interchange><InterchangeHeader><SenderID></SenderID><RecipientID>{0}</RecipientID><InterchangeVersion>1.0</InterchangeVersion><SenderApplicationVersion>1.0</SenderApplicationVersion><InterchangeID>0000346</InterchangeID></InterchangeHeader><Payload><blah>message text</blah></Payload></Interchange></ns0:RetrieveResponse>", recipientID), resposeXML.OuterXml);

			Assert.AreEqual(eHubOutboxEnvelopeCount, Factory.eHubOutboxEnvelope.Count());
			Assert.AreEqual(eHubOutboxEnvelopeArchiveCount + 1, Factory.eHubOutboxEnvelopeArchive.Count());
		}

		#endregion

		#region Implementation

		eHubClient CreateClient(string clientID)
		{
			var client = new eHubClient() { CC_ID = clientID, CC_PK = Guid.NewGuid(), CC_Odyssey_OH = Guid.NewGuid() };
			Factory.AddToeHubClient(client);
			return client;
		}

		protected override string ConnectionString
		{
			get { return ConfigHelper.DbServerConnectionString(Constants.DefaultDatabaseName); }
		}

		PayloadRepository Repository
		{
			//get { return repository ?? (repository = new PayloadRepository(Factory)); }
			get { return repository ?? (repository = new PayloadRepository()); }
		}
		PayloadRepository repository;

		protected override EntityFactory Factory
		{
			get { return Repository.Factory; }
		}

		#region Request Xml

		const string RequestXml = @"<RetrieveRequest>
    <RequestID>1</RequestID> 
    <RecipientID>{0}</RecipientID> 
    <MaxResponseSize/>
  </RetrieveRequest>";

		#endregion

		#region Invalid Interchange

		const string InvalidInterchange = @"
    <RecipientID>RECV43632623</RecipientID>
    <InterchangeVersion>4456789703</InterchangeVersion>
    <SenderApplicationVersion>4565643</SenderApplicationVersion>
    <InterchangeID>1</InterchangeID>
  </InterchangeHeader>
  <Messages>
    <Message>
      <JobShipment>
        <Housebill>housebill</Housebill>
        <TransportMode>SEA</TransportMode>
        <ContainerMode>FCL</ContainerMode>
      </JobShipment>
    </Message>
    <Message>
      <Declaration>
        <DeclarationReference>B43641325</DeclarationReference>
      </Declaration>
    </Message>
    <Message>
      <Order>
        <OrderNumber>Nub32623</OrderNumber>
        <InvoiceNumber>Inv43662</InvoiceNumber>
      </Order>
    </Message>
  </Messages>
</Interchange>";

		#endregion

		#region Valid Interchange

		const string ValidInterchange = @"<Interchange>
  <InterchangeHeader>
    <SenderID>{0}</SenderID>
    <RecipientID>{1}</RecipientID>
    <InterchangeVersion>4456789703</InterchangeVersion>
    <SenderApplicationVersion>4565643</SenderApplicationVersion>
    <InterchangeID>1</InterchangeID>
  </InterchangeHeader>
  <Payload>
    <JobShipment>
      <Housebill>housebill</Housebill>
      <TransportMode>SEA</TransportMode>
      <ContainerMode>FCL</ContainerMode>
    </JobShipment>
  </Payload>
</Interchange>";

		#endregion

		#region Valid Interchange With Empty Payload

		const string ValidInterchangeWithEmptyPayload = @"<Interchange>
  <InterchangeHeader>
    <SenderID>{0}</SenderID>
    <RecipientID>{1}</RecipientID>
    <InterchangeVersion>4456789703</InterchangeVersion>
    <SenderApplicationVersion>4565643</SenderApplicationVersion>
    <InterchangeID>1</InterchangeID>
  </InterchangeHeader>
  <Payload />
 </Interchange>";

		#endregion

		#endregion
	}
}
