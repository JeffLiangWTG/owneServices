using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	[TestedType(typeof(CartonisationDiagnostics))]
	public class CartonisationDiagnosticsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSerialisation()
		{
			var diag = new CartonisationDiagnostics();
			var carton = new DummyCartonDefinition();
			carton.CartonName = "ZZZ";
			carton.DimensionUQ = "M";
			carton.EmptyWeight = 1;
			carton.Height = 2;
			carton.Length = 3;
			carton.MaxFillPercent = 4;
			carton.MaxNumberOfUnits = 5;
			carton.MaxWeight = 6;
			carton.Volume = 7;
			carton.VolumeUQ = "M3";
			carton.Width = 8;
			carton.WeightUQ = "KG";

			diag.Cartons.Add(carton);

			var pack = new DummyCartonisableItemDefinition();
			pack.ProductName = "YYY";
			pack.DimensionUQ = "CM";
			pack.Height = 10;
			pack.Length = 11;
			pack.Volume = 12;
			pack.VolumeUQ = "D3";
			pack.Weight = 13;
			pack.WeightUQ = "G";
			pack.Width = 14;

			var itemToPack = new DummyCartonisableItem(Guid.NewGuid(), 20, pack);
			itemToPack.Location = "LOCATION";

			diag.Cartons.Add(carton);
			diag.ItemsToPack.Add(itemToPack);

			using (var directory = new TempDirectory())
			{
				diag.SaveCartonsToXml($"{directory.DirectoryName}\\cartons.xml");
				diag.LoadCartonsFromXml($"{directory.DirectoryName}\\cartons.xml");
			}

			AssertEquals(1, diag.Cartons.Count);
			var loadedCarton = diag.Cartons[0];
			AssertNotEquals("Loaded carton should be a different object", carton, loadedCarton);
			AssertEquals(loadedCarton.CartonName, carton.CartonName);
			AssertEquals(loadedCarton.DimensionUQ, carton.DimensionUQ);
			AssertEquals(loadedCarton.EmptyWeight, carton.EmptyWeight);
			AssertEquals(loadedCarton.Height, carton.Height);
			AssertEquals(loadedCarton.Length, carton.Length);
			AssertEquals(loadedCarton.MaxFillPercent, carton.MaxFillPercent);
			AssertEquals(loadedCarton.MaxNumberOfUnits, carton.MaxNumberOfUnits);
			AssertEquals(loadedCarton.MaxWeight, carton.MaxWeight);
			AssertEquals(loadedCarton.Volume, carton.Volume);
			AssertEquals(loadedCarton.VolumeUQ, carton.VolumeUQ);
			AssertEquals(loadedCarton.Width, carton.Width);
			AssertEquals(loadedCarton.WeightUQ, carton.WeightUQ);

			using (var directory = new TempDirectory())
			{
				diag.SaveItemsToPackToXml($"{directory.DirectoryName}\\items.xml");
				diag.LoadItemsToPackFromXml($"{directory.DirectoryName}\\items.xml");
			}

			AssertEquals(1, diag.ItemsToPack.Count);
			var loadedItemToPack = diag.ItemsToPack[0];
			AssertNotEquals("Loaded item to pack should be a different object", itemToPack, loadedItemToPack);
			AssertEquals(loadedItemToPack.Quantity, itemToPack.Quantity);
			AssertEquals(loadedItemToPack.Location, itemToPack.Location);
			AssertEquals(loadedItemToPack.ItemDefinition.ProductName, itemToPack.ItemDefinition.ProductName);
			AssertEquals(loadedItemToPack.ItemDefinition.DimensionUQ, itemToPack.ItemDefinition.DimensionUQ);
			AssertEquals(loadedItemToPack.ItemDefinition.Height, itemToPack.ItemDefinition.Height);
			AssertEquals(loadedItemToPack.ItemDefinition.Length, itemToPack.ItemDefinition.Length);
			AssertEquals(loadedItemToPack.ItemDefinition.Volume, itemToPack.ItemDefinition.Volume);
			AssertEquals(loadedItemToPack.ItemDefinition.VolumeUQ, itemToPack.ItemDefinition.VolumeUQ);
			AssertEquals(loadedItemToPack.ItemDefinition.Width, itemToPack.ItemDefinition.Width);
			AssertEquals(loadedItemToPack.ItemDefinition.Weight, itemToPack.ItemDefinition.Weight);
			AssertEquals(loadedItemToPack.ItemDefinition.WeightUQ, itemToPack.ItemDefinition.WeightUQ);
		}
	}
}
