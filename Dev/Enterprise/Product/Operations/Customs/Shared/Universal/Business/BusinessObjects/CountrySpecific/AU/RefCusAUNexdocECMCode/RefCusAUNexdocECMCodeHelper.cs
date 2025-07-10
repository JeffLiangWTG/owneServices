using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public static class RefCusAUNexdocECMCodeHelper
	{
		const string supplementaryCode = "SupplementaryCode";
		const string supplementaryDesc = "SupplementaryDesc";
		const string packTypeCode = "PackTypeCode";
		const string packTypeDesc = "PackTypeDesc";
		const string preservationCode = "PreservationCode";
		const string preservationDesc = "PreservationDesc";

		static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, Func<ZString> getKey, Func<CodeDescriptionPairList> createList)
		{
			return factory.GetCachedValue(getKey(), () => createList());
		}

		public static CodeDescriptionPairList GetSupplementaryCachedList(BusinessObjectFactory factory, char commodityCode, ZString productTypeCode, ZDateTime date)
		{
			return GetCachedList(factory, () => "RefCusAUNexdocECMCode_Supplementary_" + GetKey(commodityCode, productTypeCode, date.Date), () => CreateList(factory, commodityCode, productTypeCode, date.Date, supplementaryCode, supplementaryDesc));
		}

		public static CodeDescriptionPairList GetPackTypeCachedList(BusinessObjectFactory factory, char commodityCode, ZString productTypeCode, ZDateTime date)
		{
			return GetCachedList(factory, () => "RefCusAUNexdocECMCode_PackType_" + GetKey(commodityCode, productTypeCode, date.Date), () => CreateList(factory, commodityCode, productTypeCode, date.Date, packTypeCode, packTypeDesc));
		}

		public static CodeDescriptionPairList GetPreservationCachedList(BusinessObjectFactory factory, char commodityCode, ZString productTypeCode, ZDateTime date)
		{
			return GetCachedList(factory, () => "RefCusAUNexdocECMCode_Preservation_" + GetKey(commodityCode, productTypeCode, date.Date), () => CreateList(factory, commodityCode, productTypeCode, date.Date, preservationCode, preservationDesc));
		}

		static CodeDescriptionPairList CreateList(BusinessObjectFactory factory, char commodityCode, ZString productTypeCode, ZDate date, ZString code, ZString desc)
		{
			var dynamicBOs = GetCachedCollection(factory, commodityCode, productTypeCode, date);
			var list = new CodeDescriptionPairList();
			var loadedCodes = dynamicBOs.Select(x => new { Code = new ZString(x[code]), Description = new ZString(x[desc]) }).Where(x => !x.Description.IsEmpty).Distinct();
			if (loadedCodes.Any())
			{
				foreach (var codeList in loadedCodes)
				{
					list.AddPair(codeList.Code, codeList.Description);
				}
			}
			list.Sort();
			return list;
		}

		public static DynamicBusinessObjectCollection GetCachedCollection(BusinessObjectFactory factory, char commodityCode, ZString productTypeCode, ZDateTime date)
		{
			return GetCachedCollection(factory, () => "RefCusAUNexdocECMCode_" + GetKey(commodityCode, productTypeCode, date.Date), () => CreateBusinessObjectCollection(factory, commodityCode, productTypeCode, date.Date));
		}

		static DynamicBusinessObjectCollection GetCachedCollection(BusinessObjectFactory factory, Func<ZString> getKey, Func<DynamicBusinessObjectCollection> createList)
		{
			return factory.GetCachedValue(getKey(), () => createList());
		}

		public static ZString GetKey(char commodityCode, ZString productTypeCode, ZDate date)
		{
			var result = new ZStringBuilder(commodityCode.ToString());
			result.Append(productTypeCode.PadRight(AutoRefCusAUNexdocECMCode.Schema.ZY5_ProductTypeCodeMaxLength));
			result.Append(date.ToString("yyMMdd", CultureInfo.InvariantCulture));
			return result.ToStringWithDelimiterBetweenAppends("_");
		}

		static DynamicBusinessObjectCollection CreateBusinessObjectCollection(BusinessObjectFactory factory, char commodityCode, ZString productTypeCode, ZDate date)
		{
			var dynamicBOs = new DynamicBusinessObjectCollection(factory);

			if (commodityCode != ' ' && !productTypeCode.IsEmpty)
			{
				dynamicBOs.Load(System.FormattableString.Invariant($@"SELECT ZY5_SupplementaryCode AS {supplementaryCode}, ISNULL(Supplementary.ZZD_Description, '') AS {supplementaryDesc},
	ZY5_PackTypeCode AS {packTypeCode}, ISNULL(Pack.ZZD_Description, '') AS {packTypeDesc},
	ZY5_PreservationCode AS {preservationCode}, ISNULL(Preservation.ZZD_Description, '') AS {preservationDesc}
FROM RefDatabase_RefCusAUNexdocECMCode
LEFT JOIN RefDatabase_RefCusCodeList Supplementary ON Supplementary.ZZD_Code = ZY5_SupplementaryCode AND Supplementary.ZZD_ZZZ_NKDataGrouping = 'AU' AND Supplementary.ZZD_ZZK_NKCodeType = 'NSUPP' AND Supplementary.ZZD_StartDate <= '{date}' AND Supplementary.ZZD_EndDate >= '{date}'
LEFT JOIN RefDatabase_RefCusCodeList Pack ON Pack.ZZD_Code = ZY5_PackTypeCode AND Pack.ZZD_ZZZ_NKDataGrouping = 'AU' AND Pack.ZZD_ZZK_NKCodeType = 'NPCKT' AND Pack.ZZD_StartDate <= '{date}' AND Pack.ZZD_EndDate >= '{date}'
LEFT JOIN RefDatabase_RefCusCodeList Preservation ON Preservation.ZZD_Code = ZY5_PreservationCode AND Preservation.ZZD_ZZZ_NKDataGrouping = 'AU' AND Preservation.ZZD_ZZK_NKCodeType = 'NPRST' AND Preservation.ZZD_StartDate <= '{date}' AND Preservation.ZZD_EndDate >= '{date}'
WHERE ZY5_ProductTypeCode = '{productTypeCode}' AND ZY5_CommodityCode = '{commodityCode}'"));
			}
			return dynamicBOs;
		}
	}
}
