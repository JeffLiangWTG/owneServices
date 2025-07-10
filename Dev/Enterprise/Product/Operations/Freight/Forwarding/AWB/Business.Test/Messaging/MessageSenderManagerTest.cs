using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class MessageSenderManagerTest : TestCaseWithFactory
	{
		public void TestWorksWithNormalStaffRecord()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "W&W";
			staff.GS_LoginName = "Womack&Womack";
			staff.GS_EmailAddress = "womack.and.womack@soulwalking.co.uk";
			Factory.Save();

			CIMEDIMessage message;
			using (Env.Instance.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				message = Factory.New<CIMEDIMessage>();
				Factory.Save();
			}

			var managerBeforeSave = new MessageSenderManager(message);
			managerBeforeSave.LogSenderIfRequired(null);
			AssertEquals("managerBeforeSave.SendersEmailAddress", "womack.and.womack@soulwalking.co.uk", managerBeforeSave.SendersEmailAddress);

			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedMessage = reloadingFactory.Load<CIMEDIMessage>(message.PK);
			var managerAfterReload = new MessageSenderManager(reloadedMessage);

			AssertEquals("managerAfterReload.SendersEmailAddress", "womack.and.womack@soulwalking.co.uk", managerAfterReload.SendersEmailAddress);
		}

		public void TestWorksWithOrgContactWhenSpecified()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "W&W";
			staff.GS_EmailAddress = "womack.and.womack@soulwalking.co.uk";
			Factory.Save();

			CIMEDIMessage message;
			using (Env.Instance.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				message = Factory.New<CIMEDIMessage>();
				Factory.Save();
			}

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "WALDISLAX";

			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Mickey Mouse";
			contact.OC_Email = "mickey.mouse@disney.com";

			var managerBeforeSave = new MessageSenderManager(message);
			managerBeforeSave.LogSenderIfRequired(contact);
			AssertEquals("managerBeforeSave.SendersEmailAddress", "mickey.mouse@disney.com", managerBeforeSave.SendersEmailAddress);

			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedMessage = reloadingFactory.Load<CIMEDIMessage>(message.PK);
			var managerAfterReload = new MessageSenderManager(reloadedMessage);

			AssertEquals("managerAfterReload.SendersEmailAddress", "mickey.mouse@disney.com", managerAfterReload.SendersEmailAddress);
		}
	}
}
