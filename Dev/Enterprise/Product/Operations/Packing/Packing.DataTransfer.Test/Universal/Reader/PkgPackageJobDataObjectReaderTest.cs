using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Testing;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	public class PkgPackageJobDataObjectReaderTest : PkgPackageDataObjectReaderTestCase
	{
		#region TestPkgPackageJobDataObjectReaderRequiresParentJob

		public void TestPkgPackageJobDataObjectReaderRequiresParentJob()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);

			AssertExceptionThrown(typeof(ArgumentNullException), () => new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, new UniversalObjectFactory(), null));
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs()
		{
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "XYZ");

			var packageData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = "ABC", Weight = 150m };
			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData })
			{
				Content = CollectionContent.Complete
			});

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.MatchOnPackageIDs);
			var packageJob = reader.ReadIntoBusinessObject();
			AssertContainsExactElementsInAnyOrder("Should have deleted Packages that did not match.", new[] { package1 }, packageJob.Packages);
			AssertEquals("Should have updated Package Weight.", 150m, package1.KP_Weight);
		}

		#endregion

		#region HandlingUnits

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersMoved_PackedViaParentPackage_ShouldBeUpdated

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersMoved_PackedViaParentPackage_ShouldBeUpdated()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// Current Package Tree
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			//			HU2Inner2
			var hu1 = Helper.CreatePackage(packageJob.PK, "HU1", 1, PackType.Freight.PLT, 1, 1, 1, "M", 1, "M3", 1, "KG");
			var hu1Inner = Helper.CreatePackage(packageJob.PK, "HU1Inner", 1, PackType.Freight.PKG, 1, 1, 1, "M", 1, "M3", 1, "KG");
			hu1.Packages.Add(hu1Inner);
			var hu2 = Helper.CreatePackage(packageJob.PK, "HU2", 1, PackType.Freight.PLT, 1, 1, 1, "M", 1, "M3", 1, "KG");
			hu1.Packages.Add(hu2);
			var hu2Inner1 = Helper.CreatePackage(packageJob.PK, "HU2Inner1", 1, PackType.Freight.PKG, 1, 1, 1, "M", 1, "M3", 1, "KG");
			var hu2Inner2 = Helper.CreatePackage(packageJob.PK, "HU2Inner2", 1, PackType.Freight.PKG, 1, 1, 1, "M", 1, "M3", 1, "KG");
			hu2.Packages.Add(hu2Inner1);
			hu2.Packages.Add(hu2Inner2);

			// PackingLine Tree To Be Imported
			//	HU1
			//		HU1Inner
			//		HU2Inner2
			//		HU2
			//			HU2Inner1
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU1",
				Length = 5,
				Width = 5,
				Height = 5,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 5,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU1Inner",
				Length = 10,
				Width = 10,
				Height = 10,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 10,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 10,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU2",
				Length = 20,
				Width = 20,
				Height = 20,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 20,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 20,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU2Inner1",
				Length = 30,
				Width = 30,
				Height = 30,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 30,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 30,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2Inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU2Inner2",
				Length = 40,
				Width = 40,
				Height = 40,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 40,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 40,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU1Inner1, packingLineForHU2Inner2, packingLineForHU2 });
			packingLineForHU2.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2Inner1 });

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU1 })
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job has 5 packages.", new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			var hu1FromDB = newFactory.Load<PkgPackage>(hu1.PK);
			var hu1InnerFromDB = newFactory.Load<PkgPackage>(hu1Inner.PK);
			var hu2FromDB = newFactory.Load<PkgPackage>(hu2.PK);
			var hu2Inner1FromDB = newFactory.Load<PkgPackage>(hu2Inner1.PK);
			var hu2Inner2FromDB = newFactory.Load<PkgPackage>(hu2Inner2.PK);

			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.", new PkgPackage[] { hu1FromDB, hu1InnerFromDB, hu2FromDB, hu2Inner1FromDB, hu2Inner2FromDB }, packageJobFromDB.GetAllPackagesOnJob());

			var allPackageJobPackages = packageJobFromDB.GetAllPackagesOnJob();
			var hu1AfterRead = allPackageJobPackages.Single(p => p.PK == hu1.PK);
			AssertEquals("Should only have 3 inners", 3, hu1AfterRead.Packages.Count);

			var hu1InnerAfterRead = hu1AfterRead.Packages.Single(p => p.PK == hu1Inner.PK);
			var hu2Inner2AfterRead = hu1AfterRead.Packages.Single(p => p.PK == hu2Inner2.PK);
			var hu2AfterRead = hu1AfterRead.Packages.Single(p => p.PK == hu2.PK);
			AssertEquals("Should only have 1 inner", 1, hu2AfterRead.Packages.Count);

			var hu2Inner1AfterRead = hu2AfterRead.Packages.Single(p => p.PK == hu2Inner1.PK);

			AssertPackage(hu1AfterRead, "HU1", 1, PackType.Freight.PLT, 5, 5, 5, "CM", 5, "D3", 1000, "G", ZGuid.Empty);
			AssertPackage(hu1InnerAfterRead, "HU1Inner", 1, PackType.Freight.BOX, 10, 10, 10, "CM", 10, "D3", 10, "G", ZGuid.Empty);
			AssertPackage(hu2AfterRead, "HU2", 1, PackType.Freight.PLT, 20, 20, 20, "CM", 20, "D3", 20, "G", ZGuid.Empty);
			AssertPackage(hu2Inner1AfterRead, "HU2Inner1", 1, PackType.Freight.BOX, 30, 30, 30, "CM", 30, "D3", 30, "G", ZGuid.Empty);
			AssertPackage(hu2Inner2AfterRead, "HU2Inner2", 1, PackType.Freight.BOX, 40, 40, 40, "CM", 40, "D3", 40, "G", ZGuid.Empty);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersRemoved_PackedViaParentPackage_ShouldBeDeleted

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersRemoved_PackedViaParentPackage_ShouldBeDeleted()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// Current Package Tree
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			//			HU2Inner2
			var hu1 = Helper.CreatePackage(packageJob.PK, "HU1", 1, PackType.Freight.PLT, 1, 1, 1, "M", 1, "M3", 1, "KG");
			var hu1Inner = Helper.CreatePackage(packageJob.PK, "HU1Inner", 1, PackType.Freight.PKG, 1, 1, 1, "M", 1, "M3", 1, "KG");
			hu1.Packages.Add(hu1Inner);

			var hu2 = Helper.CreatePackage(packageJob.PK, "HU2", 1, PackType.Freight.PLT, 1, 1, 1, "M", 1, "M3", 1, "KG");
			hu1.Packages.Add(hu2);

			var hu2Inner1 = Helper.CreatePackage(packageJob.PK, "HU2Inner1", 1, PackType.Freight.PKG, 1, 1, 1, "M", 1, "M3", 1, "KG");
			var hu2Inner2 = Helper.CreatePackage(packageJob.PK, "HU2Inner2", 1, PackType.Freight.PKG, 1, 1, 1, "M", 1, "M3", 1, "KG");
			hu2.Packages.Add(hu2Inner1);
			hu2.Packages.Add(hu2Inner2);

			// PackingLine Tree To Be Imported
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU1",
				Length = 5,
				Width = 5,
				Height = 5,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 5,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU1Inner",
				Length = 10,
				Width = 10,
				Height = 10,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 10,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 10,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU2",
				Length = 20,
				Width = 20,
				Height = 20,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 20,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 20,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU2Inner1",
				Length = 30,
				Width = 30,
				Height = 30,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 30,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 30,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU1Inner1, packingLineForHU2 });
			packingLineForHU2.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2Inner1 });

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU1 })
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job should have 5 packages.", new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			var hu1FromDB = newFactory.Load<PkgPackage>(hu1.PK);
			var hu1InnerFromDB = newFactory.Load<PkgPackage>(hu1Inner.PK);
			var hu2FromDB = newFactory.Load<PkgPackage>(hu2.PK);
			var hu2Inner1FromDB = newFactory.Load<PkgPackage>(hu2Inner1.PK);

			Assert("Package should have been deleted after importing.", hu2Inner2.IsDeleted);

			AssertContainsExactElementsInAnyOrder("Package job should have 4 packages after importing.",
			new PkgPackage[] { hu1FromDB, hu1InnerFromDB, hu2FromDB, hu2Inner1FromDB }, packageJobFromDB.GetAllPackagesOnJob());

			var allPackageJobPackages = packageJobFromDB.GetAllPackagesOnJob();
			var hu1AfterRead = allPackageJobPackages.Single(p => p.PK == hu1.PK);

			AssertEquals("Should only have 2 inners.", 2, hu1AfterRead.Packages.Count);
			var hu1InnerAfterRead = hu1AfterRead.Packages.Single(p => p.PK == hu1Inner.PK);
			var hu2AfterRead = hu1AfterRead.Packages.Single(p => p.PK == hu2.PK);

			AssertEquals("Should only have 1 inner.", 1, hu2AfterRead.Packages.Count);
			var hu2Inner1AfterRead = hu2AfterRead.Packages.Single(p => p.PK == hu2Inner1.PK);

			AssertPackage(hu1AfterRead, "HU1", 1, PackType.Freight.PLT, 5, 5, 5, "CM", 5, "D3", 0, "G", ZGuid.Empty);
			AssertPackage(hu1InnerAfterRead, "HU1Inner", 1, PackType.Freight.BOX, 10, 10, 10, "CM", 10, "D3", 10, "G", ZGuid.Empty);
			AssertPackage(hu2AfterRead, "HU2", 1, PackType.Freight.PLT, 20, 20, 20, "CM", 20, "D3", 0, "G", ZGuid.Empty);
			AssertPackage(hu2Inner1AfterRead, "HU2Inner1", 1, PackType.Freight.BOX, 30, 30, 30, "CM", 30, "D3", 30, "G", ZGuid.Empty);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersMoved_PackedViaDivot_ShouldBeUpdated

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersMoved_PackedViaDivot_ShouldBeUpdated()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.BOFactory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Current Package Tree
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			//			HU2Inner2
			var hu1 = Helper.CreatePackage(packageJob.PK, "HU1", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Inner = Helper.CreatePackage(packageJob.PK, "HU1Inner", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2 = Helper.CreatePackage(packageJob.PK, "HU2", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2Inner1 = Helper.CreatePackage(packageJob.PK, "HU2Inner1", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2Inner2 = Helper.CreatePackage(packageJob.PK, "HU2Inner2", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");

			var hu1Hu1InnerDivot = Helper.PackHandlingUnit(hu1, hu1Inner, topHandlingUnit: hu1);
			var hu1Hu2Divot = Helper.PackHandlingUnit(hu1, hu2, topHandlingUnit: hu1);
			var hu2Hu2Inner1Divot = Helper.PackHandlingUnit(hu2, hu2Inner1, topHandlingUnit: hu1);
			var hu2Hu2Inner2Divot = Helper.PackHandlingUnit(hu2, hu2Inner2, topHandlingUnit: hu1);

			// PackingLine Tree To Be Imported
			//	HU1
			//		HU1Inner
			//		HU2Inner2
			//		HU2
			//			HU2Inner1
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU1",
				Length = 5,
				Width = 5,
				Height = 5,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 5,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU1Inner",
				Length = 10,
				Width = 10,
				Height = 10,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 10,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 10,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU2",
				Length = 20,
				Width = 20,
				Height = 20,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 20,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 20,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU2Inner1",
				Length = 30,
				Width = 30,
				Height = 30,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 30,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 30,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2Inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU2Inner2",
				Length = 40,
				Width = 40,
				Height = 40,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 40,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 40,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU1Inner1, packingLineForHU2Inner2, packingLineForHU2 });
			packingLineForHU2.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2Inner1 });

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU1 })
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job has 5 packages.",
			new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, parentJob, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			var hu1FromDB = newFactory.Load<PkgPackage>(hu1.PK);
			var hu1InnerFromDB = newFactory.Load<PkgPackage>(hu1Inner.PK);
			var hu2FromDB = newFactory.Load<PkgPackage>(hu2.PK);
			var hu2Inner1FromDB = newFactory.Load<PkgPackage>(hu2Inner1.PK);
			var hu2Inner2FromDB = newFactory.Load<PkgPackage>(hu2Inner2.PK);

			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.",
			new PkgPackage[] { hu1FromDB, hu1InnerFromDB, hu2FromDB, hu2Inner1FromDB, hu2Inner2FromDB }, packageJobFromDB.GetAllPackagesOnJob());

			AssertEquals("Should still have 4 divots", 4,
			newFactory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);

			var allPackageJobPackages = packageJobFromDB.GetAllPackagesOnJob();
			var hu1AfterRead = allPackageJobPackages.Single(p => p.PK == hu1.PK);

			AssertEquals("Should only have 3 inners", 3, hu1AfterRead.PackageHandlingUnitHandlingUnitDivots.Count);
			var hu1InnerAfterRead = hu1AfterRead.PackageHandlingUnitHandlingUnitDivots.Single(d => d.Package.PK == hu1Inner.PK).Package;
			var hu2Inner2AfterRead = hu1AfterRead.PackageHandlingUnitHandlingUnitDivots.Single(d => d.Package.PK == hu2Inner2.PK).Package;
			var hu2AfterRead = hu1AfterRead.PackageHandlingUnitHandlingUnitDivots.Single(d => d.Package.PK == hu2.PK).Package;

			AssertEquals("Should only have 1 inner", 1, hu2AfterRead.PackageHandlingUnitHandlingUnitDivots.Count);
			var hu2Inner1AfterRead = hu2AfterRead.PackageHandlingUnitHandlingUnitDivots.Single(d => d.Package.PK == hu2Inner1.PK).Package;

			AssertPackage(hu1AfterRead, "HU1", 1, PackType.Freight.PLT, 5, 5, 5, "CM", 5, "D3", 5, "G", ZGuid.Empty);
			AssertPackage(hu1InnerAfterRead, "HU1Inner", 1, PackType.Freight.BOX, 10, 10, 10, "CM", 10, "D3", 10, "G", hu1AfterRead.PK);
			AssertPackage(hu2AfterRead, "HU2", 1, PackType.Freight.PLT, 20, 20, 20, "CM", 20, "D3", 20, "G", hu1AfterRead.PK);
			AssertPackage(hu2Inner1AfterRead, "HU2Inner1", 1, PackType.Freight.BOX, 30, 30, 30, "CM", 30, "D3", 30, "G", hu1AfterRead.PK);
			AssertPackage(hu2Inner2AfterRead, "HU2Inner2", 1, PackType.Freight.BOX, 40, 40, 40, "CM", 40, "D3", 40, "G", hu1AfterRead.PK);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersRemoved_PackedViaDivot_ShouldBeDeleted

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersRemoved_PackedViaDivot_ShouldBeDeleted()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.BOFactory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Current Package Tree
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			//			HU2Inner2
			var hu1 = Helper.CreatePackage(packageJob.PK, "HU1", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Inner = Helper.CreatePackage(packageJob.PK, "HU1Inner", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2 = Helper.CreatePackage(packageJob.PK, "HU2", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2Inner1 = Helper.CreatePackage(packageJob.PK, "HU2Inner1", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2Inner2 = Helper.CreatePackage(packageJob.PK, "HU2Inner2", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Hu1InnerDivot = Helper.PackHandlingUnit(hu1, hu1Inner, topHandlingUnit: hu1);
			var hu1Hu2Divot = Helper.PackHandlingUnit(hu1, hu2, topHandlingUnit: hu1);
			var hu2Hu2Inner1Divot = Helper.PackHandlingUnit(hu2, hu2Inner1, topHandlingUnit: hu1);
			var hu2Hu2Inner2Divot = Helper.PackHandlingUnit(hu2, hu2Inner2, topHandlingUnit: hu1);

			// PackingLine Tree To Be Imported
			//	HU1
			//		HU2
			//			HU2Inner1
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU1",
				Length = 5,
				Width = 5,
				Height = 5,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 5,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU2",
				Length = 10,
				Width = 10,
				Height = 10,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 10,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 10,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU2Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HU2Inner1",
				Length = 20,
				Width = 20,
				Height = 20,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 20,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 20,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2 });
			packingLineForHU2.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2Inner1 });

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU1 })
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job has 5 packages.",
			new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, parentJob, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			var hu1FromDB = newFactory.Load<PkgPackage>(hu1.PK);
			var hu2FromDB = newFactory.Load<PkgPackage>(hu2.PK);
			var hu2Inner1FromDB = newFactory.Load<PkgPackage>(hu2Inner1.PK);

			AssertContainsExactElementsInAnyOrder("Package job should have 3 packages remaining.",
			new PkgPackage[] { hu1FromDB, hu2FromDB, hu2Inner1FromDB }, packageJobFromDB.GetAllPackagesOnJob());

			AssertEquals("Package should have been deleted.", true, hu1Inner.IsDeleted);
			AssertEquals("Package should have been deleted.", true, hu2Inner2.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu1Hu1InnerDivot.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu2Hu2Inner2Divot.IsDeleted);
			AssertEquals("Divot should not have been deleted.", false, hu2Hu2Inner1Divot.IsDeleted);
			AssertEquals("Divot should not have been deleted.", false, hu1Hu2Divot.IsDeleted);

			var remainingDivots = newFactory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Should have only 2 divots", new[] { hu1Hu2Divot.PK, hu2Hu2Inner1Divot.PK }, remainingDivots.Select(d => d.PK));

			var allPackageJobPackages = packageJobFromDB.GetAllPackagesOnJob();
			var hu1AfterRead = allPackageJobPackages.Single(p => p.PK == hu1.PK);

			AssertEquals("Should only have 1 inner", 1, hu1AfterRead.PackageHandlingUnitHandlingUnitDivots.Count);
			var hu2AfterRead = hu1AfterRead.PackageHandlingUnitHandlingUnitDivots.Single(d => d.Package.PK == hu2.PK).Package;

			AssertEquals("Should only have 1 inner", 1, hu2AfterRead.PackageHandlingUnitHandlingUnitDivots.Count);
			var hu2Inner1AfterRead = hu2AfterRead.PackageHandlingUnitHandlingUnitDivots.Single(d => d.Package.PK == hu2Inner1.PK).Package;

			AssertPackage(hu1AfterRead, "HU1", 1, PackType.Freight.PLT, 5, 5, 5, "CM", 5, "D3", 5, "G", ZGuid.Empty);
			AssertPackage(hu2AfterRead, "HU2", 1, PackType.Freight.PLT, 10, 10, 10, "CM", 10, "D3", 10, "G", hu1.PK);
			AssertPackage(hu2Inner1AfterRead, "HU2Inner1", 1, PackType.Freight.BOX, 20, 20, 20, "CM", 20, "D3", 20, "G", hu1.PK);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersUnpacked_PackedViaDivot_ShouldBeUpdated

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitInnersUnpacked_PackedViaDivot_ShouldBeUpdated()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.BOFactory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Current Package Tree
			//	HU
			//		Inner1
			//		Inner2
			var hu = Helper.CreatePackage(packageJob.PK, "HU", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var huInner1 = Helper.CreatePackage(packageJob.PK, "HUInner1", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var huInner2 = Helper.CreatePackage(packageJob.PK, "HUInner2", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var huInner1Divot = Helper.PackHandlingUnit(hu, huInner1, topHandlingUnit: hu);
			var huInner2Divot = Helper.PackHandlingUnit(hu, huInner2, topHandlingUnit: hu);

			// PackingLine Tree To Be Imported
			//	HU1
			//		Inner1
			//	Inner2
			var packingLineForHU = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU",
				Length = 5,
				Width = 5,
				Height = 5,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 5,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HUInner1",
				Length = 10,
				Width = 10,
				Height = 10,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 10,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 10,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU1Inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HUInner2",
				Length = 20,
				Width = 20,
				Height = 20,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 20,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 20,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			packingLineForHU.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU1Inner1 });

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU, packingLineForHU1Inner2 })
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job has 3 packages.",
			new PkgPackage[] { hu, huInner1, huInner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, parentJob, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			var huFromDB = newFactory.Load<PkgPackage>(hu.PK);
			var huInner1FromDB = newFactory.Load<PkgPackage>(huInner1.PK);
			var huInner2FromDB = newFactory.Load<PkgPackage>(huInner2.PK);

			AssertContainsExactElementsInAnyOrder("Package job should have 3 packages remaining.",
			new PkgPackage[] { huFromDB, huInner1FromDB, huInner2FromDB }, packageJobFromDB.GetAllPackagesOnJob());

			var onlyDivotRemaining = newFactory.BOFactory.Load<PkgPackageHandlingUnitDivot>(huInner1Divot.PK);
			AssertEquals(huFromDB.PK, onlyDivotRemaining.KPD_KP_HandlingUnit);
			AssertEquals(huInner1FromDB.PK, onlyDivotRemaining.KPD_KP_Package);
			AssertEquals("Unpacked package divot should have been deleted.", true, huInner2Divot.IsDeleted);
			AssertPackage(huFromDB, "HU", 1, PackType.Freight.PLT, 5, 5, 5, "CM", 5, "D3", 5, "G", ZGuid.Empty);
			AssertPackage(huInner1FromDB, "HUInner1", 1, PackType.Freight.BOX, 10, 10, 10, "CM", 10, "D3", 10, "G", hu.PK);
			AssertPackage(huInner2FromDB, "HUInner2", 1, PackType.Freight.BOX, 20, 20, 20, "CM", 20, "D3", 20, "G", ZGuid.Empty);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitDeletedAndInnersUnpacked_PackedViaDivot_ShouldBeUpdated

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitDeletedAndInnersUnpacked_PackedViaDivot_ShouldBeUpdated()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.BOFactory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Current Package Tree
			//	HU1
			//		Inner1
			//		Inner2
			var hu1 = Helper.CreatePackage(packageJob.PK, "HU", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Inner1 = Helper.CreatePackage(packageJob.PK, "HUInner1", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Inner2 = Helper.CreatePackage(packageJob.PK, "HUInner2", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Inner1Divot = Helper.PackHandlingUnit(hu1, hu1Inner1, topHandlingUnit: hu1);
			var hu1Inner2Divot = Helper.PackHandlingUnit(hu1, hu1Inner2, topHandlingUnit: hu1);

			// PackingLine Tree To Be Imported
			//	Inner1
			//	Inner2
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HUInner1",
				Length = 5,
				Width = 5,
				Height = 5,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 5,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};
			var packingLineForHU1Inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.BOX },
				ReferenceNumber = "HUInner2",
				Length = 10,
				Width = 10,
				Height = 10,
				LengthUnit = new UnitOfLength() { Code = "CM" },
				Volume = 10,
				VolumeUnit = new UnitOfVolume() { Code = "D3" },
				Weight = 10,
				WeightUnit = new UnitOfWeight() { Code = "G" }
			};

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU1Inner1, packingLineForHU1Inner2 })
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job has 3 packages.",
			new PkgPackage[] { hu1, hu1Inner1, hu1Inner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, parentJob, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			var hu1Inner1FromDB = newFactory.Load<PkgPackage>(hu1Inner1.PK);
			var hu1Inner2FromDB = newFactory.Load<PkgPackage>(hu1Inner2.PK);

			AssertContainsExactElementsInAnyOrder("Package job should have 2 packages remaining.",
			new PkgPackage[] { hu1Inner1FromDB, hu1Inner2FromDB }, packageJobFromDB.GetAllPackagesOnJob());

			AssertEquals("Package should have been deleted.", true, hu1.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu1Inner1Divot.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu1Inner2Divot.IsDeleted);
			AssertEquals("Should have only 0 divots", 0,
			newFactory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertPackage(hu1Inner1FromDB, "HUInner1", 1, PackType.Freight.BOX, 5, 5, 5, "CM", 5, "D3", 5, "G", ZGuid.Empty);
			AssertPackage(hu1Inner2FromDB, "HUInner2", 1, PackType.Freight.BOX, 10, 10, 10, "CM", 10, "D3", 10, "G", ZGuid.Empty);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitDeletedAndInnersUnpacked_PackedViaDivot_ShouldBeUpdated

		public void TestPkgPackageJobDataObjectReader_ImportOptions_MatchOnPackageIDs_MultiLevelHandlingUnitAllExistingPackagesRemoved_PackedViaDivot_ShouldBeDeleted()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.BOFactory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// Current Package Tree
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			//			HU2Inner2
			var hu1 = Helper.CreatePackage(packageJob.PK, "HU1", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Inner = Helper.CreatePackage(packageJob.PK, "HU1Inner", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2 = Helper.CreatePackage(packageJob.PK, "HU2", 1, PackType.Freight.PLT, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2Inner1 = Helper.CreatePackage(packageJob.PK, "HU2Inner1", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu2Inner2 = Helper.CreatePackage(packageJob.PK, "HU2Inner2", 1, PackType.Freight.PKG, 1m, 1m, 1m, "M", 1m, "M3", 1, "KG");
			var hu1Hu1InnerDivot = Helper.PackHandlingUnit(hu1, hu1Inner, topHandlingUnit: hu1);
			var hu1Hu2Divot = Helper.PackHandlingUnit(hu1, hu2, topHandlingUnit: hu1);
			var hu2Hu2Inner1Divot = Helper.PackHandlingUnit(hu2, hu2Inner1, topHandlingUnit: hu1);
			var hu2Hu2Inner2Divot = Helper.PackHandlingUnit(hu2, hu2Inner2, topHandlingUnit: hu1);

			// PackingLine Tree To Be Imported
			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				Content = CollectionContent.Complete
			});

			AssertContainsExactElementsInAnyOrder("Precondition - Package job has 5 packages.",
			new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2 }, packageJob.GetAllPackagesOnJob());

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, parentJob, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var packageJobFromDB = newFactory.Load<PkgPackageJob>(packageJob.PK);
			AssertEquals("Package job should have 0 packages remaining.", 0, packageJobFromDB.GetAllPackagesOnJob().Length);
			AssertEquals("Should have 0 divots remaining.", 0, newFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Package should have been deleted.", true, hu1.IsDeleted);
			AssertEquals("Package should have been deleted.", true, hu1Inner.IsDeleted);
			AssertEquals("Package should have been deleted.", true, hu2.IsDeleted);
			AssertEquals("Package should have been deleted.", true, hu2Inner1.IsDeleted);
			AssertEquals("Package should have been deleted.", true, hu2Inner2.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu1Hu1InnerDivot.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu1Hu2Divot.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu2Hu2Inner1Divot.IsDeleted);
			AssertEquals("Divot should have been deleted.", true, hu2Hu2Inner2Divot.IsDeleted);
		}

		#endregion

		void AssertPackage(PkgPackage package, string packageId, int qty, string packType, decimal length, decimal width, decimal height, string dimUQ, decimal volume, string volumeUQ, decimal weight, string weightUQ, ZGuid topHandlingUnitPackagePK)
		{
			CombineAssertions(() =>
			{
				AssertEquals(packageId, package.KP_PackageID);
				AssertEquals(topHandlingUnitPackagePK, package.KP_KP_TopHandlingUnitPackage);
				AssertEquals(qty, package.KP_PackageQty);
				AssertEquals(packType, package.KP_F3_NKPackType);
				AssertEquals(length, package.KP_Length);
				AssertEquals(width, package.KP_Width);
				AssertEquals(height, package.KP_Height);
				AssertEquals(dimUQ, package.KP_DimensionUQ);
				AssertEquals(volume, package.KP_Volume);
				AssertEquals(volumeUQ, package.KP_VolumeUQ);
				AssertEquals(weight, package.KP_Weight);
				AssertEquals(weightUQ, package.KP_WeightUQ);
			});
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_KeepUnmatchedPackages

		public void TestPkgPackageJobDataObjectReader_ImportOptions_KeepUnmatchedPackages()
		{
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "XYZ");

			var packageData = new PackingLine { PackQty = 1, ReferenceNumber = "ABC", Weight = 150m };
			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData })
			{
				Content = CollectionContent.Complete
			});

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.KeepUnmatchedPackages);
			var packageJob = reader.ReadIntoBusinessObject();
			AssertContainsExactElementsInAnyOrder("Should *not* have deleted Packages that did not match.", new[] { package1, package2 }, packageJob.Packages);
			AssertEquals("Should have updated Package Weight.", 150m, package1.KP_Weight);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_ImportOptions_Default_ExistingNonTrackablePackagesShouldBeDeleted

		public void TestPkgPackageJobDataObjectReader_ImportOptions_Default_ExistingNonTrackablePackagesShouldBeDeleted()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// Current Package Tree
			//	HU
			//		PKG
			//		BOX
			//		BAG
			var hu1 = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU");
			var nonTrackable1 = packageJob.Packages.AddNew(PackType.Freight.PKG);
			var nonTrackable2 = packageJob.Packages.AddNew(PackType.Freight.BOX);
			var nonTrackable3 = packageJob.Packages.AddNew(PackType.Freight.BAG);
			hu1.Packages.Add(nonTrackable1);
			hu1.Packages.Add(nonTrackable2);
			hu1.Packages.Add(nonTrackable3);

			nonTrackable1.KP_Weight = 1m;
			nonTrackable2.KP_Weight = 2m;
			nonTrackable3.KP_Weight = 3m;

			// PackingLine Tree
			//	HU
			//		PKG
			//		CTN
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU"
			};
			var packingLineForNewNonTrackable1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PKG },
				Weight = 1m
			};
			var packingLineForNewNonTrackable2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.CTN },
				Weight = 2m
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForNewNonTrackable1, packingLineForNewNonTrackable2 });

			AssertContainsExactElementsInAnyOrder("Precondition - Package job should have 4 packages.", new PkgPackage[] { hu1, nonTrackable1, nonTrackable2, nonTrackable3 }, packageJob.GetAllPackagesOnJob());

			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineForHU1 })
			{
				Content = CollectionContent.Complete
			});

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.MatchOnPackageIDs);
			var packageJobFromReader = reader.ReadIntoBusinessObject();
			var allPackageJobPackages = packageJobFromReader.GetAllPackagesOnJob();

			var huAfterRead = allPackageJobPackages.Single(p => p.PK == hu1.PK);

			AssertEquals("Should only have 2 inners", 2, huAfterRead.Packages.Count);
			var nonTrackable1AfterRead = huAfterRead.Packages.Single(p => p.KP_F3_NKPackType == PackType.Freight.PKG);
			var nonTrackable2AfterRead = huAfterRead.Packages.Single(p => p.KP_F3_NKPackType == PackType.Freight.CTN);

			AssertContainsExactElementsInAnyOrder("Package job should only have the newly imported packages.", new PkgPackage[] { huAfterRead, nonTrackable1AfterRead, nonTrackable2AfterRead }, packageJob.GetAllPackagesOnJob());
			AssertEquals("Weight should have been updated for matched packages.", 1m, nonTrackable1AfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 2m, nonTrackable2AfterRead.KP_Weight);
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_FieldMappings

		public void TestPkgPackageJobDataObjectReader_FieldMappings()
		{
			AssertPkgPackageJobDataObjectReader_FieldMappings(shouldImportBookedDimensions: false);
		}

		public void TestPkgPackageJobDataObjectReader_FieldMappings_ShouldAlsoImportBookedDimensions()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsImportingBookedDimensions);
			var parentJob = Factory.New<DummyWithPackingSupportsImportingBookedDimensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			AssertPkgPackageJobDataObjectReader_FieldMappings(shouldImportBookedDimensions: true, pkgPackageJob);
		}

		void AssertPkgPackageJobDataObjectReader_FieldMappings(bool shouldImportBookedDimensions, PkgPackageJob customPackageJob = null)
		{
			#region SetupPackingJob

			Data.CreatePackingData();
			Factory.SaveForTesting();

			var pkgPackageJob = customPackageJob ?? Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";

			var container = pkgPackageJob.Packages.AddNew("CNT");
			container.KP_PackageQty = 1;
			container.Container.K0_AirVentFlowRate = 7m;
			container.Container.K0_AirVentFlowRateUnit = "M2";
			container.Container.K0_ContainerMode = "AIR";
			container.KP_DunnageWeight = 8m;
			container.Container.K0_HumidityPercent = 9;
			container.Container.K0_IsControlledAtmosphere = true;
			container.Container.K0_IsDamaged = true;
			container.Container.K0_IsEmpty = true;
			container.Container.K0_IsSealOk = true;
			container.Container.K0_IsShipperOwned = true;
			container.Container.K0_Quality = "RIC";
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			container.Container.K0_RefrigGeneratorID = "REFRIG123";
			container.Container.K0_Seal1 = "SEAL-1";
			container.Container.K0_Seal2 = "SEAL-2";
			container.Container.K0_Seal3 = "SEAL-3";
			container.Container.K0_SetPointTemp = 10m;
			container.Container.K0_SetPointTempUnit = Constants.Temperature.Centigrade;
			container.Container.K0_Status = "ARV";
			container.KP_TareWeight = 11m;
			container.Container.K0_TempRecorderSerialNumber = "TEMPSER123";
			container.KP_DimensionUQ = "M";
			container.KP_Height = 1m;
			container.KP_Length = 2m;
			container.KP_PackageID = "CONT-1";
			container.KP_VolumeUQ = "M3";
			container.KP_Weight = 10m; // bumped by 9 when inners weight set
			container.KP_WeightUQ = "T";
			container.KP_Width = 6m;
			container.KP_TransportRef = "TRANSPORT REF";
			container.KP_GoodsDescription = "GOODS DESC";
			container.KP_HSCode = "HARMON CODE";
			container.KP_Volume = 12m;
			container.UNDGs.Add(Data.UndgDataItemEXP);

			var box = container.Packages.AddNew("BOX");
			box.KP_DimensionUQ = "M";
			box.KP_Height = 1m;
			box.KP_Length = 2m;
			box.KP_PackageID = "PACKAGE123";
			box.KP_PackageQty = 1;
			box.KP_VolumeUQ = "M3";
			box.KP_DunnageWeight = 4m;
			box.KP_TareWeight = 5.5m;
			box.KP_Weight = 5m; // bumped by 4 when inner weight set
			box.KP_WeightUQ = "T";
			box.KP_Width = 6m;
			box.KP_MarksAndNumbers = "MARK123";
			box.KP_TransportRef = "TRANSPORT REF";
			box.KP_GoodsDescription = "GOODS DESC";
			box.KP_HSCode = "HARMON CODE";
			box.KP_ExternalReference = "BOX External Ref";
			box.KP_Volume = 12m;
			box.KP_IsFumigated = true;
			box.KP_IsNonStackable = true;
			box.KP_IsTopLoadOnly = true;
			box.KP_IsHeatTreated = true;
			box.KP_IsISPMPallet = true;
			box.KP_IsPillaged = true;

			var ctn = box.Packages.AddNew("CTN");
			ctn.KP_DimensionUQ = "IN";
			ctn.KP_Height = 7m;
			ctn.KP_Length = 8m;
			ctn.KP_PackageID = "CTN123";
			ctn.KP_PackageQty = 9;
			ctn.KP_VolumeUQ = "M3";
			ctn.KP_DunnageWeight = 3m;
			ctn.KP_TareWeight = 5m;
			ctn.KP_Weight = 4;
			ctn.KP_WeightUQ = "T";
			ctn.KP_Width = 11m;
			ctn.KP_MarksAndNumbers = "CTN MARK123";
			ctn.KP_TransportRef = "CTN TRANSPORT REF";
			ctn.KP_GoodsDescription = "CTN GOODS DESC";
			ctn.KP_HSCode = "CTN HARMON CODE";
			ctn.KP_ExternalReference = "CTN External Ref";
			ctn.KP_Volume = 13m;
			ctn.UNDGs.Add(Data.UndgDataItemLOS);
			ctn.KP_IsFumigated = true;
			ctn.KP_IsNonStackable = true;
			ctn.KP_IsTopLoadOnly = true;
			ctn.KP_IsHeatTreated = true;
			ctn.KP_IsISPMPallet = true;
			ctn.KP_IsPillaged = true;

			#endregion

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, dummy, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			#region AssertPackingJob

			AssertNotNull(pkgJobRead);
			AssertNotEquals(pkgPackageJob, pkgJobRead);

			var containerRead = pkgJobRead.Packages[0];
			CombineAssertions("containerRead", () =>
			{
				AssertEquals("containerRead.KP_DimensionUQ", "M", containerRead.KP_DimensionUQ);
				AssertEquals("containerRead.KP_Height", 1m, containerRead.KP_Height);
				AssertEquals("containerRead.KP_Length", 2m, containerRead.KP_Length);
				AssertEquals("containerRead.KP_PackageID", "CONT-1", containerRead.KP_PackageID);
				AssertEquals("containerRead.KP_PackageQty", 1, containerRead.KP_PackageQty);
				AssertEquals("containerRead.KP_Volume", 12m, containerRead.KP_Volume);
				AssertEquals("containerRead.KP_VolumeUQ", "M3", containerRead.KP_VolumeUQ);
				AssertEquals("containerRead.KP_Weight", 19.001m, containerRead.KP_Weight);
				AssertEquals("containerRead.KP_WeightUQ", "T", containerRead.KP_WeightUQ);
				AssertEquals("containerRead.KP_Width", 6m, containerRead.KP_Width);
				AssertEquals("containerRead.KP_TareWeight", 11m, containerRead.KP_TareWeight);
				AssertEquals("containerRead.KP_DunnageWeight", 8m, containerRead.KP_DunnageWeight);

				if (shouldImportBookedDimensions)
				{
					AssertEquals("containerRead.BookedDimensions.KPB_KP_Package", containerRead.PK, containerRead.BookedDimensions.KPB_KP_Package);
					AssertEquals("containerRead.BookedDimensions.KPB_DimensionUQ", "M", containerRead.BookedDimensions.KPB_DimensionUQ);
					AssertEquals("containerRead.BookedDimensions.KPB_Height", 1m, containerRead.BookedDimensions.KPB_Height);
					AssertEquals("containerRead.BookedDimensions.KPB_Length", 2m, containerRead.BookedDimensions.KPB_Length);
					AssertEquals("containerRead.BookedDimensions.KPB_PackageQty", 1, containerRead.BookedDimensions.KPB_PackageQty);
					AssertEquals("containerRead.BookedDimensions.KPB_Volume", 12m, containerRead.BookedDimensions.KPB_Volume);
					AssertEquals("containerRead.BookedDimensions.KPB_VolumeUQ", "M3", containerRead.BookedDimensions.KPB_VolumeUQ);
					AssertEquals("containerRead.BookedDimensions.KPB_Weight", 19.001m, containerRead.BookedDimensions.KPB_Weight);
					AssertEquals("containerRead.BookedDimensions.KPB_WeightUQ", "T", containerRead.BookedDimensions.KPB_WeightUQ);
					AssertEquals("containerRead.BookedDimensions.KPB_Width", 6m, containerRead.BookedDimensions.KPB_Width);
				}

				AssertEquals("containerRead.KP_TransportRef", "TRANSPORT REF", containerRead.KP_TransportRef);
				AssertEquals("containerRead.KP_GoodsDescription", "GOODS DESC", containerRead.KP_GoodsDescription);
				AssertEquals("containerRead.KP_HSCode", "HARMON CODE", containerRead.KP_HSCode);
				AssertEquals("containerRead.Container.K0_AirVentFlowRate", 7m, containerRead.Container.K0_AirVentFlowRate);
				AssertEquals("containerRead.Container.K0_AirVentFlowRateUnit", "M2", containerRead.Container.K0_AirVentFlowRateUnit);
				AssertEquals("containerRead.Container.K0_ContainerMode", "AIR", containerRead.Container.K0_ContainerMode);
				AssertEquals("containerRead.KP_DunnageWeight", 8m, containerRead.KP_DunnageWeight);
				AssertEquals("containerRead.Container.K0_HumidityPercent", (ZByte)9, containerRead.Container.K0_HumidityPercent);
				AssertEquals("containerRead.Container.K0_IsControlledAtmosphere", true, containerRead.Container.K0_IsControlledAtmosphere);
				AssertEquals("containerRead.Container.K0_IsDamaged", true, containerRead.Container.K0_IsDamaged);
				AssertEquals("containerRead.Container.K0_IsEmpty", true, containerRead.Container.K0_IsEmpty);
				AssertEquals("containerRead.Container.K0_IsSealOk", true, containerRead.Container.K0_IsSealOk);
				AssertEquals("containerRead.Container.K0_IsShipperOwned", true, containerRead.Container.K0_IsShipperOwned);
				AssertEquals("containerRead.Container.K0_Quality", "RIC", containerRead.Container.K0_Quality);
				AssertEquals("containerRead.Container.K0_RC_ContainerType", "20GP", containerRead.Container.ContainerType.RC_Code);
				AssertEquals("containerRead.Container.K0_RefrigGeneratorID", "REFRIG123", containerRead.Container.K0_RefrigGeneratorID);
				AssertEquals("containerRead.Container.K0_Seal1", "SEAL-1", containerRead.Container.K0_Seal1);
				AssertEquals("containerRead.Container.K0_Seal2", "SEAL-2", containerRead.Container.K0_Seal2);
				AssertEquals("containerRead.Container.K0_Seal3", "SEAL-3", containerRead.Container.K0_Seal3);
				AssertEquals("containerRead.Container.K0_SetPointTemp", 10m, containerRead.Container.K0_SetPointTemp);
				AssertEquals("containerRead.Container.K0_SetPointTempUnit", Constants.Temperature.Centigrade, containerRead.Container.K0_SetPointTempUnit);
				AssertEquals("containerRead.Container.K0_Status", "ARV", containerRead.Container.K0_Status);
				AssertEquals("containerRead.KP_TareWeight", 11m, containerRead.KP_TareWeight);
				AssertEquals("containerRead.Container.K0_TempRecorderSerialNumber", "TEMPSER123", containerRead.Container.K0_TempRecorderSerialNumber);
				AssertEquals("Container should Contain 1 Dangerous Goods", 1, containerRead.UNDGs.Count);
				AssertCollectionContains("EXP", containerRead.UNDGs.Select(u => u.Substance.DG_Code));
			});

			var boxRead = containerRead.Packages[0];
			CombineAssertions("boxRead", () =>
			{
				AssertEquals("boxRead.KP_DimensionUQ", "M", boxRead.KP_DimensionUQ);
				AssertEquals("boxRead.KP_Height", 1m, boxRead.KP_Height);
				AssertEquals("boxRead.KP_Length", 2m, boxRead.KP_Length);
				AssertEquals("boxRead.KP_PackageID", "PACKAGE123", boxRead.KP_PackageID);
				AssertEquals("boxRead.KP_PackageQty", 1, boxRead.KP_PackageQty);
				AssertEquals("boxRead.KP_F3_NKPackType", "BOX", boxRead.KP_F3_NKPackType);
				AssertEquals("boxRead.KP_Volume", 12m, boxRead.KP_Volume);
				AssertEquals("boxRead.KP_VolumeUQ", "M3", boxRead.KP_VolumeUQ);
				AssertEquals("boxRead.KP_Weight", 9m, boxRead.KP_Weight);
				AssertEquals("boxRead.KP_WeightUQ", "T", boxRead.KP_WeightUQ);
				AssertEquals("boxRead.KP_Width", 6m, boxRead.KP_Width);
				AssertEquals("boxRead.KP_TareWeight", 5.5m, boxRead.KP_TareWeight);
				AssertEquals("boxRead.KP_DunnageWeight", 4m, boxRead.KP_DunnageWeight);
				AssertEquals("boxRead.KP_IsFumigated", true, boxRead.KP_IsFumigated);
				AssertEquals("boxRead.KP_IsNonStackable", true, boxRead.KP_IsNonStackable);
				AssertEquals("boxRead.KP_IsTopLoadOnly", true, boxRead.KP_IsTopLoadOnly);
				AssertEquals("boxRead.KP_IsHeatTreated", true, boxRead.KP_IsHeatTreated);
				AssertEquals("boxRead.KP_IsISPMPallet", true, boxRead.KP_IsISPMPallet);
				AssertEquals("boxRead.KP_IsPillaged", true, boxRead.KP_IsPillaged);

				if (shouldImportBookedDimensions)
				{
					AssertEquals("boxRead.BookedDimensions.KPB_KP_Package", boxRead.PK, boxRead.BookedDimensions.KPB_KP_Package);
					AssertEquals("boxRead.BookedDimensions.KPB_DimensionUQ", "M", boxRead.BookedDimensions.KPB_DimensionUQ);
					AssertEquals("boxRead.BookedDimensions.KPB_Height", 1m, boxRead.BookedDimensions.KPB_Height);
					AssertEquals("boxRead.BookedDimensions.KPB_Length", 2m, boxRead.BookedDimensions.KPB_Length);
					AssertEquals("boxRead.BookedDimensions.KPB_PackageQty", 1, boxRead.BookedDimensions.KPB_PackageQty);
					AssertEquals("boxRead.BookedDimensions.KPB_Volume", 12m, boxRead.BookedDimensions.KPB_Volume);
					AssertEquals("boxRead.BookedDimensions.KPB_VolumeUQ", "M3", boxRead.BookedDimensions.KPB_VolumeUQ);
					AssertEquals("boxRead.BookedDimensions.KPB_Weight", 9m, boxRead.BookedDimensions.KPB_Weight);
					AssertEquals("boxRead.BookedDimensions.KPB_WeightUQ", "T", boxRead.BookedDimensions.KPB_WeightUQ);
					AssertEquals("boxRead.BookedDimensions.KPB_Width", 6m, boxRead.BookedDimensions.KPB_Width);
				}

				AssertEquals("boxRead.KP_TransportRef", "TRANSPORT REF", boxRead.KP_TransportRef);
				AssertEquals("boxRead.KP_MarksAndNumbers", "MARK123", boxRead.KP_MarksAndNumbers);
				AssertEquals("boxRead.KP_GoodsDescription", "GOODS DESC", boxRead.KP_GoodsDescription);
				AssertEquals("boxRead.KP_HSCode", "HARMON CODE", boxRead.KP_HSCode);
				AssertEquals("boxRead.KP_ExternalRef", "BOX External Ref", boxRead.KP_ExternalReference);
				AssertEquals("Box should Contain 0 Dangerous Goods", 0, boxRead.UNDGs.Count);
			});

			var ctnRead = boxRead.Packages[0];
			CombineAssertions("ctnRead", () =>
			{
				AssertEquals("ctnRead.KP_DimensionUQ", "IN", ctnRead.KP_DimensionUQ);
				AssertEquals("ctnRead.KP_Height", 7m, ctnRead.KP_Height);
				AssertEquals("ctnRead.KP_Length", 8m, ctnRead.KP_Length);
				AssertEquals("ctnRead.KP_PackageID", "", ctnRead.KP_PackageID);
				AssertEquals("ctnRead.KP_PackageQty", 9, ctnRead.KP_PackageQty);
				AssertEquals("ctnRead.KP_F3_NKPackType", "CTN", ctnRead.KP_F3_NKPackType);
				AssertEquals("ctnRead.KP_Volume", 13m, ctnRead.KP_Volume);
				AssertEquals("ctnRead.KP_VolumeUQ", "M3", ctnRead.KP_VolumeUQ);
				AssertEquals("ctnRead.KP_Weight", 4m, ctnRead.KP_Weight);
				AssertEquals("ctnRead.KP_WeightUQ", "T", ctnRead.KP_WeightUQ);
				AssertEquals("ctnRead.KP_Width", 11m, ctnRead.KP_Width);
				AssertEquals("ctnRead.KP_TareWeight", 5m, ctnRead.KP_TareWeight);
				AssertEquals("ctnRead.KP_DunnageWeight", 3m, ctnRead.KP_DunnageWeight);
				AssertEquals("ctnRead.KP_IsFumigated", true, ctnRead.KP_IsFumigated);
				AssertEquals("ctnRead.KP_IsNonStackable", true, ctnRead.KP_IsNonStackable);
				AssertEquals("ctnRead.KP_IsTopLoadOnly", true, ctnRead.KP_IsTopLoadOnly);
				AssertEquals("ctnRead.KP_IsHeatTreated", true, ctnRead.KP_IsHeatTreated);
				AssertEquals("ctnRead.KP_IsISPMPallet", true, ctnRead.KP_IsISPMPallet);
				AssertEquals("ctnRead.KP_IsPillaged", true, ctnRead.KP_IsPillaged);

				if (shouldImportBookedDimensions)
				{
					AssertEquals("ctnRead.BookedDimensions.KPB_KP_Package", ctnRead.PK, ctnRead.BookedDimensions.KPB_KP_Package);
					AssertEquals("ctnRead.BookedDimensions.KPB_DimensionUQ", "IN", ctnRead.BookedDimensions.KPB_DimensionUQ);
					AssertEquals("ctnRead.BookedDimensions.KPB_Height", 7m, ctnRead.BookedDimensions.KPB_Height);
					AssertEquals("ctnRead.BookedDimensions.KPB_Length", 8m, ctnRead.BookedDimensions.KPB_Length);
					AssertEquals("ctnRead.BookedDimensions.KPB_PackageQty", 9, ctnRead.BookedDimensions.KPB_PackageQty);
					AssertEquals("ctnRead.BookedDimensions.KPB_Volume", 13m, ctnRead.BookedDimensions.KPB_Volume);
					AssertEquals("ctnRead.BookedDimensions.KPB_VolumeUQ", "M3", ctnRead.BookedDimensions.KPB_VolumeUQ);
					AssertEquals("ctnRead.BookedDimensions.KPB_Weight", 4m, ctnRead.BookedDimensions.KPB_Weight);
					AssertEquals("ctnRead.BookedDimensions.KPB_WeightUQ", "T", ctnRead.BookedDimensions.KPB_WeightUQ);
					AssertEquals("ctnRead.BookedDimensions.KPB_Width", 11m, ctnRead.BookedDimensions.KPB_Width);
				}

				AssertEquals("ctnRead.KP_TransportRef", "CTN TRANSPORT REF", ctnRead.KP_TransportRef);
				AssertEquals("ctnRead.KP_MarksAndNumbers", "CTN MARK123", ctnRead.KP_MarksAndNumbers);
				AssertEquals("ctnRead.KP_GoodsDescription", "CTN GOODS DESC", ctnRead.KP_GoodsDescription);
				AssertEquals("ctnRead.KP_HSCode", "CTN HARMON CODE", ctnRead.KP_HSCode);
				AssertEquals("ctnRead.KP_ExternalReference", "CTN External Ref", ctnRead.KP_ExternalReference);
				AssertEquals("Box should Contain 1 Dangerous Good", 1, ctnRead.UNDGs.Count);
				AssertCollectionContains("LOS", ctnRead.UNDGs.Select(u => u.Substance.DG_Code));
			});

			if (!shouldImportBookedDimensions)
			{
				var bookedDimensions = Factory.Load<PkgPackageBookedDetail>(new ZQuery());
				AssertEquals("Given the dummy packing object does not implement IPackingParentSupportsImportingBookedDimensions then the import should not import any booked dimensions.", 0, bookedDimensions.Length);
			}

			#endregion
		}

		#endregion

		#region TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_ShouldRemapPackType

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_ShouldRemapPackType()
		{
			Data.CreatePackingData();

			// Scenario 1: 
			// When package has a mapping from customs to freight pack type in country custom registry;
			// Should remap the pack type.

			ExecutePkgPackageJobObjectReaderTest("US", DataContextType.CustomsDeclaration, "PL1", new PackageType() { Code = "PK" }, 1);
			AssertPkgPackageJobObjectReaderTest("PL1", "PKG", 1);

			// Scenario 2: 
			// When package does not have a mapping from customs to freight pack type;
			// Should keep the existing pack type.

			ExecutePkgPackageJobObjectReaderTest("US", DataContextType.CustomsDeclaration, "PL2", new PackageType() { Code = "XYZ" }, 1);
			AssertPkgPackageJobObjectReaderTest("PL2", "XYZ", 1);

			// Scenario 3:
			// When a country doesn't have registry for mapping customs to freight type;
			// Should keep the existing pack type.

			ExecutePkgPackageJobObjectReaderTest("IN", DataContextType.CustomsDeclaration, "PL3", new PackageType() { Code = "PK" }, 1);
			AssertPkgPackageJobObjectReaderTest("PL3", "PK", 1);

			// Scenario 4:
			// When data context type is not Custom Declaration;
			// Should keep the existing pack type.

			ExecutePkgPackageJobObjectReaderTest("US", DataContextType.ForwardingShipment, "PL4", new PackageType() { Code = "PK" }, 1);
			AssertPkgPackageJobObjectReaderTest("PL4", "PK", 1);
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WhenPackTypeNull_DoNotClearPackType()
		{
			Data.CreatePackingData();
			// Scenario 5:
			// When data context type is Custom Declaration and the pack type is null;
			// Should still default to PKG as per PkgPackageDataObjectReader.

			ExecutePkgPackageJobObjectReaderTest("US", DataContextType.CustomsDeclaration, "PL5", null, 1);
			AssertPkgPackageJobObjectReaderTest("PL5", "PKG", 1);
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WhenPackTypeCodeNull_DoNotClearPackType()
		{
			Data.CreatePackingData();

			// Scenario 6:
			// When data context type is Custom Declaration and the pack type has null code;
			// Should still default to PKG as per PkgPackageDataObjectReader.

			ExecutePkgPackageJobObjectReaderTest("US", DataContextType.CustomsDeclaration, "PL6", new PackageType() { Code = null }, 1);
			AssertPkgPackageJobObjectReaderTest("PL6", "PKG", 1);
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WhenPackTypeCodeEmpty_DoNotClearPackType()
		{
			Data.CreatePackingData();

			// Scenario 7:
			// When data context type is Custom Declaration and the pack type has empty string as code;
			// Should still default to PKG as per PkgPackageDataObjectReader.

			ExecutePkgPackageJobObjectReaderTest("US", DataContextType.CustomsDeclaration, "PL7", new PackageType() { Code = ZString.Empty }, 1);
			AssertPkgPackageJobObjectReaderTest("PL7", "PKG", 1);
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WithPacksConversion_US()
		{
			// Scenario 8-US:
			// When data context type is Custom Declaration and the pack type has code;
			// Should return the designated result in Packs Conversion. if no conversion defined, then return the same value.

			AssertPackConversion(Core.Constants.CountryCodes.UnitedStates, "US1", "CTN");
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WithPacksConversion_CH()
		{
			// Scenario 9-CH:
			// When data context type is Custom Declaration and the pack type has code;
			// Should return the designated result in Packs Conversion. if no conversion defined, then return the same value.

			AssertPackConversion(Core.Constants.CountryCodes.Switzerland, "CH1", "CTN");
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WithPacksConversion_NA()
		{
			// Scenario 10-NA:
			// When data context type is Custom Declaration and the pack type has code;
			// Should return the designated result in Packs Conversion. if no conversion defined, then return the same value.

			var refCusCodeList = Factory.New<Customs.Universal.ZZRefCusCodeListCombined>();
			refCusCodeList.ZZD_CodeType = "ASYCO";
			refCusCodeList.ZZD_CountryOrGrouping = "ZZ";
			refCusCodeList.ZZD_Code = "NA";
			refCusCodeList.ZZD_Description = "Namibia";
			refCusCodeList.ZZD_StartDate = ZDateTime.Today;
			refCusCodeList.ZZD_EndDate = ZDateTime.Today;
			Factory.SaveForTesting();

			AssertPackConversion(Core.Constants.CountryCodes.Namibia, "NA1", "CTN");
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WithPacksConversion_FJ()
		{
			// Scenario 11-FJ:
			// When data context type is Custom Declaration and the pack type has code;
			// Should return the designated result in Packs Conversion. if no conversion defined, then return the same value.

			AssertPackConversion(Core.Constants.CountryCodes.Fiji, "FJ1", "CTN");
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WithPacksConversion_AT()
		{
			// Scenario 12-AT:
			// When data context type is Custom Declaration and the pack type has code;
			// Should return the designated result in Packs Conversion. if no conversion defined, then return the same value.

			AssertPackConversion(Core.Constants.CountryCodes.Austria, "AT1", "CTN");
		}

		public void TestPkgPackageJobObjectReader_WhenDataSourceCustomDeclaration_WithPacksConversion_AU()
		{
			// Scenario 13-AU:
			// When data context type is Custom Declaration and the pack type has code;
			// Should return the designated result in Packs Conversion. if no conversion defined, then return the same value.

			AssertPackConversion(Core.Constants.CountryCodes.Australia, "AU1", "CTN");
		}

		void AssertPackConversion(string countryCode, string customsPack, string commercialPack)
		{
			Data.CreatePackingData();
			CreateRefPack(countryCode, customsPack, commercialPack);
			ExecutePkgPackageJobObjectReaderTest(countryCode, DataContextType.CustomsDeclaration, customsPack, new PackageType() { Code = customsPack }, 1);
			AssertPkgPackageJobObjectReaderTest(customsPack, commercialPack, 1);
		}

		void CreateRefPack(string countryCode, string customsPack, string commercialPack)
		{
			var refPack = Factory.BOFactory.New<BaseRefPacks>();
			refPack.RP_Type = RPTypeList.Codes.PackingDeclaration;
			refPack.RP_CustomsCountry = countryCode;
			refPack.RP_CustomsPack = customsPack;
			refPack.RP_CommercialPack = commercialPack;
			Factory.SaveForTesting();
		}

		void ExecutePkgPackageJobObjectReaderTest(string country, DataContextType contextType, string referenceNumber, PackageType packType, int packQty)
		{
			// Arrange

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = packQty,
				PackType = packType,
				ReferenceNumber = referenceNumber
			};

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine })
			{
				Content = CollectionContent.Complete
			});

			var company = Factory.New<GlbCompany>();
			company.SetCountry(country);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(company);
			dataContext.AddDataSource(contextType, "DMY");
			shipment.DataContext = dataContext;

			Logger.TopLevelDataObject = shipment;

			var packageHeaders = Factory.Load<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.KPH_PackageID, referenceNumber));

			AssertEquals(
				string.Format("Precondtion: There should be no package with ID = {0}", referenceNumber),
				0,
				packageHeaders.Length);

			// Act

			var reader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption);
			reader.ReadIntoBusinessObject();
		}

		void AssertPkgPackageJobObjectReaderTest(string referenceNumber, string expectedPackType, int expectedPackQty)
		{
			var packageHeaders = Factory.Load<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.KPH_PackageID, referenceNumber));
			AssertEquals(string.Format("Expecting a package header with ID = {0}", referenceNumber), 1, packageHeaders.Length);

			var packageHeader = packageHeaders.Single();
			var packages = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_KPH_PackageHeader, packageHeader.PK));
			AssertEquals(string.Format("Expecting a package with package ID = {0}", referenceNumber), 1, packages.Length);

			var package = packages.Single();
			AssertEquals(string.Format("Expecting a package with {0} as quantity", expectedPackQty), expectedPackQty, package.KP_PackageQty);
			AssertEquals(string.Format("Expecting a package with mapped pack '{0}'", expectedPackType), expectedPackType, package.KP_F3_NKPackType);
		}

		#endregion

		#region TestReadIntoContainerAndPopulateContainerLinks_ContainerDOIsNull

		public void TestReadIntoContainerAndPopulateContainerLinks_ContainerDOIsNull()
		{
			Data.CreatePackingData();

			var reader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption);
			var containerReader = new PkgPackageContainerDataObjectReader(new Container(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.PackageJob.Packages);
			ArgumentNullExceptionAssertUtil.Assert("containerDO", () => reader.ReadIntoContainerAndPopulateContainerLinks(null, containerReader));
		}

		#endregion

		#region TestReadIntoContainerAndPopulateContainerLinks_ContainerReaderIsNull

		public void TestReadIntoContainerAndPopulateContainerLinks_ContainerReaderIsNull()
		{
			Data.CreatePackingData();

			var reader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption);
			ArgumentNullExceptionAssertUtil.Assert("reader", () => reader.ReadIntoContainerAndPopulateContainerLinks(new Container(DefaultDataObjectWriterStrategy.TestInstance), null));
		}

		#endregion

		#region TestImportWithSameLink

		public void TestImportContainersWithSameLink()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.ReferenceNumber = "VINABC";
			packageDataObject.ContainerLink = 1;

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.Link = 1;
			containerDataObject1.ContainerNumber = "VINABC1";
			containerDataObject1.FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.RollOnRollOff };

			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.Link = 1;
			containerDataObject2.ContainerNumber = "VINABC2";
			containerDataObject2.FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.RollOnRollOff };

			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject1, containerDataObject2 });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot add Container VINABC2, has duplicate link value 1.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportPackageWithSameLink()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var packageDataObject1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject1.ReferenceNumber = "VINABC1";
			packageDataObject1.Link = 1;

			var packageDataObject2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject2.ReferenceNumber = "VINABC2";
			packageDataObject2.Link = 1;

			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject1, packageDataObject2 });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot add Package VINABC2, has duplicate link value 1.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_ReaderClearsPackageActionStrategy

		public void TestPkgPackageJobObjectReader_ReaderClearsPackageActionStrategy()
		{
			Data.CreatePackingData();
			var dummy = Data.Dummy;
			var packageJob = Data.PackageJob;
			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packageJob))).GetDataObject(packageJob);
			var container = packageJob.Packages.AddNew("CNT");

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ container, new PackageActionStrategy(container, PackageAction.Delete, "Cannot delete.") }
			};
			var actionStrategyNone = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ container, new PackageActionStrategy(container) }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNone);

			AssertEquals("Precondition: Container is deletable", true, container.CanDelete);

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete, false);

			AssertEquals("Precondition: Container is deletable", true, packageJob.Packages.First().CanDelete);

			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, dummy, ImportOption);
			var result = reader.ReadIntoBusinessObject();

			AssertEquals("Container should exist", 1, result.Packages.Count);
			AssertEquals("Container should be undeletable", false, result.Packages.First().CanDelete);
		}

		#endregion

		#region TestROROCreatesAPackageWithTypeUnit

		public void TestROROCreatesAPackageWithTypeUnit()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.GoodsDescription = "VERYNICE";
			packageDataObject.HarmonisedCode = "HSCODE";
			packageDataObject.PackingLineID = "PackLine ID";
			packageDataObject.ReferenceNumber = "VINABC";
			packageDataObject.ContainerLink = 1;

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.Link = 1;
			containerDataObject.ContainerNumber = "VINABC";
			containerDataObject.FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.RollOnRollOff };
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			var package = packageJob.Packages.Single();
			AssertEquals("VINABC", package.KP_PackageID);
			AssertEquals(Constants.PkgUnit.Unit, package.KP_F3_NKPackType);
			AssertEquals(1, package.KP_PackageQty);
			AssertEquals("HSCODE", package.KP_HSCode);
			AssertEquals("PackLine ID", package.KP_ExternalReference);
			AssertEquals("VERYNICE", package.KP_GoodsDescription);
		}

		#endregion

		#region TestGetPackageTypeFromLinkedContainer

		public void TestGetPackageTypeFromLinkedContainer()
		{
			// roro splits it's data across the Container and Package universal entities
			var carContainer = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			carContainer.Link = 1;
			carContainer.ContainerNumber = "VINABC1";
			carContainer.FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.RollOnRollOff };

			var carPackage = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			carPackage.GoodsDescription = "Car";
			carPackage.ReferenceNumber = "VINABC1";
			carPackage.ContainerLink = 1;

			// break bulk also splits it's data across the Container and Package universal entities
			var massiveWheelContainer = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			massiveWheelContainer.Link = 2;
			massiveWheelContainer.ContainerNumber = "WHEEL1";
			massiveWheelContainer.FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.BreakBulk };

			var massiveWheelPackage = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			massiveWheelPackage.GoodsDescription = "Wheel";
			massiveWheelPackage.ReferenceNumber = "WHEEL1";
			massiveWheelPackage.ContainerLink = 2;

			// 2 packages with no links (1 without type)
			var can = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			can.GoodsDescription = "Can";

			var bottle = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			bottle.GoodsDescription = "Bottle";
			bottle.PackType = new PackageType { Code = Constants.PkgUnit.Bottle };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { carPackage, massiveWheelPackage, can, bottle });
			shipment.SetContainerCollection(() => new DataObjectList<Container> { carContainer, massiveWheelContainer });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(4, packageJob.Packages.Count);
			AssertEquals("Package Type is set in ContainerReader.", Constants.PkgUnit.Unit, packageJob.Packages.Single(p => p.KP_GoodsDescription == "Car").KP_F3_NKPackType);
			AssertEquals("Package Type is set in ContainerReader.", Constants.PkgUnit.BreakBulk, packageJob.Packages.Single(p => p.KP_GoodsDescription == "Wheel").KP_F3_NKPackType);
			AssertEquals("Does not have Container link to this line, but it has no PackType so should set as default 'PKG'.", Constants.PkgUnit.Package, packageJob.Packages.Single(p => p.KP_GoodsDescription == "Can").KP_F3_NKPackType);
			AssertEquals("Packing Line has type 'Bottle', so should keep it as Bottle.", Constants.PkgUnit.Bottle, packageJob.Packages.Single(p => p.KP_GoodsDescription == "Bottle").KP_F3_NKPackType);
		}

		#endregion

		#region TestCreationOfPackageFromJobCreatesContainersAsUnit

		public void TestCreationOfPackageFromJobCreatesContainersAsUnit()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.OuterPacks = 2;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Container };

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			var package = packageJob.Packages.Single();
			AssertEquals(Constants.PkgUnit.Unit, package.KP_F3_NKPackType);
			AssertEquals(2, package.KP_PackageQty);
		}

		#endregion

		#region TestNoPackageIsCreatedFromJobIfOuterPacksIsEmpty

		public void TestNoPackageIsCreatedFromJobIfOuterPacksIsEmpty()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.OuterPacks = null;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Container };

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(0, packageJob.Packages.Count);
		}

		#endregion

		#region TestPackingLinePackedViaContainerNumber

		public void TestPackingLinePackedViaContainerNumber()
		{
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.GoodsDescription = "My Pack";
			packageDataObject.PackQty = 1;
			packageDataObject.ContainerNumber = "CONT1234567";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONT1234567";
			containerDataObject.ContainerType = new ContainerType() { Code = "20GP" };

			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			var container = packageJob.Packages.Single();
			AssertEquals("CONT1234567", container.KP_PackageID);
			AssertEquals(true, container.IsContainer);

			var outer = container.Packages.Single();
			AssertEquals("My Pack", outer.KP_GoodsDescription);
			AssertEquals(false, outer.IsContainer);
		}

		#endregion

		#region TestPackingLineIsContainerPackedIntoContainer

		public void TestPackingLineIsContainerPackedIntoContainer()
		{
			Data.CreatePackingData();

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.ContainerNumber = "123";
			packingLine.PackType = new PackageType { Code = Constants.PkgUnit.Container };
			packingLine.PackQty = 1;
			packingLine.ReferenceNumber = "P999";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "123";
			containerDataObject.ContainerType = new ContainerType { Code = "20GP" };

			var company = Factory.New<GlbCompany>();
			company.SetCountry("US");

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(company);
			dataContext.AddDataSource(DataContextType.CustomsDeclaration, "ZZZ");

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			shipment.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			shipment.DataContext = dataContext;
			Logger.TopLevelDataObject = shipment;

			var reader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Package P999 must not have Pack Type 'CNT' as it is inside a container or package 123.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestTotalsPackageIsAddedIfAllLooseOutersHaveNoWeightsOrVolumesAndLooseOutersAreAddedAsInners

		public void TestTotalsPackageIsAddedIfAllLooseOutersHaveNoWeightsOrVolumesAndLooseOutersAreAddedAsInners()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.OuterPacks = 10;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Bottle };
			packageJobDataObject.TotalVolume = 22.3m;
			packageJobDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "CF" };
			packageJobDataObject.TotalWeight = 10.6m;
			packageJobDataObject.TotalWeightUnit = new UnitOfWeight { Code = "LB" };

			var packageDataObject1 = new PackingLine { PackQty = 10, PackType = new PackageType { Code = Constants.PkgUnit.Bottle }, ReferenceNumber = "" };
			var packageDataObject2 = new PackingLine { PackQty = 5, ReferenceNumber = "" };

			int containerLink = 1;
			var containerDataObject = new Container { ContainerNumber = "CONT123", Link = containerLink, ContainerType = new ContainerType { Code = "20GP" } };
			var containerPackageDataObject = new PackingLine { PackQty = 2, ReferenceNumber = "", ContainerLink = containerLink };

			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject1, packageDataObject2, containerPackageDataObject });

			var dummy = Factory.New<DummyWithPacking>();
			var reader1 = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob1 = reader1.ReadIntoBusinessObject();
			packageJob1.AssertPackageTree("No weights/volumes on any outer or first level packs on containers, but some loose are packed into the container and some not, so no way to group them under a Totals Package. ABC should be in the container.", @"
- 1 CNT 2280 KG (CONT123)
  - 2 PKG
- 5 PKG
- 10 BOT
");

			var dummy2 = Factory.New<DummyWithPacking>();
			packageJobDataObject.SetContainerCollection(() => null);
			packageJobDataObject.PackingLineCollection.Remove(containerPackageDataObject);
			var reader2 = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy2, ImportOption);
			var packageJob2 = reader2.ReadIntoBusinessObject();
			packageJob2.AssertPackageTree("Since no loose outers have any weights or volumes, an outer totals pack should be added and inners added to it.", @"
- 10 BOT 10.6 LB 22.3 CF
  - 5 PKG
  - 10 BOT
");

			var dummy3 = Factory.New<DummyWithPacking>();
			packageDataObject2.Weight = 5.3m;
			var reader3 = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy3, ImportOption);
			var packageJob3 = reader3.ReadIntoBusinessObject();
			packageJob3.AssertPackageTree("One of the loose outers has a weight, should read in packages as normal.", @"
- 5 PKG 5.3 KG
- 10 BOT
");

			var dummy4 = Factory.New<DummyWithPacking>();
			packageDataObject2.Weight = null;
			packageDataObject2.Volume = 7.8m;
			var reader4 = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy4, ImportOption);
			var packageJob4 = reader4.ReadIntoBusinessObject();
			packageJob4.AssertPackageTree("One of the loose outers has a volume, should read in packages as normal.", @"
- 5 PKG 7.8 M3
- 10 BOT
");

			var dummy5 = Factory.New<DummyWithPacking>();
			packageJobDataObject.PackingLineCollection.Remove(packageDataObject2);
			var reader5 = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy5, ImportOption);
			var packageJob5 = reader5.ReadIntoBusinessObject();
			packageJob5.AssertPackageTree("Since there was only one pack with the same type and quantity, add the totals to it, so there should be no inners.", "- 10 BOT 10.6 LB 22.3 CF");

			var dummy6 = Factory.New<DummyWithPacking>();
			packageDataObject1.PackType = null;
			var reader6 = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy6, ImportOption);
			var packageJob6 = reader6.ReadIntoBusinessObject();
			packageJob6.AssertPackageTree("Since there was only one pack with the same quantity, add the totals to it, so there should be no inners.", @"- 10 BOT 10.6 LB 22.3 CF
  - 10 PKG
				");
		}

		#endregion

		#region TestTotalsPackageIsAddedToTheContainerIfItIsTheOnlyPackage

		public void TestTotalsPackageIsAddedToTheContainerIfItIsTheOnlyPackage()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.OuterPacks = 10;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Bottle };
			packageJobDataObject.TotalVolume = 22.3m;
			packageJobDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "CF" };
			packageJobDataObject.TotalWeight = 10.6m;
			packageJobDataObject.TotalWeightUnit = new UnitOfWeight { Code = "LB" };

			int containerLink = 1;
			var containerDataObject = new Container { ContainerNumber = "CONT123", Link = containerLink, ContainerType = new ContainerType { Code = "20GP" } };
			var containerPackageDataObject = new PackingLine { PackQty = 10, ReferenceNumber = "", ContainerLink = containerLink };

			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { containerPackageDataObject });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			packageJob.AssertPackageTree("As there is only one loose package on the Job and it has the same quantity as the Job the totals will be added to the package inside the container. The container will be at the top level.", @"
- 1 CNT 2280 KG (CONT123)
  - 10 BOT 10.6 LB 22.3 CF
    - 10 PKG
");
		}

		#endregion

		#region TestQtyMustBeOneWhenPackageIDIsSet

		public void TestQtyMustBeOneWhenPackageIDIsSet()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.GoodsDescription = "GoodsDescription";
			packageDataObject.HarmonisedCode = "HarmonisedCode";
			packageDataObject.PackingLineID = "PackLineID";
			packageDataObject.ReferenceNumber = "VINABC";
			packageDataObject.PackQty = 25;
			packageDataObject.ContainerLink = 1;

			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			var package = packageJob.Packages.Single();
			AssertEquals("", package.KP_PackageID);
			AssertEquals(25, package.KP_PackageQty);
			AssertEquals("HarmonisedCode", package.KP_HSCode);
			AssertEquals("PackLineID", package.KP_ExternalReference);
			AssertEquals("GoodsDescription", package.KP_GoodsDescription);

			var dummy2 = Factory.New<DummyWithPacking>();
			packageDataObject.PackQty = 1;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy2, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			package = packageJob.Packages.Single();
			AssertEquals("VINABC", package.KP_PackageID);
		}

		#endregion

		#region TestDontCreateTotalPackageWhenJobOnlyHasPackQuantity

		public void TestDontCreateTotalPackageWhenJobOnlyHasPackQuantity()
		{
			var packageJobDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDO.OuterPacks = 0;
			packageJobDO.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Box };
			packageJobDO.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine { PackType = new PackageType { Code = Constants.PkgUnit.Carton } } });

			var dummy = Factory.New<DummyWithPacking>();
			var packageJob = new PkgPackageJobDataObjectReader(packageJobDO, Logger, Factory, dummy, ImportOption).ReadIntoBusinessObject();
			packageJob.AssertPackageTree("Should NOT create new packing as Loose Packages are specified. (null qty defaults as 1 - existing functionality)", "- 1 CTN");

			var dummy2 = Factory.New<DummyWithPacking>();
			packageJobDO.OuterPacks = 1;
			packageJob = new PkgPackageJobDataObjectReader(packageJobDO, Logger, Factory, dummy2, ImportOption).ReadIntoBusinessObject();
			packageJob.AssertPackageTree("Should still NOT create new top level packing.", "- 1 CTN");

			var dummy3 = Factory.New<DummyWithPacking>();
			packageJobDO.SetPackingLineCollection(() => new DataObjectList<PackingLine> { });
			packageJob = new PkgPackageJobDataObjectReader(packageJobDO, Logger, Factory, dummy3, ImportOption).ReadIntoBusinessObject();
			packageJob.AssertPackageTree("There are no Loose Packs, so use Job Totals.", "- 1 BOX");
		}

		#endregion

		#region TestTotalNoOfPackageIsUsedWhenOuterPackageIsEmpty

		public void TestTotalNoOfPackageIsUsedWhenOuterPackageIsEmpty()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.TotalWeight = 10m;
			packageJobDataObject.OuterPacks = 5;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Pallet };
			packageJobDataObject.TotalNoOfPacks = 10;
			packageJobDataObject.TotalNoOfPacksPackageType = new PackageType { Code = Constants.PkgUnit.Box };

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			var package = packageJob.Packages.Single();
			AssertEquals(Constants.PkgUnit.Pallet, package.KP_F3_NKPackType);
			AssertEquals(5, package.KP_PackageQty);

			var dummy2 = Factory.New<DummyWithPacking>();
			packageJobDataObject.OuterPacksPackageType = null;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy2, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			package = packageJob.Packages.Single();
			AssertEquals(Constants.PkgUnit.Box, package.KP_F3_NKPackType);
			AssertEquals(10, package.KP_PackageQty);

			var dummy3 = Factory.New<DummyWithPacking>();
			packageJobDataObject.OuterPacks = null;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Pallet };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy3, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			package = packageJob.Packages.Single();
			AssertEquals(Constants.PkgUnit.Box, package.KP_F3_NKPackType);
			AssertEquals(10, package.KP_PackageQty);

			var dummy4 = Factory.New<DummyWithPacking>();
			packageJobDataObject.TotalNoOfPacksPackageType = null;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy4, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(0, packageJob.Packages.Count);

			var dummy5 = Factory.New<DummyWithPacking>();
			packageJobDataObject.OuterPacks = 5;
			packageJobDataObject.OuterPacksPackageType = null;
			packageJobDataObject.TotalNoOfPacks = null;
			packageJobDataObject.TotalNoOfPacksPackageType = new PackageType { Code = Constants.PkgUnit.Box };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy5, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(0, packageJob.Packages.Count);

			var dummy6 = Factory.New<DummyWithPacking>();
			packageJobDataObject.OuterPacks = null;
			packageJobDataObject.OuterPacksPackageType = null;
			packageJobDataObject.TotalNoOfPacks = null;
			packageJobDataObject.TotalNoOfPieces = 5;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy6, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			package = packageJob.Packages.Single();
			AssertEquals(Constants.PkgUnit.Piece, package.KP_F3_NKPackType);
			AssertEquals(5, package.KP_PackageQty);
		}

		#endregion

		#region TestNoOfPackageIsDefaultToOneWhenPopulatePackageFromJob

		public void TestNoOfPackageIsDefaultToOneWhenPopulatePackageFromJob()
		{
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.OuterPacks = null;
			packageJobDataObject.OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Bottle };
			packageJobDataObject.TotalWeight = 100m;
			packageJobDataObject.TotalWeightUnit = new UnitOfWeight { Code = "KG" };

			var packageDataObject = new PackingLine { PackQty = 10, ReferenceNumber = "" };
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });

			var dummy = Factory.New<DummyWithPacking>();
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy, ImportOption);
			var packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(1, packageJob.Packages.Count);
			AssertEquals(1, packageJob.Packages[0].KP_PackageQty);

			var dummy2 = Factory.New<DummyWithPacking>();
			packageJobDataObject.OuterPacks = 0;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy2, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(1, packageJob.Packages.Count);
			AssertEquals(1, packageJob.Packages[0].KP_PackageQty);

			var dummy3 = Factory.New<DummyWithPacking>();
			packageJobDataObject.OuterPacks = 10;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, dummy3, ImportOption);
			packageJob = reader.ReadIntoBusinessObject();
			AssertEquals(1, packageJob.Packages.Count);
			AssertEquals(10, packageJob.Packages[0].KP_PackageQty);
		}

		#endregion

		#region TestPopulatePackageFromJobWhenDataObjectHasContainersButNoPackages

		public void TestPopulatePackageFromJob_WhenDataObjectHasContainersButNoPackagesAndParentIsDtbBookingConsolidation()
		{
			var parentJob = (IPackingParent)Factory.BOFactory.New<IDtbBookingConsolidation>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container = pkgPackageJob.Packages.AddNew(PkgUnit.Container, 1);
			container.KP_PackageID = "CNT1";
			container.Container.K0_RC_ContainerType = container20GP.PK;

			Factory.SaveForTesting();

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			pkgPackageJobDataObject.TotalNoOfPacks = 0;

			AssertEquals("Precondition: Parent BO should be a DtbBookingConsolidation.", DtbBookingConsolidationSchema.Constants.Prefix, parentJob.TablePrefix);

			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("No package should have been created.", 0, pkgJobRead.Packages.Count(p => p.KP_F3_NKPackType != "CNT"));
				AssertEquals("Should have a container.", 1, pkgJobRead.Containers.Count);
			});
		}

		public void TestPopulatePackageFromJob_WhenDataObjectHasContainersButNoPackagesAndParentIsNotDtbBookingConsolidation()
		{
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container = pkgPackageJob.Packages.AddNew(PkgUnit.Container, 1);
			container.KP_PackageID = "CNT1";
			container.Container.K0_RC_ContainerType = container20GP.PK;

			Factory.SaveForTesting();

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			pkgPackageJobDataObject.TotalNoOfPacks = 0;

			AssertNotEquals("Precondition: Parent BO should NOT be a DtbBookingConsolidation.", DtbBookingConsolidationSchema.Constants.Prefix, parentJob.TablePrefix);

			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("A package should have been created.", 1, pkgJobRead.Packages.Count(p => p.KP_F3_NKPackType != "CNT"));
				AssertEquals("Should have a container.", 1, pkgJobRead.Containers.Count);
			});
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_DimensionUQBlankThrowException

		public void TestPkgPackageJobObjectReader_KP_DimensionUQBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_DimensionUQ] CHECK ((KP_Length = 0 AND KP_Width = 0 AND KP_Height = 0) OR KP_DimensionUQ <> '');
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Length = 0m,
				Width = 0m,
				Height = 0m,
				LengthUnit = new UnitOfLength { Code = "" }
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packingLine.Length = 45m;
			var expectedErrorMessage = @"A dimension unit of measurement is required if a dimension value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Length: 45
Width: 0
Height: 0
Dimension Unit: ";

			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Length > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			packingLine.Length = 0m;
			packingLine.Width = 40m;
			expectedErrorMessage = @"A dimension unit of measurement is required if a dimension value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Length: 0
Width: 40
Height: 0
Dimension Unit: ";

			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Width > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			packingLine.Width = 0m;
			packingLine.Height = 40m;
			expectedErrorMessage = @"A dimension unit of measurement is required if a dimension value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Length: 0
Width: 0
Height: 40
Dimension Unit: ";

			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Height > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			packingLine.Height = 0m;
			packingLine.LengthUnit = new UnitOfLength { Code = "M" };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("All dimensions are zero and LengthUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_HeightLessThanZeroThrowException

		public void TestPkgPackageJobObjectReader_KP_HeightLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Height] CHECK (KP_Height >= 0);
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Height = 0m
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Height = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Height = 5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Height > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Height = -5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Height < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package height must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Height: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_LengthLessThanZeroThrowException

		public void TestPkgPackageJobObjectReader_KP_LengthLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Length] CHECK (KP_Length >= 0);
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Length = 0m
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Length = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Length = 5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Length > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Length = -5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Length < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package length must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Length: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_ReqTempMinGreaterThanKP_ReqTempMaxThrowException

		public void TestPkgPackageJobObjectReader_KP_ReqTempMinGreaterThanKP_ReqTempMaxThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_RequiredTemperatureMinimum_KP_RequiredTemperatureMaximum] CHECK (KP_RequiredTemperatureMaximum >= KP_RequiredTemperatureMinimum);
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				RequiredTemperatureMinimum = 0m,
				RequiredTemperatureMaximum = 0m
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Temps equal: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.RequiredTemperatureMaximum = 10m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Max temp > min temp: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.RequiredTemperatureMaximum = -5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("max temp < min temp: Exception should be thrown", typeof(DataObjectReadFailureException), @"Minimum temperature cannot be greater than maximum temperature.
ID: ABC
Pack Type: PKG
Quantity: 1
Required Min Temperature: 0
Required Max Temperature: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_VolumeLessThanZeroThrowException

		public void TestPkgPackageJobObjectReader_KP_VolumeLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Volume] CHECK (KP_Volume >= 0);
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				Volume = 0m,
				ReferenceNumber = "ABC"
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Volume = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = 5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Volume > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = -5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Volume < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package volume must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Volume: -5
Volume Unit: M3", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_VolumeUqBlankThrowException

		public void TestPkgPackageJobObjectReader_KP_VolumeUqBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_VolumeUQ] CHECK (KP_Volume = 0 OR KP_VolumeUQ <> '');
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Volume = 0m,
				VolumeUnit = new UnitOfVolume { Code = "" }
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = 45m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Volume > 0 and VolumeUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Volume Unit required if a package volume value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Volume: 45
Volume Unit: ", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = 0m;
			packingLine.VolumeUnit = new UnitOfVolume { Code = "M3" };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Volume is zero and VolumeUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_WeightLessThanZeroThrowException

		public void TestPkgPackageJobObjectReader_KP_WeightLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Weight] CHECK (KP_Weight >= 0);
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Weight = 0m
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Weight = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = 5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Weight > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = -5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Weight < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package weight must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Weight: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_WeightUqBlankThrowException

		public void TestPkgPackageJobObjectReader_KP_WeightUqBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_WeightUQ] CHECK (KP_Weight = 0 OR KP_WeightUQ <> '');
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Weight = 0m,
				WeightUnit = new UnitOfWeight { Code = "" }
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = 45m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Weight > 0 and WeightUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Weight Unit required if a package weight value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Weight: 45
Weight Unit: ", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = 0m;
			packingLine.WeightUnit = new UnitOfWeight { Code = "KG" };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Weight is zero and WeightUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_KP_WidthLessThanZeroThrowException

		public void TestPkgPackageJobObjectReader_KP_WidthLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Width] CHECK (KP_Width >= 0);
			Data.CreatePackingData();

			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Width = 0m
			};
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Width = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Width = 5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Width > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Width = -5m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Width < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package width must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Width: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_VolumeUqBlankThrowException

		public void TestPkgPackageJobObjectReader_VolumeUqBlankThrowException()
		{
			Data.CreatePackingData();

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "OOLU8936272",
				Link = 1,
				FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.RollOnRollOff }
			};
			var containerPackageDataObject = new PackingLine { PackQty = 8, ContainerNumber = "OOLU8936272", ReferenceNumber = "", ContainerLink = 1 };
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OuterPacks = 1,
				OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Bottle },
				AgentsReference = "ABC",
				TotalVolume = 0m,
				TotalVolumeUnit = new UnitOfVolume { Code = "" }
			};
			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { containerPackageDataObject });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packageJobDataObject.TotalVolume = 45m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Volume > 0 and VolumeUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Volume Unit required if a package volume value is entered.
ID: 
Pack Type: BOT
Quantity: 1
Volume: 45
Volume Unit: ", () => reader.ReadIntoBusinessObject());

			packageJobDataObject.TotalVolume = 0m;
			packageJobDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Volume is zero and VolumeUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobObjectReader_WeightUqBlankThrowException

		public void TestPkgPackageJobObjectReader_WeightUqBlankThrowException()
		{
			Data.CreatePackingData();

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "OOLU8936272",
				Link = 1,
				FCL_LCL_AIR = new ContainerMode { Code = Constants.ContainerModes.RollOnRollOff }
			};
			var containerPackageDataObject = new PackingLine { PackQty = 8, ContainerNumber = "OOLU8936272", ReferenceNumber = "", ContainerLink = 1 };
			var packageJobDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OuterPacks = 1,
				OuterPacksPackageType = new PackageType { Code = Constants.PkgUnit.Bottle },
				AgentsReference = "ABC",
				TotalWeight = 0m,
				TotalWeightUnit = new UnitOfWeight { Code = "" }
			};
			packageJobDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			packageJobDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { containerPackageDataObject });
			var reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packageJobDataObject.TotalWeight = 45m;
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertExceptionThrown("Weight > 0 and WeightUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Weight Unit required if a package weight value is entered.
ID: 
Pack Type: BOT
Quantity: 1
Weight: 45
Weight Unit: ", () => reader.ReadIntoBusinessObject());

			packageJobDataObject.TotalWeight = 0m;
			packageJobDataObject.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			reader = new PkgPackageJobDataObjectReader(packageJobDataObject, Logger, Factory, Data.Dummy, ImportOption);
			AssertNoExceptionThrown("Weight is zero and WeightUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageJobDataObjectReader_CreatePackageExtension

		public void TestPkgPackageJobDataObjectReader_CreatePackageExtension_NoContainerID()
		{
			TestPkgPackageJobDataObjectReader_CreatePackageExtension(containerID: "");
		}

		public void TestPkgPackageJobDataObjectReader_CreatePackageExtension_ContainerID()
		{
			TestPkgPackageJobDataObjectReader_CreatePackageExtension(containerID: "CNT1");
		}

		void TestPkgPackageJobDataObjectReader_CreatePackageExtension(string containerID)
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsPackageExtensions);
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			Factory.SaveForTesting();

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container = pkgPackageJob.Packages.AddNew(PkgUnit.Container, 1);
			container.KP_PackageID = containerID;
			container.Container.K0_RC_ContainerType = container20GP.PK;

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			AssertEquals(pkgPackageJob, pkgJobRead);
			var containerBO = pkgJobRead.Containers.Single();
			var extension = containerBO.PackageExtensions.Single();
			AssertEquals(parentJob.PK, extension.KPN_ParentID);
			AssertEquals(parentJob.TablePrefix, extension.KPN_ParentTableCode);
			AssertEquals(containerBO.PK, extension.KPN_KP_Package);

			// Read same package container again must not create a new extension.
			var readerForReRead = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var packageJobAfterReRead = readerForReRead.ReadIntoBusinessObject();

			AssertEquals(pkgPackageJob, packageJobAfterReRead);
			var containerBOAfterReRead = packageJobAfterReRead.Containers.Single();

			var extensionAfterReRead = containerBOAfterReRead.PackageExtensions.Single();
			if (ImportOption == ImportOption.PartialMatch)
			{
				AssertEquals("Extension must not changed if import option is partial match.", extension.PK, extensionAfterReRead.PK);
			}
			AssertEquals(parentJob.PK, extensionAfterReRead.KPN_ParentID);
			AssertEquals(parentJob.TablePrefix, extensionAfterReRead.KPN_ParentTableCode);
			AssertEquals(containerBOAfterReRead.PK, extensionAfterReRead.KPN_KP_Package);
		}

		public void TestPkgPackageJobDataObjectReader_CreatePackageExtension_DifferentPackageJobs()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsPackageExtensions);
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var differentParentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var differentPackageJob = PkgPackageJob.LoadOrCreatePackageJob(differentParentJob);
			Factory.SaveForTesting();
			AssertEquals("Precondition", 0, Factory.Load<PkgPackageExtension>(new ZQuery()).Length);

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container = pkgPackageJob.Packages.AddNew(PkgUnit.Container, 1);
			container.KP_PackageID = "CNT1";
			container.Container.K0_RC_ContainerType = container20GP.PK;

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			AssertEquals(pkgPackageJob, pkgJobRead);
			var containerBO = pkgJobRead.Containers.Single();
			var extensionForFirstPackageJob = containerBO.PackageExtensions.Single();
			AssertEquals(parentJob.PK, extensionForFirstPackageJob.KPN_ParentID);
			AssertEquals(parentJob.TablePrefix, extensionForFirstPackageJob.KPN_ParentTableCode);
			AssertEquals(containerBO.PK, extensionForFirstPackageJob.KPN_KP_Package);

			// Read same container into another Package Job.
			var readerForDifferentPackageJob = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, differentParentJob, ImportOption);
			var differentPackageJobAfterRead = readerForDifferentPackageJob.ReadIntoBusinessObject();

			AssertEquals(differentPackageJob, differentPackageJobAfterRead);
			var containerBOAfterReRead = differentPackageJobAfterRead.Containers.Single();
			var extensionAfterContainerReReadInDifferentPackageJob = containerBOAfterReRead.PackageExtensions.Single();

			AssertNotEquals("Extensions must be different.", extensionForFirstPackageJob.PK, extensionAfterContainerReReadInDifferentPackageJob.PK);
			AssertEquals(differentParentJob.PK, extensionAfterContainerReReadInDifferentPackageJob.KPN_ParentID);
			AssertEquals(differentParentJob.TablePrefix, extensionAfterContainerReReadInDifferentPackageJob.KPN_ParentTableCode);
			AssertEquals(containerBOAfterReRead.PK, extensionAfterContainerReReadInDifferentPackageJob.KPN_KP_Package);
		}

		public void TestPkgPackageJobDataObjectReader_CreatePackageExtension_SamePackageJobsWithMultipleExtensions()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsPackageExtensions);
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			var packageForDifferentExtension = Factory.New<PkgPackage>();

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var containerPackage = pkgPackageJob.Packages.AddNew(PkgUnit.Container, 1);
			containerPackage.KP_PackageID = "CNT1";
			containerPackage.Container.K0_RC_ContainerType = container20GP.PK;
			var extensionForSameParentTableCodeAndContainer = CreateExtension(parentJob.PK, containerPackage.PK, parentJob.TablePrefix);
			var extensionForDifferentParentTableCodeAndSameContainer = CreateExtension(parentJob.PK, containerPackage.PK, "KP");
			var extensionForDifferentParentTableCodeAndDifferentContainer = CreateExtension(parentJob.PK, packageForDifferentExtension.PK, "KP");

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			AssertEquals(pkgPackageJob, pkgJobRead);
			var containerBO = pkgJobRead.Containers.Single();

			if (ImportOption == ImportOption.PartialMatch)
			{
				AssertPackageExtension(extensionForSameParentTableCodeAndContainer, parentJob.PK, parentJob.TablePrefix, containerBO.PK);
				AssertPackageExtension(extensionForDifferentParentTableCodeAndSameContainer, parentJob.PK, "KP", containerBO.PK);
				AssertPackageExtension(extensionForDifferentParentTableCodeAndDifferentContainer, parentJob.PK, "KP", packageForDifferentExtension.PK);
			}
			else
			{
				Assert(extensionForSameParentTableCodeAndContainer.IsDeleted);
				Assert(extensionForDifferentParentTableCodeAndSameContainer.IsDeleted);
				Assert("Since extension is for different Container it should not be deleted.", !extensionForDifferentParentTableCodeAndDifferentContainer.IsDeleted);
			}
		}

		void AssertPackageExtension(PkgPackageExtension extension, ZGuid expectedParentJobPK, ZString expectedParentTableCode, ZGuid containerPK)
		{
			var matchedExtension = Factory.Load<PkgPackageExtension>(extension.PK);
			AssertEquals(expectedParentJobPK, matchedExtension.KPN_ParentID);
			AssertEquals(expectedParentTableCode, matchedExtension.KPN_ParentTableCode);
			AssertEquals(containerPK, matchedExtension.KPN_KP_Package);
		}

		PkgPackageExtension CreateExtension(ZGuid parentJobPK, ZGuid containerPackagePK, ZString parentTableCode)
		{
			var extensionForSameParentTableCodeAndContainer = Factory.New<PkgPackageExtension>();
			extensionForSameParentTableCodeAndContainer.KPN_ParentID = parentJobPK;
			extensionForSameParentTableCodeAndContainer.KPN_ParentTableCode = parentTableCode;
			extensionForSameParentTableCodeAndContainer.KPN_KP_Package = containerPackagePK;
			return extensionForSameParentTableCodeAndContainer;
		}

		#endregion

		public void TestPkgPackageJobDataObjectReader_Container_PackType_AIR()
		{
			TestPkgPackageJobDataObjectReader_Container_PackType(containerMode: "AIR");
		}

		public void TestPkgPackageJobDataObjectReader_Container_PackType_SEA()
		{
			TestPkgPackageJobDataObjectReader_Container_PackType(containerMode: "SEA");
		}

		public void TestPkgPackageJobDataObjectReader_Container_PackType_ROA()
		{
			TestPkgPackageJobDataObjectReader_Container_PackType(containerMode: "ROA");
		}

		void TestPkgPackageJobDataObjectReader_Container_PackType(string containerMode)
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsPackageExtensions);
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var differentParentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var differentPackageJob = PkgPackageJob.LoadOrCreatePackageJob(differentParentJob);
			Factory.SaveForTesting();
			AssertEquals("Precondition", 0, Factory.Load<PkgPackageExtension>(new ZQuery()).Length);

			var containerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, containerMode));
			var container = pkgPackageJob.Packages.AddNew(PkgUnit.Container, 1);
			container.KP_PackageID = "CNT1";
			container.Container.K0_RC_ContainerType = containerType.PK;

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var reader = new PkgPackageJobDataObjectReader(pkgPackageJobDataObject, Logger, Factory, parentJob, ImportOption);
			var pkgJobRead = reader.ReadIntoBusinessObject();

			AssertEquals(pkgPackageJob, pkgJobRead);
			var containerBO = pkgJobRead.Packages.Single();
			AssertEquals(PkgUnit.Container, containerBO.KP_F3_NKPackType);
		}

		protected virtual ImportOption ImportOption
		{
			get { return ImportOption.Default; }
		}

		#region Logger

		protected TestErrorLogger Logger
		{
			get { return logger ?? (logger = new TestErrorLogger()); }
		}

		TestErrorLogger logger;

		#endregion
	}

	public class PkgPackageJobDataObjectReaderForPartialMatchTest : PkgPackageJobDataObjectReaderTest
	{
		#region TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatch

		public void TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatch_CollectionContentIsComplete()
		{
			TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatchCore(CollectionContent.Complete);
		}

		public void TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatch_CollectionContentIsEmpty()
		{
			TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatchCore();
		}

		public void TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatch_CollectionContentIsPartial()
		{
			TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatchCore(CollectionContent.Partial);
		}

		void TestPkgPackageJobDataObjectReader_ImportOptions_PartialMatchCore(CollectionContent? content = null)
		{
			Data.CreatePackingData();
			var packages = Data.PackageJob.Packages;
			var package1 = packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = packages.AddNew(Constants.PkgUnit.Pallet, "XYZ");

			var packageData = new PackingLine { PackQty = 1, ReferenceNumber = "ABC", Weight = 150m, PackType = new PackageType() { Code = Constants.PkgUnit.Unit } };
			var packageParentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			packageParentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData })
			{
				Content = content
			});

			var reader = new PkgPackageJobDataObjectReader(packageParentData, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var packageJob = reader.ReadIntoBusinessObject();

			if (content == CollectionContent.Complete)
			{
				AssertEquals("Should use PackingLine to replace the packages in job", 1, packageJob.Packages.Count);
				var newPackage = packageJob.Packages.Single();
				AssertEquals("Should not create a new package.", package1.PK, newPackage.PK);
				AssertPackage(newPackage, "ABC", Constants.PkgUnit.Unit, 1, 150m);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder("Should *not* have deleted Packages that did not match.", new[] { package1, package2 }, packageJob.Packages);
				AssertEquals("Should have updated Package Weight.", 150m, package1.KP_Weight);
			}
		}

		void AssertPackage(PkgPackage package, string expectNumber, ZString packType, ZInt expectCount, ZDecimal expectWeight)
		{
			AssertEquals("Package Number", expectNumber, package.KP_PackageID);
			AssertEquals("Package Type", packType, package.KP_F3_NKPackType);
			AssertEquals("Package Qty", expectCount, package.KP_PackageQty);
			AssertEquals("Weight", expectWeight, package.KP_Weight);
		}

		#endregion

		#region TestContainerCollectionIsCompleteButPacklineLineCollectionIsPartial

		public void TestContainerCollectionIsCompleteButPacklineLineCollectionIsPartial()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew(Constants.PkgUnit.Container, "CON1");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;

			var package1 = container.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = container.Packages.AddNew(Constants.PkgUnit.Pallet, "XYZ");

			var containerData = new Container { ContainerCount = 1, ContainerNumber = "CON2", ContainerType = new ContainerType { Code = "20GP" }, Link = 0 };
			var packageData1 = new PackingLine { ReferenceNumber = "ABC", PackQty = 1 };
			var packageData2 = new PackingLine { ReferenceNumber = "XYZ", PackQty = 1, ContainerLink = 0 };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerData }) { Content = CollectionContent.Complete });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageData1, packageData2 }));

			var reader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var newJob = reader.ReadIntoBusinessObject();

			AssertEquals(2, newJob.Packages.Count);
			AssertEquals("CON1 should be removed", false, newJob.Packages.Any(p => p.KP_PackageID == "CON1"));
			AssertEquals("The Container1 should be deleted", true, container.IsDeleted);

			var container2 = newJob.Packages.FirstOrDefault(p => p.KP_PackageID == "CON2");
			AssertNotNull("CON2 should be created", container2);
			AssertNotEquals("Package1 should be re-created and in top level (not moved) to keep the solution simple.", package1.PK, newJob.Packages.Single(p => p.KP_PackageID == "ABC").PK);
			AssertNotEquals("Package2 should be re-created (not moved) to keep the solution simple.", package2.PK, container2.Packages.Single().PK);
			AssertEquals("The Package1 should be deleted", true, package1.IsDeleted);
			AssertEquals("The Package2 should be deleted", true, package2.IsDeleted);
		}

		#endregion

		protected override ImportOption ImportOption => ImportOption.PartialMatch;
	}
}
