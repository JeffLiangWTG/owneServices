using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	public static class AddressTestData
	{
		public static OrgHeader PopulateForCountry(this OrgHeader orgHeader, string countryCode, string fullName = "Address", string closestPort = null)
		{
			_ = orgHeader ?? throw new ArgumentNullException(nameof(closestPort));
			orgHeader.OH_FullName = fullName;

			switch (countryCode)
			{
				case Constants.CountryCodes.Australia:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit52";
					orgHeader.MainAddress.Address2 = "Dorcus yamadai";
					orgHeader.MainAddress.City = "Sydney";
					orgHeader.MainAddress.Postcode = "2017";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
					break;

				case Constants.CountryCodes.China:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "Beijing";
					orgHeader.MainAddress.Postcode = "999";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "CN";
					break;

				case Constants.CountryCodes.NewZealand:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 52";
					orgHeader.MainAddress.Address2 = "83 Ulster road";
					orgHeader.MainAddress.City = "Auckland";
					orgHeader.MainAddress.Postcode = "1010";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "NZ";
					break;

				case Constants.CountryCodes.Singapore:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "Singapore City";
					orgHeader.MainAddress.Postcode = "999";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "SG";
					break;

				case Constants.CountryCodes.Thailand:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "Thailand City";
					orgHeader.MainAddress.Postcode = "4242";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "TH";
					break;

				case Constants.CountryCodes.UnitedKingdom:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "United Kingdom City";
					orgHeader.MainAddress.Postcode = "1011";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "GB";
					break;

				case Constants.CountryCodes.Indonesia:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "Indonesia City";
					orgHeader.MainAddress.Postcode = "666";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "ID";
					break;

				case Constants.CountryCodes.Japan:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "Japan City";
					orgHeader.MainAddress.Postcode = "1337";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "JP";
					break;

				case Constants.CountryCodes.KoreaSouth:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? countryCode.GetDefaultPort();
					orgHeader.MainAddress.Address1 = "Unit 100";
					orgHeader.MainAddress.Address2 = "11 Why Street";
					orgHeader.MainAddress.City = "Seoul City";
					orgHeader.MainAddress.Postcode = "01004";
					orgHeader.MainAddress.OA_RN_NKCountryCode = "KR";
					break;

				default:
					orgHeader.OH_RL_NKClosestPort = closestPort ?? throw new ArgumentNullException(nameof(closestPort));
					orgHeader.MainAddress.Address1 = "Unit 42";
					orgHeader.MainAddress.Address2 = "42 Forty Two Street";
					orgHeader.MainAddress.City = "Fort Ytu";
					orgHeader.MainAddress.Postcode = "424242";
					orgHeader.MainAddress.OA_RN_NKCountryCode = countryCode;
					break;
			}

			return orgHeader;
		}

		public static string GetDefaultPort(this string countryCode) => countryCode switch
		{
			Constants.CountryCodes.Australia => "AUSYD",
			Constants.CountryCodes.China => "CNSZX",
			Constants.CountryCodes.NewZealand => "NZAKL",
			Constants.CountryCodes.Singapore => "SGSIN",
			Constants.CountryCodes.Thailand => "THPHI",
			Constants.CountryCodes.KoreaSouth => "KRSEL",
			_ => "GBLON"
		};
	}
}
