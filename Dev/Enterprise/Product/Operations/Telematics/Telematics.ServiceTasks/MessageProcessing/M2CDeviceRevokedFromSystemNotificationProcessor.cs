using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Telematics.Business;

namespace Enterprise.Telematics.ServiceTasks.MessageProcessing
{
	class M2CDeviceRevokedFromSystemNotificationProcessor : IMessageProcessor
	{
		#region IMessageProcessor

		public EHubMessageType MessageType
		{
			get { return EHubMessageType.M2CDeviceRevokedFromSystemNotification; }
		}

		public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer)
		{
			var message = messageContainer.GetInternalMessage<M2CDeviceRevokedFromSystemNotificationMessage>();

			var device = GlbDevice.FindDeviceByMobileServicesIdentifier(factory, message.device_identifier);
			if (device == null)
			{
				return;
			}

			if (device.V3_IsBYOD)
			{
				device.V3_Status = GlbDeviceStatusList.Codes.Unregistered;
			}
			else
			{
				device.V3_IsActive = false;
			}
		}

		#endregion
	}
}
