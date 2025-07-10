using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ShipmentSummary))]
	sealed class ShipmentSummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShipmentSummary()
		{
			var shipmentSummary = GetNewShipmentSummary();

			AssertEquals("S00012345", shipmentSummary.UniqueConsignRef);
			AssertEquals("C00099999", shipmentSummary.ConsolReference);
			AssertEquals(new ZDateTime(2015, 1, 1), shipmentSummary.DepartureDate);
			AssertEquals("SGSIN", shipmentSummary.Origin);
			AssertEquals("CNSHA", shipmentSummary.Destination);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewShipmentSummary();
		}

		internal static ShipmentSummary GetNewShipmentSummary()
		{
			var sql = "select JS_PK = NEWID(), JS_UniqueConsignRef = 'S00012345', JK_UniqueConsignRef = 'C00099999', DepartureDate = CAST('01-JAN-2015' AS DATE), JS_RL_NKOrigin = 'SGSIN', JS_RL_NKDestination = 'CNSHA'";
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load(sql);

			var dynBO = collection[0];
			var shipmentSummary = new ShipmentSummary(dynBO);

			return shipmentSummary;
		}
	}
}
