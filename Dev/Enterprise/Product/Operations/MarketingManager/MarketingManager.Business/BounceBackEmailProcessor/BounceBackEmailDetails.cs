using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mail;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using MimeKit;

namespace Enterprise.MarketingManager.Business
{
	public class BounceBackEmailDetails
	{
		public BounceBackEmailDetails(BusinessObjectFactory factory, byte[] bodyEml, byte[] headerEml = null)
		{
			this.factory = factory;
			this.bodyEml = bodyEml;

			try
			{
				if (headerEml != null && headerEml.Length > 0)
				{
					headerMessage = MimeMessageExtensions.CreateMessageFromEml(headerEml);
					var bodyMessageString = string.Concat(Encoding.UTF8.GetString(headerEml), CRLF, CRLF, Encoding.UTF8.GetString(bodyEml));
					bodyMessage = MimeMessageExtensions.CreateMessageFromEml(bodyMessageString);
				}
				else
				{
					headerMessage = new MimeMessage();
					bodyMessage = MimeMessageExtensions.CreateMessageFromEml(bodyEml);
				}
			}
			catch (Exception ex) when (ex.Source.StartsWith(nameof(MimeKit)))
			{
				var key = $"{GetType().Name}.BounceBackEmailDetails.{ex.GetType().Name}";
				var headerEmlString = headerEml != null ? Encoding.UTF8.GetString(headerEml) : "";
				var bodyEmlString = bodyEml != null ? Encoding.UTF8.GetString(bodyEml) : "";
				ErrorReporter.ReportOnce(key, $"Exception parsing EML, headerEml: {headerEmlString}, bodyEml: {bodyEmlString}", ex);
			}
			finally
			{
				if (headerMessage == null)
				{
					headerMessage = new MimeMessage();
				}

				if (bodyMessage == null)
				{
					bodyMessage = new MimeMessage();
				}
			}
		}

		readonly BusinessObjectFactory factory;
		readonly byte[] bodyEml;
		readonly MimeMessage headerMessage;
		readonly MimeMessage bodyMessage;
		const string CRLF = "\r\n";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element accessor")]
		const string DeliveryStatusNotificationAction = "Action";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element accessor")]
		const string DeliveryStatusNotificationActionDelayed = "delayed";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element accessor")]
		const string DeliveryStatusNotificationActionFailed = "failed";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Element accessor")]
		const string DeliveryStatusNotificationStatus = "Status";
		const string DeliveryStatusNotificationDiagnosticCode = "Diagnostic-Code";
		const string DeliveryStatusNotificationOriginalRecipient = "Original-Recipient";
		const string DeliveryStatusNotificationFinalRecipient = "Final-Recipient";

		#region Properties

		#region Sender

		public string From
		{
			get
			{
				return bodyMessage.From.Count > 0 ? bodyMessage.From[0].ToString() : string.Empty;
			}
		}

		public string SenderNameAddress
		{
			get
			{
				string result = string.Empty;

				var mailbox = bodyMessage.GetSenderOrFrom();

				if (mailbox != null)
				{
					if (!string.IsNullOrEmpty(mailbox.Name))
					{
						result += '"' + mailbox.Name + '"';
					}

					if (!string.IsNullOrEmpty(mailbox.Address))
					{
						if (result.Length > 0)
						{
							result += ' ';
						}

						result += '<' + mailbox.Address + '>';
					}
				}
				return result;
			}
		}

		public string SenderAddress
		{
			get => bodyMessage.GetSenderOrFrom()?.Address ?? string.Empty;
		}

		public string SenderName
		{
			get => bodyMessage.GetSenderOrFrom()?.Name ?? string.Empty;
		}

		public ZGuid SenderStaffID
		{
			get
			{
				var idString = bodyMessage.Headers[BounceEmailParser.BounceEmailConstants.SenderStaffIDKey];
				if (string.IsNullOrEmpty(idString))
				{
					idString = BounceEmailBodyParser.SenderStaffID;
				}

				if (!ZGuid.TryParse(idString, out ZGuid result))
				{
					result = ZGuid.Empty;
				}

				return result;
			}
		}

		public GlbStaff SenderStaff => factory.Load<GlbStaff>(SenderStaffID);

		#endregion

		#region Business Entity

		public ZGuid BusinessEntityID
		{
			get
			{
				var idString = bodyMessage.Headers[BounceEmailParser.BounceEmailConstants.BusinessEntityIDKey];
				if (string.IsNullOrEmpty(idString))
				{
					idString = BounceEmailBodyParser.BusinessEntityID;
				}

				if (!ZGuid.TryParse(idString, out ZGuid result))
				{
					result = ZGuid.Empty;
				}

				return result;
			}
		}

		public string BusinessEntityTableCode
		{
			get
			{
				var result = bodyMessage.Headers[BounceEmailParser.BounceEmailConstants.BusinessEntityTableCodeKey];
				if (string.IsNullOrEmpty(result))
				{
					result = BounceEmailBodyParser.BusinessEntityTableCode;
				}

				return result;
			}
		}

		public BusinessObject BusinessEntity
		{
			get
			{
				if (businessEntity == null)
				{
					if (BusinessEntityTableCode == GlbCompanyCampaignItemSchema.Constants.Prefix || string.IsNullOrEmpty(BusinessEntityTableCode))
					{
						businessEntity = factory.Load<GlbCompanyCampaignItem>(BusinessEntityID);
					}
					else if (BusinessEntityTableCode == JobHeaderSchema.Constants.Prefix)
					{
						businessEntity = factory.Load<JobHeader>(BusinessEntityID);
					}
					else
					{
						var parentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(BusinessEntityTableCode, false);
						if (parentType != null)
						{
							businessEntity = factory.Load(parentType, BusinessEntityID);
						}
					}
				}
				return businessEntity;
			}
		}
		BusinessObject businessEntity;

		public string BusinessEntityJobNumber
		{
			get
			{
				string result = string.Empty;
				var entity = BusinessEntity;
				if (entity != null)
				{
					result = (BusinessEntityTableCode == JobHeaderSchema.Constants.Prefix)
						? entity[JobHeaderSchema.JH_JobNum].ToString()
						: (entity as IJobHeaderParent)?.JobNumber ?? string.Empty;
				}
				return result;
			}
		}

		#endregion

		#region Document Name

		public string DocumentName
		{
			get
			{
				var result = bodyMessage.Headers[BounceEmailParser.BounceEmailConstants.DocumentNameKey];
				if (string.IsNullOrEmpty(result))
				{
					result = BounceEmailBodyParser.DocumentName;
				}
				else
				{
					result = BounceEmailParser.DecodeDocumentName(result);
				}

				return result;
			}
		}

		#endregion

		#region Sent Time

		public string SentTimeText => BounceEmailBodyParser.SenderTimeText;

		#endregion

		#region Body

		public string Body
		{
			get
			{
				var bodyText = GetBodyText(bodyMessage);
				return !string.IsNullOrWhiteSpace(bodyText) ? bodyText : GetEml();
			}
		}

		string GetBodyText(MimeMessage emailBody)
		{
			var emailParts = emailBody.BodyParts;
			var multipartTexts = new List<string>();

			if (emailParts != null)
			{
				foreach (var part in emailParts.Where(x => x != null))
				{
					if (part is MessagePart messagePart)
					{
						multipartTexts.Add(messagePart.ToString());
					}
					else if (part is TextRfc822Headers textRfc822Headers)
					{
						multipartTexts.Add(textRfc822Headers.ToString());
					}
					else if (part is TextPart textPart && textPart.IsPlain)
					{
						multipartTexts.Add(textPart.Text);
					}
					else if (part is MessageDeliveryStatus messageDeliveryStatus)
					{
						multipartTexts.Add(messageDeliveryStatus.ToString());
					}
				}
			}
			var texts = new string[] { emailBody.TextBody }.Union(multipartTexts);

			return string.Join(CRLF, texts.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		#endregion

		#region Bounce Reason

		public string BounceReason
		{
			get
			{
				if (BounceResult.Count > 0)
				{
					return BounceResult.First()?.Reason ?? string.Empty;
				}
				return BounceEmailBodyParser.Reason;
			}
		}

		public string BounceReasonCode => BounceEmailParser.GetStatusCodeFromMatchedPhrase(BounceReason);

		List<MailBounceResult> BounceResult
		{
			get
			{
				if (bounceResult == null)
				{
					bounceResult = ExamineAllBounceResults(bodyMessage);
				}
				return bounceResult;
			}
		}
		List<MailBounceResult> bounceResult;

		#endregion

		#region Bounce Recipients

		public MailAddressCollection BouncedRecipients
		{
			get
			{
				if (bouncedRecipients == null)
				{
					var failedRecipientsHeader = headerMessage.Headers["X-Failed-Recipients"];
					if (!string.IsNullOrEmpty(failedRecipientsHeader))
					{
						try
						{
							var failedRecipientsCollection = new MailAddressCollection { failedRecipientsHeader };
							bouncedRecipients = failedRecipientsCollection;
						}
						catch (FormatException)
						{
						}
					}

					if (bouncedRecipients == null)
					{
						bouncedRecipients = GetBouncedRecipients();
					}

					if (!bouncedRecipients.Any())
					{
						var recipients = GetRecipients();
						if (recipients.Any())
						{
							var failedRecipientsCollection = new MailAddressCollection();
							foreach (var recipient in recipients)
							{
								try
								{
									failedRecipientsCollection.Add(recipient);
								}
								catch (FormatException)
								{
									InvalidRecipients.Add(recipient);
								}
							}

							bouncedRecipients = failedRecipientsCollection;
						}
					}
				}

				return bouncedRecipients;
			}
		}

		MailAddressCollection bouncedRecipients;

		public List<string> InvalidRecipients => invalidRecipients ?? (invalidRecipients = new List<string>());
		List<string> invalidRecipients;

		public virtual MailAddressCollection GetBouncedRecipients()
		{
			return BounceEmailBodyParser.BouncedRecipients;
		}

		public virtual string[] GetRecipients()
		{
			return BounceResult.Select(x => x.Recipient ?? "").Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
		}

		#endregion

		#region Diagnositic Info

		public bool ShouldIncludeDiagnositicInfo =>
			Body.Contains(BounceEmailParser.BounceEmailConstants.BusinessEntityIDKey)
			|| Body.Contains(BounceEmailParser.BounceEmailConstants.ToKey)
			|| Body.Contains(BounceEmailParser.BounceEmailConstants.CcKey)
			|| Body.Contains(BounceEmailParser.BounceEmailConstants.BccKey);

		public ZString DiagnositicInfo => BounceEmailBodyParser.GetDiagnositicInfo();

		#endregion

		#endregion

		#region Get Eml content

		public string GetEml()
		{
			return Encoding.UTF8.GetString(bodyEml) ?? String.Empty;
		}

		public string GetShortEml()
		{
			if (shortBodyEml == null)
			{
				const int ShortEmlMaxLength = 4000;

				shortBodyEml = GetEml();
				if (shortBodyEml.Length > ShortEmlMaxLength)
				{
					shortBodyEml = shortBodyEml.Substring(0, ShortEmlMaxLength);
				}
			}
			return shortBodyEml;
		}
		string shortBodyEml;

		List<MailBounceResult> ExamineAllBounceResults(MimeMessage message)
		{
			var bounceResult = new List<MailBounceResult>();
			var deliveryStatusList = message.BodyParts.OfType<MessageDeliveryStatus>();
			foreach (var deliveryStatus in deliveryStatusList)
			{
				foreach (var statusGroup in deliveryStatus.StatusGroups)
				{
					var action = statusGroup[DeliveryStatusNotificationAction];
					if (action == DeliveryStatusNotificationActionDelayed || action == DeliveryStatusNotificationActionFailed)
					{
						var recipient = GetRecipientFromHeader(statusGroup[DeliveryStatusNotificationOriginalRecipient] ?? statusGroup[DeliveryStatusNotificationFinalRecipient]);

						bounceResult.Add(new MailBounceResult
						{
							Recipient = recipient,
							Action = action,
							Status = statusGroup[DeliveryStatusNotificationStatus],
							Reason = statusGroup[DeliveryStatusNotificationDiagnosticCode],
						});
					}
				}
			}
			return bounceResult;
		}

		string GetRecipientFromHeader(string recipientHeader)
		{
			if (string.IsNullOrEmpty(recipientHeader))
			{
				return string.Empty;
			}

			var recipientHeaderList = recipientHeader.Split(new[] { ';' }, 2);
			if (recipientHeaderList.Length == 2)
			{
				return recipientHeaderList[1].Trim();
			}
			else
			{
				return recipientHeaderList[0].Trim();
			}
		}

		#endregion

		BounceEmailParser BounceEmailBodyParser
		{
			get
			{
				if (bounceEmailBodyParser == null)
				{
					bounceEmailBodyParser = new BounceEmailParser(Body);
				}
				return bounceEmailBodyParser;
			}
		}
		BounceEmailParser bounceEmailBodyParser;

		class MailBounceResult
		{
			public string Recipient { get; set; }
			public string Reason { get; set; }
			public string Action { get; set; }
			public string Status { get; set; }
		}
	}
}
