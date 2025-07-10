using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public interface IWorkflowConditionValueParentCache
	{
		ZString GetConditionValue(ProcessTask task);
	}
}
