using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsLocationInfo))]
	class WhsConsolidationLocationInfoTest : DataObjectInfoTestCase<WhsLocationInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			var locationPK = Guid.NewGuid();
			var consolidationLocationInfo = new WhsLocationInfo(locationPK, "A1", "A-1", "NOR");
			AssertEquals(locationPK, consolidationLocationInfo.LocationPK);
			AssertEquals("A1", consolidationLocationInfo.LocationString);
			AssertEquals("A-1", consolidationLocationInfo.LocationString_UserFriendly);
		}

		#endregion

		#region Implementation

		protected new WhsLocationInfo Parent
		{
			get
			{
				return (WhsLocationInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsLocationInfo();
		}

		#endregion
	}
}
