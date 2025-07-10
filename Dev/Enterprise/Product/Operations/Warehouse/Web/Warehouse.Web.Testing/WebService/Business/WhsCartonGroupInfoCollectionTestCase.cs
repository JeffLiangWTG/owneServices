using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsCartonGroupInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsCartonGroupInfo>
	{
		#region TestConstructorForCartonSize

		public void TestConstructorForCartonSize()
		{
			var cartonGroup = Helper.CreateWhsCartonGroup("1", "1");
			var size1 = Helper.CreateWhsCartonSize("AAA");
			var size2 = Helper.CreateWhsCartonSize("BBB");

			var cartonGroup2 = Helper.CreateWhsCartonGroup("2", "2");
			var size3 = Helper.CreateWhsCartonSize("CCC");
			var size4 = Helper.CreateWhsCartonSize("DDD");

			cartonGroup.CartonSizes.AddRange(new[] { size1, size2 });
			cartonGroup2.CartonSizes.AddRange(new[] { size3, size4 });

			var cartonGroupInfoCollection = new WhsCartonGroupInfoCollection(new[] { cartonGroup, cartonGroup2 });
			AssertNotNull(cartonGroupInfoCollection);
			AssertEquals(2, cartonGroupInfoCollection.Count);
			AssertEquals(cartonGroup.WCG_Code, cartonGroupInfoCollection[0].Code);
			AssertEquals(2, cartonGroupInfoCollection[0].CartonSizes.Count);
			AssertEquals(cartonGroup2.WCG_Code, cartonGroupInfoCollection[1].Code);
			AssertEquals(2, cartonGroupInfoCollection[1].CartonSizes.Count);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsCartonGroupInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsCartonGroupInfo);
		}

		protected override WhsCartonGroupInfo GetNewObjectInfo()
		{
			return new WhsCartonGroupInfo();
		}

		protected override DataObjectInfoCollection<WhsCartonGroupInfo> GetNewObjectInfoCollection()
		{
			return new WhsCartonGroupInfoCollection();
		}

		#endregion
	}
}
