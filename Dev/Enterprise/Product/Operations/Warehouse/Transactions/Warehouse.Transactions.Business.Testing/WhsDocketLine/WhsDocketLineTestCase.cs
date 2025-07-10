using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using WhsPickLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsPickLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLineTestCase<TDocketLine, TDocket> : WhsBusinessObjectTestCase
		where TDocketLine : WhsDocketLine
		where TDocket : WhsDocket
	{
		#region Schema

		public virtual void TestSchema()
		{
			AssertEquals("WE_PackQuantity", WhsDocketLine.Schema.WE_PackQuantity);
		}

		#endregion

		#region Type Decider

		public void TestTypeDecider()
		{
			AssertEquals(typeof(TDocket), DocketLine.DocketType);
		}

		#endregion

		#region FactoryLoadingWrongDocketLine

		#region TestFactoryLoadWrongTypeDocketLine

		public void TestFactoryLoadWrongTypeDocketLine()
		{
			var docket = GetNewDocketLineReadyToFinalise();
			var typeList = new[] { typeof(WhsOrderLine), typeof(WhsReceiveLine), typeof(WhsTransferLine), typeof(WhsAdjustmentLine), typeof(WhsWorkOrderLine) };
			Factory.Save();

			//AssertType("Precondition", GetExpectedBusinessObjectType(), docket);
			Assert("Precondition", docket.GetType().IsAssignableFrom(GetExpectedBusinessObjectType()));

			foreach (var t in typeList)
			{
				if (!docket.GetType().IsAssignableFrom(t) && !docket.GetType().IsSubclassOf(t))
				{
					NUnit.Framework.Assert.That(delegate
					{
						Factory.Load(t, docket.PK);
					}, CustomConstraints.InnermostExceptionThrown(typeof(NotSupportedException)));
				}
				else
				{
					AssertNoExceptionThrown("Should not throw exception", () => Factory.Load(t, docket.PK));
				}
			}
		}

		#endregion

		#region TestFactoryLoadWrongTypeDocketLineFlag

		[ExpectNoExceptions]
		public void TestFactoryLoadWrongTypeDocketLineFlag()
		{
			AssertNoExceptionThrown(() => TestFactoryLoadWrongTypeDocketLineSaveFactory(true));
		}

		#endregion

		#region TestFactoryLoadWrongTypeDocketLineFlag_InMemory()

		[ExpectNoExceptions]
		public void TestFactoryLoadWrongTypeDocketLineFlag_InMemory()
		{
			AssertNoExceptionThrown(() => TestFactoryLoadWrongTypeDocketLineSaveFactory(false));
		}

		#endregion

		#region TestFactoryLoadWrongTypeDocketLineSaveFactory

		void TestFactoryLoadWrongTypeDocketLineSaveFactory(bool saveFactory = true)
		{
			var docket = GetNewDocketLineReadyToFinalise();
			var typeList = new Type[] { typeof(WhsOrderLine), typeof(WhsReceiveLine), typeof(WhsTransferLine), typeof(WhsAdjustmentLine), typeof(WhsWorkOrderLine) };
			if (saveFactory)
			{
				Factory.Save();
			}
			//AssertType("Precondition", GetExpectedBusinessObjectType(), docket);
			Assert("Precondition", docket.GetType().IsAssignableFrom(GetExpectedBusinessObjectType()));

			foreach (var t in typeList)
			{
				if (!docket.GetType().IsAssignableFrom(t) && !docket.GetType().IsSubclassOf(t))
				{
					NUnit.Framework.Assert.That(delegate
					{
						Factory.Load(t, docket.PK);
					}, CustomConstraints.InnermostExceptionThrown(typeof(NotSupportedException)));
				}
				else
				{
					AssertNoExceptionThrown("Should not throw exception", () => Factory.Load(t, docket.PK));
				}
			}
		}

		#endregion

		#endregion

		#region TestChangeInventoryHeldCode

		public void TestChangeInventoryHeldCode()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			AssertEquals("Since docket is not finalized it should not change the status.", false, docketLine.ChangeInventoryHeldCode(true));
			AssertEquals(ZString.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);

			docketLine.HeldCodeChangeQuantity = 10m;
			docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertEquals(false, docketLine.ChangeInventoryHeldCode(false));
			AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());

			if (CanHaveInventoryAttached)
			{
				AssertEquals(InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals("No new Docket Lines should be created.", 1, Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK)).Length);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());

				docketLine.HeldCodeToChangeTo = string.Empty;
				AssertEquals("Precondition", ZString.Empty, docketLine.HeldCodeToChangeTo);

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals("No new Docket Lines should be created", 1, Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK)).Length);
				AssertEquals(InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());

				// when changing status partially, then inventory record must be split.
				docketLine.HeldCodeChangeQuantity = 2m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				var inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
				var inventoryInMemory = Factory.Load<WhsInventoryView>(inMemoryQuery);
				AssertEquals("WI_OH_Client should be correct for original and cloned line", true, inventoryInMemory.All(i => i.WI_OH_Client == docketLine.Docket.WD_OH_Client));
				AssertEquals("WI_WW_Whs should be correct for original and cloned line", true, inventoryInMemory.All(i => i.WI_WW_Whs == docketLine.Docket.WD_WW_Whs));
				AssertEquals("WI_HeldCode should be correct for original and cloned line", true, inventoryInMemory.All(i => i.WI_HeldCode == i.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode));

				var allDocketLines = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK));
				AssertEquals("New docket line record should be created.", 2, allDocketLines.Length);
				AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());

				var newDocketLine = allDocketLines.Single(i => i.WE_StockOnHand == 2m);
				AssertEquals(InventoryStatus.Codes.Held, newDocketLine.WE_CurrentInventoryStatus);
				AssertEquals(InventoryHoldCodes.Codes.Damaged, newDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(string.Empty, newDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("New docket line should have finalised status.", "FIN", newDocketLine.WE_DocketLineStatus);
				AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());
			}
			else
			{
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals("Should not have changed hold code", string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestChangeInventoryHeldCode_HoldCodeReason()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);

			docketLine.HeldCodeChangeQuantity = 10m;
			docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			docketLine.HoldReasonToChangeTo = "Alien sighting";

			if (CanHaveInventoryAttached)
			{
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals("No new Docket Lines should be created.", 1, Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK)).Length);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Alien sighting", docketLine.WE_CurrentHoldReason);

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, docketLine.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "Status Changed from 'AVL' to 'HEL'|FDT=2020-01-01T00:00:00|NEW=HEL|RES=Alien sighting|TYP=Hold Code");
				AssertEquals(1, docketLine.Logs.Find(logQuery).Length);
				AssertEquals("Hold Reason should be cleared.", "", docketLine.HoldReasonToChangeTo);
			}
			else
			{
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals("Should not have changed hold code", string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
			}
		}

		public void TestChangeInventoryHeldCode_OriginalHeldCode_PartialHoldCodeChange()
		{
			if (CanHaveInventoryAttached)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var docketLine = GetNewDocketLineReadyToFinalise(data, InventoryHoldCodes.Codes.Held);
				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKCurrentInventoryHeldCode);

				docketLine.HeldCodeToChangeTo = "";
				docketLine.HeldCodeChangeQuantity = 10m;
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(10m, docketLine.WE_TransactionQuantity);
				AssertEquals(10m, docketLine.WE_StockOnHand);
				Factory.Save();

				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.HeldCodeChangeQuantity = 9m;
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(10m, docketLine.WE_TransactionQuantity);
				AssertEquals(1m, docketLine.WE_StockOnHand);

				var newDocketLine = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, docketLine.PK)).Single();
				AssertEquals(InventoryStatus.Codes.Available, newDocketLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Held, newDocketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, newDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Held, newDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(9m, newDocketLine.WE_TransactionQuantity);
				AssertEquals(9m, newDocketLine.WE_StockOnHand);
				AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestChangeInventoryHeldCode_ChangeToLCC_CachesInventoryHeldCode()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);

			docketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			docketLine.HeldCodeChangeQuantity = 10m;
			docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			docketLine.HoldReasonToChangeTo = "Alien sighting";

			if (CanHaveInventoryAttached)
			{
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals("No new Docket Lines should be created.", 1, Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK)).Length);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.LostInCycleCount, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Alien sighting", docketLine.WE_CurrentHoldReason);

				AssertEquals(InventoryHoldCodes.Codes.Damaged, docketLine.PreviousHeldCodeForCycleCount);
			}
			else
			{
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals("Should not have changed hold code", InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestChangeInventoryHeldCode_ChangeToLCCAndSplitInventory_CachesInventoryHeldCode()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);

			docketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			docketLine.HeldCodeChangeQuantity = 5m;
			docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			docketLine.HoldReasonToChangeTo = "Alien sighting";

			if (CanHaveInventoryAttached)
			{
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(string.Empty, docketLine.WE_CurrentHoldReason);

				AssertEquals(string.Empty, docketLine.PreviousHeldCodeForCycleCount);

				var newLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, docketLine.PK));
				AssertEquals(InventoryStatus.Codes.Held, newLine.WE_CurrentInventoryStatus);
				AssertEquals(InventoryHoldCodes.Codes.Damaged, newLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.LostInCycleCount, newLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Alien sighting", newLine.WE_CurrentHoldReason);

				AssertEquals(InventoryHoldCodes.Codes.Damaged, newLine.PreviousHeldCodeForCycleCount);
			}
			else
			{
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals("Should not have changed hold code", InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
			}
		}

		[TestDate(2020, 12, 08, 08, 08, 08)]
		public void TestChangeInventoryHeldCode_UpdatesLocationLastAllocatedOrChangedDateUtc()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);
				AssertEquals("Precondition: inventory is available.", InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Precondition: inventory is available.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				var location = docketLine.Location;
				var utcNow = ZDateTime.UtcNow;
				var oldUtcTime = utcNow.AddDays(-2);
				location.WLV_LastAllocatedOrChangedDateUtc = oldUtcTime;
				AssertNotEquals("Precondition: location's WLV_LastAllocatedOrChangedDateUtc is different from the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, utcNow);

				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				AssertEquals(false, docketLine.ChangeInventoryHeldCode(false));

				AssertEquals("Inventory is not updated.", InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Inventory is not updated.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Location's WLV_LastAllocatedOrChangedDateUtc is not updated to the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, oldUtcTime);

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals("Invnentory is updated.", InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Invnentory is updated.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Invnentory is updated.", InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Location's WLV_LastAllocatedOrChangedDateUtc is updated to the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, utcNow);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 12, 08, 08, 08, 08)]
		public void TestChangeInventoryHeldCode_PartialHoldCodeChange_UpdatesLocationLastAllocatedOrChangedDateUtc()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);
				AssertEquals("Precondition: inventory is available.", InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Precondition: inventory is available.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				var location = docketLine.Location;
				var utcNow = ZDateTime.UtcNow;
				var oldUtcTime = utcNow.AddDays(-2);
				location.WLV_LastAllocatedOrChangedDateUtc = oldUtcTime;
				AssertNotEquals("Precondition: location's WLV_LastAllocatedOrChangedDateUtc is different from the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, utcNow);

				// when changing status partially, then inventory record must be split.
				docketLine.HeldCodeChangeQuantity = 2m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				var allDocketLines = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK));
				AssertEquals("New docket line record should be created.", 2, allDocketLines.Length);

				var newDocketLine = allDocketLines.Single(i => i.WE_StockOnHand == 2m);
				AssertEquals("New docket line should have finalised status.", "FIN", newDocketLine.WE_DocketLineStatus);
				AssertEquals("Both docket lines have the same location.", newDocketLine.WE_WL, docketLine.WE_WL);

				AssertEquals("Invnentory status is held.", InventoryStatus.Codes.Held, newDocketLine.WE_CurrentInventoryStatus);
				AssertEquals("Invnentory status is held.", string.Empty, newDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Invnentory status is held.", InventoryHoldCodes.Codes.Damaged, newDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Location's WLV_LastAllocatedOrChangedDateUtc is updated to the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, utcNow);

				AssertNoExceptionThrown("Inventory sync trigger should not fail", () => Factory.Save());
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 12, 08, 08, 08, 08)]
		public void TestChangeInventoryHeldCode_DoesNotUpdateLocationLastAllocatedOrChangedDateUtcIfInventoryStatusUnchanged()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);
				AssertEquals("Precondition: inventory is available.", InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Precondition: inventory is available.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				var location = docketLine.Location;
				var utcNow = ZDateTime.UtcNow;
				var oldUtcTime = utcNow.AddDays(-2);
				location.WLV_LastAllocatedOrChangedDateUtc = oldUtcTime;
				AssertNotEquals("Precondition: location's WLV_LastAllocatedOrChangedDateUtc is different from the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, utcNow);

				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				AssertEquals("Inventory is updated.", InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Inventory is updated.", string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Inventory is updated.", InventoryHoldCodes.Codes.Held, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Location's WLV_LastAllocatedOrChangedDateUtc is updated to the current utc time.", location.WLV_LastAllocatedOrChangedDateUtc, utcNow);

				location.WLV_LastAllocatedOrChangedDateUtc = oldUtcTime; // reset time for test
				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				AssertEquals("Inventory status is not updated.", InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Inventory is updated.", InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Location's WLV_LastAllocatedOrChangedDateUtc is not updated.", location.WLV_LastAllocatedOrChangedDateUtc, oldUtcTime);

				docketLine.HeldCodeChangeQuantity = docketLine.WE_TransactionQuantity;
				docketLine.HeldCodeToChangeTo = "";
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				AssertEquals("Inventory status is updated.", InventoryStatus.Codes.Available, docketLine.WE_CurrentInventoryStatus);
				AssertEquals("Inventory is updated.", "", docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertNotEquals("Location's WLV_LastAllocatedOrChangedDateUtc is updated.", location.WLV_LastAllocatedOrChangedDateUtc, oldUtcTime);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestCreateWhsInventoryHoldChangeLog

		public void TestCreateWhsInventoryHoldChangeLog()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals("Since docket is not finalized it should not change the status.", false, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals(ZString.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);

				docketLine.HeldCodeChangeQuantity = 10m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.HoldReasonToChangeTo = "Explosion of Sun";

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				var createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery());
				AssertEquals("Expected new Log to be Created", 1, createdLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", docketLine.PK, createdLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("LogVersion Incorrect", (short)(1), createdLogs[0].WHL_LogVersion);
				AssertEquals("Hold Code Incorrect", docketLine.WE_WHC_NKCurrentInventoryHeldCode, createdLogs[0].WHL_WHC_NKCode);
				AssertEquals("Hold Reason Incorrect", docketLine.WE_CurrentHoldReason, createdLogs[0].WHL_Reason);
				AssertNotNull("WHL_EventTime is populated.", createdLogs[0].WHL_EventTime);

				docketLine.HeldCodeChangeQuantity = 10m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				docketLine.HoldReasonToChangeTo = "Explosion of Moon";

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery()).OrderByDescending(log => log.WHL_LogVersion).ToArray();
				AssertEquals("Expected new Log to be Created", 2, createdLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", docketLine.PK, createdLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("LogVersion Incorrect", (short)(2), createdLogs[0].WHL_LogVersion);
				AssertEquals("Hold Code Incorrect", docketLine.WE_WHC_NKCurrentInventoryHeldCode, createdLogs[0].WHL_WHC_NKCode);
				AssertEquals("Hold Reason Incorrect", docketLine.WE_CurrentHoldReason, createdLogs[0].WHL_Reason);
				AssertNotNull("WHL_EventTime is populated.", createdLogs[0].WHL_EventTime);

				docketLine.HeldCodeChangeQuantity = 5m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
				docketLine.HoldReasonToChangeTo = "Explosion of Mars";

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery());
				AssertEquals("Expected new Log to be Created", 3, createdLogs.Length);

				var clonedDocketLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, docketLine.PK));
				var clonedLineLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, clonedDocketLine.PK));

				AssertEquals("Expected new Log to be Created from Line Split", 1, clonedLineLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", clonedDocketLine.PK, clonedLineLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("LogVersion Incorrect", (short)(1), clonedLineLogs[0].WHL_LogVersion);
				AssertEquals("Hold Code Incorrect", clonedDocketLine.WE_WHC_NKCurrentInventoryHeldCode, clonedLineLogs[0].WHL_WHC_NKCode);
				AssertEquals("Hold Reason Incorrect", clonedDocketLine.WE_CurrentHoldReason, clonedLineLogs[0].WHL_Reason);
				AssertNotNull("WHL_EventTime is populated.", clonedLineLogs[0].WHL_EventTime);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCreateWhsInventoryHoldChangeLog_EventTimeIsSameAsStmALogEventTime()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals("Since docket is not finalized it should not change the status.", false, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals(ZString.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);

				docketLine.HeldCodeChangeQuantity = 10m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.HoldReasonToChangeTo = "Just Because";

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				var createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery());
				AssertEquals("Expected new Log to be Created", 1, createdLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", docketLine.PK, createdLogs[0].WHL_WE_ParentDocketLine);
				AssertNotNull("WHL_EventTime is populated.", createdLogs[0].WHL_EventTime);

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, docketLine.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Status Changed from 'AVL' to 'HEL'");

				var stmAlog = docketLine.Logs.Find(logQuery).Single();
				AssertEquals("Created Inventory Hold Change Log and StmALog have the same event time.", createdLogs[0].WHL_EventTime.ToDateTime(), stmAlog.SL_EventTime);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2023, 1, 1, 10, 30, 30)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCreateWhsInventoryHoldChangeLog_EventTimeUsesWarehouseBranch()
		{
			if (CanHaveInventoryAttached)
			{
				var sgBranch = Helper.CreateGlbBranch("ABC");
				sgBranch.GB_RL_NKHomePort = "SGSIN";
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				data.Whs1.WW_GB_RelatedCompanyBranch = sgBranch.PK;
				Factory.Save();

				var docketLine = GetNewDocketLineReadyToFinalise(data);
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals("Since docket is not finalized it should not change the status.", false, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals(ZString.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);

				docketLine.HeldCodeChangeQuantity = 10m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.HoldReasonToChangeTo = "Just Because";

				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				var createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery());
				AssertEquals("Expected new Log to be Created", 1, createdLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", docketLine.PK, createdLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("WHL_EventTime uses warehouse branch offset.", new ZDateTimeOffset(new ZDateTime(2023, 1, 1, 18, 30, 30), new TimeSpan(8, 0, 0)), createdLogs[0].WHL_EventTime);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2023, 1, 1, 10, 30, 30)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCreateWhsInventoryHoldChangeLog_EventTimeUsesWarehouseBranch_MultipleWarehouses()
		{
			if (CanHaveInventoryAttached)
			{
				var sgBranch = Helper.CreateGlbBranch("ABC");
				sgBranch.GB_RL_NKHomePort = "SGSIN";
				var data1 = new TestDataSimpleEnvironment(Factory, 2, 1);
				data1.Whs1.WW_GB_RelatedCompanyBranch = sgBranch.PK;

				var sydBranch = Helper.CreateGlbBranch("SDD");
				sydBranch.GB_RL_NKHomePort = "AUSYD";

				var data2 = new TestDataSimpleEnvironmentWithDifferentWarehouse(Factory, 2, 1, data1);
				Factory.Save();

				var docketLine1 = GetNewDocketLineReadyToFinalise(data1);
				docketLine1.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine1.Docket);

				var docketLine2 = (TDocketLine)DocketHelper.GetNewFinalisableDocketWithOneLine(data2, 10m, externalReference: "X2").Lines[0];
				docketLine2.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine2.Docket);

				docketLine1.HeldCodeChangeQuantity = 10m;
				docketLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine1.HoldReasonToChangeTo = "Just Because";
				docketLine2.HeldCodeChangeQuantity = 10m;
				docketLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine2.HoldReasonToChangeTo = "Just Because";

				AssertEquals(true, docketLine1.ChangeInventoryHeldCode(true));
				AssertEquals(true, docketLine2.ChangeInventoryHeldCode(true));

				var createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery());
				AssertEquals("Expected new Log to be Created", 2, createdLogs.Length);

				var log1 = createdLogs.Single(l => l.WHL_WE_ParentDocketLine == docketLine1.PK);
				var log2 = createdLogs.Single(l => l.WHL_WE_ParentDocketLine == docketLine2.PK);
				AssertEquals("WHL_EventTime uses warehouse branch offset.", new ZDateTimeOffset(new ZDateTime(2023, 1, 1, 18, 30, 30), new TimeSpan(8, 0, 0)), log1.WHL_EventTime);
				AssertEquals("WHL_EventTime uses warehouse branch offset.", new ZDateTimeOffset(new ZDateTime(2023, 1, 1, 20, 30, 30), new TimeSpan(10, 0, 0)), log2.WHL_EventTime);
			}
			else
			{
				Assert(true);
			}
		}

		class TestDataSimpleEnvironmentWithDifferentWarehouse : TestDataSimpleEnvironment
		{
			public TestDataSimpleEnvironmentWithDifferentWarehouse(BusinessObjectFactory factory, short cols, short levels, TestDataSimpleEnvironment data)
				: base(factory, cols, levels, true)
			{
				Whs1 = Helper.CreateWarehouse("2", "B", cols, levels);
				Org1 = data.Org1;
				Part1 = data.Part1;
				Part2 = data.Part2;
			}

			protected override void CreateEnvironment()
			{
			}
		}

		public void TestCreateWhsInventoryHoldChangeLog_LineSplit()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				AssertEquals("Since docket is not finalized it should not change the status.", false, docketLine.ChangeInventoryHeldCode(true));
				AssertEquals(ZString.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine.Docket);

				docketLine.HeldCodeChangeQuantity = 10m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.HoldReasonToChangeTo = "Explosion of Sun";
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));

				docketLine.HeldCodeChangeQuantity = 5m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
				docketLine.HoldReasonToChangeTo = "Explosion of Moon";
				AssertEquals(true, docketLine.ChangeInventoryHeldCode(true));
				var createdLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, docketLine.PK));

				var clonedDocketLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, docketLine.PK));
				var clonedLineLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, clonedDocketLine.PK));

				AssertEquals("Expected original log to be created", 1, createdLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", docketLine.PK, createdLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("LogVersion Incorrect", (short)(1), createdLogs[0].WHL_LogVersion);
				AssertEquals("Hold Code Incorrect", docketLine.WE_WHC_NKCurrentInventoryHeldCode, createdLogs[0].WHL_WHC_NKCode);
				AssertEquals("Hold Reason Incorrect", docketLine.WE_CurrentHoldReason, createdLogs[0].WHL_Reason);

				AssertEquals("Expected new Log to be Created from Line Split", 1, clonedLineLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", clonedDocketLine.PK, clonedLineLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("LogVersion Incorrect", (short)(1), clonedLineLogs[0].WHL_LogVersion);
				AssertEquals("Hold Code Incorrect", clonedDocketLine.WE_WHC_NKCurrentInventoryHeldCode, clonedLineLogs[0].WHL_WHC_NKCode);
				AssertEquals("Hold Reason Incorrect", clonedDocketLine.WE_CurrentHoldReason, clonedLineLogs[0].WHL_Reason);

				clonedDocketLine.HeldCodeChangeQuantity = 5m;
				clonedDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				clonedDocketLine.HoldReasonToChangeTo = "Explosion of Jupiter";
				AssertEquals(true, clonedDocketLine.ChangeInventoryHeldCode(true));

				clonedLineLogs = Factory.Load<WhsInventoryHoldChangeLog>(new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, clonedDocketLine.PK)).OrderByDescending(log => log.WHL_LogVersion).ToArray();

				AssertEquals("Expected new Log to be Created from Line Split", 2, clonedLineLogs.Length);
				AssertEquals("Parent DocketLine Incorrect", clonedDocketLine.PK, clonedLineLogs[0].WHL_WE_ParentDocketLine);
				AssertEquals("LogVersion Incorrect", (short)(2), clonedLineLogs[0].WHL_LogVersion);
				AssertEquals("Hold Code Incorrect", clonedDocketLine.WE_WHC_NKCurrentInventoryHeldCode, clonedLineLogs[0].WHL_WHC_NKCode);
				AssertEquals("Hold Reason Incorrect", clonedDocketLine.WE_CurrentHoldReason, clonedLineLogs[0].WHL_Reason);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Customs Stuff

		public void TestCustomsData()
		{
			TestCustomsDataCore();
		}

		protected virtual void TestCustomsDataCore()
		{
			var docket = GetNewWhsDocket();
			var line = GetNewBusinessObject();
			line.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			line.WE_FinalisedDate = DateTimeOffset.UtcNow;
			AssertNotNull(line.CustomsData);
			AssertEquals(true, line.CustomsData.WB_ParentIDInfo.ReadOnly);
			AssertEquals("WB_ParentID", line.PK, line.CustomsData.WB_ParentID);
			AssertEquals("WB_ParentTableCode", WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode, line.CustomsData.WB_ParentTableCode);

			var docketLine = GetNewBusinessObject(docket);
			var existingAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			existingAttribute.SetParent(docketLine);
			AssertEquals("Loads existing attribute", existingAttribute, docketLine.CustomsData);
			AssertEquals("Existing attribute should be registered editable.", true, docketLine.IsRegisteredEditableChildObject(docketLine.CustomsData));
		}

		public void TestIsBondedTransaction() => TestIsBondedTransactionCore();

		protected virtual void TestIsBondedTransactionCore()
		{
			AssertEquals(false, GetNewBusinessObject(GetNewWhsDocket()).IsCustomsTransaction);
		}

		public void TestIsWarehouseBondEnabled()
		{
			var line = GetNewBusinessObject();
			var docket = GetNewWhsDocket();
			line.WE_WD = docket.PK;
			var whs = Helper.CreateWarehouse("WHS1");
			docket.WD_WW_Whs = whs.PK;
			Helper.EnableWarehouseForBond(whs, true);
			Assert("Should be Bond Whs", line.IsWarehouseBondEnabled);

			docket.WD_WW_Whs = ZGuid.Empty;
			Assert("Should not be Bond Whs", !line.IsWarehouseBondEnabled);
			docket.WD_WW_Whs = whs.PK;

			line.WE_WD = ZGuid.Empty;
			Assert("Should not be Bond Whs", !line.IsWarehouseBondEnabled);
		}

		public void TestIsWarehouseExciseEnabled()
		{
			var line = GetNewBusinessObject();
			var docket = GetNewWhsDocket();
			line.WE_WD = docket.PK;
			var whs = Helper.CreateWarehouse("WHS1");
			docket.WD_WW_Whs = whs.PK;
			Helper.EnableWarehouseForExcise(whs, true);
			Assert("Should be Excise Whs", line.IsWarehouseExciseEnabled);

			docket.WD_WW_Whs = ZGuid.Empty;
			Assert("Should not be Excise Whs", !line.IsWarehouseExciseEnabled);
			docket.WD_WW_Whs = whs.PK;

			line.WE_WD = ZGuid.Empty;
			Assert("Should not be Excise Whs", !line.IsWarehouseExciseEnabled);
		}

		public void TestClassification()
		{
			var line = GetNewBusinessObject();
			AssertEquals("CustomsTariffLookup", line.CustomsTariffLookupInfo.Name);
			AssertEquals("CustomsTariffItem", line.CustomsTariffItemInfo.Name);
			AssertEquals("CustomsTariffDesc", line.CustomsTariffDescInfo.Name);

			var part = Factory.New<OrgSupplierPart>();
			line.WE_OP = part.PK;

			var classification = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();
			classification[CusClassificationSchema.CC_ClassificationType.Name] = "IMP";
			classification[CusClassificationSchema.CC_LookupCode.Name] = "LOOKUP";
			classification[CusClassificationSchema.CC_TariffNum.Name] = "9901.22.23";
			classification[CusClassificationSchema.CC_Description.Name] = "DESCRIPTION";

			var pivot = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			pivot[CusClassPartPivotSchema.CI_CC.Name] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_OP.Name] = part.PK;

			AssertEquals("LOOKUP", line.CustomsTariffLookup);
			AssertEquals("9901.22.23", line.CustomsTariffItem);
			AssertEquals("DESCRIPTION", line.CustomsTariffDesc);
		}

		#endregion

		#region BusinessObject Overrides

		public void TestLightValidatonDisabled()
		{
			AssertEquals(false, DocketLine.LightValidationEnabled);
		}

		public virtual void TestRunPreSaveValidationCore()
		{
			AssertNoErrors(DocketLine.WE_OPInfo);
			DocketLine.RunPreSaveValidation();
			AssertHasError(DocketLine.WE_OPInfo, "Please enter a Product Code.");
		}

		public virtual void TestRunLoadValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 120m);

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			var docketLinePK = docketLine.PK;

			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 10m;
			docketLine.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines.
			Factory.Save();

			docket = null;
			var newFactory1 = new BusinessObjectFactory();
			docketLine = newFactory1.Load<TDocketLine>(docketLinePK);
			AssertEquals("Precondition", data.Part1.OP_StockKeepingUnit, docketLine.WE_F3_NKPackType);
			AssertEquals(docketLine.WE_TransactionQuantity, docketLine.WE_PackQuantity);

			docketLine.WE_F3_NKPackType = "CTN";
			docketLine.WE_PackQuantity = 10m;
			docketLine.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines.
			AssertEquals(120m, docketLine.WE_TransactionQuantity);
			newFactory1.Save();

			docket = null;
			var newFactory2 = new BusinessObjectFactory();
			docketLine = newFactory2.Load<TDocketLine>(docketLinePK);
			AssertEquals("CTN", docketLine.WE_F3_NKPackType);
			AssertEquals(120m, docketLine.WE_TransactionQuantity);
			AssertEquals(10m, docketLine.WE_PackQuantity);
		}

		#region TestInventoryViewIsUpdatedOnDataRefresh

		public void TestInventoryViewIsUpdatedOnDataRefresh()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);

			if (docketLine.IsInventoryLine)
			{
				var docket = docketLine.Docket;
				docket.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				AssertIsFinalisedPrecondition(docketLine.Docket);
				var currentTotalUnits = docketLine.WE_StockOnHand;
				AssertEquals("Precondition: total units match.", currentTotalUnits, docketLine.Inventory[0].WI_TotalUnits);

				var newFactory = new BusinessObjectFactory();
				var helperInNewFactory = new WhsTestHelperFunctions(newFactory);
				var adjustmentInNewFactory = helperInNewFactory.CreateWhsAdjustment(docket.Client, docket.Warehouse, "AD2", Notify);
				helperInNewFactory.CreateWhsAdjustmentLine(adjustmentInNewFactory, docketLine.SupplierPart, -1m, docketLine.Location);
				adjustmentInNewFactory.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustmentInNewFactory);
				newFactory.Save();

				AssertEquals("Precondition: WE_StockOnHand updated via data refresh bus.", currentTotalUnits - 1, docketLine.WE_StockOnHand);
				AssertEquals("WI_TotalUnits updated via data refresh bus.", currentTotalUnits - 1, docketLine.Inventory[0].WI_TotalUnits);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Cloning

		#region TestClone

		public void TestClone()
		{
			if (DocketLine.SupportsClone())
			{
				TestCloneWE_WD(DocketLine);
				TestCloneLineNoAndSubLineNo(DocketLine);
				TestClonePicked(DocketLine);
				TestClonePutaway(DocketLine);
				TestClone_HeldCode(DocketLine);
				TestCloneWE_FinalisedDate(DocketLine);
				TestCloneWE_DocketLineStatus(DocketLine);
				TestClone_OtherStuff(DocketLine);
				TestCloneParentLineAndMatchingLine(DocketLine);
			}
			else
			{
				Assert("Clone is not supported, no need to test it.", true);
			}
		}

		protected virtual void TestCloneWE_WD(WhsDocketLine docketLine)
		{
			var clone1 = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Cloned WE_WD should be empty", ZGuid.Empty, clone1.WE_WD);

			var clone2 = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Cloned WE_WD should be empty", ZGuid.Empty, clone2.WE_WD);
		}

		void TestCloneWE_FinalisedDate(WhsDocketLine docketLine)
		{
			docketLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Cloned WE_FinalisedDate should be empty", ZDateTimeOffset.Empty, clone.WE_FinalisedDate);
		}

		protected virtual void TestCloneWE_DocketLineStatus(WhsDocketLine docketLine)
		{
			docketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Cloned WE_DocketLineStatus should be empty", "", clone.WE_DocketLineStatus);
		}

		protected virtual void TestCloneLineNoAndSubLineNo(WhsDocketLine docketLine)
		{
			var docket = docketLine.Docket;
			docketLine.WE_LineNo = 4;
			docketLine.WE_SubLineNo = 3;

			var clone1 = (WhsDocketLine)docketLine.Clone();
			if (!docket.Lines.Contains(clone1))
			{
				docket.Lines.Add(clone1);
			}

			var clone2 = (WhsDocketLine)docketLine.Clone();
			if (!docket.Lines.Contains(clone2))
			{
				docket.Lines.Add(clone2);
			}

			AssertEquals("Original SubLineNo should not change", (short)3, docketLine.WE_SubLineNo);
			AssertEquals("Cloned SubLineNo should be incremented", (short)4, clone1.WE_SubLineNo);
			AssertEquals("Cloned SubLineNo should be incremented", (short)5, clone2.WE_SubLineNo);
			AssertEquals("Cloned LineNo should equal original", docketLine.WE_LineNo, clone1.WE_LineNo);
			AssertEquals("Cloned LineNo should equal original", docketLine.WE_LineNo, clone2.WE_LineNo);

			docketLine.WE_LineNo = 16;
			var clone3 = (WhsDocketLine)docketLine.Clone();
			if (!docket.Lines.Contains(clone3))
			{
				docket.Lines.Add(clone3);
			}
			AssertEquals("Cloned SubLineNo should be incremented", (short)4, clone3.WE_SubLineNo);
			AssertEquals("Cloned LineNo should equal original", docketLine.WE_LineNo, clone3.WE_LineNo);
		}

		protected virtual void TestClonePicked(WhsDocketLine docketLine)
		{
		}

		protected virtual void TestClonePutaway(WhsDocketLine docketLine)
		{
			var user = Helper.CreateGlbStaff("AAA", "AAA");
			docketLine.WE_GS_NKPutawayBy = user.GS_Code;
			docketLine.WE_PutawayTime = ZDateTimeOffset.Today;

			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("", clone.WE_GS_NKPutawayBy);
			AssertEquals(ZDateTimeOffset.Empty, clone.WE_PutawayTime);
		}

		protected virtual void TestCloneParentLineAndMatchingLine(WhsDocketLine docketLine)
		{
			docketLine.WE_WE_MatchingLine = GetNewBusinessObject(GetNewWhsDocket()).PK;
			docketLine.WE_WE_ParentDocketLine = GetNewBusinessObject(GetNewWhsDocket()).PK;

			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Should not clone Matching Line.", ZGuid.Empty, clone.WE_WE_MatchingLine);
			AssertEquals("Should not clone Parent Line.", ZGuid.Empty, clone.WE_WE_ParentDocketLine);
		}

		void TestClone_OtherStuff(WhsDocketLine docketLine)
		{
			var today = ZDateTimeOffset.Today;
			docketLine.WE_ReceiveCrossDockOrderNo = "ORD1";
			docketLine.WE_RequiredByDate = today;

			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("WE_ReceiveCrossDockOrderNo", "ORD1", clone.WE_ReceiveCrossDockOrderNo);
			AssertEquals("WE_RequiredByDate", today, clone.WE_RequiredByDate);
		}

		void TestClone_HeldCode(WhsDocketLine docketLine)
		{
			docketLine.WE_WHC_NKOriginalInventoryHeldCode = "ABC";
			docketLine.WE_WHC_NKCurrentInventoryHeldCode = "DEF";
			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Cloned WE_WHC_NKOriginalInventoryHeldCode should be empty", "ABC", clone.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("Cloned WE_WHC_NKCurrentInventoryHeldCode should be empty", "DEF", clone.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region TestClone_UpdateTotals

		public void TestClone_UpdateTotals()
		{
			TestClone_UpdateTotalsCore();
		}

		protected virtual void TestClone_UpdateTotalsCore()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);

			if (docketLine.SupportsClone() && docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
				Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.02m, "M3");

				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;

				AssertEquals("Precondition - ensure Total Weight is correct", 20m, docket.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Volume is correct", 0.2m, docket.WD_TotalCubic);
				if (docket as WhsReceive == null) // Receives TotalUnitsFromLines is calculated from Inventory not DocketLines
				{
					AssertEquals("Precondition - ensure Total Line Units is correct", 10m, docket.WD_TotalUnitsFromLines);
				}

				var clone = (WhsDocketLine)docketLine.Clone();
				docket.Lines.Add(clone); // in the system it's happens when new Line added to the Grid, but in test we need to simulate it.
				AssertEquals("Total Weight should be correct", 40m, docket.WD_TotalWeight);
				AssertEquals("Total Volume should be correct", 0.4m, docket.WD_TotalCubic);
				if (docket as WhsReceive == null) // Receives TotalUnitsFromLines is calculated from Inventory not DocketLines
				{
					AssertEquals("Total Line Units should be correct", 20m, docket.WD_TotalUnitsFromLines);
				}
			}
			else
			{
				Assert("Clone or/and Update Totals is not supported, no need to test it.", true);
			}
		}

		#endregion

		#region TestClone_CustomsDataAndUSBondedColumns

		public void TestClone_CustomsDataAndUSBondedColumns()
		{
			if (SupportsCustomsSubType)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				Helper.EnableWarehouseForBond(data.Whs1, true);
				Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				SetDocketToCustomsTransaction(docket);
				var docketLine = docket.Lines.AddNew();
				docketLine.WE_PackageGroupId = "GROUP1";
				docketLine.WE_PerPackageQty = 5m;
				docketLine.WE_BondedEntryKey = "KEY-1";
				docketLine.WE_AllocationKey = "ALO-1";

				var customsData = docketLine.CustomsData;
				customsData.WB_EntryKey = "KEY";
				customsData.WB_EntryLineNo = (ZShort)1;
				customsData.WB_CustomsQty = 10m;

				var clonedDocketLine = (WhsDocketLine)docketLine.Clone();
				AssertEquals("WE_PackageGroupId", "GROUP1", docketLine.WE_PackageGroupId);
				AssertEquals("WE_PerPackageQty", 5m, docketLine.WE_PerPackageQty);
				AssertEquals("WE_BondedEntryKey", "KEY-1", docketLine.WE_BondedEntryKey);
				AssertEquals("WE_AllocationKey", "ALO-1", docketLine.WE_AllocationKey);

				var clonedCustomsData = clonedDocketLine.CustomsData;
				AssertEquals("WB_EntryKey", "KEY", clonedCustomsData.WB_EntryKey);
				AssertEquals("WB_EntryLineNo", (ZShort)1, clonedCustomsData.WB_EntryLineNo);
				AssertEquals("WB_CustomsQty", 10m, clonedCustomsData.WB_CustomsQty);
				AssertEquals("WB_ParentID", clonedDocketLine.PK, clonedCustomsData.WB_ParentID);
			}
			else
			{
				Assert("This docket cannot be customs.", true);
			}
		}

		protected virtual void SetDocketToCustomsTransaction(TDocket docket)
		{
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;
		}

		#endregion

		#region TestClone_CustomsDataReadonly

		public void TestClone_CustomsDataReadonly()
		{
			if (SupportsCustomsSubType)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				Helper.EnableWarehouseForBond(data.Whs1, true);
				Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
				data.Whs1.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				if (docket.WD_DocketType == DocketType.Codes.Order)
				{
					docket.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
				}
				else
				{
					SetDocketToCustomsTransaction(docket);
				}

				var docketLine = docket.Lines.AddNew();

				var customsData = docketLine.CustomsData;
				AssertEquals("Precondition", docket.WD_DocketType == DocketType.Codes.Order, customsData.ReadOnly);

				var clonedDocketLine = (WhsDocketLine)docketLine.Clone();
				var clonedCustomsData = clonedDocketLine.CustomsData;
				clonedCustomsData.WB_ParentID = clonedDocketLine.PK;
				clonedDocketLine.WE_WD = docket.PK;
				AssertEquals($"Docket Type is ('{docket.WD_DocketType}') only for order it should be readonly.", docket.WD_DocketType == DocketType.Codes.Order, clonedCustomsData.ReadOnly);
			}
			else
			{
				Assert("This docket cannot be customs.", true);
			}
		}

		#endregion

		#endregion

		#region Fetch Strategy

		public void TestGetFetchStrategyIsCorrectType()
		{
			AssertEquals(FetchStrategyType, DocketLine.FetchStrategy.GetType());
		}

		protected virtual Type FetchStrategyType
		{
			get { return typeof(WhsDocketLineFetchStrategy); }
		}

		#endregion

		#region TestIsInventoryLine

		public void TestIsInventoryLine()
		{
			TestIsInventoryLineCore();
		}

		protected virtual void TestIsInventoryLineCore()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals(false, docketLine.IsInventoryLine);
		}

		#endregion

		#region TestOnSaved_SetsInventoryRowState

		public void TestOnSaved_SetsInventoryRowState()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.Docket.FinaliseDocket();
				AssertIsFinalisedPrecondition(docketLine);
				AssertEquals("Precondition: Has Inventory.", 1, docketLine.Inventory.Count);

				var inventory = docketLine.Inventory[0];
				var inventoryRow = ((IBusinessObjectInternals)inventory).Row;
				AssertNotEquals("Precondition", DataRowState.Unchanged, inventoryRow.RowState);

				CombineAssertions(() =>
				{
					Factory.Save();
					AssertEquals("Should change to Unchanged after Factory save.", DataRowState.Unchanged, inventoryRow.RowState);

					inventory.Delete();
					AssertEquals("Should change to deleted.", DataRowState.Deleted, inventoryRow.RowState);
				});
			}
			else
			{
				Assert("Inventory cannot be attached to his DocketLine type.", true);
			}
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var docketLine = Factory.New<TDocketLine>();
			AssertEquals("Docket Line", docketLine.HumanReadableName);
		}

		#endregion

		#endregion

		#region Saving

		[UseSnapshotProtection]
		public void TestOverReduceCheckOnInventoryLine()
		{
			if (CanHaveInventoryAttached)
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var factory = new BusinessObjectFactory(connection);
					var data = new TestDataSimpleEnvironment(factory, 2, 1);
					var helper = new WhsTestHelperFunctions(factory);
					var finalisableDocketHelper = GetNewDocketHelper(factory);
					finalisableDocketHelper.GetNewFinalisableDocketWithOneLine(data, 25m, "D1", finalise: true);
					finalisableDocketHelper.GetNewFinalisableDocketWithOneLine(data, 25m, "D2", finalise: true);
					finalisableDocketHelper.GetNewFinalisableDocketWithOneLine(data, 25m, "D3", finalise: true);
					finalisableDocketHelper.GetNewFinalisableDocketWithOneLine(data, 25m, "D4", finalise: true);

					var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
					var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 100m);
					var pick = helper.CreatePickNew(order);

					using (connection.BeginTransactionWithManager())
					{
						connection.ExecuteNonQuery(WhsPickLineDO.UpdateWhere(l => l.WZ_WE_TransactionLine == orderLine.PK.ToGuid()).Set(l => l.WZ_Units, 24).AsSQL());
						connection.CommitTransaction();
					}

					factory.ReloadAllSafe<WhsPickLine>(); // reload picklines in memory

					BusinessObjectFactory.SavingEventHandler handler = f => { f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f)); };
					factory.Saving += handler;

					foreach (var inventoryLine in pick.GetAllPickLines().Select(pl => pl.InventoryLine))
					{
						inventoryLine.HasChanges = true; // to cause inventory lines to have changes
					}

					// pre-condition
					Helper.AssertZCannotSaveExceptionThrown("Test", factory.Save);
					factory.Saving -= handler;

					var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
					foreach (var pickLine in pick.GetAllPickLines())
					{
						var inventoryLine = pickLine.InventoryLine;

						// reduce WE_StockOnHand from 25 to 23, Committed Qty is 24
						// if the trigger suspension is not working, saving this change to the DB will fail because the inventory is updated first, the trigger will then fire on save before we have had a chance to also update the pickline.
						var adjustmentLine = helper.CreateWhsAdjustmentLine(adjustment, inventoryLine.SupplierPart, -2m, inventoryLine.Location);
						helper.CreateWhsPickLine(adjustmentLine, inventoryLine.Inventory[0], 2m); // manually creating pick line to ensure correct inventory is committed - DO NOT RE-USE

						// reduce committed qty from 24 to 23
						// after doing this, we are no longer over-committing
						pickLine.WZ_Units--;
					}
					adjustment.FinaliseDocket();
					AssertIsFinalisedPrecondition(adjustment);

					AssertNoExceptionThrown(factory.Save); // should not fail when saving this transaction
				}
			}
			else
			{
				Assert("This test is only relevant to Docket Lines that create Inventory.", true);
			}
		}

		// throw exception before committing transaction
		class ServiceThatThrowsException : IAfterOnSavingBOProcessingService
		{
			public ServiceThatThrowsException(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<ServiceThatThrowsException>();
				throw new ZCannotSaveException("Test", "Test", ExceptionType.BusinessFailure);
			}
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals(ExpectedDefaultInventoryStatus, docketLine.WE_OriginalInventoryStatus);
			AssertEquals(ExpectedDefaultInventoryStatus, docketLine.WE_CurrentInventoryStatus);

			TestSetDefaultValuesCore(docketLine);
		}

		protected abstract string ExpectedDefaultInventoryStatus { get; }

		protected virtual void TestSetDefaultValuesCore(WhsDocketLine docketLine)
		{
		}

		#endregion

		#region TestDelete

		#region TestDelete

		public void TestDelete()
		{
			DocketLine.WE_BondedEntryKey = "E1-1";

			var attribute = Factory.New<WhsBondedWarehouseAttribute>();
			attribute.WB_ParentID = DocketLine.PK;
			attribute.WB_EntryKey = "E1";
			attribute.WB_EntryLineNo = 1;
			AssertEquals("Precondition", attribute.PK, DocketLine.CustomsData.PK);
			AssertEquals("Precondition", false, attribute.IsDeleted);
			AssertEquals("Precondition", false, DocketLine.IsDeleted);

			DocketLine.Delete();

			AssertEquals(true, attribute.IsDeleted);
			AssertEquals(true, DocketLine.IsDeleted);
		}

		#endregion

		#region TestDelete_Inventory

		public void TestDelete_Inventory()
		{
			if (CanHaveInventoryAttached)
			{
				var inventory1 = (DocketLine.Inventory.Count > 0)
					? DocketLine.Inventory[0]
					: DocketLine.Inventory.AddNew();
				var inventory2 = DocketLine.Inventory.AddNew();
				AssertEquals("Precondition", 2, DocketLine.Inventory.Count);
				AssertEquals("Precondition", false, inventory1.IsDeleted);
				AssertEquals("Precondition", false, inventory2.IsDeleted);
				AssertEquals("Precondition", false, DocketLine.IsDeleted);

				DocketLine.Delete();

				AssertEquals(true, DocketLine.IsDeleted);
				AssertEquals(0, DocketLine.Inventory.Count);
				AssertEquals(true, inventory1.IsDeleted);
				AssertEquals(true, inventory2.IsDeleted);
			}
			else
			{
				Assert("Inventory cannot be attached to his DocketLine type.", true);
			}
		}

		protected virtual bool CanHaveInventoryAttached
		{
			get { return true; }
		}

		#endregion

		#region TestDelete_ModifyingMaxLineNo

		public void TestDelete_ModifyingMaxLineNo()
		{
			var docket = GetNewWhsDocket();

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();

			AssertEquals((ZShort)2, docket.MaxLineNoManager.CurrentMaxLineNo);

			line1.Delete();
			AssertEquals((ZShort)2, docket.MaxLineNoManager.CurrentMaxLineNo);

			line2.Delete();
			AssertEquals((ZShort)0, docket.MaxLineNoManager.CurrentMaxLineNo);
		}

		#endregion

		#region TestOnDeleteByInventory

		public virtual void TestOnDeleteByInventory()
		{
			var docket = GetNewWhsDocket();
			var line = GetNewBusinessObject(docket);

			var inventory1 = line.Inventory.AddNew();
			var inventory2 = line.Inventory.AddNew();
			inventory1.WI_InDocketLineType = inventory2.WI_InDocketLineType = docket.WD_DocketType;
			inventory1.WI_WE_InDocketLine = inventory2.WI_WE_InDocketLine = line.PK;

			inventory1.Delete();
			AssertEquals(true, inventory1.IsDeleted);

			AssertEquals(ShouldDocketLineBeDeletedByInventory, inventory2.IsDeleted);
			AssertEquals(ShouldDocketLineBeDeletedByInventory, line.IsDeleted);
		}

		protected virtual bool ShouldDocketLineBeDeletedByInventory
		{
			get { return false; }
		}

		#endregion

		#region TestDelete_DoesNotRecalculateTotalWeightAndVolume

		public void TestDelete_DoesNotRecalculateTotalWeightAndVolume()
		{
			TestDelete_DoesNotRecalculateTotalWeightAndVolumeCore();
		}

		protected virtual void TestDelete_DoesNotRecalculateTotalWeightAndVolumeCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 5m, Constants.Weight.Kilograms, 2m, Constants.Volume.CubicDecimetres);

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var docketLine1 = CreateWhsDocketLine(docket, data.Part1, 10m);
				var docketLine2 = CreateWhsDocketLine(docket, data.Part1, 20m);
				AssertWeightVolume(docket, 150m, Constants.Weight.Kilograms, 0.06m, Constants.Volume.CubicMetres);

				// Manually modification of Weight/Volume is allowed.
				docket.WD_TotalWeight = 200m;
				docket.WD_TotalCubic = 0.1m;
				AssertWeightVolume(docket, 200m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicMetres);

				// Deleting lines will reduce Weight/Volume of deleted line from the total amount without recalculating the totals.
				docketLine2.Delete();
				AssertWeightVolume(docket, 100m, Constants.Weight.Kilograms, 0.06m, Constants.Volume.CubicMetres);
			}
			else
			{
				Assert("Update Totals is not supported, no need to test it.", true);
			}
		}

		WhsDocketLine CreateWhsDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			var line = docket.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = units;

			return line;
		}

		void AssertWeightVolume(WhsDocket docket, decimal expectedWeight, string expectedWeightUQ, decimal expectedVolume, string expectedVolumeUQ)
		{
			AssertEquals("WD_TotalWeight", docket.WD_TotalWeight, expectedWeight);
			AssertEquals("WD_TotalWeightUnit", docket.WD_TotalWeightUnit, expectedWeightUQ);
			AssertEquals("WD_TotalVolume", docket.WD_TotalCubic, expectedVolume);
			AssertEquals("WD_TotalVolumeUnit", docket.WD_TotalCubicUnit, expectedVolumeUQ);
		}

		#endregion

		#region TestDelete_PickLines

		public void TestDelete_PickLines()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			var pickLine1 = docketLine.PickLines.AddNew();
			var pickLine2 = docketLine.PickLines.AddNew();
			AssertEquals("Precondition: PickLine is not deleted.", false, pickLine1.IsDeleted);
			AssertEquals("Precondition: PickLine is not deleted.", false, pickLine2.IsDeleted);

			docketLine.Delete();
			AssertEquals("When Docket Line is deleted, should delete all pick lines attached.", true, pickLine1.IsDeleted);
			AssertEquals("When Docket Line is deleted, should delete all pick lines attached.", true, pickLine2.IsDeleted);
		}

		#endregion

		#region TestDelete_InventoryHoldChangeLogs

		public void TestDelete_InventoryHoldChangeLogs()
		{
			if (CanHaveInventoryAttached)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = CreateWhsDocketLine(docket, data.Part1, 10m);
				
				var changeLog1 = Factory.New<WhsInventoryHoldChangeLog>();
				changeLog1.WHL_WE_ParentDocketLine = docketLine.PK;
				changeLog1.WHL_LogVersion = 1;

				var changeLog2 = Factory.New<WhsInventoryHoldChangeLog>();
				changeLog2.WHL_WE_ParentDocketLine = docketLine.PK;
				changeLog2.WHL_LogVersion = 2;

				docketLine.Delete();
				AssertNoExceptionThrown(Factory.Save);

				AssertEquals(true, docketLine.IsDeleted);
				AssertEquals(true, changeLog1.IsDeleted);
				AssertEquals(true, changeLog2.IsDeleted);
			}
			else
			{
				Assert("Inventory cannot be attached to his DocketLine type.", true);
			}
		}

		#endregion

		#endregion

		#region TestHasChanges

		public void TestHasChanges()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			docketLine.Docket.FinaliseDocket();
			AssertEquals(true, docketLine.IsFinalised);
			Factory.Save();
			if (CanHaveInventoryAttached)
			{
				var inventory = docketLine.Inventory.FirstOrDefault();
				AssertEquals("Precondition", false, inventory.HasChanges);

				docketLine.HasChanges = true;
				AssertEquals(false, inventory.HasChanges);

				docketLine.IsInventoryEditForm = true;
				docketLine.HasChanges = false;
				AssertEquals(false, inventory.HasChanges);

				docketLine.HasChanges = true;
				AssertEquals(true, inventory.HasChanges);

				docketLine.HasChanges = false;
				AssertEquals(true, inventory.HasChanges);

				docketLine.HasChanges = true;
				inventory.HasChanges = false;

				using (docketLine.SuspendSettingHasChanges())
				{
					docketLine.HasChanges = false;
					AssertEquals(false, inventory.HasChanges);
				}
			}
			else
			{
				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				docketLine = factory2.Load<TDocketLine>(docketLine.PK);
				AssertNoExceptionThrown(() => docketLine.HasChanges = true);
				AssertEquals("Should not load Inventory.", 0, factory2.GetTableHitCount(WhsInventoryViewSchema.Constants.TableName));
			}
		}

		#endregion

		#region Related Business Objects

		#region TestDocket

		public virtual void TestDocket()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WD = docket.PK;
			AssertEquals(docket.PK, docketLine.Docket.PK);
		}

		#endregion

		#region TestSupplierPart

		public virtual void TestSupplierPart()
		{
			var part = OrgSupplierPart.New(Factory);
			var docketLine = GetNewBusinessObject();
			docketLine.WE_OP = part.PK;
			AssertEquals(part, docketLine.SupplierPart);
		}

		#endregion

		#region TestInventory

		public void TestInventory()
		{
			TestInventoryCore();
		}

		protected virtual void TestInventoryCore()
		{
			var docketLine1 = GetNewBusinessObject(Docket);
			var docketLine2 = GetNewBusinessObject(Docket);

			var inventory1 = (docketLine1.Inventory.Count > 0)
				? docketLine1.Inventory[0]
				: docketLine1.Inventory.AddNew();
			var inventory2 = (docketLine2.Inventory.Count > 0)
				? docketLine2.Inventory[0]
				: docketLine2.Inventory.AddNew();

			AssertEquals(true, docketLine1.IsRegisteredEditableChildObject(docketLine1.Inventory));
			AssertEquals(1, docketLine1.Inventory.Count);
			AssertEquals(1, docketLine2.Inventory.Count);

			AssertCollectionContains(inventory1, docketLine1.Inventory);
			AssertCollectionContains(inventory2, docketLine2.Inventory);

			AssertCollectionNotContains(inventory2, docketLine1.Inventory);
			AssertCollectionNotContains(inventory1, docketLine2.Inventory);
		}

		#endregion

		#region TestReservedPickLines

		public void TestReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);
			var pickableDocketLine = docketLine as WhsPickableDocketLine;
			if (pickableDocketLine != null) // lines that will be reserving stock
			{
				pickableDocketLine.PickableDocket.Pick.CancelPick(); // order need to be un-picked to reserve stock.

				var receiveLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m));
				Helper.CreateReservePickLine(pickableDocketLine, receiveLine.Inventory[0], 3m);
				AssertEquals("Reserved quantity for the line should be 3.", 3m, docketLine.ReservedPickLines.Sum(l => l.WZ_Units));
			}
			else // lines that will create inventory from which stock will be reserved.
			{
				docketLine.Docket.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(docketLine.Docket);

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				var orderLine = order.Lines[0];
				orderLine.ReserveStockIfAbleTo(docketLine.Inventory[0], 3m);
				AssertEquals("Reserved quantity for the line should be 3.", 3m, docketLine.ReservedPickLines.Sum(l => l.WZ_Units));
			}
		}

		#endregion

		#region TestLocationArea

		public void TestLocationArea()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var pickingArea = Helper.CreateArea(whs, "PIK");
			var putawayArea = Helper.CreateArea(whs, "PUT");
			var loc = whs.DefaultLocation;
			loc.WLV_WA_PickingArea = pickingArea.PK;
			loc.WLV_WA_PutawayArea = putawayArea.PK;

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals(pickingArea, docketLine.LocationArea);
		}

		#endregion

		#region TestBOMComponentLinks

		public void TestBOMComponentLinks()
		{
			var docketLine = GetNewBusinessObject(Docket);
			docketLine.WE_WE_OriginalDocketLineForRating = docketLine.PK;
			AssertEquals(0, docketLine.BOMComponentLinks.Count());

			var link1 = Factory.New<WhsBOMInventoryPivot>();
			link1.WIP_WE_InventoryLine = docketLine.PK;
			AssertComponentLinks(link1);

			var link2 = Factory.New<WhsBOMInventoryPivot>();
			link2.WIP_WE_ComponentLine = docketLine.PK;
			AssertComponentLinks(link1);

			var link3 = Factory.New<WhsBOMInventoryPivot>();
			link3.WIP_WE_InventoryLine = ZGuid.NewZGuid();
			AssertComponentLinks(link1);

			var link4 = Factory.New<WhsBOMInventoryPivot>();
			link4.WIP_WE_InventoryLine = docketLine.PK;
			AssertComponentLinks(link1, link4);

			docketLine.WE_WE_OriginalDocketLineForRating = ZGuid.NewZGuid();
			AssertEquals(0, docketLine.BOMComponentLinks.Count());

			void AssertComponentLinks(params WhsBOMInventoryPivot[] expectedLinks)
			{
				if (TestBOMComponentLinks_ExpectedLinkResult)
				{
					AssertContainsExactElementsInAnyOrder(expectedLinks, docketLine.BOMComponentLinks);
				}
				else
				{
					AssertEquals(0, docketLine.BOMComponentLinks.Count());
				}
			}
		}

		public virtual bool TestBOMComponentLinks_ExpectedLinkResult => true;

		#endregion

		#region TestComponentInventoryLines

		public void TestComponentInventoryLines()
		{
			var docketLine = GetNewBusinessObject(Docket);

			var componentLine1 = CreateBOMComponentLinkForLine(docketLine);

			// create PickLine for the same Inventory.
			var pickLine = componentLine1.Inventory[0].CommittedPickLines.Single();
			var newPickLine = Factory.New<WhsPickLine>();
			newPickLine.WZ_WE_TransactionLine = pickLine.WZ_WE_TransactionLine;
			newPickLine.WZ_Units = 1m;
			newPickLine.WZ_WE_OriginalPickedInventoryLine = componentLine1.PK;

			var componentLine2 = CreateBOMComponentLinkForLine(docketLine);

			var someOtherReceiveLine = Factory.New<WhsReceiveLine>();
			var componentLine3 = CreateBOMComponentLinkForLine(someOtherReceiveLine);

			if (docketLine.BOMComponentLinks.Any())
			{
				AssertContainsExactElementsInAnyOrder(new[] { componentLine1, componentLine2 }, docketLine.ComponentInventoryLines);
			}
			else
			{
				AssertEquals("Non-Inventory Lines should not have BOM Component Links.", 0, docketLine.ComponentInventoryLines.Count);
			}

			WhsReceiveLine CreateBOMComponentLinkForLine(WhsDocketLine line)
			{
				var link = Factory.New<WhsBOMInventoryPivot>();
				link.WIP_WE_InventoryLine = line.PK;

				var workOrderLine = Factory.New<WhsWorkOrderLine>();
				var inventoryLine = Factory.New<WhsReceiveLine>();
				var pickLine = Factory.New<WhsPickLine>();
				pickLine.WZ_WE_InventoryLine = inventoryLine.PK;
				pickLine.WZ_WE_TransactionLine = workOrderLine.PK;
				pickLine.WZ_Units = 1m;

				link.WIP_WE_ComponentLine = workOrderLine.PK;

				return inventoryLine;
			}
		}

		#endregion

		#endregion

		#region Validation

		#region TestRunPreSaveValidation_ClearsOutPackageGroupIDAndPerPackageQtyIfDocketIsNotUSBondedJob

		public void TestRunPreSaveValidation_ClearsOutPackageGroupIDAndPerPackageQtyIfDocketIsNotUSBondedJob()
		{
			TestRunPreSaveValidation_ClearsOutPackageGroupIDAndPerPackageQtyIfDocketIsNotUSBondedJobCore();
		}

		protected virtual void TestRunPreSaveValidation_ClearsOutPackageGroupIDAndPerPackageQtyIfDocketIsNotUSBondedJobCore()
		{
			WhsDocket nullDocketToTestDocketLineWithNoDocket = null;
			AssertValuesAreCleared(nullDocketToTestDocketLineWithNoDocket, shouldClearDocketLineValues: true);

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var nonBondedNonUSDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			AssertValuesAreCleared(nonBondedNonUSDocket, shouldClearDocketLineValues: true);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var nonBondedUSDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			AssertValuesAreCleared(nonBondedUSDocket, shouldClearDocketLineValues: true);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedNonUSDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			SetDocketToCustomsTransaction(nonBondedUSDocket);
			AssertValuesAreCleared(bondedNonUSDocket, shouldClearDocketLineValues: true);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var bondedUSDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			SetDocketToCustomsTransaction(bondedUSDocket);
			AssertValuesAreCleared(bondedUSDocket, shouldClearDocketLineValues: false);
		}

		public void TestRunPreSaveValidation_CheckTemporaryProducts()
		{
			// Arrange
			const string errorMsg = "Please Create Product Files for the line.";

			var docketLine = GetNewBusinessObject();
			docketLine.RunPreSaveValidation();

			AssertHasRowError("Has invalid temporary product validation error.", docketLine, errorMsg);

			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			docketLine.WE_OP = part.PK;

			// Action
			docketLine.RunPreSaveValidation();

			// Assert
			AssertNoRowErrors("No invalid temporary product validation error.", docketLine);
		}

		void AssertValuesAreCleared(WhsDocket docket, bool shouldClearDocketLineValues)
		{
			var docketLine = docket != null ? GetNewBusinessObject(docket) : (WhsDocketLine)GetNewBusinessObject();
			docketLine.WE_PackageGroupId = "ABC";
			docketLine.WE_PerPackageQty = 2m;
			docketLine.RunPreSaveValidation();
			AssertEquals(shouldClearDocketLineValues ? "" : "ABC", docketLine.WE_PackageGroupId);
			AssertEquals(shouldClearDocketLineValues ? 0m : 2m, docketLine.WE_PerPackageQty);
		}

		#endregion

		#region TestSynchroniseOffsetsToWarehouseTime

		public void TestSynchroniseOffsetsToWarehouseTime_AdjustmentArrivalDate()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			var warehouse = docketLine.Warehouse;
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			docketLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));
			docketLine.SynchroniseOffsetsToWarehouseTime(calculationTimeZone);

			Assert("DocketLine should not be in error.", !docketLine.HasErrors);
			var expectedOffset = warehouse.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				docketLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		public void TestSynchroniseOffsetsToWarehouseTime_AdjustmentArrivalDate_FailsIfPropertyInError()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			var warehouse = docketLine.Warehouse;
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();

			docketLine.SupplierPart.OP_IsActive = false;
			docketLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(0024, 06, 12, 12, 30, 00, TimeSpan.FromHours(0));

			docketLine.RunPreSaveValidation();
			Assert("PropertyInfo should be in error.", docketLine.WE_AdjustmentArrivalDateInfo.HasErrors());

			docketLine.SynchroniseOffsetsToWarehouseTime(calculationTimeZone);
			AssertEquals("Do not sychronise Offset if Docket is in Error.",
				"12-Jun-0024 12:30 +00:00",
				docketLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm zzz"));
		}

		#endregion

		#region TestSettingPackQuantitySetsWE_TransactionQuantity

		public void TestSettingPackQuantitySetsWE_TransactionQuantity()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Helper.CreateProductUnit(part, "CTN", "BOX", 2m);

			var docketLine = GetNewBusinessObject();

			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 0;
			docketLine.WE_PackQuantity = 3;
			AssertEquals(3.0m, docketLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestSettingPackQuantitySetsWE_TransactionQuantity_WithEmptyPackType

		public void TestSettingPackQuantitySetsWE_TransactionQuantity_WithEmptyPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			var part = Helper.CreateProduct(data.Org1, "P1");

			docketLine.WE_OP = part.PK;
			docketLine.WE_F3_NKPackType = "";
			docketLine.WE_PackQuantity = 2m;
			AssertEquals("If PackType is empty, should set to PackQuantity.", 2.0m, docketLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestSettingPackQuantity_SetsPackTypeOnTemporaryProducts

		public void TestSettingPackQuantity_SetsPackTypeOnTemporaryProducts()
		{
			// Weird case, but WhsInventoryView handled it before so WhsDocketLine should too
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = ZGuid.Invalid; // Temp Product
			docketLine.ProductCode = "TEMP";

			if (docketLine.IsTemporaryProduct)
			{
				AssertEquals("Precondition", "", docketLine.WE_F3_NKPackType);
				AssertEquals("Precondition", "UNT", docketLine.ProductUQ);

				docketLine.WE_PackQuantity = 1m;
				AssertEquals("Should set WE_F3_NKPackType to UNT.", "UNT", docketLine.WE_F3_NKPackType);
				AssertEquals("ProductUQ should be unchanged.", "UNT", docketLine.ProductUQ);
			}
			else
			{
				Assert("docketLine does not support temporary products.", !(docketLine is ISupportTemporaryProduct));
			}
		}

		#endregion

		#region TestSettingWE_TransactionQuantitySetsPackQuantity

		public void TestSettingWE_TransactionQuantitySetsPackQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			var part = Helper.CreateProduct(data.Org1, "P1");
			Helper.CreateProductUnit(part, "BOX", 2m);

			docketLine.WE_OP = part.PK;
			docketLine.WE_F3_NKPackType = "BOX";
			docketLine.WE_TransactionQuantity = 2m;
			AssertEquals(1.0m, docketLine.WE_PackQuantity);
		}

		#endregion

		#region TestSettingWE_TransactionQuantitySetsPackQuantity_WithEmptyPackType

		public void TestSettingWE_TransactionQuantitySetsPackQuantity_WithEmptyPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			var part = Helper.CreateProduct(data.Org1, "P1");

			docketLine.WE_OP = part.PK;
			docketLine.WE_F3_NKPackType = "";
			docketLine.WE_TransactionQuantity = 2m;
			AssertEquals("If PackType is empty, should set to WE_TransactionQuantity.", 2.0m, docketLine.WE_PackQuantity);
		}

		#endregion

		#region TestGetOrderedQuantityFromPackageQuantity

		public void TestGetOrderedQuantityFromPackageQuantity()
		{
			var docketLine = GetNewBusinessObject(Docket);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m
			Helper.CreateProductUnit(part, "BOX", 2m);

			AssertEquals("Precondition", true, docketLine.WE_F3_NKPackType.IsEmpty);
			docketLine.WE_OP = part.PK;
			AssertEquals("Precondition", 10m, docketLine.GetOrderedQuantityFromPackageQuantity(10m));

			docketLine.WE_F3_NKPackType = "CTN";
			AssertEquals("CTN", docketLine.WE_F3_NKPackType);
			AssertEquals(120m, docketLine.GetOrderedQuantityFromPackageQuantity(10m));

			docketLine.WE_F3_NKPackType = "BOX";
			AssertEquals("BOX", docketLine.WE_F3_NKPackType);
			AssertEquals(20m, docketLine.GetOrderedQuantityFromPackageQuantity(10m));
		}

		public void TestGetOrderedQuantityFromPackageQuantity_ZeroOrderedQuantity()
		{
			var docketLine = GetNewBusinessObject(Docket);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Helper.CreateProductUnit(part, "KG", 0.04m);

			docketLine.WE_F3_NKPackType = "KG";
			docketLine.WE_OP = part.PK;
			AssertEquals(0m, docketLine.GetOrderedQuantityFromPackageQuantity(12m));
			AssertEquals(1m, docketLine.GetOrderedQuantityFromPackageQuantity(13m));
		}

		public void TestGetPackageQuantityFromOrderedQuantity_ZeroPackageQuantity()
		{
			var docketLine = GetNewBusinessObject(Docket);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Helper.CreateProductUnit(part, "KG", 1000m);

			docketLine.WE_F3_NKPackType = "KG";
			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 4m;
			AssertEquals(0m, docketLine.WE_PackQuantity);

			docketLine.WE_TransactionQuantity = 5m;
			AssertEquals(0.01m, docketLine.WE_PackQuantity);
		}

		#endregion

		#region TestGetNewValidation

		public void TestGetNewValidation()
		{
			AssertNotNull(DocketLine.Validation);
			AssertEquals(GetExpectedValidationType(), DocketLine.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();

		#endregion

		#endregion

		#region Lookups

		public void TestGetNewLookups()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertNotNull(docketLine.Lookups);
			AssertEquals(GetExpectedLookupsType(), docketLine.Lookups.GetType());
		}

		protected abstract Type GetExpectedLookupsType();

		#endregion

		#region Properties

		#region DocketLine Flags

		public virtual void TestIsDocketFinalising()
		{
			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket();
			AssertEquals(false, docketLine.IsDocketFinalising);
			using (new SemaphoreManager(docket.FinaliseDocketSemaphore))
			{
				docketLine.WE_WD = ZGuid.Empty;
				AssertEquals(false, docketLine.IsDocketFinalising);
				docketLine.WE_WD = docket.PK;
				AssertEquals(true, docketLine.IsDocketFinalising);
			}
			AssertEquals(false, docketLine.IsDocketFinalising);
		}

		public void TestIsDocketFinalised()
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("Precondition", false, docketLine.IsDocketFinalised);

			var docket = GetNewWhsDocket(docketLine);
			AssertEquals("Precondition", false, docketLine.IsDocketFinalised);

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals(true, docketLine.IsDocketFinalised);
			AssertEquals(docket.IsFinalised, docketLine.IsDocketFinalised);
		}

		public void TestIsDocketCancelled()
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("Precondition", false, docketLine.IsDocketCancelled);

			var docket = GetNewWhsDocket(docketLine);
			AssertEquals("Precondition", false, docketLine.IsDocketCancelled);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, docketLine.IsDocketCancelled);
			AssertEquals(docket.IsCancelled, docketLine.IsDocketCancelled);
		}

		public void TestIsDocketFinalisedOrCancelled()
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("Precondition", false, docketLine.IsDocketFinalisedOrCancelled);

			var docket = GetNewWhsDocket(docketLine);
			AssertEquals("Precondition", false, docketLine.IsDocketFinalisedOrCancelled);

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(true, docketLine.IsDocketFinalisedOrCancelled);
			AssertEquals(docket.IsFinalisedOrCancelled, docketLine.IsDocketFinalisedOrCancelled);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, docketLine.IsDocketFinalisedOrCancelled);
			AssertEquals(docket.IsFinalisedOrCancelled, docketLine.IsDocketFinalisedOrCancelled);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(false, docketLine.IsDocketFinalisedOrCancelled);
			AssertEquals(docket.IsFinalisedOrCancelled, docketLine.IsDocketFinalisedOrCancelled);
		}

		public virtual void TestCanUpdateFromInventory()
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("Precondition", true, docketLine.CanUpdateFromInventory);

			var docket = GetNewWhsDocket(docketLine);
			AssertEquals("Precondition", true, docketLine.CanUpdateFromInventory);

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(false, docketLine.CanUpdateFromInventory);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(false, docketLine.CanUpdateFromInventory);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(true, docketLine.CanUpdateFromInventory);
		}

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			TestIsFinalisedCore(new TestDataSimpleEnvironment(Factory));
		}

		protected virtual void TestIsFinalisedCore(TestDataSimpleEnvironment data)
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("Initially line should not be finalised.", false, docketLine.IsFinalised);
			AssertEquals("Initially line should not be finalised.", false, docketLine.WE_FinalisedDate.IsValid);
			docketLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals("When docket line finalise date is set, the property should reflect it.", true, docketLine.IsFinalised);
		}

		#endregion

		#region TestIsTemporaryProduct

		public void TestIsTemporaryProduct()
		{
			TestIsTemporaryProductCore();
		}

		protected virtual void TestIsTemporaryProductCore()
		{
			Assert("This BizO doesn't use temporary products.", !DocketLine.IsTemporaryProduct);
		}

		#endregion

		#region TestIsAvailable

		public void TestIsAvailable()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_CurrentInventoryStatus = CodeLists.InventoryStatus.Codes.Available;
			AssertEquals(true, docketLine.IsAvailable);
			docketLine.WE_CurrentInventoryStatus = CodeLists.InventoryStatus.Codes.Held;
			AssertEquals(false, docketLine.IsAvailable);
			docketLine.WE_CurrentInventoryStatus = CodeLists.InventoryStatus.Codes.Putaway;
			AssertEquals(false, docketLine.IsAvailable);
		}

		#endregion

		#region TestHasEDocsOrNotesAttached

		public void TestHasEDocsOrNotesAttached()
		{
			var line = GetNewBusinessObject();
			AssertEquals(false, line.HasEDocsOrNotesAttached);

			line.Notes.AddNew();
			AssertEquals(true, line.HasEDocsOrNotesAttached);

			line.Notes.RemoveAndDeleteAll();
			AssertEquals(false, line.HasEDocsOrNotesAttached);

			var storageMain = CreateStorageMain(line);
			AssertEquals(true, line.HasEDocsOrNotesAttached);

			line.Notes.AddNew();
			AssertEquals(true, line.HasEDocsOrNotesAttached);
		}

		BusinessObject CreateStorageMain(TDocketLine docketLine)
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var docFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(new BusinessObjectFactory());
			var storageMain = docFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));
			storageMain[StorageMainSchema.SM_ParentFK.Name] = docketLine.PK;
			docFactory.Save();

			return storageMain;
		}

		#endregion

		#endregion

		#region TestCountryCode

		public void TestCountryCode()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var docket = GetNewWhsDocket(Helper.CreateClient(), whs);
			var docketLine = GetNewBusinessObject(docket);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.Australia).RL_Code;
			AssertEquals(Core.Constants.CountryCodes.Australia, docketLine.CountryCode);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, docketLine.CountryCode);

			docketLine.WE_WD = ZGuid.Empty;
			AssertEquals(ZString.Empty, docketLine.CountryCode);
		}

		#endregion

		#region TestDocketOriginal

		public void TestDocketOriginal()
		{
			var docket1 = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket1);
			AssertEquals(docket1.PK, docketLine.DocketOriginal.PK);

			var docket2 = GetNewWhsDocket();
			var originalDocketLine = GetNewBusinessObject(docket2);
			docketLine.WE_WE_OriginalDocketLineForRating = originalDocketLine.PK;
			AssertNotEquals("Precondition", docket1, docket2);
			AssertEquals(docket2.PK, docketLine.DocketOriginal.PK);
		}

		#endregion

		#region TestHeldCodeToChangeTo

		[TestDate(2015, 1, 1, 11, 23, 37)]
		public void TestHeldCodeToChangeTo()
		{
			if (CanHaveInventoryAttached)
			{
				var abcCode = Helper.CreateInventoryHeldCode("ABC", "ABC");

				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.Docket.FinaliseDocket();
				AssertEquals(true, docketLine.IsFinalised);

				var origUnits = docketLine.WE_StockOnHand;
				Factory.Save();

				// assert no clone is created if all units are split
				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeChangeQuantity = origUnits;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals("Should be able to save via the form", true, docketLine.Inventory.HasChanges);
				Factory.Save();
				AssertEquals("Should have cleared properties used during Held Code Change.", 0m, docketLine.HeldCodeChangeQuantity);
				AssertEquals("Should have cleared properties used during Held Code Change.", string.Empty, docketLine.HeldCodeToChangeTo);

				AssertEquals(false, docketLine.HasChanges);
				AssertEquals(false, docketLine.Inventory[0].HasChanges);

				AssertEquals("Should not have created a second docket line.", 1, Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docketLine.Docket.PK)).Length);
				AssertEquals(InventoryStatus.Codes.Available, docketLine.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertEquals(string.Empty, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);

				var logQuery = new ZQuery(StmALogSchema.SL_Parent, docketLine.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "Status Changed from 'AVL' to 'HEL'|FDT=2015-01-01T11:23:37|NEW=DAM|TYP=Hold Code");
				AssertEquals(1, docketLine.Logs.Find(logQuery).Length);

				docketLine.HeldCodeToChangeTo = "ABC";
				Factory.Save();

				logQuery = new ZQuery(StmALogSchema.SL_Parent, docketLine.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "|FDT=2015-01-01T11:23:37|NEW=ABC|OLD=DAM|TYP=Hold Code");
				AssertEquals(1, docketLine.Logs.Find(logQuery).Length);

				// assert a clone is created if partial units are split
				docketLine.HeldCodeChangeQuantity = origUnits - 2m;
				docketLine.HeldCodeToChangeTo = InventoryStatus.Codes.Held;
				Factory.Save();
				AssertEquals("Should have cleared properties used during Held Code Change.", 0m, docketLine.HeldCodeChangeQuantity);
				AssertEquals("Should have cleared properties used during Held Code Change.", string.Empty, docketLine.HeldCodeToChangeTo);

				var originalDocketLine = Factory.LoadTop1<TDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 2m));
				var clonedDocketLine = Factory.LoadTop1<TDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, origUnits - 2m));

				AssertEquals(origUnits, originalDocketLine.WE_TransactionQuantity);
				AssertEquals(InventoryStatus.Codes.Held, originalDocketLine.WE_CurrentInventoryStatus);
				AssertEquals("", originalDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("ABC", originalDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(docketLine.PK, originalDocketLine.PK);
				AssertEquals(1, originalDocketLine.Logs.Find(logQuery).Length);

				AssertEquals(origUnits - 2m, clonedDocketLine.WE_TransactionQuantity);
				AssertEquals(InventoryStatus.Codes.Held, clonedDocketLine.WE_CurrentInventoryStatus);
				AssertEquals("ABC", clonedDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals(InventoryHoldCodes.Codes.Held, clonedDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertNotEquals(docketLine.PK, clonedDocketLine.PK);
				AssertEquals(false, clonedDocketLine.WE_IsOriginalInventory);

				logQuery = new ZQuery(StmALogSchema.SL_Parent, clonedDocketLine.PK);
				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, "|FDT=2015-01-01T11:23:37|NEW=HEL|OLD=ABC|TYP=Hold Code");
				AssertEquals(1, clonedDocketLine.Logs.Find(logQuery).Length);

				var newFactory = new BusinessObjectFactory();
				var reloadedDocket = Factory.Load<WhsDocket>(docketLine.Docket.PK);
				AssertEquals("New split docket lines should not be included in original docket lines", 1, reloadedDocket.Lines.Count);

				originalDocketLine.HeldCodeChangeQuantity = 0;
				AssertEquals(0m, originalDocketLine.HeldCodeChangeQuantity);
			}
			else
			{
				// cannot change hold code for non-inventory docket lines.
				Assert(true);
			}
		}

		#endregion

		#region TestHeldCodeToChangeTo_DoesNotFireMultipleTimes

		[TestDate(2015, 1, 1, 11, 23, 37)]
		public void TestHeldCodeToChangeTo_DoesNotFireMultipleTimes()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.Docket.FinaliseDocket();
				AssertEquals(true, docketLine.IsFinalised);
				Factory.Save();

				// assert a clone is created if partial units are split
				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeChangeQuantity = 1m;
				docketLine.HeldCodeToChangeTo = InventoryStatus.Codes.Held;
				Factory.Save();

				var originalDocketLine = Factory.LoadTop1<TDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 9m));
				var clonedDocketLine = Factory.LoadTop1<TDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 1m));

				AssertNotNull(originalDocketLine);
				AssertNotNull(clonedDocketLine);

				docketLine.Logs.AddNew();
				Factory.Save();
				AssertEquals("Should not have split again.", 9m, originalDocketLine.WE_StockOnHand);
			}
			else
			{
				// cannot change hold code for non-inventory docket lines.
				Assert(true);
			}
		}

		#endregion

		#region TestHeldCodeToChangeTo_DefaultsHoldReason

		public void TestHeldCodeToChangeTo_DefaultsHoldReason()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.Docket.FinaliseDocket();
				AssertEquals(true, docketLine.IsFinalised);
				Factory.Save();

				// assert a clone is created if partial units are split
				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeChangeQuantity = docketLine.WE_StockOnHand;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				AssertEquals("Hold Reason should be empty.", "", docketLine.HoldReasonToChangeTo);

				docketLine.HoldReasonToChangeTo = "Whatever";
				Factory.Save();

				AssertEquals("Precondition: Hold Reason set.", "Whatever", docketLine.WE_CurrentHoldReason);

				docketLine.HeldCodeToChangeTo = "";
				AssertEquals("Hold Reason to change to should not default when clearing Hold Code.", "", docketLine.HoldReasonToChangeTo);

				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals("Hold Reason to change to should be defaulted to current Hold Reason.", "Whatever", docketLine.HoldReasonToChangeTo);

				docketLine.HeldCodeToChangeTo = "";
				docketLine.HoldReasonToChangeTo = "I wanna be the very best";
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertEquals("Hold Reason to change to should not default if it already has a value.", "I wanna be the very best", docketLine.HoldReasonToChangeTo);
			}
			else
			{
				// cannot change hold code for non-inventory docket lines.
				Assert(true);
			}
		}

		#endregion

		#region TestWE_P9_TaskInfo

		public void TestWE_P9_TaskInfo()
		{
			var org = Helper.CreateClient();
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var docket = GetNewWhsDocket(org, null);
			var docketLine = GetNewBusinessObject(docket);
			AssertEquals("Should be readonly.", true, docketLine.WE_P9_TaskInfo.ReadOnly);
		}

		#endregion

		#region TestWE_OP

		public virtual void TestWE_OP()
		{
			var org = Helper.CreateClient();
			var part1 = Helper.CreateProduct(org, "P1");
			var part2 = Helper.CreateProduct(org, "P2");
			var docket = GetNewWhsDocket(org, null);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = part1.PK;
			AssertEquals(part1.PK, docketLine.WE_OP);

			// test SetLineDataBasedOnProduct()

			part2.OP_StockKeepingUnit = "KG";
			docketLine.WE_F3_NKPackType = "";
			docketLine.WE_OP = part2.PK;
			AssertEquals(part2.OP_StockKeepingUnit, docketLine.WE_F3_NKPackType);

			docketLine.WE_F3_NKPackType = "#!@"; // junk
			docketLine.WE_OP = part1.PK;
			AssertEquals("#!@", docketLine.WE_F3_NKPackType);

			SetLineAttributes(docketLine);
			docketLine.WE_OP = part2.PK;
			AssertLineAttributes(docketLine, ZDate.Empty, ZDate.Empty, "", "", "");

			org.MiscServ.OM_IMPartAttrib1Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMPartAttrib2Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMPartAttrib3Type = Enterprise.MasterFiles.Business.PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMUseExpiryDate = true;
			org.MiscServ.OM_IMUsePackingDate = true;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetProductAttributeUse(org, part1, attribNo, true);
			}

			SetLineAttributes(docketLine);
			docketLine.WE_OP = part1.PK;
			AssertLineAttributes(docketLine, ZDate.Today, ZDate.Today, "PA1", "PA2", "PA3");

			// assert ReadOnlyChange() is called

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			docketLine.WE_OP = part2.PK;
			AssertEquals(true, docketLine.WE_OPInfo.ReadOnly);
		}

		public void TestWE_OP_RecalculatesUnitsFromPackQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);

			var line = GetNewDocketLineReadyToFinalise(data);
			line.WE_OP = data.Part1.PK;
			line.WE_TransactionQuantity = 1m;
			line.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("Precondition.", 1m, line.WE_PackQuantity);
			AssertEquals("Precondition.", 5m, line.WE_TransactionQuantity);

			line.WE_OP = data.Part2.PK;
			AssertEquals("Should have retained PackQuantity.", 1m, line.WE_PackQuantity);
			AssertEquals("Should have recalculated Units.", 10m, line.WE_TransactionQuantity);
		}

		public void TestWE_OP_SetValueToInventory()
		{
			var docket = GetNewWhsDocket();
			TestSetValueToInventoryView(GetNewBusinessObject(docket), WhsDocketLineSchema.WE_OP.Name, WhsInventoryViewSchema.WI_OP.Name, ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());
		}

		public virtual void TestWE_OP_UpdateClearsSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
			Factory.Save();

			var docket = GetNewWhsDocket(data.Org1, null);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_SerialNumber = "SN1";

			docketLine.WE_OP = data.Part2.PK;
			AssertEquals("Serial number not cleared since new product uses serial number.", "SN1", docketLine.WE_SerialNumber);

			docketLine.WE_OP = part3.PK;
			AssertEquals("Serial number cleared since new product does not use serial number.", string.Empty, docketLine.WE_SerialNumber);
		}

		#endregion

		#region TestWE_F3_NKPackType

		public void TestWE_F3_NKPackType()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m
			Helper.CreateProductUnit(part, "BOX", 2m);

			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 10m;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, docketLine.WE_F3_NKPackType);
			AssertEquals("Precondition", 10m, docketLine.WE_PackQuantity);

			docketLine.WE_F3_NKPackType = "CTN";
			AssertEquals("CTN", docketLine.WE_F3_NKPackType);
			AssertEquals(120m, docketLine.WE_TransactionQuantity);

			docketLine.WE_F3_NKPackType = "BOX";
			AssertEquals("BOX", docketLine.WE_F3_NKPackType);
			AssertEquals(20m, docketLine.WE_TransactionQuantity);
		}

		public void TestWE_F3_NKPackType_SetValueToInventoryView()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", "", docketLine.WE_F3_NKPackType);

			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_F3_NKPackType.Name, WhsInventoryViewSchema.WI_F3_NKPackType.Name, (ZString)"BOX", (ZString)"BAG", (ZString)"PLT");
		}

		public void TestWE_F3_NKPackType_ReCalculateWE_TransactionQuantityOnlyWhenNecessary()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 24);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 65);

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			docketLine.WE_TransactionQuantity = 864m;
			AssertEquals("Precondition", 864m, docketLine.WE_TransactionQuantity);

			docketLine.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("Re-seting PackType to the same value should not have recalculated WE_TransactionQuantity.", 864m, docketLine.WE_TransactionQuantity);
		}

		#region TestWE_F3_NKPackType_UpdateProductUQForTemporaryProducts

		public void TestWE_F3_NKPackType_UpdateProductUQForTemporaryProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 24);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 65);

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);

			docketLine.WE_F3_NKPackType = "BOX";
			AssertEquals("BOX", docketLine.WE_F3_NKPackType);
			AssertEquals("UNT", docketLine.ProductUQ);

			docketLine.WE_OP = ZGuid.Invalid; // Temp Product
			docketLine.ProductCode = "TEMP";

			if (docketLine.IsTemporaryProduct)
			{
				docketLine.WE_F3_NKPackType = "PLT";
				AssertEquals("PLT", docketLine.WE_F3_NKPackType);
				AssertEquals("PLT", docketLine.ProductUQ);
			}
			else
			{
				Assert("docketLine does not support temporary products.", !(docketLine is ISupportTemporaryProduct));
			}
		}

		#endregion

		#endregion

		#region TestWE_PackQuantity

		#region TestWE_PackQuantity

		public virtual void TestWE_PackQuantity()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m
			Helper.CreateProductUnit(part, "BOX", 2m);

			var docketLine = GetNewBusinessObject();
			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 10m;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, docketLine.WE_F3_NKPackType);
			AssertEquals("Precondition", 10m, docketLine.WE_PackQuantity);

			docketLine.WE_PackQuantity = 25m;
			AssertEquals(25m, docketLine.WE_TransactionQuantity);

			docketLine.WE_F3_NKPackType = "CTN";
			AssertEquals(25m, docketLine.WE_PackQuantity);
			AssertEquals(300m, docketLine.WE_TransactionQuantity);

			docketLine.WE_PackQuantity = 15m;
			AssertEquals(180m, docketLine.WE_TransactionQuantity);

			docketLine.WE_F3_NKPackType = "BOX";
			AssertEquals(15m, docketLine.WE_PackQuantity);
			AssertEquals(30m, docketLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestWE_PackQuantity_WithEmptyPackType

		public virtual void TestWE_PackQuantity_WithEmptyPackType()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);

			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m

			docketLine.WE_OP = part.PK;
			part.OP_StockKeepingUnit = "";
			docketLine.WE_F3_NKPackType = "";
			docketLine.WE_PackQuantity = 10m;
			AssertEquals(10m, docketLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestWE_PackQuantity_GetsRecalcedOnLoad

		public virtual void TestWE_PackQuantity_GetsRecalcedOnLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 120m);

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_F3_NKPackType = "CTN";
			docketLine.WE_TransactionQuantity = 120m;
			docket.RunPreSaveValidation(); // to commit inventory by transfers and adjustments
			AssertEquals(10m, docketLine.WE_PackQuantity);
			Factory.Save();

			// Check that PackQuantity is calculated if there are unit conversions into PackType.
			var otherFactory = new BusinessObjectFactory();
			var docketLine2 = otherFactory.Load<WhsDocketLine>(docketLine.PK);
			AssertEquals(10m, docketLine2.WE_PackQuantity);

			// Check that PackQuantity is displayed if there is no unit conversions into PackType.
			docketLine.WE_F3_NKPackType = "PLT";
			docketLine.WE_TransactionQuantity = 50m;
			docket.RunPreSaveValidation(); // to commit inventory by transfers and adjustments
			Factory.Save();
			var otherFactory2 = new BusinessObjectFactory();
			var docketLine3 = otherFactory2.Load<WhsDocketLine>(docketLine.PK);
			AssertEquals(50m, docketLine3.WE_PackQuantity);
		}

		#endregion

		#region TestWE_PackQuantity_DecimalOverflowException

		public void TestWE_PackQuantity_DecimalOverflowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Container, 1000m);

			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_F3_NKPackType = Constants.PkgUnit.Container;
			docketLine.WE_TransactionQuantity = 1m;
			AssertEquals("Precondition", 1m, docketLine.WE_TransactionQuantity);

			AssertNoExceptionThrown(() => docketLine.WE_PackQuantity = decimal.MaxValue);
			AssertEquals("If WE_TransactionQuantity cannot be calculated, it should be set to same value as WE_PackQuantity.", decimal.MaxValue, docketLine.WE_TransactionQuantity);
			AssertHasRowError(docketLine, "Attempt to overflow capacity of Quantity. Please validate your setup and restart the process.");
		}

		#endregion

		#region TestClearPackQuantity

		public void TestClearPackQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 2m;
			AssertEquals(2m, docketLine.WE_PackQuantity);

			((IBusinessObjectInternals)docketLine).Row[WhsDocketLineSchema.Constants.WE_TransactionQuantity] = 1m;
			AssertEquals(2m, docketLine.WE_PackQuantity);

			docketLine.ClearPackQuantity();
			AssertEquals(1m, docketLine.WE_PackQuantity);
		}

		#endregion

		#endregion

		#region TestWE_TransactionQuantity

		#region TestWE_TransactionQuantity

		public void TestWE_TransactionQuantity()
		{
			DocketLine = GetNewBusinessObject(Docket);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m
			Helper.CreateProductUnit(part, "BOX", 2m);

			DocketLine.WE_OP = part.PK;
			DocketLine.WE_TransactionQuantity = 10m;
			AssertEquals("Precondition", part.OP_StockKeepingUnit, DocketLine.WE_F3_NKPackType);
			AssertEquals("Precondition", 10m, DocketLine.WE_PackQuantity);

			DocketLine.WE_TransactionQuantity = 25m;
			AssertEquals(25m, DocketLine.WE_PackQuantity);

			DocketLine.WE_F3_NKPackType = "CTN";
			AssertEquals(25m, DocketLine.WE_PackQuantity);
			AssertEquals(300m, DocketLine.WE_TransactionQuantity);

			DocketLine.WE_TransactionQuantity = 24m;
			AssertEquals(2m, DocketLine.WE_PackQuantity);

			DocketLine.WE_F3_NKPackType = "BOX";
			AssertEquals(2m, DocketLine.WE_PackQuantity);
			AssertEquals(4m, DocketLine.WE_TransactionQuantity);

			DocketLine.WE_TransactionQuantity = 30m;
			AssertEquals(15m, DocketLine.WE_PackQuantity);
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalLineUnits

		public void TestWE_TransactionQuantity_UpdatesTotalLineUnits()
		{
			if (SettingWE_TransactionQuantityUpdatesTotalLineUnits)
			{
				OrgHeader client = Helper.CreateClient();
				OrgSupplierPart part = Helper.CreateProduct(client, "P1");

				AssertEquals("Precondition", 0m, Docket.WD_TotalUnitsFromLines);

				DocketLine.WE_OP = part.PK;
				DocketLine.WE_TransactionQuantity = 10m;
				AssertEquals("Total Line Units is incorrect", 10m, Docket.WD_TotalUnitsFromLines);
			}
			else
			{
				Assert("Action is not supported.", true);
			}
		}

		protected virtual bool SettingWE_TransactionQuantityUpdatesTotalLineUnits
		{
			get { return true; }
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume()
		{
			if (SettingWE_TransactionQuantityUpdatesTotalWeightAndVolume)
			{
				TestWE_TransactionQuantity_UpdatesTotalWeightAndVolumeCore();
			}
			else
			{
				Assert("Action is not supported.", true);
			}
		}

		protected virtual void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolumeCore()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");

			var docket = GetNewWhsDocket();
			docket.WD_TotalCubicUnit = Constants.Volume.Litre;
			docket.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition:", 0m, docket.WD_TotalWeight);
			AssertEquals("Precondition:", 0m, docket.WD_TotalCubic);

			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 10m;

			AssertEquals("Total Weight is incorrect", 44.09m, docket.WD_TotalWeight);
			AssertEquals("Total Volume is incorrect", 200m, docket.WD_TotalCubic);
		}

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCC()
		{
			if (SettingWE_TransactionQuantityUpdatesTotalWeightAndVolume)
			{
				TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore();
			}
			else
			{
				Assert("Action is not supported.", true);
			}
		}

		protected virtual void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Helper.SetProductWeightAndVolume(part, 1m, Constants.Weight.Kilograms, 2000m, Constants.Volume.CubicCentimeters);

			var docket = GetNewWhsDocket();
			docket.WD_TotalCubicUnit = Constants.Volume.Litre;
			docket.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition:", 0m, docket.WD_TotalWeight);
			AssertEquals("Precondition:", 0m, docket.WD_TotalCubic);

			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 10m;

			AssertEquals("Total Weight is incorrect", 22.05m, docket.WD_TotalWeight);
			AssertEquals("Total Volume is incorrect", 20m, docket.WD_TotalCubic);
		}

		protected virtual bool SettingWE_TransactionQuantityUpdatesTotalWeightAndVolume => true;

		#endregion

		#region TestWE_TransactionQuantity_WithEmptyPackType

		public virtual void TestWE_TransactionQuantity_WithEmptyPackType()
		{
			DocketLine = GetNewBusinessObject(Docket);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m

			DocketLine.WE_OP = part.PK;
			part.OP_StockKeepingUnit = "";
			DocketLine.WE_F3_NKPackType = "";
			DocketLine.WE_TransactionQuantity = 10m;
			AssertEquals(10m, DocketLine.WE_PackQuantity);
		}

		#endregion

		#region TestOnSave_SetLocationsLastAllocatedOrChangedDate

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 12, 12)]
		public void TestOnSave_SetLocationsLastAllocatedOrChangedDate()
		{
			if (CanHaveInventoryAttached)
			{
				TestDateAttribute.UseUNLOCO = true;

				var now = ZDateTime.UtcNow;
				var data = new TestDataSimpleEnvironment(Factory);
				var whs = data.Whs1;
				var location = Helper.CreateRowAndGenerateLocations(whs, "location").Locations[0];
				Factory.Save();

				var docketLine = GetNewDocketLineReadyToFinalise(data);
				Factory.Save();

				AssertEquals("Last allocated or changed date should begin empty.", ZDateTime.Empty, location.WLV_LastAllocatedOrChangedDateUtc);

				docketLine.WE_WL = location.PK;
				Factory.Save();

				AssertEquals("Change to a WE_WL should update new locations WLV_LastAllocatedOrChangedDateUtc.", now, location.WLV_LastAllocatedOrChangedDateUtc);

				TestDateAttribute.Date = now.AddDays(1).ToDateTime();
				docketLine.WE_PartAttrib1 = "new Attrib";
				Factory.Save();

				AssertEquals("Change to a WE_WL should update locations WLV_LastAllocatedOrChangedDateUtc.", now.AddDays(1), location.WLV_LastAllocatedOrChangedDateUtc);

				TestDateAttribute.Date = now.AddDays(2).ToDateTime();
				docketLine.WE_CustomDate1 = now;
				Factory.Save();

				AssertEquals("Change to a WE_WL should update locations WLV_LastAllocatedOrChangedDateUtc.", now.AddDays(2), location.WLV_LastAllocatedOrChangedDateUtc);
			}
			else
			{
				Assert(true);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 12, 12)]
		public void TestOnSave_SetLocationsLastAllocatedOrChangedDate_UpdatesPreviousLocation()
		{
			if (CanHaveInventoryAttached)
			{
				TestDateAttribute.UseUNLOCO = true;

				var now = ZDateTime.UtcNow;
				var data = new TestDataSimpleEnvironment(Factory);
				var whs = data.Whs1;
				var location1 = Helper.CreateRowAndGenerateLocations(whs, "location1").Locations[0];
				var location2 = Helper.CreateRowAndGenerateLocations(whs, "location2").Locations[0];
				Factory.Save();

				var docketLine = GetNewDocketLineReadyToFinalise(data);
				Factory.Save();

				AssertEquals("Last allocated or changed date should begin empty.", ZDateTime.Empty, location1.WLV_LastAllocatedOrChangedDateUtc);

				docketLine.WE_WL = location1.PK;
				Factory.Save();

				AssertEquals("Change to a WE_WL should update new locations WLV_LastAllocatedOrChangedDateUtc.", now, location1.WLV_LastAllocatedOrChangedDateUtc);

				TestDateAttribute.Date = now.AddDays(1).ToDateTime();
				docketLine.WE_WL = location2.PK;
				Factory.Save();

				AssertEquals("Change to a WE_WL should update old locations WLV_LastAllocatedOrChangedDateUtc.", now.AddDays(1), location1.WLV_LastAllocatedOrChangedDateUtc);
				AssertEquals("Change to a WE_WL should update new locations WLV_LastAllocatedOrChangedDateUtc.", now.AddDays(1), location2.WLV_LastAllocatedOrChangedDateUtc);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2018, 12, 12)]
		public void TestOnSave_FinalisedDontSetLocationsLastAllocatedOrChangedDate()
		{
			if (CanHaveInventoryAttached)
			{
				var now = ZDateTime.Now;
				var data = new TestDataSimpleEnvironment(Factory);
				var whs = data.Whs1;
				var location = Helper.CreateRowAndGenerateLocations(whs, "location").Locations[0];
				Factory.Save();

				var docketLine = GetNewDocketLineReadyToFinalise(data);
				Factory.Save();

				AssertEquals("Last allocated or changed date should begin empty.", ZDateTime.Empty, location.WLV_LastAllocatedOrChangedDateUtc);

				docketLine.WE_WL = location.PK;
				Factory.Save();

				AssertEquals("Change to a WE_WL should update new locations WLV_LastAllocatedOrChangedDateUtc.", now, location.WLV_LastAllocatedOrChangedDateUtc);

				TestDateAttribute.Date = now.AddDays(1).ToDateTime();
				docketLine.Docket.FinaliseDocket();
				docketLine.WE_CustomDate1 = now;
				Factory.Save();

				AssertEquals("Change to a WE_WL when finalised should not update a locations WLV_LastAllocatedOrChangedDateUtc.", now, location.WLV_LastAllocatedOrChangedDateUtc);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestWE_TransactionQuantity_WithLargeUnitConversion

		public void TestWE_TransactionQuantity_WithLargeUnitConversion()
		{
			// Refer to WI00080290
			// The important part is that we don't default WE_TransactionQuantity in this case
			// I have retained the behaviour we use on WhsDocketLine (current ALP-GP1 behaviour) which is different to DRD's original fix
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "BOX", 10000);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			line.WE_F3_NKPackType = "BOX";
			line.WE_TransactionQuantity = 5m;
			AssertEquals(0m, line.WE_PackQuantity);

			line.WE_TransactionQuantity = 600m;
			AssertEquals(0.06m, line.WE_PackQuantity);

			line.WE_TransactionQuantity = 0m;
			AssertEquals(0m, line.WE_PackQuantity);
		}

		#endregion

		#region TestWE_TransactionQuantity_DecimalOverflowException

		public void TestWE_TransactionQuantity_DecimalOverflowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Piece, 0.001m);

			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_F3_NKPackType = Constants.PkgUnit.Piece;
			docketLine.WE_TransactionQuantity = 1m;
			AssertEquals("Precondition", 1000m, docketLine.WE_PackQuantity);

			AssertNoExceptionThrown(() => docketLine.WE_TransactionQuantity = decimal.MaxValue);
			AssertEquals("If WE_PackQuantity cannot be calculated, it should be set to same value as WE_TransactionQuantity.", decimal.MaxValue, docketLine.WE_PackQuantity);
			AssertHasRowError(docketLine, "Attempt to overflow capacity of Pack Quantity. Please validate your setup and restart the process.");
		}

		#endregion

		#endregion

		#region TestWE_StockOnHand_SetValueToInventory

		public void TestWE_StockOnHand_SetValueToInventory()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", 0m, docketLine.WE_StockOnHand);

			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_StockOnHand.Name, WhsInventoryViewSchema.WI_TotalUnits.Name, (ZDecimal)2m, (ZDecimal)5m, (ZDecimal)9m);
		}

		#endregion

		#region TestWE_WHC_NKOriginalInventoryHeldCode

		public void TestWE_WHC_NKOriginalInventoryHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);

			docketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals(ShouldSettingHeldCodeChangeStatusBeforeFinalised ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Pending, docketLine.WE_OriginalInventoryStatus);
			AssertEquals(ShouldSettingHeldCodeChangeStatusBeforeFinalised ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Pending, docketLine.WE_CurrentInventoryStatus);

			docketLine.WE_WHC_NKOriginalInventoryHeldCode = "";
			AssertEquals(ShouldSettingHeldCodeChangeStatusBeforeFinalised ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Pending, docketLine.WE_OriginalInventoryStatus);
			AssertEquals(ShouldSettingHeldCodeChangeStatusBeforeFinalised ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Pending, docketLine.WE_CurrentInventoryStatus);
		}

		protected virtual bool ShouldSettingHeldCodeChangeStatusBeforeFinalised
		{
			get { return true; }
		}

		#endregion

		#region TestWE_WHC_NKCurrentInventoryHeldCode

		public void TestWE_WHC_NKCurrentInventoryHeldCode_SetsPreviousHeldCodeForCycleCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);

			if (docketLine.IsInventoryLine)
			{
				docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Setting WE_WHC_NKCurrentInventoryHeldCode to DAM should not update cache.", string.Empty, docketLine.PreviousHeldCodeForCycleCount);

				docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.LostInCycleCount;
				AssertEquals(InventoryHoldCodes.Codes.LostInCycleCount, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Setting WE_WHC_NKCurrentInventoryHeldCode to LCC should update cache.", InventoryHoldCodes.Codes.Damaged, docketLine.PreviousHeldCodeForCycleCount);

				docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.LostInCycleCount;
				AssertEquals("Setting WE_WHC_NKCurrentInventoryHeldCode to LCC multiple times should not update cache.", InventoryHoldCodes.Codes.Damaged, docketLine.PreviousHeldCodeForCycleCount);

				docketLine.WE_WHC_NKCurrentInventoryHeldCode = string.Empty;
				AssertEquals(string.Empty, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Setting inventory status to Available should not update cache.", InventoryHoldCodes.Codes.Damaged, docketLine.PreviousHeldCodeForCycleCount);
			}
			else
			{
				docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(InventoryHoldCodes.Codes.Damaged, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Should never update cache.", string.Empty, docketLine.PreviousHeldCodeForCycleCount);

				docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.LostInCycleCount;
				AssertEquals(InventoryHoldCodes.Codes.LostInCycleCount, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Should never update cache.", string.Empty, docketLine.PreviousHeldCodeForCycleCount);
			}
		}

		#endregion

		#region TestWE_WHC_NKOrderedHeldCode

		public void TestWE_WHC_NKOrderedHeldCode_ReadOnly()
		{
			AssertEquals(DocketLine.WE_WHC_NKOrderedHeldCodeInfo.ReadOnly, true);
		}

		#endregion

		#region TestIsBOMProduct

		public virtual void TestIsBOMProduct()
		{
			AssertEquals(false, DocketLine.IsBOMProduct);

			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart part1 = Helper.CreateProduct(org, "P1");

			DocketLine.WE_OP = part1.PK;
			AssertEquals(false, DocketLine.IsBOMProduct);

			OrgSupplierPart part2 = Helper.CreateProduct(org, "P2");
			Helper.CreateProductBOM(part1, part2);
			AssertEquals(true, DocketLine.IsBOMProduct);

			DocketLine.WE_OP = part2.PK;
			AssertEquals(false, DocketLine.IsBOMProduct);
		}

		#endregion

		#region TestIsBOMProductInfo

		public void TestIsBOMProductInfo()
		{
			AssertEquals(WhsDocketLine.Schema.IsBOMProduct, DocketLine.IsBOMProductInfo.Name);
		}

		#endregion

		#region TestIsDocketCreatedFromPickByBOM

		public void TestIsCreatedFromPickByBOM() => TestIsCreatedFromPickByBOMCore();

		protected virtual void TestIsCreatedFromPickByBOMCore() => AssertEquals(false, DocketLine.IsCreatedFromPickByBOM);

		#endregion

		#region TestQuantityNotMet

		public void TestQuantityNotMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket(data.Org1, data.Whs1));
			SetSumOfUnitsMet(docketLine, 10m);

			docketLine.WE_TransactionQuantity = 25m;
			bool isSumOfUnitsZero = docketLine.SumOfUnitsMet == 0m;
			AssertEquals("QuantityNotMet should be Units minus Units Met.", isSumOfUnitsZero ? 25m : 15m, docketLine.QuantityNotMet);
		}

		#endregion

		#region TestSumOfUnitsMet

		public void TestSumOfUnitsMet()
		{
			TestSumOfUnitsMetCore();
		}

		protected virtual void TestSumOfUnitsMetCore()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			SetSumOfUnitsMet(docketLine, 15.5m);
			docketLine.Docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(15.5m, docketLine.SumOfUnitsMet);
		}

		#endregion

		#region TestWE_WD

		#region TestWE_WD_ModifyingMaxLineNo

		public void TestWE_WD_ModifyingMaxLineNo()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject();
			docket.Lines.Add(docketLine);
			AssertEquals("Precondition:", (ZShort)1, docket.MaxLineNoManager.CurrentMaxLineNo);

			var docketLine1 = GetNewBusinessObject();
			docketLine1.WE_WD = docket.PK;
			AssertEquals("When added DocketLine with LineNo == 0, then it should be set to CurrentMaxLineNo + 1", (ZShort)2, docketLine1.WE_LineNo);
			AssertEquals("When added DocketLine with LineNo == 0, CurrentMaxLineNo should be modified", (ZShort)2, docket.MaxLineNoManager.CurrentMaxLineNo);

			var docketLine2 = GetNewBusinessObject();
			docketLine2.WE_LineNo = 5;
			docketLine2.WE_WD = docket.PK;
			AssertEquals("When added DocketLine with LineNo != 0, then it's LineNo should stay the same.", (ZShort)5, docketLine2.WE_LineNo);
			AssertEquals("When added DocketLine with LineNo != 0, CurrentMaxLineNo should be modified", (ZShort)5, docket.MaxLineNoManager.CurrentMaxLineNo);

			var docket2 = GetNewWhsDocket();
			AssertEquals("Precondition:", (ZShort)0, docket2.MaxLineNoManager.CurrentMaxLineNo);

			docketLine2.WE_WD = docket2.PK;
			AssertEquals("When DocketLine change parent, the old Parent CurrentMaxLineNo should be modified", (ZShort)2, docket.MaxLineNoManager.CurrentMaxLineNo);
			AssertEquals("When DocketLine change parent, the new Parent CurrentMaxLineNo should be modified", (ZShort)5, docket2.MaxLineNoManager.CurrentMaxLineNo);
		}

		public void TestWE_WD_ModifyingMaxLineNo_ArithmeticOverflow()
		{
			var docket = GetNewWhsDocket();
			var docketLine1 = docket.Lines.AddNew();
			docketLine1.WE_LineNo = short.MaxValue;

			AssertNoExceptionThrown("Line number should not go over maximum value.", () =>
			{
				var docketLine2 = docket.Lines.AddNew();
				AssertEquals("When line number cannot be assigned it should be left as 0.", ZShort.Zero, docketLine2.WE_LineNo);
			});
		}

		#endregion

		#region TestWE_WD_UpdatingTotals

		public void TestWE_WD_UpdatingTotals()
		{
			TestWE_WD_UpdatingTotalsCore();
		}

		protected virtual void TestWE_WD_UpdatingTotalsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.02m, "M3");

			var docket1 = GetNewWhsDocket(data.Org1, data.Whs1);
			var docket2 = GetNewWhsDocket(data.Org1, data.Whs1);
			if (docket1.ShouldUpdateWeightAndVolumeOnTheFly && !(docket1 is WhsReceive)) // For Receive Totals are calculated when Inventory Collection Changed rather that for DocketLine.WE_WD
			{
				CreateWhsDocketLine(docket1, data.Part1, 10m);
				CreateWhsDocketLine(docket2, data.Part1, 10m);
				var migratingDocketLine = CreateWhsDocketLine(docket1, data.Part1, 10m);
				docket1.WD_TotalWeight = 70m;
				docket1.WD_TotalCubic = 0.5m;
				docket2.WD_TotalWeight = 60m;
				docket2.WD_TotalCubic = 0.3m;
				AssertEquals("Precondition - ensure Total Weight is correct", 70m, docket1.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Cubic is correct", 0.5m, docket1.WD_TotalCubic);
				AssertEquals("Precondition - ensure Total Line Units is correct", 20m, docket1.WD_TotalUnitsFromLines);

				AssertEquals("Precondition - ensure Total Weight is correct", 60m, docket2.WD_TotalWeight);
				AssertEquals("Precondition - ensure Total Cubic is correct", 0.3m, docket2.WD_TotalCubic);
				AssertEquals("Precondition - ensure Total Line Units is correct", 10m, docket2.WD_TotalUnitsFromLines);

				migratingDocketLine.WE_WD = docket2.PK;
				AssertEquals("Total Weight should be updated, but not recalculated.", 50m, docket1.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated, but not recalculated.", 0.3m, docket1.WD_TotalCubic);
				AssertEquals("Total Line Units should be updated.", 10m, docket1.WD_TotalUnitsFromLines);

				AssertEquals("Total Weight should be updated.", 80m, docket2.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated.", 0.5m, docket2.WD_TotalCubic);
				AssertEquals("Total Line Units should be updated.", 20m, docket2.WD_TotalUnitsFromLines);
			}
			else
			{
				Assert("Update Totals is not supported, no need to test it.", true);
			}
		}

		#endregion

		#region TestWE_WD_SetValueToInventory

		public void TestWE_WD_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_WD.Name, WhsInventoryViewSchema.WI_WD.Name, ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());
		}

		#endregion

		#endregion

		#region TestWE_LineNo

		public void TestWE_LineNo()
		{
			Docket.Lines.Add(DocketLine);
			Docket.Lines.AddNew();

			AssertEquals("Precondition:", (ZShort)1, DocketLine.WE_LineNo);
			AssertEquals("Precondition:", (ZShort)2, Docket.MaxLineNoManager.CurrentMaxLineNo);

			DocketLine.WE_LineNo = 5;
			AssertEquals((ZShort)5, DocketLine.WE_LineNo);
			AssertEquals("If new LineNo is greater that CurrentMaxLineNo, then it should be updated", (ZShort)5, Docket.MaxLineNoManager.CurrentMaxLineNo);

			DocketLine.WE_LineNo = 1;
			AssertEquals((ZShort)1, DocketLine.WE_LineNo);
			AssertEquals("If old LineNo was CurrentMaxLineNo, then it should be updated", (ZShort)2, Docket.MaxLineNoManager.CurrentMaxLineNo);
		}

		#endregion

		#region TestWE_ClientUQ

		public void TestWE_ClientUQ()
		{
			AssertEquals("Precondition", "UNT", DocketLine.WE_ClientUQ);

			OrgHeader org = Helper.CreateClient();
			DocketLine.Docket.WD_OH_Client = org.PK;

			OrgSupplierPart part = Helper.CreateProduct(org, "P1");
			DocketLine.WE_OP = part.PK;
			part.OP_StockKeepingUnit = "MAA";
			AssertEquals("MAA", DocketLine.WE_ClientUQ);

			part.RelatedOrganisations.FindByOrganisationAndRelationship(org, OrgPartRelation.RelationshipTypes.Owner).OU_ClientUQ = "BAA";
			AssertEquals("BAA", DocketLine.WE_ClientUQ);
		}

		#endregion

		#region TestWE_PackQuantity_TemporaryProduct_SetsWE_TransactionQuantity

		public void TestWE_PackQuantity_TemporaryProduct_SetsWE_TransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = ZGuid.Empty;
			docketLine.ProductCode = "TESTPRODUCT1";

			if (docketLine.IsTemporaryProduct)
			{
				docketLine.WE_PackQuantity = 110m;
				AssertEquals(110m, docketLine.WE_TransactionQuantity);
			}
			else
			{
				Assert("docketLine does not support temporary products.", !(docketLine is ISupportTemporaryProduct));
			}
		}

		#endregion

		#region TestWE_RequiredByDate

		public void TestWE_RequiredByDate()
		{
			AssertEquals("Precondition: ", ZDateTimeOffset.Empty, DocketLine.WE_RequiredByDate);

			DocketLine.WE_RequiredByDate = ZDateTimeOffset.Today;
			AssertEquals(ZDateTimeOffset.Today, DocketLine.WE_RequiredByDate);
		}

		#endregion

		#region Attributes

		#region TestWE_ExpiryDate

		public void TestWE_ExpiryDate()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_ExpiryDate.Name, WhsInventoryViewSchema.WI_ExpiryDate.Name, ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(9));
		}

		#endregion

		#region TestWE_PackingDate

		public void TestWE_PackingDate()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_PackingDate.Name, WhsInventoryViewSchema.WI_PackingDate.Name, ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(9));
		}

		public void TestPackingDateCalculatesExpiryDate()
		{
			if (TestDocketLineCanCalculateExpiryDateFromPackingDate)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

				var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
				productParam.W3_MaximumShelfLife = 10;
				Factory.Save();

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = docket.Lines.AddNew();
				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;

				AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, docketLine.WE_PackingDate);
				AssertEquals("Precondition: Packing date is not readonly.", false, docketLine.WE_PackingDateInfo.ReadOnly);
				AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, docketLine.WE_ExpiryDate);
				AssertEquals("Precondition: Expiry date is not readonly.", false, docketLine.WE_ExpiryDateInfo.ReadOnly);

				docketLine.WE_PackingDate = new ZDate(2020, 04, 15);
				AssertEquals("Expiry date is calculated from packing date and shelf life.", new ZDate(2020, 04, 25), docketLine.WE_ExpiryDate);
			}
			else
			{
				Assert("Docket does not calculate expiry date from packing date.", true);
			}
		}

		public void TestPackingDateCalculatesExpiryDate_NoProductParam()
		{
			if (TestDocketLineCanCalculateExpiryDateFromPackingDate)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
				Factory.Save();

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = docket.Lines.AddNew();
				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;

				AssertEquals("Precondition: product has no product param.", null, docketLine.Product.GetParamsByWhsAndClient(data.Whs1, data.Org1));
				AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, docketLine.WE_PackingDate);
				AssertEquals("Precondition: Packing date is not readonly.", false, docketLine.WE_PackingDateInfo.ReadOnly);
				AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, docketLine.WE_ExpiryDate);
				AssertEquals("Precondition: Expiry date is not readonly.", false, docketLine.WE_ExpiryDateInfo.ReadOnly);

				docketLine.WE_PackingDate = new ZDate(2020, 04, 15);
				AssertEquals("Expiry date is not calculated from packing date and shelf life.", ZDate.Empty, docketLine.WE_ExpiryDate);
			}
			else
			{
				Assert("Docket does not calculate expiry date from packing date.", true);
			}
		}

		public void TestPackingDateCalculatesExpiryDate_NoProductShelfLife()
		{
			if (TestDocketLineCanCalculateExpiryDateFromPackingDate)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

				var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
				Factory.Save();

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = docket.Lines.AddNew();
				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;

				AssertEquals("Precondition: product has no shelf life.", ZShort.Zero, productParam.W3_MaximumShelfLife);
				AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, docketLine.WE_PackingDate);
				AssertEquals("Precondition: Packing date is not readonly.", false, docketLine.WE_PackingDateInfo.ReadOnly);
				AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, docketLine.WE_ExpiryDate);
				AssertEquals("Precondition: Expiry date is not readonly.", false, docketLine.WE_ExpiryDateInfo.ReadOnly);

				docketLine.WE_PackingDate = new ZDate(2020, 04, 15);
				AssertEquals("Expiry date is not calculated from packing date and shelf life.", ZDate.Empty, docketLine.WE_ExpiryDate);
			}
			else
			{
				Assert("Docket does not calculate expiry date from packing date.", true);
			}
		}

		public void TestPackingDateCalculatesExpiryDate_ProductWithPackingDateOnly()
		{
			if (TestDocketLineCanCalculateExpiryDateFromPackingDate)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

				var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
				productParam.W3_MaximumShelfLife = 10;
				Factory.Save();

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = docket.Lines.AddNew();
				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;

				AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, docketLine.WE_PackingDate);
				AssertEquals("Precondition: Packing date is not readonly.", false, docketLine.WE_PackingDateInfo.ReadOnly);
				AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, docketLine.WE_ExpiryDate);
				AssertEquals("Precondition: Expiry date is readonly.", true, docketLine.WE_ExpiryDateInfo.ReadOnly);

				docketLine.WE_PackingDate = new ZDate(2020, 04, 15);
				AssertEquals("Expiry date is not calculated from packing date and shelf life.", ZDate.Empty, docketLine.WE_ExpiryDate);
			}
			else
			{
				Assert("Docket does not calculate expiry date from packing date.", true);
			}
		}

		public void TestPackingDateCalculatesExpiryDate_ExpiryDateNotEmpty()
		{
			if (TestDocketLineCanCalculateExpiryDateFromPackingDate)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

				var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
				productParam.W3_MaximumShelfLife = 10;
				Factory.Save();

				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = docket.Lines.AddNew();
				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;

				AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, docketLine.WE_PackingDate);
				AssertEquals("Precondition: Packing date is not readonly.", false, docketLine.WE_PackingDateInfo.ReadOnly);
				AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, docketLine.WE_ExpiryDate);
				AssertEquals("Precondition: Expiry date is not readonly.", false, docketLine.WE_ExpiryDateInfo.ReadOnly);

				docketLine.WE_ExpiryDate = new ZDate(2020, 04, 20);
				docketLine.WE_PackingDate = new ZDate(2020, 04, 15);
				AssertEquals("Expiry date is not calculated from packing date and shelf life.", new ZDate(2020, 04, 20), docketLine.WE_ExpiryDate);
			}
			else
			{
				Assert("Docket does not calculate expiry date from packing date.", true);
			}
		}

		protected virtual bool TestDocketLineCanCalculateExpiryDateFromPackingDate => false;

		#endregion

		#region TestWE_PartAttrib1

		[TestDate(2013, 2, 26)]
		public void TestWE_PartAttrib1_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsed()
		{
			TestWE_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsDocketLineSchema.WE_PartAttrib1, AttributeNumber.One);
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_PartAttrib1.Name, WhsInventoryViewSchema.WI_PartAttrib1.Name, (ZString)"A1", (ZString)"A2", (ZString)"A3");
		}

		#endregion

		#region TestWE_PartAttrib2

		[TestDate(2013, 2, 26)]
		public void TestWE_PartAttrib2_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsed()
		{
			TestWE_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsDocketLineSchema.WE_PartAttrib2, AttributeNumber.Two);
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_PartAttrib2.Name, WhsInventoryViewSchema.WI_PartAttrib2.Name, (ZString)"A1", (ZString)"A2", (ZString)"A3");
		}

		#endregion

		#region TestWE_PartAttrib3

		[TestDate(2013, 2, 26)]
		public void TestWE_PartAttrib3_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsed()
		{
			TestWE_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(WhsDocketLineSchema.WE_PartAttrib3, AttributeNumber.Three);
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_PartAttrib3.Name, WhsInventoryViewSchema.WI_PartAttrib3.Name, (ZString)"A1", (ZString)"A2", (ZString)"A3");
		}

		#endregion

		#region TestWE_SerialNumber

		public void TestWE_SerialNumber_SetInventoryValue()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_SerialNumber.Name, WhsInventoryViewSchema.WI_SerialNumber.Name, (ZString)"A1", (ZString)"A2", (ZString)"A3");
		}

		public void TestSerialNumberReadOnly_NoDocket()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WD = ZGuid.Empty;

			Assert(docketLine.WE_WD.IsEmpty);
			Assert(docketLine.WE_SerialNumberInfo.ReadOnly);
		}

		public void TestSerialNumberReadOnly_NoProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();

			Assert(docketLine.WE_OP.IsEmpty);
			Assert(docketLine.WE_SerialNumberInfo.ReadOnly);
		}

		public void TestSerialNumberReadOnly_AfterPickProductAndAttribsReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_UseSerialNumber = true;

			TestAfterPickProductAndAttribsReadOnly(docketLine.WE_SerialNumberInfo);
		}

		public void TestSerialNumberReadOnly_SerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestSerialNumberReadOnly_SerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestSerialNumberReadOnly_ClientDoesNotUseSerialNumber()
		{
			TestSerialNumberReadOnly_SerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestSerialNumberReadOnly_SerialNumberCore(bool clientUsesSerialNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			data.Org1.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			data.Part1.RelatedOrganisations[0].OU_UseSerialNumber = clientUsesSerialNumber;

			AssertEquals(!clientUsesSerialNumber, docketLine.WE_SerialNumberInfo.ReadOnly);
		}

		#endregion

		#region TestWE_PartAttrib_SetExpiryDateIfJulianBatchNumberIsUsedCore

		void TestWE_PartAttrib_SetExpiryDateAndPackingDateIfJulianBatchNumberIsUsedCore(SchemaStringColumn partAttributeColumn, AttributeNumber attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;

			// Normal Attribute
			AssertEquals("Precondition", ZDateTime.Empty, docketLine.WE_ExpiryDate);

			docketLine[partAttributeColumn] = "ABC1005";
			AssertEquals("Expiry date should not be modified when normal attribute is set.", ZDateTime.Empty, docketLine.WE_ExpiryDate);

			// Julian Batch Number Attribute
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber); // should turn on Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true); // should turn on Expiry Date
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;

			docketLine[partAttributeColumn] = "";
			AssertEquals("Expiry date should be empty when Julian Batch Number is not set.", ZDateTime.Empty, docketLine.WE_ExpiryDate);
			AssertEquals("Packing date should be empty when Julian Batch Number is not set.", ZDateTime.Empty, docketLine.WE_PackingDate);

			docketLine[partAttributeColumn] = "ABC1005";
			AssertEquals("Expiry date should be set when Julian Batch Number is set.", new ZDateTime(2010 + 1, 1, 1).AddDays((005 - 1) + 10), docketLine.WE_ExpiryDate);
			AssertEquals("Packing date should be set when Julian Batch Number is set.", new ZDateTime(2010 + 1, 1, 1).AddDays((005 - 1)), docketLine.WE_PackingDate);

			docketLine[partAttributeColumn] = "ABC4050";
			AssertEquals("Expiry date should be set when Julian Batch Number is set.", new ZDateTime(2000 + 4, 1, 1).AddDays((050 - 1) + 10), docketLine.WE_ExpiryDate);
			AssertEquals("Packing date should be set when Julian Batch Number is set.", new ZDateTime(2000 + 4, 1, 1).AddDays((050 - 1)), docketLine.WE_PackingDate);

			docketLine[partAttributeColumn] = "1005ABC";
			AssertEquals("Expiry date should be empty when Julian Batch Number is incorrect.", ZDateTime.Empty, docketLine.WE_ExpiryDate);
			AssertEquals("Packing date should be empty when Julian Batch Number is incorrect.", ZDateTime.Empty, docketLine.WE_PackingDate);
		}

		#endregion

		#endregion

		#region TestWE_PalletID

		public void TestWE_PalletID()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", "", docketLine.WE_PalletID);

			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_PalletID.Name, WhsInventoryViewSchema.WI_PalletID.Name, (ZString)"PLT-1", (ZString)"PLT-2", (ZString)"PLT-3");
		}

		#endregion

		#region TestWE_PalletID_MaxLength

		public void TestWE_PalletID_MaxLength()
		{
			var docketLine = DocketLine;
			AssertNoExceptionThrown(() => docketLine.WE_PalletID = "123456789012345678901234567890");
		}

		#endregion

		#region TestWE_TransferFromPalletId_MaxLength

		public void TestWE_TransferFromPalletId_MaxLength()
		{
			var docketLine = DocketLine;
			AssertNoExceptionThrown(() => docketLine.WE_TransferFromPalletId = "123456789012345678901234567890");
		}

		#endregion

		#region TestWE_OriginalInventoryStatus

		public void TestWE_OriginalInventoryStatus()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_OriginalInventoryStatus.Name, WhsInventoryView.Schema.OriginalInventoryStatus, (ZString)"XXX", (ZString)"YYY", (ZString)"ZZZ");
		}

		#endregion

		#region TestWE_CurrentInventoryStatus

		public void TestWE_CurrentInventoryStatus()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_CurrentInventoryStatus.Name, WhsInventoryViewSchema.WI_InventoryStatus.Name, (ZString)"XXX", (ZString)"YYY", (ZString)"ZZZ");
		}

		#endregion

		#region TestWE_ClientOrderedUnits

		public void TestWE_ClientOrderedUnits()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", 0m, docketLine.WE_ClientOrderedUnits);

			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_ClientOrderedUnits.Name, WhsInventoryView.Schema.WI_ExpectedReceiptQuantity, (ZDecimal)2m, (ZDecimal)5m, (ZDecimal)7m);
		}

		#endregion

		#region TestWE_WE_OriginalDocketLineForRating

		public void TestWE_WE_OriginalDocketLineForRating_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_WE_OriginalDocketLineForRating.Name, WhsInventoryViewSchema.WI_WE_OriginalInDocketLineForRating.Name, ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());
		}

		#endregion

		#region TestWE_BondedEntryKey

		public void TestWE_BondedEntryKey_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_BondedEntryKey.Name, WhsInventoryViewSchema.WI_BondedEntryKey.Name, (ZString)"PLT-1", (ZString)"PLT-2", (ZString)"PLT-3");
		}

		#endregion

		#region TestWE_AllocationKey

		public void TestWE_AllocationKey_SetValueToInventory()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_AllocationKey.Name, WhsInventoryViewSchema.WI_AllocationKey.Name, (ZString)"PLT-1", (ZString)"PLT-2", (ZString)"PLT-3");
		}

		#endregion

		#region TestWE_IsOriginalInventory

		public void TestWE_IsOriginalInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_IsOriginalInventory.Name, WhsInventoryViewSchema.WI_IsOriginalReceiptLine.Name, ZBool.False, ZBool.True, ZBool.False);
		}

		#endregion

		#region TestWE_AdjustmentArrivalDate

		[TestDate(2017, 8, 9, 11, 11, 11)]
		public void TestWE_AdjustmentArrivalDate()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLineSchema.WE_AdjustmentArrivalDate.Name, WhsInventoryViewSchema.WI_ArrivalDate.Name, ZDateTimeOffset.Now.AddDays(2), ZDateTimeOffset.Now.AddDays(5), ZDateTimeOffset.Now.AddDays(9));
		}

		#endregion

		#region TestWE_PartAttrib1_MaxLength

		public void TestWE_PartAttrib1_MaxLength()
		{
			var docketLine = GetNewBusinessObject();
			AssertNoExceptionThrown(() => docketLine.WE_PartAttrib1 = "".PadLeft(WhsDocketLineSchema.WE_PartAttrib1.MaxLength, 'A'));
		}

		#endregion

		protected virtual void AssertMaxLengthExceededCore(Exception exception)
		{
			Assert(exception is MaxLengthExceededException);
		}

		#region TestWE_PartAttrib1_Exceed_MaxLength

		public void TestWE_PartAttrib1_Exceed_MaxLength()
		{
			try
			{
				var docketLine = GetNewBusinessObject();
				var exception = AssertExceptionThrown<Exception>(() =>
					docketLine.WE_PartAttrib1 = ZString.Replicate('A', WhsDocketLineSchema.WE_PartAttrib1.MaxLength + 1));
				AssertMaxLengthExceededCore(exception);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		#region TestWE_PartAttrib2_MaxLength

		public void TestWE_PartAttrib2_MaxLength()
		{
			var docketLine = GetNewBusinessObject();
			AssertNoExceptionThrown(() => docketLine.WE_PartAttrib2 = "".PadLeft(WhsDocketLineSchema.WE_PartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWE_PartAttrib2_Exceed_MaxLength

		public void TestWE_PartAttrib2_Exceed_MaxLength()
		{
			try
			{
				var docketLine = GetNewBusinessObject();
				var exception = AssertExceptionThrown<Exception>(() =>
					docketLine.WE_PartAttrib2 = ZString.Replicate('A', WhsDocketLineSchema.WE_PartAttrib2.MaxLength + 1));
				AssertMaxLengthExceededCore(exception);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWE_PartAttrib3_MaxLength

		public void TestWE_PartAttrib3_MaxLength()
		{
			var docketLine = GetNewBusinessObject();
			AssertNoExceptionThrown(() => docketLine.WE_PartAttrib3 = "".PadLeft(WhsDocketLineSchema.WE_PartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestWE_PartAttrib3_Exceed_MaxLength

		public void TestWE_PartAttrib3_Exceed_MaxLength()
		{
			try
			{
				var docketLine = GetNewBusinessObject();
				var exception = AssertExceptionThrown<Exception>(() =>
					docketLine.WE_PartAttrib3 = ZString.Replicate('A', WhsDocketLineSchema.WE_PartAttrib3.MaxLength + 1));
				AssertMaxLengthExceededCore(exception);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWE_FinalisedDate_TruncatesMilliseconds

		[TestDate(2018, 7, 5, 08, 32, 17, 234)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWE_FinalisedDate_TruncatesMilliseconds()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);
			docketLine.Docket.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(docketLine.Docket);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var docketLineInNewFactory = newFactory.Load<WhsDocketLine>(docketLine.PK);
			AssertEquals("WE_FinalisedDate should contain seconds.",
				data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2018, 7, 5, 08, 32, 17, 000)),
				docketLineInNewFactory.WE_FinalisedDate);
		}

		#endregion

		#region Calculated Properties

		#region TestCommittedQuantityIncludingUnfinalisedReceipt

		public void TestCommittedQuantityIncludingUnfinalisedReceipt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			AssertEquals(10m, receive.Lines[0].CommittedQuantityIncludingUnfinalisedReceipt);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			AssertEquals(0m, orderLine.CommittedQuantityIncludingUnfinalisedReceipt);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertEquals("", DocketLine.ProductCode);

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("ProductCode should be set from WI_OP", "P1", DocketLine.ProductCode);

			DocketLine.ProductCode = "NEWPRODUCT";
			AssertEquals("ProductCode still should come from SupplierPart since it is not null", "P1", DocketLine.ProductCode);

			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("ProductCode should has previously set value since SupplierPart is null.", "NEWPRODUCT", DocketLine.ProductCode);

			DocketLine.WE_OP = ZGuid.Invalid;
			DocketLine.ProductCode = "NEWPRODUCT-2";
			AssertEquals("ProductCode should not be reset", "NEWPRODUCT-2", DocketLine.ProductCode);
		}

		public void TestProductCode_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLine.Schema.ProductCode, WhsInventoryView.Schema.WI_OP_PartNum, (ZString)"P1", (ZString)"P2", (ZString)"P3");
		}

		#endregion

		#region TestProductDesc

		public void TestProductDesc()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertEquals("", DocketLine.ProductDesc);

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("ProductDesc should be set from WI_OP", "P1", DocketLine.ProductDesc);

			DocketLine.ProductDesc = "NEWDESC";
			AssertEquals("ProductDesc still should come from SupplierPart since it is not null", "P1", DocketLine.ProductDesc);

			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("ProductDesc should has previously set value since SupplierPart is null.", "NEWDESC", DocketLine.ProductDesc);

			DocketLine.WE_OP = ZGuid.Invalid;
			DocketLine.ProductDesc = "NEWDESC-2";
			AssertEquals("ProductDesc should not be reset", "NEWDESC-2", DocketLine.ProductDesc);
		}

		public void TestProductDesc_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLine.Schema.ProductDesc, WhsInventoryView.Schema.WI_OP_Desc, (ZString)"P1", (ZString)"P2", (ZString)"P3");
		}

		#endregion

		#region TestCommodityCode

		public void TestCommodityCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_RH_NKCommodityCode = "COMM";
			AssertEquals("", DocketLine.CommodityCode);

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("CommodityCode should be set from WI_OP", "COMM", DocketLine.CommodityCode);

			DocketLine.CommodityCode = "CMM1";
			AssertEquals("CommodityCode still should come from SupplierPart since it is not null", "COMM", DocketLine.CommodityCode);

			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("CommodityCode should has previously set value since SupplierPart is null.", "CMM1", DocketLine.CommodityCode);

			DocketLine.WE_OP = ZGuid.Invalid;
			DocketLine.CommodityCode = "CMM2";
			AssertEquals("CommodityCode should not be reset", "CMM2", DocketLine.CommodityCode);
		}

		public void TestCommodityCode_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLine.Schema.CommodityCode, WhsInventoryView.Schema.CommodityCode, (ZString)"P1", (ZString)"P2", (ZString)"P3");
		}

		#endregion

		#region TestProductUQ

		public void TestProductUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "BOX";
			AssertEquals("UNT", DocketLine.ProductUQ);

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("BOX", DocketLine.ProductUQ);

			DocketLine.WE_OP = ZGuid.Invalid;
			DocketLine.ProductCode = "TempProd";
			AssertEquals("UNT", DocketLine.ProductUQ);

			DocketLine.ProductUQ = "PLT";
			AssertEquals("PLT", DocketLine.ProductUQ);
			if (DocketLine is ISupportTemporaryProduct)
			{
				AssertEquals("PLT", DocketLine.WE_F3_NKPackType);
			}

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("BOX", DocketLine.ProductUQ);
		}

		public void TestProductUQ_SetValueToInventory()
		{
			TestSetValueToInventoryView(DocketLine, WhsDocketLine.Schema.ProductUQ, WhsInventoryView.Schema.WI_UnitsUQ, (ZString)"P1", (ZString)"P2", (ZString)"P3");
		}

		#endregion

		#region TestPackUOM

		public void TestPackUOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "BOX";

			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var docketLine = Helper.CreateWhsReceiveLine(docket, data.Part1, 15m);
			AssertEquals("BOX", docketLine.ProductUQ);

			var packType = docketLine.PackType;
			AssertEquals("Precondition: Default is blank.", string.Empty, packType.F3_UOMType);
			AssertEquals("Precondition: Default is blank.", string.Empty, docketLine.PackUOM);

			packType.F3_UOMType = "PLT";
			AssertEquals("F3_UOMType updated.", "PLT", packType.F3_UOMType);
			AssertEquals("PackUOM updated correctly.", "PLT", docketLine.PackUOM);

			packType.F3_UOMType = "SPC";
			AssertEquals("F3_UOMType updated.", "SPC", packType.F3_UOMType);
			AssertEquals("PackUOM updated correctly.", "SPC", docketLine.PackUOM);
		}

		#endregion

		#region TestOriginalInventoryStatusDescription

		public void TestOriginalInventoryStatusDescription()
		{
			var docketLine = DocketLine;
			docketLine.WE_OriginalInventoryStatus = "";
			AssertEquals("", docketLine.OriginalInventoryStatusDescription);

			foreach (CodeDescriptionPair inventoryStatus in docketLine.Lookups.InventoryStatuses)
			{
				docketLine.WE_OriginalInventoryStatus = inventoryStatus.Code;
				AssertEquals(inventoryStatus.Description, docketLine.OriginalInventoryStatusDescription);
			}
		}

		#endregion

		#region TestReceiptReference

		public void TestReceiptReference()
		{
			var docket1 = GetNewWhsDocket();
			var line1 = docket1.Lines.AddNew();
			docket1.WD_ExternalReference = "1";
			AssertEquals("When original docket line for rating is not specified, reference should be taken from current job.", "1", line1.ReceiptReference);

			var docket2 = GetNewWhsDocket();
			var line2 = docket2.Lines.AddNew();
			docket2.WD_ExternalReference = "2";
			line1.WE_WE_OriginalDocketLineForRating = line2.PK;
			AssertEquals("When original docket line for rating is specified, reference should be taken from it.", "2", line1.ReceiptReference);
		}

		#endregion

		#region TestClientPK

		public void TestClientPK()
		{
			var docket = GetNewWhsDocket();
			var docketLine = docket.Lines.AddNew();
			AssertEquals("Precondition", ZGuid.Empty, docketLine.ClientPK);

			var client = Helper.CreateClient("CLIENT");
			docket.WD_OH_Client = client.PK;
			AssertEquals(client.PK, docketLine.ClientPK);
		}

		#endregion

		#region TestLocationAreaType

		public void TestLocationAreaType()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var pickingArea = Helper.CreateArea(whs, "PIK", "AT1");
			var putawayArea = Helper.CreateArea(whs, "PUT", "AT2");
			var loc = whs.DefaultLocation;
			loc.WLV_WA_PickingArea = pickingArea.PK;
			loc.WLV_WA_PutawayArea = putawayArea.PK;
			Factory.Save();

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("AT1", docketLine.LocationAreaType);
		}

		#endregion

		#region TestLocationAreaName

		public void TestLocationAreaName()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var pickingArea = Helper.CreateArea(whs, "PIK", "AT1");
			var putawayArea = Helper.CreateArea(whs, "PUT", "AT2");
			var loc = whs.DefaultLocation;
			loc.WLV_WA_PickingArea = pickingArea.PK;
			loc.WLV_WA_PutawayArea = putawayArea.PK;
			Factory.Save();

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("PIK", docketLine.LocationAreaName);
		}

		public void TestLocationAreaName_Translatable()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var pickingArea = Helper.CreateArea(whs, "PIK");
			var putawayArea = Helper.CreateArea(whs, "PUT");
			var loc = whs.DefaultLocation;
			loc.WLV_WA_PickingArea = pickingArea.PK;
			loc.WLV_WA_PutawayArea = putawayArea.PK;
			pickingArea.WA_Name = "Test Area";

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("LocationAreaName in English.", "Test Area", docketLine.LocationAreaName);

			var resKey = pickingArea.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(pickingArea, "Test Area").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("LocationAreaName in Chinese.", "测试区", docketLine.LocationAreaName);
			}
		}

		#endregion

		#region TestReservedQuantity

		public void TestReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);
			var pickableDocketLine = docketLine as WhsPickableDocketLine;
			if (pickableDocketLine != null) // lines that will be reserving stock
			{
				pickableDocketLine.PickableDocket.Pick.CancelPick(); // order need to be un-picked to reserve stock.

				var receiveLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m));
				Helper.CreateReservePickLine(pickableDocketLine, receiveLine.Inventory[0], 3m);
				AssertEquals("Reserved quantity for the line should be 3.", 3m, docketLine.ReservedQuantity);
			}
			else // lines that will create inventory from which stock will be reserved.
			{
				docketLine.Docket.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(docketLine.Docket);

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				var orderLine = order.Lines[0];
				orderLine.ReserveStockIfAbleTo(docketLine.Inventory[0], 3m);
				AssertEquals("Reserved quantity for the line should be 3.", 3m, docketLine.ReservedQuantity);
			}
		}

		#endregion

		#region TestHumanReadableShortcutName

		public void TestHumanReadableShortcutName()
		{
			TestHumanReadableShortcutNameCore();
		}

		protected virtual void TestHumanReadableShortcutNameCore()
		{
			AssertEquals("Docket Line", DocketLine.HumanReadableShortcutName);
		}

		#endregion

		#endregion

		#region TestWE_StockOnHandValue

		public void TestWE_StockOnHandValue()
		{
			var docketLine = DocketLine;

			docketLine.WE_StockOnHand = 10m;
			AssertEquals("Precondition", 10m, docketLine.WE_StockOnHand);
			AssertEquals("", 10m, docketLine.WE_StockOnHand);

			docketLine.WE_StockOnHand = 20m;
			AssertEquals("Precondition", 20m, docketLine.WE_StockOnHand);
			AssertEquals("", 20m, docketLine.WE_StockOnHand);
		}

		#endregion

		//

		#region TestSetValueToInventoryView

		protected static void TestSetValueToInventoryView(TDocketLine docketLine, ZString docketLineColumn, ZString inventoryViewColumn, IZType value1, IZType value2, IZType value3)
		{
			if (!(docketLine is WhsPickableDocketLine))
			{
				var docket = docketLine.Docket; // need to be in from in case the field we test is WE_WD.
				docketLine[docketLineColumn] = value1;
				AssertEquals(value1, docketLine[docketLineColumn]);

				WhsInventoryView inventory = null;
				if (docketLine.Inventory.Count > 0)
				{
					inventory = docketLine.Inventory[0];
				}
				else
				{
					inventory = docketLine.Inventory.AddNew();
					inventory.WI_InDocketLineType = docket.WD_DocketType;
					inventory.WI_WE_InDocketLine = docketLine.PK;
				}
				docketLine[docketLineColumn] = value2;
				AssertEquals("When docket line have an inventory then any value set to a docket line should be populated to inventory.", value2, inventory[inventoryViewColumn]);

				inventory[inventoryViewColumn] = value3;
				AssertEquals("Other way around should also be true.", value3, docketLine[docketLineColumn]);

				TestSetValueToInventoryViewWithSpace(docketLine, inventory, docketLineColumn, inventoryViewColumn, value1);
			}
			else
			{
				Assert("This type of DocketLine doesn't create inventory.", true);
			}
		}

		static void TestSetValueToInventoryViewWithSpace(TDocketLine docketLine, WhsInventoryView inventory, ZString docketLineColumn, ZString inventoryViewColumn, IZType value1)
		{
			// they have local variable to store data
			// in setter of base class trim the value
			var exceptZStringColumns = new[] { WhsDocketLine.Schema.CommodityCode, WhsDocketLine.Schema.ProductDesc, WhsDocketLine.Schema.ProductUQ, WhsDocketLine.Schema.ProductCode };

			// check for trailing space
			if (value1 is ZString && !exceptZStringColumns.Any(c => c == docketLineColumn))
			{
				var value = value1.ToString();
				ZString valueWithSpace = value.Remove(value.Length - 1) + " "; // in case if properti has max length like inventory status
				docketLine[docketLineColumn] = valueWithSpace;
				AssertEquals("It will remove the space in setter.", valueWithSpace.TrimEndSpaceTab(), docketLine[docketLineColumn]);
				AssertEquals("Setting property on docket line should populate inventory as well.", docketLine[docketLineColumn], inventory[inventoryViewColumn]);

				ZString valueWithTab = value.Remove(value.Length - 1) + "\t";
				docketLine[docketLineColumn] = valueWithSpace;
				AssertEquals("It will remove the tab in setter.", valueWithTab.TrimEndSpaceTab(), docketLine[docketLineColumn]);
				AssertEquals("Setting property on docket line should populate inventory as well.", docketLine[docketLineColumn], inventory[inventoryViewColumn]);
			}
		}

		#endregion

		#region TestIsSyncingDocketLineInventoryView

		public void TestIsSyncingDocketLineInventoryView()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			var inventory = receive.Inventory[0];

			var reciveline = receive.Lines[0];
			inventory.WI_PartAttrib1Info.ValueChanged += (sender, e) => { inventory.WI_PartAttrib2 = "Att2 "; };

			Assert("Precondition", inventory.WI_PartAttrib1.IsEmpty);
			Assert("Precondition", inventory.WI_PartAttrib2.IsEmpty);
			Assert("Precondition", reciveline.WE_PartAttrib1.IsEmpty);
			Assert("Precondition", reciveline.WE_PartAttrib2.IsEmpty);

			reciveline.WE_PartAttrib1 = "Att1 ";
			AssertEquals("Precondition", "Att1", reciveline.WE_PartAttrib1);
			AssertEquals("Precondition", "Att1", inventory.WI_PartAttrib1);
			AssertEquals("Precondition", "Att2", reciveline.WE_PartAttrib2);
			AssertEquals("Precondition", "Att2", inventory.WI_PartAttrib2);
		}

		#endregion

		#endregion

		#region PropertyInfos

		#region HeldCodeToChangeTo

		public void TestHeldCodeChangeQuantityInfo()
		{
			AssertEquals(WhsDocketLine.Schema.HeldCodeChangeQuantity, DocketLine.HeldCodeChangeQuantityInfo.Name);
		}

		public void TestHeldCodeChangeQuantityInfo_ReadOnly()
		{
			TestInventoryEditFormReadOnly(WhsDocketLine.Schema.HeldCodeChangeQuantity);
		}

		#endregion

		#region TestHeldCodeToChangeToInfo

		public void TestHeldCodeToChangeToInfo()
		{
			AssertEquals(WhsDocketLine.Schema.HeldCodeToChangeTo, DocketLine.HeldCodeToChangeToInfo.Name);
		}

		public void TestHeldCodeToChangeToInfo_ReadOnly()
		{
			TestInventoryEditFormReadOnly(WhsDocketLine.Schema.HeldCodeToChangeTo);
		}

		#endregion

		#region TestHeldCodeToChangeToInfo

		public void TestHoldReasonToChangeToInfo()
		{
			AssertEquals(WhsDocketLine.Schema.HoldReasonToChangeTo, DocketLine.HoldReasonToChangeToInfo.Name);
		}

		public void TestHoldReasonToChangeToInfo_ReadOnly()
		{
			TestInventoryEditFormReadOnly(WhsDocketLine.Schema.HoldReasonToChangeTo);
		}

		#endregion

		#region TestWE_AdjustmentArrivalDateInfo

		public void TestWE_AdjustmentArrivalDateInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestStandardReadOnly(d => d.WE_AdjustmentArrivalDateInfo);
			TestWE_AdjustmentArrivalDateInfoCore(data);
		}

		protected virtual void TestWE_AdjustmentArrivalDateInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_AllocationKeyInfo

		public void TestWE_AllocationKeyInfo()
		{
			TestWE_AllocationKeyInfoCore();
		}

		protected virtual void TestWE_AllocationKeyInfoCore()
		{
			AssertEquals(true, GetNewBusinessObject().WE_AllocationKeyInfo.ReadOnly);
		}

		#endregion

		#region TestWE_BondedEntryKeyInfo

		public void TestWE_BondedEntryKeyInfo()
		{
			if (UsesBondedEntryKey)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var docket = GetNewWhsDocket(data.Org1, data.Whs1);
				var docketLine = GetNewBusinessObject(docket);
				TestReadOnly(d => d.WE_BondedEntryKeyInfo, true, true, true, true);

				Helper.EnableWarehouseForBond(data.Whs1, true);
				SetDocketToCustomsTransaction(docket);
				TestWE_BondedEntryKeyInfoCore(docketLine.WE_BondedEntryKeyInfo);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void TestWE_BondedEntryKeyInfoCore(ZPropertyInfo bondedEntryKeyInfo)
		{
			TestStandardReadOnly(bondedEntryKeyInfo);
		}

		protected virtual bool UsesBondedEntryKey
		{
			get { return true; }
		}

		#endregion

		#region TestWE_CurrentInventoryStatusInfo

		public void TestWE_CurrentInventoryStatusInfo()
		{
			TestReadOnly(d => d.WE_CurrentInventoryStatusInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_LineCommentInfo

		public void TestWE_LineCommentInfo()
		{
			TestStandardReadOnly(d => d.WE_LineCommentInfo);
		}

		#endregion

		#region TestWE_PackageGroupIdInfo

		public void TestWE_PackageGroupIdInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(docketLine.WE_PackageGroupIdInfo, data);
		}

		#endregion

		#region TestWE_PackQuantityInfo

		public void TestWE_PackQuantityInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			AssertEquals(WhsDocketLine.Schema.WE_PackQuantity, docketLine.WE_PackQuantityInfo.Name);

			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(docketLine.WE_PackQuantityInfo, data);
			TestWE_PackQuantityInfoCore(data);
		}

		protected virtual void TestWE_PackQuantityInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_PerPackageQtyInfo

		public void TestWE_PerPackageQtyInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			TestWE_PerPackageQtyInfoCore(docketLine, data);
		}

		protected virtual void TestWE_PerPackageQtyInfoCore(WhsDocketLine docketLine, TestDataSimpleEnvironment data)
		{
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(docketLine.WE_PerPackageQtyInfo, data);
		}

		#endregion

		#region TestWE_F3_NKPackTypeInfo

		public void TestWE_F3_NKPackTypeInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(docketLine.WE_F3_NKPackTypeInfo, data);
			TestWE_F3_NKPackTypeInfoCore(data);
		}

		protected virtual void TestWE_F3_NKPackTypeInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region CustomAttributes

		public void TestWE_CustomAttrib1Info()
		{
			TestReadOnly(d => d.WE_CustomAttrib1Info, true, true, true, true);
		}

		public void TestWE_CustomAttrib2Info()
		{
			TestReadOnly(d => d.WE_CustomAttrib2Info, true, true, true, true);
		}

		public void TestWE_CustomAttrib3Info()
		{
			TestReadOnly(d => d.WE_CustomAttrib3Info, true, true, true, true);
		}

		public void TestWE_CustomAttrib4Info()
		{
			TestReadOnly(d => d.WE_CustomAttrib4Info, true, true, true, true);
		}

		public void TestWE_CustomAttrib5Info()
		{
			TestReadOnly(d => d.WE_CustomAttrib5Info, true, true, true, true);
		}

		public void TestWE_CustomAttrib6Info()
		{
			TestReadOnly(d => d.WE_CustomAttrib6Info, true, true, true, true);
		}

		public void TestWE_CustomDecimal1Info()
		{
			TestReadOnly(d => d.WE_CustomDecimal1Info, true, true, true, true);
		}

		public void TestWE_CustomDecimal2Info()
		{
			TestReadOnly(d => d.WE_CustomDecimal2Info, true, true, true, true);
		}

		public void TestWE_CustomDecimal3Info()
		{
			TestReadOnly(d => d.WE_CustomDecimal3Info, true, true, true, true);
		}

		public void TestWE_CustomDecimal4Info()
		{
			TestReadOnly(d => d.WE_CustomDecimal4Info, true, true, true, true);
		}

		public void TestWE_CustomDecimal5Info()
		{
			TestReadOnly(d => d.WE_CustomDecimal5Info, true, true, true, true);
		}

		public void TestWE_CustomDate1Info()
		{
			TestReadOnly(d => d.WE_CustomDate1Info, true, true, true, true);
		}

		public void TestWE_CustomDate2Info()
		{
			TestReadOnly(d => d.WE_CustomDate2Info, true, true, true, true);
		}

		public void TestWE_CustomDate3Info()
		{
			TestReadOnly(d => d.WE_CustomDate3Info, true, true, true, true);
		}

		public void TestWE_CustomDate4Info()
		{
			TestReadOnly(d => d.WE_CustomDate4Info, true, true, true, true);
		}

		public void TestWE_CustomDate5Info()
		{
			TestReadOnly(d => d.WE_CustomDate5Info, true, true, true, true);
		}

		public void TestWE_CustomFlag1Info()
		{
			TestReadOnly(d => d.WE_CustomFlag1Info, true, true, true, true);
		}

		public void TestWE_CustomFlag2Info()
		{
			TestReadOnly(d => d.WE_CustomFlag2Info, true, true, true, true);
		}

		public void TestWE_CustomFlag3Info()
		{
			TestReadOnly(d => d.WE_CustomFlag3Info, true, true, true, true);
		}

		public void TestWE_CustomFlag4Info()
		{
			TestReadOnly(d => d.WE_CustomFlag4Info, true, true, true, true);
		}

		public void TestWE_CustomFlag5Info()
		{
			TestReadOnly(d => d.WE_CustomFlag5Info, true, true, true, true);
		}

		public void TestWE_CustomTextBlobInfo()
		{
			TestReadOnly(d => d.WE_CustomTextBlob1Info, true, true, true, true);
		}

		#endregion

		#region PartAttributes

		#region TestWE_PartAttrib1Info

		public void TestWE_PartAttrib1Info()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestWE_PartAttribInfoCore(data, WhsDocketLineSchema.WE_PartAttrib1, AttributeNumber.One);
			TestWE_PartAttrib1InfoCore(data);
		}

		protected virtual void TestWE_PartAttrib1InfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_PartAttrib2Info

		public void TestWE_PartAttrib2Info()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestWE_PartAttribInfoCore(data, WhsDocketLineSchema.WE_PartAttrib2, AttributeNumber.Two);
			TestWE_PartAttrib2InfoCore(data);
		}

		protected virtual void TestWE_PartAttrib2InfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_PartAttrib3Info

		public void TestWE_PartAttrib3Info()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestWE_PartAttribInfoCore(data, WhsDocketLineSchema.WE_PartAttrib3, AttributeNumber.Three);
			TestWE_PartAttrib3InfoCore(data);
		}

		protected virtual void TestWE_PartAttrib3InfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_SerialNumberInfo

		public void TestWE_SerialNumberInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			TestWE_PartAttribInfoCore(data, WhsDocketLineSchema.WE_SerialNumber, AttributeNumber.Serial);
			TestWE_SerialNumberInfoCore(data);
		}

		protected virtual void TestWE_SerialNumberInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_ExpiryDateInfo

		#region TestWE_ExpiryDateInfo

		public void TestWE_ExpiryDateInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestWE_PartAttribInfoCore(data, WhsDocketLineSchema.WE_ExpiryDate, AttributeNumber.ExpiryDate);
			TestWE_ExpiryDateInfoCore(data);
		}

		protected virtual void TestWE_ExpiryDateInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_ExpiryDateInfo_WithJulianBatchNumberAttribute

		public void TestWE_ExpiryDateInfo_WithJulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			TestDateInfo_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, docketLine, AttributeNumber.One, docketLine.WE_ExpiryDateInfo);
			TestDateInfo_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, docketLine, AttributeNumber.Two, docketLine.WE_ExpiryDateInfo);
			TestDateInfo_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, docketLine, AttributeNumber.Three, docketLine.WE_ExpiryDateInfo);
		}

		#endregion

		#region TestWE_ExpiryDateInfo_ReadOnly

		public void TestWE_ExpiryDateInfo_ReadOnly()
		{
			// Julian batch edge cases are tested elsewhere
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			TestPartAttributesReadOnly(docketLine.WE_ExpiryDateInfo, data);
		}

		#endregion

		#endregion

		#region TestWE_PackingDateInfo

		public void TestWE_PackingDateInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestWE_PartAttribInfoCore(data, WhsDocketLineSchema.WE_PackingDate, AttributeNumber.PackingDate);
			TestWE_PackingDateInfoCore(data);
		}

		protected virtual void TestWE_PackingDateInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#region TestWE_PackingDateInfo_WithJulianBatchNumberAttribute

		public void TestWE_PackingDateInfo_WithJulianBatchNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			TestDateInfo_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, docketLine, AttributeNumber.One, docketLine.WE_PackingDateInfo);
			TestDateInfo_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, docketLine, AttributeNumber.Two, docketLine.WE_PackingDateInfo);
			TestDateInfo_WithJulianBatchNumberAttributeCore(data.Org1, data.Part1, docketLine, AttributeNumber.Three, docketLine.WE_PackingDateInfo);
		}

		void TestDateInfo_WithJulianBatchNumberAttributeCore(OrgHeader client, OrgSupplierPart part, WhsDocketLine docketLine, AttributeNumber attributeNumber, ZPropertyInfo propertyInfo)
		{
			Helper.SetClientAttributeType(client, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(client, part, attributeNumber, true);
			AssertEquals(false, propertyInfo.ReadOnly);

			Helper.SetClientAttributeType(client, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(client, part, attributeNumber, true);
			AssertEquals(true, propertyInfo.ReadOnly);
			Helper.SetProductAttributeUse(client, part, attributeNumber, false); // clean up
		}

		#endregion

		public void TestWE_PackingDateInfo_ReadOnly()
		{
			// Julian batch edge cases are tested elsewhere
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			TestPartAttributesReadOnly(docketLine.WE_PackingDateInfo, data);
		}

		#endregion

		#region TestWE_PartAttribInfoCore

		protected void TestWE_PartAttribInfoCore(TestDataSimpleEnvironment data, SchemaColumn docketLinePartAttribSchemaColumn, AttributeNumber attribNo)
		{
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			var partAttribInfo = docketLine.ZPropertyInfoHash[docketLinePartAttribSchemaColumn.Name];
			TestReadOnly(partAttribInfo, true, true, true, true);

			docketLine.WE_OP = data.Part1.PK;
			Helper.SetClientAttributeType(data.Org1, attribNo, true);
			Helper.SetProductAttributeUse(data.Org1, docketLine.SupplierPart, attribNo, true);
			TestPartAttributesReadOnly(partAttribInfo, data);
		}

		#endregion

		#endregion

		#region TestWE_TransactionQuantityInfo

		public void TestWE_TransactionQuantityInfo()
		{
			TestWE_TransactionQuantityInfoCore();
		}

		protected virtual void TestWE_TransactionQuantityInfoCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(docketLine.WE_TransactionQuantityInfo, data);
		}

		#endregion

		#region TestWE_OPInfo

		public void TestWE_OPInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Docket = GetNewWhsDocket(data.Org1, data.Whs1);
			DocketLine = GetNewBusinessObject(Docket);
			DocketLine.WE_OP = data.Part1.PK;

			TestProductReadOnly(DocketLine.WE_OPInfo);
			TestWE_OPInfoCore(data);
		}

		protected virtual void TestWE_OPInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_OriginalInventoryStatusInfo

		public void TestWE_OriginalInventoryStatusInfo()
		{
			TestReadOnly(d => d.WE_OriginalInventoryStatusInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_WLInfo

		public void TestWE_WLInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			DocketLine.Docket.WD_WW_Whs = Factory.New<WhsWarehouse>().PK;
			TestDestLocationReadOnly(DocketLine.WE_WLInfo);
			TestWE_WLInfoCore(data, DocketLine.WE_WLInfo);
		}

		public void TestDestWarehouseReadOnly()
		{
			TestDestWarehouseReadOnly(d => d.CommodityCodeInfo);
		}

		protected virtual void TestWE_WLInfoCore(TestDataSimpleEnvironment data, ZPropertyInfo info)
		{
		}

		#endregion

		#region TestWE_WHC_NKOriginalInventoryHeldCodeInfo_ReadOnly

		public void TestWE_WHC_NKOriginalInventoryHeldCodeInfo_ReadOnly()
		{
			TestHeldCodeReadOnly(d => d.WE_WHC_NKOriginalInventoryHeldCodeInfo);
		}

		#endregion

		#region TestWE_WHC_NKOriginalInventoryHeldCodeInfo_ConcurrencyPolicy

		public void TestWE_WHC_NKOriginalInventoryHeldCodeInfo_ConcurrencyPolicy()
		{
			AssertEquals(ConcurrencyPolicy.Strict, DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWE_WHC_NKCurrentInventoryHeldCodeInfo_ReadOnly

		public void TestWE_WHC_NKCurrentInventoryHeldCodeInfo_ReadOnly()
		{
			TestReadOnly(d => d.WE_WHC_NKCurrentInventoryHeldCodeInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_WHC_NKCurrentInventoryHeldCodeInfo_ConcurrencyPolicy

		public void TestWE_WHC_NKCurrentInventoryHeldCodeInfo_ConcurrencyPolicy()
		{
			AssertEquals(ConcurrencyPolicy.Strict, DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWE_CurrentHoldReasonInfo

		public void TestWE_CurrentHoldReasonInfo()
		{
			TestReadOnly(d => d.WE_CurrentHoldReasonInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_LineNoInfo

		public void TestWE_LineNoInfo()
		{
			TestWE_LineNoInfoCore();
		}

		protected virtual void TestWE_LineNoInfoCore()
		{
			TestReadOnly(d => d.WE_LineNoInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_SubLineNoInfo

		public void TestWE_SubLineNoInfo()
		{
			TestWE_SubLineNoInfoCore();
		}

		protected virtual void TestWE_SubLineNoInfoCore()
		{
			TestReadOnly(d => d.WE_SubLineNoInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_UnitDiscountAmountInfo

		public virtual void TestWE_UnitDiscountAmountInfo()
		{
			TestStandardReadOnly(d => d.WE_UnitDiscountAmountInfo);
		}

		#endregion

		#region TestWE_UnitDiscountPercentInfo

		public virtual void TestWE_UnitDiscountPercentInfo()
		{
			TestStandardReadOnly(d => d.WE_UnitDiscountPercentInfo);
		}

		#endregion

		#region TestWE_UnitPriceAfterDiscountInfo

		public virtual void TestWE_UnitPriceAfterDiscountInfo()
		{
			TestStandardReadOnly(d => d.WE_UnitPriceAfterDiscountInfo);
		}

		#endregion

		#region TestWE_ExtendedLinePriceInfo

		public void TestWE_ExtendedLinePriceInfo()
		{
			TestStandardReadOnly(d => d.WE_ExtendedLinePriceInfo);
		}

		#endregion

		#region TestWE_ExtendedLinePriceInfo_ConcurrencyPolicy

		public void TestWE_ExtendedLinePriceInfo_ConcurrencyPolicy()
		{
			AssertEquals(ConcurrencyPolicy.Strict, GetNewBusinessObject().WE_ExtendedLinePriceInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWE_RecommendedUnitPriceInfo

		public virtual void TestWE_RecommendedUnitPriceInfo()
		{
			TestStandardReadOnly(d => d.WE_RecommendedUnitPriceInfo);
		}

		#endregion

		#region TestWE_RX_NKUnitPriceCurrencyInfo

		public void TestWE_RX_NKUnitPriceCurrencyInfo()
		{
			TestStandardReadOnly(d => d.WE_RX_NKUnitPriceCurrencyInfo);
		}

		#endregion

		#region TestWE_PutawayTime_ReadOnly

		public void TestWE_PutawayTime_ReadOnly()
		{
			TestReadOnly(d => d.WE_PutawayTimeInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_PalletIDInfo

		public void TestWE_PalletIDInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestDestPalletIDReadonly(d => d.WE_PalletIDInfo);
			TestWE_PalletIDInfoCore(data);
		}

		protected virtual void TestWE_PalletIDInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_GS_NKPutawayByInfo

		public void TestWE_GS_NKPutawayByInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestReadOnly(d => d.WE_GS_NKPutawayByInfo, false, false, true, true);
			TestWE_GS_NKPutawayByInfoCore(data);
		}

		protected virtual void TestWE_GS_NKPutawayByInfoCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestWE_StockOnHandInfo

		public void TestWE_StockOnHandInfo()
		{
			TestReadOnly(d => d.WE_StockOnHandInfo, true, true, true, true);
		}

		#endregion

		#region TestWE_StockOnHandInfo_ConcurrencyPolicy

		public void TestWE_StockOnHandInfo_ConcurrencyPolicy()
		{
			AssertEquals(ConcurrencyPolicy.Strict, DocketLine.WE_StockOnHandInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWE_WPL_PutawayLineInfo

		public void TestWE_WPL_PutawayLineInfo()
		{
			TestReadOnly(d => d.WE_WPL_PutawayLineInfo, true, true, true, true);
		}

		#endregion

		#region Calculated Properties

		#region TestProductDescInfo

		public void TestProductDescInfo()
		{
			TestTempProductReadOnly(DocketLine.ProductDescInfo);
		}

		#endregion

		#region TestCommodityCodeInfo

		public void TestCommodityCodeInfo()
		{
			TestTempProductReadOnly(DocketLine.CommodityCodeInfo);
		}

		#endregion

		#endregion

		#region ReadOnly

		protected virtual void TestStandardReadOnly(Func<TDocketLine, ZPropertyInfo> getInfo)
		{
			TestReadOnly(getInfo, false, false, true, true);
		}

		protected virtual void TestStandardReadOnly(ZPropertyInfo info)
		{
			TestReadOnly(info, false, false, true, true);
		}

		protected virtual void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			TestStandardReadOnly(info);
		}

		protected virtual void TestAfterPickReadOnly(Func<TDocketLine, ZPropertyInfo> getInfo, TestDataSimpleEnvironment data)
		{
			TestStandardReadOnly(getInfo);
		}

		protected virtual void TestProductReadOnly(ZPropertyInfo info) => TestAfterPickProductAndAttribsReadOnly(info);

		protected virtual void TestPartAttributesReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data) => TestAfterPickProductAndAttribsReadOnly(info);

		protected virtual void TestAfterPickProductAndAttribsReadOnly(ZPropertyInfo info) => TestStandardReadOnly(info);

		protected void TestReadOnly(Func<TDocketLine, ZPropertyInfo> getInfo, bool isNewReadOnly, bool entered, bool finalised, bool cancelled,
			Action<Func<TDocketLine, bool>, string, TDocket, TDocketLine> additionalAssertions = null)
		{
			TestReadOnly(d => getInfo(d).ReadOnly, d => getInfo(d).Name, isNewReadOnly, entered, finalised, cancelled, additionalAssertions);
		}

		protected void TestReadOnly(
			Func<TDocketLine, bool> getReadOnly,
			Func<TDocketLine, string> getName,
			bool isEmptyReadOnly,
			bool wareHouse,
			bool recieved,
			bool pending,
			bool arrived,
			bool held)
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WD = docket.PK;
			var name = getName(docketLine);

			docket.WD_WW_Whs = ZGuid.Empty;
			AssertEquals($"{name} From should  {(recieved ? "" : " not ")} Since there is no warehouse.", isEmptyReadOnly, getReadOnly(docketLine));

			var warehouse = Factory.New<WhsWarehouse>();
			docket.WD_WW_Whs = warehouse.PK;

			AssertEquals(wareHouse, getReadOnly(docketLine));

			docketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			AssertEquals($"{name} From should  {(recieved ? "" : " not ")} be readonly when inventory status is Putaway.", recieved, getReadOnly(docketLine));

			docketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals($"{name} From should  {(pending ? "" : " not ")} be readonly when inventory status is Pending.", pending, getReadOnly(docketLine));

			docketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Arrived;
			AssertEquals($"{name} Fromshould  {(arrived ? "" : " not ")} be readonly when inventory status is Arrived.", arrived, getReadOnly(docketLine));

			docketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			AssertEquals($"{name} Fromshould  {(held ? "" : " not ")} be readonly when inventory status is Held.", held, getReadOnly(docketLine));
		}

		protected void TestReadOnly(
			Func<TDocketLine, bool> getReadOnly,
			Func<TDocketLine, string> getName,
			bool isNewReadOnly,
			bool entered,
			bool finalised,
			bool cancelled,
			Action<Func<TDocketLine, bool>, string, TDocket, TDocketLine> additionalAssertions = null)
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WD = docket.PK;
			var name = getName(docketLine);

			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals($"{name} should  {(isNewReadOnly ? "" : " not ")} be readonly if docket is New", isNewReadOnly, getReadOnly(docketLine));

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals($"{name} should {(entered ? "" : " not ")} be readonly if docket is Entered", entered, getReadOnly(docketLine));

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals($"{name} should {(finalised ? "" : " not ")} be readonly if docket is Finalised", finalised, getReadOnly(docketLine));

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals($"{name} should {(cancelled ? "" : " not ")} be readonly if docket is Cancelled", cancelled, getReadOnly(docketLine));

			if (additionalAssertions != null)
			{
				additionalAssertions(getReadOnly, name, docket, docketLine);
			}
		}

		protected void TestReadOnly(ZPropertyInfo info, bool isNewReadOnly, bool entered, bool finalised, bool cancelled)
		{
			var line = (WhsDocketLine)info.BizObj;
			var docket = Argument.NotNull(line.Docket, "line.Docket");

			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(info.Name + " should " + (isNewReadOnly ? "" : " not ") + "be readonly if docket is New", isNewReadOnly, line.ZPropertyInfoHash[info.Name].ReadOnly);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(info.Name + " should " + (entered ? "" : " not ") + "be readonly if docket is Entered", entered, line.ZPropertyInfoHash[info.Name].ReadOnly);

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals(info.Name + " should " + (finalised ? "" : " not ") + "be readonly if docket is Finalised", finalised, line.ZPropertyInfoHash[info.Name].ReadOnly);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(info.Name + " should " + (cancelled ? "" : " not ") + "be readonly if docket is Cancelled", cancelled, line.ZPropertyInfoHash[info.Name].ReadOnly);
		}

		protected void TestInventoryEditFormReadOnly(string propertyName)
		{
			var line = GetNewDocketLineReadyToFinalise();
			AssertEquals(string.Format("{0} should be read only if not on inventory form.", propertyName), true, line.ZPropertyInfoHash[propertyName].ReadOnly);

			line.IsInventoryEditForm = true;
			AssertEquals(string.Format("{0} should be read only if on inventory form and unfinalised.", propertyName), true, line.ZPropertyInfoHash[propertyName].ReadOnly);

			line.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(line.Docket);
			AssertEquals(string.Format("{0} should *not* be read only if finalised and on inventory edit form.", propertyName), false, line.ZPropertyInfoHash[propertyName].ReadOnly);

			line.IsInventoryEditForm = false;
			AssertEquals(string.Format("{0} should be read only if not on inventory form, even if finalised.", propertyName), true, line.ZPropertyInfoHash[propertyName].ReadOnly);
		}

		protected virtual void TestTempProductReadOnly(ZPropertyInfo info)
		{
			AssertEquals(true, info.ReadOnly);
		}

		protected virtual void TestDestLocationReadOnly(ZPropertyInfo info)
		{
			TestReadOnly(info, false, false, true, true);
		}

		protected virtual void TestDestWarehouseReadOnly(Func<WhsDocketLine, ZPropertyInfo> getInfo)
		{
			TestReadOnly(getInfo, true, true, true, true);
		}

		protected virtual void TestDestPalletIDReadonly(ZPropertyInfo info)
		{
			TestReadOnly(info, false, false, true, true);
		}

		protected virtual void TestDestPalletIDReadonly(Func<WhsDocketLine, ZPropertyInfo> getInfo)
		{
			TestReadOnly(getInfo, false, false, true, true);
		}

		protected virtual void TestHeldCodeReadOnly(ZPropertyInfo info)
		{
			TestStandardReadOnly(info);
		}

		protected virtual void TestHeldCodeReadOnly(Func<WhsDocketLine, ZPropertyInfo> getInfo)
		{
			TestStandardReadOnly(getInfo);
		}

		#endregion

		#region TestIsValidationEnabled

		public void TestIsValidationEnabled()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			docket.WD_DocketStatus = DocketStatus.Codes.New;
			Assert("Precondition: property is NOT readonly", !docketLine.WE_LineCommentInfo.ReadOnly);
			Assert("Validation for NOT readonly property should be enabled", docketLine.IsValidationEnabled(docketLine.WE_LineCommentInfo));

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			docketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			docketLine.WE_FinalisedDate = DateTimeOffset.Now;
			Assert("Precondition: property is readonly", docketLine.WE_LineCommentInfo.ReadOnly);
			Assert("Validation for readonly property should be disabled", !docketLine.IsValidationEnabled(docketLine.WE_LineCommentInfo));
		}

		#endregion

		#endregion

		#region TestConstraints

		#region TestConstraint_WE_CurrentInventoryStatus

		[ExpectNoExceptions]
		public void TestConstraint_WE_CurrentInventoryStatus_Unfinalized()
		{
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WE_CurrentInventoryStatus";
			var docketLine = SetupDocketLineForCurrentInventoryStatusConstraintTest();

			AssertEquals("Docket line must not be finalized.", false, docketLine.IsFinalised);
			AssertEquals("Expected original and current inventory statuses to be equal.", docketLine.WE_OriginalInventoryStatus, docketLine.WE_CurrentInventoryStatus);

			Factory.Save();

			docketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.PuttingAway;
			AssertNotEquals("Expected original and current inventory statuses to be different.", docketLine.WE_OriginalInventoryStatus, docketLine.WE_CurrentInventoryStatus);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WE_CurrentInventoryStatus");
		}

		protected virtual TDocketLine SetupDocketLineForCurrentInventoryStatusConstraintTest()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "");

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			docket.FillWithValidTestData();

			var docketLine = GetNewBusinessObject(docket, needWarehouse: false);
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_F3_NKPackType = "UNT";
			docketLine.WE_TransactionQuantity = 10m;
			docketLine.RunPreSaveValidation();

			return docketLine;
		}

		#endregion

		#endregion

		#region Location

		#region TestWarehouse

		public void TestWarehouse()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var docket = GetNewWhsDocket();
			var line = GetNewBusinessObject(docket);
			docket.WD_WW_Whs = whs.PK;
			AssertEquals(whs, line.Warehouse);
		}

		#endregion

		#region TestWarehousePK

		public void TestWarehousePK()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var docket = GetNewWhsDocket();
			var line = GetNewBusinessObject(docket, false);
			AssertEquals("Precondition", ZGuid.Empty, line.WarehousePK);

			docket.WD_WW_Whs = whs1.PK;
			AssertEquals(whs1.PK, line.WarehousePK);

			docket.WD_OH_Client = client.PK;
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = whs.DefaultLocation.PK;
			AssertEquals(whs.DefaultLocation, docketLine.Location);
		}

		#endregion

		#region TestCurrentLocationPickAreaName_Translatable

		public void TestCurrentLocationPickAreaName_Translatable()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var loc = whs.DefaultLocation;
			var area = loc.PickingArea;
			area.WA_Name = "Test Area";

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("CurrentLocationPickAreaName in English.", "Test Area", docketLine.CurrentLocationPickAreaName);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "Test Area").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("CurrentLocationPickAreaName in Chinese.", "测试区", docketLine.CurrentLocationPickAreaName);
			}
		}

		#endregion

		#region TestCurrentLocation

		public void TestCurrentLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", ResString.GetMultilingualString("Test", "Test"), false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			nonDockDoorLocation.PickingArea.WA_AreaType = "ABC";
			nonDockDoorLocation.PickingArea.WA_Name = "NonDDLAreaName";
			nonDockDoorLocation.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			dockDoorLocation.PickingArea.WA_AreaType = "XYZ";
			dockDoorLocation.WLV_LocationStatus = LocationStatus.Codes.Held;
			dockDoorLocation.PickingArea.WA_Name = "DDLAreaName";

			var arrivalDate = ZDateTimeOffset.Today;
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var pendingLine = CreateReceiveLineForDifferentStatuses(receive, data.Part1, null, 1m, "A", InventoryStatus.Codes.Pending);
			var receivedLine = CreateReceiveLineForDifferentStatuses(receive, data.Part1, dockDoorLocation, 2m, "B", InventoryStatus.Codes.Received, arrivalDate);
			var putawayLine = CreateReceiveLineForDifferentStatuses(receive, data.Part2, nonDockDoorLocation, 4m, "D", InventoryStatus.Codes.Putaway, arrivalDate);
			var arrivedLine = CreateReceiveLineForDifferentStatuses(receive, data.Part2, null, 5m, "E", InventoryStatus.Codes.Arrived, arrivalDate);
			Factory.Save();
			AssertEquals("Precondition: ArrivalDate - Received", arrivalDate, receivedLine.WE_AdjustmentArrivalDate);
			AssertEquals("Precondition: ArrivalDate - Arrived", arrivalDate, arrivedLine.WE_AdjustmentArrivalDate);
			AssertEquals("", pendingLine.CurrentLocationString);
			AssertCurrentLocationProperties(receivedLine, dockDoorLocation);
			AssertCurrentLocationProperties(putawayLine, nonDockDoorLocation);
			AssertCurrentLocationProperties(arrivedLine, null);
		}

		public void TestCurrentLocation_FixedWidthLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z040302");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, warehouse.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			Factory.Save();
			AssertEquals("Z-04-03-02", receiveLine.CurrentLocationString);
		}

		protected void AssertCurrentLocationProperties(WhsDocketLine line, WhsLocation expectedLocation)
		{
			if (expectedLocation != null)
			{
				AssertEquals(expectedLocation.PK, line.CurrentLocation.PK);
				AssertEquals(expectedLocation?.WLV_LocationString_UserFriendly ?? string.Empty, line.CurrentLocationString);
				AssertEquals(expectedLocation.PickingArea.WA_Name, line.CurrentLocationPickAreaName);
				AssertEquals(expectedLocation.PickingArea.WA_AreaType, line.CurrentLocationPickAreaType);
				AssertEquals(expectedLocation.LocationStatuses.GetDescriptionFromCode(expectedLocation.WLV_LocationStatus), line.CurrentLocationStatus);
			}
			else
			{
				AssertNull(line.CurrentLocation);
				AssertEquals("", line.CurrentLocationString);
				AssertEquals("", line.CurrentLocationPickAreaName);
				AssertEquals("", line.CurrentLocationPickAreaType);
				AssertEquals("", line.CurrentLocationPickAreaType);
				AssertEquals("", line.CurrentLocationStatus);
			}
		}

		WhsDocketLine CreateReceiveLineForDifferentStatuses(WhsReceive receive, OrgSupplierPart part, WhsLocation location, ZDecimal qty, string palletID, string inventoryStatus, ZDateTimeOffset? arrivalDate = null)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, qty);
			var receiveLine = inventory.InDocketLine;
			receiveLine.WE_AdjustmentArrivalDate = arrivalDate != null && !arrivalDate.Value.IsEmpty ? arrivalDate.Value : receiveLine.WE_AdjustmentArrivalDate;
			receiveLine.WE_WL = location != null ? location.PK : ZGuid.Empty;
			receiveLine.WE_PalletID = palletID;
			receiveLine.WE_OriginalInventoryStatus = inventoryStatus;
			AssertEquals("Precondition", receiveLine.WE_OriginalInventoryStatus, inventoryStatus);
			return receiveLine;
		}

		#endregion

		#region TestWE_WL

		public void TestWE_WL()
		{
			var whs = Helper.CreateWarehouse("1", "A", 3, 1);
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(Docket, false);
			AssertEquals("Precondition", ZGuid.Empty, docketLine.WE_WL);

			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_WL.Name, WhsInventoryViewSchema.WI_WL.Name, locations[0].PK, locations[1].PK, locations[2].PK);
		}

		#endregion

		#region TestLocationString

		public void TestLocationString()
		{
			TestLocationStringCore();
		}

		protected virtual void TestLocationStringCore()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);
			Factory.Save();

			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			docket.WD_WW_Whs = whs1.PK;
			AssertEquals(whs1.PK, docketLine.WarehousePK);

			docketLine.LocationString = "A-1";
			AssertEquals("A-1", docketLine.LocationString);
			AssertEquals(whs1.FindLocation("A-1"), docketLine.Location);

			docketLine.LocationString = "A-2";
			AssertEquals("A-2", docketLine.LocationString);
			AssertEquals(whs1.FindLocation("A-2"), docketLine.Location);

			docketLine.LocationString = "";
			AssertEquals("", docketLine.LocationString);
			AssertEquals(null, docketLine.Location);

			docketLine.LocationString = "BLA";
			AssertEquals("BLA", docketLine.LocationString);
			AssertEquals(null, docketLine.Location);
		}

		public void TestLocationString_FixedWidthLocation()
		{
			TestLocationString_FixedWidthLocationCore();
		}

		protected virtual void TestLocationString_FixedWidthLocationCore()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "A00200201");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "A00100101");
			Factory.Save();

			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(warehouse.PK, docketLine.WarehousePK);

			docketLine.LocationString = "A00200201";
			AssertEquals("A-002-002-01", docketLine.LocationString);
			AssertEquals(location1, docketLine.Location);

			docketLine.LocationString = "A00100101";
			AssertEquals("A-001-001-01", docketLine.LocationString);
			AssertEquals(location2, docketLine.Location);

			docketLine.LocationString = "";
			AssertEquals("", docketLine.LocationString);
			AssertEquals(null, docketLine.Location);

			docketLine.LocationString = "A-001-001-01";
			AssertEquals("A-001-001-01", docketLine.LocationString);
			AssertEquals(location2, docketLine.Location);

			docketLine.LocationString = "BLA";
			AssertEquals("BLA", docketLine.LocationString);
			AssertEquals(null, docketLine.Location);
		}

		#endregion

		#region TestLocationString_MaxLength

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestLocationString_MaxLength()
		{
			DocketLine = GetNewBusinessObject();
			try
			{
				DocketLine.LocationString = ZString.Replicate('A', DocketLine.LocationStringInfo.MaxLength + 1);
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		#endregion

		#region TestLocationStringInfo

		public void TestLocationStringInfo()
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("LocationString", docketLine.LocationStringInfo.Name);
			AssertEquals(36, docketLine.LocationStringInfo.MaxLength);
		}

		#endregion

		#endregion

		#region Find Attributes

		public void TestUseChosenInventoryRow()
		{
			TestUseChosenInventoryRowCore();
		}

		protected virtual void TestUseChosenInventoryRowCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			SetInventoryDataForUseChosenInventoryRowMethod(data.Line111);
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			docketLine.CustomsData.WB_EntryLineNo = 1;
			docketLine.CustomsData.WB_EntryKey = "DEF";
			docketLine.WE_PerPackageQty = 3m;
			docketLine.WE_PackageGroupId = "321";
			docketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			docketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			docketLine.UseChosenInventoryRowFindAttributes(data.Line111);
			AssertUseChosenInventoryRowHasSetProperties(data.Line111, docketLine);
		}

		public virtual void TestUseChosenInventoryRowErrorNotifications()
		{
			var docket = GetNewWhsDocket();
			var docketLine = GetNewBusinessObject(docket);
			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			docketLine.UseChosenInventoryRowFindAttributes(Factory.New<WhsInventoryView>());
			AssertEquals(true, ((NotificationBuffer)docketLine.Docket.NotificationManager.Peek).ContainsNotificationType(CannotUseChosenInventoryError));

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			((NotificationBuffer)docket.NotificationManager.Peek).Clear();
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			docketLine.UseChosenInventoryRowFindAttributes(Factory.New<WhsInventoryView>());
			AssertEquals(true, ((NotificationBuffer)docket.NotificationManager.Peek).ContainsNotificationType(CannotUseChosenInventoryError));

			((NotificationBuffer)docket.NotificationManager.Peek).Clear();
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			docketLine.UseChosenInventoryRowFindAttributes(Factory.New<WhsInventoryView>());
			AssertEquals(false, ((NotificationBuffer)docket.NotificationManager.Peek).ContainsNotificationType(CannotUseChosenInventoryError));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestUseChosenInventoryRowHandlesNullInventory()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			docketLine.UseChosenInventoryRowFindAttributes(null);
		}

		protected virtual ErrorType CannotUseChosenInventoryError => WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled;

		protected virtual void SetInventoryDataForUseChosenInventoryRowMethod(WhsInventoryView inventory)
		{
			inventory.WI_PartAttrib1 = "PA1";
			inventory.WI_CustomAttrib_2 = "CA2";
			inventory.WI_PalletID = "PID1";
		}

		protected virtual void AssertUseChosenInventoryRowHasSetProperties(WhsInventoryView expected, WhsDocketLine actual)
		{
			AssertEquals("Product: ", expected.WI_OP, actual.WE_OP);
			AssertEquals("Part Attribute 1: ", expected.WI_PartAttrib1, actual.WE_PartAttrib1);
			AssertEquals("Custom Attribute 2: ", expected.WI_CustomAttrib_2, actual.WE_CustomAttrib2);
			AssertEquals("WB_EntryLineNo", (ZShort)1, actual.CustomsData.WB_EntryLineNo);
			AssertEquals("WB_EntryKey", "DEF", actual.CustomsData.WB_EntryKey);
			AssertEquals("WE_PerPackageQty", 3m, actual.WE_PerPackageQty);
			AssertEquals("WE_PackageGroupId", "321", actual.WE_PackageGroupId);
			AssertEquals("WE_WHC_NKOriginalInventoryHeldCode", InventoryHoldCodes.Codes.Damaged, actual.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("WE_WHC_NKCurrentInventoryHeldCode", InventoryHoldCodes.Codes.Held, actual.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region TestOperationalActionsFieldVisibility

		public void TestOperationalActionsFieldVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_F3_NKPackType.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_ClientOrderedUnits.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_TransactionQuantity.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_ExpiryDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_PartAttrib1.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_PartAttrib2.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_PartAttrib3.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_SerialNumber.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_BondedEntryKey.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_PackingDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_AdjustmentArrivalDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_DocketLineStatus.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_OriginalInventoryStatus.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_CurrentInventoryStatus.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_FinalisedDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_PalletID.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_ReceiveCrossDockOrderNo.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_RequiredByDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_TransferFromPalletId.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_IsOriginalInventory.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_StockOnHand.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty("WE_PackQuantity")).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty("WE_PackageGroupId")).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(WhsDocketLineSchema.WE_PerPackageQty.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(nameof(WhsDocketLine.WE_CurrentHoldReason))).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(nameof(WhsDocketLine.HeldCodeChangeQuantity))).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(nameof(WhsDocketLine.HoldReasonToChangeTo))).ReadOnly);
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsDocketLine).GetProperty(WhsDocketLine.Schema.OriginalHeldCode)));
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsDocketLine).GetProperty(nameof(WhsDocketLine.WE_P9_Task))).ReadOnly);

			// These fields are currently not supported, should become readonly in future if architecture supports them
			var fieldsTester = ObjectFactory.Get<IOperationalActionFieldTester>();
			AssertEquals(true, fieldsTester.IsFieldUnsupported(GetExpectedBusinessObjectType(), WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode.Name));
			AssertEquals(true, fieldsTester.IsFieldUnsupported(GetExpectedBusinessObjectType(), WhsDocketLineSchema.WE_WHC_NKOriginalInventoryHeldCode.Name));
			TestOperationalActionsFieldVisibilityCore();
		}

		protected virtual void TestOperationalActionsFieldVisibilityCore()
		{
		}

		#endregion

		#region IWhsDocketLine

		public void TestIWhsDocketLineProperties()
		{
			var originalDocketLinePK = ZGuid.NewZGuid();
			var locationPK = ZGuid.NewZGuid();
			var arrivalDate = new ZDateTimeOffset(2019, 12, 5);
			var expiredDate = new ZDate(2019, 4, 27);
			var packingDate = new ZDate(2017, 7, 2);
			var finalisedDate = new ZDateTimeOffset(2019, 8, 2);

			var data = new TestDataSimpleEnvironment(Factory);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var docketLine = Helper.CreateWhsReceiveLine(docket, data.Part1, 15m);
			data.Part1.OP_StockKeepingUnit = "BOX";
			docketLine.WE_F3_NKPackType = "BOX";
			Factory.Save();

			var packType = docketLine.PackType;
			packType.F3_UOMType = "SPC";
			Factory.Save();

			docketLine.WE_AdjustmentArrivalDate = arrivalDate;
			docketLine.WE_BondedEntryKey = "BondedKeyTest";
			docketLine.WE_DocketLineStatus = DocketStatus.Codes.Error;
			docketLine.WE_ExpiryDate = expiredDate;
			docketLine.WE_ExtendedLinePrice = 45.5m;
			docketLine.WE_WHC_NKOriginalInventoryHeldCode = "ERU";
			docketLine.WE_WHC_NKCurrentInventoryHeldCode = "WER";
			docketLine.WE_PackageGroupId = "EDRS";
			docketLine.WE_PackingDate = packingDate;
			docketLine.WE_PartAttrib1 = "Blue";
			docketLine.WE_PartAttrib2 = "Large";
			docketLine.WE_PartAttrib3 = "SN0254185";
			docketLine.WE_PalletID = "PLT432";
			docketLine.WE_PerPackageQty = 84m;
			docketLine.WE_WE_ParentDocketLine = ZGuid.BrettsGuid;
			docketLine.WE_WE_OriginalDocketLineForRating = originalDocketLinePK;
			docketLine.WE_WL = locationPK;
			docketLine.WE_LineNo = 83;
			docketLine.WE_SubLineNo = 68;
			docketLine.WE_CustomAttrib1 = "CustomAttrib";
			docketLine.WE_OriginalInventoryStatus = "FRT";
			docketLine.WE_CurrentInventoryStatus = "FES";

			CombineAssertions(() =>
			{
				AssertEquals("Interface PackUOM is correct", "SPC", ((IWhsDocketLine)docketLine).PackUOM);
				AssertEquals("Interface WE_WD is correct", docket.PK, ((IWhsDocketLine)docketLine).WE_WD);
				AssertEquals("Interface WE_OP is correct", data.Part1.PK, ((IWhsDocketLine)docketLine).WE_OP);
				AssertEquals("Interface WE_AdjustmentArrivalDate is correct", arrivalDate, ((IWhsDocketLine)docketLine).WE_AdjustmentArrivalDate);
				AssertEquals("Interface WE_BondedEntryKey is correct", "BondedKeyTest", ((IWhsDocketLine)docketLine).WE_BondedEntryKey);
				AssertEquals("Interface WE_DocketLineStatus is correct", DocketStatus.Codes.Error, ((IWhsDocketLine)docketLine).WE_DocketLineStatus);
				AssertEquals("Interface WE_ExpiryDate is correct", expiredDate, ((IWhsDocketLine)docketLine).WE_ExpiryDate);
				AssertEquals("Interface WE_ExtendedLinePrice is correct", 45.5m, ((IWhsDocketLine)docketLine).WE_ExtendedLinePrice);
				AssertEquals("Interface WE_F3_NKPackType is correct", "BOX", ((IWhsDocketLine)docketLine).WE_F3_NKPackType);
				AssertEquals("Interface WE_OriginalInventoryStatus is correct", "FRT", ((IWhsDocketLine)docketLine).WE_OriginalInventoryStatus);
				AssertEquals("Interface WE_CurrentInventoryStatus is correct", "FES", ((IWhsDocketLine)docketLine).WE_CurrentInventoryStatus);
				AssertEquals("Interface WE_WHC_NKOriginalInventoryHeldCode is correct", "ERU", ((IWhsDocketLine)docketLine).WE_WHC_NKOriginalInventoryHeldCode);
				AssertEquals("Interface WE_WHC_NKCurrentInventoryHeldCode is correct", "WER", ((IWhsDocketLine)docketLine).WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals("Interface WE_PackageGroupId is correct", "EDRS", ((IWhsDocketLine)docketLine).WE_PackageGroupId);
				AssertEquals("Interface WE_PackingDate is correct", packingDate, ((IWhsDocketLine)docketLine).WE_PackingDate);
				AssertEquals("Interface WE_PartAttrib1 is correct", "Blue", ((IWhsDocketLine)docketLine).WE_PartAttrib1);
				AssertEquals("Interface WE_PartAttrib2 is correct", "Large", ((IWhsDocketLine)docketLine).WE_PartAttrib2);
				AssertEquals("Interface WE_PartAttrib3 is correct", "SN0254185", ((IWhsDocketLine)docketLine).WE_PartAttrib3);
				AssertEquals("Interface WE_PalletID is correct", "PLT432", ((IWhsDocketLine)docketLine).WE_PalletID);
				AssertEquals("Interface WE_PerPackageQty is correct", 84m, ((IWhsDocketLine)docketLine).WE_PerPackageQty);
				AssertEquals("Interface WE_WE_ParentDocketLine is correct", ZGuid.BrettsGuid, ((IWhsDocketLine)docketLine).WE_WE_ParentDocketLine);
				AssertEquals("Interface WE_WE_OriginalDocketLineForRating is correct", originalDocketLinePK, ((IWhsDocketLine)docketLine).WE_WE_OriginalDocketLineForRating);
				AssertEquals("Interface WE_WL is correct", locationPK, ((IWhsDocketLine)docketLine).WE_WL);
				AssertEquals("Interface WE_LineNo is correct", (ZShort)83, ((IWhsDocketLine)docketLine).WE_LineNo);
				AssertEquals("Interface WE_SubLineNo is correct", (ZShort)68, ((IWhsDocketLine)docketLine).WE_SubLineNo);
				AssertEquals("Interface WE_CustomAttrib1 is correct", "CustomAttrib", ((IWhsDocketLine)docketLine).WE_CustomAttrib1);
			});

			docketLine.WE_PackQuantity = 27m;
			AssertEquals("Interface WE_PackQuantity is correct", 27m, ((IWhsDocketLine)docketLine).WE_PackQuantity);

			docketLine.WE_ClientOrderedUnits = 34m;
			AssertEquals("Interface WE_ClientOrderedUnits is correct", 34m, ((IWhsDocketLine)docketLine).WE_ClientOrderedUnits);

			docketLine.WE_StockOnHand = 41m;
			AssertEquals("Interface WE_StockOnHand is correct", 41m, ((IWhsDocketLine)docketLine).WE_StockOnHand);

			docketLine.WE_TransactionQuantity = 54m;
			AssertEquals("Interface WE_TransactionQuantity is correct", 54m, ((IWhsDocketLine)docketLine).WE_TransactionQuantity);

			docketLine.WE_FinalisedDate = finalisedDate;
			AssertEquals("Interface WE_FinalisedDate is correct", finalisedDate, ((IWhsDocketLine)docketLine).WE_FinalisedDate);
		}

		#endregion

		#region ILineAttributes Members

		public void TestILineAttributesExpiryDate()
		{
			var date = ZDate.Today.AddDays(10);
			var docketLine = GetNewBusinessObject();
			docketLine.WE_ExpiryDate = date;
			AssertEquals(date, ((ILineAttributes)docketLine).ExpiryDate);
		}

		public void TestILineAttributesPackingDate()
		{
			var date = ZDate.Today.AddDays(-10);
			var docketLine = GetNewBusinessObject();
			docketLine.WE_PackingDate = date;
			AssertEquals(date, ((ILineAttributes)docketLine).PackingDate);
		}

		public void TestILineAttributesBondedEntryKey()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_BondedEntryKey = "BEK-1";
			AssertEquals("BEK-1", ((ILineAttributes)docketLine).BondedEntryKey);
		}

		public void TestILineAttributesPartAttrib1()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_PartAttrib1 = "PA1";
			AssertEquals("PA1", ((ILineAttributes)docketLine).PartAttrib1);
		}

		public void TestILineAttributesPartAttrib2()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_PartAttrib2 = "PA2";
			AssertEquals("PA2", ((ILineAttributes)docketLine).PartAttrib2);
		}

		public void TestILineAttributesPartAttrib3()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_PartAttrib3 = "PA3";
			AssertEquals("PA3", ((ILineAttributes)docketLine).PartAttrib3);
		}

		public void TestILineAttributesSerialNumber()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_SerialNumber = "SNN";
			AssertEquals("SNN", ((ILineAttributes)docketLine).SerialNumber);
		}

		public void TestILineAttributesAllocationKey()
		{
			var docketLine = GetNewBusinessObject();
			docketLine.WE_AllocationKey = "ALO";
			AssertEquals("ALO", ((ILineAttributes)docketLine).AllocationKey);
		}

		public void TestILineAttributesSetAttributes()
		{
			var expiry = ZDate.Today.AddDays(1);
			var packing = ZDate.Today.AddDays(-1);

			var docketLine = GetNewBusinessObject();
			docketLine.WE_ExpiryDate = expiry;
			docketLine.WE_PackingDate = packing;
			docketLine.WE_BondedEntryKey = "BEK-1";
			docketLine.WE_PartAttrib1 = "PA1";
			docketLine.WE_PartAttrib2 = "PA2";
			docketLine.WE_PartAttrib3 = "PA3";
			docketLine.WE_SerialNumber = "SNN";
			docketLine.WE_AllocationKey = "ALO";

			var docketLineClone = (WhsDocketLine)GetNewBusinessObject();
			docketLineClone.SetAttributes(docketLine);
			AssertEquals(true, AttributeComparer.Compare(docketLineClone, docketLine));
		}

		#endregion

		#region ILineCustomAttributes Members

		public void TestICustomLineAttributesCustomAttrib1()
		{
			DocketLine.WE_CustomAttrib1 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)DocketLine).CustomAttrib1);
		}

		public void TestICustomLineAttributesCustomAttrib2()
		{
			DocketLine.WE_CustomAttrib2 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)DocketLine).CustomAttrib2);
		}

		public void TestICustomLineAttributesCustomAttrib3()
		{
			DocketLine.WE_CustomAttrib3 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)DocketLine).CustomAttrib3);
		}

		public void TestICustomLineAttributesCustomAttrib4()
		{
			DocketLine.WE_CustomAttrib4 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)DocketLine).CustomAttrib4);
		}

		public void TestICustomLineAttributesCustomAttrib5()
		{
			DocketLine.WE_CustomAttrib5 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)DocketLine).CustomAttrib5);
		}

		public void TestICustomLineAttributesCustomAttrib6()
		{
			DocketLine.WE_CustomAttrib6 = "CA";
			AssertEquals("CA", ((ILineCustomAttributes)DocketLine).CustomAttrib6);
		}

		public void TestICustomLineAttributesCustomDecimal1()
		{
			DocketLine.WE_CustomDecimal1 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)DocketLine).CustomDecimal1);
		}

		public void TestICustomLineAttributesCustomDecimal2()
		{
			DocketLine.WE_CustomDecimal2 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)DocketLine).CustomDecimal2);
		}

		public void TestICustomLineAttributesCustomDecimal3()
		{
			DocketLine.WE_CustomDecimal3 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)DocketLine).CustomDecimal3);
		}

		public void TestICustomLineAttributesCustomDecimal4()
		{
			DocketLine.WE_CustomDecimal4 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)DocketLine).CustomDecimal4);
		}

		public void TestICustomLineAttributesCustomDecimal5()
		{
			DocketLine.WE_CustomDecimal5 = 10.5m;
			AssertEquals(10.5m, ((ILineCustomAttributes)DocketLine).CustomDecimal5);
		}

		public void TestILineCustomAttributesCustomDate1()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			DocketLine.WE_CustomDate1 = date;
			AssertEquals(date, ((ILineCustomAttributes)DocketLine).CustomDate1);
		}

		public void TestILineCustomAttributesCustomDate2()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			DocketLine.WE_CustomDate2 = date;
			AssertEquals(date, ((ILineCustomAttributes)DocketLine).CustomDate2);
		}

		public void TestILineCustomAttributesCustomDate3()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			DocketLine.WE_CustomDate3 = date;
			AssertEquals(date, ((ILineCustomAttributes)DocketLine).CustomDate3);
		}

		public void TestILineCustomAttributesCustomDate4()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			DocketLine.WE_CustomDate4 = date;
			AssertEquals(date, ((ILineCustomAttributes)DocketLine).CustomDate4);
		}

		public void TestILineCustomAttributesCustomDate5()
		{
			ZDateTime date = ZDateTime.Today.AddDays(10);
			DocketLine.WE_CustomDate5 = date;
			AssertEquals(date, ((ILineCustomAttributes)DocketLine).CustomDate5);
		}

		public void TestILineCustomAttributesCustomFlag1()
		{
			DocketLine.WE_CustomFlag1 = true;
			AssertEquals(true, ((ILineCustomAttributes)DocketLine).CustomFlag1);
		}

		public void TestILineCustomAttributesCustomFlag2()
		{
			DocketLine.WE_CustomFlag2 = true;
			AssertEquals(true, ((ILineCustomAttributes)DocketLine).CustomFlag2);
		}

		public void TestILineCustomAttributesCustomFlag3()
		{
			DocketLine.WE_CustomFlag3 = true;
			AssertEquals(true, ((ILineCustomAttributes)DocketLine).CustomFlag3);
		}

		public void TestILineCustomAttributesCustomFlag4()
		{
			DocketLine.WE_CustomFlag4 = true;
			AssertEquals(true, ((ILineCustomAttributes)DocketLine).CustomFlag4);
		}

		public void TestILineCustomAttributesCustomFlag5()
		{
			DocketLine.WE_CustomFlag5 = true;
			AssertEquals(true, ((ILineCustomAttributes)DocketLine).CustomFlag5);
		}

		public void TestILineCustomAttributesCustomTextBlob1()
		{
			DocketLine.WE_CustomTextBlob1 = "TEXTBLOB";
			AssertEquals("TEXTBLOB", ((ILineCustomAttributes)DocketLine).CustomTextBlob1);
		}

		public void TestILineCustomAttributesSetCustomAttributes()
		{
			ZDateTime date1 = ZDateTime.Today.AddDays(1);
			ZDateTime date2 = ZDateTime.Today.AddDays(2);
			ZDateTime date3 = ZDateTime.Today.AddDays(3);
			ZDateTime date4 = ZDateTime.Today.AddDays(4);
			ZDateTime date5 = ZDateTime.Today.AddDays(5);

			DocketLine.WE_CustomAttrib1 = "CA1";
			DocketLine.WE_CustomAttrib2 = "CA2";
			DocketLine.WE_CustomAttrib3 = "CA3";
			DocketLine.WE_CustomAttrib4 = "CA4";
			DocketLine.WE_CustomAttrib5 = "CA5";
			DocketLine.WE_CustomAttrib6 = "CA6";

			DocketLine.WE_CustomDecimal1 = 1.1m;
			DocketLine.WE_CustomDecimal2 = 2.2m;
			DocketLine.WE_CustomDecimal3 = 3.3m;
			DocketLine.WE_CustomDecimal4 = 4.4m;
			DocketLine.WE_CustomDecimal5 = 5.5m;

			DocketLine.WE_CustomDate1 = date1;
			DocketLine.WE_CustomDate2 = date2;
			DocketLine.WE_CustomDate3 = date3;
			DocketLine.WE_CustomDate4 = date4;
			DocketLine.WE_CustomDate5 = date5;

			DocketLine.WE_CustomFlag1 = true;
			DocketLine.WE_CustomFlag2 = true;
			DocketLine.WE_CustomFlag3 = true;
			DocketLine.WE_CustomFlag4 = true;
			DocketLine.WE_CustomFlag5 = true;

			DocketLine.WE_CustomTextBlob1 = "TEXTBLOB1";

			var docketLine1 = GetNewBusinessObject();
			docketLine1.SetCustomAttributes(DocketLine);
			AssertEquals(true, new CustomAttributeComparer().Compare(docketLine1, DocketLine));
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		public void TestIsInventoryAdjustedOutOnSiblings()
		{
			TestIsInventoryAdjustedOutOnSiblingsCore();
		}

		protected virtual void TestIsInventoryAdjustedOutOnSiblingsCore()
		{
			AssertEquals(false, DocketLine.IsInventoryAdjustedOutOnSiblings(Factory.New<WhsInventoryView>()));
		}

		public virtual void TestIsRegisteredForUniqueSerialNumberChecking()
		{
			DocketLine = GetNewBusinessObject();
			AssertEquals(false, DocketLine.IsRegisteredForUniqueSerialNumberChecking);
		}

		public void TestIsValidForUniqueSerialNumberChecking()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var product = Helper.CreateProduct(client, "P1");
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Serial, true);

			var docket = GetNewWhsDocket(client, null);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = product.PK;
			docketLine.WE_SerialNumber = "PA1";

			AssertEquals("Should be validated", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			client.PartAttributeManager.SetProductToUseAttribute(product, 6, false);
			AssertEquals("Should not be validated - Serial # not used on product", false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
			client.PartAttributeManager.SetProductToUseAttribute(product, 6, true);
			AssertEquals("Should be validated - Confirmation", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals("Should not be validated - Docket is finalized", false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals("Should be validated - Confirmation", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			docketLine.WE_OP = ZGuid.Empty;
			AssertEquals("Should not be validated - Product is null", false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
			docketLine.WE_OP = product.PK;
			AssertEquals("Should be validated - Confirmation", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			docketLine.WE_WD = ZGuid.Empty;
			AssertEquals("Should not be validated - Docket is null", false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
			docketLine.WE_WD = docket.PK;
			AssertEquals("Should be validated - Confirmation", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("Should not be validated - Client is null", false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
			docket.WD_OH_Client = client.PK;
			AssertEquals("Should be validated - Confirmation", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			docketLine.WE_SerialNumber = "";
			AssertEquals("Should not be validated - Value is empty", false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
			docketLine.WE_SerialNumber = "PA1";
			AssertEquals("Should be validated - Confirmation", true, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));

			Helper.SetProductAttributeUse(client, product, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(false, docketLine.IsValidForUniqueSerialNumberChecking(docketLine.WE_SerialNumber));
		}

		public void TestIsSerialNumberUsedOnThis()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var product = Helper.CreateProduct(client, "P1");
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Serial, true);

			var docket = GetNewWhsDocket(client, null);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = product.PK;
			docketLine.WE_SerialNumber = "PA1";

			AssertEquals("IsSerialNumberUsedOnThis false as Serial is different", false, docketLine.IsSerialNumberUsedOnThis("PA3"));

			AssertEquals("IsSerialNumberUsedOnThis true as Serial is turned on", true, docketLine.IsSerialNumberUsedOnThis("PA1"));

			client.PartAttributeManager.SetProductToUseAttribute(docketLine.SupplierPart, 6, false);
			AssertEquals("IsSerialNumberUsedOnThis false as Serial is turned off", false, docketLine.IsSerialNumberUsedOnThis("PA1"));
		}

		public void TestIsSerialNumberUsedOnSiblings()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var product = Helper.CreateProduct(client, "P1");
			client.MiscServ.OM_IMUseSerialNumber = true;
			client.PartAttributeManager.SetProductToUseAttribute(product, 1, true);
			client.PartAttributeManager.SetProductToUseAttribute(product, 2, true);
			client.PartAttributeManager.SetProductToUseAttribute(product, 3, true);
			client.PartAttributeManager.SetProductToUseAttribute(product, 6, true);

			var docket = GetNewWhsDocket(client, null);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.WE_OP = product.PK;
			docketLine.WE_PartAttrib1 = "PA1";
			docketLine.WE_PartAttrib2 = "PA2";
			docketLine.WE_PartAttrib3 = "PA3";
			docketLine.WE_SerialNumber = "SN1";

			var product2 = Helper.CreateProduct(client, "P2");
			client.PartAttributeManager.SetProductToUseAttribute(product2, 1, true);
			client.PartAttributeManager.SetProductToUseAttribute(product2, 2, true);
			client.PartAttributeManager.SetProductToUseAttribute(product2, 3, true);
			client.PartAttributeManager.SetProductToUseAttribute(product2, 6, true);

			var docketLine1 = docket.Lines.AddNew();
			var docketLine2 = docket.Lines.AddNew();
			docketLine1.WE_OP = product.PK;
			docketLine2.WE_OP = product2.PK;
			docketLine1.WE_TransactionQuantity = 1;
			docketLine2.WE_TransactionQuantity = 1;

			docketLine1.WE_PartAttrib1 = "1PA1";
			docketLine1.WE_PartAttrib2 = "1PA2";
			docketLine1.WE_PartAttrib3 = "1PA3";
			docketLine1.WE_SerialNumber = "1SN1";
			docketLine2.WE_PartAttrib1 = "2PA1";
			docketLine2.WE_PartAttrib2 = "2PA2";
			docketLine2.WE_PartAttrib3 = "2PA3";
			docketLine2.WE_SerialNumber = "2SN1";

			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("PA1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("PA2"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("PA3"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("SN1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("1PA1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("1PA2"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("1PA3"));
			AssertEquals(IsRegisteredForSerialCheck, docketLine.IsSerialNumberUsedOnSiblings("1SN1"));
			AssertEquals("Attributes can't be serials.", false, docketLine.IsSerialNumberUsedOnSiblings("2PA1"));
			AssertEquals("Attributes can't be serials.", false, docketLine.IsSerialNumberUsedOnSiblings("2PA2"));
			AssertEquals("Attributes can't be serials.", false, docketLine.IsSerialNumberUsedOnSiblings("2PA3"));
			AssertEquals("No match because product is different", false, docketLine.IsSerialNumberUsedOnSiblings("2SN1"));

			WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI");

			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("PA1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("PA2"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("PA3"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("SN1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("1PA1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("1PA2"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("1PA3"));
			AssertEquals(IsRegisteredForSerialCheck, docketLine.IsSerialNumberUsedOnSiblings("1SN1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("2PA1"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("2PA2"));
			AssertEquals(false, docketLine.IsSerialNumberUsedOnSiblings("2PA3"));
			AssertEquals(IsRegisteredForSerialCheck, docketLine.IsSerialNumberUsedOnSiblings("2SN1"));
		}

		protected virtual bool IsRegisteredForSerialCheck { get { return false; } }

		#endregion

		#region TestAutoCalculatePriceReadOnly

		public void TestAutoCalculatePriceReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			client.MiscServ.OM_WhsIsRecalculateOrderPricing = false;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			var extendedLinePriceColumn = orderLine.WE_ExtendedLinePriceInfo;
			AssertEquals(extendedLinePriceColumn.Name + " should not be readonly", false, extendedLinePriceColumn.ReadOnly);

			client.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			AssertEquals(extendedLinePriceColumn.Name + " should be readonly", true, extendedLinePriceColumn.ReadOnly);
		}

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public void TestIProcessHandlingInfoProvider()
		{
			IProcessHandlingInfoProvider docketLineHandlingInfoProvider = GetNewBusinessObject();

			AssertEquals(ExpectedDocketLineHandlingInfoProvider, docketLineHandlingInfoProvider.ProcessHandlingInfo.GetType());
		}

		protected virtual Type ExpectedDocketLineHandlingInfoProvider
		{
			get { return typeof(WhsDocketLineProcessHandlingInfo); }
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		public void TestICustomLabelsConfigOrgProviderConfigOrg()
		{
			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			var org = Factory.New<OrgHeader>();
			docket.WD_OH_Client = org.PK;

			if (CustomFieldsSupported)
			{
				AssertEquals(org, ((ICustomLabelsConfigOrgProvider)docketLine).ConfigOrg);
			}
			else
			{
				AssertNull(((ICustomLabelsConfigOrgProvider)docketLine).ConfigOrg);
			}
		}

		protected virtual bool CustomFieldsSupported => true;

		public void TestICustomLabelsConfigOrgProviderConfigOrgChanged()
		{
			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			var org = Factory.New<OrgHeader>();
			var configOrgChangedCalled = false;

			((ICustomLabelsConfigOrgProvider)docketLine).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			docket.WD_OH_Client = org.PK;

			if (CustomFieldsSupported)
			{
				AssertEquals("ConfigOrgChanged called", true, configOrgChangedCalled);
				AssertEquals(org, ((ICustomLabelsConfigOrgProvider)docketLine).ConfigOrg);
			}
			else
			{
				AssertEquals("ConfigOrgChanged *not* called", false, configOrgChangedCalled);
				AssertNull(((ICustomLabelsConfigOrgProvider)docketLine).ConfigOrg);
			}
		}

		#endregion

		#region ICustomLabelsProvider Members

		public void TestCustomLabelsProvider()
		{
			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			docket.WD_OH_Client = org.PK;

			var provider = new WhsDocketLine.CustomLabelsProvider(docketLine);
			AssertEquals(docketLine, provider.ConfigOrgProvider);

			var list = provider.GetCustomFields(org, Factory);
			AssertEquals("the client", list.ConfigOrgLocatedAt);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
						WhsDocketLineSchema.WE_CustomAttrib1.Name,
						WhsDocketLineSchema.WE_CustomAttrib2.Name,
						WhsDocketLineSchema.WE_CustomAttrib3.Name,
						WhsDocketLineSchema.WE_CustomAttrib4.Name,
						WhsDocketLineSchema.WE_CustomAttrib5.Name,
						WhsDocketLineSchema.WE_CustomAttrib6.Name,
						WhsDocketLineSchema.WE_CustomDate1.Name,
						WhsDocketLineSchema.WE_CustomDate2.Name,
						WhsDocketLineSchema.WE_CustomDate3.Name,
						WhsDocketLineSchema.WE_CustomDate4.Name,
						WhsDocketLineSchema.WE_CustomDate5.Name,
						WhsDocketLineSchema.WE_CustomDecimal1.Name,
						WhsDocketLineSchema.WE_CustomDecimal2.Name,
						WhsDocketLineSchema.WE_CustomDecimal3.Name,
						WhsDocketLineSchema.WE_CustomDecimal4.Name,
						WhsDocketLineSchema.WE_CustomDecimal5.Name,
						WhsDocketLineSchema.WE_CustomFlag1.Name,
						WhsDocketLineSchema.WE_CustomFlag2.Name,
						WhsDocketLineSchema.WE_CustomFlag3.Name,
						WhsDocketLineSchema.WE_CustomFlag4.Name,
						WhsDocketLineSchema.WE_CustomFlag5.Name,
						WhsDocketLineSchema.WE_CustomTextBlob1.Name,
				}, list.Cast<CustomLabelInfo>().Select(i => i.PropertyName));
		}

		#endregion

		#region IEDocsProvider Members

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			var lineEDocProvider = ((IEDocsProvider)GetNewBusinessObject()).GetEDocsProviderSupporter();
			AssertNotNull(lineEDocProvider);
			AssertType<JobInvoicingEDocsProviderSupporter>(lineEDocProvider);
		}

		public void TestIDocumentSupportable_DocumentSupporter()
		{
			var line = GetNewBusinessObject();
			var lineDocSupportable = (IDocumentSupportable)line;
			if (line.Inventory.Count > 0) // receive, adjustment and transfer lines.
			{
				AssertNotNull(lineDocSupportable.DocumentSupporter);
				AssertType<WhsInventoryDocumentSupporter>(lineDocSupportable.DocumentSupporter);
				AssertEquals(line.Inventory[0], lineDocSupportable.DocumentSupporter.BusinessObject);
			}
			else // order, work order lines.
			{
				AssertNotNull(lineDocSupportable.DocumentSupporter);
				AssertType<WhsInventoryDocumentSupporter>(lineDocSupportable.DocumentSupporter);
				AssertNull(lineDocSupportable.DocumentSupporter.BusinessObject);
			}
		}

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var line = GetNewBusinessObject();
			var info = ((IDocManagerSupport)line).DocManagerInfo;
			AssertEquals(line, info.BusinessEntity);
			AssertEquals(Constants.DocManagerCodes.WarehouseInventory, info.DocManagerCode);
		}

		#endregion

		#region ICodeDescription

		public void TestICodeDescription_PK()
		{
			var line = GetNewBusinessObject();
			var iPK = ((ICodeDescription)line).PK;
			AssertEquals(line.PK, iPK);
		}

		public void TestICodeDescription_Code()
		{
			var line = GetNewBusinessObject();
			var iCode = ((ICodeDescription)line).Code;
			AssertEquals("!FINDBOX!" + line.PK.ToString(), iCode);
		}

		public void TestICodeDescription_Description()
		{
			var line = GetNewBusinessObject();
			var iDescription = ((ICodeDescription)line).Description;
			AssertEquals("UNUSED", iDescription);
		}

		public void TestCode()
		{
			var line = GetNewBusinessObject();
			AssertEquals("!FINDBOX!" + line.PK.ToString(), line.Code);
		}

		public void TestDescription()
		{
			var line = GetNewBusinessObject();
			AssertEquals("UNUSED", line.Description);
		}

		#endregion

		#region ClearInventoryCache

		public void TestClearInventoryCache()
		{
			var inventory = DocketLine.Inventory;
			DocketLine.ClearInventoryCache();
			AssertNotEquals(inventory, DocketLine.Inventory);
		}

		#endregion

		#region TestWE_TotalOrderValue

		public void TestWE_ExtendedLinePriceTotalOrderValue()
		{
			var docket = Docket;
			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();

			SetLineOrderValue(line1, 2);
			SetLineOrderValue(line2, 2);
			AssertEquals("The Total Order Value is updated to the total on the Lines.", 4m, docket.WD_TotalOrderValue);

			SetLineOrderValue(line1, 1);
			SetLineOrderValue(line2, 4);
			AssertEquals("The Total Order Value is updated to the total on the Lines.", 5m, docket.WD_TotalOrderValue);

			docket.WD_TotalOrderValue = 20;
			SetLineOrderValue(line1, 2);
			SetLineOrderValue(line2, 2);
			AssertEquals("The Total Order Value is not Equal, so do not update Order Total.", 20m, docket.WD_TotalOrderValue);
		}

		void SetLineOrderValue(WhsDocketLine line, ZDecimal price)
		{
			line.WE_ExtendedLinePrice = price;
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted

		#region TestWhyIsThisInventoryCommitted

		public void TestWhyIsThisInventoryCommitted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(TestDataForInventory.DoNotFinaliseReceive);
			var inventory = data.Receive11.Inventory[0];

			data.Receive11.FinaliseDocket();
			var docketLine = inventory.InDocketLine;
			AssertEquals(true, data.Receive11.IsFinalised);
			AssertEquals("This inventory item does not have any committed units.", docketLine.WhyIsThisInventoryCommitted());
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", data.Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var pick1 = Helper.CreatePickNew(order1);
			Factory.Save();     // ZDBOnlyQuery in Inventory.WhyIsThisInventoryCommitted()
			docketLine.ClearPickAllocationsCache();
			AssertEquals(ZString.Format("This inventory item has units committed to:\r\nPick(s): {0}.", pick1.WP_PickNo), docketLine.WhyIsThisInventoryCommitted());

			// assert both pick1 and pick2 are reported
			var pick2 = Helper.CreatePickNew(order2);
			Factory.Save();     // ZDBOnlyQuery in Inventory.WhyIsThisInventoryCommitted()
			docketLine.ClearPickAllocationsCache();
			AssertEquals(ZString.Format("This inventory item has units committed to:\r\nPick(s): {0}, {1}.", pick1.WP_PickNo, pick2.WP_PickNo), docketLine.WhyIsThisInventoryCommitted());

			// assert pick1 is no longer reported as it is finalised
			order1.FinaliseDocket();
			pick1.FinalisePick();

			Factory.Save();     // ZDBOnlyQuery in Inventory.WhyIsThisInventoryCommitted()
			docketLine.ClearPickAllocationsCache();
			AssertEquals(ZString.Format("This inventory item has units committed to:\r\nPick(s): {0}.", pick2.WP_PickNo), docketLine.WhyIsThisInventoryCommitted());

			order2.FinaliseDocket();
			pick2.FinalisePick();
			AssertEquals(true, pick1.IsFinalised);
			AssertEquals(true, pick2.IsFinalised);

			docketLine.ClearPickAllocationsCache();
			Factory.Save();     // ZDBOnlyQuery in Inventory.WhyIsThisInventoryCommitted()
			AssertEquals("This inventory item does not have any committed units.", docketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_Adjustments

		public void TestWhyIsThisInventoryCommitted_Adjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A")); // adjustment
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, data.Whs1.FindLocation("A")); // nothing committed
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -15m, inventory1.Location);
			adjustmentLine.RunPreSaveValidation(); // to commit stock;
			AssertEquals("Precondition: Stock is committed.", 15m, adjustmentLine.CommittedQuantity);

			Factory.Save();

			AssertEquals("Precondition - adjustment should have committed stock.", 15m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - Nothing should have been committed.", 0m, inventory2.CommittedQuantityIncludingUnfinalisedReceipt);

			AssertEquals(string.Format("This inventory item has units committed to:\r\nAdjustment(s): {0}.", adjustment.WD_DocketID), inventory1.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals("This inventory item does not have any committed units.", inventory2.InDocketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_UnFinalisedReceive

		public void TestWhyIsThisInventoryCommitted_UnFinalisedReceive()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(TestDataForInventory.DoNotFinaliseReceive);

			data.Receive11.FinaliseDocket();
			var docketLine = data.Receive11.Inventory[0].InDocketLine;
			AssertEquals(true, data.Receive11.IsFinalised);
			AssertEquals("This inventory item does not have any committed units.", docketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_FinalisedTransferLine

		public void TestWhyIsThisInventoryCommitted_FinalisedTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine2.RunPreSaveValidation();
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Should show committed to Pick Message.",
	@"This inventory item has units committed to:
Pick(s): P00000001.", transferLine1.Inventory[0].InDocketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_WithAllAttributes

		public void TestWhyIsThisInventoryCommitted_WithAllAttributes()
		{
			// this test comprehensively asserts that all attributes are correctly
			// checked (independant of each other) in GetPicksCommittingThisInventoryFilter()

			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.DoNotFinaliseReceive);

			AssertEquals("This inventory item has committed units because Receive Goods Job: " + data.Receive21.WD_DocketID + " is not finalized", data.Line212.InDocketLine.WhyIsThisInventoryCommitted());

			data.Receive11.FinaliseDocket();
			data.Receive12.FinaliseDocket();
			data.Receive21.FinaliseDocket();
			data.Receive22.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs2, "O2", data.Notify);
			var order212 = Helper.CreateWhsOrder(data.Org2, data.Whs1, "O212", data.Notify);
			var order213 = Helper.CreateWhsOrder(data.Org2, data.Whs1, "O213", data.Notify);
			var order214 = Helper.CreateWhsOrder(data.Org2, data.Whs1, "O214", data.Notify);
			var order215 = Helper.CreateWhsOrder(data.Org2, data.Whs1, "O215", data.Notify);
			var order216 = Helper.CreateWhsOrder(data.Org2, data.Whs1, "O216", data.Notify);
			var order217 = Helper.CreateWhsOrder(data.Org2, data.Whs1, "O217", data.Notify);
			var order221 = Helper.CreateWhsOrder(data.Org2, data.Whs2, "O221", data.Notify);
			var order222 = Helper.CreateWhsOrder(data.Org2, data.Whs2, "O222", data.Notify);
			var order223 = Helper.CreateWhsOrder(data.Org2, data.Whs2, "O223", data.Notify);
			order1.WD_DocketSubType = OrderType.Codes.Customs;
			order2.WD_DocketSubType = OrderType.Codes.Customs;
			order212.WD_DocketSubType = OrderType.Codes.Customs;
			order213.WD_DocketSubType = OrderType.Codes.Customs;
			order214.WD_DocketSubType = OrderType.Codes.Customs;
			order215.WD_DocketSubType = OrderType.Codes.Customs;
			order216.WD_DocketSubType = OrderType.Codes.Customs;
			order217.WD_DocketSubType = OrderType.Codes.Customs;
			order221.WD_DocketSubType = OrderType.Codes.Customs;
			order222.WD_DocketSubType = OrderType.Codes.Customs;
			order223.WD_DocketSubType = OrderType.Codes.Customs;

			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine212 = Helper.CreateWhsOrderLine(order212, data.Part2, 10m);
			var orderLine213 = Helper.CreateWhsOrderLine(order213, data.Part2, 10m);
			var orderLine214 = Helper.CreateWhsOrderLine(order214, data.Part2, 10m);
			var orderLine215 = Helper.CreateWhsOrderLine(order215, data.Part2, 10m);
			var orderLine216 = Helper.CreateWhsOrderLine(order216, data.Part2, 10m);
			var orderLine217 = Helper.CreateWhsOrderLine(order217, data.Part2, 10m);
			var orderLine221 = Helper.CreateWhsOrderLine(order221, data.Part1, 10m, data.Line221.WI_BondedEntryKey, "DummyOutward-1", "");
			var orderLine222 = Helper.CreateWhsOrderLine(order222, data.Part2, 10m);
			var orderLine223 = Helper.CreateWhsOrderLine(order223, data.Part1, 10m, data.Line223.WI_BondedEntryKey, "DummyOutward-1", "");

			Helper.SetDocketLineAttributes(orderLine212, data.Line212);
			Helper.SetDocketLineAttributes(orderLine213, data.Line213);
			Helper.SetDocketLineAttributes(orderLine214, data.Line214);
			Helper.SetDocketLineAttributes(orderLine215, data.Line215);
			Helper.SetDocketLineAttributes(orderLine216, data.Line216);
			Helper.SetDocketLineAttributes(orderLine217, data.Line217);
			Helper.SetDocketLineAttributes(orderLine222, data.Line222);

			var pick1 = Helper.CreatePickByAttachingOrders(order1);
			var pick2 = Helper.CreatePickByAttachingOrders(order2);
			var pick212 = Helper.CreatePickByAttachingOrders(order212);
			var pick213 = Helper.CreatePickByAttachingOrders(order213);
			var pick214 = Helper.CreatePickByAttachingOrders(order214);
			var pick215 = Helper.CreatePickByAttachingOrders(order215);
			var pick216 = Helper.CreatePickByAttachingOrders(order216);
			var pick217 = Helper.CreatePickByAttachingOrders(order217);
			var pick221 = Helper.CreatePickByAttachingOrders(order221);
			var pick222 = Helper.CreatePickByAttachingOrders(order222);
			var pick223 = Helper.CreatePickByAttachingOrders(order223);

			Factory.Save();     // ZDBOnlyQuery in Inventory.WhyIsThisInventoryCommitted()

			var expectedMessage = "This inventory item has units committed to:\r\nPick(s): {0}.";
			AssertEquals(ZString.Format(expectedMessage, pick1.WP_PickNo), data.Line111.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick2.WP_PickNo), data.Line121.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick212.WP_PickNo), data.Line212.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick213.WP_PickNo), data.Line213.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick214.WP_PickNo), data.Line214.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick215.WP_PickNo), data.Line215.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick216.WP_PickNo), data.Line216.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick217.WP_PickNo), data.Line217.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick221.WP_PickNo), data.Line221.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick222.WP_PickNo), data.Line222.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(ZString.Format(expectedMessage, pick223.WP_PickNo), data.Line223.InDocketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_Transfers

		public void TestWhyIsThisInventoryCommitted_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1")); // picked
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, data.Whs1.FindLocation("A-1")); // picked + transferred
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, part3, 100m, data.Whs1.FindLocation("A-1")); // transferred
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, part4, 100m, data.Whs1.FindLocation("A-1")); // not committed
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR2", data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part2, 15m, "A-1", "A-2");
			transferLine1.RunPreSaveValidation(); // to commit stock;

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, part3, 15m, "A-1", "A-2");
			transferLine2.RunPreSaveValidation(); // to commit stock;

			Factory.Save();

			AssertEquals("Precondition - order1 should have committed stock.", 10m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - order2 and transfer1 should have committed stock.", 25m, inventory2.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - transfer2 should have committed stock.", 15m, inventory3.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - no stock should have been committed.", 0m, inventory4.CommittedQuantityIncludingUnfinalisedReceipt);

			AssertEquals(string.Format("This inventory item has units committed to:\r\nPick(s): {0}.", pick1.WP_PickNo), inventory1.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(string.Format("This inventory item has units committed to:\r\nPick(s): {0}.\r\nTransfer(s): {1}.", pick2.WP_PickNo, transfer1.WD_DocketID), inventory2.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals(string.Format("This inventory item has units committed to:\r\nTransfer(s): {0}.", transfer2.WD_DocketID), inventory3.InDocketLine.WhyIsThisInventoryCommitted());
			AssertEquals("This inventory item does not have any committed units.", inventory4.InDocketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_WaitingReplenishmentPicks

		public void TestWhyIsThisInventoryCommitted_WaitingReplenishmentPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			var inventory = (WhsInventoryView)receive.Inventory.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsAwaitingReplenishment = true;
			Factory.Save();
			AssertEquals(ZString.Format("This inventory item has units committed to:\r\nPick(s): {0}.", pick.WP_PickNo), inventory.InDocketLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestPickAllocationsAsString

		public void TestPickAllocationsAsString()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2", Notify);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 15m);
			var reservedOrderLine = Helper.CreateWhsOrderLine(order3, data.Part1, 5m);
			var reservedPickLine = reservedOrderLine.ReserveStockIfAbleTo(data.Line111);
			AssertEquals("Precondition: Stock is reserved.", 5m, reservedPickLine.ReservedQuantity);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var availableInventory = pick3.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PickLineQuantity > 0m);
			availableInventory.PickLineQuantity = 0m;

			Factory.Save();
			var docketLine = data.Line111.InDocketLine;
			AssertEquals(2, docketLine.PickAllocations.Count);
			AssertEquals("P00000001", docketLine.PickAllocations.ElementAt(0).WP_PickNo);
			AssertEquals("P00000002", docketLine.PickAllocations.ElementAt(1).WP_PickNo);
			AssertEquals("P00000001, P00000002", docketLine.PickAllocationsAsString);
		}

		#endregion

		#endregion

		#region TestNotAllocateableInventoryCannotBeAttachedToPickLine

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);
			Factory.Save();

			if (docketLine.IsInventoryLine)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, docketLine.WE_StockOnHand);
				var orderLine = order.Lines[0];
				Helper.CreatePickNew(order);
				Assert("Precondition: Stock is Picked.", orderLine.PickLineQuantity > 0);
				Factory.Save();

				docketLine.HeldCodeChangeQuantity = docketLine.WE_StockOnHand;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);

				var exceptionThrown = false;
				try
				{
					Factory.Save();
				}
				catch (ZSaveException e)
				{
					AssertEquals(WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable, e.InnerException.InnerException.Message);
					exceptionThrown = true;
				}

				AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine_TransitionToInTransitAndStagedAndReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);
			Factory.Save();

			if (docketLine.IsInventoryLine)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, docketLine.WE_StockOnHand);
				var orderLine = order.Lines[0];
				Helper.CreatePickNew(order);
				Assert("Precondition: Stock is Picked.", orderLine.PickLineQuantity > 0);
				Factory.Save();

				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
				AssertNoExceptionThrown(Factory.Save);

				transferLine.FinaliseDocketLine();
				AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
				AssertNoExceptionThrown(Factory.Save);

				transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.ReadyToPack;
				AssertEquals("Precondition - should be ReadyToPack.", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine_ReservedPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docketLine = GetNewDocketLineReadyToFinalise(data);
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);
			Factory.Save();

			if (docketLine.IsInventoryLine)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, docketLine.WE_StockOnHand);
				var orderLine = order.Lines[0];
				Helper.CreateReservePickLine(orderLine, docketLine.Inventory[0], docketLine.WE_StockOnHand);
				AssertEquals("Precondition", docketLine.WE_StockOnHand, docketLine.ReservedPickLines.Sum(l => l.WZ_Units));
				Factory.Save();

				docketLine.HeldCodeChangeQuantity = docketLine.WE_StockOnHand;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine_VirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var docketLine = GetNewDocketLineReadyToFinalise(data);
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);
			Factory.Save();

			if (docketLine.IsInventoryLine)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, docketLine.WE_StockOnHand);
				var orderLine = order.Lines[0];
				Helper.CreatePickNew(order);
				Assert("Precondition: Stock is Picked.", orderLine.PickLineQuantity > 0);
				Factory.Save();

				docketLine.HeldCodeChangeQuantity = docketLine.WE_StockOnHand;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine_VirtualWarehouse_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			if (SupportsBOM)
			{
				Helper.CreateProductBOM(data.Part2, data.Part1, 1m, "UNT");
			}
			Factory.Save();

			var docketLine = GetNewDocketLineReadyToFinalise(data);
			docketLine.Docket.FinaliseDocket();
			AssertIsFinalisedPrecondition(docketLine.Docket);
			Factory.Save();

			if (docketLine.IsInventoryLine)
			{
				var order = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "O1", data.Part2, docketLine.WE_StockOnHand);
				var orderLine = order.Lines[0];
				Helper.CreatePickNew(order);
				Assert("Precondition: Stock is Picked.", orderLine.ChildComponentLines.Single().PickLineQuantity > 0);
				Factory.Save();

				docketLine.HeldCodeChangeQuantity = docketLine.WE_StockOnHand;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				docketLine.ChangeInventoryHeldCode(true);
				AssertEquals(InventoryStatus.Codes.Held, docketLine.WE_CurrentInventoryStatus);

				var exceptionThrown = false;
				try
				{
					Factory.Save();
				}
				catch (ZSaveException e)
				{
					AssertEquals(WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable, e.InnerException.InnerException.Message);
					exceptionThrown = true;
				}

				AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Implementation

		protected virtual bool SupportsBOM => true;

		protected virtual bool SupportsCustomsSubType => true;

		protected virtual void SetSumOfUnitsMet(WhsDocketLine docketLine, ZDecimal unitsMet)
		{
			docketLine.WE_TransactionQuantity = unitsMet;
		}

		#region Methods for Asserting Line Properties

		protected void AssertLine(WhsDocketLine line, ZGuid productPK, ZString locationString, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
		{
			AssertEquals("Product incorrect", productPK, line.WE_OP);
			AssertEquals("Location incorrect", locationString, line.LocationString);
			AssertLineAttributes(line, expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3);
		}

		protected void AssertLineAttributes(WhsDocketLine line, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
		{
			AssertEquals("Expiry Date incorrect", expiryDate, line.WE_ExpiryDate);
			AssertEquals("Packing Date incorrect", packingDate, line.WE_PackingDate);
			AssertEquals("Part Attrib1 incorrect", partAttrib1, line.WE_PartAttrib1);
			AssertEquals("Part Attrib2 incorrect", partAttrib2, line.WE_PartAttrib2);
			AssertEquals("Part Attrib3 incorrect", partAttrib3, line.WE_PartAttrib3);
		}

		#endregion

		#region Methods for Setting Attribute values

		protected void SetLineAttributes(WhsDocketLine line)
		{
			SetLineAttributes(line, ZDate.Today, ZDate.Today, "PA1", "PA2", "PA3");
		}

		protected void SetLineAttributes(WhsDocketLine line, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
		{
			line.WE_ExpiryDate = expiryDate;
			line.WE_PackingDate = packingDate;
			line.WE_PartAttrib1 = partAttrib1;
			line.WE_PartAttrib2 = partAttrib2;
			line.WE_PartAttrib3 = partAttrib3;
		}

		protected void ClearLine(WhsDocketLine line)
		{
			line.WE_OP = ZGuid.Empty;
			line.WE_WL = ZGuid.Empty;
			line.LocationString = "";
			line.WE_ExpiryDate = ZDate.Empty;
			line.WE_PackingDate = ZDate.Empty;
			line.WE_PartAttrib1 = "";
			line.WE_PartAttrib2 = "";
			line.WE_PartAttrib3 = "";
		}

		#endregion

		#region Methods for creating Dockets and creating / attaching DocketLines

		protected virtual TDocket GetNewWhsDocket()
		{
			return Factory.New<TDocket>();
		}

		protected new TDocketLine GetNewBusinessObject()
		{
			return (TDocketLine)Factory.New(GetExpectedBusinessObjectType());
		}

		protected virtual TDocket GetNewWhsDocket(WhsDocketLine docketLine)
		{
			return GetNewWhsDocket(null, null, docketLine);
		}

		protected virtual TDocket GetNewWhsDocket(OrgHeader org, WhsWarehouse whs, WhsDocketLine docketLine)
		{
			var docket = GetNewWhsDocket(org, whs);
			docket.Lines.Add(docketLine);
			return docket;
		}

		protected virtual TDocket GetNewWhsDocket(OrgHeader org, WhsWarehouse whs)
		{
			// this creates a new docket that can be saved to the database
			var docket = GetNewWhsDocket();
			if (org != null)
			{
				docket.WD_OH_Client = org.PK;
			}

			if (whs != null)
			{
				docket.WD_WW_Whs = whs.PK;
			}

			return docket;
		}

		protected virtual TDocketLine GetNewBusinessObject(WhsDocket docket, bool needWarehouse = true)
		{
			var docketLine = (TDocketLine)docket.Lines.AddNew();
			if (needWarehouse && (NeedDocketLine_Location || NeedDocketLine_TransferFrom))
			{
				var warehouse = docket.Warehouse;
				if (warehouse == null)
				{
					docket.WD_WW_Whs = Helper.CreateWarehouse("WHS", "A", 2, 1).PK;
				}
				else if (warehouse.DefaultLocation == null)
				{
					Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
				}

				docketLine.WE_WL = NeedDocketLine_Location ? docket.Warehouse.DefaultLocation.PK : ZGuid.Empty;
				docketLine.WE_WL_TransferFrom = NeedDocketLine_TransferFrom ? docket.Warehouse.DefaultLocation.PK : ZGuid.Empty;
			}

			return docketLine;
		}

		#region GetNewDocketLineReadyToFinalise

		protected TDocketLine GetNewDocketLineReadyToFinalise(TestDataSimpleEnvironment data = null, string originalHoldCode = null)
		{
			data = data ?? new TestDataSimpleEnvironment(Factory, 2, 1);
			return (TDocketLine)DocketHelper.GetNewFinalisableDocketWithOneLine(data, 10m, originalHoldCode: originalHoldCode).Lines[0];
		}

		FinalisableDocketHelper<TDocket> DocketHelper
		{
			get { return docketHelper ?? (docketHelper = GetNewDocketHelper(Factory)); }
		}

		protected abstract FinalisableDocketHelper<TDocket> GetNewDocketHelper(BusinessObjectFactory factory);
		FinalisableDocketHelper<TDocket> docketHelper;

		#endregion

		#region NeedDocketLine_TransferFrom

		protected virtual bool NeedDocketLine_TransferFrom
		{
			get { return false; }
		}
		protected virtual bool NeedDocketLine_Location
		{
			get { return false; }
		}

		#endregion

		#endregion

		protected TDocket Docket
		{
			get { return docket ?? (docket = GetNewWhsDocket()); }
			set { docket = value; }
		}

		protected TDocketLine DocketLine
		{
			get { return docketLine ?? (docketLine = GetNewBusinessObject(Docket)); }
			set { docketLine = value; }
		}

		TDocket docket;
		TDocketLine docketLine;

		#endregion
	}
}
