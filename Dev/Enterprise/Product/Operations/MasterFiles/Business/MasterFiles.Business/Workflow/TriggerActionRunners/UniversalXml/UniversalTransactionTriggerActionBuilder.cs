using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	sealed class UniversalTransactionTriggerActionBuilder : UniversalXmlTriggerActionBuilder
	{
		public UniversalTransactionTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override string FileFormat
		{
			get { return EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction; }
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalTransactionDataObjectWriter(outboundSessionManager);
		}

		protected override BusinessObject GetParent()
		{
			return ActionInfo.ParentBO;
		}
	}
}
