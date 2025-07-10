using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingPackageCollection_PackageView))]
	sealed class DtbBookingPackageCollection_PackageViewTest : NonPersistentBusinessObjectCollectionTestCase<DtbBookingPackageCollection_PackageView>
	{
		public void TestRefreshCollectionWhileCollectionListChangedEventsDelayed()
		{
			var booking = GetNewBookingBizO();
			booking.PackageJob.FillWithValidTestData();
			var instructionPic1 = booking.Instructions.AddNew();
			instructionPic1.KN_InstructionType = "PIC";
			var packageCnt1 = Helper.CreatePackage("PKG1", 1, "CNT");
			packageCnt1.Container.FillWithValidTestData();
			packageCnt1.KP_KJ_ParentPackageJob = booking.PackageJob.PK;
			var divot1 = Helper.CreatePackageDivot(instructionPic1, packageCnt1, 1);
			var packagePlt2 = Helper.CreatePackage("PKG2", 1, "PLT");
			var divot2 = Helper.CreatePackageDivot(instructionPic1, packagePlt2, 1);
			packagePlt2.KP_KJ_ParentPackageJob = booking.PackageJob.PK;
			Factory.Save();

			var assignToEnsurePackagesPackageViewIsInstantiated = booking.Packages_PackageView_ForTest;
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				AssertEquals("PackageView should have two packages still", 2, booking.Packages_PackageView_ForTest.Count);
				divot2.Delete();
				packagePlt2.Delete();
				AssertEquals("PackageView should have two packages still as we have not yet refreshed - CountChanged is currently paused", 2, booking.Packages_PackageView_ForTest.Count);
				booking.Packages_PackageView_ForTest.RefreshCollection();
				AssertEquals("PackageView should have one package after removing one and refreshing", 1, booking.Packages_PackageView_ForTest.Count);
			}
		}

		public void TestAllowNew()
		{
			var booking = GetNewBookingBizO();
			var collection = GetPackageCollection_PackageView(booking);
			AssertEquals("Don't allow the user to add new", false, collection.AllowNew);
		}

		public void TestCreateNonPersistentBusinessObject()
		{
			var booking = GetNewBookingBizO();
			var collection = GetPackageCollection_PackageView(booking);

			var newPackage_PackageView = collection.AddNew();
			AssertNotNull("This will be called when binding on a grid, shouldn't be null, just empty", newPackage_PackageView);
			AssertNull("Empty, so warpped package should be null", newPackage_PackageView.Package);
			AssertNull("Empty, so warpped booking should be null", newPackage_PackageView.Booking);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "PackageInPackageViewCollection has already been initialied.")]
		public void TestInitialise_Twice()
		{
			var booking = GetNewBookingBizO();
			var collection = GetPackageCollection_PackageView(booking);
			collection.Initialise();
			collection.Initialise();
		}

		public void TestInitialise_WithoutInitialising()
		{
			var booking = GetNewBookingBizO();
			var collection = GetPackageCollection_PackageView(booking);
			var instruction = booking.Instructions.AddNew();
			var packageDivot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertEquals("PackageInPackageViewCollection wasn't initialised, so hasn't hooked onto the collection", 0, Collection.Count);
		}

		public void TestInitialise_InitialiseBefore()
		{
			var booking = GetNewBookingBizO();
			var packages_PackageView = GetPackageCollection_PackageView(booking);
			packages_PackageView.Initialise();
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			var instruction = booking.Instructions.AddNew();
			var packageDivot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;

			var wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<DtbBookingPackage_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("PackageInPackageViewCollection was initialised, so should have new package", new PkgPackage[] { package }, wrappedPackages);
		}

		public void TestInitialise_InitialiseAfter()
		{
			var booking = GetNewBookingBizO();
			var packages_PackageView = GetPackageCollection_PackageView(booking);
			var instruction = booking.Instructions.AddNew();
			var packageDivot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			packages_PackageView.Initialise();
			var wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<DtbBookingPackage_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("PackageInPackageViewCollection was initialised, so should have new package", new PkgPackage[] { package }, wrappedPackages);
		}

		public void TestAddRemove()
		{
			var booking = GetNewBookingBizO();
			var packages_PackageView = GetPackageCollection_PackageView(booking);
			packages_PackageView.Initialise();
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			var instruction = booking.Instructions.AddNew();
			var packageDivot = instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;

			var wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<DtbBookingPackage_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("Added", new PkgPackage[] { package }, wrappedPackages);

			packageDivot.KD_KP_Package = ZGuid.Empty;
			wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<DtbBookingPackage_PackageView>(), p => p.Package);
			AssertEquals("Removed", 0, packages_PackageView.Count);

			var instruction2 = booking.Instructions.AddNew();
			var packageDivot2 = instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<DtbBookingPackage_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("Added for new instruction", new PkgPackage[] { package2 }, wrappedPackages);
		}

		public void TestFind()
		{
			var booking = GetNewBookingBizO();
			var packages_PackageView = GetPackageCollection_PackageView(booking);
			packages_PackageView.Initialise();
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			var instruction = booking.Instructions.AddNew();
			var packageDivot1 = instruction.PackageDivots.AddNew();
			var packageDivot2 = instruction.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			packageDivot1.KD_KP_Package = package1.PK;
			packageDivot2.KD_KP_Package = package2.PK;
			AssertEquals("Should now have 2 package views", 2, packages_PackageView.Count);

			var package1View = packages_PackageView.Find(package1);
			var package2View = packages_PackageView.Find(package2);
			AssertEquals("Found view is package1", package1, package1View.Package);
			AssertEquals("Found view is package2", package2, package2View.Package);
			AssertContainsExactElementsInAnyOrder("Ensure same instances", new DtbBookingPackage_PackageView[] { package1View, package2View }, packages_PackageView);
		}

		protected override DtbBookingPackageCollection_PackageView GetCollectionToTest()
		{
			return GetPackageCollection_PackageView(Booking);
		}

		DtbBooking Booking
		{
			get { return booking ?? (booking = GetNewBookingBizO()); }
		}
		DtbBooking booking;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DtbBookingPackage_PackageView(Factory.New<PkgPackage>(), Booking);
		}

		DtbBooking GetNewBookingBizO()
		{
			return Helper.CreateBooking();
		}

		DtbBookingPackageCollection_PackageView GetPackageCollection_PackageView(DtbBooking booking)
		{
			return new DtbBookingPackageCollection_PackageView(booking);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(DtbBookingPackageCollection_PackageView);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
