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
using WiseRates.Constants;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class FallbackSubjectToCharges : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string RatesProviderCode = nameof(RatesProviderCode);
			public const string TransportMode = nameof(TransportMode);
			public const string ContainerMode = nameof(ContainerMode);
			public const string IsFallbackEnabled = nameof(IsFallbackEnabled);
			public const string IsValidForSupportOnly = nameof(IsValidForSupportOnly);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return
				new FallbackSubjectToCharges().InitialiseValues
				(
					RatesProviderCode,
					TransportMode,
					ContainerMode,
					IsFallbackEnabled,
					IsValidForSupportOnly
				);
		}

		#region RatesProvider

		ZString ratesProviderCode;

		public ZPropertyInfo RatesProviderCodeInfo => GetZPropertyInfo(FallbackSubjectToCharges.Schema.RatesProviderCode);

		[ResourceStringData("FallbackSubjectToChargesControl|2ad8fa1f-1cab-496b-be1b-cb3ddf2d931b", Caption = "Rates Provider")]
		[List("RatesProviderCodeList")]
		[MaxLength(4)]
		public ZString RatesProviderCode
		{
			get
			{
				return GetValidRatesProviderCode(ratesProviderCode);
			}
			set
			{
				CheckMaximumLength(RatesProviderCodeInfo, value);
				ratesProviderCode = value;
				RatesProviderCodeInfo.RefreshBinding();
				Validate();
			}
		}

		public CodeDescriptionPairList RatesProviderCodeList
		{
			get
			{
				if (ratesProviderCodeList == null)
				{
					ratesProviderCodeList = new CodeDescriptionPairList();
					ratesProviderCodeList.AddPair(WRConstants.RateProviders.CargoSphere, "CargoSphere");
					ratesProviderCodeList.AddPair(WRConstants.RateProviders.CargoGuide, (NoResString)"Cargoguide"); // It's a name
				}
				return ratesProviderCodeList;
			}
		}

		CodeDescriptionPairList ratesProviderCodeList;

		static string GetValidRatesProviderCode(string ratesProviderCode)
		{
			//Backward compatibility: if ratesProvider has value CS then it corresponds to CargoSphere (CGSP)
			return (ratesProviderCode == "CS") ? WRConstants.RateProviders.CargoSphere : ratesProviderCode;
		}

		#endregion

		#region TransportMode

		ZString transportMode;
		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		[ResourceStringData("FallbackSubjectToChargesControl|219a7a14-6836-46d8-8692-3e25b1202d9d", Caption = "Transport Mode")]
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
		public ZPropertyInfo ContainerModeInfo => GetZPropertyInfo(FallbackSubjectToCharges.Schema.ContainerMode);

		[ResourceStringData("FallbackSubjectToChargesControl|0781ee52-2634-44e3-a01a-20003e06455b", Caption = "Container Mode")]
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

		#region IsFallbackEnabled

		[ResourceStringData("FallbackSubjectToChargesControl|152f0cd0-b8c4-4e6d-8dbf-768ece81d813", Caption = "Fallback to Costing")]
		public ZBool IsFallbackEnabled
		{
			get
			{
				return isFallbackEnabled;
			}
			set
			{
				isFallbackEnabled = value;
				IsFallbackEnabledInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsFallbackEnabledInfo => GetZPropertyInfo(FallbackSubjectToCharges.Schema.IsFallbackEnabled);

		ZBool isFallbackEnabled = false;

		#endregion

		FallbackSubjectToCharges InitialiseValues(string ratesProviderCode, string transportMode, string containerMode, bool isFallbackEnabled, bool isValidForSupportOnly)
		{
			// I don't update the bindings for the following three fields
			// as this code is only meant to be called for populating the
			// default data, deserialising or cloning.
			this.ratesProviderCode = ratesProviderCode;
			this.transportMode = transportMode;
			this.containerMode = containerMode;

			IsFallbackEnabled = isFallbackEnabled;
			IsValidForSupportOnly = isValidForSupportOnly;

			ContainerModeInfo.RefreshBinding();
			TransportModeInfo.RefreshBinding();
			RatesProviderCodeInfo.RefreshBinding();

			return this;
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.RatesProviderCode, RatesProviderCode);
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
			writer.WriteElementString(Schema.IsFallbackEnabled, IsFallbackEnabled.ToString());
			writer.WriteElementString(Schema.IsValidForSupportOnly, IsValidForSupportOnly.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			string ratesProviderCode = null;
			string transportMode = null;
			string containerMode = null;
			bool isFallbackEnabled = false;
			bool isValidForSupportOnly = false;

			XmlReader reader = wrapper.Reader;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case Schema.RatesProviderCode:
						ratesProviderCode = reader.ReadElementString();
						break;

					case Schema.TransportMode:
						transportMode = reader.ReadElementString();
						break;

					case Schema.ContainerMode:
						containerMode = reader.ReadElementString();
						break;

					case Schema.IsFallbackEnabled:
						isFallbackEnabled = ZBool.ParseSafe(reader.ReadElementString(), false);
						break;

					case Schema.IsValidForSupportOnly:
						isValidForSupportOnly = ZBool.ParseSafe(reader.ReadElementString(), false);
						break;

					default:
						reader.ReadElementString();
						break;
				}
			}

			InitialiseValues(ratesProviderCode, transportMode, containerMode, isFallbackEnabled, isValidForSupportOnly);
		}

		#endregion

		#region ReadOnly Override

		public override bool ReadOnly
		{
			get
			{
				var isReadOnly = base.ReadOnly;
				if (!isReadOnly && !Env.CurrentUser.IsSupportUser)
				{
					return IsValidForSupportOnly;
				}

				return isReadOnly;
			}
		}

		ZBool IsValidForSupportOnly { get; set; }

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

				MultilingualString invalidTransportModeMessage = ResString.GetMultilingualString("A64A1AD4-1E54-4CEC-8A95-7F18AEC97D52", "Transport Mode not valid");

				var validForAir = (RatesProviderCode == WRConstants.RateProviders.CargoGuide) && (TransportMode == Core.Constants.TransportModes.Air);
				var validForSea = (RatesProviderCode == WRConstants.RateProviders.CargoSphere) && (TransportMode == Core.Constants.TransportModes.Sea);

				if (!(validForAir || validForSea))
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
				// Regular user can read this option as valid but one cannot add this option because it's invalid for oneself.
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
				BasicValidation(RatesProviderCodeInfo);

				MultilingualString identicalFallbackForSubjectToConfigurationExists = ResString.GetMultilingualString("455BD68D-7883-4D90-B47F-1478787D72C2", "Fallback for Subject To charges with identical values already exists");

				foreach (var collection in ParentCollections.OfType<FallbackSubjectToChargesCollection>())
				{
					foreach (FallbackSubjectToCharges fallbackCharge in collection)
					{
						if (PK != fallbackCharge.PK
							&& RatesProviderCode == fallbackCharge.RatesProviderCode
							&& TransportMode == fallbackCharge.TransportMode
							&& ContainerMode == fallbackCharge.ContainerMode)
						{
							RatesProviderCodeInfo.AddError(identicalFallbackForSubjectToConfigurationExists);
							fallbackCharge.RatesProviderCodeInfo.AddError(identicalFallbackForSubjectToConfigurationExists);
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
