using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemDispatchTransportationUnitProcessHandlingInfoProviderTest : TestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var consignment1 = Factory.New<WhsItemDispatchConsignment>();
			var consignment2 = Factory.New<WhsItemDispatchConsignment>();
			var packageState1Forconsignment1 = consignment1.PackageStates.AddNew();
			var packageState2Forconsignment1 = consignment1.PackageStates.AddNew();
			var packageState1Forconsignment2 = consignment2.PackageStates.AddNew();
			consignment1.WDC_ConsignmentID = "D1";
			consignment2.WDC_ConsignmentID = "D2";

			var processTaskForConsignment1 = Factory.New<ProcessTask>();
			processTaskForConsignment1.P9_ParentID = consignment1.PK;
			processTaskForConsignment1.TriggerConditions.TriggerEventCode = Events.ItemDocumentJobFinalisedCode;
			processTaskForConsignment1.P9_ParentTableCode = WhsItemDispatchConsignmentSchema.Constants.Prefix;
			processTaskForConsignment1.P9_RespondToCascadedEvents = true;

			var processTaskForConsignment2 = Factory.New<ProcessTask>();
			processTaskForConsignment2.P9_ParentID = consignment2.PK;
			processTaskForConsignment2.TriggerConditions.TriggerEventCode = Events.ItemDocumentJobFinalisedCode;
			processTaskForConsignment2.P9_ParentTableCode = WhsItemDispatchConsignmentSchema.Constants.Prefix;
			processTaskForConsignment2.P9_RespondToCascadedEvents = true;

			var unit = Factory.New<WhsItemDispatchTransportationUnit>();
			unit.WDH_ReferenceNumber = "H1";

			packageState1Forconsignment1.WPS_WDH_TransitDispatchHeader = unit.PK;
			packageState2Forconsignment1.WPS_WDH_TransitDispatchHeader = unit.PK;
			packageState1Forconsignment2.WPS_WDH_TransitDispatchHeader = unit.PK;

			var handlingInfo = new WhsItemDispatchTransportationUnitProcessHandlingInfoProviderForTest(unit);

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ItemDocumentJobFinalisedCode;
			}

			var cascadingLinks = handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog);
			AssertEquals(2, cascadingLinks.Count());
			AssertEquals(processTaskForConsignment1, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment1.PK).Triggers.Single());
			AssertEquals(processTaskForConsignment2, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment2.PK).Triggers.Single());
		}

		public void TestPopulateCascadingTargets_PackageStateWithNullConsignment()
		{
			var unit = Factory.New<WhsItemDispatchTransportationUnit>();
			unit.WDH_ReferenceNumber = "H1";
			var packageState1ForTransportationUnit = unit.PackageStates.AddNew();

			var handlingInfo = new WhsItemDispatchTransportationUnitProcessHandlingInfoProviderForTest(unit);

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ItemDocumentJobFinalisedCode;
			}

			AssertNoExceptionThrown("A package state's consignment may be null", () => handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog));
		}

		#endregion

		#region WhsItemDispatchTransportationUnitProcessHandlingInfoProviderForTest

		class WhsItemDispatchTransportationUnitProcessHandlingInfoProviderForTest : WhsItemDispatchTransportationUnitProcessHandlingInfoProvider
		{
			public WhsItemDispatchTransportationUnitProcessHandlingInfoProviderForTest(WhsItemDispatchTransportationUnit unit) : base(unit)
			{
			}

			public IEnumerable<CascadingLink> ExposePopulateCascadingTargetsForTest(IStmALog logBeingAdded)
			{
				return base.PopulateCascadingTargets(logBeingAdded);
			}
		}

		#endregion
	}
}
