using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class EDICommunicationsMode : AutoEDICommunicationsMode, IEDICommunicationsMode, IDataVersionLoggingSupported
	{
		public EDICommunicationsMode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Modules
		{
			public const string Shipnet = "SHN";
			public const string US_BIRD = "BRD";
			public const string ContainerMovements = "CMM";
			public const string ClientSpecific = "CLI";
			public const string Netting = "NET";
			public const string CreditControlledDocumentApproval = "CCA";
			public const string CredentialSettingConfigurationManagement = "CSC";
			public const string GlobalElectronicInvoicing = "GEI";
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			EK_LastFailed = ZDateTime.Empty;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("2ae56cc9-07e2-4d03-906e-93ce09d4fb53", "{0} - {1} - {2} - {3}", Organisation.OH_Code, EK_Module, EK_CommsDirection, EK_CommunicationsTransport);

		protected override ZString HumanReadableShortcutNameCore => Res.GetString("da744703-ce0a-4fc0-a2b2-27be575221fb", "{0} - {1} - {2} - {3}", Organisation.OH_Code, EK_Module, EK_CommsDirection, EK_CommunicationsTransport);

		public ZString EK_OH_Code => Organisation?.OH_Code ?? ZString.Empty;

		public ZString EK_OH_FullName => Organisation?.OH_FullName ?? ZString.Empty;

		[List("Lookups.CommsDirectionList")]
		public override ZString EK_CommsDirection
		{
			get
			{
				return base.EK_CommsDirection;
			}
			set
			{
				base.EK_CommsDirection = value;
				Validation.ValidateEK_CommunicationsTransport();
			}
		}

		[List("Lookups.CommunicationsTransportList")]
		public override ZString EK_CommunicationsTransport
		{
			get { return base.EK_CommunicationsTransport; }
			set
			{
				base.EK_CommunicationsTransport = value;
				if (EK_Module != Modules.Shipnet)
				{
					Validation.ValidateEK_Destination();
				}

				#region Logic for View

				if (value == EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector
					|| value == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)
				{
					EK_Filename = ZString.Empty;
					EK_ServerAddressSubject = ZString.Empty;
					if (EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XML
						&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent
						&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment
						&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
						&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule
						&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity
						&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch)
					{
						EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
					}
				}

				if (value != EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)
				{
					EK_ECC_CommunicationPartyConfig = ZGuid.Empty;
				}

				EK_FilenameInfo.RefreshBinding();
				EK_ServerAddressSubjectInfo.RefreshBinding();
				EK_FtpLockingMethodInfo.RefreshBinding();
				EK_PortNumberInfo.RefreshBinding();
				EK_LoginNameInfo.RefreshBinding();
				EK_PasswordInfo.RefreshBinding();
				EK_CertificateInfo.RefreshBinding();

				Validation.ValidateEK_ServerAddressSubject();
				Validation.ValidateEK_Filename();

				Validation.ValidateEK_FileFormat();
				Validation.ValidateEK_CommsDirection();

				Validation.ValidateEK_ECC_CommunicationPartyConfig();

				#endregion
			}
		}

		protected virtual bool IsTransportWebService
		{
			get
			{
				return EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService
					|| EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface
					|| EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			}
		}

		[ReadOnlyMember(nameof(IsNonFTPCommunicationsTransport))]
		public override ZString EK_FtpLockingMethod
		{
			get { return base.EK_FtpLockingMethod; }
			set { base.EK_FtpLockingMethod = value; }
		}

		[ReadOnlyMember(nameof(IsNonFTPCommunicationsTransport))]
		public override ZInt EK_PortNumber
		{
			get { return base.EK_PortNumber; }
			set { base.EK_PortNumber = value; }
		}

		[ReadOnlyMember(nameof(IsNonFTPCommunicationsTransport))]
		public override ZString EK_LoginName
		{
			get { return base.EK_LoginName; }
			set { base.EK_LoginName = value; }
		}

		[List("Lookups.ModuleList")]
		public override ZString EK_Module
		{
			get { return base.EK_Module; }
			set
			{
				base.EK_Module = value;

				if (EK_Module == Modules.US_BIRD)
				{
					EK_Filename = EDIMessageDelivery.ReplacementConstants.JobNumber + "_" + EDIMessageDelivery.ReplacementConstants.DateTime + "_" + EDIMessageDelivery.ReplacementConstants.Type + ".txt";
				}
			}
		}

		[List("Lookups.Organisations")]
		public override ZGuid EK_OH_MessageVAN
		{
			get { return base.EK_OH_MessageVAN; }
			set { base.EK_OH_MessageVAN = value; }
		}

		[List("Lookups.MessagePurposeList")]
		public override ZString EK_MessagePurpose
		{
			get { return base.EK_MessagePurpose; }
			set { base.EK_MessagePurpose = value; }
		}

		[ReadOnlyMember(nameof(IsNonFTPCommunicationsTransport))]
		public override ZString EK_Password
		{
			get { return base.EK_Password; }
			set { base.EK_Password = value; }
		}

		[ReadOnlyMember(nameof(IsNonFTPCommunicationsTransport))]
		public override ZBlob EK_Certificate
		{
			get { return base.EK_Certificate; }
			set { base.EK_Certificate = value; }
		}

		protected bool IsNonFTPCommunicationsTransport
		{
			get { return EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.FTP; }
		}

		[List("Lookups.FileFormatList")]
		public override ZString EK_FileFormat
		{
			get { return base.EK_FileFormat; }
			set
			{
				base.EK_FileFormat = value;
				Validation.ValidateEK_CommunicationsTransport();
			}
		}

		public ZBool IsFileFormatSupported(ZString fileFormat)
		{
			var fileFormatMatches = fileFormat.EqualsIgnoringCase(EK_FileFormat) || EK_FileFormat.EqualsIgnoringCase(EDICommunicationsModeFileFormatList.Codes.All);

			if (fileFormatMatches && UniversalDataFileFormats.Contains(fileFormat))
			{
				return IsTransportUniversalXml;
			}
			else
			{
				return fileFormatMatches;
			}
		}

		public bool IsTransportUniversalXml
		{
			get
			{
				return EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService
					|| EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface
					|| EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;
			}
		}

		public IEnumerable<ZString> UniversalDataFileFormats
		{
			get
			{
				yield return EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				yield return EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				yield return EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				yield return EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule;
				yield return EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch;
				yield return EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity;
			}
		}

		[ReadOnlyMember(nameof(IsEK_FilenameReadOnly))]
		public override ZString EK_Filename
		{
			get { return base.EK_Filename; }
			set { base.EK_Filename = value; }
		}

		IOrgHeader IMessageDestinationSource.Organisation => Organisation;

		public OrgHeader Organisation
		{
			get
			{
				var org = (OrgHeader)Factory.Load(typeof(OrgHeader), EK_ParentID);
				if (org == null)
				{
					var companyData = Factory.Load<OrgCompanyData>(EK_ParentID);
					if (companyData != null)
					{
						org = companyData.Organisation;
					}
				}
				return org;
			}
		}

		protected bool EK_ServerAddressSubject_ReadOnly
		{
			get
			{
				return
					!(
						EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment ||
						EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText ||
						EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.Email ||
						(EK_Module == Modules.Shipnet && EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.FTP) ||
						(
							IsTransportWebService
							&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.FHL
							&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.FWB
						)
					)
					|| EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			}
		}

		protected bool IsEK_FilenameReadOnly
		{
			get
			{
				return
					!(
						EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile ||
						EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment ||
						EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.FTP ||
						EK_CommunicationsTransport == ShipnetExportCommunicationsTransportMappingList.Codes.Email ||
						(
							IsTransportWebService
							&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.FHL
							&& EK_FileFormat != EDICommunicationsModeFileFormatList.Codes.FWB
						)
					)
					|| EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
			}
		}

		protected bool EK_LocalPartyVanID_ReadOnly
		{
			get
			{
				return IsTransportWebService;
			}
		}

		protected bool EK_RelatedPartyVanID_ReadOnly
		{
			get
			{
				return IsTransportWebService;
			}
		}

		protected bool EK_MessagePurpose_ReadOnly
		{
			get
			{
				return false;
			}
		}

		protected bool IsCommunicationPartyReadOnly
		{
			get => EK_CommunicationsTransport != EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
		}

		[List("Lookups.TransportModes")]
		public override ZString EK_TransportMode
		{
			get => base.EK_TransportMode;
			set => base.EK_TransportMode = value;
		}

		[List("Lookups.RecipientRoles")]
		public override ZString EK_RecipientRole
		{
			get => base.EK_RecipientRole;
			set => base.EK_RecipientRole = value;
		}

		[List("Lookups.Events")]
		public override ZString EK_EventCode
		{
			get => base.EK_EventCode;
			set => base.EK_EventCode = value;
		}

		[List("Lookups.EventReferenceTypes")]
		public override ZString EK_EventReferenceConditionType
		{
			get => base.EK_EventReferenceConditionType;
			set => base.EK_EventReferenceConditionType = value;
		}

		[ReadOnlyMember(nameof(EK_EventReferenceConditionValueReadOnlyMember))]
		public override ZString EK_EventReferenceConditionValue
		{
			get => base.EK_EventReferenceConditionValue;
			set => base.EK_EventReferenceConditionValue = value;
		}

		public bool EK_EventReferenceConditionValueReadOnlyMember => !Lookups.EventReferenceTypes.ContainsCode(EK_EventReferenceConditionType);

		[List("Lookups.Parties")]
		[ReadOnlyMember(nameof(IsCommunicationPartyReadOnly))]
		public ZGuid CommunicationParty
		{
			get => CommunicationPartyConfig?.ECC_ECP_Party ?? default;
			set
			{
				var party = Factory.Load<EDICommunicationParty>(value);
				if (party != null)
				{
					EK_ECC_CommunicationPartyConfig = party.OutboundConfig.PK;
				}
				else
				{
					EK_ECC_CommunicationPartyConfig = ZGuid.Empty;
				}
			}
		}

		[List("Lookups.Parties")]
		public ZGuid EK_ECP_CommunicationParty
		{
			get => CommunicationParty;
			set
			{
				CommunicationParty = value;
			}
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime EK_LastFailed
		{
			get { return base.EK_LastFailed; }
			set { base.EK_LastFailed = value; }
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid EK_GG
		{
			get { return base.EK_GG; }
			set { base.EK_GG = value; }
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString EK_DestinationFolder
		{
			get { return base.EK_DestinationFolder;
			}
			set { base.EK_DestinationFolder = value; }
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString EK_SourceFolder
		{
			get { return base.EK_SourceFolder; }
			set { base.EK_SourceFolder = value; }
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			var shouldBeReadOnly = false;

			if (property.Name == EK_CommunicationsTransportInfo.Name || property.Name == EK_DestinationInfo.Name)
			{
				shouldBeReadOnly = !Organisation.SecurityProvider.HasModifyConfigGeneralSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region EK_ECC_CommunicationPartyConfigInfo

		public override ZPropertyInfo EK_ECC_CommunicationPartyConfigInfo
		{
			get { return GetZPropertyInfo(EDICommunicationsModeSchema.Constants.EK_ECC_CommunicationPartyConfig, Res.GetString("1386bdf8-d2fa-4fd6-9bb6-713baf463277", "EDI Client Config")); }
		}

		#endregion
	}
}
