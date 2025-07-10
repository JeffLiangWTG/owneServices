using System;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Telematics.ServiceTasks.MessageProcessing
{
	class W2CDeviceRegistrationResponseMessageProcessor : IMessageProcessor
	{
		public W2CDeviceRegistrationResponseMessageProcessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public EHubMessageType MessageType => EHubMessageType.W2CDeviceRegistrationResponse;

		public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer)
		{
			var message = messageContainer.GetInternalMessage<W2CDeviceRegistrationResponseMessage>();
			var byod = GlbDevice.FindBYODByHumanReadableIdentifier(factory, message.device_client_identifier);
			if (byod != null)
			{
				switch (message.error_code)
				{
					case DeviceRegistrationRequestError.Duplicated:
						byod.V3_Status = GlbDeviceStatusList.Codes.RegistrationFailure;
						AddException(byod, (NoResString)"Registration Failure", (NoResString)"BYOD with this client identifier is already registered.");
						break;
					case DeviceRegistrationRequestError.DetailsChanged:
						byod.V3_Status = GlbDeviceStatusList.Codes.RegistrationFailure;
						AddException(byod, (NoResString)"Registration Failure", (NoResString)"BYOD details have changed which is not allowed. Deregister this device and create a new BYOD.");
						break;
					case DeviceRegistrationRequestError.NotFound:
						byod.V3_Status = GlbDeviceStatusList.Codes.DeregistrationFailure;
						AddException(byod, (NoResString)"De-registration Failure", (NoResString)"Unable to deregister BYOD - device with this client identifier is not registered.");
						break;
				}

				factory.Save();
			}
			else
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Message recieved for BYOD with a client identifier '{message.device_client_identifier}' but such device could not be found."));
			}
		}

		void AddException(GlbDevice byod, string customDescription, string text)
		{
			var note = byod.Notes.AddNew();
			note.ST_NoteType = StmNoteDescription.Pub;
			note.ST_Description = customDescription;
			note.ST_NoteDataAsText = text;
		}

		readonly ILogger logger;
	}
}
