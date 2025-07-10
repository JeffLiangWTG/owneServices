using System;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(ExamSettingsController))]
	public class ExamSettingsControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new ExamSettingsController();
			AssertEquals(ModuleIDs.ExamSetting, controller.ModuleID);
			AssertEquals(Env.Security.ExamSettingView, controller.CheckPointForViewExposedForTest);
			AssertEquals(Env.Security.ExamSettingNew, controller.CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.ExamSettingEdit, controller.CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.ExamSettingDelete, controller.CheckPointForDeleteExposedForTest);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(ExamSetting);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ExamSetting;
		}
	}
}
