using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsPickFaceInfoCollectionCase : DataObjectInfoCollectionTestCase<WhsPickFaceInfo>
	{
		#region Constructor

		public void TestConstructor()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var pickfaceLocation = data.Whs1.DefaultLocation;
			helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation);

			var pickFaceInfoCollection = new WhsPickFaceInfoCollection(WhsProduct.GetWhsProduct(data.Part1));

			AssertEquals(1, pickFaceInfoCollection.Count);
			AssertEquals("111", pickFaceInfoCollection[0].ClientCode);
			AssertEquals(pickfaceLocation.ToLocationString(), pickFaceInfoCollection[0].LocationString);
		}

		public void TestConstructor_FixedWidthLocation()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z040302");
			helper.CreateProductPickFace(data.Part1, data.Org1, location);

			var pickFaceInfoCollection = new WhsPickFaceInfoCollection(WhsProduct.GetWhsProduct(data.Part1));

			AssertEquals(1, pickFaceInfoCollection.Count);
			var pickFaceInfo = pickFaceInfoCollection[0];
			AssertEquals("111", pickFaceInfo.ClientCode);
			AssertEquals("Z040302", pickFaceInfo.LocationString);
			AssertEquals("Z-04-03-02", pickFaceInfo.LocationString_UserFriendly);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsPickFaceInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsPickFaceInfoCollection);
		}

		protected override WhsPickFaceInfo GetNewObjectInfo()
		{
			return new WhsPickFaceInfo();
		}

		protected override DataObjectInfoCollection<WhsPickFaceInfo> GetNewObjectInfoCollection()
		{
			return new WhsPickFaceInfoCollection();
		}

		#endregion
	}
}
