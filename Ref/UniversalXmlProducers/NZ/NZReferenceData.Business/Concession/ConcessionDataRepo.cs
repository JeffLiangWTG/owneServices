using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public sealed class ConcessionDataRepo : ITopLevelDataRepo<RefCusTariff>
	{
		public ConcessionDataRepo()
		{
			tariffs = new Dictionary<string, RefCusTariff>();
		}

		readonly Dictionary<string, RefCusTariff> tariffs;

		#region IDataRepo

		DateTime IDataRepo.PublicationTime { get; set; }

		void IDataRepo.Add(string key, RefDataRepoModelEntityType data)
		{
			if (data is RefCusTariff tariff)
			{
				tariffs.Add(key, tariff);
			}
			else
			{
				throw new InvalidOperationException("Trying add unexpected data.");
			}
		}

		void IDataRepo.AddRange<TKey, TValue>(IDictionary<TKey, TValue> dataPairs)
		{
			if (dataPairs is IDictionary<string, RefCusTariff> tariffPairs)
			{
				foreach (var pair in tariffPairs)
				{
					tariffs.Add(pair.Key, pair.Value);
				}
			}
			else
			{
				throw new InvalidOperationException("Trying add unexpected data.");
			}
		}

		void IDataRepo.Remove(string key, RefDataRepoModelEntityType data)
		{
			if (data is RefCusTariff)
			{
				tariffs.Remove(key);
			}
			else
			{
				throw new InvalidOperationException("Trying remove unexpected data.");
			}
		}

		void IDataRepo.RemoveInvalidData()
		{
			var emptyTariffKeys = tariffs.Where(x => x.Value.RefCusRates == null).Select(x => x.Key).ToArray();
			foreach (var key in emptyTariffKeys)
			{
				tariffs.Remove(key);
			}
		}

		public void Sort()
		{
		}

		void IDataRepo.Clear()
		{
			tariffs.Clear();
		}

		#endregion

		#region ITopLevelDataRepo

		IReadOnlyCollection<RefCusTariff> ITopLevelDataRepo<RefCusTariff>.Get() => tariffs.Values.ToArray();

		RefCusTariff ITopLevelDataRepo<RefCusTariff>.Load(string key)
		{
			tariffs.TryGetValue(key, out var result);
			return result;
		}

		#endregion
	}
}
