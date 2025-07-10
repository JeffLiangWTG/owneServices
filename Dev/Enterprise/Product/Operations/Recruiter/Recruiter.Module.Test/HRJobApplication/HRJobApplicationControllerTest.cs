using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module
{
	[TestedType(typeof(HRJobApplicationController))]
	public class HRJobApplicationControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new HRJobApplicationController();
			AssertEquals(ModuleIDs.HRJobApplication, controller.ModuleID);
		}

		public void TestUrlOpenable()
		{
			var controller = new HRJobApplicationController();
			Assert("Any company should be able to open any HRJobApplication", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(HRJobApplication);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.HRJobApplication;
		}
	}
}
