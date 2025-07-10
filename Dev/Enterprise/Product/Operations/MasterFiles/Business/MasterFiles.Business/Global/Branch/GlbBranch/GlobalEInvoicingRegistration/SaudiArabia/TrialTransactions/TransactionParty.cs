using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public abstract class TransactionParty
	{
		protected TransactionParty(OrgHeader org) : this(org.OH_FullName, org.MainAddress, org.CustomsCodes)
		{
		}

		protected TransactionParty(string companyName, OrgAddress address, OrgCusCodeCollection customsCodes)
		{
			CompanyName = companyName;
			AddressCode = address.OA_Code;
			BuildingNumber = GetBuildingNumber(address.OA_Address1);
			StreetName = GetStreetName(address.OA_Address1);
			CitySubdivisionName = address.OA_Address2;
			State = address.OA_State;
			City = address.OA_City;
			PostCode = address.OA_PostCode;
			CountryCode = address.OA_RN_NKCountryCode;
			CustomsCodesByType = GetCustomsCodesForSaudiArabia(customsCodes);
		}

		protected abstract OrgCusCode Identification { get; }

		public string CompanyName { get; }
		public string AddressCode { get; }
		public string BuildingNumber { get; }
		public string StreetName { get; }
		public string CitySubdivisionName { get; }
		public string State { get; }
		public string City { get; }
		public string PostCode { get; }
		public string CountryCode { get; }
		public string VatCode => GetCustomsCodeOrNull(OrgCusCode.CodeTypes.VATCode)?.OK_CustomsRegNo ?? string.Empty;
		public string IdentificationType => ConvertIdentificationType(Identification?.OK_CodeType ?? string.Empty);
		public string IdentificationNumber => Identification?.OK_CustomsRegNo ?? string.Empty;
		protected Dictionary<string, OrgCusCode> CustomsCodesByType { get; }

		protected OrgCusCode GetCustomsCodeOrNull(string code)
		{
			return CustomsCodesByType.TryGetValue(code, out var customsCode)
				? customsCode
				: null;
		}

		static string GetBuildingNumber(string address)
		{
			var addressSplit = address.Split(new[] { ' ', ',' }, 2);
			return addressSplit[0].Trim();
		}

		static string GetStreetName(string address)
		{
			var addressSplit = address.Split(new[] { ' ', ',' }, 2);
			return addressSplit.Length > 1 ? addressSplit[1].Trim() : string.Empty;
		}

		string ConvertIdentificationType(string type) => type == OrgCusCode.CodeTypes.CorporationCode
			? OrgCusCode.CodeTypes.CompanyRegistrationNumber
			: type;

		Dictionary<string, OrgCusCode> GetCustomsCodesForSaudiArabia(OrgCusCodeCollection customsCodes)
		{
			return customsCodes
				.GetAllOrgCusCodesForCountryAndCodes(Core.Constants.CountryCodes.SaudiArabia,
					OrgCusCode.CodeTypes.VATCode,
					SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN,
					OrgCusCode.CodeTypes.CompanyRegistrationNumber,
					OrgCusCode.CodeTypes.CorporationCode,
					SaudiArabiaOrgCusCodeInfo.OrgCusCodes.NAT,
					OrgCusCode.CodeTypes.PassportID)
				.ToDictionary(c => c.OK_CodeType.ToString());
		}
	}
}
