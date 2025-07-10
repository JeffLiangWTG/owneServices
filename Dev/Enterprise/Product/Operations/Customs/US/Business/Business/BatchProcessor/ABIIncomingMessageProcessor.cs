using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.Business
{
	public class ABIIncomingMessageProcessor : IncomingMessageProcessor
	{
		protected override ZQuery GetQueuedQuery() => AddNotInEDIMessageQueueStateQuery(base.GetQueuedQuery());

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new ABIMessageProcessorFactory(Logger));
			return result;
		}

		public static IReadOnlyList<ZString> GetMessageTypesToExclude() => ABIMessageProcessorFactory.GetMessageTypesToExclude();
	}
}
