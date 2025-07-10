using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Cartonisation.Integration;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	class LocalSearchSolution
	{
		public LocalSearchSolution()
		{
			cartons = new List<CartonItems>();
		}

		public LocalSearchSolution(IEnumerable<CartonItems> lines)
		{
			cartons = lines.ToList();
		}

		internal IReadOnlyCollection<CartonItems> Cartons => cartons;
		readonly List<CartonItems> cartons;

		public void AddCartons(IEnumerable<CartonItems> items)
		{
			ClearCachedValues();
			cartons.AddRange(items);
		}

		void ClearCachedValues()
		{
			result = null;
			totalVolume = null;
			totalCost = null;
			totalPackedItems = null;
			totalDistinctProductsPerPackage = null;
			totalDistinctLocationsPerPackage = null;
		}

		public IEnumerable<ICartonWithItems> Result
		{
			get
			{
				if (result == null)
				{
					result = new List<ICartonWithItems>(cartons.Count);
					foreach (var cartonWithItems in Cartons)
					{
						var groupedItems = cartonWithItems.ItemsInList
							.GroupBy(i => i.OriginalPK)
							.Select(o => new ContentResult(o.Key, o.Sum(i => i.QTY)));

						result.Add(new CartonWithItems { CartonPK = cartonWithItems.Carton.PK, Items = groupedItems });
					}
				}
				return result;
			}
		}
		List<ICartonWithItems> result;

		internal decimal TotalVolume
			=> totalVolume ?? (totalVolume = Cartons.Sum(l => l.Carton.Volume)).Value;

		decimal? totalVolume;

		internal decimal TotalCost
			=> totalCost ?? (totalCost = Cartons.Sum(l => l.Carton.Cost)).Value;

		decimal? totalCost;

		internal int TotalPackedItems
			=> totalPackedItems ?? (totalPackedItems = Cartons.Sum(l => l.ItemsInList.Count)).Value;

		int? totalPackedItems;

		internal int TotalDistinctProductsPerPackage
			=> totalDistinctProductsPerPackage ?? (totalDistinctProductsPerPackage = Cartons.Sum(l => l.ItemsInList.Select(i => i.ItemDefinition.PK).Distinct().Count())).Value;

		int? totalDistinctProductsPerPackage;

		internal int TotalDistinctLocationsPerPackage
			=> totalDistinctLocationsPerPackage ?? (totalDistinctLocationsPerPackage = Cartons.Sum(l => l.ItemsInList.Select(i => i.LocationPK).Distinct().Count())).Value;

		int? totalDistinctLocationsPerPackage;
	}
}
