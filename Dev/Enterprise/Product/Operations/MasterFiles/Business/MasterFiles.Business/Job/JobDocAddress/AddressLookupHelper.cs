using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class AddressLookupHelper
	{
		public static CodeDescriptionPairList GetStateList(this BusinessObjectFactory factory, ZString countryCode, ZBool hasErrors)
		{
			if (countryCode.IsEmpty || hasErrors || factory == null)
			{
				return new CodeDescriptionPairList();
			}

			var dictionary = GetDictionary(factory);
			CodeDescriptionPairList stateList;
			if (!dictionary.TryGetValue(countryCode, out stateList))
			{
				var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				stateList = new CodeDescriptionPairList();
				if (country != null)
				{
					foreach (var state in country.States.Where(x => x.RW_IsActive))
					{
						stateList.AddPair(state.RW_Code, state.RW_DescriptionMultilingual);
					}
				}
				dictionary.Add(countryCode, stateList);
			}
			return stateList;
		}

		public static void ClearStateList(this BusinessObjectFactory factory, ZString countryCode)
		{
			if (!countryCode.IsEmpty && factory != null)
			{
				var dictionary = GetDictionary(factory);
				dictionary.Remove(countryCode);
			}
		}

		static Dictionary<ZString, CodeDescriptionPairList> GetDictionary(BusinessObjectFactory factory) => factory.GetCachedValue("CountryStateListDictionary", () => new Dictionary<ZString, CodeDescriptionPairList>());
	}
}
