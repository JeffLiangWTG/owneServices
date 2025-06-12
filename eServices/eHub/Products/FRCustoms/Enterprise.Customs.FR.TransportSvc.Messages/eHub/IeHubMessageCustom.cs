using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	public class IeHubMessageCustom : IeHubMessage
	{
		readonly string senderId = string.Empty;
		readonly string recipientID = string.Empty;
		readonly string applicationCode = string.Empty;
		readonly string fileName = string.Empty;
		readonly string schemaName = string.Empty;
		Stream messageStream;
		Guid trackingID;

		public IeHubMessageCustom(string senderID, string recipientID, string applicationCode, Guid trackingID, string message)
		{
			this.senderId = senderID;
			this.recipientID = recipientID;
			this.applicationCode = applicationCode;
			this.trackingID = trackingID;
			this.messageStream = new MemoryStream(Encoding.UTF8.GetBytes(message));
			this.fileName = trackingID + ".xml";
		}

		public IeHubMessageCustom(string senderID, string recipientID, string applicationCode, string schemaName)
		{
			this.senderId = senderID;
			this.recipientID = recipientID;
			this.applicationCode = applicationCode;
			this.schemaName = schemaName;
		}

		public IeHubMessageCustom(string senderID, string recipientID, Guid trackingID)
		{
			this.senderId = senderID;
			this.recipientID = recipientID;
			this.trackingID = trackingID;
		}

		public IeHubMessageCustom()
		{
		}

		public Guid TrackingID => !string.IsNullOrEmpty(trackingID.ToString()) ? trackingID : new Guid();

		public string SenderID => senderId;

		public string RecipientID => recipientID;

		public MessageSchemaType SchemaType => MessageSchemaType.Xml;

		public string ApplicationCode => applicationCode;

		public string SchemaName => schemaName;

		public Stream MessageStream => messageStream;

		public string EmailSubject => string.Empty;

		public string Filename => fileName;

		public void SetStream(Stream theStream)
		{
			this.messageStream = theStream;
		}

		public void Dispose()
		{
			if (messageStream != null)
			{
				messageStream.Close();
				messageStream.Dispose();
			}
		}
	}
}
