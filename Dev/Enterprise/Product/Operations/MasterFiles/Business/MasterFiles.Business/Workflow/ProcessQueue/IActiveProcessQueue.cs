
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IActiveProcessQueue : IBusiness
	{
		ZString QueueName { get; set; }
		ZPropertyInfo QueueNameInfo { get; }
		MultilingualString QueueNameCaption { get; }
		ZPropertyInfo QueueNameCaptionInfo { get; }

		ZString Status { get; set; }
		ZPropertyInfo StatusInfo { get; }
		MultilingualString StatusCaption { get; }
		ZPropertyInfo StatusCaptionInfo { get; }

		ZString SubStatus { get; set; }
		ZPropertyInfo SubStatusInfo { get; }
		MultilingualString SubStatusCaption { get; }
		ZPropertyInfo SubStatusCaptionInfo { get; }
		bool HasSubStatuses { get; }

		ZString AssignedTo { get; set; }
		ZPropertyInfo AssignedToInfo { get; }
		MultilingualString AssignedToCaption { get; }
		ZPropertyInfo AssignedToCaptionInfo { get; }

		ZString Reason { get; set; }
		ZPropertyInfo ReasonInfo { get; }
		MultilingualString ReasonCaption { get; }
		ZPropertyInfo ReasonCaptionInfo { get; }

		ZString P4_CustomAttrib8 { get; set; }
		ZPropertyInfo P4_CustomAttrib8Info { get; }

		ZDateTime P4_CustomDate4 { get; set; }
		ZPropertyInfo P4_CustomDate4Info { get; }

		IActiveProcessQueueLookups Lookups { get; }
	}
}
