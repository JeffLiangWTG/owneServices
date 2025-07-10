using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.MarketingManager.Business.BounceBackEmailProcessor))]

namespace Enterprise.MarketingManager.Business
{
	public class BounceBackEmailProcessor
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^(?!.*(failed transmission|{EDIFAX} [a-fA-F0-9]{8}[-]([a-fA-F0-9]{4}[-]){3}[a-fA-F0-9]{12} failure)).*(Non delivery report|Undeliverable|fail|delivery status|returned|Benachrichtung zum Übermittlungsstatus|Fehlgeschlagen|Ekki hægt að afhenda|E-Mail entitled|Impossibile recapitare|Kan ikke leveres|Não Entregues|Nie można dostarczyć|No se puede entregar|Non remis|Olevererbart|Onbestelbaar|Unzustellbar|Zprávu nelze doručit, doručit|Не удается доставить, доставить|배달되지 않음|未送达)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
		public bool ProcessMailItem(MailItem item, ILogger logger)
		{
			bool isSuccess = false;

			using (Env.Instance.SuspendBranchAccessError())
			{
				BounceBackEmailDetails bounceDetails = null;
				var updatedStaffMailAddresses = new List<ZString>();
				if (!string.IsNullOrEmpty(item.MI_Header) || !string.IsNullOrEmpty(item.MI_Body))
				{
					var factory = new BusinessObjectFactory();
					bounceDetails = new BounceBackEmailDetails(factory, Encoding.UTF8.GetBytes(item.MI_Body), Encoding.UTF8.GetBytes(item.MI_Header));
					try
					{
						isSuccess = ProcessEmailCore(bounceDetails, factory, updatedStaffMailAddresses);
						if (!isSuccess)
						{
							foreach (var attachment in item.MailAttachments)
							{
								using (var stream = ((MailAttachment)attachment).GetMA_DataReader())
								using (var targetStream = new MemoryStream())
								{
									stream.CopyTo(targetStream);
									byte[] attachmentBytes = targetStream.ToArray();

									bounceDetails = new BounceBackEmailDetails(factory, attachmentBytes);
									isSuccess = ProcessEmailCore(bounceDetails, factory, updatedStaffMailAddresses);
									if (isSuccess)
									{
										break;
									}
								}
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						string key = this.GetType().Name + ".ProcessEmail." + ex.GetType().Name;
						ErrorReporter.ReportOnce(key, ex.Message, ex);
						throw;
					}
				}

				var processResult = CreateProcessResult(isSuccess, item, bounceDetails);
				WriteToLog(processResult, logger);
				if (isSuccess)
				{
					SendNotificationEmail(item, processResult, updatedStaffMailAddresses, logger);
				}
			}

			return isSuccess;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		internal bool ProcessEmailCore(BounceBackEmailDetails bounceDetails, BusinessObjectFactory factory, IList<ZString> updatedStaffMailAddresses)
		{
			var isSuccess = false;

			if (bounceDetails.BusinessEntityID.IsValid)
			{
				var businessEntity = bounceDetails.BusinessEntity;
				if (businessEntity is GlbCompanyCampaignItem campaignItem)
				{
					campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
					AddEmailBodyAsNote(campaignItem, bounceDetails.GetShortEml(), bounceDetails.BounceReasonCode);
					isSuccess = true;
				}
				else if (businessEntity != null)
				{
					foreach (var recipient in bounceDetails.BouncedRecipients)
					{
						var reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(
							ZString.Empty,
							new[]
							{
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.NonDeliveryReceipt),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Name, recipient.Address),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, bounceDetails.DocumentName)
							});

						businessEntity.GetLogs().AddNew(AutoEvents.DocumentNotDelivered, reference);
						isSuccess = true;
					}

					if (bounceDetails.InvalidRecipients.Any())
					{
						var reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(ZString.Empty,
							new[]
							{
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.Failed),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, bounceDetails.DocumentName)
							});

						businessEntity.GetLogs().AddNew(AutoEvents.DocumentNotDelivered, reference);
						isSuccess = true;
					}
				}
			}

			foreach (var recipient in bounceDetails.BouncedRecipients)
			{
				var emailAddress = GlbEmailAddress.LoadOrNew(factory, recipient.Address);
				if (!IsStaffMail(factory, recipient.Address))
				{
					emailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;
				}
				else if (emailAddress.GI_DeliveryStatus != EmailDeliveryReportStatus.Codes.NonDeliveryReport)
				{
					emailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;
					updatedStaffMailAddresses.Add(recipient.Address);
				}
				emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				AddEmailBodyAsNote(emailAddress, bounceDetails.GetShortEml(), bounceDetails.BounceReasonCode);
				isSuccess = true;
			}

			if (isSuccess)
			{
				factory.Save();
			}

			return isSuccess;
		}

		BounceBackEmailProcessResult CreateProcessResult(bool isSuccess, MailItem item, BounceBackEmailDetails details)
		{
			if (item == null || details == null)
			{
				return new BounceBackEmailProcessResult();
			}

			var result = new BounceBackEmailProcessResult()
			{
				IsSuccess = isSuccess,
				MailItemPK = item.PK,
				MailItemReceivedTimeUtc = item.MI_ReceivedDateTime,
				MailItemSubject = item.MI_Subject,
				FromAddress = item.MI_From,
				SentTimeText = details.SentTimeText,
				SenderStaffID = details.SenderStaffID,
				SenderStaff = details.SenderStaff,
				BusinessEntity = details.BusinessEntity,
				BusinessEntityID = details.BusinessEntityID,
				BusinessEntityTableCode = details.BusinessEntityTableCode,
				BusinessEntityInDatabase = (details.BusinessEntity != null),
				DocumentName = details.DocumentName,
				JobNumber = details.BusinessEntityJobNumber,
				BounceReasonCode = details.BounceReasonCode,
				BouncedRecipients = details.BouncedRecipients.ToString()
			};

			if (!isSuccess && details.ShouldIncludeDiagnositicInfo)
			{
				result.DiagnositicInfo = details.DiagnositicInfo;
			}

			return result;
		}

		#region Bounce Back Email Note

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed as it is not needed to be visible.")]
		public const string EmailNoteDescription = "Email bounce back";

		public void AddEmailBodyAsNote(BusinessObject item, string emailBody, string bounceReasonCode)
		{
			var stmNote = GetLastBounceBackEmailNote(item)
				?? item.GetNotes().AddNew(true, EmailNoteDescription, ZString.Empty);
			stmNote.ST_NoteDataAsText = ZString.Join("\r\n", new ZString[] { "<" + bounceReasonCode + "> " + new BouncebackErrorCodes().GetDescriptionFromCode(bounceReasonCode), emailBody.Trim(new char[] { '\r', '\n' }) });
		}

		public static StmNote GetLastBounceBackEmailNote(BusinessObject bizObj)
		{
			if (!bizObj.IsInDatabase)
			{
				return bizObj.GetNotes().FindByDescription(EmailNoteDescription).FirstOrDefault();
			}

			var query = new ZDBOnlyQuery(typeof(StmNote));

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ParentId", bizObj.PK, StmNoteSchema.ST_ParentID);
			sqlParams.Add("@Description", EmailNoteDescription, StmNoteSchema.ST_Description);

			string sql = @"ST_PK =
(
	SELECT TOP 1 ST_PK FROM dbo.StmNote
	WHERE ST_Description = @Description
		AND ST_Table = '" + bizObj.TableName + @"'
		AND ST_parentId = @ParentId
	ORDER BY ST_SystemCreateTimeUtc DESC 
)";

			query.AddFilterAndZSQLParameterCollection(sql, sqlParams);
			return bizObj.Factory.LoadTop1<StmNote>(query);
		}

		#endregion

		#region Detailed log

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteToLog(BounceBackEmailProcessResult result, ILogger logger)
		{
			#region SuppressResourceStringsCheckRegion

			var logText =
$@"Bounce email response received from <{result.FromAddress}> is processed {(result.IsSuccess ? "successfully" : "unsuccessfully")}.
Subject: {result.MailItemSubject}
MailItemPK: {result.MailItemPK.ToString()}
SentTime: {result.SentTimeText}
SenderStaffID: {result.SenderStaffID.ToString()}
BouncedRecipients: {result.BouncedRecipients}
BounceReasonCode: {result.BounceReasonCode}
BusinessEntityID: {result.BusinessEntityID}
BusinessEntityTableCode: {result.BusinessEntityTableCode}
BusinessEntityInDatabase: {result.BusinessEntityInDatabase}
JobNumber: {result.JobNumber}
DocumentName: {result.DocumentName}
";

			#endregion

			var builder = new ZStringBuilder(logText);
			if (!result.DiagnositicInfo.IsEmpty)
			{
				builder.AppendLine();
				builder.AppendLine(result.DiagnositicInfo);
			}

			logger.Log(LogType.Information, builder.ToString());
		}

		#endregion

		#region Notification Email

		void SendNotificationEmail(MailItem mailItem, BounceBackEmailProcessResult result, IList<ZString> updatedStaffMailAddresses, ILogger logger)
		{
			var factory = new BusinessObjectFactory();
			var emailTemplate = SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.Value;
			var parser = new BounceBackEmailProcessResultParser(factory);
			var email = new EmailDef()
			{
				ContentType = EmailContentTypes.PlainText,
				FromAddress = GetDoNotReplyEmailAddress(),
				Subject = parser.Parse(result, emailTemplate.EmailSubject),
				Body = parser.Parse(result, emailTemplate.EmailBody)
			};

			var recipients = GetNotificationRecipient(factory, result);
			recipients = recipients.Where(r => !updatedStaffMailAddresses.Any(a => a.EqualsIgnoringCase(r))).ToList();
			var sendNDRGroup = !recipients.Any();
			UpdateNDRStatus(factory, recipients);
			recipients = FilterOutNDRRecipients(factory, recipients);

			if (recipients.Any())
			{
				using (var memSteam = new MemoryStream())
				{
					mailItem.SaveEntireEmailAsEml(new UnclosableStream(memSteam));
					var suffix = ".eml";
					var bouncedEmailAsAttachment = new AttachmentDef(mailItem.MI_Subject.Left(AutoMailDBAttachments.Schema.MA_FileNameMaxLength - suffix.Length) + suffix, AttachmentDef.StreamToByteArray(memSteam));
					email.Attachments.Add(bouncedEmailAsAttachment);
				}

				email.AddRecipientForUserCommunication(recipients.Select(x => (string)x).ToArray());
				Env.OutgoingMailManager.CreateAndSave(email, factory);
			}
			else if (sendNDRGroup)
			{
				try
				{
					Env.OutgoingMailManager.Create(new BusinessObjectFactory(), email,
						SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.Value,
						GroupSourceLocator.GetFromRegistryItem(SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup));

					recipients = email.Recipients.Cast<RecipientDef>().Select(x => (ZString)x.Email).ToList();
					recipients = FilterOutNDRRecipients(factory, recipients);

					if (recipients.Any())
					{
						email.Recipients.Clear();
						email.AddRecipientForUserCommunication(recipients.Select(x => (string)x).ToArray());
						Env.OutgoingMailManager.CreateAndSave(email, factory);
					}
				}
				catch (EmailHasNoRecipientsException)
				{
					logger.Log(LogType.Warning,
						string.Format(CultureInfo.InvariantCulture,
						"The Non-Delivery Receipt Notification Group is either not specified or is empty. This group is specified in the registry at {0}.",
						((IRegistryItemInternals)SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup).Location));
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void UpdateNDRStatus(BusinessObjectFactory factory, IList<ZString> recipients)
		{
			var result = recipients.Select(r => GlbEmailAddress.Load(factory, r))
						.Where(m => m != null)
						.Where(m => m.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
						.Where(m => m.GI_DeliveryReportTimeUtc.IsEmpty || m.GI_DeliveryReportTimeUtc.AddHours(24) < DateTime.UtcNow)
						.ToList();

			if (result.Any())
			{
				foreach (var mailAddress in result)
				{
					mailAddress.GI_DeliveryStatus = ZString.Empty;
				}

				factory.Save();
			}
		}

		static IList<ZString> FilterOutNDRRecipients(BusinessObjectFactory factory, IList<ZString> recipients)
		{
			var result = recipients;

			if (recipients.Any())
			{
				var query = new ZQuery(GlbEmailAddressSchema.GI_DeliveryStatus, EmailDeliveryReportStatus.Codes.NonDeliveryReport);
				var ndrAddresses = GlbEmailAddress.Load(factory, recipients, query).Select(x => x.GI_EmailAddress);
				result = recipients.Where(r => !ndrAddresses.Any(a => a.EqualsIgnoringCase(r))).ToArray();
			}

			return result;
		}

		IList<ZString> GetNotificationRecipient(BusinessObjectFactory factory, BounceBackEmailProcessResult result)
		{
			if (StaffCanReceiveNotification(result.SenderStaff))
			{
				return new[] { result.SenderStaff.GS_EmailAddress };
			}

			var originalMailItemSender = FindOriginalMailItemSenderStaff(factory, result.BouncedRecipients, result.MailItemSubject);
			if (StaffCanReceiveNotification(originalMailItemSender))
			{
				return new[] { originalMailItemSender.GS_EmailAddress };
			}

			if (result.BusinessEntity != null)
			{
				var businessEntityOwnerStaff = FindBusinessEntityOwnerStaff(result.BusinessEntity, result.BusinessEntityTableCode);
				if (StaffCanReceiveNotification(businessEntityOwnerStaff))
				{
					return new[] { businessEntityOwnerStaff.GS_EmailAddress };
				}

				if (result.BusinessEntity is IJobHeaderParent)
				{
					var query = new ZQuery(JobHeaderSchema.JH_ParentID, result.BusinessEntity.PK);
					query.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
					var jobHeaders = factory.Load<JobHeader>(query);
					var operatorStaffEmailAddresses = jobHeaders.Where(x => StaffCanReceiveNotification(x.RepOps)).Select(x => x.RepOps.GS_EmailAddress);
					if (operatorStaffEmailAddresses.Any())
					{
						return operatorStaffEmailAddresses.ToList();
					}
				}

				var logs = result.BusinessEntity.GetLogs();
				var createUser = logs.AddedLog?.User;
				if (StaffCanReceiveNotification(createUser))
				{
					return new[] { createUser.GS_EmailAddress };
				}
				else
				{
					var lastEditNonSystemUser = logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).FirstOrDefault(x => StaffCanReceiveNotification(x.User))?.User;
					if (StaffCanReceiveNotification(lastEditNonSystemUser))
					{
						return new[] { lastEditNonSystemUser.GS_EmailAddress };
					}
				}
			}

			return new List<ZString>();
		}

		GlbStaff FindOriginalMailItemSenderStaff(BusinessObjectFactory factory, ZString bouncedRecipients, ZString bouncedEmailSubject)
		{
			GlbStaff senderStaff = null;

			if (!bouncedRecipients.IsEmpty)
			{
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@bouncedEmailSubject", bouncedEmailSubject, MailDBItemsSchema.MI_Subject);

				var originalMailItemQuery = new ZDBOnlyQuery(typeof(MailItem));
				originalMailItemQuery.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
				originalMailItemQuery.AddToFilter(MailDBItemsSchema.MI_SendDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddDays(-1));
				originalMailItemQuery.AddFilterAndZSQLParameterCollection($"CHARINDEX({MailDBItemsSchema.Constants.MI_Subject}, @bouncedEmailSubject, 0) > 0", sqlParams);

				var recipientSubQuery = new ZDBOnlySubQuery(typeof(MailRecipient), MailDBRecipientsSchema.MR_MI);
				recipientSubQuery.AddToFilter(MailDBRecipientsSchema.MR_RecipientType, MailRecipient.RecipientTypes.TO);
				recipientSubQuery.AddToFilter(MailDBRecipientsSchema.MR_RecipientMailAddress, bouncedRecipients);
				originalMailItemQuery.AddSubQuery(recipientSubQuery, JoinCondition.And);
				originalMailItemQuery.OrderBy = MailDBItemsSchema.Constants.MI_SendDateTime + OrderByClause.Descending;

				var originalMailItem = factory.LoadTop1<MailItem>(originalMailItemQuery);
				if (originalMailItem != null)
				{
					ZString senderEmail = ZString.Empty;
					if (!originalMailItem.MI_ReplyTo.IsEmpty)
					{
						senderEmail = originalMailItem.MI_ReplyTo;
					}
					else if (!originalMailItem.MI_From.IsEmpty)
					{
						try
						{
							var mailAddress = new MailAddress(originalMailItem.MI_From.Replace("\"", ""));
							senderEmail = mailAddress.Address;
						}
						catch (FormatException)
						{ }
					}

					if (!senderEmail.IsEmpty)
					{
						var senderStaffQuery = new ZQuery(GlbStaffSchema.GS_EmailAddress, senderEmail);
						senderStaffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
						senderStaffQuery.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
						senderStaff = factory.LoadTop1<GlbStaff>(senderStaffQuery);
					}
				}
			}

			return senderStaff;
		}

		GlbStaff FindBusinessEntityOwnerStaff(BusinessObject businessEntity, ZString businessEntityTableCode)
		{
			GlbStaff result = null;
			if (businessEntityTableCode == JobHeaderSchema.Constants.Prefix)
			{
				var jobHeader = businessEntity as JobHeader;
				result = jobHeader?.RepOps;
			}
			else if (businessEntityTableCode == GlbCompanyCampaignItemSchema.Constants.Prefix)
			{
				var campaignItem = businessEntity as GlbCompanyCampaignItem;
				result = campaignItem?.CompanyCampaign?.CampaignCoordinator;
			}
			
			return result;
		}

		bool IsStaffMail(BusinessObjectFactory factory, ZString mailAddress)
		{
			var senderStaffQuery = new ZQuery(GlbStaffSchema.GS_EmailAddress, mailAddress);
			senderStaffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			senderStaffQuery.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			return factory.Exists(typeof(GlbStaff), senderStaffQuery);
		}

		static bool StaffCanReceiveNotification(IGlbStaff staff)
		{
			return staff != null && staff.GS_IsActive && !staff.GS_IsSystemAccount && !staff.GS_EmailAddress.IsEmpty;
		}

		string GetDoNotReplyEmailAddress()
		{
			var doNotReplyEmailAddress = Env.Registry.SMTPDefaultDoNotReplyEmailAddress;
			var doNotReplyEmailAddressDefaultValue = RawDataRegistry.DefaultDoNotReplyEmailAddress;

			if (doNotReplyEmailAddress.Equals(doNotReplyEmailAddressDefaultValue))
			{
				var mailboxEmailAddress = Env.Registry.MailboxEmailAddress;
				if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(doNotReplyEmailAddress) &&
					EmailAddressValidation.IsEmailAddressValidAndNotEmpty(mailboxEmailAddress))
				{
					try
					{
						var prefix = doNotReplyEmailAddress.Split('@')[0];
						var domain = mailboxEmailAddress.Substring(mailboxEmailAddress.IndexOf('@') + 1);
						doNotReplyEmailAddress = string.Format(CultureInfo.InvariantCulture, "{0}@{1}", prefix, domain);
					}
					catch (IndexOutOfRangeException)
					{
					}
				}
			}
			else if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(doNotReplyEmailAddress))
			{
				doNotReplyEmailAddress = doNotReplyEmailAddressDefaultValue;
			}

			return doNotReplyEmailAddress;
		}

		class BounceBackEmailProcessResultParser : DocumentParser<BounceBackEmailProcessResult>
		{
			public BounceBackEmailProcessResultParser(BusinessObjectFactory factory)
				: base(factory)
			{
			}
			protected override Type TypeOfWrapper
			{
				get { return ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(); }
			}
		}

		#endregion
	}
}
