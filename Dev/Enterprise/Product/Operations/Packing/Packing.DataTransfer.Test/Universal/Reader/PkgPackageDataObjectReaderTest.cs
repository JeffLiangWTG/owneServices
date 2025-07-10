using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class PkgPackageDataObjectReaderTest : PkgPackageDataObjectReaderTestCase
	{
		#region TestExistingPackageConstructor

		public void TestExistingPackageConstructor()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PkgPackageDataObjectReader(packingLine, Logger, Factory, default(PkgPackage)));

			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, package);
			var packageFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(package, packageFromReader);
		}

		#endregion

		#region TestConstructor_ImportOptionIsPartialMatch

		public void TestConstructor_ImportOptionIsPartialMatch()
		{
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "P1" };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "P2" };
			packingLine1.SetPackingLineCollection(() => new List<PackingLine>() { packingLine2 });

			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var reader = new PkgPackageDataObjectReader(packingLine1, Logger, Factory, packageJob, packageJob.Packages, importOption: ImportOption.PartialMatch, null, false, false, null, false);
			var packageFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(1, packageJob.Packages.Count);
			var newPackage = packageJob.Packages.Single();
			AssertEquals("New package should be created", "P1", newPackage.KP_PackageID);
			AssertEquals("Should not populate child package when ImportOption is PartialMatch", 0, newPackage.Packages.Count);
		}

		#endregion

		#region TestConstructor_ExistingPackage_ImportOptionIsPartialMatch

		public void TestConstructor_ExistingPackage_ImportOptionIsPartialMatch()
		{
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "P1" };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "P2" };
			packingLine1.SetPackingLineCollection(() => new List<PackingLine>() { packingLine2 });

			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package = packageJob.Packages.AddNew();
			var reader = new PkgPackageDataObjectReader(packingLine1, Logger, Factory, packageJob, packageJob.Packages, importOption: ImportOption.PartialMatch, targetPackage: package, false, false, null, false);
			var packageFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(1, packageJob.Packages.Count);
			AssertEquals(package, packageJob.Packages.Single());
			AssertEquals("Existing package should be updated", "P1", package.KP_PackageID);
			AssertEquals("Should not populate child package when ImportOption is PartialMatch", 0, package.Packages.Count);
		}

		#endregion

		#region TestMatchOnPackageIDs

		public void TestMatchOnPackageIDs()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew("PLT", "ABC");
			var package2 = packageJob.Packages.AddNew("PLT", "XYZ");

			var packageData = new PackingLine { ReferenceNumber = "ABc", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should NOT have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have matched Package with same ID.", package1, matchedPackage);
			AssertEquals("Should have updated the Package with same ID.", 150m, matchedPackage.KP_Weight);
		}

		#endregion

		#region TestMatchOnPackageIDs_MultiLevelHandlingUnitInnersMatched_PackedViaParentPackage_ShouldBeUpdated

		public void TestMatchOnPackageIDs_MultiLevelHandlingUnitInnersMatched_PackedViaParentPackage_ShouldBeUpdated()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// Current Package Tree
			//	HU1
			//		HU1Inner
			//		HU2
			//			HU2Inner1
			//			HU2Inner2
			var hu1 = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU1");
			var hu1Inner = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU1Inner");
			hu1.Packages.Add(hu1Inner);
			var hu2 = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU2");
			hu1.Packages.Add(hu2);
			var hu2Inner1 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU2Inner1");
			var hu2Inner2 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU2Inner2");
			hu2.Packages.Add(hu2Inner1);
			hu2.Packages.Add(hu2Inner2);

			hu2.KP_Weight = 1;
			hu1Inner.KP_Weight = 1;
			hu2Inner1.KP_Weight = 1;
			hu2Inner2.KP_Weight = 1;

			// PackingLine Tree To Be Imported
			//	HU1
			//		HU1Inner
			//		HU2Inner2
			//		HU2
			//			HU2Inner1
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU1"
			};
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU1Inner",
				Weight = 10
			};
			var packingLineForHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2",
				Weight = 20
			};
			var packingLineForHU2Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2Inner1",
				Weight = 30
			};
			var packingLineForHU2Inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2Inner2",
				Weight = 40
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU1Inner1, packingLineForHU2Inner2, packingLineForHU2 });
			packingLineForHU2.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2Inner1 });

			AssertEquals("Precondition - package job should only have 5 packages.", 5, packageJob.GetAllPackagesOnJob().Length);

			var reader = new PkgPackageDataObjectReader(packingLineForHU1, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			var newPackage = reader.ReadIntoBusinessObject();
			var hu1InnerAfterRead = newPackage.Packages.Single(p => p.PK == hu1Inner.PK);
			var hu2Inner2AfterRead = newPackage.Packages.Single(p => p.PK == hu2Inner2.PK);
			var hu2AfterRead = newPackage.Packages.Single(p => p.PK == hu2.PK);
			var hu2Inner1AfterRead = hu2AfterRead.Packages.Single(p => p.PK == hu2Inner1.PK);

			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.", new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2 }, packageJob.GetAllPackagesOnJob());
			AssertEquals("Weight should have been updated for matched packages.", 10m, hu1InnerAfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 40m, hu2Inner2AfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 20m, hu2AfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 30m, hu2Inner1AfterRead.KP_Weight);
		}

		#endregion

		#region TestMatchOnPackageIDs_MultiLevelHandlingUnit_PackedViaDivots_WithReferenceNumbers

		public void TestMatchOnPackageIDs_MultiLevelHandlingUnit_PackedViaDivots_WithReferenceNumbers()
		{
			Data.CreatePackingData();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.BOFactory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			// PackingLine Tree To Be Imported
			//	HandlingUnit-01
			//		HandlingUnit-02
			//			HandlingUnit-03
			//				Inner-01
			//			Inner-02
			//		Inner-03
			var handlingUnitPackingLine1 = CreatePackingLine("HandlingUnit-01", "", PackType.Freight.PLT, 450m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine2 = CreatePackingLine("HandlingUnit-02", "", PackType.Freight.PLT, 400m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine3 = CreatePackingLine("HandlingUnit-03", "", PackType.Freight.PLT, 350m, 10m, 10m, 10m, marksAndNumbers: "Furnitures", previousPackingLineID: "PreviousPackingLine");
			var inner1 = CreatePackingLine("Inner-01", "", PackType.Freight.BOX, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = CreatePackingLine("Inner-02", "", PackType.Freight.CTN, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
			var inner3 = CreatePackingLine("Inner-03", "", PackType.Freight.BAG, 150m, 1m, 1m, 1m, marksAndNumbers: "Furnitures");
			handlingUnitPackingLine1.SetPackingLineCollection(() => new List<PackingLine> { handlingUnitPackingLine2, inner3 });
			handlingUnitPackingLine2.SetPackingLineCollection(() => new List<PackingLine> { handlingUnitPackingLine3, inner2 });
			handlingUnitPackingLine3.SetPackingLineCollection(() => new List<PackingLine> { inner1 });

			AssertEquals("Precondition - No Divots have been created yet.", 0,
			Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);

			AssertEquals("Precondition - No Packages have been created yet.", 0,
			Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);

			var reader = new PkgPackageDataObjectReader(handlingUnitPackingLine1, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();
			AssertEquals("There should be 6 packages on the package Job", 6, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			AssertEquals(6, packages.Count());
			var handlingUnitPackage1 = AssertAndReturnPackage(packages, packageJob.KJ_ParentID, ZGuid.Empty, ZGuid.Empty, "HandlingUnit-01", "", "Furnitures", 450m, "KG", 10m, 10m, 10m, "CM", 1, PackType.Freight.PLT);
			var handlingUnitPackage2 = AssertAndReturnPackage(packages, packageJob.KJ_ParentID, handlingUnitPackage1.PK, handlingUnitPackage1.PK, "HandlingUnit-02", "", "Furnitures", 400m, "KG", 10m, 10m, 10m, "CM", 1, PackType.Freight.PLT);
			var handlingUnitPackage3 = AssertAndReturnPackage(packages, packageJob.KJ_ParentID, handlingUnitPackage2.PK, handlingUnitPackage1.PK, "HandlingUnit-03", "", "Furnitures", 350m, "KG", 10m, 10m, 10m, "CM", 1, PackType.Freight.PLT, "PreviousPackingLine");
			var inner1Package = AssertAndReturnPackage(packages, packageJob.KJ_ParentID, handlingUnitPackage3.PK, handlingUnitPackage1.PK, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, PackType.Freight.BOX);
			var inner2Package = AssertAndReturnPackage(packages, packageJob.KJ_ParentID, handlingUnitPackage2.PK, handlingUnitPackage1.PK, "Inner-02", "", "Furnitures", 150m, "KG", 2m, 2m, 2m, "CM", 1, PackType.Freight.CTN);
			var inner3Package = AssertAndReturnPackage(packages, packageJob.KJ_ParentID, handlingUnitPackage1.PK, handlingUnitPackage1.PK, "Inner-03", "", "Furnitures", 150m, "KG", 1m, 1m, 1m, "CM", 1, PackType.Freight.BAG);

			var divots = Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("5 Divots should have been created.", 5, divots.Length);
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage1.PK && d.KPD_KP_Package == handlingUnitPackage2.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage1.PK && d.KPD_KP_Package == inner3Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage2.PK && d.KPD_KP_Package == handlingUnitPackage3.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage2.PK && d.KPD_KP_Package == inner2Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage3.PK && d.KPD_KP_Package == inner1Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
		}

		public PkgPackage AssertAndReturnPackage(IEnumerable<PkgPackage> packages, ZGuid packageJobParentPk, ZGuid parentPackagePK, ZGuid topHandlingUnitPackagePK, string packageId, string externalReference, string marksAndNumbers, decimal weight, string weightUQ, decimal length, decimal width, decimal height, string dimUQ, int packageQty, string packType, string previousPackingLineID = "")
		{
			var package = packages.Single(p => p.KP_PackageID == packageId);
			AssertEquals(parentPackagePK, package.KP_KP_ParentPackage);
			AssertEquals(topHandlingUnitPackagePK, package.KP_KP_TopHandlingUnitPackage);
			AssertEquals(packageJobParentPk, package.PackageJob?.KJ_ParentID);
			AssertEquals(packageQty, package.KP_PackageQty);
			AssertEquals(marksAndNumbers, package.KP_MarksAndNumbers);
			AssertEquals(weight, package.KP_Weight);
			AssertEquals(weightUQ, package.KP_WeightUQ);
			AssertEquals(length, package.KP_Length);
			AssertEquals(width, package.KP_Width);
			AssertEquals(height, package.KP_Height);
			AssertEquals(dimUQ, package.KP_DimensionUQ);
			AssertEquals(packType, package.KP_F3_NKPackType);
			AssertEquals(externalReference, package.KP_ExternalReference);
			AssertEquals(previousPackingLineID, package.KP_PreviousPackLineID);

			return package;
		}

		#endregion

		#region TestMatchOnPackageIDs_MultiLevelHandlingUnit_PackedViaDivots_WithoutReferenceNumbers

		public void TestMatchOnPackageIDs_MultiLevelHandlingUnit_PackedViaDivots_WithoutReferenceNumbers()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// PackingLine Tree To Be Imported
			//	HandlingUnit-NoID
			//		Inner-01
			//	HandlingUnit-02
			//		Inner-NoID
			//	HandlingUnit-03
			//		Inner-02
			//		Inner-NoID
			var handlingUnitPackingLine1 = CreatePackingLine(null, "", PackType.Freight.PLT, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine2 = CreatePackingLine("HandlingUnit-02", "", PackType.Freight.PLT, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine3 = CreatePackingLine("HandlingUnit-03", "", PackType.Freight.PLT, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var inner1 = CreatePackingLine("Inner-01", "", PackType.Freight.BOX, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = CreatePackingLine(null, "", PackType.Freight.CTN, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
			var inner3 = CreatePackingLine("Inner-02", "", PackType.Freight.BAG, 150m, 3m, 3m, 3m, marksAndNumbers: "Furnitures");
			var inner4 = CreatePackingLine("", "", PackType.Freight.BLC, 150m, 4m, 4m, 4m, marksAndNumbers: "Furnitures");
			handlingUnitPackingLine1.SetPackingLineCollection(() => new List<PackingLine> { inner1 });
			handlingUnitPackingLine2.SetPackingLineCollection(() => new List<PackingLine> { inner2 });
			handlingUnitPackingLine3.SetPackingLineCollection(() => new List<PackingLine> { inner3, inner4 });

			AssertEquals("Precondition - No Divots have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Packages have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);

			var reader = new PkgPackageDataObjectReader(handlingUnitPackingLine1, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();

			reader = new PkgPackageDataObjectReader(handlingUnitPackingLine2, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();

			reader = new PkgPackageDataObjectReader(handlingUnitPackingLine3, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			reader.ReadIntoBusinessObject();

			AssertContainsExactElementsInAnyOrder("7 Packages have been created.", new string[] { "Inner-01", "HandlingUnit-02", "HandlingUnit-03", "Inner-02", "", "", "" }, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Select(p => p.KP_PackageID));
			AssertEquals("0 Divots should have been created.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
		}

		#endregion

		#region TestMatchOnPackageIDs_MultiLevelHandlingUnit_PackedViaDivots_WithReferenceNumbers_InnersUnpacked

		public void TestMatchOnPackageIDs_MultiLevelHandlingUnit_PackedViaDivots_WithReferenceNumbers_InnersUnpacked()
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
			//			HU2Inner3
			//			HU2Inner4
			var hu1 = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU1");
			var hu1Inner = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU1Inner");
			hu1.Packages.Add(hu1Inner);
			var hu2 = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU2");
			hu1.Packages.Add(hu2);
			var hu2Inner1 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU2Inner1");
			var hu2Inner2 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU2Inner2");
			var hu2Inner3 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU2Inner3");
			var hu2Inner4 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU2Inner4");
			hu2.Packages.Add(hu2Inner1);
			hu2.Packages.Add(hu2Inner2);
			hu2.Packages.Add(hu2Inner3);
			hu2.Packages.Add(hu2Inner4);

			hu2.KP_Weight = 1;
			hu1Inner.KP_Weight = 1;
			hu2Inner1.KP_Weight = 1;
			hu2Inner2.KP_Weight = 1;
			hu2Inner3.KP_Weight = 1;
			hu2Inner4.KP_Weight = 1;

			// PackingLine Tree To Be Imported
			//	HU1
			//		HU1Inner
			//		HU2Inner2
			//		HU2
			//			HU2Inner1
			//	HU2Inner3
			//	HU2Inner4
			var packingLineForHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU1"
			};
			var packingLineForHU1Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU1Inner",
				Weight = 10
			};
			var packingLineForHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2",
				Weight = 20
			};
			var packingLineForHU2Inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2Inner1",
				Weight = 30
			};
			var packingLineForHU2Inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2Inner2",
				Weight = 40
			};
			var packingLineForHU2Inner3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2Inner3",
				Weight = 50
			};
			var packingLineForHU2Inner4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "HU2Inner4",
				Weight = 60
			};
			packingLineForHU1.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU1Inner1, packingLineForHU2Inner2, packingLineForHU2 });
			packingLineForHU2.SetPackingLineCollection(() => new List<PackingLine>() { packingLineForHU2Inner1 });

			AssertEquals("Precondition - package job should only have 7 packages.", 7, packageJob.GetAllPackagesOnJob().Length);

			var readerForHU1Package = new PkgPackageDataObjectReader(packingLineForHU1, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			var hu1Package = readerForHU1Package.ReadIntoBusinessObject();
			var allPackages = packageJob.GetAllPackagesOnJob();
			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.", new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2, hu2Inner3, hu2Inner4 }, allPackages);

			var hu1InnerAfterRead = allPackages.Single(p => p.PK == hu1Inner.PK);
			var hu2Inner2AfterRead = allPackages.Single(p => p.PK == hu2Inner2.PK);
			var hu2AfterRead = allPackages.Single(p => p.PK == hu2.PK);
			var hu2Inner1AfterRead = allPackages.Single(p => p.PK == hu2Inner1.PK);
			AssertEquals("Weight should have been updated for matched packages.", 10m, hu1InnerAfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 40m, hu2Inner2AfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 20m, hu2AfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 30m, hu2Inner1AfterRead.KP_Weight);

			var readerForHU2Inner3Package = new PkgPackageDataObjectReader(packingLineForHU2Inner3, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs, isProcessingOuterPackLine: false);
			var hu2Inner3Package = readerForHU2Inner3Package.ReadIntoBusinessObject();
			allPackages = packageJob.GetAllPackagesOnJob();
			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.", new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2, hu2Inner3, hu2Inner4 }, packageJob.GetAllPackagesOnJob());
			AssertEquals("Weight should have been updated for matched packages.", 50m, hu2Inner3Package.KP_Weight);
			AssertEquals("Top handling unit package won't be cleared if isProcessingOuterPackLine is false.", hu1.PK, hu2Inner3Package.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Divot won't be deleted if isProcessingOuterPackLine is false.", 1, hu2Inner3Package.PackageHandlingUnitPackageDivots.Count);

			var readerForHU2Inner4Package = new PkgPackageDataObjectReader(packingLineForHU2Inner4, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs, isProcessingOuterPackLine: true);
			var hu2Inner4Package = readerForHU2Inner4Package.ReadIntoBusinessObject();
			allPackages = packageJob.GetAllPackagesOnJob();
			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.", new PkgPackage[] { hu1, hu1Inner, hu2, hu2Inner1, hu2Inner2, hu2Inner3, hu2Inner4 }, packageJob.GetAllPackagesOnJob());
			AssertEquals("Weight should have been updated for matched packages.", 60m, hu2Inner4Package.KP_Weight);
			AssertEquals("Top handling unit package will be cleared if isProcessingOuterPackLine is true.", Guid.Empty, hu2Inner4Package.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Divot will be deleted if isProcessingOuterPackLine is true.", 0, hu2Inner4Package.PackageHandlingUnitPackageDivots.Count);
		}

		#endregion

		#region TestMatchOnPackageIDs_HasTargetPackage_ExisitingPackageIDIsNotEmpty

		public void TestMatchOnPackageIDs_HasTargetPackage_ExisitingPackageIDIsNotEmpty()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew("PLT", "ABC");
			var package2 = packageJob.Packages.AddNew("PLT", "XYZ");

			var packageData = new PackingLine { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, targetPackage: package2);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should NOT have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have matched Package by target package", package2, matchedPackage);
			AssertEquals("No need to update the Package ID by target package if the Package ID has existed.", "XYZ", matchedPackage.KP_PackageID);
			AssertEquals("Should have updated the Package by target package.", 150m, matchedPackage.KP_Weight);
		}

		#endregion

		#region TestMatchOnPackageIDs_HasTargetPackage_ExisitingPackageIDIsEmpty

		public void TestMatchOnPackageIDs_HasTargetPackage_ExisitingPackageIDIsEmpty()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "");

			var packageData = new PackingLine { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, targetPackage: package2);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should NOT have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have matched Package by target package", package2, matchedPackage);
			AssertEquals("Should have updated the Package by target package.", "DEF", matchedPackage.KP_PackageID);
			AssertEquals("Should have updated the Package by target package.", 150m, matchedPackage.KP_Weight);
		}

		#endregion

		#region TestMatchAndRelabelOnPreviousPackageID

		public void TestMatchAndRelabelOnPreviousPackageID_RenamePackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "PKG1");
			Factory.SaveForTesting();

			var packageData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG5", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "PKG1" }
			});
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchAndRelabelOnPreviousPackageID);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should NOT have created a new Package.", 1, Data.PackageJob.Packages.Count);
			AssertEquals("Should have matched Package by previous ID.", package1, matchedPackage);
			AssertEquals("Should have updated the PackageID.", "PKG5", matchedPackage.KP_PackageID);
			AssertEquals("Should have updated the Package Weight.", 150m, matchedPackage.KP_Weight);
			AssertContains("Information - Renaming package PKG1 to PKG5.", Logger.Logs);
		}

		public void TestMatchAndRelabelOnPreviousPackageID_NoMatch()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "PKG1");

			var packageData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG5", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "PKG20" }
			});
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchAndRelabelOnPreviousPackageID);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have the PackageID.", "PKG5", matchedPackage.KP_PackageID);
			AssertEquals("Should have the Package Weight.", 150m, matchedPackage.KP_Weight);
			AssertContains("Warning - Unable to find package to rename with package id PKG20.", Logger.Logs);
		}

		public void TestMatchAndRelabelOnPreviousPackageID_RenamedPackageIsInSameBatch()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "PKG1");

			var packageData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG5", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "PKG1" }
			});
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchAndRelabelOnPreviousPackageID);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have the PackageID.", "PKG5", matchedPackage.KP_PackageID);
			AssertEquals("Should have the Package Weight.", 150m, matchedPackage.KP_Weight);
			AssertContains("Warning - Unable to find package to rename with package id PKG1.", Logger.Logs);
		}

		public void TestMatchAndRelabelOnPreviousPackageID_RenamePackage_PackageJobHasDuplicatedPackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "PKG1");
			var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "PKG2");
			Factory.SaveForTesting();

			var packageData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG2", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "PKG1" }
			});
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchAndRelabelOnPreviousPackageID);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should NOT have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have matched Package by previous ID.", package2, matchedPackage);
			AssertEquals("Should NOT have updated the PackageID.", "PKG2", matchedPackage.KP_PackageID);
			AssertEquals("Should have updated the Package Weight.", 150m, matchedPackage.KP_Weight);
			AssertContains("Warning - Unable to find package to rename with package id PKG1.", Logger.Logs);
		}

		#endregion

		#region TestTargetPackage

		public void TestTargetPackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "XYZ");

			var packageData = new PackingLine { ReferenceNumber = "ABC", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, targetPackage: package1);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should NOT have created a new Package.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Should have matched Package by target package", package1, matchedPackage);
			AssertEquals("Should have updated the Package by target package.", 150m, matchedPackage.KP_Weight);
		}

		#endregion

		#region TestTargetPackageIsNull

		public void TestTargetPackageIsNull()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = Data.PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = Data.PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "XYZ");

			var packageData = new PackingLine { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages);
			var newPackage = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should not match on existing packages", false, new PkgPackage[] { package1, package2 }.Any(p => p.PK == newPackage.PK));
			AssertEquals("Should have created a new Package.", 3, Data.PackageJob.Packages.Count);
			AssertEquals("New package should be added.", "DEF", newPackage.KP_PackageID);
			AssertEquals("New package should be added.", 1, newPackage.KP_PackageQty);
			AssertEquals("New package should be added.", Constants.PkgUnit.Pallet, newPackage.KP_F3_NKPackType);
			AssertEquals("New package should be added.", 150m, newPackage.KP_Weight);
		}

		#endregion

		#region TestMatchedByPackingLineID

		public void TestMatchedByPackingLineID()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			package1.KP_ExternalReference = "Packline1";

			var packageData = new PackingLine { PackQty = 1, PackingLineID = "PackLine1", Weight = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackingLineID);
			var matchedPackage = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should have updated packline's Weight.", 150m, matchedPackage.KP_Weight);
		}

		public void TestMatchedByPackingLineID_MatchInnerByPacklineId()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var handlingUnit = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "HandlingUnit");
			var packline = handlingUnit.Packages.AddNew(Constants.PkgUnit.Pallet);
			packline.KP_ExternalReference = "InnerPackline1";
			packline.KP_Weight = 10m;

			var handlingUnitPackingLine = CreatePackingLine("HandlingUnit", "", PackType.Freight.PLT);
			var innerPackline1 = CreatePackingLine("", "InnerPackline1", PackType.Freight.PLT, weight: 150m);
			var innerPackline2 = CreatePackingLine("", "InnerPackline2", PackType.Freight.PLT, weight: 20m);
			handlingUnitPackingLine.SetPackingLineCollection(() => new List<PackingLine> { innerPackline1, innerPackline2 });

			var packageReader = new PkgPackageDataObjectReader(handlingUnitPackingLine, Logger, Factory, packageJob, packageJob.Packages);
			var matchedHU = packageReader.ReadIntoBusinessObject();
			AssertEquals("Should have 2 packages on Handling Unit", 2, matchedHU.Packages.Count);
			var inner1 = matchedHU.Packages.FirstOrDefault(p => p.KP_ExternalReference == "InnerPackline1");
			AssertEquals("Should have updated weight for InnerPackline1.", 150m, inner1.KP_Weight);
			var inner2 = matchedHU.Packages.FirstOrDefault(p => p.KP_ExternalReference == "InnerPackline2");
			AssertNotNull("Should create new packline for InnerPackline2.", inner2);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_FieldMappings

		public void TestPkgPackageDataObjectReader_FieldMappings()
		{
			AssertPkgPackageDataObjectReader_FieldMappings(shouldImportBookedDimensions: false);
		}

		public void TestPkgPackageDataObjectReader_FieldMappings_ShouldAlsoImportBookedDimensions()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsImportingBookedDimensions);
			var parentJob = Factory.New<DummyWithPackingSupportsImportingBookedDimensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			AssertPkgPackageDataObjectReader_FieldMappings(shouldImportBookedDimensions: true, pkgPackageJob);
		}

		void AssertPkgPackageDataObjectReader_FieldMappings(bool shouldImportBookedDimensions, PkgPackageJob customPackageJob = null)
		{
			#region Setup Packing

			Data.CreatePackingData();
			Factory.SaveForTesting();

			var pkgPackageJob = customPackageJob ?? Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";

			var container = pkgPackageJob.Packages.AddNew("CNT");
			var box = container.Packages.AddNew("BOX");
			box.KP_DimensionUQ = "M";
			box.KP_Height = 1m;
			box.KP_Length = 2m;
			box.KP_PackageID = "PACKAGE123";
			box.KP_PackageQty = 1;
			box.KP_VolumeUQ = "M3";
			box.KP_Weight = 5m; // bumped by 4 when inner weight set
			box.KP_WeightUQ = "T";
			box.KP_Width = 6m;
			box.KP_MarksAndNumbers = "MARK123";
			box.KP_TransportRef = "TRANSPORT REF";
			box.KP_GoodsDescription = "GOODS DESC";
			box.KP_HSCode = "HARMON CODE";
			box.KP_ExternalReference = "BOX External Ref";
			box.KP_Volume = 12m;
			box.KP_DamagedReason = "LOS";
			box.UNDGs.Add(Data.UndgDataItemEXP);

			var carton = box.Packages.AddNew("CTN");
			carton.KP_DimensionUQ = "IN";
			carton.KP_Height = 7m;
			carton.KP_Length = 8m;
			carton.KP_PackageID = "CTN123";
			carton.KP_PackageQty = 9;
			carton.KP_VolumeUQ = "M3";
			carton.KP_Weight = 4m;
			carton.KP_WeightUQ = "T";
			carton.KP_Width = 11m;
			carton.KP_MarksAndNumbers = "CTN MARK123";
			carton.KP_TransportRef = "CTN TRANSPORT REF";
			carton.KP_GoodsDescription = "CTN GOODS DESC";
			carton.KP_HSCode = "CTN HARMON CODE";
			carton.KP_ExternalReference = "CTN External Ref";
			carton.KP_Volume = 13m;
			carton.KP_RequiresTemperatureControl = true;
			carton.KP_RequiredTemperatureMinimum = -10m;
			carton.KP_RequiredTemperatureMaximum = -5m;
			carton.KP_RequiredTemperatureUnit = "C";
			carton.KP_RH_NKCommodityCode = "GEN";
			carton.KP_DamagedReason = "WT";
			carton.UNDGs.Add(Data.UndgDataItemLOS);

			#endregion

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob), writerStrategy: DefaultDataObjectWriterStrategy.TestInstance)).GetDataObject(pkgPackageJob);
			var packingLineData = pkgPackageJobDataObject.PackingLineCollection[0];

			var dummy = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);

			var reader = new PkgPackageDataObjectReader(packingLineData, Logger, Factory, packageJob, packageJob.Packages);
			var boxRead = reader.ReadIntoBusinessObject();

			AssertNotNull(boxRead);
			AssertNotEquals(box, boxRead);

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
				AssertEquals("boxRead.KP_DamagedReason", "LOS", boxRead.KP_DamagedReason);
				AssertEquals("boxRead.KP_ExternalReference", "BOX External Ref", boxRead.KP_ExternalReference);
				AssertEquals("Box should Contain 1 Dangerous Good", 1, boxRead.UNDGs.Count);
				AssertEquals("Dangerous Good's ParentTableCode should be" + PkgPackageSchema.Constants.Prefix, PkgPackageSchema.Constants.Prefix, boxRead.UNDGs[0].DI_ParentTableCode);
				AssertCollectionContains("EXP", boxRead.UNDGs.Select(u => u.Substance.DG_Code));
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
				if (shouldImportBookedDimensions)
				{
					AssertEquals("boxRead.BookedDimensions.KPB_KP_Package", ctnRead.PK, ctnRead.BookedDimensions.KPB_KP_Package);
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
				AssertEquals("ctnRead.KP_RequiresTemperatureControl", true, ctnRead.KP_RequiresTemperatureControl);
				AssertEquals("ctnRead.KP_RequiredTemperatureMinimum", -10m, ctnRead.KP_RequiredTemperatureMinimum);
				AssertEquals("ctnRead.KP_RequiredTemperatureMaximum", -5m, ctnRead.KP_RequiredTemperatureMaximum);
				AssertEquals("ctnRead.KP_RequiredTemperatureUnit", "C", ctnRead.KP_RequiredTemperatureUnit);
				AssertEquals("ctnRead.KP_RH_NKCommodityCode", "GEN", ctnRead.KP_RH_NKCommodityCode);
				AssertEquals("ctnRead.KP_DamagedReason", "WT", ctnRead.KP_DamagedReason);
				AssertEquals("Box should Contain 1 Dangerous Good", 1, ctnRead.UNDGs.Count);
				AssertEquals("Dangerous Good's ParentTableCode should be" + PkgPackageSchema.Constants.Prefix, PkgPackageSchema.Constants.Prefix, ctnRead.UNDGs[0].DI_ParentTableCode);
				AssertCollectionContains("LOS", ctnRead.UNDGs.Select(u => u.Substance.DG_Code));
			});

			if (!shouldImportBookedDimensions)
			{
				var bookedDimensions = Factory.Load<PkgPackageBookedDetail>(new ZQuery());
				AssertEquals("Given the dummy packing object does not implement IPackingParentSupportsImportingBookedDimensions then the import should not import any booked dimensions.", 0, bookedDimensions.Length);
			}
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_IsDamaged

		public void TestPkgPackageDataObjectReader_KP_IsDamaged()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				PackType = new PackageType { Code = Constants.PkgUnit.Pallet, Description = "Pallet" },
				OutturnDamagedQty = 1
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var bizo = reader.ReadIntoBusinessObject();
			AssertEquals(true, bizo.KP_IsDamaged);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KPPackageQtyZero

		public void TestPkgPackageDataObjectReader_KPPackageQtyZeroThrowException()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				PackType = new PackageType { Code = Constants.PkgUnit.Pallet, Description = "Pallet" },
				PackQty = 0,
				IsUnknownQty = false
			};

			var expectedErrorMessage = @"All package counts must be greater than zero.
ID: 
Pack Type: PLT
Quantity: 0";

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			packingLine.IsUnknownQty = true;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());

			expectedErrorMessage = @"All package counts must be greater than zero.
ID: 
Pack Type: BOX
Quantity: -1";
			packingLine.PackType = new PackageType { Code = Constants.PkgUnit.Box, Description = "Box" };
			packingLine.PackQty = -1;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			packingLine.PackQty = 1;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestValidatePackageQuantityAndType_PackType

		public void TestValidatePackageQuantityAndType_PackType()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				PackType = null,
				PackQty = 1
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertEquals(Constants.PkgUnit.Package, reader.ReadIntoBusinessObject().KP_F3_NKPackType);

			packingLine.PackType = new PackageType { Code = "", Description = "" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertEquals(Constants.PkgUnit.Package, reader.ReadIntoBusinessObject().KP_F3_NKPackType);

			packingLine.PackType = new PackageType { Code = Constants.PkgUnit.Box, Description = "Box" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertEquals(Constants.PkgUnit.Box, reader.ReadIntoBusinessObject().KP_F3_NKPackType);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_DimensionUQBlankThrowException

		public void TestPkgPackageDataObjectReader_KP_DimensionUQBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_DimensionUQ] CHECK ((KP_Length = 0 AND KP_Width = 0 AND KP_Height = 0) OR KP_DimensionUQ <> '');
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Length = 0m,
				Width = 0m,
				Height = 0m,
				LengthUnit = new UnitOfLength { Code = "" }
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
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

			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
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

			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
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

			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Height > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			packingLine.Height = 0m;
			packingLine.LengthUnit = new UnitOfLength { Code = "M" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("All dimensions are zero and LengthUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_HeightLessThanZeroThrowException

		public void TestPkgPackageDataObjectReader_KP_HeightLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Height] CHECK (KP_Height >= 0);
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Height = 0m
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Height = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Height = 5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Height > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Height = -5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Height < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package height must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Height: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_LengthLessThanZeroThrowException

		public void TestPkgPackageDataObjectReader_KP_LengthLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Length] CHECK (KP_Length >= 0);
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Length = 0m
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Length = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Length = 5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Length > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Length = -5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Length < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package length must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Length: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_ReqTempMinGreaterThanKP_ReqTempMaxThrowException

		public void TestPkgPackageDataObjectReader_KP_ReqTempMinGreaterThanKP_ReqTempMaxThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_RequiredTemperatureMinimum_KP_RequiredTemperatureMaximum] CHECK (KP_RequiredTemperatureMaximum >= KP_RequiredTemperatureMinimum);
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				RequiredTemperatureMinimum = 0m,
				RequiredTemperatureMaximum = 0m
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Temps equal: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.RequiredTemperatureMaximum = 10m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Max temp > min temp: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.RequiredTemperatureMaximum = -5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("max temp < min temp: Exception should be thrown", typeof(DataObjectReadFailureException), @"Minimum temperature cannot be greater than maximum temperature.
ID: ABC
Pack Type: PKG
Quantity: 1
Required Min Temperature: 0
Required Max Temperature: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_VolumeLessThanZeroThrowException

		public void TestPkgPackageDataObjectReader_KP_VolumeLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Volume] CHECK (KP_Volume >= 0);
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				Volume = 0m,
				ReferenceNumber = "ABC"
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Volume = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = 5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Volume > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = -5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Volume < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package volume must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Volume: -5
Volume Unit: M3", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_VolumeUqBlankThrowException

		public void TestPkgPackageDataObjectReader_KP_VolumeUqBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_VolumeUQ] CHECK (KP_Volume = 0 OR KP_VolumeUQ <> '');
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Volume = 0m,
				VolumeUnit = new UnitOfVolume { Code = "" }
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = 45m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Volume > 0 and VolumeUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Volume Unit required if a package volume value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Volume: 45
Volume Unit: ", () => reader.ReadIntoBusinessObject());

			packingLine.Volume = 0m;
			packingLine.VolumeUnit = new UnitOfVolume { Code = "M3" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Volume is zero and VolumeUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_WeightLessThanZeroThrowException

		public void TestPkgPackageDataObjectReader_KP_WeightLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Weight] CHECK (KP_Weight >= 0);
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Weight = 0m
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Weight = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = 5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Weight > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = -5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Weight < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package weight must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Weight: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_WeightUqBlankThrowException

		public void TestPkgPackageDataObjectReader_KP_WeightUqBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_WeightUQ] CHECK (KP_Weight = 0 OR KP_WeightUQ <> '');
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Weight = 0m,
				WeightUnit = new UnitOfWeight { Code = "" }
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = 45m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Weight > 0 and WeightUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Weight Unit required if a package weight value is entered.
ID: ABC
Pack Type: PKG
Quantity: 1
Weight: 45
Weight Unit: ", () => reader.ReadIntoBusinessObject());

			packingLine.Weight = 0m;
			packingLine.WeightUnit = new UnitOfWeight { Code = "KG" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Weight is zero and WeightUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_WidthLessThanZeroThrowException

		public void TestPkgPackageDataObjectReader_KP_WidthLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Width] CHECK (KP_Width >= 0);
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				Width = 0m
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Width = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Width = 5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("Width > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.Width = -5m;
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("Width < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package width must not be negative.
ID: ABC
Pack Type: PKG
Quantity: 1
Width: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_WeightUQInvalid

		public void TestPkgPackageDataObjectReader_KP_WeightUQInvalid()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				WeightUnit = new UnitOfWeight() { Code = "KG", Description = "Valid" }
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("WeightUQ = KG: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.WeightUnit = new UnitOfWeight() { Code = "AA", Description = "Invalid" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("WeightUQ  = AA: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package weight unit is invalid.
ID: ABC
Pack Type: PKG
Quantity: 1
Weight Unit: AA", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_VolumeUQInvalid

		public void TestPkgPackageDataObjectReader_KP_VolumeUQInvalid()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				VolumeUnit = new UnitOfVolume() { Code = "M3", Description = "Valid" }
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("VolumeUQ = M3: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.VolumeUnit = new UnitOfVolume() { Code = "AA", Description = "Invalid" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("VolumeUQ  = AA: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package volume unit is invalid.
ID: ABC
Pack Type: PKG
Quantity: 1
Volume Unit: AA", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_KP_DimensionUQInvalid

		public void TestPkgPackageDataObjectReader_KP_DimensionUQInvalid()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine
			{
				ReferenceNumber = "ABC",
				LengthUnit = new UnitOfLength() { Code = "MM", Description = "Valid" }
			};
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertNoExceptionThrown("DimensionUQ = MM: No exception required.", () => reader.ReadIntoBusinessObject());

			packingLine.LengthUnit = new UnitOfLength() { Code = "AA", Description = "Invalid" };
			reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			AssertExceptionThrown("DimensionUQ  = AA: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package dimension unit is invalid.
ID: ABC
Pack Type: PKG
Quantity: 1
Dimension Unit: AA", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_PopulateUNDGs

		public void TestPkgPackageDataObjectReader_PopulateUNDGs()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var box = pkgPackageJob.Packages.AddNew(Constants.PkgUnit.Box, "BOX1");

			box.UNDGs.Add(Data.UndgDataItemLOS);
			box.UNDGs.Add(Data.UndgDataItemEXP);

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, pkgPackageJob), writerStrategy: DefaultDataObjectWriterStrategy.TestInstance)).GetDataObject(pkgPackageJob);
			var packingLine = pkgPackageJobDataObject.PackingLineCollection[0];

			var reader1 = new PkgPackageDataObjectReader(packingLine, Logger, Factory, pkgPackageJob, pkgPackageJob.Packages, ImportOption.MatchOnPackageIDs);
			var package = reader1.ReadIntoBusinessObject();

			Factory.SaveForTesting();
			Factory.SaveAtEndOfImport(logger);

			AssertEquals("Package should contain 2 Dangerous Goods", 2, package.UNDGs.Count);
			AssertEquals("Box should contain 2 Dangerous Goods", 2, box.UNDGs.Count);
			packingLine.Link = null;

			var reader2 = new PkgPackageDataObjectReader(packingLine, Logger, Factory, package.PackageJob, package.PackageJob.Packages, ImportOption.MatchOnPackageIDs);
			package = reader2.ReadIntoBusinessObject();

			Factory.SaveForTesting();
			Factory.SaveAtEndOfImport(logger);

			AssertEquals("Package should contain 2 Dangerous Goods after importing again", 2, package.UNDGs.Count);
			AssertEquals("Box should contain 2 Dangerous Goods after importing again", 2, box.UNDGs.Count);
		}

		#endregion

		#region TestUXMLShipmentAdditionalReferencesPANNumber

		public void TestUXMLShipmentAdditionalReferencesPANNumber()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var packingLine = new PackingLine { ReferenceNumber = "ABC", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packingLine.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			var panReference = CreatePortReference("PAN", "Customs Entry Number", "AU", "PAN Number", "CLR");
			packingLine.SetPortReferenceCollection(() => new List<PortReference>());
			packingLine.PortReferenceCollection.Add(panReference);

			var packageReader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var createdPackage = packageReader.ReadIntoBusinessObject();
			var customEntryNumbers = createdPackage.PortReferences;

			var panOnPackage = customEntryNumbers.Find(new ZQuery(CusEntryNumSchema.CE_EntryType, "PAN")).Single();
			AssertEquals("PAN Number", (ZString)panOnPackage[CusEntryNumSchema.Constants.CE_EntryNum]);
			AssertEquals("AU", (ZString)panOnPackage[CusEntryNumSchema.Constants.CE_RN_NKCountryCode]);
			AssertEquals("CLR", (ZString)panOnPackage[CusEntryNumSchema.Constants.CE_EntryStatus]);
		}

		PortReference CreatePortReference(string code, string description, string countryCode, string reference, string statusCode)
		{
			var referenceType = new PortReferenceType() { Code = code, Description = description };
			var country = new Country { Code = countryCode };
			var status = new PortReferenceStatus { Code = statusCode };
			return new PortReference { Type = referenceType, Country = country, Reference = reference, Status = status };
		}

		#endregion

		#region TestUXMLShipmentAdditionalReferences

		public void TestUXMLShipmentAdditionalReferences()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var packingLine = new PackingLine { ReferenceNumber = "ABC", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packingLine.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			var reference = CreateAdditionalReference("TES", "Test Type", "Test Number");
			packingLine.SetAdditionalReferenceCollection(() => new List<AdditionalReference>());
			packingLine.AdditionalReferenceCollection.Add(reference);

			var packageReader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var createdPackage = packageReader.ReadIntoBusinessObject();
			var cusEntryReferences = createdPackage.CusEntryNumReferences;

			var cusEntryNumber = cusEntryReferences.Find(new ZQuery(CusEntryNumSchema.CE_EntryType, "TES")).Single();
			AssertEquals("Test Number", (ZString)cusEntryNumber[CusEntryNumSchema.Constants.CE_EntryNum]);
		}

		AdditionalReference CreateAdditionalReference(string code, string description, string reference)
		{
			var referenceType = new EntryType() { Code = code, Description = description };
			return new AdditionalReference { Type = referenceType, ReferenceNumber = reference };
		}

		#endregion

		#region TestInnerPackagesWithPackTypeOfCNTAreImportedAsUnit

		public void TestInnerPackagesWithPackTypeOfCNTAreImportedAsUnit()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.PackType = new PackageType { Code = Constants.PkgUnit.Container };
			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var package = reader.ReadIntoBusinessObject();
			AssertEquals(Constants.PkgUnit.Unit, package.KP_F3_NKPackType);
		}

		#endregion

		#region TestCreatePkgPackageWithLargeDimensions

		public void TestCreatePkgPackageWithLargeDimensions()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.PackType = new PackageType { Code = Constants.PkgUnit.Container };
			packingLine.LengthUnit = new UnitOfLength { Code = "M" };

			// 999^3 = 979146657 > 999,999.999
			packingLine.Height = 999m;
			packingLine.Length = 999m;
			packingLine.Width = 999m;
			packingLine.VolumeUnit = new UnitOfVolume { Code = "M3" };

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var package = reader.ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		#endregion

		#region TestPkgPackageDataObjectReader_LoosePackageID_ParentJobNotSupportConcreteLoosePackage

		public void TestPkgPackageDataObjectReader_LoosePackageID_ParentJobNotSupportConcreteLoosePackage()
		{
			Data.CreatePackingData();
			AssertNull("Precondition: ParentJob does not support conversion of a loose package ID to concrete package", Data.Dummy as DummyWithPackingSupportConcreteLoosePackage);

			TestPkgPackageDataObjectReader_LoosePackageID(Data.PackageJob);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_LoosePackageID_ParentJobSupportConcreteLoosePackage

		public void TestPkgPackageDataObjectReader_LoosePackageID_ParentJobSupportConcreteLoosePackage()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportConcreteLoosePackage);

			var parentJob = Factory.New<DummyWithPackingSupportConcreteLoosePackage>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			TestPkgPackageDataObjectReader_LoosePackageID(packageJob);
		}

		void TestPkgPackageDataObjectReader_LoosePackageID(PkgPackageJob packageJob)
		{
			var packingLine = new PackingLine
			{
				PackQty = 1,
				ReferenceNumber = "ABC"
			};

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var newPackage = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new concrete package", 1, packageJob.Packages.Count);
			AssertEquals("ABC", newPackage.KP_PackageID);
			AssertEquals(1, newPackage.KP_PackageQty);
			AssertEquals(Constants.PkgUnit.Package, newPackage.KP_F3_NKPackType);
			AssertEquals(@"Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...", Logger.Logs);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_LoosePackageID_ParentJobSupportConcreteLoosePackage_ExistingPackage

		public void TestPkgPackageDataObjectReader_LoosePackageID_ParentJobSupportConcreteLoosePackage_ExistingPackage()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportConcreteLoosePackage);

			var parentJob = Factory.New<DummyWithPackingSupportConcreteLoosePackage>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			var package1 = packageJob.Packages.AddNew(PkgUnit.Unit, "ABC");

			var packingLine = new PackingLine
			{
				PackQty = 1,
				ReferenceNumber = "ABC"
			};

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages, targetPackage: package1);
			var newPackage = reader.ReadIntoBusinessObject();
			AssertEquals("No Package Extensions must be created.", 0, newPackage.PackageExtensions.Count);
			AssertEquals("Should not create a new package", package1.PK, newPackage.PK);
			AssertEquals("ABC", newPackage.KP_PackageID);
			AssertEquals(1, newPackage.KP_PackageQty);
			AssertEquals("PackType should not be updated", PkgUnit.Unit, newPackage.KP_F3_NKPackType);
			AssertEquals(@"Information - Successfully loaded matching PkgPackage.
Error - Cannot populate PkgPackage because:
There is a Package with ID 'ABC' in this Package Job, the new Loose Package ID will be ignored.", Logger.Logs);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_CreatePackageExtension

		public void TestPkgPackageDataObjectReader_CreatePackageExtension_WhenPackageJobSupportsPackageExtensions()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsPackageExtensions);
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var packLine = new PackingLine { PackType = new PackageType { Code = PkgUnit.Unit }, PackQty = 1, ReferenceNumber = "ULDNumber" };
			var packageReader = new PkgPackageDataObjectReader(packLine, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs, shouldCreatePackageExtension: true);
			var package = packageReader.ReadIntoBusinessObject();
			var packageExtension = package.PackageExtensions.Single();

			AssertEquals(parentJob.PK, packageExtension.KPN_ParentID);
			AssertEquals(parentJob.TablePrefix, packageExtension.KPN_ParentTableCode);
			AssertEquals(PkgUnit.Unit, package.KP_F3_NKPackType);
			AssertEquals("ULDNumber", package.KP_PackageID);
		}

		public void TestPkgPackageDataObjectReader_WhenPackageJobSupportsPackageExtensions_ShouldNotCreatePackageExtensionIsFalse()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsPackageExtensions);
			var parentJob = Factory.New<DummyWithPackingSupportsPackageExtensions>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var packLine = new PackingLine { PackType = new PackageType { Code = PkgUnit.Unit }, PackQty = 1, ReferenceNumber = "ULDNumber" };
			var packageReader = new PkgPackageDataObjectReader(packLine, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs, shouldCreatePackageExtension: false);
			var package = packageReader.ReadIntoBusinessObject();
			AssertEquals("Since shouldCreatePackageExtension is false no package extensions must be created.", 0, package.PackageExtensions.Count);
			AssertEquals(PkgUnit.Unit, package.KP_F3_NKPackType);
			AssertEquals("ULDNumber", package.KP_PackageID);

			Factory.SaveForTesting();
			var newFactory = new UniversalObjectFactory();
			var packageJobInNewFactory = newFactory.BOFactory.Load<PkgPackageJob>(packageJob.PK);
			var packageReaderInNewFactory = new PkgPackageDataObjectReader(packLine, Logger, newFactory, packageJobInNewFactory, packageJobInNewFactory.Packages, ImportOption.MatchOnPackageIDs, shouldCreatePackageExtension: false);
			var packageInNewFactory = packageReaderInNewFactory.ReadIntoBusinessObject();
			AssertEquals("Since shouldCreatePackageExtension is false no package extensions must be created when resending instructions.", 0, packageInNewFactory.PackageExtensions.Count);
			AssertEquals(PkgUnit.Unit, packageInNewFactory.KP_F3_NKPackType);
			AssertEquals("ULDNumber", packageInNewFactory.KP_PackageID);
		}

		public void TestPkgPackageDataObjectReader_WhenPackageJobDoesNotSupportsPackageExtensions()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			var parentJob = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var packLine = new PackingLine { PackType = new PackageType { Code = PkgUnit.Unit }, PackQty = 1, ReferenceNumber = "ULDNumber" };
			var packageReader = new PkgPackageDataObjectReader(packLine, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs, shouldCreatePackageExtension: true);
			var package = packageReader.ReadIntoBusinessObject();
			AssertEquals("Since DummyWithPacking does not support Package Extensions, no extensions must be created.", 0, package.PackageExtensions.Count);
			AssertEquals(PkgUnit.Unit, package.KP_F3_NKPackType);
			AssertEquals("ULDNumber", package.KP_PackageID);

			Factory.SaveForTesting();
			var newFactory = new UniversalObjectFactory();
			var packageJobInNewFactory = newFactory.BOFactory.Load<PkgPackageJob>(packageJob.PK);
			var packageReaderInNewFactory = new PkgPackageDataObjectReader(packLine, Logger, newFactory, packageJobInNewFactory, packageJobInNewFactory.Packages, ImportOption.MatchOnPackageIDs, shouldCreatePackageExtension: true);
			var packageInNewFactory = packageReaderInNewFactory.ReadIntoBusinessObject();
			AssertEquals("Since DummyWithPacking does not support Package Extensions, no package extensions must be created when resending instructions.", 0, packageInNewFactory.PackageExtensions.Count);
			AssertEquals(PkgUnit.Unit, packageInNewFactory.KP_F3_NKPackType);
			AssertEquals("ULDNumber", packageInNewFactory.KP_PackageID);
		}

		#endregion

		#region TestPkgPackageDataObjectReader_ImportInnersAsNonTrackableItem

		public void TestPkgPackageDataObjectReader_ImportInnersAsNonTrackableItem()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var package1 = packageJob.Packages.AddNew(PackType.Freight.PLT, "P1");

			package1.KP_Weight = 1;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "P1"
			};
			var inner1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				ReferenceNumber = "P1Inner",
				Weight = 10
			};
			var inner2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 5,
				Weight = 20
			};
			packingLine.SetPackingLineCollection(() => new List<PackingLine>() { inner1, inner2 });

			AssertEquals("Precondition - package job should only have 1 package.", 1, packageJob.GetAllPackagesOnJob().Length);

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages, ImportOption.ImportInnersAsNonTrackableItem, targetPackage: package1);
			var newPackage = reader.ReadIntoBusinessObject();
			var inner1AfterRead = newPackage.Packages.Single(p => p.KP_PackageQty == 1);
			var inner2AfterRead = newPackage.Packages.Single(p => p.KP_PackageQty == 5);

			AssertContainsExactElementsInAnyOrder("Package job should have the same packages.", new PkgPackage[] { package1, inner1AfterRead, inner2AfterRead }, packageJob.GetAllPackagesOnJob());
			AssertEquals("Weight should have been updated for matched packages.", 10m, inner1AfterRead.KP_Weight);
			AssertEquals("Weight should have been updated for matched packages.", 20m, inner2AfterRead.KP_Weight);
		}

		#endregion

		public void TestIsCheckedWeighedCubed_WhenIsCheckedWeighedCubed_IsTrue_ShouldKP_IsCheckedWeighedCubed_IsTrue()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.IsCheckedWeighedCubed = true;

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var package = reader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_IsCheckedWeighedCubed);
		}

		public void TestIsDamaged_WhenIsDamaged_IsTrue_ShouldKP_IsDamaged_IsTrue()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.IsDamaged = true;

			var reader = new PkgPackageDataObjectReader(packingLine, Logger, Factory, packageJob, packageJob.Packages);
			var package = reader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_IsDamaged);
		}

		#region TestLogs

		#region TestLogForPackagesMeasurementUpdate

		public void TestLogForPackagesMeasurementUpdate_WithNoPackageID()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			packageJob.Packages.AddNew("PLT", "");
			Factory.SaveForTesting();

			var packageData = new PackingLine { ReferenceNumber = "", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Length = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			packageReader.ReadIntoBusinessObject();
			AssertNotContains("Package with ID has updated Length: From 0 to 150.", logger.Logs);
		}

		public void TestLogForPackagesMeasurementUpdate_LengthUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.Length = 150m, "Package with ID ABC has updated Length: From 0 to 150.");
		}

		public void TestLogForPackagesMeasurementUpdate_HeightUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.Height = 150m, "Package with ID ABC has updated Height: From 0 to 150.");
		}

		public void TestLogForPackagesMeasurementUpdate_WidthUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.Width = 150m, "Package with ID ABC has updated Width: From 0 to 150.");
		}

		public void TestLogForPackagesMeasurementUpdate_DimensionUnitUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.LengthUnit = new UnitOfLength { Code = "KM" }, "Package with ID ABC has updated Dimension Unit: From M to KM.");
		}

		public void TestLogForPackagesMeasurementUpdate_WeightUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.Weight = 150m, "Package with ID ABC has updated Weight: From 0 to 150.");
		}

		public void TestLogForPackagesMeasurementUpdate_WeightUnitUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.WeightUnit = new UnitOfWeight { Code = "T" }, "Package with ID ABC has updated Weight Unit: From KG to T.");
		}

		public void TestLogForPackagesMeasurementUpdate_VolumeUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.Volume = 150m, "Package with ID ABC has updated Volume: From 0 to 150.");
		}

		public void TestLogForPackagesMeasurementUpdate_VolumeUnitUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.VolumeUnit = new UnitOfVolume { Code = "CF" }, "Package with ID ABC has updated Volume Unit: From M3 to CF.");
		}

		public void TestLogForPackagesMeasurementUpdate_NoMeasurementUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => a.PackType = new PackageType { Code = "BAG" }, "Package with ID ABC did not update weight, volume or dimension.");
		}

		public void TestLogForPackagesMeasurementUpdate_MultiMeasurementUpdate()
		{
			TestLogForPackagesMeasurementUpdateCore(a => { a.Length = 100m; a.Weight = 50.5m; a.VolumeUnit = new UnitOfVolume { Code = "GA" }; }, "Package with ID ABC has updated Length: From 0 to 100, Weight: From 0 to 50.5, Volume Unit: From M3 to GA.");
		}

		void TestLogForPackagesMeasurementUpdateCore(Action<PackingLine> action, string log)
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			packageJob.Packages.AddNew("PLT", "ABC");
			Factory.SaveForTesting();

			var packageData = new PackingLine { ReferenceNumber = "ABC", PackQty = 1, PackType = new PackageType { Code = "PLT" } };
			action(packageData);
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs);
			packageReader.ReadIntoBusinessObject();
			AssertContains(log, logger.Logs);
		}

		#endregion

		#region TestLogForBookedPackagesMeasurementUpdate

		public void TestLogForBookedPackagesMeasurementUpdate_WithNoPackageID()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsImportingBookedDimensions);
			var parentJob = Factory.New<DummyWithPackingSupportsImportingBookedDimensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			Factory.SaveForTesting();
			var package = pkgPackageJob.Packages.AddNew("PLT", "ABC");
			package.BookedDimensions.KPB_PackageQty = 2;
			Factory.SaveForTesting();

			var packageData = new PackingLine { ReferenceNumber = "", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Length = 150m };
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, pkgPackageJob, pkgPackageJob.Packages, ImportOption.MatchOnPackageIDs);
			packageReader.ReadIntoBusinessObject();
			AssertNotContains("Booked Package Detail with ID has updated Length: From 0 to 150.", logger.Logs);
		}

		public void TestLogForBookedPackagesMeasurementUpdate_LengthUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.Length = 150m, "Booked Package Detail with ID ABC has updated Length: From 0 to 150.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_HeightUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.Height = 150m, "Booked Package Detail with ID ABC has updated Height: From 0 to 150.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_WidthUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.Width = 150m, "Booked Package Detail with ID ABC has updated Width: From 0 to 150.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_DimensionUnitUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.LengthUnit = new UnitOfLength { Code = "KM" }, "Booked Package Detail with ID ABC has updated Dimension Unit: From M to KM.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_WeightUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.Weight = 150m, "Booked Package Detail with ID ABC has updated Weight: From 0 to 150.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_WeightUnitUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.WeightUnit = new UnitOfWeight { Code = "T" }, "Booked Package Detail with ID ABC has updated Weight Unit: From KG to T.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_VolumeUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.Volume = 150m, "Booked Package Detail with ID ABC has updated Volume: From 0 to 150.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_VolumeUnitUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.VolumeUnit = new UnitOfVolume { Code = "CF" }, "Booked Package Detail with ID ABC has updated Volume Unit: From M3 to CF.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_NoMeasurementUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => a.PackType = new PackageType { Code = "BAG" }, "Booked Package Detail with ID ABC did not update weight, volume or dimension.");
		}

		public void TestLogForBookedPackagesMeasurementUpdate_MultiMeasurementUpdate()
		{
			TestLogForBookedPackagesMeasurementUpdateCore(a => { a.Length = 100m; a.Weight = 50.5m; a.VolumeUnit = new UnitOfVolume { Code = "GA" }; }, "Booked Package Detail with ID ABC has updated Length: From 0 to 100, Weight: From 0 to 50.5, Volume Unit: From M3 to GA.");
		}

		void TestLogForBookedPackagesMeasurementUpdateCore(Action<PackingLine> action, string log)
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsImportingBookedDimensions);
			var parentJob = Factory.New<DummyWithPackingSupportsImportingBookedDimensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			Factory.SaveForTesting();
			var package = pkgPackageJob.Packages.AddNew("PLT", "ABC");
			package.BookedDimensions.KPB_PackageQty = 2;
			Factory.SaveForTesting();

			var packageData = new PackingLine { ReferenceNumber = "ABC", PackQty = 1, PackType = new PackageType { Code = "PLT" } };
			action(packageData);
			var packageReader = new PkgPackageDataObjectReader(packageData, Logger, Factory, pkgPackageJob, pkgPackageJob.Packages, ImportOption.MatchOnPackageIDs);
			packageReader.ReadIntoBusinessObject();
			AssertContains(log, logger.Logs);
		}

		#endregion

		#endregion

		#region Logger

		TestErrorLogger Logger
		{
			get { return logger ?? (logger = new TestErrorLogger()); }
		}

		TestErrorLogger logger;

		#endregion

		#region Implementation

		#region CreatePackingLine

		public PackingLine CreatePackingLine(string referenceNumber, string packingLineID, string packType, decimal? weight = 0m, decimal length = 0m, decimal width = 0m, decimal height = 0m, string weightUQ = "KG", string dimUQ = "CM", int packQty = 1, string marksAndNumbers = "", string previousPackingLineID = "")
		{
			return new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = referenceNumber,
				PackingLineID = packingLineID,
				PackType = new PackageType() { Code = packType, Description = packType },
				Weight = weight,
				Length = length,
				Width = width,
				Height = height,
				WeightUnit = new UnitOfWeight() { Code = weightUQ },
				LengthUnit = new UnitOfLength() { Code = dimUQ },
				MarksAndNos = marksAndNumbers,
				PackQty = packQty,
				PreviousPackingLineID = previousPackingLineID,
			};
		}

		#endregion

		#endregion
	}
}
