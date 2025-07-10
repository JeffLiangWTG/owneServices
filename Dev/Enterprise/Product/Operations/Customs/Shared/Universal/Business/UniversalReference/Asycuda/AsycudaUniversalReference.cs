using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public static class AsycudaUniversalReference
	{
		public static class RefCusCodeListTypes
		{
			public static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, ZString country, ZString codeType, ZString[] attributeNames = null)
			{
				return Universal.RefCusCodeListTypes.GetCachedList(factory, country, codeType, ZDateTime.Today, attributeNames);
			}

			public static UntranslatableCodeDescriptionPairList GetCachedUntranslatableList(BusinessObjectFactory factory, ZString country, ZString codeType, ZString[] attributeNames = null)
			{
				var result =
					new UntranslatableCodeDescriptionPairList(
						(NoResString)"GetCachedList fetches strings from the database and does not need translation");

				result.AddRange(GetCachedList(factory, country, codeType, attributeNames));

				return result;
			}

			public static CodeDescriptionPairList GetCachedListMatchSingleAttributeValues(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, bool matchIfAttributeNotExists, ZString attributeName, ZString[] attributeValues, string transportMode)
			{
				return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, country, codeType, date, matchIfAttributeNotExists, attributeName, attributeValues, transportMode);
			}
		}

		public static class CusRefPackLoaderHelper
		{
			public static void MessageErrorIfNeeded(string countryCode, string commercialUQ, ZPropertyInfo zPropertyInfoForWarning, BusinessObjectFactory factory)
			{
				Universal.BusinessObjects.ZZRefCusCodeListCombined.CusRefPackLoaderHelper.MessageErrorIfNeeded(countryCode, commercialUQ, ZDateTime.Today, zPropertyInfoForWarning, factory);
			}
		}

		public static class CustomsStatusAttributeHelper
		{
			public static bool IsCustomsCleared(BusinessObjectFactory factory, ZString country, ZString customsStatus)
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, customsStatus, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, ZDateTime.Now)
							?.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsCleared) ?? false;
			}

			public static bool ShouldAddEntryDocsToEDocs(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime date)
			{
				var statusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, date);
				return statusCode?.HasAttribute(RefCusCodeListAttributeTypes.Codes.IAddEntryDocsToEDocs) ?? false;
			}
		}
	}
}
