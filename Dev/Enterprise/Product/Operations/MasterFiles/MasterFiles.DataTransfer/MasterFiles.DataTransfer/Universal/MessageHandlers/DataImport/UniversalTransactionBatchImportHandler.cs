using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalTransactionBatchImportHandler : UniversalXmlImportHandler<TransactionBatch>
	{
		public UniversalTransactionBatchImportHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalTransactionBatchImportHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
	}
}
