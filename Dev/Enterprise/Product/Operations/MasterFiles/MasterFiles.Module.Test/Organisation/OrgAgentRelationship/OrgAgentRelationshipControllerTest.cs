using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAgentRelationshipController))]
	sealed class OrgAgentRelationshipControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ProfitShare;
		}

		public void TestModuleID()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.ProfitShare);
			AssertEquals(ModuleIDs.ProfitShare, controller.ModuleID);
		}

		public void TestCheckpoints()
		{
			var controller = new OrgAgentRelationshipControllerForTest();
			AssertEquals("SecurityCheckpointForNew", Env.Security.ProfitShareNew, controller.CheckPointForNew);
			AssertEquals("SecurityCheckpointForView", Env.Security.ProfitShareView, controller.CheckPointForView);
			AssertEquals("SecurityCheckpointForEdit", Env.Security.ProfitShareEdit, controller.CheckPointForEdit);
			AssertEquals("SecurityCheckpointForDelete", Env.Security.ProfitShareDelete, controller.CheckPointForDelete);
		}

		public void TestOverrides()
		{
			var controller = new OrgAgentRelationshipControllerForTest();
			Assert(controller.MakeUrlsOnlyOpenableForCurrentCompany);
			AssertEquals(typeof(OrgAgentRelationship), controller.TypeOfTopLevelBusinessObject);
		}

		class OrgAgentRelationshipControllerForTest : OrgAgentRelationshipController
		{
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
			public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;
			public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;
			public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
		}
	}
}
