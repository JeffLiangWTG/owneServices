using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;

namespace Enterprise.Telematics.ServiceTasks.MessageTypeProcessors
{
	class TelematicsXmlMessageTypeProcessor : IMessageTypeProcessor
	{
		public TelematicsXmlMessageTypeProcessor(ILogger logger)
			: this(
				logger,
				ObjectFactory.Get<ITelematicsXmlMessageTypeProcessorsFactory>().GetProcessors(logger).ToArray()
			)
		{
		}

		internal TelematicsXmlMessageTypeProcessor(ILogger logger, params ITelematicsXmlMessageProcessor[] telematicsXmlMessageProcessors)
		{
			_ = logger ?? throw new ArgumentNullException(nameof(logger));

			var firstNullProcessor = (telematicsXmlMessageProcessors ?? throw new ArgumentNullException(nameof(telematicsXmlMessageProcessors)))
				.Select((processor, i) => new { index = i + 1, processor })
				.Where(arg => arg.processor == null)
				.Select(arg => arg.index)
				.FirstOrDefault();
			if (firstNullProcessor > 0)
			{
				throw new ArgumentNullException(FormattableString.Invariant($"{nameof(telematicsXmlMessageProcessors)}[{firstNullProcessor - 1}]"));
			}

			this.telematicsXmlMessageProcessors = telematicsXmlMessageProcessors;
		}

		public string MessageType { get; } = TelematicsMessageList.Codes.TelematicsXmlData;

		public int Process(BusinessObjectFactory factory, string from, string messageText)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));
			_ = messageText ?? throw new ArgumentNullException(nameof(messageText));

			if (string.IsNullOrWhiteSpace(messageText))
			{
				return 0;
			}

			var element = XElement.Parse(messageText);
			var message = element.Nodes().SingleOrDefault()?.ToString();
			return telematicsXmlMessageProcessors.Sum(processor => processor.Process(factory, message));
		}

		readonly ITelematicsXmlMessageProcessor[] telematicsXmlMessageProcessors;
	}
}
