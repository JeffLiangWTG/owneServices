using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;

namespace Enterprise.Telematics.ServiceTasks.MessageTypeProcessors
{
	public class ProtobufMessageTypeProcessor : IMessageTypeProcessor
	{
		#region Client Extension Hook

		public static void SetClientExtensionHook(IEnumerable<IMessageProcessor> clientSpecificProcessor)
		{
			ClientExtensionProcessor.Value = clientSpecificProcessor;
		}

		static readonly Overridable<IEnumerable<IMessageProcessor>> ClientExtensionProcessor = new Overridable<IEnumerable<IMessageProcessor>>();

		#endregion

		public ProtobufMessageTypeProcessor(ILogger logger, BusinessObjectFactory factory)
		{
			_ = logger ?? throw new ArgumentNullException(nameof(logger));
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var messageProcessors = new List<IMessageProcessor>
			{
				new M2CDeviceAssignedToSystemNotificationProcessor(),
				new W2CDeviceRegistrationResponseMessageProcessor(logger),
				new M2CDeviceRevokedFromSystemNotificationProcessor(),
				new M2CDeviceLocationDataNotificationProcessor(logger),
			};

			var clientExtension = ClientExtensionProcessor.Value;
			if (clientExtension != null)
			{
				messageProcessors.AddRange(clientExtension);
			}

			MessageProcessors = messageProcessors;
		}

		internal IList<IMessageProcessor> MessageProcessors { get; set; }

		public int Process(BusinessObjectFactory factory, string from, string messageText)
		{
			var pushMessagesProcessed = 0;
			var element = XElement.Parse(messageText);
			var innerMessages = EHubMessageSerializer.Deserialize(element);

			foreach (var innerMessage in innerMessages)
			{
				foreach (var processor in MessageProcessors)
				{
					if (processor.MessageType == EHubMessageType.InvalidType || processor.MessageType == innerMessage.message_type)
					{
						processor.Process(factory, from, innerMessage);
					}
				}
				pushMessagesProcessed++;
			}

			return pushMessagesProcessed;
		}

		public string MessageType { get; } = TelematicsMessageList.Codes.ProtobufData;
	}
}
