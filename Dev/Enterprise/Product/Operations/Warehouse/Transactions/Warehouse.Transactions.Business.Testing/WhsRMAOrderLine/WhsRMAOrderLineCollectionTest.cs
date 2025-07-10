using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsRMAOrderLineCollection))]
	public class WhsRMAOrderLineCollectionTest : WhsNonPersistentBusinessObjectCollectionTestCase<WhsRMAOrderLineCollection>
	{
		#region TestConstructor_NoRMAOrderLines

		public void TestConstructor_NoRMAOrderLines()
		{
			AssertExceptionThrown("No RMAOrderLines passed in, should throw exception", typeof(ArgumentNullException), () => new WhsRMAOrderLineCollection(Factory, null));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			return WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10m, 10m);
		}

		protected override WhsRMAOrderLineCollection GetCollectionToTest() => new WhsRMAOrderLineCollection(Factory, Array.Empty<WhsRMAOrderLine>());

		#endregion
	}
}
