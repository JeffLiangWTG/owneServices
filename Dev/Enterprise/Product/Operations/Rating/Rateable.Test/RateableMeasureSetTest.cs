using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using static Enterprise.MasterFiles.Business.MeasureInfo;

namespace Enterprise.Rating.Rateable.Test
{
	public class RateableMeasureSetTest : TestCaseWithFactory
	{
		public void TestSetPackageCountWithCommodity()
		{
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetPackageCountWithCommodity(5m, "HAZ");
			AssertEquals(5m, measures.GetActual(MeasureType.Package));
		}

		public void TestCreateContainerList()
		{
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			AssertEquals(false, measures.HasMeasureType(MeasureType.ContainerCount));

			measures.CreateContainerList();
			AssertEquals(true, measures.HasMeasureType(MeasureType.ContainerCount));
			AssertEquals(0m, (decimal)measures.GetActual(MeasureType.ContainerCount));
		}

		public void TestAddLCL()
		{
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.AddLCL("GEN", 5, "KG", 9, "M3");
			AssertEquals(true, measures.HasMeasureType(MeasureType.ContainerCount));
			AssertEquals(0m, (decimal)measures.GetActual(MeasureType.ContainerCount));
			var container = measures.GetAllContainers().Single();
			AssertEquals(5m, container.ContainerWeightInKG);
			AssertEquals(9m, container.ContainerVolumeInM3);
		}

		public void TestUpdateCommodities()
		{
			var container1 = ZGuid.NewZGuid();
			var container2 = ZGuid.NewZGuid();

			var package1 = new ContainerInfo(20, "KG", 2, "M3", 1, 0, "1234");
			var package2 = new ContainerInfo(10, "KG", 1, "M3", 1, 0, "1235");

			var measures = new RateableMeasureSet();
			measures.AddContainerWithCommodityAndNumber(container1, "AAA", "1234", new ContainerInfo());
			measures.AddContainerWithCommodityAndNumber(container2, "BBB", "1235", new ContainerInfo());
			measures.AddPackageUnitWithCommodity(package1, "AA", "CCC");
			measures.AddPackageUnitWithCommodity(package2, "BB", "DDD");
			measures.AddWeightAndVolumeWithCommodityAndPackageType(100, 1, "EEE", "CC");
			measures.AddWeightAndVolumeWithCommodityAndPackageType(200, 3, "FFF", "CC");

			measures.AddPartList(MeasureType.Shipment,
				new JobLevelPart { CommodityCode = null, ShipmentCount = 6 });

			measures.AddPartList(MeasureType.Chargeable,
				new JobLevelPart
				{
					CommodityCode = "GGG",
					ChargeableUnit = "KG",
					ChargeableMeasure =
					new ClientProviderValues(1, 2, 3)
				});

			CombineAssertions("Expect distinct commodities", () =>
			{
				AssertContainsExactElementsInAnyOrder("Precondition", new[] {
					"AAA",
					"BBB",
					"CCC",
					"DDD",
					"EEE",
					"FFF",
					"GGG"
				}, measures.GetDistinctCommodities());

				var partList = measures.GetPartList(MeasureType.Unit).Cast<RateableContainer>().Select(x => new { x.ContainerWeightInKG, x.ContainerVolumeInM3, x.Reference }).ToArray();
				var expectedPartList = new[]
				{
					new
					{
						ContainerWeightInKG = (decimal)20,
						ContainerVolumeInM3 = (decimal)2,
						Reference = "1234",
					},
					new
					{
						ContainerWeightInKG = (decimal)10,
						ContainerVolumeInM3 = (decimal)1,
						Reference = "1235",
					}
				};
				AssertContainsExactElementsInAnyOrder(expectedPartList, partList);

				AssertEquals(300, measures.GetPartList(MeasureType.Weight).Sum(i => (int)i.WeightMeasure.Actual));
				AssertEquals(4, measures.GetPartList(MeasureType.Volume).Sum(i => (int)i.VolumeMeasure.Actual));
				AssertEquals(2, measures.GetContainerTypeAndCommodityList().Count());
			});

			measures.UpdateCommodities("ZZZ");

			CombineAssertions("Expect one commodity but all the other values unchanged", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "ZZZ" }, measures.GetDistinctCommodities());

				var partList = measures.GetPartList(MeasureType.Unit).Cast<RateableContainer>().Select(x => new { x.ContainerWeightInKG, x.ContainerVolumeInM3, x.Reference }).ToArray();
				var expectedPartList = new[]
				{
					new
					{
						ContainerWeightInKG = (decimal)20,
						ContainerVolumeInM3 = (decimal)2,
						Reference = "1234",
					},
					new
					{
						ContainerWeightInKG = (decimal)10,
						ContainerVolumeInM3 = (decimal)1,
						Reference = "1235",
					}
				};
				AssertContainsExactElementsInAnyOrder(expectedPartList, partList);

				AssertEquals(300, measures.GetPartList(MeasureType.Weight).Sum(i => (int)i.WeightMeasure.Actual));
				AssertEquals(4, measures.GetPartList(MeasureType.Volume).Sum(i => (int)i.VolumeMeasure.Actual));
				AssertEquals(2, measures.GetContainerTypeAndCommodityList().Count());
			});
		}

		public void TestShipments()
		{
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			AssertEquals(false, measures.Shipments.HasValue);

			measures.Shipments = 5;
			AssertEquals(5m, measures.Shipments.Value);
		}

		public void TestConvertUnitsDifferentCasing()
		{
			AssertNoExceptionThrown(() =>
			{
				RateableMeasureSet.Convert(10, "kg", "KG");
			});
		}

		public void TestAddContainerGroup()
		{
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var container1 = new ContainerInfo(containerNumber: "CONT0001");
			var container2 = new ContainerInfo(containerNumber: "CONT0002");
			var container3 = new ContainerInfo(containerNumber: "CONT0003");
			measures.AddContainerGroup(pk1, "GEN", new[] { container1, container2 });
			measures.AddContainerGroup(pk2, "HAZ", new[] { container3 });

			var containers = measures.GetContainerGroups().ToList();
			AssertEquals("group count", 2, containers.Count);
			AssertEquals(pk1, containers[0].ContainerTypePK);
			AssertEquals(pk2, containers[1].ContainerTypePK);
			AssertEquals("GEN", containers[0].CommodityCode);
			AssertEquals("HAZ", containers[1].CommodityCode);
			var containerInfos = measures.GetAllContainers().Select(c => c.ContainerNumber);
			AssertContainsExactElementsInAnyOrder(new[] { "CONT0001", "CONT0002", "CONT0003" }, containerInfos);
		}

		public void TestGetContainerTypeAndCommodityList()
		{
			var measures = new RateableMeasureSet();

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var pk3 = ZGuid.NewZGuid();

			measures.AddContainerGroup(pk1, new ZString("GEN"), new[] { new ContainerInfo(), new ContainerInfo() });
			measures.AddContainerGroup(pk2, new ZString("GEN"), new[] { new ContainerInfo() });
			measures.AddContainerGroup(pk2, new ZString("FAK"), new[] { new ContainerInfo() });
			measures.AddContainerGroup(pk3, new ZString("HAZ"), new[] { new ContainerInfo() });

			var actual = measures.GetContainerTypeAndCommodityList();
			var expected1 = (ContainerTypePk: pk1, ContainerQuality: string.Empty, CommodityCode: new ZString("GEN"), ContainerCount: 2);
			var expected2 = (ContainerTypePk: pk2, ContainerQuality: string.Empty, CommodityCode: new ZString("GEN"), ContainerCount: 1);
			var expected3 = (ContainerTypePk: pk2, ContainerQuality: string.Empty, CommodityCode: new ZString("FAK"), ContainerCount: 1);
			var expected4 = (ContainerTypePk: pk3, ContainerQuality: string.Empty, CommodityCode: new ZString("HAZ"), ContainerCount: 1);
			var expected = new[] { expected1, expected2, expected3, expected4 };

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestGetContainerCountForContainerType()
		{
			var measures = new RateableMeasureSet();

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var pk3 = ZGuid.NewZGuid();

			measures.AddContainerGroup(pk1, new ZString("GEN"), new[] { new ContainerInfo(), new ContainerInfo() });
			measures.AddContainerGroup(pk2, new ZString("GEN"), new[] { new ContainerInfo(), new ContainerInfo(), new ContainerInfo() });
			measures.AddContainerGroup(pk2, new ZString("FAK"), new[] { new ContainerInfo() });
			measures.AddContainerGroup(pk3, new ZString("HAZ"), new[] { new ContainerInfo() });
			measures.AddContainerGroup(ZGuid.Empty, new ZString("FOO"), new[] { new ContainerInfo(), new ContainerInfo() });

			AssertEquals("result for a single measure point", 2m, measures.GetContainerCountForContainerType(pk1.ToGuid()));
			AssertEquals("result for multiple measure points is the total", 4m, measures.GetContainerCountForContainerType(pk2.ToGuid()));
			AssertEquals("result is zero when PK not found", 0m, measures.GetContainerCountForContainerType(Guid.NewGuid()));
			AssertEquals("result for empty container type", 2m, measures.GetContainerCountForContainerType(null));
		}

		public void TestGetTotalTEUForContainerType()
		{
			var measures = new RateableMeasureSet();

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var pk3 = ZGuid.NewZGuid();

			measures.AddContainerGroup(pk1, new ZString("GEN"), new[] { new ContainerInfo(teu: 2), new ContainerInfo(teu: 2) });
			measures.AddContainerGroup(pk2, new ZString("GEN"), new[] { new ContainerInfo(teu: 1), new ContainerInfo(teu: 1), new ContainerInfo(teu: 1) });
			measures.AddContainerGroup(pk2, new ZString("FAK"), new[] { new ContainerInfo(teu: 1) });
			measures.AddContainerGroup(pk3, new ZString("HAZ"), new[] { new ContainerInfo(teu: 2) });
			measures.AddContainerGroup(ZGuid.Empty, new ZString("FOO"), new[] { new ContainerInfo(), new ContainerInfo() });

			AssertEquals("result for a single measure point", 4m, measures.GetTotalTEUForContainerType(NullableHelper.ToNullable(pk1)));
			AssertEquals("result for multiple measure points uses all points", 4m, measures.GetTotalTEUForContainerType(NullableHelper.ToNullable(pk2)));
			AssertEquals("result is zero when PK not found", 0m, measures.GetTotalTEUForContainerType(Guid.NewGuid()));
		}

		public void TestGetAllContainerInfos()
		{
			var measures = new RateableMeasureSet();

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			var container1 = new ContainerInfo(teu: 1, containerNumber: "1");
			var container2 = new ContainerInfo(teu: 1, containerNumber: "2");
			var container3 = new ContainerInfo(teu: 1, containerNumber: "3");
			var container4 = new ContainerInfo(teu: 1, containerNumber: "4");
			var container5 = new ContainerInfo(teu: 1, containerNumber: "5");

			measures.AddContainerGroup(pk1, new ZString("GEN"), new[] { container1, container2 });
			measures.AddContainerGroup(pk2, new ZString("GEN"), new[] { container3 });
			measures.AddContainerGroup(pk2, new ZString("HAZ"), new[] { container4, container5 });

			var actual = measures.GetAllContainers().Select(c => new
			{
				c.ContainerNumber,
				c.TEU
			});
			var expected = new[]
			{
				new { ContainerNumber = "1", TEU = 1m },
				new { ContainerNumber = "2", TEU = 1m },
				new { ContainerNumber = "3", TEU = 1m },
				new { ContainerNumber = "4", TEU = 1m },
				new { ContainerNumber = "5", TEU = 1m }
			};
			AssertContainsExactElementsInAnyOrder(expected, actual);

			var actualWhenEmpty = new RateableMeasureSet().GetAllContainers();
			AssertEquals("expect empty list when measure is missing", 0, actualWhenEmpty.Count());
			Assert(true);
		}

		public void TestBuildPackageTypeAndCountMap()
		{
			var measures = new RateableMeasureSet();

			var whsPK = ZGuid.NewZGuid();

			measures.CreateWarehouseDocketPackageCountList(null);
			measures.AddWarehouseDocketPackageCount(1m, whsPK, "O1", "PLT", false);
			measures.AddWarehouseDocketPackageCount(1m, whsPK, "O2", "PLT", true);
			measures.AddWarehouseDocketPackageCount(1m, whsPK, "O3", "PKG", false);
			measures.AddWarehouseDocketPackageCount(1m, whsPK, "O4", "PKG", true);
			measures.AddWarehouseDocketPackageCount(1m, whsPK, "O5", "UNT", false);

			var allPackagesMap = measures.BuildPackageTypeAndCountMap(false);

			allPackagesMap.TryGetValue("PLT", out var pltCount);
			allPackagesMap.TryGetValue("PKG", out var pkgCount);
			allPackagesMap.TryGetValue("UNT", out var untCount);
			AssertEquals(3, allPackagesMap.Count);
			AssertEquals(2m, pltCount);
			AssertEquals(2m, pkgCount);
			AssertEquals(1m, untCount);

			var loadedPackagesMap = measures.BuildPackageTypeAndCountMap(true);

			loadedPackagesMap.TryGetValue("PLT", out pltCount);
			loadedPackagesMap.TryGetValue("PKG", out pkgCount);
			loadedPackagesMap.TryGetValue("UNT", out untCount);
			AssertEquals(3, loadedPackagesMap.Count);
			AssertEquals(1m, pltCount);
			AssertEquals(1m, pkgCount);
			AssertEquals(0m, untCount);

			var emptyMeasureMap = new RateableMeasureSet().BuildPackageTypeAndCountMap(false);
			AssertEquals("expect empty map when measure is missing", false, emptyMeasureMap.Any());
		}

		public void TestFilterContainers_WhenContainerCountNotExist()
		{
			var measures = new RateableMeasureSet();

			Assert("Prerequisite", !measures.HasMeasureType(MeasureType.ContainerCount));

			AssertNoExceptionThrown(() => measures.FilterContainers(null, null, null));
		}

		public void TestSetCombinedMeasureFrom()
		{
			#region Weight

			var weight1 = new RateablePartList();
			weight1.WeightUnit = "KG";
			weight1.AddPart(new RateablePart { Weight = 10 });
			weight1.AddPart(new RateablePart { Weight = 20 });

			var weight2 = new RateablePartList();
			weight2.WeightUnit = "G";
			weight2.AddPart(new RateablePart { Weight = 1000 });
			weight2.AddPart(new RateablePart { Weight = 2000 });

			var weight3 = new RateablePartList();
			weight3.WeightUnit = "KG";
			weight3.AddPart(new RateablePart { Weight = 30 });

			#endregion

			#region Volume

			var volume1 = new RateablePartList();
			volume1.VolumeUnit = "M3";
			volume1.AddPart(new RateablePart { Volume = 1 });
			volume1.AddPart(new RateablePart { Volume = 2 });

			var volume2 = new RateablePartList();
			volume2.VolumeUnit = "CC";
			volume2.AddPart(new RateablePart { Volume = 1000000 });
			volume2.AddPart(new RateablePart { Volume = 2000000 });

			var volume3 = new RateablePartList();
			volume3.VolumeUnit = "M3";
			volume3.AddPart(new RateablePart { Volume = 3 });

			#endregion

			#region Package

			var package1 = new RateablePartList();
			package1.AddPart(new RateablePart { PackageCount = 1, PackageType = "PLT" });
			package1.AddPart(new RateablePart { PackageCount = 2, PackageType = "BOX" });

			var package2 = new RateablePartList();
			package2.AddPart(new RateablePart { PackageCount = 3, PackageType = "CTN" });
			package2.AddPart(new RateablePart { PackageCount = 4, PackageType = "BOX" });

			#endregion

			var set1 = new RateableMeasureSet();
			set1.AddPartList(MeasureType.Weight, weight1);
			set1.AddPartList(MeasureType.Volume, volume1);
			set1.AddPartList(MeasureType.Package, package1);

			var set2 = new RateableMeasureSet();
			set2.AddPartList(MeasureType.Weight, weight2);
			set2.AddPartList(MeasureType.Volume, volume2);
			set2.AddPartList(MeasureType.Package, package2);

			var set3 = new RateableMeasureSet();
			set3.AddPartList(MeasureType.Weight, weight3);
			set3.AddPartList(MeasureType.Volume, volume3);

			var set = new RateableMeasureSet();
			set.SetCombinedMeasureFrom(new[] { set1, set2, set3 }, MeasureType.Weight, MeasureType.Volume, MeasureType.Package, MeasureType.LoadingMeters, MeasureType.Chargeable);

			var resultWeightParts = set.GetPartList(MeasureType.Weight);
			AssertEquals("KG", resultWeightParts.WeightUnit);
			var weightParts = resultWeightParts.Cast<RateablePart>().Select(x => new { x.Weight }).ToArray();
			var expectedWeight = new[]
			{
				new { Weight = (decimal)10 }, new { Weight = (decimal)20 }, new { Weight = (decimal)1 }, new { Weight = (decimal)2 }, new { Weight = (decimal)30 }
			};
			AssertContainsExactElementsInAnyOrder(expectedWeight, weightParts);

			var resultVolumeParts = set.GetPartList(MeasureType.Volume);
			AssertEquals("M3", resultVolumeParts.VolumeUnit);
			var volumeParts = resultVolumeParts.Cast<RateablePart>().Select(x => new { x.Volume }).ToArray();
			var expectedVolume = new[]
			{
				new { Volume = (decimal)1 }, new { Volume = (decimal)2 }, new { Volume = (decimal)1 }, new { Volume = (decimal)2 }, new { Volume = (decimal)3 }
			};
			AssertContainsExactElementsInAnyOrder(expectedVolume, volumeParts);

			var packageParts = set.GetPartList(MeasureType.Package).Cast<RateablePart>().Select(p => new { p.PackageCount, p.PackageType }).ToArray();
			var expectedPackageParts = new[]
			{
				new { PackageCount = (decimal?)1, PackageType = "PLT" },
				new { PackageCount = (decimal?)2, PackageType = "BOX" },
				new { PackageCount = (decimal?)3, PackageType = "CTN" },
				new { PackageCount = (decimal?)4, PackageType = "BOX" }
			};
			AssertContainsExactElementsInAnyOrder(expectedPackageParts, packageParts);

			Assert(true);
		}

		public void TestAddWarehouseBOMKitCount()
		{
			var set = new RateableMeasureSet();
			set.CreateWarehouseBOMKitPartList((s) => { });

			var pk1 = Guid.NewGuid();
			set.AddWarehouseBOMKitCount(pk1, 2m);
			var partList = set.GetPartList(MeasureType.BOMKit);
			AssertEquals(1, partList.Count);
			AssertEquals(pk1, partList[0].ProductPk);
			AssertEquals(2m, partList[0].UnitCount);

			var pk2 = Guid.NewGuid();
			set.AddWarehouseBOMKitCount(pk2, 3m);
			AssertEquals(2, partList.Count);
			var part2 = partList.Single(l => l.ProductPk == pk2);
			AssertEquals(3m, part2.UnitCount);
		}

		public void TestCreateWarehouseBOMKitPartList()
		{
			var set = new RateableMeasureSet();
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			set.CreateWarehouseBOMKitPartList((s) =>
			{
				s.AddWarehouseBOMKitCount(pk1, 2m);
				s.AddWarehouseBOMKitCount(pk2, 3m);
			});

			var partList = set.GetPartList(MeasureType.BOMKit);
			AssertEquals(2, partList.Count);
			var part1 = partList.Single(l => l.ProductPk == pk1);
			var part2 = partList.Single(l => l.ProductPk == pk2);
			AssertEquals(2m, part1.UnitCount);
			AssertEquals(3m, part2.UnitCount);
		}
	}
}
