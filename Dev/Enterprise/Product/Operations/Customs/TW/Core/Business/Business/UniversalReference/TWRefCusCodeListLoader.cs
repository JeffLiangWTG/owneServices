using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.Business
{
	public static class TWRefCusCodeListLoader
	{
		public static ZZRefCusCodeListCombined GetCustomsOffice(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetTWCodeByType(factory, code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, date);
		}

		public static ZZRefCusCodeListCombined GetLocationOfGoods(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetTWCodeByType(factory, code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, date);
		}

		public static ZZRefCusCodeListCombined GetPackingHouse(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return GetTWCodeByType(factory, code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, date);
		}

		public static ZZRefCusCodeListCombined GetTWCodeByType(BusinessObjectFactory factory, ZString code, ZString codeType, ZDateTime date)
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.Taiwan, codeType, date);
		}

		public static ZZRefCusCodeListCombined[] GetICIPlacesByCustomsOffice(BusinessObjectFactory factory, ZString officeCode, ZDateTime date)
		{
			var attributeFilter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsOffice, SQLComparisonOperator.Equal, officeCode) };
			var places = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, date, attributeFilter);
			if (places.Length == 0 && officeCode.Length > 1)
			{
				attributeFilter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsOffice, SQLComparisonOperator.StartsWith, officeCode.Left(1)) };
				places = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, date, attributeFilter);
			}
			return places;
		}

		public static ZString GetSingleLocationOfGoodsCodeOrDefault(BusinessObjectFactory factory, ZString officeCode, ZDateTime date)
		{
			var attributeFilter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsOffice, SQLComparisonOperator.StartsWith, officeCode) };
			var locationOfGoodsCollection = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, date, attributeFilter);
			var list = locationOfGoodsCollection.Cast<CargoWise.Integration.ICodeDescription>();
			return list.Count() == 1 ? list.Single().Code : string.Empty;
		}

		public static ZZRefCusCodeListCombined[] GetExportDeclarationType(BusinessObjectFactory factory, ZDateTime date)
		{
			var isExportFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsExport, SQLComparisonOperator.Equal, YesNoList.Codes.Yes);
			var notInIsImportFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsImport, JoinCondition.And, true, new ZString[] { YesNoList.Codes.Yes });
			var notInIsExpressDeliveryFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, JoinCondition.And, true, new ZString[] { YesNoList.Codes.Yes });
			var attributeFilter = new RefCusCodeListAttributeFilter[] { isExportFilter, notInIsImportFilter, notInIsExpressDeliveryFilter };
			var exportDeclarationTypeCollection = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, date, attributeFilter);
			return exportDeclarationTypeCollection;
		}

		public static ZZRefCusCodeListCombined[] GetImportDeclarationType(BusinessObjectFactory factory, ZDateTime date)
		{
			var isImportFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsImport, SQLComparisonOperator.Equal, YesNoList.Codes.Yes);
			var notInIsExportFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsExport, JoinCondition.And, true, new ZString[] { YesNoList.Codes.Yes });
			var notInIsExpressDeliveryFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.IsExpressDelivery, JoinCondition.And, true, new ZString[] { YesNoList.Codes.Yes });
			var attributeFilter = new RefCusCodeListAttributeFilter[] { isImportFilter, notInIsExportFilter, notInIsExpressDeliveryFilter };
			var importDeclarationTypeCollection = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, date, attributeFilter);
			return importDeclarationTypeCollection;
		}

		public static ZZRefCusCodeListCombined GetProcessingUnit(BusinessObjectFactory factory, bool isMessageTypeNX101, ZString code, ZDateTime date)
		{
			var codeType = isMessageTypeNX101 ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginIssuingUnit : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit;
			return GetTWCodeByType(factory, code, codeType, date);
		}

		public static ZString LoadTaiwanRefCusCodeListDescription(BusinessObjectFactory factory, ZString code, ZString codeType) => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.Taiwan, codeType, ZDateTime.Today)?.ZZD_DescriptionInfo?.OriginalValue?.ToString() ?? ZString.Empty;
	}
}
