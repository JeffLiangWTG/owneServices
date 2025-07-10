using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	public sealed class UniversalShipmentTriggerActionBuilder : UniversalXmlTriggerActionBuilder
	{
		public UniversalShipmentTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override string FileFormat
		{
			get { return EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment; }
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalShipmentDataObjectWriter(outboundSessionManager);
		}

		protected override BusinessObject GetParent()
		{
			return ActionInfo.ParentBO;
		}
	}
}
