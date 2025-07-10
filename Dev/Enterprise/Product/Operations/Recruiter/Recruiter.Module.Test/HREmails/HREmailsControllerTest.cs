using System;
using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HREmailsController))]
	public class HREmailsControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new HREmailsController();
			AssertEquals(ModuleIDs.HREmails, controller.ModuleID);
		}

		public void TestUrlOpenable()
		{
			var controller = new HREmailsController();
			Assert("Any company should be able to open any email", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(HREmails);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.HREmails;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var email = Factory.NewWithValidTestData<HREmails>();
			email.MI_Direction = MailDirection.Receive;
			Factory.Save();
			return email;
		}
	}
}
