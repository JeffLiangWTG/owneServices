using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.GUI
{
	public static class DataGroupingRelatedFilterHelper
	{
		public static class ListGetters
		{
			public static Func<BusinessObjectFactory, ZString, ICodeDescriptionPairList> TariffTypeGetter => GetTariffTypeList;

			static ICodeDescriptionPairList GetTariffTypeList(BusinessObjectFactory factory, ZString dataGrouping)
			{
				return factory.GetCachedValue($"DataGroupingRelatedFilterHelper|TariffTypeList_{dataGrouping}", () =>
				{
					var result = new CodeDescriptionPairList();
					var refCusTariffTypeList = RefCusTariffTypeList.GetCachedList(factory, dataGrouping);
					if (refCusTariffTypeList.Count > 0)
					{
						result.AddRange(refCusTariffTypeList);
					}
					else
					{
						result.Add(new CodeDescriptionPair(Constants.TariffTypes.HarmonizedSystem,
							Res.GetString("8C00EC75-AB49-4F41-8227-86C54A62E3C3", "Harmonized System Nomenclature")));
					}
					return result;
				});
			}
		}

		#region Default ResourceString Getters

		public static class ResourceStringGetters
		{
			public static ResourceStringData TariffTypeResourceString => Res.GetData("{28D1CF10-5662-45DF-ADF9-8829577AC13C}", "Tariff Type");
		}

		#endregion
	}
}
