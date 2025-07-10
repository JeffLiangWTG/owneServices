using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsCartonSizeInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsCartonSizeInfo>
	{
		#region TestConstructorForCartonSizeInfoCollection

		public void TestConstructorForCartonSizeInfoCollection()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("ORG", "WhsOrg");
			var smallCarton = Helper.CreateWhsCartonSize("SML");
			var bigCarton = Helper.CreateWhsCartonSize("BIG");

			cartonGroup.CartonSizes.AddRange(new[] { smallCarton, bigCarton });

			var cartonSizeInfoCollection = new WhsCartonSizeInfoCollection(cartonGroup.CartonSizes);
			AssertNotNull(cartonSizeInfoCollection);
			AssertEquals(2, cartonSizeInfoCollection.Count);
			AssertEquals(cartonGroup.CartonSizes[0].WCS_Code, cartonSizeInfoCollection[0].Code);
			AssertEquals(cartonGroup.CartonSizes[1].WCS_Code, cartonSizeInfoCollection[1].Code);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsCartonSizeInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsCartonSizeInfo);
		}

		protected override WhsCartonSizeInfo GetNewObjectInfo()
		{
			return new WhsCartonSizeInfo();
		}

		protected override DataObjectInfoCollection<WhsCartonSizeInfo> GetNewObjectInfoCollection()
		{
			return new WhsCartonSizeInfoCollection();
		}

		#endregion
	}
}
