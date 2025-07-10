using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsClientPickingParams))]
	class WhsClientPickingParamsTest : NonPersistentBusinessObjectTestCase
	{
		#region Related Entities

		#region TestClient

		public void TestClient()
		{
			var client = Helper.CreateClient();
			var pickingParams = WhsClientPickingParams.GetClientPickingParams(client);
			AssertEquals("Correct Client should be set.", client, pickingParams.Client);
		}

		#endregion

		#region TestWarehousePickPackParams

		public void TestWarehousePickPackParams()
		{
			var client = Helper.CreateClient();
			var pickingParams = WhsClientPickingParams.GetClientPickingParams(client);
			AssertNotNull(pickingParams.WarehousePickPackParams);
			AssertEquals("Client Pick Pack Parameters collection should have correct type.", typeof(WhsClientPickPackParamsByWhsCollection), pickingParams.WarehousePickPackParams.GetType());
			AssertEquals("Client Pick Pack Parameters collection should be registered editable.", true, pickingParams.IsRegisteredEditableChildObject(pickingParams.WarehousePickPackParams));

			var pickPackParams = pickingParams.WarehousePickPackParams.AddNew();
			AssertEquals("Collection should have correct relationship.", client.PK, pickPackParams.WPP_OH_Client);
		}

		#endregion

		#endregion

		#region TestGetClientPickingParams

		public void TestGetClientPickingParams()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WhsClientPickingParams.GetClientPickingParams(null));

			var client = Helper.CreateClient();
			var pickingParams = WhsClientPickingParams.GetClientPickingParams(client);
			AssertEquals("Picking Parameters BizO should be registered as child editable for the Client.", true, client.IsRegisteredEditableChildObject(pickingParams));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return WhsClientPickingParams.GetClientPickingParams(Helper.CreateClient());
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
