using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsClientPickPackParamsByWhsCollection))]
	class WhsClientPickPackParamsByWhsCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsClientPickPackParamsByWhsCollection>
	{
		#region TestRelationship

		public void TestRelationship()
		{
			var client = Helper.CreateClient();
			var collection = new WhsClientPickPackParamsByWhsCollection(client);
			var pickPackParameter = collection.AddNew();
			AssertEquals("New Element should point to the correct Warehouse.", client.PK, pickPackParameter.WPP_OH_Client);
		}

		#endregion

		#region Implementation

		protected override WhsClientPickPackParamsByWhsCollection GetCollectionToTest()
		{
			return new WhsClientPickPackParamsByWhsCollection(Helper.CreateClient());
		}

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
