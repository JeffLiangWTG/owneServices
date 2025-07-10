using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class MessageProcessorReportMailer
	{
		public MessageProcessorReportMailer(BusinessObjectFactory factory, LoggingInformation logger)
		{
			this.factory = factory;
			this.ediLogger = logger;
		}

		public MessageProcessorReportMailer(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.factory = factory;
			this.xmlLogger = logger;
		}

		readonly BusinessObjectFactory factory;
		readonly LoggingInformation ediLogger;
		readonly IXmlImportLogger xmlLogger;

		public void SendReport(EmailDef email, BusinessObject parent, ZString emailMode, ZGuid emailGroup)
		{
			if (emailMode == Core.Constants.EmailTo.StaffMember || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				if (parent != null)
				{
					var userToNotify = GetUserToNotify(parent);
					if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
					{
						AddRecipientCore(email, userToNotify.GS_EmailAddress, RecipientDef.RecipientTypes.TO);
					}
				}
			}

			if (emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup ||
							(ReportWhenNoParent && parent == null))
			{
				CopyGroupToEmails(parent, email, emailGroup);
			}

			SendReport(email);
		}

		protected virtual void SendReport(EmailDef email)
		{
			if (email.Recipients.Count > 0 || email.CCRecipients.Count > 0 || email.BCCRecipients.Count > 0)
			{
				try
				{
					if (factory != null)
					{
						Env.OutgoingMailManager.Create(factory, email);
					}
					else // Just in case there's some error handling trying to report something when the factory has not been set.
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (EmailSendFailedException e)
				{
					var logMessage = (NoResString)"Couldn't send email: " + e.Message + (NoResString)".  Here are the contents of the email that couldn't be sent:\r\n\r\n" +
						(NoResString)"SUBJECT: " + email.Subject + (NoResString)"\r\n" +
						(NoResString)"BODY: " + email.Body + (NoResString)"\r\n";

					CreateLogEntry(LogType.Error, logMessage);
				}
			}
		}

		protected void CopyGroupToEmails(BusinessObject parent, EmailDef email, ZGuid groupToCopy)
		{
			if (!groupToCopy.IsEmpty)
			{
				GlbGroup group;
				if (parent != null)
				{
					group = parent.Factory.Load<GlbGroup>(groupToCopy);
				}
				else
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					group = factory.Load<GlbGroup>(groupToCopy);
				}

				if (group != null)
				{
					var emailGroupUtility = new EmailGroupUtility();

					foreach (GlbStaff staff in group.Staff)
					{
						if (!staff.GS_EmailAddress.IsEmpty && !emailGroupUtility.IsHostNotificationEmail(staff.GS_EmailAddress))
						{
							AddRecipientCore(email, staff.GS_EmailAddress, RecipientDef.RecipientTypes.CC);
						}
					}
				}
			}
		}

		void CreateLogEntry(LogType logType, string logMessage)
		{
			ediLogger?.Log(logMessage);
			xmlLogger?.LogBoth(logType, logMessage);
		}

		protected virtual bool ReportWhenNoParent
		{
			get { return false; }
		}

		protected virtual void AddRecipientCore(EmailDef emailDef, string email, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForUserCommunication(email, type);
		}

		protected virtual GlbStaff GetUserToNotify(BusinessObject parent)
		{
			return GetLastNonBatchProcessorStaffToSendMessage(parent);
		}

		protected virtual ZString ApplicationCodeForGetUserToNotify
		{
			get { return ZString.Empty; }
		}

		protected GlbStaff GetLastNonBatchProcessorStaffToSendMessage(BusinessObject parent)
		{
			var rawSQLQuery =
				"SELECT TOP 1 " + EDIMessage.Schema.EM_SystemCreateUser +
				" FROM " + EDIMessageSchema.Constants.SqlSchemaName + "." + EDIMessageSchema.Constants.TableName +
				" WHERE " + EDIMessage.Schema.EM_SystemCreateUser + " != @BatchProcessorInitials" +
				" AND " + EDIMessage.Schema.EM_ReceiveTransmit + " = @TransmitCode" +
				" AND " + EDIMessage.Schema.EM_LinkUniqueID + " = @ParentPK" +
				(ApplicationCodeForGetUserToNotify.IsEmpty ? "" : (NoResString)" AND " + EDIMessage.Schema.EM_ApplicationCode + (NoResString)" = @AppCode") +
				" ORDER BY " + EDIMessage.Schema.EM_SystemCreateTimeUtc + " DESC";

			var @params = new ZSqlParameterCollection();
			@params.Add("@BatchProcessorInitials", User.ServiceUserCode, EDIMessageSchema.EM_SystemCreateUser);
			@params.Add("@TransmitCode", "TRX", EDIMessageSchema.EM_ReceiveTransmit);
			@params.Add("@ParentPK", parent.PK, EDIMessageSchema.EM_LinkUniqueID);
			if (!ApplicationCodeForGetUserToNotify.IsEmpty)
			{
				@params.Add("@AppCode", ApplicationCodeForGetUserToNotify, EDIMessageSchema.EM_ApplicationCode);
			}

			var dynamicCollection = new DynamicBusinessObjectCollection(parent.Factory);
			dynamicCollection.Load(rawSQLQuery, @params);

			if (dynamicCollection.Count == 1)
			{
				return parent.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)dynamicCollection[0][EDIMessage.Schema.EM_SystemCreateUser]);
			}
			else
			{
				return null;
			}
		}
	}
}
