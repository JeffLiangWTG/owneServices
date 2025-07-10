using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CommonShipment = Enterprise.Freight.Business.CommonShipment;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed partial class AgencyShipmentTest : BaseAgencyTest
	{
		#region Events Management

		public void TestEventParameters_FreightLoaded()
		{
			var helper = new VoyageTestHelper(Factory);
			var carrier = helper.CreateCarrier("MAERSK");
			var voyage = helper.CreateSeaVoyage("Visund", "123", carrier.PK, "AUMEL", "NZWLG");

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.JS_ShippedOnBoardDate = ZDateTime.Now;

			var freightLoadedLog = shipment.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals("|FAC=CTO|LOC=AUMEL", freightLoadedLog.SL_Reference);
		}

		public void TestEventParameters_FreightLoaded_Matching()
		{
			var helper = new VoyageTestHelper(Factory);
			var carrier = helper.CreateCarrier("MAERSK");
			var voyage = helper.CreateSeaVoyage("Visund", "123", carrier.PK, "AUMEL", "NZWLG");

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.JS_JX = voyage.Sailings[0].PK;
			AssertEquals("Precondition", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			var eventTime = ZDateTimeOffset.Now;

			shipment.Logs.AddNew(Events.FreightLoaded, "|FAC=OFFICE|LOC=AUMEL", eventTime);
			AssertEquals("Facility not matched", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			shipment.Logs.AddNew(Events.FreightLoaded, "|FAC=CTO|LOC=USLAX", eventTime);
			AssertEquals("Location not matched", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);

			shipment.Logs.AddNew(Events.FreightLoaded, "|FAC=CTO|LOC=AUMEL", eventTime);
			AssertEquals("Parameters matched", eventTime.ToZDateTime(), shipment.JS_ShippedOnBoardDate);
		}

		public void TestFactorySave_ShipmentStatusIsUpdated_GenerateStatusUpdatedEvent()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<AgencyShipment>();

				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				Factory.Save();
				AssertEventHasBeenGenerated(
					shipment,
					Events.StatusUpdated,
					EventConstants.EventReferenceParameters.Codes.Type.As(Constants.EventReferenceMessageTypes.ShipmentStatus),
					EventConstants.EventReferenceParameters.Codes.New.As(ShipmentStatusList.Codes.Booked));

				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
				Factory.Save();
				AssertEventHasBeenGenerated(
					shipment,
					Events.StatusUpdated,
					EventConstants.EventReferenceParameters.Codes.Type.As(Constants.EventReferenceMessageTypes.ShipmentStatus),
					EventConstants.EventReferenceParameters.Codes.Old.As(ShipmentStatusList.Codes.Booked),
					EventConstants.EventReferenceParameters.Codes.New.As(ShipmentStatusList.Codes.ElectronicShippingInstruction),
					EventConstants.EventReferenceParameters.Codes.Reason.As(Constants.EventReferenceParameterReasons.ElectronicShippingInstructionReceived));

				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();
				AssertEventHasBeenGenerated(
					shipment,
					Events.StatusUpdated,
					EventConstants.EventReferenceParameters.Codes.Type.As(Constants.EventReferenceMessageTypes.ShipmentStatus),
					EventConstants.EventReferenceParameters.Codes.Old.As(ShipmentStatusList.Codes.ElectronicShippingInstruction),
					EventConstants.EventReferenceParameters.Codes.New.As(ShipmentStatusList.Codes.ElectronicBooking),
					EventConstants.EventReferenceParameters.Codes.Reason.As(Constants.EventReferenceParameterReasons.ElectronicBookingReceived));
			}
		}

		void AssertEventHasBeenGenerated(AgencyShipment shipment, Event expectedEvent, params KeyValuePair<string, string>[] expectedParameters)
		{
			var evnt = shipment.Logs.MostRecentLogByEventTime(expectedEvent);

			AssertNotNull(string.Format("A {0} has been generated", expectedEvent), evnt);

			foreach (var parameter in expectedParameters)
			{
				AssertEquals(string.Format("{0} parameter value", parameter.Key), parameter.Value, evnt.Parameters[parameter.Key]);
			}
		}

		#endregion

		public void TestPortCalculationConsistency()
		{
			var now = ZDateTime.Now;
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = CreateSailing("NZAKL", "USLAX", now).PK;

			var additionalLeg = shipment.Transports.AddNew();
			additionalLeg.JW_ETD = now.AddDays(-1);
			additionalLeg.JW_RL_NKLoadPort = "AUSYD";
			additionalLeg.JW_RL_NKDiscPort = "CNSHA";

			Factory.Save();
			ReleaseFactory();

			shipment = Factory.Load<AgencyShipment>(shipment.PK);
			var x = shipment.Sailing; // invoke Sailing getter
			AssertEquals("Load port should come from sailing first", "NZAKL", shipment.JS_NKLoadPort);
			AssertEquals("Discharge port should come from sailing first", "USLAX", shipment.JS_NKDischargePort);

			ReleaseFactory();

			shipment = Factory.Load<AgencyShipment>(shipment.PK);
			AssertEquals("Load port should come from sailing", "NZAKL", shipment.JS_NKLoadPort);
			AssertEquals("Discharge port should come from sailing first", "USLAX", shipment.JS_NKDischargePort);
		}

		public void TestFieldsSetToReadOnlyForMainTransportLeg()
		{
			var shipment1 = Factory.New<AgencyShipment>();
			var sailing = CreateSailing("AUSYD", "USLAX", ZDateTime.Today);
			shipment1.JS_JX = sailing.PK;
			AssertEquals(1, shipment1.Transports.Count);

			var transport = shipment1.Transports[0];
			CheckDefaultEditableFieldsForAgencyShipment(transport);
			Assert("Vessel should not be read only for main transport leg", !transport.JW_VesselInfo.ReadOnly);
			Assert("Flight should not be read only for main transport leg", !transport.JW_VoyageFlightInfo.ReadOnly);
			Assert("Load Port should not be read only for main transport leg", !transport.JW_RL_NKLoadPortInfo.ReadOnly);
			Assert("Carrier should not be read only for main transport leg", !transport.CarrierPKInfo.ReadOnly);

			var transport1 = shipment1.Transports.AddNew();
			transport1.JW_TransportType = Core.Constants.TransportModes.Air;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;

			CheckDefaultEditableFieldsForAgencyShipment(transport1);
			Assert("Vessel should not be read only", !transport1.JW_VesselInfo.ReadOnly);
			Assert("Flight should not be read only", !transport1.JW_VoyageFlightInfo.ReadOnly);
			Assert("Load Port should not be read only", !transport1.JW_RL_NKLoadPortInfo.ReadOnly);
			Assert("Carrier should not be read only", !transport1.CarrierPKInfo.ReadOnly);
		}

		public void TestFieldsSetToReadOnlyForTransportLegWhichBecomesMainAfterCreation()
		{
			var sailing = CreateSailing("AUSYD", "USLAX", ZDateTime.Today);
			var shipment = Factory.New<AgencyShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_JX = sailing.PK;
			transport.JW_TransportType = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

			shipment.JS_JX = sailing.PK;

			CheckDefaultEditableFieldsForAgencyShipment(transport);
			Assert("Vessel should not be read only for main transport leg", !transport.JW_VesselInfo.ReadOnly);
			Assert("Flight should not be read only for main transport leg", !transport.JW_VoyageFlightInfo.ReadOnly);
			Assert("Load Port should not be read only for main transport leg", !transport.JW_RL_NKLoadPortInfo.ReadOnly);
			Assert("Carrier should not be read only for main transport leg", !transport.CarrierPKInfo.ReadOnly);
		}

		void CheckDefaultEditableFieldsForAgencyShipment(Transport transport)
		{
			Assert("Parent should ALWAYS be read only", transport.JW_ParentDescriptionInfo.ReadOnly);
			Assert("Carrier Booking reference should not be ReadOnly", !transport.JW_CarrierBookingReferenceInfo.ReadOnly);
			Assert("Carrier Service should not be ReadOnly", !transport.JW_PL_NKCarrierServiceLevelInfo.ReadOnly);
			Assert("Creditor should not be ReadOnly", !transport.CreditorPKInfo.ReadOnly);
			Assert("Creditor Address should not be ReadOnly", !transport.JW_OA_CreditorAddressInfo.ReadOnly);
		}

		public void TestShipmentIsLoggedWhenSailingChanged()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "AVRORA";
			vessel2.RV_Name = "TITANIC";

			var voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			var voyage2 = Factory.NewWithValidTestData<JobVoyage>();
			voyage1.JV_VoyageFlight = "233S";
			voyage2.JV_VoyageFlight = "133N";
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;

			var origin1 = Factory.New<VoyageOrigin>();
			var origin2 = Factory.New<VoyageOrigin>();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin2.JA_RL_NKPortOfLoading = "USLAX";
			origin1.JA_JV = voyage1.PK;
			origin2.JA_JV = voyage2.PK;

			var destination1 = Factory.New<VoyageDestination>();
			var destination2 = Factory.New<VoyageDestination>();
			destination1.JB_RL_NKPortOfDischarge = "USLAX";
			destination2.JB_RL_NKPortOfDischarge = "AUDRW";
			destination1.JB_JV = voyage1.PK;
			destination2.JB_JV = voyage2.PK;

			var sailing1 = Factory.New<JobSailing>();
			var sailing2 = Factory.New<JobSailing>();
			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;
			sailing1.JX_JB = destination1.PK;
			sailing2.JX_JB = destination2.PK;

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();

			shipment.JS_JX = sailing1.PK;

			Factory.Save();
			AssertLogExists(shipment, @"SAILING ""AVRORA - 233S"" ADDED");

			shipment.JS_JX = ZGuid.Empty;
			shipment.JS_JX = sailing2.PK;

			Factory.Save();
			AssertLogExists(shipment, @"SAILING ""AVRORA - 233S"" REPLACED WITH ""TITANIC - 133N""");

			shipment.JS_JX = sailing1.PK;
			shipment.JS_JX = ZGuid.Empty;

			AssertLogExists(shipment, @"SAILING ""AVRORA - 233S"" REPLACED WITH ""TITANIC - 133N""");

			Factory.Save();
			AssertLogExists(shipment, @"SAILING ""TITANIC - 133N"" REMOVED");
		}

		void AssertLogExists(BusinessObject bizO, string reference, bool exists = true)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EDT");
			query.AddToFilter(StmALogSchema.SL_Reference, reference);

			if (exists)
			{
				AssertNotNull(Factory.LoadTop1<StmALog>(query));
			}
			else
			{
				AssertNull(Factory.LoadTop1<StmALog>(query));
			}
		}

		public void TestLogContainerChanges_BookedContainers()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Factory.Save();

			AssertLoggingContainerEvents(shipment, shipment.BookedContainers);
		}

		public void TestLogContainerChanges_RealContainers()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();

			AssertLoggingContainerEvents(shipment, shipment.RealContainers);
		}

		void AssertLoggingContainerEvents(AgencyShipment shipment, AgencyShipmentContainerDependentCollection containersCollection)
		{
			var container = containersCollection.AddNew();
			container.JC_ContainerNum = "AAAA";

			Factory.Save();

			var log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded);

			AssertNotNull("ContainerJobAdded log created", log);
			AssertEquals("ContainerJobAdded container number logged", "AAAA", log.SL_Reference);

			container.Delete();

			Factory.Save();

			log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved);

			AssertNotNull("ContainerJobRemoved log created", log);
			AssertEquals("ContainerJobRemoved container number logged", "AAAA", log.SL_Reference);
		}

		public void TestLogContainerChanges_DoNotLogAnyEventsIfShipmentIsNotInDatabase()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.BookedContainers.AddNew();
			shipment.RealContainers.AddNew();

			shipment.RealContainers[0].Delete();

			Factory.Save();

			AssertNull("should not log ContainerJobAdded for shipment not in db", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded));
			AssertNull("should not log ContainerJobRemoved for shipment not in db", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved));
		}

		public void TestLogContainerChanges_ContainerRemovedWithoutSaving()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "AAAA";

			Factory.Save();

			container.Delete();

			AssertEquals("prerequisite - container removed from collection", false, shipment.RealContainers.Contains(container.PK));
			AssertNull("ContainerJobRemoved log not created before saving", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved));

			Factory.Save();

			AssertNotNull("ContainerJobRemoved log created after saving", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved));
		}

		public void TestLogContainerChanges_EventReference_NoContainerNumber()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();

			var container = shipment.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 4;

			Factory.Save();

			var log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded);

			AssertNotNull("ContainerJobAdded log created", log);
			AssertEquals("ContainerJobAdded container number logged", "20GP (4)", log.SL_Reference);

			container.JC_ContainerNum = "AAAA";
			container.Delete();

			Factory.Save();

			log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved);

			AssertNotNull("ContainerJobRemoved log created", log);
			AssertEquals("ContainerJobRemoved container number logged, container number wasn't persisted so it wasn't used", "20GP (4)", log.SL_Reference);
		}

		public void TestLogContainerChanges_EventReference_WithContainerNumber()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();

			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "AAAA";

			Factory.Save();

			var log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded);

			AssertNotNull("ContainerJobAdded log created", log);
			AssertEquals("ContainerJobAdded container number logged", "AAAA", log.SL_Reference);

			container.Delete();

			Factory.Save();

			log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved);

			AssertNotNull("ContainerJobRemoved log created", log);
			AssertEquals("ContainerJobRemoved container number logged", "AAAA", log.SL_Reference);
		}

		public void TestLogContainerChanges_LoggingIsSuspendedDuringConfirmation()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

			var container = shipment.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 3;

			Factory.Save();

			AssertNull("prerequisite - ContainerJobAdded log not created", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded));

			shipment.Confirm();

			Factory.Save();

			AssertNull("ContainerJobAdded log not created during confirmation", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded));
		}

		public void TestLogContainerChanges_LoggingResumesAfterConfirmation()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

			var container = shipment.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 3;

			Factory.Save();

			AssertNull("prerequisite - ContainerJobAdded log not created", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded));

			shipment.Confirm();

			container.JC_ContainerNum = "XXXX";
			container.Delete();

			container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "BBBB";

			Factory.Save();

			var log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved);

			AssertNull("ContainerJobRemoved log not created as container wasn't in database", shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobRemoved));

			log = shipment.Logs.MostRecentLogByEventTime(Events.ContainerJobAdded);

			AssertNotNull("ContainerJobAdded log created", log);
			AssertEquals("ContainerJobAdded container number logged", "BBBB", log.SL_Reference);
		}

		public void TestLogContainerChanges_NoDangerousGoodsEventsAreLoggedDuringConfirmation()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

			var container = shipment.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 3;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew();

			Factory.Save();

			AssertNull("prerequisite - DangerousGoodsChanged log not created", shipment.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged));

			shipment.Confirm();
			Factory.Save();

			AssertNull("DangerousGoodsChanged log not created", shipment.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged));

			shipment.OuterPackLines[0].UNDGs.AddNew();

			Factory.Save();

			AssertNotNull("DangerousGoodsChanged log created", shipment.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged));
		}

		public void TestLogContainerChanges_NoForceLoadingOfPacklinesAndContainers()
		{
			var creationFactory = new BusinessObjectFactory();

			var shipment = creationFactory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			var packLine = shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].FillWithValidTestData();
			packLine.UNDGs.AddNew();
			packLine.UNDGs[0].FillWithValidTestData();

			var shippingContainer = shipment.ShippingContainers.AddNew();
			shipment.ShippingContainers[0].FillWithValidTestData();
			shippingContainer.UNDGs.AddNew();
			shippingContainer.UNDGs[0].FillWithValidTestData();

			creationFactory.Save();

			shipment = Factory.Load<AgencyShipment>(shipment.PK);

			AssertEquals("Prerequisite: containers are not loaded", 0, Factory.GetTableHitCount(JobContainerSchema.Constants.TableName));
			AssertEquals("Prerequisite: packlines are not loaded", 0, Factory.GetTableHitCount(JobPackLinesSchema.Constants.TableName));

			Factory.Save();

			AssertEquals("Should not force load containers on save", 0, Factory.GetTableHitCount(JobContainerSchema.Constants.TableName));
			AssertEquals("Should not force load packlines on save", 0, Factory.GetTableHitCount(JobPackLinesSchema.Constants.TableName));
		}

		public void TestOriginDestinationLogging()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			Factory.Save();

			AssertLogExists(shipment, "ORIGIN/DESTINATION SET TO AUSYD/USLAX", false);
			AssertLogExists(shipment, "ORIGIN CHANGED FROM AUSYD TO AUBNE", false);
			AssertLogExists(shipment, "ORIGIN/DESTINATION CHANGED FROM AUBNE/USLAX TO AUMEL/USNYC", false);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			AssertLogExists(shipment, "ORIGIN/DESTINATION SET TO AUSYD/USLAX");
			AssertLogExists(shipment, "ORIGIN CHANGED FROM AUSYD TO AUBNE", false);
			AssertLogExists(shipment, "ORIGIN/DESTINATION CHANGED FROM AUBNE/USLAX TO AUMEL/USNYC", false);

			shipment.JS_RL_NKOrigin = "AUBNE";
			Factory.Save();

			AssertLogExists(shipment, "ORIGIN/DESTINATION SET TO AUSYD/USLAX");
			AssertLogExists(shipment, "ORIGIN CHANGED FROM AUSYD TO AUBNE");
			AssertLogExists(shipment, "ORIGIN/DESTINATION CHANGED FROM AUBNE/USLAX TO AUMEL/USNYC", false);

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "USNYC";
			Factory.Save();

			AssertLogExists(shipment, "ORIGIN/DESTINATION SET TO AUSYD/USLAX");
			AssertLogExists(shipment, "ORIGIN CHANGED FROM AUSYD TO AUBNE");
			AssertLogExists(shipment, "ORIGIN/DESTINATION CHANGED FROM AUBNE/USLAX TO AUMEL/USNYC");
		}

		public void TestIVoyageFinderParentMembers()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "GBLON";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("IVoyageFinderParent.TransportMode", Core.Constants.TransportModes.Sea, ((IVoyageFinderParent)shipment).TransportMode);
			AssertEquals("IVoyageFinderParent.LoadPort", "AUSYD", ((IVoyageFinderParent)shipment).LoadPort);
			AssertEquals("IVoyageFinderParent.DischargePort", "GBLON", ((IVoyageFinderParent)shipment).DischargePort);
			AssertEquals("IVoyageFinderParent.CarrierPK", shipment.BookedShippingLinePK, ((IVoyageFinderParent)shipment).CarrierPK);

			shipment.JS_NKLoadPort = ZString.Empty;
			shipment.JS_NKDischargePort = ZString.Empty;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertEquals("IVoyageFinderParent.LoadPort should fallback to shipment's origin", "NZAKL", ((IVoyageFinderParent)shipment).LoadPort);
			AssertEquals("IVoyageFinderParent.DischargePort should fallback to shipment's destination", "USLAX", ((IVoyageFinderParent)shipment).DischargePort);
		}

		public void TestLoadFromPrefix()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			BusinessObject bo = secondFactory.Load("JS", shipment.PK);

			AssertType("Should be an AgencyBooking, not a forwarding shipment", typeof(AgencyBooking), bo);
		}

		public void TestAccessingSailingDoesNotSetHasChanges()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ExportSailing.PK;
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";

			Factory.Save();

			shipment = new BusinessObjectFactory().Load<AgencyShipment>(shipment.PK);

			CombineAssertions(delegate
			{
				AssertEquals("precondition: Origin", "", shipment.JS_RL_NKOrigin);
				AssertEquals("precondition: Destination", "", shipment.JS_RL_NKDestination);
			});

			JobSailing forTheSakeOfAccessing = shipment.Sailings.Count > 0 ? shipment.Sailings[0] : null;

			CombineAssertions(delegate
			{
				AssertEquals("Access sailing", ExportSailing.PK, forTheSakeOfAccessing.PK);
				AssertEquals("Origin should still be empty", "", shipment.JS_RL_NKOrigin);
				AssertEquals("Destination should still be empty", "", shipment.JS_RL_NKDestination);
				AssertEquals("Shipment should not have changes", false, shipment.HasChanges);
			});
		}

		public void TestDefaultBillWeightAndVolumeUnits()
		{
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicYards);
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.ShortTons);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("used bol volume registry setting", Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals("used bol weight registry setting", Constants.Weight.Tonnes, shipment.JS_UnitOfWeight);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("used booking volume registry setting", Constants.Volume.CubicYards, shipment.JS_UnitOfVolume);
			AssertEquals("used booking weight registry setting", Constants.Weight.ShortTons, shipment.JS_UnitOfWeight);

			shipment.OuterPackLines.RemoveAndDeleteAll();

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			line1.JL_ActualWeightUQ = Constants.Weight.Pounds;

			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			line2.JL_ActualWeightUQ = Constants.Weight.Pounds;

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("used packlines volume", Constants.Volume.CubicFeet, shipment.JS_UnitOfVolume);
			AssertEquals("used packlines weight", Constants.Weight.Pounds, shipment.JS_UnitOfWeight);
			AssertEquals("packline weight hasn't changed", Constants.Volume.CubicFeet, line1.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Pounds, line1.JL_ActualWeightUQ);
			AssertEquals("packline weight hasn't changed", Constants.Volume.CubicFeet, line2.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Pounds, line2.JL_ActualWeightUQ);

			shipment.JS_UnitOfVolume = ZString.Empty;
			shipment.JS_UnitOfWeight = ZString.Empty;

			line1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			line1.JL_ActualWeightUQ = Constants.Weight.Pounds;

			line2.JL_ActualVolumeUQ = ZString.Empty;
			line2.JL_ActualWeightUQ = ZString.Empty;

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("packlines with empty volume are not taken under consideration", Constants.Volume.CubicFeet, shipment.JS_UnitOfVolume);
			AssertEquals("packlines with empty weight are not taken under consideration", Constants.Weight.Pounds, shipment.JS_UnitOfWeight);
			AssertEquals("packline weight hasn't changed", Constants.Volume.CubicFeet, line1.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Pounds, line1.JL_ActualWeightUQ);
			AssertEquals("packline weight hasn't changed", ZString.Empty, line2.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", ZString.Empty, line2.JL_ActualWeightUQ);

			line2.JL_ActualVolumeUQ = Constants.Volume.Litre;
			line2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("volume defaulted from registry because of inconsistent unit on packline", Constants.Volume.CubicYards, shipment.JS_UnitOfVolume);
			AssertEquals("weight defaulted from registry because of inconsistent unit on packline", Constants.Weight.ShortTons, shipment.JS_UnitOfWeight);
			AssertEquals("packline weight hasn't changed", Constants.Volume.CubicFeet, line1.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Pounds, line1.JL_ActualWeightUQ);
			AssertEquals("packline weight hasn't changed", Constants.Volume.Litre, line2.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Kilograms, line2.JL_ActualWeightUQ);

			shipment.JS_UnitOfWeight = Constants.Volume.Litre;
			shipment.JS_UnitOfVolume = Constants.Weight.Kilograms;

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("volume always defaults for FCL", Constants.Volume.CubicYards, shipment.JS_UnitOfVolume);
			AssertEquals("weight always defaults for FCL", Constants.Weight.ShortTons, shipment.JS_UnitOfWeight);
			AssertEquals("packline weight hasn't changed", Constants.Volume.CubicFeet, line1.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Pounds, line1.JL_ActualWeightUQ);
			AssertEquals("packline weight hasn't changed", Constants.Volume.Litre, line2.JL_ActualVolumeUQ);
			AssertEquals("packline volume hasn't changed", Constants.Weight.Kilograms, line2.JL_ActualWeightUQ);

			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			shipment.DefaultWeightAndVolumeUnits();
			AssertEquals("volume does not defaulted for non-FCL", Constants.Volume.Litre, shipment.JS_UnitOfVolume);
			AssertEquals("weight does not defaulted for non-FCL", Constants.Weight.Kilograms, shipment.JS_UnitOfWeight);
		}

		public void TestConvertUnitsWithoutPacklines()
		{
			AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.Litre);
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);

			AgencyShipment shipment = Factory.New<AgencyBooking>();
			shipment.JS_GoodsDescription = "Magic";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_ContainerCount = 1;
			container.JC_TareWeight = 2.4;
			container.JC_Calc_NetWeight = 15;

			AssertEquals("precondition:", 0, shipment.OuterPackLines.Count);
			AssertNoExceptionThrown(shipment.Confirm);
			AssertEquals("no new packlines should have been added.", 0, shipment.OuterPackLines.Count);
		}

		#region Confirm

		public void TestConfirm_ShipmentStatus()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			if (shipment.Lookups.JS_ShipmentStatus_List == null)
			{ } // poke

			shipment.Confirm();
			AssertEquals("The Shipment should now be Confirmed", ShipmentStatusList.Codes.Confirmed, shipment.JS_ShipmentStatus);
		}

		public void TestConfirm_BookingConfirmedEventAdded()
		{
			var shipment = Factory.New<AgencyShipment>();
			AssertEquals("Precondition: not a bill of lading", false, shipment.IsBillOfLadingStage);

			StmALog[] logsBefore = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code));
			AssertEquals("A new shipment should not have a booking confirmed event log", 0, logsBefore.Length);

			shipment.Confirm();

			StmALog[] logsAfter = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code));
			AssertEquals("Should have a single booking confirmed event log", 1, logsAfter.Length);
		}

		public void TestConfirm_ConvertUnits()
		{
			AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.Litre);
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);

			AgencyShipment shipment = Factory.New<AgencyBooking>();
			shipment.JS_ActualWeight = 21;
			shipment.JS_ActualVolume = 26000;
			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.BookedContainers.RemoveAndDeleteAll();
			shipment.RealContainers.RemoveAndDeleteAll();

			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_ContainerCount = 1;
			container.JC_TareWeight = 2.4;

			AgencyShipmentPackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = container.PK;
			packline1.JL_ActualWeight = 9;
			packline1.JL_ActualVolume = 10000;

			AgencyShipmentPackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_JC = container.PK;
			packline2.JL_ActualWeight = 11;
			packline2.JL_ActualVolume = 15000;

			container.JC_GrossWeight = 23;

			shipment.Confirm();

			CombineAssertions(delegate
			{
				AssertEquals("shipment weight", 21000m, shipment.JS_ActualWeight);
				AssertEquals("shipment weight unit", Constants.Weight.Kilograms, shipment.JS_UnitOfWeight);
				AssertEquals("shipment volume", 26m, shipment.JS_ActualVolume);
				AssertEquals("shipment volume unit", Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);

				AssertEquals("packline1 weight", 9000m, packline1.JL_ActualWeight);
				AssertEquals("packline1 weight unit", Constants.Weight.Kilograms, packline1.JL_ActualWeightUQ);
				AssertEquals("packline1 volume", 10m, packline1.JL_ActualVolume);
				AssertEquals("packline1 volume unit", Constants.Volume.CubicMetres, packline1.JL_ActualVolumeUQ);

				AssertEquals("packline2 weight", 11000m, packline2.JL_ActualWeight);
				AssertEquals("packline2 weight unit", Constants.Weight.Kilograms, packline2.JL_ActualWeightUQ);
				AssertEquals("packline2 volume", 15m, packline2.JL_ActualVolume);
				AssertEquals("packline2 volume unit", Constants.Volume.CubicMetres, packline2.JL_ActualVolumeUQ);

				AssertEquals("booked container tare weight", 2.4m, container.JC_TareWeight);
				AssertEquals("booked container gross weight", 23m, container.JC_GrossWeight);
				AssertEquals("booked container weight unit", Constants.Weight.Tonnes, container.JC_GrossWeightUQ);

				AssertEquals("real container count", 1, shipment.RealContainers.Count);
			});

			CombineAssertions(delegate
			{
				CommonContainer realContainer = shipment.RealContainers[0];

				AssertEquals("real container tare weight", 2400m, realContainer.JC_TareWeight);
				AssertEquals("real container gross weight", 23000m, realContainer.JC_GrossWeight);
				AssertEquals("real container weight unit", Constants.Weight.Kilograms, realContainer.JC_GrossWeightUQ);
			});
		}

		public void TestConfirm_ConvertUnits_RoundedTo3Decimals()
		{
			AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Pounds);
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicFeet);

			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);

			AgencyShipment shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ActualWeight = 100.123;
			shipment.JS_ActualVolume = 8.192;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 200.234;
			packLine.JL_ActualVolume = 16.384;

			shipment.Confirm();

			AssertEquals("shipment weight", 45.415m, shipment.JS_ActualWeight);
			AssertEquals("shipment volume", 0.232m, shipment.JS_ActualVolume);

			AssertEquals("pack weight", 90.825m, packLine.JL_ActualWeight);
			AssertEquals("pack volume", 0.464m, packLine.JL_ActualVolume);
		}

		public void TestConfirm_ConvertTopLevelPacksUnits()
		{
			AgencyRegistry.Instance.ConvertToDefaultUnitsOnConfirmation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);

			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyBooking>();
				shipment.JS_PackingMode = mode;

				var container1 = shipment.BookedContainers.AddNew();
				container1.JC_GrossWeight = 1.024m;
				container1.JC_GrossWeightUQ = Constants.Weight.Tonnes;
				container1.JC_GrossVolume = 16m;
				container1.JC_GrossVolumeUQ = Constants.Volume.Litre;

				var container2 = shipment.BookedContainers.AddNew();
				container2.JC_GrossWeight = 2.048m;
				container2.JC_GrossWeightUQ = Constants.Weight.Tonnes;
				container2.JC_GrossVolume = 32m;
				container2.JC_GrossVolumeUQ = Constants.Volume.Litre;

				shipment.Confirm();

				CombineAssertions(delegate
				{
					AssertEquals("weight", 1024m, container1.JC_GrossWeight);
					AssertEquals("weight unit", Constants.Weight.Kilograms, container1.JC_GrossWeightUQ);
					AssertEquals("volume", 0.016m, container1.JC_GrossVolume);
					AssertEquals("volume unit", Constants.Volume.CubicMetres, container1.JC_GrossVolumeUQ);

					AssertEquals("weight", 2048m, container2.JC_GrossWeight);
					AssertEquals("weight unit", Constants.Weight.Kilograms, container2.JC_GrossWeightUQ);
					AssertEquals("volume", 0.032m, container2.JC_GrossVolume);
					AssertEquals("volume unit", Constants.Volume.CubicMetres, container2.JC_GrossVolumeUQ);
				});
			}
		}

		public void TestConfirm_ConvertTopLevelPacks()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = mode;

				var container1 = shipment.BookedContainers.AddNew();
				var container2 = shipment.BookedContainers.AddNew();

				AssertContainsExactElementsInAnyOrder(new AgencyShipmentContainer[] { container1, container2 }, shipment.BookedContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.RealContainers);
				AssertContainsExactElementsInAnyOrder(new AgencyShipmentContainer[] { container1, container2 }, shipment.TopLevelPacks);

				if (shipment.IsRollOnRollOff)
				{
					AssertContainsExactElementsInAnyOrder(new AgencyShipmentContainer[] { container1, container2 }, shipment.Vehicles);
				}

				shipment.Confirm();

				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.BookedContainers);
				AssertContainsExactElementsInAnyOrder(new AgencyShipmentContainer[] { container1, container2 }, shipment.RealContainers);
				AssertContainsExactElementsInAnyOrder(new AgencyShipmentContainer[] { container1, container2 }, shipment.TopLevelPacks);

				if (shipment.IsRollOnRollOff)
				{
					AssertContainsExactElementsInAnyOrder(new AgencyShipmentContainer[] { container1, container2 }, shipment.Vehicles);
				}
			}
		}

		public void TestConfirm_SplitVehicles()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			var vehicle = shipment.ShippingContainers.AddNew();
			vehicle.JC_ContainerCount = 3;
			vehicle.JC_GrossVolume = 6m;
			vehicle.JC_GrossWeight = 9m;

			shipment.Confirm();
			AssertEquals(3, shipment.ShippingContainers.Count);

			AssertEquals(3m, shipment.ShippingContainers[0].JC_GrossWeight);
			AssertEquals(3m, shipment.ShippingContainers[1].JC_GrossWeight);
			AssertEquals(3m, shipment.ShippingContainers[2].JC_GrossWeight);

			AssertEquals(2m, shipment.ShippingContainers[0].JC_GrossVolume);
			AssertEquals(2m, shipment.ShippingContainers[1].JC_GrossVolume);
			AssertEquals(2m, shipment.ShippingContainers[2].JC_GrossVolume);
		}

		#endregion

		public void TestDefaultBillWeightUnit()
		{
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();

			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();

			AssertEquals("Shipment1", Constants.Weight.Kilograms, shipment1.JS_UnitOfWeight);
			AssertEquals("Shipment2", Constants.Weight.Tonnes, shipment2.JS_UnitOfWeight);
		}

		public void TestDefaultBillVolumeUnit()
		{
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();

			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.MegaLitre);
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();

			AssertEquals("Shipment1", Constants.Volume.CubicMetres, shipment1.JS_UnitOfVolume);
			AssertEquals("Shipment2", Constants.Volume.MegaLitre, shipment2.JS_UnitOfVolume);
		}

		public void TestRegistryWeightUnit()
		{
			AgencyRegistry.Instance.DefaultBillWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Kilograms);
			AgencyRegistry.Instance.DefaultBookingWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertEquals("Precondition", false, shipment.IsBillOfLadingStage);
			AssertEquals(Constants.Weight.Tonnes, shipment.RegistryWeightUnit);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals("Precondition", true, shipment.IsBillOfLadingStage);
			AssertEquals(Constants.Weight.Kilograms, shipment.RegistryWeightUnit);
		}

		public void TestRegistryVolumeUnit()
		{
			AgencyRegistry.Instance.DefaultBillVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.CubicMetres);
			AgencyRegistry.Instance.DefaultBookingVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Volume.MegaLitre);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertEquals("Precondition", false, shipment.IsBillOfLadingStage);
			AssertEquals(Constants.Volume.MegaLitre, shipment.RegistryVolumeUnit);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals("Precondition", true, shipment.IsBillOfLadingStage);
			AssertEquals(Constants.Volume.CubicMetres, shipment.RegistryVolumeUnit);
		}

		public void TestGetNewValidation()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AssertEquals("Type of Validation", typeof(AgencyShipmentValidation), shipment.Validation.GetType());
		}

		public void TestChangingIsDomesticFreghtWorksWithAgencyIncoTerms()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;
			shipment.IsDomesticFreight = true;
			shipment.IsDomesticFreight = false;
			AssertEquals(Constants.DomesticPaymentTerms.Collect, shipment.JS_INCO);

			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			shipment.IsDomesticFreight = true;
			shipment.IsDomesticFreight = false;
			AssertEquals(Constants.DomesticPaymentTerms.Prepaid, shipment.JS_INCO);
		}

		public void TestGetCanOverrideCheckpoint()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			AssertEquals("Prequisite", true, shipment.IsImport());

			SecurityCheckpoint checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress);
			AssertEquals(Env.Security.None, checkpoint);

			checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress);
			AssertEquals(Env.Security.None, checkpoint);

			checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsignorPickupAddress);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestLoadDischargePersistance()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today).PK;

			Factory.Save();

			BusinessObjectFactory newFactory;
			AgencyShipment shipmentInNewFactory;

			newFactory = new BusinessObjectFactory();
			shipmentInNewFactory = newFactory.Load<AgencyShipment>(shipment.PK);

			AssertEquals("Load Port", "AUBNE", shipmentInNewFactory.JS_NKLoadPort);

			AssertEquals("Discharge Port", "NLAMS", shipmentInNewFactory.JS_NKDischargePort);
			AssertNotNull("Discharge Port", shipmentInNewFactory.CalcDischargePort);
			AssertEquals("Discharge Port", "NLAMS", shipmentInNewFactory.CalcDischargePort.Code);

			shipmentInNewFactory.JS_NKLoadPort = "AUSYD";
			AssertEquals("Load Port", "AUSYD", shipmentInNewFactory.JS_NKLoadPort);
			AssertEquals("Discharge Port (setting the load port should not clear the discharge port)", "NLAMS", shipmentInNewFactory.JS_NKDischargePort);

			newFactory = new BusinessObjectFactory();
			shipmentInNewFactory = newFactory.Load<AgencyShipment>(shipment.PK);

			AssertEquals("Load Port", "AUBNE", shipmentInNewFactory.JS_NKLoadPort);
			AssertEquals("Discharge Port", "NLAMS", shipmentInNewFactory.JS_NKDischargePort);

			shipmentInNewFactory.JS_NKDischargePort = "NZAKL";
			AssertEquals("Load Port (setting the discharge port should not clear the load port)", "AUBNE", shipmentInNewFactory.JS_NKLoadPort);
			AssertEquals("Discharge Port", "NZAKL", shipmentInNewFactory.JS_NKDischargePort);
		}

		public void TestJS_A_BKD()
		{
			JobSailing sailing0 = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today.AddDays(-1));
			JobSailing sailing1 = CreateSailing("AUBNE", "SGSIN", ZDateTime.Today.AddDays(1));
			JobSailing sailing2 = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today.AddDays(2));
			JobSailing sailing3 = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today.AddDays(3));
			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_A_BKD = ZDateTime.Empty;
			shipment.JS_NKLoadPort = "AUBNE";
			shipment.JS_NKDischargePort = "NLAMS";
			AssertEquals("Expecting no sailing to be chosen.", ZGuid.Empty, shipment.JS_JX);

			shipment.JS_A_BKD = ZDateTime.Now;
			AssertEquals("Expected sailing2 (first non-expired sailing matching ports)", sailing2.PK, shipment.JS_JX);
		}

		public void TestJS_NKLoadPort()
		{
			JobSailing sailing1 = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today);
			JobSailing sailing2 = CreateSailing("AUSYD", "NLAMS", ZDateTime.Today);
			JobSailing sailing3 = CreateSailing("AUCNS", "NLAMS", ZDateTime.Today);
			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_A_BKD = ZDateTime.Today;
			shipment.JS_NKLoadPort = "";
			AssertNull("Load Port", shipment.CalcLoadPort);
			shipment.JS_NKDischargePort = "NLAMS";
			AssertEquals("Expecting no sailing to be chosen", ZGuid.Empty, shipment.JS_JX);

			shipment.JS_NKLoadPort = "AUSYD";
			AssertNotNull("Load Port", shipment.CalcLoadPort);
			AssertEquals("Load Port Code", "AUSYD", shipment.CalcLoadPort.Code);
			AssertEquals("Expected sailing2", sailing2.PK, shipment.JS_JX);

			shipment.JS_NKLoadPort = "";
			AssertNull("Load Port", shipment.CalcLoadPort);
			AssertEquals("Expecting no sailing to be chosen", ZGuid.Empty, shipment.JS_JX);
			AssertEquals("Expecting load port to be empty", "", shipment.JS_NKLoadPort);

			shipment.JS_NKLoadPort = "AUCNS";
			AssertNotNull("Load Port", shipment.CalcLoadPort);
			AssertEquals("Load Port Code", "AUCNS", shipment.CalcLoadPort.Code);
			AssertEquals("Expected sailing3", sailing3.PK, shipment.JS_JX);

			AssertEquals("Should NOT be ReadOnly", false, shipment.JS_NKLoadPortInfo.ReadOnly);
			Factory.Save();
			AssertEquals("Should be ReadOnly", true, shipment.JS_NKLoadPortInfo.ReadOnly);

			shipment.JS_NKLoadPort = "";
			AssertNotNull("Load Port", shipment.CalcLoadPort);
			AssertEquals("Load Port Code", "AUCNS", shipment.CalcLoadPort.Code);
			AssertEquals("Expected sailing3", sailing3.PK, shipment.JS_JX);
		}

		public void TestJS_NKDischargePort()
		{
			JobSailing sailing1 = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today);
			JobSailing sailing2 = CreateSailing("AUBNE", "SGSIN", ZDateTime.Today);
			JobSailing sailing3 = CreateSailing("AUBNE", "GBLON", ZDateTime.Today);
			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_A_BKD = ZDateTime.Today;
			shipment.JS_NKLoadPort = "AUBNE";
			shipment.JS_NKDischargePort = "";
			AssertNull("Discharge Port", shipment.CalcDischargePort);
			AssertEquals("Expecting no sailing to be chosen", ZGuid.Empty, shipment.JS_JX);

			shipment.JS_NKDischargePort = "SGSIN";
			AssertNotNull("Discharge Port", shipment.CalcDischargePort);
			AssertEquals("Discharge Port Code", "SGSIN", shipment.CalcDischargePort.Code);
			AssertEquals("Expected sailing2", sailing2.PK, shipment.JS_JX);

			shipment.JS_NKDischargePort = "";
			AssertNull("Discharge Port", shipment.CalcDischargePort);
			AssertEquals("Expecting no sailing to be chosen", ZGuid.Empty, shipment.JS_JX);
			AssertEquals("Expecting discharge port to be empty", "", shipment.JS_NKDischargePort);

			shipment.JS_NKDischargePort = "GBLON";
			AssertNotNull("Discharge Port", shipment.CalcDischargePort);
			AssertEquals("Discharge Port Code", "GBLON", shipment.CalcDischargePort.Code);
			AssertEquals("Expected sailing2", sailing3.PK, shipment.JS_JX);

			AssertEquals("Should NOT be ReadOnly", false, shipment.JS_NKDischargePortInfo.ReadOnly);
			Factory.Save();
			AssertEquals("Should be ReadOnly", true, shipment.JS_NKDischargePortInfo.ReadOnly);

			shipment.JS_NKDischargePort = "";
			AssertNotNull("Discharge Port", shipment.CalcDischargePort);
			AssertEquals("Discharge Port Code", "GBLON", shipment.CalcDischargePort.Code);
			AssertEquals("Expected sailing2", sailing3.PK, shipment.JS_JX);
		}

		public void TestFCLBookingRefDetection()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			sailing.JX_ReservedMasterBill = "123456";

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_JX = sailing.PK;

			AssertEquals("JS_CFSReference set when setting JS_JX.", ZString.Empty, shipment.JS_CFSReference);
		}

		public void TestJS_Calc_TEUCount()
		{
			RefContainer gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			AgencyShipment booking = Factory.New<AgencyShipment>();
			booking.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container1 = booking.BookedContainers.AddNew();
			container1.JC_RC = gp20.PK;
			container1.JC_ContainerCount = 3;

			AgencyShipmentContainer container2 = booking.BookedContainers.AddNew();
			container2.JC_RC = gp40.PK;
			container2.JC_ContainerCount = 5;

			AssertEquals("TEU Count for booking is 13", 13m, booking.JS_Calc_TEUCount);
		}

		public void TestJS_Calc_ContainerCount()
		{
			AgencyShipment booking = Factory.New<AgencyShipment>();
			booking.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container1 = booking.BookedContainers.AddNew();
			container1.JC_ContainerCount = 1;

			AgencyShipmentContainer container2 = booking.BookedContainers.AddNew();
			container2.JC_ContainerCount = 4;

			AgencyShipmentContainer container3 = booking.BookedContainers.AddNew();
			container3.JC_ContainerCount = 0;

			AssertEquals("Shipment should have 3 containers.", 3, booking.BookedContainers.Count);
			AssertEquals("TEU Count for Shipment is 6", 6, booking.JS_Calc_ContainerCount);
		}

		public void TestHasETDPassed()
		{
			JobSailing sailing = CreateSailing("AUBNE", "NLAMS", ZDateTime.Empty);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			AssertEquals("no date", ZBool.False, shipment.HasETDPassed);

			sailing.Origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			AssertEquals("HasETDPassed with tomorrows date", ZBool.False, shipment.HasETDPassed);

			sailing.Origin.JA_E_DEP = ZDateTime.Today.AddDays(-1);
			AssertEquals("HasETDPassed with yesterdays date", ZBool.True, shipment.HasETDPassed);
		}

		public void TestCalcFieldsCopied()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today).PK;

			AgencyShipment copiedShipment = (AgencyShipment)shipment.TemplateCopy();
			AssertEquals("Calculated Load Port Copied", "AUBNE", copiedShipment.JS_NKLoadPort);
			AssertEquals("Calculated Discharge Port Copied", "NLAMS", copiedShipment.JS_NKDischargePort);
		}

		public void TestOnLoaded()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = CreateSailing("AUSYD", "USLAX", ZDateTime.Today).PK;
			shipment.JS_GoodsDescription = "Goods";
			shipment.JS_OuterPacks = 3;

			AgencyShipment canceledShipment = Factory.New<AgencyShipment>();
			canceledShipment.JS_JX = CreateSailing("AUSYD", "USLAX", ZDateTime.Today).PK;
			canceledShipment.JS_GoodsDescription = "Goods";
			canceledShipment.JS_OuterPacks = 3;
			canceledShipment.JS_IsCancelled = ZBool.True;

			Factory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();

			AgencyShipment loadedShipment = loadFactory.Load<AgencyShipment>(shipment.PK);
			Assert("Expecting loaded shipment's date to be read only.", loadedShipment.JS_A_BKDInfo.ReadOnly);
			AssertNotNull("Expecting loaded shipment's sailing not to be null.", loadedShipment.Sailing);

			AgencyShipment loadedCanceledShipment = loadFactory.Load<AgencyShipment>(canceledShipment.PK);
			Assert("Expecting loaded shipment to be read only.", loadedCanceledShipment.ReadOnly);
		}

		public void TestBookingContainers()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			RefContainer containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container1.JC_RC = containerType.PK;

			Factory.Save();

			AssertEquals("Expecting booking containers to have 1 container.", 1, shipment.BookedContainers.Count);
		}

		#region Test FCL Real and Booked Containers

		public void TestFCLBookedContainers()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var container = shipment.FCLBookedContainers.AddNew();
			AssertEquals("container mode set to FCL for new", Core.Constants.ContainerModes.FCL, container.JC_ContainerMode);

			foreach (var mode in AgencyShipmentContainerModeList.GetContainerModes(Core.Constants.ContainerModes.FCL))
			{
				container.JC_ContainerMode = mode;

				shipment.FCLContainers.Rebuild();
				shipment.FCLBookedContainers.Rebuild();
				shipment.Vehicles.Rebuild();
				shipment.TopLevelPacks.Rebuild();

				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.RealContainers);
				AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.FCLBookedContainers);
				AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.BookedContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.Vehicles);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.TopLevelPacks);
			}
		}

		public void TestFCLContainers()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var container = shipment.FCLContainers.AddNew();
			AssertEquals("container mode set to FCL for new", Core.Constants.ContainerModes.FCL, container.JC_ContainerMode);

			foreach (var mode in AgencyShipmentContainerModeList.GetContainerModes(Core.Constants.ContainerModes.FCL))
			{
				container.JC_ContainerMode = mode;

				shipment.FCLContainers.Rebuild();
				shipment.FCLBookedContainers.Rebuild();
				shipment.Vehicles.Rebuild();
				shipment.TopLevelPacks.Rebuild();

				AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.FCLContainers);
				AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.RealContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLBookedContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.BookedContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.Vehicles);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.TopLevelPacks);
			}
		}

		public void TestFCLContainersIncludesRealContainersWithNoMode()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var fclContainer = shipment.RealContainers.AddNew();
			fclContainer.JC_ContainerMode = Constants.ContainerModes.FCL;

			var lclContainer = shipment.RealContainers.AddNew();
			lclContainer.JC_ContainerMode = Constants.ContainerModes.LCL;

			var liquidContainer = shipment.RealContainers.AddNew();
			liquidContainer.JC_ContainerMode = Constants.ContainerModes.Liquid;

			shipment.FCLContainers.Rebuild();

			AssertEquals("Expected to empty the liquid container's mode as the type is incompatiable", ZString.Empty, liquidContainer.JC_ContainerMode);

			var expectedCollection = new[] { fclContainer, lclContainer, liquidContainer };

			AssertContainsExactElementsInAnyOrder(expectedCollection, shipment.FCLContainers);
			AssertContainsExactElementsInAnyOrder(expectedCollection, shipment.RealContainers);
		}

		public void TestFCLContainersIncludesBookedContainersWithNoMode()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var fclContainer = shipment.BookedContainers.AddNew();
			fclContainer.JC_ContainerMode = Constants.ContainerModes.FCL;

			var lclContainer = shipment.BookedContainers.AddNew();
			lclContainer.JC_ContainerMode = Constants.ContainerModes.LCL;

			var liquidContainer = shipment.BookedContainers.AddNew();
			liquidContainer.JC_ContainerMode = Constants.ContainerModes.Liquid;

			shipment.FCLBookedContainers.Rebuild();

			AssertEquals("Expected to empty the liquid container's mode as the type is incompatiable", ZString.Empty, liquidContainer.JC_ContainerMode);

			var expectedCollection = new[] { fclContainer, lclContainer, liquidContainer };

			AssertContainsExactElementsInAnyOrder(expectedCollection, shipment.FCLBookedContainers);
			AssertContainsExactElementsInAnyOrder(expectedCollection, shipment.BookedContainers);
		}

		#endregion

		public void TestBookedVehicles()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;

			var container = shipment.Vehicles.AddNew();
			AssertEquals("container mode set to FCL for new", Core.Constants.ContainerModes.RollOnRollOff, container.JC_ContainerMode);

			foreach (var mode in AgencyShipmentContainerModeList.GetContainerModes(Core.Constants.ContainerModes.RollOnRollOff))
			{
				container.JC_ContainerMode = mode;

				shipment.FCLContainers.Rebuild();
				shipment.FCLBookedContainers.Rebuild();
				shipment.Vehicles.Rebuild();

				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLBookedContainers);
				AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.Vehicles);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.RealContainers);
				AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.BookedContainers);
			}
		}

		public void TestVehicles()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;

			var vehicle = shipment.Vehicles.AddNew();
			AssertEquals("container mode set to FCL for new", Core.Constants.ContainerModes.RollOnRollOff, vehicle.JC_ContainerMode);

			foreach (var mode in AgencyShipmentContainerModeList.GetContainerModes(Core.Constants.ContainerModes.RollOnRollOff))
			{
				vehicle.JC_ContainerMode = mode;

				shipment.FCLContainers.Rebuild();
				shipment.FCLBookedContainers.Rebuild();
				shipment.Vehicles.Rebuild();

				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLBookedContainers);
				AssertContainsExactElementsInAnyOrder(new[] { vehicle }, shipment.Vehicles);
				AssertContainsExactElementsInAnyOrder(new[] { vehicle }, shipment.RealContainers);
				AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.BookedContainers);
			}
		}

		public void TestBookedTopLevelPacks()
		{
			foreach (var shipmentMode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
				shipment.JS_PackingMode = shipmentMode;

				var container = shipment.TopLevelPacks.AddNew();
				AssertEquals("container mode set to shipment mode for new", shipmentMode, container.JC_ContainerMode);

				foreach (var mode in AgencyShipmentContainerModeList.GetContainerModes(shipmentMode))
				{
					container.JC_ContainerMode = mode;

					shipment.FCLContainers.Rebuild();
					shipment.FCLBookedContainers.Rebuild();
					shipment.TopLevelPacks.Rebuild();

					AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLContainers);
					AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLBookedContainers);
					AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.TopLevelPacks);
					AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.RealContainers);
					AssertContainsExactElementsInAnyOrder(new[] { container }, shipment.BookedContainers);
				}
			}
		}

		public void TestTopLevelPacks()
		{
			foreach (var shipmentMode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				shipment.JS_PackingMode = shipmentMode;

				var topLevelPack = shipment.TopLevelPacks.AddNew();
				AssertEquals("container mode set to shipment mode for new", shipmentMode, topLevelPack.JC_ContainerMode);

				foreach (var mode in AgencyShipmentContainerModeList.GetContainerModes(shipmentMode))
				{
					topLevelPack.JC_ContainerMode = mode;

					shipment.FCLContainers.Rebuild();
					shipment.FCLBookedContainers.Rebuild();
					shipment.TopLevelPacks.Rebuild();

					AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLContainers);
					AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.FCLBookedContainers);
					AssertContainsExactElementsInAnyOrder(new[] { topLevelPack }, shipment.TopLevelPacks);
					AssertContainsExactElementsInAnyOrder(new[] { topLevelPack }, shipment.RealContainers);
					AssertContainsExactElementsInAnyOrder(Array.Empty<AgencyShipmentContainer>(), shipment.BookedContainers);
				}
			}
		}

		public void TestTopLevelPacksTotals()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Pallet;

			var topLevelPack1 = shipment.TopLevelPacks.AddNew();
			topLevelPack1.JC_ContainerCount = 10;
			topLevelPack1.JC_GrossWeight = 100m;
			topLevelPack1.JC_GrossVolume = 1.1m;
			topLevelPack1.JC_F3_NKPackType = Constants.PkgUnit.Drum;

			var topLevelPack2 = shipment.TopLevelPacks.AddNew();
			topLevelPack2.JC_ContainerCount = 20;
			topLevelPack2.JC_GrossWeight = 200m;
			topLevelPack2.JC_GrossVolume = 2.2m;
			topLevelPack2.JC_F3_NKPackType = Constants.PkgUnit.Drum;

			AssertEquals(30, shipment.TopLevelPacksTotalPacks);
			AssertEquals(300m, shipment.TopLevelPacksTotalWeightInShipmentWeightUnit);
			AssertEquals(3.3m, shipment.TopLevelPacksTotalVolumeInShipmentVolumeUnit);
			AssertEquals(Constants.PkgUnit.Drum, shipment.TopLevelPacksPackagesUnit);
		}

		public void TestUpdateVehiclesFromShipment()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;

			var vehicle1 = shipment.Vehicles.AddNew();

			AssertEquals("prerequisite", 0m, vehicle1.JC_GrossWeight);
			AssertEquals("prerequisite", Core.Constants.Weight.Kilograms, vehicle1.JC_GrossWeightUQ);
			AssertEquals("prerequisite", 0m, vehicle1.JC_GrossVolume);
			AssertEquals("prerequisite", Core.Constants.Volume.CubicMetres, vehicle1.JC_GrossVolumeUQ);
			AssertEquals("prerequisite", ZString.Empty, vehicle1.JC_Description);

			shipment.JS_ActualWeight = 2500m;
			AssertEquals("JC_GrossWeight updated from JS_ActualWeight", 2500m, vehicle1.JC_GrossWeight);

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals("JC_GrossWeightUQ updated from JS_UnitOfWeight", Core.Constants.Weight.Pounds, vehicle1.JC_GrossWeightUQ);

			shipment.JS_ActualVolume = 10.5m;
			AssertEquals("JC_GrossVolume updated from JS_ActualVolume", 10.5m, vehicle1.JC_GrossVolume);

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals("JC_GrossVolumeUQ updated from JS_UnitOfVolume", Core.Constants.Volume.CubicFeet, vehicle1.JC_GrossVolumeUQ);

			shipment.JS_GoodsDescription = "cars";
			AssertEquals("JC_Description updated from JS_GoodsDescription", "cars", vehicle1.JC_Description);

			var vehicle2 = shipment.Vehicles.AddNew();
			vehicle2.JC_GrossWeight = vehicle1.JC_GrossWeight;
			vehicle2.JC_GrossWeightUQ = vehicle1.JC_GrossWeightUQ;
			vehicle2.JC_GrossVolume = vehicle1.JC_GrossVolume;
			vehicle2.JC_GrossVolumeUQ = vehicle1.JC_GrossVolumeUQ;
			vehicle2.JC_Description = vehicle1.JC_Description;

			shipment.JS_ActualWeight = 3500m;
			AssertEquals("JC_GrossWeight not updated from JS_ActualWeight", 2500m, vehicle1.JC_GrossWeight);
			AssertEquals("JC_GrossWeight not updated from JS_ActualWeight", 2500m, vehicle2.JC_GrossWeight);

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AssertEquals("JC_GrossWeightUQ updated from JS_UnitOfWeight", Core.Constants.Weight.Kilograms, vehicle1.JC_GrossWeightUQ);
			AssertEquals("JC_GrossWeightUQ updated from JS_UnitOfWeight", Core.Constants.Weight.Kilograms, vehicle2.JC_GrossWeightUQ);

			shipment.JS_ActualVolume = 20.5m;
			AssertEquals("JC_GrossVolume not updated from JS_ActualVolume", 10.5m, vehicle1.JC_GrossVolume);
			AssertEquals("JC_GrossVolume not updated from JS_ActualVolume", 10.5m, vehicle1.JC_GrossVolume);

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			AssertEquals("JC_GrossVolumeUQ updated from JS_UnitOfVolume", Core.Constants.Volume.CubicMetres, vehicle1.JC_GrossVolumeUQ);
			AssertEquals("JC_GrossVolumeUQ updated from JS_UnitOfVolume", Core.Constants.Volume.CubicMetres, vehicle2.JC_GrossVolumeUQ);

			shipment.JS_GoodsDescription = "fast cars";
			AssertEquals("JC_Description updated from JS_GoodsDescription", "fast cars", vehicle1.JC_Description);
			AssertEquals("JC_Description updated from JS_GoodsDescription", "fast cars", vehicle1.JC_Description);
		}

		public void TestUpdateTopLevelPacksFromShipment()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
				shipment.JS_PackingMode = mode;

				var topLevelPack1 = shipment.TopLevelPacks.AddNew();

				AssertEquals("prerequisite", 0m, topLevelPack1.JC_GrossWeight);
				AssertEquals("prerequisite", Core.Constants.Weight.Kilograms, topLevelPack1.JC_GrossWeightUQ);
				AssertEquals("prerequisite", 0m, topLevelPack1.JC_GrossVolume);
				AssertEquals("prerequisite", Core.Constants.Volume.CubicMetres, topLevelPack1.JC_GrossVolumeUQ);
				AssertEquals("prerequisite", ZString.Empty, topLevelPack1.JC_Description);

				shipment.JS_ActualWeight = 2500m;
				AssertEquals("JC_GrossWeight updated from JS_ActualWeight", 2500m, topLevelPack1.JC_GrossWeight);

				shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
				AssertEquals("JC_GrossWeightUQ updated from JS_UnitOfWeight", Core.Constants.Weight.Pounds, topLevelPack1.JC_GrossWeightUQ);

				shipment.JS_ActualVolume = 10.5m;
				AssertEquals("JC_GrossVolume updated from JS_ActualVolume", 10.5m, topLevelPack1.JC_GrossVolume);

				shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
				AssertEquals("JC_GrossVolumeUQ updated from JS_UnitOfVolume", Core.Constants.Volume.CubicFeet, topLevelPack1.JC_GrossVolumeUQ);

				shipment.JS_GoodsDescription = "cars";
				AssertEquals("JC_Description updated from JS_GoodsDescription", "cars", topLevelPack1.JC_Description);

				var topLevelPack2 = shipment.TopLevelPacks.AddNew();
				topLevelPack2.JC_GrossWeight = topLevelPack1.JC_GrossWeight;
				topLevelPack2.JC_GrossWeightUQ = topLevelPack1.JC_GrossWeightUQ;
				topLevelPack2.JC_GrossVolume = topLevelPack1.JC_GrossVolume;
				topLevelPack2.JC_GrossVolumeUQ = topLevelPack1.JC_GrossVolumeUQ;
				topLevelPack2.JC_Description = topLevelPack1.JC_Description;

				shipment.JS_ActualWeight = 3500m;
				AssertEquals("JC_GrossWeight not updated from JS_ActualWeight", 2500m, topLevelPack1.JC_GrossWeight);
				AssertEquals("JC_GrossWeight not updated from JS_ActualWeight", 2500m, topLevelPack2.JC_GrossWeight);

				shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
				AssertEquals("JC_GrossWeightUQ updated from JS_UnitOfWeight", Core.Constants.Weight.Kilograms, topLevelPack1.JC_GrossWeightUQ);
				AssertEquals("JC_GrossWeightUQ updated from JS_UnitOfWeight", Core.Constants.Weight.Kilograms, topLevelPack2.JC_GrossWeightUQ);

				shipment.JS_ActualVolume = 20.5m;
				AssertEquals("JC_GrossVolume not updated from JS_ActualVolume", 10.5m, topLevelPack1.JC_GrossVolume);
				AssertEquals("JC_GrossVolume not updated from JS_ActualVolume", 10.5m, topLevelPack1.JC_GrossVolume);

				shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
				AssertEquals("JC_GrossVolumeUQ updated from JS_UnitOfVolume", Core.Constants.Volume.CubicMetres, topLevelPack1.JC_GrossVolumeUQ);
				AssertEquals("JC_GrossVolumeUQ updated from JS_UnitOfVolume", Core.Constants.Volume.CubicMetres, topLevelPack2.JC_GrossVolumeUQ);

				shipment.JS_GoodsDescription = "fast cars";
				AssertEquals("JC_Description updated from JS_GoodsDescription", "fast cars", topLevelPack1.JC_Description);
				AssertEquals("JC_Description updated from JS_GoodsDescription", "fast cars", topLevelPack1.JC_Description);
			}
		}

		public void TestSailing()
		{
			JobSailing sailing = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing.PK;

			AssertEquals("PreCondition: Sailings count == 1", 1, shipment.Sailings.Count);
			AssertNotNull("Sailing should not be null when Sailings count > 0", shipment.Sailing);

			shipment.Sailings.RemoveAll();
			shipment.JS_JX = ZGuid.Empty;
			AssertNull("Sailing should be null when Sailings count == 0", shipment.Sailing);
		}

		public void TestSailings()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertEquals("No Sailings", 0, shipment.Sailings.Count);

			shipment.JS_JX = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today).PK;
			AssertEquals("Sailings when JS_JX set", 1, shipment.Sailings.Count);

			shipment.JS_JX = CreateSailing("AUBNE", "GBLON", ZDateTime.Today).PK;
			AssertEquals("Sailings when JS_JX set again", 1, shipment.Sailings.Count);

			shipment.JS_JX = ZGuid.Empty;
			AssertEquals("Sailings when JS_JX set to ZGuid.Empty", 0, shipment.Sailings.Count);
		}

		public void TestClearSailing()
		{
			var today = ZDateTime.Today;

			var sailing = CreateSailing("AUSYD", "CNCAN", today.AddDays(1));
			Factory.Save();

			var shipment = Factory.New<AgencyShipment>();
			AssertNull(shipment.Sailing);

			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "CNCAN";
			shipment.JS_E_DEP = today.AddDays(1);

			AssertEquals("Sailing should be updated.", sailing, shipment.Sailing);
			AssertNotNull(shipment.Sailing);

			shipment.JS_JX = ZGuid.Empty;
			shipment.JS_NKLoadPort = "";
			shipment.JS_NKDischargePort = "";
			shipment.JS_E_DEP = ZDateTime.Invalid;
			AssertNull(shipment.Sailing);
		}

		public void TestHasChanges_WhenDepartureIsNotChanged()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.Logs.RemoveAndDeleteAll();

			Factory.Save();

			shipment.JS_E_DEP = ZDateTime.Today;
			Assert(!shipment.HasChanges);
		}

		public void TestHasChanges_WhenArrivalIsNotChanged()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_E_ARV = ZDateTime.Today;
			shipment.Logs.RemoveAndDeleteAll();

			Factory.Save();

			shipment.JS_E_ARV = ZDateTime.Today;
			Assert(!shipment.HasChanges);
		}

		public void TestGetNextSailing()
		{
			ZDateTime today = ZDateTime.Today;

			JobSailing sailing1 = CreateSailing("AUSYD", "CNCAN", today.AddDays(2));
			JobSailing sailing2 = CreateSailing("AUSYD", "CNCAN", today.AddDays(4));
			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "CNCAN";
			shipment.JS_E_DEP = today.AddDays(1);

			CombineAssertions(delegate
			{
				AssertEquals("Earlier sailing should be selected from the list.", sailing1, shipment.Sailing);
				AssertContainsExactElementsInAnyOrder("Both sailings should be in the list.",
					new JobSailing[] { sailing1, sailing2 },
					shipment.RetrieveRelatedSailings());
			});

			shipment.JS_E_DEP = today.AddDays(3);

			CombineAssertions(delegate
			{
				AssertEquals("Sailing should be selected from the list.", sailing2, shipment.Sailing);
				AssertContainsExactElementsInAnyOrder("only the second sailing should be in the list.",
					new JobSailing[] { sailing2 },
					shipment.RetrieveRelatedSailings());
			});

			shipment.JS_E_DEP = today.AddDays(1);

			CombineAssertions(delegate
			{
				AssertEquals("Sailing should not be updated.", sailing2, shipment.Sailing);
				AssertContainsExactElementsInAnyOrder("Both sailings should be in the list.",
					new JobSailing[] { sailing1, sailing2 },
					shipment.RetrieveRelatedSailings());
			});

			shipment.JS_E_DEP = ZDateTime.Empty;

			CombineAssertions(delegate
			{
				AssertEquals("Sailing should not be updated.", sailing2, shipment.Sailing);
				AssertContainsExactElementsInAnyOrder("Both sailings should be in the list.",
					new JobSailing[] { sailing1, sailing2 },
					shipment.RetrieveRelatedSailings());
			});

			shipment.JS_E_DEP = ZDateTime.Invalid;

			CombineAssertions(delegate
			{
				AssertEquals("Sailing should not be updated.", sailing2, shipment.Sailing);
				AssertContainsExactElementsInAnyOrder("Both sailings should be in the list.",
					new JobSailing[] { sailing1, sailing2 },
					shipment.RetrieveRelatedSailings());
			});
		}

		public void TestTemplateCopyShouldNotChangeTheSailing()
		{
			JobSailing thirdSailing = CreateSailing(HomePort, OverseasPort, ZDateTime.Today.AddDays(3));
			JobSailing firstSailing = CreateSailing(HomePort, OverseasPort, ZDateTime.Today.AddDays(1));
			JobSailing secondSailing = CreateSailing(HomePort, OverseasPort, ZDateTime.Today.AddDays(2));
			Factory.Save();

			AgencyShipment copy;
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_NKLoadPort = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_NKDischargePort = OverseasPort;
			AssertEquals("precondition: should have auto-selected firstSailing", firstSailing.PK, shipment.JS_JX);

			shipment.JS_JX = secondSailing.PK;
			copy = (AgencyShipment)shipment.TemplateCopy();
			AssertEquals("secondSailing is still valid, dont change it.", secondSailing.PK, copy.JS_JX);

			shipment.JS_A_BKD = ZDateTime.Today.AddDays(3);
			shipment.JS_JX = secondSailing.PK;
			copy = (AgencyShipment)shipment.TemplateCopy();
			AssertEquals("secondSailing is no longer valid, still dont change it.", secondSailing.PK, copy.JS_JX);
		}

		[ExpectNoExceptions]
		public void TestInvalidShortDates()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			shipment.JS_NKLoadPort = HomePort;
			shipment.JS_NKDischargePort = OverseasPort;

			shipment.JS_E_DEP = ZDateTime.Invalid;
			shipment.JS_E_DEP = ZDateTime.MinSmallDateTimeValue.AddDays(-1);
			shipment.JS_E_DEP = ZDateTime.MaxSmallDateTimeValue.AddDays(1);
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(3);
			shipment.JS_E_DEP = ZDateTime.Empty;

			shipment.JS_A_BKD = ZDateTime.Invalid;
			shipment.JS_A_BKD = ZDateTime.MinSmallDateTimeValue.AddDays(-1);
			shipment.JS_A_BKD = ZDateTime.MaxSmallDateTimeValue.AddDays(1);
			shipment.JS_A_BKD = ZDateTime.Today.AddDays(3);
			shipment.JS_A_BKD = ZDateTime.Empty;
			CargoWise.Common.ErrorReporter.Clear();
		}

		public void TestHasHazardous()
		{
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment1.BookedContainers.AddNew();
			PackLine packLine = shipment2.OuterPackLines.AddNew();

			container.JC_RH_NKContainerCommodityCode = "GEN";
			packLine.JL_RH_NKCommodityCode = "GEN";

			Assert(!shipment1.HasHazardous);
			Assert(!shipment2.HasHazardous);

			container.JC_RH_NKContainerCommodityCode = "HAZ";
			packLine.JL_RH_NKCommodityCode = "MTHZ";

			Assert(shipment1.HasHazardous);
			Assert(shipment2.HasHazardous);

			container.JC_RH_NKContainerCommodityCode = "GEN";
			packLine.JL_RH_NKCommodityCode = "GEN";

			Assert(!shipment1.HasHazardous);
			Assert(!shipment2.HasHazardous);

			container.UNDGs.AddNew();

			Assert(shipment1.HasHazardous);
			Assert(!shipment2.HasHazardous);
		}

		public void TestHazardousStatus()
		{
			AgencyShipmentContainer hazContainer = Factory.New<AgencyShipmentContainer>();
			hazContainer.JC_RH_NKContainerCommodityCode = "HAZ";
			AgencyShipmentContainer nonContainer = Factory.New<AgencyShipmentContainer>();
			nonContainer.JC_RH_NKContainerCommodityCode = "GEN";

			AgencyShipmentPackLine hazPackLine = Factory.New<AgencyShipmentPackLine>();
			hazPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			hazPackLine.JL_RH_NKCommodityCode = "MTHZ";
			AgencyShipmentPackLine nonPackLine = Factory.New<AgencyShipmentPackLine>();
			nonPackLine.JL_FreightMode = FreightConstants.OuterPackType;
			nonPackLine.JL_RH_NKCommodityCode = "GEN";

			AgencyShipment shipment = Factory.New<AgencyShipment>();

			//	0	0	0	0	N
			AssertEquals("HazardousStatus should be 'NON'", "NON", shipment.HazardousStatus);

			//	0	0	0	1	N
			shipment.OuterPackLines.Add(nonPackLine);
			AssertEquals("HazardousStatus should be 'NON'", "NON", shipment.HazardousStatus);

			//	0	0	1	1	M
			shipment.OuterPackLines.Add(hazPackLine);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	0	0	1	0	H
			shipment.OuterPackLines.Remove(nonPackLine);
			AssertEquals("HazardousStatus should be 'HAZ'", "HAZ", shipment.HazardousStatus);

			//	0	1	1	0	M
			shipment.BookedContainers.Add(nonContainer);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	0	1	0	0	N
			shipment.OuterPackLines.Remove(hazPackLine);
			AssertEquals("HazardousStatus should be 'NON'", "NON", shipment.HazardousStatus);

			//	0	1	0	1	N
			shipment.OuterPackLines.Add(nonPackLine);
			AssertEquals("HazardousStatus should be 'NON'", "NON", shipment.HazardousStatus);

			//	0	1	1	1	M
			shipment.OuterPackLines.Add(hazPackLine);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	1	1	1	M
			shipment.BookedContainers.Add(hazContainer);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	0	1	1	M
			shipment.BookedContainers.Remove(nonContainer);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	0	0	1	M
			shipment.OuterPackLines.Remove(hazPackLine);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	1	0	1	M
			shipment.BookedContainers.Add(nonContainer);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	1	0	0	M
			shipment.OuterPackLines.Remove(nonPackLine);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	1	1	0	M
			shipment.OuterPackLines.Add(hazPackLine);
			AssertEquals("HazardousStatus should be 'MIX'", "MIX", shipment.HazardousStatus);

			//	1	0	1	0	H
			shipment.BookedContainers.Remove(nonContainer);
			AssertEquals("HazardousStatus should be 'HAZ'", "HAZ", shipment.HazardousStatus);

			//	1	0	0	0	H
			shipment.OuterPackLines.Remove(hazPackLine);
			AssertEquals("HazardousStatus should be 'HAZ'", "HAZ", shipment.HazardousStatus);
		}

		public void TestDocManagerCode()
		{
			CombineAssertions(delegate
			{
				AgencyShipment shipment = Factory.New<AgencyShipment>();
				AssertType(typeof(AgencyShipmentDocManagerInfo), shipment.DocManagerInfo);
				AssertEquals("Code should be ASH. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "ASH", ((IDocManagerSupport)shipment).DocManagerInfo.DocManagerCode);
			});
		}

		#region CheckTotalsDiffer

		public void TestCheckTotalsDiffer()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ActualVolume = 1m;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.AddNew().JL_ActualVolume = 2m;

			bool eventRaised = false;
			shipment.UpdateShipmentTotalsPackQuantityVariation += (s, e) => { eventRaised = true; };

			shipment.CheckTotalsDiffer();
			AssertEquals("Event was raised", true, eventRaised);
			AssertEquals("Shipment was updated", 2m, shipment.JS_ActualVolume);

			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = mode;
				shipment.JS_OuterPacks = 1;
				shipment.JS_ActualVolume = 1m;

				shipment.TopLevelPacks[0].JC_GrossVolume = 1m;

				eventRaised = false;
				shipment.UpdateShipmentTotalsPackQuantityVariation += (s, e) => { eventRaised = true; };
				shipment.CheckTotalsDiffer();

				AssertEquals("Event was not raised: pack totals was in sync", false, eventRaised);
				AssertEquals("Shipment was not updated", 1m, shipment.JS_ActualVolume);

				shipment.TopLevelPacks.RemoveAndDeleteAll();

				shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
				shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

				var topLevelPack1 = shipment.TopLevelPacks.AddNew();
				topLevelPack1.JC_ContainerCount = 1;
				topLevelPack1.JC_GrossVolume = 1.1m;
				topLevelPack1.JC_GrossWeight = 100m;

				var topLevelPack2 = shipment.TopLevelPacks.AddNew();
				topLevelPack2.JC_ContainerCount = 2;
				topLevelPack2.JC_GrossVolume = 20m;
				topLevelPack2.JC_GrossVolumeUQ = Constants.Volume.CubicFeet;
				topLevelPack2.JC_GrossWeight = 200m;
				topLevelPack2.JC_GrossWeightUQ = Constants.Weight.Pounds;

				eventRaised = false;
				shipment.CheckTotalsDiffer();

				AssertEquals("Event was raised: pack totals was out of sync", true, eventRaised);
				AssertEquals("Shipment was updated from packs", 3, shipment.JS_OuterPacks);
				AssertEquals("Shipment was updated from packs, with unit's conversion", 1.666m, shipment.JS_ActualVolume);
				AssertEquals("Shipment was updated from packs, with unit's conversion", 190.718m, shipment.JS_ActualWeight);
			}
		}

		#endregion

		#region JS_PackingMode

		public void TestIsRollOnRollOff()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals(false, shipment.IsRollOnRollOff);

			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(false, shipment.IsRollOnRollOff);

			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(true, shipment.IsRollOnRollOff);
		}

		public void TestIsTopLevelPacksMode()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = Constants.ContainerModes.FCL;
				AssertEquals(false, shipment.IsTopLevelPacksMode);

				shipment.JS_PackingMode = "XXX";
				AssertEquals(false, shipment.IsTopLevelPacksMode);

				shipment.JS_PackingMode = mode;
				AssertEquals(true, shipment.IsTopLevelPacksMode);
			}
		}

		public void TestJS_PackingMode_Changing_WillRaiseEventIfNecessary()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			shipment.RealContainers.AddNew();

			bool eventShouldBeRaised = true;
			bool shouldCancel = true;
			string messageTemplate = @"Changing cargo type will remove all data from:
{0}

Are you sure you want to continue?
";
			string expectedMessageInner = "Containers";

			shipment.PackingModeChanging += (s, e) =>
			{
				if (!eventShouldBeRaised)
				{
					Fail("Event should not be raised.");
				}

				AssertEquals("Caption", "Confirm Cargo Type Change", e.Caption);
				AssertEquals("Message", string.Format(messageTemplate, expectedMessageInner), e.Message);
				e.Cancel = shouldCancel;
			};

			// From FCL
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			AssertEquals("User cancelled mode change", Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			AssertEquals(true, shipment.RealContainers.Any());

			shouldCancel = false;
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			AssertEquals("User accepted mode change", Constants.ContainerModes.BreakBulk, shipment.JS_PackingMode);
			AssertEquals(false, shipment.RealContainers.Any());

			// To ROR
			shipment.OuterPackLines.AddNew();

			expectedMessageInner = "Packs";
			shouldCancel = true;
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			AssertEquals("User cancelled mode change", Constants.ContainerModes.BreakBulk, shipment.JS_PackingMode);
			AssertEquals(true, shipment.OuterPackLines.Any());

			shouldCancel = false;
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			AssertEquals("User accepted mode change", Constants.ContainerModes.RollOnRollOff, shipment.JS_PackingMode);
			AssertEquals(false, shipment.OuterPackLines.Any());

			// From ROR

			// Event should not be raised when there is no vehicles
			eventShouldBeRaised = false;
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			shipment.BookedContainers.AddNew();

			eventShouldBeRaised = true;
			expectedMessageInner = "Vehicles";
			shouldCancel = true;
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			AssertEquals("User cancelled mode change", Constants.ContainerModes.RollOnRollOff, shipment.JS_PackingMode);
			AssertEquals(true, shipment.ShippingContainers.Any());

			shouldCancel = false;
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			AssertEquals("User accepted mode change", Constants.ContainerModes.BreakBulk, shipment.JS_PackingMode);
			AssertEquals(false, shipment.Sailings.Any());
		}

		public void TestJS_PackingMode_ChangingFromFCL_ClearsContainers()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();

			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			AssertEquals("Expecting fcl containers to have been cleared.", 0, shipment.BookedContainers.Count);
			AssertEquals("Expecting fcl containers to have been cleared.", 0, shipment.RealContainers.Count);
			AssertEquals("Containers have been deleted", true, new[] { container1, container2 }.All(container => container.IsDeleted));
		}

		public void TestJS_PackingMode_ChangingToFCL_ClearsPacklines()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = "XXX";

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Packlines have been deleted", true, new[] { packline1, packline2 }.All(packline => packline.IsDeleted));
		}

		public void TestJS_PackingMode_ChangingToNonFCL_ClearsPacklines()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AgencyShipment shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = Constants.ContainerModes.FCL;

				var packline1 = shipment.OuterPackLines.AddNew();
				var packline2 = shipment.OuterPackLines.AddNew();

				shipment.JS_PackingMode = mode;
				AssertEquals("Packlines have been deleted", true, new[] { packline1, packline2 }.All(packline => packline.IsDeleted));
			}
		}

		public void TestSettingGoodsDescriptionDoesNotCreatePackLinesForNonFCL()
		{
			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = mode;

				shipment.JS_GoodsDescription = "camel toes";
				AssertEquals(0, shipment.OuterPackLines.Count);

				shipment.JS_PackingMode = Constants.ContainerModes.FCL;

				shipment.JS_GoodsDescription = "bear ears";
				AssertEquals(1, shipment.OuterPackLines.Count);
			}
		}

		public void TestSettingOuterPacksDoesNotCreatePackLinesForModesWherePacklinesAreNotUsed()
		{
			var shipment = Factory.New<AgencyShipment>();

			int outerPacks = 10;
			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				shipment.JS_PackingMode = mode;
				shipment.OuterPackLines.RemoveAndDeleteAll();
				shipment.JS_OuterPacks = outerPacks++;

				AssertEquals("Packlines are not used for TopLevelPacks", false, shipment.OuterPackLines.Any());
			}

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.JS_OuterPacks = outerPacks++;

			AssertEquals("Default packline created for FCL", true, shipment.OuterPackLines.Any());
		}

		#endregion

		public void TestJS_OuterPacks_TopLevelPacksMode_WillDefaultPackageCountOnDefaultTopLevelPack()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = mode;
				shipment.JS_OuterPacks = 10;

				AssertEquals((short)10, shipment.TopLevelPacks[0].JC_ContainerCount);
			}
		}

		public void TestJS_F3_NKPackType_TopLevelPacksMode_WillDefaultPackageTypeOnDefaultTopLevelPack()
		{
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = mode;
				shipment.JS_F3_NKPackType = "BOX";

				AssertEquals("BOX", shipment.TopLevelPacks[0].JC_F3_NKPackType);
			}
		}

		public void TestStatusValidation()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = "XYZ";
			AssertHasErrors("XYZ is an invalid code", shipment.JS_ShipmentStatusInfo);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertNoNotifications(ShipmentStatusList.Codes.Booked + " is a valid code", shipment.JS_ShipmentStatusInfo);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertHasErrors(ShipmentStatusList.Codes.Confirmed + " is only valid if the shipment has been confirmed", shipment.JS_ShipmentStatusInfo);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertNoNotifications(ShipmentStatusList.Codes.WaitListed + " is a valid code", shipment.JS_ShipmentStatusInfo);
		}

		public void TestNoteTypesCore()
		{
			var booking = Factory.New<AgencyShipment>();
			AssertCollectionContains(PredefinedNoteTypes.Instance.ContainerReleaseNote, booking.NoteTypes);
			AssertCollectionContains(PredefinedNoteTypes.Instance.ForwardingInstructionNotes, booking.NoteTypes);
			AssertCollectionContains(PredefinedNoteTypes.Instance.WebUserNote, booking.NoteTypes);
		}

		public void TestLastSavedUsage()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = RC_20GP_PK;

			AssertEquals("The shipment has not yet been saved", true, shipment.LastSavedUsage.IsEmpty);

			container.JC_ContainerCount = 5;
			AssertEquals("The shipment still has not been saved", true, shipment.LastSavedUsage.IsEmpty);

			Factory.Save();
			AssertEquals("The Shipment has been saved", false, shipment.LastSavedUsage.IsEmpty);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AgencyShipment shipment2 = factory2.Load<AgencyShipment>(shipment.PK);
			shipment2.BookedContainers[0].JC_RC = RC_20RE_PK;

			AssertEquals("The Shipment has just been loaded in a fresh factory", false, shipment2.LastSavedUsage.IsEmpty);
			AssertEquals("The container in the db is still using GP TEU's", 5m, shipment2.LastSavedUsage.GP_TEU);
			AssertEquals("The change to reefef has not yet been saved", 0m, shipment2.LastSavedUsage.Reefer_TEU);
		}

		public void TestLastSavedUsage_WasWaitListed()
		{
			AgencyShipment shipment = NewBulkShipment(ExportSailing, null, false, false, 50, 50);
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AgencyShipment shipment2 = newFactory.Load<AgencyShipment>(shipment.PK);
			AssertEquals("Was not confirmed, should have an empty allocation", true, shipment2.LastSavedUsage.IsEmpty);
		}

		public void TestLastSavedSailing()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			Factory.Save();

			AssertEquals("new shipments have no sailing", ZGuid.Empty, shipment.JS_JX_LastSavedSailing);

			shipment.JS_JX = ExportSailing.PK;
			AssertEquals("setting the current sailing should not affect the last saved sailing.", ZGuid.Empty, shipment.JS_JX_LastSavedSailing);

			Factory.Save();
			AssertEquals("Saving the shipment should update the last saved sailing", ExportSailing.PK, shipment.JS_JX_LastSavedSailing);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AgencyShipment shipment2 = factory2.Load<AgencyShipment>(shipment.PK);

			AssertEquals("Loading the shipment should set the last saved sailing.", ExportSailing.PK, shipment.JS_JX_LastSavedSailing);
		}

		public void TestShouldEnforceAllocations()
		{
			FreightConfigurationRegistry.Instance.UseGlobalAllocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertEquals("no sailing", false, shipment.ShouldEnforceAllocations);

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKC";

			JobSailing sailing = voyage.Sailings[0];
			shipment.JS_JX = sailing.PK;

			sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Ignore;
			AssertEquals("Ignore Allocations", false, shipment.ShouldEnforceAllocations);

			sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			AssertEquals("Sailing Allocations", false, shipment.ShouldEnforceAllocations);

			Factory.Save();
			AssertEquals("Sailing Allocations", true, shipment.ShouldEnforceAllocations);

			origin.JA_RL_NKPortOfLoading = "SGSIN";
			destination.JB_RL_NKPortOfDischarge = "AUBNE";

			FreightConfigurationRegistry.Instance.UseGlobalAllocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Import", true, shipment.ShouldEnforceAllocations);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertEquals("Waitlisted", false, shipment.ShouldEnforceAllocations);
		}

		public void TestDefaultReleaseType()
		{
			AssertEquals(ZString.Empty, AgencyRegistry.Instance.ReleaseTypeDefault.Value);
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertEquals(ZString.Empty, shipment.JS_ReleaseType);

			AgencyRegistry.Instance.ReleaseTypeDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SWB");
			shipment = Factory.New<AgencyShipment>();
			AssertEquals("SWB", shipment.JS_ReleaseType);
		}

		public void TestRegistryServiceLevel()
		{
			Env.Registry.ServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "D2D").PK.ToGuid();
			AgencyRegistry.Instance.ServiceLevelDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").PK.ToGuid());

			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AssertEquals("Agency Service Level is taken from Agency Registry Item", "DEF", shipment.RegistryServiceLevel.RS_Code);
			AssertEquals("Service level is defaulted", "DEF", shipment.JS_RS_NKServiceLevel);
		}

		public void TestGetAddressBookSelection()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			OrgHeader consignor = Factory.New<OrgHeader>();
			OrgContact consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "consignor";
			consignor.OH_IsConsignor = true;

			OrgHeader consignee = Factory.New<OrgHeader>();
			OrgContact consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "consignee";
			consignor.OH_IsConsignee = true;

			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgContact carrierContact = carrier.Contacts.AddNew();
			carrierContact.OC_ContactName = "carrier";
			carrier.OH_IsShippingProvider = true;

			OrgHeader principal = Factory.New<OrgHeader>();
			OrgContact principalContact = principal.Contacts.AddNew();
			principalContact.OC_ContactName = "principal";
			principal.OH_IsShippingLine = true;

			OrgHeader bookingParty = Factory.New<OrgHeader>();
			OrgContact bookingPartyContact = bookingParty.Contacts.AddNew();
			bookingPartyContact.OC_ContactName = "bookingParty";

			AddressBookSelection selection = ((ISendEmailSource)shipment).GetAddressBookSelection();
			AssertEquals(0, selection.Recipients.Count);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;

			selection = ((ISendEmailSource)shipment).GetAddressBookSelection();
			AssertEquals(5, selection.Recipients.Count);
			AssertEquals(consignorContact.OC_ContactName, selection.Recipients[0].Name);
			AssertEquals(consigneeContact.OC_ContactName, selection.Recipients[1].Name);
			AssertEquals(principalContact.OC_ContactName, selection.Recipients[2].Name);
			AssertEquals(carrierContact.OC_ContactName, selection.Recipients[3].Name);
			AssertEquals(bookingPartyContact.OC_ContactName, selection.Recipients[4].Name);
		}

		public void TestEmailSubject()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "V00001";
			String emailSubject = ((ISendEmailSource)shipment).EmailSubject;
			AssertEquals("Agency Shipment - V00001", emailSubject);
		}

		public void TestReleaseType()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AssertMultilineASCIIEquals("",
				AgencyRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList().ElementsAsString,
				shipment.Lookups.JS_ReleaseType_List.ElementsAsString);
		}

		public void TestBillOfLadingCustomisation()
		{
			BillOfLadingNumberCustomisation shipmentNumberCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(shipmentNumberCustomisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "SHP", true);

			BillOfLadingNumberCustomisation billOfLadingCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(billOfLadingCustomisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "BOL", true);

			AgencyRegistry.Instance.OceanBillNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingCustomisation);
			AgencyRegistry.Instance.OceanBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shipmentNumberCustomisation);

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;

			Factory.Save();

			AssertEquals("Should get the ShipmentNumberCustomisation object from the 'OceanBillShipmentNumberCustomisation' registry item", "VSHP00000001", shipment.JS_UniqueConsignRef);
			AssertEquals("Should get the BillOfLadingCustomisation object from the 'OceanBillNumberCustomisation' registry item", "VBOL00000001", shipment.JS_HouseBill);
		}

		public void TestNeedsHouseBill()
		{
			AgencyShipment shipment = NewShipment(ImportSailing, null, false, true);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			shipment.Factory.Save();

			AssertEquals("Should have not set JS_HouseBill", ZString.Empty, shipment.JS_HouseBill);
			AssertEquals("Should have not set JS_CFSReference", ZString.Empty, shipment.JS_CFSReference);

			shipment = NewShipment(ImportSailing, null, false, true);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			shipment.Factory.Save();

			AssertEquals("Should have set JS_HouseBill", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill);

			shipment = NewShipment(ImportSailing, null, false, true);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			shipment.Factory.Save();

			AssertEquals("Should have set JS_HouseBill", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill);
			AssertEquals("Should have set JS_CFSReference", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference);

			shipment = NewShipment(ImportSailing, null, false, true);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.AlwaysGenerateBookingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			shipment.Factory.Save();

			AssertEquals("Should have set JS_HouseBill", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill);
			AssertEquals("Should have set JS_CFSReference", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference);

			shipment = NewShipment(ExportSailing, null, false, false);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			AgencyRegistry.Instance.AlwaysGenerateBookingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			shipment.Factory.Save();

			AssertEquals("Should have set JS_CFSReference", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference);
			AssertEquals("Should have set JS_HouseBill", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill);

			shipment = NewShipment(ExportSailing, null, false, false);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.AlwaysGenerateBookingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			shipment.Factory.Save();

			AssertEquals("Should have set JS_CFSReference", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference);

			shipment = NewShipment(ImportSailing, null, false, true);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			AgencyRegistry.Instance.AlwaysGenerateBookingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AgencyRegistry.Instance.AlwaysGenerateBillOfLadingNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			shipment.Factory.Save();

			AssertEquals("Should have not set JS_HouseBill", ZString.Empty, shipment.JS_HouseBill);
			AssertEquals("Should have not set JS_CFSReference", ZString.Empty, shipment.JS_CFSReference);
		}

		public void TestContainersSplitOnConfirm()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			container1.JC_RC = RC_20GP_PK;
			container1.JC_ContainerCount = 1;
			container1.JC_TareWeight = 2400;

			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			container2.JC_RC = RC_20RE_PK;
			container2.JC_ContainerCount = 3;
			container2.JC_TareWeight = 7500;

			AgencyShipmentPackLine packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = container1.PK;
			packline1.JL_ActualWeight = 20000;

			AgencyShipmentPackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_JC = container2.PK;
			packline2.JL_ActualWeight = 54000;

			AgencyShipmentPackLine packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_JC = ZGuid.Empty;
			packline3.JL_ActualWeight = 10000;

			shipment.Confirm();

			int count1 = 0;
			int count2 = 0;
			ZGuid gpContainer = ZGuid.Empty;

			foreach (AgencyShipmentContainer container in shipment.RealContainers)
			{
				AssertEquals("JC_ContainerCount", 1, (int)container.JC_ContainerCount);

				if (container.JC_RC == RC_20GP_PK)
				{
					AssertEquals("JC_ContainerCount", (short)1, container.JC_ContainerCount);
					AssertEquals("JC_Calc_NetWeight", 20000m, container.JC_Calc_NetWeight);
					AssertEquals("JC_TareWeight", 2400m, container.JC_TareWeight);
					count1++;
					gpContainer = container.PK;
				}
				else if (container.JC_RC == RC_20RE_PK)
				{
					AssertEquals("JC_ContainerCount", (short)1, container.JC_ContainerCount);

					AssertEquals("JC_Calc_NetWeight", 18000m, container.JC_Calc_NetWeight);
					AssertEquals("JC_TareWeight", 2500m, container.JC_TareWeight);
					count2++;
				}
				else
				{
					Fail("unexpected container type");
				}
			}

			AssertEquals("should be 1 20GP container", 1, count1);
			AssertEquals("should be 3 20RE containers", 3, count2);

			AssertEquals("packline1 was packed into a container with count 1 and so can remain packed.", gpContainer, packline1.JL_JC);
			AssertEquals("packline2 was packed into a container with count 4 and so must be unpacked.", ZGuid.Empty, packline2.JL_JC);
			AssertEquals("packline3 was not packed", ZGuid.Empty, packline3.JL_JC);
		}

		public void TestContainersSplitAndReuseOnConfirm()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer bContainer1 = shipment.BookedContainers.AddNew();
			bContainer1.JC_ContainerNum = "TEST4100013";
			bContainer1.JC_RC = RC_20GP_PK;
			bContainer1.JC_ContainerCount = 1;
			bContainer1.JC_TareWeight = 2400;
			bContainer1.JC_Calc_NetWeight = 1000;

			AgencyShipmentContainer bContainer2 = shipment.BookedContainers.AddNew();
			bContainer2.JC_ContainerNum = "";
			bContainer2.JC_RC = RC_20GP_PK;
			bContainer2.JC_ContainerCount = 2;
			bContainer2.JC_TareWeight = 4800;
			bContainer2.JC_Calc_NetWeight = 4000;

			AgencyShipmentContainer bContainer3 = shipment.BookedContainers.AddNew();
			bContainer3.JC_ContainerNum = "";
			bContainer3.JC_RC = RC_40GP_PK;
			bContainer3.JC_ContainerCount = 3;
			bContainer3.JC_TareWeight = 9600;
			bContainer3.JC_Calc_NetWeight = 6000;

			AgencyShipmentContainer bContainer4 = shipment.RealContainers.AddNew();
			bContainer4.JC_ContainerNum = "TEST4100029";
			bContainer4.JC_RC = RC_40RE_PK;
			bContainer4.JC_ContainerCount = 1;
			bContainer4.JC_TareWeight = 3500;
			bContainer4.JC_Calc_NetWeight = 1000;

			AgencyShipmentContainer rContainer1 = shipment.RealContainers.AddNew();
			rContainer1.JC_ContainerNum = "TEST4100013";
			rContainer1.JC_RC = RC_20GP_PK;
			rContainer1.JC_TareWeight = 2400;
			rContainer1.JC_Calc_NetWeight = 1500;
			rContainer1.JC_SealNum = "R1A";
			rContainer1.JC_AdditionalSealNum = "R1B";
			rContainer1.JC_Additional2SealNum = "R1C";

			AgencyShipmentContainer rContainer2 = shipment.RealContainers.AddNew();
			rContainer2.JC_ContainerNum = "TEST4100034";
			rContainer2.JC_RC = RC_20GP_PK;
			rContainer2.JC_TareWeight = 2400;
			rContainer2.JC_Calc_NetWeight = 1000;
			rContainer2.JC_SealNum = "R2A";
			rContainer2.JC_AdditionalSealNum = "R2B";
			rContainer2.JC_Additional2SealNum = "R2C";

			AgencyShipmentContainer rContainer3 = shipment.RealContainers.AddNew();
			rContainer3.JC_ContainerNum = "TEST4100050";
			rContainer3.JC_RC = RC_20GP_PK;
			rContainer3.JC_TareWeight = 2400;
			rContainer3.JC_IsShipperOwned = true;
			rContainer3.JC_Calc_NetWeight = 3500;
			rContainer3.JC_SealNum = "R3A";
			rContainer3.JC_AdditionalSealNum = "R3B";
			rContainer3.JC_Additional2SealNum = "R3C";

			shipment.Confirm();

			Converter<AgencyShipmentContainer, string> converter = delegate(AgencyShipmentContainer container)
			{
				return string.Format("{0}\r\n{1} - {2}\r\n{3:0.000} KG\r\n{4:0.000} KG\r\n{5} - {6} - {7}",
					container.JC_ContainerNum,
					container.Container == null ? "<null>" : container.Container.RC_Code.ToString(),
					container.JC_IsShipperOwned,
					container.JC_TareWeight,
					container.JC_Calc_NetWeight,
					container.JC_SealNum,
					container.JC_AdditionalSealNum,
					container.JC_Additional2SealNum);
			};

			AssertContainsExactElementsInAnyOrder(
				"Real Containers",
				new string[]
				{
					"TEST4100013\r\n20GP - N\r\n2400.000 KG\r\n1500.000 KG\r\nR1A - R1B - R1C",
					"TEST4100029\r\n40RE - N\r\n3500.000 KG\r\n1000.000 KG\r\n -  - ",
					"TEST4100034\r\n20GP - N\r\n2400.000 KG\r\n1000.000 KG\r\nR2A - R2B - R2C",
					"TEST4100050\r\n20GP - Y\r\n2400.000 KG\r\n3500.000 KG\r\nR3A - R3B - R3C",
					"\r\n20GP - N\r\n2400.000 KG\r\n3000.000 KG\r\n -  - ",
					"\r\n40GP - N\r\n3200.000 KG\r\n2000.000 KG\r\n -  - ",
					"\r\n40GP - N\r\n3200.000 KG\r\n2000.000 KG\r\n -  - ",
					"\r\n40GP - N\r\n3200.000 KG\r\n2000.000 KG\r\n -  - ",
				},
				Array.ConvertAll(shipment.RealContainers.ToArray<AgencyShipmentContainer>(), converter));
		}

		public void TestContainersIsControlledAndIsFreezerOnConfirm()
		{
			var shipment = Factory.New<AgencyBooking>();

			var container = shipment.BookedContainers.AddNew();
			container.JC_RC = RC_20RE_PK;
			container.JC_ContainerCount = 2;
			container.JC_TareWeight = 2.4;
			container.JC_GrossWeight = 23;

			container.JC_ContainerNum = string.Empty;
			container.JC_IsControlledAtmosphere = false;
			container.IsFreezer = false;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;

			shipment.Confirm();

			AssertEquals("real container count", 2, shipment.RealContainers.Count);

			var realContainer = shipment.RealContainers[0];

			AssertEquals("real container IsControlled.", false, realContainer.JC_IsControlledAtmosphere);
			AssertEquals("real container IsFreezer", false, realContainer.IsFreezer);

			realContainer = shipment.RealContainers[1];

			AssertEquals("real container IsControlled.", false, realContainer.JC_IsControlledAtmosphere);
			AssertEquals("real container IsFreezer", false, realContainer.IsFreezer);
		}

		public void TestConfirm_CopySealNumberAndVGMFromBookedContainer()
		{
			var shipment = Factory.New<AgencyShipment>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var bContainer1 = shipment.BookedContainers.AddNew();
			bContainer1.JC_ContainerNum = "TEST0000001";
			bContainer1.JC_RC = RC_20GP_PK;
			bContainer1.JC_ContainerCount = 1;
			bContainer1.JC_TareWeight = 2400;
			bContainer1.JC_Calc_NetWeight = 1000;
			bContainer1.JC_SealNum = "0001";
			bContainer1.GrossWeightVerifiedByAddress.OrganisationPK = org1.PK;

			var bContainer2 = shipment.BookedContainers.AddNew();
			bContainer2.JC_ContainerNum = "";
			bContainer2.JC_RC = RC_20GP_PK;
			bContainer2.JC_ContainerCount = 1;
			bContainer2.JC_TareWeight = 4800;
			bContainer2.JC_Calc_NetWeight = 4000;
			bContainer2.JC_SealNum = "0002";
			bContainer2.GrossWeightVerifiedByAddress.OrganisationPK = org2.PK;

			var rContainer = shipment.RealContainers.AddNew();
			rContainer.JC_ContainerNum = "TEST0000001";
			rContainer.JC_RC = RC_20GP_PK;
			rContainer.JC_TareWeight = 2400;
			rContainer.JC_Calc_NetWeight = 1500;

			shipment.Confirm();

			AssertEquals("real container count", 2, shipment.RealContainers.Count);

			shipment.Confirm();

			AssertEquals("real container count", 2, shipment.RealContainers.Count);
			AssertEquals("JC_SealNum is excluded when copying booked container into existing real container.", string.Empty, shipment.RealContainers[0].JC_SealNum);
			AssertEquals("GrossWeightVerifiedByAddress is excluded when copying booked container into existing real container.", ZGuid.Empty, shipment.RealContainers[0].GrossWeightVerifiedByAddress.OrganisationPK);

			AssertEquals("JC_SealNum is copied when cloning booked container into existing real container.", "0002", shipment.RealContainers[1].JC_SealNum);
			AssertEquals("GrossWeightVerifiedByAddress is copied when cloning booked container into existing real container.", org2.PK, shipment.RealContainers[1].GrossWeightVerifiedByAddress.OrganisationPK);
		}

		public void TestTemplateCopyShouldClearTheseFields()
		{
			OrgHeader consignor = NewConsignor();
			OrgHeader consignee = NewConsignee();
			OrgHeader notifyParty = NewConsignee();

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_TotalPackageCount = 50;
			shipment.JS_ActualWeight = 60;
			shipment.JS_ActualVolume = 70;
			shipment.OuterPackLines.AddNew().JL_Description = "Line Description";
			shipment.DetailedGoodsDescriptionNoteText = "Long Description";
			shipment.CustomsEntryNumberType = "CCN";
			shipment.CustomsEntryNumber = "12345";

			shipment.JS_UniqueConsignRef = "UCRef";
			shipment.JS_BookingReference = "BKRef";
			shipment.JS_CFSReference = "CFSRef";

			AgencyShipment clone = (AgencyShipment)shipment.TemplateCopy();
			AssertEquals("JS_TotalPackageCount should be cleared on the clone", 0, clone.JS_TotalPackageCount);
			AssertEquals("JS_ActualWeight should be cleared on the clone", 0m, clone.JS_ActualWeight);
			AssertEquals("JS_ActualVolume should be cleared on the clone", 0m, clone.JS_ActualVolume);

			AssertEquals("JS_UniqueConsignRef should be cleared on the clone", "", clone.JS_UniqueConsignRef);
			AssertEquals("JS_BookingReference should be cleared on the clone", "", clone.JS_BookingReference);
			AssertEquals("JS_CFSReference should be cleared on the clone", "", clone.JS_CFSReference);
			AssertEquals("DetailedGoodsDescriptionNoteText should be cleared", "", clone.DetailedGoodsDescriptionNoteText);
			AssertEquals("CustomsEntryNumberType should be cleared", "", clone.CustomsEntryNumberType);
			AssertEquals("CustomsEntryNumber should be cleared", "", clone.CustomsEntryNumber);

			AssertEquals("Packlines should not be cloned", 0, clone.OuterPackLines.Count);
		}

		public void TestSupportBookingParty()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			IDocAddresses addresses = shipment;

			AssertCollectionContains("should support booking party", DocAddressType.BookingPartyDocumentaryAddress, addresses.SupportedAddressTypes);
			AssertEquals(shipment.BookingPartyDocumentaryAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress).DefaultDocAddressType);
		}

		public void TestSetDefaultValues()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertEquals("JS_PackingMode must default to FCL", Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			AssertEquals("JS_TransportMode must default to Sea", Constants.TransportModes.Sea, shipment.JS_TransportMode);
			AssertEquals("JS_ShipmentStatus must default to Booked", ShipmentStatusList.Codes.Booked, shipment.JS_ShipmentStatus);
			AssertEquals("JS_A_BKD must be set to today", ZDateTime.Today, shipment.JS_A_BKD);
			AssertEquals("JS_IsShipping must be set.", true, shipment.JS_IsShipping);
			AssertEquals("JS_IsBooking must not be set.", false, shipment.JS_IsBooking);
			AssertEquals("JS_IsForwardRegistered must not be set.", false, shipment.JS_IsForwardRegistered);
			AssertEquals("JS_IsCFSRegistered must not be set.", false, shipment.JS_IsCFSRegistered);

			ReleaseTypes releaseTypes = AgencyRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;
			AgencyRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			BillOfLading bol = Factory.New<BillOfLading>();
			AssertEquals("JS_NoOriginalBills must be set.", (byte)5, (byte)bol.JS_NoOriginalBills);
			AssertEquals("JS_NoCopyBills must be set", (byte)6, (byte)bol.JS_NoCopyBills);
		}

		public void TestSetJS_ReleaseType()
		{
			ReleaseTypes releaseTypes = AgencyRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;

			ReleaseType swbReleaseType = releaseTypes.Types.FindByCode("SWB");
			swbReleaseType.OriginalsNumber = 0;
			swbReleaseType.CopiesNumber = 2;

			AgencyRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			BillOfLading bol = Factory.New<BillOfLading>();
			AssertEquals("JS_NoOriginalBills must be set.", (byte)5, (byte)bol.JS_NoOriginalBills);
			AssertEquals("JS_NoCopyBills must be set", (byte)6, (byte)bol.JS_NoCopyBills);

			bol.JS_ReleaseType = "SWB";
			AssertEquals("JS_NoOriginalBills must be set.", (byte)0, (byte)bol.JS_NoOriginalBills);
			AssertEquals("JS_NoCopyBills must be set", (byte)2, (byte)bol.JS_NoCopyBills);
		}

		public void TestHumanReadableName()
		{
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			shipment1.JS_UniqueConsignRef = "V00001001";
			AssertEquals("Shipping Booking V00001001", shipment1.HumanReadableName);

			AgencyBooking shipment2 = Factory.New<AgencyBooking>();
			shipment2.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			shipment2.JS_UniqueConsignRef = "V00001002";
			AssertEquals("Shipping Booking V00001002", shipment2.HumanReadableName);

			AgencyBooking shipment3 = Factory.New<AgencyBooking>();
			shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment3.JS_UniqueConsignRef = "V00001003";
			AssertEquals("Shipping Booking V00001003", shipment3.HumanReadableName);

			BillOfLading shipment4 = Factory.New<BillOfLading>();
			shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;
			shipment4.JS_UniqueConsignRef = "V00001004";
			AssertEquals("Shipping Forwarding Instruction V00001004", shipment4.HumanReadableName);

			BillOfLading shipment5 = Factory.New<BillOfLading>();
			shipment5.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment5.JS_UniqueConsignRef = "V00001005";
			AssertEquals("Shipping Bill of Lading V00001005", shipment5.HumanReadableName);
		}

		public void TestPopulateBillOfLadingAndBookingRefOnSave_Import()
		{
			AgencyShipment importShipment = NewShipment(ImportSailing, null, false, true);
			importShipment.Factory.Save();

			AssertEquals("Should NOT have set JS_HouseBill", "", importShipment.JS_HouseBill);
			AssertEquals("Should NOT have set JS_CFSReference", "", importShipment.JS_CFSReference);
		}

		public void TestPopulateBillOfLadingAndBookingRefOnSave_Export()
		{
			AgencyShipment exportShipment = NewShipment(ExportSailing, null, false, true);
			exportShipment.Factory.Save();

			AssertEquals("Should have set JS_HouseBill", exportShipment.JS_UniqueConsignRef, exportShipment.JS_HouseBill);
			AssertEquals("Should have set JS_CFSReference", exportShipment.JS_UniqueConsignRef, exportShipment.JS_CFSReference);
		}

		public void TestPrincipalValidation()
		{
			OrgHeader principal = NewPrincipal();
			OrgHeader notPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			const string ErrorMessage1 = "Enter a valid Principal";
			const string ErrorMessage2 = "The selected Principal is no longer valid. Please choose a new Principal from the list.";
			const string ErrorMessage3 = "Please enter a Principal.";

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertNotEquals("Pre-condition: not a Web Booking", ShipmentStatusList.Codes.WebBooking, shipment.JS_ShipmentStatus);

			shipment.JS_OH_DeliveryAgent = notPrincipal.PK;
			AssertHasError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_OH_DeliveryAgent = principal.PK;
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_OH_DeliveryAgent = ZGuid.Missing;
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertHasError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertHasError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;

			shipment.JS_OH_DeliveryAgent = notPrincipal.PK;
			AssertHasError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_OH_DeliveryAgent = principal.PK;
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage1);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage2);
			AssertNoError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			// Changing Status triggers JS_OH_DeliveryAgent validation
			AssertHasError(shipment.JS_OH_DeliveryAgentInfo, ErrorMessage3);
		}

		public void TestPrincipal_List()
		{
			OrgHeader principal = NewPrincipal();
			OrgHeader notPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader carrierAndPrincipal = NewPrincipal();
			OrgHeader carrierNotPrincipal = NewCarrier();
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();

			ZQuery filter = new ZQuery(OrgHeaderSchema.PK, new ZGuid[] { principal.PK, notPrincipal.PK, carrierAndPrincipal.PK, carrierNotPrincipal.PK });

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertNotNull("Principal_List", shipment.Lookups.Principal_List);
			ShipsAgencyPrincipalCollection principals = shipment.Lookups.Principal_List;
			principals.Load();

			AssertContainsExactElementsInAnyOrder("Principal + Carrier and Principal", new BusinessObject[] { principal, carrierAndPrincipal }, principals.Find(filter));

			JobTradeLaneVoyage tradeLane = sailing.Voyage.TradeLanes.AddNew();
			tradeLane.NB_OH = principal.PK;
			sailing.Voyage.JV_OH_Line = carrierAndPrincipal.PK;
			shipment.JS_JX = sailing.PK;

			principals = shipment.Lookups.Principal_List;
			principals.Load();
			AssertContainsExactElementsInAnyOrder("Principal + Carrier and Principal", new BusinessObject[] { principal, carrierAndPrincipal }, principals.Find(filter));

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			try
			{
				Globals.IsWeb = true;
				shipment.JS_OH_DeliveryAgent = ZGuid.Empty;

				principals = shipment.Lookups.Principal_List;
				AssertContainsExactElementsInAnyOrder("Principal + Carrier and Principal", new BusinessObject[] { principal, carrierAndPrincipal }, principals.Find(filter));

				sailing.Voyage.JV_OH_Line = carrierNotPrincipal.PK;
				principals = shipment.Lookups.Principal_List;
				AssertContainsExactElementsInAnyOrder("Should be one principal in the list", new BusinessObject[] { principal }, principals.Find(filter));

				sailing.Voyage.TradeLanes.DeleteAll();
				principals = shipment.Lookups.Principal_List;
				AssertContainsExactElementsInAnyOrder("Should be no principals in the list", Array.Empty<BusinessObject>(), principals.Find(filter));

				Globals.IsWeb = false;
				shipment.JS_OH_DeliveryAgent = principal.PK;
				AssertEquals("Assigned as Principal", principal.PK, shipment.Principal.PK);
				Globals.IsWeb = true;

				principals = shipment.Lookups.Principal_List;
				AssertContainsExactElementsInAnyOrder("Should be one principal in the list", new BusinessObject[] { principal }, principals.Find(filter));
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		public void TestPrincipalList_AllPrincipalsAreDeniedForCurrentUser_ReturnEmptyList()
		{
			var allowedPrincipals = Array.Empty<OrgHeader>();
			var disallowedPrincipals = new OrgHeader[] { NewPrincipal(), NewPrincipal() };
			var allPrincipalsQuery = new ZQuery(OrgHeaderSchema.PK, allowedPrincipals.Concat(disallowedPrincipals).Select(p => p.PK).ToList());
			var user = NewStaff(allowedPrincipals);
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipment = Factory.New<AgencyShipment>();
				var principalsList = shipment.Lookups.Principal_List;
				principalsList.Load();

				AssertContainsExactElementsInAnyOrder("None principals should be returned", Array.Empty<BusinessObject>(), principalsList.Find(allPrincipalsQuery));
			}
		}

		public void TestPrincipalList_AllPrincipalsAreAllowedForCurrentUser_ReturnListOfAllPrincipals()
		{
			var allPrincipals = new OrgHeader[] { NewPrincipal(), NewPrincipal() };
			var allPrincipalsQuery = new ZQuery(OrgHeaderSchema.PK, allPrincipals.Select(p => p.PK).ToList());
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.AgencyPrincipalAccess.IsAllowed = true;

				var shipment = Factory.New<AgencyShipment>();
				var principalsList = shipment.Lookups.Principal_List;
				principalsList.Load();

				AssertContainsExactElementsInAnyOrder("All principals are returned", allPrincipals, principalsList.Find(allPrincipalsQuery));
			}
		}

		public void TestPrincipalList_SpecificPrincipalsAreAllowedForCurrentUser_ReturnListOfAllowedPrincipals()
		{
			var allowedPrincipals = new OrgHeader[] { NewPrincipal(), NewPrincipal() };
			var disallowedPrincipals = new OrgHeader[] { NewPrincipal(), NewPrincipal() };
			var allPrincipalsQuery = new ZQuery(OrgHeaderSchema.PK, allowedPrincipals.Concat(disallowedPrincipals).Select(p => p.PK).ToList());
			var user = NewStaff(allowedPrincipals);
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipment = Factory.New<AgencyShipment>();
				var principalsList = shipment.Lookups.Principal_List;
				principalsList.Load();

				AssertContainsExactElementsInAnyOrder("Only allowed principals are returned", allowedPrincipals, principalsList.Find(allPrincipalsQuery));
			}
		}

		[ExpectNoExceptions]
		public void TestDirtyDbOnlyQueryDuringValidation()
		{
			GlbBranch firstBranch = GlbBranch.CurrentBranch;
			GlbBranch alternateBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, firstBranch.GB_GC));

			OrgHeader principal = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "SNTH";
			voyage.JV_OH_Line = principal.PK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, alternateBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
				AgencyShipment shipment = anotherFactory.New<AgencyShipment>();

				shipment.JS_JX = sailing.PK;
				shipment.JS_OH_DeliveryAgent = shipment.BookedShippingLinePK;
			}
		}

		public void TestBookingPartyNameOrPK()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "LEB";
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.BookingPartyDocumentaryAddress.E2_CompanyName = "ANZ";
			AssertEquals("ANZ", shipment.BookingPartyNameOrPK);

			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = false;
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK.ToString(), shipment.BookingPartyNameOrPK);

			shipment.BookingPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty.ToString(), shipment.BookingPartyNameOrPK);

			shipment.BookingPartyDocumentaryAddress.OrganisationPK = org.PK;
			ZString code = RelatedBusinessObjectAttribute.GetCodeForGuid(shipment.BookingPartyNameOrPKInfo);
			AssertEquals("Shipment.BookingPartyNameOrPK should be Org.PK", org.PK, new ZGuid(shipment.BookingPartyNameOrPKInfo.Value));
			AssertEquals("Ensure that the RelatedBusinessObjectAttribute is correct.", "LEB", code);
		}

		public void TestSettingBookingPartyNameOrPkWithInvalidGuid()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = false;

			shipment.BookingPartyNameOrPK = "not a guid";
			AssertEquals(ZGuid.Empty.ToString(), shipment.BookingPartyNameOrPK);
		}

		public void TestBookingPartyFieldType()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), shipment.BookingPartyFieldType);

			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.OrganisationGuid), shipment.BookingPartyFieldType);
		}

		public void TestBookingPartyNameOrPKMaxLength()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(JobDocAddressSchema.E2_CompanyName.MaxLength, shipment.BookingPartyNameOrPKInfo.MaxLength);

			shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = false;
			AssertEquals(ZGuid.Empty.ToString().Length, shipment.BookingPartyNameOrPKInfo.MaxLength);
		}

		public void TestTransportModeIsSeaAndReadOnly()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertEquals("JS_TransportMode must be SEA", Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
			AssertEquals("JS_TransportMode must be readonly.", true, shipment.JS_TransportModeInfo.ReadOnly);
		}

		public void TestLoadAllocataionUsageSet()
		{
			GenericLoadAllocationUsageSetTest(false);
		}

		public void TestLoadAllocationUsageSet_ByPrincipal()
		{
			GenericLoadAllocationUsageSetTest(true);
		}

		public void GenericLoadAllocationUsageSetTest(bool byPrincipal)
		{
			SetupSailing(false, false, true);

			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();

			decimal expectedWeight;

			if (byPrincipal)
			{
				Sailing1.Origin.VoyageCountry.J0_AllocationsByPrincipal = true;
				expectedWeight = 10;
			}
			else
			{
				expectedWeight = 25;
			}

			Factory.Save();

			NewBulkShipment(Sailing1, principal1, false, true, 10, 10);
			NewBulkShipment(Sailing1, principal2, false, true, 15, 15);

			Factory.Save();

			AgencyShipment shipment = NewBulkShipment(Sailing1, principal1, false, true, 12, 12);

			AllocationUsageSet set = shipment.LoadAllocationUsageSet();
			AssertEquals("Expected Weight", expectedWeight, set.Used.Tonnes);
		}

		public void TestPrincipalAndDeliveryAgent()
		{
			OrgHeader principal = NewPrincipal();
			Factory.Save();
			AgencyShipment shipment = NewFCLShipment(Sailing1, principal, false, true, 20, 2);
			AssertEquals("Principal should be stored in the JS_OH_DeliveryAgent Field", principal.PK, shipment.JS_OH_DeliveryAgent);
			AssertNull("In Agency Delivery Agent should be null", shipment.DeliveryAgent);
			AssertNotNull("Principal should exist on Shipment", shipment.Principal);
		}

		public void TestDefaultFromSailing_SailingWithCarrierWhichIsNotPrincipal_DoNotPopulatePrincipal()
		{
			ShippingCompany1.CompanyData.OB_CRIsShipsAgencyPrincipal = false;
			ImportSailing.Voyage.JV_OH_Line = ShippingCompany1.PK;
			Factory.Save();

			var shipmentToTest = Factory.New<AgencyShipment>();
			shipmentToTest.JS_JX = ImportSailing.PK;

			AssertEquals("The principal should be empty", ZGuid.Empty, shipmentToTest.JS_OH_DeliveryAgent);
		}

		public void TestDefaultFromSailing_SailingWithCarrierWhichIsDeniedPrincipal_DoNotPopulatePrincipal()
		{
			ShippingCompany1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			ImportSailing.Voyage.JV_OH_Line = ShippingCompany1.PK;
			var user = NewStaff(Array.Empty<OrgHeader>());
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipmentToTest = Factory.New<AgencyShipment>();
				shipmentToTest.JS_JX = ImportSailing.PK;

				AssertEquals("The principal should be empty", ZGuid.Empty, shipmentToTest.JS_OH_DeliveryAgent);
			}
		}

		public void TestDefaultFromSailing_SailingWithCarrierWhichIsAllowedPrincipalAndPrincipalIsNotYetSet_PopulatePrincipalFromCarrier()
		{
			var principal = NewPrincipal();
			var user = NewStaff(new OrgHeader[] { principal });
			ImportSailing.Voyage.JV_OH_Line = principal.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipmentToTest = Factory.New<AgencyShipment>();
				shipmentToTest.JS_JX = ImportSailing.PK;

				AssertEquals("The principal should be populated from the carrier", principal.PK, shipmentToTest.JS_OH_DeliveryAgent);
			}
		}

		public void TestDefaultFromSailing_SailingWithCarrierWhichIsAllowedPrincipalAndPrincipalIsAlreadySet_DoNotPopulatePrincipalFromCarrier()
		{
			var hamilton = NewPrincipal();
			var button = NewPrincipal();
			var user = NewStaff(new OrgHeader[] { hamilton, button });
			ImportSailing.Voyage.JV_OH_Line = button.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipmentToTest = Factory.New<AgencyShipment>();
				shipmentToTest.JS_OH_DeliveryAgent = hamilton.PK;
				shipmentToTest.JS_JX = ImportSailing.PK;

				AssertEquals("The principal should not be populated from the carrier", hamilton.PK, shipmentToTest.JS_OH_DeliveryAgent);
			}
		}

		public void TestDefaultFromSailing_TradeLaneWithDeniedPrincipal_DoNotPopulatePrincipal()
		{
			ShippingCompany1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			ImportSailing.Voyage.JV_OH_Line = ShippingCompany1.PK;

			var user = NewStaff(Array.Empty<OrgHeader>());
			var tradeLane = Factory.New<JobTradeLane>();
			var tradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			tradeLane.EJ_OH_RelatedOrg = ShippingCompany1.PK;
			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			tradeLaneVoyage.NB_JV = ImportSailing.Voyage.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipmentToTest = Factory.New<AgencyShipment>();
				shipmentToTest.JS_JX = ImportSailing.PK;

				AssertEquals("The principal should be empty", ZGuid.Empty, shipmentToTest.JS_OH_DeliveryAgent);
			}
		}

		public void TestDefaultFromSailing_TradeLaneWithAllowedPrincipalAndPrincipalIsNotYetSet_PopulatePrincipalFromTradeLane()
		{
			var principal = NewPrincipal();
			ImportSailing.Voyage.JV_OH_Line = principal.PK;

			var user = NewStaff(new OrgHeader[] { principal });
			var tradeLane = Factory.New<JobTradeLane>();
			var tradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			tradeLane.EJ_OH_RelatedOrg = principal.PK;
			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			tradeLaneVoyage.NB_JV = ImportSailing.Voyage.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipmentToTest = Factory.New<AgencyShipment>();
				shipmentToTest.JS_JX = ImportSailing.PK;

				AssertEquals("The principal should be populated from the trade lane", principal.PK, shipmentToTest.JS_OH_DeliveryAgent);
			}
		}

		public void TestDefaultFromSailing_TradeLaneWithAllowedPrincipalAndPrincipalIsAlreadySet_DoNotPopulatePrincipalFromTradeLane()
		{
			var hamilton = NewPrincipal();
			var button = NewPrincipal();
			ImportSailing.Voyage.JV_OH_Line = button.PK;

			var user = NewStaff(new OrgHeader[] { hamilton, button });
			var tradeLane = Factory.New<JobTradeLane>();
			var tradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			tradeLane.EJ_OH_RelatedOrg = button.PK;
			tradeLaneVoyage.NB_EJ = tradeLane.PK;
			tradeLaneVoyage.NB_JV = ImportSailing.Voyage.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var shipmentToTest = Factory.New<AgencyShipment>();
				shipmentToTest.JS_OH_DeliveryAgent = hamilton.PK;
				shipmentToTest.JS_JX = ImportSailing.PK;

				AssertEquals("The principal should not be populated from the trade lane", hamilton.PK, shipmentToTest.JS_OH_DeliveryAgent);
			}
		}

		public void TestDefaultFromSailing_EstimatedDates()
		{
			var date = new ZDate(2023, 06, 01);
			var etd = date.AddDays(2);
			var eta = date.AddDays(3);

			var sailing = CreateSailing("AUMEL", "SGSIN", etd);
			sailing.Destination.JB_E_ARV = eta;
			Factory.Save();

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = "SEA";
			AssertEquals(ZGuid.Empty, shipment.JS_JX);
			AssertEquals(ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals(ZDateTime.Empty, shipment.JS_E_ARV);

			shipment.JS_JX = sailing.PK;
			AssertEquals(etd, shipment.JS_E_DEP);
			AssertEquals(eta, shipment.JS_E_ARV);

			var arvLog = shipment.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertEquals("To: 04-Jun-23|LOC=SGSIN|MOD=SEA", arvLog.SL_Reference);
			var depLog = shipment.Logs.MostRecentLogByEventTime(Events.Departure);
			AssertEquals("To: 03-Jun-23|LOC=AUMEL|MOD=SEA", depLog.SL_Reference);
		}

		[ShipmentDateUpdateConfiguration(Value = true)]
		public void TestDefaultingFromSailing()
		{
			ZDateTime today = ZDateTime.Today;

			ShippingCompany1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			ShippingCompany2.CompanyData.OB_CRIsShipsAgencyPrincipal = false;
			Factory.Save();

			ImportSailing.Voyage.JV_OH_Line = ShippingCompany1.PK;
			ExportSailing.Origin.JA_A_DEP = today;

			AgencyShipment shipment = Factory.New<AgencyShipment>();

			shipment.JS_JX = ImportSailing.PK;
			AssertEquals("Should Set the Carrier from the sailing", ShippingCompany1.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
			AssertEquals("Nothing to default shipped on board date to", ZDateTime.Empty, shipment.JS_ShippedOnBoardDate);
			AssertEquals("Nothing to default issued date from", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			shipment.JS_JX = ExportSailing.PK;
			AssertEquals("Should default shipped on board date", today, shipment.JS_ShippedOnBoardDate);
			AssertEquals("Should default issued date", today, shipment.JS_HouseBillIssueDate);

			ImportSailing1.Voyage.JV_OH_Line = ShippingCompany2.PK;
			shipment.JS_JX = ImportSailing1.PK;
			AssertEquals("Should set the carrier from the sailing", ShippingCompany2.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
			AssertEquals("Should not clear shipped on board date", today, shipment.JS_ShippedOnBoardDate);
			AssertEquals("Should not clear issued date", today, shipment.JS_HouseBillIssueDate);
		}

		public void TestSuspendDefaultingCarrierFromSailing()
		{
			ImportSailing.Voyage.JV_OH_Line = ShippingCompany1.PK;
			ExportSailing.Voyage.JV_OH_Line = ShippingCompany2.PK;

			var shipment = Factory.New<AgencyShipment>();

			using (shipment.SuspendDefaultingCarrierFromSailing())
			{
				shipment.JS_JX = ImportSailing.PK;
			}

			AssertEquals("Should suspend setting the Carrier from the sailing", ZGuid.Empty, shipment.JS_OA_BookedShippingLineAddress);

			shipment.JS_JX = ExportSailing.PK;
			AssertEquals("Should set the Carrier from the sailing", ShippingCompany2.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
		}

		public void TestCarrierIsReadonlyWhenSailingIsAttached()
		{
			var shipment = Factory.New<AgencyShipment>();
			Assert(!shipment.JS_OA_BookedShippingLineAddressInfo.ReadOnly);

			shipment.JS_JX = ImportSailing.PK;
			Assert(shipment.JS_OA_BookedShippingLineAddressInfo.ReadOnly);

			shipment.JS_JX = ZGuid.Empty;
			Assert(!shipment.JS_OA_BookedShippingLineAddressInfo.ReadOnly);
		}

		public void TestHasPostedCharges()
		{
			OrgHeader principal = NewPrincipal();
			AgencyShipment shipment = NewShipment(null, principal, false, false);
			Factory.Save();

			AssertEquals("Principal should NOT be ReadOnly", false, shipment.JS_OH_DeliveryAgentInfo.ReadOnly);

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobCharge charge = NewCharge(job);
			AssertEquals("Charge should NOT be CostPosted", false, charge.IsCostPosted);
			AssertEquals("Charge should NOT be RevenuePosted", false, charge.IsRevenuePosted);
			AssertEquals("Should be FALSE", false, shipment.HasPostedCharges);
			AssertEquals("Principal should NOT be ReadOnly", false, shipment.JS_OH_DeliveryAgentInfo.ReadOnly);

			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("Charge should be CostPosted", true, charge.IsCostPosted);
			AssertEquals("Should be TRUE", true, shipment.HasPostedCharges);
			AssertEquals("Principal should be ReadOnly", true, shipment.JS_OH_DeliveryAgentInfo.ReadOnly);

			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals("Charge should NOT be CostPosted", false, charge.IsCostPosted);
			AssertEquals("Charge should NOT be RevenuePosted", false, charge.IsRevenuePosted);
			AssertEquals("Should be FALSE", false, shipment.HasPostedCharges);
			AssertEquals("Principal should NOT be ReadOnly", false, shipment.JS_OH_DeliveryAgentInfo.ReadOnly);

			charge = NewCharge(job);
			charge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals("Charge should be RevenuePosted", true, charge.IsRevenuePosted);
			AssertEquals("Should be TRUE", true, shipment.HasPostedCharges);
			AssertEquals("Principal should be ReadOnly", true, shipment.JS_OH_DeliveryAgentInfo.ReadOnly);
		}

		public void TestPostedStateChanged()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();

			int count = 0;
			shipment.JS_OH_DeliveryAgentInfo.ValueChanged += new EventHandler(delegate
			{ count++; });

			((IJobInvoicingPlugIn)shipment).InvoicingSupporter.PostedStateChanged();
			AssertEquals("JS_OH_DeliveryAgentInfo.ValueChanged event should be raised only once", 1, count);
		}

		public void TestIBillGenerationSupport_Default()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			IBillGenerationSupport support = shipment;
			AssertEquals("Default TranshipmentIndicator", "0", support.TranshipmentIndicator);
			AssertNull("Default carrier principal", support.CarrierPrincipal);
			AssertNull("Default Destination", support.Destination);
			AssertNull("Default Origin", support.Origin);
			AssertEquals("Default TransportMode", support.TransportMode, Core.Constants.TransportModes.Sea);
			AssertNull("Default Load", support.Load);
			AssertNull("Default Discharge", support.Discharge);
		}

		public void TestIBillGeneratinSupport_CarrierPrincipal()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			IBillGenerationSupport support = shipment;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = org.PK;
			AssertEquals("CarrierPrincipal for agency shipment", support.CarrierPrincipal, org);
		}

		public void TestIBillGenerationSupport_Load_Discharge()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			IBillGenerationSupport support = shipment;
			RefUNLOCO loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefUNLOCO dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "HKHKG");

			shipment.JS_NKLoadPort = loadPort.RL_Code;
			shipment.JS_NKDischargePort = dischargePort.RL_Code;

			AssertEquals("IBillGenerationSupport.Load should be AUSYD", loadPort, support.Load);
			AssertEquals("IBillGenerationSupport.Discharge should be HKHKG", dischargePort, support.Discharge);
		}

		public void TestIBillGenerationSupport_TranshipmentIndicator_Laden()
		{
			ContainerTranshipmentIndicatorCollection collection = ContainerTranshipmentIndicatorCollection.NewAndPopulate();
			collection.Laden.Domestic = "1";
			collection.Laden.Tranship = "2";
			collection.Laden.Direct = "3";
			collection.Empty.Domestic = "4";
			collection.Empty.Tranship = "5";
			collection.Empty.Direct = "6";
			collection.BreakBulk.Domestic = "7";
			collection.BreakBulk.Tranship = "8";
			collection.BreakBulk.Direct = "9";

			AgencyRegistry.Instance.ContainerTranshipmentIndicator.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.BookedContainers.AddNew().JC_IsEmptyContainer = false;

			IBillGenerationSupport support = shipment;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_NKDischargePort = "AUBNE";
			AssertEquals("1", support.TranshipmentIndicator);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("2", support.TranshipmentIndicator);

			shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("3", support.TranshipmentIndicator);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.BookedContainers.RemoveAndDeleteAll();
			shipment.RealContainers.AddNew().JC_IsEmptyContainer = false;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_NKDischargePort = "AUBNE";
			AssertEquals("1", support.TranshipmentIndicator);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("2", support.TranshipmentIndicator);

			shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("3", support.TranshipmentIndicator);
		}

		public void TestIBillGenerationSupport_TranshipmentIndicator_Empty()
		{
			ContainerTranshipmentIndicatorCollection collection = ContainerTranshipmentIndicatorCollection.NewAndPopulate();
			collection.Laden.Domestic = "1";
			collection.Laden.Tranship = "2";
			collection.Laden.Direct = "3";
			collection.Empty.Domestic = "4";
			collection.Empty.Tranship = "5";
			collection.Empty.Direct = "6";
			collection.BreakBulk.Domestic = "7";
			collection.BreakBulk.Tranship = "8";
			collection.BreakBulk.Direct = "9";

			AgencyRegistry.Instance.ContainerTranshipmentIndicator.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.BookedContainers.AddNew().JC_IsEmptyContainer = true;

			IBillGenerationSupport support = shipment;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_NKDischargePort = "AUBNE";
			AssertEquals("4", support.TranshipmentIndicator);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("5", support.TranshipmentIndicator);

			shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("6", support.TranshipmentIndicator);

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.BookedContainers.RemoveAndDeleteAll();
			shipment.RealContainers.AddNew().JC_IsEmptyContainer = true;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_NKDischargePort = "AUBNE";
			AssertEquals("4", support.TranshipmentIndicator);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("5", support.TranshipmentIndicator);

			shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("6", support.TranshipmentIndicator);
		}

		public void TestIBillGenerationSupport_TranshipmentIndicator_BreakBulk()
		{
			ContainerTranshipmentIndicatorCollection collection = ContainerTranshipmentIndicatorCollection.NewAndPopulate();
			collection.Laden.Domestic = "1";
			collection.Laden.Tranship = "2";
			collection.Laden.Direct = "3";
			collection.Empty.Domestic = "4";
			collection.Empty.Tranship = "5";
			collection.Empty.Direct = "6";
			collection.BreakBulk.Domestic = "7";
			collection.BreakBulk.Tranship = "8";
			collection.BreakBulk.Direct = "9";

			AgencyRegistry.Instance.ContainerTranshipmentIndicator.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			IBillGenerationSupport support = shipment;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_NKDischargePort = "AUBNE";
			AssertEquals("7", support.TranshipmentIndicator);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("8", support.TranshipmentIndicator);

			shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("9", support.TranshipmentIndicator);

			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_NKDischargePort = "AUBNE";
			AssertEquals("7", support.TranshipmentIndicator);

			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("8", support.TranshipmentIndicator);

			shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("9", support.TranshipmentIndicator);
		}

		public void TestCustomsEntryNumberType()
		{
			ZString cAN = CANType.CustomsAuthorityNumber.Code;
			ZString cCN = CANType.ContingencyCustomsAuthorityNumber.Code;
			ZString xDD = CMRExportExemptionCodes.EXDD.Code;

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();

			AssertEquals("Shipment should has NO Entry Number", 0, shipment.CusEntryNumbers.Count);
			AssertEquals("Number Type should be Empty", ZString.Empty, shipment.CustomsEntryNumberType);
			AssertEquals("Number should be Empty", ZString.Empty, shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = cCN;
			AssertEquals("Shipment should has Entry Number", 1, shipment.CusEntryNumbers.Count);
			AssertEquals("Number Type should be 'CCN'", "CCN", shipment.CustomsEntryNumberType);
			AssertEquals("Number should be Empty", ZString.Empty, shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumber = "111";
			AssertEquals("Number should be '111'", "111", shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = cAN;
			AssertEquals("Number Type should be 'CAN'", "CAN", shipment.CustomsEntryNumberType);

			shipment.CustomsEntryNumberType = xDD;
			AssertEquals("Number Type should be 'EXDD'", "EXDD", shipment.CustomsEntryNumberType);
			AssertEquals("Number should be Empty", ZString.Empty, shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = cCN;
			AssertEquals("Number Type should be 'CCN'", "CCN", shipment.CustomsEntryNumberType);

			shipment.CustomsEntryNumberType = "";
			AssertEquals("Shipment should has NO Entry Number", 0, shipment.CusEntryNumbers.Count);
			AssertEquals("Number Type should be Empty", ZString.Empty, shipment.CustomsEntryNumberType);
			AssertEquals("Number should be Empty", ZString.Empty, shipment.CustomsEntryNumber);

			shipment.CustomsEntryNumberType = "";
			AssertEquals("Shipment should has NO Entry Number", 0, shipment.CusEntryNumbers.Count);
			AssertEquals("Number Type should be Empty", ZString.Empty, shipment.CustomsEntryNumberType);
			AssertEquals("Number should be Empty", ZString.Empty, shipment.CustomsEntryNumber);
		}

		public void TestShipmentCustomsEntryNumber()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertEquals(typeof(AgencyShipmentCustomsEntryNumber), shipment.ShipmentCustomsEntryNumber.GetType());
		}

		public void TestCommunityTransitStatus()
		{
			ZString cAN = CANType.CustomsAuthorityNumber.Code;
			ZString cCN = CANType.ContingencyCustomsAuthorityNumber.Code;

			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();

			AssertEquals("CT Status should be Empty", ZString.Empty, shipment.JS_CommunityTransitStatus);

			shipment.JS_CommunityTransitStatus = cCN;
			AssertEquals("CT Status should be 'CCN'", "CCN", shipment.JS_CommunityTransitStatus);

			shipment.CustomsEntryNumberType = cAN;
			AssertEquals("CT Status Type should be 'CAN'", "CAN", shipment.CustomsEntryNumberType);
		}

		public void TestIsCompanyRegisteredForGST()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();

			AssertEquals("IsCompanyRegisteredForGST should be TRUE", true, shipment.IsCompanyRegisteredForGST);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertEquals("IsCompanyRegisteredForGST should be FALSE", false, shipment.IsCompanyRegisteredForGST);
		}

		public void TestJS_HBLAWBChargesDisplay()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();

			const string expected =
				"NON - No Charges Showing\r\n" +
				"SHW - Show Collect Charges\r\n" +
				"AGR - Show \"As Agreed\" in the charges section\r\n" +
				"ALL - Show All Charges - Prepaid & Collect\r\n" +
				"PPD - Show Prepaid Charges" +
				"";

			PropertyDescriptor property = TypeDescriptor.GetProperties(shipment)[JobShipmentSchema.Constants.JS_HBLAWBChargesDisplay];
			ReadOnlyCodeDescriptionPairList list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(shipment, property);
			AssertMultilineASCIIEquals("Should be correct JS_HBLAWBChargesDisplay_List", expected, list.ElementsAsString);

			AssertEquals("JS_HBLAWBChargesDisplay should be SHW", "SHW", shipment.JS_HBLAWBChargesDisplay);

			AgencyRegistry.Instance.AgencyOBLChargesDefaultDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PPD");
			shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AssertEquals("JS_HBLAWBChargesDisplay should be PPD", "PPD", shipment.JS_HBLAWBChargesDisplay);
		}

		public void TestSendingReceivingAgentAddresses()
		{
			var principalA = Factory.NewWithValidTestData<OrgHeader>();

			var portA1 = principalA.CarrierAppointedAgentPorts_Agency.AddNew();
			var orgA1 = Factory.NewWithValidTestData<OrgHeader>();
			portA1.O5_PortOrCountry = "AUSYD";
			portA1.O5_OA_AgentOfficeAddress = orgA1.MainAddress.PK;

			var portA2 = principalA.CarrierAppointedAgentPorts_Agency.AddNew();
			var orgA2 = Factory.NewWithValidTestData<OrgHeader>();
			portA2.O5_PortOrCountry = "AUMEL";
			portA2.O5_OA_AgentOfficeAddress = orgA2.MainAddress.PK;

			var portA3 = principalA.CarrierAppointedAgentPorts_Agency.AddNew();
			var orgA3 = Factory.NewWithValidTestData<OrgHeader>();
			portA3.O5_PortOrCountry = "NZAKL";
			portA3.O5_OA_AgentOfficeAddress = orgA3.MainAddress.PK;

			var portA4 = principalA.CarrierAppointedAgentPorts_Agency.AddNew();
			var orgA4 = Factory.NewWithValidTestData<OrgHeader>();
			portA4.O5_PortOrCountry = "USLAX";
			portA4.O5_OA_AgentOfficeAddress = orgA4.MainAddress.PK;

			var principalB = Factory.NewWithValidTestData<OrgHeader>();

			var portB1 = principalB.CarrierAppointedAgentPorts_Agency.AddNew();
			var orgB1 = Factory.NewWithValidTestData<OrgHeader>();
			portB1.O5_PortOrCountry = "UAIEV";
			portB1.O5_OA_AgentOfficeAddress = orgB1.MainAddress.PK;

			var shipment = Factory.New<AgencyShipment>();

			AssertEquals(true, shipment.SendingAgentAddressPKInfo.ReadOnly);
			AssertEquals(true, shipment.ReceivingAgentAddressPKInfo.ReadOnly);
			AssertSendingAgent(shipment, null);
			AssertReceivingAgent(shipment, null);

			shipment.JS_OH_DeliveryAgent = principalB.PK;
			AssertSendingAgent(shipment, null);
			AssertReceivingAgent(shipment, null);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertSendingAgent(shipment, null);
			AssertReceivingAgent(shipment, null);

			shipment.JS_RL_NKDestination = "UAIEV";
			AssertSendingAgent(shipment, null);
			AssertReceivingAgent(shipment, orgB1.MainAddress);

			shipment.JS_OH_DeliveryAgent = principalA.PK;
			AssertSendingAgent(shipment, orgA1.MainAddress);
			AssertReceivingAgent(shipment, null);

			shipment.JS_RL_NKDestination = "USLAX";
			AssertSendingAgent(shipment, orgA1.MainAddress);
			AssertReceivingAgent(shipment, orgA4.MainAddress);

			shipment.JS_RL_NKOrigin = "";
			AssertSendingAgent(shipment, null);
			AssertReceivingAgent(shipment, orgA4.MainAddress);

			shipment.JS_RL_NKDestination = "";
			AssertSendingAgent(shipment, null);
			AssertReceivingAgent(shipment, null);

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "NZAKL";
			AssertSendingAgent(shipment, orgA2.MainAddress);
			AssertReceivingAgent(shipment, orgA3.MainAddress);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newShipment = newFactory.Load<AgencyShipment>(shipment.PK);
			var sendingAddress = newFactory.Load<OrgAddress>(orgA2.MainAddress.PK);
			var receivingAddress = newFactory.Load<OrgAddress>(orgA3.MainAddress.PK);
			AssertSendingAgent(newShipment, sendingAddress);
			AssertReceivingAgent(newShipment, receivingAddress);
		}

		public void TestImportReleaseDepot()
		{
			ZDateTime today = ZDateTime.Today;
			SetupSailing(false, false, false);

			var depot1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var depot2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

			var shipment = Factory.New<AgencyShipment>();
			AssertEquals(0, shipment.TransportsIncludingRelated.Count);
			AssertEquals(null, shipment.ImportReleaseDepot);

			shipment.JS_JX = Sailing1.PK;
			AssertEquals(1, shipment.TransportsIncludingRelated.Count);
			AssertEquals(null, shipment.ImportReleaseDepot);

			var transport1 = shipment.TransportsIncludingRelated[0];
			transport1.JW_ATA = today;
			transport1.JW_OA_ArrivalLocation = depot1.PK;
			AssertEquals(depot1, shipment.ImportReleaseDepot);

			var transport2 = shipment.TransportsIncludingRelated.AddNew();
			transport2.JW_ATA = today.AddDays(10);
			transport2.JW_IsLinked = true;
			transport2.JW_JX = Sailing2.PK;
			AssertEquals(2, shipment.TransportsIncludingRelated.Count);
			AssertEquals(null, shipment.ImportReleaseDepot);

			transport2.JW_OA_ArrivalLocation = depot2.PK;
			AssertEquals(depot2, shipment.ImportReleaseDepot);
		}

		public void TestExportReceivingDepot()
		{
			ZDateTime today = ZDateTime.Today;
			SetupSailing(false, false, false);

			var depot1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var depot2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

			var shipment = Factory.New<AgencyShipment>();
			AssertEquals(0, shipment.TransportsIncludingRelated.Count);
			AssertEquals(null, shipment.ExportReceivingDepot);

			shipment.JS_JX = Sailing1.PK;
			AssertEquals(1, shipment.TransportsIncludingRelated.Count);
			AssertEquals(null, shipment.ExportReceivingDepot);

			var transport1 = shipment.TransportsIncludingRelated[0];
			transport1.JW_ATA = today;
			var transport2 = shipment.TransportsIncludingRelated.AddNew();
			transport2.JW_ATA = today.AddDays(10);
			transport2.JW_IsLinked = true;
			transport2.JW_JX = Sailing2.PK;
			AssertEquals(2, shipment.TransportsIncludingRelated.Count);
			AssertEquals(null, shipment.ExportReceivingDepot);

			transport2.JW_OA_DepartureLocation = depot2.PK;
			AssertEquals(null, shipment.ExportReceivingDepot);

			transport1.JW_OA_DepartureLocation = depot1.PK;
			AssertEquals(depot1, shipment.ExportReceivingDepot);
		}

		public void TestChildCollectionsWouldBeDeletedOnDelete()
		{
			var shipment = Factory.New<AgencyShipment>();

			var realContainer = shipment.RealContainers.AddNew();
			var bookedContainer = shipment.BookedContainers.AddNew();
			var workflowItem = shipment.WorkflowItems.AddNew();

			var objectsFromChildCollections = new BusinessObject[] { realContainer, bookedContainer, workflowItem };
			AssertEquals(false, objectsFromChildCollections.Any(bizo => bizo.IsDeleted));

			shipment.Delete();
			AssertEquals(true, objectsFromChildCollections.All(bizo => bizo.IsDeleted));
		}

		public void TestSupportedAddressTypes_ShouldContainSendingForwarderAddress()
		{
			var shipment = Factory.New<AgencyShipment>();
			var addresses = (IDocAddresses)shipment;

			AssertCollectionContains("should support sending forwarder address", DocAddressType.SendingForwarderAddress, addresses.SupportedAddressTypes);
			AssertEquals(shipment.SendingForwarderAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.SendingForwarderAddress).DefaultDocAddressType);
		}

		public void TestSupportedAddressTypes_ShouldContainReceivingForwarderAddress()
		{
			var shipment = Factory.New<AgencyShipment>();
			var addresses = (IDocAddresses)shipment;

			AssertCollectionContains("should support receiving forwarder address", DocAddressType.ReceivingForwarderAddress, addresses.SupportedAddressTypes);
			AssertEquals(shipment.ReceivingForwarderAddress.DocAddressType, addresses.GetDocAddressRequirement(DocAddressType.ReceivingForwarderAddress).DefaultDocAddressType);
		}

		public void TestGetBusinessObjectsWithRelatedEvents_Booking_TheListShouldIncludeOnlyBookedContainers()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "FJSUV";

			var expectedResult = new List<BusinessObject>(booking.BusinessObjectsWithRelatedEvents);

			var containers = Enumerable.Range(0, 4).Select(i => Factory.New<AgencyShipmentContainer>()).ToList();
			booking.BookedContainers.AddRange(containers.Where((c, i) => i % 2 == 0));
			booking.RealContainers.AddRange(containers.Where((c, i) => i % 2 != 0));

			expectedResult.AddRange(booking.BookedContainers);

			var actualResult = booking.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder("Only booked containers are returned", expectedResult, actualResult);
		}

		public void TestGetBusinessObjectsWithRelatedEvents_BillOfLading_TheListShouldIncludeOnlyRealContainers()
		{
			var billOfLoading = Factory.New<BillOfLading>();
			billOfLoading.JS_RL_NKOrigin = "AUSYD";
			billOfLoading.JS_RL_NKDestination = "FJSUV";

			var expectedResult = new List<BusinessObject>(billOfLoading.BusinessObjectsWithRelatedEvents);

			var containers = Enumerable.Range(0, 4).Select(i => Factory.New<AgencyShipmentContainer>()).ToList();
			billOfLoading.BookedContainers.AddRange(containers.Where((c, i) => i % 2 == 0));
			billOfLoading.RealContainers.AddRange(containers.Where((c, i) => i % 2 != 0));

			expectedResult.AddRange(billOfLoading.RealContainers);

			var actualResult = billOfLoading.BusinessObjectsWithRelatedEvents;
			AssertContainsExactElementsInAnyOrder("Only real containers are returned", expectedResult, actualResult);
		}

		public void TestDoNotCreateDuplicateMainSailings()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "001";

			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new ZDateTime(2012, 12, 1);

			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			voyage1.GenerateSailings();

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "002";

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = new ZDateTime(2012, 12, 1);

			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";

			voyage2.GenerateSailings();

			Factory.Save();

			Func<Transport, string> transportFormattter = (t) =>
			{
				return string.Format("{0}|{1}|{2}->{3}",
					t.JW_TransportType,
					t.JW_VoyageFlight,
					t.JW_RL_NKLoadPort,
					t.JW_RL_NKDiscPort);
			};

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage1.Sailings[0].PK;

			AssertContainsExactElementsInAnyOrder("expected one main voyage", new[]
			{
				"MAI|001|AUSYD->NZAKL"
			},
			shipment.Transports.Cast<Transport>().Select(transportFormattter));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			shipment = newFactory.Load<AgencyShipment>(shipment.PK);
			shipment.JS_JX = voyage2.Sailings[0].PK;

			AssertContainsExactElementsInAnyOrder("expected one main voyage", new[]
			{
				"MAI|002|AUSYD->NZAKL"
			},
			shipment.Transports.Cast<Transport>().Select(transportFormattter));
		}

		public void TestShipmentStatusConcurrencyPolicy()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var shipmentFactory1 = factory1.Load<AgencyShipment>(shipment.PK);
			var shipmentFactory2 = factory2.Load<AgencyShipment>(shipment.PK);

			shipmentFactory1.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			shipmentFactory2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Should throw concurrency error.");
			}
			catch (ZSaveConcurrencyException ex)
			{
				AssertContainsExactElementsInAnyOrder(new[] { shipmentFactory2 }, ex.BusinessObjects);
				ZExceptionReporting.HandleSaveException(ex);
			}

			try
			{
				factory2.Save();
				Fail("Should throw save exception as changes cannot be merged.");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShipmentStatusChangedEventReference()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;

				var parameters = new KeyValuePair<string, string>[]
				{
				new KeyValuePair<string, string>(Params.New, ShipmentStatusList.Codes.Confirmed),
				new KeyValuePair<string, string>(Params.Old, ShipmentStatusList.Codes.Confirmed),
				new KeyValuePair<string, string>(Params.Reason, ":)"),
				new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus)
				};

				shipment.Logs.CreateOrRecreateEventLog(Events.StatusUpdated, EstimateActual.Actual, ZDateTimeOffset.UtcNow, "", parameters);
				AssertEquals("|NEW=CNF|OLD=CNF|RES=:)|TYP=Shipment Status"
					, shipment.Logs.GetAllLogs().Cast<StmALog>().Single(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);

				Factory.Save();

				AssertEquals("|NEW=WTL|TYP=Shipment Status"
					, shipment.Logs.GetAllLogs().Cast<StmALog>().Single(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);
			}
		}

		public void TestShipmentStatusChangedEventReference_Prefix()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<AgencyShipment>();
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

				Factory.Save();
				AssertEquals("|NEW=BKD|TYP=Shipment Status"
					, shipment.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);

				Thread.Sleep(10);
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
				Factory.Save();
				AssertEquals("|NEW=EBK|OLD=BKD|RES=Electronic Booking Received|TYP=Shipment Status"
					, shipment.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);

				Thread.Sleep(10);
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
				shipment.PurposeDescription = "Purpose";
				Factory.Save();
				AssertEquals("Purpose|NEW=ESI|OLD=EBK|RES=Electronic Shipping Instruction Received|TYP=Shipment Status"
					, shipment.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);

				Thread.Sleep(10);
				shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;
				shipment.PurposeDescription = "Amendment";
				Factory.Save();
				AssertEquals("Amendment|NEW=EBC|OLD=ESI|TYP=Shipment Status"
					, shipment.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);

				foreach (ICodeDescription status in new ShipmentStatusList())
				{
					if (new[] { ShipmentStatusList.Codes.ElectronicBooking, ShipmentStatusList.Codes.ElectronicShippingInstruction, ShipmentStatusList.Codes.EBookingCancellationRequest }.Contains(status.Code))
					{
						continue;
					}

					Thread.Sleep(10);
					shipment.JS_ShipmentStatus = status.Code;
					Factory.Save();
					AssertStartsWith("No customized reference"
						, "|NEW="
						, shipment.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).First(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode && x.Parameters[Params.Type] == Core.Constants.EventReferenceMessageTypes.ShipmentStatus).SL_Reference);
				}
			}
		}

		#region TestEventPropagation

		public void TestArrivalAndDepartureEventsOnAgencyShipmentShouldBePropagatedToTransportLeg()
		{
			var shipment = Factory.New<AgencyShipment>();

			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";

			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";

			AssertEquals("Precondition: Arrival event not recorded", null, firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertEquals("Precondition: Departure event not recorded", null, secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));

			shipment.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal) });
			AssertNull("Arrival Event with incomplete parameters has not been propagated", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));

			shipment.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 20), new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL") });
			AssertNull("Departure Event with incomplete parameters has not been propagated", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));

			var eventParameters = new KeyValuePair<string, string>[] {
				new KeyValuePair<string,string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal),
				new KeyValuePair<string,string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL")
			};

			var arrivalEvent = shipment.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), eventParameters);
			var departureEvent = shipment.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 18), eventParameters);

			AssertNotNull("Arrival Event has been propagated to arrival transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated to arrival transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Departure));
			AssertNotNull("Departure Event has been propagated to departure transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
			AssertNull("Arrival Event has not been propagated to departure transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Arrival));

			AssertEquals("Arrival Date should be correct", arrivalEvent.SL_EventTime, firstTransport.JW_ATA);
			AssertEquals("Departure Date should be correct", departureEvent.SL_EventTime, secondTransport.JW_ATD);

			AssertEquals("Arrival Event from transport leg should not cascade back to Consol", arrivalEvent, arrivalEvent);
			AssertEquals("Departure Event from transport leg should not cascade back to Consol", departureEvent, departureEvent);
		}

		public void TestArrivalDepartureEventOnAgencyShipmentIsNotPropagatedWhenTransportHasDate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";

			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_ETA = new ZDateTime(2015, 05, 21);

			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_ATD = new ZDateTime(2015, 05, 19);

			var eventParameters = new KeyValuePair<string, string>[] {
				new KeyValuePair<string,string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Code.Terminal),
				new KeyValuePair<string,string>(EventConstants.EventReferenceParameters.Codes.Location, "AUMEL")
			};

			var arrivalEvent = shipment.Logs.AddNew(Events.Arrival, new ZDateTimeOffset(2015, 05, 20), true, eventParameters);
			var departureEvent = shipment.Logs.AddNew(Events.Departure, new ZDateTimeOffset(2015, 05, 18), false, eventParameters);

			AssertNull("Arrival Event has not been propagated transport leg", firstTransport.Logs.MostRecentLogByEventTime(Events.Arrival));
			AssertNull("Departure Event has not been propagated transport leg", secondTransport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		public void TestArrivalAndDepartureEventsParameters()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = "ROA";

			var transport = shipment.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = "ROA";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";

			var arvParameters = transport.GetParametersForEvent(Events.Arrival);
			AssertEquals(EventConstants.Facilities.Code.Terminal, arvParameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("AUMEL", arvParameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("ROA", arvParameters[EventConstants.EventReferenceParameters.Codes.Mode]);

			var depParameters = transport.GetParametersForEvent(Events.Departure);
			AssertEquals(EventConstants.Facilities.Code.Terminal, depParameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("AUSYD", depParameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("ROA", depParameters[EventConstants.EventReferenceParameters.Codes.Mode]);
		}

		#endregion

		#region PickupRoadOrRailLeg/DeliveryRoadOrRailLeg Defaulting

		public void TestPickupLeg_Defaulting_TransportTypeChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertPickupLeg_Defaulting_TransportTypeChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestPickupLeg_Defaulting_LoadPortChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertPickupLeg_Defaulting_LoadPortChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestPickupLeg_Defaulting_ConsignorChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertPickupLeg_Defaulting_ConsignorChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestPickupLeg_Defaulting_TransportModeChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertPickupLeg_Defaulting_TransportModeChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestDeliveryLeg_Defaulting_TransportTypeChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertDeliveryLeg_Defaulting_TransportTypeChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestDeliveryLeg_Defaulting_DischargePortChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertDeliveryLeg_Defaulting_DischargePortChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestDeliveryLeg_Defaulting_ConsigneeChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertDeliveryLeg_Defaulting_ConsigneeChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestDeliveryLeg_Defaulting_TransportModeChange()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertDeliveryLeg_Defaulting_TransportModeChange(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		#region PickupRoadOrRailLeg/DeliveryRoadOrRailLeg Defaulting Helpers

		void AssertPickupLeg_Defaulting_TransportTypeChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var transport = SetupTransport(shipment, transportMode, Core.Constants.TransportPlanningType.MainVessel);

			shipment.ConsignorPickupAddress.OrganisationPK = consignor.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "AUSYD";
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_TransportType = transportType;
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertPickupLeg_Defaulting_LoadPortChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		public void AssertPickupLeg_Defaulting_ConsignorChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var consignor1 = SetupConsignor("TESTIGNOR1", "TEST CONSIGNOR ADDRESS 1");

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals(consignor1.MainAddress.PK, transport.JW_OA_DepartureLocation);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor2 = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor2.Addresses.Count);
			var pickupAddress = consignor2.Addresses[1];

			transport.JW_OA_DepartureLocation = ZGuid.Empty;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertPickupLeg_Defaulting_TransportModeChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var transport = SetupTransport(shipment, Core.Constants.TransportModes.Air, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "AUSYD";
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_TransportMode = transportMode;
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_Defaulting_TransportTypeChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var transport = SetupTransport(shipment, transportMode, Core.Constants.TransportPlanningType.MainVessel);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_TransportType = transportType;
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_Defaulting_DischargePortChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_Defaulting_ConsigneeChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var consignee1 = SetupConsignee("TESTIGNEE1", "TEST CONSIGNEE ADDRESS 1");

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee1.PK;
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(consignee1.MainAddress.PK, transport.JW_OA_ArrivalLocation);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			var consignee2 = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee2.Addresses.Count);
			var deliveryAddress = consignee2.Addresses[1];

			transport.JW_OA_ArrivalLocation = ZGuid.Empty;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee2.PK;

			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_Defaulting_TransportModeChange(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var transport = SetupTransport(shipment, Core.Constants.TransportModes.Air, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_TransportMode = transportMode;
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		OrgHeader SetupConsignorWithRelatedParty(ZString relatedPartyTransportMode, ZString relatedPartyContainerMode, OrgHeader relatedParty)
		{
			var consignor = SetupConsignor("TESTIGNOR", "TEST CONSIGNOR ADDRESS");

			var pickupAddress = consignor.Addresses.AddNew();
			pickupAddress.OA_Address1 = "Test Pickup Address";
			pickupAddress.AddAddressType(OrgAddressType.Pickup);

			consignor.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, relatedPartyTransportMode, relatedPartyContainerMode, GlbCompany.CurrentCompany);
			var orgRelatedParty = consignor.AllRelatedParties[0];
			orgRelatedParty.PR_OA = pickupAddress.PK;

			return consignor;
		}

		OrgHeader SetupConsignor(ZString code, ZString address1)
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = code;
			consignor.OH_IsConsignor = true;
			consignor.MainAddress.OA_Address1 = address1;
			consignor.MainAddress.AddAddressType(OrgAddressType.Office);

			return consignor;
		}

		OrgHeader SetupConsigneeWithRelatedParty(ZString relatedPartyTransportMode, ZString relatedPartyContainerMode, OrgHeader relatedParty)
		{
			var consignee = SetupConsignee("TESTIGNEE", "TEST CONSIGNEE ADDRESS");

			var deliveryAddress = consignee.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "Test Delivery Address";
			deliveryAddress.AddAddressType(OrgAddressType.Delivery);

			consignee.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, relatedPartyTransportMode, relatedPartyContainerMode, GlbCompany.CurrentCompany);
			var orgRelatedParty = consignee.AllRelatedParties[0];
			orgRelatedParty.PR_OA = deliveryAddress.PK;

			return consignee;
		}

		OrgHeader SetupConsignee(ZString code, ZString address1)
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = code;
			consignee.OH_IsConsignee = true;
			consignee.MainAddress.OA_Address1 = address1;
			consignee.MainAddress.AddAddressType(OrgAddressType.Office);

			return consignee;
		}

		Transport SetupTransport(AgencyShipment shipment, ZString transportMode, ZString transportType)
		{
			var etd = ZDateTime.Now;
			var eta = etd.AddHours(5);
			var atd = etd.AddHours(1);
			var ata = eta.AddHours(1);

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = transportMode;
			transport.JW_TransportType = transportType;
			transport.JW_ETA = eta;
			transport.JW_ATA = ata;
			transport.JW_ETD = etd;
			transport.JW_ATD = atd;
			transport.JW_IsLinked = true;

			return transport;
		}

		List<Tuple<ZString, ZString>> TransportModeTypes
		{
			get
			{
				return new List<Tuple<ZString, ZString>>()
				{
					{ Core.Constants.TransportModes.Road, Core.Constants.TransportPlanningType.PreCarriage },
					{ Core.Constants.TransportModes.Road, Core.Constants.TransportPlanningType.OnForwarding },
					{ Core.Constants.TransportModes.Road, Core.Constants.TransportPlanningType.Other },
					{ Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.PreCarriage },
					{ Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.OnForwarding },
					{ Core.Constants.TransportModes.Rail, Core.Constants.TransportPlanningType.Other }
				};
			}
		}

		List<Tuple<ZString, ZString, ZString>> PackingAndRelatedPartyTransportContainerModes
		{
			get
			{
				if (packingAndRelatedPartyTransportContainerModes == null)
				{
					packingAndRelatedPartyTransportContainerModes = new List<Tuple<ZString, ZString, ZString>>();

					var shipment = Factory.New<AgencyShipment>();
					foreach (var packingModePair in shipment.Lookups.JS_PackingMode_List.ToArray())
					{
						packingAndRelatedPartyTransportContainerModes.Add(new Tuple<ZString, ZString, ZString>(packingModePair.Code, Constants.TransportModes.Sea, packingModePair.Code));
						packingAndRelatedPartyTransportContainerModes.Add(new Tuple<ZString, ZString, ZString>(packingModePair.Code, Constants.TransportModes.All, ZString.Empty));
					}
				}

				return packingAndRelatedPartyTransportContainerModes;
			}
		}
		List<Tuple<ZString, ZString, ZString>> packingAndRelatedPartyTransportContainerModes;

		#endregion

		#endregion

		#region PickupRoadOrRailLeg/DeliveryRoadOrRailLeg Carrier Defaulting

		public void TestPickupLeg_CarrierDefaulting_RelatedPartyMatching()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes.Where(p => p.Item2 != Constants.TransportModes.All))
				{
					AssertPickupLeg_CarrierDefaulting_RelatedPartyMatching(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestDeliveryLeg_CarrierDefaulting_RelatedPartyMatching()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes.Where(p => p.Item2 != Constants.TransportModes.All))
				{
					AssertDeliveryLeg_CarrierDefaulting_RelatedPartyMatching(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_ForLinkTransportMode_Sea(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
					AssertPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToLinkTransportMode_All(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
					AssertPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToRelatedParty(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		public void TestDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty()
		{
			foreach (var modeTypePair in TransportModeTypes)
			{
				foreach (var modeTuple in PackingAndRelatedPartyTransportContainerModes)
				{
					AssertDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_ForLinkTransportMode_Sea(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
					AssertDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToLinkTransportMode_All(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
					AssertDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToRelatedParty(modeTypePair.Item1, modeTypePair.Item2, modeTuple.Item1, modeTuple.Item2, modeTuple.Item3);
				}
			}
		}

		#region PickupRoadOrRailLeg/DeliveryRoadOrRailLeg Carrier Defaulting Helpers

		void AssertPickupLeg_CarrierDefaulting_RelatedPartyMatching(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			Assert(relatedPartyTransportMode != Constants.TransportModes.All);

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty1);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var relatedParty2 = Factory.NewWithValidTestData<OrgHeader>();
			consignor.AddRelatedParty(relatedParty2.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			AssertEquals(2, consignor.AllRelatedParties.Count);
			var orgRelatedParty2 = consignor.AllRelatedParties[1];
			orgRelatedParty2.PR_OA = pickupAddress.PK;

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "AUSYD";
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(relatedParty1.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_CarrierDefaulting_RelatedPartyMatching(ZString transportMode, ZString transportType, ZString packingMode, ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			Assert(relatedPartyTransportMode != Constants.TransportModes.All);

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = packingMode;

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty1);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var relatedParty2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee.AddRelatedParty(relatedParty2.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			AssertEquals(2, consignee.AllRelatedParties.Count);
			var orgRelatedParty2 = consignee.AllRelatedParties[1];
			orgRelatedParty2.PR_OA = deliveryAddress.PK;

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(relatedParty1.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_ForLinkTransportMode_Sea(ZString transportMode, ZString transportType, ZString containerMode,
				ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKDestination = "AUSYD";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var consignee = SetupConsignee("TESTIGNEE", "TEST CONSIGNEE ADDRESS");

			var seaLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			SetupBuyerLink(consignor, consignee, Constants.TransportModes.Sea, containerMode, seaLinkRelatedParty);

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "NZAKL";
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(seaLinkRelatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToLinkTransportMode_All(ZString transportMode, ZString transportType, ZString containerMode,
				ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKDestination = "AUSYD";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var consignee = SetupConsignee("TESTIGNEE", "TEST CONSIGNEE ADDRESS");

			var airLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var buyerLink = SetupBuyerLink(consignor, consignee, Constants.TransportModes.Air, containerMode, airLinkRelatedParty);

			var allLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var trnMode2 = buyerLink.OrgSupBuyLinkTrnModes.AddNew();
			trnMode2.PF_TransportMode = Constants.TransportModes.All;
			trnMode2.PF_ContainerMode = ZString.Empty;
			trnMode2.PF_OH_PickupCartageContractor = allLinkRelatedParty.PK;

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "NZAKL";
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(allLinkRelatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertPickupLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToRelatedParty(ZString transportMode, ZString transportType, ZString containerMode,
				ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKDestination = "AUSYD";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = SetupConsignorWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignor.Addresses.Count);
			var pickupAddress = consignor.Addresses[1];

			var consignee = SetupConsignee("TESTIGNEE", "TEST CONSIGNEE ADDRESS");

			var airLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			SetupBuyerLink(consignor, consignee, Constants.TransportModes.Air, containerMode, airLinkRelatedParty);

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			Assert(transport.JW_OA_DepartureLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKLoadPort = "NZAKL";
			AssertEquals(pickupAddress.PK, transport.JW_OA_DepartureLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_ForLinkTransportMode_Sea(ZString transportMode, ZString transportType, ZString containerMode,
				ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKOrigin = "NZAKL";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var consignor = SetupConsignor("TESTIGNOR", "TEST CONSIGNOR ADDRESS");

			var seaLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			SetupSupplierLink(consignor, consignee, Constants.TransportModes.Sea, containerMode, seaLinkRelatedParty);

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(seaLinkRelatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToLinkTransportMode_All(ZString transportMode, ZString transportType, ZString containerMode,
				ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKOrigin = "NZAKL";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var consignor = SetupConsignor("TESTIGNOR", "TEST CONSIGNOR ADDRESS");

			var airLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var supplierLink = SetupSupplierLink(consignor, consignee, Constants.TransportModes.Air, containerMode, airLinkRelatedParty);

			var allLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var trnMode2 = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			trnMode2.PF_TransportMode = Constants.TransportModes.All;
			trnMode2.PF_ContainerMode = ZString.Empty;
			trnMode2.PF_OH_DeliveryCartageContractor = allLinkRelatedParty.PK;

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(allLinkRelatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		void AssertDeliveryLeg_CarrierDefaulting_BuyerSupplierLinkRelatedParty_FallbackToRelatedParty(ZString transportMode, ZString transportType, ZString containerMode,
				ZString relatedPartyTransportMode, ZString relatedPartyContainerMode)
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_PackingMode = containerMode;
			shipment.JS_RL_NKOrigin = "NZAKL";

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = SetupConsigneeWithRelatedParty(relatedPartyTransportMode, relatedPartyContainerMode, relatedParty);
			AssertEquals(2, consignee.Addresses.Count);
			var deliveryAddress = consignee.Addresses[1];

			var consignor = SetupConsignor("TESTIGNOR", "TEST CONSIGNOR ADDRESS");

			var airLinkRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
			SetupSupplierLink(consignor, consignee, Constants.TransportModes.Air, containerMode, airLinkRelatedParty);

			var transport = SetupTransport(shipment, transportMode, transportType);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			Assert(transport.JW_OA_ArrivalLocation.IsEmpty);
			Assert(transport.JW_OA_CarrierAddress.IsEmpty);

			transport.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(deliveryAddress.PK, transport.JW_OA_ArrivalLocation);
			AssertEquals(relatedParty.MainAddress.PK, transport.JW_OA_CarrierAddress);
		}

		OrgSupplierBuyerLink SetupBuyerLink(OrgHeader consignor, OrgHeader consignee, ZString transportMode, ZString containerMode, OrgHeader linkRelatedParty)
		{
			var buyerLink = consignor.BuyerLinks.AddNew();
			buyerLink.OL_OH_Buyer = consignee.PK;

			buyerLink.OrgSupBuyLinkTrnModes.RemoveAll();
			var trnMode1 = buyerLink.OrgSupBuyLinkTrnModes.AddNew();
			trnMode1.PF_TransportMode = transportMode;
			trnMode1.PF_ContainerMode = containerMode;
			trnMode1.PF_OH_PickupCartageContractor = linkRelatedParty.PK;

			return buyerLink;
		}

		OrgSupplierBuyerLink SetupSupplierLink(OrgHeader consignor, OrgHeader consignee, ZString transportMode, ZString containerMode, OrgHeader linkRelatedParty)
		{
			var supplierLink = consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;

			supplierLink.OrgSupBuyLinkTrnModes.RemoveAll();
			var trnMode1 = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			trnMode1.PF_TransportMode = transportMode;
			trnMode1.PF_ContainerMode = containerMode;
			trnMode1.PF_OH_DeliveryCartageContractor = linkRelatedParty.PK;

			return supplierLink;
		}

		#endregion

		#endregion

		public void TestAddRulesToNotes()
		{
			var rule1 = Factory.NewWithValidTestData<RefCountryRules>();
			rule1.R7_RN_NKOrigin = "AU";
			rule1.R7_RN_NKDestination = "DE";
			rule1.R7_Notes = "This is a client visible Notes";
			rule1.R7_IsClientVisible = ZBool.True;

			var rule2 = Factory.NewWithValidTestData<RefCountryRules>();
			rule2.R7_RN_NKOrigin = "AU";
			rule2.R7_RN_NKDestination = "DE";
			rule2.R7_Notes = "This is an internal Notes";
			rule2.R7_IsClientVisible = ZBool.False;

			Factory.Save();

			var booking = Factory.New<AgencyShipment>();
			booking.JS_RL_NKOrigin = "AUBNE";
			booking.JS_RL_NKDestination = "DEHAM";

			Factory.Save();
			Assert(booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault().ST_NoteText.Contains("This is a client visible Notes"));
			Assert(booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is an internal Notes"));
		}

		#region AgencyShipmentExRateSource & ExRatePort

		public void TestExRateSourceWhenExRatePortIsEmpty()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "ITMIL";
			shipment.JS_RL_NKDestination = "SGSIN";

			var provider = (IJobInvoicingExRateSourceProvider)shipment;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageFlight = "001";

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "ITMIL";
			origin1.JA_E_DEP = ZDateTime.Now;

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "ITROM";
			origin2.JA_E_DEP = ZDateTime.Now.AddDays(1);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(5);

			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings[0].PK;

			var rate1 = voyage.ExRates.AddNew();
			rate1.E8_RX_NKExCurrency = "USD";
			rate1.E8_VoyageExchangeRate = 1.11m;

			var rate2 = voyage.ExRates.AddNew();
			rate2.E8_RX_NKExCurrency = "SGD";
			rate2.E8_VoyageExchangeRate = 1.66m;

			var rate3 = voyage.ExRates.AddNew();
			rate3.E8_RX_NKExCurrency = "USD";
			rate3.E8_VoyageExchangeRate = 1.22m;
			rate3.E8_RL_NKPort = "ITMIL";

			var rate4 = voyage.ExRates.AddNew();
			rate4.E8_RX_NKExCurrency = "SGD";
			rate4.E8_VoyageExchangeRate = 1.77;
			rate4.E8_RL_NKPort = "SGSIN";

			var rate5 = voyage.ExRates.AddNew();
			rate5.E8_RX_NKExCurrency = "AUD";
			rate5.E8_VoyageExchangeRate = 1.55m;
			rate5.E8_RL_NKPort = "ITROM";

			var rate6 = voyage.ExRates.AddNew();
			rate6.E8_RX_NKExCurrency = "EUR";
			rate6.E8_VoyageExchangeRate = 1.70m;

			var exRateSource = provider.GetExRateSource(ExRateSourceType.Voyage);
			AssertNotNull(exRateSource);
			AssertNotNull("Enumerator should not be null", exRateSource.GetEnumerator());
			AssertSequencesEqual("ExRatePort is empty and exRateSource should be sorted by currency.", new[] { rate5, rate6, rate2, rate1 }, exRateSource);

			AssertEquals("AUD exchange rate", 1.55m, exRateSource.GetExchangeRate("AUD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("EUR exchange rate", 1.70m, exRateSource.GetExchangeRate("EUR", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("SGD exchange rate", 1.66m, exRateSource.GetExchangeRate("SGD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("USD exchange rate", 1.11m, exRateSource.GetExchangeRate("USD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull("HKD exchange rate should be null", exRateSource.GetExchangeRate("HKD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull("Empty currency code should return null", exRateSource.GetExchangeRate(ZString.Empty, ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
		}

		public void TestExRateSourceWhenExRatePortIsOriginOrDestinationPort()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "ITMIL";
			shipment.JS_RL_NKDestination = "SGSIN";

			var provider = (IJobInvoicingExRateSourceProvider)shipment;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageFlight = "001";

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "ITMIL";
			origin1.JA_E_DEP = ZDateTime.Now;

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "ITROM";
			origin2.JA_E_DEP = ZDateTime.Now.AddDays(1);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(5);

			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings[0].PK;

			var rate1 = voyage.ExRates.AddNew();
			rate1.E8_RX_NKExCurrency = "USD";
			rate1.E8_VoyageExchangeRate = 1.11m;

			var rate2 = voyage.ExRates.AddNew();
			rate2.E8_RX_NKExCurrency = "SGD";
			rate2.E8_VoyageExchangeRate = 1.66m;

			var rate3 = voyage.ExRates.AddNew();
			rate3.E8_RX_NKExCurrency = "USD";
			rate3.E8_VoyageExchangeRate = 1.22m;
			rate3.E8_RL_NKPort = "ITMIL";

			var rate4 = voyage.ExRates.AddNew();
			rate4.E8_RX_NKExCurrency = "SGD";
			rate4.E8_VoyageExchangeRate = 1.77;
			rate4.E8_RL_NKPort = "SGSIN";

			var rate5 = voyage.ExRates.AddNew();
			rate5.E8_RX_NKExCurrency = "AUD";
			rate5.E8_VoyageExchangeRate = 1.55m;
			rate5.E8_RL_NKPort = "ITROM";

			var rate6 = voyage.ExRates.AddNew();
			rate6.E8_RX_NKExCurrency = "EUR";
			rate6.E8_VoyageExchangeRate = 1.70m;

			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;
			var exRateSource = provider.GetExRateSource(ExRateSourceType.Voyage);
			AssertNotNull(exRateSource);
			AssertNotNull("Enumerator should not be null", exRateSource.GetEnumerator());
			AssertSequencesEqual("ExRatePort should be ITMIL and exRateSource should be sorted by currency.", new[] { rate6, rate2, rate3 }, exRateSource);

			AssertNull("AUD exchange rate should be null", exRateSource.GetExchangeRate("AUD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("EUR exchange rate", 1.70m, exRateSource.GetExchangeRate("EUR", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("SGD exchange rate", 1.66m, exRateSource.GetExchangeRate("SGD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("USD exchange rate", 1.22m, exRateSource.GetExchangeRate("USD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull("HKD exchange rate should be null", exRateSource.GetExchangeRate("HKD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull("Empty currency code should return null", exRateSource.GetExchangeRate(ZString.Empty, ZGuid.Empty, ExchangeRateValidLedgerEnum.None));

			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;
			exRateSource = provider.GetExRateSource(ExRateSourceType.Voyage);
			AssertNotNull(exRateSource);
			AssertNotNull("Enumerator should not be null", exRateSource.GetEnumerator());
			AssertSequencesEqual("ExRatePort should be SGSIN and exRateSource should be sorted by currency.", new[] { rate6, rate4, rate1 }, exRateSource);

			AssertNull("AUD exchange rate should be null", exRateSource.GetExchangeRate("AUD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("EUR exchange rate", 1.70m, exRateSource.GetExchangeRate("EUR", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("SGD exchange rate", 1.77m, exRateSource.GetExchangeRate("SGD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertEquals("USD exchange rate", 1.11m, exRateSource.GetExchangeRate("USD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull("HKD exchange rate should be null", exRateSource.GetExchangeRate("HKD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
			AssertNull("Empty currency code should return null", exRateSource.GetExchangeRate(ZString.Empty, ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
		}

		#endregion

		#region Default JS_JX

		public void TestDefaultJS_JX_WhenMainLegChanged()
		{
			var shipment = Factory.New<AgencyShipment>();

			var sailing1 = CreateSailing("AUSYD", "USLAX", ZDateTime.Today);
			var sailing2 = CreateSailing("AUMEL", "HKHKG", ZDateTime.Today);

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			AssertEquals(ZGuid.Empty, shipment.JS_JX);
			AssertEquals(1, shipment.Transports.Count);

			transport1.JW_JX = sailing1.PK;
			AssertEquals(1, shipment.Transports.Count);
			AssertEquals(sailing1.PK, shipment.JS_JX);

			transport1.JW_JX = sailing2.PK;
			AssertEquals(1, shipment.Transports.Count);
			AssertEquals(sailing2.PK, shipment.JS_JX);

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportCodes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			AssertEquals(sailing2.PK, shipment.JS_JX);
			AssertEquals(2, shipment.Transports.Count);

			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			AssertEquals(sailing2.PK, shipment.JS_JX);
			AssertEquals(2, shipment.Transports.Count);
			AssertHasError(transport2.JW_TransportTypeInfo, "Can't have more than one MAI Transport Type");
		}

		public void TestDefaultJS_JX_WhenMainLegTypeChanged()
		{
			var sailing1 = CreateSailing("AUSYD", "USLAX", ZDateTime.Today);

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing1.PK;
			AssertEquals("Main leg is created", 1, shipment.Transports.Count);

			shipment.Transports[0].JW_TransportType = Constants.TransportPlanningType.Other;
			AssertEquals(ZGuid.Empty, shipment.JS_JX);
			AssertEquals("Transport should not be removed when type is changed from main to other", 1, shipment.Transports.Count);
		}

		public void TestDefaultJS_JX_ShouldKeepJS_JX()
		{
			var sailing1 = CreateSailing("AUSYD", "USLAX", ZDateTime.Today);

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = sailing1.PK;
			AssertEquals("Main leg is created", 1, shipment.Transports.Count);
			AssertEquals("JS_JX should be kept unchanged", sailing1.PK, shipment.JS_JX);
		}

		public void TestDefaultJS_JX_WhenTransportsCountChanged()
		{
			var shipment = Factory.New<AgencyShipment>();

			var sailing1 = CreateSailing("AUSYD", "USLAX", ZDateTime.Today);

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport1.JW_JX = sailing1.PK;
			AssertEquals(1, shipment.Transports.Count);
			AssertEquals(sailing1.PK, shipment.JS_JX);

			shipment.Transports.RemoveAndDeleteAll();
			AssertEquals(0, shipment.Transports.Count);
			Assert(shipment.JS_JX.IsEmpty);
		}

		public void TestShouldEnforceAllocations_WhenJW_IsLinkedIsChecked()
		{
			AssertShouldEnforceAllocations(true);
			AssertShouldEnforceAllocations(false);

			void AssertShouldEnforceAllocations(bool useGlobalAllocations)
			{
				using (FreightConfigurationRegistry.Instance.UseGlobalAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useGlobalAllocations))
				{
					var shipment = Factory.New<AgencyShipment>();

					var transport1 = shipment.Transports.AddNew();
					transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
					transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
					transport1.JW_RL_NKLoadPort = "AUSYD";
					transport1.JW_RL_NKDiscPort = "SGSIN";
					transport1.JW_Vessel = "ADELAIDE EXPRESS";
					transport1.JW_VoyageFlight = "123W";
					transport1.JW_ETD = ZDate.Today;
					AssertEquals(1, shipment.Transports.Count);
					AssertNull(shipment.Sailing);
					AssertEquals(false, shipment.ShouldEnforceAllocations);

					transport1.JW_IsLinked = true;
					AssertNotNull(shipment.Sailing);
					AssertEquals(false, shipment.ShouldEnforceAllocations);
				}
			}
		}

		public void TestNoException_WhenJW_IsLinkedIsUnchecked()
		{
			var shipment = Factory.New<AgencyShipment>();

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_Vessel = "ADELAIDE EXPRESS";
			transport1.JW_VoyageFlight = "123W";
			transport1.JW_ETD = ZDate.Today;
			transport1.JW_IsLinked = true;
			AssertNotNull(shipment.Sailing);

			Factory.Save();

			AssertNoExceptionThrown(() => transport1.JW_IsLinked = false);
			AssertEquals(1, shipment.Transports.Count);
			Assert("Main transport should not be deleted", !transport1.IsDeleted);
		}

		#endregion

		#region Implementation

		JobVoyage Voyage;
		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;
		JobSailing Sailing1;
		JobSailing Sailing2;
		JobSailing Sailing3;
		JobSailing Sailing4;

		void AssertSendingAgent(AgencyShipment shipment, OrgAddress expectedAddress)
		{
			ZGuid addrPK = expectedAddress == null ? ZGuid.Empty : expectedAddress.PK;
			ZGuid orgPK = expectedAddress == null ? ZGuid.Empty : expectedAddress.Header.PK;

			AssertEquals(expectedAddress, shipment.SendingAgentAddress);
			AssertEquals(addrPK, shipment.SendingAgentAddressPK);
			AssertEquals(addrPK, shipment.SendingAgentAddressPK_ZAddress.AddressFK);
			AssertEquals(orgPK, shipment.SendingAgentAddressPK_ZAddress.OrgPK);
		}

		void AssertReceivingAgent(AgencyShipment shipment, OrgAddress expectedAddress)
		{
			ZGuid addrPK = expectedAddress == null ? ZGuid.Empty : expectedAddress.PK;
			ZGuid orgPK = expectedAddress == null ? ZGuid.Empty : expectedAddress.Header == null ? ZGuid.Empty : expectedAddress.Header.PK;

			AssertEquals(expectedAddress, shipment.ReceivingAgentAddress);
			AssertEquals(addrPK, shipment.ReceivingAgentAddressPK);
			AssertEquals(addrPK, shipment.ReceivingAgentAddressPK_ZAddress.AddressFK);
			AssertEquals(orgPK, shipment.ReceivingAgentAddressPK_ZAddress.OrgPK);
		}

		void SetupSailing(bool countryAllocation, bool originAllocation, bool sailingAllocation)
		{
			Voyage = Factory.New<JobVoyage>();

			Origin1 = Voyage.Origins.AddNew();
			Origin1.JA_RL_NKPortOfLoading = "AUBNE";
			Origin1.JA_E_DEP = ZDateTime.Now.AddDays(10);

			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUDRW";
			Origin2.JA_E_DEP = ZDateTime.Now.AddDays(16);

			Origin3 = Voyage.Origins.AddNew();
			Origin3.JA_RL_NKPortOfLoading = "SGSIN";
			Origin3.JA_E_DEP = ZDateTime.Now.AddDays(22);

			VoyageDestination destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUDRW";
			destination1.JB_E_ARV = ZDateTime.Now.AddDays(14);

			VoyageDestination destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = ZDateTime.Now.AddDays(20);

			VoyageDestination destination3 = Voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "MYBAG";
			destination3.JB_E_ARV = ZDateTime.Now.AddDays(26);

			Sailing1 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			Sailing2 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin1.JA_RL_NKPortOfLoading, destination1.JB_RL_NKPortOfDischarge);
			Sailing3 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin2.JA_RL_NKPortOfLoading, destination2.JB_RL_NKPortOfDischarge);
			Sailing4 = Voyage.Sailings.GetSailingFromLoadAndDischarge(Origin3.JA_RL_NKPortOfLoading, destination3.JB_RL_NKPortOfDischarge);

			string allocationMethod = AllocationMethodList.Codes.NotSet;
			if (countryAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Country;
				foreach (VoyageCountry country in new VoyageCountry[] { Origin1.VoyageCountry, Origin3.VoyageCountry })
				{
					SlotAllocation allocation = country.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 80);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 240);
				}
			}

			if (originAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Origin;
				foreach (VoyageOrigin origin in new VoyageOrigin[] { Origin1, Origin2, Origin3 })
				{
					SlotAllocation allocation = origin.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 80);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 240);
				}
			}

			if (sailingAllocation)
			{
				allocationMethod = AllocationMethodList.Codes.Sailing;
				foreach (JobSailing sailing in new JobSailing[] { Sailing1, Sailing2, Sailing3, Sailing4 })
				{
					SlotAllocation allocation = sailing.SlotAllocations.GetAllocation(ZGuid.Empty);
					allocation.SetAspect(AllocationAspectTypes.TEU, 40);
					allocation.SetAspect(AllocationAspectTypes.Tonnes, 160);
				}
			}

			foreach (VoyageCountry country in new VoyageCountry[] { Origin1.VoyageCountry, Origin3.VoyageCountry })
			{
				country.J0_AllocationMethod = allocationMethod;
			}
		}

		JobCharge NewCharge(JobHeader job)
		{
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			if (job != null)
			{
				charge.JR_JH = job.PK;
			}
			AccTransactionLines lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_ARLine = lines.PK;
			lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_APLine = lines.PK;

			return charge;
		}

		void SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail, bool fountain)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			element.Fountain = fountain;
		}

		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		#endregion

		#region IsDPSFreightMovementRestricted

		public void TestIsDPSFreightMovementRestricted()
		{
			var shipment = Factory.New<AgencyShipment>();
			AssertEquals(false, ((ICreditControlledDocumentDelivery)shipment).IsDPSFreightMovementRestricted);

			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			using (LinerAgencyDataRegistry.Instance.LinerAgencyEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				AssertEquals(false, ((ICreditControlledDocumentDelivery)shipment).IsDPSFreightMovementRestricted);

				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
				complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
				AssertEquals(true, ((ICreditControlledDocumentDelivery)shipment).IsDPSFreightMovementRestricted);
			}
			AssertEquals(false, ((ICreditControlledDocumentDelivery)shipment).IsDPSFreightMovementRestricted);
		}

		#endregion

		public void TestEventsFired()
		{
			bool onShowMessageOnGUIIsFired = false;

			var shipment = Factory.New<AgencyShipment>();
			shipment.OnShowMessageOnGUI += (s, e) => onShowMessageOnGUIIsFired = true;
			shipment.ShowMessageOnGUI("aa", "bb");
			Assert("onShowMessageOnGUIIsFired", onShowMessageOnGUIIsFired);
		}
	}
}
