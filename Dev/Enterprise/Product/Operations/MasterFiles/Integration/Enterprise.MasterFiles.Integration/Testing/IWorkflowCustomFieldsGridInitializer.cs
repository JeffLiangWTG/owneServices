using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowCustomFieldsGridInitializer
	{
		void AddWorkflowCustomFieldsColumns(
			object grid,
			IBusinessObjectCollection collection,
			string workflowType,
			bool isVisible = false,
			bool isReadonly = false,
			ZGuid company = default(ZGuid),
			ZGuid branch = default(ZGuid),
			ZGuid department = default(ZGuid));
	}
}
