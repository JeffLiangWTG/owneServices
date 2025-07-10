
using System;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Business
{
	public static class Constants
	{
		public const string CountryCode = "PL";
		public const string EuropeanUnionCode = "EUN";

		public static class ShipmentType
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
		}

		public static class ProgramFunctions
		{
			public const string CudExchangeRates = "CUD";
			public const string CusExchangeRates = "CUS";
			public const string Tariffs = "TARIFFS";
			public const string Dictionaries = "DICTIONARIES";
			public const string Quota = "QUOTA";
			public const string Normalize = "NORMALIZE";
			public const string TaricUpdate = "TARICUPDATE";
			public const string DownloadIsztar4Base = "DOWNLOADISZTAR4BASE";
			public const string CusProcedure = "CUSPROCEDURE";
		}

		public static DateTime LatestVatChangeDate => new DateTime(2011, 1, 1);

		public const long MaxAcceptableSizeOfNormalizedBaseFileInBytes = 1000000000; // 1GB

		public static class AppSettingsKeys
		{
			public const string PUESCLogin = "PUESCLogin";
			public const string PUESCPassword = "PUESCPassword";
			public const string PUESCUrl = "PUESCUrl";

			public const string TariffUpdateFolder = "TariffUpdateFolder";
			public const string TariffArchiveFolder = "TariffArchiveFolder";

			public const string Taric4BaseFilePath = "Taric4BaseFilePath";
		}

		public static class Soap
		{
			public static class Puesc
			{
				public const string TariffUpdateResponseFilename = "KopertaContent.xml";
				public const string TariffUpdateRequestFileName = "isztar4-Req.xml";
			}

			public static class XConstants
			{
				public const string System = "system";
				public const string To = "To";
				public const string Created = "Created";
				public const string EncodingType = "EncodingType";
				public const string Id = "Id";
				public const string Nonce = "Nonce";
				public const string MustUnderstand = "MustUnderstand";
				public const string Password = "Password";
				public const string Security = "Security";
				public const string Type = "Type";
				public const string Username = "Username";
				public const string UsernameToken = "UsernameToken";
				public const string StartDate = "startDate";
				public const string EndDate = "endDate";
				public const string IsztarHistoryRequest = "IsztarHistoryRequest";
				public const string AcceptObligation = "akceptuje_zobowiazanie";
			}

			public static class SoapNamespaces
			{
				public const string IsztarAlias = "tns";
				public const string XsiAlias = "xsi";
				public const string SecurityAlias = "o";
				public const string SecurityUtilityAlias = "u";
				public static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
				public static readonly XNamespace Isztar = "http://www.mf.gov.pl/schematy/isztar/ecipSeapInpParams/2014/01";
				public static readonly XNamespace Security = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
				public static readonly XNamespace SoapEnvelope = "http://schemas.xmlsoap.org/soap/envelope/";
				public static readonly XNamespace SecurityUtility = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
			}

			public static class XAttributeValues
			{
				public const string EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
				public const string PasswordType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest";
			}
		}

	}
}
