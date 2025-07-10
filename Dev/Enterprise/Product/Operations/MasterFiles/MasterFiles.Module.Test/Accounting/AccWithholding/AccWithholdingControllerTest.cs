using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccWithholdingController))]
	sealed class AccWithholdingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccWithholding;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var withholding = Factory.New<AccWithholding>();
			withholding.AW_GC = Factory.LoadTop1(typeof(GlbCompany), new ZQuery()).PK;
			Factory.Save();
			return withholding;
		}

		public override void TestNewForm()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "User1";

			Factory.Save();

			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			var departmentPK = EnvProxy.Instance.CurrentDepartment.PK;

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branchPK, departmentPK))
			{
				AssertNull("NewForm should be null", Controller.ShowNewForm());
				AssertEquals("You are not allowed to add a new withholding tax code", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, branchPK, departmentPK))
			{
				AssertNotNull("NewForm should be null", Controller.ShowNewForm());
			}
		}

		public override void TestDeleteForm()
		{
			var sourceEntity = GetBusinessObjectThatIsInTheDatabase();
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(sourceEntity));
			AssertEquals(sourceEntity.ReasonForNotAbleToDelete, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
