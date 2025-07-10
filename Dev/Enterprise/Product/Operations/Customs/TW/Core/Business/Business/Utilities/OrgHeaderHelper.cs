using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Business
{
	public static class OrgHeaderHelper
	{
		[ThreadSafe]
		public static string[] CBPCodeTypes = new string[] { OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark };

		[ThreadSafe]
		public static string[] WareHoseCodeTypes = new string[] { OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.CodeTypes.ControlledPremisesID };

		[ThreadSafe]
		public static string[] IDCodeTypesWithCustomCode = new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, Constants.OrgCusCodeType.CustomCode };

		[ThreadSafe]
		public static string[] BCDExporterBondedIDCodeTypes = new string[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark };

		public static bool CheckHasCusCode(OrgHeader orgHeader, string countryCode, IEnumerable<string> customCodes)
		{
			return orgHeader.CustomsCodes?.Cast<OrgCusCode>().Any(c => c.OK_RN_NKCodeCountry == countryCode && customCodes.Contains(c.OK_CodeType.ToString())) ?? false;
		}

		public static bool CheckHasVatInTW(OrgHeader orgHeader) => !orgHeader.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode).IsEmpty;

		public static OrgCusCode GetWareHouseOrgCusCode(this OrgAddress address) => address.GetOrgCusCode(WareHoseCodeTypes);

		public static ZString GetVatOrPasOrPid(OrgHeader orgHeader) => orgHeader.GetIDOrgCusCode()?.OK_CustomsRegNo ?? ZString.Empty;

		public static OrgCusCode GetIDOrgCusCode(this OrgHeader orgHeader) => orgHeader.GetOrgCusCode(IDCodeTypesWithCustomCode);

		public static OrgCusCode GetLocalProcessorIDOrgCusCode(this OrgHeader orgHeader, OrgAddress address)
		{
			OrgCusCode result;
			result = address.GetOrgCusCode(new string[] { OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber });
			if (result == null)
			{
				result = orgHeader.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID });
			}
			return result;
		}

		public static OrgCusCode GetOrgCusCode(this OrgAddress address, IEnumerable<string> codesToLookFor) => GetOrgCusCode(address?.CustomsCodes?.Cast<OrgCusCode>(), codesToLookFor);

		public static OrgCusCode GetOrgCusCode(this OrgHeader orgHeader, IEnumerable<string> codesToLookFor) => GetOrgCusCode(orgHeader?.CustomsCodes?.Cast<OrgCusCode>(), codesToLookFor);

		static OrgCusCode GetOrgCusCode(IEnumerable<OrgCusCode> customsCodes, IEnumerable<string> codesToLookFor)
		{
			OrgCusCode result = null;
			if (customsCodes != null)
			{
				foreach (var codeType in codesToLookFor)
				{
					result = customsCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Taiwan && codeType == x.OK_CodeType);
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		public static ZString GetCustomsRegNo(this OrgAddress checkingAddress, string codeTypeToCheck)
		{
			ZString result;
			switch (codeTypeToCheck)
			{
				case OrgCusCode.TaiwanCodeTypes.AEO:
				case OrgCusCode.CodeTypes.PassportID:
				case OrgCusCode.TaiwanCodeTypes.PID:
				case OrgCusCode.CodeTypes.VATCode:
				case OrgCusCode.TaiwanCodeTypes.TPC:
				case OrgCusCode.CodeTypes.CarrierCode:
					result = checkingAddress?.Header?.GetCustomsRegNo(codeTypeToCheck) ?? ZString.Empty;
					break;
				default:
					result = checkingAddress?.CustomsCodes?.GetCustomsRegNo(codeTypeToCheck, Core.Constants.CountryCodes.Taiwan) ?? ZString.Empty;
					break;
			}
			return result;
		}

		public static ZString GetCustomsRegNo(this OrgAddress checkingAddress, string[] codesToLookFor)
		{
			return checkingAddress?.CustomsCodes?.Where(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Taiwan && codesToLookFor.Contains<string>(x.OK_CodeType))?.FirstOrDefault()?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public static ZString GetCustomsRegNo(this OrgHeader orgHeader, string codeType)
		{
			return orgHeader?.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.Taiwan) ?? ZString.Empty;
		}

		public static ZString GetCustomsRegNo(this OrgHeader orgHeader, string codeType, string countryCode)
		{
			return orgHeader?.CustomsCodes?.GetCustomsRegNo(codeType, countryCode) ?? ZString.Empty;
		}

		public static ZString GetChineseName(this OrgAddress address)
		{
			var result = ZString.Empty;
			if (address != null)
			{
				if (address.OA_Language == Core.SharedConstants.Languages.ChineseTraditional)
				{
					result = address.CompanyName;
				}
				if (result.IsEmpty)
				{
					result = address.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.ChineseTraditional)?.CompanyName ?? ZString.Empty;
				}
			}
			return result;
		}

		public static ZString GetChineseAddress(this OrgAddress address)
		{
			var result = ZString.Empty;

			if (address != null)
			{
				if (address.OA_Language == Core.SharedConstants.Languages.ChineseTraditional)
				{
					result = GetChineseFormattedAddress(address.Postcode, address.Country.GetChineseCountryName(), address.City, address.Address1, address.Address2);
				}

				if (result.IsEmpty)
				{
					var translatedAddress = address.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.ChineseTraditional);
					if (translatedAddress != null)
					{
						result = GetChineseFormattedAddress(translatedAddress.Postcode, address.Country.GetChineseCountryName(), translatedAddress.City, translatedAddress.Address1, translatedAddress.Address2);
					}
				}
			}

			return result;
		}

		public static ZString GetChineseCountryName(this RefCountry country)
		{
			return country?.RN_DescMultilingual?.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional).ToString() ?? ZString.Empty;
		}

		static ZString GetChineseFormattedAddress(ZString postcode, ZString countryName, ZString city, ZString address1, ZString address2)
		{
			var stringBuilder = new ZStringBuilder();
			if (!address1.IsEmpty)
			{
				stringBuilder.AppendIfNotEmpty(postcode);
				stringBuilder.AppendIfNotEmpty(countryName);
				stringBuilder.AppendIfNotEmpty(city);
				stringBuilder.Append(address1);
				stringBuilder.AppendIfNotEmpty(address2);
			}
			return stringBuilder.ToString();
		}

		public static ZString GetPartyIdentifierCode(string codeType)
		{
			var result = ZString.Empty;
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.VATCode:
					result = PartyIdentifierCodeList.Codes._58;
					break;
				case OrgCusCode.CodeTypes.PassportID:
					result = PartyIdentifierCodeList.Codes._53;
					break;
				case OrgCusCode.TaiwanCodeTypes.PID:
					result = PartyIdentifierCodeList.Codes._174;
					break;
			}
			return result;
		}

		public static ZBool HasAddressOfLanguage(this OrgAddress address, ZString languageNeeded)
		{
			var result = false;
			if (address != null)
			{
				result = (address.Language == languageNeeded) || (address.GetTranslatedAddressInSpecificLanguage(languageNeeded) != null);
			}
			return result;
		}

		public static ZBool HasCompanyNameOfLanguage(this OrgAddress address, ZString languageNeeded)
		{
			var result = false;
			if (address != null)
			{
				result = address.Language == languageNeeded && !address.CompanyName.IsEmpty;
				if (!result)
				{
					var translateAddress = address.GetTranslatedAddressInSpecificLanguage(languageNeeded);
					result = translateAddress != null && !translateAddress.OTA_CompanyName.IsEmpty;
				}
			}
			return result;
		}

		public static CodeDescriptionPairList GetCodeTypeList(this BusinessObjectFactory factory, params string[] codeTypes)
		{
			var list = new CodeDescriptionPairList();
			var twOrgCusCodes = new OrgCodeLists().CustomsCodes_List(factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Taiwan));
			codeTypes.ForEach(x =>
			{
				if (x == Constants.OrgCusCodeType.CustomCode)
				{
					list.AddPair(Constants.OrgCusCodeType.CustomCode, ResString.GetMultilingualString("Enterprise.Customs.TW.Business.OrgHeaderHelper|CustomCodeResString", "Custom Code"));
				}
				else if (x == Constants.CCPPrefix)
				{
					list.AddPair(Constants.CCPPrefix, ResString.GetMultilingualString("Enterprise.Customs.TW.Business.OrgHeaderHelper|BondedIDForTheForeignCompanyResString", "Bonded ID for the foreign company"));
				}
				else
				{
					list.AddPair(x, twOrgCusCodes.GetDescriptionFromCode(x));
				}
			});
			return list;
		}
	}
}
