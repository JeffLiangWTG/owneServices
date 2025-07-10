using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	public static class ValidationConstants
	{
		public static class Weight
		{
			public static string WeightMustBeGreaterThanZero
			{
				get { return ResString.GetMultilingualString("USMessaging|00843BE8-600A-488f-AAA5-234F63C06323", "Please enter a value greater than zero which represent the gross weight in pounds or kilos."); }
			}

			public static string WeightGreaterThanMaximumKilogramsAllowed(ZDecimal weight, ZDecimal maximuAllowed)
			{
				return ResString.GetMultilingualString("USMessaging|223D1DD6-EB70-4c05-9313-74F2FB7E7884", "The entered value '{0:N0}' in kilograms is greater than the maximum allowed kilograms '{1:N0}'.", weight, maximuAllowed);
			}

			public static string WeightGreaterThanMaximumPoundsAllowed(ZDecimal weight, ZDecimal maximuAllowed)
			{
				return ResString.GetMultilingualString("USMessaging|7CFF5485-EA19-48fa-9144-CE0671F4090E", "The entered value '{0:N0}' is greater than the maximum allowed pounds '{1:N0}'.", weight, maximuAllowed);
			}

			public static string WeightUQTypeAllowed
			{
				get { return ResString.GetMultilingualString("USMessaging|7DF5588C-57FB-4190-8E00-1C2C5203F44F", "The system will report this weight in whole kilograms when it is reported to Customs as Customs only accepts the weight in whole pounds or whole kilograms."); }
			}

			public static string WeightUQInWholePounds
			{
				get { return ResString.GetMultilingualString("USMessaging|A48CFBCE-28C3-42bc-AAF5-EAFDBC7206F6", "The system will report this weight in whole pounds when it is reported to Customs as Customs only accepts whole numbers."); }
			}

			public static string WeightUQInWholeKilograms
			{
				get { return ResString.GetMultilingualString("USMessaging|31661980-4173-4b73-AF88-A75B3C8F7A24", "The system will report this weight in whole kilograms when it is reported to Customs as Customs only accepts whole numbers."); }
			}
		}

		public static class Volume
		{
			public static string VolumeMustBeGreaterThanZero
			{
				get { return ResString.GetMultilingualString("USMessaging|70D6BCCA-23FD-43a4-B376-A9D53FAEC7F0", "Please enter a value greater than zero which represent the volume in cubic feet or cubic meters."); }
			}

			public static string VolumeGreaterThanMaximumCubicMetresAllowed(ZDecimal volume, ZDecimal maximuAllowed)
			{
				return ResString.GetMultilingualString("USMessaging|F3C8A03D-8104-4b06-AC11-289E7110C66B", "The entered value '{0:N0}' in cubic meters is greater than the maximum allowed cubic meters '{1:N0}'.", volume, maximuAllowed);
			}

			public static string VolumeGreaterThanMaximumCubicFeetAllowed(ZDecimal volume, ZDecimal maximuAllowed)
			{
				return ResString.GetMultilingualString("USMessaging|C28A106F-5886-482b-BC94-8A7EC3E743B6", "The entered value '{0:N0}' is greater than the maximum allowed cubic feet '{1:N0}'.", volume, maximuAllowed);
			}

			public static string VolumeUQTypeAllowed
			{
				get { return ResString.GetMultilingualString("USMessaging|A6707CF7-FB43-473e-94A5-7433EDD8DE20", "The system will report this volume in whole cubic meters when it is reported to Customs as Customs only accepts the volume in whole cubic feet or whole cubic meters."); }
			}

			public static string VolumeUQInWholeCubicFeet
			{
				get { return ResString.GetMultilingualString("USMessaging|ABB0D486-46D1-41c6-82EF-F651192B65B1", "The system will report this volume in whole cubic feet when it is reported to Customs as Customs only accepts whole numbers."); }
			}

			public static string VolumeUQInWholeCubicMetres
			{
				get { return ResString.GetMultilingualString("USMessaging|71C5EFA4-00CE-4bf9-B9C0-1DBA8E9C304D", "The system will report this volume in whole cubic meters when it is reported to Customs as Customs only accepts whole numbers."); }
			}
		}

		public static class Characters
		{
			public static string InvalidCharacters(ZString propertyName, ZString value)
			{
				return ResString.GetMultilingualString("USMessaging|6D8DA156-B474-4234-93CA-0230BC5CA880", "{0} : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with {1}.", propertyName, value);
			}
		}
	}
}
