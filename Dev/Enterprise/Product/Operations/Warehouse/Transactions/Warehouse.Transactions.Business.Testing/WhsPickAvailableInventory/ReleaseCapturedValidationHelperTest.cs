using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ReleaseCapturedValidationHelperTest : WhsTestCaseWithFactory
	{
		#region TestCheckPickedPickLinesAreFullyReleaseCaptured

		public void TestCheckPickedPickLinesAreFullyReleaseCaptured_Mandatory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];

			var pickLine1 = orderLine.PickLines[0];
			var pickLine2 = orderLine.PickLines[1];

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			using (pickLine1.SuspendValidationTesting())
			{
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine1.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1 });
				AssertNoErrors(pickLine1.WZ_PickedDateTimeInfo);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine1.WZ_PickedDateTimeInfo, orderedInventory, null, data.Org1, new[] { pickLine1 });
				AssertNoErrors(pickLine1.WZ_PickedDateTimeInfo);

				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine1.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1 });
				AssertHasError(pickLine1.WZ_PickedDateTimeInfo, "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked.");

				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine1.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1 });
				AssertNoErrors(pickLine1.WZ_PickedDateTimeInfo);
			}

			using (pickLine2.SuspendValidationTesting())
			{
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine2.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1, pickLine2 });
				AssertHasError(pickLine2.WZ_PickedDateTimeInfo, "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked.");

				pickLine2.WZ_PickedDateTimeInfo.ClearAllNotifications();
				pickLine2.WZ_ReleaseCapturedPartAttrib1 = "PA1";

				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine2.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine2 });
				AssertNoErrors(pickLine2.WZ_PickedDateTimeInfo);
			}
		}

		public void TestCheckPickedPickLinesAreFullyReleaseCaptured_NonMandatory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			var pickLine1 = orderLine.PickLines[0];
			pickLine1.WZ_ReleaseCapturedPartAttrib1 = "";
			pickLine1.WZ_ReleaseCapturedPartAttrib2 = "PA2";
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = orderLine.PickLines[1];
			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "PA1";
			pickLine2.WZ_ReleaseCapturedPartAttrib2 = "";
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;

			using (pickLine1.SuspendValidationTesting())
			{
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine1.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1 });
				AssertNoErrors(pickLine1.WZ_PickedDateTimeInfo);
			}

			using (pickLine2.SuspendValidationTesting())
			{
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine2.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1, pickLine2 });
				AssertHasError(pickLine2.WZ_PickedDateTimeInfo, "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked.");

				pickLine2.WZ_PickedDateTimeInfo.ClearAllNotifications();
				pickLine2.WZ_ReleaseCapturedPartAttrib2 = "PA2";
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine2.WZ_PickedDateTimeInfo, orderedInventory, orderLine.Product, data.Org1, new[] { pickLine1, pickLine2 });
				AssertNoErrors(pickLine2.WZ_PickedDateTimeInfo);
			}
		}

		#endregion

		#region TestCheckPickedPickLinesAreFullyReleaseCaptured_WithWorkOrder

		public void TestCheckPickedPickLinesAreFullyReleaseCaptured_WithWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsWorkOrderLine(order, data.Part1, 2m);
			var childLine = orderLine.ChildComponentLines.ElementAt(0);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];

			var pickLine = pick.GetAllPickLines().First();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			using (pickLine.SuspendValidationTesting())
			{
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine.WZ_PickedDateTimeInfo, orderedInventory, childLine.Product, data.Org1, new[] { pickLine });
				AssertNoErrors(pickLine.WZ_PickedDateTimeInfo);

				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine.WZ_PickedDateTimeInfo, null, childLine.Product, data.Org1, new[] { pickLine });
				AssertNoErrors(pickLine.WZ_PickedDateTimeInfo);
			}
		}

		#endregion

		#region TestCheckPickedPickLinesAreFullyReleaseCaptured_WithComponentLineOnSalesOrder

		public void TestCheckPickedPickLinesAreFullyReleaseCaptured_WithComponentLineOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var childLine = orderLine.ChildComponentLines.ElementAt(0);
			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.IsComponentOrderedInventoryOnSalesOrder);

			var pickLine = pick.GetAllPickLines().First();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			using (pickLine.SuspendValidationTesting())
			{
				ReleaseCapturedValidationHelper.CheckPickedPickLinesAreFullyReleaseCaptured(pickLine.WZ_PickedDateTimeInfo, orderedInventory, childLine.Product, data.Org1, new[] { pickLine });
				AssertNoErrors(pickLine.WZ_PickedDateTimeInfo);
			}
		}

		#endregion
	}
}
