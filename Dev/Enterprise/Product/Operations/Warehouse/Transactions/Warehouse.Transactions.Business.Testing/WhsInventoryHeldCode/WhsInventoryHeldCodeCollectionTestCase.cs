using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeCollection))]
	public class WhsInventoryHeldCodeCollectionTestCase : WhsActiveBusinessObjectCollectionTestCase<WhsInventoryHeldCodeCollection>
	{
		#region TestConstructor_ClientIsNotNull

		public void TestConstructor_ClientIsNotNull()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var holdCode1 = helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var holdCode2 = helper.CreateInventoryHeldCode("BBB", "BBB for system");
			var holdCode3 = helper.CreateInventoryHeldCode("DDD", "DDD for client 1", client1.PK);
			var holdCode4 = helper.CreateInventoryHeldCode("DDD", "DDD for client 2", client2.PK);
			var holdCode5 = helper.CreateInventoryHeldCode("EEE", "EEE for client 2", client2.PK);

			var collection1 = new WhsInventoryHeldCodeCollection(Factory, client1.PK);
			AssertCollectionContains(holdCode1, collection1);
			AssertCollectionContains(holdCode2, collection1);
			AssertCollectionContains(holdCode3, collection1);
			AssertCollectionNotContains(holdCode4, collection1);
			AssertCollectionNotContains(holdCode5, collection1);
			var collection2 = new WhsInventoryHeldCodeCollection(Factory, client2.PK);
			AssertCollectionContains(holdCode1, collection2);
			AssertCollectionContains(holdCode2, collection2);
			AssertCollectionContains(holdCode4, collection2);
			AssertCollectionContains(holdCode5, collection2);
			AssertCollectionNotContains(holdCode3, collection2);
		}

		public void TestConstructor_ClientPKIsEmpty()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var holdCode1 = helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var holdCode2 = helper.CreateInventoryHeldCode("BBB", "BBB for system");
			var holdCode3 = helper.CreateInventoryHeldCode("DDD", "DDD for client 1", client1.PK);
			var holdCode4 = helper.CreateInventoryHeldCode("DDD", "DDD for client 2", client2.PK);
			var holdCode5 = helper.CreateInventoryHeldCode("EEE", "EEE for client 2", client2.PK);

			var collection = new WhsInventoryHeldCodeCollection(Factory, ZGuid.Empty);
			AssertCollectionContains(holdCode1, collection);
			AssertCollectionContains(holdCode2, collection);
			AssertCollectionNotContains(holdCode3, collection);
			AssertCollectionNotContains(holdCode4, collection);
			AssertCollectionNotContains(holdCode5, collection);
		}

		#endregion

		#region TestConstructor_FactoryReturnsAllCodes

		public void TestConstructor_FactoryReturnsAllCodes()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var holdCode1 = helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var holdCode2 = helper.CreateInventoryHeldCode("BBB", "BBB for system");
			var holdCode3 = helper.CreateInventoryHeldCode("DDD", "DDD for client 1", client1.PK);
			var holdCode4 = helper.CreateInventoryHeldCode("DDD", "DDD for client 2", client2.PK);

			var collection1 = new WhsInventoryHeldCodeCollection(Factory);
			AssertCollectionContains(holdCode1, collection1);
			AssertCollectionContains(holdCode2, collection1);
			AssertCollectionContains(holdCode3, collection1);
			AssertCollectionContains(holdCode4, collection1);
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("C1");
			var client2 = helper.CreateClient("C2");
			var holdCode1 = helper.CreateInventoryHeldCode("ABC", "ABC for system");
			var holdCode2 = helper.CreateInventoryHeldCode("BBB", "BBB for system");
			var holdCode3 = helper.CreateInventoryHeldCode("DDD", "DDD for client 1", client1.PK);
			var holdCode4 = helper.CreateInventoryHeldCode("EEE", "EEE test", client1.PK);
			var holdCode5 = helper.CreateInventoryHeldCode("DDD2", "DDD for client 2", client2.PK);
			var holdCode6 = helper.CreateInventoryHeldCode("EEE2", "EEE test", client2.PK);

			var collection = new WhsInventoryHeldCodeCollection(Factory);
			AssertCollectionContains(holdCode1, collection);
			AssertCollectionContains(holdCode2, collection);
			AssertCollectionContains(holdCode3, collection);
			AssertCollectionContains(holdCode4, collection);
			AssertCollectionContains(holdCode5, collection);
			AssertCollectionContains(holdCode6, collection);

			AssertEquals(true, ((ICodeDescriptionPairList)collection).ContainsCode("DDD"));
			AssertEquals(false, ((ICodeDescriptionPairList)collection).ContainsCode("TST"));

			AssertEquals("ABC for system", ((ICodeDescriptionPairList)collection).GetDescriptionFromCode("ABC"));
			AssertEquals("", ((ICodeDescriptionPairList)collection).GetDescriptionFromCode("TST"));
			AssertEquals("DDD for client 1", ((ICodeDescriptionPairList)collection).GetDescriptionFromCode("DDD"));
			AssertEquals("EEE test", ((ICodeDescriptionPairList)collection).GetDescriptionFromCode("EEE"));

			using (var mockData = Res.UseMockData())
			{
				var key = holdCode1.WHC_DescriptionInfo.CustomizableDataResourceStrings.Source.GetKey(null, "ABC for system");
				mockData.Put(key, new ResourceStringData(key, "ABC-测试"));
				AssertEquals("ABC-测试", ((ICodeDescriptionPairList)collection).GetDescriptionFromCode("ABC"));
			}
		}

		#endregion

		#region Implementation

		#region GetCollectionToTest

		protected override WhsInventoryHeldCodeCollection GetCollectionToTest()
		{
			return new WhsInventoryHeldCodeCollection(Factory);
		}

		#endregion

		#endregion
	}
}
