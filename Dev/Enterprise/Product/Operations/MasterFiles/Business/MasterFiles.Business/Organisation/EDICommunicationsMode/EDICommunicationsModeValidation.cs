using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationsModeValidation : AutoEDICommunicationsModeValidation
	{
		public EDICommunicationsModeValidation(AutoEDICommunicationsMode parent)
			: base(parent)
		{
		}

		protected new EDICommunicationsMode Parent => (EDICommunicationsMode)base.Parent;

		protected override void CheckEK_Destination()
		{
			base.CheckEK_Destination();
			switch (Parent.EK_CommunicationsTransport)
			{
				case EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment:
				case EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText:
				case ShipnetExportCommunicationsTransportMappingList.Codes.Email:
					CheckEK_DestinationForEmailAddress();
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.FTP:
					if (Parent.EK_Module != EDICommunicationsMode.Modules.Shipnet)
					{
						CheckEK_DestinationForFTPServerURL(Parent.EK_DestinationInfo);
					}
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile:
					CheckEK_DestinationForFilePath();
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.EHubService:
					CheckEK_DestinationForEHubClientID();
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface:
					CheckEK_DestinationForEAdaptorRecipientID();
					break;
				case EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector:
					CheckEK_DestinationForHTTPServerURL();
					break;
			}

			if (Parent.EK_Module == EDICommunicationsMode.Modules.Shipnet && Parent.EK_CommunicationsTransport != ShipnetExportCommunicationsTransportMappingList.Codes.FTP)
			{
				CheckEK_DestinationForShipnetDestination();
			}
		}

		void CheckEK_DestinationForEmailAddress()
		{
			if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.EK_Destination))
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("afce971b-71f5-40e9-8dc5-21f49b5ea491", "Enter a valid email address"));
			}
		}

		internal void CheckEK_DestinationForFTPServerURL(ZPropertyInfo ftpServerURLRelatedPropertyInfo)
		{
			if (ftpServerURLRelatedPropertyInfo.Value is ZString)
			{
				ZString propertyValue = (ZString)ftpServerURLRelatedPropertyInfo.Value;
				if (!propertyValue.StartsWith("ftp://"))
				{
					ftpServerURLRelatedPropertyInfo.AddError(Res.GetString("11d7a4ae-fcc8-4693-a9c5-0e8ccb0d56b2", "URI must start with {0}", "ftp://"));
				}

				// Let .Net access the quality of the URI:
				try
				{
					Uri uri = new Uri(propertyValue);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ftpServerURLRelatedPropertyInfo.AddError(ex.Message);
				}
			}
			else
			{
				throw new DeveloperNotificationException((NoResString)"This validation is for ZString Property only");
			}
		}

		void CheckEK_DestinationForFilePath()
		{
			if (Parent.EK_Module != EDICommunicationsMode.Modules.Shipnet)
			{
				MandatoryValidation.CheckEntered(Parent.EK_DestinationInfo);
			}
			if (Parent.EK_Destination.ContainsAnyChar(new string(System.IO.Path.GetInvalidPathChars())))
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("7b3127ef-77ed-429b-8325-a82ff68033e9", "Enter a valid directory name"));
			}
		}

		void CheckEK_DestinationForShipnetDestination()
		{
			ZString propertyDescription = ZString.Empty;
			switch (Parent.EK_CommunicationsTransport)
			{
				case ShipnetExportCommunicationsTransportMappingList.Codes.Email:
					propertyDescription = (NoResString)"Email Address";
					break;

				case ShipnetExportCommunicationsTransportMappingList.Codes.FTP:
					propertyDescription = (NoResString)"Destination Directory";
					break;

				case ShipnetExportCommunicationsTransportMappingList.Codes.File:
					propertyDescription = (NoResString)"Destination Directory";
					break;
			}

			if (!propertyDescription.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.EK_DestinationInfo, propertyDescription);
			}
		}

		void CheckEK_DestinationForEHubClientID()
		{
			if (Parent.EK_Destination.Length == 0)
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("6f4c3b6c-cdc3-4829-b01d-91c7ade55434", "Please enter an eHub Client ID."));
			}
			else if (Parent.EK_Destination.Length > 36)
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("53E4456E-51B6-42FC-999B-200003DD3513", "eHub Client ID cannot be longer than 36 characters."));
			}
		}

		void CheckEK_DestinationForEAdaptorRecipientID()
		{
			if (Parent.EK_Destination.Length == 0)
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("17a703d8-c943-4c6f-8231-654390e1c89b", "Please enter an eAdaptor Recipient ID."));
			}
			else if (Parent.EK_Destination.Length > 25)
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("a542caa3-fa21-4101-a548-04a99ebfca50", "eAdaptor Recipient ID cannot be longer than 25 characters."));
			}
		}

		void CheckEK_DestinationForHTTPServerURL()
		{
			var uriString = Parent.EK_Destination;
			if (!Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
			{
				Parent.EK_DestinationInfo.AddError(Res.GetString("ccfff431-fe88-44a4-bbd7-6c9f42616d88", "Destination should be a valid HTTP Address"));
			}
			else
			{
				var uri = new Uri(uriString);
				if (Uri.CheckHostName(uri.Host) == UriHostNameType.Unknown)
				{
					Parent.EK_DestinationInfo.AddError(Res.GetString("cec9dce0-5474-4618-8742-95cc4ef4f2f5", "Unknown host type, please check the address you input"));
				}
				if (!Uri.CheckSchemeName(uri.Scheme))
				{
					Parent.EK_DestinationInfo.AddError(Res.GetString("dda26054-cc7f-43f8-b1ff-2f7f07636153", "Invalid scheme, address need to start with {0} or {1}", "http://", "https://"));
				}
				if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
				{
					Parent.EK_DestinationInfo.AddError(Res.GetString("dda26054-cc7f-43f8-b1ff-2f7f07636153", "Invalid scheme, address need to start with {0} or {1}", "http://", "https://"));
				}
			}
		}

		protected override void CheckEK_LoginName()
		{
			base.CheckEK_LoginName();
			if (Parent.EK_Module == EDICommunicationsMode.Modules.Shipnet &&
				Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.FTP)
			{
				MandatoryValidation.CheckEntered(Parent.EK_LoginNameInfo, Res.GetString("61184024-23ba-482e-a245-d4d759cef4d2", "Username"));
			}
		}

		protected override void CheckEK_PortNumber()
		{
			base.CheckEK_PortNumber();
			if (Parent.EK_Module == EDICommunicationsMode.Modules.Shipnet &&
				Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.FTP)
			{
				MandatoryValidation.CheckEntered(Parent.EK_PortNumberInfo, Res.GetString("7246f161-042d-452b-a39a-268c84b1b224", "Port Number"));
			}
		}

		protected override void CheckEK_Module()
		{
			base.CheckEK_Module();
			if (Parent.EK_ParentTableCode == OrgHeaderSchema.Constants.Prefix)
			{
				ListValidation.ErrorIfInvalidCode(Parent.EK_ModuleInfo);
			}
			if (Parent.EK_Module == EDICommunicationsMode.Modules.ClientSpecific)
			{
				ValidateModuleIsUnique(EDICommunicationsMode.Modules.ClientSpecific, Res.GetString("af9da60b-1470-4927-8bff-52a6740298a3", "A client specific communications mode already exists for this organization"));
			}
			if (Parent.EK_Module == EDICommunicationsMode.Modules.Netting)
			{
				var orgCompanyData = OrgCompanyData.Load(Parent.Factory, Parent.EK_ParentID, GlbCompany.CurrentCompany.PK);
				if (orgCompanyData != null && (!orgCompanyData.OB_IsCreditor || !orgCompanyData.OB_IsDebtor))
				{
					Parent.EK_ModuleInfo.AddError(Res.GetString("be3e40d1-665c-45ec-a570-e5ec1c3e70d5", "An organization participating in netting must be marked as both Payable and Receivable."));
				}
			}
		}

		void ValidateModuleIsUnique(string module, string errorMessage)
		{
			foreach (EDICommunicationsMode mode in Parent.Organisation.EDICommunicationsModes)
			{
				if (Parent != mode && mode.EK_Module == module)
				{
					Parent.EK_ModuleInfo.AddError(errorMessage);
					break;
				}
			}
		}

		protected override void CheckEK_FileFormat()
		{
			base.CheckEK_FileFormat();
			if (Parent.EK_Module != EDICommunicationsMode.Modules.Shipnet)
			{
				ListValidation.ErrorIfInvalidCode(Parent.EK_FileFormatInfo);
			}
			else
			{
				MandatoryValidation.CheckEntered(Parent.EK_FileFormatInfo, Res.GetString("3f5ea39c-271f-4030-b6fb-83fbfdd7d8a1", "valid file extension"));
			}

			if (!CompatibleTransportFileFormat)
			{
				Parent.EK_FileFormatInfo.AddError(EHubFileFormatCompatibilityErrorMessage);
			}
		}

		protected override void CheckEK_CommunicationsTransport()
		{
			base.CheckEK_CommunicationsTransport();
			if (Parent.EK_CommunicationsTransport.Trim().Length == 0)
			{
				Parent.EK_CommunicationsTransportInfo.AddError(Res.GetString("8d44db92-f8fa-42f5-b2c8-825cca6f9498", "Enter a valid selection"));
			}
			else
			{
				if (Parent.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationEmail &&
					Parent.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText)
				{
					Parent.EK_CommunicationsTransportInfo.AddError(Res.GetString("f50420ac-8981-4dd1-87f6-020bb1671194", "Only \"{0} : {1}\" can be selected for \"{2} : {3}\" format.", EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, EDICommunicationsModeCommunicationsTransportList.Descriptions.EmailAsText, EDICommunicationsModeFileFormatList.Codes.NotificationEmail, EDICommunicationsModeFileFormatList.Descriptions.NotificationEmail));
				}

				if (Parent.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail &&
					Parent.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText)
				{
					Parent.EK_CommunicationsTransportInfo.AddError(Res.GetString("f50420ac-8981-4dd1-87f6-020bb1671194", "Only \"{0} : {1}\" can be selected for \"{2} : {3}\" format.", EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, EDICommunicationsModeCommunicationsTransportList.Descriptions.EmailAsText, EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail, EDICommunicationsModeFileFormatList.Descriptions.NotificationBodyEmail));
				}

				if (Parent.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.OrderImportReport &&
					Parent.EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment)
				{
					Parent.EK_CommunicationsTransportInfo.AddError(Res.GetString("f50420ac-8981-4dd1-87f6-020bb1671194", "Only \"{0} : {1}\" can be selected for \"{2} : {3}\" format.", EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, EDICommunicationsModeCommunicationsTransportList.Descriptions.EmailAsAttachment, EDICommunicationsModeFileFormatList.Codes.OrderImportReport, EDICommunicationsModeFileFormatList.Descriptions.OrderImportReport));
				}

				if (!CompatibleTransportDirection)
				{
					Parent.EK_CommunicationsTransportInfo.AddError
					(
						Res.GetString
						(
							"371b0db3-4a41-484f-b3d1-55f435949b72",
							"{0} can only be used with Comm. Direction {1} ({2})",
							Parent.EK_CommunicationsTransport,
							EDICommunicationsModeCommsDirectionList.Codes.Transmit,
							EDICommunicationsModeCommsDirectionList.Descriptions.Transmit
						)
					);
				}

				if (Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector
					&& !Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector)
				{
					Parent.EK_CommunicationsTransportInfo.AddError(DeliveryFactory.NativeXMLConnectorPermissionFailureMessage);
				}

				if (!CompatibleTransportFileFormat)
				{
					Parent.EK_CommunicationsTransportInfo.AddError(EHubFileFormatCompatibilityErrorMessage);
				}

				if (!Parent.IsTransportUniversalXml)
				{
					if (Parent.UniversalDataFileFormats.Contains(Parent.EK_FileFormat))
					{
						Parent.EK_CommunicationsTransportInfo.AddError(Res.GetString("897df382-4cbc-47dd-bbd0-959220258114", "Only [{0}], [{1}] or [{2}] can be selected when using Universal XML.", EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface));
					}
					else if (Parent.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.All)
					{
						Parent.EK_CommunicationsTransportInfo.AddWarning(Res.GetString("DAB6B762-8E59-4CA5-8A40-31095E9D5215", "[{0}] is not supported for Universal XML file formats.", Parent.EK_CommunicationsTransport));
					}
				}

				if (Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface
					&& CompatibleTransportDirection
					&& ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) == null
					&& eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value.IsNullOrEmpty())
				{
					Parent.EK_CommunicationsTransportInfo.AddError(Res.GetString("9fb233be-ad9a-4980-8bf8-4b58a8c86f1a", "Registry: eServices>eAdaptor>Outbound>Service URL must be specified when using EDP Transport"));
				}

				ListValidation.ErrorIfInvalidCode(Parent.EK_CommunicationsTransportInfo);
			}
		}

		IEnumerable<ZString> EHubCompatibleFileFormats
		{
			get
			{
				yield return EDICommunicationsModeFileFormatList.Codes.XML;
				yield return EDICommunicationsModeFileFormatList.Codes.EXL;
				yield return EDICommunicationsModeFileFormatList.Codes.FXL;
				yield return EDICommunicationsModeFileFormatList.Codes.FHL;
				yield return EDICommunicationsModeFileFormatList.Codes.FWB;

				foreach (var universalDataFileFormat in Parent.UniversalDataFileFormats)
				{
					yield return universalDataFileFormat;
				}
			}
		}

		string EHubFileFormatCompatibilityErrorMessage
		{
			get
			{
				return Res.GetString("a0e83844-d7c4-4e54-b0b8-ae4f37b1bb22"
					, "Comm. Transport {0} can only be used with the following File Formats: {1}."
					, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService
					, string.Join(", ", EHubCompatibleFileFormats.ToArray()));
			}
		}

		protected override void CheckEK_ServerAddressSubject()
		{
			base.CheckEK_ServerAddressSubject();
			if (Parent.EK_Module == EDICommunicationsMode.Modules.Shipnet)
			{
				ZString propertyDescription = ZString.Empty;
				switch (Parent.EK_CommunicationsTransport)
				{
					case ShipnetExportCommunicationsTransportMappingList.Codes.Email:
						propertyDescription = (NoResString)"Email Subject";
						break;
					case ShipnetExportCommunicationsTransportMappingList.Codes.FTP:
						propertyDescription = (NoResString)"Server Address";
						break;
				}
				if (!propertyDescription.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.EK_ServerAddressSubjectInfo, propertyDescription);

					if (Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.FTP)
					{
						CheckEK_DestinationForFTPServerURL(Parent.EK_ServerAddressSubjectInfo);
					}
				}
			}
			else if (
				Parent.EK_Module == WorkflowDescriptors.OrderWorkflowDescriptorCode &&
				Parent.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.OrderImportReport &&
				!Parent.EK_ServerAddressSubject.IsEmpty)
			{
				Parent.EK_ServerAddressSubjectInfo.AddError(Res.GetString("a32b79e7-4a67-4698-837e-2c65d3490dc4", "You can't specify a subject for an Order Import Report."));
			}
		}

		protected static Regex FilenameMacross
		{
			get
			{
				if (fFilenameMacross == null)
				{
					fFilenameMacross = new Regex(@"\(\*.*\*\)", RegexOptions.Compiled);
				}
				return fFilenameMacross;
			}
		}
		[ThreadStatic]
		static Regex fFilenameMacross;

		protected override void CheckEK_Filename()
		{
			base.CheckEK_Filename();

			ZString name = Parent.EK_Filename.ToLower();
			if (name.Contains((NoResString)"(*datetime*)") && !FilenameMacross.IsMatch(name.Replace("(*datetime*)", "")))
			{
				Parent.EK_FilenameInfo.AddWarning(Res.GetString("935f1984-bf98-4d24-a0a2-fa428dd50ec6", "When specifying {0}, you must also specify other details about the job otherwise files may be overwritten if several are created at the same time.", "(*DateTime*)"));
			}

			if (Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile ||
				Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.FTP ||
				Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.Email ||
				Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.File)
			{
				string field = Res.GetString("28c2ae04-4559-4eb8-94bd-1ae839c17c6e", "File Name");
				if (Parent.EK_Module == EDICommunicationsMode.Modules.Shipnet)
				{
					field = Res.GetString("75fbc533-d6e1-4796-a7ca-44af5a83f20b", "Export {0}", field);
				}
				MandatoryValidation.CheckEntered(Parent.EK_FilenameInfo, field);
			}
		}

		protected override void CheckEK_Password()
		{
			base.CheckEK_Password();
			if (Parent.EK_Module == EDICommunicationsMode.Modules.Shipnet &&
				Parent.EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.FTP)
			{
				MandatoryValidation.CheckEntered(Parent.EK_PasswordInfo, Res.GetString("aec903a6-a27c-46c8-967d-5f61f3e692c7", "Password"));
			}
		}

		protected override void CheckEK_CommsDirection()
		{
			base.CheckEK_CommsDirection();
			if (!CompatibleTransportDirection)
			{
				Parent.EK_CommsDirectionInfo.AddError
				(
					Res.GetString
					(
						"3441d06e-4258-41b1-b87c-b8ae4fcf994c",
						"Only TRX (Transmit) can be used with Comm. Transport {0}",
						Parent.EK_CommunicationsTransport
					)
				);
			}
		}

		bool CompatibleTransportDirection
		{
			get
			{
				var condition = Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService
					|| Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				var violation = !Parent.EK_CommsDirection.Trim().IsEmpty && Parent.EK_CommsDirection != EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				return !(condition && violation);
			}
		}

		bool CompatibleTransportFileFormat
		{
			get
			{
				var condition = Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				var violation = !(!Parent.EK_FileFormat.IsEmpty && EHubCompatibleFileFormats.Contains(Parent.EK_FileFormat));
				return !(condition && violation);
			}
		}

		protected override void CheckEK_TransportMode()
		{
			base.CheckEK_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.EK_TransportModeInfo);
		}

		protected override void CheckEK_RecipientRole()
		{
			base.CheckEK_RecipientRole();
			ListValidation.ErrorIfInvalidCode(Parent.EK_RecipientRoleInfo);
		}

		protected override void CheckEK_ECC_CommunicationPartyConfig()
		{
			base.CheckEK_ECC_CommunicationPartyConfig();
			if (Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface && ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) != null)
			{
				var communicationPartyConfig = Parent.CommunicationPartyConfig;
				if (Parent.EK_ECC_CommunicationPartyConfig == ZGuid.Empty)
				{
					if (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value.IsNullOrEmpty())
					{
						Parent.EK_ECC_CommunicationPartyConfigInfo.AddError(Res.GetString("8766f6e3-65f9-4ee2-9d36-de570f5f2e57", "An EDI Client or Registry: eServices>eAdaptor>Outbound>Service URL must be specified when using EDP Transport"));
					}
				}
				else if (!(communicationPartyConfig?.IsActive ?? false))
				{
					var notification = Parent.EK_ECC_CommunicationPartyConfigInfo.Notifications.FirstOrDefault(n => n.Message.Contains(Parent.EK_ECC_CommunicationPartyConfigInfo.HumanReadableName.ToString()));
					var inactiveConfigErrorMessage = Res.GetString("FDE37337-EB03-4E30-AE3F-35396FA12695", "EDI Client outbound config must be active");
					if (notification != null)
					{
						Parent.EK_ECC_CommunicationPartyConfigInfo.ReplaceNotification(notification, NotificationType.Error, inactiveConfigErrorMessage);
					}
					else
					{
						Parent.EK_ECC_CommunicationPartyConfigInfo.AddError(inactiveConfigErrorMessage);
					}
				}
			}
		}

		protected override void CheckEK_EventCode()
		{
			base.CheckEK_RecipientRole();
			ListValidation.ErrorIfInvalidCode(Parent.EK_EventCodeInfo);
		}

		protected override void CheckEK_EventReferenceConditionType()
		{
			base.CheckEK_EventReferenceConditionType();
			ListValidation.ErrorIfInvalidCode(Parent.EK_EventReferenceConditionTypeInfo);
		}

		protected override void CheckEK_EventReferenceConditionValue()
		{
			base.CheckEK_EventReferenceConditionValue();
			var type = Parent.EK_EventReferenceConditionType;
			if (Parent.Lookups.EventReferenceTypes.ContainsCode(type))
			{
				CheckReferenceConditionValue(Parent.Factory, type, Parent.EK_EventReferenceConditionValueInfo);
			}
		}

		protected override void CheckEK_MessagePurpose()
		{
			base.CheckEK_MessagePurpose();
			ListValidation.WarnIfInvalidCode(Parent.EK_MessagePurposeInfo, Parent.Lookups.MessagePurposeList, GetInvalidPurposeCodeMessage());
		}

		internal static MultilingualString GetInvalidPurposeCodeMessage() => ResString.GetMultilingualString("d60ce9c6-8b30-4581-aa5e-17a2299683a9", "The value is not found. Please enter a known code or create a new one in the EDI Messaging Module.");

		internal static void CheckReferenceConditionValue(BusinessObjectFactory factory, string code, ZPropertyInfo info)
		{
			switch (code)
			{
				case EventReferenceConditionList.Codes.EventReference:
				case EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions:
				case EventReferenceConditionList.Codes.EventReferenceWithWildcards:
				case EventReferenceConditionList.Codes.EventReferenceParameters:
					TriggerConditionsViewModelValidation.CheckTriggerConditionValue(factory, code, info, () => false);
					break;

				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Unsupported code [{code}]."));
			}
		}
	}
}
