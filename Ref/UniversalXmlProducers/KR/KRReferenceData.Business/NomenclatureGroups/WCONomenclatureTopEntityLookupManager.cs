using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.Formula.Functions;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class WCONomenclatureTopEntityLookupManager : ITopEntityLookupManager<RefCusNomenclatureGroup>
	{
		public WCONomenclatureTopEntityLookupManager(IEnumerable<RefCusNomenclatureGroup> wcoNomenclatures)
		{
			this.wcoNomenclatures = wcoNomenclatures;
		}
		readonly IEnumerable<RefCusNomenclatureGroup> wcoNomenclatures;

		public RefCusNomenclatureGroup GetTopEntity(Dictionary<string, RefCusNomenclatureGroup> topEntitiesByKey, KeyLookupDetails<RefCusNomenclatureGroup> keyDetails)
		{
			IEnumerable<RefCusNomenclatureGroup> existingEntities = wcoNomenclatures;
			RefCusNomenclatureGroup result = null;
			var refCusNomenclatureGroupType = typeof(RefCusNomenclatureGroup);

			foreach (string key in keyDetails.KeyFieldNames)
			{
				string value = null;

				if (keyDetails.KeyValues.TryGetValue(key, out value))
				{
					existingEntities = existingEntities.Where(x => refCusNomenclatureGroupType.GetProperty(key).GetValue(x).ToString() == value);

					result = existingEntities.SingleOrDefault();
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		RefCusNomenclatureGroup ITopEntityLookupManager<RefCusNomenclatureGroup>.GetTopEntity(Dictionary<string, RefCusNomenclatureGroup> topEntitiesByKey, KeyLookupDetails<RefCusNomenclatureGroup> keyDetails) => GetTopEntity(topEntitiesByKey, keyDetails);
	}
}
