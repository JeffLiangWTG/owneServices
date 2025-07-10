using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class WorkflowDescriptorListWithStandaloneTaskType : WorkflowDescriptorList, IWorkflowDescriptorListWithStandaloneTaskType
	{
		public WorkflowDescriptorListWithStandaloneTaskType()
		{
			AddPair(WorkflowDescriptors.StandAloneTaskWorkflowDescriptor, ResString.GetMultilingualString("c56ea0a9-b8e4-4de6-bef1-a19a7c61e881", "Standalone Tasks"));
			Sort();
		}
	}
}
