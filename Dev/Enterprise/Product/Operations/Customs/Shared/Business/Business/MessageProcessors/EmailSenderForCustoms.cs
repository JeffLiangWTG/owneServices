using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business
{
	public class EmailSender
	{
		public EmailSender(LoggingInformation logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		public void SendNotification(EmailDef email, GlbStaff userToNotify, string emailMode, Guid group, IRegistryItem groupRegistry)
		{
			if (emailMode == Core.Constants.EmailTo.StaffMember || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForSystemCommunication(userToNotify.GS_EmailAddress);
				}
			}
			Guid notificationGroupPk = Guid.Empty;
			if (emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				notificationGroupPk = group;
			}
			SendEmail_SaveNow(email, notificationGroupPk, groupRegistry);
		}

		public void SendNotification(EmailDef email, GlbStaff userToNotify, string emailMode, Guid group, IRegistryItem groupRegistry, BusinessObjectFactory factoryInWhichToSaveLater)
		{
			this.factory = factoryInWhichToSaveLater;
			if (emailMode == Core.Constants.EmailTo.StaffMember || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
				{
					email.AddRecipientForSystemCommunication(userToNotify.GS_EmailAddress);
				}
			}
			Guid notificationGroupPk = Guid.Empty;
			if (emailMode == Core.Constants.EmailTo.NominatedGroup || emailMode == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
			{
				notificationGroupPk = group;
			}
			SendEmail_SaveLater(email, notificationGroupPk, groupRegistry);
		}

		public void SendNotification(EmailDef email, Guid group, IRegistryItem groupRegistry)
		{
			SendEmail_SaveNow(email, group, groupRegistry);
		}

		public void SendNotification(EmailDef email, Guid group, IRegistryItem groupRegistry, BusinessObjectFactory factoryInWhichToSaveLater)
		{
			this.factory = factoryInWhichToSaveLater;
			SendEmail_SaveLater(email, group, groupRegistry);
		}

		public void SendEmail_SaveNow(EmailDef email, Guid notificationGroupPk, IRegistryItem groupRegistry)
		{
			try
			{
				Env.OutgoingCustomsMailManager.CreateAndSave(email, notificationGroupPk, GroupSourceLocator.GetFromRegistryItem(groupRegistry));
			}
			catch (EmailHasNoRecipientsException)
			{
				logger.LogWarning((NoResString)"No recipients were found to send this email to. Check that the staff who queued the Job's original message has a published email address, or that the Notification or Post Masters group have users with published email addresses.");
			}
			catch (EmailSendFailedException e)
			{
				logger.LogWarning((NoResString)"Email not sent: " + e.Message);
			}
		}

		public void SendEmail_SaveLater(EmailDef email, Guid notificationGroupPk, IRegistryItem groupRegistry)
		{
			try
			{
				Env.OutgoingCustomsMailManager.Create(Factory, email, notificationGroupPk, GroupSourceLocator.GetFromRegistryItem(groupRegistry));
			}
			catch (EmailHasNoRecipientsException)
			{
				logger.LogWarning((NoResString)"No recipients were found to send this email to. Check that the staff who queued the Job's original message has a published email address, or that the Notification or Post Masters group have users with published email addresses.");
			}
			catch (EmailSendFailedException e)
			{
				logger.LogWarning((NoResString)"Email not sent: " + e.Message);
			}
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}
	}
}
