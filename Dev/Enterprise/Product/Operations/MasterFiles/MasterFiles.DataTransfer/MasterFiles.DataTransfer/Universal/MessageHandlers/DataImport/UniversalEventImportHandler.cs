using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalEventImportHandler : UniversalXmlImportHandler<UniversalEvent>
	{
		public UniversalEventImportHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalEventImportHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalEvent;
	}
}
