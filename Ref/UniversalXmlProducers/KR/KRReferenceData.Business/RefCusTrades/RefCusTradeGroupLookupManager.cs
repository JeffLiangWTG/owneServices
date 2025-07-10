using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class RefCusTradeGroupLookupManager : ITopEntityLookupManager<RefCusTradeGroup>
	{
		public static RefCusTradeGroup GetTopEntity(Dictionary<string, RefCusTradeGroup> topEntitiesByKey, KeyLookupDetails<RefCusTradeGroup> keyDetails)
		{
			var keyString = keyDetails.GetCombinedKeyValues();
			RefCusTradeGroup result = null;
			if (!string.IsNullOrEmpty(keyString) && !topEntitiesByKey.TryGetValue(keyString, out result))
			{
				result = (RefCusTradeGroup)Activator.CreateInstance(typeof(RefCusTradeGroup));
			}
			return result;
		}
		RefCusTradeGroup ITopEntityLookupManager<RefCusTradeGroup>.GetTopEntity(Dictionary<string, RefCusTradeGroup> topEntitiesByKey, KeyLookupDetails<RefCusTradeGroup> keyDetails) => GetTopEntity(topEntitiesByKey, keyDetails);
	}
}
