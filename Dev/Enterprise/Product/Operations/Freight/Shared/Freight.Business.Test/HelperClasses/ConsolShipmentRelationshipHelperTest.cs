using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolShipmentRelationshipHelperTest : BaseFreightTest
	{
		#region Events

		public void TestAddEvents()
		{
			var attach = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.Code);
			var detach = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Detached.Code);

			var consol = Factory.New<CommonConsol>();
			consol.JK_MasterBillNum = Guid.NewGuid().ToString("N").Substring(0, 10);

			var shipment = Factory.New<CommonShipment>();
			Factory.Save();

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);

			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment);

			AssertEquals("Even new shipments get event", 1, shipment.Logs.Find(attach).Length);
			AssertEquals(string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_Reference);
			AssertEquals("Should not have deferred firing workflow on ATC event", false, shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_FireWorkflow);
			AssertEquals(1, shipment.Logs.Find(detach).Length);
			AssertEquals(string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), shipment.Logs.MostRecentLogByEventTime(Events.Detached).SL_Reference);
			AssertEquals("Should not have deferred firing workflow on DTC event", false, shipment.Logs.MostRecentLogByEventTime(Events.Detached).SL_FireWorkflow);
		}

		public void TestAddEvent_ConsolNotInDatabase()
		{
			var attachQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.Code);

			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001235";

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);

			AssertEquals("Shipment contains ATC event", 1, shipment.Logs.Find(attachQuery).Length);
			AssertEquals("Event Ref uses PK", string.Format("{0}|TYP=Consol", consol.PK), shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_Reference);
			AssertEquals("Should have deferred firing workflow on ATC event", true, shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_FireWorkflow);
		}

		public void TestDetachEvent_WhenRemovingShipmentFromConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment1 = Factory.New<CommonShipment>();

			Factory.Save();

			var shipment2 = Factory.New<CommonShipment>();
			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment1);
			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment2);

			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment1);
			Assert("Detached event should be added.", shipment1.Logs.Find(s => s.SL_SE_NKEvent == Events.DetachedCode).Any());

			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment2);
			Assert("Detached event should not be added.", !shipment2.Logs.Find(s => s.SL_SE_NKEvent == Events.DetachedCode).Any());
		}

		public void TestClearAttachEventsWithConsolPKWhenDetach()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.Shipments.Add(shipment);

			var log = shipment.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertEquals(string.Format("{0}|TYP=Consol", consol.PK), log.SL_Reference);

			consol.Shipments.Remove(shipment);
			AssertEquals("Detaching from consol should remove unsaved attached log.", 0, shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.Code)).Length);
		}

		public void TestDetachEventWouldNotBeDuplicatedForTheSameUnsavedPair()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONSOL";

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT";
			Factory.Save();

			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment);
			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment);

			var detachEventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Detached.Code);
			AssertEquals("Detach event added only once", 1, shipment.Logs.Find(detachEventFilter).Length);

			Factory.Save();

			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment);
			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment);
			AssertEquals("Another detach event was added - checking for duplicates only in unsaved logs", 2, shipment.Logs.Find(detachEventFilter).Length);
		}

		public void TestAddEventsChinese()
		{
			using (var language = Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				var attach = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.Code);
				var detach = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Detached.Code);

				var consol = Factory.New<CommonConsol>();
				consol.JK_MasterBillNum = Guid.NewGuid().ToString("N").Substring(0, 10);

				var shipment = Factory.New<CommonShipment>();
				Factory.Save();

				ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);

				ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment);

				AssertEquals(1, shipment.Logs.Find(attach).Length);
				AssertEquals(string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_Reference);
				AssertNoErrorContaining(shipment.Logs.Find(attach), (log) => log.SL_ReferenceInfo, "only accepts English language characters");
				AssertEquals(1, shipment.Logs.Find(detach).Length);
				AssertEquals(string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), shipment.Logs.MostRecentLogByEventTime(Events.Detached).SL_Reference);
				AssertNoErrorContaining(shipment.Logs.Find(detach), (log) => log.SL_ReferenceInfo, "only accepts English language characters");
			}
		}

		#endregion

		#region Related Shipments

		public void TestRelatedShipments_FromConsol()
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);

			var masterShipment = FreightTestHelper.GetShipment<CommonShipment>("MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var sub1 = FreightTestHelper.GetShipment("SUB_1", masterShipment, Constants.ShipmentTypes.StandardHouse, Factory);
			var sub2 = FreightTestHelper.GetShipment("SUB_2", masterShipment, Constants.ShipmentTypes.StandardHouse, Factory);
			Factory.Save();

			FreightTestHelper.AssertShipmentCollection("Precondition", consol.Shipments);
			FreightTestHelper.AssertConsolCollection("Precondition", masterShipment.Consols);
			FreightTestHelper.AssertConsolCollection("Precondition", sub1.Consols);
			FreightTestHelper.AssertConsolCollection("Precondition", sub2.Consols);

			consol.Shipments.Add(masterShipment);
			FreightTestHelper.AssertShipmentCollection("Master + 2 subs added", consol.Shipments, masterShipment, sub1, sub2);
			FreightTestHelper.AssertConsolCollection("1 consol on master", masterShipment.Consols, consol);
			FreightTestHelper.AssertConsolCollection("1 consol on sub1", sub1.Consols, consol);
			FreightTestHelper.AssertConsolCollection("1 consol on sub2", sub2.Consols, consol);

			consol.Shipments.Remove(masterShipment);
			FreightTestHelper.AssertShipmentCollection("Master removed", consol.Shipments, sub1, sub2);
			FreightTestHelper.AssertConsolCollection("No consol on master", masterShipment.Consols);
			FreightTestHelper.AssertConsolCollection("1 consol on sub1", sub1.Consols, consol);
			FreightTestHelper.AssertConsolCollection("1 consol on sub2", sub2.Consols, consol);
		}

		public void TestRemoveFromGridShipments()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment sub1 = masterShipment.CoLoadShipments.AddNew();
			CommonShipment sub2 = masterShipment.CoLoadShipments.AddNew();
			consol.Shipments.Add(masterShipment);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol consol_NewFactory = newFactory.Load<CommonConsol>(consol.PK);
			consol_NewFactory.GridShipments.ShouldShowChildShipments = true;
			AssertEquals(3, consol_NewFactory.GridShipments.Count);
			AssertEquals(3, consol_NewFactory.Shipments.Count);
			consol_NewFactory.GridShipments.Remove(sub2.PK);
			AssertEquals(2, consol_NewFactory.GridShipments.Count);
			AssertEquals(2, consol_NewFactory.Shipments.Count);
		}

		public void TestRelatedShipments_FromShipment()
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);

			var masterShipment = FreightTestHelper.GetShipment<CommonShipment>("MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var sub1 = FreightTestHelper.GetShipment("SUB_1", masterShipment, Constants.ShipmentTypes.StandardHouse, Factory);
			var sub2 = FreightTestHelper.GetShipment("SUB_2", masterShipment, Constants.ShipmentTypes.StandardHouse, Factory);
			Factory.Save();

			FreightTestHelper.AssertShipmentCollection("Precondition", consol.Shipments);
			FreightTestHelper.AssertConsolCollection("Precondition", masterShipment.Consols);
			FreightTestHelper.AssertConsolCollection("Precondition", sub1.Consols);
			FreightTestHelper.AssertConsolCollection("Precondition", sub2.Consols);

			masterShipment.Consols.Add(consol);
			FreightTestHelper.AssertShipmentCollection("Master + 2 subs added", consol.Shipments, masterShipment, sub1, sub2);
			FreightTestHelper.AssertConsolCollection("1 consol on master", masterShipment.Consols, consol);
			FreightTestHelper.AssertConsolCollection("1 consol on sub1", sub1.Consols, consol);
			FreightTestHelper.AssertConsolCollection("1 consol on sub2", sub2.Consols, consol);

			Factory.Save();

			masterShipment.Consols.Remove(consol);
			consol.Shipments.Load();
			FreightTestHelper.AssertShipmentCollection("Master removed", consol.Shipments, sub1, sub2);
			FreightTestHelper.AssertConsolCollection("No consol on master", masterShipment.Consols);
			FreightTestHelper.AssertConsolCollection("1 consol on sub1", sub1.Consols, consol);
			FreightTestHelper.AssertConsolCollection("1 consol on sub2", sub2.Consols, consol);
		}

		[ExpectNoExceptions]
		public void TestRelatedShipments_DataRefresh_StackOverflow()
		{
			BusinessObjectFactory consolFactory = new BusinessObjectFactory();
			var consol = (CommonConsol)consolFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consolFactory.Save();

			CommonShipment masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonShipment sub1 = masterShipment.CoLoadShipments.AddNew();
			CommonShipment sub2 = masterShipment.CoLoadShipments.AddNew();
			Factory.Save();

			var shipment = consolFactory.Load<CommonShipment>(masterShipment.PK);
			consol.Shipments.Add(shipment);
			consolFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment masterShipmentReloaded = newFactory.Load<CommonShipment>(masterShipment.PK);
			masterShipmentReloaded.Consols.Remove(masterShipmentReloaded.Consols[0]);
			newFactory.Save();
		}

		#endregion

		#region Pack / Unpack Containers

		public void TestPackUnpackContainers()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = false;
			CommonContainer container = consol.Containers.AddNew();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();

			consol.Shipments.Add(shipment);
			AssertEquals("AutomaticallyUpdatePackLineContainers was set to false so container should not be packed", 0, container.PackLines.Count);
		}

		public void TestPackContainersAuto_IfGrossWithOverride()
		{
			var consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = true;
			var container = consol.Containers.AddNew();
			container.JC_IsGrossWeightOverridden = true;

			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;

			consol.Shipments.Add(shipment);
			AssertEquals("AutomaticallyUpdatePackLineContainers was set to true so container should be packed", true, container.PackLines.Contains(packLine));
		}

		public void TestPackUnpackContainersAuto()
		{
			var consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = true;
			var container = consol.Containers.AddNew();

			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;

			consol.Shipments.Add(shipment);
			AssertEquals("AutomaticallyUpdatePackLineContainers was set to true so container should be packed", true, container.PackLines.Contains(packLine));

			shipment.Consols.Remove(consol);
			AssertEquals("CommonShipment should be unpacked from the consol", 0, container.PackLines.Count);
		}

		public void TestUnpackShipment()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();

			packLine1.SetContainer(consol, container);
			AssertEquals("Packline packed into container", true, container.PackLines.Contains(packLine1));

			var shipment2 = consol.Shipments.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			packLine2.SetContainer(consol, container);
			AssertEquals("Packline packed into container", true, container.PackLines.Contains(packLine2));

			ConsolShipmentRelationshipHelper.UnpackShipment(consol, shipment1);
			AssertEquals("Shipment1 should be unpacked from the consol's containers", false, container.PackLines.Contains(packLine1));
			AssertEquals("Shipment2 should be still packed into consol's containers", true, container.PackLines.Contains(packLine2));
		}

		public void TestShipmentRemovedFromConsol_UnpackShipmentFromMasterConsol()
		{
			var masterConsol = Factory.New<CommonConsol>();
			masterConsol.JK_AgentType = Constants.AgentType.AWBMaster;

			var container = masterConsol.Containers.AddNew();

			var consol = Factory.New<CommonConsol>();
			consol.JK_JK_MasterConsol = masterConsol.PK;

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();

			packLine1.SetContainer(masterConsol, container);
			AssertEquals("Packline packed into container", true, container.PackLines.Contains(packLine1));

			ConsolShipmentRelationshipHelper.ShipmentRemovedFromConsol(consol, shipment1);
			AssertEquals("Shipment1 should be unpacked from the master consol's containers", false, container.PackLines.Contains(packLine1));
		}

		public void TestUnpackMasterAndSubShipments()
		{
			var consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = true;
			var container = consol.Containers.AddNew();

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var subShipment = masterShipment.CoLoadShipments.AddNew();
			var subPackLine1 = subShipment.OuterPackLines.AddNew();
			var subPackLine2 = subShipment.OuterPackLines.AddNew();
			var subPackLine3 = subShipment.OuterPackLines.AddNew();

			subPackLine1.JL_PackageCount = 1;
			subPackLine2.JL_PackageCount = 2;
			subPackLine3.JL_PackageCount = 3;

			consol.Shipments.Add(masterShipment);
			AssertEquals("Packlines should be attached to the consol", 3, container.PackLines.Count);

			masterShipment.Consols.Remove(consol);
			AssertEquals("Removing mastershipment should not unpack shipments from consol", 3, container.PackLines.Count);

			subShipment.OuterPackLines.RemoveAndDelete(subPackLine3);
			AssertEquals("A packline should be unpacked from the consol", 2, container.PackLines.Count);

			subShipment.Consols.Remove(consol);
			AssertEquals("All of SubShipments packlines should be unpacked from the consol", 0, container.PackLines.Count);
		}

		public void TestUnpackMasterAndSubShipments_BlindCoLoad()
		{
			var consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = true;
			var container = consol.Containers.AddNew();

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			var subShipment = masterShipment.CoLoadShipments.AddNew();
			var subPackLine1 = subShipment.OuterPackLines.AddNew();
			var subPackLine2 = subShipment.OuterPackLines.AddNew();
			var subPackLine3 = subShipment.OuterPackLines.AddNew();

			subPackLine1.JL_PackageCount = 1;
			subPackLine2.JL_PackageCount = 2;
			subPackLine3.JL_PackageCount = 3;

			consol.Shipments.Add(masterShipment);
			AssertEquals("Packlines should be attached to the consol", 3, container.PackLines.Count);

			masterShipment.Consols.Remove(consol);
			AssertEquals("Removing mastershipment should not unpack shipments from consol", 3, container.PackLines.Count);

			subShipment.OuterPackLines.RemoveAndDelete(subPackLine3);
			AssertEquals("A packline should be unpacked from the consol", 2, container.PackLines.Count);

			subShipment.Consols.Remove(consol);
			AssertEquals("All of SubShipments packlines should be unpacked from the consol", 0, container.PackLines.Count);
		}

		public void TestConsolContainerIsNotAllocatedToShipmentPacklineAfterConsolIsDetached()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = true;
			CommonContainer container = consol.Containers.AddNew();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.Consols.Add(consol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment = newFactory.Load<CommonShipment>(shipment.PK);
			var loadedConsol = newFactory.Load<CommonConsol>(consol.PK);
			var loadedContainer = newFactory.Load<CommonContainer>(container.PK);
			loadedShipment.Consols.Remove(loadedConsol);
			var packLine = loadedShipment.OuterPackLines.AddNew();

			newFactory.Save();

			AssertEquals("Consol should be removed from shipment", 0, loadedShipment.Consols.Count);
			AssertEquals("Container should not contain packlines from the detached shipment", 0, loadedContainer.PackLines.Count);
			AssertNull("There should not be Current Consol on pack lines", loadedShipment.OuterPackLines.CurrentConsol);

			AssertEquals("Consol should be removed from shipment", 0, shipment.Consols.Count);
			AssertEquals("Container should not contain packlines from the detached shipment", 0, container.PackLines.Count);
			AssertNull("There should not be Current Consol on pack lines", shipment.OuterPackLines.CurrentConsol);
		}

		#endregion

		#region Departure / Arrival Dates

		public void TestUpdateDepartureArrivalDates()
		{
			ZDateTime today = ZDateTime.Today;
			CommonShipment shipment = Factory.New<CommonShipment>();

			CommonConsol consol0 = CreateConsol(today);
			CommonConsol consol1 = CreateConsol(today.AddDays(20));
			CommonConsol consol2 = CreateConsol(today.AddDays(40));

			AssertEquals("precondition:", ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals("precondition:", ZDateTime.Empty, shipment.JS_E_ARV);

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol1, shipment);
			AssertDates(today.AddDays(20), today.AddDays(35), shipment);

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol2, shipment);
			AssertDates(today.AddDays(20), today.AddDays(55), shipment);

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol0, shipment);
			AssertDates(today, today.AddDays(55), shipment);
		}

		public void TestUpdateDepartureArrivalDatesSuppressed()
		{
			ZDateTime today = ZDateTime.Today;
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = CreateConsol(today);

			AssertDates(ZDateTime.Empty, ZDateTime.Empty, shipment);

			shipment.IsSuppressedETAETDOnAttachToConsol = true;
			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			AssertDates(ZDateTime.Empty, ZDateTime.Empty, shipment);
			Assert(!shipment.IsSuppressedETAETDOnAttachToConsol);
		}

		void AssertDates(ZDateTime eTD, ZDateTime eTA, CommonShipment shipment)
		{
			AssertEquals("ETD", eTD, shipment.JS_E_DEP);
			AssertEquals("ETA", eTA, shipment.JS_E_ARV);
		}

		CommonConsol CreateConsol(ZDateTime fromDate)
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport1 = consol.Transports[0];
			transport1.JW_ETD = fromDate;
			transport1.JW_ETA = fromDate.AddDays(5);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_ETD = fromDate.AddDays(10);
			transport2.JW_ETA = fromDate.AddDays(15);

			return consol;
		}

		#endregion

		#region Available / Storage Dates

		public void TestUpdateAvailableStorageDates()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_FCLAvailable = new ZDateTime(2002, 1, 1);
			container1.JC_ArrivalCTOStorageStartDate = new ZDateTime(2002, 2, 2);
			container1.JC_LCLAvailable = new ZDateTime(2002, 3, 3);
			container1.JC_LCLStorageCommences = new ZDateTime(2002, 4, 4);
			var container2 = consol.Containers.AddNew();
			container2.JC_FCLAvailable = new ZDateTime(2001, 1, 1);
			container2.JC_ArrivalCTOStorageStartDate = new ZDateTime(2001, 2, 2);
			container2.JC_LCLAvailable = new ZDateTime(2001, 3, 3);
			container2.JC_LCLStorageCommences = new ZDateTime(2001, 4, 4);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = container1.PK;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			packLine2.JL_JC = container2.PK;

			consol.Shipments.Add(shipment);
			AssertEquals("JP_FCLAvailable updated", new ZDateTime(2001, 1, 1), shipment.DocsAndCartage.JP_FCLAvailable);
			AssertEquals("JP_FCLStorageCommences updated", new ZDateTime(2001, 2, 2), shipment.DocsAndCartage.JP_FCLStorageCommences);
			AssertEquals("JP_LCLAvailable updated", new ZDateTime(2001, 3, 3), shipment.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("JP_LCLStorageCommences updated", new ZDateTime(2001, 4, 4), shipment.DocsAndCartage.JP_LCLStorageCommences);

			shipment.Consols.Remove(consol);
			AssertEquals("JP_FCLAvailable reverted", ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLAvailable);
			AssertEquals("JP_FCLStorageCommences reverted", ZDateTime.Empty, shipment.DocsAndCartage.JP_FCLStorageCommences);
			AssertEquals("JP_LCLAvailable reverted", ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("JP_LCLStorageCommences reverted", ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLStorageCommences);
		}

		#endregion

		#region Flags

		public void TestUpdateFlags()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_IsCFS = false;
			consol.JK_IsForwarding = true;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsCFSRegistered = false;

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			AssertEquals("CommonShipment should not be CFS", false, shipment.JS_IsCFSRegistered);

			consol.JK_IsCFS = true;
			AssertEquals("precondition: CommonShipment should not be CFS", false, shipment.JS_IsCFSRegistered);

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			AssertEquals("Adding a forwarding CommonShipment to a cfs consol should make the CommonShipment cfs", true, shipment.JS_IsCFSRegistered);
		}

		#endregion

		#region Buyers Consol

		public void TestUpdateMasterShipmentForBCN()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			CommonShipment otherShipment = consol.Shipments.AddNew();

			CommonShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			CommonShipment shipment = Factory.New<CommonShipment>();

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			AssertEquals("CommonShipment should not have been given a coload master when added to a FCL consol", ZGuid.Empty, shipment.JS_JS_ColoadMasterShipment);

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			AssertEquals("CommonShipment should not be given a coload master when added to a BCN consol", ZGuid.Empty, shipment.JS_JS_ColoadMasterShipment);
		}

		public void TestUpdateMasterShipmentForBCN_Master()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			CommonShipment otherShipment = consol.Shipments.AddNew();

			CommonShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			CommonShipment shipment = Factory.New<CommonShipment>();

			ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment);
			AssertEquals("CommonShipment should not have been given master when added to a FCL consol", ZGuid.Empty, shipment.JS_JS_ColoadMasterShipment);
		}

		#endregion

		#region New Shipment In New Consol

		public void TestNewShipmentInNewConsol()
		{
			AssertNewShipmentInNewConsol(SharedConstants.Languages.English);
			AssertNewShipmentInNewConsol(SharedConstants.Languages.ChineseSimplified);
		}

		void AssertNewShipmentInNewConsol(string languageCode)
		{
			using (var language = Res.TemporarilySwitchLanguage(languageCode))
			{
				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_ConsolMode = Constants.ContainerModes.FCL;

				CommonShipment shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = System.Guid.NewGuid().ToString("N").Substring(0, 10);

				AssertNotNull(shipment.Logs.MostRecentLogByEventTime(Events.Attached));
				AssertNotEquals(ZDateTime.Empty, shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_EventTime);
				AssertEquals(string.Format("{0}|TYP=Consol", consol.PK.ToString()), shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_Reference);

				Factory.Save();

				var logEntry = shipment.Logs.MostRecentLogByEventTime(Events.Attached);

				AssertNoErrorContaining(logEntry, (log) => log.SL_ReferenceInfo, "only accepts English language characters");

				AssertEquals(string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), logEntry.SL_Reference);
			}
		}

		#endregion

		#region Cross-Factory Work

		public void TestCrossFactoryWork()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.Factory.Save();

			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.Attached));

			CommonConsol consol = new BusinessObjectFactory().New<CommonConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Shipments.Add(consol.Factory.Load<CommonShipment>(shipment.PK));

			AssertEquals(1, consol.Shipments.Count);
			AssertNotNull(consol.Shipments[0].Logs.MostRecentLogByEventTime(Events.Attached));

			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.Attached));
			AssertEquals("Only ADD event should be logged", 1, shipment.Logs.GetAllLogs().Count);

			consol.Factory.Save();

			AssertNotNull("Data Refresh should consequently pass event to other factories.", shipment.Logs.MostRecentLogByEventTime(Events.Attached));
			AssertEquals("ADD and ATC events should be logged", 2, shipment.Logs.GetAllLogs().Count);
			AssertEquals("Data Refresh does not changes objects by itself.", false, shipment.HasChanges);
			AssertEquals(false, shipment.Logs.MostRecentLogByEventTime(Events.Attached).HasChanges);
			AssertNotEquals(ZDateTime.Empty, shipment.Logs.MostRecentLogByEventTime(Events.Attached).SL_EventTime);
		}

		#endregion

		#region Attach Consol to Shipment and check ETA

		public void TestAttachConsolToShipmentAndCheckETA()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];
			consol.JK_RL_NKLoadPort = "GBHAM";
			consol.JK_RL_NKDischargePort = "AUSYD";
			transport.JW_ETD = new ZDateTime(2010, 3, 4);
			transport.JW_ETA = new ZDateTime(2010, 4, 4);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "GBLON";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_E_DEP = new ZDateTime(2010, 3, 2);
			shipment.JS_E_ARV = new ZDateTime(2010, 4, 8);
			shipment.Consols.Add(consol);
			AssertEquals(new ZDateTime(2010, 4, 8), shipment.JS_E_ARV);
		}

		#endregion

		#region Implementation

		void AssertNoErrorContaining(StmALog log, Func<StmALog, ZPropertyInfo> propertyInfoGetter, string partialErrorMessage)
		{
			AssertNoErrorContaining(new StmALog[] { log }, propertyInfoGetter, partialErrorMessage);
		}

		void AssertNoErrorContaining(StmALog[] logs, Func<StmALog, ZPropertyInfo> propertyInfoGetter, string partialErrorMessage)
		{
			foreach (StmALog log in logs)
			{
				var propertyInfo = propertyInfoGetter(log);

				foreach (INotification notification in propertyInfoGetter(log).Notifications)
				{
					if (notification.Type.EnumValueName == "Error")
					{
						AssertNotContains(partialErrorMessage, notification.Message);
					}
				}
			}
		}

		#endregion

		#region Exception

		public void TestAvoidNullReferenceException_Functionality_UpdateDates()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.Transports.ParentDeleting();
			consol.Transports.FirstImportTransportByLoadAndDischarge();
			AssertNoExceptionThrown(() => ConsolShipmentRelationshipHelper.ShipmentAddedToConsol(consol, shipment));
		}

		#endregion
	}
}
