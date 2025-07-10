using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public static class ZARefCusCodeListTypes
	{
		public static CodeDescriptionPairList GetBankCodeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCustomsOfficeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCustomsStatusList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetAddInWithROOTypeAttribute(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, new ZString[] { RefCusCodeListAttributeTypes.Codes.ROOType });
		}

		public static CodeDescriptionPairList GetAddInWithEmptyAttribute(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, new ZString[] { RefCusCodeListAttributeTypes.Codes.Empty });
		}

		public static CodeDescriptionPairList GetAddInWithAmountAttribute(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, new ZString[] { RefCusCodeListAttributeTypes.Codes.Amount });
		}

		public static CodeDescriptionPairList GetAddInWithAllowEmptyAttribute(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, new ZString[] { RefCusCodeListAttributeTypes.Codes.AllowEmpty });
		}

		public static CodeDescriptionPairList GetAddInWithAllowSpaceAttribute(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, new ZString[] { RefCusCodeListAttributeTypes.Codes.AllowSpace });
		}

		public static ZString[] GetAdditionalInformationsMappedToSchedule(BusinessObjectFactory factory, ZDateTime date, ZString schedule)
		{
			ZString[] result = null;
			if (factory != null && !schedule.IsEmpty)
			{
				factory.GetCachedValue("ZAAdditionalInformationListMappedToSchedule", () =>
				{
					var dictionaryList = new Dictionary<ZString, List<ZString>>();
					var loadedCodes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, addAttributeFetchHints: true);
					foreach (var loadedCode in loadedCodes)
					{
						foreach (var value in loadedCode.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Where(x => !x.ZZE_Value.IsEmpty && x.ZZE_ZXE_NKName.EqualsIgnoringCase(RefCusCodeListAttributeTypes.Codes.Schedule)).Select(x => x.ZZE_Value).ToArray())
						{
							List<ZString> list;
							if (!dictionaryList.TryGetValue(value, out list))
							{
								list = new List<ZString>();
								dictionaryList.Add(value, list);
							}
							list.Add(loadedCode.ZZD_Code);
						}
					}
					var dictionary = new Dictionary<ZString, ZString[]>();
					foreach (var pair in dictionaryList.OrderBy(x => x.Key))
					{
						var list = pair.Value;
						list.Sort();
						dictionary.Add(pair.Key, list.ToArray());
					}
					return dictionary;
				}).TryGetValue(schedule, out result);
			}
			return result ?? System.Array.Empty<ZString>();
		}

		public static ZString[] GetAdditionalInformationAttributeValuesFor(BusinessObjectFactory factory, ZDateTime date, ZString code, ZString attributeName)
		{
			return RefCusCodeListAttributeTypes.GetAttributeValuesFor(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, code, attributeName);
		}

		public static CodeDescriptionPairList GetAdditionalInformationList(BusinessObjectFactory factory, ZDateTime date, bool isExport, bool isLine1)
		{
			CodeDescriptionPairList result = null;
			if (factory != null)
			{
				factory.GetCachedValue<IDictionary<ZString, CodeDescriptionPairList>>("ZAAdditionalInformationList", () =>
				{
					var dictionary = new Dictionary<ZString, CodeDescriptionPairList>();
					var loadedCodes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, addAttributeFetchHints: true);
					if (loadedCodes.Length > 0)
					{
						foreach (var loadedCode in loadedCodes)
						{
							var hasOnlyForLine1Attribute = false;
							var hasImportAttribute = false;
							var hasExportAttribute = false;
							foreach (var attribute in loadedCode.Attributes.Cast<ZZRefCusCodeListAttributeCombined>())
							{
								switch (attribute.ZZE_ZXE_NKName)
								{
									case RefCusCodeListAttributeTypes.Codes.OnlyForLine1:
										hasOnlyForLine1Attribute = true;
										break;
									case RefCusCodeListAttributeTypes.Codes.Import:
										hasImportAttribute = true;
										break;
									case RefCusCodeListAttributeTypes.Codes.Export:
										hasExportAttribute = true;
										break;
								}
								if (hasOnlyForLine1Attribute && hasImportAttribute && hasExportAttribute)
								{
									break;
								}
							}
							var isAttributeForBoth = hasImportAttribute == hasExportAttribute;
							if (hasExportAttribute || isAttributeForBoth)
							{
								AddAdditionalInformation(dictionary, GetAdditionalInformationKey(true, true), loadedCode);
								if (!hasOnlyForLine1Attribute)
								{
									AddAdditionalInformation(dictionary, GetAdditionalInformationKey(true, false), loadedCode);
								}
							}
							if (hasImportAttribute || isAttributeForBoth)
							{
								AddAdditionalInformation(dictionary, GetAdditionalInformationKey(false, true), loadedCode);
								if (!hasOnlyForLine1Attribute)
								{
									AddAdditionalInformation(dictionary, GetAdditionalInformationKey(false, false), loadedCode);
								}
							}
						}
						foreach (var list in dictionary.Values)
						{
							list.Sort();
						}
					}
					return dictionary;
				}).TryGetValue(GetAdditionalInformationKey(isExport, isLine1), out result);
			}
			return result ?? new CodeDescriptionPairList();
		}

		static void AddAdditionalInformation(Dictionary<ZString, CodeDescriptionPairList> dictionary, string key, ZZRefCusCodeListCombined loadedCode)
		{
			CodeDescriptionPairList list;
			if (!dictionary.TryGetValue(key, out list))
			{
				list = new CodeDescriptionPairList();
				dictionary.Add(key, list);
			}
			list.Add(loadedCode);
		}

		static string GetAdditionalInformationKey(bool isExport, bool isLine1)
		{
			return string.Format(Culture.Invariant, "{0}_{1}", isExport, isLine1);
		}

		public static CodeDescriptionPairList GetAddInWithCusApprovedExporterAttribute(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, date, new ZString[] { RefCusCodeListAttributeTypes.Codes.CusApprovedExporter });
		}

		public static CodeDescriptionPairList GetZADocumentTypeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ZADocumentType, ZDateTime.Today);
		}
	}
}
