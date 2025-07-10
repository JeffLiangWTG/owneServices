using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CommunicationReminder : Reminder
	{
		public static CommunicationReminder New(ZDateTime originalReminderDateUtc, ZDateTime reminderDateUtc, OrgSalesCall communication, IEnumerable<CommunicationReminderRecipient> recipients, bool isPublic, BusinessObjectFactory factory = null)
		{
			ReminderType type = reminderDateUtc.IsEmpty ? ReminderType.Cancellation : ReminderType.Confirmed;
			string subject = isPublic ? communication.OQ_CallSummary.ToString() : GetInternalSubject(communication);

			ZStringBuilder plainTextBody = new ZStringBuilder();
			ZStringBuilder htmlTextBody = new ZStringBuilder();

			if (isPublic)
			{
				string plainTextNotes = communication.ClientVisibleNote;
				plainTextBody.AppendLine(plainTextNotes);
				htmlTextBody.AppendLine(WebUtility.HtmlEncode(plainTextNotes));
			}
			else
			{
				AddInternalBody(plainTextBody, communication, false);
				AddInternalBody(htmlTextBody, communication, true);
			}

			string body = plainTextBody.ToString();
			string htmlBody = "<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=\"Arial\">"
				+ htmlTextBody.ToString()
				+ "</FONT></BODY></HTML>";

			ZDateTime date = (type == ReminderType.Cancellation) ? originalReminderDateUtc : reminderDateUtc;
			CommunicationReminder result = new CommunicationReminder(communication.PK.ToString() + Constants.ReminderIdentifier, communication.PK, new ZString(communication.TableName), DateTimeKind.Utc, date, communication.OQ_Duration, subject, body, htmlBody, null, factory);
			result.Location = communication.Location;
			result.ReminderType = type;
			result.Recipients.AddRange(recipients);

			return result;
		}

		public static CommunicationReminder New(ZDateTime originalReminderDateUtc, ZDateTime reminderDateUtc, OrgSalesCall communication, IEnumerable<CommunicationReminderRecipient> recipients, ZString notes, BusinessObjectFactory factory = null)
		{
			ReminderType type = reminderDateUtc.IsEmpty ? ReminderType.Cancellation : ReminderType.Confirmed;
			string subject = communication.OQ_CallSummary;

			ZStringBuilder plainTextBody = new ZStringBuilder();
			ZStringBuilder htmlTextBody = new ZStringBuilder();

			plainTextBody.AppendLine(notes);
			htmlTextBody.AppendLine(WebUtility.HtmlEncode(notes));

			string body = plainTextBody.ToString();
			string htmlBody = "<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=\"Arial\">"
				+ htmlTextBody.ToString()
				+ "</FONT></BODY></HTML>";

			ZDateTime date = (type == ReminderType.Cancellation) ? originalReminderDateUtc : reminderDateUtc;
			CommunicationReminder result = new CommunicationReminder(communication.PK.ToString() + Constants.ReminderIdentifier, communication.PK, new ZString(communication.TableName), DateTimeKind.Utc, date, communication.OQ_Duration, subject, body, htmlBody, null, factory);
			result.Location = communication.Location;
			result.ReminderType = type;
			result.Recipients.AddRange(recipients);

			return result;
		}

		static string GetInternalSubject(OrgSalesCall communication)
		{
			var subject = new OrgSalesCallParser(communication.Factory).Parse(communication, OrganisationsDataRegistry.Instance.CommunicationInternalCalendarReminderSubjectTemplate.Value.EmailSubject);
			if (subject.IsEmpty)
			{
				string orgID = communication.Header != null ? communication.Header.OH_Code : communication.OrgName;
				subject = Res.GetString("D4387E89-9030-40F2-9C9C-C34F11C07AB4", "Communication [{0}] {1} - {2}", communication.OQ_TypeOfCall, orgID, communication.OQ_CallSummary);
			}
			return subject;
		}

		static void AddClientIDSection(ZStringBuilder builder, OrgSalesCall communication, bool forHtml)
		{
			var org = communication.Header;
			var orgName = communication.OrgName.ToString();
			string clientLabel = Res.GetString("18C89C58-528B-4F94-B9B8-605D6FA72A4D", "Client");
			string clientValue = org != null ? string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", org.OH_Code, orgName) : orgName;

			if (forHtml)
			{
				string orgUrl = null;
				if (org != null)
				{
					orgUrl = string.Format(CultureInfo.InvariantCulture, @"<a href=""{0}"">{1}</a>",
						ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, org.PK.ToGuid()),
						WebUtility.HtmlEncode(org.OH_Code));
				}

				string htmlClientValue = orgUrl != null ? string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", orgUrl, orgName) : WebUtility.HtmlEncode(clientValue);
				SalesReminderBuilder.AddHtmlLabelAndAlreadyEncodedValue(builder, clientLabel, htmlClientValue);
			}
			else
			{
				SalesReminderBuilder.AddPlainLabelAndValue(builder, clientLabel, clientValue);
			}
		}

		static void AddCommunicationIDSection(ZStringBuilder builder, OrgSalesCall communication, bool forHtml)
		{
			string communicationLabel = Res.GetString("91361146-5294-40CB-8B80-231FC5135773", "Communication ID");
			if (forHtml)
			{
				string communicationUrl = string.Format(CultureInfo.InvariantCulture, @"<a href=""{0}"">{1}</a>",
					ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Communication, communication.PK.ToGuid()),
					WebUtility.HtmlEncode(communication.OQ_CommunicationID));

				SalesReminderBuilder.AddHtmlLabelAndAlreadyEncodedValue(builder, communicationLabel, communicationUrl);
			}
			else
			{
				SalesReminderBuilder.AddPlainLabelAndValue(builder, communicationLabel, communication.OQ_CommunicationID);
			}
		}

		static void AddInternalBody(ZStringBuilder builder, OrgSalesCall communication, bool forHtml)
		{
			var org = communication.Header;
			Action<ZStringBuilder, string, string> addLabelAndValue;
			Func<string, string> getNotesHeader;
			if (forHtml)
			{
				addLabelAndValue = SalesReminderBuilder.AddHtmlLabelAndToBeEncodedValue;
				getNotesHeader = header => string.Format(CultureInfo.InvariantCulture, (NoResString)"<u><strong>{0}</strong></u>:", header);
			}
			else
			{
				addLabelAndValue = SalesReminderBuilder.AddPlainLabelAndValue;
				getNotesHeader = header => string.Format(CultureInfo.InvariantCulture, "{0}:", header);
			}
			AddClientIDSection(builder, communication, forHtml);
			AddCommunicationIDSection(builder, communication, forHtml);
			builder.AppendLine();
			addLabelAndValue(builder, Res.GetString("A3365591-7F3E-4491-BE78-9853EC0FFFCC", "Method"), communication.Lookups.OQ_TypeOfCall_List.GetDescriptionFromCode(communication.OQ_TypeOfCall));
			if (communication.SalesRep != null)
			{
				addLabelAndValue(builder, Res.GetString("743A4529-E405-4B99-84CC-4DFE90AF6E08", "Staff Coordinator"), communication.SalesRep.GS_FullName);
			}
			builder.AppendLine();
			if (communication.IsLinkedToInquiry)
			{
				SalesEnquiryReminderBuilder.AddOrganizationAndContactDetails(builder, communication.LinkedInquiry, forHtml);
			}
			else
			{
				SalesReminderBuilder.AddOrganizationAndContactDetails(builder, org, communication.Contact, forHtml);
			}
			if (org != null)
			{
				SalesReminderBuilder.AddOrganizationLastSalesCall(builder, org, forHtml);
				builder.AppendLine();
			}
			builder.AppendLine(getNotesHeader(Res.GetString("4B4B6E9B-3DAD-412B-8E61-4AF0D0419959", "Internal Notes")));
			builder.AppendLine(ORtfTextUtil.RtfToText(communication.OQ_SalesCallNotes));
			builder.AppendLine();
			builder.AppendLine(getNotesHeader(Res.GetString("A7C9F37B-D522-4E4F-B6F8-D030B4BC3E8D", "Follow Up Notes")));
			builder.Append(ORtfTextUtil.RtfToText(communication.OQ_FollowupNotes));
		}

		public CommunicationReminder(string identifier, ZGuid parentPK, ZString slTable, DateTimeKind dateTimeKind, ZDateTime date, ZDateTime duration, string subject, string body, string htmlBody, ITimeZone timeZoneOverride = null, BusinessObjectFactory factory = null)
			: base(identifier, parentPK, slTable, dateTimeKind, date, (date.IsValid && duration.IsValid) ? date + duration.ToTimeSpan() : date, subject, body, htmlBody, timeZoneOverride, factory)
		{
			rawDate = date;
		}

		readonly ZDateTime rawDate;

		protected override uint GenerateSequenceNumber()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var filter = new ZQuery(StmALogSchema.SL_Parent, parentPK);
			filter.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode));
			filter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Reminder.Constants.ReferencePrefix + Constants.ReminderIdentifier);

			return (uint)factory.GetDatabaseCount(typeof(StmALog), filter);
		}

		protected override void OnAppointmentCreated()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			if (Factory == null || BaseStmALog.GetMaster(parentPK, slTable, newFactory) != null)
			{
				AddLogs(newFactory);
				newFactory.Save();
			}
			else if (BaseStmALog.GetMaster(parentPK, slTable, Factory) != null)
			{
				AddLogs(Factory);
			}
		}

		void AddLogs(BusinessObjectFactory factory)
		{
			appointmentCreatedLogs = new List<StmALog>();
			foreach (var recipient in Recipients)
			{
				var log = factory.New<StmALog>();
				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_Table = slTable;
					log.SL_Parent = parentPK;
					log.SL_SE_NKEvent = Reminder.Constants.EventCode;
					log.SL_Reference = GenerateReference(Sequence, recipient);
				}
				appointmentCreatedLogs.Add(log);
			}
		}

		IList<StmALog> appointmentCreatedLogs;
		public IList<StmALog> AppointmentCreatedLogs
		{
			get { return appointmentCreatedLogs; }
		}

		string GenerateReference(uint sequence, ReminderRecipient recipient)
		{
			var communicationRecipient = recipient as CommunicationReminderRecipient;
			var recipientPk = (communicationRecipient != null) ? communicationRecipient.ParentPk.ToString() : "";
			var date = (ReminderType == ReminderType.Cancellation) ? ZDateTime.Empty.ToLongTimeString() : rawDate.ToLongTimeString();
			string typeCode = ReminderType == ReminderType.Cancellation ? Constants.TypeCancelCode : Constants.TypeRequestCode;
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}{1} Recipient:{2} Type:{3} Date:{4} Seq:{5}", Reminder.Constants.ReferencePrefix, Constants.ReminderIdentifier, recipientPk, typeCode, date, sequence);
		}

		public static bool IsReminderSentLog(StmALog reminderLog)
		{
			return reminderLog.SL_Reference.Contains((NoResString)"Type:" + CommunicationReminder.Constants.TypeRequestCode);
		}

		public static ZDateTime GetReminderDateFromLog(StmALog log)
		{
			ZDateTime date;
			var reminderDateRegex = new Regex("Date:(?<DATE>.+) Seq:");
			var match = reminderDateRegex.Match(log.SL_Reference);

			if (match.Success && ZDateTime.TryParseExact(match.Groups["DATE"].Value, out date, ZDateTime.LongTimeFormat))
			{
				return date;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		internal new class Constants
		{
			internal const string ReminderIdentifier = "FollowUpCall";
			internal const string TypeRequestCode = "REQ";
			internal const string TypeCancelCode = "CAN";
		}
	}

	class OrgSalesCallParser : DocumentParser<OrgSalesCall>
	{
		public OrgSalesCallParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		protected override Type TypeOfWrapper
		{
			get { return ObjectFactory.GetType<DocumentWrappers.IDocSalesCall>(); }
		}
	}
}
