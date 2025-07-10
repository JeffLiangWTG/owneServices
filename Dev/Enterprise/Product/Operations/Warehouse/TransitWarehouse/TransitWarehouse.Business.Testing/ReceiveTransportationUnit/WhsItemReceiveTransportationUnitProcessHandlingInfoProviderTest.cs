using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing.ReceiveHeader
{
	class WhsItemReceiveTransportationUnitProcessHandlingInfoProviderTest : TestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS");
			Factory.Save();
			var consignment1 = Helper.CreateReceiveConsignment("R1", warehouse.PK);
			var consignment2 = Helper.CreateReceiveConsignment("R2", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("H2", warehouse.PK, warehouse.DefaultLocation.PK);

			var packageState1Forconsignment1 = Helper.CreatePackageState(consignment1, 1, "PKG", "P1", "PUT", rtu);
			var packageState2Forconsignment1 = Helper.CreatePackageState(consignment1, 1, "PKG", "P2", "PUT", rtu);
			var packageState1Forconsignment2 = Helper.CreatePackageState(consignment2, 1, "PKG", "P3", "PUT", rtu);

			var processTaskForConsignment1 = CreateProcessTask(consignment1, Events.ItemDocumentJobFinalisedCode);
			var processTaskForConsignment2 = CreateProcessTask(consignment2, Events.ItemDocumentJobFinalisedCode);

			var handlingInfo = new WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest(rtu);

			var eventLog = CreateEventLogForRTU(rtu, Events.ItemDocumentJobFinalisedCode);
			Factory.Save();

			var cascadingLinks = handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog);
			AssertEquals(2, cascadingLinks.Count());
			AssertEquals(processTaskForConsignment1, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment1.PK).Triggers.Single());
			AssertEquals(processTaskForConsignment2, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment2.PK).Triggers.Single());
		}

		public void TestPopulateCascadingTargets_ConsignmentsLinkedViaASN()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS");
			Factory.Save();

			var consignment1 = Helper.CreateReceiveConsignment("R1", warehouse.PK);
			var consignment2 = Helper.CreateReceiveConsignment("R2", warehouse.PK);
			var consignmentNotLinked = Helper.CreateReceiveConsignment("R3", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("H1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("H2", warehouse.PK, warehouse.DefaultLocation.PK);

			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);

			var packageState1Forconsignment1 = Helper.CreatePackageState(consignment1, 1, "PKG", "P1", "PUT", rtu2, receiveASN: asn1);
			var packageState2Forconsignment1 = Helper.CreatePackageState(consignment1, 1, "PKG", "P2", "PUT", rtu2, receiveASN: asn1);
			var packageState1Forconsignment2 = Helper.CreatePackageState(consignment2, 1, "PKG", "P3", "PUT", rtu2, receiveASN: asn1);
			var packageState1Forconsignment2AssignedToAnotherASN = Helper.CreatePackageState(consignment2, 1, "PKG", "P4", "PUT", rtu2, receiveASN: asn2);
			var unlinkedPackageState = Helper.CreatePackageState(consignmentNotLinked, 1, "PKG", "P5", "PUT", rtu2, receiveASN: asn2);

			var processTaskForConsignment1 = CreateProcessTask(consignment1, Events.GateInCode);
			var processTaskForConsignment2 = CreateProcessTask(consignment2, Events.GateInCode);

			var handlingInfo = new WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest(rtu1);

			var eventLog = CreateEventLogForRTU(rtu1, Events.GateInCode);
			Factory.Save();

			var cascadingLinks = handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog);
			AssertEquals(2, cascadingLinks.Count());
			AssertEquals(processTaskForConsignment1, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment1.PK).Triggers.Single());
			AssertEquals(processTaskForConsignment2, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment2.PK).Triggers.Single());
		}

		public void TestPopulateCascadingTargets_RTULinksToMultipleASNs()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS");
			Factory.Save();
			var consignment1 = Helper.CreateReceiveConsignment("R1", warehouse.PK);
			var consignment2 = Helper.CreateReceiveConsignment("R2", warehouse.PK);
			var consignmentNotLinked = Helper.CreateReceiveConsignment("R3", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("H1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("H2", warehouse.PK, warehouse.DefaultLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn2.PK);

			var packageState1Forconsignment1 = Helper.CreatePackageState(consignment1, 1, "PKG", "P1", "PUT", rtu2, receiveASN: asn1);
			var packageState2Forconsignment1 = Helper.CreatePackageState(consignment1, 1, "PKG", "P1", "PUT", rtu2, receiveASN: asn1);
			var packageState1Forconsignment2 = Helper.CreatePackageState(consignment2, 1, "PKG", "P1", "PUT", rtu2, receiveASN: asn2);

			var processTaskForConsignment1 = CreateProcessTask(consignment1, Events.GateInCode);
			var processTaskForConsignment2 = CreateProcessTask(consignment2, Events.GateInCode);

			var handlingInfo = new WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest(rtu1);

			var eventLog = CreateEventLogForRTU(rtu1, Events.GateInCode);
			Factory.Save();

			var cascadingLinks = handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog);
			AssertEquals(2, cascadingLinks.Count());
			AssertEquals(processTaskForConsignment1, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment1.PK).Triggers.Single());
			AssertEquals(processTaskForConsignment2, cascadingLinks.Single(c => c.Parent.LogsParentPK == consignment2.PK).Triggers.Single());
		}

		public void TestPopulateCascadingTargets_PackageStateWithNullConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS");
			Factory.Save();
			var rtu = Helper.CreateReceiveTransportationUnit("H1", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageWithoutRCN = Helper.CreatePackageState(rtu, 1, "PKG", "P1", "ARV");

			var handlingInfo = new WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest(rtu);

			var eventLog = CreateEventLogForRTU(rtu, Events.ItemDocumentJobFinalisedCode);
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there is a PackageState without RCN.", () => handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog));
		}

		public void TestPopulateCascadingTargets_PackageStateInASNWithNullConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS");
			Factory.Save();
			var rtu1 = Helper.CreateReceiveTransportationUnit("H1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("H2", warehouse.PK, warehouse.DefaultLocation.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			var packageWithoutRCN = Helper.CreatePackageState(rtu2, 1, "PKG", "P1", "ARV", receiveASN: asn);

			var handlingInfo = new WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest(rtu1);

			var eventLog = CreateEventLogForRTU(rtu1, Events.ItemDocumentJobFinalisedCode);
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there is a PackageState without RCN.", () => handlingInfo.ExposePopulateCascadingTargetsForTest(eventLog));
		}

		StmALog CreateEventLogForRTU(WhsItemReceiveTransportationUnit rtu, string eventCode)
		{
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = eventCode;
				eventLog.SL_Parent = rtu.PK;
				eventLog.SL_Table = rtu.TableName;
			}

			return eventLog;
		}

		ProcessTask CreateProcessTask(WhsItemReceiveConsignment consignment1, string eventCode)
		{
			var processTaskForConsignment1 = Factory.New<ProcessTask>();
			processTaskForConsignment1.P9_ParentID = consignment1.PK;
			processTaskForConsignment1.TriggerConditions.TriggerEventCode = eventCode;
			processTaskForConsignment1.P9_ParentTableCode = WhsItemReceiveConsignmentSchema.Constants.Prefix;
			processTaskForConsignment1.P9_RespondToCascadedEvents = true;
			return processTaskForConsignment1;
		}

		#endregion

		#region WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest

		class WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest : WhsItemReceiveTransportationUnitProcessHandlingInfoProvider
		{
			public WhsItemReceiveTransportationUnitProcessHandlingInfoProviderForTest(WhsItemReceiveTransportationUnit unit) : base(unit)
			{
			}

			public IEnumerable<CascadingLink> ExposePopulateCascadingTargetsForTest(IStmALog logBeingAdded)
			{
				return base.PopulateCascadingTargets(logBeingAdded);
			}
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
