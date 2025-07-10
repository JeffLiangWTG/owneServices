using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	class UniversalEventTriggerActionBuilder : UniversalXmlTriggerActionBuilder
	{
		public UniversalEventTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
			this.triggeringLogFromQueuedLog = eventInfo.Event;
		}

		readonly BaseStmALog triggeringLogFromQueuedLog;

		protected override sealed string FileFormat
		{
			get { return EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent; }
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalEventDataObjectWriter(outboundSessionManager);
		}

		protected override sealed BusinessObject GetParent()
		{
			return triggeringLogFromQueuedLog;
		}
	}
}
