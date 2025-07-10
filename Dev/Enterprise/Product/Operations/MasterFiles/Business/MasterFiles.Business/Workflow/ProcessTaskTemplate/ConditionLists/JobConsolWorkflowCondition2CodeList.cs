using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobConsolWorkflowCondition2CodeList : AutoJobConsolWorkflowCondition2CodeList
	{
		public JobConsolWorkflowCondition2CodeList()
		{
			AddPair("", "");
			AddRange(new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode));
			RemoveCode(Core.Constants.ContainerModes.Other);
		}
	}
}
