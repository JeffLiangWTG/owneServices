using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingConsolidationPkgPackageJob))]
	public class DtbBookingConsolidationPkgPackageJobTest : PkgPackageJobTest
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<DtbBookingConsolidationPkgPackageJob>();

		public void TestPackages_WhenParentIsMasterConsolidation()
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
			var subConsolidation1Package = subConsolidation1.PackageJob.Packages.AddNew();
			var innerPackage = subConsolidation1Package.Packages.AddNew();

			var subConsolidation2 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation2.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation2.KB_MasterBookingVersion = 1;
			var subConsolidation2Package = subConsolidation2.PackageJob.Packages.AddNew();

			var unrelatedConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			unrelatedConsolidation.PackageJob.Packages.AddNew();

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidation1Package.PK, packageQty: subConsolidation1Package.KP_PackageQty),
				(packagePK: innerPackage.PK, packageQty: innerPackage.KP_PackageQty),
				(packagePK: subConsolidation2Package.PK, packageQty: subConsolidation2Package.KP_PackageQty),
			};

			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), masterConsolidation.PackageJob.GetType());
			AssertContainsExactElementsInAnyOrder("Should only have found packages attached to subs of the master consolidation.", new List<PkgPackage>() { subConsolidation1Package, subConsolidation2Package }, masterConsolidation.PackageJob.Packages);
			AssertCollectionNotContains("Only outer packages should be included", innerPackage, masterConsolidation.PackageJob.Packages);
		}

		public void TestSubBookingPKsToInclude_IsNotNull()
		{
			var bookingPackageJob = Factory.New<DtbBookingConsolidationPkgPackageJob>();
			AssertNotNull("SubBookingPKsToInclude should not be null.", bookingPackageJob.SubBookingPKsToInclude);
		}

		public void TestSubConsolidationPKsToExclude_IsNotNull()
		{
			var bookingPackageJob = Factory.New<DtbBookingConsolidationPkgPackageJob>();
			AssertNotNull("SubConsolidationPKsToExclude should not be null.", bookingPackageJob.SubConsolidationPKsToExclude);
		}

		public void TestSubBookingPKsToInclude_IsEmptiedOnSave()
		{
			var bookingPackageJob = Factory.New<DtbBookingConsolidationPkgPackageJob>();
			bookingPackageJob.SubBookingPKsToInclude.Add(ZGuid.NewZGuid());

			AssertEquals("Precondition: SubBookingPKsToInclude should contain 1 record.", 1, bookingPackageJob.SubBookingPKsToInclude.Count);

			Factory.Save();

			AssertEquals("SubBookingPKsToInclude should now contain no records.", 0, bookingPackageJob.SubBookingPKsToInclude.Count);
		}

		public void TestSubConsolidationPKsToExclude_IsEmptiedOnSave()
		{
			var bookingPackageJob = Factory.New<DtbBookingConsolidationPkgPackageJob>();
			bookingPackageJob.SubConsolidationPKsToExclude.Add(ZGuid.NewZGuid());

			AssertEquals("Precondition: SubConsolidationPKsToExclude should contain 1 record.", 1, bookingPackageJob.SubConsolidationPKsToExclude.Count);

			Factory.Save();

			AssertEquals("SubConsolidationPKsToExclude should now contain no records.", 0, bookingPackageJob.SubConsolidationPKsToExclude.Count);
		}

		public void TestPackages_WhenParentIsNotMasterConsolidation()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var consolidationPackage = consolidation.PackageJob.Packages.AddNew();

			var unrelatedConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			unrelatedConsolidation.PackageJob.Packages.AddNew();

			Factory.Save();

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), consolidation.PackageJob.GetType());
			AssertContainsExactElementsInAnyOrder("Should only have found packages directly attached to the consolidation.", new List<PkgPackage>() { consolidationPackage }, consolidation.PackageJob.Packages);
		}

		public void TestDeleteIsNotSupported()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = consolidation.Bookings.AddNew();
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = consolidation.KB_MasterBookingVersion;

			Factory.Save();

			var packageJob = consolidation.PackageJob;
			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), packageJob.GetType());
			AssertExceptionThrown<NotSupportedException>("Since DtbBookingConsolidationPkgPackageJob can't be saved, it also shouldn't support deletion.", () =>
			{
				packageJob.Delete();
			});
		}

		public void TestPackageJobDoesNotSave_WhenIsMasterConsolidation()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = consolidation.Bookings.AddNew();
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = consolidation.KB_MasterBookingVersion;
			var packageJob = consolidation.PackageJob;

			Factory.Save();

			AssertEquals("Master consolidation package job should not have saved.", false, packageJob.IsInDatabase);
		}

		public void TestHasChangesWhenParentConsolidationIsMaster()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = consolidation.Bookings.AddNew();
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = consolidation.KB_MasterBookingVersion;
			var packageJob = consolidation.PackageJob;

			AssertEquals("HasChanges should be false.", false, packageJob.HasChanges);

			packageJob.HasChanges = true;

			AssertEquals("HasChanges should still be false.", false, packageJob.HasChanges);
		}

		public void TestPackagesReturnsCorrectCollectionTypeWhenConsolidationIsMaster()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = consolidation.Bookings.AddNew();
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = consolidation.KB_MasterBookingVersion;

			Factory.Save();

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), consolidation.PackageJob.GetType());
			AssertNotNull("Collection should be the correct type.", consolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);
		}

		public void TestPackagesReturnsCorrectCollectionTypeWhenConsolidationIsNotMaster()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();

			Factory.Save();

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), consolidation.PackageJob.GetType());
			AssertEquals("Collection should not be a MasterBookingPkgPackageCollection.", null, consolidation.PackageJob.Packages as MasterBookingPkgPackageCollection);
		}

		public void TestContainsPackage_ShouldSearchFromSubPackages()
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

			var subConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = 1;
			var subConsolidationPackage = subConsolidation.PackageJob.Packages.AddNew();
			var innerPackage = subConsolidationPackage.Packages.AddNew();

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidationPackage.PK, packageQty: subConsolidationPackage.KP_PackageQty),
				// Don't assign innerPackage
			};

			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			Assert("Should include top level package", masterConsolidation.PackageJob.ContainsPackage(subConsolidationPackage));
			AssertEquals("Currently can't retrieve inner packages of an outer package when only the outer package has been assigned to an instruction, should be fixed with work item following on from WI00809382", false, masterConsolidation.PackageJob.ContainsPackage(innerPackage));

			masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidationPackage.PK, packageQty: subConsolidationPackage.KP_PackageQty),
				(packagePK: innerPackage.PK, packageQty: innerPackage.KP_PackageQty),
			};

			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();
			Assert("Should include top level package", masterConsolidation.PackageJob.ContainsPackage(subConsolidationPackage));
			Assert("Should include inner package", masterConsolidation.PackageJob.ContainsPackage(innerPackage));
		}

		public void TestGetAllPackagesOnJob_ShouldSearchFromSubPackages()
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

			var subConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = 1;
			var subConsolidationPackage = subConsolidation.PackageJob.Packages.AddNew();
			var innerPackage = subConsolidationPackage.Packages.AddNew();

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidationPackage.PK, packageQty: subConsolidationPackage.KP_PackageQty),
				// Don't assign innerPackage
			};

			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			var allPackages = masterConsolidation.PackageJob.GetAllPackagesOnJob();
			AssertContainsExactElementsInAnyOrder(new[] { subConsolidationPackage }, allPackages);
			AssertCollectionNotContains("Currently can't retrieve inner packages of an outer package when only the outer package has been assigned to an instruction, should be fixed with work item following on from WI00809382", innerPackage, allPackages);

			masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidationPackage.PK, packageQty: subConsolidationPackage.KP_PackageQty),
				(packagePK: innerPackage.PK, packageQty: innerPackage.KP_PackageQty),
			};

			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			allPackages = masterConsolidation.PackageJob.GetAllPackagesOnJob();
			AssertContainsExactElementsInAnyOrder(new[] { subConsolidationPackage, innerPackage }, allPackages);
		}

		public void TestPackageJobIsReadOnlyWhenParentHasAnyAttachedMasterBookings()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var masterBooking = consolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;

			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), consolidation.PackageJob.GetType());
			AssertEquals("Package Job should be read only.", true, consolidation.PackageJob.ReadOnly);
		}

		public void TestPackageJobIsNotReadOnlyWhenParentHasNoAttachedMasterBookings()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			AssertEquals("Precondition: Attached booking should not be a master booking.", false, booking.KM_IsMaster);
			AssertEquals("Precondition: Package job should be the correct type.", typeof(DtbBookingConsolidationPkgPackageJob), consolidation.PackageJob.GetType());
			AssertEquals("Package Job should not be read only.", false, consolidation.PackageJob.ReadOnly);
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
	}
}
