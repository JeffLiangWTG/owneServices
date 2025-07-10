using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	class TelematicsDataMessageProcessor : ITelematicsXmlMessageProcessor
	{
		public TelematicsDataMessageProcessor(ILogger logger)
			: this(logger, new DeviceMessageProcessor(logger, new TelEdgeDbTreePlanner()))
		{
		}

		internal TelematicsDataMessageProcessor(ILogger logger, ITelematicsMessageProcessor<DeviceMessage> deviceMessageProcessor)
		{
			_ = logger ?? throw new ArgumentNullException(nameof(logger));
			this.deviceMessageProcessor = deviceMessageProcessor ?? throw new ArgumentNullException(nameof(deviceMessageProcessor));
		}

		public int Process(BusinessObjectFactory factory, string messageText)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			if (string.IsNullOrEmpty(messageText) ||
				!XmlDataSerializer.TryDeserialize<TelematicsDataMessage>(messageText, out var telematicsDataMessage))
			{
				return 0;
			}

			return telematicsDataMessage
				.DeviceMessages
				.Sum(message => deviceMessageProcessor.Process(factory, message));
		}

		readonly ITelematicsMessageProcessor<DeviceMessage> deviceMessageProcessor;
	}
}
