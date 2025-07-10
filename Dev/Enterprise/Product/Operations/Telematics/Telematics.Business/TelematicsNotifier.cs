using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.Business
{
	public class TelematicsNotifier : ITelematicsNotifier
	{
		public void NotifyMobileServicesOfDeviceDetails(BusinessObjectFactory factory, string mobileServicesClientId, string deviceFriendlyIdentifier, bool isBYOD, string manufacturer, string description, DeviceKind deviceKind, string deviceDrivenIdentifier, string clientLicenceCode)
		{
			var messages = new List<EHubMessageContainer>
			{
				new EHubMessageContainer
				{
					message_type = EHubMessageType.W2MDeviceDetailsUpdatedNotification,
					message_data = new W2MDeviceDetailsUpdatedNotificationMessage
					{
						device_friendly_identifier = deviceFriendlyIdentifier,
						device_kind = deviceKind,
						device_software_readable_identifier = deviceDrivenIdentifier,
						device_manufacturer = manufacturer,
						device_model = description,
						system_licence_code = clientLicenceCode,
						device_client_identifier = description,
						is_byod = isBYOD
					}.Serialize(),
				},
			};

			SaveEHubMessagesToSend(factory, messages, mobileServicesClientId);
		}

		public void NotifyMobileServicesOfNewDeviceParent(BusinessObjectFactory factory, string deviceFriendlyIdentifier, string parentCode, string parentDescription, string parentType)
		{
			var messages = new List<EHubMessageContainer>
			{
				new EHubMessageContainer
				{
					message_type = EHubMessageType.C2WDeviceAssignedToUserNotification,
					message_data = new C2WDeviceAssignedToUserNotificationMessage
					{
						device_friendly_identifier = deviceFriendlyIdentifier,
						parent_code = parentCode,
						parent_description = parentDescription,
						parent_type = parentType,
					}.Serialize(),
				},
			};

			var recipient = SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			SaveEHubMessagesToSend(factory, messages, recipient);
		}

		public void NotifyTelematicsServicesOfDeviceDetails(BusinessObjectFactory factory, IEnumerable<string> telematicsClientIds, string humanReadableIdentifier, string model, string hardwareIdentifier, string deviceWasAssignedTo, string deviceAssignedTo)
		{
			var changeMessage = new DeviceAssignmentChangeMessage();
			if (!deviceAssignedTo.IsNullOrEmpty())
			{
				changeMessage.AssignDevicesToClients = new[]
				{
					new AssignDeviceToClient
					{
						CargoWiseOneLicense = deviceAssignedTo,
						DeviceAssignmentTime = DateTimeOffset.UtcNow,
						DeviceHardwareIdentifier = hardwareIdentifier,
						DeviceHumanReadableIdentifier = humanReadableIdentifier,
						DeviceModel = model,
					}
				}.ToList();
			}

			if (!deviceWasAssignedTo.IsNullOrEmpty())
			{
				changeMessage.RevokeDevicesFromClients = new[]
				{
					new RevokeDeviceFromClient
					{
						CargoWiseOneLicense = deviceWasAssignedTo,
						DeviceHardwareIdentifier = hardwareIdentifier,
						DeviceHumanReadableIdentifier = humanReadableIdentifier,
					}
				}.ToList();
			}

			var message = XmlDataSerializer.SerializeToTelematicsXmlData(changeMessage);

			foreach (var recipient in telematicsClientIds)
			{
				var interchange = factory.New<EDIInterchange>();
				interchange.EI_To = recipient;
				interchange.EI_From = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.Telematics;
				interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.Telematics;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;

				var ediMessage = interchange.ContainedMessages.AddNew();
				ediMessage.EM_ApplicationCode = ApplicationCodeList.Codes.Telematics;
				ediMessage.EM_MessageSubType = TelematicsMessageList.Codes.TelematicsXmlData;
				ediMessage.EM_MessageText = message;
				ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				ediMessage.EM_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				ediMessage.MessageNumberStrategy = new TelematicsMessageNumberStrategy(factory);
			}
		}

		public void NotifyMobileServicesOfBYODRegistration(BusinessObjectFactory factory, string deviceClientIdentifier, string deviceModel, DeviceKind deviceKind, string deviceIdentifier)
		{
			var messages = new List<EHubMessageContainer>
			{
				new EHubMessageContainer
				{
					message_type = EHubMessageType.C2WDeviceRegistrationRequest,
					message_data = new C2WDeviceRegistrationRequestMessage
					{
						device_client_identifier = deviceClientIdentifier,
						device_model = deviceModel,
						device_key = new DeviceKey { kind = deviceKind, identifier = deviceIdentifier },
					}.Serialize(),
				},
			};

			var recipient = SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			SaveEHubMessagesToSend(factory, messages, recipient);
		}

		public void NotifyMobileServicesOfBYODDeregistration(BusinessObjectFactory factory, string deviceClientIdentifier)
		{
			var messages = new List<EHubMessageContainer>
			{
				new EHubMessageContainer
				{
					message_type = EHubMessageType.C2WDeviceDeregistrationRequest,
					message_data = new C2WDeviceDeregistrationRequestMessage
					{
						device_client_identifier = deviceClientIdentifier,
					}.Serialize(),
				},
			};

			var recipient = SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			SaveEHubMessagesToSend(factory, messages, recipient);
		}

		public bool ShouldNotifySynchronously => !isCdcEnabled.Value;

		readonly Lazy<bool> isCdcEnabled = new Lazy<bool>(() => GetIsCdcEnabled(), LazyThreadSafetyMode.PublicationOnly);

		static bool GetIsCdcEnabled()
		{
			var cdcTable = new CdcTable(GlbDeviceAssignmentDivotSchema.Constants.SqlSchemaName, GlbDeviceAssignmentDivotSchema.Constants.TableName);
			return cdcTable.IsCdcEnabled(Db.Connection);
		}

		static void SaveEHubMessagesToSend(BusinessObjectFactory factory, IEnumerable<EHubMessageContainer> messages, string recipient)
		{
			var messageList = messages.ToList();

			while (messageList.Any())
			{
				var serializedBody = EHubMessageSerializer.SerializeToLimit(messageList, EHubMessageSerializer.DefaultMessageSizeLimitInBytes);
				var messageText = serializedBody.ToString();

				var interchange = factory.New<EDIInterchange>();
				interchange.EI_To = recipient;
				interchange.EI_From = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.Telematics;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_BodyText = messageText;

				var message = interchange.ContainedMessages.AddNew();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.Telematics;
				message.EM_MessageSubType = TelematicsMessageList.Codes.ProtobufData;
				message.EM_MessageText = messageText;
				message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message.EM_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				message.MessageNumberStrategy = new TelematicsMessageNumberStrategy(factory);
			}
		}
	}
}
