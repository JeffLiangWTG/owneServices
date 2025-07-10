using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class PackageForPackingInfoCollectionTestCase : DataObjectInfoCollectionTestCase<PackageForPackingInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			var packageInfosForPacking = new List<PackageForPackingInfo>();
			packageInfosForPacking.Add(new PackageForPackingInfo());
			packageInfosForPacking.Add(new PackageForPackingInfo());

			var packageInfoForPackingCollection = new PackageForPackingInfoCollection(packageInfosForPacking.ToArray());
			AssertEquals("The collection should contain 2 PackageForPackingInfo objects.", 2, packageInfoForPackingCollection.Count);
		}

		#endregion

		#region Implementation

		protected new PackageForPackingInfoCollection Parent => (PackageForPackingInfoCollection)base.Parent;

		protected override Type GetExpectedObjectInfoType() => typeof(PackageForPackingInfo);

		protected override Type GetExpectedCollectionType() => typeof(PackageForPackingInfoCollection);

		protected override PackageForPackingInfo GetNewObjectInfo() => new PackageForPackingInfo();

		protected override DataObjectInfoCollection<PackageForPackingInfo> GetNewObjectInfoCollection() => new PackageForPackingInfoCollection();

		#endregion
	}
}
