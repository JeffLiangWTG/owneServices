using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgMatchApprovalController))]
	public class OrgMatchApprovalControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.OrgMatchApproval, new OrgMatchApprovalController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			TestOrgMatchApprovalController controller = new TestOrgMatchApprovalController();
			AssertEquals("For New", Env.Security.OrgMatchApproval, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.OrgMatchApproval, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.OrgMatchApproval, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.OrgMatchApproval, controller.CheckPointForDelete);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyBusinessObject parent = Factory.New<DummyBusinessObject>();
			DummyOrgMatchApproval dummyMatchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(parent.PK, OrgMatchApprovalType.DummyType);

			Factory.Save();
			return dummyMatchApproval;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgMatchApproval;
		}

		class TestOrgMatchApprovalController : OrgMatchApprovalController
		{
			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}
	}
}
