using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class IEventDataContextManagerWithTriggeringLogExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestGetHoldCodeChangedContexts

		public void TestGetHoldCodeChangedContexts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "YYY";
			AssertEquals(0, ((IEventDataContextManagerWithTriggeringLog)null).GetHoldCodeChangedEventContexts(receive).Count());

			var dummyContextManager = new DummyEventContextManager();
			AssertEquals("No Triggering Event, so there should be no Hold Code Changes.", 0, dummyContextManager.GetHoldCodeChangedEventContexts(receive).Count());

			var log = inventory.InDocketLine.Logs.AddNew(Events.ServiceCommenced, "Propagated: All",
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "XXX"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, "YYY"));
			dummyContextManager.TriggeringLogForUseInPopulatingEventContext = log;
			AssertEquals("Incorrect Event Type, so there should be no Hold Code Changes.", 0, dummyContextManager.GetHoldCodeChangedEventContexts(receive).Count());

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.ChangeOfIdentifierCode;
				((AutoStmALog)log).SL_IsCancelled = true;
				AssertEquals("Event is cancelled, so there should be no Hold Code Changes.", 0, dummyContextManager.GetHoldCodeChangedEventContexts(receive).Count());
				((AutoStmALog)log).SL_IsCancelled = false; // clean-up

				var oldReference = log.SL_Reference;
				log.SL_Reference = "RandomCrap";
				AssertEquals("Not a Propagated Event, so there should be no Hold Code Changes.", 0, dummyContextManager.GetHoldCodeChangedEventContexts(receive).Count());
				log.SL_Reference = oldReference; // clean-up

				AssertEquals("No Original Docket so there should be no Hold Code Changes.", 0, dummyContextManager.GetHoldCodeChangedEventContexts(null).Count());
				AssertContainsExactElementsInAnyOrder("Should correctly populate Hold Code Changes.", new[] { "InventoryWithChangedHoldCode - 10x P1 changed from 'XXX' to 'YYY'" },
					dummyContextManager.GetHoldCodeChangedEventContexts(receive).Select(c => c.Key.ToString() + " - " + c.Value));
			}
		}

		#endregion

		#region TestGetHoldCodeChangedContexts_EmptyOld

		public void TestGetHoldCodeChangedContexts_EmptyOld()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "HEL";

			inventory.InDocketLine.Logs.AddNew(Events.ChangeOfIdentifier, now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, "HEL"));

			var log = receive.Logs.AddNew(Events.ChangeOfIdentifier, "Propagated: All", now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, "HEL"));

			var dummyContextManager = new DummyEventContextManager();
			dummyContextManager.TriggeringLogForUseInPopulatingEventContext = log;
			AssertContainsExactElementsInAnyOrder("Should correctly populate Hold Code Changes.",
				new[] { "InventoryWithChangedHoldCode - 10x P1 changed from '' to 'HEL'", },
				dummyContextManager.GetHoldCodeChangedEventContexts(receive).Select(c => c.Key.ToString() + " - " + c.Value));
		}

		#endregion

		#region TestGetHoldCodeChangedContexts_WillNotBlowUpForNonStmALogs

		public void TestGetHoldCodeChangedContexts_WillNotBlowUpForNonStmALogs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Factory.Save();
			var log = Factory.New<DummyStmALog>();
			log.SL_Table = "WhsDocket";
			log.SL_Parent = receive.PK;
			log.SL_EventTime = ZDateTime.Now;
			log.SL_SE_NKEvent = Events.WarehouseJobEnteredCode;
			log.SL_Reference = "";

			var dummyContextManager = new DummyEventContextManager();
			dummyContextManager.TriggeringLogForUseInPopulatingEventContext = log;
			AssertNoExceptionThrown(() => dummyContextManager.GetHoldCodeChangedEventContexts(receive));
		}

		#endregion

		#region TestGetHoldCodeChangedContexts_Attributes

		public void TestGetHoldCodeChangedContexts_Attributes()
		{
			var now = ZDateTimeOffset.Now;
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			inventory1.WI_SerialNumber = "PSERN";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12m, new ZDate(year, 1, 3), new ZDate(year, 1, 4), "SA1", "SA2", "SA3", "");
			inventory2.WI_SerialNumber = "SSERN";
			Factory.Save();

			receive.Lines[0].Logs.AddNew(Events.ChangeOfIdentifier, now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "XXX"));
			receive.Lines[1].Logs.AddNew(Events.ChangeOfIdentifier, now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "XXX"));

			var log = receive.Logs.AddNew(Events.ChangeOfIdentifier, "Propagated: All", now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, "XXX"));

			var dummyContextManager = new DummyEventContextManager();
			dummyContextManager.TriggeringLogForUseInPopulatingEventContext = log;
			AssertContainsExactElementsInAnyOrder("Should correctly populate Hold Code Changes.",
				new[]
				{
					string.Format("InventoryWithChangedHoldCode - 10x P1 (PA1=PA1, PA2=PA2, PA3=PA3, SN=PSERN, EXP={0}, PCK={1}) changed from 'XXX' to ''", new ZDateTime(year, 1, 1).ToShortDateString(), new ZDateTime(year, 1, 2).ToShortDateString()),
					string.Format("InventoryWithChangedHoldCode - 12x P1 (PA1=SA1, PA2=SA2, PA3=SA3, SN=SSERN, EXP={0}, PCK={1}) changed from 'XXX' to ''", new ZDateTime(year, 1, 3).ToShortDateString(), new ZDateTime(year, 1, 4).ToShortDateString())
				},
				dummyContextManager.GetHoldCodeChangedEventContexts(receive).Select(c => c.Key.ToString() + " - " + c.Value));
		}

		#endregion

		#region TestGetHoldCodeChangedContexts_NonOriginalInventory

		public void TestGetHoldCodeChangedContexts_NonOriginalInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transfer.RunPreSaveValidation();

			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			Factory.Save();

			transferLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertEquals("Should successfully change Hold Code.", true, transferLine1.ChangeInventoryHeldCode(true));
			Factory.Save();

			var dummyContextManager = new DummyEventContextManager();
			dummyContextManager.TriggeringLogForUseInPopulatingEventContext = receive.Logs.Find(l => l.SL_SE_NKEvent == Events.ChangeOfIdentifierCode).Single();
			AssertContainsExactElementsInAnyOrder("Should correctly populate Hold Code Changes.",
				new[] { "InventoryWithChangedHoldCode - 6x P1 changed from '' to 'HEL'", },
				dummyContextManager.GetHoldCodeChangedEventContexts(receive).Select(c => c.Key.ToString() + " - " + c.Value));
		}

		#endregion

		#region TestGetHoldCodeChangedContexts_PartiallyChangedInventory

		public void TestGetHoldCodeChangedContexts_PartiallyChangedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var line = receive.Lines.Single();
			line.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			line.HeldCodeChangeQuantity = 4m;
			AssertEquals("Should successfully change Hold Code.", true, line.ChangeInventoryHeldCode(true));
			Factory.Save();

			var dummyContextManager = new DummyEventContextManager();
			dummyContextManager.TriggeringLogForUseInPopulatingEventContext = receive.Logs.Find(l => l.SL_SE_NKEvent == Events.ChangeOfIdentifierCode).Single();
			AssertContainsExactElementsInAnyOrder("Should correctly populate Hold Code Changes.",
				new[] { "InventoryWithChangedHoldCode - 4x P1 changed from '' to 'HEL'", },
				dummyContextManager.GetHoldCodeChangedEventContexts(receive).Select(c => c.Key.ToString() + " - " + c.Value));
		}

		#endregion

		#region Implementation

		class DummyStmALog : BaseStmALog
		{
			public DummyStmALog(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool IsSavedByFactory
			{
				get { return false; }
			}
		}

		class DummyEventContextManager : EventDataContextManager<DummyBusinessObject>, IEventDataContextManagerWithTriggeringLog
		{
			public BaseStmALog TriggeringLogForUseInPopulatingEventContext
			{
				get;
				set;
			}

			protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
			{
				throw new System.NotImplementedException();
			}

			protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
			{
				throw new System.NotImplementedException();
			}

			public override ZString DataContextKey
			{
				get { throw new System.NotImplementedException(); }
			}

			public override DataContextType DataContextType
			{
				get { throw new System.NotImplementedException(); }
			}

			public override string DefaultOutputDirectory
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
			{
				return null;
			}
		}

		#endregion
	}
}
