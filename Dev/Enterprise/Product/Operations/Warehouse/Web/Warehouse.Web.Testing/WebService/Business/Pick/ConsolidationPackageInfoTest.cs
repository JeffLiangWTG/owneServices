using System;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	[TestedType(typeof(ConsolidationPackageInfo))]
	class ConsolidationPackageInfoTest : DataObjectInfoTestCase<ConsolidationPackageInfoTest>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var orderPK = Guid.NewGuid();
			var package = new PackageInfo { PK = Guid.NewGuid(), PackageID = "A" };
			var info = new ConsolidationPackageInfo(package, orderPK, true);

			AssertNotNull(info.Package);
			AssertEquals("A", info.Package.PackageID);
			AssertEquals(package.PK, info.Package.PK);
			AssertEquals(orderPK, info.OrderPK);
			AssertEquals(true, info.CannotOverrideConsolidationLocation);
		}

		public void TestEmptyConstructor()
		{
			var info = new ConsolidationPackageInfo();

			AssertNull(info.Package);
			AssertEquals(Guid.Empty, info.OrderPK);
			AssertEquals(false, info.CannotOverrideConsolidationLocation);
		}

		#endregion

		#region Implementation

		protected new ConsolidationPackageInfo Parent => (ConsolidationPackageInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo() => new ConsolidationPackageInfo();

		#endregion
	}
}
