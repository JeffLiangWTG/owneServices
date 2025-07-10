using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public static class DeliveryFactory
	{
		public static IDelivery Build(IEDICommunicationsMode mode)
		{
			Argument.NotNull(mode, "IEDICommunicationsMode");
			return GetDeliveryByMode(mode);
		}

		static IDelivery GetDeliveryByMode(IEDICommunicationsMode mode)
		{
			IDelivery result = null;
			switch (mode.EK_CommunicationsTransport)
			{
				case EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment:
					result = new EmailAttachmentDelivery();
					break;

				case EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText:
					result = new EmailDelivery();
					break;

				case EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile:
					if (Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector)
					{
						result = new FileDelivery();
						break;
					}
					throw new NotAllowedException(SaveToFileInterfaceConnectorPermissionFailureMessage + String.Format(CultureInfo.InvariantCulture, "[{0} - {1}].", mode.Organisation.OH_Code, mode.Organisation.OH_FullName));

				case EDICommunicationsModeCommunicationsTransportList.Codes.FTP:
					if (Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector)
					{
						result = new FtpDelivery();
						break;
					}
					throw new NotAllowedException(FTPInterfaceConnectorPermissionFailureMessage + String.Format(CultureInfo.InvariantCulture, "[{0} - {1}].", mode.Organisation.OH_Code, mode.Organisation.OH_FullName));

				case EDICommunicationsModeCommunicationsTransportList.Codes.EHubService:
					result = new EHubDelivery
					{
						ErrorNotifier = new EmailNotifier(),
					};
					break;

				case EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector:
					if (Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector)
					{
						result = ObjectFactory.Get<IDelivery>("NativeWebServiceDelivery");
						break;
					}
					throw new NotAllowedException(NativeXMLConnectorPermissionFailureMessage + String.Format(CultureInfo.InvariantCulture, "[{0} - {1}].", mode.Organisation.OH_Code, mode.Organisation.OH_FullName));

				case EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface:
					result = new EAdaptorDelivery
					{
						ErrorNotifier = new EmailNotifier(),
					};
					break;

				case EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface:
					result = new XTDelivery();
					break;
				default:
					throw new NotSupportedException();
			}

			return result;
		}

		internal static string NativeXMLConnectorPermissionFailureMessage
		{
			get { return Res.GetString("daa2324b-f6a8-40c1-894f-0996b961d440", "Cannot Send Via Native XML Connector as it is not enabled on this system. Please check the EDI Communications Modes configured on the Organization "); }
		}

		static string FTPInterfaceConnectorPermissionFailureMessage
		{
			get { return Res.GetString("9bf7279c-280b-40c0-a8cf-d1a7ac4b49d2", "Cannot Send Via FTP as this system does not have an Interface Connector License. Please check the EDI Communications Modes configured on the Organization "); }
		}

		static string SaveToFileInterfaceConnectorPermissionFailureMessage
		{
			get { return Res.GetString("70566b0c-e502-4772-9b74-2f5d4d905022", "Cannot Save to File as this system does not have an Interface Connector License. Please check the EDI Communications Modes configured on the Organization "); }
		}
	}
}
