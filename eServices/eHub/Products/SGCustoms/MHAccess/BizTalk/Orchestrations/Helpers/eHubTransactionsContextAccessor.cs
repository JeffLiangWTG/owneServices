using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class eHubTransactionsContextAccessor
	{
		public static void UpdateSubscriptionValueWithBatchNumberForAIRAED(string providerID, string subscriberID, string batchNumber, XmlDocument xmlDocument)
		{
			var xPathValues = new List<Tuple<int, string, string>>();
			var collector = new XPathValueCollector(jobNumberXPath, messageUniqueReferencePart1XPath, messageUniqueReferencePart2XPath, messageUniqueReferencePart3XPath, batchDateXPath);

			using (var virtualStream = new Microsoft.BizTalk.Streaming.VirtualStream())
			{
				xmlDocument.Save(virtualStream);
				xPathValues = collector.Collect(virtualStream);
			}

			var values = xPathValues.ToArray();

			if (values.Length != 5) throw new ArgumentException(string.Format("Expected to collect 5 mandatory values but only collected {0}", values.Length));

			var jobNumber = values[0].Item3;
			var messageUniqueReferencePart1 = values[1].Item3;
			var messageUniqueReferencePart2 = values[2].Item3;
			var messageUniqueReferencePart3 = values[3].Item3;
			var batchDate = values[4].Item3;

			var messageUniqueReference = messageUniqueReferencePart1 + messageUniqueReferencePart2 + messageUniqueReferencePart3;
			var batchDateNumber = batchDate + batchNumber;

			NewDataModelAccessor().InsertSubscriptionValue("SGCMSG", providerID, subscriberID, batchDateNumber, jobNumber, "BatchNo-JobNo");
			NewDataModelAccessor().InsertSubscriptionValue("SGCMSG", providerID, subscriberID, batchDateNumber, messageUniqueReference, "BatchNo-MsgUniqueReference");
		}

        public static bool TryGetSubscriptionReferenceValueForAEP(XmlDocument xmlDocument, out string IDTKey)
        {
            IDTKey = string.Empty;
	        List<Tuple<int, string, string>> xPathValues;
            var collector = new XPathValueCollector(AEPMessageUniqueReferencePart1XPath, AEPMessageUniqueReferencePart2XPath, AEPMessageUniqueReferencePart3XPath);

	        using (var virtualStream = new Microsoft.BizTalk.Streaming.VirtualStream())
	        {
	            xmlDocument.Save(virtualStream);
	            xPathValues = collector.Collect(virtualStream);
	        }

	        var values = xPathValues.ToArray();
	        var messageUniqueReferencePart1 = values[0].Item3;
            var messageUniqueReferencePart2 = values[1].Item3;
            var messageUniqueReferencePart3 = values[2].Item3;
            if (values.Length != 3 ||string.IsNullOrEmpty(messageUniqueReferencePart1) || 
                string.IsNullOrEmpty(messageUniqueReferencePart2) || string.IsNullOrEmpty(messageUniqueReferencePart3)) return false;

            IDTKey = messageUniqueReferencePart1 + messageUniqueReferencePart2 + messageUniqueReferencePart3 + "E";
            return true;
        }

		static string jobNumberXPath = @"/*[local-name()='EFACT_31_AIRAED']/*[local-name()='UNH']/*[local-name()='UNH1']";
		static string messageUniqueReferencePart1XPath = @"/*[local-name()='EFACT_31_AIRAED']/*[local-name()='IDT']/*[local-name()='IDT1']/*[local-name()='IDT1.1']";
		static string messageUniqueReferencePart2XPath = @"/*[local-name()='EFACT_31_AIRAED']/*[local-name()='IDT']/*[local-name()='IDT1']/*[local-name()='IDT1.2']";
		static string messageUniqueReferencePart3XPath = @"/*[local-name()='EFACT_31_AIRAED']/*[local-name()='IDT']/*[local-name()='IDT1']/*[local-name()='IDT1.3']";
		static string batchDateXPath = @"/*[local-name()='EFACT_31_AIRAED']/*[local-name()='DTM']/*[local-name()='DTM1']";

        static string AEPMessageUniqueReferencePart1XPath = @"/*[local-name()='EFACT_31_AIRAEP']/*[local-name()='IDT']/*[local-name()='IDT1']/*[local-name()='IDT1.1']";
        static string AEPMessageUniqueReferencePart2XPath = @"/*[local-name()='EFACT_31_AIRAEP']/*[local-name()='IDT']/*[local-name()='IDT1']/*[local-name()='IDT1.2']";
        static string AEPMessageUniqueReferencePart3XPath = @"/*[local-name()='EFACT_31_AIRAEP']/*[local-name()='IDT']/*[local-name()='IDT1']/*[local-name()='IDT1.3']";

		public static Func<DataModelAccessor> NewDataModelAccessor = () => new DataModelAccessor();

		#region SucceedMessages

		public static void SucceedMessages(string senderID, string recipientID, Guid mHAccessMessageTrackingID, string inboxMessageContent, string outboxMessageContent, ArrayList outboxMessageTrackingIDArrayList, ILog logger = null)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => SucceedMessages(senderID, recipientID, mHAccessMessageTrackingID, inboxMessageContent, outboxMessageContent, outboxMessageTrackingIDArrayList), logger);
		}

		private static void SucceedMessages(string senderID, string recipientID, Guid mHAccessMessageTrackingID, string inboxMessageContent, string outboxMessageContent, ArrayList outboxMessageTrackingIDArrayList)
		{
			using (var context = NewEHubTransactionContext())
			using (var transaction = context.BeginTransaction())
			{
				//Insert inbox and outbox
				inboxMessageContent = System.String.Format("<eHubBatchMessage>{0}</eHubBatchMessage>", inboxMessageContent);

				var eHubGatewayMessageForInbox = new CargoWise.eHub.Common.eHubGatewayMessage();
				eHubGatewayMessageForInbox.ApplicationCode = "BIZ";
				eHubGatewayMessageForInbox.ClientID = recipientID;
				eHubGatewayMessageForInbox.MessageTrackingID = mHAccessMessageTrackingID;
				eHubGatewayMessageForInbox.SchemaType = CargoWise.eHub.Common.MessageSchemaType.Xml;
				eHubGatewayMessageForInbox.SchemaName = "eHubBatchMessage";
				eHubGatewayMessageForInbox.EmailSubject = "";
				eHubGatewayMessageForInbox.FileName = "";
				eHubGatewayMessageForInbox.MessageStream = CargoWise.eHub.Common.Extensions.StreamExtensions.CompressAndEncode(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(inboxMessageContent)));

				var eHubGatewayMessageForOutbox = new CargoWise.eHub.Common.eHubGatewayMessage();
				eHubGatewayMessageForOutbox.ApplicationCode = "BIZ";
				eHubGatewayMessageForOutbox.ClientID = recipientID;
				eHubGatewayMessageForOutbox.MessageTrackingID = mHAccessMessageTrackingID;
				eHubGatewayMessageForOutbox.SchemaType = CargoWise.eHub.Common.MessageSchemaType.FlatFile;
				eHubGatewayMessageForOutbox.SchemaName = "eHubBatchMessage";
				eHubGatewayMessageForOutbox.EmailSubject = "";
				eHubGatewayMessageForOutbox.FileName = "";
				eHubGatewayMessageForOutbox.MessageStream = CargoWise.eHub.Common.Extensions.StreamExtensions.CompressAndEncode(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(outboxMessageContent)));

				Guid inboxPK = Guid.NewGuid();
				var inboxText = ReadMessage(eHubGatewayMessageForInbox, 300);
				SetSchemaName(eHubGatewayMessageForInbox, inboxText);
				var inboxDeclarationSequence = DeclarationSequence(inboxText);

				var outboxText = ReadMessage(eHubGatewayMessageForOutbox, 300);
				SetSchemaName(eHubGatewayMessageForOutbox, outboxText);
				var outboxDeclarationSequence = DeclarationSequence(outboxText);

				var envelopeTrackingID = Guid.NewGuid();

				InsertToInbox(senderID, envelopeTrackingID, inboxPK, MessageStatus.Processed, eHubGatewayMessageForInbox, false, context);
				InsertToInboxXMLContent(eHubGatewayMessageForInbox, inboxPK, context, inboxDeclarationSequence);
				InsertToOutbox(senderID, envelopeTrackingID, eHubGatewayMessageForOutbox, inboxPK, context, outboxDeclarationSequence);

				//UpdateOutboxMessageBatchEnvelopeTrackingID
				var outboxMessageTrackingIDs = outboxMessageTrackingIDArrayList.ToArray(typeof(string)).Cast<string>().ToArray();
				foreach (var messageTrackingID in outboxMessageTrackingIDs)
				{
					var outboxMessagePK = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == messageTrackingID).Select(_ => _.OI_PK).First();
					context.ExecuteSqlCommand("update eHubOutboxMessage set OI_BatchEnvelopeTrackingID = @batchEnvelopeTrackingID where OI_PK = @pk", new SqlParameter("batchEnvelopeTrackingID", mHAccessMessageTrackingID), new SqlParameter("pk", outboxMessagePK));

					var messageTypePK = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == messageTrackingID).Select(_ => _.OI_DT_Target).First();
					var messageType = context.eHubMessageTypes.Single(_ => _.DT_PK == messageTypePK).DT_Code;
					var messageContent = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == messageTrackingID).Select(_ => _.OI_Content).First();
					var recipientPK = context.eHubClients.Single(_ => _.CC_ID == recipientID).CC_PK;
					var senderPK = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == messageTrackingID).Select(_ => _.OI_CC_Sender).First();

					var ackMessage = new eHubInboxMessage()
					{
						EI_PK = Guid.NewGuid(),
						EI_MessageTrackingID = Guid.NewGuid().ToString(),
						EI_EnvelopeTrackingID = Guid.Empty.ToString(),
						EI_CC_Sender = recipientPK,
						EI_CC_Recipient = senderPK,
						EI_MessageType = messageType,
						EI_IsFlatFile = false,
						EI_EmailSubjectOverride = string.Empty,
						EI_FileNameOverride = "ACKSuccess",
						EI_ApplicationCode = "SGC",
						EI_Status = 0,
						EI_Content = messageContent,
						EI_InsertUTC = DateTime.UtcNow,
						EI_LastUpdateUTC = DateTime.UtcNow,
						EI_SN = null
					};
					context.eHubInboxMessages.Add(ackMessage);
				}

				//UpdateMessageDistributionStatus
				context.ExecuteSqlCommand("exec [dbo].[UpdateMessageDistributionStatus] @MessageTrackingID, @SenderID, @RecipientID, @CurrentDateTimeUTC",
					new SqlParameter("MessageTrackingID", mHAccessMessageTrackingID),
					new SqlParameter("SenderID", senderID),
					new SqlParameter("RecipientID", recipientID),
					new SqlParameter("CurrentDateTimeUTC", DateTime.UtcNow));

				context.SaveChanges();

				transaction.Commit();
			}
		}

		static byte[] DeclarationSequence(string text)
		{
			return Encoding.UTF8.GetBytes(Regex.Match(text, @"^(<\?*?xml.*?>[\w\W]*?)<.*?").Groups[1].Value);
		}

		static string ReadMessage(eHubGatewayMessage message, int length = -1)
		{
			if (message != null)
			{
				message.MessageStream.SeekBegin();
				using (var reader = new DecodingDecompressingReader(message.MessageStream))
				{
					if (length == -1) return reader.ReadToEnd();
					else return reader.ReadString(length);
				}
			}
			return string.Empty;
		}

		static void SetSchemaName(eHubGatewayMessage message, string text)
		{
			if (IsXML(text))
			{
				message.SchemaName = GetSchemaName(text);
				message.SchemaType = MessageSchemaType.Xml;
			}
		}

		static bool IsXML(string text)
		{
			return Regex.IsMatch(text, @"^<.*?>");
		}

		static string GetSchemaName(string text)
		{
			var root = Regex.Replace(text, @"^(<\?*?xml.*?>)*([\w\W]*?<.*?)(>[\w\W]*)", "$2/>").Trim();
			XDocument doc = XDocument.Parse(root);
			return string.Format("{0}{1}{2}", doc.Root.Name.NamespaceName, string.IsNullOrEmpty(doc.Root.Name.NamespaceName) ? "" : "#", doc.Root.Name.LocalName);
		}

		static void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery, eHubTransactionsContext context)
		{
			message.MessageStream.SeekBegin();
			bool isSmallMessage = message.MessageStream.Length <= StreamExtensions.BufferSize;
			var inboxContent = message.MessageStream;
			var inboxContentString = isSmallMessage ? inboxContent.ReadToEnd() : string.Empty;

		    context.ExecuteSqlCommand("exec [dbo].[InsertInbox] @InboxPK, @MessageTrackingID, @EnvelopeTrackingID, @SenderID, @RecipientID, @MessageType, @IsFlatFile, @EmailSubject, @FileName, @ApplicationCode, @Status, @CurrentDateTimeUTC, @AssignSN, @Content",
		        new SqlParameter("InboxPK", inboxPK),
		        new SqlParameter("MessageTrackingID", message.MessageTrackingID.ToString()),
		        new SqlParameter("EnvelopeTrackingID", envelopeTrackingID.ToString()),
		        new SqlParameter("SenderID", senderID),
		        new SqlParameter("RecipientID", message.ClientID),
		        new SqlParameter("MessageType", message.SchemaName),
		        new SqlParameter("IsFlatFile", message.SchemaType.Equals(MessageSchemaType.FlatFile)),
		        new SqlParameter("EmailSubject", message.EmailSubject),
		        new SqlParameter("FileName", message.FileName),
		        new SqlParameter("ApplicationCode", message.ApplicationCode),
		        new SqlParameter("Status", (byte)status),
		        new SqlParameter("CurrentDateTimeUTC", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")),
		        new SqlParameter("AssignSN", orderedDelivery),
		        new SqlParameter("Content", inboxContentString));

			if (!isSmallMessage)
			{
				var reader = new StreamReader(inboxContent);
				char[] buffer = new char[StreamExtensions.BufferSize];
				int charsread = reader.Read(buffer, 0, buffer.Length);
				int offset = 0;
				while (charsread > 0)
				{
					if (charsread < buffer.Length) Array.Resize(ref buffer, charsread);
					context.ExecuteSqlCommand("exec [dbo].[WriteInboxContent] @InboxPK, @Offset, @ContentChunk",
						new SqlParameter("InboxPK", inboxPK),
						new SqlParameter("Offset", offset),
						new SqlParameter("ContentChunk", buffer));
					offset += charsread;
					charsread = reader.Read(buffer, 0, buffer.Length);
				}
			}
		}

		static void InsertToInboxXMLContent(eHubGatewayMessage message, Guid inboxPK, eHubTransactionsContext context, byte[] cropSequence)
		{
			if (message.SchemaType == MessageSchemaType.Xml)
			{
				message.MessageStream.SeekBegin();
				var uncompressedStream = message.MessageStream.DecodeAndDecompress();
				var targetStream = new CropStream(uncompressedStream, cropSequence, CropMode.Exclusive);

				context.ExecuteSqlCommand("exec [dbo].[InsertInboxXMLContent] @InboxPK, @XMLContent, @MessageType, @UncompressedLength",
					new SqlParameter("InboxPK", inboxPK),
					new SqlParameter("XMLContent", new SqlXml(targetStream)),
					new SqlParameter("MessageType", message.SchemaName),
					new SqlParameter("UncompressedLength", targetStream.Length));
			}
		}

		static void InsertToOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, Guid inboxPK, eHubTransactionsContext context, byte[] cropSequence)
		{
			message.MessageStream.SeekBegin();
			bool isSmallMessage = message.MessageStream.Length <= StreamExtensions.BufferSize;
			var uncompressedStream = message.MessageStream.DecodeAndDecompress();
            var targetStream = new CropStream(uncompressedStream, cropSequence, CropMode.Exclusive);

			var outboxPK = Guid.NewGuid();
			var isXml = message.SchemaType == MessageSchemaType.Xml;
			var outboxContent = targetStream.CompressAndEncode();
			var outboxContentString = isSmallMessage ? outboxContent.ReadToEnd() : string.Empty;

			if (isXml)
			{
				context.ExecuteSqlCommand(@"exec [dbo].[InsertOutboxMessage] 
@PK = @PK, 
@SenderID = @SenderID, 
@RecipientID = @RecipientID, 
@EnvelopeTrackingID = @EnvelopeTrackingID, 
@MessageTrackingID = @MessageTrackingID, 
@InternalTrackingID = @InternalTrackingID, 
@OverrideEmailSubject = @OverrideEmailSubject, 
@OverrideFilename = @OverrideFilename, 
@Status = @Status, 
@InsertUTC = @InsertUTC, 
@TargetMessageType = @TargetMessageType, 
@Content = @Content, 
@XMLContent = @XMLContent, 
@UncompressedLength = @UncompressedLength",
					new SqlParameter("PK", outboxPK),
					new SqlParameter("SenderID", senderID),
					new SqlParameter("RecipientID", message.ClientID),
					new SqlParameter("EnvelopeTrackingID", envelopeTrackingID),
					new SqlParameter("MessageTrackingID", message.MessageTrackingID),
					new SqlParameter("InternalTrackingID", inboxPK),
					new SqlParameter("OverrideEmailSubject", message.EmailSubject),
					new SqlParameter("OverrideFilename", message.FileName),
					new SqlParameter("Status", MessageStatus.Processing),
					new SqlParameter("InsertUTC", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")),
					new SqlParameter("TargetMessageType", message.SchemaName),
					new SqlParameter("Content", outboxContentString),
					new SqlParameter("XMLContent", new SqlXml(targetStream)),
					new SqlParameter("UncompressedLength", targetStream.Length));
			}
			else
			{
				context.ExecuteSqlCommand(@"exec [dbo].[InsertOutboxMessage] 
@PK = @PK, 
@SenderID = @SenderID, 
@RecipientID = @RecipientID, 
@EnvelopeTrackingID = @EnvelopeTrackingID, 
@MessageTrackingID = @MessageTrackingID, 
@InternalTrackingID = @InternalTrackingID, 
@OverrideEmailSubject = @OverrideEmailSubject, 
@OverrideFilename = @OverrideFilename, 
@Status = @Status, 
@InsertUTC = @InsertUTC, 
@TargetMessageType = @TargetMessageType, 
@Content = @Content, 
@XMLContent = @XMLContent, 
@UncompressedLength = @UncompressedLength",
					new SqlParameter("PK", outboxPK),
					new SqlParameter("SenderID", senderID),
					new SqlParameter("RecipientID", message.ClientID),
					new SqlParameter("EnvelopeTrackingID", envelopeTrackingID),
					new SqlParameter("MessageTrackingID", message.MessageTrackingID),
					new SqlParameter("InternalTrackingID", inboxPK),
					new SqlParameter("OverrideEmailSubject", message.EmailSubject),
					new SqlParameter("OverrideFilename", message.FileName),
					new SqlParameter("Status", MessageStatus.Processing),
					new SqlParameter("InsertUTC", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")),
					new SqlParameter("TargetMessageType", message.SchemaName),
					new SqlParameter("Content", outboxContentString),
					new SqlParameter("XMLContent", DBNull.Value),
					new SqlParameter("UncompressedLength", targetStream.Length));
			}

			if (!isSmallMessage)
			{
				outboxContent.SeekBegin();
				var reader = new StreamReader(outboxContent);
				char[] buffer = new char[StreamExtensions.BufferSize];
				int charsread = reader.Read(buffer, 0, buffer.Length);
				int offset = 0;
				while (charsread > 0)
				{
					if (charsread < buffer.Length) Array.Resize(ref buffer, charsread);
					context.ExecuteSqlCommand("exec [dbo].[WriteOutboxContent] @OutboxPK, @Offset, @ContentChunk",
						new SqlParameter("OutboxPK", outboxPK),
						new SqlParameter("Offset", offset),
						new SqlParameter("ContentChunk", buffer));
					offset += charsread;
					charsread = reader.Read(buffer, 0, buffer.Length);
				}
			}
		}

		#endregion
		public static void FailMessages(string senderID, string recipientID, ArrayList outboxMessageTrackingIDArrayList, string error, ILog logger = null)
		{
			try
			{
				var exceptionArrayList = new ArrayList { new Exception(error) };
				DatabaseAccessHelpers.AccessDatabaseWithRetries(() => FailMessages(senderID, recipientID, outboxMessageTrackingIDArrayList, exceptionArrayList, true), logger);
			}
			catch (Exception exception)
			{
				throw new Exception($"Exception thrown when failing messages: {exception}\nOriginal error is: {error}");
			}
		}

		public static void FailMessages(string senderID, string recipientID, ArrayList outboxMessageTrackingIDArrayList, ArrayList exceptionArrayList, ILog logger = null)
		{
			try
			{
				DatabaseAccessHelpers.AccessDatabaseWithRetries(() => FailMessages(senderID, recipientID, outboxMessageTrackingIDArrayList, exceptionArrayList, false), logger);
			}
			catch (Exception exception)
			{
				throw new Exception($"Exception thrown when failing messages: {exception}");
			}
		}

		private static void FailMessages(string senderID, string recipientID, ArrayList outboxMessageTrackingIDArrayList, ArrayList exceptionArrayList, bool isSingleError)
		{
			var outboxMessageTrackingIDs = outboxMessageTrackingIDArrayList.ToArray(typeof(string)).Cast<string>().ToArray();

			using (var context = NewEHubTransactionContext())
			using (var transaction = context.BeginTransaction())
			{
				var index = 0;
				foreach (var outboxMessageTrackingID in outboxMessageTrackingIDs)
				{
					var exception = (isSingleError ? exceptionArrayList[0] : exceptionArrayList[index++]) as Exception;
					context.ExecuteSqlCommand("exec [dbo].[InsertError] @ErrorPK, @Source, @ErrorType, @Description, @ErrorDetail, @InboxPK, @OutboxPK, @CurrentDateTimeUTC, @InboxMessageTrackingID, @OutboxMessageTrackingID, @Alerted",
						new SqlParameter("ErrorPK", Guid.NewGuid()),
						new SqlParameter("Source", "BIZ"),
						new SqlParameter("ErrorType", "Fai"),
						new SqlParameter("Description", isSingleError ? exception.Message : exception.ToString()),
						new SqlParameter("ErrorDetail", DBNull.Value),
						new SqlParameter("InboxPK", DBNull.Value),
						new SqlParameter("OutboxPK", DBNull.Value),
						new SqlParameter("CurrentDateTimeUTC", DateTime.UtcNow),
						new SqlParameter("InboxMessageTrackingID", DBNull.Value),
						new SqlParameter("OutboxMessageTrackingID", new Guid(outboxMessageTrackingID)),
						new SqlParameter("Alerted", 1));

					var outboxMessageType = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == outboxMessageTrackingID).Select(_ => _.OI_DT_Target).First();
					var outboxMessageContent = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == outboxMessageTrackingID).Select(_ => _.OI_Content).First();
					var senderPK = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == outboxMessageTrackingID).Select(_ => _.OI_CC_Sender).First();
					var recipientPK = context.eHubClients.Single(_ => _.CC_ID == recipientID).CC_PK;
					var messageType = context.eHubMessageTypes.Single(_ => _.DT_PK == outboxMessageType).DT_Code;

					var inboxMessage = new eHubInboxMessage()
					{
						EI_PK = Guid.NewGuid(),
						EI_MessageTrackingID = Guid.NewGuid().ToString(),
						EI_EnvelopeTrackingID = Guid.Empty.ToString(),
						EI_CC_Sender = recipientPK,
						EI_CC_Recipient = senderPK,
						EI_MessageType = messageType,
						EI_IsFlatFile = false,
						EI_EmailSubjectOverride = string.Empty,
						EI_FileNameOverride = FormatError(exception.Message, outboxMessageTrackingID),
						EI_ApplicationCode = "SGC",
						EI_Status = 0,
						EI_Content = outboxMessageContent,
						EI_InsertUTC = DateTime.UtcNow,
						EI_SN = null
					};
					context.eHubInboxMessages.Add(inboxMessage);
				}

				context.SaveChanges();

				transaction.Commit();
			}
		}

		public static void UpdateMessageStatus(int outboxStatus, ArrayList outboxMessageTrackingIDArrayList, ILog logger = null)
		{
			var outboxMessageTrackingIDs = outboxMessageTrackingIDArrayList.ToArray(typeof(string)).Cast<string>().ToArray();

			DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = NewEHubTransactionContext())
				using (var transaction = context.BeginTransaction())
				{
					foreach (var outboxMessageTrackingID in outboxMessageTrackingIDs)
					{
						context.ExecuteSqlCommand("exec [dbo].[UpdateMessageStatus] @MessageTrackingID, @InboxPK, @OutboxPK, @InboxStatus, @OutboxStatus",
							new SqlParameter("MessageTrackingID", outboxMessageTrackingID),
							new SqlParameter("InboxPK", DBNull.Value),
							new SqlParameter("OutboxPK", DBNull.Value),
							new SqlParameter("InboxStatus", DBNull.Value),
							new SqlParameter("OutboxStatus", value: 0));
					}

					transaction.Commit();
				}
			}, logger);
		}

		public static void UpdateOutboxMessageBatchEnvelopeTrackingID(string batchEnvelopeTrackingID, ArrayList messageTrackingIDArrayList)
		{
			var messageTrackingIDs = messageTrackingIDArrayList.ToArray(typeof(string)).Cast<string>().ToArray();

			using (var context = NewEHubTransactionContext())
			{
				using (var transaction = context.BeginTransaction())
				{
					try
					{
						foreach (var messageTrackingID in messageTrackingIDs)
						{
							var outboxMessagePK = context.eHubOutboxMessages.Where(_ => _.OI_MessageTrackingID == messageTrackingID).Select(_ => _.OI_PK).First();
							context.ExecuteSqlCommand("update eHubOutboxMessage set OI_BatchEnvelopeTrackingID = @batchEnvelopeTrackingID where OI_PK = @pk", new SqlParameter("batchEnvelopeTrackingID", batchEnvelopeTrackingID), new SqlParameter("pk", outboxMessagePK));
						}

						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw;
					}
				}
			}
		}

        public static void RemoveFailedMessages(ArrayList messageTrackingIDs, ArrayList failedMessageTrackingIDs)
        {
            if (messageTrackingIDs.Count > 0 && failedMessageTrackingIDs.Count > 0)
	        {
                foreach (var failedMessageTrackingId in failedMessageTrackingIDs)
	            {
                    messageTrackingIDs.Remove(failedMessageTrackingId);
	            }
	        }
        }

		public static bool IsAccountValid(string clientID, string accountID, ILog logger = null)
		{
			return 1 == GetAccountFlag(clientID, accountID, logger);
		}

		public static int GetAccountFlag(string clientID, string accountID, ILog logger = null)
		{
			var account = DatabaseAccessHelpers.AccessDatabaseWithRetries(() => GetAccount(accountID, clientID), logger);
			if (account == null) return 0;
			return account.CX_Flag1 ?? 0;
		}

		public static string GetAccountPassword(string clientID, string accountID, ILog logger)
		{
			var account = DatabaseAccessHelpers.AccessDatabaseWithRetries(() => GetAccount(accountID, clientID), logger);
			if (account == null) return string.Empty;
			return account.CX_Password1;
		}

        private static eHubClientRegistration GetAccount(string accountID, string clientID)
		{
			using (var context = NewEHubTransactionContext())
			{
				return (from registration in context.eHubClientRegistrations
					join registrationType in context.eHubRegistrationTypes on registration.CX_RT equals registrationType.RT_PK
					join client in context.eHubClients on registration.CX_CC equals client.CC_PK
					where registrationType.RT_ID == "SGCustomsAccount" && client.CC_ID == clientID && registration.CX_Code == accountID
					select registration).FirstOrDefault();
			}
		}

		public static string FormatError(string error, string outboxMessageTrackingID)
		{
			var result = string.Empty;
			var errorCode = "99999999"; //A number that SGCustoms unlikely uses.
			var errorMessage = error;

			var match = Regex.Match(error, "^Status: .*?; State: .*?; ErrorCode: (?<ErrorCode>.*?); ErrorMessage: (?<ErrorMessage>.*?)$", RegexOptions.Compiled);
			if (match.Success)
			{
				errorCode = match.Groups["ErrorCode"].Value;
				errorMessage = match.Groups["ErrorMessage"].Value;
			}

			result = $"ErrorCode: {errorCode};ErrorMessage: {errorMessage};LinkedOutboxMessageTrackingID: {outboxMessageTrackingID}.";

			return Truncate(result, 256); //Promoted properties are limited to 256 characters in length.
		}

		static string Truncate(string input, int maxLength)
		{
			if (string.IsNullOrEmpty(input) || input.Length <= maxLength) return input;
			return input.Substring(0, maxLength);
		}

		public static Func<eHubTransactionsContext> NewEHubTransactionContext = () => new eHubTransactionsContext();
	}
}
