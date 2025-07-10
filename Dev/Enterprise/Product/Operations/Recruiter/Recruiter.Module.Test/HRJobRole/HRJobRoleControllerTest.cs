using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobRoleController))]
	public class HRJobRoleControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(HRJobRole);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.HRJobRole;
		}

		public void TestUrlOpenable()
		{
			var controller = new HRJobRoleController();
			Assert("Any company should be able to open any job role", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
