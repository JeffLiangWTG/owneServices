using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReleaseLineCollection))]
	class WhsReleaseLineCollectionTest : WhsNonPersistentBusinessObjectCollectionTestCase<WhsReleaseLineCollection>
	{
		#region TestAddNewElement

		public void TestAddNewElement()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine1.ReleaseLines[0].Quantity = 0m;

			var releaseLinesOnOrderLine2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2);
			bool listChangedFired = false;
			ListChangedEventHandler handlerForNonPersistentReleaseLines = (sender, e) =>
			{
				if (e.ListChangedType == ListChangedType.ItemAdded)
				{
					listChangedFired = true;
					AssertEquals("Precondition: Sum of Units Met is Zero.", 0m, releaseLinesOnOrderLine2.SumOfUnitsMet);

					orderLine2.ReleaseLines[1].Quantity = 10m;
					AssertEquals("Sum of Units Met should update because List Changed should have been suspended until after Persistent Release Line was Hooked.", 10m, releaseLinesOnOrderLine2.SumOfUnitsMet);
					AssertEquals("Redundant original release line should be deleted.", 1, releaseLinesOnOrderLine2.Count);
				}
			};

			((IBindingList)orderLine2.ReleaseLines).ListChanged += handlerForNonPersistentReleaseLines;

			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);
			var releaseLine = releaseLinesOnOrderLine2.AddNew();
			AssertEquals(false, releaseLinesOnOrderLine2.IsNonCommittedCollectionElement(releaseLine));
			AssertEquals("Ensure Assertion Delegate was invoked.", true, listChangedFired);
			((IBindingList)orderLine2.ReleaseLines).ListChanged -= handlerForNonPersistentReleaseLines; // clean-up

			releaseLine.Quantity = 10m;
			releaseLine.PartAttribute1 = "RED";
			var pickLine = orderLine2.PickLines.Single(p => p.WZ_ReleaseCapturedPartAttrib1 == "RED");

			AssertEquals("AddNew() should Hook Release Line AttributeChanged Event which should Create Release Captured Attributes.", 0m, pickLine.UnreleaseCapturedQty);
			AssertEquals("AddNew() should Hook Release Line AttributeChanged Event which should Create Release Captured Attributes.", 10m, pickLine.WZ_Units);
			AssertEquals("Release Lines Collection should have correct Sum of Units Met.", 10m, releaseLinesOnOrderLine2.SumOfUnitsMet);

			ListChangedEventHandler handler = (sender, e) => releaseLinesOnOrderLine2.AddNew();

			((IBindingList)releaseLinesOnOrderLine2).ListChanged += handler;
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not call AddNew() recursively.", () => releaseLinesOnOrderLine2.AddNew());

			((IBindingList)releaseLinesOnOrderLine2).ListChanged -= handler;
			releaseLinesOnOrderLine2.ClearCollection();
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not call AddNew() when Release Lines is Invalidated.", () => releaseLinesOnOrderLine2.AddNew());
		}

		public void TestAddNewElement_SetAttributesToUnReleasedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: false);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			inventory.Lines[0].WE_PartAttrib2 = "HIYA";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine1.ReleaseLines[0].Quantity = 0m;
			var releaseLinesOnOrderLine2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2);

			using (releaseLinesOnOrderLine2.SuspendAdditionallyForImport())
			{
				var releaseLine = releaseLinesOnOrderLine2.AddNew();
				AssertEquals("Release line should have non-RCA PartAttribute2 correct", "HIYA", releaseLine.PartAttribute2);
			}
		}

		#endregion

		#region TestAddNew_Attributes

		public void TestAddNew_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var year = ZDate.Today.Year;
			var newReleaseLine = orderLine.ReleaseLines.AddNew("PA1", "PA2", "PA3", "SER", new ZDate(year, 1, 1), new ZDate(year, 1, 2));
			AssertEquals(nameof(newReleaseLine.PartAttribute1), "PA1", newReleaseLine.PartAttribute1);
			AssertEquals(nameof(newReleaseLine.PartAttribute2), "PA2", newReleaseLine.PartAttribute2);
			AssertEquals(nameof(newReleaseLine.PartAttribute3), "PA3", newReleaseLine.PartAttribute3);
			AssertEquals(nameof(newReleaseLine.SerialNumber), "SER", newReleaseLine.SerialNumber);
			AssertEquals(nameof(newReleaseLine.ExpiryDate), new ZDate(year, 1, 1), newReleaseLine.ExpiryDate);
			AssertEquals(nameof(newReleaseLine.PackingDate), new ZDate(year, 1, 2), newReleaseLine.PackingDate);
		}

		#endregion

		#region TestAddNew_IBindingList

		public void TestAddNew_IBindingList()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine.ReleaseLines[0].Quantity = 1m;

			var pickLine = orderLine.PickLines.Single(p => !p.HasReleaseCapturedAttribs);
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var newReleaseLine1 = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			AssertEquals("IBinding List should add new element as non-committed.", true, releaseLines.IsNonCommittedCollectionElement(newReleaseLine1));
			AssertEquals("Sum of Units met should be 1m.", 1m, releaseLines.SumOfUnitsMet);

			newReleaseLine1.Quantity = 9m;
			newReleaseLine1.PartAttribute1 = "RED";
			AssertEquals("Since Release Line is not committed, the Pick Line should not get Release Captured.", 10m, pickLine.UnreleaseCapturedQty);
			AssertEquals("Release Lines Collection should have correct Sum of Units Met.", 10m, releaseLines.SumOfUnitsMet);

			releaseLines.DeleteReleaseLine(newReleaseLine1);
			AssertEquals("Should delete correct Non-Persistent Release Line.", true, newReleaseLine1.IsDeleted);
			AssertEquals("Should have removed Non-Persistent Release Line from Collection.", 1, releaseLines.Count);
			AssertEquals("Release Lines Collection should have correct Sum of Units Met.", 1m, releaseLines.SumOfUnitsMet);

			var newReleaseLine2 = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			newReleaseLine2.Delete();
			AssertEquals("Should Delete correct Non-Persistent Release Line.", true, newReleaseLine2.IsDeleted);
			AssertEquals("Should Remove Non-Persistent Release Line.", 1, releaseLines.Count);
		}

		#endregion

		#region TestAddNew_Quantity

		public void TestAddNew_Quantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 3m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);
			AssertExceptionThrown(typeof(ArgumentException), "Do not attempt to Create a Release Line with a Negative Quantity.\r\nParameter name: quantity",
				() => orderLine.ReleaseLines.AddNew("RED", "", "", "", ZDate.Empty, ZDate.Empty, -1m));

			var releaseLine2 = orderLine.ReleaseLines.AddNew("RED", "", "", "", ZDate.Empty, ZDate.Empty, 2m);
			AssertEquals(nameof(releaseLine2.PartAttribute1), "RED", releaseLine2.PartAttribute1);
			AssertEquals(nameof(releaseLine2.Quantity), 2m, releaseLine2.Quantity);
			AssertEquals("Release Captured Qty is Correct.", 2m, orderLine.PickLines.Where(l => l.HasReleaseCapturedAttribs).Sum(l => l.WZ_Units));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 }, order.PackableItemParents.Typed);
		}

		#endregion

		#region TestAllowNewAndRemove

		public void TestAllowNewAndRemove()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("AllowNew should be false if no Attributes are used.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be false if no Attributes are used.", false, releaseLines.AllowRemove);
		}

		public void TestAllowNewAndRemove_FinalizedPick_SerialNumberEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			orderLine.ReleaseLines[0].SerialNumber = "SN0";
			orderLine.ReleaseLines[0].Quantity = 1m;

			for (var i = 1; i < 10; i++)
			{
				var releaseLine = orderLine.ReleaseLines.AddNew("", "", "", "SN" + i, ZDate.Empty, ZDate.Empty);
				releaseLine.Quantity = 1m;
			}

			AssertEquals("Precondition", 10, orderLine.ReleaseLines.Count);
			AssertEquals("Precondition", 10m, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Sum(rl => rl.Quantity));

			order.FinaliseDocket();
			AssertEquals("Order should be finalised.", true, order.IsFinalised);
			AssertEquals("Pick should not be Finalised.", false, pick.IsFinalised);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			pick.FinalisePick();
			AssertEquals("Pick should be Finalised.", true, pick.IsFinalised);
			AssertEquals("Release Lines should not have AllowNew true.", false, releaseLines.AllowNew);
			AssertEquals("Release Lines should not have AllowRemove true.", false, releaseLines.AllowRemove);
		}

		public void TestAllowNewAndRemove_ReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "A", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "B", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "A", "", "", "");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "B", "", "", "");
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;

			var releaseLines1 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine1);
			var releaseLines2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2);
			var releaseLines3 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine3);

			AssertEquals("AllowNew should be false if there are no pick lines.", false, releaseLines1.AllowNew);
			AssertEquals("AllowNew should be false if there are no pick lines.", false, releaseLines2.AllowNew);

			var availInvA = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(ordInv => ordInv.AvailableInventories).Cast<WhsPickAvailableInventory>().First(availInv => availInv.PartAttrib2 == "A");
			availInvA.PickLineQuantity = 10m;
			releaseLines2[0].Quantity = 0m;

			AssertEquals("AllowNew should be false if exists ReleaseLine.Quantity = 0.", false, releaseLines1.AllowNew);
			AssertEquals("AllowNew should be false if exists ReleaseLine.Quantity = 0.", false, releaseLines2.AllowNew);
			AssertEquals("AllowNew should be false as there are no suitable pick lines.", false, releaseLines3.AllowNew);

			releaseLines2[0].Quantity = 5m;
			AssertEquals("AllowNew should be false if exists ReleaseLine.Quantity = 0.", false, releaseLines1.AllowNew);
			AssertEquals("AllowNew should be true if there are available units.", true, releaseLines2.AllowNew);
			AssertEquals("AllowRemove should be true if pick not finalised and product has release captured attribute.", true, releaseLines2.AllowRemove);
			AssertEquals("AllowNew should be false as there are no suitable pick lines.", false, releaseLines3.AllowNew);

			availInvA.PickLineQuantity = 0m;
			var availInvB = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(ordInv => ordInv.AvailableInventories).Cast<WhsPickAvailableInventory>().Where(availInv => availInv.PartAttrib2 == "B");
			availInvB.ForEach(ai => ai.Allocate = true);
			releaseLines3[0].Quantity = 0m;

			AssertEquals("AllowNew should be true if there are pick lines and available units.", true, releaseLines1.AllowNew);
			AssertEquals("AllowRemove should be true if pick not finalised and product has release captured attribute.", true, releaseLines1.AllowRemove);
			AssertEquals("AllowNew should be false as there are no suitable pick lines.", false, releaseLines2.AllowNew);
			AssertEquals("AllowRemove should be false as there are no suitable pick lines.", false, releaseLines2.AllowRemove);
			AssertEquals("AllowNew should be true if exists ReleaseLine.Quantity = 0.", false, releaseLines3.AllowNew);
			AssertEquals("AllowRemove should be true if pick not finalised and product has release captured attribute.", true, releaseLines3.AllowRemove);

			releaseLines1[0].Quantity = 10m;
			AssertEquals("AllowNew should be not be true as OrderedQty is Met.", false, releaseLines1.AllowNew);
			AssertEquals("AllowNew should be not be true as there are no availabe units.", false, releaseLines3.AllowNew);

			releaseLines1[0].Quantity = 0m;
			AssertEquals("Precondition.", false, releaseLines1.AllowNew);
			AssertEquals("Precondition.", true, releaseLines1.AllowRemove);
			AssertEquals("Precondition.", false, releaseLines3.AllowNew);
			AssertEquals("Precondition.", false, releaseLines3.AllowRemove);

			releaseLines3[0].Quantity = 5m;
			AssertEquals("AllowNew should be false if exists ReleaseLine.Quantity = 0.", false, releaseLines1.AllowNew);
			AssertEquals("AllowRemove should be true.", true, releaseLines1.AllowRemove);
			AssertEquals("AllowNew should false as OrderedQty is Met.", false, releaseLines3.AllowNew);
			AssertEquals("AllowRemove should be true.", true, releaseLines3.AllowRemove);
		}

		public void TestAllowNewAndRemove_ReleaseCapturedAttributes_PickLinesPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;

			var availInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(ordInv => ordInv.AvailableInventories).Cast<WhsPickAvailableInventory>().Single();
			availInv.PickLineQuantity = 5m;

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine1);
			releaseLines[0].Quantity = 5m;
			releaseLines[0].PartAttribute1 = "RED";
			AssertEquals("AllowNew should be false as OrderedQty is Met.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be true.", true, releaseLines.AllowRemove);

			var pickLine1 = pick.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = DateTime.Now;
			pickLine1.WZ_GS_NKAssignedTo = "E";
			releaseLines[0].Quantity = 4m;
			AssertEquals("AllowNew should be false as pickLine1 is Picked.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be false as pickLine1 is Picked.", false, releaseLines.AllowRemove);

			availInv.PickLineQuantity = 10m;
			releaseLines[0].Quantity = 5m;
			releaseLines[1].Quantity = 5m;
			releaseLines[1].PartAttribute1 = "BLUE";
			AssertEquals("AllowNew should be false as OrderedQty is Met.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be ture.", true, releaseLines.AllowRemove);

			var pickLine2 = pick.GetAllPickLines().Single(pl => pl.WZ_GS_NKAssignedTo == "");
			pickLine2.WZ_PickedDateTime = DateTime.Now;
			pickLine2.WZ_GS_NKAssignedTo = "E";
			AssertEquals("AllowNew should be false as all pick lines have been picked.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be false as all pick lines have been picked.", false, releaseLines.AllowRemove);

			releaseLines[1].Quantity = 4m;
			AssertEquals("AllowNew should be false as all pick lines have been picked.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be false as all pick lines have been picked.", false, releaseLines.AllowRemove);
		}

		public void TestAllowNewAndRemove_OrderLineDeleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			orderLine.Delete();
			AssertEquals("Order line is deleted.", true, orderLine.IsDeleted);

			AssertEquals("AllowNew should be false if order line is deleted.", false, releaseLines.AllowNew);
			AssertEquals("AllowRemove should be false if order line is deleted.", false, releaseLines.AllowRemove);
		}

		#endregion

		#region TestClearCollection

		public void TestClearCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, null, "PLT-123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, null, "PLT-456");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PalletID == "PLT-123");
			availableInventory.PickLineQuantity = 0m;

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition: 1 release Line.", 1, releaseLines.Count);
			AssertEquals("Precondition: 1 release Line.", 1, releaseLines.Cast<WhsReleaseLine>().Count(r => r.PartAttribute1 == "" && r.Quantity == 5m));

			var releaseLineBlue = releaseLines[0];
			releaseLineBlue.PartAttribute1 = "BLUE";

			var releaseLineEmpty = releaseLines.AddNew();
			releaseLineEmpty.Quantity = 10m;
			AssertEquals("There should be two Release Lines.", 2, releaseLines.Count);

			releaseLines.ClearCollection();
			AssertEquals("Has Changes for Release Lines should be false when Cleared.", false, releaseLines.HasChanges);
			AssertEquals("When removing Release Lines, they should get deleted so they don't remain in use.", true, releaseLineBlue.IsDeleted);
			AssertEquals("When removing Release Lines, they should get deleted so they don't remain in use.", true, releaseLineEmpty.IsDeleted);

			using (releaseLines.SuspendRebuild())
			{
				AssertEquals("When Release Lines is cleared it should be empty.", 0, releaseLines.Count);
			}

			AssertEquals("Release Lines should be Unregistered as child editable when Cleared.", false, orderLine.IsRegisteredEditableChildObject(releaseLines));
			AssertEquals("Sum of Units Met for Release Lines should be Zero when Cleared.", 0m, releaseLines.SumOfUnitsMet);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 10m;
			pickLine.WZ_WE_TransactionLine = orderLine.PK;

			using (releaseLines.SuspendRebuild())
			{
				AssertEquals("When Release Lines is cleared it should not be Hooked to Pick Lines Collection.", 0, releaseLines.Count);
			}

			AssertEquals("Has Changes for Release Lines should be false when Cleared.", false, releaseLines.HasChanges);
		}

		#endregion

		#region TestClearCollection_MultipleOrderLines

		public void TestClearCollection_MultipleOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 10m;

			var releaseLines1 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine1);
			var releaseLines2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2);
			AssertEquals("Precondition: 1 release Line.", 1, releaseLines1.Count);
			AssertEquals("Precondition: 1 release Line.", 1, releaseLines2.Count);

			releaseLines1.ClearCollection();
			AssertEquals("Has Changes for Release Lines should be false when Cleared.", false, releaseLines1.HasChanges);
			AssertEquals("Should not affect collection on second order line.", false, releaseLines2.HasChanges);

			using (releaseLines1.SuspendRebuild())
			using (releaseLines2.SuspendRebuild())
			{
				AssertEquals("When Release Lines is cleared it should be empty.", 0, releaseLines1.Count);
				AssertEquals("Should not affect collection on second order line.", 1, releaseLines2.Count);
			}

			AssertEquals("Release Lines should be Unregistered as child editable when Cleared.", false, orderLine1.IsRegisteredEditableChildObject(releaseLines1));
			AssertEquals("Should not affect collection on second order line.", true, orderLine2.IsRegisteredEditableChildObject(releaseLines2));

			AssertEquals("Sum of Units Met for Release Lines should be Zero when Cleared.", 0m, releaseLines1.SumOfUnitsMet);
			AssertEquals("Should not affect collection on second order line.", 5m, releaseLines2.SumOfUnitsMet);
		}

		#endregion

		#region TestClearCollection_SuspendsPackableItemParentsCountChanged

		public void TestClearCollection_SuspendsPackableItemParentsCountChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, null, "PLT-123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, null, "PLT-456");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition: 1 release Line.", 1, releaseLines.Count);
			AssertEquals("Precondition: 1 release Line.", 1, releaseLines.Cast<WhsReleaseLine>().Count(r => r.PartAttribute1 == "" && r.Quantity == 15m));

			var releaseLine1 = releaseLines[0];
			releaseLine1.Quantity = 12m;
			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.PartAttribute1 = "RED";
			releaseLine2.Quantity = 3m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));

			int countChangedHitCount = 0;
			IPackingParentWithPackableItems packingParent = order;
			packingParent.PackableItemParentsCountChanged += (sender, e) => countChangedHitCount++;
			releaseLines.ClearCollection();
			AssertEquals("Packable Item Parents Count Changed should only fire once.", 1, countChangedHitCount);

			// if we access PackableItemParents it should be rebuilt.
			AssertEquals("Release Lines Collection gets rebuilt when accessing Packable Items.", 2, order.PackageJob.PackableItemParents.Count);
			AssertEquals("Release Lines Collection gets rebuilt when accessing Packable Items.", 2, order.PackableItemParents.Count);
			AssertEquals("Packable Item Parents Count Changed should only fire once when rebuilding.", 2, countChangedHitCount);
		}

		#endregion

		#region TestCommittingElement

		public void TestCommittingElement()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine.ReleaseLines[0].Quantity = 1m;
			orderLine.ReleaseLines[0].PartAttribute1 = "BLUE";

			var pickLine = orderLine.PickLines.Single(p => !p.HasReleaseCapturedAttribs);
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var newReleaseLine = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			AssertEquals("IBinding List should add new element as non-committed.", true, releaseLines.IsNonCommittedCollectionElement(newReleaseLine));

			newReleaseLine.Quantity = 9m;
			newReleaseLine.PartAttribute1 = "RED";
			AssertEquals("Since Release Line is not committed, the Pick Line should not get Release Captured.", 9m, pickLine.UnreleaseCapturedQty);

			((ICancelAddNew)releaseLines).EndNew(1);
			AssertEquals("Release Line should be committed.", false, releaseLines.IsNonCommittedCollectionElement(newReleaseLine));
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 0m, pickLine.UnreleaseCapturedQty);
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 2, orderLine.PickLines.Count);
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 1, orderLine.PickLines.Count(a => a.WZ_ReleaseCapturedPartAttrib1 == "RED" && a.WZ_Units == 9m));
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 1, orderLine.PickLines.Count(a => a.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && a.WZ_Units == 1m));
			((ICancelAddNew)releaseLines).EndNew(1);
			AssertEquals("Release Line should be committed.", false, releaseLines.IsNonCommittedCollectionElement(newReleaseLine));
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 0m, pickLine.UnreleaseCapturedQty);
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 2, orderLine.PickLines.Count);
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 1, orderLine.PickLines.Count(a => a.WZ_ReleaseCapturedPartAttrib1 == "RED" && a.WZ_Units == 9m));
			AssertEquals("Since Release Line is committed, the Pick Line should get Release Captured.", 1, orderLine.PickLines.Count(a => a.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && a.WZ_Units == 1m));

			releaseLines.DeleteReleaseLine(newReleaseLine);
			AssertEquals("Correct Release Line should be deleted.", true, newReleaseLine.IsDeleted);
			AssertEquals("Release Line should have been removed from the Collection.", 1, releaseLines.Count);
		}

		public void TestCommittingElement_CallsRefreshBindingIfItWasDeferred()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Precondition: Quantity has been reduced to 1.", 1m, releaseLine1.Quantity);

			var listChangedHitCount = 0;
			((IBindingList)orderLine.ReleaseLines).ListChanged += (sender, e) =>
			{
				if (e.ListChangedType == ListChangedType.Reset)
				{
					listChangedHitCount++;
				}
			};

			var newReleaseLine = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertEquals("Should not have called Reset List Change since the new element is not yet committed.", 0, listChangedHitCount);

			newReleaseLine.HasChanges = true;
			((ICancelAddNew)orderLine.ReleaseLines).EndNew(1);
			AssertEquals("Release Line should no longer be committed.", false, orderLine.ReleaseLines.IsNonCommittedCollectionElement(newReleaseLine));
			AssertEquals("On successful commit of the new element, Reset List Changed should have been called as it was deferred till commit.", 1, listChangedHitCount);
		}

		public void TestCommittingElement_AddsToPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			var releaseLine2 = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			releaseLine2.PartAttribute1 = "RED";
			releaseLine2.Quantity = 3m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			((ICancelAddNew)orderLine.ReleaseLines).EndNew(orderLine.ReleaseLines.Count - 1); // commit element
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 }, order.PackableItemParents.Typed);

			var releaseLine3 = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 }, order.PackableItemParents.Typed);

			releaseLine3.PartAttribute1 = "RED"; // Duplicate
			releaseLine3.Quantity = 4m;
			((ICancelAddNew)orderLine.ReleaseLines).EndNew(orderLine.ReleaseLines.Count - 1); // commit element
			AssertEquals("Precondition: Release Line is Committed.", false, orderLine.ReleaseLines.IsNonCommittedCollectionElement(releaseLine3));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 }, order.PackableItemParents.Typed);

			releaseLine3.PartAttribute1 = "BLUE";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 }, order.PackableItemParents.Typed);

			var releaseLine4 = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 }, order.PackableItemParents.Typed);

			releaseLine4.PartAttribute2 = "XXX";
			releaseLine4.Quantity = 2m;
			((ICancelAddNew)orderLine.ReleaseLines).EndNew(orderLine.ReleaseLines.Count - 1); // commit element
			AssertEquals("Precondition: Release Line is Committed.", false, orderLine.ReleaseLines.IsNonCommittedCollectionElement(releaseLine4));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 }, order.PackableItemParents.Typed);
		}

		public void TestCommittingElement_CalculateExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_UnitPriceAfterDiscount = 2m;
			Helper.CreatePickNew(order);
			AssertEquals("Precondition", 20m, orderLine.WE_ExtendedLinePrice);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Precondition: Quantity has been reduced to 1.", 1m, releaseLine1.Quantity);

			var newReleaseLine = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertEquals("It should set the default for the remaining values.", 9m, newReleaseLine.Quantity);
			AssertEquals("Should be able to update the price, including the new line.", 20m, orderLine.WE_ExtendedLinePrice);
		}

		#endregion

		#region TestGetTotalUnitsMet

		public void TestGetTotalUnitsMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_UnitPriceAfterDiscount = 2m;
			Helper.CreatePickNew(order);
			AssertEquals("Precondition", 10m, orderLine.ReleaseLines.GetTotalUnitsMet());

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Quantity has been reduced to 1.", 1m, releaseLine1.Quantity);
			AssertEquals("Quantity has been reduced to 1.", 1m, orderLine.ReleaseLines.GetTotalUnitsMet());

			var newReleaseLine = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertEquals("It should set the default for the remaining values.", 9m, newReleaseLine.Quantity);
			AssertEquals("Should includ the new line.", 10m, orderLine.ReleaseLines.GetTotalUnitsMet());
		}

		#endregion

		#region TestCopyNonReleaseCapturedAttributes

		[TestDate(2019, 1, 1)]
		public void TestCopyNonReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var year = ZDate.Today.Year;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 1, 2), new ZDate(year, 1, 3), "", "RED", "MEDIUM", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "SN001";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			AssertEquals("Precondition.", "", releaseLine2.PartAttribute1);
			AssertEquals("Precondition.", "", releaseLine2.PartAttribute2);
			AssertEquals("Precondition.", "", releaseLine2.PartAttribute3);
			AssertEquals("Precondition.", ZDateTime.Empty, releaseLine2.ExpiryDate);
			AssertEquals("Precondition.", ZDateTime.Empty, releaseLine2.PackingDate);

			WhsReleaseLineCollection.CopyNonReleaseCapturedAttributes(releaseLine2, releaseLine1);
			AssertEquals("Should not set release captured attribute.", "", releaseLine2.PartAttribute1);
			AssertEquals("Should set attributes.", "RED", releaseLine2.PartAttribute2);
			AssertEquals("Should set attributes.", "MEDIUM", releaseLine2.PartAttribute3);
			AssertEquals("Should set attributes.", new ZDate(year, 1, 2), releaseLine2.ExpiryDate);
			AssertEquals("Should set attributes.", new ZDate(year, 1, 3), releaseLine2.PackingDate);
		}

		#endregion

		#region TestDeleteReleaseLine

		public void TestDeleteReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine1);
			var releaseLine1 = releaseLines[0];

			// cannot delete null release line
			AssertExceptionThrown(typeof(InvalidOperationException), "Release Lines should be deleted with the correct Collection.",
				() => releaseLines.DeleteReleaseLine(null));

			releaseLine1.Quantity = 5m;
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLinesForOtherOrderLine = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2);
			var releaseLineForOtherOrderLine = releaseLinesForOtherOrderLine[0];

			// cannot delete release line from another collection
			AssertExceptionThrown(typeof(InvalidOperationException), "Release Lines should be deleted with the correct Collection.",
				() => releaseLines.DeleteReleaseLine(releaseLineForOtherOrderLine));

			AssertEquals("Precondition: Sum of Units Met is correct.", 5m, releaseLines.SumOfUnitsMet);

			releaseLines.DeleteReleaseLine(releaseLine1);
			AssertEquals("Deleting Persistent Release Line should have deleted Non-Persistent one.", true, releaseLine1.IsDeleted);
			AssertEquals("Delete should correctly reduce Sum of Units Met.", 0m, releaseLines.SumOfUnitsMet);
			AssertEquals("Release Line should have been removed from the collection.", 0, releaseLines.Count);

			var releaseLine3 = releaseLines.AddNew();
			releaseLine3.Quantity = 6m;
			releaseLine3.PartAttribute1 = "RED";

			var releaseLine4 = releaseLines.AddNew();
			releaseLine4.Quantity = 4m;
			releaseLine4.PartAttribute1 = "BLUE";
			var pickLine1 = orderLine1.PickLines.Single(p => p.WZ_ReleaseCapturedPartAttrib1 == "RED");
			var pickLine2 = orderLine1.PickLines.Single(p => p.WZ_ReleaseCapturedPartAttrib1 == "BLUE");

			AssertEquals("PickLine is fully Release Captured.", 0m, pickLine1.UnreleaseCapturedQty);
			AssertEquals("PickLine is fully Release Captured.", 0m, pickLine2.UnreleaseCapturedQty);
			AssertEquals("PickLine has correct Release Captured Attribs.", 6m, pickLine1.WZ_Units);
			AssertEquals("PickLine has correct Release Captured Attribs.", 4m, pickLine2.WZ_Units);
			AssertEquals("Sum of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);

			var hasChangedWasSet = false;
			releaseLines.HasChangesChanged += (sender, e) => { hasChangedWasSet = true; };
			releaseLines.DeleteReleaseLine(releaseLine3);
			AssertEquals("PickLine is not Release Captured.", 6m, pickLine1.UnreleaseCapturedQty);
			AssertEquals("PickLine is not Release Captured.", true, !pickLine1.HasReleaseCapturedAttribs);
			AssertEquals("PickLine has correct Release Captured Attribs.", 4m, pickLine2.WZ_Units);
			AssertEquals("PickLine has correct Release Captured Attribs.", "BLUE", pickLine2.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Sum of Units Met is correct.", 4m, releaseLines.SumOfUnitsMet);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine4 }, releaseLines);
			AssertEquals("Deleting Persistent Release Line should set HasChanges on Release Lines collection.", true, hasChangedWasSet);

			releaseLine4.Delete();
			AssertEquals("PickLine is not Release Captured and merged with the other one.", 10m, pickLine1.UnreleaseCapturedQty);
			AssertEquals("PickLine is not Release Captured.", true, !pickLine1.HasReleaseCapturedAttribs);
			AssertEquals("PickLine is deleted after merge.", true, pickLine2.IsDeleted);
			AssertEquals("Sum of Units Met is correct.", 0m, releaseLines.SumOfUnitsMet);
			AssertEquals("Release Line should have been removed from the collection.", 0, releaseLines.Count);
		}

		#endregion

		#region TestDeleteReleaseLine_ElementIsNotCommitted

		public void TestDeleteReleaseLine_ElementIsNotCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine.ReleaseLines[0].Quantity = 1m;
			orderLine.ReleaseLines[0].PartAttribute1 = "BLUE";

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var newReleaseLine1 = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			AssertEquals("Precondition: element is not committed.", true, releaseLines.IsNonCommittedCollectionElement(newReleaseLine1));
			AssertEquals("Release Lines contains Non-Committed Element.", 2, releaseLines.Count);

			newReleaseLine1.Quantity = 9m;
			AssertEquals("Sum Of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);

			releaseLines.DeleteReleaseLine(newReleaseLine1);
			AssertEquals("Non-Persistent Release Line is deleted.", true, newReleaseLine1.IsDeleted);
			AssertEquals("Sum Of Units Met is updated.", 1m, releaseLines.SumOfUnitsMet);
			AssertEquals("Release Lines should no longer contain Non-Committed Element.", 1, releaseLines.Count);

			var newReleaseLine2 = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			AssertEquals("Precondition: element is not committed.", true, releaseLines.IsNonCommittedCollectionElement(newReleaseLine2));
			AssertEquals("Release Lines contains Non-Committed Element.", 2, releaseLines.Count);

			newReleaseLine2.Quantity = 9m;
			AssertEquals("Sum Of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);

			newReleaseLine2.Delete();
			AssertEquals("Non-Persistent Release Line is deleted.", true, newReleaseLine2.IsDeleted);
			AssertEquals("Sum Of Units Met is updated.", 1m, releaseLines.SumOfUnitsMet);
			AssertEquals("Release Lines should no longer contain Non-Committed Element.", 1, releaseLines.Count);
		}

		#endregion

		#region TestDeleteReleaseLine_NegativeReleaseLine

		public void TestDeleteReleaseLine_NegativeReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine1 = releaseLines[0];
			AssertEquals("Precondition: Sum of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);

			releaseLine1.Quantity = 4m;
			AssertEquals("Sum of Units Met is correct.", 4m, releaseLines.SumOfUnitsMet);

			releaseLine1.PartAttribute1 = "GREEN";
			var pickLine1 = orderLine.PickLines.Single(p => p.WZ_ReleaseCapturedPartAttrib1 == "GREEN");
			var pickLine2 = orderLine.PickLines.Single(p => !p.HasReleaseCapturedAttribs);

			AssertEquals("Pick Line is Release Captured.", 6m, pickLine2.UnreleaseCapturedQty);

			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 6m;
			releaseLine2.PartAttribute1 = "ORANGE"; // that's the colour of Maciej's hair :P - BRS
			AssertEquals("Sum of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);
			AssertEquals("Pick Line is fully Release Captured.", 0m, pickLine2.UnreleaseCapturedQty);

			releaseLine1.Quantity = -8;
			AssertEquals("Sum of Units Met factors in negative Lines.", -2m, releaseLines.SumOfUnitsMet);
			AssertEquals("Pick Line should not be updated.", 0m, pickLine1.UnreleaseCapturedQty);

			releaseLines.DeleteReleaseLine(releaseLine1);
			AssertEquals("Sum of Units Met is corrected when Deleting.", 6m, releaseLines.SumOfUnitsMet);
			AssertEquals("Pick Line is no longer fully Release Captured.", 4m, pickLine1.UnreleaseCapturedQty);
		}

		#endregion

		#region TestDeleteReleaseLine_NegativeReleaseLine_AfterAttributesAreSetToEmpty

		public void TestDeleteReleaseLine_NegativeReleaseLine_AfterAttributesAreSetToEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine1 = releaseLines[0];
			AssertEquals("Precondition: Sum of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);

			releaseLine1.Quantity = 4m;
			AssertEquals("Sum of Units Met is correct.", 4m, releaseLines.SumOfUnitsMet);

			releaseLine1.PartAttribute1 = "GREEN";
			var pickLine1 = orderLine.PickLines.Single(p => p.WZ_ReleaseCapturedPartAttrib1 == "GREEN");
			var pickLine2 = orderLine.PickLines.Single(p => !p.HasReleaseCapturedAttribs);
			AssertEquals("Pick Line is Release Captured.", 0m, pickLine1.UnreleaseCapturedQty);
			AssertEquals("Pick Line is Release Captured.", 6m, pickLine2.UnreleaseCapturedQty);

			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 6m;
			releaseLine2.PartAttribute1 = "ORANGE";
			AssertEquals("Sum of Units Met is correct.", 10m, releaseLines.SumOfUnitsMet);
			AssertEquals("Pick Line is fully Release Captured.", 0m, pickLine2.UnreleaseCapturedQty);

			releaseLine1.Quantity = -8;
			AssertEquals("Sum of Units Met factors in negative Lines.", -2m, releaseLines.SumOfUnitsMet);
			AssertEquals("Pick Line should not be updated.", 0m, pickLine1.UnreleaseCapturedQty);
			AssertEquals("Pick Line should not be updated.", 0m, pickLine2.UnreleaseCapturedQty);

			releaseLine1.PartAttribute1 = "";
			AssertEquals("Pick Line is no fully longer Release Captured.", 4m, pickLine1.UnreleaseCapturedQty);

			releaseLines.DeleteReleaseLine(releaseLine1);
			AssertEquals("Sum of Units Met is corrected when Deleting.", 6m, releaseLines.SumOfUnitsMet);
			AssertEquals("Pick Line is still not fully Release Captured.", 4m, pickLine1.UnreleaseCapturedQty);
		}

		#endregion

		#region TestGetKey

		public void TestGetKey_InventoryLineAndPickLine_Null()
		{
			var key = WhsReleaseLineCollection.GetKey(null, null);
			AssertEquals("", key);

			var inventoryLine = Factory.New<WhsReceiveLine>();
			key = WhsReleaseLineCollection.GetKey(inventoryLine, null);
			AssertEquals("", key);

			var pickLine = Factory.New<WhsPickLine>();
			key = WhsReleaseLineCollection.GetKey(null, pickLine);
			AssertEquals("", key);
		}

		public void TestGetKey_InventoryLineAndPickLine_PickLineHasNoRCAs()
		{
			var inventoryLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();

			inventoryLine.WE_PartAttrib1 = "A1";
			var key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|||||", key);

			inventoryLine.WE_PartAttrib2 = "A2";
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2||||", key);

			inventoryLine.WE_PartAttrib3 = "A3";
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|A3|||", key);

			inventoryLine.WE_SerialNumber = "SS";
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|A3|SS||", key);

			var date = new ZDate(2023, 5, 12);
			inventoryLine.WE_ExpiryDate = date;
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|A3|SS|12-May-23|", key);

			inventoryLine.WE_PackingDate = date.AddDays(1);
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|A3|SS|12-May-23|13-May-23", key);
		}

		public void TestGetKey_InventoryLineAndPickLine_InventoryHasNoAttribs()
		{
			var inventoryLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();

			pickLine.WZ_ReleaseCapturedPartAttrib1 = "A1";
			var key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|||||", key);

			pickLine.WZ_ReleaseCapturedPartAttrib2 = "A2";
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2||||", key);

			pickLine.WZ_ReleaseCapturedPartAttrib3 = "A3";
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|A3|||", key);

			pickLine.WZ_ReleaseCapturedSerialNumber = "SS";
			key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|A3|SS||", key);
		}

		public void TestGetKey_InventoryLineAndPickLine()
		{
			var date = new ZDate(2023, 5, 12);
			var inventoryLine = Factory.New<WhsReceiveLine>();
			inventoryLine.WE_PartAttrib1 = "A1";
			inventoryLine.WE_PartAttrib2 = "A2";
			inventoryLine.WE_ExpiryDate = date;
			inventoryLine.WE_PackingDate = date.AddDays(1);

			var pickLine = Factory.New<WhsPickLine>();

			pickLine.WZ_ReleaseCapturedPartAttrib1 = "P1";
			pickLine.WZ_ReleaseCapturedPartAttrib2 = "P2";
			pickLine.WZ_ReleaseCapturedPartAttrib3 = "P3";
			pickLine.WZ_ReleaseCapturedSerialNumber = "XX";

			var key = WhsReleaseLineCollection.GetKey(inventoryLine, pickLine);
			AssertEquals("A1|A2|P3|XX|12-May-23|13-May-23", key);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection

		public void TestGetNewReleaseLinesCollection()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WhsReleaseLineCollection.GetNewReleaseLinesCollection(null));

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Ensure multiple pick lines for some order lines
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Ensure multiple pick lines for some order lines
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Ensure multiple pick lines for some order lines
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m); // Ensure multiple pick lines for some order lines
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 80m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order1Line = order1.Lines[0];
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order2Line1 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Should be multiple pick lines for some order lines", 5, pick.GetAllPickLines().Count());
			AssertEquals("Should be multiple pick lines for some order lines", true, pick.GetAllPickLines().Count() > pick.Orders.Cast<WhsOrder>().Sum(o => o.Lines.Count));

			var releaseLines1 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(order1Line);
			var releaseLines2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(order2Line1);
			var releaseLines3 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(order2Line2);
			AssertEquals("Release Lines should be registered as child editable.", true, order1Line.IsRegisteredEditableChildObject(releaseLines1));
			AssertEquals("Release Lines should be registered as child editable.", true, order2Line1.IsRegisteredEditableChildObject(releaseLines2));
			AssertEquals("Release Lines should be registered as child editable.", true, order2Line2.IsRegisteredEditableChildObject(releaseLines3));
			AssertNotEquals("Should have unique collections", releaseLines1, releaseLines2);
			AssertNotEquals("Should have unique collections", releaseLines1, releaseLines3);
			AssertNotEquals("Should have unique collections", releaseLines2, releaseLines3);

			var releaseLine1 = (WhsReleaseLine)releaseLines1.Single();
			var releaseLine2 = (WhsReleaseLine)releaseLines2.Single();
			var releaseLine3 = (WhsReleaseLine)releaseLines3.Single();
			AssertEquals("Should correctly populate Quantity.", 10m, releaseLine1.Quantity);
			AssertEquals("Should correctly populate Quantity.", 10m, releaseLine2.Quantity);
			AssertEquals("Should correctly populate Quantity.", 80m, releaseLine3.Quantity);
			AssertEquals("Should be fully released.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be fully released.", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("Should be fully released.", 0m, releaseLine3.UnreleasedQty);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_Attributes

		public void TestGetNewReleaseLinesCollection_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "RED", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Release Lines should be registered as child editable.", true, orderLine.IsRegisteredEditableChildObject(releaseLines));
			AssertEquals("Should contain 2 release Lines, 5 BLUE and 10 RED.", 2, releaseLines.Count);
			AssertEquals("Should have 5 BLUE as a Release Line.", 1, releaseLines.Cast<WhsReleaseLine>().Count(r => r.PartAttribute1 == "BLUE" && r.Quantity == 5m));
			AssertEquals("Should have 10 RED as a Release Line.", 1, releaseLines.Cast<WhsReleaseLine>().Count(r => r.PartAttribute1 == "RED" && r.Quantity == 10m));
			AssertEquals("Sum of Units Met should be 10 + 5.", 15m, releaseLines.SumOfUnitsMet);
			AssertEquals("Should be 0 units unreleased.", 0m, releaseLines[0].UnreleasedQty);
			AssertEquals("Should be 0 units unreleased.", 0m, releaseLines[1].UnreleasedQty);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_OverReleased

		public void TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_OverReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;

			orderLine.ReleaseLines[0].PartAttribute1 = "RED";
			AssertEquals("Precondition, 10 Release Captured Units.", 10m, pick.GetAllPickLines().Where(l => l.HasReleaseCapturedAttribs).Sum(l => l.WZ_Units));
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderLineInFactory2 = factory2.Load<WhsOrderLine>(orderLine.PK);
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

			pickInFactory2.OrderedInventories[0].AvailableInventories[0].PickLineQuantity -= 4m;
			AssertEquals("Not de-allocated.", 10m, pickInFactory2.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			var releaseLines = orderLineInFactory2.ReleaseLines;
			AssertEquals("Release Lines should be registered as child editable.", true, orderLineInFactory2.IsRegisteredEditableChildObject(releaseLines));
			AssertEquals("Should contain 1 release Line.", 1, releaseLines.Count);

			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single();
			AssertEquals("Sum of Units Met should be 10.", 10m, releaseLines.SumOfUnitsMet);
			AssertEquals("Should be 10 units released.", 10m, releaseLine.Quantity);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine.UnreleasedQty);
			AssertEquals("Attributes set from RCA.", "RED", releaseLine.PartAttribute1);

			var pickLines = pickInFactory2.GetAllPickLines();
			releaseLine.Quantity = 8m;
			AssertEquals("Sum of Units Met should be 8.", 8m, releaseLines.SumOfUnitsMet);
			AssertEquals("Should be 8 units released.", 8m, releaseLine.Quantity);
			AssertEquals("Should be 0 units unreleased.", 2m, releaseLine.UnreleasedQty);
			AssertEquals("8 Release Captured Units.", 8m, pickLines.Single(l => l.HasReleaseCapturedAttribs).WZ_Units);
			AssertEquals("2 un Release Captured Units.", 2m, pickLines.Single(l => !l.HasReleaseCapturedAttribs).WZ_Units);
			AssertEquals("Release Captured Attribute is Red.", "RED", pickLines.Single(l => l.HasReleaseCapturedAttribs).WZ_ReleaseCapturedPartAttrib1);

			releaseLine.Quantity = 6m;
			AssertEquals("Sum of Units Met should be 6.", 6m, releaseLines.SumOfUnitsMet);
			AssertEquals("Should be 6 units released.", 6m, releaseLine.Quantity);
			AssertEquals("Should be no units unreleased.", 4m, releaseLine.UnreleasedQty);
			AssertEquals("6 Release Captured Units.", 6m, pickLines.Single(l => l.HasReleaseCapturedAttribs).WZ_Units);
			AssertEquals("Release Captured Attribute is Red.", "RED", pickLines.Single(l => l.HasReleaseCapturedAttribs).WZ_ReleaseCapturedPartAttrib1);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_OverReleased_MultipleLinesAndOrders

		public void TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_OverReleased_MultipleLinesAndOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today, data.Part1, 10m, "", data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today, data.Part1, 10m, "", data.Whs1.FindLocation("A-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today, data.Part1, 10m, "", data.Whs1.FindLocation("A-3"), "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order1, order2);
			order1Line1.ReleaseLines[0].PartAttribute1 = "RED";
			order1Line2.ReleaseLines[0].PartAttribute1 = "RED";
			order2Line.ReleaseLines[0].PartAttribute1 = "RED";

			AssertEquals("Precondition, 3 AvailableInventory.", 3, pick.OrderedInventories[0].AvailableInventories.Count);
			var pickLines = pick.GetAllPickLines().ToList();
			AssertEquals("Precondition, 3 Pick Lines.", 3, pickLines.Count);
			AssertEquals("Precondition, 10 Release Captured Units on each PickLine.", true, pickLines[0].HasReleaseCapturedAttribs && pickLines[0].WZ_Units == 10m);
			AssertEquals("Precondition, 10 Release Captured Units on each PickLine.", true, pickLines[1].HasReleaseCapturedAttribs && pickLines[1].WZ_Units == 10m);
			AssertEquals("Precondition, 10 Release Captured Units on each PickLine.", true, pickLines[2].HasReleaseCapturedAttribs && pickLines[2].WZ_Units == 10m);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var order1Line1InFactory2 = factory2.Load<WhsOrderLine>(order1Line1.PK);
			var order1Line2InFactory2 = factory2.Load<WhsOrderLine>(order1Line2.PK);
			var order2LineInFactory2 = factory2.Load<WhsOrderLine>(order2Line.PK);
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

			var availableInventory1 = pickInFactory2.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pickInFactory2.OrderedInventories[0].AvailableInventories[1];
			var availableInventory3 = pickInFactory2.OrderedInventories[0].AvailableInventories[2];
			availableInventory1.PickLineQuantity = 9m;
			availableInventory2.PickLineQuantity = 8m;
			availableInventory3.PickLineQuantity = 7m;
			AssertEquals("Not de-allocated.", 10m, availableInventory1.PickLineQuantity);
			AssertEquals("Not de-allocated.", 10m, availableInventory2.PickLineQuantity);
			AssertEquals("Not de-allocated.", 10m, availableInventory3.PickLineQuantity);

			var releaseLines1 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(order1Line1InFactory2);
			var releaseLine1 = releaseLines1.Cast<WhsReleaseLine>().Single();
			AssertEquals("Sum of Units Met should be 10", 10m, releaseLines1.SumOfUnitsMet);
			AssertEquals("Should be 10 units released.", 10m, releaseLine1.Quantity);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine1.UnreleasedQty);

			var releaseLines2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(order1Line2InFactory2);
			var releaseLine2 = releaseLines2.Cast<WhsReleaseLine>().Single();
			AssertEquals("Sum of Units Met should be 10", 10m, releaseLines2.SumOfUnitsMet);
			AssertEquals("Should be 10 units released.", 10m, releaseLine2.Quantity);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine2.UnreleasedQty);

			var releaseLines3 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(order2LineInFactory2);
			var releaseLine3 = releaseLines3.Cast<WhsReleaseLine>().Single();
			AssertEquals("Sum of Units Met should be 10", 10m, releaseLines3.SumOfUnitsMet);
			AssertEquals("Should be 10 units released.", 10m, releaseLine3.Quantity);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("Should be 0 units unreleased, as there are release captured attribs and can't be de-allocated.", 0m, releaseLine3.UnreleasedQty);

			releaseLine1.Quantity = 9m;
			AssertEquals("Should be 1 units unreleased.", 1m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be 1 units unreleased.", 1m, releaseLine2.UnreleasedQty);
			AssertEquals("Should be 1 units unreleased.", 1m, releaseLine3.UnreleasedQty);
			AssertEquals("29 Release Captured Units in total.", 29m, pickInFactory2.GetAllPickLines().Where(l => l.HasReleaseCapturedAttribs).Sum(l => l.WZ_Units));

			releaseLine2.Quantity = 5m;
			AssertEquals("Should be 6 units unreleased.", 6m, releaseLine1.UnreleasedQty);
			AssertEquals("Should be 6 units unreleased.", 6m, releaseLine2.UnreleasedQty);
			AssertEquals("Should be 6 units unreleased.", 6m, releaseLine3.UnreleasedQty);
			AssertEquals("24 Release Captured Units in total.", 24m, pickInFactory2.GetAllPickLines().Where(l => l.HasReleaseCapturedAttribs).Sum(l => l.WZ_Units));
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_PartiallyRCed

		public void TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_PartiallyRCed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 20m;

			orderLine1.ReleaseLines[0].PartAttribute1 = "RED";
			orderLine2.ReleaseLines[0].PartAttribute1 = "BLUE";
			orderLine1.ReleaseLines[0].Quantity = 5m;
			orderLine2.ReleaseLines[0].Quantity = 3m;

			AssertEquals("Precondition, 8 Release Captured Units.", 8m, pick.GetAllPickLines().Sum(pl => pl.ReleaseCapturedQty));
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderLine1InFactory2 = factory2.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine3InFactory2 = factory2.Load<WhsOrderLine>(orderLine2.PK);
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

			var releaseLines1 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine1InFactory2);
			var releaseLines2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine3InFactory2);
			AssertEquals("Should contain 2 release Lines.", 2, releaseLines1.Count);
			AssertEquals("Should contain 2 release Lines.", 2, releaseLines2.Count);

			var releaseCapturedLineRed = releaseLines1.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "RED");
			var releaseCapturedLineBlue = releaseLines2.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "BLUE");
			var emptyReleaseLine1 = releaseLines1.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1.IsEmpty);
			var emptyReleaseLine2 = releaseLines2.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1.IsEmpty);

			AssertEquals("Sum of Units Met should be 10", 10m, releaseLines1.SumOfUnitsMet);
			AssertEquals("Sum of Units Met should be 8", 10m, releaseLines2.SumOfUnitsMet);

			AssertEquals("Should be 5 units released.", 5m, releaseCapturedLineRed.Quantity);
			AssertEquals("Should be 0 units unreleased.", 0m, releaseCapturedLineRed.UnreleasedQty);

			AssertEquals("Should be 5 units released.", 5m, emptyReleaseLine1.Quantity);
			AssertEquals("Should be 0 units unreleased.", 0m, emptyReleaseLine1.UnreleasedQty);

			AssertEquals("Should be 3 units released.", 3m, releaseCapturedLineBlue.Quantity);
			AssertEquals("Should be 0 units unreleased.", 0m, releaseCapturedLineBlue.UnreleasedQty);

			AssertEquals("Should be 7 units released.", 7m, emptyReleaseLine2.Quantity);
			AssertEquals("Should be 0 units unreleased.", 0m, emptyReleaseLine2.UnreleasedQty);

			emptyReleaseLine2.Quantity -= 1m;
			AssertEquals("Should be 1 unit unreleased.", 1m, emptyReleaseLine2.UnreleasedQty);
			AssertEquals("Should be 1 unit unreleased.", 1m, releaseCapturedLineBlue.UnreleasedQty);
			AssertEquals("Should be 1 unit unreleased.", 1m, releaseCapturedLineRed.UnreleasedQty);
			AssertEquals("Should be 1 unit unreleased.", 1m, emptyReleaseLine1.UnreleasedQty);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_WithNonReleaseCapturedAttributes

		public void TestGetNewReleaseLinesCollection_ReleaseCapturedAttributes_WithNonReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, string.Empty, "PA2", "PA3", string.Empty);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;

			var releaseLineCollection = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Should be 1 line.", 1, releaseLineCollection.Count);
			AssertEquals("Should have *not* have populated release captured part attributes.", string.Empty, releaseLineCollection[0].PartAttribute1);
			AssertEquals("Should have populated part attributes.", "PA2", releaseLineCollection[0].PartAttribute2);
			AssertEquals("Should have populated part attributes.", "PA3", releaseLineCollection[0].PartAttribute3);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_WithUnreleasedQuantity

		public void TestGetNewReleaseLinesCollection_WithUnreleasedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var receiveOnClient1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var receiveOnClient2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m);
			var receiveWithOtherPart = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 20m);

			AssertIsFinalisedPrecondition(receiveOnClient1);
			Factory.Save();

			var orderClient1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = orderClient1.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(orderClient1, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(orderClient1, data.Part2, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(orderClient1, data.Part1, 10m);

			var orderClient2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 10m);
			Helper.CreatePickNew(orderClient1, orderClient2);

			var orderOnOtherPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(orderOnOtherPick);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = true };
			var orderClient1InFactory2 = factory2.Load<WhsOrder>(orderClient1.PK);
			var orderClient2InFactory2 = factory2.Load<WhsOrder>(orderClient2.PK);
			var orderLine1InFactory2 = factory2.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InFactory2 = factory2.Load<WhsOrderLine>(orderLine2.PK);
			var orderLine3InFactory2 = factory2.Load<WhsOrderLine>(orderLine3.PK);
			var orderLine4InFactory2 = factory2.Load<WhsOrderLine>(orderLine4.PK);

			var releaseLinesClient1Part1Line1 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine1InFactory2);
			releaseLinesClient1Part1Line1[0].Quantity -= 5m;
			AssertEquals("Precondition", 5m, releaseLinesClient1Part1Line1[0].UnreleasedQty);

			var releaseLinesClient1Part1Line2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine2InFactory2);
			AssertEquals("Precondition.", 10m, releaseLinesClient1Part1Line2[0].Quantity);
			AssertEquals("Should have populated UnreleasedQty.", 5m, releaseLinesClient1Part1Line2[0].UnreleasedQty);

			releaseLinesClient1Part1Line2[0].Quantity += 6m;
			AssertEquals("Precondition", -1m, releaseLinesClient1Part1Line2[0].UnreleasedQty);

			var releaseLinesClient1Part1Line4 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine4InFactory2);
			AssertEquals("Precondition.", 10m, releaseLinesClient1Part1Line4[0].Quantity);
			AssertEquals("Should have populated UnreleasedQty.", -1m, releaseLinesClient1Part1Line4[0].UnreleasedQty);

			var releaseLinesClient2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderClient2InFactory2.Lines[0]);
			AssertEquals("Precondition.", 10m, releaseLinesClient2[0].Quantity);
			AssertEquals("Should *not* have populated UnreleasedQty as the client is different.", 0m, releaseLinesClient2[0].UnreleasedQty);

			var releaseLinesPart2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine3InFactory2);
			AssertEquals("Precondition.", 10m, releaseLinesPart2[0].Quantity);
			AssertEquals("Should *not* have populated UnreleasedQty as the Part is different.", 0m, releaseLinesPart2[0].UnreleasedQty);

			var releaseLinesOnPick2 = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderOnOtherPick.Lines[0]);
			AssertEquals("Precondition.", 10m, releaseLinesOnPick2[0].Quantity);
			AssertEquals("Should *not* have populated UnreleasedQty as the Pick is different.", 0m, releaseLinesOnPick2[0].UnreleasedQty);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_NoPick

		public void TestGetNewReleaseLinesCollection_NoPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("There are no release Lines on an un-picked Order.", 0, releaseLines.Count);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_NoProduct

		public void TestGetNewReleaseLinesCollection_NoProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine.WE_OP = ZGuid.Empty;
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("There are no release Lines on an Order Line with no Product.", 0, releaseLines.Count);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_OrderLineIsDeleted

		public void TestGetNewReleaseLinesCollection_OrderLineIsDeleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			orderLine.Delete();
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("OrderLine is Deleted should have no Release Lines.", 0, releaseLines.Count);
			AssertEquals("Public Methods and properties should handle deleted Order Line.", false, releaseLines.IsOrderLineOverReleased);
			AssertNoExceptionThrown(() => releaseLines.RunPreSaveValidation());
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_PickByBOM

		public void TestGetNewReleaseLinesCollection_PickByBOM()
		{
			TestGetNewReleaseLinesCollection_PickByBOM(pickPartiallyByBom: false);
		}

		public void TestGetNewReleaseLinesCollection_PickByBOM_PickedPartiallyByBOM()
		{
			TestGetNewReleaseLinesCollection_PickByBOM(pickPartiallyByBom: true);
		}

		void TestGetNewReleaseLinesCollection_PickByBOM(bool pickPartiallyByBom)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, data.Whs1.FindLocation("A-1"));

			if (pickPartiallyByBom)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, bike, 5m, data.Whs1.FindLocation("A-1"));
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, bike, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", true, orderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition", true, orderLine.ChildComponentLines.Count > 0);

			if (pickPartiallyByBom)
			{
				AssertEquals("Should have partially picked using kits.", true, pick.GetAllPickLines().Any(pl => pl.SupplierPart.PK == bike.PK));
				AssertEquals("Should have partially picked using components.", true, pick.GetAllPickLines().Any(pl => pl.SupplierPart.PK == wheel.PK));
			}

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Should have one release line.", 1, releaseLines.Count);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Should be 10 units released.", 10m, releaseLine.Quantity);
			AssertEquals("Should be completely released.", 0m, releaseLine.UnreleasedQty);
		}

		[GuiTest]
		public void TestGetNewReleaseLinesCollection_PickByBOM_PickedPartiallyByBOM_NotReadonly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, attributeName: "Colour");
			Helper.SetProductAttributeUse(data.Org1, bike, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 5m, data.Whs1.FindLocation("A-1"));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, bike, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", true, orderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition", false, orderLine.ChildComponentLines.Count > 0);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 20m, data.Whs1.FindLocation("A-1"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive2.IsFinalised);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			pick.AutoAllocateItems();

			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Precondition", true, orderLine.ChildComponentLines.Count > 0);
			AssertEquals("Should have partially picked using kits.", true, pick.GetAllPickLines().Any(pl => pl.SupplierPart.PK == bike.PK));
			AssertEquals("Should have partially picked using components.", true, pick.GetAllPickLines().Any(pl => pl.SupplierPart.PK == wheel.PK));

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Should have one release line.", 1, releaseLines.Count);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Should be 10 units released.", 10m, releaseLine.Quantity);
			AssertEquals("Should be completely released.", 0m, releaseLine.UnreleasedQty);
			AssertEquals("Should NOT be readonly.", false, releaseLine.PartAttribute1Info.ReadOnly);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_IsForComponentLines

		public void TestGetNewReleaseLinesCollection_IsForComponentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, bike, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, wheel, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 5m, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, ZDate.Empty, ZDate.Empty, "RED", "", "", "");

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, bike, 8m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", true, orderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition", true, orderLine.ChildComponentLines.Count > 0);

			AssertEquals("Should have partially picked using kits.", true, pick.GetAllPickLines().Any(pl => pl.SupplierPart.PK == bike.PK));
			AssertEquals("Should have partially picked using components.", true, pick.GetAllPickLines().Any(pl => pl.SupplierPart.PK == wheel.PK));

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Should have 2 release line.", 2, releaseLines.Count);

			var releaseLine = releaseLines.Cast<WhsReleaseLine>().FirstOrDefault(r => r.PartAttribute1 == "BLUE" && r.Quantity == 5m);
			AssertEquals("Should be false.", false, releaseLine.IsForComponentLines);

			releaseLine = releaseLines.Cast<WhsReleaseLine>().FirstOrDefault(r => r.PartAttribute1 == "" && r.Quantity == 3m); // PartAttribute should be empty for ReleaseLine created for Kit, Quantity should indicate Kit built from component
			AssertEquals("Should be true.", true, releaseLine.IsForComponentLines);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_SuspendsShortfallCalculation

		public void TestGetNewReleaseLinesCollection_SuspendsShortfallCalculation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			releaseLines.ClearCollection();

			bool isValidationSuspendedDuringBuildOfCollection = false;
			releaseLines.CountChanged += (sender, e) => isValidationSuspendedDuringBuildOfCollection = orderLine.IsValidationSuspended;

			var poke = releaseLines.Count;
			AssertEquals("Order Line Validation should be suspended while building the Collection.", true, isValidationSuspendedDuringBuildOfCollection);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_WhenIsBOMComponentLineOnSalesOrder

		public void TestGetNewReleaseLinesCollection_WhenIsBOMComponentLineOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, mainProduct, 15m);
			Factory.Save();

			Helper.CreatePickNew(orderLine.PickableDocket);
			var childLine = orderLine.ChildComponentLines.Single();
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(childLine);
			AssertEquals("BOM Component Lines will not have Release Lines.", 0, releaseLines.Count);
		}

		#endregion

		#region TestGetNewReleaseLinesCollection_Performance

		public void TestGetNewReleaseLinesCollection_Performance_ListCountChanged()
		{
			// Test added to make sure DelayListChangedEvents is not needed in GetNewReleaseLinesCollection
			// if this changes consider adding it back in but what for defect from WhsOrderCollection see WI00692793
			const int numberOfLinesToCreate = 50;

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO");
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), data.Part1, numberOfLinesToCreate);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OOI");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, numberOfLinesToCreate);

			var pick = Helper.CreatePickNew(order);

			var serialNumber = 0;
			foreach (var releaseLine in orderLine.ReleaseLines.Cast<WhsReleaseLine>())
			{
				releaseLine.Quantity = 1m;
				releaseLine.SerialNumber = "SN" + (++serialNumber);
				AssertNoErrors(releaseLine.PartAttribute1Info);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newFactory_OrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);

			var pickLineCollectionCount = 0;
			newFactory_OrderLine.PickLines.CountChanged += (s, e) => pickLineCollectionCount++;

			_ = WhsReleaseLineCollection.GetNewReleaseLinesCollection(newFactory_OrderLine);

			AssertEquals("PickLines.CountChanged fired no times", 0, pickLineCollectionCount);
		}

		public void TestGetNewReleaseLinesCollection_Performance_PersistentPropertiesHitCount()
		{
			const int numberOfLinesToCreate = 50;

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO");
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), data.Part1, numberOfLinesToCreate);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OOI");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, numberOfLinesToCreate);

			var pick = Helper.CreatePickNew(order);

			var serialNumber = 0;
			foreach (var releaseLine in orderLine.ReleaseLines.Cast<WhsReleaseLine>())
			{
				releaseLine.Quantity = 1m;
				releaseLine.SerialNumber = "SN" + (++serialNumber);
				AssertNoErrors(releaseLine.PartAttribute1Info);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newFactory_OrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);

			AssertPersistentPropertiesHitCount("Properties hits should be consistent", null, 245, () =>
			{
				_ = WhsReleaseLineCollection.GetNewReleaseLinesCollection(newFactory_OrderLine);
			}, newFactory);
		}

		#endregion

		#region TestGetPackableItems

		public void TestGetPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "LARGE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 4m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			var releaseLine2 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");
			var releaseLine3 = orderLine2.ReleaseLines[0];

			AssertExceptionThrown(typeof(InvalidOperationException), "Only pass in a Release Line that is Related to this Collection.", () => orderLine1.ReleaseLines.GetPackableItems(releaseLine3));
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"), orderLine1.ReleaseLines.GetPackableItems(releaseLine1));
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE"), orderLine1.ReleaseLines.GetPackableItems(releaseLine2));
			AssertContainsExactElementsInAnyOrder(orderLine2.PickLines, orderLine2.ReleaseLines.GetPackableItems(releaseLine3));

			releaseLine1.Quantity = 4m;
			var blueReleaseLine = orderLine1.ReleaseLines.AddNew("MEDIUM", "", "", "", ZDate.Empty, ZDate.Empty);
			blueReleaseLine.PartAttribute2 = "BLUE";
			blueReleaseLine.Quantity = 6m;
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => !pl.HasReleaseCapturedAttribs && pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"), orderLine1.ReleaseLines.GetPackableItems(releaseLine1));

			var pickLineForMedium = orderLine1.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "MEDIUM");
			Assert("Precondition: We have release Captured Attribs.", pickLineForMedium.Any(p => p.HasReleaseCapturedAttribs));
			AssertContainsExactElementsInAnyOrder(pickLineForMedium.Where(p => p.WZ_ReleaseCapturedPartAttrib2 == "BLUE"), orderLine1.ReleaseLines.GetPackableItems(blueReleaseLine));

			releaseLine2.Quantity = 3m;
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE"), orderLine1.ReleaseLines.GetPackableItems(releaseLine2));

			var redReleaseLine = orderLine1.ReleaseLines.AddNew("LARGE", "", "", "", ZDate.Empty, ZDate.Empty);
			redReleaseLine.PartAttribute2 = "RED";
			redReleaseLine.Quantity = 7m;

			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => !pl.HasReleaseCapturedAttribs && pl.InventoryLine.WE_PartAttrib1 == "LARGE"), orderLine1.ReleaseLines.GetPackableItems(releaseLine2));

			var releaseCapturedAttribsForLarge = orderLine1.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE");
			Assert("Precondition: We have release Captured Attribs.", releaseCapturedAttribsForLarge.Any(p => p.HasReleaseCapturedAttribs));
			AssertContainsExactElementsInAnyOrder(releaseCapturedAttribsForLarge.Where(pl => pl.HasReleaseCapturedAttribs), orderLine1.ReleaseLines.GetPackableItems(redReleaseLine));

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine1, 4m);

			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "" && pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"), orderLine1.ReleaseLines.GetPackableItems(releaseLine1));

			blueReleaseLine.Quantity = 5m;
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "" && pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"), orderLine1.ReleaseLines.GetPackableItems(releaseLine1));
			Assert("Precondition: We have release Captured Attribs.", pickLineForMedium.Any(p => p.HasReleaseCapturedAttribs));
			AssertContainsExactElementsInAnyOrder(pickLineForMedium.Where(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "BLUE"), orderLine1.ReleaseLines.GetPackableItems(blueReleaseLine));

			releaseLine1.Quantity = 5m;
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines.Where(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "" && pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"), orderLine1.ReleaseLines.GetPackableItems(releaseLine1));
			Assert("Precondition: We have release Captured Attribs.", pickLineForMedium.Any(p => p.HasReleaseCapturedAttribs));
			AssertContainsExactElementsInAnyOrder(pickLineForMedium.Where(pl => pl.WZ_ReleaseCapturedPartAttrib2 == "BLUE"), orderLine1.ReleaseLines.GetPackableItems(blueReleaseLine));
		}

		public void TestGetPackableItems_FinalisedOrder()
		{
			TestGetPackableItems_FinalisedCore(false);
		}

		public void TestGetPackableItems_FinalisedPick()
		{
			TestGetPackableItems_FinalisedCore(true);
		}

		void TestGetPackableItems_FinalisedCore(bool finalisePick)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "LARGE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "LARGE", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			releaseLine1.PartAttribute2 = "BLUE";
			var releaseLine2 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");
			releaseLine2.PartAttribute2 = "RED";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			if (finalisePick)
			{
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(pick);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);

			var releaseLine1InNewFactory = orderLine1InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			var releaseLine2InNewFactory = orderLine1InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");
			var releaseLine3InNewFactory = orderLine2InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			var releaseLine4InNewFactory = orderLine2InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");

			AssertContainsExactElementsInAnyOrder(
				orderLine1InNewFactory.PickLines.Single(pl => pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"),
				orderLine1InNewFactory.ReleaseLines.GetPackableItems(releaseLine1InNewFactory));

			AssertContainsExactElementsInAnyOrder(
				orderLine1InNewFactory.PickLines.Single(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE"),
				orderLine1InNewFactory.ReleaseLines.GetPackableItems(releaseLine2InNewFactory));

			AssertContainsExactElementsInAnyOrder(
				orderLine2InNewFactory.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"),
				orderLine2InNewFactory.ReleaseLines.GetPackableItems(releaseLine3InNewFactory));

			AssertContainsExactElementsInAnyOrder(
				orderLine2InNewFactory.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE"),
				orderLine2InNewFactory.ReleaseLines.GetPackableItems(releaseLine4InNewFactory));
		}

		public void TestGetPackableItems_FinalisedPick_ModifiedAttributeDefinition()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "LARGE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "LARGE", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			releaseLine1.PartAttribute2 = "BLUE";
			var releaseLine2 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");
			releaseLine2.PartAttribute2 = "RED";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, false);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);

			var releaseLine1InNewFactory = orderLine1InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			var releaseLine2InNewFactory = orderLine1InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");
			var releaseLine3InNewFactory = orderLine2InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			var releaseLine4InNewFactory = orderLine2InNewFactory.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");

			AssertContainsExactElementsInAnyOrder(
				orderLine1InNewFactory.PickLines.Single(pl => pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"),
				orderLine1InNewFactory.ReleaseLines.GetPackableItems(releaseLine1InNewFactory));

			AssertContainsExactElementsInAnyOrder(
				orderLine1InNewFactory.PickLines.Single(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE"),
				orderLine1InNewFactory.ReleaseLines.GetPackableItems(releaseLine2InNewFactory));

			AssertContainsExactElementsInAnyOrder(
				orderLine2InNewFactory.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "MEDIUM"),
				orderLine2InNewFactory.ReleaseLines.GetPackableItems(releaseLine3InNewFactory));

			AssertContainsExactElementsInAnyOrder(
				orderLine2InNewFactory.PickLines.Where(pl => pl.InventoryLine.WE_PartAttrib1 == "LARGE"),
				orderLine2InNewFactory.ReleaseLines.GetPackableItems(releaseLine4InNewFactory));
		}

		#endregion

		#region TestIsAnyReleaseCapturedQuantityBelowPickedQuantity

		public void TestIsAnyReleaseCapturedQuantityBelowPickedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.Quantity = 9m;
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Pick Line is not picked so Release Captured Quantity can be less than picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			orderLine.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			AssertEquals("Pick Line was split so Release Captured Quantity is still correct.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));
			AssertEquals("Null Release Line should return false for IsAnyReleaseCapturedQuantityBelowPickedQuantity().", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(null));
			AssertEquals("Unreleased Qty is 1, so there will be a validation error on Save.", 1m, releaseLine.UnreleasedQty);

			// We will not update picked PickLines from changing releaseLines's Attributes/Quantity since this will trigger remerge of picked PickLines.
			// so mark PickLines as not picked to proceed.
			orderLine.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Empty);

			releaseLine.PartAttribute1 = "";
			AssertEquals("No RC Attribs on Release Line should return false for IsAnyReleaseCapturedQuantityBelowPickedQuantity().", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			releaseLine.PartAttribute1 = "RED";

			var otherReleaseLine = releaseLines.AddNew();
			otherReleaseLine.Quantity = 1m;

			otherReleaseLine.PartAttribute1 = "GREEN";

			orderLine.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			AssertEquals("Pick Line is picked and Release Captured Quantity matches picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));
			AssertEquals("Pick Line is picked and Release Captured Quantity matches picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(otherReleaseLine));

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLine.Quantity = 1m;
			}
			AssertEquals("Release Captured Quantity would have been reduced to 1 and matches picked quantity.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));
			AssertEquals("Unreleased Qty is 8, so there will be a validation error on Save.", 8m, releaseLine.UnreleasedQty);

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLine.Quantity = 0m;
				AssertEquals("Zero Quantity Release Line should return false for IsAnyReleaseCapturedQuantityBelowPickedQuantity().", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

				releaseLine.Quantity = -1m;
				AssertEquals("Negative Release Line should return false for IsAnyReleaseCapturedQuantityBelowPickedQuantity().", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

				releaseLine.Quantity = 9m;
			}
			AssertEquals("Pick Line is picked and Release Captured Quantity matches picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			WhsReleaseLine releaseLineUnCommitted;
			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLine.Quantity = 7m;
				AssertEquals("Pick Line is not picked so Release Captured Quantity can be less than picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

				using (new SemaphoreManager(pick.FinalisePickSemaphore))
				{
					AssertEquals("Release Captured Quantity would have been reduced to 7 and matches picked quantity.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));
					AssertEquals("Unreleased Qty is 2, so there will be a validation error on Save.", 2m, releaseLine.UnreleasedQty);
				}

				pick.WP_PickStatus = PickStatus.Codes.Finalised;
				AssertEquals("Release Captured Quantity would have been reduced to 7 and matches picked quantity.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));
				AssertEquals("Unreleased Qty is 2, so there will be a validation error on Save.", 2m, releaseLine.UnreleasedQty);
				pick.WP_PickStatus = PickStatus.Codes.PickSlip; // clean-up

				releaseLineUnCommitted = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
				releaseLineUnCommitted.Quantity = 1m;
				releaseLineUnCommitted.PartAttribute1 = "BLUE";
				AssertEquals("Pick Line is not picked so Release Captured Quantity can be less than picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));
			}
			AssertEquals("Pick Line is picked so Release Captured Quantity cannot be less than picked qty.", true, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLineUnCommitted.Quantity = 0m;
				AssertEquals("Zero Quantity on Uncommitted Release Line should return false for IsAnyReleaseCapturedQuantityBelowPickedQuantity().", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));

				releaseLineUnCommitted.Quantity = 2m;
			}
			AssertEquals("Pick Line is picked and Release Captured Quantity matches picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));

			((ICancelAddNew)releaseLines).EndNew(2);
			AssertEquals("Precondition: Element is Committed.", false, releaseLines.IsNonCommittedCollectionElement(releaseLineUnCommitted));
			AssertEquals("Pick Line is fully released.", 0m, orderLine.PickLines.Sum(pl => pl.UnreleaseCapturedQty));
			AssertEquals("Pick Line is picked and Release Captured Quantity matches picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLineUnCommitted.Quantity = 3m;
			}
			AssertEquals("Pick Line is picked and Release Captured Quantity is greater than picked qty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));
		}

		#endregion

		#region TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_ManyPickLines

		public void TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_ManyPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			var pickLineBatch123 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			pickLineBatch123.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));
			pickLineBatch123.WZ_PickedDateTime = ZDateTimeOffset.Empty; // clean-up

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK);
			var releaseLinesInOtherFactory = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLineInOtherFactory);
			var releaseLineInOtherFactory = releaseLinesInOtherFactory[0];
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineInOtherFactory));

			releaseLineInOtherFactory.PartAttribute1 = "RED";
			AssertEquals("PickLine and Release Captured Qty match.", false, releaseLinesInOtherFactory.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineInOtherFactory));

			var pickLine = orderLineInOtherFactory.PickLines.Single(p => p.HasReleaseCapturedAttribs);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("PickLine and Release Captured Qty match.", false, releaseLinesInOtherFactory.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineInOtherFactory));

			using (UnpickPickLinesForChangingRCAs(orderLineInOtherFactory))
			{
				releaseLineInOtherFactory.Quantity = 4m;
			}
			// while unpicked, we can reduce release captured attribs, so there are only 4 release captured, which matches the release line.
			AssertEquals("PickLine and Release Captured Qty match.", false, releaseLinesInOtherFactory.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineInOtherFactory));

			using (UnpickPickLinesForChangingRCAs(orderLineInOtherFactory))
			{
				releaseLineInOtherFactory.PartAttribute1 = "";
			}
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineInOtherFactory));
		}

		#endregion

		#region TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_InTransitLines

		public void TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_InTransitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			var pickLineBatch123 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			Helper.PickAndMakeInTransitTransfer(pickLineBatch123, ZDateTimeOffset.Now);
			var newPickLine = orderLine.PickLines.Single(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsValid);
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Should have added RCAs on In-Transit PickLine.", newPickLine.WZ_Units, newPickLine.ReleaseCapturedQty);
			AssertEquals("Should have added RCAs on In-Transit PickLine.", "RED", newPickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("PickLine and Release Captured Qty match.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLine.Quantity = 4m;
			}
			// while unpicked, we can reduce release captured attribs, so there are only 4 release captured, which matches the release line.
			AssertEquals("PickLine and Release Captured Qty match.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLine.PartAttribute1 = "";
			}
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			releaseLine.RunPreSaveValidation();
			AssertEquals("RelaseLines and PickLines are out of sync and has error.",
				"Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.", releaseLine.UnreleasedQtyInfo.Notifications.Single().Message);
		}

		IDisposable UnpickPickLinesForChangingRCAs(WhsOrderLine orderLine)
		{
			var pickLinesWithOriginalPickedInventory = orderLine.PickLines.Select(l => (l.PK, OriginalPickInentoryLine: l.WZ_WE_OriginalPickedInventoryLine)).ToDictionary(o => o.PK);

			orderLine.PickLines.ForEach(pl =>
			{
				pl.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				pl.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			});

			return new DisposableAction(() => orderLine.PickLines.ForEach(pl =>
			{
				pl.WZ_PickedDateTime = ZDateTimeOffset.Now;
				if (pickLinesWithOriginalPickedInventory.TryGetValue(pl.PK, out var value))
				{
					pl.WZ_WE_OriginalPickedInventoryLine = value.OriginalPickInentoryLine;
				}
			}));
		}

		#endregion

		#region TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_InTransitLines_UncommittedReleaseLine

		public void TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_InTransitLines_UncommittedReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine));

			var pickLineBatch123 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			Helper.PickAndMakeInTransitTransfer(pickLineBatch123, ZDateTimeOffset.Now);
			var newPickLine = orderLine.PickLines.Single(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsValid);

			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Should have added RCAs on In-Transit PickLine.", newPickLine.WZ_Units, newPickLine.ReleaseCapturedQty);
			AssertEquals("Should have added RCAs on In-Transit PickLine.", "RED", newPickLine.WZ_ReleaseCapturedPartAttrib1);

			WhsReleaseLine releaseLineUnCommitted;
			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLine.Quantity = 4m;
				releaseLineUnCommitted = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
				releaseLineUnCommitted.Quantity = 1m;
				releaseLineUnCommitted.PartAttribute1 = "BLUE";
			}
			AssertEquals("Pick Line is picked and Release Captured Quantity is less than picked qty.", true, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));

			using (UnpickPickLinesForChangingRCAs(orderLine))
			{
				releaseLineUnCommitted.PartAttribute1 = "";
			}
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLineUnCommitted));

			releaseLine.RunPreSaveValidation();
			AssertEquals("RelaseLines and PickLines are out of sync and has error.",
				"Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.", releaseLine.UnreleasedQtyInfo.Notifications.Single().Message);
		}

		#endregion

		#region TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_NonMandatoryAttributes

		public void TestIsAnyReleaseCapturedQuantityBelowPickedQuantity_NonMandatoryAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, null, "PLT-2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			orderLine.ClearReleaseLines();

			var releaseCapturedPickLine = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "PLT-2");
			var nonReleaseCapturedPickLine = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "PLT-1");
			releaseCapturedPickLine.WZ_ReleaseCapturedPartAttrib2 = "BATCH123";
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine1 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			var releaseLine2 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "");
			AssertEquals("Release Line Quantity should be correct.", 5m, releaseLine1.Quantity);
			AssertEquals("Release Line Quantity should be correct.", 10m, releaseLine2.Quantity);
			AssertEquals("Should be no error.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine1));
			AssertEquals("Should be no error.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine2));

			releaseCapturedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			nonReleaseCapturedPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Should be no error.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine1));
			AssertEquals("Should be no error.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine2));

			releaseLine1.Quantity = 4m;
			AssertEquals("PickLine should be unchanged", 5m, releaseCapturedPickLine.WZ_Units);
			AssertEquals("Should be an error as picked quantity of BATCH123 is still 5.", true, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine1));
			AssertEquals("Should be no error.", false, releaseLines.IsAnyReleaseCapturedQuantityBelowPickedQuantity(releaseLine2));
			AssertHasError(releaseLine1.QuantityInfo, "You must Release Capture the same Quantity that has been Picked for this Order Line.");

			releaseLine1.Quantity = 5m;
			AssertNoErrors("No errors as quantity is correct again.", releaseLine1.QuantityInfo);

			order.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Order should successfully finalise.", true, order.IsFinalised);

			pick.FinalisePick();
			AssertEquals("Pick should successfully finalise.", true, pick.IsFinalised);
		}

		#endregion

		#region TestIsDuplicateReleaseLine

		public void TestIsDuplicateReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			var releaseLines = orderLine.ReleaseLines;

			var releaseLine1 = releaseLines[0];
			releaseLine1.PartAttribute1 = "RED";
			AssertEquals("Should not be a duplicate ad there is only one line.", false, releaseLines.IsDuplicateReleaseLine(releaseLine1));

			var releaseLine2 = releaseLines.AddNew();
			AssertEquals("Should not be a duplicate as attributes are different.", false, releaseLines.IsDuplicateReleaseLine(releaseLine2));

			releaseLine2.PartAttribute1 = "RED";
			AssertEquals("Release Line 2 is a duplicate.", true, releaseLines.IsDuplicateReleaseLine(releaseLine2));
			AssertEquals("Release Line 1 is not a duplicate as it was the originally created line.", false, releaseLines.IsDuplicateReleaseLine(releaseLine1));

			releaseLine2.PartAttribute1 = "BLUE";
			AssertEquals("Release Line 2 is no longer a duplicate as the attribute is different.", false, releaseLines.IsDuplicateReleaseLine(releaseLine2));

			releaseLine2.PartAttribute1 = "RED";
			AssertEquals("Precondition.", true, releaseLines.IsDuplicateReleaseLine(releaseLine2));
			releaseLine1.Delete();
			AssertEquals("No longer a duplicate", false, releaseLines.IsDuplicateReleaseLine(releaseLine2));

			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.ReleaseLines.AddNew().PartAttribute1 = "RED";
			orderLine2.ReleaseLines.AddNew().PartAttribute1 = "BLUE";

			AssertEquals("Should not be considered duplicates as another order line is involved.", false, orderLine2.ReleaseLines.IsDuplicateReleaseLine(releaseLine1));
			AssertEquals("Should not be considered duplicates as another order line is involved.", false, orderLine2.ReleaseLines.IsDuplicateReleaseLine(releaseLine2));
		}

		#endregion

		#region TestIsOrderLineOverReleased

		public void TestIsOrderLineOverReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Order Line is not over released.", false, releaseLines.IsOrderLineOverReleased);

			var releaseLine1 = releaseLines[0];
			releaseLine1.Quantity = 9m;
			AssertEquals("Order Line is not over released.", false, releaseLines.IsOrderLineOverReleased);

			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			AssertEquals("Order Line is not over released.", false, releaseLines.IsOrderLineOverReleased);

			releaseLine2.Quantity = 2m;
			AssertEquals("Order Line is over released.", true, releaseLines.IsOrderLineOverReleased);
		}

		#endregion

		#region TestIsReleaseCapturedAttributeUnallocated

		public void TestIsReleaseCapturedAttributeUnallocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.Quantity = 11m;
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Released Quantity is greater than Release Captured Qty, but 'RED' has still been Captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			pickLine.ClearReleaseCapturedAttributes();
			AssertEquals("Release Captured Attribute 'RED' has not been captured.", true, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));
			AssertEquals("Null Release Line should return false for IsReleaseCapturedAttributeUnallocated().", false, releaseLines.IsReleaseCapturedAttributeUnallocated(null));

			releaseLine.PartAttribute1 = "";
			AssertEquals("No RC Attribs on Release Line should return false for IsReleaseCapturedAttributeUnallocated().", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			releaseLine.PartAttribute1 = "RED";
			var releaseLineDuplicate = releaseLines.AddNew();
			releaseLineDuplicate.Quantity = 1m;
			releaseLineDuplicate.PartAttribute1 = "RED";
			AssertEquals("Release Captured Attribute 'RED' has been captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));
			AssertEquals("Release Captured Attribute 'RED' has been captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineDuplicate));

			pickLine.ClearReleaseCapturedAttributes();
			AssertEquals("Release Captured Attribute 'RED' has not been captured.", true, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));
			AssertEquals("Release Captured Attribute 'RED' has not been captured.", true, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineDuplicate));

			releaseLine.Quantity = 0m;
			pickLine.ClearReleaseCapturedAttributes();
			AssertEquals("Release Captured Attribute 'RED' has not been captured.", true, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			releaseLine.Quantity = -1m;
			AssertEquals("Negative Release Line should return false for IsReleaseCapturedAttributeUnallocated().", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			releaseLine.Quantity = 9m;
			AssertEquals("Release Captured Attribute 'RED' has been captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			releaseLine.Quantity = 7m;
			AssertEquals("Release Captured Attribute 'RED' has been captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			var releaseLineUnCommitted = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			releaseLineUnCommitted.Quantity = 1m;
			releaseLineUnCommitted.PartAttribute1 = "BLUE";
			AssertEquals("Pick Line has Unreleased Qty.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineUnCommitted));

			releaseLine.Quantity = 10m;
			AssertEquals("Pick Line is fully released.", true, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineUnCommitted));

			releaseLine.Quantity = 8m;
			releaseLineUnCommitted.Quantity = 2m;
			AssertEquals("Pick Line has Unreleased Qty.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineUnCommitted));

			((ICancelAddNew)releaseLines).EndNew(2);
			AssertEquals("Precondition: Element is Committed.", false, releaseLines.IsNonCommittedCollectionElement(releaseLineUnCommitted));
			AssertEquals("Pick Line is fully released.", 0m, orderLine.PickLines.Sum(l => l.UnreleaseCapturedQty));
			AssertEquals("Release Captured Attribute 'BLUE' has been captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineUnCommitted));

			releaseLineUnCommitted.Quantity = 1m;
			AssertEquals("Release Captured Attribute 'BLUE' has been captured.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLineUnCommitted));
		}

		#endregion

		#region TestIsReleaseCapturedAttributeUnallocated_ManyPickLines

		public void TestIsReleaseCapturedAttributeUnallocated_ManyPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK);
			var releaseLinesInOtherFactory = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLineInOtherFactory);
			var releaseLineInOtherFactory = releaseLinesInOtherFactory.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsReleaseCapturedAttributeUnallocated(releaseLineInOtherFactory));

			var pickLine = orderLineInOtherFactory.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			releaseLineInOtherFactory.PartAttribute1 = "RED";
			AssertEquals("Release Captured Attribute 'RED' is captured.", false, releaseLinesInOtherFactory.IsReleaseCapturedAttributeUnallocated(releaseLineInOtherFactory));

			pickLine.ClearReleaseCapturedAttributes();
			AssertEquals("Release Captured Attribute 'RED' is not captured.", true, releaseLinesInOtherFactory.IsReleaseCapturedAttributeUnallocated(releaseLineInOtherFactory));

			releaseLineInOtherFactory.Quantity = 11m;
			AssertEquals("Release Captured Attribute 'RED' is captured.", false, releaseLinesInOtherFactory.IsReleaseCapturedAttributeUnallocated(releaseLineInOtherFactory));

			releaseLineInOtherFactory.Quantity = 4m;
			AssertEquals("Release Captured Attribute 'RED' is captured.", false, releaseLinesInOtherFactory.IsReleaseCapturedAttributeUnallocated(releaseLineInOtherFactory));
		}

		#endregion

		#region IsReleaseLinePickedFromPutawayLocation

		public void IsReleaseLinePickedFromPutawayLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.Quantity = 9m;
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Pick Line is not picked.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Pick Line is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));
			AssertEquals("Null Release Line should return false for IsReleaseLinePickedFromPutawayLocation().", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(null));

			releaseLine.PartAttribute1 = "";
			AssertEquals("No RC Attribs on Release Line should return false for IsReleaseLinePickedFromPutawayLocation().", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			releaseLine.PartAttribute1 = "RED";
			var otherReleaseLine = releaseLines.AddNew();
			otherReleaseLine.Quantity = 1m;
			otherReleaseLine.PartAttribute1 = "ORANGE";
			AssertEquals("Pick Line is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));
			AssertEquals("Pick Line is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(otherReleaseLine));

			releaseLine.Quantity = 0m;
			AssertEquals("Pick Line is picked, but this Release Line is not being Picked, IsReleaseLinePickedFromPutawayLocation() should return false.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			releaseLine.Quantity = -1m;
			AssertEquals("Negative Release Line should return false for IsReleaseLinePickedFromPutawayLocation().", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			releaseLine.Quantity = 9m;
			AssertEquals("Pick Line is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			releaseLine.Quantity = 7m;
			AssertEquals("Pick Line is not picked.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			var releaseLineUnCommitted = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			releaseLineUnCommitted.Quantity = 1m;
			releaseLineUnCommitted.PartAttribute1 = "BLUE";
			AssertEquals("Pick Line is not picked, but IsReleaseLinePickedFromPutawayLocation() returns false for non-committed elements.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLineUnCommitted));

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Pick Line is picked, but IsReleaseLinePickedFromPutawayLocation() returns false for non-committed elements.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLineUnCommitted));

			releaseLineUnCommitted.PartAttribute1 = "GREEN";
			((ICancelAddNew)releaseLines).EndNew(2);
			AssertEquals("Precondition: Element is Committed.", false, releaseLines.IsNonCommittedCollectionElement(releaseLineUnCommitted));
			AssertEquals("Pick Line is released Captured.", 1m, pickLine.UnreleaseCapturedQty);
			AssertEquals("Pick Line is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLineUnCommitted));

			releaseLineUnCommitted.PartAttribute1 = ""; // Unrelease capture
			releaseLine.Quantity = 9m;
			releaseLineUnCommitted.PartAttribute1 = "GREEN";

			AssertEquals("Pick Line is over Released, so IsReleaseLinePickedFromPutawayLocation() should return false.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLineUnCommitted));
			AssertEquals("Pick Line is over Released, but this release line has a Quantity almost equal to the Picked qty, IsReleaseLinePickedFromPutawayLocation() should return true.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));
		}

		#endregion

		#region IsReleaseLinePickedFromPutawayLocation_ManyPickLines

		public void IsReleaseLinePickedFromPutawayLocation_ManyPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			var pickLineForBatch123 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			pickLineForBatch123.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));
			pickLineForBatch123.WZ_PickedDateTime = ZDateTimeOffset.Empty; // clean-up

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK);
			var releaseLinesInOtherFactory = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLineInOtherFactory);
			var releaseLineInOtherFactory = releaseLinesInOtherFactory.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsReleaseLinePickedFromPutawayLocation(releaseLineInOtherFactory));

			var pickLine = orderLineInOtherFactory.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("PickLine is picked, but all Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsReleaseLinePickedFromPutawayLocation(releaseLineInOtherFactory));

			releaseLineInOtherFactory.PartAttribute1 = "RED";
			AssertEquals("PickLine is picked.", true, releaseLinesInOtherFactory.IsReleaseLinePickedFromPutawayLocation(releaseLineInOtherFactory));

			releaseLineInOtherFactory.Quantity = 4m;
			AssertEquals("PickLine is picked.", true, releaseLinesInOtherFactory.IsReleaseLinePickedFromPutawayLocation(releaseLineInOtherFactory));

			releaseLineInOtherFactory.Quantity = 11m;
			AssertEquals("PickLine is picked, even though it is over released.", true, releaseLinesInOtherFactory.IsReleaseLinePickedFromPutawayLocation(releaseLineInOtherFactory));
		}

		#endregion

		#region IsReleaseLinePickedFromPutawayLocation_InTransitLines

		public void IsReleaseLinePickedFromPutawayLocation_InTransitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			var pickLineForBatch123 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			Helper.PickAndMakeInTransitTransfer(pickLineForBatch123, ZDateTimeOffset.Now);
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			releaseLine.PartAttribute1 = "RED";
			AssertEquals("PickLine is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));

			releaseLine.Quantity = 4m;
			AssertEquals("PickLine is picked.", true, releaseLines.IsReleaseLinePickedFromPutawayLocation(releaseLine));
		}

		#endregion

		#region TestIsReleaseLineUnderCaptured

		public void TestIsReleaseLineUnderCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.Quantity = 9m;
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Released Qty and Release Captured Qty is the same.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));

			pickLine.WZ_Units = 9m;
			AssertEquals("Released Qty and Release Captured Qty is the same.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));
			AssertEquals("Null Release Line should return false for IsReleaseLineUnderCaptured().", false, releaseLines.IsReleaseLineUnderCaptured(null));

			releaseLine.Quantity = 10m;
			AssertEquals("Pick Line was updated as well.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));

			releaseLine.PartAttribute1 = "";
			AssertEquals("No RC Attribs on Release Line should return false for IsReleaseLineUnderCaptured().", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));

			// clean-up
			releaseLine.PartAttribute1 = "RED";
			releaseLine.Quantity = 9m;
			AssertEquals("Released and Release Captured Quantity match.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));

			releaseLine.Quantity = 7m;
			AssertEquals("Released and Release Captured Quantity match.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));

			var releaseLineUnCommitted = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			releaseLineUnCommitted.Quantity = 1m;
			releaseLineUnCommitted.PartAttribute1 = "BLUE";
			AssertEquals("Released Qty is less than Unreleased Qty.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLineUnCommitted));

			releaseLine.Quantity = 10m;
			AssertEquals("Released Qty is greater than Unreleased Qty.", true, releaseLines.IsReleaseLineUnderCaptured(releaseLineUnCommitted));

			releaseLineUnCommitted.Quantity = 0m;
			AssertEquals("Zero Quantity on Uncommitted Release Line should return false for IsReleaseLineUnderCaptured().", false, releaseLines.IsReleaseLineUnderCaptured(releaseLineUnCommitted));

			// clean-up
			releaseLine.Quantity = 7m;
			releaseLineUnCommitted.Quantity = 3m;
			AssertEquals("Released Qty is equal to Unreleased Qty.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLineUnCommitted));

			((ICancelAddNew)releaseLines).EndNew(1);
			AssertEquals("Precondition: Element is Committed.", false, releaseLines.IsNonCommittedCollectionElement(releaseLineUnCommitted));
			AssertEquals("Pick Line is fully released.", 0m, orderLine.PickLines.Sum(l => l.UnreleaseCapturedQty));
			AssertEquals("Released Qty and Release Captured Qty match.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLineUnCommitted));

			releaseLineUnCommitted.Quantity = 4m;
			AssertEquals("Released Qty is greater than Release Captured Qty.", true, releaseLines.IsReleaseLineUnderCaptured(releaseLineUnCommitted));
		}

		#endregion

		#region TestIsReleaseLineUnderCaptured_ManyPickLines

		public void TestIsReleaseLineUnderCaptured_ManyPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BATCH456", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));

			var pickLineBatch123 = orderLine.PickLines.Single(p => p.InventoryLine.WE_PartAttrib2 == "BATCH123");
			pickLineBatch123.WZ_Units = 9m;
			AssertEquals("There are no Release Captured Attribs.", false, releaseLines.IsReleaseLineUnderCaptured(releaseLine));
			pickLineBatch123.WZ_Units = 10m; // clean-up

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK);
			var releaseLinesInOtherFactory = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLineInOtherFactory);
			var releaseLineInOtherFactory = releaseLinesInOtherFactory.Cast<WhsReleaseLine>().Single(r => r.PartAttribute2 == "BATCH123");
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsReleaseLineUnderCaptured(releaseLineInOtherFactory));

			var pickLine = otherFactory.Load<WhsPickLine>(pickLineBatch123.PK);
			pickLine.WZ_Units = 9m;
			AssertEquals("All Release Captured Attribs are empty.", false, releaseLinesInOtherFactory.IsReleaseLineUnderCaptured(releaseLineInOtherFactory));
			pickLine.WZ_Units = 10m; // clean-up

			releaseLineInOtherFactory.PartAttribute1 = "RED";
			AssertEquals("Released and Release Captured Qty match.", false, releaseLinesInOtherFactory.IsReleaseLineUnderCaptured(releaseLineInOtherFactory));

			releaseLineInOtherFactory.Quantity = 10m;
			orderLineInOtherFactory.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").WZ_Units = 9m;
			AssertEquals("Released Qty is greater than Release Captured Qty.", true, releaseLinesInOtherFactory.IsReleaseLineUnderCaptured(releaseLineInOtherFactory));

			releaseLineInOtherFactory.Quantity = 4m;
			AssertEquals("Released Qty and Release Captured Qty match.", false, releaseLinesInOtherFactory.IsReleaseLineUnderCaptured(releaseLineInOtherFactory));
		}

		#endregion

		#region TestISerialSplittableLine_AddNew_SuspendValidation

		public void TestISerialSplittableLine_AddNew_SuspendValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals(true, releaseLine.IsSerialisedProduct());

			bool validationWasCalled = false;
			releaseLine.QuantityInfo.AdditionalValidation += () =>
			{
				validationWasCalled = true;
			};

			orderLine.ReleaseLines.SetupNewElementButDoNotAddIt(releaseLine, true);
			AssertEquals(false, validationWasCalled);
		}

		#endregion

		#region TestPickLinesCacheIsUpdatedWhenPickLinesCollectionIsUpdated

		public void TestPickLinesCacheIsUpdatedWhenPickLinesCollectionIsUpdated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			var pickLine = orderLine.PickLines.Single();
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Release Line should be considered Allocated.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			var inventoryPK = pickLine.WZ_WE_InventoryLine;
			pickLine.Delete();
			AssertEquals("Release Line should no longer be considered Allocated as there is no Pick Line.", true, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			var newPickLine = Factory.New<WhsPickLine>();
			newPickLine.WZ_Units = 10m;
			newPickLine.WZ_WE_InventoryLine = inventoryPK;
			newPickLine.WZ_ReleaseCapturedPartAttrib1 = "RED";
			newPickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals("Release Line should be considered Allocated.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pickLineInOtherFactory = otherFactory.Load<WhsPickLine>(newPickLine.PK);
			pickLineInOtherFactory.Delete();
			AssertEquals("Precondition: Release Line should be considered Allocated.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(releaseLine));

			otherFactory.Save();
			AssertEquals("Release line collection cleared after pickline delete on data refresh.", false, releaseLines.Contains(releaseLine));
			Assert("Release line collection cleared after pickline delete on data refresh.", releaseLines.IsNullOrEmpty());
			Factory.Save();

			var newPickLineInOtherFactory = otherFactory.New<WhsPickLine>();
			newPickLineInOtherFactory.WZ_Units = 10m;
			newPickLineInOtherFactory.WZ_WE_InventoryLine = inventoryPK;
			newPickLineInOtherFactory.WZ_ReleaseCapturedPartAttrib1 = "RED";
			newPickLineInOtherFactory.WZ_WE_TransactionLine = orderLine.PK;
			otherFactory.Save();

			releaseLines.ClearCollection(); // clear collection again after it got rebuilt from the assertions above
			var newReleaseLine = (WhsReleaseLine)releaseLines.Single();
			AssertEquals("Release Line should be considered Picked.", false, releaseLines.IsReleaseCapturedAttributeUnallocated(newReleaseLine));
		}

		#endregion

		#region TestRemoveNonCommittedElement_CallsRefreshBindingIfItWasDeferred

		public void TestRemoveNonCommittedElement_CallsRefreshBindingIfItWasDeferred()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Precondition: Quantity has been reduced to 1.", 1m, releaseLine1.Quantity);

			var listChangedHitCount = 0;
			((IBindingList)orderLine.ReleaseLines).ListChanged += (sender, e) =>
			{
				if (e.ListChangedType == ListChangedType.Reset)
				{
					listChangedHitCount++;
				}
			};

			var newReleaseLine = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertEquals("Should not have called Reset List Change since the new element is not yet committed.", 0, listChangedHitCount);

			((ICancelAddNew)orderLine.ReleaseLines).CancelNew(1);
			AssertEquals("Release Line should have been removed and deleted.", true, newReleaseLine.IsDeleted);
			AssertEquals("On successful commit of the new element, Reset List Changed should have been called as it was deferred till commit.", 1, listChangedHitCount);
		}

		#endregion

		#region TestRemoveAndDelete_DoesNotCallRemoveAndDeleteAgain

		public void TestRemoveAndDelete_DoesNotCallRemoveAndDeleteAgain()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			Helper.CreatePickNew(order);

			var releaseLines = order.Lines[0].ReleaseLines;
			var releaseLine1 = releaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Quantity should be reduced.", 1m, releaseLine1.Quantity);

			var countChangedHitCount = 0;
			var releaseLine2 = ((IBindingList)releaseLines).AddNew();
			releaseLines.CountChanged += (sender, e) => countChangedHitCount++;

			((ICancelAddNew)releaseLines).EndNew(1);
			AssertEquals("Should have only fired Count changed once, when Calling RemoveAndDelete() via EndNew.", 1, countChangedHitCount);
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes

		public void TestReleaseLinesChangingReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var today = ZDate.Today;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, location1, today.AddDays(1), today.AddDays(2), "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location2, today.AddDays(1), today.AddDays(2), "", "", "", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location3, today.AddDays(1), today.AddDays(2), "", "", "", "");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2, today.AddDays(2), today.AddDays(3), "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 19m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line has correctly Picked stock.", 4, orderLine.PickLines.Count);

			var pickLine1 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			var pickLine2 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			var pickLine3 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			var pickLine4 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition: Order Line has released correct stock.", 2, releaseLines.Count);

			var releaseLine1 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.ExpiryDate == today.AddDays(1) && r.PackingDate == today.AddDays(2));
			var releaseLine2 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.ExpiryDate == today.AddDays(2) && r.PackingDate == today.AddDays(3));
			AssertEquals("Precondition: Order Line has released correct stock.", 9m, releaseLine1.Quantity);
			AssertEquals("Precondition: Order Line has released correct stock.", 10m, releaseLine2.Quantity);

			releaseLine1.Quantity = 3m;
			var releaseLine3 = releaseLines.AddNew();
			releaseLine3.Quantity = 5m;
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine1.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine2.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine3.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine4.HasReleaseCapturedAttribs);

			var allPickLines = orderLine.PickLines;

			releaseLine1.PartAttribute1 = "RED";
			AssertEquals("Should have Release Captured 3 Red Stock.", 3m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "" && rca.WZ_ReleaseCapturedPartAttrib3 == "").Sum(rca => rca.WZ_Units));

			releaseLine1.PartAttribute2 = "BATCH123";
			AssertEquals("Should have Release Captured 3 Red & BATCH123 Stock.", 3m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "").Sum(rca => rca.WZ_Units));

			releaseLine1.PartAttribute3 = "STYLEX11";
			AssertEquals("Should have Release Captured 3 Red & BATCH123 & STYLEX11 Stock.", 3m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));

			releaseLine1.Quantity = 4m;
			AssertEquals("Should have Release Captured 4 Red & BATCH123 & STYLEX11 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute1 = "BLUE";
			AssertEquals("Should have Release Captured 10 Blue Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && rca.WZ_ReleaseCapturedPartAttrib2 == "" && rca.WZ_ReleaseCapturedPartAttrib3 == "").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute3 = "STYLEX33";
			AssertEquals("Should have Release Captured 4 Red & BATCH123 & STYLEX11 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Blue & STYLEX33 Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && rca.WZ_ReleaseCapturedPartAttrib2 == "" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX33").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute3 = "STYLEX11";
			AssertEquals("Should have Release Captured 4 Red & BATCH123 & STYLEX11 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Blue & STYLEX11 Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && rca.WZ_ReleaseCapturedPartAttrib2 == "" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute2 = "BATCH123";
			AssertEquals("Should have Release Captured 4 Red & BATCH123 & STYLEX11 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Blue & BATCH123 & STYLEX11 Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));

			releaseLine3.PartAttribute1 = "GREEN";
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));

			releaseLine3.SetExpiryDateForTesting(today.AddDays(1));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));

			releaseLine3.PartAttribute2 = "BATCH456";
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH456"));

			releaseLine3.PartAttribute3 = "STYLEX22";
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH456"));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX22"));

			releaseLine3.SetPackingDateForTesting(today.AddDays(3));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN"));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH456"));
			AssertEquals("No matching Pick Lines, cannot Release Capture Stock.", 0, allPickLines.Concat(allPickLines).Count(rca => rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX22"));

			releaseLine3.SetPackingDateForTesting(today.AddDays(2));
			AssertEquals("Should have Release Captured 4 Red & BATCH123 & STYLEX11 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 5 Green & BATCH456 & STYLEX22 Stock.", 5m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH456" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX22").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Blue & BATCH123 & STYLEX11 Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "BLUE" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123" && rca.WZ_ReleaseCapturedPartAttrib3 == "STYLEX11").Sum(rca => rca.WZ_Units));

			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			releaseLine3.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoErrors(releaseLine3);

			AssertNoExceptionThrown(() => Factory.Save());

			foreach (var pickLine in allPickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			releaseLine3.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoErrors(releaseLine3);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_MixedPartAttribs

		public void TestReleaseLinesChangingReleaseCapturedAttributes_MixedPartAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);

			var today = ZDate.Today;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, location1, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location2, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location3, ZDate.Empty, ZDate.Empty, "MEDIUM", "", "", "");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2, ZDate.Empty, ZDate.Empty, "LARGE", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 19m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line has correctly Picked stock.", 4, orderLine.PickLines.Count);

			var pickLine1 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			var pickLine2 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			var pickLine3 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			var pickLine4 = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition: Order Line has released correct stock.", 2, releaseLines.Count);

			var releaseLine1 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "MEDIUM");
			var releaseLine2 = releaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "LARGE");
			AssertEquals("Precondition: Order Line has released correct stock.", 9m, releaseLine1.Quantity);
			AssertEquals("Precondition: Order Line has released correct stock.", 10m, releaseLine2.Quantity);

			releaseLine1.Quantity = 3m;
			var releaseLine3 = releaseLines.AddNew();
			releaseLine3.Quantity = 5m;
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine1.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine2.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine3.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine4.HasReleaseCapturedAttribs);

			var allPickLines = orderLine.PickLines;

			releaseLine1.PartAttribute2 = "RED";
			AssertEquals("Should have Release Captured 3 Medium Red Stock.", 3m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "").Sum(rca => rca.WZ_Units));

			releaseLine1.PartAttribute3 = "BATCH123";
			AssertEquals("Should have Release Captured 3 Medium Red & BATCH123 Stock.", 3m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));

			releaseLine1.Quantity = 4m;
			AssertEquals("Should have Release Captured 4 Medium Red & BATCH123 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute2 = "RED";
			AssertEquals("Should have Release Captured 4 Medium Red & BATCH123 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Large Red Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute3 = "BATCH456";
			AssertEquals("Should have Release Captured 4 Medium Red & BATCH123 Stock.", 4m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Large Red & BATCH456 Stock.", 10m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH456").Sum(rca => rca.WZ_Units));

			releaseLine2.PartAttribute3 = "BATCH123";
			AssertEquals("Should have Release Captured 4 Medium Red & BATCH123 Stock.", 4m, allPickLines.Where(rca => rca.WZ_WE_InventoryLine != inventory4.WI_WE_InDocketLine && rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Large Red & BATCH123 Stock.", 10m, allPickLines.Where(rca => rca.WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine && rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));

			releaseLine3.PartAttribute1 = "MEDIUM";
			AssertEquals("No Release Captured Attributes entered.", 14m, allPickLines.Where(pl => !!pl.HasReleaseCapturedAttribs).Sum(rca => rca.WZ_Units));

			releaseLine3.PartAttribute2 = "RED";
			AssertEquals("Should have Release Captured 4 Medium Red & BATCH123 Stock.", 4m, allPickLines.Where(rca => rca.WZ_WE_InventoryLine != inventory4.WI_WE_InDocketLine && rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 5 Medium Red Stock.", 5m, allPickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 10 Large Red & BATCH123 Stock.", 10m, allPickLines.Where(rca => rca.WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine && rca.WZ_ReleaseCapturedPartAttrib1 == "" && rca.WZ_ReleaseCapturedPartAttrib2 == "RED" && rca.WZ_ReleaseCapturedPartAttrib3 == "BATCH123").Sum(rca => rca.WZ_Units));

			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			releaseLine3.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoErrors(releaseLine3);

			AssertNoExceptionThrown(() => Factory.Save());

			foreach (var pickLine in allPickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			releaseLine3.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoErrors(releaseLine3);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_UpdatingAttributes

		public void TestReleaseLinesChangingReleaseCapturedAttributes_UpdatingAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order Line has correctly Picked stock.", 1, orderLine.PickLines.Count);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition: Order Line has released correct stock.", 1, releaseLines.Count);

			var releaseLine1 = releaseLines[0];
			releaseLine1.Quantity = 6m;
			Factory.Save();

			releaseLine1.PartAttribute1 = "RED";
			AssertEquals("Should have Release Captured 6 Red.", 6m, orderLine.PickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED").Sum(rca => rca.WZ_Units));

			Factory.Save();

			releaseLine1.PartAttribute1 = "GREEN";
			AssertEquals("Should have Release Captured 6 Green.", 6m, orderLine.PickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN").Sum(rca => rca.WZ_Units));

			Factory.Save();

			releaseLine1.PartAttribute2 = "BATCH123";
			AssertEquals("Should have Release Captured 6 Green & BATCH123.", 6m, orderLine.PickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123").Sum(rca => rca.WZ_Units));

			Factory.Save();

			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute1 = "RED";
			releaseLine2.PartAttribute2 = "BATCH123";
			AssertEquals("Should have Release Captured 6 Green & BATCH123.", 6m, orderLine.PickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "GREEN" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123").Sum(rca => rca.WZ_Units));
			AssertEquals("Should have Release Captured 4 RED & BATCH123.", 4m, orderLine.PickLines.Where(rca => rca.WZ_ReleaseCapturedPartAttrib1 == "RED" && rca.WZ_ReleaseCapturedPartAttrib2 == "BATCH123").Sum(rca => rca.WZ_Units));
			Factory.Save();
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_AddRCA_SplitPickLine

		public void TestReleaseLinesChangingReleaseCapturedAttributes_AddRCA_SplitPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: 1 PickLine and 10 units", 10m, pickLine.WZ_Units);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 5m;
			releaseLine.PartAttribute2 = "RED";

			AssertEquals("We have 2 PickLines now", 2, orderLine.PickLines.Count);
			AssertEquals("One has attribute2 set", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
			AssertEquals("One has empty attribute2", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "").WZ_Units);

			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			releaseLine3.Quantity = 3;
			releaseLine3.PartAttribute2 = "BLUE";
			AssertEquals("We have 3 PickLines now", 3, orderLine.PickLines.Count);
			AssertEquals("The RED one has 5 units", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
			AssertEquals("The BLUE one has 3 units", 3m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "BLUE").WZ_Units);
			AssertEquals("The empty one has 2 units", 2m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "").WZ_Units);
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_AddRCA_SameQtyDoesNotSplitPickLine

		public void TestReleaseLinesChangingReleaseCapturedAttributes_AddRCA_SameQtyDoesNotSplitPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var pickLine = orderLine.PickLines.Single();
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 10m;

			AssertEquals("Precondition: 1 PickLine and 10 units", 10m, pickLine.WZ_Units);

			releaseLine.PartAttribute2 = "RED";

			AssertEquals("We still have 1 PickLine", 1, orderLine.PickLines.Count);
			AssertEquals("It has attribute2 set", 10m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_ReduceRCAQty_SplitPickLine

		public void TestReleaseLinesChangingReleaseCapturedAttributes_ReduceRCAQty_SplitPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute2 = "RED";
			releaseLine.Quantity = 10m;

			var pickLines = orderLine.PickLines;
			AssertEquals("Precondition: 1 Red PickLine", 1, pickLines.Count);
			AssertEquals("Precondition: Red PickLine", "RED", pickLines.Single(l => l.WZ_Units == 10m).WZ_ReleaseCapturedPartAttrib2);

			releaseLine.Quantity = 5m;

			AssertEquals("We have 2 PickLines now", 2, orderLine.PickLines.Count);
			AssertEquals("Red PickLine", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
			AssertEquals("Empty PickLine", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "").WZ_Units);
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_ReduceRCAQty_RemergePickLine

		public void TestReleaseLinesChangingReleaseCapturedAttributes_ReduceRCAQty_RemergePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute2 = "RED";
			releaseLine.Quantity = 5m;

			AssertEquals("Precondition: 2 PickLines", 2, orderLine.PickLines.Count);
			AssertEquals("Precondition: Red PickLine", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "RED").WZ_Units);
			AssertEquals("Precondition: Empty PickLine", 5m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "").WZ_Units);

			releaseLine.Delete();

			AssertEquals("Pick Lines remerged", 1, orderLine.PickLines.Count);
			AssertEquals("It has empty attribute2 ", 10m, orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib2 == "").WZ_Units);
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_ReduceRCAQty_SkipPickedPickLines

		public void TestReleaseLinesChangingReleaseCapturedAttributes_ReduceRCAQty_SkipPickedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, null, "PLT-2");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PalletID == "PLT-1");
			var availableInventory2 = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(i => i.PalletID == "PLT-2");
			availableInventory1.PickLineQuantity = 2m;
			availableInventory2.PickLineQuantity = 0m;

			Factory.Save();

			var redReleaseLine = orderLine.ReleaseLines[0];
			redReleaseLine.PartAttribute2 = "RED";
			redReleaseLine.Quantity = 2m;

			AssertEquals("Precondition: 1 PickLine", 1, orderLine.PickLines.Count);
			var pickLine1 = orderLine.PickLines.Single();
			AssertEquals("Precondition: Red PickLine", 2m, pickLine1.WZ_Units);

			pickLine1.WZ_PickedDateTime = DateTime.Now;
			pickLine1.WZ_GS_NKAssignedTo = "E";
			AssertEquals("pickLine1 is picked.", true, pickLine1.IsPickedFromPutawayLocation);

			redReleaseLine.Quantity = 1m;
			AssertEquals("WZ_Units of pickLine1 is not reduced.", 2m, pickLine1.WZ_Units);

			availableInventory2.PickLineQuantity = 3m;

			AssertEquals("Precondition: 2 PickLines", 2, orderLine.PickLines.Count);

			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(l => l.PartAttribute2 == "");
			releaseLine2.Delete();
			redReleaseLine = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(l => l.PartAttribute2 == "RED");
			redReleaseLine.Quantity = 5m;
			var pickLine2 = orderLine.PickLines.Single(pl => pl.WZ_Units == 3m);
			AssertEquals("Precondition: pickLine1 has 2m units.", 2m, pickLine1.WZ_Units);
			AssertEquals("Precondition: pickLine2 is not picked.", false, pickLine2.IsPickedFromPutawayLocation);

			redReleaseLine.Quantity = 4m;
			AssertEquals("3 PickLines now.", 3, orderLine.PickLines.Count);
			AssertEquals("pickLine1 still has 2 units.", 2m, orderLine.PickLines.Single(pl => pl.PK == pickLine1.PK).WZ_Units);
			AssertEquals("We have a not picked PickLine with 2m units and RED attrib.", "RED", orderLine.PickLines.Single(pl => pl.WZ_Units == 2m && !pl.IsPickedFromPutawayLocation).WZ_ReleaseCapturedPartAttrib2);
			AssertEquals("We have a new PickLine with 1m unit and empty Release Captured Attributes.", "", orderLine.PickLines.Single(pl => pl.WZ_Units == 1m).WZ_ReleaseCapturedPartAttrib2);

			redReleaseLine.Quantity = 2m;
			AssertEquals("2 PickLines now.", 2, orderLine.PickLines.Count);
			AssertEquals("pickLine1 still has 2 units.", 2m, orderLine.PickLines.Single(pl => pl.PK == pickLine1.PK).WZ_Units);
			pickLine2 = orderLine.PickLines.Single(pl => pl.WZ_Units == 3m);
			AssertEquals("pickLine2 has empty Release Captured Attributes.", "", pickLine2.WZ_ReleaseCapturedPartAttrib2);

			redReleaseLine.Quantity = 1m;
			AssertEquals("Still 2 PickLines.", 2, orderLine.PickLines.Count);
			AssertEquals("pickLine1 still has 2 units.", 2m, orderLine.PickLines.Single(pl => pl.PK == pickLine1.PK).WZ_Units);
			pickLine2 = orderLine.PickLines.Single(pl => pl.WZ_Units == 3m);
			AssertEquals("pickLine2 has empty Release Captured Attributes.", "", pickLine2.WZ_ReleaseCapturedPartAttrib2);
		}

		#endregion

		#region TestReleaseLinesChangingReleaseCapturedAttributes_IgnorePickByBOMPickLines

		public void TestReleaseLinesChangingReleaseCapturedAttributes_IgnorePickByBOMPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, bike, AttributeNumber.One, true, setReleaseCaptured: true);

			var location1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, bike, 4m, location1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 4m, location1);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 6m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: 1 normal Pick Line and 1 created from Pick By BOM.", 2, orderLine.PickLines.Count);

			var pickLine1 = orderLine.PickLines.Single(l => !l.IsPickByBOMKitPickLine());
			var pickLine2 = orderLine.PickLines.Single(l => l.IsPickByBOMKitPickLine());
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine1.HasReleaseCapturedAttribs);
			AssertEquals("Precondition: No stock is Release Captured.", false, pickLine2.HasReleaseCapturedAttribs);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition.", 1, releaseLines.Count);

			var releaseLine1 = releaseLines.Cast<WhsReleaseLine>().Single();
			AssertEquals("Precondition: Order Line has released correct stock.", 6m, releaseLine1.Quantity);

			releaseLine1.Quantity = 4m;
			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 2m;

			var allPickLines = orderLine.PickLines;

			releaseLine2.PartAttribute1 = "RED";
			AssertEquals("Should have Release Captured 2 Red Stock.", 2m, allPickLines.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").Sum(l => l.WZ_Units));
			AssertEquals("Pick Lines created from Pick By BOM should not have Releaes Captured Attributes.", false, pickLine2.HasReleaseCapturedAttribs);
			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoExceptionThrown(() => Factory.Save());

			releaseLine1.Quantity = 1m;
			releaseLine2.Quantity = 5m;
			AssertEquals("Should have Release Captured 4 Red Stock.", 4m, allPickLines.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").Sum(l => l.WZ_Units));
			AssertEquals("Pick Lines created from Pick By BOM should not have Releaes Captured Attributes.", false, pickLine2.HasReleaseCapturedAttribs);
			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertHasError(releaseLine2.QuantityInfo, "The Release Captured Quantity for this Product cannot be greater than the Quantity Allocated for this Order Line.");
			AssertNoExceptionThrown(() => Factory.Save());

			releaseLine1.Quantity = 2m;
			releaseLine2.Quantity = 4m;
			AssertEquals("Should have Release Captured 4 Red Stock.", 4m, allPickLines.Where(l => l.WZ_ReleaseCapturedPartAttrib1 == "RED").Sum(l => l.WZ_Units));
			AssertEquals("Pick Lines created from Pick By BOM should not have Releaes Captured Attributes.", false, pickLine2.HasReleaseCapturedAttribs);
			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoExceptionThrown(() => Factory.Save());

			foreach (var pickLine in allPickLines.Where(l => !l.IsPickByBOMKitPickLine()))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}
			foreach (var pickLine in orderLine.ChildComponentLines.SelectMany(l => l.PickLines))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}

			releaseLine1.Validation.ValidateAll();
			releaseLine2.Validation.ValidateAll();
			AssertNoErrors(releaseLine1);
			AssertNoErrors(releaseLine2);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestRemovingReleaseLines_UpdatesPackableItems

		public void TestRemovingReleaseLines_UpdatesPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 4m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "RED";
			releaseLine2.Quantity = 3m;

			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			releaseLine3.PartAttribute1 = "BLUE";
			releaseLine3.Quantity = 2m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine1), GetGroupingKey(releaseLine2), GetGroupingKey(releaseLine3) },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));

			releaseLine2.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine3 }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine3 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine1), GetGroupingKey(releaseLine3) },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));

			orderLine.ReleaseLines.Remove(releaseLine3);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine1) }, order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));

			releaseLine1.PartAttribute1 = "GREEN";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine1) }, order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));
		}

		#endregion

		#region TestSetDefaultsForNewChild_SetsUnitsForNewChildToRemainingOrderedQuantityForSerialProducts

		public void TestSetDefaultsForNewChild_SetsUnitsForNewChildToRemainingOrderedQuantityForSerialProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 3m;

			using (orderLine.ReleaseLines.SuspendSettingDefaults())
			{
				var releaseLine2 = orderLine.ReleaseLines.AddNew();
				AssertEquals(0m, releaseLine2.Quantity);
			}

			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			AssertEquals(2m, releaseLine3.Quantity);
		}

		#endregion

		#region TestSplittingPackedPickLines

		public void TestSplittingPackedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: PickLines not split.", 1, orderLine.PickLines.Count);
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, orderLine.ReleaseLines.GetPackableItems(releaseLine));

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 7m);
			AssertEquals("PickLines should be split.", 2, orderLine.PickLines.Count);
			AssertEquals("PickLines should be split.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 7m && !pl.IsUnpacked(Factory)));
			AssertEquals("PickLines should be split.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 3m));
			AssertContainsExactElementsInAnyOrder(orderLine.PickLines, orderLine.ReleaseLines.GetPackableItems(releaseLine));
		}

		#endregion

		#region TestSplittingPackedPickLines_ReleaseCapture

		public void TestSplittingPackedPickLines_ReleaseCapture()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreatePickNew(order1);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Precondition: Quantity reduced to 1 for Serials.", 1m, releaseLine1.Quantity);

			var releaseLine2 = orderLine1.ReleaseLines.AddNew();
			releaseLine2.Quantity = 8m;
			releaseLine1.Quantity = 2m;

			AssertEquals("Pick Lines have been splited to 3.", 3, orderLine1.PickLines.Count);
			AssertEquals("Sum of WZ_Units is 10.", 10m, orderLine1.PickLines.Sum(p => p.WZ_Units));
			AssertEquals("2 Pick Line with same serial number.", 2, orderLine1.PickLines.Count(p => p.WZ_ReleaseCapturedSerialNumber == "SN1"));
			AssertNotNull("1 Pick Line with 8m units.", orderLine1.PickLines.Single(p => p.WZ_ReleaseCapturedSerialNumber != "SN1" && p.WZ_Units == 8m));

			var package = order1.PackageJob.Packages.AddNew();
			package.Pack(releaseLine2, 8m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine2.IsPacked);
			AssertEquals("Pick Lines are still 3.", 3, orderLine1.PickLines.Count);
			AssertNotNull("1 Pick Line with 8m units.", orderLine1.PickLines.Single(p => p.WZ_ReleaseCapturedSerialNumber != "SN1" && p.WZ_Units == 8m));
			AssertEquals("2 Pick Line with same serial number.", 2, orderLine1.PickLines.Count(p => p.WZ_ReleaseCapturedSerialNumber == "SN1" && p.WZ_Units == 1m));
			AssertEquals("Release Line is packed.", false, orderLine1.PickLines.Single(p => p.WZ_ReleaseCapturedSerialNumber != "SN1" && p.WZ_Units == 8m).IsUnpacked(Factory));

			releaseLine1.SerialNumber = "SN2"; // change attribute and quantity to 1.

			var items = orderLine1.ReleaseLines.GetPackableItems(releaseLine1);
			var releaseCapturedAttrib = (WhsPickLine)items.Single();
			AssertEquals("Release Captured Attrib is correct.", "SN2", releaseCapturedAttrib.WZ_ReleaseCapturedSerialNumber);
			AssertEquals("Release Captured Attrib is correct.", 1m, releaseCapturedAttrib.WZ_Units);

			package.Pack(releaseCapturedAttrib, releaseLine1);
			AssertEquals("Release Line is packed.", true, releaseLine1.IsPacked);

			releaseLine1.Quantity = 2m;
			AssertEquals("3 Pick Lines.", 3, orderLine1.PickLines.Count);
			AssertNotNull("Pick Line with 8m units.", orderLine1.PickLines.Single(p => p.WZ_ReleaseCapturedSerialNumber != "SN2" && p.WZ_Units == 8m));
			AssertEquals("2 Pick Lines with the same Serial Number.", 2, orderLine1.PickLines.Count(p => p.WZ_ReleaseCapturedSerialNumber == "SN2" && p.WZ_Units == 1m));

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.CreatePickNew(order2);

			var orderLine2ReleaseLine1 = orderLine2.ReleaseLines[0];
			orderLine2ReleaseLine1.SerialNumber = "SN3";
			AssertEquals("Quantity reduced to 1 for Serials.", 1m, orderLine2ReleaseLine1.Quantity);

			var orderLine2ReleaseLine2 = orderLine2.ReleaseLines.AddNew();
			orderLine2ReleaseLine2.Quantity = 4m;
			orderLine2ReleaseLine1.Quantity = 6m;
			AssertEquals("Pick Lines should split.", 3, orderLine2.PickLines.Count);
			AssertEquals("Pick Lines should split", 10m, orderLine2.PickLines.Sum(p => p.WZ_Units));
			AssertEquals("1 empty RCA Line.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 4m));
			AssertEquals("1 with Serial Number.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 1m && pl.WZ_ReleaseCapturedSerialNumber == "SN3"));
			AssertEquals("1 with Serial Number.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 5m && pl.WZ_ReleaseCapturedSerialNumber == "SN3"));

			var packageOnOtherOrder = order2.PackageJob.Packages.AddNew();
			packageOnOtherOrder.Pack(orderLine2ReleaseLine2, 3m);
			AssertEquals("Release Line is packed.", true, orderLine2ReleaseLine2.IsPacked);
			AssertEquals("Pick Lines are split", 4, orderLine2.PickLines.Count);
			AssertEquals("Pick Line is packed.", false, orderLine2.PickLines.Single(p => p.WZ_Units == 3m).IsUnpacked(Factory));
			AssertEquals("1 Packed Line.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 3m && !pl.IsUnpacked(Factory)));
			AssertEquals("1 empty RCA Line.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 1m && pl.WZ_ReleaseCapturedSerialNumber == ""));
			AssertEquals("1 with Serial Number.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 1m && pl.WZ_ReleaseCapturedSerialNumber == "SN3"));
			AssertEquals("1 with Serial Number.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 5m && pl.WZ_ReleaseCapturedSerialNumber == "SN3"));

			orderLine2ReleaseLine1.Quantity = 2m;
			AssertEquals("3 Pick Lines.", 3, orderLine2.PickLines.Count);
			AssertEquals("1 Packed Line stay untouched.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 3m && !pl.IsUnpacked(Factory)));
			AssertEquals("1 empty RCA Line merged with newly split line.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 5m && pl.WZ_ReleaseCapturedSerialNumber == ""));
			AssertEquals("1 with Serial Number.", 1, orderLine2.PickLines.Count(pl => pl.WZ_Units == 2m && pl.WZ_ReleaseCapturedSerialNumber == "SN3"));
		}

		#endregion

		#region TestSplittingPackedPickLines_ReleaseCapture_Adding

		public void TestSplittingPackedPickLines_ReleaseCapture_Adding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			AssertEquals("Precondition: Quantity reduced to 1 for Serials.", 1m, releaseLine1.Quantity);

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 8m;
			releaseLine1.Quantity = 2m;
			AssertEquals("Pick Lines have been splited to 3.", 3, orderLine.PickLines.Count);
			AssertEquals("Sum of WZ_Units is 10.", 10m, orderLine.PickLines.Sum(p => p.WZ_Units));

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine2, 8m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine2.IsPacked);
			AssertEquals("Pick Lines stays as 3.", 3, orderLine.PickLines.Count);
			AssertEquals("Sum of WZ_Units is the same.", 10m, orderLine.PickLines.Sum(p => p.WZ_Units));

			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			releaseLine3.SerialNumber = "SN2";
			releaseLine3.Quantity = 1m;

			AssertEquals("2 for serials and 1 for empty.", 3, orderLine.PickLines.Count);
		}

		#endregion

		#region TestSplittingPackedPickLines_ReleaseCapture_Removing

		public void TestSplittingPackedPickLines_ReleaseCapture_Removing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "RED";

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine1, 6m);
			AssertEquals("PickLine should be split.", 2, orderLine.PickLines.Count);
			AssertEquals("PickLine should be split.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 6m && pl.WZ_ReleaseCapturedPartAttrib1 == "RED" && !pl.IsUnpacked(Factory)));
			AssertEquals("PickLine should be split.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 4m && pl.WZ_ReleaseCapturedPartAttrib1 == "RED" && pl.IsUnpacked(Factory)));

			releaseLine1.Quantity = 8m;
			AssertEquals("PickLine should remain split.", 3, orderLine.PickLines.Count);
			AssertEquals("PickLine should remain split.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 6m && pl.WZ_ReleaseCapturedPartAttrib1 == "RED" && !pl.IsUnpacked(Factory)));
			AssertEquals("Only unpacked PickLine should be changed.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 2m && pl.WZ_ReleaseCapturedPartAttrib1 == "RED" && pl.IsUnpacked(Factory)));
			AssertEquals("Only unpacked PickLine should be changed.", 1, orderLine.PickLines.Count(pl => pl.WZ_Units == 2m && pl.WZ_ReleaseCapturedPartAttrib1 != "RED" && pl.IsUnpacked(Factory)));
		}

		#endregion

		#region TestSumOfUnitsMet

		public void TestSumOfUnitsMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Sum of Units met should be sum of Release Lines.", 10m, releaseLines.SumOfUnitsMet);

			var releaseLine1 = releaseLines[0];
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.Quantity = 4m;
			AssertEquals("Sum of Units met should be sum of Release Lines.", 4m, releaseLines.SumOfUnitsMet);

			var releaseLine2 = releaseLines.AddNew();
			releaseLine1.PartAttribute1 = "BLUE";
			releaseLine2.Quantity = 5m;
			AssertEquals("Sum of Units met should be sum of Release Lines.", 9m, releaseLines.SumOfUnitsMet);

			releaseLine1.Quantity = 2m;
			AssertEquals("Sum of Units met should be sum of Release Lines.", 7m, releaseLines.SumOfUnitsMet);

			releaseLine1.Delete();
			AssertEquals("Sum of Units met should be sum of Release Lines.", 5m, releaseLines.SumOfUnitsMet);

			releaseLines.UpdateSumOfUnitsMet(releaseLine2, 2m);
			AssertEquals("UpdateSumOfUnitsMet() should add the changed Quantity.", 8m, releaseLines.SumOfUnitsMet);
		}

		#endregion

		#region TestSuspendListChanged

		public void TestSuspendListChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);

			bool listChangedFiredOnReleaseLines = false;
			((IBindingList)releaseLines).ListChanged += (sender, e) => listChangedFiredOnReleaseLines = true;

			using (releaseLines.SuspendListChanged())
			{
				releaseLines.AddNew();
				AssertEquals("List Changed suspended, so List Changed should not fire.", false, listChangedFiredOnReleaseLines);
			}

			AssertEquals("List Changed was suspended, but will fire when suspend finishes.", true, listChangedFiredOnReleaseLines);
			listChangedFiredOnReleaseLines = false; // clean-up

			releaseLines.AddNew();
			AssertEquals("List Changed is not suspended, so List Changed should fire.", true, listChangedFiredOnReleaseLines);
		}

		#endregion

		#region TestUpdateSumOfUnitsMet

		public void TestUpdateSumOfUnitsMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.Quantity = 1m;
			releaseLines.UpdateSumOfUnitsMet(releaseLine, 0m);
			AssertEquals("Calling UpdateSumOfUnitsMet(), should update SumOfUnitsMet.", 2m, releaseLines.SumOfUnitsMet);

			releaseLine.PartAttribute1 = "RED";

			AssertEquals("Should have Captured 1 Unit of Red.", 9m, orderLine.PickLines.Sum(p => p.UnreleaseCapturedQty));
			AssertNotNull("Should have Captured 1 Unit of Red.", orderLine.PickLines.Single(p => p.WZ_ReleaseCapturedPartAttrib1 == "RED"));
			AssertEquals("Should have Captured 1 Unit of Red.", 1m, orderLine.PickLines.Sum(p => p.ReleaseCapturedQty));

			orderLine.PickLines.DeleteAll();

			releaseLines.UpdateSumOfUnitsMet(releaseLine, 0m);
		}

		#endregion

		#region TestUpdateSumOfUnitsMet_UpdatesPackableItems

		public void TestUpdateSumOfUnitsMet_UpdatesPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);

			using (releaseLine.SuspendUpdatingSumOfUnitsMetAndOrderFields())
			{
				releaseLine.Quantity = 0m;
				AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);

				releaseLines.UpdateSumOfUnitsMet(releaseLine, 10m);
				AssertEquals("Should be no valid Release Lines.", 0, order.PackableItemParents.Count);

				releaseLine.Quantity = 5m;
				AssertEquals("Should be no valid Release Lines.", 0, order.PackableItemParents.Count);

				releaseLines.UpdateSumOfUnitsMet(releaseLine, 0m);
				AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			}

			var duplicateReleaseLine = releaseLines.AddNew();
			using (duplicateReleaseLine.SuspendUpdatingSumOfUnitsMetAndOrderFields())
			{
				duplicateReleaseLine.Quantity = 0;
				releaseLines.UpdateSumOfUnitsMet(duplicateReleaseLine, 0m);
				AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);

				duplicateReleaseLine.Quantity = 5m;
				releaseLines.UpdateSumOfUnitsMet(duplicateReleaseLine, 0m);
				AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);

				duplicateReleaseLine.PartAttribute1 = "RED"; // make release line non-duplicate
				AssertContainsExactElementsInAnyOrder(new[] { releaseLine, duplicateReleaseLine }, order.PackableItemParents.Typed);
			}
		}

		#endregion

		#region TestUpdateSumOfUnitsMet_WithNonCommittedReleaseLine

		public void TestUpdateSumOfUnitsMet_WithNonCommittedReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.Quantity = 4m;

			var nonCommittedReleaseLine = (WhsReleaseLine)((IBindingList)releaseLines).AddNew();
			nonCommittedReleaseLine.Quantity = 6m;
			nonCommittedReleaseLine.PartAttribute1 = "RED";

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Should be no Release Captured Attributes added for Non-Committed Elements.", false, orderLine.PickLines.Any(p => p.HasReleaseCapturedAttribs));

			releaseLines.UpdateSumOfUnitsMet(nonCommittedReleaseLine, 0m);
			AssertEquals("Should be no Release Captured Attributes added for Non-Committed Elements.", false, orderLine.PickLines.Any(p => p.HasReleaseCapturedAttribs));
		}

		#endregion

		#region TestUpdatingReleaseCapturedAttributes_UpdatesPackableItems

		public void TestUpdatingReleaseCapturedAttributes_UpdatesPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine) },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));

			releaseLine.PartAttribute1 = "RED";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine) },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));

			releaseLine.PartAttribute2 = "XXX";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine) },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));

			releaseLine.PartAttribute2 = "";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { GetGroupingKey(releaseLine) },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(p => p.Key));
		}

		#endregion

		#region TestMergePickLine

		public void TestMergePickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 5);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1-2"));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var releaseLines = orderLine.ReleaseLines;

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				var packableItems1 = releaseLines.GetPackableItems(releaseLines[0]);
				AssertEquals("Should have 2 packable items.", 2, packableItems1.Count());

				var pickLine = orderLine.PickLines[0];
				var newPickLine = pickLine.Split(7m);
				var packableItems2 = releaseLines.GetPackableItems(releaseLines[0]);
				AssertEquals("Still should have 2 packable items.", 2, packableItems2.Count());

				releaseLines.MergePickLine(newPickLine);
				var packableItems3 = releaseLines.GetPackableItems(releaseLines[0]);
				AssertEquals("Even list change events are delayed, after calling MergePickLine there should be 3 packable items.", 3, packableItems3.Count());
			}
		}

		#endregion

		// interfaces

		#region IBusiness Members

		public void TestRunPreSaveValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked.", 10m, orderLine.PickLineQuantity);

			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition: Nothing Release Captured.", 10m, pickLine.UnreleaseCapturedQty);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			var releaseLine = releaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			AssertEquals("Pick Line is Release Captured.", 0m, pickLine.UnreleaseCapturedQty);

			releaseLine.Validation.ValidateAll();
			AssertNoErrors(releaseLine);

			pickLine.Delete();
			releaseLine.Validation.ValidateAll();
			AssertHasError(releaseLine.PartAttribute1Info, "This Attribute cannot be Release Captured for the Product because there is not enough Stock Allocated to this Order Line.");

			((IBusiness)releaseLines).RunPreSaveValidation();

			AssertHasError(releaseLine.PartAttribute1Info, "This Attribute cannot be Release Captured for the Product because there is not enough Stock Allocated to this Order Line.");
			AssertHasWarning(orderLine.WE_OPInfo, WhsOrderLineValidation.ProductHasNoPalletDefinitionError);
		}

		public void TestRunPreSaveValidation_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked.", 10m, orderLine.PickLineQuantity);

			var pickLine = orderLine.PickLines.Single();
			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Precondition: Nothing Release Captured.", 10m, pickLine.UnreleaseCapturedQty);

			// add new Release Captured Attribute directly to pick Line as could happen in RF.
			pickLine.WZ_Units = 1m;
			pickLine.WZ_ReleaseCapturedSerialNumber = "SN1";
			releaseLines.MergePickLine(pickLine); // you can't change release lines on the UI without the internal dictionary knowing about it

			releaseLines.RunPreSaveValidation();
			AssertHasRowError(orderLine, @$"There are 1x [Serial Number: SN1] Release Captured Attributes for Product P1 but only 0 released on this Order Line.
This may have been caused by entering Release Captured Attributes in {Core.Constants.ProductName} before Release Capturing Attributes in RF. You must update the Release Lines for this Order line to match the Release Captured Quantity.");
		}

		#region TestFinalisationValidation_DBHits

		public void TestFinalisationValidation_DBHits()
		{
			const int numberOfLinesToCreate = 15; // Lines to create

			// Set up collection with 15 releaselines with each having a unique serial number
			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO");
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), data.Part1, numberOfLinesToCreate);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			for (var i = 0; i < numberOfLinesToCreate; i++)
			{
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			}

			var pick = Helper.CreatePickNew(order);

			var serialNumber = 0;
			foreach (var line in order.Lines.Cast<WhsOrderLine>())
			{
				AssertEquals("Precondition: Order line has release line", 1, line.ReleaseLines.Count);
				var releaseLine = line.ReleaseLines[0];
				releaseLine.Quantity = 1m;
				releaseLine.SerialNumber = "SN" + (++serialNumber);
				AssertNoErrors(releaseLine.PartAttribute1Info);
			}

			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ RefCountrySchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 3 },
				{ StmEventSchema.Constants.TableName, 4 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 5 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				// See WI00182969 - Investigate Release Capture Finalise Performance
				// 30 hits from serial number uniqueness check.
				{ WhsPickLineSchema.Constants.TableName, 34 },
				{ WhsPickByLabelJobSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 17 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
			};

			// Load order and pick then finalise both
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newFactory_Order = newFactory.Load<WhsOrder>(order.PK);

			using (RowFactory.SetCachedTables())
			using (AssertDbHitsWithUsefulQueryInformation(expectedHitCounts, newFactory))
			{
				newFactory_Order.FinaliseDocket();
				AssertIsFinalisedPrecondition(newFactory_Order);

				var newFactory_Pick = newFactory.Load<WhsPick>(pick.PK);
				newFactory_Pick.FinalisePick();
				AssertIsFinalisedPrecondition(newFactory_Pick);
			}
		}

		#endregion

		#endregion

		#region ICollection Members

		public void TestCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked.", 10m, orderLine.PickLineQuantity);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Count should show One Release Line.", 1, releaseLines.Count);
			AssertEquals("Count should show One Release Line.", 1, ((ICollection)releaseLines).Count);
			AssertEquals("Count should show One Release Line.", 1, ((BusinessObjectCollection)releaseLines).Count);

			var releaseLine1 = releaseLines[0];
			releaseLine1.Quantity = 3m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 7m;
			releaseLine2.PartAttribute1 = "BLUE";
			AssertEquals("Count should show Two Release Lines.", 2, releaseLines.Count);
			AssertEquals("Count should show Two Release Lines.", 2, ((ICollection)releaseLines).Count);

			releaseLines.ClearCollection();

			using (releaseLines.SuspendRebuild())
			{
				AssertEquals("While Release Lines Collection is invalidated, it should be empty.", 0, releaseLines.Count);
			}

			AssertEquals("Accessing count through interface should rebuild invalidated Release Lines.", 2, ((ICollection)releaseLines).Count);
			releaseLines.ClearCollection();

			using (releaseLines.SuspendRebuild())
			{
				AssertEquals("While Release Lines Collection is invalidated, it should be empty.", 0, releaseLines.Count);
			}

			AssertEquals("Accessing count through class should rebuild invalidated Release Lines.", 2, releaseLines.Count);
		}

		#endregion

		#region IEnumerable Members

		public void TestIEnumerable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Order is picked.", 10m, orderLine.PickLineQuantity);

			var releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
			AssertEquals("Enumeration should find One Release Line.", 1, releaseLines.Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find One Release Line.", 1, ((IEnumerable<BusinessObject>)releaseLines).Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find One Release Line.", 1, ((BusinessObjectCollection)releaseLines).Where(r => r != null).ToArray().Length);

			var releaseLine1 = releaseLines[0];
			releaseLine1.Quantity = 3m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = releaseLines.AddNew();
			releaseLine2.Quantity = 7m;
			releaseLine2.PartAttribute1 = "BLUE";
			AssertEquals("Enumeration should find Two Release Lines.", 2, releaseLines.Where(r => r != null).ToArray().Length);
			AssertEquals("Enumeration should find Two Release Lines.", 2, ((IEnumerable<BusinessObject>)releaseLines).Where(r => r != null).ToArray().Length);

			releaseLines.ClearCollection();

			using (releaseLines.SuspendRebuild())
			{
				AssertEquals("While Release Lines Collection is invalidated, it should be empty.", 0, releaseLines.Where(r => r != null).ToArray().Length);
			}

			AssertEquals("Accessing Enumerator through interface should rebuild invalidated Release Lines.", 2, ((IEnumerable<BusinessObject>)releaseLines).Where(r => r != null).ToArray().Length);
			releaseLines.ClearCollection();

			using (releaseLines.SuspendRebuild())
			{
				AssertEquals("While Release Lines Collection is invalidated, it should be empty.", 0, releaseLines.Where(r => r != null).ToArray().Length);
			}

			AssertEquals("Accessing Enumerator through class should rebuild invalidated Release Lines.", 2, releaseLines.Where(r => r != null).ToArray().Length);
		}

		#endregion

		//

		#region Implementation

		static GroupingKey GetGroupingKey(IPackableItemParent releaseLine)
		{
			return releaseLine.PackableItems.FirstOrDefault()?.Key;
		}

		protected override WhsReleaseLineCollection GetCollectionToTest()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			return WhsReleaseLineCollection.GetNewReleaseLinesCollection(orderLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			return new WhsReleaseLine(orderLine);
		}

		#endregion

	}
}
