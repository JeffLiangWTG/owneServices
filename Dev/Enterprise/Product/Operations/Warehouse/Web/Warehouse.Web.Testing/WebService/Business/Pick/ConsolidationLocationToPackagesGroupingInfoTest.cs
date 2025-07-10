using System;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	[TestedType(typeof(ConsolidationLocationToPackagesGroupingInfo))]
	class ConsolidationLocationToPackagesGroupingInfoTest : DataObjectInfoTestCase<ConsolidationLocationToPackagesGroupingInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var locationPK = Guid.NewGuid();
			var package1 = new ConsolidationPackageInfo { OrderPK = Guid.NewGuid(), Package = new PackageInfo { PK = Guid.NewGuid(), PackageID = "A" } };
			var package2 = new ConsolidationPackageInfo { OrderPK = Guid.NewGuid(), Package = new PackageInfo { PK = Guid.NewGuid(), PackageID = "B" } };
			var info = new ConsolidationLocationToPackagesGroupingInfo(new WhsLocationInfo(locationPK, "Location1", "Location-1", "CON"), new[] { package1, package2 });

			AssertNotNull(info.ConsolidationLocation);
			AssertEquals("Location1", info.ConsolidationLocation.LocationString);
			AssertEquals("Location-1", info.ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(locationPK, info.ConsolidationLocation.LocationPK);
			AssertContainsExactElementsInAnyOrder(new[] { package1, package2 }, info.Packages);
		}

		public void TestEmptyConstructor()
		{
			var info = new ConsolidationLocationToPackagesGroupingInfo();

			AssertNull(info.ConsolidationLocation);
			AssertNull(info.Packages);
	}

		#endregion

		#region Implementation

		protected new ConsolidationLocationToPackagesGroupingInfo Parent
		{
			get
			{
				return (ConsolidationLocationToPackagesGroupingInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new ConsolidationLocationToPackagesGroupingInfo();
		}

		#endregion
	}
}
