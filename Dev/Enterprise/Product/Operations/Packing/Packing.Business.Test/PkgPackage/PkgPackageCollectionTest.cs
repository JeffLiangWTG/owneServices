using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageCollection))]
	public class PkgPackageCollectionTest : PkgPackageCollectionCommonTest<PkgPackageCollection>
	{
		#region TestConstructorThrowsExceptionIfMasterHasNoPackageJobLink

		public void TestConstructorThrowsExceptionIfMasterHasNoPackageJobLink()
		{
			var dodgyPackage = Factory.New<PkgPackage>();
			AssertExceptionThrown(typeof(ArgumentException), () => new PkgPackageCollection(dodgyPackage));
		}

		#endregion

		#region TestConstructorThrowsNoExceptionIfMasterHasNoPackageJobLinkWhenDeleting

		public void TestConstructorThrowsNoExceptionIfMasterHasNoPackageJobLinkWhenDeleting()
		{
			var dodgyPackage = Factory.New<PkgPackage>();
			AssertNoExceptionThrown(() => dodgyPackage.Delete()); // this will instantiate a PackageCollection to attempt to delete package.Packages
		}

		#endregion

		#region Relationship

		#region TestRelationship_WithPackageJob

		public void TestRelationship_WithPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = new PkgPackageCollection(packageJob);
			Factory.Save(); // ensure save is ok

			AssertEquals(packageJob, collection.Relationship.Master);
		}

		#endregion

		#region TestRelationship_WithPackage

		public void TestRelationship_WithPackage()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			var collection = new PkgPackageCollection(package);
			Factory.Save(); // ensure save is ok

			AssertEquals(package, collection.Relationship.Master);
		}

		#endregion

		#region TestRelationship_Packages

		public void TestRelationship_Packages()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var pallet1 = packageJob.Packages.AddNew("PLT");
			var pallet2 = packageJob.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var carton1a = box1.Packages.AddNew("CTN");
			var carton1b = box1.Packages.AddNew("CTN");

			AssertContainsExactElementsInAnyOrder("PackageJob Packages.", new PkgPackage[] { pallet1, pallet2 }, packageJob.Packages);
			AssertContainsExactElementsInAnyOrder("Pallet1 Packages.", box1, pallet1.Packages);
			AssertContainsExactElementsInAnyOrder("Pallet2 Packages.", Array.Empty<PkgPackage>(), pallet2.Packages);
			AssertContainsExactElementsInAnyOrder("Box1 Packages.", new PkgPackage[] { carton1a, carton1b }, box1.Packages);
			AssertContainsExactElementsInAnyOrder("Carton1a Packages.", Array.Empty<PkgPackage>(), carton1a.Packages);
			AssertContainsExactElementsInAnyOrder("Carton2b Packages.", Array.Empty<PkgPackage>(), carton1b.Packages);
		}

		#endregion

		#endregion

		#region Add/AddNew

		#region TestAdd_SetsPackType

		protected override ZString ExpectedDefaultPackType
		{
			get { return PackingRegistry.Instance.OuterPackageUnit.Value; }
		}

		#endregion

		#region TestAddNewElement_PackTypeIsLastUsedPackType

		public void TestAddNewElement_PackTypeIsLastUsedPackType()
		{
			PackingRegistry.Instance.SetOuterPackageUnitForTest("PLT");
			PackingRegistry.Instance.SetInnerPackageUnitForTest("BOX");

			var packageJob = Factory.New<PkgPackageJob>();
			var collection = new PkgPackageCollection(packageJob);

			// test outers
			var outer1 = collection.AddNew();
			AssertEquals("No initial Outer Pack Type set, should have used the registry default.", "PLT", outer1.KP_F3_NKPackType);

			outer1.KP_F3_NKPackType = "CNT";
			var outer2 = collection.AddNew();
			AssertEquals("Pack Type should be defaulted to the last-used Outer Pack Type.", "CNT", outer2.KP_F3_NKPackType);

			// test inners
			var inner1 = outer1.Packages.AddNew();
			AssertEquals("No initial Inner Pack Type set, should have used the registry default.", "BOX", inner1.KP_F3_NKPackType);

			inner1.KP_F3_NKPackType = "KEG";
			var inner2 = outer2.Packages.AddNew();
			AssertEquals("Pack Type should be defaulted to the last-used Inner Pack Type.", "KEG", inner2.KP_F3_NKPackType);
		}

		#endregion

		#region TestAddNewElement_PackageAlwaysLinksToPackageJob

		public void TestAddNewElement_PackageAlwaysLinksToPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			var collection = new PkgPackageCollection(packageJob);
			var package = collection.AddNew();
			AssertEquals("Top level packages should point to the PackageJob.", packageJob.PK, package.KP_KJ_ParentPackageJob);

			var childCollection = new PkgPackageCollection(package);
			var childPackage = childCollection.AddNew();
			AssertEquals("Child packages should also point to the PackageJob (to improve query performance).", packageJob.PK, childPackage.KP_KJ_ParentPackageJob);
		}

		#endregion

		#region TestAdd_ClearsParentPackageLinkWhenMasterIsPackageJob

		public void TestAdd_ClearsParentPackageLinkWhenMasterIsPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = new PkgPackageCollection(packageJob);
			var pallet = packageJob.Packages.AddNew("PLT");

			var box = pallet.Packages.AddNew("BOX");
			AssertEquals(false, box.KP_KP_ParentPackage.IsEmpty);
			AssertCollectionNotContains("Precondition", box, collection);

			collection.Add(box);
			AssertEquals(true, box.KP_KP_ParentPackage.IsEmpty);
			AssertCollectionContains(box, collection);
		}

		public void TestAddRange_ClearsParentPackageLinkWhenMasterIsPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = new PkgPackageCollection(packageJob);
			var pallet = packageJob.Packages.AddNew("PLT");

			var box = pallet.Packages.AddNew("BOX");
			AssertEquals(false, box.KP_KP_ParentPackage.IsEmpty);
			AssertCollectionNotContains("Precondition", box, collection);

			collection.AddRange(new[] { box });
			AssertEquals(true, box.KP_KP_ParentPackage.IsEmpty);
			AssertCollectionContains(box, collection);
		}

		#endregion

		#region TestAddNewWithPackageTypeAndQty

		public void TestAddNewWithPackageTypeAndQty()
		{
			var package1 = Collection.AddNew("BOX");
			AssertEquals("BOX", package1.KP_F3_NKPackType);
			AssertEquals(1, package1.KP_PackageQty);

			var package2 = Collection.AddNew("KEG", 3);
			AssertEquals("KEG", package2.KP_F3_NKPackType);
			AssertEquals(3, package2.KP_PackageQty);
		}

		#endregion

		#region TestAddNewWithPackTypeAndID

		public void TestAddNewWithPackTypeAndID()
		{
			var package = Collection.AddNew("BOX", "abc");
			AssertEquals("BOX", package.KP_F3_NKPackType);
			AssertEquals("abc", package.KP_PackageID);
		}

		#endregion

		public void TestAddNew_SuspendsNotificationChangedOnCollection()
		{
			var collection = GetCollectionToTest();
			var notificationsChangedHitCount = 0;
			((IBusinessObjectState)collection).NotificationsChanged += (sender, e) => notificationsChangedHitCount++;

			var package = collection.AddNew("BOX", "abc");
			AssertEquals("BOX", package.KP_F3_NKPackType);
			AssertEquals("abc", package.KP_PackageID);
			AssertEquals("Notifications Changed on Collection should be suspended during AddNew().", 0, notificationsChangedHitCount);
		}

		public void TestAddNewElement_EmptyUQOnPackTypeDefaultsDimensionUQFromRegistry()
		{
			PackingRegistry.Instance.SetDimensionUnitForTest("CM");

			var packTypeDefault = Factory.NewWithValidTestData<RefPackType>();
			packTypeDefault.F3_Code = "DEF";
			packTypeDefault.F3_UnitOfDimension = "MM";
			Factory.Save();
			PackingRegistry.Instance.SetOuterPackageUnitForTest("DEF");

			var packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "FOO";
			packType.F3_UnitOfDimension = "";

			var package = Collection.AddNew("FOO");
			AssertEquals("CM", package.KP_DimensionUQ);
		}

		public void TestAddNewElement_EmptyUQOnPackTypeDefaultsWeightUQFromRegistry()
		{
			PackingRegistry.Instance.SetWeightUnitForTest("MG");

			var packTypeDefault = Factory.NewWithValidTestData<RefPackType>();
			packTypeDefault.F3_Code = "DEF";
			packTypeDefault.F3_UnitOfWeight = "T";
			Factory.Save();
			PackingRegistry.Instance.SetOuterPackageUnitForTest("DEF");

			var packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "FOO";
			packType.F3_UnitOfWeight = "";

			var package = Collection.AddNew("FOO");
			AssertEquals("MG", package.KP_WeightUQ);
		}

		public void TestAddNewElement_NonemptyUQOnPackTypeDefaultsDimensionUQFromPackType()
		{
			PackingRegistry.Instance.SetDimensionUnitForTest("CM");

			var packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "FOO";
			packType.F3_UnitOfDimension = "KM";

			var package = Collection.AddNew("FOO");
			AssertEquals("KM", package.KP_DimensionUQ);
		}

		public void TestAddNewElement_NonemptyUQOnPackTypeDefaultsWeightUQFromPackType()
		{
			PackingRegistry.Instance.SetWeightUnitForTest("MG");

			var packType = Factory.NewWithValidTestData<RefPackType>();
			packType.F3_Code = "FOO";
			packType.F3_UnitOfWeight = "G";

			var package = Collection.AddNew("FOO");
			AssertEquals("G", package.KP_WeightUQ);
		}

		#endregion

		#region TestToStringSummary

		public void TestToStringSummary()
		{
			var collection = new PkgPackageCollection(Factory.New<PkgPackageJob>());
			AssertEquals("", collection.ToStringSummary());

			// 2 pallets
			var pallet1 = collection.AddNew();
			var pallet2 = collection.AddNew();
			pallet2.KP_PackageQty = 2;
			pallet1.KP_F3_NKPackType = "PLT";
			pallet2.KP_F3_NKPackType = "PLT";

			AssertEquals("3x PLT", collection.ToStringSummary());

			// add 3 boxes
			var box1 = collection.AddNew();
			var box2 = collection.AddNew();
			var box3 = collection.AddNew();
			box1.KP_F3_NKPackType = "BOX";
			box2.KP_F3_NKPackType = "BOX";
			box3.KP_F3_NKPackType = "BOX";
			AssertEquals("3x BOX, 3x PLT", collection.ToStringSummary());

			// add 1 empty package
			var emptyPackage = collection.AddNew();
			emptyPackage.KP_F3_NKPackType = "";
			AssertEquals("1x ???, 3x BOX, 3x PLT", collection.ToStringSummary());

			// rename the bad package to carton
			emptyPackage.KP_F3_NKPackType = "CTN";
			AssertEquals("3x BOX, 1x CTN, 3x PLT", collection.ToStringSummary());

			// add 1 bag
			var bag = collection.AddNew("BAG");
			AssertEquals("1x BAG...", collection.ToStringSummary(1));
			AssertEquals("1x BAG, 3x BOX, 1x CTN...", collection.ToStringSummary());
			AssertEquals("1x BAG, 3x BOX, 1x CTN, 3x PLT", collection.ToStringSummary(4));
		}

		#endregion

		#region Implementation

		protected override PkgPackageCollection GetCollectionToTest()
		{
			return new PkgPackageCollection(Factory.New<PkgPackageJob>());
		}

		protected override PkgPackageCollection GetNewCollection(PkgPackageJob packageJob)
		{
			return new PkgPackageCollection(packageJob);
		}

		#endregion
	}
}
