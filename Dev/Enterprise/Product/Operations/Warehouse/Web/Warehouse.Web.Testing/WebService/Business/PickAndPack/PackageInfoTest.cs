using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PackageInfo))]
	class PackageInfoTest : DataObjectInfoTestCase<PackageInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);

			var data = new TestDataForPacking(Factory);
			data.CreatePackingData();

			var package = data.PackageJob.Packages.AddNew("PLT", "123");
			package.Pack(data.DummyLine1, 5m);
			package.Pack(data.DummyLine2, 3m);

			var packageInfo = new PackageInfo(package);
			AssertEquals("Package PK", package.PK, packageInfo.PK);
			AssertEquals("Package ID", "123", packageInfo.PackageID);
			AssertEquals("Package Type", "PLT", packageInfo.PackType);
			AssertEquals("Quantity Packed", 8m, packageInfo.QtyPacked);
		}

		#endregion

		#region TestPackagePK

		public void TestPackagePK()
		{
			var packageInfo = new PackageInfo();
			AssertEquals(Guid.Empty, packageInfo.PK);

			packageInfo.PK = Guid.NewGuid();
			AssertNotEquals(Guid.Empty, packageInfo.PK);
		}

		#endregion

		#region TestPackageID

		public void TestPackageID()
		{
			var packageInfo = new PackageInfo();
			AssertEquals("", packageInfo.PackageID);

			packageInfo.PackageID = "ABC";
			AssertEquals("ABC", packageInfo.PackageID);
		}

		#endregion

		#region TestPackType

		public void TestPackType()
		{
			var packageInfo = new PackageInfo();
			AssertEquals("", packageInfo.PackType);

			packageInfo.PackType = "PLT";
			AssertEquals("PLT", packageInfo.PackType);
		}

		#endregion

		#region TestQtyPacked

		public void TestQtyPacked()
		{
			var packageInfo = new PackageInfo();
			AssertEquals(0m, packageInfo.QtyPacked);

			packageInfo.QtyPacked = 10m;
			AssertEquals(10m, packageInfo.QtyPacked);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PackageInfo();
		}

		#endregion
	}
}
