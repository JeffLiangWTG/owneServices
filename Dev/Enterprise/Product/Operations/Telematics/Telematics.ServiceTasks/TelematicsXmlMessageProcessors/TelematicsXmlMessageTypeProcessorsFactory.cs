using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	public class TelematicsXmlMessageTypeProcessorsFactory : ITelematicsXmlMessageTypeProcessorsFactory
	{
		public void AddProcessorFunction(Func<ILogger, ITelematicsXmlMessageProcessor> func)
		{
			processors.Add(func ?? throw new ArgumentNullException(nameof(func)));
		}

		public IEnumerable<ITelematicsXmlMessageProcessor> GetProcessors(ILogger logger)
		{
			_ = logger ?? throw new ArgumentNullException(nameof(logger));

			return processors.Select(func => func(logger));
		}

		readonly List<Func<ILogger, ITelematicsXmlMessageProcessor>> processors = new List<Func<ILogger, ITelematicsXmlMessageProcessor>>
		{
			logger => new TelematicsDataMessageProcessor(logger),
			logger => new TcaRegistrationMessageProcessor(logger),
		};
	}
}
