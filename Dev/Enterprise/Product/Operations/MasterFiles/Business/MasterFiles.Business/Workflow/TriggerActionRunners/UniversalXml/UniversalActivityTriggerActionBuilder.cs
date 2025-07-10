using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	sealed class UniversalActivityTriggerActionBuilder : UniversalXmlTriggerActionBuilder
	{
		public UniversalActivityTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override string FileFormat => EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity;

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalActivityDataObjectWriter(outboundSessionManager);
		}

		protected override BusinessObject GetParent()
		{
			return ActionInfo.ParentBO;
		}
	}
}
