using System;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eServices.eHub.Common;
using CargoWise.eServices.eHub.Common.EntityModel;

namespace CargoWise.eServices.eHub.Business
{
	[Serializable]
	public class PayloadRepository : IDisposable
	{
		#region Save

		public void SaveInbox(string interchangeID, string interchangeXml, out eHubInboxEnvelope inbox, out eHubClient recipient)
		{
			var xInterchange = XDocument.Parse(interchangeXml);

			var senderID = xInterchange.Elements().Descendants().Where(el => el.Name == "SenderID").FirstOrDefault().Value;
			var senders = Factory.eHubClient.Where(client => client.CC_ID == senderID);
			if (senders.Count() == 0) throw new Exception(string.Format("Unable to find Sender with ID = '{0}'", senderID));

			var recipientID = xInterchange.Elements().Descendants().Where(el => el.Name == "RecipientID").FirstOrDefault().Value;
			var recipients = Factory.eHubClient.Where(client => client.CC_ID == recipientID);
			if (recipients.Count() == 0) throw new Exception(string.Format("Unable to find Recipient with ID = '{0}'", recipientID));

			var paylod = xInterchange.Elements().Descendants("Payload");
			if (paylod.Descendants().Count() == 0) throw new Exception("Payload is empty.");

			var inboxEnvelope = new eHubInboxEnvelope();
			inboxEnvelope.IE_PK = Guid.NewGuid();
			inboxEnvelope.IE_ID = interchangeID;
			inboxEnvelope.Sender = senders.First();
			inboxEnvelope.IE_SenderApplicationVersion = xInterchange.Elements().Descendants().Where(el => el.Name == "SenderApplicationVersion").First().Value;
			inboxEnvelope.IE_Version = xInterchange.Elements().Descendants().Where(el => el.Name == "InterchangeVersion").First().Value;
			inboxEnvelope.IE_Original = interchangeXml;
			Factory.AddToeHubInboxEnvelope(inboxEnvelope);

			var status = new eHubInboxEnvelopeStatus();
			status.IS_PK = Guid.NewGuid();
			status.IS_Status = Constants.InboxEnvelopeStatus.Receiverd;
			status.IS_UTC = DateTime.UtcNow;
			status.Inbox = inboxEnvelope;
			Factory.AddToeHubInboxEnvelopeStatus(status);

			Factory.SaveChanges(true);

			inbox = inboxEnvelope;
			recipient = recipients.First();
		}

		public void SaveOutbox(string content, eHubClient recipient, eHubInboxEnvelope inboxEnvelope)
		{
			var document = new eHubDocument();
			document.DC_PK = Guid.NewGuid();
			document.DC_Content = content;
			Factory.AddToeHubDocument(document);

			var outboxEnvelope = new eHubOutboxEnvelope();
			outboxEnvelope.OI_PK = Guid.NewGuid(); //!!!!! Not sure about outbox ID and why would we need it. At this stage just a Guid.
			outboxEnvelope.OI_ID = Guid.NewGuid().ToString();
			outboxEnvelope.Recipient = recipient;
			outboxEnvelope.Document = document;
			outboxEnvelope.Inbox = inboxEnvelope;
			Factory.AddToeHubOutboxEnvelope(outboxEnvelope);

			Factory.SaveChanges(true);
		}

		#endregion

		#region Retrieve

		public XmlDocument Retrieve(string requestID, string requestXML)
		{
			XElement xResponse;
			try
			{
				var xRequest = XDocument.Parse(requestXML);
				var recipientID = xRequest.Elements().Descendants("RecipientID").FirstOrDefault().Value;
				var recipients = Factory.eHubClient.Where(client => client.CC_ID == recipientID);
				if (recipients.Count() == 0) throw new Exception(string.Format("Unable to find Recipient with ID = '{0}'", recipientID));

				var envelopes = Factory.eHubOutboxEnvelope.Where(envelope => envelope.Recipient.Equals(recipients.FirstOrDefault()));
				xResponse = new XElement("{http://CargoWise.eServices.eHub.Schema}RetrieveResponse", new XAttribute(XNamespace.Xmlns + "ns0", "http://CargoWise.eServices.eHub.Schema"),
					new XElement("RequestID", requestID),
					new XElement("HasErrors", false),
					new XElement("ErrorMessage", string.Empty),
					from envelope in
						(from envelope in envelopes
						 select new
						 {
							 SenderID = envelope.Inbox != null ? envelope.Inbox.Sender.CC_ID : string.Empty,
							 Version = envelope.Inbox != null ? envelope.Inbox.IE_Version : Constants.InterchangeVersion,
							 AppVersion = envelope.Inbox != null ? envelope.Inbox.IE_SenderApplicationVersion : Constants.DefaultApplicationVersion,
							 ID = envelope.OI_ID,
							 Payload = envelope.Document.DC_Content
						 }).ToList()
					select new XElement("Interchange",
						new XElement("InterchangeHeader",
							new XElement("SenderID", envelope.SenderID),
							new XElement("RecipientID", recipientID),
							new XElement("InterchangeVersion", envelope.Version),
							new XElement("SenderApplicationVersion", envelope.AppVersion),
							new XElement("InterchangeID", envelope.ID)),
						new XElement("Payload", XElement.Parse(envelope.Payload))));

				foreach (var envelope in (from envelope in envelopes
										  select new
										  {
											  Recipient = envelope.Recipient,
											  Payload = envelope.Document.DC_PK,
											  Inbox = envelope.Inbox != null ? envelope.Inbox.IE_PK : Guid.Empty,
										  }).ToList())
				{
					var envelpeArchive = new eHubOutboxEnvelopeArchive();
					envelpeArchive.OA_PK = Guid.NewGuid();
					envelpeArchive.Recipient = envelope.Recipient;
					envelpeArchive.OA_DC_Document = envelope.Payload;
					envelpeArchive.OA_IE_Inbox = envelope.Inbox;
					envelpeArchive.OA_ArchiveUTC = DateTime.UtcNow;

					Factory.AddToeHubOutboxEnvelopeArchive(envelpeArchive);
				}
				foreach (var envelope in envelopes) Factory.DeleteObject(envelope);
				Factory.SaveChanges();
			}
			catch (Exception ex)
			{
				string innerMessage = ex.InnerException != null ? ex.InnerException.Message : "<empty>";

				xResponse = new XElement("{http://CargoWise.eServices.eHub.Schema}RetrieveResponse", new XAttribute(XNamespace.Xmlns + "ns0", "http://CargoWise.eServices.eHub.Schema"),
				new XElement("RequestID", requestID),
				new XElement("HasErrors", true),
				new XElement("ErrorMessage", string.Format("{1}{1}ErrorMessage: {0}{1}{1}Inner Exception: {3}{1}{1}Stack Trace: {1}{2}{1}{1}", ex.Message, System.Environment.NewLine, ex.StackTrace, innerMessage)));
			}

			var result = new XmlDocument();
			result.LoadXml(xResponse.ToString());
			return result;
		}

		#endregion

		#region Factory

		public EntityFactory Factory
		{
			get { return factory ?? (factory = new EntityFactory(ConfigHelper.DbServerConnectionString(Constants.DefaultDatabaseName))); }
		}
		EntityFactory factory;
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (Factory.Connection.State == ConnectionState.Open) Factory.Connection.Close();
			Factory.Dispose();
		}

		#endregion
	}
}
