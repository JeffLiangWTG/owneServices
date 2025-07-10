using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal class EIDOBaseMessageProcessor : BaseMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = new List<ApplicationTypeMessageProcessor>();
			result.Add(new EIDOTypeMessageProcessor(Logger));
			return result;
		}
	}
}


