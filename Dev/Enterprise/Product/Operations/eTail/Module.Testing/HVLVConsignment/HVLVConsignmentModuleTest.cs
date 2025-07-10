using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVConsignmentModule))]
	class HVLVConsignmentModuleTest : ZModuleBasherTest
	{
		public void TestActionMenuOperationalActions()
		{
			using (var module = GetModule())
			{
				var grid = module.DisplayGrid as ZDisplayGrid;
				var actionMenu = grid.ContextMenu.MenuItems.FindByText("Actions");
				actionMenu.ShowPopupMenu();
				var operationalActionsMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				AssertNotNull(operationalActionsMenu);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Security.HVLVConsignment, module.SecurityCheckpoint);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Licence.Forwarder, module.LicenceCheckPoint);
			}
		}

		public void TestBizoOperations()
		{
			using (var module = GetModule())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Allow New", false, module.AllowNew);
					AssertEquals("Allow View", true, module.AllowView);
					AssertEquals("Allow Edit", true, module.AllowEdit);
					AssertEquals("Allow Delete", true, module.AllowDelete);
					AssertEquals("Allow Activate/Deactivate", false, module.AllowDefaultActivateDeactivate);
				});
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = GetModule())
			{
				AssertType<HVLVConsignmentFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		public void TestGetNewController()
		{
			using (var module = GetModule())
			{
				AssertType<HVLVConsignmentController>(module.GetNewController());
			}
		}

		public void TestGetNewFilterControl()
		{
			using (var module = GetModule())
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<HVLVConsignmentFilterControl>(filterControl);
			}
		}

		public void TestGetNewBusinessObjectCollection()
		{
			using (var module = GetModule())
			{
				AssertType<ActiveBusinessObjectCollection<HVLVConsignment>>(module.GetNewBusinessObjectCollection());
			}
		}

		public void TestWorkflow()
		{
			using (var module = (HVLVConsignmentModule)GetModule())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Supports Workflow", true, module.SupportsWorkflow);
					AssertEquals("Workflow Type", WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode, module.WorkflowType);
				});
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.HVLVConsignment;

		#endregion
	}
}
