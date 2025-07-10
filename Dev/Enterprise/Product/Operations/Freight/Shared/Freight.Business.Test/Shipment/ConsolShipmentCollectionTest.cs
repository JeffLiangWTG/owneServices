using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class ConsolShipmentCollectionTest : BaseFreightTest
	{
		#region Consol Shipments Limit

		public void TestConsolLimitNotificationsUpdatedOnDataRefresh()
		{
			const string expectedError = @"The number of Shipments on a Consol is limited for performance and database management reasons to 4 Shipments. Above 2 Shipments you will receive this message for every additional Shipment added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			using (FreightDataRegistry.Instance.ShipmentsPerConsolLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (FreightDataRegistry.Instance.ShipmentsPerConsolLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var consol = Factory.New<CommonConsol>();
				consol.Shipments.AddNew();
				consol.Shipments.AddNew();
				consol.Shipments.AddNew();

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consolInNewFactory = newFactory.Load<CommonConsol>(consol.PK);
				var newShipmentInNewFactory = newFactory.New<CommonShipment>();
				newShipmentInNewFactory.Consols.Add(consolInNewFactory);

				consol.Shipments.AddNew();

				Factory.Save();

				AssertHasRowError(consolInNewFactory, expectedError);
			}
		}

		#endregion

		#region Sub Shipment Adding / Removing

		public void TestSubShipmentsAddedRemovedFromConsol()
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("BCN", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var sub1 = FreightTestHelper.GetShipment("SUB_1", shipment, Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var sub2 = FreightTestHelper.GetShipment("SUB_2", shipment, Constants.ShipmentTypes.BuyersConsolLead, Factory);

			consol.Shipments.Add(shipment);
			FreightTestHelper.AssertShipmentCollection(consol.Shipments, shipment, sub1, sub2);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol);
			FreightTestHelper.AssertConsolCollection(sub1.Consols, consol);
			FreightTestHelper.AssertConsolCollection(sub2.Consols, consol);

			consol.Shipments.Remove(sub2);
			FreightTestHelper.AssertShipmentCollection(consol.Shipments, shipment, sub1);

			consol.Shipments.Remove(shipment);
			FreightTestHelper.AssertShipmentCollection(consol.Shipments, sub1);
			shipment.Consols.Load();
			sub1.Consols.Load();
			sub2.Consols.Load();
			FreightTestHelper.AssertConsolCollection(shipment.Consols);
			FreightTestHelper.AssertConsolCollection(sub1.Consols, consol);
			FreightTestHelper.AssertConsolCollection(sub2.Consols);
		}

		public void TestRemovedShipments_DeleteUnsaved()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("BCN", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			consol.Shipments.Add(shipment);
			consol.Shipments.Remove(shipment);
			AssertEquals(true, shipment.IsDeleted);
		}

		#endregion

		#region Update ETA/ETD

		[ExpectNoExceptions]
		public void TestNoOutOfIndexExceptionWhenUpdateShipmentsETA()
		{
			var today = ZDateTime.Today;
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "Shipment5";
			shipment.JS_E_DEP = new ZDateTime(2079, 5, 6);
			shipment.JS_E_ARV = new ZDateTime(2079, 5, 7);
			consol.Shipments.Add(shipment);
			consol.Shipments.UpdateShipmentsETA(today.AddMonths(2), today);
			Factory.Save();

			AssertEquals(shipment.JS_E_ARV, new ZDateTime(2079, 5, 7));
		}

		public void TestUpdateShipmentsETD()
		{
			ZDateTime today = ZDateTime.Today;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;
			transport1.JW_ETD = today.AddDays(10);
			transport1.JW_ETA = today.AddDays(20);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = OverseasPort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;
			transport2.JW_ETD = today.AddDays(25);
			transport2.JW_ETA = today.AddDays(35);

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_E_DEP = today.AddDays(5);
			shipment1.JS_E_ARV = today.AddDays(40);

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			shipment2.Consols.Add(consol2);
			shipment2.JS_E_DEP = today.AddDays(20);
			shipment2.JS_E_ARV = today.AddDays(41);

			var shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";
			shipment3.Consols.Add(consol1);
			shipment3.JS_E_DEP = today.AddDays(4);
			shipment3.JS_E_ARV = today.AddDays(25);

			var shipment4 = Factory.New<CommonShipment>();
			shipment4.JS_UniqueConsignRef = "Shipment4";
			shipment4.Consols.Add(consol1);
			shipment4.JS_E_DEP = ZDateTime.Empty;
			shipment4.JS_E_ARV = ZDateTime.Empty;

			AssertEquals("Information text", "Shipment1 ETD updated\r\nShipment3 ETD updated\r\nShipment4 ETD updated", consol1.Shipments.UpdateShipmentsETD(today.AddDays(4), today));
			AssertEquals("shipment1's etd should have been pushed back 4 days", today.AddDays(9), shipment1.JS_E_DEP);
			AssertEquals("shipment2 is not on consol1 so should have stayed the same", today.AddDays(20), shipment2.JS_E_DEP);
			AssertEquals("shipment3's etd should have been pushed back 4 days", today.AddDays(8), shipment3.JS_E_DEP);
			AssertEquals("shipment4's etd should have been updated to new date", today.AddDays(4), shipment4.JS_E_DEP);

			AssertEquals("Information text", "Shipment2 ETD updated", consol2.Shipments.UpdateShipmentsETD(today, today.AddDays(2)));
			AssertEquals("shipment1 has a different departure consol so should have stayed the same", today.AddDays(9), shipment1.JS_E_DEP);
			AssertEquals("shipment2's etd should have been brought forward 2 days", today.AddDays(18), shipment2.JS_E_DEP);
			AssertEquals("shipment3 is not on consol2 so should have stayed the same", today.AddDays(8), shipment3.JS_E_DEP);
			AssertEquals("shipment4 is not on consol2 so should have stayed the same", today.AddDays(4), shipment4.JS_E_DEP);
		}

		public void TestUpdateShipmentsETDViaTransport()
		{
			ZDateTime today = ZDateTime.Today;

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;
			transport1.JW_ETD = today.AddDays(10);
			transport1.JW_ETA = today.AddDays(20);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = OverseasPort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;
			transport2.JW_ETD = today.AddDays(25);
			transport2.JW_ETA = today.AddDays(35);

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_E_DEP = today.AddDays(5);
			shipment1.JS_E_ARV = today.AddDays(40);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.Consols.Add(consol2);
			shipment2.JS_E_DEP = today.AddDays(20);
			shipment2.JS_E_ARV = today.AddDays(41);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.Consols.Add(consol1);
			shipment3.JS_E_DEP = today.AddDays(4);
			shipment3.JS_E_ARV = today.AddDays(25);

			transport1.JW_ETD = today.AddDays(14);
			AssertEquals("shipment1's etd should stay the same as it's still less than the new date", today.AddDays(5), shipment1.JS_E_DEP);
			AssertEquals("shipment2 is not on consol1 so should have stayed the same", today.AddDays(20), shipment2.JS_E_DEP);
			AssertEquals("shipment3's etd should stay the same too", today.AddDays(4), shipment3.JS_E_DEP);

			transport2.JW_ETD = today.AddDays(23);
			AssertEquals("shipment1 has a different departure consol so should have stayed the same", today.AddDays(5), shipment1.JS_E_DEP);
			AssertEquals("shipment2's etd should have been brought forward 2 days", today.AddDays(20), shipment2.JS_E_DEP);
			AssertEquals("shipment3 is not on consol2 so should have stayed the same", today.AddDays(4), shipment3.JS_E_DEP);

			transport1.JW_ETD = today.AddDays(3);
			AssertEquals("shipment1's etd should stayed the same as it's still less than the new date", today.AddDays(3), shipment1.JS_E_DEP);
			AssertEquals("shipment2 is not on consol1 so should have stayed the same", today.AddDays(20), shipment2.JS_E_DEP);
			AssertEquals("shipment3's etd should stayed the same as it's still less than the new date", today.AddDays(3), shipment3.JS_E_DEP);

			transport2.JW_ETD = today.AddDays(18);
			AssertEquals("shipment1 has a different departure consol so should have stayed the same", today.AddDays(3), shipment1.JS_E_DEP);
			AssertEquals("shipment2's etd should stayed the same as it's still less than the new date", today.AddDays(18), shipment2.JS_E_DEP);
			AssertEquals("shipment3 is not on consol2 so should have stayed the same", today.AddDays(3), shipment3.JS_E_DEP);
		}

		public void TestUpdateShipmentsETA()
		{
			var today = ZDateTime.Today;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;
			transport1.JW_ETD = today.AddDays(10);
			transport1.JW_ETA = today.AddDays(20);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = OverseasPort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;
			transport2.JW_ETD = today.AddDays(25);
			transport2.JW_ETA = today.AddDays(35);

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_E_DEP = today.AddDays(5);
			shipment1.JS_E_ARV = today.AddDays(40);

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			shipment2.Consols.Add(consol2);
			shipment2.JS_E_DEP = today.AddDays(20);
			shipment2.JS_E_ARV = today.AddDays(41);

			var shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";
			shipment3.Consols.Add(consol1);
			shipment3.JS_E_DEP = today.AddDays(4);
			shipment3.JS_E_ARV = today.AddDays(25);

			var shipment4 = Factory.New<CommonShipment>();
			shipment4.JS_UniqueConsignRef = "Shipment4";
			shipment4.Consols.Add(consol1);
			shipment4.JS_E_DEP = ZDateTime.Empty;
			shipment4.JS_E_ARV = ZDateTime.Empty;

			AssertEquals("Information text", "Shipment3 ETA updated\r\nShipment4 ETA updated", consol1.Shipments.UpdateShipmentsETA(today, today.AddDays(2)));
			AssertEquals("shipment1 has a different arrival consol so should have stayed the same", 40, (shipment1.JS_E_ARV - today).Days);
			AssertEquals("shipment2 is not on consol1 so should have stayed the same", 41, (shipment2.JS_E_ARV - today).Days);
			AssertEquals("shipment3's eta should have been brought forward 2 days", 23, (shipment3.JS_E_ARV - today).Days);
			AssertEquals("shipment4's eta should have been updated to new date", 0, (shipment4.JS_E_ARV - today).Days);

			AssertEquals("Information text", "Shipment1 ETA updated\r\nShipment2 ETA updated", consol2.Shipments.UpdateShipmentsETA(today.AddDays(4), today));
			AssertEquals("shipment1's eta should have been pushed back 4 days", 44, (shipment1.JS_E_ARV - today).Days);
			AssertEquals("shipment2's eta should have been pushed back 4 days", 45, (shipment2.JS_E_ARV - today).Days);
			AssertEquals("shipment3 is not on consol2 so should have stayed the same", 23, (shipment3.JS_E_ARV - today).Days);
			AssertEquals("shipment4 is not on consol2 so should have stayed the same", 0, (shipment4.JS_E_ARV - today).Days);
		}

		public void TestUpdateShipmentsETDAndETAFromScheduleWithMultipleLegs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VesselX";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VoyageX";

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";
			origin1.JA_E_DEP = ZDate.Today.AddDays(1);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";
			destination1.JB_E_ARV = ZDate.Today.AddDays(2);

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";
			origin2.JA_E_DEP = ZDate.Today.AddDays(3);

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "MYKUL";
			destination2.JB_E_ARV = ZDate.Today.AddDays(4);

			voyage.GenerateSailings();

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_RL_NKOrigin = "AUBNE";
			shipment1.JS_RL_NKDestination = "MYKUL";

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_RL_NKOrigin = "AUBNE";
			shipment2.JS_RL_NKDestination = "MYKUL";
			shipment2.JS_E_DEP = ZDate.Today.AddDays(2);
			shipment2.JS_E_ARV = ZDate.Today.AddDays(3);

			var shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_RL_NKOrigin = "AUBNE";
			shipment3.JS_RL_NKDestination = "MYKUL";
			shipment3.JS_E_DEP = ZDate.Today;
			shipment3.JS_E_ARV = ZDate.Today.AddDays(5);

			var consol = Factory.New<CommonConsol>();
			consol.Shipments.AddRange(shipment1, shipment2, shipment3);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "SEA";
			transport1.JW_JX = voyage.Sailings.Cast<JobSailing>().First(s => s.Origin.JA_RL_NKPortOfLoading == "AUBNE" && s.Destination.JB_RL_NKPortOfDischarge == "NZAKL").PK;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_JX = voyage.Sailings.Cast<JobSailing>().First(s => s.Origin.JA_RL_NKPortOfLoading == "NZAKL" && s.Destination.JB_RL_NKPortOfDischarge == "MYKUL").PK;

			AssertEquals("Blank ETD should be updated", ZDate.Today.AddDays(1), shipment1.JS_E_DEP);
			AssertEquals("Blank ETA should be updated", ZDate.Today.AddDays(4), shipment1.JS_E_ARV);

			AssertEquals("ETD after departure transport should be updated", ZDate.Today.AddDays(1), shipment2.JS_E_DEP);
			AssertEquals("ETA before arrival transport should be updated", ZDate.Today.AddDays(4), shipment2.JS_E_ARV);

			AssertEquals("ETD before departure transport should not be updated", ZDate.Today, shipment3.JS_E_DEP);
			AssertEquals("ETA after arrival transport should not be updated", ZDate.Today.AddDays(5), shipment3.JS_E_ARV);
		}

		public void TestUpdateShipmentsETAViaTransport()
		{
			ZDateTime today = ZDateTime.Today;

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;
			transport1.JW_ETD = today.AddDays(10);
			transport1.JW_ETA = today.AddDays(20);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = OverseasPort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;
			transport2.JW_ETD = today.AddDays(25);
			transport2.JW_ETA = today.AddDays(35);

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);
			shipment1.Consols.Add(consol2);
			shipment1.JS_E_DEP = today.AddDays(5);
			shipment1.JS_E_ARV = today.AddDays(40);

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";
			shipment2.Consols.Add(consol2);
			shipment2.JS_E_DEP = today.AddDays(20);
			shipment2.JS_E_ARV = today.AddDays(41);

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";
			shipment3.Consols.Add(consol1);
			shipment3.JS_E_DEP = today.AddDays(4);
			shipment3.JS_E_ARV = today.AddDays(25);

			transport1.JW_ETA = today.AddDays(18);
			AssertEquals("shipment1 has a different arrival consol so should have stayed the same", 40, (shipment1.JS_E_ARV - today).Days);
			AssertEquals("shipment2 is not on consol1 so should have stayed the same", 41, (shipment2.JS_E_ARV - today).Days);
			AssertEquals("shipment3's eta be the same as it's greater than the new date", 25, (shipment3.JS_E_ARV - today).Days);

			transport2.JW_ETA = today.AddDays(39);
			AssertEquals("shipment1's eta should have been pushed back 4 days", 40, (shipment1.JS_E_ARV - today).Days);
			AssertEquals("shipment2's eta should have been pushed back 4 days", 41, (shipment2.JS_E_ARV - today).Days);
			AssertEquals("shipment3 is not on consol2 so should have stayed the same", 25, (shipment3.JS_E_ARV - today).Days);

			transport1.JW_ETA = today.AddDays(27);
			AssertEquals("shipment1 has a different arrival consol so should have stayed the same", 40, (shipment1.JS_E_ARV - today).Days);
			AssertEquals("shipment2 is not on consol1 so should have stayed the same", 41, (shipment2.JS_E_ARV - today).Days);
			AssertEquals("shipment3's eta should change as it's less than the new date", 27, (shipment3.JS_E_ARV - today).Days);

			transport2.JW_ETA = today.AddDays(45);
			AssertEquals("shipment1's eta should change as it's less than the new date", 45, (shipment1.JS_E_ARV - today).Days);
			AssertEquals("shipment2's eta should change as it's less than the new date", 45, (shipment2.JS_E_ARV - today).Days);
			AssertEquals("shipment3 is not on consol2 so should have stayed the same", 27, (shipment3.JS_E_ARV - today).Days);
		}

		public void TestUpdateShipmentsETDETAIfEmpty()
		{
			ZDateTime today = ZDateTime.Today;

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.Consols.Add(consol1);

			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;
			transport1.JW_ETD = today.AddDays(10);
			transport1.JW_ETA = today.AddDays(20);

			AssertEquals("shipment etd should be set", today.AddDays(10), shipment1.JS_E_DEP);
			AssertEquals("shipment eta should be set", today.AddDays(20), shipment1.JS_E_ARV);
		}

		public void TestArrivalDepartureChangesPropagateCorrectlyToAsmShipment()
		{
			var startTime = new ZDateTime(2019, 11, 04);
			var endTime = new ZDateTime(2019, 12, 19);

			var newStartTime = new ZDateTime(2019, 11, 03);
			var newEndTime = new ZDateTime(2019, 12, 20);
			var secondEndTimeChange = new ZDateTime(2019, 12, 21);

			Func<CommonShipment, CommonShipment> setInitialShipmentFieldsForAsmShipmentTest = shpmnt =>
			{
				shpmnt.JS_RL_NKOrigin = "AUSYD";
				shpmnt.JS_RL_NKDestination = "SGSIN";
				shpmnt.JS_E_DEP = startTime;
				shpmnt.JS_E_ARV = endTime;
				return shpmnt;
			};

			Action<CommonConsol, CommonShipment, CommonShipment> assertionHelper = (consol, asm, child) =>
			{
				AssertEquals("Precondition", child.JS_E_DEP, asm.JS_E_DEP);
				AssertEquals("Precondition", child.JS_E_ARV, asm.JS_E_ARV);

				consol.Shipments.UpdateShipmentsETD(newStartTime, startTime);
				CombineAssertions(() =>
				{
					AssertEquals("ASM shipment should have the same ETD date as the coload shipment", child.JS_E_DEP, asm.JS_E_DEP);
					AssertEquals("ETD should propagate to ASM shipment correctly", newStartTime, asm.JS_E_DEP);
				});

				consol.Shipments.UpdateShipmentsETA(newEndTime, endTime);
				CombineAssertions(() =>
				{
					AssertEquals("ASM shipment should have the same ETA date as the coload shipment", child.JS_E_ARV, asm.JS_E_ARV);
					AssertEquals("ETA should propagate to ASM shipment correctly", newEndTime, asm.JS_E_ARV);
				});

				consol.Shipments.UpdateShipmentsETA(secondEndTimeChange, newEndTime);
				CombineAssertions(() =>
				{
					AssertEquals("ASM shipment should have the same ETA date as the coload shipment after multiple updates", child.JS_E_ARV, asm.JS_E_ARV);
					AssertEquals("ETA should propagate to ASM shipment correctly after multiple updates", secondEndTimeChange, asm.JS_E_ARV);
				});
			};

			var consolAsmChild = Factory.NewWithValidTestData<CommonConsol>();
			consolAsmChild.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolAsmChild.JK_AgentType = Constants.AgentType.CoLoad;
			var shipmentAsm = setInitialShipmentFieldsForAsmShipmentTest(consolAsmChild.Shipments.AddNew());
			var shipment = setInitialShipmentFieldsForAsmShipmentTest(consolAsmChild.Shipments.AddNew());
			shipment.JS_JS_ColoadMasterShipment = shipmentAsm.PK;
			assertionHelper(consolAsmChild, shipmentAsm, shipment);

			var consolChildAsm = Factory.NewWithValidTestData<CommonConsol>();
			consolChildAsm.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolChildAsm.JK_AgentType = Constants.AgentType.CoLoad;
			var shipment2 = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsm.Shipments.AddNew());
			var shipmentAsm2 = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsm.Shipments.AddNew());
			shipment2.JS_JS_ColoadMasterShipment = shipmentAsm2.PK;
			assertionHelper(consolChildAsm, shipmentAsm2, shipment2);

			var consolChildAsmChild = Factory.NewWithValidTestData<CommonConsol>();
			consolChildAsmChild.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolChildAsmChild.JK_AgentType = Constants.AgentType.CoLoad;
			var child1 = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsmChild.Shipments.AddNew());
			var asm1 = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsmChild.Shipments.AddNew());
			var child2 = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsmChild.Shipments.AddNew());
			child1.JS_JS_ColoadMasterShipment = asm1.PK;
			child2.JS_JS_ColoadMasterShipment = asm1.PK;
			assertionHelper(consolChildAsmChild, asm1, child1);

			var consolChildAsmAsm = Factory.NewWithValidTestData<CommonConsol>();
			consolChildAsmAsm.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolChildAsmAsm.JK_AgentType = Constants.AgentType.CoLoad;
			var littleChild = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsmAsm.Shipments.AddNew());
			var middleAsm = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsmAsm.Shipments.AddNew());
			var masterAsm = setInitialShipmentFieldsForAsmShipmentTest(consolChildAsmAsm.Shipments.AddNew());
			littleChild.JS_JS_ColoadMasterShipment = middleAsm.PK;
			middleAsm.JS_JS_ColoadMasterShipment = masterAsm.PK;
			assertionHelper(consolChildAsmAsm, masterAsm, littleChild);
		}

		#endregion

		#region Default Values

		public void TestShipmentDefaults()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			Consol.JK_RL_NKLoadPort = "L0001";
			Consol.JK_RL_NKDischargePort = "D0002";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "L0001";
			transport1.JW_RL_NKDiscPort = "D0001";

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "L0002";
			transport2.JW_RL_NKDiscPort = "D0002";

			ConsolShipmentCollection shipmentCollection = new ConsolShipmentCollection(Consol);
			shipmentCollection.AddNew();

			Assert("CommonShipment object added", shipmentCollection.Count == 1);
			CommonShipment shipment = shipmentCollection[0];

			AssertEquals("Default Transport Mode from Consol", Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
			AssertEquals("Default Packing Mode from Consol", Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			AssertEquals("Default Origin from Consol", "L0001", shipment.JS_RL_NKOrigin);
			AssertEquals("Default Destination from Consol", "D0002", shipment.JS_RL_NKDestination);
		}

		#endregion

		#region Default Outer Pack Line

		public void TestAddDefaultOuterPackLineIfThereAreContainers()
		{
			CommonContainer container1 = Factory.New<CommonContainer>();
			Consol.Containers.Add(container1);

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 2;
			AssertEquals("Consol should be added to CommonShipment", Consol, shipment.Consols[0]);
			AssertEquals("Default OuterPackLines", 1, shipment.OuterPackLines.Count);
			AssertEquals("Container on default outer packline", container1, shipment.OuterPackLines[0].GetContainer(Consol));
		}

		#endregion

		#region Consol / Shipments Collection

		public void TestShipmentGetsAddedIntoConsolShipmentsCollection()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals("Consol count", 0, shipment.Consols.Count);

			CommonConsol consol = shipment.Consols.AddNew();
			AssertEquals("Consol count", 1, shipment.Consols.Count);

			AssertEquals("Consol in Shipment.Consols", consol, shipment.Consols[0]);
			AssertEquals("Shipment in Consol.Shipments", shipment, consol.Shipments[0]);

			AssertEquals("Shipments can be added manually", true, consol.Shipments.AllowAddNew);
		}

		#endregion

		#region Matching Related Parties

		public void TestCheckNonMatchingReceivingAgentWithShipmentRelatedParties()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			var shipment2 = Factory.NewWithValidTestData<CommonShipment>();

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var warningMessage = "message";
				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(warningMessage);
				consol.Shipments.Add(shipment1);
				AssertHasRowWarningContaining(shipment1, warningMessage);
				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
				consol.Shipments.Add(shipment2);
				AssertNoRowWarningContaining(shipment2, "message");
			}
		}

		public void TestCheckNonMatchingSendingAgentWithShipmentRelatedParties()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			var shipment2 = Factory.NewWithValidTestData<CommonShipment>();

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var warningMessage = "message";
				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(warningMessage);
				consol.Shipments.Add(shipment1);
				AssertHasRowWarningContaining(shipment1, warningMessage);
				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
				consol.Shipments.Add(shipment2);
				AssertNoRowWarningContaining(shipment2, "message");
			}
		}

		#endregion

		#region Duplicate Load/Discharge

		public void TestCheckDuplicateLoadingAndDischarge()
		{
			string origin1 = "O1111";
			string origin2 = "O2222";
			string destination1 = "D1111";
			string destination2 = "D2222";

			CommonShipment shipment = Factory.New<CommonShipment>();

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKDischargePort = destination1;
			consol1.JK_RL_NKLoadPort = origin1;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKDiscPort = destination1;
			transport1.JW_RL_NKLoadPort = origin1;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = destination2;
			consol2.JK_RL_NKLoadPort = origin2;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKDiscPort = destination2;
			transport2.JW_RL_NKLoadPort = origin2;

			CommonConsol consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol3.JK_RL_NKDischargePort = destination1;
			consol3.JK_RL_NKLoadPort = origin1;
			Transport transport3 = consol3.Transports[0];
			transport3.JW_RL_NKDiscPort = destination1;
			transport3.JW_RL_NKLoadPort = origin1;

			consol1.Shipments.Add(shipment);
			Assert("Should be no errors", !consol1.Shipments[0].HasRowErrors);

			consol2.Shipments.Add(shipment);
			shipment.ClearRowNotifications();
			Assert("Different Loading and Discharge", !consol2.Shipments[0].HasRowErrors);

			consol3.Shipments.Add(shipment);
			Assert("Duplicate Loading and Discharge", consol3.Shipments[0].HasRowErrors);
		}

		#endregion

		#region Shipment Totals

		public void TestUpdateShipmentTotals()
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			CommonShipment shipment2 = Factory.New<CommonShipment>();

			shipment1.JS_OuterPacks = 1;
			shipment1.JS_ActualWeight = 10m;
			shipment1.JS_ActualVolume = .1m;

			shipment2.JS_OuterPacks = 2;
			shipment2.JS_ActualWeight = 20m;
			shipment2.JS_ActualVolume = .2m;

			Consol.Shipments.Add(shipment1);
			AssertEquals("Add CommonShipment, update Qty.", 1m, Consol.JK_TotalShipmentQuantity);
			AssertEquals("Add CommonShipment, update Weight.", 10m, Consol.JK_TotalShipmentWeight);
			AssertEquals("Add CommonShipment, update Volumn.", .1m, Consol.JK_TotalShipmentVolume);

			Consol.Shipments.Add(shipment2);
			AssertEquals("Add another CommonShipment, update Qty.", 3m,

				Consol.JK_TotalShipmentQuantity);
			AssertEquals("Add another CommonShipment, update Weight.", 30m,

				Consol.JK_TotalShipmentWeight);
			AssertEquals("Add another CommonShipment, update Volumn.", .3m,

				Consol.JK_TotalShipmentVolume);

			Consol.Shipments.Remove(shipment1);
			AssertEquals("Remove CommonShipment, update Qty.", 2m, Consol.JK_TotalShipmentQuantity);
			AssertEquals("Remove CommonShipment, update Weight.", 20m, Consol.JK_TotalShipmentWeight);
			AssertEquals("Remove CommonShipment, update Volumn.", .2m, Consol.JK_TotalShipmentVolume);
		}

		#endregion

		#region Remove

		public void TestRemove()
		{
			CommonShipment savedShipment = Factory.New<CommonShipment>();
			Factory.Save();
			Consol.Shipments.Add(savedShipment);
			CommonShipment newShipment = Consol.Shipments.AddNew();
			Consol.Shipments.Remove(newShipment);
			Factory.Save();

			BusinessObject findNewShipment = Factory.Load(typeof(CommonShipment), newShipment.PK);
			AssertNull("Detach unsaved shipment. CommonShipmentshould be deleted.", findNewShipment);
		}

		public void TestATCEventAddedWhenShipmentAttachedToConsol()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			Factory.Save();

			CommonShipment shipment2 = Factory.New<CommonShipment>();

			StmALog attachEvent = shipment1.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertNull("Precondition - attach event shouldn't exist", attachEvent);

			consol.Shipments.Add(shipment1);
			attachEvent = shipment1.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertNotNull("Expecting Shipment1 to have an ATC event", attachEvent);
			AssertEquals("Expecting AttachEvent to have reference.", string.Format("{0}|TYP=Consol", consol.JK_UniqueConsignRef), attachEvent.SL_Reference);

			attachEvent = shipment2.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertNull("Precondition - attach event shouldn't exist", attachEvent);

			consol.Shipments.Add(shipment2);
			attachEvent = shipment2.Logs.MostRecentLogByEventTime(Events.Attached);
			AssertNotNull("Shipment2 should have an ATC event", attachEvent);
		}

		public void TestDTCEventAddedWhenShipmentDetachedFromConsol()
		{
			var consol1 = Factory.New<CommonConsol>();
			var shipment1 = Factory.New<CommonShipment>();
			Factory.Save();

			var shipment2 = Factory.New<CommonShipment>();

			consol1.Shipments.Add(shipment1);
			consol1.Shipments.Add(shipment2);

			var detachEvent = shipment1.Logs.MostRecentLogByEventTime(Events.Detached);
			AssertNull("Precondition - detach event shouldn't exist", detachEvent);

			consol1.Shipments.Remove(shipment1);

			detachEvent = shipment1.Logs.MostRecentLogByEventTime(Events.Detached);
			AssertNotNull("Expecting Shipment1 to have a DTC event", detachEvent);
			AssertEquals("Expecting DetachEvent to have reference.", "C00001001|TYP=Consol", detachEvent.SL_Reference);

			detachEvent = shipment2.Logs.MostRecentLogByEventTime(Events.Detached);
			AssertNull("Precondition - detach event shouldn't exist", detachEvent);

			consol1.Shipments.Remove(shipment2);
			Assert("Expecting Shipment2 to to be deleted - it will therefore not have a DTC event", shipment2.IsDeleted);
		}

		public void TestDTCEventIsNotAddedWhileRefreshingViaDataRefreshBus()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var shipmentReloaded = anotherFactory.Load<CommonShipment>(shipment.PK);
			var consolReloaded = shipmentReloaded.Consols[0];

			AssertEquals("Precondition", 0, shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Detached.Code)).Length);
			AssertEquals(true, Factory.RefreshEnabled);
			AssertEquals(true, anotherFactory.RefreshEnabled);

			shipmentReloaded.Consols.Remove(consolReloaded);
			anotherFactory.Save();
			Factory.Save();

			AssertEquals("Only one DTC event was created", 1, shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Detached.Code)).Length);
		}

		#endregion

		#region CFS Registered

		public void TestIsCFSRegisteredBehavior()
		{
			CommonShipment shipment = Consol.Shipments.AddNew();
			Consol.JK_IsCFS = true;
			AssertEquals("TestATCEventAddedWhenShipmentAttachedToConsol CFS flag set", true, shipment.JS_IsCFSRegistered);
			CommonShipment shipment2 = Consol.Shipments.AddNew();
			AssertEquals("CommonShipment 2 CFS flag defaulted", true, shipment2.JS_IsCFSRegistered);

			Consol.JK_IsCFS = false;
			AssertEquals("CommonShipment CFS flag set", false, shipment.JS_IsCFSRegistered);
			AssertEquals("CommonShipment 2 CFS flag set", false, shipment2.JS_IsCFSRegistered);
			CommonShipment shipment3 = Consol.Shipments.AddNew();
			AssertEquals("CommonShipment 3 CFS flag defaulted", false, shipment3.JS_IsCFSRegistered);
			Consol.JK_IsCFS = true;
			AssertEquals("CommonShipment CFS flag set", true, shipment.JS_IsCFSRegistered);
			AssertEquals("CommonShipment 2 CFS flag set", true, shipment2.JS_IsCFSRegistered);
			AssertEquals("CommonShipment 3 CFS flag set", true, shipment3.JS_IsCFSRegistered);
		}

		#endregion

		#region Deleted Shipment Management

		public void TestSetConsignRefsOnDeletedShipments()
		{
			CommonConsol parentConsol = Factory.New<CommonConsol>();
			ConsolShipmentCollection collection = new ConsolShipmentCollection(parentConsol);
			CommonShipment shipment1 = collection.AddNew();
			CommonShipment shipment2 = collection.AddNew();
			CommonShipment shipment3 = collection.AddNew();
			shipment2.Delete();
			Factory.Save();
			AssertEquals("Collection should have 2 shipments with consignment number set", 2, collection.Count);
			Assert("CommonShipment 1 Unique Consignment Ref not set", !shipment1.JS_UniqueConsignRef.IsEmpty);
			Assert("CommonShipment 3 Unique Consignment Ref not set", !shipment3.JS_UniqueConsignRef.IsEmpty);
		}

		[ExpectNoExceptions()]
		public void TestDeletedShipmentAndSaving_ShipmentIsNotSaved()
		{
			CommonConsol parent = Factory.New<CommonConsol>();
			ConsolShipmentCollection collection = new ConsolShipmentCollection(parent);
			CommonShipment shipment = Factory.New<CommonShipment>();
			parent.Shipments.Add(shipment);
			shipment.Delete();
			Factory.Save();
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestDeletedShipmentAndSaving_ShipmentIsSaved()
		{
			CommonConsol parent = Factory.New<CommonConsol>();
			ConsolShipmentCollection collection = new ConsolShipmentCollection(parent);
			CommonShipment shipment = Factory.New<CommonShipment>();
			Factory.Save();
			parent.Shipments.Add(shipment);
			shipment.Delete();
		}

		#endregion

		#region Buyers Consol Parent

		public void TestOnAddedBCNShipmentDoNotSetParentToItself()
		{
			CommonConsol parent = Factory.New<CommonConsol>();
			parent.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			ConsolShipmentCollection collection = new ConsolShipmentCollection(parent);
			CommonShipment shipment = parent.Shipments.AddNew();
			Factory.Save();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			parent.Shipments.Remove(shipment);
			parent.Shipments.Add(shipment);
			AssertEquals("Buyers Consol Master CommonShipment should not have its parent Shipment set", ZGuid.Empty, shipment.JS_JS_ColoadMasterShipment);
		}

		#endregion

		#region CFS Consol

		public void TestShipmentBecomesCFSIfAddedToCFSConsol()
		{
			Consol.JK_IsCFS = true;
			Consol.JK_IsForwarding = true;
			CommonShipment shipmentToAdd = Factory.New<CommonShipment>();
			Assert("Precondition - CommonShipment is not CFS registered", !shipmentToAdd.JS_IsCFSRegistered);

			Consol.Shipments.Add(shipmentToAdd);
			Assert("A CommonShipment should becme CFS registered when attached to a CFS+forwarding consol", shipmentToAdd.JS_IsCFSRegistered);

			Consol.JK_IsCFS = false;
			CommonShipment shipmentToAdd2 = Factory.New<CommonShipment>();
			Consol.Shipments.Add(shipmentToAdd2);
			Assert("CommonShipment should not become CFS registered if consol is not CFS registered", !shipmentToAdd2.JS_IsCFSRegistered);
		}

		#endregion

		#region Implementation

		CommonConsol Consol;

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<CommonConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = new SailingsForTestClasses(Factory).SydLaxSailing.PK;
		}

		#endregion
	}
}
