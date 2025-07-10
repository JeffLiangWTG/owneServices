using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class MessageProcessorMailHelperTestCase : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetUserToNotify()
		{
			var batchProcessorStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);
			var postmasterStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, "CWPostMaster");
			var currentStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);

			var parent = Factory.New<TestHelperEDIMessage>();
			var mailHelper = new MessageProcessorReportMailerForTest(Factory, logger);

			var result = mailHelper.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertNull(result);

			SimulateMessage(parent, postmasterStaffMember, ZDateTime.Now.AddMinutes(-15), EDIMessage.Direction.Transmit);
			result = mailHelper.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", postmasterStaffMember, result);

			SimulateMessage(parent, batchProcessorStaffMember, ZDateTime.Now.AddMinutes(-10), EDIMessage.Direction.Receive);
			result = mailHelper.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", postmasterStaffMember, result);

			SimulateMessage(parent, currentStaffMember, ZDateTime.Now.AddMinutes(-5), EDIMessage.Direction.Transmit);
			result = mailHelper.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", currentStaffMember, result);

			SimulateMessage(parent, batchProcessorStaffMember, ZDateTime.Now, EDIMessage.Direction.Transmit);
			result = mailHelper.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", currentStaffMember, result);
		}

		public void TestSendReport()
		{
			var parent = Factory.New<TestHelperEDIMessage>();
			SimulateMessage(parent, postMasterStaffMember, ZDateTime.Now.AddMinutes(-15), EDIMessage.Direction.Transmit);

			var mailHelper = new MessageProcessorReportMailerForTest(Factory, logger);

			AssertAcknowledgement(mailHelper, parent, Core.Constants.EmailTo.NoEmails, 0, 0);
			AssertAcknowledgement(mailHelper, parent, Core.Constants.EmailTo.StaffMember, 1, 0);
			AssertAcknowledgement(mailHelper, parent, Core.Constants.EmailTo.NominatedGroup, 0, 1);
			AssertAcknowledgement(mailHelper, parent, Core.Constants.EmailTo.StaffMemberAndNominatedGroup, 1, 1);

			AssertImpediment(mailHelper, parent, Core.Constants.EmailTo.NoEmails, 0, 0);
			AssertImpediment(mailHelper, parent, Core.Constants.EmailTo.StaffMember, 1, 0);
			AssertImpediment(mailHelper, parent, Core.Constants.EmailTo.NominatedGroup, 0, 1);
			AssertImpediment(mailHelper, parent, Core.Constants.EmailTo.StaffMemberAndNominatedGroup, 1, 1);

			AssertError(mailHelper, parent, Core.Constants.EmailTo.NoEmails, 0, 0);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.StaffMember, 1, 0);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.NominatedGroup, 0, 1);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.StaffMemberAndNominatedGroup, 1, 1);

			parent = null;

			AssertError(mailHelper, parent, Core.Constants.EmailTo.NoEmails, 0, 0);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.StaffMember, 0, 0);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.NominatedGroup, 0, 1);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.StaffMemberAndNominatedGroup, 0, 1);

			mailHelper.ReportWhenNoParentExposed = true;

			AssertError(mailHelper, parent, Core.Constants.EmailTo.NoEmails, 0, 1);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.StaffMember, 0, 1);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.NominatedGroup, 0, 1);
			AssertError(mailHelper, parent, Core.Constants.EmailTo.StaffMemberAndNominatedGroup, 0, 1);
		}

		#region Implementation

		protected void AssertAcknowledgement(MessageProcessorReportMailer mailHelper, BusinessObject parent, ZString acknowledgementEmailMode, int recipients, int cCRecipients)
		{
			EmailDef email = new EmailDef();
			mailHelper.SendReport(email, parent, acknowledgementEmailMode, EmailGroup);

			AssertEquals("Acknowledgement Recipients", recipients, email.Recipients.Count);
			AssertEquals("Acknowledgement CCRecipients", cCRecipients, email.CCRecipients.Count);
		}

		protected void AssertImpediment(MessageProcessorReportMailer mailHelper, BusinessObject parent, ZString impedimentEmailMode, int recipients, int cCRecipients)
		{
			EmailDef email = new EmailDef();
			mailHelper.SendReport(email, parent, impedimentEmailMode, EmailGroup);

			AssertEquals("Recipients", recipients, email.Recipients.Count);
			AssertEquals("CCRecipients", cCRecipients, email.CCRecipients.Count);
		}

		protected void AssertError(MessageProcessorReportMailer mailHelper, BusinessObject parent, ZString errorEmailMode, int recipients, int cCRecipients)
		{
			EmailDef email = new EmailDef();
			mailHelper.SendReport(email, parent, errorEmailMode, EmailGroup);

			AssertEquals("Recipients", recipients, email.Recipients.Count);
			AssertEquals("CCRecipients", cCRecipients, email.CCRecipients.Count);
		}

		ZGuid EmailGroup
		{
			get
			{
				if (fAcknowledgementEmailGroup.IsEmpty)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					GlbGroup group = factory.New<GlbGroup>();
					var currentStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);

					group.Staff.Add(currentStaffMember);
					currentStaffMember.GS_EmailAddress = "TEST@EDI.COM.AU";
					var anotherStaff = factory.New<GlbStaff>();
					anotherStaff.GS_EmailAddress = "Hosting.Notifications@cargowise.com";
					group.Staff.Add(anotherStaff);
					factory.Save();
					fAcknowledgementEmailGroup = group.PK;
				}
				return fAcknowledgementEmailGroup;
			}
		}
		protected ZGuid fAcknowledgementEmailGroup;

		protected void SimulateMessage(BusinessObject parent, GlbStaff sender, ZDateTime addedTime, ZString direction)
		{
			var newMessage = (EDIMessage)parent.Factory.New(typeof(TestHelperEDIMessage));
			newMessage.EM_LinkedObject = parent;
			newMessage.EM_ReceiveTransmit = direction;
			parent.Factory.Save();

			// These tests might depend on the log being changed outside the main factory.
			newMessage.EM_SystemCreateUser = sender.GS_Code;
			newMessage.EM_SystemCreateTimeUtc = addedTime;
			parent.Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new LoggingInformation();

			postMasterStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, "CWPostMaster");
			postMasterStaffMember.GS_EmailAddress = "SYSADMIN@EDI.COM.AU";
			Factory.Save();
		}

		protected LoggingInformation logger;
		protected GlbStaff postMasterStaffMember;

		#region MailHelperForTest

		protected class MessageProcessorReportMailerForTest : MessageProcessorReportMailer
		{
			public MessageProcessorReportMailerForTest(BusinessObjectFactory factory, LoggingInformation logger)
				: base(factory, logger)
			{
			}

			public new GlbStaff GetLastNonBatchProcessorStaffToSendMessage(BusinessObject parent)
			{
				return base.GetLastNonBatchProcessorStaffToSendMessage(parent);
			}

			public bool ReportWhenNoParentExposed { get; set; }

			protected override bool ReportWhenNoParent
			{
				get { return ReportWhenNoParentExposed; }
			}
		}

		#endregion

		#region TestHelperEDIMessage

		public class TestHelperEDIMessage : EDIMessage
		{
			public TestHelperEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//dont do anything
			}
		}

		#endregion

		#endregion

	}
}
