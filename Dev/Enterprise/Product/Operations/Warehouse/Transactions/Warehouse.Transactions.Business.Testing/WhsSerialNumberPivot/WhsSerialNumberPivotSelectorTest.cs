using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsSerialNumberPivotSelector))]
	class WhsSerialNumberPivotSelectorTest : NonPersistentBusinessObjectTestCase
	{
		#region Constructor

		public void TestConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN1";
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			AssertNoExceptionThrown(() => new WhsSerialNumberPivotSelector(pivot, (_) => true));
			AssertExceptionThrown<ArgumentException>("SerialNumberPivot are null", () => new WhsSerialNumberPivotSelector(null, (_) => true));
			AssertExceptionThrown<ArgumentException>("ReadOnlyForChangingAllocationsProvider are null", () => new WhsSerialNumberPivotSelector(pivot, (Func<ZGuid, bool>)null));

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			AssertNoExceptionThrown(() => new WhsSerialNumberPivotSelector(pivot, pickLine));
			AssertExceptionThrown<ArgumentException>("SerialNumberPivot are null", () => new WhsSerialNumberPivotSelector(null, pickLine));
			AssertExceptionThrown<ArgumentException>("PickLineforPicking are null", () => new WhsSerialNumberPivotSelector(pivot, (WhsPickLine)null));
		}

		#endregion

		#region TestSelected

		public void TestSelected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN1";
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var pivotSelector = CreateSelector(pivot);
			var refreshBindingCalled = 0;
			var onSelectionChangedCount = 0;
			pivotSelector.SelectedInfo.ValueChanged += (s, e) => refreshBindingCalled++;
			pivotSelector.OnSelectionChanged += (s, e) => onSelectionChangedCount++;
			AssertEquals("Precondition", 0, refreshBindingCalled);
			AssertEquals("Precondition", 0, onSelectionChangedCount);

			pivotSelector.Selected = true;
			AssertEquals(1, refreshBindingCalled);
			AssertEquals(1, onSelectionChangedCount);
		}

		#endregion

		#region TestSerialNumberValue

		public void TestSerialNumberValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN1";
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();
			var pivotSelector = CreateSelector(pivot);
			AssertEquals(pivot.SerialNumberValue, pivotSelector.SerialNumberValue);
		}

		#endregion

		#region TestToggleSelected

		public void TestToggleSelected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN1";
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			var pivotSelector = CreateSelector(pivot);
			var refreshBindingCalled = 0;
			var onSelectionChangedCount = 0;
			pivotSelector.SelectedInfo.ValueChanged += (s, e) => refreshBindingCalled++;
			pivotSelector.OnSelectionChanged += (s, e) =>
			{
				var selector = (WhsSerialNumberPivotSelector)s;
				selector.SelectSerialNumber(selector.Selected ? ZGuid.Empty : pickLine.PK);

				onSelectionChangedCount++;
			};
			AssertEquals("Precondition", 0, refreshBindingCalled);
			AssertEquals("Precondition", 0, onSelectionChangedCount);

			pivotSelector.Selected = true;
			AssertEquals(1, refreshBindingCalled);
			AssertEquals(1, onSelectionChangedCount);

			pivotSelector.Selected = true;
			AssertEquals("Should not call again.", 1, refreshBindingCalled);
			AssertEquals("Should not call again.", 1, onSelectionChangedCount);
		}

		#endregion

		#region TestWhsSerialNumberPivotSelectorProperties

		public void TestWhsSerialNumberPivotSelectorProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN1";
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			var selector = CreateSelector(pivot);
			selector.OnSelectionChanged += (s, e) =>
			{
				selector.SelectSerialNumber(selector.Selected ? ZGuid.Empty : pickLine.PK);
			};

			AssertEquals(ZGuid.Empty, selector.PickingLinePK);
			AssertEquals(receiveLine.PK, selector.InventoryPK);
			AssertEquals("SN1", selector.SerialNumberValue);
		}

		#endregion

		#region TestSelectSerialNumber

		public void TestSelectSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_SerialNumber = "SN1";
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			var selectorNoPickLineSpecified = CreateSelector(pivot);
			AssertEquals(null, selectorNoPickLineSpecified.PickLineforPicking);
			AssertExceptionThrown<InvalidOperationException>("Only call SelectSerialNumber when pickline is already taken from this serial number.", selectorNoPickLineSpecified.SelectSerialNumber);

			var selectorPickLineSpecified = new WhsSerialNumberPivotSelector(pivot, pickLine);
			AssertEquals(ZGuid.Empty, selectorPickLineSpecified.PickingLinePK);
			AssertEquals(pickLine.PK, selectorPickLineSpecified.PickLineforPicking.PK);
			selectorPickLineSpecified.SelectSerialNumber();
			AssertEquals(pickLine.PK, selectorPickLineSpecified.PickingLinePK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var pivot = receiveLine1.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";

			return CreateSelector(pivot);
		}

		static WhsSerialNumberPivotSelector CreateSelector(WhsSerialNumberPivot pivot) => new WhsSerialNumberPivotSelector(pivot, (_) => true);

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
