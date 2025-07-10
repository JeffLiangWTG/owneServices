using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class UpdateWhsPickCriticalChangesVersionIDHelperTest : WhsTestCaseWithFactory
	{
		#region TestUpdatePickVersion_InvalidArgument

		public void TestUpdatePickVersion_InvalidArgument()
		{
			AssertExceptionThrown<ArgumentException>("Pick PK should be valid", "Pick PK should be valid.",
				() => UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					ZGuid.Empty));
			AssertExceptionThrown<ArgumentException>("Pick PK should be valid", "Pick PK should be valid.",
				() => UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					ZGuid.Invalid));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(null,
					ZGuid.NewZGuid()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(null,
					Factory.New<WhsPickLine>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(null,
					Factory.New<WhsPickableDocket>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(null,
					Factory.New<WhsOrderLine>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(null,
					Factory.New<WhsWorkOrderLine>()));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					(WhsPickLine)null));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					(WhsPickableDocket)null));
			AssertExceptionThrown<ArgumentNullException>(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					(WhsPickableDocketLine)null));
			AssertNoExceptionThrown(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					ZGuid.NewZGuid()));
			AssertNoExceptionThrown(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					Factory.New<WhsPickLine>()));
			AssertNoExceptionThrown(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					Factory.New<WhsPickableDocket>()));
			AssertNoExceptionThrown(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					Factory.New<WhsOrderLine>()));
			AssertNoExceptionThrown(() =>
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(NewFactory(),
					Factory.New<WhsWorkOrderLine>()));
		}

		#endregion

		#region TestUpdatePickVersion_PickPk

		public void TestUpdatePickVersion_PickPk()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			Factory.Save();

			var pickVersion = pick.WP_CriticalChangesVersionID;
			pick.Factory.Save();
			AssertEquals("Not change if not bind to after save factory.", pickVersion,
				pick.WP_CriticalChangesVersionID);

			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory, pick.PK);
			pick.Factory.Save();
			AssertEquals("Should update version.", true, pick.WP_CriticalChangesVersionID.IsValid);
			AssertNotEquals("Should update version.", pickVersion, pick.WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestUpdatePickVersion_NotHaveRelatedPick

		public void TestUpdatePickVersion_NotHaveRelatedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			Factory.Save();

			var pickVersion = pick.WP_CriticalChangesVersionID;
			pick.Factory.Save();
			AssertEquals("Not change if not bind to after save factory.", pickVersion,
				pick.WP_CriticalChangesVersionID);

			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory,
				NewFactory().New<WhsPickLine>());
			pick.Factory.Save();
			AssertEquals("Should * not * update version.", pickVersion, pick.WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestUpdatePickVersion_ByPickLine_NotLoadAndProcessRelatedObjects

		public void TestUpdatePickVersion_ByPickLine_NotLoadAndProcessRelatedObjects()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 40m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, order1, order2);
			Factory.Save();
			var pickLine1 = orderLine1.PickLines.Single();
			var splitPickLine = pickLine1.Split(5m);
			Factory.Save();

			var pickLine1PK = pickLine1.PK;
			var pickLine2PK = splitPickLine.PK;
			var pickLine3PK = orderLine2.PickLines.Single().PK;
			var pickLine4PK = order2.Lines.Single().PickLines.Single().PK;
			var pickNewFactory = new BusinessObjectFactory { RefreshEnabled = false };
			AssertEquals("Expect to have 4 picklines", 4, pick.GetAllPickLines().Count());
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pickNewFactory,
				pickLine1);

			AssertDBLoadCountOnSubscribe("No hits, already processed!", pickNewFactory, pickLine1PK, expectedCount: 0);
			AssertDBLoadCountOnSubscribe("It already processed oederLine1.", pickNewFactory, pickLine2PK,
				expectedCount: 0);
			AssertDBLoadCountOnSubscribe("Need to check oederLine1 and already processed order1.", pickNewFactory,
				pickLine3PK, expectedCount: 1);
			AssertDBLoadCountOnSubscribe("Need to check oederLine2 and order2.", pickNewFactory, pickLine4PK,
				expectedCount: 2);

			pickNewFactory.Save();
			AssertDBLoadCountOnSubscribe("Need to check all after save!", pickNewFactory, pickLine1PK,
				expectedCount: 2);
		}

		static void AssertDBLoadCountOnSubscribe(string message, BusinessObjectFactory pickFactory, ZGuid pickLinePK,
			int expectedCount)
		{
			var newFactory = new BusinessObjectFactory();
			var pickLine = newFactory.Load<WhsPickLine>(pickLinePK);
			newFactory.ResetDatabaseLoadCount();
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pickFactory, pickLine);
			AssertEquals(message, expectedCount, newFactory.DatabaseLoadCount);
		}

		#endregion

		#region TestUpdatePickVersion_SubscribeMoreThanOnePick

		public void TestUpdatePickVersion_SubscribeMoreThanOnePick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var pick2 = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order2);
			Factory.Save();

			var pickVersion1 = pick1.WP_CriticalChangesVersionID;
			var pickVersion2 = pick2.WP_CriticalChangesVersionID;

			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick1.Factory, pick1.PK);
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick1.Factory, pick2.PK);
			Factory.Save();

			AssertNotEquals("Should update version.", pickVersion1, pick1.WP_CriticalChangesVersionID);
			AssertNotEquals("Should update version.", pickVersion2, pick2.WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestUpdatePickVersion_UpdateOnce

		public void TestUpdatePickVersion_UpdateOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			Factory.Save();

			var pickVersion = pick.WP_CriticalChangesVersionID;
			var called = 0;
			pick.WP_CriticalChangesVersionIDInfo.ValueChanged += (o, s) => called++;

			for (int i = 0; i < 3; i++)
			{
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory,
					pick.PK);
			}

			pick.Factory.Save();

			AssertEquals("Should only call once", 1, called);
			AssertNotEquals("Should update version.", pickVersion, pick.WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestUpdatePickVersion_UpdateOnce_CombinationInputType

		public void TestUpdatePickVersion_UpdateOnce_CombinationInputType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			Factory.Save();

			var pickVersion = pick.WP_CriticalChangesVersionID;
			var called = 0;
			pick.WP_CriticalChangesVersionIDInfo.ValueChanged += (o, s) => called++;

			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory,
				pick.GetAllPickLines().Single());
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory, order);
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory, pick.PK);
			pick.Factory.Save();

			AssertEquals("Should only call once", 1, called);
			AssertNotEquals("Should update version.", pickVersion, pick.WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestUpdatePickVersion_PickableDocket

		public void TestUpdatePickVersion_PickableDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false);
			Factory.Save();

			var pickVersion = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			pick.Orders.Add(order);
			pick.Factory.Save();
			AssertNotEquals("Should update pick version.", pickVersion,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);

			var pickVersionAfterAttach = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory,
				NewFactory().New<WhsOrder>());
			pick.Factory.Save();
			AssertEquals("Should * not * update version.", pickVersionAfterAttach,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
		}

		#endregion

		#region TestUpdatePickVersion_PickableDocketLine

		public void TestUpdatePickVersion_PickableDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var pickVersion = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			pick.Factory.Save();
			AssertNotEquals("Should update pick version.", pickVersion,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);

			var pickVersionAfterAddOrderLine = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(pick.Factory,
				NewFactory().New<WhsOrderLine>());
			pick.Factory.Save();
			AssertEquals("Should * not * update version.", pickVersionAfterAddOrderLine,
				NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID);
		}

		#endregion
	}
}
