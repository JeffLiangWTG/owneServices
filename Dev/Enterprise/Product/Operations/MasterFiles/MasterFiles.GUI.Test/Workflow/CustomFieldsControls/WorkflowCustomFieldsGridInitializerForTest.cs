using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowCustomFieldsGridInitializerForTest : IWorkflowCustomFieldsGridInitializer
	{
		public void AddWorkflowCustomFieldsColumns(
			object grid,
			IBusinessObjectCollection collection,
			string workflowType,
			bool isVisible = false,
			bool isReadonly = false,
			ZGuid company = new ZGuid(),
			ZGuid branch = new ZGuid(),
			ZGuid department = new ZGuid())
		{
			WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns((ZGrid)grid, collection, workflowType, isVisible, isReadonly, company, branch, department);
		}
	}
}
