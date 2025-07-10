using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business
{
	public static class TWRefCusCodeListTypes
	{
		public static CodeDescriptionPairList GetMethodOfCalculationList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("TWRefCusCodeListTypes.GetMethodOfCalculationList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(GetCustomsPackUnitsList(factory));
				list.AddPair(UniversalReferenceConstants.MethodOfCalculation.Percentage);
				list.Sort();
				return list;
			});
		}

		public static CodeDescriptionPairList GetCategoryCodesOfCAAAircraftParts(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACodeCategory, ZDateTime.Today);
		}

		public static AircraftPartsCodeDescriptionPairList GetCAAAircraftPartsCodes(BusinessObjectFactory factory, ZString category)
		{
			return factory.GetCachedValue("TWRefCusCodeListTypes.GetCAAAircraftPartsCodes_" + category, () =>
			{
				var result = new AircraftPartsCodeDescriptionPairList();
				if (!category.IsEmpty)
				{
					var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Taiwan, Codes.TaiwanAircraftPartCAACode, ZDateTime.Today);
					collection.Load();
					category = string.Format("{0}.", category);
					collection.Where(x => x.ZZD_Code.StartsWith(category)).ToList().ForEach(x => result.AddPair(x.ZZD_Code.Replace(category, ZString.Empty), x.ZZD_Description));
					result.Sort();
				}
				return result;
			});
		}

		public static CodeDescriptionPairList GetCustomsOfficeList(BusinessObjectFactory factory, JobDeclaration declaration)
		{
			var result = new CodeDescriptionPairList();
			var transportMode = declaration?.TransportMode ?? ZString.Empty;
			if (transportMode == TransportTypeList.Codes.Sea || transportMode == TransportTypeList.Codes.Air)
			{
				result = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsOffice, ZDateTime.Today, null, transportMode, false);
			}
			return result;
		}

		public static CodeDescriptionPairList GetCustomsOfficeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsOffice, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCustomsOfficeList(BusinessObjectFactory factory, CusInBondHeader header)
		{
			var transportMode = ZString.Empty;
			switch (header?.BH_ImportTransportMode ?? ZString.Empty)
			{
				case InBondTransportModeCodes.Codes.SEA:
					transportMode = TransportTypeList.Codes.Sea;
					break;
				case InBondTransportModeCodes.Codes.AIR:
					transportMode = TransportTypeList.Codes.Air;
					break;
			}

			CodeDescriptionPairList result;
			if (transportMode.IsEmpty)
			{
				result = GetCustomsOfficeList(factory);
			}
			else
			{
				result = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsOffice, ZDateTime.Today, null, transportMode, false);
			}
			return result;
		}

		public static IBusinessObjectCollection GetGoodsLocationCollection(BusinessObjectFactory factory, IZType value)
		{
			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory,
				Core.Constants.CountryCodes.Taiwan,
				Codes.Facilities,
				ZDateTime.Today);

			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", new ZString(RefCusCodeListAttributeTypes.Codes.CustomsOffice)));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", value));
			return collection;
		}

		public static IBusinessObjectCollection GetCustomsOfficeCollection(BusinessObjectFactory factory) => ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsOffice, ZDateTime.Today);

		public static CodeDescriptionPairList GetContactOfficeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWReceivingUnit, ZDateTime.Today, null, ZString.Empty, false);
		}

		public static CodeDescriptionPairList GetControllingMessageTypeList(BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWControllingMessageMessageType, ZDateTime.Today);

		public static IEnumerable<ZZRefCusCodeListCombined> GetControllingAgencyCollection(BusinessObjectFactory factory, ZString messageType)
		{
			var result = Enumerable.Empty<ZZRefCusCodeListCombined>();
			var attributes = GetControllingMessageMessageTypeRefCusCodeListCombined(factory, messageType)?.Attributes;
			if (attributes != null)
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWControllingAgency, ZDateTime.Today);
				collection.Load();
				result = collection.Where(x => attributes?.HasAttribute(RefCusCodeListAttributeTypes.Codes.ControlAgency, x.ZZD_Code) ?? false);
			}
			return result;
		}

		internal static ZZRefCusCodeListCombined GetControllingMessageMessageTypeRefCusCodeListCombined(BusinessObjectFactory factory, ZString messageType)
		{
			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWControllingMessageMessageType, ZDateTime.Today);
			collection.Load();
			return collection.Where(x => x.ZZD_Code == messageType)?.FirstOrDefault();
		}

		public static CodeDescriptionPairList GetControllingAgencyList(BusinessObjectFactory factory, bool controllingMessage = false)
		{
			return factory.GetCachedValue("TWRefCusCodeListTypes.GetControllingAgencyList_" + controllingMessage.ToString(), () =>
			{
				CodeDescriptionPairList list;
				if (controllingMessage)
				{
					list = new ControllingAgencyList();
				}
				else
				{
					list = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWControllingAgency, ZDateTime.Today, null, ZString.Empty, false);
				}

				return list;
			});
		}

		public static CodeDescriptionPairList GetCustomsPackUnitsList(BusinessObjectFactory factory, string languageCode = "")
		{
			if (string.IsNullOrEmpty(languageCode))
			{
				languageCode = TranslationHelper.GetCurrentLanguageCode();
			}
			return factory.GetCachedValue("TWRefCusCodeListTypes.GetPackingUnitsOfMeasurementList" + languageCode, ()
				=> RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWCustomsPackUnits, ZDateTime.Today, languageCode: languageCode));
		}

		public static CodeDescriptionPairList GetCommercialPackUnitsList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWCommercialPackUnits, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetProcessingUnitList(BusinessObjectFactory factory, ZString value, ZDateTime date, bool isMessageTypeNX101 = false)
		{
			CodeDescriptionPairList list;
			if (isMessageTypeNX101)
			{
				list = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginIssuingUnit, date, false, RefCusCodeListAttributeTypes.Codes.Type, new[] { value });
			}
			else
			{
				list = RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, Core.Constants.CountryCodes.Taiwan, Codes.TWReceivingUnit, date, true, RefCusCodeListAttributeTypes.Codes.ControlAgency, new[] { value });
			}
			return list;
		}

		public static ZString GetBankAccountNoByCustomsOfficeCode(BusinessObjectFactory factory, ZString code)
		{
			return RefCusCodeListAttributeTypes.GetAttributeValuesFor(factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsOffice, ZDateTime.Today, code, RefCusCodeListAttributeTypes.Codes.BankAccountNo).FirstOrDefault();
		}

		public static IBusinessObjectCollection GetPackingHouseList(BusinessObjectFactory factory, ZString attributeName1, ZString attributeValue1, ZString attributeName2, ZString attributeValue2)
		{
			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory,
				Core.Constants.CountryCodes.Taiwan,
				Codes.TaiwanPackingHouse,
				ZDateTime.Today);

			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(attributeName1, "Property", attributeValue1));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(attributeName2, "Property", attributeValue2));
			return collection;
		}

		public static IEnumerable<ZString> GetPackingHouseTariffs(BusinessObjectFactory factory)
		{
			var collection = ZZRefCusCodeListCombined.Loader.Load(factory,
				Core.Constants.CountryCodes.Taiwan,
				Codes.TaiwanPackingHouse,
				ZDateTime.Today);
			return collection.Cast<ZZRefCusCodeListCombined>().Where(x => x.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff)).Select(x => x.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Tariff));
		}
	}
}
