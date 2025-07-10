using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business
{
	sealed class UniversalTransactionBatchTriggerActionBuilder : UniversalXmlTriggerActionBuilder
	{
		public UniversalTransactionBatchTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
			: base(descriptor, actionInfo, eventInfo)
		{
		}

		protected override string FileFormat
		{
			get { return EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch; }
		}

		protected override ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager)
		{
			return Descriptor.GetUniversalTransactionBatchDataObjectWriter(outboundSessionManager);
		}

		protected override BusinessObject GetParent()
		{
			return ActionInfo.ParentBO;
		}
	}
}
