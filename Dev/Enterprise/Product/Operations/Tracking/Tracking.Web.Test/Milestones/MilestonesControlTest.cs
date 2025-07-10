using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class MilestonesControlTest : TestCaseWithFactory
	{
		public void TestBindingToEntityWithMilestones()
		{
			var control = new MilestonesControlForTesting();
			control.SetupForTesting();
			control.SetIsInEditModeForTesting(true);

			var webTestHelper = new TestHelper(Factory);
			webTestHelper.TestSiteUser.Login(webTestHelper.TestOrg.OH_Code, webTestHelper.TestContact.OC_Email, webTestHelper.TestContact.PasswordForTesting);
			var right = webTestHelper.TestOrg.SecurityRights.AddNew();
			right.OX_SecurityItemName = WebSecurityRightsList.WebActualMilestonesUpdate.SecurityItemName;
			right.OX_Granted = true;

			Factory.Save();

			var entity = GetEntityWithEditableMilestones();
			control.Bind(entity);
			AssertEquals(0, entity.EditableMilestones.Count);
			AssertEquals(false, control.AllowUpdate);
			AssertEquals(false, control.MilestonesGridForTesting.AllowEdit);
			AssertEquals(false, control.MilestonesGridForTesting.ReadOnly);

			entity.EditableMilestones.AddNew();
			AssertEquals(1, entity.EditableMilestones.Count);
			AssertEquals(true, control.AllowUpdate);

			control.Bind(entity);
			AssertEquals(true, control.MilestonesGridForTesting.AllowEdit);
			AssertEquals(false, control.MilestonesGridForTesting.ReadOnly);

			control.SetIsInEditModeForTesting(false);
			control.Bind(entity);
			AssertEquals(false, control.MilestonesGridForTesting.AllowEdit);
			AssertEquals(true, control.MilestonesGridForTesting.ReadOnly);
		}

		public void TestBindingToEntityWithoutMilestones()
		{
			var control = new MilestonesControlForTesting();
			control.SetupForTesting();
			control.SetIsInEditModeForTesting(false);

			var webTestHelper = new TestHelper(Factory);
			webTestHelper.TestSiteUser.Login(webTestHelper.TestOrg.OH_Code, webTestHelper.TestContact.OC_Email, webTestHelper.TestContact.PasswordForTesting);
			var right = webTestHelper.TestOrg.SecurityRights.AddNew();
			right.OX_SecurityItemName = WebSecurityRightsList.WebActualMilestonesUpdate.SecurityItemName;
			right.OX_Granted = true;

			Factory.Save();

			var entity = GetEntityWithoutEditableMilestones();
			control.Bind(entity);
			AssertEquals(false, control.AllowUpdate);
			AssertEquals(false, control.MilestonesGridForTesting.AllowEdit);
			AssertEquals(false, control.MilestonesGridForTesting.ReadOnly);
		}

		object GetEntityWithoutEditableMilestones()
		{
			var workflowProvider = Factory.New<DummyWithWorkflow>();
			var entity = new
			{
				Milestones = new TrackingMilestoneCollection(workflowProvider, false)
			};

			return entity;
		}

		IMilestonesProvider GetEntityWithEditableMilestones()
		{
			var workflowProvider = Factory.New<DummyWithWorkflow>();
			var milestonesProviderMock = new Mock<IMilestonesProvider>();
			var milestones = new TrackingMilestoneCollection(workflowProvider, true);
			milestonesProviderMock.Setup(m => m.EditableMilestones).Returns(milestones);

			return milestonesProviderMock.Object;
		}
	}
}
