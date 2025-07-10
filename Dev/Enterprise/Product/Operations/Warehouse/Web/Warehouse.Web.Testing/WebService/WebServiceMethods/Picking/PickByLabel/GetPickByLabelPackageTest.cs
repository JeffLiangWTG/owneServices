using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.PickByLabel.Testing;
using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.Testing
{
	class GetPickByLabelPackageTest : WhsPickingSecureServiceTestCase
	{
		#region PickByLabel

		#region TestGetPickByLabelPackage

		public void TestGetPickByLabelPackage_Pallet()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Pallet);
		}

		public void TestGetPickByLabelPackage_Pallet_AllFlagsChecked()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase);
		}

		public void TestGetPickByLabelPackage_Pallet_PickByLabelDisabled()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Pallet, uomPickTypesToEnable: "");
		}

		public void TestGetPickByLabelPackage_Pallet_OnlyPickByCases()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Pallet, uomPickTypesToEnable: UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_Case()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_Case_AllFlagsChecked()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase);
		}

		public void TestGetPickByLabelPackage_Case_PickByLabelDisabled()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Case, uomPickTypesToEnable: "");
		}

		public void TestGetPickByLabelPackage_Case_OnlyPickByPallet()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.Case, uomPickTypesToEnable: UOMPackTypesList.Codes.Pallet);
		}

		public void TestGetPickByLabelPackage_SplitCase()
		{
			TestGetPickByLabelPackage(UOMPackTypesList.Codes.SplitCase);
		}

		void TestGetPickByLabelPackage(string uomType, params string[] uomPickTypesToEnable)
		{
			if (!uomPickTypesToEnable.Any())
			{
				uomPickTypesToEnable = new[] { uomType };
			}

			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomType);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomPickTypesToEnable.Contains(UOMPackTypesList.Codes.SplitCase);
			pick.WP_PickCasesByLabel = uomPickTypesToEnable.Contains(UOMPackTypesList.Codes.Case);
			pick.WP_PickPalletsByLabel = uomPickTypesToEnable.Contains(UOMPackTypesList.Codes.Pallet);

			AssertEquals("Precondition.", 4, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pick.GetAllPickLines().All(pl => pl.AllocatedPackType.F3_UOMType == uomType));

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package1, pickLines[1]);
			packingHelper.CreatePackageDivot(package2, pickLines[2]);
			packingHelper.CreatePackageDivot(package1, pickLines[3]);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-2", false);

			AssertSuccessfulResponse(response1, webService1);
			AssertSuccessfulResponse(response2, webService2);

			if (uomPickTypesToEnable.Contains(uomType) && uomType != UOMPackTypesList.Codes.SplitCase)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);
					AssertNotNull("Expecting non null job.", response1.Job);
					AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response1.Job.Reference);

					AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
					AssertNotNull("Expecting non null job.", response2.Job);
					AssertEquals("Expecting job with correct ID.", "PACKAGE-2", response2.Job.Reference);
				});

				pickLines = pick.GetAllPickLines().ToArray();
				AssertEquals("All picklines should be is picking", true, pickLines.Cast<WhsPickLine>().All(pl => pl.WZ_IsPicking));

				AssertPickLines(pickLines.Except(pickLines[2]).Cast<WhsPickLine>(), response1.Job.Lines);
				AssertPickLines(new[] { pickLines[2] }, response2.Job.Lines);

				// clean up
				foreach (var pickLine in pick.GetAllPickLines())
				{
					pickLine.WZ_GS_NKAssignedTo = "";
					pickLine.WZ_IsPicking = false;
				}

				// Pick 1 line, assert that it is not returned
				pick.GetAllPickLines().First().WZ_PickedDateTime = ZDateTimeOffset.Today;
				Helper.Factory.Save();

				var webService3 = GetNewWebService(data.Whs1, staff);
				var response3 = webService3.GetPickByLabelPackage("PACKAGE-1", false);
				AssertSuccessfulResponse(response3, webService3);

				var picklines = pick.GetAllPickLines().ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Expecting response without error.", ErrorTypes.None, response3.Error);
					AssertNotNull("Expecting non null job.", response3.Job);
					AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response3.Job.Reference);
				});
				AssertPickLines(new[] { picklines[1], picklines[3] }, response3.Job.Lines);

				// Putaway the picked line so it's not returned to the RF device (i.e. simulate user scanning at end of picking)
				var dockDoorTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
				dockDoorTransferLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(dockDoorTransferLine);

				// Suspend triggers in order to allow incorrect data for testing WS.
				using (SuspendTrigger("TG_WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalized", WhsPickLineSchema.Constants.TableName))
				using (SuspendTrigger("TG_PreventReassigningPickLine", WhsPickLineSchema.Constants.TableName))
				{
					foreach (var pickLine in new[] { pickLines[0], pickLines[1], pickLines[3] })
					{
						pickLine.WZ_GS_NKAssignedTo = "BRS";
					}
					Helper.Factory.Save();
				}

				var webService4 = GetNewWebService(data.Whs1, staff);
				var response4 = webService4.GetPickByLabelPackage("PACKAGE-1", false);
				AssertSuccessfulResponse(response4, webService4);

				CombineAssertions(() =>
				{
					AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response4.Error);
					AssertNull(response4.Job);
					AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response4.ErrorMessage);
				});
			}
			else
			{
				// Split case lines or lines on a pick with Pick By Label not enabled should not be returned in Pick By Label
				CombineAssertions(() =>
				{
					AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response1.Error);
					AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response1.ErrorMessage);
					AssertNull(response1.Job);

					AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response2.Error);
					AssertEquals("Label 'PACKAGE-2' cannot be found or assigned to someone else.", response2.ErrorMessage);
					AssertNull(response2.Job);
				});
			}
		}

		#region SuspendTrigger

		IDisposable SuspendTrigger(string triggerName, string tableName)
		{
			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => Db.Connection.ExecuteNonQuery($"ENABLE TRIGGER {triggerName} ON {tableName}"));
		}

		#endregion

		#endregion

		#region TestGetPickByLabelPackage_AlreadyAssignedToAnotherUser

		public void TestGetPickByLabelPackage_AlreadyAssignedToAnotherUser()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff1 = Helper.CreateGlbStaff("1", "1");
			var staff2 = Helper.CreateGlbStaff("2", "2");
			var staff3 = Helper.CreateGlbStaff("3", "3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			AssertEquals("Precondition.", 3, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pick.GetAllPickLines().All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package1, pickLines[1]);
			packingHelper.CreatePackageDivot(package1, pickLines[2]);
			Helper.Factory.Save();

			// Assign 1 line to ourselves, assert that there is no prompt
			pickLines = pick.GetAllPickLines().ToArray();
			pickLines[2].WZ_GS_NKAssignedTo = staff1.GS_Code;

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Expecting not null job.", response1.Job);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);

			pickLines = pick.GetAllPickLines().ToArray();
			AssertPickLines(pickLines, response1.Job.Lines);

			foreach (var pickline in pickLines)
			{
				pickline.WZ_GS_NKAssignedTo = "";
				pickline.WZ_IsPicking = false;
			}

			Helper.Factory.Save();

			// Assign 1 line to another user, assert that we prompt the user to reassign
			pick.GetAllPickLines().First().WZ_GS_NKAssignedTo = staff2.GS_Code;

			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.YesNoEnquiry, response2.Error);
				AssertEquals("Some of the items are assigned to be picked by '2'. Would you like to assign all items to yourself?", response2.ErrorMessage);
				AssertNull("Expecting null job.", response2.Job);
			});

			pickLines = pick.GetAllPickLines().ToArray();
			// Assign 2 lines, make sure the error message works properly
			pickLines[1].WZ_GS_NKAssignedTo = staff3.GS_Code;

			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1, staff1);
			var response3 = webService3.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.YesNoEnquiry, response3.Error);
				AssertEquals("Some of the items are assigned to be picked by '2', '3'. Would you like to assign all items to yourself?", response3.ErrorMessage);
				AssertNull("Expecting null job.", response3.Job);
			});

			// Pick one line, should not prompt for that line
			pick.GetAllPickLines().First().WZ_PickedDateTime = ZDateTimeOffset.Today;

			Helper.Factory.Save();

			var webService4 = GetNewWebService(data.Whs1, staff1);
			var response4 = webService4.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response4, webService4);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.YesNoEnquiry, response4.Error);
				AssertEquals("Some of the items are assigned to be picked by '3'. Would you like to assign all items to yourself?", response4.ErrorMessage);
				AssertNull("Expecting null job.", response4.Job);
			});

			// Send response with reassign flag set, should reassign and return the unpicked line (but not the picked on)
			var webService5 = GetNewWebService(data.Whs1, staff1);
			var response5 = webService5.GetPickByLabelPackage("PACKAGE-1", true);
			AssertSuccessfulResponse(response5, webService5);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response5.Error);
			AssertNotNull("Expecting not null job.", response5.Job);

			pickLines = pick.GetAllPickLines().ToArray();
			var unpickedPickLines = pickLines.Except(pickLines.First());

			AssertPickLines(unpickedPickLines, response5.Job.Lines);
			AssertEquals("All lines returned should be assigned to the current user.", true, unpickedPickLines.All(pl => pl.WZ_GS_NKAssignedTo == staff1.GS_Code));
		}

		#endregion

		#region TestGetPickByLabelPackage_PackageInAnotherWarehouse

		public void TestGetPickByLabelPackage_PackageInAnotherWarehouse()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var whs2 = Helper.CreateWarehouse("2");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			whs2.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pick.GetAllPickLines().All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pick.GetAllPickLines().Single());
			Helper.Factory.Save();

			// Assert that the package doesnt get returned if we are logged in in another warehouse
			var webService1 = GetNewWebService(whs2, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response1.Error);
				AssertNull(response1.Job);
				AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response1.ErrorMessage);
			});

			// Assert that changing the warehouse results in the pallet being found
			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
			AssertNotNull("Expecting not null job.", response2.Job);

			AssertPickLines(pick.GetAllPickLines(), response2.Job.Lines);
		}

		#endregion

		#region TestGetPickByLabelPackage_PackageClosed

		public void TestGetPickByLabelPackage_PackageClosed()
		{
			TestGetPickByLabelPackage_PackageClosedOrReleasedFlags((package, isSet) => package.KP_ClosedTimeUtc = isSet ? ZDateTime.UtcNow : ZDateTime.Empty);
		}

		public void TestGetPickByLabelPackage_PackageReleased()
		{
			TestGetPickByLabelPackage_PackageClosedOrReleasedFlags((package, isSet) => package.KP_ReleasedTimeUtc = isSet ? ZDateTime.UtcNow : ZDateTime.Empty);
		}

		public void TestGetPickByLabelPackage_PackageReleasedViaJob()
		{
			TestGetPickByLabelPackage_PackageClosedOrReleasedFlags((package, isSet) => package.KP_IsReleasedViaJob = isSet);
		}

		void TestGetPickByLabelPackage_PackageClosedOrReleasedFlags(Action<PkgPackage, bool> packageClosedOrReleasedSetter)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", 1, picklines.Count());
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, picklines.First());
			packageClosedOrReleasedSetter(package1, true);
			Helper.Factory.Save();

			// Assert that the package doesnt get returned
			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response1.Error);
				AssertNull(response1.Job);
				AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response1.ErrorMessage);
			});

			// Assert that changing the flag results in the package getting returned
			packageClosedOrReleasedSetter(package1, false);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
			AssertNotNull("Expecting not null job.", response2.Job);

			AssertPickLines(pick.GetAllPickLines(), response2.Job.Lines);
		}

		#endregion

		#region TestGetPickByLabelPackage_WithInvalidPackedItems

		public void TestGetPickByLabelPackage_WithInvalidPackedItems_Case_AndSplitCase()
		{
			TestGetPickByLabelPackage_WithInvalidPackedItems(UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase);
		}

		public void TestGetPickByLabelPackage_WithInvalidPackedItems_Case_AndPallet()
		{
			TestGetPickByLabelPackage_WithInvalidPackedItems(UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet);
		}

		public void TestGetPickByLabelPackage_WithInvalidPackedItems_Case_AndEmpty()
		{
			TestGetPickByLabelPackage_WithInvalidPackedItems(UOMPackTypesList.Codes.Case, "");
		}

		public void TestGetPickByLabelPackage_WithInvalidPackedItems_Pallet_AndSplitCase()
		{
			TestGetPickByLabelPackage_WithInvalidPackedItems(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.SplitCase);
		}

		public void TestGetPickByLabelPackage_WithInvalidPackedItems_Pallet_AndCase()
		{
			TestGetPickByLabelPackage_WithInvalidPackedItems(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_WithInvalidPackedItems_Pallet_AndEmpty()
		{
			TestGetPickByLabelPackage_WithInvalidPackedItems(UOMPackTypesList.Codes.Pallet, "");
		}

		void TestGetPickByLabelPackage_WithInvalidPackedItems(string uomTypeForPickByLabel, string invalidUOMTypeAdded)
		{
			// There can be some weird scenarios which result from the user manually adding items into a Pick By Label package.
			// After discussion with product team, we decided the best behaviour would be to show an error message if the
			// data shape does not match what we would expect from pick by label.
			// These are case that "shouldn't happen" in practice.
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Bag, invalidUOMTypeAdded);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order, data.Part2, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var picklines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition.", 2, picklines.Length);
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package1, picklines[1]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Expecting response with error.", "Package has invalid Packed Items and cannot be Picked By Label.", response.ErrorMessage);
				AssertNull("Expecting null job.", response.Job);
			});
		}

		#endregion

		#region TestGetPickByLabelActiveJob_CheckPackageContainBOMLine

		public void TestGetPickByLabelActiveJob_CheckPackageContainBOMLine_AllArePickByBOMLines()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 2);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 40m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, bike, 10m);
			Helper.CreateWhsOrderLine(order, bike, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickPalletsByLabel = false;

			var picklines = pick.GetAllPickLines().ToArray();
			picklines.ToList().ForEach(pickLine => pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package1, picklines[1]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);

			AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Expecting response with error.", "Package has BOM kits to assemble and cannot be Picked by Label.", response.ErrorMessage);
			AssertNull("Expecting null job.", response.Job);
		}

		public void TestGetPickByLabelActiveJob_CheckPackageContainBOMLine_MixedLinesWithPickByBOMLine()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 2);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bike, 4m);
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, bike, 14m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickPalletsByLabel = false;

			var picklines = pick.GetAllPickLines().ToArray();
			picklines.ToList().ForEach(pickLine => pickLine.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package1, picklines[1]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);

			AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Expecting response with error.", "Package has BOM kits to assemble and cannot be Picked by Label.", response.ErrorMessage);
			AssertNull("Expecting null job.", response.Job);
		}

		#endregion

		#region TestGetPickByLabelPackage_WithMultipleLocations

		public void TestGetPickByLabelPackage_WithMultipleLocations_Case()
		{
			TestGetPickByLabelPackage_WithMultipleLocations(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_WithMultipleLocations_Pallet()
		{
			TestGetPickByLabelPackage_WithMultipleLocations(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_WithMultipleLocations(string uomTypeForPickByLabel)
		{
			// This can only happen if the user has modified the package manually
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			AssertNotEquals("Precondition", inv1.WI_WL, inv2.WI_WL);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var picklines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition.", 2, picklines.Length);
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package1, picklines[1]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Expecting response with error.", "Package has lines from multiple Locations and cannot be Picked By Label.", response.ErrorMessage);
				AssertNull("Expecting null job.", response.Job);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_DuplicatedIDs

		public void TestGetPickByLabelPackage_DuplicatedIDs()
		{
			// Packages created for Pick By Label should have a unique ID but this might not be system wide unique
			// If they don't (until package selection is potentially implemented later), we should return one package
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition.", 2, picklines.Length);
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			var package2 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, picklines[0]);
			packingHelper.CreatePackageDivot(package2, picklines[1]);
			Helper.Factory.Save();

			// Assert that only one (any one) package is found
			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);
			AssertNotNull("Expecting not null job.", response1.Job);
			AssertEquals("Should contain 1 pick line.", 1, response1.Job.Lines.Count);

			// Pick + putaway first pick line, assert package 2 is returned
			picklines = pick.GetAllPickLines().ToArray();
			var pickingPickLine = picklines.Single(pl => pl.WZ_IsPicking);
			pickingPickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var dockDoorTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			dockDoorTransferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(dockDoorTransferLine);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
			AssertNotNull("Expecting not null job.", response2.Job);

			var unpickedPickline = pick.GetAllPickLines().Single(pl => pl.PK != pickingPickLine.PK);
			AssertPickLines(new[] { unpickedPickline }, response2.Job.Lines);
		}

		#endregion

		#region TestGetPickByLabelPackage_EmptyID

		public void TestGetPickByLabelPackage_EmptyID()
		{
			var webService = GetNewWebService();
			var emptyResponse = webService.GetPickByLabelPackage("", false);
			AssertSuccessfulResponse(emptyResponse, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Please provide a Label ID.", emptyResponse.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, emptyResponse.Error);
				AssertNull(emptyResponse.Job);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_NullID

		public void TestGetPickByLabelPackage_NullID()
		{
			var webService = GetNewWebService();
			var nullResponse = webService.GetPickByLabelPackage(null, false);
			AssertSuccessfulResponse(nullResponse, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Please provide a Label ID.", nullResponse.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, nullResponse.Error);
				AssertNull(nullResponse.Job);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_InvalidID

		public void TestGetPickByLabelPackage_InvalidID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertNull(response.Job);
			AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response.ErrorMessage);
		}

		#endregion

		#region TestGetPickByLabelPackage_FiltersPickLinesForCommittingDockDoorStock

		public void TestGetPickByLabelPackage_FiltersPickLinesForCommittingDockDoorStock()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.WD_PickPriority = 1;
			var order1Line1 = order.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			pick.GetAllPickLines().ForEach(pl => pl.WZ_GS_NKAssignedTo = staff.GS_Code);

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, order1Line1.PickLines.Single());
			packingHelper.CreatePackageDivot(package1, order1Line2.PickLines.Single());

			Helper.PickAndMakeInTransitTransfer(order1Line1.PickLines.Single(), ZDateTimeOffset.Now);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_GS_NKAssignedTo = staff.GS_Code);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
				AssertEquals("Should *not* be Putaway only.", false, response.Job.IsPutawayOnly);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response.Job.Reference);
			});

			AssertPickLines(order1Line2.PickLines, response.Job.Lines);
		}

		#endregion

		#region TestGetPickByLabelPackage_RequiresPutaway

		public void TestGetPickByLabelPackage_RequiresPutaway_Case()
		{
			TestGetPickByLabelPackage_RequiresPutaway(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_RequiresPutaway_Pallet()
		{
			TestGetPickByLabelPackage_RequiresPutaway(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_RequiresPutaway(string uomTypeForPickByLabel)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response.Job.Reference);
				AssertEquals("Should be putaway only.", true, response.Job.IsPutawayOnly);
				AssertEquals("Should have no lines.", false, response.Job.Lines.Any());
			});
		}

		public void TestGetPickByLabelPackage_RequiresPutaway_NotPickedByPicker()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickCasesByLabel = false;
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);

			Helper.Factory.Save();

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);

			var initialJobCount = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Length;
			AssertEquals("Initially the staff should not be assigned a job", 0, initialJobCount);

			// start picking the package that requires putaway
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);
		}

		public void TestGetPickByLabelPackage_RequiresPutaway_PickedByPicker()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickCasesByLabel = false;
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);

			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = pickByLabelJob.Labels.Single();

			// complete the pick, closing the task
			var pickingInfo = new PickingInfo(5m, true);
			var completePickResponse = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				pickingInfo,
				null,
				true);

			// pick the same package that now requires putaway
			var getLabelResponse = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(getLabelResponse, webService);
			AssertNotNull("Expecting non null job.", getLabelResponse.Job);

			// ensure extra job, label and task were not created
			var otherFactory = new BusinessObjectFactory();
			var pickByLabelJobInOtherFactory = otherFactory.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			var labelInOtherFactory = pickByLabelJobInOtherFactory.Labels.Single();
		}

		#endregion

		#region TestGetPickByLabelPackage_AlreadyPutaway

		public void TestGetPickByLabelPackage_AlreadyPutaway_Case()
		{
			TestGetPickByLabelPackage_AlreadyPutaway(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_AlreadyPutaway_Pallet()
		{
			TestGetPickByLabelPackage_AlreadyPutaway(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_AlreadyPutaway(string uomTypeForPickByLabel)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertEquals("Should be finalised.", true, transferLine.IsFinalised);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertNull(response.Job);
				AssertEquals("Label 'PACKAGE-1' is already picked and putaway.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_AssignedPutaway

		public void TestGetPickByLabelPackage_AssignedPutaway_Case()
		{
			TestGetPickByLabelPackage_AssignedPutaway(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_AssignedPutaway_Pallet()
		{
			TestGetPickByLabelPackage_AssignedPutaway(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_AssignedPutaway(string uomTypeForPickByLabel)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package1, pickLine2);
			Helper.Factory.Save();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Should be finalised.", true, transferLine1.IsFinalised);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response.Job.Reference);
				AssertEquals("Should be putaway only.", true, response.Job.IsPutawayOnly);
				AssertEquals("Should have no lines.", false, response.Job.Lines.Any());
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_AddPackageFromPickHasDifferentLocationFromExistingPickOnPickJob

		public void TestGetPickByLabelPackage_AddPackageFromPickHasDifferentLocationFromExistingPickOnPickJob_PackingStation()
		{
			TestGetPickByLabelPackage_AddPackageFromPickHasDifferentLocationFromExistingPickOnPickJobCore(differentPackingStation: true);
		}

		public void TestGetPickByLabelPackage_AddPackageFromPickHasDifferentLocationFromExistingPickOnPickJob_DockDoor()
		{
			TestGetPickByLabelPackage_AddPackageFromPickHasDifferentLocationFromExistingPickOnPickJobCore(differentPackingStation: false);
		}

		void TestGetPickByLabelPackage_AddPackageFromPickHasDifferentLocationFromExistingPickOnPickJobCore(bool differentPackingStation)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test1", false, 0, LocationClasses.Codes.DDL);
			var otherDockDoorLocation = data.Whs1.FindLocation("A-2");
			otherDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var packingStationLocationType = Helper.CreateLocationType("ABC", "Test2", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "K");
			var otherPackingLocation = newRow1.Locations[0];
			otherPackingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = false;

			pick.WP_WL_PackingStation = differentPackingStation ? packingLocation.PK : pick.WP_WL_PackingStation;

			var pickLines1 = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines1[0];
			var pickLine2 = pickLines1[1];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1-1");
			var package2 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1-2");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Should be putaway.", true, transferLine1.IsPutaway);
			AssertEquals("Should NOT be putaway.", false, transferLine2.IsPutaway);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Helper.Factory.Save();
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_CartoniseSplitCases = false;
			pick2.WP_PickCasesByLabel = true;
			pick2.WP_PickPalletsByLabel = false;

			pick2.WP_WL_DockDoor = differentPackingStation ? pick2.WP_WL_DockDoor : otherDockDoorLocation.PK;
			pick2.WP_WL_PackingStation = differentPackingStation ? otherPackingLocation.PK : pick2.WP_WL_PackingStation;

			var pickLines2 = pick2.GetAllPickLines().ToArray();
			var pickLine3 = pickLines2[0];
			var pickLine4 = pickLines2[1];
			pickLine3.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine4.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package3 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-1");
			var package4 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-2");
			packingHelper.CreatePackageDivot(package3, pickLine3);
			packingHelper.CreatePackageDivot(package4, pickLine4);
			Helper.Factory.Save();

			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);
			transferLine3.FinaliseDocketLine();
			AssertEquals("Should be putaway.", true, transferLine3.IsPutaway);
			AssertEquals("Should NOT be putaway.", false, transferLine4.IsPutaway);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1-2", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Expecting non null job.", response1.Job);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-2-2", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertNull(response2.Job);
			AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response2.Error);
			var expectedErrorMessage = differentPackingStation
				? "Error - WTL_KP_Package: Package 'PACKAGE-2-2' is assigned to a different packing station location than previous labels."
				: "Error - WTL_KP_Package: Package 'PACKAGE-2-2' is assigned to a different dock door location than previous labels.";
			AssertEquals(expectedErrorMessage, response2.ErrorMessage);
		}

		public void TestGetPickByLabelPackage_AddPackageFromPickHasNoPackingStation_PopulatePackingStationForNewPackage()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var packingStationLocationType = Helper.CreateLocationType("ABC", "Test2", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = false;

			pick.WP_WL_PackingStation = packingLocation.PK;

			var pickLines1 = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines1[0];
			var pickLine2 = pickLines1[1];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1-1");
			var package2 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1-2");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Should be putaway.", true, transferLine1.IsPutaway);
			AssertEquals("Should NOT be putaway.", false, transferLine2.IsPutaway);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Helper.Factory.Save();
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_CartoniseSplitCases = false;
			pick2.WP_PickCasesByLabel = true;
			pick2.WP_PickPalletsByLabel = false;

			var pickLines2 = pick2.GetAllPickLines().ToArray();
			var pickLine3 = pickLines2[0];
			var pickLine4 = pickLines2[1];
			pickLine3.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine4.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package3 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-1");
			var package4 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-2");
			packingHelper.CreatePackageDivot(package3, pickLine3);
			packingHelper.CreatePackageDivot(package4, pickLine4);
			Helper.Factory.Save();

			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);
			transferLine3.FinaliseDocketLine();
			AssertEquals("Should be putaway.", true, transferLine3.IsPutaway);
			AssertEquals("Should NOT be putaway.", false, transferLine4.IsPutaway);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1-2", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Expecting non null job.", response1.Job);

			AssertEquals("Precondition: Packing Station on Pick2 should be empty", ZGuid.Empty, pick2.WP_WL_PackingStation);
			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-2-2", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
			AssertNotNull("Expecting non null job.", response2.Job);
			AssertEquals("Expecting job with correct ID.", "PACKAGE-2-2", response2.Job.Reference);
			AssertEquals("Should populate Packing Station on Pick2", packingLocation.PK, pick2.WP_WL_PackingStation);
		}

		public void TestGetPickByLabelPackage_AddPackageFromPickHasNoPackingStation_DBHits()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Case);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var packingStationLocationType = Helper.CreateLocationType("ABC", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			Helper.Factory.Save();
			for (var i = 0; i < 5; i++)
			{
				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1" + i);
				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2" + i);
				var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3" + i);
				var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4" + i);
				for (var j = 0; j < 10; j++)
				{
					Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
					Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
					Helper.CreateWhsOrderLine(order3, data.Part1, 5m);
					Helper.CreateWhsOrderLine(order4, data.Part1, 5m);
				}
				var pick = Helper.CreatePickNew(order1, order2, order3, order4);
				pick.WP_CartoniseSplitCases = false;
				pick.WP_PickCasesByLabel = true;
				pick.WP_PickPalletsByLabel = false;

				pick.WP_WL_PackingStation = packingLocation.PK;
				var pickLines = pick.GetAllPickLines().ToArray();
				pickLines.ForEach(pl => pl.WZ_GS_NKAssignedTo = staff.GS_Code);
				var pickLinesForOrder1 = pickLines.Where(pl => pl.DocketLine.WE_WD.Equals(order1.PK));
				var pickLinesForOrder2 = pickLines.Where(pl => pl.DocketLine.WE_WD.Equals(order2.PK));
				var pickLinesForOrder3 = pickLines.Where(pl => pl.DocketLine.WE_WD.Equals(order3.PK));
				var pickLinesForOrder4 = pickLines.Where(pl => pl.DocketLine.WE_WD.Equals(order4.PK));
				for (var j = 0; j < 10; j++)
				{
					var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1-" + i + j);
					var package2 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-" + i + j);
					var package3 = order3.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-3-" + i + j);
					var package4 = order4.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-4-" + i + j);
					packingHelper.CreatePackageDivot(package1, pickLinesForOrder1.ElementAt(j));
					packingHelper.CreatePackageDivot(package2, pickLinesForOrder2.ElementAt(j));
					packingHelper.CreatePackageDivot(package3, pickLinesForOrder3.ElementAt(j));
					packingHelper.CreatePackageDivot(package4, pickLinesForOrder4.ElementAt(j));
				}
				pickLines.ForEach(pl => Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now));
			}

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			for (var i = 0; i < 5; i++) //Each Pick is called once
			{
				var response1 = webService1.GetPickByLabelPackage("PACKAGE-1-" + i + 1, false);
				AssertSuccessfulResponse(response1, webService1);
				AssertNotNull("Expecting non null job.", response1.Job);
			}

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Helper.Factory.Save();
			var newOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(newOrder, data.Part1, 5m);
			Helper.CreateWhsOrderLine(newOrder, data.Part1, 5m);
			var newPick = Helper.CreatePickNew(newOrder);
			newPick.WP_CartoniseSplitCases = false;
			newPick.WP_PickCasesByLabel = true;
			newPick.WP_PickPalletsByLabel = false;

			var newPickLines = newPick.GetAllPickLines().ToArray();
			var pickLine1 = newPickLines[0];
			var pickLine2 = newPickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;

			var newPackage1 = newOrder.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-1");
			var newPackage2 = newOrder.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2-2");
			packingHelper.CreatePackageDivot(newPackage1, pickLine1);
			packingHelper.CreatePackageDivot(newPackage2, pickLine2);
			Helper.Factory.Save();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Should be putaway.", true, transferLine1.IsPutaway);
			AssertEquals("Should NOT be putaway.", false, transferLine2.IsPutaway);
			Helper.Factory.Save();

			AssertEquals("Precondition: Packing Station on Pick2 should be empty", ZGuid.Empty, newPick.WP_WL_PackingStation);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 4 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 6 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 3 },
				{ WhsPickByLabelJobSchema.Constants.TableName, 1 },
				{ WhsPickByLabelLabelSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 7 },
				{ WhsDockDoorAssignmentSchema.Constants.TableName, 2 },
			};

			var webService2 = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService2.Factory))
			{
				var response2 = webService2.GetPickByLabelPackage("PACKAGE-2-2", false);
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
				AssertNotNull("Expecting non null job.", response2.Job);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-2-2", response2.Job.Reference);
				AssertEquals("Should populate Packing Station on Pick2", packingLocation.PK, newPick.WP_WL_PackingStation);
			}
		}

		#endregion

		#region TestGetPickByLabelPackage_UnassignedPutaway

		public void TestGetPickByLabelPackage_UnassignedPutaway_Case()
		{
			TestGetPickByLabelPackage_UnassignedPutaway(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_UnassignedPutaway_Pallet()
		{
			TestGetPickByLabelPackage_UnassignedPutaway(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_UnassignedPutaway(string uomTypeForPickByLabel)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_GS_NKPutawayBy = "";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertNull(response.Job);
				AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_PutawayAssignedToSomeoneElse

		public void TestGetPickByLabelPackage_PutawayAssignedToSomeoneElse_Case()
		{
			TestGetPickByLabelPackage_PutawayAssignedToSomeoneElse(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_PutawayAssignedToSomeoneElse_Pallet()
		{
			TestGetPickByLabelPackage_PutawayAssignedToSomeoneElse(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_PutawayAssignedToSomeoneElse(string uomTypeForPickByLabel)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff1 = Helper.CreateGlbStaff("A", "A");
			var staff2 = Helper.CreateGlbStaff("B", "B");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff1.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_GS_NKPutawayBy = staff2.GS_Code;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response with error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertNull(response.Job);
				AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_PutawayRequired_PickedBySomeoneElse

		public void TestGetPickByLabelPackage_PutawayRequired_PickedBySomeoneElse_Case()
		{
			TestGetPickByLabelPackage_PutawayRequired_PickedBySomeoneElse(UOMPackTypesList.Codes.Case);
		}

		public void TestGetPickByLabelPackage_PutawayRequired_PickedBySomeoneElse_Pallet()
		{
			TestGetPickByLabelPackage_PutawayRequired_PickedBySomeoneElse(UOMPackTypesList.Codes.Pallet);
		}

		void TestGetPickByLabelPackage_PutawayRequired_PickedBySomeoneElse(string uomTypeForPickByLabel)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff1 = Helper.CreateGlbStaff("A", "A");
			var staff2 = Helper.CreateGlbStaff("B", "B");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff2.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_GS_NKPutawayBy = staff1.GS_Code;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response.Job.Reference);
				AssertEquals("Should be putaway only.", true, response.Job.IsPutawayOnly);
				AssertEquals("Should have no lines.", false, response.Job.Lines.Any());
			});
		}

		#endregion

		#region  TestGetPickByLabelPackage_AddToWhsPickByLabelProcess

		public void TestGetPickByLabelPackage_AddToWhsPickByLabelProcess()
		{
			var uomTypeForPickByLabel = UOMPackTypesList.Codes.Pallet;
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, uomTypeForPickByLabel);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = uomTypeForPickByLabel == UOMPackTypesList.Codes.SplitCase;
			pick.WP_PickCasesByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Case;
			pick.WP_PickPalletsByLabel = uomTypeForPickByLabel == UOMPackTypesList.Codes.Pallet;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();
			var noCondition = new ZQuery();
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Any());
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Any());

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			var job = Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Single();
			var label = Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Single();
			CombineAssertions(() =>
			{
				AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response.Job.Reference);
				AssertEquals("WhsPickByLabelJob WTK_GS_NKAssignedTo.", staff.GS_Code, job.WTK_GS_NKAssignedTo);
				AssertEquals("WhsPickByLabelJob WTK_WW_Warehouse.", data.Whs1.PK, job.WTK_WW_Warehouse);
				AssertEquals("WhsPickByLabelJob WTK_WL_DockDoor.", pick.WP_WL_DockDoor, job.WTK_WL_DockDoor);
				AssertEquals("WhsPickByLabelLabel WTL_KP_Package.", package1.PK, label.WTL_KP_Package);
			});
		}

		#endregion

		#region TestGetPickByLabelPackage_AddWhsPickByLabelJobOnce

		public void TestGetPickByLabelPackage_AddWhsPickByLabelJobOnce()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			pickLines[0].WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLines[1].WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			Helper.Factory.Save();
			var noCondition = new ZQuery();
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Any());
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Any());

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			var job = Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Single();
			var label = Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Single();

			response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertNull("Call successfully.", response.ErrorMessage);

			AssertEquals(job.PK, Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Single().PK);
			AssertEquals(label.PK, Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Single().PK);

			response = webService.GetPickByLabelPackage("PACKAGE-2", false);

			AssertEquals("Should not create new job.", job.PK, Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Single().PK);
			AssertEquals("Still previous label exists.", label.PK, Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Where(l => l.PK.Equals(label.PK)).Single().PK);
			var label2 = Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Where(l => !l.PK.Equals(label.PK)).Single();
		}

		#endregion

		#region TestGetPickByLabelPackage_DockDoorAssignment

		public void TestGetPickByLabelPackage_DockDoorAssignment()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_PickPalletsByLabel = true;

			var pickLine1 = pick1.GetAllPickLines().Single();
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_PickPalletsByLabel = true;

			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package2 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var noCondition = new ZQuery();
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Any());
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Any());
			AssertEquals("Precondition", false, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentTableCode, WhsPickByLabelLabelSchema.Constants.Prefix)).Any());

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-2", false);
			AssertSuccessfulResponse(response2, webService2);

			var dockDoorAssignments = webService2.Factory.Load<WhsDockDoorAssignment>(noCondition);
			AssertEquals("Only 1 DDA created", 1, dockDoorAssignments.Length);

			var dda = dockDoorAssignments[0];
			AssertEquals("DDA correct DDL", dda.WDA_WL_AssignedDockDoor, data.Whs1.WW_DefaultOutboundDockDoor);
			AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

			AssertEquals("Pick1 DDA set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);
		}

		public void TestGetPickByLabelPackage_DockDoorAssignment_Error()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_PickPalletsByLabel = true;

			var pickLine1 = pick1.GetAllPickLines().Single();
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			Helper.Factory.Save();

			var ddaService = new Mock<IWhsPickDockDoorAssignmentService>();
			ddaService.Setup(m =>
				m.GeneratePickDockDoorAssignment(
					It.IsAny<ZGuid>(),
					It.Is<DockDoorAssignmentLinkType>(t => t == DockDoorAssignmentLinkType.PickByLabel),
					It.IsAny<ZGuid>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns("Error");

			using (ObjectFactory.Substitute(ddaService.Object))
			{
				var webService1 = GetNewWebService(data.Whs1, staff);
				var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Error", response1.ErrorMessage);
			}
		}

		#endregion

		#region TestGetPickByLabelPackage_AddLabelForDifferentDDL

		public void TestGetPickByLabelPackage_AddLabelForDifferentDDL()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var otherDDL = data.Whs1.FindLocation("A-1");
			otherDDL.WLV_WLT_LocationType = ddlLocationType.PK;

			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			var job = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);

			pick.WP_WL_DockDoor = otherDDL.PK;
			Helper.Factory.Save();
			response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertNull("When we have an error the job should be empty.", response.Job);
			AssertEquals("Should allow user to scan label which are going to different dock door location.", "Error - WTL_KP_Package: Package 'PACKAGE-1' is assigned to a different dock door location than previous labels.", response.ErrorMessage);
		}

		#endregion

		#region TestGetPickByLabelPackage_AddLabelForDifferentDDL

		public void TestGetPickByLabelPackage_GetDDLFromLabel()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var otherDDL = data.Whs1.FindLocation("A-1");
			otherDDL.WLV_WLT_LocationType = ddlLocationType.PK;

			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			pick.WP_WL_DockDoor = otherDDL.PK;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			AssertNotEquals("Precondition.", "A-1", data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString); // "DOCKDOOR"
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			Helper.Factory.Save();
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertEquals("Dock door location in job should match with pick dock door location ('A-1') **not** default warehouse dock door location ('DOCKDOOR').", "A-1", response.Job.DockDoorLocation);
		}

		#endregion

		#region TestGetPickByLabelPackage_PutawayOnly_FactoryConcurrencySaveError

		public void TestGetPickByLabelPackage_PutawayOnly_FactoryConcurrencySaveError()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part2.PartUnits.RemoveAndDeleteAll();
			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = false;
			pick.WP_PickCasesByLabel = false;
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLine).Row, TestConnection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has modified the job. Please restart the operation and try again.", response.ErrorMessage);
		}

		#endregion

		#region TestGetPickByLabelPackage_FactoryConcurrencySaveError

		public void TestGetPickByLabelPackage_FactoryConcurrencySaveError()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var staff1 = Helper.CreateGlbStaff("1", "1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			AssertEquals("Precondition.", 3, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pick.GetAllPickLines().All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package1, pickLines[1]);
			packingHelper.CreatePackageDivot(package1, pickLines[2]);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLines[0]).Row, TestConnection);
			webService1.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has been assigned to or modified this Job. Please restart the operation and try again.", response.ErrorMessage);
		}

		#endregion

		#region TestPackageIsPickedAndPutaway

		public void TestPackageIsPickedAndPutaway()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNull("Precondition", response1.ErrorMessage);

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var dockDoorTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			dockDoorTransferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(dockDoorTransferLine);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Label 'PACKAGE-1' is already picked and putaway.", response2.ErrorMessage);

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.GetPickByLabelPackage("PACKAGE-2", false);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals("Label 'PACKAGE-2' cannot be found or assigned to someone else.", response3.ErrorMessage);
		}

		#endregion

		#region TestSupportUserShouldBeAbleToPickByLabel

		public void TestSupportUserShouldBeAbleToPickByLabel()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.SupportUserName));

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Globals.IsUserInteractive = false;
				var webService = GetNewWebService(data.Whs1);
				var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
				AssertEquals("Expecting job with correct ID.", "PACKAGE-1", response.Job.Reference);

				var job = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
				var label = Helper.Factory.Load<WhsPickByLabelLabel>(new ZQuery(WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob, job.PK)).Single();
				AssertEquals("Should not change.", false, Globals.IsUserInteractive);
			}
		}

		#endregion

		#region TestGetPickByLabelPackage_CancelledPickByLabel

		public void TestGetPickByLabelPackage_CancelledPickByLabel_AddedToOldJob()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			// start the pick, creating the task
			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNull("Precondition: successful web service request.", response1.ErrorMessage);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-2", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertNull("Precondition: successful web service request.", response2.ErrorMessage);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			AssertEquals("Pick by label job has 2 labels.", 2, pickByLabelJob.Labels.Count);
			var label1 = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-1");
			var label2 = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-2");

			AssertEquals("Precondition: pick line1 is picking.", true, pickLine1.WZ_IsPicking);
			AssertEquals("Precondition: pick line2 is picking.", true, pickLine2.WZ_IsPicking);

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response3, webService3);
			AssertNull("Precondition: successful web service request.", response3.ErrorMessage);

			AssertEquals("Only 1 label is left on the job and it's 'PACKAGE-2'.", "PACKAGE-2", pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.KP_PackageID);
			AssertEquals("WhsPickByLabelLabel1 is deleted.", true, label1.IsDeleted);
			AssertEquals("Pick line1 is not picking.", false, pickLine1.WZ_IsPicking);

			// pick 'PACKAGE-1' again after cancelling
			var webService4 = GetNewWebService(data.Whs1, staff);
			var response4 = webService4.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response4, webService4);
			AssertNull("Precondition: successful web service request.", response4.ErrorMessage);

			var newFactory = new BusinessObjectFactory();
			var pickByLabelJobInAnotherFactory = newFactory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			AssertEquals("Pick by label job has 2 labels again.", 2, pickByLabelJobInAnotherFactory.Labels.Count);
			var newLabel1 = pickByLabelJobInAnotherFactory.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-1");
			AssertEquals("Pick line1 is picking again.", true, pickLine1.WZ_IsPicking);
		}

		public void TestGetPickByLabelPackage_CancelledPickByLabel_AddedToNewJob()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff1 = Helper.CreateGlbStaff("A", "A");
			var staff2 = Helper.CreateGlbStaff("B", "B");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			// start the pick, creating the task
			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNull("Precondition: successful web service request.", response1.ErrorMessage);

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var response2 = webService2.GetPickByLabelPackage("PACKAGE-2", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertNull("Precondition: successful web service request.", response2.ErrorMessage);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff1.GS_Code)).Single();
			AssertEquals("Pick by label job has 2 labels.", 2, pickByLabelJob.Labels.Count);
			var label1 = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-1");
			var label2 = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-2");

			AssertEquals("Precondition: pick line1 is picking.", true, pickLine1.WZ_IsPicking);
			AssertEquals("Precondition: pick line2 is picking.", true, pickLine2.WZ_IsPicking);

			var webService3 = GetNewWebService(data.Whs1, staff1);
			var response3 = webService3.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response3, webService3);
			AssertNull("Precondition: successful web service request.", response3.ErrorMessage);

			AssertEquals("Only 1 label is left on the job and it's 'PACKAGE-2'.", "PACKAGE-2", pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.KP_PackageID);
			AssertEquals("WhsPickByLabelLabel1 is deleted.", true, label1.IsDeleted);
			AssertEquals("Pick line1 is not picking.", false, pickLine1.WZ_IsPicking);

			// pick 'PACKAGE-1' again after cancelling
			var webService4 = GetNewWebService(data.Whs1, staff2);
			var response4 = webService4.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response4, webService4);
			AssertNull("Precondition: successful web servive request.", response4.ErrorMessage);

			AssertEquals("Original Pick by label job still has 1 label.", 1, pickByLabelJob.Labels.Count);
			var newLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff2.GS_Code)).Single();
			var newLabel1 = newLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-1");
			AssertEquals("Pick line1 is picking again.", true, pickLine1.WZ_IsPicking);
			AssertEquals("Pick line1 is picking again.", "B", pickLine1.WZ_GS_NKAssignedTo);
		}

		#endregion

		#region TestIsPartiallyPicked

		public void TestGetPickByLabelPackage_IsPartiallyPicked()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);

			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition: pick line is picked.", true, pickLine2.IsPicked);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Job only has 1 line to pick.", 1, response.Job.Lines.Count);
			AssertEquals("Is partially picked is true.", true, response.Job.HasStartedPicking);
		}

		#endregion

		#endregion

		#region TestGetPickByLabelActiveJob

		#region TestGetPickByLabelActiveJob_Empty

		public void TestGetPickByLabelActiveJob_Empty()
		{
			TestGetPickByLabelActiveJob_EmptyCore();
		}

		void TestGetPickByLabelActiveJob_EmptyCore()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.Factory.Save();

			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertEquals("Should response without error and empty job pk.", Guid.Empty, response.JobPK);
			AssertEquals("Should response without error and no labels.", null, response.Labels);
			AssertEquals("Should response without error and no dock door location.", null, response.DockDoorLocationString);
			AssertEquals("Should response without error and no dock door location.", null, response.DockDoorLocationString_UserFriendly);
			AssertEquals("Should response without error and not allowed.", false, response.AllowPickDockDoorLocationOverride);
			AssertEquals("Should response without error and false result.", false, response.IsUsingDirectedPackingConsolidation);
			AssertEquals("Should response without error and false result.", false, response.IsPackingStationAllowed);
		}

		#endregion

		#region TestGetPickByLabelActiveJob

		public void TestGetPickByLabelActiveJob_HasPackingStation() => TestGetPickByLabelActiveJobCore(hasPackingStation: true, allowOverride: false);
		public void TestGetPickByLabelActiveJob_HasPackingStation_AllowDDLOverride() => TestGetPickByLabelActiveJobCore(hasPackingStation: true, allowOverride: true);

		public void TestGetPickByLabelActiveJob_NoPackingStation() => TestGetPickByLabelActiveJobCore(hasPackingStation: false, allowOverride: false);
		public void TestGetPickByLabelActiveJob_NoPackingStation_AllowDDLOverride() => TestGetPickByLabelActiveJobCore(hasPackingStation: false, allowOverride: true);

		void TestGetPickByLabelActiveJobCore(bool hasPackingStation, bool allowOverride)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");

			if (hasPackingStation)
			{
				var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
				var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
				var packingLocation = newRow.Locations[0];
				packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			}

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = allowOverride;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));
			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response1 = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);
			AssertSuccessfulResponse(response1, webService);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_OnePackage = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_OnePackage, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response_OnePackage.Error);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_OnePackage.JobPK);
			AssertEquals("Should have one label in list and match with package ID.", "PACKAGE-1", response_OnePackage.Labels.Single().PackageID);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response_OnePackage.DockDoorLocationString);
			AssertEquals("AllowPickDockDoorLocationOverride should be correct.", allowOverride, response_OnePackage.AllowPickDockDoorLocationOverride);
			AssertEquals("Order1 does not enable DirectedPackingConsolidation.", false, response_OnePackage.IsUsingDirectedPackingConsolidation);
			AssertEquals("IsPackingStationAllowed", hasPackingStation, response_OnePackage.IsPackingStationAllowed);

			var package2 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			Helper.Factory.Save();

			var response2 = webService.GetPickByLabelPackage("PACKAGE-2", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response2.Error);
			AssertSuccessfulResponse(response2, webService);

			var response_TwoPackages = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_TwoPackages, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response_TwoPackages.Error);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_TwoPackages.JobPK);
			AssertContainsExactElementsInAnyOrder("Should have one label in list and match with package ID.", new[] { "PACKAGE-1", "PACKAGE-2" }, response_TwoPackages.Labels.Select(l => l.PackageID).ToArray());
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response_TwoPackages.DockDoorLocationString);
			AssertEquals("AllowPickDockDoorLocationOverride should be correct.", allowOverride, response_TwoPackages.AllowPickDockDoorLocationOverride);
			AssertEquals("Order2 enabled DirectedPackingConsolidation so should be true with both.", true, response_TwoPackages.IsUsingDirectedPackingConsolidation);
			AssertEquals("IsPackingStationAllowed", hasPackingStation, response_TwoPackages.IsPackingStationAllowed);
		}

		public void TestGetPickByLabelActiveJob_PickWithLooseInventory()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.WD_UseDirectedPackingConsolidation = true;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));
			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response1 = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);
			AssertSuccessfulResponse(response1, webService);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_OnePackage = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_OnePackage, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response_OnePackage.Error);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_OnePackage.JobPK);
			AssertEquals("Should have one label in list and match with package ID.", "PACKAGE-1", response_OnePackage.Labels.Single().PackageID);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response_OnePackage.DockDoorLocationString);
			AssertEquals("Pick has loose inventory.", false, response_OnePackage.IsUsingDirectedPackingConsolidation);
		}

		public void TestGetPickByLabelActiveJob_PickByBOM()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[1].PickLines[0]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, webService);

			var response_GetPick = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_GetPick, webService);
			AssertEquals("Pick by BOM can't use Pick by Label in the first place, so the code doesn't check it at all.", true, response_GetPick.IsPackingStationAllowed);
		}

		public void TestGetPickByLabelActiveJob_DockDoorLocation_UserFriendly()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			warehouse.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 5, 4, 3);
			Helper.Factory.Save();

			var dockdoorLocationType = Helper.Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			var dockdoorLocation = warehouse.FindLocation("Z030201");
			dockdoorLocation.WLV_LocationStatus = "NOR";
			dockdoorLocation.WLV_WLT_LocationType = dockdoorLocationType.PK;
			Helper.Factory.Save();

			warehouse.WW_DefaultInboundDockDoor = dockdoorLocation.PK;
			warehouse.WW_DefaultOutboundDockDoor = dockdoorLocation.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, "O2", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));
			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, order1.Lines[0].PickLines[0]);
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse, staff);
			var response1 = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);
			AssertSuccessfulResponse(response1, webService);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response.JobPK);
			AssertEquals("Should have one label in list and match with package ID.", "PACKAGE-1", response.Labels.Single().PackageID);
			AssertEquals("Dock door location in job should match with pick dock door location.", "Z030201", response.DockDoorLocationString);
			AssertEquals("Dock door location in job should match with pick dock door location.", "Z-03-02-01", response.DockDoorLocationString_UserFriendly);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_OpenJobWithNoLabels

		public void TestGetPickByLabelActiveJob_OpenJobWithNoLabels()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			pickLines[0].WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			var noCondition = new ZQuery();
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Any());
			AssertEquals("Precondition", false, Helper.Factory.Load<WhsPickByLabelLabel>(noCondition).Any());

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Expecting non null job.", response1.Job);

			pickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			package1.Delete();
			var job = Helper.Factory.Load<WhsPickByLabelJob>(noCondition).Single();
			AssertEquals("Precondition.", 0, job.Labels.Count);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response2, webService1);
			AssertEquals("Should return job pk.", job.PK, response2.JobPK);
			AssertEquals("Should have no labels.", 0, response2.Labels.Length);
			AssertEquals("Should have no assigned location.", string.Empty, response2.AssignedPutawayLocation);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_DifferentUser

		public void TestGetPickByLabelActiveJob_DifferentUser()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff1 = Helper.CreateGlbStaff("Bob", "Bob");
			var staff2 = Helper.CreateGlbStaff("Mat", "Mat");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));
			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, order.Lines[0].PickLines[0]);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response1.Error);
			AssertSuccessfulResponse(response1, webService1);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_ForBob = webService1.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_ForBob, webService1);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_ForBob.JobPK);

			var webService2 = GetNewWebService(data.Whs1, staff2);
			var response_Mat = webService2.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_Mat, webService2);
			AssertEquals("Bob has active job,Mat should not get Bob's active jobs.", Guid.Empty, response_Mat.JobPK);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_DifferentWarehouse

		public void TestGetPickByLabelActiveJob_DifferentWarehouse()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("Bob", "Bob");
			var whs2 = Helper.CreateWarehouse("2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));
			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines[0]);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response = webService1.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, webService1);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_ForStuff1 = webService1.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_ForStuff1, webService1);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_ForStuff1.JobPK);

			var webService2 = GetNewWebService(whs2, staff);
			var response_Whs2 = webService2.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_Whs2, webService2);
			AssertEquals("Should not return active job for different warehouse.", Guid.Empty, response_Whs2.JobPK);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_OnlyReturnActiveJob

		public void TestGetPickByLabelActiveJob_OnlyReturnActiveJob()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var staff = Helper.CreateGlbStaff("Bob", "Bob");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var picklines = pick.GetAllPickLines();
			AssertEquals("Precondition.", true, picklines.All(pl => pl.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));
			AssertNull("Precondition.", new BusinessObjectFactory().LoadTop1<WhsPickByLabelJob>(new ZQuery()));

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, order.Lines[0].PickLines[0]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, webService);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_GetJob = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_GetJob, webService);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_GetJob.JobPK);

			picklines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Helper.Factory.Save();

			pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
			pickByLabelJob.Factory.Save();
			AssertEquals("Precondition: Should have finalised the pick by label job.", false, pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			response_GetJob = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_GetJob, webService);
			AssertEquals("There is not active job (is closed).", Guid.Empty, response_GetJob.JobPK);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_PutawayOnly

		public void TestGetPickByLabelActiveJob_PutawayOnly()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine);
			Helper.Factory.Save();

			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_OnePackage = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_OnePackage, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response_OnePackage.Error);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_OnePackage.JobPK);
			AssertEquals("Should have one label in list and match with package ID.", "PACKAGE-1", response_OnePackage.Labels.Single().PackageID);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response_OnePackage.DockDoorLocationString);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_AllowPickDockDoorLocationOverride_SomeStockInDDL

		public void TestGetPickByLabelActiveJob_AllowPickDockDoorLocationOverride_SomeStockInDDL()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package1, pickLine2);
			Helper.Factory.Save();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Expecting non null job.", response.Job);

			var pickByLabelJob = new BusinessObjectFactory().Load<WhsPickByLabelJob>(new ZQuery()).Single();
			var response_OnePackage = webService.GetPickByLabelActiveJob();
			AssertSuccessfulResponse(response_OnePackage, webService);
			AssertEquals("Expecting response without error.", ErrorTypes.None, response_OnePackage.Error);
			AssertEquals("Should return job pk.", pickByLabelJob.PK, response_OnePackage.JobPK);
			AssertEquals("Should have one label in list and match with package ID.", "PACKAGE-1", response_OnePackage.Labels.Single().PackageID);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response_OnePackage.DockDoorLocationString);
			AssertEquals("AllowPickDockDoorLocationOverride should be false since pkg2 in DDL.", false, response_OnePackage.AllowPickDockDoorLocationOverride);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_AlreadyScaned

		public void TestGetPickByLabelActiveJob_AlreadyScaned_NotPutawayOnly_ActiveJob()
		{
			TestGetPickByLabelActiveJob_AlreadyScanedCore(putawayOnly: false, pickByLabelJobIsClosed: false, hasError: false);
		}
		public void TestGetPickByLabelActiveJob_AlreadyScaned_NotPutawayOnly_NotActiveJob()
		{
			TestGetPickByLabelActiveJob_AlreadyScanedCore(putawayOnly: false, pickByLabelJobIsClosed: true, hasError: true);
		}

		public void TestGetPickByLabelActiveJob_AlreadyScaned_PutawayOnly_ActiveJob()
		{
			TestGetPickByLabelActiveJob_AlreadyScanedCore(putawayOnly: true, pickByLabelJobIsClosed: false, hasError: false);
		}

		public void TestGetPickByLabelActiveJob_AlreadyScaned_PutawayOnly_NotActiveJob()
		{
			TestGetPickByLabelActiveJob_AlreadyScanedCore(putawayOnly: true, pickByLabelJobIsClosed: true, hasError: true);
		}

		void TestGetPickByLabelActiveJob_AlreadyScanedCore(bool putawayOnly, bool pickByLabelJobIsClosed, bool hasError)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine = pickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			if (putawayOnly)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}

			var job = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, staff.GS_Code, package.PK);
			if (pickByLabelJobIsClosed)
			{
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
				WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(job);
				AssertEquals("Should have finalised the pick by label job.", false, job.WTK_FinalisedDate.IsEmpty);
			}
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetPickByLabelPackage("PACKAGE-1", false);
			AssertSuccessfulResponse(response, webService);
			if (hasError)
			{
				AssertNull("When we have an error the job should be empty.", response.Job);
				AssertEquals("When package is already scanned.", "Package has been scanned before, cannot scan again.", response.ErrorMessage);
			}
			else
			{
				AssertNotNull("Expecting non null job.", response.Job);
				AssertNull(response.ErrorMessage);
			}
		}

		#endregion

		#region TestGetPickByLabelActiveJob_PartiallyFinished

		[TestDate(2020, 6, 11, 14, 54, 0)]
		public void TestGetPickByLabelActiveJob_PartiallyFinished()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 5m);

			var staff = Helper.CreateGlbStaff("A", "A");
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1, order2, order3);
			pick1.WP_PickPalletsByLabel = true;

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order4);
			pick2.WP_PickPalletsByLabel = true;

			Helper.Factory.Save();

			pick1.AllocatePackageLabels();
			var pkgPackage_NotPicked = order1.PackageJob.Packages.Single();
			var pkgPackage_Picked = order2.PackageJob.Packages.Single();
			var pkgPackage_Putaway = order3.PackageJob.Packages.Single();

			pick2.AllocatePackageLabels();
			var pkgPackage_Finalised = order4.PackageJob.Packages.Single();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_NotPicked.PK);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Picked.PK);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Putaway.PK);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Finalised.PK);
			Helper.Factory.Save();

			// pick labels
			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var pickLine3 = order3.Lines.Single().PickLines.Single();
			var pickLine4 = order4.Lines.Single().PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);

			// putaway labels
			transferLine3.FinaliseDocketLine();
			transferLine4.FinaliseDocketLine();
			pick2.FinaliseAllOrders();
			order4.Lines.Single().PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			order4.WD_DocketStatus = WhsOrderStatus.Codes.Departed;
			pick2.WP_PickStatus = PickStatus.Codes.Finalised; // hack to bypass normal split of PBL jobs
			pick2.WP_FinalizedDateUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(pick2);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickByLabelJob.WTK_FinalisedDate);
			AssertEquals("Precondition", 4, pickByLabelJob.Labels.Count);

			var service = GetNewWebService(data.Whs1, staff);
			var response = service.GetPickByLabelActiveJob();
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, service);
			AssertNotEquals("Should return new Job pk.", pickByLabelJob.PK, response.JobPK);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick1.DockDoorLocation.ToLocationString(), response.DockDoorLocationString);
			AssertEquals("Assigned Putaway location in job should match with pick dock door location.", pick1.DockDoorLocation.WLV_LocationString, response.AssignedPutawayLocation);
			AssertEquals("Assigned Putaway location in job should match with pick dock door location.", pick1.DockDoorLocation.WLV_LocationString_UserFriendly, response.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Assigned Putaway location in job should match with pick dock door location.", pick1.DockDoorLocation.WLV_LocationClass, response.AssignedPutawayLocationClass);

			AssertEquals("Should have not putaway labels in the list.", 2, response.Labels.Length);
			response.Labels.Single(p => p.PackageID == pkgPackage_NotPicked.KP_PackageID);
			response.Labels.Single(p => p.PackageID == pkgPackage_Picked.KP_PackageID);

			var otherFactory = new BusinessObjectFactory();
			var existingPBLJobInOtherFactory = otherFactory.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("Should have been finalised.", ZDateTimeOffset.Now, existingPBLJobInOtherFactory.WTK_FinalisedDate);
			AssertEquals("Should have picked packages attached.", 2, existingPBLJobInOtherFactory.Labels.Count);
			existingPBLJobInOtherFactory.Labels.Cast<WhsPickByLabelLabel>().Single(l => l.WTL_KP_Package == pkgPackage_Putaway.PK);
			existingPBLJobInOtherFactory.Labels.Cast<WhsPickByLabelLabel>().Single(l => l.WTL_KP_Package == pkgPackage_Finalised.PK);

			var newPBLJobInOtherFactory = otherFactory.Load<WhsPickByLabelJob>(response.JobPK);
			AssertEquals("Should not be finalised.", ZDateTimeOffset.Empty, newPBLJobInOtherFactory.WTK_FinalisedDate);
			AssertEquals("Should have not picked packages attached.", 2, newPBLJobInOtherFactory.Labels.Count);
			newPBLJobInOtherFactory.Labels.Cast<WhsPickByLabelLabel>().Single(l => l.WTL_KP_Package == pkgPackage_NotPicked.PK);
			newPBLJobInOtherFactory.Labels.Cast<WhsPickByLabelLabel>().Single(l => l.WTL_KP_Package == pkgPackage_Picked.PK);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_AssignedPutaway

		[TestDate(2020, 6, 11, 14, 54, 0)]
		public void TestGetPickByLabelActiveJob_AssignedPutaway_ToDockDoor()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 5m);

			var staff = Helper.CreateGlbStaff("A", "A");
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			Helper.Factory.Save();

			pick.AllocatePackageLabels();
			var pkgPackage_Picked = order1.PackageJob.Packages.Single();
			var pkgPackage_Putaway = order2.PackageJob.Packages.Single();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Picked.PK);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Putaway.PK);
			Helper.Factory.Save();

			// pick labels
			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var pickLine2 = order2.Lines.Single().PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			// putaway labels
			transferLine2.FinaliseDocketLine();
			Helper.Factory.Save();
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickByLabelJob.WTK_FinalisedDate);
			AssertEquals("Precondition", 2, pickByLabelJob.Labels.Count);

			var service = GetNewWebService(data.Whs1, staff);
			var response = service.GetPickByLabelActiveJob();
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, service);
			AssertNotEquals("Should return new Job pk.", pickByLabelJob.PK, response.JobPK);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response.DockDoorLocationString);
			AssertEquals("Packing Station in job should be empty.", Guid.Empty, response.PackingStationPK);
			AssertEquals("Assigned Putaway location in job should match with pick dock door location.", pick.DockDoorLocation.WLV_LocationString, response.AssignedPutawayLocation);
			AssertEquals("Assigned Putaway location in job should match with pick dock door location.", pick.DockDoorLocation.WLV_LocationString_UserFriendly, response.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Assigned Putaway location in job should match with pick dock door location.", pick.DockDoorLocation.WLV_LocationClass, response.AssignedPutawayLocationClass);
		}

		[TestDate(2020, 6, 11, 14, 54, 0)]
		public void TestGetPickByLabelActiveJob_AssignedPutaway_ToPackingStation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 5m);

			var staff = Helper.CreateGlbStaff("A", "A");
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			Helper.Factory.Save();

			pick.AllocatePackageLabels();
			var pkgPackage_Picked = order1.PackageJob.Packages.Single();
			var pkgPackage_Putaway = order2.PackageJob.Packages.Single();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Picked.PK);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Putaway.PK);
			Helper.Factory.Save();

			// pick labels
			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var pickLine2 = order2.Lines.Single().PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingStationLocation.PK;
			pick.WP_WL_PackingStation = packingStationLocation.PK;

			// putaway labels
			transferLine2.FinaliseDocketLine();
			Helper.Factory.Save();
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickByLabelJob.WTK_FinalisedDate);
			AssertEquals("Precondition", 2, pickByLabelJob.Labels.Count);

			var service = GetNewWebService(data.Whs1, staff);
			var response = service.GetPickByLabelActiveJob();
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, service);
			AssertNotEquals("Should return new Job pk.", pickByLabelJob.PK, response.JobPK);
			AssertEquals("Dock door location in job should match with pick dock door location.", pick.DockDoorLocation.ToLocationString(), response.DockDoorLocationString);
			AssertEquals("Packing Station in job should match with pick packing station location.", packingStationLocation.PK.ToGuid(), response.PackingStationPK);
			AssertEquals("Assigned Putaway location in job should match with pick packing station location.", packingStationLocation.WLV_LocationString, response.AssignedPutawayLocation);
			AssertEquals("Assigned Putaway location in job should match with pick packing station location.", packingStationLocation.WLV_LocationString_UserFriendly, response.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Assigned Putaway location in job should match with pick packing station location.", packingStationLocation.WLV_LocationClass, response.AssignedPutawayLocationClass);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_FullyFinished_NotFinalised

		[TestDate(2020, 6, 11, 14, 54, 0)]
		public void TestGetPickByLabelActiveJob_FullyFinished_NotFinalised()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 5m);

			var staff = Helper.CreateGlbStaff("A", "A");
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			Helper.Factory.Save();

			pick.AllocatePackageLabels();
			var pkgPackage_Finalised = order.PackageJob.Packages.Single();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Finalised.PK);

			Helper.PickAndMakeInTransitTransfer(order.Lines.Single().PickLines.Single(), ZDateTimeOffset.Now);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickByLabelJob.WTK_FinalisedDate);
			AssertEquals("Precondition", 1, pickByLabelJob.Labels.Count);

			var service = GetNewWebService(data.Whs1, staff);
			var response = service.GetPickByLabelActiveJob();
			AssertEquals("Expecting response without error.", ErrorTypes.None, response.Error);
			AssertSuccessfulResponse(response, service);
			AssertNotEquals("Should return new Job pk.", pickByLabelJob.PK, response.JobPK);
			AssertEquals("Job should have no DDL as it is empty.", null, response.DockDoorLocationString);
			AssertEquals("Should have no label in the list.", null, response.Labels);

			var otherFactory = new BusinessObjectFactory();
			var existingPBLJobInOtherFactory = otherFactory.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("Should have been finalised.", ZDateTimeOffset.Now, existingPBLJobInOtherFactory.WTK_FinalisedDate);
			AssertEquals("Should have putaway package attached.", 1, existingPBLJobInOtherFactory.Labels.Count);
			existingPBLJobInOtherFactory.Labels.Cast<WhsPickByLabelLabel>().Single(l => l.WTL_KP_Package == pkgPackage_Finalised.PK);
		}

		#endregion

		#region TestGetPickByLabelActiveJob_FactoryConcurrencySaveError

		public void TestGetPickByLabelActiveJob_FactoryConcurrencySaveError()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 5m);

			var staff = Helper.CreateGlbStaff("A", "A");
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			Helper.Factory.Save();

			pick.AllocatePackageLabels();
			var pkgPackage_Picked = order1.PackageJob.Packages.Single();
			var pkgPackage_Putaway = order2.PackageJob.Packages.Single();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Picked.PK);
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, pkgPackage_Putaway.PK);
			Helper.Factory.Save();

			// pick labels
			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var pickLine2 = order2.Lines.Single().PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			// putaway labels
			transferLine2.FinaliseDocketLine();
			Helper.Factory.Save();
			AssertEquals("Precondition", ZDateTimeOffset.Empty, pickByLabelJob.WTK_FinalisedDate);
			AssertEquals("Precondition", 2, pickByLabelJob.Labels.Count);

			var service = GetNewWebService(data.Whs1, staff);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLine1).Row, TestConnection);
			service.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = service.GetPickByLabelActiveJob();
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has modified the Job. Please restart the operation and try again.", response.ErrorMessage);
		}

		#endregion

		#endregion
	}
}
