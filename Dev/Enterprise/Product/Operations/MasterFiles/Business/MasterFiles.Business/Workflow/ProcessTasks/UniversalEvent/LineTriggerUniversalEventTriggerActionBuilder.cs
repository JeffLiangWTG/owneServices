using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	class LineTriggerUniversalEventTriggerActionBuilder : UniversalEventTriggerActionBuilder
	{
		public LineTriggerUniversalEventTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo) : base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			var result = (IEventDataObjectWriter)base.GetTopLevelDataObjectWriter(outboundSessionManager);
			result.PopulateAdditionalContexts = true;
			return result;
		}
	}
}
