using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ZACustoms.Helpers;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using CargoWise.eHub.Shared.Crypto;
using CargoWise.eHub.Shared.Mime;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.ZACustoms.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[System.Runtime.InteropServices.Guid("ca53f1dc-e599-4775-aaf9-624074a852b8")]
	[ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
	public class AS2Disassembler : ComponentBase, IDisassemblerComponent
	{
		protected override string DisplayName { get { return "ZAC AS2 Disassembler"; } }
		protected override Guid ClassID { get { return new Guid("5af9e6bd-4a01-4722-9841-434b5d2b1f2d"); } }
		ILog logger;

		public string ApplicationCode { get; set; }
		public string SenderID { get; set; }
		public string SchemaName { get; set; }

		public void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			logger = GetLogger(pInMsg);

			bool inboxMessageLogged = false;
			eHubCertificate recipientCert = null;
			string messageTrackingID = pInMsg.Context.ReadPropertyString<BTS.InterchangeID>().Trim('{', '}');

			try
			{
				LoggerHelpers.LogComponentStart(logger, this);
				logger.InfoFormat("Processing message with BizTalk MessageID: {0}", pInMsg.MessageID);

				if (logger.IsTraceEnabled)
					LoggerHelpers.LogProperties(logger, this);

				if (logger.IsTraceEnabled)
					LoggerHelpers.LogMessageContextProperties(pInMsg, logger);

				Guid inboxPK = GetNewGuid();
				pInMsg.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
				pInMsg.Context.WriteProperty<BTS.MessageType>("ZACustomsAS2Receive");
				pInMsg.Context.WriteProperty<InternalTrackingID>(inboxPK.ToString());
				pInMsg.Context.WriteProperty<BTS.SourceParty>(this.SenderID);
				new PipelineHelpers().CreateSeekableMessageStream(pContext, pInMsg, logger);
				Stream inMsgStream = pInMsg.BodyPart.GetOriginalDataStream();

				var inboxMsg = new eHubInboxMessage
				{
					EI_PK = inboxPK,
					EI_MessageTrackingID = messageTrackingID,
					EI_EnvelopeTrackingID = Guid.Empty.ToString(),
					eHubClient_Sender = new eHubClient { CC_ID = this.SenderID },
					EI_MessageType = String.IsNullOrWhiteSpace(this.SchemaName) ? null : this.SchemaName,
					EI_ApplicationCode = this.ApplicationCode,
					EI_Status = 1,
					EI_InsertUTC = GetUtcNow(),
					EI_LastUpdateUTC = GetUtcNow()
				};
				eHubTransactionsAccessor.WriteMessageToInbox_WithRetries(inboxMsg, inMsgStream, logger);
				inboxMessageLogged = true;
				inMsgStream.Position = 0;

				using (var decryptedStream = GetDecryptedContent(pInMsg, inMsgStream, out recipientCert))
				using (var mimeMessage = (MimePart.Multipart)MimePart.Parse(decryptedStream))
				{
					var messagePart = mimeMessage.Parts[0] as MimePart.Content;
					var signaturePart = mimeMessage.Parts[1] as MimePart.Content;
					VerifySignature(pInMsg, messagePart.Raw, signaturePart.Contents);
					CreateOutboxMessage(messagePart.Contents, recipientCert.eHubClient.CC_ID, inboxPK, messageTrackingID);

					logger.InfoFormat("Generating MDN for success");
					string sha256Mic = Convert.ToBase64String(SHA256.Create().ComputeHash(messagePart.Raw));
					EnqueueMdnMimeMessage(pInMsg, recipientCert, sha256Mic, null);
				}
			}
			catch (Exception ex)
			{
				logger.Warn("Exception: ", ex);
				logger.InfoFormat("Generating MDN for error");
				EnqueueMdnMimeMessage(pInMsg, recipientCert, null, ex);
				if (inboxMessageLogged)
					eHubTransactionsAccessor.InsertError_WithRetries(messageTrackingID, BuildExceptionDescription(pInMsg, ex), logger);
			}
			finally
			{
				logger.Info("Finished processing message");
				LoggerHelpers.LogComponentEnd(logger, this);
			}
		}

		Stream GetDecryptedContent(IBaseMessage pInMsg, Stream encryptedStream, out eHubCertificate recipientCert)
		{
			recipientCert = null;
			try
			{
				logger.Info("Decrypting message");

				byte[] encryptedBytes = ReadAllStreamAsBytes(encryptedStream);
				var recipientCertSubjectID = CmsHelpers.GetRecipientIdentifier(encryptedBytes);
				recipientCert = GetCertificateFromIdentifier(pInMsg, recipientCertSubjectID, true, logger);
				if (recipientCert == null)
					throw new Exception("Encrypting certificate invalid");
				if (logger.IsTraceEnabled)
					LogCertificateDetails(recipientCert, "Recipient certificate", logger);

				byte[] decryptedBytes = CmsHelpers.DecryptMessage(encryptedBytes, recipientCert.CE_BinaryContainer, recipientCert.CE_Password);
				return new MemoryStream(decryptedBytes);
			}
			catch (Exception ex)
			{
				if (recipientCert != null)
				{
					var certificateDetails = GetCertificateDetails(recipientCert, "Recipient certificate");

					throw new ExceptionForMdn($"processed/error: decryption-failed. {certificateDetails}", ex);
				}
				throw new ExceptionForMdn("processed/error: decryption-failed", ex);
			}
		}

		void VerifySignature(IBaseMessage pInMsg, Stream body, Stream signature)
		{
			try
			{
				byte[] signedData = ReadAllStreamAsBytes(body);
				byte[] signatureBytes = Convert.FromBase64String(new StreamReader(signature).ReadToEnd());
				SubjectIdentifier signingCertSubjectID = CmsHelpers.GetSignerIdentifier(signatureBytes);
				var signerCert = GetCertificateFromIdentifier(pInMsg, signingCertSubjectID, false, logger);
				if (signerCert == null)
					throw new Exception("Signing certificate not available");
				if (logger.IsTraceEnabled)
					LogCertificateDetails(signerCert, "Signer certificate", logger);

				CmsHelpers.VerifySignature(signedData, signatureBytes, signerCert.CE_BinaryContainer);
			}
			catch (Exception ex)
			{
				throw new ExceptionForMdn("processed/error: authentication-failed", ex);
			}

			body.Position = signature.Position = 0;
		}

		void EnqueueMdnMimeMessage(IBaseMessage pInMsg, eHubCertificate signingCert, string sha256Mic, Exception ex)
		{
			Dictionary<string, string> inputHeaders = MimeUtils.ParseHeaders(pInMsg.Context.ReadPropertyString<HTTP.InboundHttpHeaders>());

			var textPart = new MimePart.Content("text/plain");
			textPart.Headers["Content-Transfer-Encoding"] = "binary";
			textPart.Headers["Content-Id"] = "<" + GetNewGuid().ToString().ToUpper() + ">";
			textPart.Headers["Content-Description"] = "plain";

			var mdnPart = new MimePart.Content("message/disposition-notification");
			mdnPart.Headers["Content-Transfer-Encoding"] = "7bit";
			mdnPart.Headers["Content-Id"] = "<" + GetNewGuid().ToString().ToUpper() + ">";
			mdnPart.Headers["Content-Description"] = "body";

			var mdnFields = new Dictionary<string, string>();
			mdnFields["Final-Recipient"] = "rfc822; " + inputHeaders["AS2-To"];
			mdnFields["Original-Message-ID"] = inputHeaders["Message-ID"];
			if (!String.IsNullOrWhiteSpace(sha256Mic)) mdnFields["Received-Content-MIC"] = sha256Mic + ", sha256";
			mdnFields["Disposition"] = "automatic-action/MDN-sent-automatically; ";
			if (ex == null)
			{
				mdnFields["Disposition"] += "processed";
			}
			else if (ex is ExceptionForMdn)
			{
				mdnFields["Disposition"] += ex.Message;
				mdnFields["Error"] = ex.InnerException.Message;
			}
			else
			{
				mdnFields["Disposition"] += "processed/error: unexpected-processing-error";
				mdnFields["Error"] = ex.Message;
			}
			mdnPart.Contents = MimeUtils.FormatHeaders(mdnFields);

			string boundary = "_" + GetNewGuid().ToString().ToUpper() + "_";
			var mdnReport = new MimePart.Multipart("report; report-type=disposition-notification", boundary);
			mdnReport.Parts.Add(textPart);
			mdnReport.Parts.Add(mdnPart);
			if (signingCert == null)
				signingCert = CertificatesHelper.GetCertificateForMdnSigning(SenderID, inputHeaders["AS2-To"], logger);

			Stream mdnMessage;
			var headers = new Dictionary<string, string>();

			if (signingCert != null)
			{
				try
				{
					mdnMessage = mdnReport.FormatAsSMime(signingCert.CE_BinaryContainer, signingCert.CE_Password);
					headers["Message-Id"] = $"<{signingCert.eHubClient.CC_ID}_{pInMsg.Context.ReadPropertyString<MessageTrackingID>()}>";
				}
				catch
				{
					mdnMessage = mdnReport.Format();
					headers["Message-Id"] = $"<WISETECH_{pInMsg.Context.ReadPropertyString<MessageTrackingID>()}>";
				}
			}
			else
			{
				mdnMessage = mdnReport.Format();
				headers["Message-Id"] = $"<WISETECH_{pInMsg.Context.ReadPropertyString<MessageTrackingID>()}>";
			}
			headers["MIME-Version"] = "1.0";
			headers["AS2-Version"] = "1.2";
			headers["AS2-To"] = inputHeaders["AS2-From"];
			headers["AS2-From"] = inputHeaders["AS2-To"];
			headers["EDIINT-Features"] = "multiple-attachments";

			using (var hdrRdr = new StreamReader(MimeUtils.FormatHeaders(headers)))
				pInMsg.Context.WriteProperty<HTTP.UserHttpHeaders>(hdrRdr.ReadToEnd().Trim());
			pInMsg.Context.WriteProperty<BTS.RouteDirectToTP>("True");

			using (Stream smimeHeaderStream = MimeUtils.ExtractHeaders(mdnMessage))
			{
				var smimeHeaders = MimeUtils.ParseHeaders(smimeHeaderStream);
				string content = Regex.Replace(smimeHeaders["Content-Type"], @"\s+", " ");
				content = Regex.Replace(content, @"boundary=(_[\w-]{36}_)", @"boundary=""$1""");
				pInMsg.Context.WriteProperty<HTTP.ContentType>(content);
			}

			if (logger.IsTraceEnabled)
			{
				var position = mdnMessage.Position;
				logger.TraceFormat("MDN Content: {0}", new StreamReader(mdnMessage).ReadToEnd());
				mdnMessage.Position = position;
			}
			pInMsg.BodyPart.Data = mdnMessage;
			messageQueue.Enqueue(pInMsg);
		}

		private void CreateOutboxMessage(Stream contentStream, string recipientID, Guid inboxPK, string messageTrackingID)
		{
			logger.InfoFormat("Creating outbox message");
			var outboxMsg = new eHubOutboxMessage();
			outboxMsg.OI_PK = GetNewGuid();
			outboxMsg.eHubClient_Sender = new eHubClient { CC_ID = this.SenderID };
			outboxMsg.eHubClient_Recipient = new eHubClient { CC_ID = recipientID };
			outboxMsg.eHubInboxMessage = new eHubInboxMessage { EI_PK = inboxPK, EI_Status = 2 };
			outboxMsg.OI_MessageTrackingID = messageTrackingID;
			outboxMsg.OI_Status = 0;
			outboxMsg.eHubMessageType = new eHubMessageType { DT_Code = this.SchemaName };
			outboxMsg.OI_InsertUTC = GetUtcNow();
			contentStream.Position = 0;
			eHubTransactionsAccessor.WriteMessageToOutbox_WithRetries(outboxMsg, contentStream, logger);
		}

		protected virtual eHubCertificate GetCertificateFromIdentifier(IBaseMessage pInMsg, SubjectIdentifier subjectIdentifier, bool privateKey, ILog logger)
		{
			using (var context = GetDbContext())
			{
				var certificates = context.eHubCertificates.Include("eHubClient").Where(c => c.CE_Category == SenderID && privateKey == (c.CE_Password != null));
				eHubCertificate certificate = null;
				var formattedSubjectIdentifier = string.Empty;
				switch (subjectIdentifier.Type)
				{
					case SubjectIdentifierType.IssuerAndSerialNumber:
						var issuerSerial = (X509IssuerSerial)subjectIdentifier.Value;
						formattedSubjectIdentifier = $"{{IssueName: {issuerSerial.IssuerName}, SerialNumber: {issuerSerial.SerialNumber}}}";
						certificate = certificates.FirstOrDefault(c => c.CE_Issuer == issuerSerial.IssuerName && c.CE_SerialNumber == issuerSerial.SerialNumber);
						break;
					case SubjectIdentifierType.SubjectKeyIdentifier:
						formattedSubjectIdentifier = $"{{SubjectKeyIdentifier: {(string) subjectIdentifier.Value}}}";
						certificate = certificates.FirstOrDefault(c => c.CE_SubjectKeyIdentifier == (string)subjectIdentifier.Value);
						break;
					case SubjectIdentifierType.NoSignature:
					case SubjectIdentifierType.Unknown:
					default:
						break;
				}

				if (logger.IsTraceEnabled && !string.IsNullOrEmpty(formattedSubjectIdentifier))
					logger.Trace($"SubjectIdentifier: {formattedSubjectIdentifier}");

				if (certificate == null) pInMsg.Context.Write("SubjectIdentifier", "http://cargowise.com/ehub/processing/2010/06", formattedSubjectIdentifier);

				return certificate;
			}
		}

		private string BuildExceptionDescription(IBaseMessage message, Exception e)
		{
			var description = new StringBuilder(e.ToString()).Append(IssueMessageDelimiter);
			string name, ns;
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				object value = message.Context.ReadAt(i, out name, out ns);
				description.AppendFormat("{0}#{1} = {2}", ns, name, value).AppendLine();
			}

			return description.ToString();
		}

		static void LogCertificateDetails(eHubCertificate cert, string description, ILog logger)
		{
			var certificateDetails = GetCertificateDetails(cert, description);
			logger.TraceFormat(certificateDetails);
		}

		static string GetCertificateDetails(eHubCertificate cert, string description)
		{
			return $@"{description}:
	PK: {cert.CE_PK}
	Active From: {cert.CE_ActiveFromUTC:s}
	Valid From: {cert.CE_ValidFromUTC:s}
	Valid To: {cert.CE_ValidToUTC:s}
	Thumbprint: {cert.CE_Thumbprint}
	Password: {cert.CE_Password}
	Issuer: {cert.CE_Issuer}
	Serial Number: {cert.CE_SerialNumber}
	Subject Key Identifier: {cert.CE_SubjectKeyIdentifier}";
		}

		internal static byte[] ReadAllStreamAsBytes(Stream encryptedStream)
		{
			using (var memStream = new MemoryStream())
			{
				encryptedStream.CopyTo(memStream);
				return memStream.ToArray();
			}
		}

		public IBaseMessage GetNext(IPipelineContext pContext)
		{
			IBaseMessage message = null;
			LoggerHelpers.LogComponentStart(logger, this);

			if (messageQueue.Count > 0)
			{
				message = messageQueue.Dequeue();
				logger.InfoFormat("Dequeuing message to pipeline with MessageID: {0}", message.MessageID);
			}
			else
				logger.Info("End of queued messages");

			LoggerHelpers.LogComponentEnd(logger, this);
			return message;
		}

		Queue<IBaseMessage> messageQueue = new Queue<IBaseMessage>();

		internal static Func<IBaseMessage, ILog> GetLogger = (pInMsg) => LoggerHelpers.GetPipelineLogger(pInMsg);
		internal static Func<Guid> GetNewGuid = () => Guid.NewGuid();
		internal static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();
		internal static Func<DateTime> GetUtcNow = () => DateTime.UtcNow;
		const string IssueMessageDelimiter = "\r\n----------BIZTALK PROPERTIES----------\r\n";

		class ExceptionForMdn : Exception
		{
			public ExceptionForMdn(string message) : base(message) { }
			public ExceptionForMdn(string message, Exception innerException) : base(message, innerException) { }
		}
	}
}
