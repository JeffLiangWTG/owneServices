using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	class ABIMessageProcessorFactory : MessageProcessorFactory
	{
		public ABIMessageProcessorFactory(LoggingInformation logger)
			: base(logger, EDIMessage.ApplicationCodes.USCustomsImport, "US Customs ABI Message Processor")
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToExcludeCore => GetMessageTypesToExclude();

		internal static IReadOnlyList<ZString> GetMessageTypesToExclude() => GetImporterSecurityFilingMessageTypes().Concat(GetReferenceFileMessageTypes()).ToArray();
	}
}
