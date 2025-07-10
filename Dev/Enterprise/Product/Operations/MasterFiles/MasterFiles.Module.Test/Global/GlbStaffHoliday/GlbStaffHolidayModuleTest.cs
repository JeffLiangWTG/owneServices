using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffHolidayModule))]
	sealed class GlbStaffHolidayModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GlbStaffHoliday;

		public void TestModuleIdentifier()
		{
			DisposingTest((staffHolidayModule) => AssertEquals(staffHolidayModule.ID, ModuleIDs.GlbStaffHoliday));
		}

		public void TestSecurityCheckpoint()
		{
			DisposingTest((staffHolidayModule) => AssertEquals(staffHolidayModule.SecurityCheckpoint, Env.Security.Staff));
		}

		public void TestLicenceCheckPoint()
		{
			DisposingTest((staffHolidayModule) => AssertEquals(staffHolidayModule.LicenceCheckPoint, Env.Licence.Core));
		}

		public void TestSupportsWorkflow()
		{
			DisposingTest((staffHolidayModule) => AssertEquals(staffHolidayModule.SupportsWorkflow, true));
		}

		public void TestAllowCopyFilterGridHyperlinkToClipboard()
		{
			DisposingTest((staffHolidayModule) => AssertEquals(staffHolidayModule.AllowCopyFilterGridHyperlinkToClipboard, false));
		}

		public void TestWorkflowType()
		{
			DisposingTest((staffHolidayModule) => AssertEquals(staffHolidayModule.WorkflowType, WorkflowDescriptors.GlbStaffHolidayDescriptorCode));
		}

		void DisposingTest(Action<GlbStaffHolidayModule> test)
		{
			using (var staffHolidayModule = GetStaffHolidayModule())
			{
				test(staffHolidayModule);
			}
		}

		GlbStaffHolidayModule GetStaffHolidayModule() => (GlbStaffHolidayModule)ZModuleFactory.Instance.Create(GetModuleID());
	}
}
