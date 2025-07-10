using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveProcessQueueLookups : IActiveProcessQueueLookups
	{
		public ActiveProcessQueueLookups(ActiveProcessQueue activeProcessQueue)
		{
			this.ActiveProcessQueue = activeProcessQueue;
		}

		#region IActiveProcessQueueLookups Members

		public CodeDescriptionPairList QueueList
		{
			get { return (ActiveProcessQueue.QueueType == ProcessQueueType.Enum.Customs) ? Lookups.CustomsQueueList : Lookups.CommercialQueueList; }
		}

		public CodeDescriptionPairList StatusList
		{
			get { return (ActiveProcessQueue.QueueType == ProcessQueueType.Enum.Customs) ? Lookups.CustomsStatusList : Lookups.CommercialStatusList; }
		}

		public CodeDescriptionPairList SubStatusList
		{
			get { return (ActiveProcessQueue.QueueType == ProcessQueueType.Enum.Customs) ? Lookups.CustomsSubStatusList : Lookups.CommercialSubStatusList; }
		}

		public GlbStaffCollection TaskAssignedToList
		{
			get { return (ActiveProcessQueue.QueueType == ProcessQueueType.Enum.Customs) ? Lookups.CustomsTaskAssignedTos : Lookups.TaskAssignedTos; }
		}

		#endregion

		ProcessQueueLookups Lookups
		{
			get { return ActiveProcessQueue.ProcessQueue.Lookups; }
		}

		readonly ActiveProcessQueue ActiveProcessQueue;
	}
}
