using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Module.Test
{
	class ProductivityWiseModeCompatabilityTest : TestCaseWithFactory
	{
		public void TestWorkflowDescriptorValues_WhenProductivitiyWiseModeEnabled_ShouldIncludeAllModulesWhichSupportWorkflow()
		{
			// This is important because we selectively add workflow descriptors as well as modules, so they might get out of sync.

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			EnsureWorkflowDescriptorValuesIncludesAllModulesWhichSupportWorkflow("Modules required for ProductivityWise need to be added explicitly in WorkflowDescriptors.IsAllowedForProductivityWise.");
		}

		public void TestWorkflowDescriptorValues_WhenProductivitiyWiseModeDisabled_ShouldIncludeAllModulesWhichSupportWorkflow()
		{
			// This just verifies that all module WorkflowTypes are in fact present in case the wrong 3-character code is used.

			AssertEquals(false, DataRegistry.Instance.ProductivityWiseModeEnabled);
			EnsureWorkflowDescriptorValuesIncludesAllModulesWhichSupportWorkflow(null);
		}

		void EnsureWorkflowDescriptorValuesIncludesAllModulesWhichSupportWorkflow(string additionalMessage)
		{
			var workflowSupportingModules = GetAllWorkflowSupportingModulesThatAppearInTree();

			CombineAssertions($"All modules which support workflow and are present in the module tree should have their relevant WorkflowDescriptor present in the WorkflowDescriptors collection. Otherwise it will be impossible to create workflow templates for them, among other impossibilities. " + additionalMessage, () =>
			{
				foreach (var module in workflowSupportingModules)
				{
					AssertNotNull($"Module: [{module.Description}], Type: [{module.GetType().FullName}], Workflow Type: " + module.WorkflowType, WorkflowDescriptors.Instance.TryGetValueSafe(module.WorkflowType));
				}
			});
		}

		public void TestProductivityWiseWorkflowDescriptors_ShouldHaveMatchingModulesVisibleInModuleTree()
		{
			// Arrange
			Recruitment.Registry.RecruitmentDataRegistry.Instance.IsAccreditationModuleEnabled.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			ProcessMgmtTestHelper.EnableBufferManagement();
			ProcessMgmtTestHelper.EnableProductivityWise();
			var workflowSupportingModulesThatAppearInTree = GetAllWorkflowSupportingModulesThatAppearInTree().ToArray();

			var moduleTree = MasterFilesTestHelper.GetFullyLoadedModuleTree();

			// Act & Assert
			CombineAssertions("Issues found with Workflow Descriptors for ProductivityWise.", () =>
			{
				foreach (var descriptor in WorkflowDescriptors.Instance.Values)
				{
					if (descriptor.Code == WorkflowDescriptors.StandAloneTaskWorkflowDescriptor)
					{
						var taskListModule = moduleTree.FindByID(ModuleIDs.ProcessTasks.Name);
						AssertNotNull(
							$"Standalone Module [{ModuleIDs.ProcessTasks.Name}] should have a matching module within the Fully Loaded Module Tree.",
							taskListModule);
					}
					else if (!WorkflowDescriptors.HRMWorkflowDescriptors().Contains(descriptor.Code))
					{
						var matchingModule = workflowSupportingModulesThatAppearInTree.FirstOrDefault(module => module.WorkflowType == descriptor.Code);
						AssertNotNull(
							$"The workflow type [{descriptor.Code}: {descriptor.Description}] should have at least one matching module in the module tree when ProductivityWise is enabled.",
							matchingModule);
					}
				}
			});
		}

		#region Implementation

		IEnumerable<ZFilterGridModule> GetAllWorkflowSupportingModulesThatAppearInTree()
		{
			var moduleTree = MasterFilesTestHelper.GetFullyLoadedModuleTree();
			var allModules = (
				from ModuleCategory category in moduleTree.Categories.Values
				from ModuleSection section in category.Sections.Values
				from IMainFormModule module in section.Modules.Values
				select ZModuleFactory.Instance.Create(module.ModuleID)
			).ToArray();

			if (disposables == null)
			{
				disposables = new DisposableList(allModules);
			}
			else
			{
				disposables.AddRange(allModules);
			}

			return
				from module in allModules
				let gridModule = module as ZFilterGridModule
				where gridModule != null && gridModule.SupportsWorkflow && !string.IsNullOrEmpty(gridModule.WorkflowType)
				where gridModule.GetType().FullName != "Enterprise.Warehouse.Transactions.Module.WorkOrderModule" // TODO: get Domestic team to fix this. Looks like this module's WorkflowType property is wrong. Need them to verify.
				select gridModule;
		}

		protected override void SetUp()
		{
			base.SetUp();

			disposables = null;
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables?.Dispose();
		}

		DisposableList disposables;

		#endregion
	}
}
