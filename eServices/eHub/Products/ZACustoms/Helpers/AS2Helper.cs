using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Shared.Crypto;
using CargoWise.eHub.Shared.Mime;
using Common.Logging;

namespace CargoWise.eHub.Products.ZACustoms.Helpers
{
	public static class AS2Helper
	{
		public static Stream CreateSignedEncryptedMessage(string message, string senderID, string recipID, string as2From, string as2To, string msgID, string fileName, bool retrying, ILog logger)
		{
			eHubCertificate senderCert = null;
			eHubCertificate recipientCert = null;
			try
			{
				string boundary = "_" + GetNewGuid().ToString().ToUpper() + "_";

				senderCert = GetSenderCertificate(senderID, recipID, as2From, retrying, logger);
				recipientCert = GetZACustomsCertificate(recipID, as2To, logger);

				LogCertificateDetails(senderCert, "Selected signing (sender) certificate", logger);
				LogCertificateDetails(recipientCert, "Selected encrypting (recipient) certificate", logger);

				var bodyPart = new MimePart.Content("text/plain");
				bodyPart.Headers["Content-Transfer-Encoding"] = "binary";
				bodyPart.Headers["Content-Description"] = "body";
				bodyPart.Headers["Content-Disposition"] = String.Format("attachment; filename=\"{0}\"", fileName);
				bodyPart.Contents = new MemoryStream(Encoding.UTF8.GetBytes(message));

				using (var smime = bodyPart.FormatAsSMime(senderCert.CE_BinaryContainer, senderCert.CE_Password))
				using (var smimeMemStream = new MemoryStream())
				{
					smime.CopyTo(smimeMemStream);
					var smimeData = smimeMemStream.ToArray();

					if (logger.IsDebugEnabled)
						logger.Debug("AS2 Signed Message:\r\n" + Encoding.Default.GetString(smimeData));

					var tripleDES = new AlgorithmIdentifier(Oid.FromFriendlyName("3DES", OidGroup.EncryptionAlgorithm));
					var encryptedContent = CmsHelpers.EncryptMessage(smimeData, recipientCert.CE_BinaryContainer, tripleDES);
					var msgStream = new MemoryStream(encryptedContent.Length);
					msgStream.Write(encryptedContent, 0, encryptedContent.Length);
					msgStream.Position = 0;
					return msgStream;
				}
			}
			catch (Exception ex)
			{
				var messageDetails = GetMessageDetails(senderID, recipID, as2From, as2To, msgID, fileName, retrying);
				LogMessageDetails(message, messageDetails, senderID, logger, ex);

				var senderCertificateDetails = string.Empty;
				var recipientCertificateDetails = string.Empty;

				if (senderCert != null)
				{
					senderCertificateDetails = GetCertificateDetails(senderCert, "Selected signing (sender) certificate");
				}
				if (recipientCert != null)
				{
					recipientCertificateDetails = GetCertificateDetails(recipientCert, "Selected encrypting(recipient) certificate");
				}
				throw new Exception($"processed/error: encryption failed. {senderCertificateDetails}; {recipientCertificateDetails}; {messageDetails}; ", ex);
			}
		}

		static void LogMessageDetails(string message, string messageDetails, string senderID, ILog logger, Exception ex)
		{
			string logMessage = string.Format("Error while processing the following message: {0} {1}", message, messageDetails);
			logger.Error(logMessage, ex);

			if (logger.IsInfoEnabled)
			{
				try
				{
					IEnumerable<eHubCertificate> certs;
					using (var context = new eHubTransactionsContext())
					{
						certs = context.eHubCertificates.Where(c => c.CE_Category == "ZACustoms" && c.eHubClient.CC_ID == senderID).ToList();
						if (certs.Any())
						{
							var certificates = string.Join("\t+ \r\n", certs.Select(x => string.Format("ID:{0}, Owner:{1}, Owner System:{2}, Container Type:{3}, Valid From UTC:{4}, Valid to UTC:{5}, Added UTC:{6}",
								x.CE_ID, x.eHubClient.CC_ID, x.eHubClientSystem.EH_ID, x.CE_ContainerType, x.CE_ValidFromUTC, x.CE_ValidToUTC, x.CE_AddedUTC)));
							logger.InfoFormat("The list of eHubCertificates which belong to owner {0}:\r\n\t+ {1}", senderID, certificates);
						}
						else
						{
							logger.ErrorFormat("Could not find any certificate owned by {0}.", senderID);
						}
					}
				}
				catch (Exception) { }
			}
		}

		static string GetMessageDetails(string senderID, string recipID, string as2From, string as2To, string msgID, string fileName, bool retrying)
		{
			return $@"
	Sender: {senderID}
	Recipient: {recipID}
	AS2From: {as2From}
	AS2To: {as2To}
	MsgID: {msgID}
	FileName: {fileName}
	Retrying: {retrying}";
		}

		static void LogCertificateDetails(eHubCertificate cert, string description, ILog logger)
		{
			var certificateDetails = GetCertificateDetails(cert, description);
			logger.Trace(certificateDetails);
		}

		static string GetCertificateDetails(eHubCertificate cert, string description)
		{
			return $@"{description}:
	PK: {cert.CE_PK}
	Active From: {cert.CE_ActiveFromUTC:s}
	Valid From: {cert.CE_ValidFromUTC:s}
	Valid To: {cert.CE_ValidToUTC:s}
	Thumbprint: {cert.CE_Thumbprint}
	Issuer: {cert.CE_Issuer}
	Serial Number: {cert.CE_SerialNumber}
	Subject Key Identifier: {cert.CE_SubjectKeyIdentifier}";
		}

		public static bool RetrieveStatusFromMdnMessage(string mdnMessage, out string errorDescription)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(mdnMessage)))
			using (var mimeMessage = MimePart.Parse(ms) as MimePart.Multipart)
			{
				var mimeReport = mimeMessage.Parts.FirstOrDefault(p => p.MimeType == "multipart/report") as MimePart.Multipart;
				if (mimeReport != null)
				{
					var mdnPart = mimeReport.Parts.FirstOrDefault(p => p.MimeType == "message/disposition-notification") as MimePart.Content;
					if (mdnPart != null)
					{

						var mdnDetails = MimeUtils.ParseHeaders(mdnPart.Contents);
						var disposition = mdnDetails["Disposition"].Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
						string status = disposition.FirstOrDefault(d => d.StartsWith("processed"));
						if (status == "processed")
						{
							errorDescription = null;
							return true;
						}
						else
						{
							errorDescription = status;
							return false;
						}
					}
				}
			}

			errorDescription = "MDN disposition not found in message.";
			return false;
		}

		public static int RetrieveStatusFromSubscription(string trackingId, ILog logger)
		{
			using (var context = GetContext())
			{
				try
				{
					var subscriptionValue = GetEHubSubscriptionValues(context, trackingId).AsNoTracking().SingleOrDefault();

					if (subscriptionValue != null && int.TryParse(subscriptionValue.SV_Reference, out var status))
					{
						return status;
					}
				}
				catch (Exception ex)
				{
					logger.Error($"Retrieving subscription status error [MessageTrackingID: {trackingId}]", ex);
				}

				return (int)SubscriptionStatus.NotExist;
			}
		}

		public static void AddOrUpdateSubscriptionStatus(string trackingId, string senderId, string recipientId, int status, ILog logger)
		{
			using (var context = GetContext())
			{
				try
				{
					var subscriptionValue = GetEHubSubscriptionValues(context, trackingId).SingleOrDefault();

					if (subscriptionValue == null)
					{
						var subscriptionType = context.eHubSubscriptionTypes.AsNoTracking().Single(_ => _.ST_ID == Constants.SubscriptionType);
						var sender = context.eHubClients.Single(_ => _.CC_ID == senderId);
						var recipient = context.eHubClients.Single(_ => _.CC_ID == recipientId);
						subscriptionValue = new eHubSubscriptionValue
						{
							SV_PK = Guid.NewGuid(),
							SV_ST = subscriptionType.ST_PK,
							SV_CC_Sender = sender.CC_PK,
							SV_CC_Recipient = recipient.CC_PK,
							SV_Reference = status.ToString(),
							SV_ReferenceType = nameof(SubscriptionStatus),
							SV_Value = trackingId,
							SV_SubscribedUTC = DateTime.UtcNow,
							SV_ExpiryUTC = subscriptionType.ST_ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(subscriptionType.ST_ExpiryDays.Value) : (DateTime?)null,
						};
						context.eHubSubscriptionValues.Add(subscriptionValue);
					}
					else
					{
						subscriptionValue.SV_Reference = status.ToString();
					}

					context.SaveChanges();
				}
				catch (Exception ex)
				{
					logger.Error($"Failed to add/update subscription status, MessageTrackingID: {trackingId}, SenderID: {senderId}, RecipientID: {recipientId}, Status: {status}.", ex);
				}
			}
		}

		private static IQueryable<eHubSubscriptionValue> GetEHubSubscriptionValues(eHubTransactionsContext context, string trackingId)
		{
			return from val in context.eHubSubscriptionValues.Where(_ => _.SV_ReferenceType == nameof(SubscriptionStatus))
				join type in context.eHubSubscriptionTypes.Where(_ => _.ST_ID == Constants.SubscriptionType)
					on val.SV_ST equals type.ST_PK
				where val.SV_Value == trackingId
				select val;
		}

		internal static Func<string, string, string, bool, ILog, eHubCertificate> GetSenderCertificate = (senderID, recipientID, as2From, retrying, logger) => CertificatesHelper.GetSenderCertificate(senderID, recipientID, as2From, retrying, logger);
		internal static Func<string, string, ILog, eHubCertificate> GetZACustomsCertificate = (recipientID, as2From, logger) => CertificatesHelper.GetZACustomsCertificate(recipientID,as2From, logger);
		internal static Func<Guid> GetNewGuid = () => Guid.NewGuid();
		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();

		internal enum SubscriptionStatus
		{
			NotExist = 0,
			Processing = 1,
			Success = 3,
		}
	}
}
