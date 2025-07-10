using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbDepartmentModule))]
	sealed class GlbDepartmentModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestDeactivateAllDepartments()
		{
			GlbDepartment[] activeDeps = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsActive, true));
			GlbDepartment[] allDeps = Factory.Load<GlbDepartment>(new ZQuery());
			GlbDepartment[] inactiveDeps = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsActive, false));

			using (Department = new GlbDepartmentModuleForTest())
			{
				var activator = Department.GetNewBusinessObjectActivator_Exposed();
				activator.Deactivate(activeDeps, Env.Security.None);
				AssertEquals("You cannot deactivate all Departments as no one will be able to login after that. Please leave at least one active Department.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				activator.Deactivate(allDeps, Env.Security.None);
				AssertEquals("You cannot deactivate all Departments as no one will be able to login after that. Please leave at least one active Department.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				activator.Deactivate(inactiveDeps, Env.Security.None);
				AssertNotEquals("You cannot deactivate all Departments as no one will be able to login after that. Please leave at least one active Department.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowDefaultActivateDeactivate()
		{
			using (Department = new GlbDepartmentModuleForTest())
			{
				Assert(Department.AllowDefaultActivateDeactivate);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbDepartment;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			Department = new GlbDepartmentModuleForTest();
			IFilterControl controlForTest = Department.GetNewFilterControlForTest();
			Assert(controlForTest is GlbDepartmentFilterControl);
			controlForTest.Dispose();
			Department.Dispose();
		}

		public void TestGridCollection()
		{
			Department = new GlbDepartmentModuleForTest();
			Assert(Department.GetNewGridCollectionForTest() is IBusinessObjectCollection);
			Department.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			Department = new GlbDepartmentModuleForTest();
			Assert(Department.GetNewFilterBusinessObjectForTest() is FilterBusinessObject);
			Department.Dispose();
		}

		#region Implementation

		GlbDepartmentModuleForTest Department;

		#endregion
	}
}
