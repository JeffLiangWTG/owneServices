using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MessageSenderContactDetailsDefaultingHelperTest : TestCaseWithFactory
	{
		public void TestGetBrokerContact()
		{
			GlbStaff.CurrentUser.GS_FullName = "Timothy Kensington-Double-Barrelled-Shotgun";
			GlbStaff.CurrentUser.GS_WorkPhone = "+18005550100";
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, declaration.PK.ToGuid(), declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid());
			AssertEquals(GlbStaff.CurrentUser, contact);
			AssertEquals("Timothy Kensington-Double-Barrelled-Shotgun", contact.GS_FullName);
			AssertEquals("+18005550100", contact.GS_WorkPhone);
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "+123456789";
			broker.GS_Code = "BRO";
			GlbStaff.CurrentUser.GS_IsSystemAccount = true;
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, declaration.PK.ToGuid(), declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid());
			AssertEquals("Broker", contact.GS_FullName);
			AssertEquals("+123456789", contact.GS_WorkPhone);
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Dan Brown";
			staff.GS_WorkPhone = "+8615850503354";
			staff.GS_GB_HomeBranch = branch.PK;
			USCustomsDataRegistry.Instance.CargoReleaseFTZContact.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, staff.PK.ToGuid());
			contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, declaration.PK.ToGuid(), declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid());
			AssertEquals(staff, contact);
			AssertEquals("Dan Brown", contact.GS_FullName);
			AssertEquals("+8615850503354", contact.GS_WorkPhone);
		}

		public void TestGetPhoneNumber()
		{
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			branch.GB_Phone = "+18005550100";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "+8615850503354";
			staff.GS_GB_HomeBranch = branch.PK;
			var phoneNumber = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(staff);
			AssertEquals("15850503354", phoneNumber);
			staff.GS_WorkPhone = ZString.Empty;
			phoneNumber = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(staff);
			AssertEquals("8005550100", phoneNumber);
		}
	}
}
