using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	class PkgPackageJobDataObjectWriterHelperTest : PackingTestCaseWithFactory
	{
		#region TestPopulatePkgPackages_From_PackageCollection

		public void TestPopulatePkgPackages_From_Collection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(new string[] { "BOX", "PKG" }, testShipment.PackingLineCollection.Select(p => p.PackType.Code.Value));
		}

		public void TestPopulatePkgPackages_From_Collection_AppendsToExistingCollection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper.PopulateDataObject(testShipment);

			Data.PackageJob.Packages.RemoveAllFromRelationship();
			var package3 = Data.PackageJob.Packages.AddNew("PLT");
			var helper2 = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper2.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(new string[] { "BOX", "PKG", "PLT" }, testShipment.PackingLineCollection.Select(p => p.PackType.Code.Value));
		}

		public void TestPopulateMultiLevelHandlingUnits_From_Collection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var topHandlingUnit = Data.PackageJob.Packages.AddNew("PLT", "TopHU");
			var subHandlingUnit = Data.PackageJob.Packages.AddNew("PLT", "SubHU");
			var topHandlingUnitInner = Data.PackageJob.Packages.AddNew("BOX", "TopHU_Inner");
			var subHandlingUnitInner = Data.PackageJob.Packages.AddNew("PKG", "SubHU_Inner");
			var standAlonePackage = Data.PackageJob.Packages.AddNew("CTN", "StandAlone");

			Helper.CreatePackageHandlingUnitDivot(topHandlingUnit, subHandlingUnit);
			Helper.CreatePackageHandlingUnitDivot(topHandlingUnit, topHandlingUnitInner);
			Helper.CreatePackageHandlingUnitDivot(subHandlingUnit, subHandlingUnitInner);

			var packagesForWriter = new List<PkgPackage>();
			packagesForWriter.Add(topHandlingUnit);
			packagesForWriter.Add(standAlonePackage);

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter);
			helper.PopulateDataObject(testShipment);

			CombineAssertions(() =>
			{
				var topLevelPackingLines = testShipment.PackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "TopHU", "StandAlone" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));

				AssertNull(topLevelPackingLines.Single(p => p.ReferenceNumber.Value == "StandAlone").PackingLineCollection);

				var topHUPackingLine = topLevelPackingLines.Single(p => p.ReferenceNumber.Value == "TopHU");
				AssertContainsExactElementsInAnyOrder(new string[] { "TopHU_Inner", "SubHU" }, topHUPackingLine.PackingLineCollection.Select(p => p.ReferenceNumber.Value));

				AssertNull(topHUPackingLine.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "TopHU_Inner").PackingLineCollection);

				var subHandlingUnitPackingLine = topHUPackingLine.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "SubHU");
				AssertContainsExactElementsInAnyOrder(new string[] { "SubHU_Inner" }, subHandlingUnitPackingLine.PackingLineCollection.Select(p => p.ReferenceNumber.Value));

				AssertNull(subHandlingUnitPackingLine.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "SubHU_Inner").PackingLineCollection);
			});
		}

		public void TestPopulateMultiLevelHandlingUnits_From_Collection_AppendsToExistingCollection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var standAlonePackage = Data.PackageJob.Packages.AddNew("CTN", "StandAlone");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper.PopulateDataObject(testShipment);

			var topLevelPackingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "StandAlone" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));

			Data.PackageJob.Packages.RemoveAllFromRelationship();
			var topHandlingUnit = Data.PackageJob.Packages.AddNew("PLT", "TopHU");
			var topHandlingUnitInner = Data.PackageJob.Packages.AddNew("BOX", "TopHU_Inner");
			Helper.CreatePackageHandlingUnitDivot(topHandlingUnit, topHandlingUnitInner);

			var helper2 = new PkgPackageJobDataObjectWriterHelper(writeManager, new List<PkgPackage>() { topHandlingUnit });
			helper2.PopulateDataObject(testShipment);

			CombineAssertions(() =>
			{
				topLevelPackingLines = testShipment.PackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "TopHU", "StandAlone" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));

				AssertNull(topLevelPackingLines.Single(p => p.ReferenceNumber.Value == "StandAlone").PackingLineCollection);

				var topHUPackingLine = topLevelPackingLines.Single(p => p.ReferenceNumber.Value == "TopHU");
				AssertContainsExactElementsInAnyOrder(new string[] { "TopHU_Inner" }, topHUPackingLine.PackingLineCollection.Select(p => p.ReferenceNumber.Value));

				AssertNull(topHUPackingLine.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "TopHU_Inner").PackingLineCollection);
			});
		}

		public void TestPopulateDataObject_OutturnDamagedReason()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");

			package1.KP_DamagedReason = "LOS";
			package2.KP_DamagedReason = "WT";

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(new string[] { "LOS", "WT" }, testShipment.PackingLineCollection.Select(p => p.OutturnDamagedReason.Code.Value));
		}

		public void TestPopulateDataObject_PackageJobWithoutOutturn_SetsDefaultOutturn()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var parentJob = Factory.New<DummyWithPacking>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = parentJob.PK;
			packageJob.KJ_ParentTableCode = parentJob.TablePrefix;

			var package1 = packageJob.Packages.AddNew("BOX");
			package1.KP_TransportRef = "DUM";
			package1.KP_PackageID = "PKG1";
			package1.KP_PreviousPackageID = "PKG1";
			var package2 = packageJob.Packages.AddNew("PKG");
			package2.KP_PackageID = "PKG2";
			package2.KP_PreviousPackageID = "PKG2_Prev";
			var container = packageJob.Containers.AddNew();
			container.KP_PackageID = "CNT1";
			var inner = packageJob.Packages.AddNew("PLT");
			inner.KP_KP_ParentPackage = container.PK;
			inner.KP_PackageID = "PKG3";

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, parentJob, packageJob);
			helper.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(new string[] { "PKG1", "PKG2", "PKG3" }, testShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnQty));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnDamagedQty));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnPillagedQty));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnedHeight));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnedLength));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnedVolume));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnedWeight));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.OutturnedWidth));
			AssertContainsExactElementsInAnyOrder(new ZString?[] { "DUM", string.Empty, string.Empty }, testShipment.PackingLineCollection.Select(p => p.TransportReference));

			AssertEquals("Should write previous Package Id to AddInfoCollection if not empty.",
				"PKG1", testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.PreviousPackageID)).Value);
			AssertEquals("Should write previous Package Id to AddInfoCollection if not empty.",
				"PKG2_Prev", testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.PreviousPackageID)).Value);
			AssertNull("Should not write previous Package Id to AddInfoCollection if empty.",
				testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3").AddInfoCollection);
		}

		public void TestPopulateDataObject_AddInfoCollection_ManifestedPackage()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var parentJob = Factory.New<DummyWithPacking>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = parentJob.PK;
			packageJob.KJ_ParentTableCode = parentJob.TablePrefix;

			var package1 = packageJob.Packages.AddNew("BOX");
			package1.KP_PackageID = "PKG1";
			package1.BookedDimensions.KPB_PackageQty = 1;
			var package2 = packageJob.Packages.AddNew("PKG");
			package2.KP_PackageID = "PKG2";
			package2.BookedDimensions.KPB_PackageQty = 0;

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, packageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, parentJob, packageJob);
			helper.PopulateDataObject(testShipment);

			AssertEquals("Should create IsManifestedPackage AddInfoCollection if KPB_PackageQty is not 0.",
				true.ToString(), testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.IsManifestedPackage)).Value);
			AssertNull("Should not create IsManifestedPackage AddInfoCollection if KPB_PackageQty is 0.",
				testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2").AddInfoCollection);
		}

		public void TestPopulateDataObject_NoPackages_DoesNotWritePackages()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var outturn = new DummyPackingParentWithOutturn();

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packages: null, outturn);
			helper.PopulateDataObject(testShipment);

			AssertNull(testShipment.PackingLineCollection);
		}

		public void TestPopulateDataObject_FromCollectionWithContainer_IgnoresPackingParentContainers()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			package1.KP_PackageID = "PKG1";
			var package2 = Data.PackageJob.Packages.AddNew("PKG");
			package2.KP_PackageID = "PKG2";
			var containerDataObject = new Container();
			containerDataObject.Link = 5;
			testShipment.SetContainerCollection(() => new DataObjectList<Container>(new Container[] { containerDataObject }));
			var container = Data.PackageJob.Containers.AddNew();

			var outturn = new DummyPackingParentWithOutturn((p) => ("CNT1", container.Container));

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package1, package2 }, outturn, containerDataObject);
			helper.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(new string[] { "PKG1", "PKG2" }, testShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 5, 5 }, testShipment.PackingLineCollection.Select(p => p.ContainerLink));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 1, 1 }, testShipment.PackingLineCollection.Select(p => p.OutturnQty));
		}

		public void TestPopulateDataObject_WithContainersInOutturn_AppendsToExistingCollection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");
			package1.KP_PackageID = "PKG1";
			package2.KP_PackageID = "PKG2";
			var container1 = Data.PackageJob.Containers.AddNew();
			var outturn = new DummyPackingParentWithOutturn((p) => ("CNT1", container1.Container));

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package1, package2 }, outturn);
			helper.PopulateDataObject(testShipment);

			const string containerErrorMessage = "Should include containers.";
			AssertContainsExactElementsInAnyOrder(containerErrorMessage, new[] { "CNT1" }, testShipment.ContainerCollection.Select(x => x.ContainerNumber.Value));
			AssertContainsExactElementsInAnyOrder(new string[] { "PKG1", "PKG2" }, testShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 0, 0 }, testShipment.PackingLineCollection.Select(p => p.ContainerLink));

			Data.PackageJob.Packages.RemoveAllFromRelationship();
			var package3 = Data.PackageJob.Packages.AddNew("PLT");
			package3.KP_PackageID = "PKG3";
			var container2 = Data.PackageJob.Containers.AddNew();
			container2.KP_PreviousPackageID = "CNT2";
			var outturn2 = new DummyPackingParentWithOutturn((p) => ("CNT2", container2.Container));
			AssertEquals(2, ((IPackingParentWithOutturn)outturn2).TotalNumberOfPiecesOutturned);
			var helper2 = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package3 }, outturn2);
			helper2.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(containerErrorMessage, new[] { "CNT1", "CNT2" }, testShipment.ContainerCollection.Select(x => x.ContainerNumber.Value));
			AssertContainsExactElementsInAnyOrder(new string[] { "PKG1", "PKG2", "PKG3" }, testShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			// If support for this is needed, update container link logic.
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 0, 0, 0 }, testShipment.PackingLineCollection.Select(p => p.ContainerLink));
			AssertEquals(2, testShipment.TotalNoOfPiecesLanded);
			AssertEquals("Should not write previous Package Id to AddInfoCollection for containers.",
				false, testShipment.ContainerCollection.Any(c => c.AddInfoCollection?.FirstOrDefault(a => a.Key.Equals(AddInfoKeyTypes.Types.PreviousPackageID)) != null));
		}

		public void TestPopulateDataObject_UsesOverride()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");
			package1.KP_PackageID = "PKG1";
			package2.KP_PackageID = "PKG2";
			package2.KP_PreviousPackageID = "PKG2_Prev";
			package1.KP_TransportRef = "DUM";
			package2.KP_TransportRef = "DUM";
			var outturn = new DummyPackingParentWithOutturn();

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages, outturn);
			helper.PopulateDataObject(testShipment);

			AssertContainsExactElementsInAnyOrder(new string[] { "PKG1", "PKG2" }, testShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 1, 1 }, testShipment.PackingLineCollection.Select(p => p.OutturnQty));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 2, 2 }, testShipment.PackingLineCollection.Select(p => p.OutturnDamagedQty));
			AssertContainsExactElementsInAnyOrder(new ZInt?[] { 1, 1 }, testShipment.PackingLineCollection.Select(p => p.OutturnPillagedQty));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 3, 3 }, testShipment.PackingLineCollection.Select(p => p.OutturnedHeight));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 4, 4 }, testShipment.PackingLineCollection.Select(p => p.OutturnedLength));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 5, 5 }, testShipment.PackingLineCollection.Select(p => p.OutturnedVolume));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 6, 6 }, testShipment.PackingLineCollection.Select(p => p.OutturnedWeight));
			AssertContainsExactElementsInAnyOrder(new ZDecimal?[] { 7, 7 }, testShipment.PackingLineCollection.Select(p => p.OutturnedWidth));
			AssertContainsExactElementsInAnyOrder(new string[] { "DUM", "DUM" }, testShipment.PackingLineCollection.Select(p => p.TransportReference?.ToString().Trim()));
			AssertNull("Should not write previous Package Id to AddInfoCollection if empty.", testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection);
			AssertEquals("Should write previous Package Id to AddInfoCollection if not empty.",
				"PKG2_Prev", testShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.PreviousPackageID)).Value);
		}

		public void TestPopulateDataObject_WithContainerView_UsesOverride()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX");
			var package2 = Data.PackageJob.Packages.AddNew("PKG");
			package1.KP_PackageID = "PKG1";
			package2.KP_PackageID = "PKG2";
			var container1 = Data.PackageJob.Containers.AddNew();
			var outturn = new DummyPackingParentWithOutturn((p) => ("CNT1", container1.Container));

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package1, package2 }, outturn);
			helper.PopulateDataObject(testShipment);

			const string containerErrorMessage = "Should include containers.";
			AssertContainsExactElementsInAnyOrder(containerErrorMessage, new[] { "CNT1" }, testShipment.ContainerCollection.Select(x => x.ContainerNumber.Value));
			var containerDataObject = testShipment.ContainerCollection.Single();
			AssertEquals(new ZDateTime(2000, 1, 1), containerDataObject.PackDate);
			AssertEquals(new ZDateTime(2000, 1, 2), containerDataObject.LCLUnpack);
		}

		#region TestExportPackLinesWithPANReferences

		public void TestExportPackLinesWithPANReferences()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			CreateCustomsAdditionalReference(package, "PAN", "PANReference", TransitWarehouseReferenceCategories.Codes.PortReference, "CLR", "AU");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper.PopulateDataObject(testShipment);

			var references = testShipment.PackingLineCollection.Single().PortReferenceCollection;

			CombineAssertions(() =>
			{
				AssertEquals(1, references.Count);
				var panReference = references.Single();
				AssertEquals("PAN", panReference.Type.Code);
				AssertEquals("PANReference", panReference.Reference.Value);
				AssertEquals("CLR", panReference.Status.Code);
				AssertEquals("AU", panReference.Country.Code);
			});
		}

		ICusEntryNumber CreateCustomsAdditionalReference<T>(T bizO, string entryType, string entryNum, string category = "CUS", string status = "", string country = "") where T : BusinessObject
		{
			var cusEntryNumber = (BusinessObject)Factory.New<ICusEntryNumber>();
			cusEntryNumber[CusEntryNumSchema.Constants.CE_ParentID] = bizO.PK;
			cusEntryNumber[CusEntryNumSchema.Constants.CE_ParentTable] = bizO.TableName;
			cusEntryNumber[CusEntryNumSchema.Constants.CE_EntryNum] = entryNum;
			cusEntryNumber[CusEntryNumSchema.Constants.CE_EntryType] = entryType;
			cusEntryNumber[CusEntryNumSchema.Constants.CE_Category] = category;
			cusEntryNumber[CusEntryNumSchema.Constants.CE_EntryStatus] = status;
			cusEntryNumber[CusEntryNumSchema.Constants.CE_RN_NKCountryCode] = country;

			return (ICusEntryNumber)cusEntryNumber;
		}

		#endregion

		public void TestPopulatePkgPackages_SetScreeningMethodAndAviationSecurityInspectionType()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("PKG", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("PLT", "P3");
			var package4 = Data.PackageJob.Packages.AddNew("PLT", "P4");
			var package5 = Data.PackageJob.Packages.AddNew("PLT", "P5");
			var package6 = Data.PackageJob.Packages.AddNew("PLT", "P6");
			var package7 = Data.PackageJob.Packages.AddNew("PLT", "P7");
			var package8 = Data.PackageJob.Packages.AddNew("PLT", "P8");
			var package9 = Data.PackageJob.Packages.AddNew("PLT", "P9");
			var package10 = Data.PackageJob.Packages.AddNew("PLT", "P10");
			var package11 = Data.PackageJob.Packages.AddNew("PLT", "P11");

			var ovp1 = Data.PackageJob.Packages.AddNew("BOX", "OVP1");
			Helper.PackHandlingUnit(ovp1, package4, ovp1);
			Helper.PackHandlingUnit(ovp1, package5, ovp1);

			var ovp2 = Data.PackageJob.Packages.AddNew("PLT", "OVP2");
			Helper.PackHandlingUnit(ovp2, package6, ovp2);
			Helper.PackHandlingUnit(ovp2, package7, ovp2);

			var ovp3 = Data.PackageJob.Packages.AddNew("PLT", "OVP3");
			Helper.PackHandlingUnit(ovp3, package8, ovp3);
			Helper.PackHandlingUnit(ovp3, package9, ovp3);

			var ovp4 = Data.PackageJob.Packages.AddNew("PLT", "OVP4");
			Helper.PackHandlingUnit(ovp4, package10, ovp4);
			Helper.PackHandlingUnit(ovp4, package11, ovp4);

			var package1Screening1 = Helper.CreatePackageScreening(package1, "ABC", passed: false);
			package1Screening1.KPS_Time = ZDateTimeOffset.Now.AddHours(1);
			var package1Screening2 = Helper.CreatePackageScreening(package1, "XRY", passed: true);
			package1Screening2.KPS_Time = ZDateTimeOffset.Now.AddHours(2);

			var package2Screening1 = Helper.CreatePackageScreening(package2, "GHI", passed: true);
			package2Screening1.KPS_Time = ZDateTimeOffset.Now.AddHours(3);
			var package2Screening2 = Helper.CreatePackageScreening(package2, "JKL", passed: false);
			package2Screening2.KPS_Time = ZDateTimeOffset.Now.AddHours(4);

			var ovp1Screening = Helper.CreatePackageScreening(ovp1, "AOM", passed: true);

			var package6Screening = Helper.CreatePackageScreening(package6, "XRY", passed: true);
			var package7Screening = Helper.CreatePackageScreening(package7, "XRY", passed: true);

			var package8Screening = Helper.CreatePackageScreening(package8, "XRY", passed: true);
			var package9Screening = Helper.CreatePackageScreening(package9, "AOM", passed: true);

			var packagesForWriter = new List<PkgPackage> { package1, package2, package3, package4, package5, package6, package7, package8, package9, package10, package11 };
			var topHUs = new List<PkgPackage> { ovp1, ovp2, ovp3, ovp4 };

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, topLevelHandlingUnits: topHUs);
			helper.PopulateDataObject(testShipment);

			var packingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8", "P9", "P10", "P11" }, packingLines.Select(p => p.ReferenceNumber.Value));

			AssertEquals("XRY", packingLines[0].ScreeningMethod);
			AssertEquals("XRY", packingLines[0].AviationSecurityInspectionType.Code);
			AssertEquals("X-Ray Equipment", packingLines[0].AviationSecurityInspectionType.Description);
			AssertNull(packingLines[1].AviationSecurityInspectionType);
			AssertNull(packingLines[2].AviationSecurityInspectionType);

			var parentPackingLineCollection = testShipment.ParentPackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "OVP1", "OVP2", "OVP3", "OVP4" }, parentPackingLineCollection.Select(p => p.ReferenceNumber.Value));

			AssertEquals("AOM", parentPackingLineCollection[0].ScreeningMethod);
			AssertEquals("AOM", parentPackingLineCollection[0].AviationSecurityInspectionType.Code);
			AssertEquals("Subjected to any other means", parentPackingLineCollection[0].AviationSecurityInspectionType.Description);

			AssertEquals("XRY", parentPackingLineCollection[1].ScreeningMethod);
			AssertEquals("XRY", parentPackingLineCollection[1].AviationSecurityInspectionType.Code);
			AssertEquals("X-Ray Equipment", parentPackingLineCollection[1].AviationSecurityInspectionType.Description);

			AssertNull(parentPackingLineCollection[2].AviationSecurityInspectionType);
			AssertNull(parentPackingLineCollection[3].AviationSecurityInspectionType);
		}

		public void TestPopulatePkgPackages_DoNotSetScreeningMethodAndAviationSecurityInspectionTypeForOverpack()
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var package1 = Data.PackageJob.Packages.AddNew("PLT", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("PKG", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("PLT", "P3");
			var package4 = Data.PackageJob.Packages.AddNew("PKG", "P4");
			var package5 = Data.PackageJob.Packages.AddNew("PLT", "P5");
			var package6 = Data.PackageJob.Packages.AddNew("PKG", "P6");
			var package7 = Data.PackageJob.Packages.AddNew("PLT", "P7");
			var package8 = Data.PackageJob.Packages.AddNew("PKG", "P8");

			var ovp1 = Data.PackageJob.Packages.AddNew("BOX", "OVP1");
			var ovp1PackageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, ovp1, "OVP");
			Helper.PackHandlingUnit(ovp1, package1, ovp1);
			Helper.PackHandlingUnit(ovp1, package2, ovp1);

			var ovp2 = Data.PackageJob.Packages.AddNew("PLT", "OVP2");
			var ovp2PackageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, ovp2, "OVP");
			Helper.PackHandlingUnit(ovp2, package3, ovp2);
			Helper.PackHandlingUnit(ovp2, package4, ovp2);

			var hu1 = Data.PackageJob.Packages.AddNew("BOX", "HU1");
			var hu1PackageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, hu1, "HU");
			Helper.PackHandlingUnit(hu1, package5, hu1);
			Helper.PackHandlingUnit(hu1, package6, hu1);

			var hu2 = Data.PackageJob.Packages.AddNew("PLT", "HU2");
			var hu2PackageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, hu2, "HU");
			Helper.PackHandlingUnit(hu2, package7, hu2);
			Helper.PackHandlingUnit(hu2, package8, hu2);

			var package1Screening = Helper.CreatePackageScreening(package1, "XRY", passed: true);
			var package2Screening = Helper.CreatePackageScreening(package2, "XRY", passed: true);
			var package3Screening = Helper.CreatePackageScreening(package3, "XRY", passed: true);
			var package4Screening = Helper.CreatePackageScreening(package4, "XRY", passed: true);
			var package5Screening = Helper.CreatePackageScreening(package5, "XRY", passed: true);
			var package6Screening = Helper.CreatePackageScreening(package6, "XRY", passed: true);
			var package7Screening = Helper.CreatePackageScreening(package7, "XRY", passed: true);
			var package8Screening = Helper.CreatePackageScreening(package8, "XRY", passed: true);

			var packagesForWriter = new List<PkgPackage> { package1, package2, package3, package4, package5, package6, package7, package8 };
			var topHUs = new List<PkgPackage> { ovp1, ovp2, hu1, hu2 };

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, topLevelHandlingUnits: topHUs);
			helper.PopulateDataObject(testShipment);

			var packingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8" }, packingLines.Select(p => p.ReferenceNumber.Value));

			var parentPackingLineCollection = testShipment.ParentPackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "OVP1", "OVP2", "HU1", "HU2" }, parentPackingLineCollection.Select(p => p.ReferenceNumber.Value));

			AssertNull(parentPackingLineCollection[0].AviationSecurityInspectionType);
			AssertNull(parentPackingLineCollection[1].AviationSecurityInspectionType);

			AssertEquals("XRY", parentPackingLineCollection[2].ScreeningMethod);
			AssertEquals("XRY", parentPackingLineCollection[2].AviationSecurityInspectionType.Code);
			AssertEquals("X-Ray Equipment", parentPackingLineCollection[2].AviationSecurityInspectionType.Description);

			AssertEquals("XRY", parentPackingLineCollection[3].ScreeningMethod);
			AssertEquals("XRY", parentPackingLineCollection[3].AviationSecurityInspectionType.Code);
			AssertEquals("X-Ray Equipment", parentPackingLineCollection[3].AviationSecurityInspectionType.Description);
		}

		public void TestPopulatePkgPackages_WithIsHighRisk_SetScreeningMethodAndAviationSecurityInspectionType()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("PKG", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("PLT", "P3");
			var package4 = Data.PackageJob.Packages.AddNew("PLT", "P4");
			var package5 = Data.PackageJob.Packages.AddNew("PLT", "P5");
			var package6 = Data.PackageJob.Packages.AddNew("PLT", "P6");
			var package7 = Data.PackageJob.Packages.AddNew("PLT", "P7");
			var package8 = Data.PackageJob.Packages.AddNew("PLT", "P8");
			var package9 = Data.PackageJob.Packages.AddNew("PLT", "P9");

			var ovp1 = Data.PackageJob.Packages.AddNew("BOX", "OVP1");
			Helper.PackHandlingUnit(ovp1, package6, ovp1);
			Helper.PackHandlingUnit(ovp1, package7, ovp1);

			var ovp2 = Data.PackageJob.Packages.AddNew("PLT", "OVP2");
			Helper.PackHandlingUnit(ovp2, package8, ovp2);
			Helper.PackHandlingUnit(ovp2, package9, ovp2);

			var package1Screening1 = Helper.CreatePackageScreening(package1, "XRY", passed: true);
			package1Screening1.KPS_Time = ZDateTimeOffset.Now.AddHours(1);
			var package1Screening2 = Helper.CreatePackageScreening(package1, "AOM", passed: true);
			package1Screening2.KPS_Time = ZDateTimeOffset.Now.AddHours(2);

			var package2Screening1 = Helper.CreatePackageScreening(package2, "AOM", passed: true);
			package2Screening1.KPS_Time = ZDateTimeOffset.Now.AddHours(1);

			var package3Screening1 = Helper.CreatePackageScreening(package3, "XRY", passed: false);
			package3Screening1.KPS_Time = ZDateTimeOffset.Now.AddHours(1);
			var package3Screening2 = Helper.CreatePackageScreening(package3, "AOM", passed: true);
			package3Screening2.KPS_Time = ZDateTimeOffset.Now.AddHours(2);

			var package4Screening1 = Helper.CreatePackageScreening(package4, "XRY", passed: true);
			package4Screening1.KPS_Time = ZDateTimeOffset.Now.AddHours(1);
			var package4Screening2 = Helper.CreatePackageScreening(package4, "AOM", passed: false);
			package4Screening2.KPS_Time = ZDateTimeOffset.Now.AddHours(2);

			var ovp1Screening = Helper.CreatePackageScreening(ovp1, "AOM", passed: true);

			var package6Screening = Helper.CreatePackageScreening(package6, "XRY", passed: true);
			var package7Screening = Helper.CreatePackageScreening(package7, "XRY", passed: true);

			var packagesForWriter = new List<PkgPackage> { package1, package2, package3, package4, package5, package6, package7, package8, package9 };
			var topHUs = new List<PkgPackage> { ovp1, ovp2 };

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var packingParentWithOutturn = new DummyPackingParentWithOutturn(isHighRisk: true);
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn, topLevelHandlingUnits: topHUs);
			helper.PopulateDataObject(testShipment);

			var packingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8", "P9" }, packingLines.Select(p => p.ReferenceNumber.Value));

			AssertEquals("XRY", packingLines[0].ScreeningMethod);
			AssertEquals("XRY", packingLines[0].AviationSecurityInspectionType.Code);
			AssertEquals("X-Ray Equipment", packingLines[0].AviationSecurityInspectionType.Description);
			AssertEquals("AOM", packingLines[1].ScreeningMethod);
			AssertEquals("AOM", packingLines[1].AviationSecurityInspectionType.Code);
			AssertEquals("Subjected to any other means", packingLines[1].AviationSecurityInspectionType.Description);
			AssertEquals("AOM", packingLines[2].ScreeningMethod);
			AssertEquals("AOM", packingLines[2].AviationSecurityInspectionType.Code);
			AssertEquals("Subjected to any other means", packingLines[2].AviationSecurityInspectionType.Description);
			AssertNull(packingLines[3].AviationSecurityInspectionType);
			AssertNull(packingLines[4].AviationSecurityInspectionType);

			var parentPackingLineCollection = testShipment.ParentPackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "OVP1", "OVP2" }, parentPackingLineCollection.Select(p => p.ReferenceNumber.Value));

			AssertEquals("AOM", parentPackingLineCollection[0].ScreeningMethod);
			AssertEquals("AOM", parentPackingLineCollection[0].AviationSecurityInspectionType.Code);
			AssertEquals("Subjected to any other means", parentPackingLineCollection[0].AviationSecurityInspectionType.Description);

			AssertNull(parentPackingLineCollection[1].AviationSecurityInspectionType);
		}

		public void TestPopulatePkgPackages_PopulateItemNo()
		{
			Data.CreatePackingData();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var package = Factory.New<PkgPackage>();
			package.KP_Sequence = (short)1;

			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.Dummy, Data.PackageJob);
			var testPackline = helper.PopulatePkgPackageDataObject(package, 0, new DummyOutturnProvider());

			AssertNotEquals("Parent Job Type is not Warehouse Job", ParentJobType.WarehouseOrder, Data.Dummy.ParentJobType);

			AssertNull("ItemNo of Packing Line should not be set when Parent Job Type is not Warehouse Order", testPackline.ItemNo);
		}

		public void TestPopulatePkgPackages_PopulateItemNo_WarehouseOrder()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var package = Factory.New<PkgPackage>();
			package.KP_Sequence = (short)1;

			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.Dummy, Data.PackageJob);
			var testPackline = helper.PopulatePkgPackageDataObject(package, 0, new DummyOutturnProvider());

			AssertEquals("Parent Job Type is Warehouse Job", ParentJobType.WarehouseOrder, Data.Dummy.ParentJobType);

			AssertEquals("ItemNo of Packing Line should set to the sequence of the package when Parent Job Type is Warehouse Order", (short)1, testPackline.ItemNo);
		}

		public void TestPopulatePkgPackages_PopulateOverpackUNDGItems()
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var package1 = Data.PackageJob.Packages.AddNew("PLT", "P1");

			var ovp1 = Data.PackageJob.Packages.AddNew("BOX", "OVP1");
			var ovp1PackageState = whsHelper.CreateWhsItemPackageState("ARV", location, rtu, ovp1, "OVP");

			var ovpUndgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			ovpUndgSubstance.DG_ExceptedQuantityCode = "E1";
			var ovpUndgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			ovpUndgDataItem.DI_DG = ovpUndgSubstance.PK;
			ovpUndgDataItem.DI_OverpackID = ovp1.KP_PackageID;

			ovp1.UNDGs.Add(ovpUndgDataItem);

			Helper.PackHandlingUnit(ovp1, package1, ovp1);

			var packagesForWriter = new List<PkgPackage> { package1 };
			var topHUs = new List<PkgPackage> { ovp1 };

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, topLevelHandlingUnits: topHUs);
			helper.PopulateDataObject(testShipment);

			var packingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { package1.KP_PackageID }, packingLines.Select(p => p.ReferenceNumber.Value));
			AssertEquals(null, packingLines.First(p => p.ReferenceNumber.Value == package1.KP_PackageID).UNDGCollection);

			var parentPackingLineCollection = testShipment.ParentPackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { ovp1.KP_PackageID }, parentPackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertEquals(1, parentPackingLineCollection.First(p => p.ReferenceNumber.Value == ovp1.KP_PackageID).UNDGCollection.Count);
			AssertEquals(ovp1.KP_PackageID, parentPackingLineCollection.First(p => p.ReferenceNumber.Value == ovp1.KP_PackageID).UNDGCollection[0].OverpackID);
			AssertEquals(ovp1.UNDGs.AsString, parentPackingLineCollection.First(p => p.ReferenceNumber.Value == ovp1.KP_PackageID).UNDGCollection[0].UNDGCode);
		}

		public void TestPopulateDataObject_UnloadDateAndLoadDate()
		{
			Data.CreatePackingData();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var package = Factory.New<PkgPackage>();
			Assert("Precondition: package has no parent package job.", !package.KP_KJ_ParentPackageJob.IsValid);

			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package });
			var testPackline = helper.PopulatePkgPackageDataObject(package, 0, new DummyOutturnProvider());

			AssertNotNull("UnloadDate should not empty", testPackline.UnloadDate);
			AssertNotNull("LoadDate should not empty", testPackline.LoadDate);
		}

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");
			CreateCustomsAdditionalReference(package, "TES", "Test Reference", TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, Data.PackageJob.Packages);
			helper.PopulateDataObject(testShipment);

			var references = testShipment.PackingLineCollection.Single().AdditionalReferenceCollection;

			CombineAssertions(() =>
			{
				AssertEquals(1, references.Count);
				var panReference = references.Single();
				AssertEquals("TES", panReference.Type.Code);
				AssertEquals("Test Reference", panReference.ReferenceNumber.Value);
			});
		}

		#endregion

		public void TestPopulatePkgPackageDataObject_NoPackageJobDoesNotThrowException()
		{
			Data.CreatePackingData();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var package = Factory.New<PkgPackage>();
			Assert("Precondition: package has no parent package job.", !package.KP_KJ_ParentPackageJob.IsValid);

			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package });
			AssertNoExceptionThrown(() => helper.PopulatePkgPackageDataObject(package, 0));
		}

		public void TestPopulatePkgPackageDataObject_PackageHasACorrespondingPackLineID()
		{
			Data.CreatePackingData();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var package = Factory.New<PkgPackage>();
			package.KP_PreviousPackLineID = "10";
			var jobPackLinePackageFromDB = (IJobPackLinePackage)package.Factory.LoadTop1(ObjectFactory.GetType(typeof(IJobPackLinePackage)), new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, package.PK));
			AssertEquals("Precondition: package doesn't have a corresponding PackLineID.", null, jobPackLinePackageFromDB);

			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package });
			var testPackline = helper.PopulatePkgPackageDataObject(package, 0, new DummyOutturnProvider());
			AssertEquals("PreviousPackingLineID is package's KP_PreviousPackLineID.", "10", testPackline.PreviousPackingLineID);

			var packageParent = Factory.New<DummyBusinessObject>();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = packageParent.PK;
			packageJob.KJ_ParentTableCode = packageParent.TablePrefix;
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			package.KP_Volume = 10m;
			package.KP_Sequence = 1;
			package.KP_F3_NKPackType = "PKG";
			var packagePK = package.PK;

			var jobShipment = (IForwardingShipment)Factory.New(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var packLine = Factory.New(ObjectFactory.GetType(typeof(IPackLine)));
			packLine[JobPackLinesSchema.JL_JS] = jobShipment.PK;
			packLine[JobPackLinesSchema.JL_PackLineId] = "1234";
			var packLinePK = packLine[JobPackLinesSchema.PK];

			var jobPackLinePackage = Factory.New(ObjectFactory.GetType(typeof(IJobPackLinePackage)));
			jobPackLinePackage[JobPackLinePackageSchema.JPP_KP_Packge] = packagePK;
			jobPackLinePackage[JobPackLinePackageSchema.JPP_JL_PackLine] = packLinePK;
			Factory.Save();
			jobPackLinePackageFromDB = (IJobPackLinePackage)package.Factory.LoadTop1(ObjectFactory.GetType(typeof(IJobPackLinePackage)), new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, package.PK));
			AssertEquals("Precondition: package has a corresponding PackLineID.", packLinePK, jobPackLinePackageFromDB.JPP_JL_PackLine);

			helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package });
			testPackline = helper.PopulatePkgPackageDataObject(package, 0, new DummyOutturnProvider());
			AssertEquals("PreviousPackingLineID is JL_PackLineId.", "1234", testPackline.PreviousPackingLineID);
		}

		public void TestPkgPackageJobDataObjectWriterHelper_WithOrderLineDictionaryPopulatesOrderLineLink()
		{
			var parentJob = Factory.New<DummyWithPacking>();
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			pkgPackageJob.KJ_JobID = "PJ00000001";
			pkgPackageJob.KJ_ParentTableCode = parentJob.TablePrefix;
			pkgPackageJob.KJ_ParentID = parentJob.PK;

			var package1 = pkgPackageJob.Packages.AddNew("BOX");
			var package2 = pkgPackageJob.Packages.AddNew("CNT");
			package2.Container.K0_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var packableItemParent1 = parentJob.Lines.AddNew();
			packableItemParent1.Description = "HELLO";
			packableItemParent1.DescriptionSupplement = "Greeting";
			packableItemParent1.Code = "SALUTATIONS";
			package1.Pack(packableItemParent1, 1m);

			var packableItemParent2 = parentJob.Lines.AddNew();
			packableItemParent2.Description = "COLOUR";
			packableItemParent2.DescriptionSupplement = "Colourful Mind";
			packableItemParent2.Code = "SOUND";
			package2.Pack(packableItemParent2, 2m);

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var orderLinesDictionary = new Dictionary<ZGuid, ZInt>();
			orderLinesDictionary.Add(((BusinessObject)packableItemParent1).PK, 0);
			orderLinesDictionary.Add(((BusinessObject)packableItemParent2).PK, 1);

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, parentJob, pkgPackageJob, orderLineDictionary: orderLinesDictionary);
			helper.PopulateDataObject(testShipment);

			var packedItemsData1 = testShipment.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single(invoiceLine => invoiceLine.OrderLineLink == 0);
			AssertEquals("packedItemsData1.Description", "HELLO", packedItemsData1.Description);
			AssertEquals("packedItemsData1.Description", "SALUTATIONS", packedItemsData1.PartNo);

			var packedItemsData2 = testShipment.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single(invoiceLine => invoiceLine.OrderLineLink == 1);
			AssertEquals("packedItemsData2.Description", "COLOUR", packedItemsData2.Description);
			AssertEquals("packedItemsData1.Description", "SOUND", packedItemsData2.PartNo);
		}

		public void TestPopulateParentPackingLineCollection_WithHU_PopulatesAndEstablishesLinkWithPackages()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var hu1 = Data.PackageJob.Packages.AddNew("PLT", "HU1");
			var hu2 = Data.PackageJob.Packages.AddNew("PLT", "HU2");
			var hu3 = Data.PackageJob.Packages.AddNew("PLT", "HU3");
			var p1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var p2 = Data.PackageJob.Packages.AddNew("PKG", "P2");
			var p3 = Data.PackageJob.Packages.AddNew("CTN", "P3");
			var p4 = Data.PackageJob.Packages.AddNew("BOX", "P4");

			Helper.PackHandlingUnit(hu1, p1, hu1);
			Helper.PackHandlingUnit(hu2, p2, hu2);
			Helper.PackHandlingUnit(hu3, p4, hu3);

			var packagesForWriter = new List<PkgPackage> { p1, p2, p3, p4 };

			var topHUs = new List<PkgPackage> { hu1, hu2 };

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, null, null, topHUs);
			helper.PopulateDataObject(testShipment);

			CombineAssertions(() =>
			{
				var topLevelPackingLines = testShipment.PackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2", "P3", "P4" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));
				var topLevelHUs = testShipment.ParentPackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "HU1", "HU2" }, topLevelHUs.Select(p => p.ReferenceNumber.Value));
				AssertEquals(0, topLevelHUs[0].Link);
				AssertEquals(1, topLevelHUs[1].Link);
				AssertEquals(0, topLevelPackingLines[0].ParentPackingLineLink);
				AssertEquals(1, topLevelPackingLines[1].ParentPackingLineLink);
				AssertEquals(null, topLevelPackingLines[2].ParentPackingLineLink);
				AssertEquals(null, topLevelPackingLines[3].ParentPackingLineLink);
			});
		}

		public void TestPopulateParentPackingLineCollection_WithMultiLevelHU_PopulatesAndEstablishesLinkWIthPackages()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var hu1 = Data.PackageJob.Packages.AddNew("PLT", "HU1");
			var hu2 = Data.PackageJob.Packages.AddNew("PLT", "HU2");
			var subHu = Data.PackageJob.Packages.AddNew("PLT", "SubHU");
			var p1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var p2 = Data.PackageJob.Packages.AddNew("PKG", "P2");
			var p3 = Data.PackageJob.Packages.AddNew("CTN", "P3");

			Helper.CreatePackageHandlingUnitDivot(hu2, subHu);
			Helper.PackHandlingUnit(hu1, p1, hu1);
			Helper.PackHandlingUnit(hu2, p2, hu2);
			Helper.PackHandlingUnit(subHu, p3, hu2);

			var packagesForWriter = new List<PkgPackage> { p1, p2, p3 };

			var topHUs = new List<PkgPackage> { hu1, hu2 };

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, null, null, topHUs);
			helper.PopulateDataObject(testShipment);

			CombineAssertions(() =>
			{
				var topLevelPackingLines = testShipment.PackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2", "P3" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));
				var topLevelHUs = testShipment.ParentPackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "HU1", "HU2" }, topLevelHUs.Select(p => p.ReferenceNumber.Value));
				AssertEquals(0, topLevelHUs[0].Link);
				AssertEquals(1, topLevelHUs[1].Link);
				AssertEquals(0, topLevelPackingLines[0].ParentPackingLineLink);
				AssertEquals(1, topLevelPackingLines[1].ParentPackingLineLink);
				AssertEquals(1, topLevelPackingLines[2].ParentPackingLineLink);
			});
		}

		public void TestPopulatePkgPackageDataObject_PopulatesAviationSecurityAdditionalInspectionType()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var hu1 = Data.PackageJob.Packages.AddNew("PLT", "HU1");
			var hu2 = Data.PackageJob.Packages.AddNew("PLT", "HU2");
			var hu3 = Data.PackageJob.Packages.AddNew("PLT", "HU3");
			var hu4 = Data.PackageJob.Packages.AddNew("PLT", "HU4");
			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("PKG", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("CTN", "P3");
			var package4 = Data.PackageJob.Packages.AddNew("BOX", "P4");
			var package5 = Data.PackageJob.Packages.AddNew("PKG", "P5");
			var package6 = Data.PackageJob.Packages.AddNew("CTN", "P6");

			Helper.PackHandlingUnit(hu1, package1, hu1);
			Helper.PackHandlingUnit(hu2, package2, hu2);
			Helper.PackHandlingUnit(hu3, package3, hu3);
			Helper.PackHandlingUnit(hu4, package6, hu4);

			var package1Screening1 = CreatePackageScreening(package1, "SC1", true, DateTime.Now);
			var package1Screening2 = CreatePackageScreening(package1, "XRY", true, DateTime.Now.AddSeconds(2));
			var package2Screening1 = CreatePackageScreening(package2, "SC3", true, DateTime.Now);
			var package2Screening2 = CreatePackageScreening(package2, "SC4", false, DateTime.Now.AddSeconds(2));
			var package3Screening1 = CreatePackageScreening(package3, "SC5", false, DateTime.Now);
			var package3Screening2 = CreatePackageScreening(package3, "SC6", true, DateTime.Now.AddSeconds(2));
			var package4Screening1 = CreatePackageScreening(package4, "SC7", true, DateTime.Now);
			var package5Screening1 = CreatePackageScreening(package5, "SC8", true, DateTime.Now);
			var package5Screening2 = CreatePackageScreening(package5, "SC8", true, DateTime.Now.AddSeconds(2));
			var package6Screening1 = CreatePackageScreening(package6, "SC9", true, DateTime.Now);
			var package6Screening2 = CreatePackageScreening(package6, "S10", true, DateTime.Now.AddSeconds(2));

			var hu1Screening1 = CreatePackageScreening(hu1, "S11", true, DateTime.Now);
			var hu1Screening2 = CreatePackageScreening(hu1, "AOM", true, DateTime.Now.AddSeconds(2));
			var hu2Screening1 = CreatePackageScreening(hu2, "S13", false, DateTime.Now);
			var hu2Screening2 = CreatePackageScreening(hu2, "S14", true, DateTime.Now.AddSeconds(2));
			var hu3Screening1 = CreatePackageScreening(hu3, "S15", true, DateTime.Now);
			var hu3Screening2 = CreatePackageScreening(hu3, "S16", false, DateTime.Now.AddSeconds(2));
			var hu4Screening1 = CreatePackageScreening(hu4, "S17", true, DateTime.Now);
			var hu4Screening2 = CreatePackageScreening(hu4, "S18", true, DateTime.Now.AddSeconds(2));

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var packagesForWriter = new List<PkgPackage> { package1, package2, package3, package4, package5 };
			var topHUs = new List<PkgPackage> { hu1, hu2, hu3 };
			var packingParentWithOutturn = new DummyPackingParentWithOutturn(isHighRisk: true);
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn, topLevelHandlingUnits: topHUs);
			helper.PopulateDataObject(testShipment);

			packagesForWriter = new List<PkgPackage> { package6 };
			topHUs = new List<PkgPackage> { hu4 };
			packingParentWithOutturn = new DummyPackingParentWithOutturn(isHighRisk: false);
			helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn, topLevelHandlingUnits: topHUs);
			helper.PopulateDataObject(testShipment);

			CombineAssertions(() =>
			{
				var topLevelPackingLines = testShipment.PackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2", "P3", "P4", "P5", "P6" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));

				AssertEquals("XRY", topLevelPackingLines[0].AviationSecurityAdditionalInspectionType.Code);
				AssertEquals("X-Ray Equipment", topLevelPackingLines[0].AviationSecurityAdditionalInspectionType.Description);
				AssertNull(topLevelPackingLines[1].AviationSecurityAdditionalInspectionType);
				AssertNull(topLevelPackingLines[2].AviationSecurityAdditionalInspectionType);
				AssertNull(topLevelPackingLines[3].AviationSecurityAdditionalInspectionType);
				AssertNull(topLevelPackingLines[4].AviationSecurityAdditionalInspectionType);
				AssertNull(topLevelPackingLines[5].AviationSecurityAdditionalInspectionType);

				var parentPackingLines = testShipment.ParentPackingLineCollection;
				AssertContainsExactElementsInAnyOrder(new string[] { "HU1", "HU2", "HU3", "HU4" }, parentPackingLines.Select(p => p.ReferenceNumber.Value));

				AssertEquals("AOM", parentPackingLines[0].AviationSecurityAdditionalInspectionType.Code);
				AssertEquals("Subjected to any other means", parentPackingLines[0].AviationSecurityAdditionalInspectionType.Description);
				AssertNull(parentPackingLines[1].AviationSecurityAdditionalInspectionType);
				AssertNull(parentPackingLines[2].AviationSecurityAdditionalInspectionType);
				AssertNull(parentPackingLines[3].AviationSecurityAdditionalInspectionType);
			});
		}

		public void TestPopulatePkgPackageDataObject_PopulatesIsHighRisk()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "P2");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var packagesForWriter = new List<PkgPackage> { package1 };
			var packingParentWithOutturn = new DummyPackingParentWithOutturn(isHighRisk: true);
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn);
			helper.PopulateDataObject(testShipment);

			packagesForWriter = new List<PkgPackage> { package2 };
			packingParentWithOutturn = new DummyPackingParentWithOutturn(isHighRisk: false);
			helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn);
			helper.PopulateDataObject(testShipment);

			var topLevelPackingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));

			AssertEquals(true, topLevelPackingLines[0].IsHighRisk);
			AssertEquals(null, topLevelPackingLines[1].IsHighRisk);
		}

		public void TestPkgPackageJobDataObjectWriterHelper_WithOrderReferenceDictionaryPopulatesOrderLineLink()
		{
			var parentJob = Factory.New<DummyWithPacking>();
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			pkgPackageJob.KJ_JobID = "PJ00000001";
			pkgPackageJob.KJ_ParentTableCode = parentJob.TablePrefix;
			pkgPackageJob.KJ_ParentID = parentJob.PK;

			var package1 = pkgPackageJob.Packages.AddNew("BOX");
			var package2 = pkgPackageJob.Packages.AddNew("CNT");
			package2.Container.K0_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var orderReferenceDictionary = new Dictionary<ZGuid, ZInt>();
			orderReferenceDictionary.Add(package1.PK, 0);
			orderReferenceDictionary.Add(package2.PK, 1);

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob));
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, new[] { package1, package2 }, orderReferenceDictionary: orderReferenceDictionary);
			helper.PopulateDataObject(testShipment);

			var packedItemsData1 = testShipment.PackingLineCollection.First(line => line.PackType.Code.ToString() == "BOX").PackedItemCollection.Single();
			AssertEquals(0, packedItemsData1.OrderLineLink);

			var packedItemsData2 = testShipment.PackingLineCollection.First(line => line.PackType.Code.ToString() == "PCE").PackedItemCollection.Single();
			AssertEquals(1, packedItemsData2.OrderLineLink);
		}

		public void TestPopulatePkgPackageDataObject_PopulatesSecurityStatus()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("PLT", "P2");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var packagesForWriter = new List<PkgPackage> { package1 };
			var packingParentWithOutturn = new DummyPackingParentWithOutturn(overriddenAviationSecurityInspectionType: "APP");
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn);
			helper.PopulateDataObject(testShipment);

			packagesForWriter = new List<PkgPackage> { package2 };
			packingParentWithOutturn = new DummyPackingParentWithOutturn(overriddenAviationSecurityInspectionType: "");
			helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, packingParentWithOutturn);
			helper.PopulateDataObject(testShipment);

			var topLevelPackingLines = testShipment.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new string[] { "P1", "P2" }, topLevelPackingLines.Select(p => p.ReferenceNumber.Value));

			AssertEquals("APP", topLevelPackingLines[0].AviationSecurityInspectionType.Code);
			AssertEquals("", topLevelPackingLines[0].AviationSecurityInspectionType.Description);
			AssertEquals(null, topLevelPackingLines[1].AviationSecurityInspectionType);
		}

		public void TestPopulatePkgPackageDataObject_PopulatesContentType()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Data.PackageJob));

			var packagesForWriter = new List<PkgPackage> { package1 };
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesForWriter, contentType: CollectionContent.Partial);
			helper.PopulateDataObject(testShipment);

			AssertEquals(CollectionContent.Partial, testShipment.PackingLineCollection.Content);
		}

		#region Implementation

		class DummyContainerView : IContainerView
		{
			public ZDateTime? PackCompleteDate => new ZDateTime(2000, 1, 1);

			public ZDateTime? UnpackCompleteDate => new ZDateTime(2000, 1, 2);
		}

		class DummyOutturnProvider : IOutturnProvider
		{
			public DummyOutturnProvider(bool isHighRisk = false, string overriddenAviationSecurityInspectionType = "")
			{
				this.isHighRisk = isHighRisk;
				this.overriddenAviationSecurityInspectionType = overriddenAviationSecurityInspectionType;
			}

			readonly bool isHighRisk;
			readonly string overriddenAviationSecurityInspectionType;

			public int OutturnQty => 1;

			public int OutturnDamagedQty => 2;

			public int OutturnPillagedQty => 1;

			public decimal OutturnedHeight => 3;

			public decimal OutturnedLength => 4;

			public decimal OutturnedVolume => 5;

			public decimal OutturnedWeight => 6;

			public decimal OutturnedWidth => 7;

			public string ActualTransportJobID => "DUM";

			public string ActualTransportJobTypeCode => "RTU";
			public string ActualTransportJobTypeDescription => "Receive Transportation Unit";

			public string ExpectedTransportJobID => "ASN1";

			public string ExpectedTransportJobTypeCode => "ASN";
			public string ExpectedTransportJobTypeDescription => "Receive ASN";

			public ZDateTime? UnloadDate => ZDateTime.Now.AddDays(-1);

			public ZDateTime? LoadDate => ZDateTime.Now;

			public bool IsHighRisk => isHighRisk;

			public ZString OverriddenAviationSecurityInspectionType => overriddenAviationSecurityInspectionType;
		}

		class DummyPackingParentWithOutturn : IPackingParentWithOutturn
		{
			public DummyPackingParentWithOutturn(Func<PkgPackage, (string ContainerNumber, PkgPackageContainer Container)> getParentContainer = null, bool isHighRisk = false, string overriddenAviationSecurityInspectionType = "")
			{
				this.getParentContainer = getParentContainer ?? new Func<PkgPackage, (string ContainerNumber, PkgPackageContainer Container)>((package) => (null, null));

				containerView = new DummyContainerView();
				this.outturnProvider = new DummyOutturnProvider(isHighRisk, overriddenAviationSecurityInspectionType);
			}

			readonly Func<PkgPackage, (string ContainerNumber, PkgPackageContainer Container)> getParentContainer;
			readonly DummyContainerView containerView;
			readonly DummyOutturnProvider outturnProvider;

			int IPackingParentWithOutturn.TotalNumberOfPiecesOutturned => 2;

			public IContainerView GetContainerView(PkgPackageContainer container) => containerView;

			public IOutturnProvider GetOutturnProvider(PkgPackage package) => outturnProvider;

			public (string ContainerNumber, PkgPackageContainer Container) GetParentContainer(PkgPackage package) => getParentContainer(package);
		}

		PkgPackageScreening CreatePackageScreening(PkgPackage package, string methodCode, bool passed, DateTimeOffset screeningTime)
		{
			var packageScreening = package.Screenings.AddNew();
			packageScreening.KPS_GS_NKScreenedBy = "STF";
			packageScreening.KPS_Passed = passed;
			packageScreening.KPS_Method = methodCode;
			packageScreening.KPS_Time = screeningTime;

			return packageScreening;
		}

		#endregion
	}
}
