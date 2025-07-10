using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRegistrationNumberTypeList : ReadOnlyCodeDescriptionPairList
	{
		public OrgRegistrationNumberTypeList(RefCountry country)
		{
			Initialise(country);
		}

		public OrgRegistrationNumberTypeList(RefCountry[] countries)
		{
			Initialise(countries);
		}

		void Add(RefCountry country, string numberType, string description)
		{
			Elements.Add(new CodeElement(country.Code, numberType, description));
		}

		public string GetActualCode(string displayCode)
		{
			string result;
			if (ContainsCode(displayCode))
			{
				result = (displayCode.Length > 3) ? displayCode.Substring(3) : displayCode;
			}
			else
			{
				result = string.Empty;
			}
			return result;
		}

		/// <summary>
		/// Returns a list of registration number types that apply to a country, ordered by priority.
		/// </summary>
		/// <param name="country">The country to find the registration number types for.</param>
		/// <returns>An array of strings that represents a list of registration number types.</returns>
		public static string[] GetApplicableNumberTypes(RefCountry country)
		{
			List<string> result = new List<string>();
			result.AddRange(GetOrganizationalNumberTypes(country));
			result.AddRange(GetPersonalEffectsNumberTypes(country));
			return result.ToArray();
		}

		public ZString GetCountryCode(string code)
		{
			foreach (ICodeDescription item in Elements)
			{
				if (item.Code == code)
				{
					return (ZString)item.PK;
				}
			}
			return ZString.Empty;
		}

		public string GetDisplayCode(string actualCode, RefCountry country)
		{
			string result = (country.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode) ? actualCode : GetNumberTypeWithCountryPrefix(actualCode, country.RN_Code);
			return ContainsCode(result) ? result : string.Empty;
		}

		public static string GetNumberTypeWithCountryPrefix(string numberType, string countryCode)
		{
			return countryCode + ':' + numberType;
		}

		public static string[] GetOrganizationalNumberTypes(RefCountry country)
		{
			if (country == null)
			{
				return Array.Empty<string>();
			}

			var mainTypes = OrgCusCodeCountryFactory.GetIOrgCusCodeProvider(country.Code).GetMainOrganizationNumberTypes();
			return mainTypes.ToArray();
		}

		public static string[] GetPersonalEffectsNumberTypes(RefCountry country)
		{
			if (country == null)
			{
				return Array.Empty<string>();
			}

			List<string> result = new List<string>();
			result.Add(OrgCusCode.CodeTypes.PassportID);
			result.Add(OrgCusCode.CodeTypes.DriverLicenceID);

			switch (country.RN_Code)
			{
				case Constants.CountryCodes.Australia:
					result.Add(OrgCusCode.CodeTypes.MedicareID);
					break;

				case Constants.CountryCodes.Canada:
					result.Insert(0, OrgCusCode.CACodeTypes.SocialInsuranceNumber);
					break;

				case Constants.CountryCodes.Singapore:
					result.Insert(0, OrgCusCode.CodeTypes.IdentityCardNumber);
					break;

				case Constants.CountryCodes.UnitedStates:
				case Constants.CountryCodes.PuertoRico:
				case Constants.CountryCodes.MarshallIslands:
					result.Insert(0, OrgCusCode.USACodeTypes.SocialSecurityNumber);
					break;
			}

			return result.ToArray();
		}

		void Initialise(RefCountry orgCountry)
		{
			OrgCodeLists codeLists = new OrgCodeLists();
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			CodeDescriptionPairList currentCountryCusCodes = codeLists.CustomsCodes_List(currentCountry);

			foreach (string numberType in GetApplicableNumberTypes(currentCountry))
			{
				Add(currentCountry, numberType, currentCountryCusCodes.GetDescriptionFromCode(numberType));
			}

			if ((orgCountry != null) && (orgCountry.PK != currentCountry.PK))
			{
				CodeDescriptionPairList orgCountryCusCodes = codeLists.CustomsCodes_List(orgCountry);
				foreach (string numberType in GetApplicableNumberTypes(orgCountry))
				{
					Add(orgCountry, GetNumberTypeWithCountryPrefix(numberType, orgCountry.RN_Code), orgCountryCusCodes.GetDescriptionFromCode(numberType));
				}
			}
		}

		void Initialise(RefCountry[] orgCountries)
		{
			OrgCodeLists codeLists = new OrgCodeLists();
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			CodeDescriptionPairList currentCountryCusCodes = codeLists.CustomsCodes_List(currentCountry);

			foreach (string numberType in GetApplicableNumberTypes(currentCountry))
			{
				Add(currentCountry, numberType, currentCountryCusCodes.GetDescriptionFromCode(numberType));
			}

			var addedCountries = new List<string>();
			foreach (var orgCountry in orgCountries)
			{
				if (orgCountry != null && !addedCountries.Contains(orgCountry.RN_Code))
				{
					addedCountries.Add(orgCountry.RN_Code);
					if ((orgCountry != null) && (orgCountry.PK != currentCountry.PK))
					{
						CodeDescriptionPairList orgCountryCusCodes = codeLists.CustomsCodes_List(orgCountry);
						foreach (string numberType in GetApplicableNumberTypes(orgCountry))
						{
							Add(orgCountry, GetNumberTypeWithCountryPrefix(numberType, orgCountry.RN_Code), orgCountryCusCodes.GetDescriptionFromCode(numberType));
						}
					}
				}
			}
		}
	}
}
