using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IActiveProcessQueueLookups
	{
		CodeDescriptionPairList QueueList { get; }
		CodeDescriptionPairList StatusList { get; }
		CodeDescriptionPairList SubStatusList { get; }
		GlbStaffCollection TaskAssignedToList { get; }
	}
}
