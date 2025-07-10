using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;
using static Enterprise.MasterFiles.Business.MeasureInfo;

namespace Enterprise.Rating.Business.Test
{
	public class MeasureAnalyserTest : TestCaseWithFactory
	{
		public void TestGetCommodities()
		{
			var measures = new RateableMeasureSet();
			measures.AddWeightAndVolumeWithCommodityAndPackageType(1, null, "GEN", "");
			measures.AddWeightAndVolumeWithCommodityAndPackageType(2, null, "GEN", "");
			measures.AddWeightAndVolumeWithCommodityAndPackageType(3, null, "FAK", "");
			measures.AddWeightAndVolumeWithCommodityAndPackageType(4, null, "", "");
			var jobMeasures = new MeasureAnalyser(measures);
			var actual = jobMeasures.GetCommodities();
			var expected = new[] { string.Empty, "GEN", "FAK" };

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestGetContainerPKs()
		{
			var measures = new RateableMeasureSet();

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var pk3 = ZGuid.NewZGuid();

			measures.AddContainer(pk1);
			measures.AddContainer(pk2);
			measures.AddContainer(pk2);
			measures.AddContainer(pk3);

			var jobMeasures = new MeasureAnalyser(measures);
			var actual = jobMeasures.GetContainerTypePKs();
			var expected = new[] { pk1, pk2, pk3 };

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestSetTemporaryQuantity()
		{
			var measures = new RateableMeasureSet();
			measures.AddWeightAndVolumeWithCommodityAndPackageType(1, 2, new ZString("GEN"), ZString.Empty);
			measures.AddWeightAndVolumeWithCommodityAndPackageType(3, 5, new ZString("FAK"), ZString.Empty);
			var jobMeasures = new MeasureAnalyser(measures);

			AssertEquals(4m, jobMeasures.GetActual(MeasureType.Weight));
			AssertEquals(7m, jobMeasures.GetActual(MeasureType.Volume));

			var outerMeasureChanger = jobMeasures.BeginTemporaryChanges();

			outerMeasureChanger.SetTemporaryQuantity(MeasureType.Weight, new Quantity(99m, Core.Constants.Weight.Kilograms));
			outerMeasureChanger.SetTemporaryQuantity(MeasureType.Volume, new Quantity(10m, Core.Constants.Volume.CubicMetres));

			AssertEquals(99m, jobMeasures.GetActual(MeasureType.Weight));
			AssertEquals(10m, jobMeasures.GetActual(MeasureType.Volume));

			outerMeasureChanger.SetTemporaryQuantity(MeasureType.Weight, new Quantity(100m, Core.Constants.Weight.Kilograms));

			AssertEquals(100m, jobMeasures.GetActual(MeasureType.Weight));
			AssertEquals(10m, jobMeasures.GetActual(MeasureType.Volume));

			using (var innerChanger = jobMeasures.BeginTemporaryChanges())
			{
				innerChanger.SetTemporaryQuantity(MeasureType.Weight, new Quantity(200m, Core.Constants.Weight.Kilograms));
				AssertEquals(200m, jobMeasures.GetActual(MeasureType.Weight));

				AssertExceptionThrown<InvalidOperationException>(

					"Can't use outer while inner is active",
					() => outerMeasureChanger.SetTemporaryQuantity(MeasureType.Chargeable, new Quantity(100m, Core.Constants.Weight.Kilograms)));
			}

			AssertEquals("disposing inner restores the outer temporary value", 100m, jobMeasures.GetActual(MeasureType.Weight));

			outerMeasureChanger.Dispose();

			AssertEquals(4m, jobMeasures.GetActual(MeasureType.Weight));
			AssertEquals(7m, jobMeasures.GetActual(MeasureType.Volume));

			AssertExceptionThrown<InvalidOperationException>(
				"Can't use after disposing",
				() => outerMeasureChanger.SetTemporaryQuantity(MeasureType.Chargeable, new Quantity(100m, Core.Constants.Weight.Kilograms)));
		}

		public void TestSetTemporaryContainerTypeAndCommodityFilter()
		{
			var measures = new RateableMeasureSet();

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var pk3 = ZGuid.NewZGuid();

			measures.AddContainerGroup(pk1, new ZString("GEN"), new[] { new ContainerInfo(), new ContainerInfo() });
			measures.AddContainerGroup(pk2, new ZString("GEN"), new[] { new ContainerInfo() });
			measures.AddContainerGroup(pk2, new ZString("FAK"), new[] { new ContainerInfo() });
			measures.AddContainerGroup(pk3, new ZString("HAZ"), new[] { new ContainerInfo() });

			var jobMeasures = new MeasureAnalyser(measures);

			var expected1 = (ContainerTypePk: pk1, ContainerQuality: string.Empty, CommodityCode: new ZString("GEN"), ContainerCount: 2);
			var expected2 = (ContainerTypePk: pk2, ContainerQuality: string.Empty, CommodityCode: new ZString("GEN"), ContainerCount: 1);
			var expected3 = (ContainerTypePk: pk2, ContainerQuality: string.Empty, CommodityCode: new ZString("FAK"), ContainerCount: 1);
			var expected4 = (ContainerTypePk: pk3, ContainerQuality: string.Empty, CommodityCode: new ZString("HAZ"), ContainerCount: 1);

			using (var changer = jobMeasures.BeginTemporaryChanges())
			{
				changer.SetTemporaryContainerTypeAndCommodityFilter(pk2, null, "FAK");
				var tempActual = measures.GetContainerTypeAndCommodityList();
				var tempExpected = new[] { expected3 };
				AssertContainsExactElementsInAnyOrder(tempExpected, tempActual);
			}

			var actual = measures.GetContainerTypeAndCommodityList();
			var expected = new[] { expected1, expected2, expected3, expected4 };

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestSetTemporaryContainerTypeAndCommodityFilter_Weight()
			=> AssertPartsFiltered(MeasureType.Weight);

		public void TestSetTemporaryContainerTypeAndCommodityFilter_Volume()
			=> AssertPartsFiltered(MeasureType.Volume);

		static void AssertPartsFiltered(MeasureType measureType)
		{
			var measures = new RateableMeasureSet();

			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();

			var parts = new RateablePartList { HasCommodity = true, HasContainerType = true, WeightUnit = "KG", VolumeUnit = "M3" };
			parts.AddPart(new RateablePart { ContainerTypePk = pk1, CommodityCode = "GEN", Weight = 250m, Volume = 5m });
			parts.AddPart(new RateablePart { ContainerTypePk = pk2, CommodityCode = "GEN", Weight = 250m, Volume = 5m });
			parts.AddPart(new RateablePart { ContainerTypePk = pk2, CommodityCode = "FAK", Weight = 500m, Volume = 5m });
			parts.AddPart(new RateablePart { ContainerTypePk = pk3, CommodityCode = "HAZ", Weight = 500m, Volume = 5m });
			measures.AddPartList(measureType, parts);

			var jobMeasures = new MeasureAnalyser(measures);

			var expected1 = new { ContainerTypePk = pk1, CommodityCode = "GEN" };
			var expected2 = new { ContainerTypePk = pk2, CommodityCode = "GEN" };
			var expected3 = new { ContainerTypePk = pk2, CommodityCode = "FAK" };
			var expected4 = new { ContainerTypePk = pk3, CommodityCode = "HAZ" };

			using (var changer = jobMeasures.BeginTemporaryChanges())
			{
				changer.SetTemporaryContainerTypeAndCommodityFilter(pk2, null, "FAK");
				var tempActual = measures.GetPartList(measureType).Select(p => new { ContainerTypePk = (Guid)p.ContainerTypePk, p.CommodityCode });
				var tempExpected = new[] { expected3 };
				AssertContainsExactElementsInAnyOrder("Parts should be filtered.", tempExpected, tempActual);
			}

			var actual = measures.GetPartList(measureType).Select(p => new { ContainerTypePk = (Guid)p.ContainerTypePk, p.CommodityCode });
			var expected = new[] { expected1, expected2, expected3, expected4 };

			AssertContainsExactElementsInAnyOrder("Parts should no longer be filtered.", expected, actual);
		}

		public void TestGetQuantity()
		{
			var measures = new RateableMeasureSet();
			measures.SetQuantity(MeasureType.Weight, 10m, Core.Constants.Weight.Kilograms);
			measures.SetQuantity(MeasureType.Volume, 7m, "");
			var jobMeasures = new MeasureAnalyser(measures);
			var weightQuantity = jobMeasures.GetQuantity(MeasureType.Weight);
			var volumeQuantity = jobMeasures.GetQuantity(MeasureType.Volume);
			var unknownQuantity = jobMeasures.GetQuantity(MeasureType.Chargeable);

			AssertEquals("amount", 10m, weightQuantity.Amount);
			AssertEquals("unit", Core.Constants.Weight.Kilograms, weightQuantity.Unit);

			AssertEquals("amount when no units", 0m, volumeQuantity.Amount);
			AssertEquals("", volumeQuantity.Unit);

			AssertEquals("amount when no MeasureType", 0m, unknownQuantity.Amount);
			AssertEquals("unit when no MeasureType", "", unknownQuantity.Unit);
		}

		public void TestGetActualLength()
		{
			var measures = new RateableMeasureSet();
			measures.SetDeliveryDistance(2m, Core.Constants.Length.Kilometres);
			var jobMeasures = new MeasureAnalyser(measures);

			AssertEquals(2000m, jobMeasures.GetActualLength(MeasureType.DeliveryDistance, Core.Constants.Length.Metres));
			AssertEquals(0m, jobMeasures.GetActualLength(MeasureType.PickupDistance, Core.Constants.Length.Metres));
			AssertExceptionThrown<ArgumentException>(() => jobMeasures.GetActualLength(MeasureType.DeliveryDistance, Core.Constants.Weight.Kilograms));
		}

		public void TestEqualMeasures()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			var entry2 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			var entry3 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "");
			entry3.TI_RH_NKCommodityCode = "HAZ";
			var entry4 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "40GP");
			entry4.TI_RH_NKCommodityCode = "HAZ";
			var entry5 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");

			var criteria = new TestRatingCriteria();
			var lineProvider = new FastLineProvider(criteria);
			var fastLine1 = lineProvider.GetOrCreate(entry1.RateLines[0]);
			var fastLine2 = lineProvider.GetOrCreate(entry2.RateLines[0]);
			var fastLine3 = lineProvider.GetOrCreate(entry3.RateLines[0]);
			var fastLine4 = lineProvider.GetOrCreate(entry4.RateLines[0]);
			var fastLine5 = lineProvider.GetOrCreate(entry5.RateLines[0]);

			var comparer = new MeasureAnalyser(new RateableMeasureSet());
			Assert(!comparer.EqualMeasures(fastLine1, fastLine4));
			Assert(!comparer.EqualMeasures(fastLine2, fastLine4));
			Assert(!comparer.EqualMeasures(fastLine3, fastLine4));
			Assert(comparer.EqualMeasures(fastLine1, fastLine5));

			var productPK = Helper.NewOrgSupplierPart(rate.Header).PK;
			entry1.RateLines[0].TL_OP_ProductNumber = productPK;
			Assert(!comparer.EqualMeasures(fastLine1, fastLine5));
			entry5.RateLines[0].TL_OP_ProductNumber = productPK;
			Assert(comparer.EqualMeasures(fastLine1, fastLine5));
		}

		public void TestEqualMeasures_CommodityContainer()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			entry1.TI_RH_NKCommodityCode = ZString.Empty;
			var line1 = entry1.AddRateLine("ODOC");
			var entry2 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			entry2.TI_RH_NKCommodityCode = "GEN";
			var line2 = entry2.AddRateLine("ODOC");
			var entry3 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "");
			entry3.TI_RH_NKCommodityCode = ZString.Empty;
			var line3 = entry3.AddRateLine("ODOC");
			var entry4 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "40GP");
			entry4.TI_RH_NKCommodityCode = ZString.Empty;
			entry4.TI_MatchContainerRateClass = true;
			var line4 = entry4.AddRateLine("ODOC");
			var entry5 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "40HC");
			entry5.TI_RH_NKCommodityCode = ZString.Empty;
			var line5 = entry5.AddRateLine("ODOC");
			var entry6 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX", "", "20GP");
			entry6.TI_RH_NKCommodityCode = "HAZ";
			var line6 = entry6.AddRateLine("ODOC");

			var criteria = new TestRatingCriteria();
			var lineProvider = new FastLineProvider(criteria);
			var fastLine1 = lineProvider.GetOrCreate(line1);
			var fastLine2 = lineProvider.GetOrCreate(line2);
			var fastLine3 = lineProvider.GetOrCreate(line3);
			var fastLine4 = lineProvider.GetOrCreate(line4);
			var fastLine5 = lineProvider.GetOrCreate(line5);
			var fastLine6 = lineProvider.GetOrCreate(line6);

			var comparer = new MeasureAnalyser(new RateableMeasureSet());
			AssertEquals(false, comparer.EqualMeasures(fastLine1, fastLine2));
			AssertEquals(false, comparer.EqualMeasures(fastLine2, fastLine1));
			AssertEquals(false, comparer.EqualMeasures(fastLine1, fastLine6));
			AssertEquals(false, comparer.EqualMeasures(fastLine6, fastLine1));
			AssertEquals(false, comparer.EqualMeasures(fastLine2, fastLine6));
			AssertEquals(false, comparer.EqualMeasures(fastLine6, fastLine2));

			AssertEquals(false, comparer.EqualMeasures(fastLine3, fastLine4));
			AssertEquals(false, comparer.EqualMeasures(fastLine3, fastLine5));
			AssertEquals(false, comparer.EqualMeasures(fastLine4, fastLine5));
			AssertEquals(false, comparer.EqualMeasures(fastLine4, fastLine3));
			AssertEquals(false, comparer.EqualMeasures(fastLine5, fastLine3));
			AssertEquals(false, comparer.EqualMeasures(fastLine5, fastLine4));

			AssertEquals(true, comparer.EqualMeasures(fastLine5, fastLine5));
		}

		#region Implementation

		protected TestHelper Helper
			=> testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
