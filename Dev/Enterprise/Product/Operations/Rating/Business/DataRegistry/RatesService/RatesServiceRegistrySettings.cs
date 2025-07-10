using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class RatesServiceRegistrySettings : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string TransportMode = nameof(TransportMode);
			public const string ContainerMode = nameof(ContainerMode);
			public const string IsSubscriptionEnabled = nameof(IsSubscriptionEnabled);
			public const string IsValidForSupportOnly = nameof(IsValidForSupportOnly);
		}

		#endregion

		public RatesServiceRegistrySettings(string transportMode, string containerMode, bool isSubscriptionEnabled)
			: this(transportMode, containerMode, isSubscriptionEnabled, false)
		{
		}

		RatesServiceRegistrySettings(string transportMode, string containerMode, bool isSubscriptionEnabled, bool isValidForSupportOnly)
		{
			this.transportMode = transportMode;
			this.containerMode = containerMode;
			IsSubscriptionEnabled = isSubscriptionEnabled;
			IsValidForSupportOnly = isValidForSupportOnly;
		}

		public RatesServiceRegistrySettings()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RatesServiceRegistrySettings(TransportMode, ContainerMode, isSubscriptionEnabled, IsValidForSupportOnly);
		}

		#region TransportMode

		ZString transportMode;
		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		[ResourceStringData("RatesServiceRegistrySettingsControl|5abf04c6-826b-43a9-96a3-4ee68cf99d5f", Caption = "Transport Mode")]
		[List("TransportModeList")]
		[MaxLength(3)]
		public ZString TransportMode
		{
			get => transportMode;
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				transportMode = value;
				TransportModeInfo.RefreshBinding();
				Validate();
			}
		}

		public CodeDescriptionPairList TransportModeList => supportedModes.TransportModeList;

		#endregion

		#region ContainerMode

		ZString containerMode;
		public ZPropertyInfo ContainerModeInfo => GetZPropertyInfo(RatesServiceRegistrySettings.Schema.ContainerMode);

		[ResourceStringData("RatesServiceRegistrySettingsControl|75d5969a-8c15-4d26-870b-09821f4d5676", Caption = "Container Mode")]
		[List("ContainerModeList")]
		[MaxLength(3)]
		public ZString ContainerMode
		{
			get => containerMode;
			set
			{
				CheckMaximumLength(ContainerModeInfo, value);
				containerMode = value;
				ContainerModeInfo.RefreshBinding();
				Validate();
			}
		}

		public CodeDescriptionPairList ContainerModeList => supportedModes.ContainerModeList;

		#endregion

		#region IsSubscriptionEnabled

		[ResourceStringData("RatesServiceRegistrySettingsControl|75bc22e5-3399-4ecb-9d12-ef70d15839b2", Caption = "Enabled")]
		public ZBool IsSubscriptionEnabled
		{
			get
			{
				return isSubscriptionEnabled;
			}
			set
			{
				isSubscriptionEnabled = value;
				IsSubscriptionEnabledInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsSubscriptionEnabledInfo => GetZPropertyInfo(RatesServiceRegistrySettings.Schema.IsSubscriptionEnabled);

		ZBool isSubscriptionEnabled = false;

		#endregion

		#region ReadOnly override

		public override bool ReadOnly
		{
			get
			{
				var isReadOnly = base.ReadOnly;
				if (!isReadOnly && !Env.CurrentUser.IsSupportUser)
				{
					// This option is only valid for support user so it should be read-only for regular user.
					return IsValidForSupportOnly;
				}

				return isReadOnly;
			}
		}

		ZBool IsValidForSupportOnly { get; set; }

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
			writer.WriteElementString(Schema.IsSubscriptionEnabled, IsSubscriptionEnabled.ToString());
			writer.WriteElementString(Schema.IsValidForSupportOnly, IsValidForSupportOnly.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			XmlReader reader = wrapper.Reader;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case Schema.TransportMode:
						transportMode = reader.ReadElementString();
						break;

					case Schema.ContainerMode:
						containerMode = reader.ReadElementString();
						break;

					case Schema.IsSubscriptionEnabled:
						isSubscriptionEnabled = ZBool.ParseSafe(reader.ReadElementString(), false);
						break;

					case Schema.IsValidForSupportOnly:
						IsValidForSupportOnly = ZBool.ParseSafe(reader.ReadElementString(), false);
						break;

					default:
						reader.ReadElementString();
						break;
				}
			}
		}

		#endregion

		#region Validation

		void Validate()
		{
			ValidateTransportMode();
			ValidateContainerMode();
			CheckUniqueConfigurations();
		}

		void BasicValidation(ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}

		void ValidateTransportMode()
		{
			if (!IsValidationSuspended)
			{
				BasicValidation(TransportModeInfo);

				MultilingualString invalidTransportModeMessage = ResString.GetMultilingualString("f4260f9a-6d09-4d2a-b1bb-c663d880985e", "Transport Mode not valid");

				if (!(TransportMode == Core.Constants.TransportModes.Air || TransportMode == Core.Constants.TransportModes.Sea))
				{
					TransportModeInfo.AddError(invalidTransportModeMessage);
				}
			}
		}

		void ValidateContainerMode()
		{
			if (IsValidationSuspended)
			{
				return;
			}

			BasicValidation(ContainerModeInfo);

			var validity = supportedModes.GetValidity(transportMode.ToString(), containerMode.ToString());
			if (validity == SupportedTransportAndContainerModes.Validity.ValidForSupportOnly && Env.CurrentUser.IsSupportUser)
			{
				// Support user can add this option but it's only valid for support user.
				// Regular user can read this option as valid but one cannot add this option as it's invalid for oneself.
				// Please see also ReadOnly property overriding.
				IsValidForSupportOnly = true;
			}
			else if (validity == SupportedTransportAndContainerModes.Validity.ValidForSupportOnly && !IsValidForSupportOnly)
			{
				var errorMessage = Res.GetString("24c928ec-1375-45f0-b630-b7a7dfb2704e", "{0}-{1} is not a currently supported option.", transportMode, containerMode);
				ContainerModeInfo.AddError(errorMessage);
			}
			else if (validity == SupportedTransportAndContainerModes.Validity.Invalid)
			{
				var invalidContainerModeMessage = Res.GetString("591b811d-b464-41e5-b5a4-5bc6ececebdf", "Container Mode not valid");
				ContainerModeInfo.AddError(invalidContainerModeMessage);
			}
		}

		void CheckUniqueConfigurations()
		{
			if (!IsValidationSuspended)
			{
				BasicValidation(TransportModeInfo);

				MultilingualString identicalRatesServiceRateSelectorConfigurationExists = ResString.GetMultilingualString("994c2760-661d-4440-b818-591418f34fc5", "Configuration with identical criteria already exists.");
				var collection = ParentCollections.OfType<RatesServiceRegistrySettingsCollection>().FirstOrDefault();

				if (collection != null)
				{
					foreach (RatesServiceRegistrySettings ratesServiceRateSelector in collection)
					{
						if (PK != ratesServiceRateSelector.PK
							&& TransportMode == ratesServiceRateSelector.TransportMode
							&& ContainerMode == ratesServiceRateSelector.ContainerMode)
						{
							TransportModeInfo.AddError(identicalRatesServiceRateSelectorConfigurationExists);
							ratesServiceRateSelector.TransportModeInfo.AddError(identicalRatesServiceRateSelectorConfigurationExists);
							break;
						}
					}
				}
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validate();
		}

		readonly SupportedTransportAndContainerModes supportedModes = new SupportedTransportAndContainerModes();
	}
}
