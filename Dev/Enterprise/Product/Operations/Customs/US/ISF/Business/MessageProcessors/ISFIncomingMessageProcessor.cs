using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFIncomingMessageProcessor : IncomingMessageProcessor
	{
		public static ZString[] GetMessageTypesToInclude() => ISFMessageProcessorFactory.GetImporterSecurityFilingMessageTypes();

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new ISFMessageProcessorFactory(Logger));
			return result;
		}
	}
}
