using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class TWNCATKClientSetting : RegistryBusinessObjectTemplate
	{
		#region Schema and Constructors
		protected abstract class Schema
		{
			public const string MachineName = "MachineName";
			public const string EHubClientID = "EHubClientID";
			public const string EHubClientStatus = "EHubClientStatus";
			public const string SendToFolder = "SendToFolder";
			public const string RunningIntervalInSeconds = "RunningIntervalInSeconds";
		}

		public TWNCATKClientSetting() : base()
		{
		}

		public TWNCATKClientSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
			if (fallbackLevel != null)
			{
				CompanyPK = new ZGuid(fallbackLevel.CompanyPK(false));
			}
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			RunningIntervalInSeconds = 60;
		}

		ZGuid CompanyPK { get; }
		#endregion

		#region MachineName
		ZString fMachineName;
		[ResourceStringData("Enterprise.Customs.TW.Business.TWNCATKClientSetting|MachineName", Caption = "Machine Name")]
		public ZString MachineName
		{
			get => fMachineName;
			set
			{
				if (MachineName != value)
				{
					CheckMaximumLength(MachineNameInfo, value);
					SetNonPersistentPropertyValue(MachineNameInfo, ref fMachineName, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateMachineName();
					}
					if (!value.IsEmpty)
					{
						if (CurrentFactory != null && !CompanyPK.IsEmpty && EHubClientID.IsEmpty)
						{
							var currentCompany = CurrentFactory.Load<GlbCompany>(CompanyPK);
							if (currentCompany != null)
							{
								EHubClientID = currentCompany.LicenceKeyIdentifier + "_TCA";
							}
						}
					}
					else
					{
						EHubClientID = ZString.Empty;
					}
					MachineNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo MachineNameInfo => GetZPropertyInfo(Schema.MachineName);
		#endregion

		#region SendToFolder
		ZString fSendFolder;
		[ResourceStringData("Enterprise.Customs.TW.Business.TWNCATKClientSetting|SendToFolder", Caption = "Send To Folder")]
		public ZString SendToFolder
		{
			get => fSendFolder;
			set
			{
				if (SendToFolder != value)
				{
					CheckMaximumLength(SendToFolderInfo, value);
					SetNonPersistentPropertyValue(SendToFolderInfo, ref fSendFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateSendToFolder();
					}
					SendToFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SendToFolderInfo => GetZPropertyInfo(Schema.SendToFolder);
		#endregion

		#region RunningIntervalInSeconds
		ZInt fRunningIntervalInSeconds;
		[ResourceStringData("Enterprise.Customs.TW.Business.TWNCATKClientSetting|RunningIntervalInSeconds", Caption = "Running Interval (in seconds)")]
		public ZInt RunningIntervalInSeconds
		{
			get => fRunningIntervalInSeconds;
			set
			{
				if (RunningIntervalInSeconds != value)
				{
					SetNonPersistentPropertyValue(RunningIntervalInSecondsInfo, ref fRunningIntervalInSeconds, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateRunningIntervalInSeconds();
					}
					RunningIntervalInSecondsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo RunningIntervalInSecondsInfo => GetZPropertyInfo(Schema.RunningIntervalInSeconds);
		#endregion

		#region EHubClientID
		ZString fEHubClientID;
		[ResourceStringData("Enterprise.Customs.TW.Business.TWNCATKClientSetting|EHubClientID", Caption = "eHub Client ID")]
		public ZString EHubClientID
		{
			get => fEHubClientID;
			private set
			{
				fEHubClientID = value;
				EHubClientStatus = EHubClientID.IsEmpty ? ZString.Empty : new ZString(Constants.TWNCATKClient.EHubClientStatusOK);
			}
		}

		public ZPropertyInfo EHubClientIDInfo => GetZPropertyInfo(Schema.EHubClientID);
		#endregion

		#region EHubClientStatus
		[ResourceStringData("Enterprise.Customs.TW.Business.TWNCATKClientSetting|EHubClientStatus", Caption = "eHub Client Status")]
		public ZString EHubClientStatus { get; private set; }

		public ZPropertyInfo EHubClientStatusInfo => GetZPropertyInfo(Schema.EHubClientStatus);
		#endregion

		#region eHub Client Status
		ZString OriginalEHubClientStatus { get; set; }

		public bool ShouldRegisterEHubClient => !EHubClientID.IsEmpty;

		public bool ShouldUnregisterEHubClient => OriginalEHubClientStatus == Constants.TWNCATKClient.EHubClientStatusOK;
		#endregion

		#region Validation

		TWNCATKClientSettingValidation fValidation;
		public TWNCATKClientSettingValidation Validation => fValidation ?? (fValidation = new TWNCATKClientSettingValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Overrides
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TWNCATKClientSetting(fallbackLevel, factory);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MachineName, MachineName);
			writer.WriteElementString(Schema.SendToFolder, SendToFolder);
			writer.WriteElementString(Schema.RunningIntervalInSeconds, RunningIntervalInSeconds.ToString());
			writer.WriteElementString(Schema.EHubClientID, EHubClientID);
			writer.WriteElementString(Schema.EHubClientStatus, EHubClientStatus);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			MachineName = reader.ReadElementString(Schema.MachineName);
			SendToFolder = reader.ReadElementString(Schema.SendToFolder);
			RunningIntervalInSeconds = reader.ReadElementStringAsZInt(Schema.RunningIntervalInSeconds);
			EHubClientID = reader.ReadElementString(Schema.EHubClientID);
			EHubClientStatus = reader.ReadElementString(Schema.EHubClientStatus);

			OriginalEHubClientStatus = EHubClientStatus;
		}

		#endregion
	}
}
