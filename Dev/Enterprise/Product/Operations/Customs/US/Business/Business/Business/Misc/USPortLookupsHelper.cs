using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public static class USPortLookupsHelper
	{
		public static ZZRefCusCodeListCombinedCollection GetForeignPorts(BusinessObjectFactory factory, List<ZString> matches)
		{
			return factory.GetCachedValue(string.Join(",", matches.OrderBy(x => x)) + ZDateTime.Today, delegate
			{
				var result = new ZZRefCusCodeListCombinedCollection(factory);

				if (matches.Count > 0)
				{
					var foreignPorts = ZZRefCusCodeListCombined.Loader.LoadForCodes(
						factory,
						Core.Constants.CountryCodes.UnitedStates,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
						matches.ToArray(),
						ZDateTime.Today,
						null);
					result.AddRange(foreignPorts);
				}

				return result;
			});
		}

		public static ZZRefCusCodeListCombinedCollection GetRegionDistrictPorts(BusinessObjectFactory factory, List<ZString> matches)
		{
			return factory.GetCachedValue(string.Join(",", matches.OrderBy(x => x)) + ZDateTime.Today, delegate
			{
				var result = new ZZRefCusCodeListCombinedCollection(factory);

				if (matches.Count > 0)
				{
					var foreignPorts = ZZRefCusCodeListCombined.Loader.LoadForCodes(
						factory,
						Core.Constants.CountryCodes.UnitedStates,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
						matches.ToArray(),
						ZDateTime.Today,
						null);
					result.AddRange(foreignPorts);
				}

				return result;
			});
		}

		public static ZZRefCusCodeListCombinedCollection GetCustomsOfficeCodes(BusinessObjectFactory factory, List<ZString> matches)
		{
			return factory.GetCachedValue("CustomsOfficeCode:" + string.Join(",", matches.OrderBy(x => x)) + ZDateTime.Today, delegate
			{
				var result = new ZZRefCusCodeListCombinedCollection(factory);
				if (matches.Count > 0)
				{
					var codeLists = ZZRefCusCodeListCombined.Loader.LoadForCodes(factory,
						Core.Constants.CountryCodes.UnitedStates,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
						matches.ToArray(),
						ZDateTime.Today,
						new List<RefCusCodeListAttributeFilter>() { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, SQLComparisonOperator.Equal, "EXP") });
					result.AddRange(codeLists);
				}

				return result;
			});
		}
	}
}
