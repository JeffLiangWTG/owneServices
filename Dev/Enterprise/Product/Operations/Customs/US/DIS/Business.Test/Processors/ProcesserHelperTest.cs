using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class ProcesserHelperTest : TestCaseWithFactory
	{
		public void TestGetFallbackBranch()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C~~";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "B1~";
			branch1.GB_GC = company.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "B2~";
			branch2.GB_GC = company.PK;
			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_Code = "B3~";
			branch3.GB_GC = company.PK;
			Factory.Save();
			AssertEquals("GetFallbackBranch", null, ProcesserHelper.GetFallbackBranch(null, null, null));
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_GB = branch1.PK;
			AssertEquals("GetFallbackBranch", branch1, ProcesserHelper.GetFallbackBranch(null, null, incomingMessage));
			var outgingMessage = Factory.New<EDIMessage>();
			outgingMessage.EM_GB = branch2.PK;
			AssertEquals("GetFallbackBranch", branch2, ProcesserHelper.GetFallbackBranch(null, outgingMessage, incomingMessage));
			var mock = new Mock<IDISHost>();
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.BranchPK).Returns(branch3.PK);
			AssertEquals("GetFallbackBranch", branch3, ProcesserHelper.GetFallbackBranch(mock.Object, outgingMessage, incomingMessage));
			mock.VerifyAll();
		}

		public void TestSendNotification()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_LoginName = "~2";
			staff.GS_EmailAddress = "staff@pretendemail.com";
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C~~";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "B~~";
			branch.GB_GC = company.PK;
			USCustomsDataRegistry.Instance.DISMessagesGroup.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ProcesserHelper.SendNotification(Factory, "Error Message Body", "Message Text", company.PK.ToGuid(), branch.PK.ToGuid());
			AssertEquals("1 email generated", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Subject", "Error processing DIS message", email.Subject);
			AssertContains("Body", "Error Message Body", email.Body);
			AssertEquals("1 recipient", 1, email.Recipients.Count);
			var recipient = email.Recipients[0];
			AssertEquals("Email Address", "staff@pretendemail.com", recipient.Email);
			AssertEquals("1 attachment generated", 1, email.Attachments.Count);
			var attachment = email.Attachments[0];
			AssertEquals("Attachment Name", "messagetext.xml", attachment.DisplayName);
		}
	}
}
