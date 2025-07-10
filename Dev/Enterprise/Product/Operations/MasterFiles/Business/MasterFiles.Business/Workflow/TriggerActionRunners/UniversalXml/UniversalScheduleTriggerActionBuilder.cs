using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	sealed class UniversalScheduleTriggerActionBuilder : UniversalXmlTriggerActionBuilder
	{
		public UniversalScheduleTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override string FileFormat
		{
			get { return EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule; }
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalScheduleDataObjectWriter(outboundSessionManager);
		}

		protected override BusinessObject GetParent()
		{
			return ActionInfo.ParentBO;
		}
	}
}
