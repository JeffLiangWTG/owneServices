using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	[TestedType(typeof(LoosePackageCollectionDataObjectReader))]
	public class LoosePackageCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		Func<PackingLine, PkgPackageDataObjectReader, PkgPackage> FuncForTest
		{
			get { return (packingLine, reader) => { return reader.ReadIntoBusinessObject(); }; }
		}

		#region TestConstructor

		public void TestConstructor_LoggerIsNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packageJobReader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			ArgumentNullExceptionAssertUtil.Assert("logger", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), null, Factory, packageJobReader, packageJob, FuncForTest));
		}

		public void TestConstructor_FactoryIsNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packageJobReader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			ArgumentNullExceptionAssertUtil.Assert("factory", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, null, packageJobReader, packageJob, FuncForTest));
		}

		public void TestConstructor_PackageJobReaderIsNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			ArgumentNullExceptionAssertUtil.Assert("packageJobReader", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, Factory, null, packageJob, FuncForTest));
		}

		public void TestConstructor_PackageJobIsNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packageJobReader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			ArgumentNullExceptionAssertUtil.Assert("packageJob", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, Factory, packageJobReader, packageJob: null, processFunc: FuncForTest));
		}

		public void TestConstructor_ProcessFuncIsNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packageJobReader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			ArgumentNullExceptionAssertUtil.Assert("processFunc", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, Factory, packageJobReader, packageJob, null));
		}

		public void TestConstructor_LoosePackageParentIsNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packageJobReader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			AssertNoExceptionThrown("Should not below up.", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, Factory, packageJobReader, packageJob, FuncForTest));
		}

		public void TestConstructor_LoosePackageParentIsNotNull()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var package = AddPackage(packageJob.Packages, "P1", 1, "PLT");
			var container = AddPackage(packageJob.Packages, "CON1", 1, "CNT");
			var packageJobReader = new PkgPackageJobDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, Data.Dummy, ImportOption.PartialMatch);

			AssertNoExceptionThrown("Should not below up.", () => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, Factory, packageJobReader, package, FuncForTest));

			AssertExceptionThrown(typeof(ArgumentException), "LoosePackageParent should be a Non-Container (Loose) Package.",
				() => new LoosePackageCollectionDataObjectReader(new DataObjectList<PackingLine>(), Logger, Factory, packageJobReader, container, FuncForTest));
		}

		#endregion

		#region TestReadIntoCollection

		public override void TestReadIntoCollection()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var pkgPackageJob = Data.PackageJob;
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Unit);
			var packageDO2 = AddPackageDataObject(5, "", Constants.PkgUnit.Pallet);

			AssertEquals("Precondition:", 0, pkgPackageJob.Packages.Count);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1, packageDO2 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, pkgPackageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(2, pkgPackageJob.Packages.Count);
			AssertPackage(pkgPackageJob.Packages.Single(c => c.KP_PackageID == "P1"), "P1", Constants.PkgUnit.Unit, 1);
			AssertPackage(pkgPackageJob.Packages.Single(c => c.KP_PackageID != "P1"), "", Constants.PkgUnit.Pallet, 5);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_Matches

		public void TestReadIntoCollection_Empty_Matches()
		{
			TestReadIntoCollection_MatchesCore();
		}

		public void TestReadIntoCollection_Partial_Matches()
		{
			TestReadIntoCollection_MatchesCore(CollectionContent.Partial);
		}

		public void TestReadIntoCollection_Complete_Matches()
		{
			TestReadIntoCollection_MatchesCore(CollectionContent.Complete);
		}

		void TestReadIntoCollection_MatchesCore(CollectionContent? content = null)
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Unit);
			var package2 = AddPackage(packageJob.Packages, "P2", 1, Constants.PkgUnit.Pallet, weightUQ: Constants.Weight.Grams);
			var package3 = AddPackage(packageJob.Packages, "", 1, Constants.PkgUnit.Box, weight: 10m);
			var package4 = AddPackage(packageJob.Packages, "", 1, Constants.PkgUnit.Box, volume: 10m);
			var package5 = AddPackage(packageJob.Packages, "", 5, Constants.PkgUnit.Unit, volume: 10m);

			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Package);
			var packageDO2 = AddPackageDataObject(1, "P2", Constants.PkgUnit.Pallet, weightUQ: Constants.Weight.Kilograms);
			var packageDO3 = AddPackageDataObject(1, "", Constants.PkgUnit.Box, weight: 20m);
			var packageDO4 = AddPackageDataObject(1, "", Constants.PkgUnit.Box, volume: 15m, volumeUQ: Constants.Volume.Litre);
			var packageDO5 = AddPackageDataObject(5, "", Constants.PkgUnit.Bag);

			AssertEquals("Precondition:", 5, packageJob.Packages.Count);

			var packageDOList = new DataObjectList<PackingLine>(new[] { packageDO1, packageDO2, packageDO3, packageDO4, packageDO5 });
			packageDOList.Content = content;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => packageDOList);
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(packageDOList, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			var expectedCount = content == CollectionContent.Complete ? 5 : 6;
			AssertEquals(expectedCount, packageJob.Packages.Count);

			PkgPackage[] existingPackges;

			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);

			if (content != CollectionContent.Complete)
			{
				existingPackges = new PkgPackage[] { package1, package2, package3, package4, package5 };
				AssertPackage(packageJob.Packages.Single(c => c.PK == package1.PK), "P1", Constants.PkgUnit.Package, 1);
				AssertPackage(packageJob.Packages.Single(c => c.PK == package2.PK), "P2", Constants.PkgUnit.Pallet, 1);
				AssertPackage(packageJob.Packages.Single(c => c.PK == package3.PK), "", Constants.PkgUnit.Box, 1, expectWeight: 20m);
				AssertPackage(packageJob.Packages.Single(c => c.PK == package4.PK), "", Constants.PkgUnit.Box, 1, expectVolume: 15m, expectVolumeUQ: Constants.Volume.Litre);
				AssertPackage(packageJob.Packages.Single(c => c.PK == package5.PK), "", Constants.PkgUnit.Unit, 5, expectVolume: 10m);
				AssertPackage(packageJob.Packages.Except(existingPackges).Single(), "", Constants.PkgUnit.Bag, 5);
			}
			else
			{
				AssertPackage(packageJob.Packages.Single(p => p.KP_PackageID == "P1"), "P1", Constants.PkgUnit.Package, 1);
				AssertPackage(packageJob.Packages.Single(p => p.KP_PackageID == "P2"), "P2", Constants.PkgUnit.Pallet, 1);
				AssertPackage(packageJob.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box && p.KP_Weight == 20m), "", Constants.PkgUnit.Box, 1, expectWeight: 20m);
				AssertPackage(packageJob.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box && p.KP_Volume == 15m), "", Constants.PkgUnit.Box, 1, expectVolume: 15m, expectVolumeUQ: Constants.Volume.Litre);
				AssertPackage(packageJob.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Bag), "", Constants.PkgUnit.Bag, 5);
			}
		}

		#endregion

		#region TestReadIntoCollection_ParentPackageIsContainer

		public void TestReadIntoCollection_ParentPackageIsContainer()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			packageDO1.ContainerLink = 0;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(1, packageJob.Packages.Count);
			AssertEquals("Should not update existing container", container, packageJob.Packages.Single());
			AssertEquals(1, container.Packages.Count);
			AssertPackage(container.Packages.Single(), "P1", Constants.PkgUnit.Pallet, 1);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_ParentPackageIsContainer_HasPackageID_PackageIsExisted

		public void TestReadIntoCollection_ParentPackageIsContainer_HasPackageID_PackageIsExisted()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container.Packages, "P1", 1, Constants.PkgUnit.Pallet);

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Box);
			packageDO1.ContainerLink = 0;
			var packageDO2 = AddPackageDataObject(1, "P2", Constants.PkgUnit.Pallet, 10m);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1, packageDO2 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(2, packageJob.Packages.Count);
			AssertEquals("Should not update existing container", container, packageJob.Packages.Single(p => p.KP_PackageID == "CON1"));
			var newTopLevelPallet = packageJob.Packages.Single(p => p.KP_PackageID != "CON1");
			AssertPackage(newTopLevelPallet, "P2", Constants.PkgUnit.Pallet, 1, 10m);

			AssertEquals(1, container.Packages.Count);
			AssertPackage(container.Packages.Single(), "P1", Constants.PkgUnit.Box, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_ParentPackageIsContainer_HasNoPackageID_PackageIsExisted

		public void TestReadIntoCollection_ParentPackageIsContainer_HasNoPackageID_PackageIsExisted()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container.Packages, "", 1, Constants.PkgUnit.Pallet);

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Pallet);
			packageDO1.ContainerLink = 0;
			var packageDO2 = AddPackageDataObject(1, "", Constants.PkgUnit.Pallet, 10m);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1, packageDO2 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(2, packageJob.Packages.Count);
			AssertEquals("Should not update existing container", container, packageJob.Packages.Single(p => p.KP_PackageID == "CON1"));
			var newTopLevelPallet = packageJob.Packages.Single(p => p.KP_PackageID != "CON1");
			AssertPackage(newTopLevelPallet, "", Constants.PkgUnit.Pallet, 1, 10m);

			AssertEquals(1, container.Packages.Count);
			AssertPackage(container.Packages.Single(), "", Constants.PkgUnit.Pallet, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_InnerPackingCollectionIsNull

		public void TestReadIntoCollection_CollectionContentIsPartial_InnerPackingCollectionIsNull()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Pallet, 5m);
			var innerPackage = AddPackage(package1.Packages, "B1", 1, Constants.PkgUnit.Box, 5m);

			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			AssertNull("Precondition: Not set PackingLineCollection", packageDO1.PackingLineCollection);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(1, packageJob.Packages.Count);
			AssertPackage(packageJob.Packages.Single(), "P1", Constants.PkgUnit.Pallet, 1);
			AssertEquals("The existing package should no changes", innerPackage, package1.Packages.Single());
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_InnerPackingCollectionIsEmpty

		public void TestReadIntoCollection_CollectionContentIsPartial_InnerPackingCollectionIsEmpty()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Pallet, 0m);
			var innerPackage = AddPackage(package1.Packages, "B1", 1, Constants.PkgUnit.Box, 0m);

			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			packageDO1.SetPackingLineCollection(() => new List<PackingLine>());

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(1, packageJob.Packages.Count);
			var package = packageJob.Packages.Single();
			AssertPackage(package, "P1", Constants.PkgUnit.Pallet, 1);
			AssertEquals("The package2 should be removed", 0, package.Packages.Count);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_AddPackageInChildLevel_HasPackageNumber

		public void TestReadIntoCollection_CollectionContentIsPartial_AddPackageInChildLevel_HasPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Pallet, 0m);
			var package2 = AddPackage(package1.Packages, "B1", 1, Constants.PkgUnit.Box, 0m);

			var packageDO3 = AddPackageDataObject(1, "B2", Constants.PkgUnit.Box);
			var packageDO2 = AddPackageDataObject(1, "B1", Constants.PkgUnit.Box);
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			packageDO1.SetPackingLineCollection(() => new List<PackingLine>() { packageDO2 });
			packageDO2.SetPackingLineCollection(() => new List<PackingLine>() { packageDO3 });

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(1, packageJob.Packages.Count);
			AssertPackage(packageJob.Packages.Single(), "P1", Constants.PkgUnit.Pallet, 1);
			var package2AfterImport = package1.Packages.Single();
			AssertEquals("Should not recreate package", package2AfterImport.PK, package2.PK);
			AssertPackage(package2AfterImport, "B1", Constants.PkgUnit.Box, 1);
			AssertEquals(1, package2AfterImport.Packages.Count);
			AssertPackage(package2AfterImport.Packages.Single(), "B2", Constants.PkgUnit.Box, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_AddPackageInChildLevel_HasNoPackageNumber

		public void TestReadIntoCollection_CollectionContentIsPartial_AddPackageInChildLevel_HasNoPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "", 1, Constants.PkgUnit.Pallet, 0m);
			var package2 = AddPackage(package1.Packages, "", 1, Constants.PkgUnit.Box, 0m);

			var packageDO3 = AddPackageDataObject(1, "", Constants.PkgUnit.Box);
			var packageDO2 = AddPackageDataObject(1, "", Constants.PkgUnit.Box);
			var packageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Pallet);
			packageDO1.SetPackingLineCollection(() => new List<PackingLine>() { packageDO2 });
			packageDO2.SetPackingLineCollection(() => new List<PackingLine>() { packageDO3 });

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(1, packageJob.Packages.Count);
			AssertPackage(packageJob.Packages.Single(), "", Constants.PkgUnit.Pallet, 1);
			AssertEquals(1, package1.Packages.Count);
			var package2AfterImport = package1.Packages.Single();
			AssertEquals("Should not recreate package", package2AfterImport.PK, package2.PK);
			AssertPackage(package2AfterImport, "", Constants.PkgUnit.Box, 1);
			AssertEquals(1, package2AfterImport.Packages.Count);
			AssertPackage(package2AfterImport.Packages.Single(), "", Constants.PkgUnit.Box, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_HasPackageNumber

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_HasPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Pallet, 0m);
			var package2 = AddPackage(package1.Packages, "B1", 1, Constants.PkgUnit.Box, 0m);
			var package3 = AddPackage(package1.Packages, "B2", 1, Constants.PkgUnit.Box, 0m);

			var packageDO3 = AddPackageDataObject(1, "B2", Constants.PkgUnit.Box);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO3 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(2, packageJob.Packages.Count);
			AssertPackage(packageJob.Packages.Single(c => c.PK == package1.PK), "P1", Constants.PkgUnit.Pallet, 1, 0m);
			AssertPackage(packageJob.Packages.Single(c => c.PK == package3.PK), "B2", Constants.PkgUnit.Box, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_HasNoPackageNumber

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_HasNoPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "", 1, Constants.PkgUnit.Pallet, 0m);
			var package2 = AddPackage(package1.Packages, "", 1, Constants.PkgUnit.Box, 0m);
			var package3 = AddPackage(package1.Packages, "", 1, Constants.PkgUnit.Box, 0m);

			var packageDO3 = AddPackageDataObject(1, "", Constants.PkgUnit.Box);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO3 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(2, packageJob.Packages.Count);
			AssertPackage(packageJob.Packages.Single(c => c.PK == package1.PK), "", Constants.PkgUnit.Pallet, 1, 0m);
			var newPackage = packageJob.Packages.Single(c => c.PK != package1.PK);
			AssertPackage(newPackage, "", Constants.PkgUnit.Box, 1);

			AssertEquals(2, package1.Packages.Count);
			AssertContainsExactElementsInAnyOrder("Should not change the child packages in Package1", new PkgPackage[] { package2, package3 }, package1.Packages);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromTopToContainer

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromTopToContainer_HasPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Pallet);

			AssertEquals("Precondition", 2, packageJob.Packages.Count);

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			packageDO1.ContainerLink = 0;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDO }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals("Package1 have been moved into Container1", 1, packageJob.Packages.Count);
			var container1_AfterImport = packageJob.Packages.Single();
			AssertPackage(container1_AfterImport.Packages.Single(p => p.PK == package1.PK), "P1", Constants.PkgUnit.Pallet, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromTopToContainer_HasNoPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(packageJob.Packages, "", 1, Constants.PkgUnit.Package);

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Package);
			packageDO1.ContainerLink = 0;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDO }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals("Package1 should still in Top Level", true, packageJob.Packages.Any(p => p.PK == package1.PK));
			var container1_AfterImport = packageJob.Packages.Single(p => p.PK == container.PK);
			AssertPackage(container1_AfterImport.Packages.Single(), "", Constants.PkgUnit.Package, 1);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromContainerToContainer

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromContainerToContainer_HasPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			container1.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var container2 = packageJob.Packages.AddNew("CNT", "CON2");
			container2.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container1.Packages, "P1", 1, Constants.PkgUnit.Pallet);

			AssertEquals("Precondition", 2, packageJob.Packages.Count);
			var container1_BeforeImport = packageJob.Packages.Single(c => c.PK == container1.PK);
			AssertPackage(container1_BeforeImport.Packages.Single(p => p.PK == package1.PK), "P1", Constants.PkgUnit.Pallet, 1);
			var container2_BeforeImport = packageJob.Packages.Single(c => c.PK == container2.PK);
			AssertEquals("Precondition", 0, container2_BeforeImport.Packages.Count);

			var containerDO2 = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON2",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			packageDO1.ContainerLink = 0;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDO2 }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container2);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals("Package count has not changed", 2, packageJob.Packages.Count);
			var container1_AfterImport = packageJob.Packages.Single(c => c.PK == container1.PK);
			AssertEquals("Package1 have been moved from Container1 to Container2", 0, container1_AfterImport.Packages.Count);
			var container2_AfterImport = packageJob.Packages.Single(c => c.PK == container2.PK);
			AssertPackage(container2_AfterImport.Packages.Single(p => p.PK == package1.PK), "P1", Constants.PkgUnit.Pallet, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromContainerToContainer_HasNoPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			container1.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var container2 = packageJob.Packages.AddNew("CNT", "CON2");
			container2.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container1.Packages, "", 1, Constants.PkgUnit.Package);

			var containerDO2 = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON2",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Package);
			packageDO1.ContainerLink = 0;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDO2 }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container2);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals("Top Level should havs no changes", 2, packageJob.Packages.Count);
			var container1_AfterImport = packageJob.Packages.Single(c => c.PK == container1.PK);
			AssertEquals("Package1 should still in Container1", true, container1_AfterImport.Packages.Any(p => p.PK == package1.PK));
			var container2_AfterImport = packageJob.Packages.Single(c => c.PK == container2.PK);
			AssertPackage(container2_AfterImport.Packages.Single(p => p.PK != package1.PK), "", Constants.PkgUnit.Package, 1);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromContainerToTop

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromContainerToTop_HasPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			container1.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container1.Packages, "P1", 1, Constants.PkgUnit.Pallet);

			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals("Package1 have been moved into Top Level", 2, packageJob.Packages.Count);
			AssertEquals("Package1 have been moved into Top Level", true, packageJob.Packages.Any(p => p.PK == package1.PK));
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		public void TestReadIntoCollection_CollectionContentIsPartial_LevelChanged_FromContainerToTop_HasNoPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			container1.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container1.Packages, "", 1, Constants.PkgUnit.Package);

			var packageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Package);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals("New package has been created", 2, packageJob.Packages.Count);
			var container1_AfterImport = packageJob.Packages.Single(p => p.PK == container1.PK);
			var newPackage = packageJob.Packages.Single(p => p.PK != container1.PK);
			AssertNotEquals("New package has been created", newPackage.PK, package1.PK);
			AssertPackage(newPackage, "", Constants.PkgUnit.Package, 1);
			AssertNotNull("Package1 should still in Container1", container1_AfterImport.Packages.Any(p => p.PK == package1.PK));
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CollectionContentIsPartial_ShouleNotMoveWhenPackageInLoosePackage

		public void TestReadIntoCollection_CollectionContentIsPartial_ShouleNotMoveWhenPackageInLoosePackage_HasPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container.Packages, "P1", 1, Constants.PkgUnit.Pallet);
			var innerPackage1 = AddPackage(package1.Packages, "B1", 1, Constants.PkgUnit.Box);

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Pallet);
			packageDO1.ContainerLink = 0;
			var innerPackageDO1 = AddPackageDataObject(1, "B1", Constants.PkgUnit.Package);
			innerPackageDO1.ContainerLink = 0;

			packageDO1.SetPackingLineCollection(() => new List<PackingLine>() { innerPackageDO1 });

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDO }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			var container_AfterImport = packageJob.Packages.Single();
			AssertEquals("InnerPackage should not move to Container Level", 1, container_AfterImport.Packages.Count);
			var packages1_AfterImport = container_AfterImport.Packages.Single(p => p.PK == package1.PK);
			AssertPackage(packages1_AfterImport.Packages.Single(p => p.PK == innerPackage1.PK), "B1", Constants.PkgUnit.Package, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		public void TestReadIntoCollection_CollectionContentIsPartial_ShouleNotMoveWhenPackageInLoosePackage_HasNoPackageNumber()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var packageJob = Data.PackageJob;
			var container = packageJob.Packages.AddNew("CNT", "CON1");
			container.Container.K0_RC_ContainerType = data.Container20GP.PK;
			var package1 = AddPackage(container.Packages, "", 1, Constants.PkgUnit.Pallet);
			var innerPackage1 = AddPackage(package1.Packages, "", 1, Constants.PkgUnit.Box);

			var containerDO = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CON1",
				Link = 0
			};
			var packageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Pallet);
			packageDO1.ContainerLink = 0;
			var innerPackageDO1 = AddPackageDataObject(1, "", Constants.PkgUnit.Box);
			innerPackageDO1.ContainerLink = 0;

			packageDO1.SetPackingLineCollection(() => new List<PackingLine>() { innerPackageDO1 });

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDO }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO1 }));
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			packageJobReader.PackageContainerLinks.Add(0, container);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			var container_AfterImport = packageJob.Packages.Single();
			AssertEquals("InnerPackage should not move to Container Level", 1, container_AfterImport.Packages.Count);
			var packages1_AfterImport = container_AfterImport.Packages.Single(p => p.PK == package1.PK);
			var newInnerPackage = packages1_AfterImport.Packages.Single();
			AssertNotEquals("InnerPackage has been replaced", package1.PK, newInnerPackage);
			AssertPackage(newInnerPackage, "", Constants.PkgUnit.Box, 1);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CannotDeleteWhenIsUsed

		public void TestReadIntoCollection_CannotDeleteWhenIsUsed()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Pallet);
			var package3 = AddPackage(packageJob.Packages, "P3", 1, Constants.PkgUnit.Pallet);

			var actionStrategyNoDelete = new Dictionary<PkgPackage, PackageActionStrategy>
			{
				{ package1, new PackageActionStrategy(package1, PackageAction.Delete, "Cannot delete.") }
			};

			Data.Dummy.SetPackageActionResponses(actionStrategyNoDelete, true);

			var packageDO2 = AddPackageDataObject(1, "P2", Constants.PkgUnit.Pallet);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packageDO2 }) { Content = CollectionContent.Complete });
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, packageJob, FuncForTest);
			reader.ReadIntoCollection();

			AssertEquals(2, packageJob.Packages.Count);
			AssertEquals("Existing package P1 should not be deleted", package1, packageJob.Packages.Single(p => p.KP_PackageID == "P1"));
			AssertPackage(packageJob.Packages.Single(p => p.KP_PackageID != "P1"), "P2", Constants.PkgUnit.Pallet, 1);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_ProcessFunc

		public void TestReadIntoCollection_ProcessFunc()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = AddPackage(packageJob.Packages, "P1", 1, Constants.PkgUnit.Unit);

			var packageDO1 = AddPackageDataObject(1, "P1", Constants.PkgUnit.Package);
			packageDO1.Link = 1;
			var packageDO2 = AddPackageDataObject(1, "P2", Constants.PkgUnit.Package);
			packageDO2.Link = 2;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packageDO1, packageDO2 });
			var packageJobReader = new PkgPackageJobDataObjectReader(shipment, Logger, Factory, Data.Dummy, ImportOption.PartialMatch);
			var reader = new LoosePackageCollectionDataObjectReader(shipment.PackingLineCollection, Logger, Factory, packageJobReader, Data.PackageJob, (packingLine, packageReader) =>
			{
				AssertEquals("PackingLine should must in the list", true, shipment.PackingLineCollection.Any(p => p == packingLine));
				AssertNotNull("PackageReader should not be null", packageReader);

				var package = packageReader.ReadIntoBusinessObject();
				AssertNotNull(package);

				return package;
			});
			reader.ReadIntoCollection();
		}

		#endregion

		#region Implementation

		PkgPackage AddPackage(PkgPackageCollection packages, ZString packageNumber, ZInt packageQty, ZString packType, decimal weight = 5m, string weightUQ = Constants.Weight.Kilograms, decimal volume = 5m, string volumeUQ = Constants.Volume.CubicMetres)
		{
			var package = packages.AddNew(packType, packageQty);
			package.KP_PackageID = packageNumber;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;

			return package;
		}

		PackingLine AddPackageDataObject(ZLong count, ZString packageNumber, ZString packType, decimal weight = 5m, string weightUQ = Constants.Weight.Kilograms, decimal volume = 5m, string volumeUQ = Constants.Volume.CubicMetres)
		{
			var package = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);

			package.PackQty = count;
			package.ReferenceNumber = packageNumber;
			package.PackType = new PackageType { Code = packType };
			package.Weight = weight;
			package.WeightUnit = new UnitOfWeight { Code = weightUQ };
			package.Volume = volume;
			package.VolumeUnit = new UnitOfVolume { Code = volumeUQ };

			return package;
		}

		void AssertPackage(PkgPackage package, string expectNumber, ZString packType, ZInt expectCount, decimal expectWeight = 5m, string expectWeightUQ = Constants.Weight.Kilograms, decimal expectVolume = 5m, string expectVolumeUQ = Constants.Volume.CubicMetres)
		{
			AssertEquals("Package Number", expectNumber, package.KP_PackageID);
			AssertEquals("Package Type", packType, package.KP_F3_NKPackType);
			AssertEquals("Package Qty", expectCount, package.KP_PackageQty);
			AssertEquals("Weight", expectWeight, package.KP_Weight);
			AssertEquals("WeightUQ", expectWeightUQ, package.KP_WeightUQ);
			AssertEquals("Volume", expectVolume, package.KP_Volume);
			AssertEquals("VolumeUQ", expectVolumeUQ, package.KP_VolumeUQ);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory.BOFactory));
		TestDataForPacking data;

		protected NotifyForPacking Notify => notify ?? (notify = new NotifyForPacking());
		NotifyForPacking notify;

		protected PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory.BOFactory));
		PackingTestHelper helper;

		#region Logger

		protected TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());
		TestErrorLogger logger;

		#endregion

		#endregion
	}
}
