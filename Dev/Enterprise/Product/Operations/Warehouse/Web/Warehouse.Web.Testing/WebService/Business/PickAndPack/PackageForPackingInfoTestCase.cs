using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PackageForPackingInfo))]
	class PackageForPackingInfoTestCase : DataObjectInfoTestCase<PackageForPackingInfo>
	{
		#region TestConstructor

		public void TestConstructor_Empty()
		{
			var packageInfo = new PackageForPackingInfo();
			AssertEquals(Guid.Empty, packageInfo.PK);
			AssertEquals(string.Empty, packageInfo.OrderReference);
			AssertEquals(string.Empty, packageInfo.DocketID);
			AssertEquals(string.Empty, packageInfo.ToteID);
			AssertEquals(string.Empty, packageInfo.PackageID);
			AssertEquals(string.Empty, packageInfo.WeightUQ);
			AssertEquals(string.Empty, packageInfo.DimensionUQ);
			AssertEquals(string.Empty, packageInfo.CartonSize);
			AssertEquals(0m, packageInfo.PackageWeightTolerance);
			AssertEquals(0m, packageInfo.Weight);
			AssertEquals(0m, packageInfo.EmptyWeight);
			AssertEquals(0m, packageInfo.Length);
			AssertEquals(0m, packageInfo.Width);
			AssertEquals(0m, packageInfo.Height);
			AssertEquals(false, packageInfo.IsTote);
			AssertEquals(true, packageInfo.IsUsingCartonSizes);
			AssertEquals(string.Empty, packageInfo.DocketStatus);
			AssertEquals(DateTime.MinValue, packageInfo.RequiredDate);
			AssertEquals(string.Empty, packageInfo.JobID);
			AssertEquals(false, packageInfo.ClientEnforceProductScan);
			AssertEquals(string.Empty, packageInfo.ClientCode);
			AssertNull(packageInfo.ScannedProductInfos);
			AssertEquals(false, packageInfo.IsPackageSplitForPacking);
			AssertEquals(false, packageInfo.IsConsolidationHandlingUnit);
			AssertEquals(false, packageInfo.IsDirectedPacking);
			AssertEquals(false, packageInfo.OrderIsUsingDirectedPackingConsolidation);
			AssertEquals(false, packageInfo.PackageRequiresPutaway);
			AssertEquals(string.Empty, packageInfo.AssignedDockDoorLocationString);
			AssertEquals(string.Empty, packageInfo.AssignedDockDoorLocationStringUserFriendly);
			AssertEquals(false, packageInfo.AllowedToOverrideDockDoorLocation);
		}

		public void TestConstructor()
		{
			var pk = Guid.NewGuid();
			var requiredDate = DateTime.UtcNow;
			var packageInfo = new PackageForPackingInfo(pk, "ORD111", "W00000001", true, "1234567890",
				10m, 5m, "KG", 5m, true,
				5m, 5m, 5m, "M",
				"FIN", requiredDate, "JOB0001", "TestClient",
				true,
				true,
				true,
				true,
				true,
				true,
				"Test",
				"TestUserFriendly",
				allowedToOverrideDockDoorLocation: true);
			AssertEquals(pk, packageInfo.PK);
			AssertEquals("ORD111", packageInfo.OrderReference);
			AssertEquals("W00000001", packageInfo.DocketID);
			AssertEquals(true, packageInfo.IsUsingCarrierLabelIntegration);
			AssertEquals("1234567890", packageInfo.ToteID);
			AssertEquals(string.Empty, packageInfo.PackageID);
			AssertEquals("KG", packageInfo.WeightUQ);
			AssertEquals("M", packageInfo.DimensionUQ);
			AssertEquals(5m, packageInfo.PackageWeightTolerance);
			AssertEquals(true, packageInfo.PackageWeightToleranceEnabled);
			AssertEquals(10m, packageInfo.Weight);
			AssertEquals(5m, packageInfo.EmptyWeight);
			AssertEquals(5m, packageInfo.Length);
			AssertEquals(5m, packageInfo.Width);
			AssertEquals(5m, packageInfo.Height);
			AssertEquals(true, packageInfo.IsTote);
			AssertEquals(true, packageInfo.IsUsingCartonSizes);
			AssertEquals("FIN", packageInfo.DocketStatus);
			AssertEquals(requiredDate, packageInfo.RequiredDate);
			AssertEquals("JOB0001", packageInfo.JobID);
			AssertEquals(true, packageInfo.ClientEnforceProductScan);
			AssertEquals("TestClient", packageInfo.ClientCode);
			AssertNull(packageInfo.ScannedProductInfos);
			AssertEquals(false, packageInfo.IsPackageSplitForPacking);
			AssertEquals(true, packageInfo.IsConsolidationHandlingUnit);
			AssertEquals(true, packageInfo.OrderIsUsingDirectedPackingConsolidation);
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
			AssertEquals("Test", packageInfo.AssignedDockDoorLocationString);
			AssertEquals("TestUserFriendly", packageInfo.AssignedDockDoorLocationStringUserFriendly);
			AssertEquals(true, packageInfo.AllowedToOverrideDockDoorLocation);
		}

		#endregion

		#region Properties

		public void TestTotePK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			var pk = Guid.NewGuid();
			Parent.PK = pk;
			AssertEquals(pk, Parent.PK);
		}

		public void TestOrderReference()
		{
			AssertEquals(string.Empty, Parent.OrderReference);

			Parent.OrderReference = "ORD111";
			AssertEquals("ORD111", Parent.OrderReference);
		}

		public void TestDocketID()
		{
			AssertEquals(string.Empty, Parent.DocketID);

			Parent.DocketID = "W00000001";
			AssertEquals("W00000001", Parent.DocketID);
		}

		public void TestIsUsingCarrierLabelIntegration()
		{
			var info = new PackageForPackingInfo();
			AssertEquals(false, info.IsUsingCarrierLabelIntegration);

			info.IsUsingCarrierLabelIntegration = true;
			AssertEquals(true, info.IsUsingCarrierLabelIntegration);
		}

		public void TestToteID()
		{
			AssertEquals(string.Empty, Parent.ToteID);

			Parent.ToteID = "1234567890";
			AssertEquals("1234567890", Parent.ToteID);
		}

		public void TestPackageID()
		{
			AssertEquals(string.Empty, Parent.PackageID);

			Parent.PackageID = "1234567890";
			AssertEquals("1234567890", Parent.PackageID);
		}

		public void TestPackType()
		{
			AssertEquals(string.Empty, Parent.PackType);

			Parent.PackType = "BOX";
			AssertEquals("BOX", Parent.PackType);
		}

		public void TestWeightUQ()
		{
			AssertEquals(string.Empty, Parent.WeightUQ);

			Parent.WeightUQ = "KG";
			AssertEquals("KG", Parent.WeightUQ);
		}

		public void TestWeight()
		{
			AssertEquals(0m, Parent.Weight);

			Parent.Weight = 10m;
			AssertEquals(10m, Parent.Weight);
		}

		public void TestLength()
		{
			AssertEquals(0m, Parent.Length);

			Parent.Length = 10m;
			AssertEquals(10m, Parent.Length);
		}

		public void TestWidth()
		{
			AssertEquals(0m, Parent.Width);

			Parent.Length = 10m;
			AssertEquals(10m, Parent.Length);
		}

		public void TestHeight()
		{
			AssertEquals(0m, Parent.Height);

			Parent.Height = 10m;
			AssertEquals(10m, Parent.Height);
		}

		public void TestDimensionUQ()
		{
			AssertEquals(string.Empty, Parent.DimensionUQ);

			Parent.DimensionUQ = "CM";
			AssertEquals("CM", Parent.DimensionUQ);
		}

		public void TestPackageWeightTolerance()
		{
			AssertEquals(0m, Parent.PackageWeightTolerance);

			Parent.PackageWeightTolerance = 10m;
			AssertEquals(10m, Parent.PackageWeightTolerance);
		}

		public void TestPackageWeightToleranceEnabled()
		{
			AssertEquals(false, Parent.PackageWeightToleranceEnabled);

			Parent.PackageWeightToleranceEnabled = true;
			AssertEquals(true, Parent.PackageWeightToleranceEnabled);
		}

		public void TestIsTote()
		{
			AssertEquals(false, Parent.IsTote);

			Parent.IsTote = true;
			AssertEquals(true, Parent.IsTote);
		}

		public void TestIsUsingCartonSizes()
		{
			AssertEquals(true, Parent.IsUsingCartonSizes);

			Parent.IsUsingCartonSizes = false;
			AssertEquals(false, Parent.IsUsingCartonSizes);
		}

		public void TestCartonSize()
		{
			AssertEquals(string.Empty, Parent.CartonSize);

			Parent.CartonSize = "ABC";
			AssertEquals("ABC", Parent.CartonSize);
		}

		public void TestDocketStatus()
		{
			AssertEquals(string.Empty, Parent.DocketStatus);

			Parent.DocketStatus = "FIN";
			AssertEquals("FIN", Parent.DocketStatus);
		}

		public void TestRequiredDate()
		{
			AssertEquals(DateTime.MinValue, Parent.RequiredDate);

			var date = new DateTime(2017, 12, 25);
			Parent.RequiredDate = date;
			AssertEquals(date, Parent.RequiredDate);
		}

		public void TestJobID()
		{
			AssertEquals(string.Empty, Parent.JobID);

			Parent.JobID = "JOB0001";
			AssertEquals("JOB0001", Parent.JobID);
		}

		public void TestClientCode()
		{
			AssertEquals(string.Empty, Parent.ClientCode);

			Parent.ClientCode = "ABCD";
			AssertEquals("ABCD", Parent.ClientCode);
		}

		public void TestClientEnforceProductScan()
		{
			AssertEquals(false, Parent.ClientEnforceProductScan);

			Parent.ClientEnforceProductScan = true;
			AssertEquals(true, Parent.ClientEnforceProductScan);
		}

		public void TestScannedProductInfos()
		{
			AssertNull(Parent.ScannedProductInfos);

			Parent.ScannedProductInfos = new[] { new WhsPackageProductInfo() };
			AssertNotNull(Parent.ScannedProductInfos);
		}

		public void TestIsPackageClosedEarly()
		{
			AssertEquals(false, Parent.IsPackageSplitForPacking);

			Parent.IsPackageSplitForPacking = true;
			AssertEquals(true, Parent.IsPackageSplitForPacking);
		}

		public void TestIsConsolidationHandlingUnit()
		{
			AssertEquals(false, Parent.IsConsolidationHandlingUnit);

			Parent.IsConsolidationHandlingUnit = true;
			AssertEquals(true, Parent.IsConsolidationHandlingUnit);
		}

		public void TestIsUsingDirectedPackingConsolidation()
		{
			AssertEquals(false, Parent.OrderIsUsingDirectedPackingConsolidation);

			Parent.OrderIsUsingDirectedPackingConsolidation = true;
			AssertEquals(true, Parent.OrderIsUsingDirectedPackingConsolidation);
		}

		public void TestPackageRequiresPutaway()
		{
			AssertEquals(false, Parent.PackageRequiresPutaway);

			Parent.PackageRequiresPutaway = true;
			AssertEquals(true, Parent.PackageRequiresPutaway);
		}

		public void TestAssignedDockDoorLocation()
		{
			AssertEquals(string.Empty, Parent.AssignedDockDoorLocationString);
			AssertEquals(string.Empty, Parent.AssignedDockDoorLocationStringUserFriendly);

			Parent.AssignedDockDoorLocationString = "Test";
			Parent.AssignedDockDoorLocationStringUserFriendly = "TestUserFriendly";

			AssertEquals("Test", Parent.AssignedDockDoorLocationString);
			AssertEquals("TestUserFriendly", Parent.AssignedDockDoorLocationStringUserFriendly);
		}

		#endregion

		#region Implementation

		protected new PackageForPackingInfo Parent
		{
			get
			{
				return (PackageForPackingInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PackageForPackingInfo();
		}

		#endregion
	}
}
