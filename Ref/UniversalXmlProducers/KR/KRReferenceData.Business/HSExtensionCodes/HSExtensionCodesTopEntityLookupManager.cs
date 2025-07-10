using System.Collections.Generic;
using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class HSExtensionCodesTopEntityLookupManager : ITopEntityLookupManager<RefCusTariff>
	{
		public static RefCusTariff GetTopEntity(Dictionary<string, RefCusTariff> topEntitiesByKey, KeyLookupDetails<RefCusTariff> keyDetails)
		{
			RefCusTariff result = null;
			var keyString = keyDetails.GetCombinedKeyValues();
			if (!string.IsNullOrEmpty(keyString) && !topEntitiesByKey.TryGetValue(keyString, out result))
			{
				result = (RefCusTariff)Activator.CreateInstance(typeof(RefCusTariff));
			}
			return result;
		}

		RefCusTariff ITopEntityLookupManager<RefCusTariff>.GetTopEntity(Dictionary<string, RefCusTariff> topEntitiesByKey, KeyLookupDetails<RefCusTariff> keyDetails) => GetTopEntity(topEntitiesByKey, keyDetails);
	}
}
