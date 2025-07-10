using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module
{
	[TestedType(typeof(HRJobApplicantController))]
	public class HRJobApplicantControllerTest : ZControllerBasherTest
	{
		public void TestUrlOpenable()
		{
			var controller = new HRJobApplicantController();
			Assert("Any company should be able to open any job applicant", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		public void TestModuleID()
		{
			var controller = new HRJobApplicantController();
			AssertEquals(ModuleIDs.HRJobApplicant, controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(HRJobApplicant);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.HRJobApplicant;
		}
	}
}
