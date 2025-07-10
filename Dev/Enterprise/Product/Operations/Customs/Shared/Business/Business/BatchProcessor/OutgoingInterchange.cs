using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.CryptoUtilities;
using Enterprise.CryptoUtilities.SMIME;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MailManager;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public class OutgoingInterchange
	{
		public OutgoingInterchange(EDIInterchange interchange)
		{
			this.fInterchangeContents = interchange.EI_InterchangeText;
			this.Interchange = interchange;
		}

		public readonly EDIInterchange Interchange;

		public bool SendClearSigned;
		public Store SigningCertificateStore;
		public Certificate EncryptionCertificate;
		public ZString InterchangeFilename = "ESEND-99.EDI";

		public string InterchangeRecipient
		{
			get { return Interchange.EI_To; }
		}

		bool fSendMessageInBodyOfEmail;
		public bool SendMessageInBodyOfEmail
		{
			get { return fSendMessageInBodyOfEmail; }
			set { fSendMessageInBodyOfEmail = value; }
		}

		protected string primaryRecipient;
		protected ZString secondaryRecipient;

		public void PrepareSignedPayloadForEHub(LoggingInformation logger)
		{
			byte[] contentToSendAsByteArray = System.Text.Encoding.ASCII.GetBytes(ContentToSend);
			string contentType = "application/edifact; name=" + EDIDocumentNameWithExtension;
			string contentDisposition = "attachment; filename=" + EDIDocumentNameWithExtension;
			MIMEMessage outerMessage;
			var innerMessage = new SingleAttachmentMIMEMessage(EDIDocumentNameWithExtension, contentToSendAsByteArray, contentType, contentDisposition);
			if (SigningCertificateStore != null)
			{
				outerMessage = new SignedSMIMEMessage(innerMessage, SigningCertificateStore);
			}
			else
			{
				outerMessage = innerMessage;
			}
			Interchange.EI_BodyText = outerMessage.RawMIMEString;
			Interchange.EI_HeaderText = ZString.Empty;
			Interchange.EI_FooterText = ZString.Empty;
			Interchange.EI_Status = EDIInterchange.Status.Queued;
		}

		public bool Send(LoggingInformation logger, string toAddress)
		{
			return Send(logger, toAddress, ZString.Empty);
		}

		public bool Send(LoggingInformation logger, string toAddress, ZString secondCopyRecipient)
		{
			primaryRecipient = toAddress;
			secondaryRecipient = secondCopyRecipient;

			try
			{
				SendToRecipients();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (logger != null)
				{
					logger.LogWarning("Failed to send: " + e.Message + "\r\n" + e.StackTrace);
				}

				return false;
			}

#if DEBUG
			if (EmailFromOverride != null)
			{
				ReverseDirectionAndSetSenderOfOutboundEmails();
			}
#endif

			return true;
		}

		void SendToRecipients()
		{
			if (SendMessageInBodyOfEmail)
			{
				Env.OutgoingCustomsMailManager.CreateAndSaveSimple(Subject, ContentToSend, primaryRecipient);
				//TODO: Refactor to send only one email
				if (!secondaryRecipient.IsEmpty)
				{
					Env.OutgoingCustomsMailManager.CreateAndSaveSimple(Subject, ContentToSend, secondaryRecipient);
				}
			}
			else
			{
				byte[] contentToSendAsByteArray = System.Text.Encoding.ASCII.GetBytes(ContentToSend);
				if (SendClearSigned)
				{
					ClearSignedSMIMEMessage clearSignedMessage = new ClearSignedSMIMEMessage(ContentToSend, SigningCertificateStore);
					MIMEMessage outerMessage = new EnvelopedSMIMEMessage(clearSignedMessage, EncryptionCertificate);
					((IOutgoingMIMEManager)Env.OutgoingCustomsMailManager).CreateAndSaveMIME(outerMessage, Env.Registry.MailboxEmailAddress, primaryRecipient, Subject);
				}
				else
				{
					string contentType = "application/edifact; name=" + EDIDocumentNameWithExtension;
					string contentDisposition = "attachment; filename=" + EDIDocumentNameWithExtension;
					((IOutgoingMIMEManager)Env.OutgoingCustomsMailManager).CreateAndSaveSignedMIME(
						EDIDocumentNameWithExtension,
						contentToSendAsByteArray,
						contentType,
						contentDisposition,
						Env.Registry.MailboxEmailAddress,
						primaryRecipient,
						Subject,
						SigningCertificateStore,
						EncryptionCertificate);
				}

				if (!secondaryRecipient.IsEmpty)
				{
					string filename = new Random().Next(99999999).ToString();
					filename = new string('0', 8 - filename.Length) + filename + ".txt";
					((IOutgoingMIMEManager)Env.OutgoingCustomsMailManager).CreateAndSaveSignedMIME(
						filename,
						contentToSendAsByteArray,
						"application/edifact; name=" + filename,
						"attachment; filename=" + filename,
						Env.Registry.MailboxEmailAddress,
						secondaryRecipient,
						Subject,
						null,
						null);
				}
			}
		}

#if DEBUG
		protected void ReverseDirectionAndSetSenderOfOutboundEmails()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MailItem[] items = (MailItem[])factory.Load(typeof(MailItem), new ZQuery(MailDBItemsSchema.MI_Direction, "TRX"));
			foreach (MailItem item in items)
			{
				item.MI_Direction = DirectionList.Codes.Receive;
				item.MI_From = EmailFromOverride;
			}

			factory.Save();
		}

		[ThreadStatic]
		public static string EmailFromOverride;
#endif

		protected string Subject
		{
			get
			{
				string result;

				if (SigningCertificateStore == null && EncryptionCertificate == null)
				{
					result = Interchange.EI_ApplicationCode + "=" + Interchange.EI_To + "," + Interchange.EI_InterchangeNum + "," + Interchange.EI_From;
				}
				else
				{
					result = InterchangeFilename + "_" + InterchangeRecipient;
				}
				return result;
			}
		}

		protected string EDIDocumentNameWithExtension
		{
			get
			{
				if (InterchangeFilename.IndexOf(".") != -1)
				{
					return InterchangeFilename;
				}
				else
				{
					return InterchangeFilename + ".edi";
				}
			}
		}
		readonly string fInterchangeContents;

		protected virtual string ContentToSend
		{
			get { return fInterchangeContents; }
		}
	}
}
