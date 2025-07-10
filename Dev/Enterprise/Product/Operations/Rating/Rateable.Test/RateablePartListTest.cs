using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Rating.Rateable.Test
{
	public class RateablePartListTest : TestCase
	{
		public void TestGetDistinctCommodities()
		{
			var partList = new RateablePartList();
			partList.HasCommodity = true;
			partList.AddPart(new RateablePart() { CommodityCode = "AAA" });
			partList.AddPart(new RateablePart() { CommodityCode = "AAA" });
			partList.AddPart(new RateablePart() { CommodityCode = "BBB" });
			partList.AddPart(new RateablePart() { CommodityCode = "CCC" });
			partList.AddPart(new RateablePart() { CommodityCode = "CCC" });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasCommodity = true;

			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, partList.GetDistinctCommodities());
			AssertEquals(0, partListEmpty.GetDistinctCommodities().Count);
		}

		public void TestGetDistinctContainerTypePKs()
		{
			var partList = new RateablePartList();
			partList.HasContainerType = true;
			Guid? pk1 = Guid.NewGuid();
			Guid? pk2 = null;
			Guid? pk3 = Guid.NewGuid();
			partList.AddPart(new RateablePart() { ContainerTypePk = pk1 });
			partList.AddPart(new RateablePart() { ContainerTypePk = pk1 });
			partList.AddPart(new RateablePart() { ContainerTypePk = pk2 });
			partList.AddPart(new RateablePart() { ContainerTypePk = pk3 });
			partList.AddPart(new RateablePart() { ContainerTypePk = pk3 });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasContainerType = true;

			AssertContainsExactElementsInAnyOrder(new Guid?[] { null, pk1, pk3 }, partList.GetDistinctContainerTypePKs());
			AssertEquals(0, partListEmpty.GetDistinctContainerTypePKs().Count);
		}

		public void TestGetDistinctPackageTypes()
		{
			var partList = new RateablePartList();
			partList.HasPackageType = true;
			partList.AddPart(new RateablePart() { PackageType = "AAA" });
			partList.AddPart(new RateablePart() { PackageType = "AAA" });
			partList.AddPart(new RateablePart() { PackageType = "BBB" });
			partList.AddPart(new RateablePart() { PackageType = "CCC" });
			partList.AddPart(new RateablePart() { PackageType = "CCC" });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasPackageType = true;

			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, partList.GetDistinctPackageTypes());
			AssertEquals(0, partListEmpty.GetDistinctPackageTypes().Count);
		}

		public void TestGetDistinctContainerOwnerships()
		{
			var partList = new RateablePartList();
			partList.HasContainerOwnership = true;
			partList.AddPart(new RateablePart() { ContainerOwnership = "AAA" });
			partList.AddPart(new RateablePart() { ContainerOwnership = "AAA" });
			partList.AddPart(new RateablePart() { ContainerOwnership = "BBB" });
			partList.AddPart(new RateablePart() { ContainerOwnership = "CCC" });
			partList.AddPart(new RateablePart() { ContainerOwnership = "CCC" });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasContainerOwnership = true;

			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, partList.GetDistinctContainerOwnerships());
			AssertEquals(0, partListEmpty.GetDistinctContainerOwnerships().Count);
		}

		public void TestGetDistinctPalletized()
		{
			var partListTrueAndFalse = new RateablePartList();
			partListTrueAndFalse.HasPalletized = true;
			partListTrueAndFalse.AddPart(new RateablePart() { IsOnPallets = true });
			partListTrueAndFalse.AddPart(new RateablePart() { IsOnPallets = true });
			partListTrueAndFalse.AddPart(new RateablePart() { IsOnPallets = false });
			partListTrueAndFalse.AddPart(new RateablePart() { IsOnPallets = false });

			var partListAllTrue = new RateablePartList();
			partListAllTrue.HasPalletized = true;
			partListAllTrue.AddPart(new RateablePart() { IsOnPallets = true });

			var partListAllFalse = new RateablePartList();
			partListAllFalse.HasPalletized = true;
			partListAllFalse.AddPart(new RateablePart() { IsOnPallets = false });
			partListAllFalse.AddPart(new RateablePart() { IsOnPallets = false });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasPalletized = true;

			AssertContainsExactElementsInAnyOrder(new[] { true, false }, partListTrueAndFalse.GetDistinctPalletized());
			AssertContainsExactElementsInAnyOrder(new[] { true }, partListAllTrue.GetDistinctPalletized());
			AssertContainsExactElementsInAnyOrder(new[] { false }, partListAllFalse.GetDistinctPalletized());
			AssertEquals(0, partListEmpty.GetDistinctPalletized().Count);
		}

		public void TestGetDistinctProductPKs()
		{
			var partList = new RateablePartList();
			partList.HasProduct = true;
			Guid? pk1 = Guid.NewGuid();
			Guid? pk2 = null;
			Guid? pk3 = Guid.NewGuid();
			partList.AddPart(new RateablePart() { ProductPk = pk1 });
			partList.AddPart(new RateablePart() { ProductPk = pk1 });
			partList.AddPart(new RateablePart() { ProductPk = pk2 });
			partList.AddPart(new RateablePart() { ProductPk = pk3 });
			partList.AddPart(new RateablePart() { ProductPk = pk3 });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasProduct = true;

			AssertContainsExactElementsInAnyOrder(new Guid?[] { null, pk1, pk3 }, partList.GetDistinctProductPKs());
			AssertEquals(0, partListEmpty.GetDistinctProductPKs().Count);
		}

		public void TestGetDistinctWarehousePKs()
		{
			var partList = new RateablePartList();
			partList.HasWarehouse = true;
			Guid? pk1 = Guid.NewGuid();
			Guid? pk2 = null;
			Guid? pk3 = Guid.NewGuid();
			partList.AddPart(new RateablePart() { WarehousePk = pk1 });
			partList.AddPart(new RateablePart() { WarehousePk = pk1 });
			partList.AddPart(new RateablePart() { WarehousePk = pk2 });
			partList.AddPart(new RateablePart() { WarehousePk = pk3 });
			partList.AddPart(new RateablePart() { WarehousePk = pk3 });

			var partListEmpty = new RateablePartList();
			partListEmpty.HasProduct = true;

			AssertContainsExactElementsInAnyOrder(new Guid?[] { null, pk1, pk3 }, partList.GetDistinctWarehousePKs());
		}

		public void TestNewEmptyClone()
		{
			var partList = new RateablePartList();
			partList.WeightUnit = Core.Constants.Weight.Tonnes;
			partList.DeliveryDistanceUnit = Core.Constants.Length.Miles;
			partList.HasCommodity = true;
			var part = new RateablePart() { CommodityCode = "GEN" };
			partList.AddPart(part);
			AssertEquals("Precondition", "GEN", partList.GetDistinctCommodities().Single());

			var clone = partList.NewEmptyClone();
			AssertEquals("does not copy parts", 0, clone.Count);
			AssertEquals("does copy attributes", Core.Constants.Weight.Tonnes, clone.WeightUnit);
			AssertEquals("does copy attributes", Core.Constants.Length.Miles, clone.DeliveryDistanceUnit);
			AssertEquals("does copy HasX", true, clone.HasCommodity);
			AssertEquals("does not copy distinct collections", 0, clone.GetDistinctCommodities().Count);

			AssertEquals("does not alter original", 1, partList.Count);
			AssertEquals("does not alter original", "GEN", partList.GetDistinctCommodities().Single());
		}
	}
}
