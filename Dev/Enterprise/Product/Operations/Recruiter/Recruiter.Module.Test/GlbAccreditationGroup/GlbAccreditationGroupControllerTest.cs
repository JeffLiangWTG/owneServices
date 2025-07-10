using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationGroupController))]
	public class GlbAccreditationGroupControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new GlbAccreditationGroupController();
			AssertEquals(ModuleIDs.GlbAccreditationGroup, controller.ModuleID);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(GlbAccreditationGroup);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbAccreditationGroup;
		}
	}
}
