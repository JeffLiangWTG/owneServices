using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsInventoryHeldCodeInfoCollectionTest : DataObjectInfoCollectionTestCase<WhsInventoryHeldCodeInfo>
	{
		#region TestConstructorForHeldCodes

		public void TestConstructorForHeldCodes()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var heldCode1 = helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var heldCode2 = helper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1.PK);
			var heldCode3 = helper.CreateInventoryHeldCode("CCC", "test", client1.PK);
			var heldCode4 = helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			var heldCode5 = helper.CreateInventoryHeldCode("DDD", "test", client2.PK);
			Factory.Save();

			var heldCodes = new WhsInventoryHeldCodeCollection(Factory);
			var heldCodeInfoCollection = new WhsInventoryHeldCodeInfoCollection(heldCodes);
			AssertNotNull(heldCodeInfoCollection);
			AssertEquals(10, heldCodeInfoCollection.Count);
			AssertEquals(true, heldCodeInfoCollection.Exists(c => string.IsNullOrEmpty(c.Code) && string.IsNullOrEmpty(c.Description) && string.IsNullOrEmpty(c.ClientCode)));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "HEL" && c.Description == "Held" && string.IsNullOrEmpty(c.ClientCode)));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "DAM" && c.Description == "Damaged" && string.IsNullOrEmpty(c.ClientCode)));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "LCC" && c.Description == "Lost in Cycle Count" && string.IsNullOrEmpty(c.ClientCode)));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "SHORT" && c.Description == "Short Picked" && string.IsNullOrEmpty(c.ClientCode)));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "AAA" && c.Description == "AAA for system" && string.IsNullOrEmpty(c.ClientCode)));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "BBB" && c.Description == "BBB for client 1" && c.ClientCode == "C1"));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "CCC" && c.Description == "test" && c.ClientCode == "C1"));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "BBB2" && c.Description == "BBB for client 2" && c.ClientCode == "C2"));
			AssertEquals(true, heldCodeInfoCollection.Exists(c => c.Code == "DDD" && c.Description == "test" && c.ClientCode == "C2"));
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsInventoryHeldCodeInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsInventoryHeldCodeInfo);
		}

		protected override WhsInventoryHeldCodeInfo GetNewObjectInfo()
		{
			return new WhsInventoryHeldCodeInfo();
		}

		protected override DataObjectInfoCollection<WhsInventoryHeldCodeInfo> GetNewObjectInfoCollection()
		{
			return new WhsInventoryHeldCodeInfoCollection();
		}

		#endregion
	}
}

