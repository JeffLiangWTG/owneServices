using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	public class TcaRegistrationMessageProcessor : ITelematicsXmlMessageProcessor
	{
		public TcaRegistrationMessageProcessor(ILogger logger)
			: this(logger, new RimRegistrationMessageProcessor(logger), new RevokeRimRegistrationMessageProcessor(logger))
		{
		}

		internal TcaRegistrationMessageProcessor(
			ILogger logger,
			ITcaRegistrationProcessor<RimRegistrationMessage> rimRegistrationMessageProcessor,
			ITcaRegistrationProcessor<RevokeRimRegistrationMessage> revokeRimRegistrationMessageProcessor)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.rimRegistrationMessageProcessor = rimRegistrationMessageProcessor ?? throw new ArgumentNullException(nameof(rimRegistrationMessageProcessor));
			this.revokeRimRegistrationMessageProcessor = revokeRimRegistrationMessageProcessor ?? throw new ArgumentNullException(nameof(revokeRimRegistrationMessageProcessor));
		}

		public int Process(BusinessObjectFactory factory, string messageText)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			if (string.IsNullOrEmpty(messageText))
			{
				return 0;
			}

			if (XmlDataSerializer.TryDeserialize<RimRegistrationMessage>(messageText, out var registerMessage))
			{
				return rimRegistrationMessageProcessor.Process(factory, registerMessage);
			}
			if (XmlDataSerializer.TryDeserialize<RevokeRimRegistrationMessage>(messageText, out var revokeMessage))
			{
				return revokeRimRegistrationMessageProcessor.Process(factory, revokeMessage);
			}

			return 0;
		}

		protected readonly ILogger logger;
		readonly ITcaRegistrationProcessor<RimRegistrationMessage> rimRegistrationMessageProcessor;
		readonly ITcaRegistrationProcessor<RevokeRimRegistrationMessage> revokeRimRegistrationMessageProcessor;
	}
}
