using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowDescriptorLoader
	{
		IWorkflowDescriptor GetWorkflowDescriptor(ZString workflowType);
	}
}
