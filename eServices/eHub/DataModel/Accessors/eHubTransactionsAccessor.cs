using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;
using System.Text;

namespace CargoWise.eHub.DataModel.Accessors
{
	public class eHubTransactionsAccessor
	{
		public eHubTransactionsAccessor() { }

		#region GetCodeMappedValue
		public static string GetCodeMappedValue(string senderID, string recipientID, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
		{
			using (var context = GetDbContext())
				return context.SqlQuery<string>(@"EXEC [dbo].[GetRecipientCode] 
@senderClientCode = @p0, 
@recipientClientCode = @p1, 
@transformationName = @p2, 
@codeSetName = @p3, 
@resultField = @p4, 
@key1Value = @p5, 
@key2Value = @p6, 
@key3Value = @p7, 
@key4Value = @p8, 
@key5Value = @p9", senderID, recipientID, transformationName, codeSet, resultField, key1, key2, key3, key4, key5).Single();
		}

		public static string GetCodeMappedValue_WithRetries(string senderID, string recipientID, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5, ILog logger = null)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<string>(() => GetCodeMappedValue(senderID, recipientID, transformationName, codeSet, resultField, key1, key2, key3, key4, key5), logger);
		}

		#endregion

		#region eHubAsyncPollingRegistration

		public static string GetAsyncPollingConfig(string pr_pk)
		{
			using (var context = GetDbContext())
			{
				var PR_PK = Guid.Parse(pr_pk);
				var result = context.eHubAsyncPollingRegistrations.SingleOrDefault(x => x.PR_PK == PR_PK);
				return result?.PR_XML;
			}
		}

		public static string GetAsyncPollingConfig_WithRetries(string pr_pk, ILog logger = null)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<string>(() => GetAsyncPollingConfig(pr_pk), logger);
		}

		#endregion

		#region GetCounterValue
		public static string GetCounterValue(string name, int padLength)
		{
			using (var context = GetDbContext())
				return context.SqlQuery<string>("DECLARE @value varchar(20); EXEC [dbo].[GetCounterValue] @name = @p0, @padlength = @p1, @value = @value OUTPUT; SELECT @value;", name, padLength).FirstOrDefault();
		}

		public static string GetCounterValue_WithRetries(string name, int padLength, ILog logger)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<string>(() => GetCounterValue(name, padLength), logger);
		}
		#endregion

		#region GetCounterInterfaceValue
		public static string GetCounterInterfaceValue(string transformationSetName, string name, Int64 maxValue, int incrementValue)
		{
			using (var context = GetDbContext())
				return context.SqlQuery<string>(@"DECLARE @startValue bigint; 
EXEC [dbo].[GetCounterInterfaceValue] 
@TransformatonSetName = @p0, 
@Name = @p1, 
@MaxValue = @p2, 
@IncrementValue = @p3, 
@StartValue = @startValue OUTPUT; 
SELECT CAST(@startValue as varchar(max));", transformationSetName, name, maxValue, incrementValue).FirstOrDefault();
		}

		public static string GetCounterInterfaceValue_WithRetries(string transformationSetName, string name, Int64 maxValue, int incrementValue, ILog logger = null)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<string>(() => GetCounterInterfaceValue(transformationSetName, name, maxValue, incrementValue), logger);
		}
		#endregion

		#region IsProductionClient
		public static bool IsProductionClient(string clientID)
		{
			using (var context = GetDbContext())
				return context.SqlQuery<string>("EXEC [ediProdCache].[dbo].[SelectLicenceType] @EnterpriseCode = @p0, @ServerCode = @p1", clientID.Substring(0, 3), clientID.Substring(6, 3)).FirstOrDefault() == "PRD";
		}

		public static bool IsProductionClient_WithRetries(string clientID, ILog logger)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<bool>(() => IsProductionClient(clientID), logger);
		}
		#endregion

		#region UpdateMessageDistributionStatus
		public static void UpdateMessageDistributionStatus(Guid messageTrackingID, string senderID, string recipientID)
		{
			using (var context = GetDbContext())
			using (var transaction = context.BeginTransaction())
			{
				context.ExecuteSqlCommand("EXEC [dbo].[UpdateMessageDistributionStatus] @MessageTrackingID = @p0, @SenderID = @p1, @RecipientID = @p2", messageTrackingID.ToString("D"), senderID, recipientID);
				transaction.Commit();
			}
		}

		public static void UpdateMessageDistributionStatus_WithRetries(Guid messageTrackingID, string senderID, string recipientID, ILog logger)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => UpdateMessageDistributionStatus(messageTrackingID, senderID, recipientID), logger);
		}
		#endregion

		#region InsertError
		public static void InsertError(string messageTrackingID, string description)
		{
			using (var context = GetDbContext())
			using (var transaction = context.BeginTransaction())
			{
				Guid? inboxPK = context.eHubInboxMessages.Where(i => i.EI_MessageTrackingID == messageTrackingID).Select(i => i.EI_PK).FirstOrDefault();
				if (inboxPK == Guid.Empty)
					inboxPK = null;
				Guid? outboxPK = context.eHubOutboxMessages.Where(o => o.OI_MessageTrackingID == messageTrackingID).Select(o => o.OI_PK).FirstOrDefault();
				if (outboxPK == Guid.Empty)
					outboxPK = null;
				context.ExecuteSqlCommand("EXEC [dbo].[InsertError] @ErrorPK = @p0, @Source = 'BIZ', @ErrorType = 'Fai', @Description = @p1, @InboxPK = @p2, @OutboxPK = @p3, @CurrentDateTimeUTC = @p4",
					GetNewGuid(), description, inboxPK, outboxPK, GetDateTimeUtcNow().ToString("s"));
				transaction.Commit();
			};
		}

		public static void InsertError_WithRetries(string messageTrackingID, string description, ILog logger)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => InsertError(messageTrackingID, description), logger);
		}
		#endregion

		#region WriteMessageToInbox
		public static void WriteMessageToInbox(eHubInboxMessage inboxMsg, Stream content, ILog logger)
		{
			logger.Debug("Starting write inbox record.");
			using (var context = GetDbContext())
			using (var transaction = context.BeginTransaction())
			{
				if (inboxMsg.eHubClient_Sender != null)
				{
					inboxMsg.EI_CC_Sender = context.eHubClients.Where(c => c.CC_ID == inboxMsg.eHubClient_Sender.CC_ID).Select(c => c.CC_PK).SingleOrDefault();
					inboxMsg.eHubClient_Sender = null;
				}
				if (inboxMsg.eHubClient_Recipient != null)
				{
					inboxMsg.EI_CC_Recipient = context.eHubClients.Where(c => c.CC_ID == inboxMsg.eHubClient_Recipient.CC_ID).Select(c => c.CC_PK).SingleOrDefault();
					inboxMsg.eHubClient_Recipient = null;
				}
				inboxMsg.EI_Content = String.Empty;

				DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
				{
					logger.Debug("Inserting inbox record to database.");
					context.eHubInboxMessages.Add(inboxMsg);
					context.SaveChanges();
					logger.Debug("Adding compressed and encoded content to inbox record.");
					WriteCompressedAndEncodedStreamToDB(context, content,
						"UPDATE eHubInboxMessage SET EI_Content.Write({0}, {1}, 0) WHERE EI_PK = {2}", inboxMsg.EI_PK);
					transaction.Commit();
				}, logger);

				logger.InfoFormat("Saved inbox record to database. InboxPK='{0:B}' MessageTrackingID='{{{1}}}'", inboxMsg.EI_PK, inboxMsg.EI_MessageTrackingID);
			}
		}

		public static void WriteMessageToInbox_WithRetries(eHubInboxMessage inboxMsg, Stream content, ILog logger)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => WriteMessageToInbox(inboxMsg, content, logger), logger);
		}
		#endregion

		#region WriteMessageToOutbox
		public static void WriteMessageToOutbox(eHubOutboxMessage outboxMsg, Stream content, ILog logger)
		{
			logger.Debug("Starting write outbox record.");
			using (var context = GetDbContext())
			using (var transaction = context.BeginTransaction())
			{
				var inboxMsg = outboxMsg.eHubInboxMessage;
				if (inboxMsg != null)
				{
					outboxMsg.OI_EI_InboxPK = inboxMsg.EI_PK;
					outboxMsg.eHubInboxMessage = null;
				}
				if (outboxMsg.eHubClient_Sender != null)
				{
					outboxMsg.OI_CC_Sender = context.eHubClients.Where(c => c.CC_ID == outboxMsg.eHubClient_Sender.CC_ID).Select(c => c.CC_PK).SingleOrDefault();
					outboxMsg.eHubClient_Sender = null;
				}
				if (outboxMsg.eHubClient_Recipient != null)
				{
					outboxMsg.OI_CC_Recipient = context.eHubClients.Where(c => c.CC_ID == outboxMsg.eHubClient_Recipient.CC_ID).Select(c => c.CC_PK).SingleOrDefault();
					outboxMsg.eHubClient_Recipient = null;
				}
				if (outboxMsg.eHubMessageType != null)
				{
					outboxMsg.OI_DT_Target = context.eHubMessageTypes.Where(m => m.DT_Code == outboxMsg.eHubMessageType.DT_Code).Select(m => m.DT_PK).SingleOrDefault();
					outboxMsg.eHubMessageType = null;
				}
				outboxMsg.OI_Content = String.Empty;

				DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
				{
					logger.Debug("Inserting outbox record to database.");
					context.eHubOutboxMessages.Add(outboxMsg);
					context.SaveChanges();
					logger.Debug("Adding compressed and encoded content to database record.");
					WriteCompressedAndEncodedStreamToDB(context, content,
						"UPDATE eHubOutboxMessage SET OI_Content.Write({0}, {1}, 0) WHERE OI_PK = {2}", outboxMsg.OI_PK);
					if (inboxMsg != null)
					{
						logger.Debug("Updating inbox record status.");
						context.ExecuteSqlCommand("UPDATE eHubInboxMessage SET EI_Status = @p0, EI_LastUpdateUTC = @p1 WHERE EI_PK = @p2", inboxMsg.EI_Status, DateTime.UtcNow, inboxMsg.EI_PK);
					}
					transaction.Commit();
					logger.InfoFormat("Saved eHubOutboxMessage to database. OutboxPK='{0:B}' OutboxMessageTrackingID='{{{1}}}' InboxPK='{2:B}'", outboxMsg.OI_PK, outboxMsg.OI_MessageTrackingID, outboxMsg.OI_EI_InboxPK);
				}, logger);
			}
		}

		public static void WriteMessageToOutbox_WithRetries(eHubOutboxMessage outboxMsg, Stream content, ILog logger)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => WriteMessageToOutbox(outboxMsg, content, logger), logger);
		}
		#endregion

		#region WriteCompressedAndEncodedStreamToDB
		internal static void WriteCompressedAndEncodedStreamToDB(ContextBase context, Stream contentData, string command, params object[] parameters)
		{
			using (var contentWriterStream = new ContentWriterStream(context, command, parameters))
			using (var encoder = new CryptoStream(contentWriterStream, new ToBase64Transform(), CryptoStreamMode.Write))
			using (var compressor = new GZipStream(encoder, CompressionMode.Compress, true))
			{
				var parms = new object[parameters.Length + 2];
				parameters.CopyTo(parms, 2);
				int offset = 0;
				long compEncLastPos = 0;
				var buffer = new byte[BUFFER_LEN];
				int bytesRead;
				while ((bytesRead = contentData.Read(buffer, 0, buffer.Length)) > 0)
				{
					compressor.Write(buffer, 0, bytesRead);
					compressor.Flush();
					encoder.Flush();
					contentWriterStream.Position = compEncLastPos;
					while (contentWriterStream.Position < contentWriterStream.Length)
					{
						while ((bytesRead = contentWriterStream.Read(buffer, 0, BUFFER_LEN / 2)) > 0)
						{
							parms[0] = Encoding.ASCII.GetChars(buffer, 0, bytesRead);
							parms[1] = offset;
							context.ExecuteSqlCommand(command, parms);
							offset += bytesRead;
						}
					}
					compEncLastPos = contentWriterStream.Position;
				}
			}
		}

		class ContentWriterStream : Stream
		{
			public override bool CanWrite { get { return true; } }
			public override bool CanRead { get { return false; } }
			public override bool CanSeek { get { return false; } }
			long length = 0;
			public override long Length { get { return length; } }
			long position = 0;
			public override long Position
			{
				get { return position; }
				set { if (value != position) throw new NotImplementedException(); }
			}

			ContextBase context;
			string command;
			object[] sqlParms;
			byte[] savedBuffer = new byte[BUFFER_LEN * 2];
			int savedSize = 0;
			int dbOffset = 0;

			internal ContentWriterStream(ContextBase context, string command, params object[] parameters)
			{
				this.context = context;
				this.command = command;
				this.sqlParms = new object[parameters.Length + 2];
				parameters.CopyTo(sqlParms, 2);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if ((savedSize + count) > savedBuffer.Length)
					Array.Resize(ref savedBuffer, savedSize + count);

				Buffer.BlockCopy(buffer, offset, savedBuffer, savedSize, count);

				savedSize += count;
				while (savedSize >= BUFFER_LEN / 2)
					WriteToDb();
			}

			private void WriteToDb()
			{
				int writeSize = Math.Min(savedSize, BUFFER_LEN / 2);
				sqlParms[0] = Encoding.ASCII.GetChars(savedBuffer, 0, writeSize);
				sqlParms[1] = dbOffset;
				context.ExecuteSqlCommand(command, sqlParms);
				dbOffset += writeSize;
				savedSize -= writeSize;
				if (savedSize > 0)
					Buffer.BlockCopy(savedBuffer, writeSize, savedBuffer, 0, savedSize);
				sqlParms[0] = null;
			}

			public override void Flush()
			{
				while (savedSize > 0)
					WriteToDb();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					context = null;
					sqlParms = null;
					savedBuffer = null;
				}
				base.Dispose(disposing);
			}

			public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
			public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
			public override void SetLength(long value) { throw new NotImplementedException(); }
		}
		#endregion

		#region eHubCertificate

		public static eHubCertificate GetRequiredUniqueCertificate(string systemID, string id, string category)
		{
			using (var context = GetDbContext())
			{
				var certs = context.eHubCertificates.Where(c => c.CE_Category == category && c.eHubClientSystem.EH_ID == systemID && c.CE_ID == id);
				var count = certs.Count();
				if (count != 1)
					throw new InvalidOperationException(string.Format("Expected 1 but found {0} certificate(s) matching systemID={1}, id={2}, category={3}", count, systemID, id, category));

				var currentCert = certs.First();
				var currentDate = GetDateTimeUtcNow();
				if (currentCert.CE_ValidFromUTC != null && currentCert.CE_ValidToUTC != null && (currentDate < currentCert.CE_ValidFromUTC || currentDate > currentCert.CE_ValidToUTC))
					throw new InvalidOperationException(string.Format("Certificate is expired. From {0} to {1}. SystemID={2}, id={3}, category={4}", currentCert.CE_ValidFromUTC?.ToString("s"), currentCert.CE_ValidToUTC?.ToString("s"), systemID, id, category));
				if (currentCert.CE_BinaryContainer == null || currentCert.CE_BinaryContainer.Length == 0)
					throw new InvalidOperationException(string.Format("Found empty certificate matching systemID={0}, id={1}, category={2}", systemID, id, category));
				return currentCert;
			}
		}

		public static eHubCertificate GetRequiredUniqueCertificate_WithRetries(string systemID, string id, string category, ILog logger = null)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries<eHubCertificate>(() => GetRequiredUniqueCertificate(systemID, id, category), logger);
		}

		#endregion

		public static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();
		public static Func<DateTime> GetDateTimeUtcNow = () => DateTime.UtcNow;
		public static Func<Guid> GetNewGuid = () => Guid.NewGuid();

		const int BUFFER_LEN = 4096;
	}
}
