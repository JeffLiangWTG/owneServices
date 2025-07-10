using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices
{
	class eAdaptorNextContextSetterTest : TestCaseWithFactory
	{
		public void TestSetContextUsesWebContext()
		{
			eAdaptorNextConfig.Instance.ContextSetter.SetContext();
			AssertEquals(User.WebUserName, Env.CurrentUser.LoginName);
		}

		public void TestSetContextUsesSecurityProxy()
		{
			var partyConfig = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			partyConfig.ECC_ECP_Party = party.PK;
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Staff1";
			party.ECP_GS_SecurityProxy = staff1.PK;
			partyConfig.ECC_GB_Branch = Env.CurrentBranchPK;
			partyConfig.ECC_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig = partyConfig;
			using (Env.Instance.SuppressSwitchContextCheck())
			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				eAdaptorNextConfig.Instance.ContextSetter.SetContext();
				AssertEquals(staff1.GS_LoginName, Env.CurrentUser.LoginName);
			}
		}
		public void TestSetContextUsesWebUserIfNoSecurityProxy()
		{
			var partyConfig = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			partyConfig.ECC_ECP_Party = party.PK;
			partyConfig.ECC_GB_Branch = Env.CurrentBranchPK;
			partyConfig.ECC_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig = partyConfig;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Staff1";

			using (Env.Instance.SuppressSwitchContextCheck())
			using (Env.SetTemporaryUserContext(staff1.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				eAdaptorNextConfig.Instance.ContextSetter.SetContext();
				AssertEquals(User.WebUserName, Env.CurrentUser.LoginName);
			}
		}
	}
}
