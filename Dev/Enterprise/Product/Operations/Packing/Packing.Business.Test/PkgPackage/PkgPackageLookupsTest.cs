using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestWeightUQs

		public void TestWeightUQs()
		{
			var package = Factory.New<PkgPackage>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), package.Lookups.WeightUQs);
		}

		#endregion

		#region TestVolumeUQs

		public void TestVolumeUQs()
		{
			var package = Factory.New<PkgPackage>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), package.Lookups.VolumeUQs);
		}

		#endregion

		#region TestDimensionUQs

		public void TestDimensionUQs()
		{
			var package = Factory.New<PkgPackage>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length), package.Lookups.DimensionUQs);
		}

		#endregion

		#region TestPackTypes

		public void TestPackTypes()
		{
			TestPackTypesCore(p => p.Lookups.PackTypes);
		}

		public void TestGetPackTypes_WithPK()
		{
			TestPackTypesCore(p => PkgPackageLookups.GetPackTypes(Factory, p.KP_KJ_ParentPackageJob));
		}

		public void TestGetPackTypes_WithPackageJob()
		{
			TestPackTypesCore(p => PkgPackageLookups.GetPackTypes(Factory, p.PackageJob));
		}

		protected virtual PkgPackageJob GetNewPackageJobForPackTypeTest() => Factory.New<PkgPackageJob>();

		void TestPackTypesCore(Func<PkgPackage, RefPackTypeCollection> getPackTypes)
		{
			// package with no Parent Job should have all Pack Types by default
			var packageJobWithNoParent = GetNewPackageJobForPackTypeTest();
			var package1 = packageJobWithNoParent.Packages.AddNew();
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Factory, excludeCNT: false), getPackTypes(package1));

			// package with Custom Pack Types Parent Job should exclude specified Pack Types
			var data = new TestDataForPacking(Factory);
			data.CreatePackingData();
			data.Dummy.PackTypesToExclude = new ZString[] { "BOX", "PAI" };
			var package2 = data.PackageJob.Packages.AddNew();
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Factory, false).Where(p => p.F3_Code != "BOX" && p.F3_Code != "PAI"),
				getPackTypes(package2));
		}

		public void TestPackTypes_IncludeContainerType()
		{
			var package = Factory.NewWithValidTestData<PkgPackage>();
			AssertEquals(true, package.Lookups.PackTypes.ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}

		#endregion

		#region TestTemperatureUnits

		public void TestTemperatureUnits()
		{
			var package = Factory.New<PkgPackage>();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.TemperatureTypes), package.Lookups.TemperatureUnits);
		}

		#endregion

		#region TestPrinters

		public void TestPrinters()
		{
			var printQueue1 = Factory.New<IStmPrintQueue>();
			printQueue1.QueueName = "Printer1";
			printQueue1.SQ_AllowPrinting = true;

			var printQueue2 = Factory.New<IStmPrintQueue>();
			printQueue2.QueueName = "Printer2";
			printQueue2.SQ_AllowPrinting = true;
			Factory.Save();

			var package = Factory.NewWithValidTestData<PkgPackage>();
			var printers = package.Lookups.Printers;
			AssertEquals("There are 2 printers.", 2, printers.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Printer1", "Printer2" }, printers.ToArray().Cast<IStmPrintQueue>().Select(printer => printer.QueueName));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		#endregion
	}
}
