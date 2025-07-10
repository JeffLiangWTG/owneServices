using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public static class NotificationUtils
	{
		public static List<string> GetRecipients(BusinessObjectFactory factory, ZString recipient, string sendEmailOptions, ZGuid groupPK)
		{
			return new RecipientLoader(factory, recipient, sendEmailOptions, groupPK).Recipients;
		}

		public static List<string> GetRecipients(BusinessObjectFactory factory, ZBool notifySender, ZBool notifyGroup, ZString recipient, ZGuid groupPK)
		{
			return new RecipientLoader(factory, notifySender, notifyGroup, recipient, groupPK).Recipients;
		}

		class RecipientLoader
		{
			internal RecipientLoader(BusinessObjectFactory factory, ZString recipient, string sendEmailOptions, ZGuid groupPK)
			{
				this.factory = factory;

				this.Recipients = new List<string>();

				switch (sendEmailOptions)
				{
					case Core.Constants.EmailTo.StaffMember:
						AddRecipientIfSpecified(recipient);
						AddCompanyNotificationGroupEmailsIfNoOtherEmailsAdded();
						break;

					case Core.Constants.EmailTo.StaffMemberAndNominatedGroup:
						AddRecipientIfSpecified(recipient);
						AddEmailAddressesFromAllUsersInGroup(groupPK);
						AddCompanyNotificationGroupEmailsIfNoOtherEmailsAdded();
						break;

					case Core.Constants.EmailTo.NominatedGroup:
						AddEmailAddressesFromAllUsersInGroup(groupPK);
						break;
				}
			}

			internal RecipientLoader(BusinessObjectFactory factory, ZBool notifySender, ZBool notifyGroup, ZString recipient, ZGuid groupPK)
			{
				this.factory = factory;
				this.Recipients = new List<string>();

				if (notifySender)
				{
					AddRecipientIfSpecified(recipient);
				}

				if (notifyGroup)
				{
					AddEmailAddressesFromAllUsersInGroup(groupPK);
				}

				if (notifySender)
				{
					AddCompanyNotificationGroupEmailsIfNoOtherEmailsAdded();
				}
			}

			internal readonly List<string> Recipients;
			readonly BusinessObjectFactory factory;

			void AddRecipientIfSpecified(ZString recipient)
			{
				if (!recipient.IsEmpty)
				{
					Recipients.Add(recipient.Trim());
				}
			}

			void AddEmailAddressesFromAllUsersInGroup(ZGuid groupPK)
			{
				if (!groupPK.IsEmpty)
				{
					var group = factory.Load<GlbGroup>(groupPK);
					if (group != null)
					{
						foreach (GlbStaff staff in group.Staff)
						{
							AddRecipientIfSpecified(staff.GS_EmailAddress);
						}
					}
				}
			}

			void AddCompanyNotificationGroupEmailsIfNoOtherEmailsAdded()
			{
				if (Recipients.Count == 0)
				{
					foreach (var emailAddress in new EmailGroupUtility().GetCompanyNotificationGroupEmails())
					{
						Recipients.Add(emailAddress);
					}
				}
			}
		}
	}
}
