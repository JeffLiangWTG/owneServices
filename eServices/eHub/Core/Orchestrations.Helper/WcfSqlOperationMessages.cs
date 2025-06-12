using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	public class WcfSqlOperationMessages
	{
		private static readonly XNamespace NsSqlStoredProc = "http://schemas.microsoft.com/Sql/2008/05/TypedProcedures/dbo";
		public static Func<Guid> InternalNewGuid = Guid.NewGuid;
		public static Func<DateTime> GetCurrentUtcTime = () => DateTime.UtcNow;

		public static XElement GetWcfSqlInsertInbox(string senderId, string recipientId, string messageType,
			string applicationCode, string content, Guid? messageTrackingId = null, Guid? inboxPk = null,
			Guid? envelopeTrackingId = null, bool isFlatFile = false, string emailSubject = null, string fileName = null,
			DateTime? messageTime = null, int status = 0)
		{
			var wcfXml = new XElement(NsSqlStoredProc + "InsertInboxWCF",
					new XElement(NsSqlStoredProc + "InboxPK", inboxPk ?? InternalNewGuid()),
					new XElement(NsSqlStoredProc + "MessageTrackingID", messageTrackingId ?? InternalNewGuid()),
					new XElement(NsSqlStoredProc + "EnvelopeTrackingID", envelopeTrackingId ?? InternalNewGuid()),
					new XElement(NsSqlStoredProc + "SenderID", senderId),
					new XElement(NsSqlStoredProc + "RecipientID", recipientId),
					new XElement(NsSqlStoredProc + "MessageType", messageType),
					new XElement(NsSqlStoredProc + "IsFlatFile", Convert.ToInt32(isFlatFile)),
					new XElement(NsSqlStoredProc + "EmailSubject", emailSubject),
					new XElement(NsSqlStoredProc + "FileName", fileName),
					new XElement(NsSqlStoredProc + "ApplicationCode", applicationCode),
					new XElement(NsSqlStoredProc + "Status", status),
					new XElement(NsSqlStoredProc + "CurrentDateTimeUTC", messageTime ?? GetCurrentUtcTime()),
					new XElement(NsSqlStoredProc + "Content", CompressAndEncode(content))
				);
			return wcfXml;
		}

		public static XElement GetWcfSqlInsertOutbox(string senderId, string recipientId, string messageType,
			string content, Guid inboxPK, Guid? messageTrackingId = null, Guid? outboxPK = null,
			Guid? envelopeTrackingId = null, string emailSubject = null, string fileName = null,
			string transetID = null, DateTime? messageTime = null, int status = 0)
		{
			var wcfXml = new XElement(NsSqlStoredProc + "InsertOutboxMessage",
				new XElement(NsSqlStoredProc + "PK", outboxPK ?? InternalNewGuid()),
				new XElement(NsSqlStoredProc + "SenderID", senderId),
				new XElement(NsSqlStoredProc + "RecipientID", recipientId),
				new XElement(NsSqlStoredProc + "EnvelopeTrackingID", envelopeTrackingId ?? InternalNewGuid()),
				new XElement(NsSqlStoredProc + "MessageTrackingID", messageTrackingId ?? InternalNewGuid()),
				new XElement(NsSqlStoredProc + "InternalTrackingID", inboxPK),
				new XElement(NsSqlStoredProc + "OverrideEmailSubject", emailSubject),
				new XElement(NsSqlStoredProc + "OverrideFilename", fileName),
				new XElement(NsSqlStoredProc + "TransformSetID", transetID),
				new XElement(NsSqlStoredProc + "Status", status),
				new XElement(NsSqlStoredProc + "InsertUTC", messageTime ?? GetCurrentUtcTime()),
				new XElement(NsSqlStoredProc + "TargetMessageType", messageType),
				new XElement(NsSqlStoredProc + "Content", CompressAndEncode(content)),
				new XElement(NsSqlStoredProc + "XMLContent", content)
			);
			return wcfXml;
		}

		public static XmlDocument CombineWcfSqlOps(params XElement[] operations)
		{
			var compositeWcfMessage = new XDocument(
				new XElement("CompositeOperation", operations)
			);
			var request = new XmlDocument();
			using (var reader = compositeWcfMessage.CreateReader())
			{
				request.Load(reader);
			}

			return request;
		}

		private static string CompressAndEncode(string content)
		{
			var bytes = Encoding.UTF8.GetBytes(content);

			using (var inputStream = new MemoryStream(bytes))
			using (var outputStream = new MemoryStream())
			{
				using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
				{
					inputStream.CopyTo(gZipStream);
				}
				return Convert.ToBase64String(outputStream.ToArray());
			}
		}
	}
}