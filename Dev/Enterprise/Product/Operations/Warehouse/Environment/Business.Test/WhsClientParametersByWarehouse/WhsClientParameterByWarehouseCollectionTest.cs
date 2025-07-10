using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsClientParameterByWarehouseCollection))]
	class WhsClientParameterByWarehouseCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsClientParameterByWarehouseCollection>
	{
		#region Business Object Overrides

		public void TestRelationship()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse2 = Factory.New<WhsWarehouse>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var param1 = Factory.New<WhsClientParameterByWarehouse>();
			var param2 = Factory.New<WhsClientParameterByWarehouse>();
			var param3 = Factory.New<WhsClientParameterByWarehouse>();
			var param4 = Factory.New<WhsClientParameterByWarehouse>();

			param1.WY_OH_Client = org1.PK;
			param2.WY_OH_Client = org2.PK;
			param3.WY_OH_Client = org2.PK;
			param4.WY_OH_Client = org1.PK;

			param1.WY_WW_Whs = warehouse1.PK;
			param2.WY_WW_Whs = warehouse1.PK;
			param3.WY_WW_Whs = warehouse2.PK;
			param4.WY_WW_Whs = warehouse2.PK;

			param1.WY_ReceiveCategory = "RC1";
			param2.WY_ReceiveCategory = ZString.Empty;
			param3.WY_ReceiveCategory = ZString.Empty;
			param4.WY_ReceiveCategory = ZString.Empty;

			var collection1 = GetCollectionToTest(org1);
			AssertEquals(true, collection1.Contains(param1));
			AssertEquals(false, collection1.Contains(param2));
			AssertEquals(false, collection1.Contains(param3));
			AssertEquals(true, collection1.Contains(param4));

			var collection2 = GetCollectionToTest(org2);
			AssertEquals(false, collection2.Contains(param1));
			AssertEquals(true, collection2.Contains(param2));
			AssertEquals(true, collection2.Contains(param3));
			AssertEquals(false, collection2.Contains(param4));
		}

		#endregion

		#region Methods

		public void TestConstructorWithWarehouseParameter()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse2 = Factory.New<WhsWarehouse>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var param1 = Factory.New<WhsClientParameterByWarehouse>();
			var param2 = Factory.New<WhsClientParameterByWarehouse>();
			var param3 = Factory.New<WhsClientParameterByWarehouse>();
			var param4 = Factory.New<WhsClientParameterByWarehouse>();
			var param5 = Factory.New<WhsClientParameterByWarehouse>();
			var param6 = Factory.New<WhsClientParameterByWarehouse>();
			var param7 = Factory.New<WhsClientParameterByWarehouse>();
			var param8 = Factory.New<WhsClientParameterByWarehouse>();

			param1.WY_OH_Client = org1.PK;
			param2.WY_OH_Client = org2.PK;
			param3.WY_OH_Client = org2.PK;
			param4.WY_OH_Client = org1.PK;
			param5.WY_OH_Client = org1.PK;
			param6.WY_OH_Client = org1.PK;
			param7.WY_OH_Client = org1.PK;
			param8.WY_OH_Client = org2.PK;

			param1.WY_WW_Whs = warehouse1.PK;
			param2.WY_WW_Whs = warehouse1.PK;
			param3.WY_WW_Whs = warehouse2.PK;
			param4.WY_WW_Whs = warehouse2.PK;
			param5.WY_WW_Whs = warehouse1.PK;
			param6.WY_WW_Whs = ZGuid.Empty;
			param7.WY_WW_Whs = ZGuid.Empty;
			param8.WY_WW_Whs = ZGuid.Empty;

			param1.WY_ReceiveCategory = "RC1";
			param2.WY_ReceiveCategory = ZString.Empty;
			param3.WY_ReceiveCategory = ZString.Empty;
			param4.WY_ReceiveCategory = ZString.Empty;
			param5.WY_ReceiveCategory = ZString.Empty;
			param6.WY_ReceiveCategory = "RC2";
			param7.WY_ReceiveCategory = ZString.Empty;
			param8.WY_ReceiveCategory = ZString.Empty;

			var collection1 = new WhsClientParameterByWarehouseCollection(org1, warehouse1.PK);
			AssertCollectionContains(param1, collection1);
			AssertCollectionContains(param5, collection1);
			AssertCollectionNotContains(param2, collection1);
			AssertCollectionNotContains(param3, collection1);
			AssertCollectionNotContains(param4, collection1);
			AssertCollectionNotContains(param6, collection1);
			AssertCollectionNotContains(param7, collection1);

			var collection2 = new WhsClientParameterByWarehouseCollection(org1, warehouse2.PK);
			AssertCollectionNotContains(param1, collection2);
			AssertCollectionNotContains(param2, collection2);
			AssertCollectionNotContains(param3, collection2);
			AssertCollectionContains(param4, collection2);
			AssertCollectionNotContains(param6, collection2);
			AssertCollectionNotContains(param7, collection2);

			var collection3 = new WhsClientParameterByWarehouseCollection(org2, warehouse1.PK);
			AssertCollectionNotContains(param1, collection3);
			AssertCollectionContains(param2, collection3);
			AssertCollectionNotContains(param3, collection3);
			AssertCollectionNotContains(param4, collection3);
			AssertCollectionNotContains(param6, collection3);
			AssertCollectionNotContains(param7, collection3);

			var collection4 = new WhsClientParameterByWarehouseCollection(org2, warehouse2.PK);
			AssertCollectionNotContains(param1, collection4);
			AssertCollectionNotContains(param2, collection4);
			AssertCollectionContains(param3, collection4);
			AssertCollectionNotContains(param4, collection4);
			AssertCollectionNotContains(param6, collection4);
			AssertCollectionNotContains(param7, collection4);
		}

		public void TestFindWithFallback()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse2 = Factory.New<WhsWarehouse>();
			var warehouse3 = Factory.New<WhsWarehouse>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var param1 = Factory.New<WhsClientParameterByWarehouse>();
			var param2 = Factory.New<WhsClientParameterByWarehouse>();
			var param3 = Factory.New<WhsClientParameterByWarehouse>();
			var param4 = Factory.New<WhsClientParameterByWarehouse>();
			var param5 = Factory.New<WhsClientParameterByWarehouse>();
			var param6 = Factory.New<WhsClientParameterByWarehouse>();
			var param7 = Factory.New<WhsClientParameterByWarehouse>();
			var param8 = Factory.New<WhsClientParameterByWarehouse>();
			var param9 = Factory.New<WhsClientParameterByWarehouse>();

			param1.WY_OH_Client = org1.PK;
			param2.WY_OH_Client = org1.PK;
			param3.WY_OH_Client = org1.PK;
			param4.WY_OH_Client = org1.PK;
			param5.WY_OH_Client = org1.PK;
			param6.WY_OH_Client = org1.PK;
			param7.WY_OH_Client = org1.PK;
			param8.WY_OH_Client = org1.PK;
			param9.WY_OH_Client = org1.PK;

			param1.WY_WW_Whs = warehouse1.PK;
			param2.WY_WW_Whs = warehouse1.PK;
			param3.WY_WW_Whs = warehouse1.PK;
			param4.WY_WW_Whs = warehouse2.PK;
			param5.WY_WW_Whs = warehouse2.PK;
			param6.WY_WW_Whs = warehouse2.PK;
			param7.WY_WW_Whs = ZGuid.Empty;
			param8.WY_WW_Whs = ZGuid.Empty;
			param9.WY_WW_Whs = ZGuid.Empty;

			param1.WY_ReceiveCategory = "RC1";
			param2.WY_ReceiveCategory = "RC2";
			param3.WY_ReceiveCategory = ZString.Empty;
			param4.WY_ReceiveCategory = "RC1";
			param5.WY_ReceiveCategory = "RC2";
			param6.WY_ReceiveCategory = ZString.Empty;
			param7.WY_ReceiveCategory = "RC1";
			param8.WY_ReceiveCategory = "RC2";
			param9.WY_ReceiveCategory = ZString.Empty;

			var collection1 = GetCollectionToTest(org1);
			AssertEquals(param1, collection1.FindWithEmptyFallback(org1.PK, warehouse1.PK, "RC1"));
			AssertEquals(param2, collection1.FindWithEmptyFallback(org1.PK, warehouse1.PK, "RC2"));
			AssertEquals(param3, collection1.FindWithEmptyFallback(org1.PK, warehouse1.PK, ZString.Empty));
			AssertEquals(param3, collection1.FindWithEmptyFallback(org1.PK, warehouse1.PK, "RC3"));
			AssertEquals(param4, collection1.FindWithEmptyFallback(org1.PK, warehouse2.PK, "RC1"));
			AssertEquals(param5, collection1.FindWithEmptyFallback(org1.PK, warehouse2.PK, "RC2"));
			AssertEquals(param6, collection1.FindWithEmptyFallback(org1.PK, warehouse2.PK, ZString.Empty));
			AssertEquals(param6, collection1.FindWithEmptyFallback(org1.PK, warehouse2.PK, "RC3"));
			AssertEquals(param7, collection1.FindWithEmptyFallback(org1.PK, warehouse3.PK, "RC1"));
			AssertEquals(param8, collection1.FindWithEmptyFallback(org1.PK, warehouse3.PK, "RC2"));
			AssertEquals(param8, collection1.FindWithEmptyFallback(org1.PK, ZGuid.Empty, "RC2"));
			AssertEquals(param9, collection1.FindWithEmptyFallback(org1.PK, warehouse3.PK, "RC3"));
			AssertEquals(null, collection1.FindWithEmptyFallback(org2.PK, warehouse1.PK, "RC1"));

			var collection2 = GetCollectionToTest(org2);
			AssertEquals(null, collection2.FindWithEmptyFallback(org2.PK, warehouse1.PK, "RC1"));
		}

		public void TestFindWithFallback_Fallbacks()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var org1 = Factory.New<OrgHeader>();

			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var param1 = Factory.New<WhsClientParameterByWarehouse>();
			var param2 = Factory.New<WhsClientParameterByWarehouse>();
			var param3 = Factory.New<WhsClientParameterByWarehouse>();
			var param4 = Factory.New<WhsClientParameterByWarehouse>();

			param1.WY_OH_Client = org1.PK;
			param2.WY_OH_Client = org1.PK;
			param3.WY_OH_Client = org1.PK;
			param4.WY_OH_Client = org1.PK;

			param1.WY_WW_Whs = warehouse1.PK;
			param2.WY_WW_Whs = warehouse1.PK;
			param3.WY_WW_Whs = ZGuid.Empty;
			param4.WY_WW_Whs = ZGuid.Empty;

			param1.WY_ReceiveCategory = "RC1";
			param2.WY_ReceiveCategory = ZString.Empty;
			param3.WY_ReceiveCategory = "RC1";
			param4.WY_ReceiveCategory = ZString.Empty;

			var collection1 = GetCollectionToTest(org1);
			AssertEquals("Perfect match for both warehouse and receive category.", param1, collection1.FindWithEmptyFallback(org1.PK, warehouse1.PK, "RC1"));
			AssertEquals("Warehouse matches and receive category fallback to empty.", param2, collection1.FindWithEmptyFallback(org1.PK, warehouse1.PK, "DDD"));
			AssertEquals("Receive category matches and warehouse fallback to empty", param3, collection1.FindWithEmptyFallback(org1.PK, new ZGuid(), "RC1"));
			AssertEquals("Both warehouse and receive category fallback to empty", param4, collection1.FindWithEmptyFallback(org1.PK, new ZGuid(), "DDD"));
		}

		#endregion

		#region Implementation

		protected override WhsClientParameterByWarehouseCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			return GetCollectionToTest(org);
		}

		WhsClientParameterByWarehouseCollection GetCollectionToTest(OrgHeader org)
		{
			return new WhsClientParameterByWarehouseCollection(org);
		}

		#endregion
	}

	[TestedType(typeof(WhsClientParameterByWarehouseCollection))]
	class WhsWarehouseParametersByClientCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsClientParameterByWarehouseCollection>
	{
		#region Business Object Overrides

		public void TestRelationship()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse2 = Factory.New<WhsWarehouse>();

			var org1 = OrgHeader.New(Factory);
			var org2 = OrgHeader.New(Factory);

			var param1 = Factory.New<WhsClientParameterByWarehouse>();
			var param2 = Factory.New<WhsClientParameterByWarehouse>();
			var param3 = Factory.New<WhsClientParameterByWarehouse>();
			var param4 = Factory.New<WhsClientParameterByWarehouse>();

			param1.WY_OH_Client = org1.PK;
			param2.WY_OH_Client = org1.PK;
			param3.WY_OH_Client = org2.PK;
			param4.WY_OH_Client = org2.PK;

			param1.WY_WW_Whs = warehouse1.PK;
			param2.WY_WW_Whs = warehouse2.PK;
			param3.WY_WW_Whs = warehouse1.PK;
			param4.WY_WW_Whs = warehouse2.PK;

			var collection1 = GetCollectionToTest(warehouse1);
			AssertEquals(true, collection1.Contains(param1));
			AssertEquals(false, collection1.Contains(param2));
			AssertEquals(true, collection1.Contains(param3));
			AssertEquals(false, collection1.Contains(param4));

			var collection2 = GetCollectionToTest(warehouse2);
			AssertEquals(false, collection2.Contains(param1));
			AssertEquals(true, collection2.Contains(param2));
			AssertEquals(false, collection2.Contains(param3));
			AssertEquals(true, collection2.Contains(param4));
		}

		#endregion

		#region Implementation

		protected override WhsClientParameterByWarehouseCollection GetCollectionToTest()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			return GetCollectionToTest(warehouse);
		}

		WhsClientParameterByWarehouseCollection GetCollectionToTest(WhsWarehouse warehouse)
		{
			return new WhsClientParameterByWarehouseCollection(warehouse);
		}

		#endregion
	}
}
