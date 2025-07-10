using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	class ISFMessageProcessorFactory : MessageProcessorFactory, IISFMessageProcessorFactory
	{
		public ISFMessageProcessorFactory(LoggingInformation logger)
			: base(logger, CBPEDIInterchange.ApplicationCodes.USCustomsImport, "US Customs ISF Message Processor")
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => GetImporterSecurityFilingMessageTypes();
	}
}
