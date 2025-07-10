using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	sealed class UniversalEventCollectionTriggerActionBuilder : UniversalEventTriggerActionBuilder
	{
		public UniversalEventCollectionTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalEventCollectionDataObjectWriter(outboundSessionManager);
		}
	}
}
