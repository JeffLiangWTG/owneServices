using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Cartonisation.Diagnostic;
using Enterprise.Warehouse.Cartonisation.Integration;

namespace Enterprise.Warehouse.Cartonisation.Business.Testing
{
	static class CartonisationTestHelper
	{
		public const decimal DefaultMax = 999999m;
		public static ICartonisableItem CreateOrderLine(Guid orderLinePK, decimal qty, ICartonisableItemDefinition product)
		{
			var order = new DummyCartonisableItem(orderLinePK, qty, product);
			return order;
		}

		public static ICartonisableItem CreateOrderLine(decimal qty, ICartonisableItemDefinition product, string location = null)
		{
			var orderLinePK = Guid.NewGuid();
			var order = new DummyCartonisableItem(orderLinePK, qty, product);
			order.Location = location ?? string.Empty;
			return order;
		}

		public static ICartonisableItemDefinition CreateProduct(decimal height, decimal length, decimal width, decimal volume, decimal weight, string dimensionUQ, string volumeUQ, string weightUQ, bool keepUpright = false, string name = "product")
		{
			var product = new DummyCartonisableItemDefinition(height, length, width, volume, weight, dimensionUQ, volumeUQ, weightUQ, keepUpright);
			product.ProductName = name;
			return product;
		}

		public static ICartonisableItemDefinition CreateProduct(decimal height = 1, decimal length = 1, decimal width = 1, decimal volume = 0, decimal weight = 1, bool keepUpright = false, string name = "product")
		{
			if (volume == 0)
			{
				volume = length * height * width;
			}
			var product = new DummyCartonisableItemDefinition(height, length, width, volume, weight, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode, keepUpright);
			product.ProductName = name;
			return product;
		}

		public static ICartonDefinition CreateCarton(decimal length, decimal width, decimal height, decimal maxWeight, decimal maxVolume, decimal maxUnits, decimal maxFillPercent, string dimensionUQ, string volumeUQ, string weightUQ, int cost)
		{
			return new DummyCartonDefinition(length, width, height, maxWeight, 0m, maxVolume, maxUnits, maxFillPercent, dimensionUQ, volumeUQ, weightUQ, cost);
		}

		public static ICartonDefinition CreateCarton(decimal length, decimal width, decimal height, decimal maxWeight, decimal emptyWeight, decimal maxVolume, decimal maxUnits, decimal maxFillPercent)
		{
			return new DummyCartonDefinition(length, width, height, maxWeight, emptyWeight, maxVolume, maxUnits, maxFillPercent, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode, 1);
		}

		public static ICartonDefinition CreateCarton(decimal length, decimal width, decimal height, decimal maxWeight, decimal maxVolume, decimal maxUnits, decimal maxFillPercent, string dimensionUQ, string volumeUQ, string weightUQ)
		{
			return CreateCarton(length, width, height, maxWeight, maxVolume, maxUnits, maxFillPercent, dimensionUQ, volumeUQ, weightUQ, 1);
		}

		public static ICartonDefinition CreateCarton(decimal length, decimal width, decimal height, decimal maxWeight, decimal maxVolume, decimal maxUnits, decimal maxFillPercent)
		{
			return CreateCarton(length, width, height, maxWeight, maxVolume, maxUnits, maxFillPercent, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode);
		}

		public static ICartonDefinition AddNew(this List<ICartonDefinition> boxes, decimal length = 1, decimal width = 1, decimal height = 1, decimal maxWeight = DefaultMax, decimal maxVolume = 0, decimal maxUnits = DefaultMax, decimal maxFillPercent = 1)
		{
			if (maxVolume == 0)
			{
				maxVolume = height * length * width;
			}
			var box = CreateCarton(length, width, height, maxWeight, maxVolume, maxUnits, maxFillPercent);
			boxes.Add(box);
			return box;
		}

		public static ICartonisableItem AddNew(this List<ICartonisableItem> orders, decimal qty = 1, ICartonisableItemDefinition product = null, string location = null)
		{
			if (product == null)
			{
				product = CreateProduct();
			}
			var order = CreateOrderLine(qty, product, location);
			orders.Add(order);
			return order;
		}
	}
}
