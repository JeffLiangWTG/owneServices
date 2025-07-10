using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public static class RefCusCodeListAttributeNameList
	{
		public static ActiveBusinessObjectCollection<RefCusCodeListAttributeName> GetList(BusinessObjectFactory factory, ZString codeType, ZString countryOrGrouping, bool allowEmpty = false)
			=> GetList(factory, codeType, new[] { countryOrGrouping }, allowEmpty);

		public static ActiveBusinessObjectCollection<RefCusCodeListAttributeName> GetList(BusinessObjectFactory factory, ZString codeType, ZString[] countryOrGroupings, bool allowEmpty = false)
		{
			countryOrGroupings = countryOrGroupings.Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToArray();

			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "UniveralRefCusCodeListAttributeNameList_{0}_{1}_{2}", codeType, string.Join(",", countryOrGroupings), allowEmpty), () =>
			{
				var query = ZQuery.NoResultQuery;
				if (allowEmpty || (!codeType.IsEmpty && countryOrGroupings.Length > 0))
				{
					query = new ZQuery();

					if (countryOrGroupings.Length > 0)
					{
						query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZZ_NKDataGrouping, countryOrGroupings);
					}
					if (!allowEmpty || !codeType.IsEmpty)
					{
						query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZK_NKCodeType, codeType);
					}
				}

				var result = new ActiveBusinessObjectCollection<RefCusCodeListAttributeName>(factory, query);
				result.ApplySort(RefCusCodeListAttributeNameSchema.Constants.ZXE_Name, System.ComponentModel.ListSortDirection.Ascending);
				return result;
			});
		}

		public static ICodeDescriptionPairList GetListForFilter(BusinessObjectFactory factory, ZString codeType, ZString countryOrGrouping, bool allowEmpty = false)
			=> GetListForFilter(factory, codeType, new[] { countryOrGrouping }, allowEmpty);

		public static ICodeDescriptionPairList GetListForFilter(BusinessObjectFactory factory, ZString codeType, ZString[] countryOrGroupings, bool allowEmpty = false)
		{
			countryOrGroupings = countryOrGroupings.Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToArray();

			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "UniveralRefCusCodeListAttributeNameListForFilter_{0}_{1}_{2}", codeType, string.Join(",", countryOrGroupings), allowEmpty), () =>
			{
				var list = GetList(factory, codeType, countryOrGroupings, allowEmpty);
				var result = new CodeDescriptionPairList();

				foreach (var attributeName in list)
				{
					var translatedName = attributeName.TranslatedName;
					var translatedDesc = attributeName.ZXE_Description;
					result.AddPair(attributeName.ZXE_Name, translatedName == attributeName.ZXE_Name ? translatedDesc : ZString.Format("{0} - {1}", translatedName, translatedDesc));
				}
				return result;
			});
		}
	}
}
