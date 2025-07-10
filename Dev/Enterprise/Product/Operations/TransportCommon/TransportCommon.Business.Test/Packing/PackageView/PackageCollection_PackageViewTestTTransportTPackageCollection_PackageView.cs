using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class PackageCollection_PackageViewTest<TTransport, TPackageCollection_PackageView> : NonPersistentBusinessObjectCollectionTestCase<TPackageCollection_PackageView>
			where TTransport : DtbTransport
			where TPackageCollection_PackageView : PackageCollection_PackageView
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			var transport = GetNewTransportBizO();
			var collection = GetPackageCollection_PackageView(transport);
			AssertEquals("Don't allow the user to add new", false, collection.AllowNew);
		}

		#endregion

		#region TestCreateNonPersistentBusinessObject

		public void TestCreateNonPersistentBusinessObject()
		{
			var transport = GetNewTransportBizO();
			var collection = GetPackageCollection_PackageView(transport);

			var newPackage_PackageView = collection.AddNew();
			AssertNotNull("This will be called when binding on a grid, shouldn't be null, just empty", newPackage_PackageView);
			AssertNull("Empty, so warpped package should be null", newPackage_PackageView.Package);
			AssertNull("Empty, so warpped booking should be null", newPackage_PackageView.Transport);
		}

		#endregion

		#region TestInitialise

		#region TestInitialise_Twice

		[ExpectExceptionMessage(typeof(InvalidOperationException), "PackageInPackageViewCollection has already been initialied.")]
		public void TestInitialise_Twice()
		{
			var transport = GetNewTransportBizO();
			var collection = GetPackageCollection_PackageView(transport);
			collection.Initialise();
			collection.Initialise();
		}

		#endregion

		#region TestInitialise_WithoutInitialising

		public void TestInitialise_WithoutInitialising()
		{
			var transport = GetNewTransportBizO();
			var collection = GetPackageCollection_PackageView(transport);
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertEquals("PackageInPackageViewCollection wasn't initialised, so hasn't hooked onto the collection", 0, Collection.Count);
		}

		#endregion

		#region TestInitialise_InitialiseBefore

		public void TestInitialise_InitialiseBefore()
		{
			var transport = GetNewTransportBizO();
			var packages_PackageView = GetPackageCollection_PackageView(transport);
			packages_PackageView.Initialise();
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;

			var wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<Package_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("PackageInPackageViewCollection was initialised, so should have new package", new PkgPackage[] { package }, wrappedPackages);
		}

		#endregion

		#region TestInitialise_InitialiseAfter

		public void TestInitialise_InitialiseAfter()
		{
			var transport = GetNewTransportBizO();
			var packages_PackageView = GetPackageCollection_PackageView(transport);
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			packages_PackageView.Initialise();
			var wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<Package_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("PackageInPackageViewCollection was initialised, so should have new package", new PkgPackage[] { package }, wrappedPackages);
		}

		#endregion

		#endregion

		#region TestAddRemove

		public void TestAddRemove()
		{
			var transport = GetNewTransportBizO();
			var packages_PackageView = GetPackageCollection_PackageView(transport);
			packages_PackageView.Initialise();
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var package = Factory.New<PkgPackage>();
			packageDivot.KD_KP_Package = package.PK;

			var wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<Package_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("Added", new PkgPackage[] { package }, wrappedPackages);

			packageDivot.KD_KP_Package = ZGuid.Empty;
			wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<Package_PackageView>(), p => p.Package);
			AssertEquals("Removed", 0, packages_PackageView.Count);

			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			wrappedPackages = Array.ConvertAll(packages_PackageView.ToArray<Package_PackageView>(), p => p.Package);
			AssertContainsExactElementsInAnyOrder("Added for new instruction", new PkgPackage[] { package2 }, wrappedPackages);
		}

		#endregion

		#region TestFind

		public void TestFind()
		{
			var transport = GetNewTransportBizO();
			var packages_PackageView = GetPackageCollection_PackageView(transport);
			packages_PackageView.Initialise();
			AssertEquals("Precondition", 0, packages_PackageView.Count);

			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var packageDivot1 = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var packageDivot2 = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			packageDivot1.KD_KP_Package = package1.PK;
			packageDivot2.KD_KP_Package = package2.PK;
			AssertEquals("Should now have 2 package views", 2, packages_PackageView.Count);

			var package1View = packages_PackageView.Find(package1);
			var package2View = packages_PackageView.Find(package2);
			AssertEquals("Found view is package1", package1, package1View.Package);
			AssertEquals("Found view is package2", package2, package2View.Package);
			AssertContainsExactElementsInAnyOrder("Ensure same instances", new Package_PackageView[] { package1View, package2View }, packages_PackageView);
		}

		#endregion

		#region Implementation

		protected override TPackageCollection_PackageView GetCollectionToTest()
		{
			return GetPackageCollection_PackageView(Transport);
		}

		protected TTransport Transport
		{
			get { return transport ?? (transport = GetNewTransportBizO()); }
		}
		TTransport transport;

		protected abstract TPackageCollection_PackageView GetPackageCollection_PackageView(TTransport transport);
		protected abstract TTransport GetNewTransportBizO();

		#endregion
	}
}
