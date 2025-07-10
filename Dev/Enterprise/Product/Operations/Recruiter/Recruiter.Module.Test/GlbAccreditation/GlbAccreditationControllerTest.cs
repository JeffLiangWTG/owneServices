using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationController))]
	public class GlbAccreditationControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new GlbAccreditationController();
			AssertEquals(ModuleIDs.GlbAccreditation, controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(GlbAccreditation);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbAccreditation;
		}
	}
}
