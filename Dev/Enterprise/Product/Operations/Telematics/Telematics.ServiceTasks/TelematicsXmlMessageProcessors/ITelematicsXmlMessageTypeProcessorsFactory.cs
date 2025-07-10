using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	public interface ITelematicsXmlMessageTypeProcessorsFactory
	{
		IEnumerable<ITelematicsXmlMessageProcessor> GetProcessors(ILogger logger);
		void AddProcessorFunction(Func<ILogger, ITelematicsXmlMessageProcessor> func);
	}
}
