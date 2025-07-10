using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(MasterBookingPkgPackageCollection))]
	public class MasterBookingPkgPackageCollectionTest : PkgPackageCollectionTest
	{
		public void TestRelationshipFilterIsEmpty()
		{
			var packageJob = Factory.NewWithValidTestData<DtbBookingConsolidationPkgPackageJob>();
			var collection = new MasterBookingPkgPackageCollection(packageJob);

			AssertEquals("There should be no relationship filter for the collection.", string.Empty, collection.Relationship.RelationshipFilter.LiteralTextSqlFormatted);
		}

		public void TestGetsSubPackagesForMasterConsolidationParent()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation1 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation1.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation1.KB_MasterBookingVersion = 1;
			var subBooking1 = subConsolidation1.Bookings.AddNew();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subInstruction1 = subBooking1.Instructions.AddNew();
			subInstruction1.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction1.KN_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subBooking1Container1 = Helper.CreatePackageContainer("CONT1");
			var subBooking1Container1InnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1Container2 = Helper.CreatePackageContainer("CONT2");
			subBooking1.PackageJob.Packages.Add(subBooking1Container1);
			subBooking1Container1.Packages.Add(subBooking1Container1InnerLoosePackage);
			subBooking1.PackageJob.Packages.Add(subBooking1Container2);

			var subConsolidation2 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation2.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation2.KB_MasterBookingVersion = 1;
			var subBooking2 = subConsolidation2.Bookings.AddNew();
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_MasterBookingVersion = subConsolidation2.KB_MasterBookingVersion;
			var subInstruction2 = subBooking2.Instructions.AddNew();
			subInstruction2.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction2.KN_MasterBookingVersion = subConsolidation2.KB_MasterBookingVersion;

			var subBooking2Container1 = Helper.CreatePackageContainer("CONT3");
			subBooking2.PackageJob.Packages.Add(subBooking2Container1);

			var unrelatedConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var unrelatedBooking = unrelatedConsolidation.Bookings.AddNew();
			var unrelatedInstruction = unrelatedBooking.Instructions.AddNew();

			var unrelatedConsolidationContainer1 = Helper.CreatePackageContainer("CONT4");
			unrelatedBooking.PackageJob.Packages.Add(unrelatedConsolidationContainer1);

			var assignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking1Container1.PK, packageQty: subBooking1Container1.KP_PackageQty),
				(packagePK: subBooking1Container1InnerLoosePackage.PK, packageQty: subBooking1Container1InnerLoosePackage.KP_PackageQty),
				// intentionally missing subBooking1Container2, to check unassigned packages are not included
				(packagePK: subBooking2Container1.PK, packageQty: subBooking2Container1.KP_PackageQty),
			};

			var unrelatedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: unrelatedConsolidationContainer1.PK, packageQty: unrelatedConsolidationContainer1.KP_PackageQty),
			};

			AssignDivotsToInstructions(masterInstruction, assignedPackages);
			AssignDivotsToInstructions(unrelatedInstruction, unrelatedPackages);

			Factory.Save();

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);
			AssertContainsExactElementsInAnyOrder("Should only have found packages assigned to instructions of the master consolidation.", DisplayTextProvider, new List<PkgPackage>() { subBooking1Container1, subBooking2Container1 }, masterConsolidation.PackageJob.Packages);

			AssertCollectionNotContains("Inner package should not be included in PackageJob.Packages", subBooking1Container1InnerLoosePackage, masterConsolidation.PackageJob.Packages);
		}

		public void TestGetsSubPackagesForMasterConsolidationParent_WhenBookingDetached()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation1 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation1.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation1.KB_MasterBookingVersion = 1;
			var subBooking1 = subConsolidation1.Bookings.AddNew();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subInstruction1 = subBooking1.Instructions.AddNew();
			subInstruction1.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction1.KN_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;

			var subBooking1Container1 = Helper.CreatePackageContainer("CONT1");
			subBooking1.PackageJob.Packages.Add(subBooking1Container1);

			var subConsolidation2 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation2.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation2.KB_MasterBookingVersion = 1;
			var subBooking2 = subConsolidation2.Bookings.AddNew();
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_MasterBookingVersion = subConsolidation2.KB_MasterBookingVersion;
			var subInstruction2 = subBooking2.Instructions.AddNew();
			subInstruction2.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction2.KN_MasterBookingVersion = subConsolidation2.KB_MasterBookingVersion;

			var subBooking2Container1 = Helper.CreatePackageContainer("CONT2");
			subBooking2.PackageJob.Packages.Add(subBooking2Container1);

			var assignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking1Container1.PK, packageQty: subBooking1Container1.KP_PackageQty),
				(packagePK: subBooking2Container1.PK, packageQty: subBooking2Container1.KP_PackageQty),
			};

			AssignDivotsToInstructions(masterInstruction, assignedPackages);

			Factory.Save();

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), masterConsolidation.PackageJob.GetType());

			AssertContainsExactElementsInAnyOrder("Precondition: Should have found all packages assigned to instructions in the master booking.", DisplayTextProvider, new List<PkgPackage>() { subBooking1Container1, subBooking2Container1 }, masterConsolidation.PackageJob.Packages);

			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).ShouldReloadPackagesCollection = true;
			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubConsolidationPKsToExclude.Add(subConsolidation2.PK);

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);

			AssertContainsExactElementsInAnyOrder("Should only have found packages attached to subs of the master consolidation that are not detached bookings.", DisplayTextProvider, new List<PkgPackage>() { subBooking1Container1 }, masterConsolidation.PackageJob.Packages);
		}

		public void TestGetsSubPackagesForMasterConsolidationParent_WhenBookingAttached()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation1 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation1.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation1.KB_MasterBookingVersion = 1;
			var subBooking1 = subConsolidation1.Bookings.AddNew();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subInstruction1 = subBooking1.Instructions.AddNew();
			subInstruction1.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction1.KN_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subBooking1Container1 = Helper.CreatePackageContainer("CONT1");
			var subBooking1Container1InnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1Container2 = Helper.CreatePackageContainer("CONT2");
			subBooking1.PackageJob.Packages.Add(subBooking1Container1);
			subBooking1Container1.Packages.Add(subBooking1Container1InnerLoosePackage);
			subBooking1.PackageJob.Packages.Add(subBooking1Container2);

			var subConsolidation2 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var subBooking2 = subConsolidation2.Bookings.AddNew();
			var subInstruction2 = subBooking2.Instructions.AddNew();

			var subBooking2Container1 = Helper.CreatePackageContainer("CONT3");
			var subBooking2Container1InnerLoosePackage = Helper.CreatePackage("CONT3_IN");
			var subBooking2Container2 = Helper.CreatePackageContainer("CONT4");
			subBooking2.PackageJob.Packages.Add(subBooking2Container1);
			subBooking2Container1.Packages.Add(subBooking2Container1InnerLoosePackage);
			subBooking2.PackageJob.Packages.Add(subBooking2Container2);

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking1Container1.PK, packageQty: subBooking1Container1.KP_PackageQty),
				(packagePK: subBooking1Container1InnerLoosePackage.PK, packageQty: subBooking1Container1InnerLoosePackage.KP_PackageQty),
				// intentionally missing subBooking1Container2, to check unassigned packages are not included
			};

			var subInstruction2AssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subBooking2Container1.PK, packageQty: subBooking2Container1.KP_PackageQty),
				(packagePK: subBooking2Container1InnerLoosePackage.PK, packageQty: subBooking2Container1InnerLoosePackage.KP_PackageQty),
				// intentionally missing subBooking2Container2, to check unassigned packages are not included
			};

			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);
			AssignDivotsToInstructions(subInstruction2, subInstruction2AssignedPackages);

			Factory.Save();

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), masterConsolidation.PackageJob.GetType());

			AssertContainsExactElementsInAnyOrder("Precondition: Should have found all packages assigned to instructions in the master booking.", DisplayTextProvider, new List<PkgPackage>() { subBooking1Container1 }, masterConsolidation.PackageJob.Packages);

			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).ShouldReloadPackagesCollection = true;
			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubBookingPKsToInclude.Add(subBooking2.PK);

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);

			AssertContainsExactElementsInAnyOrder("Should have found all packages assigned to instructions in the master booking, plus those assigned to instructions of the newly attached sub booking.", DisplayTextProvider, new List<PkgPackage>() { subBooking1Container1, subBooking2Container1 }, masterConsolidation.PackageJob.Packages);

			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).ShouldReloadPackagesCollection = true;
			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubBookingPKsToInclude.Remove(subBooking2.PK);
			((DtbBookingConsolidationPkgPackageJob)masterConsolidation.PackageJob).SubConsolidationPKsToExclude.Add(subConsolidation2.PK);

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);

			AssertContainsExactElementsInAnyOrder("Should have found all packages assigned to instructions in the master booking, but not those assigned to instructions of the sub booking which was attached then detached.", DisplayTextProvider, new List<PkgPackage>() { subBooking1Container1 }, masterConsolidation.PackageJob.Packages);
		}

		public void TestAllPackagesAndDangerousGoodsAreReadOnly()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidationPackage1 = subConsolidation.PackageJob.Packages.AddNew();
			var subConsolidationPackage1DangerousGood1 = subConsolidationPackage1.UNDGs.AddNew();
			var subConsolidationPackage1DangerousGood2 = subConsolidationPackage1.UNDGs.AddNew();

			var subConsolidationPackage2 = subConsolidation.PackageJob.Packages.AddNew();
			var subConsolidationPackage2DangerousGood1 = subConsolidationPackage1.UNDGs.AddNew();
			var subConsolidationPackage2DangerousGood2 = subConsolidationPackage1.UNDGs.AddNew();

			Factory.Save();

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);
			Assert("All Packages should be read only.", masterConsolidation.PackageJob.Packages.All(p => p.ReadOnly));

			var dangerousGoodsCollections = masterConsolidation.PackageJob.Packages.Select(p => p.UNDGs);
			Assert("All Packages' Dangerous Goods Collections should be read only.", dangerousGoodsCollections.All(c => c.ReadOnly));

			foreach (var collection in dangerousGoodsCollections)
			{
				Assert("All Packages' Dangerous Goods should be read only.", collection.All(d => d.ReadOnly));
			}
		}

		public void TestAllInnerPackagesAreReadOnly()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidationPackage1 = subConsolidation.PackageJob.Packages.AddNew();
			var subConsolidationInnerPackage1 = subConsolidationPackage1.Packages.AddNew();
			subConsolidationInnerPackage1.Packages.AddNew();

			var subConsolidationPackage2 = subConsolidation.PackageJob.Packages.AddNew();
			var subConsolidationInnerPackage2 = subConsolidationPackage2.Packages.AddNew();
			subConsolidationInnerPackage2.Packages.AddNew();

			Factory.Save();

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);

			foreach (var package in masterConsolidation.PackageJob.Packages)
			{
				AssertPackageAndAllItsChildrenAreReadOnly(package);
			}
		}

		public void TestAllContainersAreReadOnly()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidationContainer1 = subConsolidation.PackageJob.Packages.AddNew("CNT", 2);
			subConsolidationContainer1.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			AssertNotNull("Precondition: collection should be the correct type.", masterConsolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);

			foreach (var package in masterConsolidation.PackageJob.Packages)
			{
				AssertPackageAndAllItsChildrenAreReadOnly(package);
			}
		}

		void AssertPackageAndAllItsChildrenAreReadOnly(PkgPackage package)
		{
			AssertEquals("Package " + package.KP_PackageID + " should be read only.", true, package.ReadOnly);
			if (package.Container != null)
			{
				AssertEquals("Container attached to Package " + package.KP_PackageID + " should be read only.", true, package.Container.ReadOnly);
			}

			foreach (var childPackage in package.Packages)
			{
				AssertPackageAndAllItsChildrenAreReadOnly(childPackage);
			}
		}

		void AssignDivotsToInstructions(DtbBookingInstruction instruction, (ZGuid packagePK, ZInt packageQty)[] divotDetails)
		{
			instruction.PackageDivots.DeleteAll();
			foreach (var divotDetail in divotDetails)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = divotDetail.packagePK;
				packageDivot.KD_Quantity = divotDetail.packageQty;
			}
		}

		static string DisplayTextProvider(PkgPackage package) => package.HumanReadableName;

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
