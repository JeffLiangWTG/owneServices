using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class NotificationUtilsTest : TestCaseWithFactory
	{
		public void TestGetRecipients_SendEmailOptions()
		{
			CombineAssertions(delegate
			{
				AssertEmailAddresses("No Recipient, NotificationGroup email addresses"
					, "Staff1@edi.com.au, Staff2@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, "", Core.Constants.EmailTo.StaffMember, Guid.Empty));

				AssertEmailAddresses("No Recipient, Nominated Group + StaffMember required, NotificationGroup email addresses"
					, "Staff1@edi.com.au, Staff2@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, "", Core.Constants.EmailTo.StaffMemberAndNominatedGroup, Guid.Empty));

				AssertEmailAddresses("No Recipient but Group, no NotificationGroup email addresses"
					, "bill@edi.com.au, ted@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, "", Core.Constants.EmailTo.NominatedGroup, Group2.PK));

				AssertEmailAddresses("Recipient (Staff Memeber)"
					, "rufus@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, "rufus@edi.com.au", Core.Constants.EmailTo.StaffMember, Guid.Empty));

				AssertEmailAddresses("Nominated Group"
					, "bill@edi.com.au, ted@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, "rufus@edi.com.au", Core.Constants.EmailTo.NominatedGroup, Group2.PK));

				AssertEmailAddresses("Nominated Group + Recipient (Staff Member)"
					, "bill@edi.com.au, rufus@edi.com.au, ted@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, "rufus@edi.com.au", Core.Constants.EmailTo.StaffMemberAndNominatedGroup, Group2.PK));
			});
		}

		public void TestGetRecipients_NotifySenderNotifyGroup()
		{
			CombineAssertions(delegate
			{
				AssertEmailAddresses("No Recipient, NotificationGroup email addresses"
					, "Staff1@edi.com.au, Staff2@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, true, false, "", Guid.Empty));

				AssertEmailAddresses("No Recipient, Nominated Group + StaffMember required, NotificationGroup email addresses"
					, "Staff1@edi.com.au, Staff2@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, true, true, "", Guid.Empty));

				AssertEmailAddresses("No Recipient but Group, no NotificationGroup email addresses"
					, "bill@edi.com.au, ted@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, false, true, "", Group2.PK));

				AssertEmailAddresses("Recipient (Staff Memeber)"
					, "rufus@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, true, false, "rufus@edi.com.au", Guid.Empty));

				AssertEmailAddresses("Nominated Group"
					, "bill@edi.com.au, ted@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, false, true, "rufus@edi.com.au", Group2.PK));

				AssertEmailAddresses("Nominated Group + Recipient (Staff Member)"
					, "bill@edi.com.au, rufus@edi.com.au, ted@edi.com.au"
					, NotificationUtils.GetRecipients(Factory, true, true, "rufus@edi.com.au", Group2.PK));
			});
		}

		#region Implementation

		GlbGroup Group1;
		GlbStaff Group1Staff1;
		GlbStaff Group1Staff2;
		GlbGroup Group2;
		GlbStaff Group2Staff1;
		GlbStaff Group2Staff2;
		EmailGroupUtility Utility;

		protected override void SetUp()
		{
			base.SetUp();

			Group1 = GetNewGroup("Group1");
			Group1Staff1 = Group1.Staff.AddNew();
			Group1Staff1.GS_EmailAddress = "Staff1@edi.com.au";
			Group1Staff1.GS_LoginName = "Staff1";
			Group1Staff1.GS_Code = "SF1";
			Group1Staff2 = Group1.Staff.AddNew();
			Group1Staff2.GS_EmailAddress = "Staff2@edi.com.au";
			Group1Staff2.GS_LoginName = "Staff2";
			Group1Staff2.GS_Code = "SF2";

			Group2 = GetNewGroup("Group2");
			Group2Staff1 = Group2.Staff.AddNew();
			Group2Staff1.GS_EmailAddress = "bill@edi.com.au";
			Group2Staff1.GS_LoginName = "_Bill";
			Group2Staff1.GS_Code = "_BL";
			Group2Staff2 = Group2.Staff.AddNew();
			Group2Staff2.GS_EmailAddress = "ted@edi.com.au";
			Group2Staff2.GS_LoginName = "_Ted";
			Group2Staff2.GS_Code = "_TD";

			Factory.Save();

			Utility = new EmailGroupUtility();
			Utility.SetNotificationGroup(Group1.PK.ToGuid());
		}

		static void AssertEmailAddresses(string message, string expectedEmailAddresses, List<string> actualEmailAddresses)
		{
			actualEmailAddresses.Sort();
			AssertEquals(message, expectedEmailAddresses, string.Join(", ", actualEmailAddresses.ToArray()));
		}

		GlbGroup GetNewGroup(string code)
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = code;
			group.GG_IsActive = true;

			return group;
		}

		#endregion
	}
}
