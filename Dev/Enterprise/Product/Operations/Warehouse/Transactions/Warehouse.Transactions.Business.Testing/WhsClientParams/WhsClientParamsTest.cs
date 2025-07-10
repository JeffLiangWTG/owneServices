using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsClientParams))]
	public class WhsClientParamsTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region Constructors

		public void TestConstructorAndClientProperty()
		{
			var client = Factory.New<OrgHeader>();
			var clientParams = WhsClientParams.GetClientParams(client);
			AssertEquals(client, clientParams.Client);
		}

		#endregion

		#region Related Business Objects

		#region TestClientParametersByWarehouse

		public void TestClientParametersByWarehouse()
		{
			var client = Factory.New<OrgHeader>();
			var clientParams = WhsClientParams.GetClientParams(client);
			var param1 = Factory.New<WhsClientParameterByWarehouse>();
			var param2 = Factory.New<WhsClientParameterByWarehouse>();

			param1.WY_OH_Client = client.PK;
			param2.WY_OH_Client = client.PK;

			param1.WY_WW_Whs = Factory.New<WhsWarehouse>().PK;
			param2.WY_WW_Whs = Factory.New<WhsWarehouse>().PK;

			AssertEquals(2, clientParams.ClientParametersByWarehouse.Count);
			AssertEquals(true, clientParams.IsRegisteredEditableChildObject(clientParams.ClientParametersByWarehouse));
		}

		#endregion

		#region TestGetClientParams_Cached

		public void TestGetClientParams_Cached()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WhsClientParams.GetClientParams(null));
			var client = Factory.New<OrgHeader>();

			var param1 = WhsClientParams.GetClientParams(client);
			var param2 = Factory.GetCachedValue<WhsClientParams>($"WhsClientParams|{client.PK}", null);

			AssertNotNull(param1);
			AssertNotNull(param2);
			AssertEquals(param1.PK, param2.PK);
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return WhsClientParams.GetClientParams(Factory.New<OrgHeader>());
		}

		#endregion
	}
}
