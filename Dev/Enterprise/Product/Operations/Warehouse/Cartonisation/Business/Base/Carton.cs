using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	public class Carton
	{
		public Carton(Guid pk, decimal height, decimal length, decimal width, decimal volume, decimal maxFillPercent, decimal maxNumberOfUnits, decimal maxWeight, decimal cost)
		{
			PK = pk;
			Height = height;
			Length = length;
			Width = width;
			Volume = volume;
			MaxFillPercent = maxFillPercent;
			VolumeToFill = Volume * maxFillPercent;
			MaxNumberOfUnits = maxNumberOfUnits;
			MaxWeight = maxWeight;
			SortedDims = new[] { height, length, width }.OrderBy(d => d).ToArray();
			Cost = cost;
		}

		public readonly Guid PK;
		public readonly decimal Height;
		public readonly decimal Length;
		public readonly decimal Width;
		public readonly decimal Volume;
		public readonly decimal MaxFillPercent;
		public readonly decimal MaxNumberOfUnits;
		public readonly decimal MaxWeight;
		public readonly IReadOnlyList<decimal> SortedDims;
		public readonly decimal VolumeToFill;
		public readonly decimal Cost;

		public bool CanFitItems(CartonisableItemDefinition itemDefinition, decimal qty)
		{
			return MaxQuantityCache[itemDefinition] >= qty;
		}

		public decimal GetMaxUnitsForItem(CartonisableItemDefinition itemDefinition)
		{
			if (!MaxQuantityCache.TryGetValue(itemDefinition, out var result))
			{
				var qtyByVolume = (itemDefinition.Volume != 0) ? VolumeToFill / itemDefinition.Volume : MaxNumberOfUnits;
				var qtyByWeight = (itemDefinition.Weight != 0) ? MaxWeight / itemDefinition.Weight : MaxNumberOfUnits;
				var maxQtyForTheBox = (qtyByVolume > qtyByWeight)
					? Math.Min(qtyByWeight, MaxNumberOfUnits)
					: Math.Min(qtyByVolume, MaxNumberOfUnits);
				MaxQuantityCache[itemDefinition] = result = maxQtyForTheBox;
			}

			return result;
		}

		public void SetZeroMaxUnitsForItem(CartonisableItemDefinition itemDefinition) => MaxQuantityCache[itemDefinition] = 0m;

		public bool CanFitOneSingleItem(CartonisableItemDefinition itemDefinition)
		{
			if (!CanFitSingleItemCache.TryGetValue(itemDefinition, out var result))
			{
				CanFitSingleItemCache[itemDefinition] = result = CanFitOneItemsDimensions(itemDefinition);
			}

			return result;

			bool CanFitOneItemsDimensions(CartonisableItemDefinition product)
			{
				var productFitsInCarton =
					product.SortedDims[0] <= SortedDims[0]
					&& product.SortedDims[1] <= SortedDims[1]
					&& product.SortedDims[2] <= SortedDims[2];

				if (productFitsInCarton && product.KeepUpright)
				{
					productFitsInCarton = product.Height <= Height &&
						((product.Width <= Width && product.Length <= Length) ||
						(product.Width <= Length && product.Length <= Width));
				}

				return productFitsInCarton;
			}
		}

		Dictionary<CartonisableItemDefinition, decimal> MaxQuantityCache { get; } = new Dictionary<CartonisableItemDefinition, decimal>();
		Dictionary<CartonisableItemDefinition, bool> CanFitSingleItemCache { get; } = new Dictionary<CartonisableItemDefinition, bool>();
	}
}
