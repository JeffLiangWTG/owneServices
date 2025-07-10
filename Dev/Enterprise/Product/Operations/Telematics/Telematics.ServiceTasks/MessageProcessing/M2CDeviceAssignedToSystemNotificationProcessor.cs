using System;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Telematics.ServiceTasks.MessageProcessing
{
	class M2CDeviceAssignedToSystemNotificationProcessor : IMessageProcessor
	{
		#region IMessageProcessor

		public EHubMessageType MessageType
		{
			get { return EHubMessageType.M2CDeviceAssignedToSystemNotification; }
		}

		public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer)
		{
			var message = messageContainer.GetInternalMessage<M2CDeviceAssignedToSystemNotificationMessage>();
			if (message.is_byod)
			{
				var byod = GlbDevice.FindBYODByHumanReadableIdentifier(factory, message.device_client_identifier);
				if (byod != null)
				{
					RegisterBYOD(byod, message.device_identifier);
				}
			}
			else
			{
				var device = GlbDevice.FindDeviceByMobileServicesIdentifier(factory, message.device_identifier);

				if (device == null)
				{
					device = factory.New<GlbDevice>();
					device.V3_MobileServicesIdentifier = message.device_identifier;
				}

				if (device.V3_HumanReadableIdentifier != message.device_friendly_identifier)
				{
					device.V3_HumanReadableIdentifier = message.device_friendly_identifier;
				}

				if (device.V3_Model != message.device_model)
				{
					device.V3_Model = message.device_model;
				}

				device.V3_IsActive = true;

				device.UpdateDeviceKey(message.device_key);

				if (!device.IsInDatabase)
				{
					factory.Save();
				}
			}
		}

		void RegisterBYOD(GlbDevice device, byte[] mobileServicesIdentifier)
		{
			if (device.V3_Status != GlbDeviceStatusList.Codes.PendingRegistration)
			{
				var conflictNote = device.Notes.AddNew();
				conflictNote.ST_NoteType = StmNoteDescription.Pub;
				conflictNote.ST_Description = (NoResString)"Unexpected Device Status";
				conflictNote.ST_NoteDataAsText = FormattableString.Invariant($"BYOD status expected to be '{GlbDeviceStatusList.Descriptions.PendingRegistration}' but was '{new GlbDeviceStatusList().GetDescriptionFromCode(device.V3_Status)}' instead.");
			}
			device.V3_MobileServicesIdentifier = mobileServicesIdentifier;
			device.V3_Status = GlbDeviceStatusList.Codes.Registered;
			device.Factory.Save();
		}

		#endregion
	}
}
