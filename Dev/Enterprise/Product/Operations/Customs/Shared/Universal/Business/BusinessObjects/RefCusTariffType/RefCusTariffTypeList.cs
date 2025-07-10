using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public static class RefCusTariffTypeList
	{
		public static ICodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, ZString country)
		{
			var key = string.Format(CultureInfo.InvariantCulture, "RefCusTariffTypeList_{0}", country);
			return factory.GetCachedValue<ICodeDescriptionPairList>(key, () =>
			{
				var result = new UntranslatableCodeDescriptionPairList(Res.GetString("{3AC2439E-AE5E-4E29-A0FD-BDFEC5431CC2}", "The description is already in the correct language."));
				var selectedTariffTypes = GetTariffTypesForCountry(factory, country);
				if (selectedTariffTypes != null)
				{
					result.AddRange(selectedTariffTypes);
				}
				return result;
			});
		}

		public static RefCusTariffType[] GetTariffTypesForCountry(BusinessObjectFactory factory, ZString country)
		{
			var query = RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, country);
			query.OrderBy = RefCusTariffTypeSchema.Constants.ZZI_TariffType;
			return factory.Load<RefCusTariffType>(query);
		}
	}
}
