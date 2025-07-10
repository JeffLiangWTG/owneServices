using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.Business
{
	public class USRIncomingMessageProcessor : IncomingMessageProcessor
	{
		protected override ZQuery GetQueuedQuery() => AddNotInEDIMessageQueueStateQuery(base.GetQueuedQuery());

		public static ZString[] GetMessageTypesToInclude() => USRMessageProcessorFactory.GetReferenceFileMessageTypes();

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new USRMessageProcessorFactory(Logger));
			return result;
		}
	}
}
