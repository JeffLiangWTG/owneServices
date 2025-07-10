using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVOriginLoadListHelperTest : TestCaseWithFactory
	{
		#region MatchAndUpdateShipment

		public void TestTryAttachToConsol_WhenHVMAndHVLMatched_UpdateExisting()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			AttachAndAssertMatchAndUpdateResult(1, 1);
		}

		public void TestTryAttachToConsol_WhenHVMAndHVLMatched_UpdatePackingDetails()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			var outerpackLine = hvlShipment.OuterPackLines.AddNew();
			outerpackLine.JL_PackageCount = 1;
			outerpackLine.JL_ActualWeight = 1.1;
			outerpackLine.JL_ActualWeightUQ = Weight.Kilograms;
			outerpackLine.JL_ActualVolume = 1.2;
			outerpackLine.JL_ActualVolumeUQ = Volume.CubicMetres;

			Factory.Save();

			new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { matchingMasterLoadList }, out _);
			CombineAssertions("The packing details were combined from existing data and hvlv item", () =>
			{
				AssertEquals(2, hvlShipment.TotalOuterPacks);
				AssertEquals(2.1m, hvlShipment.TotalOuterPacksWeight);
				AssertEquals(2.2m, hvlShipment.TotalOuterPacksVolume);
				AssertEquals(2, hvlShipment.JS_OuterPacksReadOnly);
				AssertEquals(2.1m, hvlShipment.JS_ActualWeightReadOnly);
				AssertEquals(2.2m, hvlShipment.JS_ActualVolumeReadOnly);
				AssertEquals(2, hvlShipment.JS_OuterPacks);
				AssertEquals(2.1m, hvlShipment.JS_ActualWeight);
				AssertEquals(2.2m, hvlShipment.JS_ActualVolume);
			});
		}

		public void TestTryAttachToConsol_DifferentDestinationLoadlistButSameInConsignment_UpdateExistingHVL()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			matchingMasterLoadList.DestinationDepot.OA_RN_NKCountryCode = CountryCodes.Canada;

			AttachAndAssertMatchAndUpdateResult(1, 1);
		}

		public void TestTryAttachToConsol_DifferentShipperAddress_CreateNewHVL()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			bookingHeader.HVH_OA_BillToParty = Factory.NewWithValidTestData<OrgAddress>().PK;

			AttachAndAssertMatchAndUpdateResult(1, 2);
		}

		public void TestTryAttachToConsol_DifferentBookingHeaderServiceLevel_CreateNewHVL()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			bookingHeader.HVH_RS_NKBookingServiceLevel = "D2D";

			AttachAndAssertMatchAndUpdateResult(1, 2);
		}

		public void TestTryAttachToConsol_DifferentMasterHouseBill_CreateNewHVMAndHVL()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			matchingMasterLoadList.HVL_HouseBillNumber = "8967435278435";
			CreateItem("item002", bookingHeader, matchingMasterLoadList, 1, "KG", 1, "M3", 1, "AUD", "BOX", "AU");
			Factory.Save();

			AttachAndAssertMatchAndUpdateResult(2, 3);
		}

		public void TestTryAttachToConsol_EmptyMasterHouseBillWithOneHVM_MergeHVM()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			matchingMasterLoadList.HVL_HouseBillNumber = "";
			CreateItem("item002", bookingHeader, matchingMasterLoadList, 1, "KG", 1, "M3", 1, "AUD", "BOX", "AU");
			Factory.Save();

			AttachAndAssertMatchAndUpdateResult(1, 2);
		}

		public void TestTryAttachToConsol_EmptyMasterHouseBillWithMutipleHVM_ReturnError()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			var hvmShipment2 = consol.Shipments.AddNew();
			hvmShipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			hvmShipment2.JS_HouseBill = "MASTER124";
			hvmShipment2.JS_RS_NKServiceLevel = "STD";
			hvmShipment2.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;

			matchingMasterLoadList.HVL_HouseBillNumber = "";
			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(consol, new[] { matchingMasterLoadList }, out _);
			Assert(!success);
			AssertContains($@"Error: There are multiple HVM shipments on consol '{consol.JK_UniqueConsignRef}'. Load list '{matchingMasterLoadList.HVL_UniqueReference}' failed to be merged.", logger.ToString().Trim());
		}

		public void TestTryAttachToConsol_MasterHouse_DifferentMasterServiceLevel_CreateNewHVMAndHVL()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			matchingMasterLoadList.HVL_RS_NKServiceLevel = "D2D";

			AttachAndAssertMatchAndUpdateResult(2, 2);
		}

		public void TestTryAttachToConsol_WhenLoadlistWithDifferentDestinationCountryConsignment_CreateMultipleHVLByDestinationCountry()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: true);

			CreateItem("item002", bookingHeader, matchingMasterLoadList, 1, "KG", 1, "M3", 1, "AUD", "BOX", "AU");
			CreateItem("item003", bookingHeader, matchingMasterLoadList, 1, "KG", 1, "M3", 1, "AUD", "BOX", "AU");
			CreateItem("item004", bookingHeader, matchingMasterLoadList, 1, "KG", 1, "M3", 1, "AUD", "BOX", "CA");

			Factory.Save();

			AttachAndAssertMatchAndUpdateResult(1, 3);
		}

		public void TestTryAttachToConsol_MatchExistingHVLShipment()
		{
			SetupExistingDataForMatchAndUpdateTests(isMasterHouse: false);

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { matchingMasterLoadList }, out _);

			Assert(success);

			var hvlShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVL"));

			AssertEquals("Should only query one HVL Shipment.", 1, hvlShipments.Length);
		}

		void SetupExistingDataForMatchAndUpdateTests(bool isMasterHouse)
		{
			shipperAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			bookingHeader = CreateBookingHeader(shipperAddress.Header, "STD");
			Factory.Save();

			consol = Factory.NewWithValidTestData<ForwardingConsol>();

			hvlShipment = consol.Shipments.AddNew();
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.JS_HouseBill = "HVLV123";
			hvlShipment.JS_RL_NKOrigin = "AUSYD";
			hvlShipment.JS_RL_NKDestination = "USCHI";
			hvlShipment.JS_RS_NKServiceLevel = "STD";
			hvlShipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;

			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RN_NKCountryCode = CountryCodes.UnitedStates;

			matchingMasterLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			matchingMasterLoadList.HVL_IsMasterHouse = isMasterHouse;
			matchingMasterLoadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			matchingMasterLoadList.HVL_RS_NKServiceLevel = "STD";
			matchingMasterLoadList.HVL_HouseBillNumber = "MASTER123";
			CreateItem("item001", bookingHeader, matchingMasterLoadList, 1, "KG", 1, "M3", 1, "AUD", "BOX");

			if (isMasterHouse)
			{
				hvmShipment = consol.Shipments.AddNew();
				hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
				hvmShipment.JS_HouseBill = "MASTER123";
				hvmShipment.JS_RS_NKServiceLevel = "STD";
				hvmShipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;

				hvlShipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;
			}

			Factory.Save();
		}

		void AttachAndAssertMatchAndUpdateResult(int expectHVMShipmentCount, int expectHVLShipmentCount)
		{
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { matchingMasterLoadList }, out _);
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("TryAttachToConsol should be successful", success);
				AssertEquals("Precondition: Load List should be consolidated", HVLVOriginLoadListStatus.Codes.Consolidated, matchingMasterLoadList.HVL_Status);

				AssertEquals($"There should be {expectHVMShipmentCount} HVM Shipment(s)",
					expectHVMShipmentCount,
					Factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValueMaster)));
				AssertEquals($"There should be {expectHVLShipmentCount} HVL Shipment(s)",
					expectHVLShipmentCount,
					Factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValue)));
			});
		}

		#endregion

		#region CreateConsolFromLoadList

		public void TestCreateConsolFromLoadList()
		{
			var carrier = Factory.New<OrgHeader>();
			var originDepot = Factory.New<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepot = Factory.New<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_E_Dep = new ZDateTime(2017, 6, 1);
			loadList.HVL_E_Arv = new ZDateTime(2017, 6, 15);
			loadList.HVL_MasterBillNumber = "1234567";
			loadList.HVL_OH_Carrier = carrier.PK;
			loadList.HVL_VesselName = "VESSEL1";
			loadList.HVL_VoyageFlight = "VOYAGE1";
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";

			var airCTO = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO.O5_PortOrCountry = originDepot.OA_RL_NKRelatedPortCode;
			airCTO.O5_OA_AgentOfficeAddress = Factory.New<OrgAddress>().PK;

			var consol = (ForwardingConsol)new HVLVOriginLoadListHelper(new DummyLogger()).CreateConsol(loadList);
			var transport = consol.Transports.Cast<Transport>().Single();

			CombineAssertions(() =>
			{
				AssertEquals("consol.JK_AgentType", AgentType.Agent, consol.JK_AgentType);
				AssertEquals("consol.JK_TransportMode", TransportModes.Air, consol.JK_TransportMode);
				AssertEquals("consol.JK_ConsolMode", ContainerModes.Loose, consol.JK_ConsolMode);
				AssertEquals("consol.JK_RL_NKLoadPort", "AUSYD", consol.JK_RL_NKLoadPort);
				AssertEquals("consol.JK_RL_NKDischargePort", "USLAX", consol.JK_RL_NKDischargePort);
				AssertEquals("consol.JK_MasterBillNum", "1234567", consol.JK_MasterBillNum);
				AssertEquals("consol.JK_OA_ShippingLineAddress", carrier.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals("consol.JK_OA_PackDepotAddress", originDepot.PK, consol.JK_OA_PackDepotAddress);
				AssertEquals("consol.JK_OA_UnpackDepotAddress", destinationDepot.PK, consol.JK_OA_UnpackDepotAddress);
				AssertEquals("consol.JK_OA_DepartureCTOAddress", loadList.OriginCTO, consol.JK_OA_DepartureCTOAddress);

				AssertEquals("transport.JW_RL_NKLoadPort", "AUSYD", transport.JW_RL_NKLoadPort);
				AssertEquals("transport.JW_RL_NKDiscPort", "USLAX", transport.JW_RL_NKDiscPort);
				AssertEquals("transport.JW_ETD", new ZDateTime(2017, 6, 1), transport.JW_ETD);
				AssertEquals("transport.JW_ETA", new ZDateTime(2017, 6, 15), transport.JW_ETA);
				AssertEquals("transport.JW_Vessel", "VESSEL1", transport.JW_Vessel);
				AssertEquals("transport.JW_VoyageFlight", "VOYAGE1", transport.JW_VoyageFlight);
			});
		}

		#endregion

		#region CreateConsolFromLoadList_ContainerMode

		public void TestCreateConsolFromLoadList_ContainerMode()
		{
			AssertConsolContainerModeFromTransportMode(TransportModes.Sea, ContainerModes.Groupage);
			AssertConsolContainerModeFromTransportMode(TransportModes.Air, ContainerModes.Loose);
			AssertConsolContainerModeFromTransportMode(TransportModes.Road, ContainerModes.LTL);
			AssertConsolContainerModeFromTransportMode(TransportModes.Rail, ContainerModes.LCL);
		}

		void AssertConsolContainerModeFromTransportMode(string transportMode, string expectedContainerMode)
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = transportMode;

			var consol = (ForwardingConsol)new HVLVOriginLoadListHelper(new DummyLogger()).CreateConsol(loadList);
			AssertEquals(transportMode, consol.JK_TransportMode);
			AssertEquals(expectedContainerMode, consol.JK_ConsolMode);
		}

		#endregion

		#region CreateConsolFromLoadList_LoadAndDischargePorts

		public void TestCreateConsolFromLoadList_LoadAndDischargePorts()
		{
			var originDepot = Factory.New<OrgAddress>();
			originDepot.OA_OH = Factory.New<OrgHeader>().PK;
			originDepot.Header.OH_RL_NKClosestPort = "AUSYD";
			var destinationDepot = Factory.New<OrgAddress>();
			destinationDepot.OA_OH = Factory.New<OrgHeader>().PK;
			destinationDepot.Header.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";

			var consol = (ForwardingConsol)new HVLVOriginLoadListHelper(new DummyLogger()).CreateConsol(loadList);
			var transport = consol.Transports.Cast<Transport>().Single();

			AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
			AssertEquals("USLAX", consol.JK_RL_NKDischargePort);
			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("USLAX", transport.JW_RL_NKDiscPort);

			originDepot.OA_RL_NKRelatedPortCode = "AUMEL";
			destinationDepot.OA_RL_NKRelatedPortCode = "USCHI";
			loadList.HVL_RL_NKOrigin = "AUMEL";
			loadList.HVL_RL_NKDestination = "USCHI";

			consol = (ForwardingConsol)new HVLVOriginLoadListHelper(new DummyLogger()).CreateConsol(loadList);
			transport = consol.Transports.Cast<Transport>().Single();

			AssertEquals("AUMEL", consol.JK_RL_NKLoadPort);
			AssertEquals("USCHI", consol.JK_RL_NKDischargePort);
			AssertEquals("AUMEL", transport.JW_RL_NKLoadPort);
			AssertEquals("USCHI", transport.JW_RL_NKDiscPort);
		}

		public void TestCreateConsolFromLoadList_WhenHVL_IsNeutralMasterIsTrue()
		{
			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "027";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.Save();

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "Org1";
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_OH = orgHeader1.PK;
			originDepot.Header.OH_RL_NKClosestPort = "AUSYD";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "Org2";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_OH = orgHeader2.PK;
			destinationDepot.Header.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_VoyageFlight = "AS111";
			loadList.HVL_IsNeutralMaster = true;

			consol = (ForwardingConsol)new HVLVOriginLoadListHelper(new DummyLogger()).CreateConsol(loadList);
			Factory.Save();

			AssertEquals(true, consol.JK_IsNeutralMaster);
			AssertEquals("02710000001", consol.JK_MasterBillNum);
		}

		#endregion

		#region AttachConsolToLoadLists

		public void TestTryAttachToConsol()
		{
			Env.Registry.FreightWeightUnit = Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Volume.CubicMetres;

			var usCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var exchangeRate = usCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
			exchangeRate.RE_SellRate = 0.5;

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.OH_RL_NKClosestPort = "AUSYD";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var containerType1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-1")).PK;
			var containerType2 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2")).PK;

			var loadList1 = CreateLoadList(TransportModes.Air, "CONT123", containerType1, destinationDepot);
			var loadList2 = CreateLoadList(TransportModes.Air, "CONT456", containerType2, destinationDepot);
			var loadList3 = CreateLoadList(TransportModes.Air, null, ZGuid.Empty, destinationDepot);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";

			var header1 = CreateBookingHeader(shipper, "STD");
			var header2 = CreateBookingHeader(shipper, "D2D");
			var header3 = CreateBookingHeader(shipper, "STD");

			var item1 = CreateItem("ITEM1", header1, loadList1, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", PkgUnit.Box);
			var item2 = CreateItem("ITEM2", header2, loadList2, 20, Weight.Kilograms, 0.2, Volume.CubicMetres, 2, "AUD", PkgUnit.Bundle);
			var item3 = CreateItem("ITEM3", header3, loadList3, 30, Weight.Pounds, 0.3, Volume.CubicFeet, 3, "USD", PkgUnit.Drum);
			var item4 = CreateItem("ITEM4", header3, loadList1, 40, Weight.Pounds, 0.4, Volume.CubicFeet, 4, "USD", PkgUnit.Case);

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList1, loadList2, loadList3 }, out _);
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("TryAttachToConsol should be successful", success);
				var containers = consol.Containers.Cast<ForwardingContainer>().ToArray();
				AssertEquals("containers.Length", 2, containers.Length);

				var container1 = containers.Single(x => x.JC_ContainerNum == "CONT123");
				AssertEquals("container1.JC_RC", containerType1, container1.JC_RC);
				AssertEquals("container1.JC_ContainerMode", ContainerModes.Loose, container1.JC_ContainerMode);
				AssertEquals("container1.JC_DeliveryMode", ZString.Empty, container1.JC_DeliveryMode);
				var container2 = containers.Single(x => x.JC_ContainerNum == "CONT456");
				AssertEquals("container2.JC_RC", containerType2, container2.JC_RC);
				AssertEquals("container2.JC_ContainerMode", ContainerModes.Loose, container2.JC_ContainerMode);
				AssertEquals("container2.JC_DeliveryMode", ZString.Empty, container2.JC_DeliveryMode);

				var shipments = consol.Shipments.Cast<ForwardingShipment>().ToArray();
				AssertEquals("shipments.Length", 2, shipments.Length);

				var stdShipment = shipments.Single(x => x.JS_RS_NKServiceLevel == "STD"); //created from loadList1 (items 1 & 4) and loadList3 (item3)
				var d2dShipment = shipments.Single(x => x.JS_RS_NKServiceLevel == "D2D"); //created from loadList2 (item2)

				var consignmentHeaders = Factory.Load<HVLVConsignmentHeader>(new ZQuery());
				AssertEquals("consignmentHeaders.Length", 2, consignmentHeaders.Length);
				AssertContainsExactElementsInAnyOrder("consignment headers should link to shipments", new[] { stdShipment.PK, d2dShipment.PK }, consignmentHeaders.Select(x => x.HCH_JS_Shipment));

				var consignmentHeaderForStdShipment = consignmentHeaders.Single(x => x.HCH_JS_Shipment == stdShipment.PK);
				var consignmentHeaderForD2dShipment = consignmentHeaders.Single(x => x.HCH_JS_Shipment == d2dShipment.PK);

				AssertEquals("stdShipment.ConsigneeDocumentaryAddress.Organisation.PK", destinationDepot.PK, stdShipment.ConsigneeDocumentaryAddress.Organisation.PK);
				AssertEquals("stdShipment.JS_RL_NKDestination", "AUSYD", stdShipment.JS_RL_NKDestination);
				AssertEquals("stdShipment.ConsignorDocumentaryAddress.Organisation.PK", shipper.PK, stdShipment.ConsignorDocumentaryAddress.Organisation.PK);
				AssertEquals("stdShipment.JS_RL_NKOrigin", "USLAX", stdShipment.JS_RL_NKOrigin);
				AssertEquals("stdShipment.JS_TransportMode", TransportModes.Air, stdShipment.JS_TransportMode);
				AssertEquals("stdShipment.JS_PackingMode", ContainerModes.ULD, stdShipment.JS_PackingMode);
				AssertEquals("stdShipment.JS_ShipmentType", ShipmentTypes.HighVolumeLowValue, stdShipment.JS_ShipmentType);
				AssertEquals("stdShipment.JS_GoodsDescription", "Various Cargo", stdShipment.JS_GoodsDescription);
				AssertEquals("stdShipment.JS_GoodsValue", 15m, stdShipment.JS_GoodsValue);
				AssertEquals("stdShipment.JS_RX_NKGoodsValueCurr", "AUD", stdShipment.JS_RX_NKGoodsValueCurr);
				AssertEquals("stdShipment.JS_OuterPacks", 3, stdShipment.JS_OuterPacks);
				AssertEquals("stdShipment.JS_ActualWeight", 41.752m, stdShipment.JS_ActualWeight);
				AssertEquals("stdShipment.JS_UnitOfWeight", Weight.Kilograms, stdShipment.JS_UnitOfWeight);
				AssertEquals("stdShipment.JS_ActualVolume ", 0.119m, stdShipment.JS_ActualVolume);
				AssertEquals("stdShipment.JS_UnitOfVolume", Volume.CubicMetres, stdShipment.JS_UnitOfVolume);
				AssertEquals("stdShipment.JS_F3_NKPackType", PkgUnit.Package, stdShipment.JS_F3_NKPackType);
				AssertEquals("item1.HVI_JS_LoadedOnShipment", stdShipment.PK, item1.HVI_JS_LoadedOnShipment);
				AssertEquals("item3.HVI_JS_LoadedOnShipment", stdShipment.PK, item3.HVI_JS_LoadedOnShipment);
				AssertEquals("item4.HVI_JS_LoadedOnShipment", stdShipment.PK, item4.HVI_JS_LoadedOnShipment);
				AssertEquals("consignment for item1 should link to consignment header for std shipment", consignmentHeaderForStdShipment.PK, item1.Consignment.HVC_HCH_Header);
				AssertEquals("consignment for item3 should link to consignment header for std shipment", consignmentHeaderForStdShipment.PK, item3.Consignment.HVC_HCH_Header);
				AssertEquals("consignment for item4 should link to consignment header for std shipment", consignmentHeaderForStdShipment.PK, item4.Consignment.HVC_HCH_Header);
				AssertEquals("consignment for item1 should have the same cluster key as consignment header has", consignmentHeaderForStdShipment.HCH_ClusterKey, item1.Consignment.HVC_ClusterKey);
				AssertEquals("consignment for item3 should have the same cluster key as consignment header has", consignmentHeaderForStdShipment.HCH_ClusterKey, item3.Consignment.HVC_ClusterKey);
				AssertEquals("consignment for item4 should have the same cluster key as consignment header has", consignmentHeaderForStdShipment.HCH_ClusterKey, item4.Consignment.HVC_ClusterKey);

				AssertEquals("stdShipment.OuterPackLines.Count", 2, stdShipment.OuterPackLines.Count);
				var stdPackLine1 = stdShipment.OuterPackLines.Cast<PackLine>().Single(x => x.JL_JC == container1.PK); // created from loadList1 (items 1 & 4)
				var stdPackLine2 = stdShipment.OuterPackLines.Cast<PackLine>().Single(x => x.JL_JC.IsEmpty); // created from loadList3 (item 3)

				AssertEquals("stdPackLine1.JL_PackageCount", 2, stdPackLine1.JL_PackageCount);
				AssertEquals("stdPackLine1.JL_ActualWeight", 28.144m, stdPackLine1.JL_ActualWeight);
				AssertEquals("stdPackLine1.JL_ActualWeightUQ", Weight.Kilograms, stdPackLine1.JL_ActualWeightUQ);
				AssertEquals("stdPackLine1.JL_ActualVolume", 0.111m, stdPackLine1.JL_ActualVolume);
				AssertEquals("stdPackLine1.JL_ActualVolumeUQ", Volume.CubicMetres, stdPackLine1.JL_ActualVolumeUQ);
				AssertEquals("stdPackLine1.JL_F3_NKPackType", PkgUnit.Package, stdPackLine1.JL_F3_NKPackType);

				AssertEquals("stdPackLine2.JL_PackageCount", 1, stdPackLine2.JL_PackageCount);
				AssertEquals("stdPackLine2.JL_ActualWeight", 13.608m, stdPackLine2.JL_ActualWeight);
				AssertEquals("stdPackLine2.JL_ActualWeightUQ", Weight.Kilograms, stdPackLine2.JL_ActualWeightUQ);
				AssertEquals("stdPackLine2.JL_ActualVolume", 0.008m, stdPackLine2.JL_ActualVolume);
				AssertEquals("stdPackLine2.JL_ActualVolumeUQ", Volume.CubicMetres, stdPackLine2.JL_ActualVolumeUQ);
				AssertEquals("stdPackLine2.JL_F3_NKPackType", PkgUnit.Drum, stdPackLine2.JL_F3_NKPackType);

				AssertEquals("d2dShipment.ConsigneeDocumentaryAddress.Organisation.PK", destinationDepot.PK, d2dShipment.ConsigneeDocumentaryAddress.Organisation.PK);
				AssertEquals("d2dShipment.JS_RL_NKDestination", "AUSYD", d2dShipment.JS_RL_NKDestination);
				AssertEquals("d2dShipment.ConsignorDocumentaryAddress.Organisation.PK", shipper.PK, d2dShipment.ConsignorDocumentaryAddress.Organisation.PK);
				AssertEquals("d2dShipment.JS_RL_NKOrigin", "USLAX", d2dShipment.JS_RL_NKOrigin);
				AssertEquals("d2dShipment.JS_TransportMode", TransportModes.Air, d2dShipment.JS_TransportMode);
				AssertEquals("d2dShipment.JS_PackingMode", ContainerModes.ULD, d2dShipment.JS_PackingMode);
				AssertEquals("d2dShipment.JS_ShipmentType", ShipmentTypes.HighVolumeLowValue, d2dShipment.JS_ShipmentType);
				AssertEquals("d2dShipment.JS_GoodsDescription", "Various Cargo", d2dShipment.JS_GoodsDescription);
				AssertEquals("d2dShipment.JS_GoodsValue", 2m, d2dShipment.JS_GoodsValue);
				AssertEquals("d2dShipment.JS_RX_NKGoodsValueCurr", "AUD", d2dShipment.JS_RX_NKGoodsValueCurr);
				AssertEquals("d2dShipment.JS_OuterPacks", 1, d2dShipment.JS_OuterPacks);
				AssertEquals("d2dShipment.JS_ActualWeight", 20m, d2dShipment.JS_ActualWeight);
				AssertEquals("d2dShipment.JS_UnitOfWeight", Weight.Kilograms, d2dShipment.JS_UnitOfWeight);
				AssertEquals("d2dShipment.JS_ActualVolume", 0.2m, d2dShipment.JS_ActualVolume);
				AssertEquals("d2dShipment.JS_UnitOfVolume", Volume.CubicMetres, d2dShipment.JS_UnitOfVolume);
				AssertEquals("d2dShipment.JS_F3_NKPackType", PkgUnit.Bundle, d2dShipment.JS_F3_NKPackType);
				AssertEquals("item2.HVI_JS_LoadedOnShipment", d2dShipment.PK, item2.HVI_JS_LoadedOnShipment);
				AssertEquals("consignment for item2 should link to consignment header for d2d shipment", consignmentHeaderForD2dShipment.PK, item2.Consignment.HVC_HCH_Header);
				AssertEquals("consignment for item2 should have the same cluster key as consignment header has", consignmentHeaderForD2dShipment.HCH_ClusterKey, item2.Consignment.HVC_ClusterKey);

				AssertEquals("d2dShipment.OuterPackLines.Count", 1, d2dShipment.OuterPackLines.Count);
				var d2dPackLine = d2dShipment.OuterPackLines.Cast<PackLine>().Single(x => x.JL_JC == container2.PK); // created from loadList2 (item2)

				AssertEquals("d2dPackLine.JL_PackageCount", 1, d2dPackLine.JL_PackageCount);
				AssertEquals("d2dPackLine.JL_ActualWeight", 20m, d2dPackLine.JL_ActualWeight);
				AssertEquals("d2dPackLine.JL_ActualWeightUQ", Weight.Kilograms, d2dPackLine.JL_ActualWeightUQ);
				AssertEquals("d2dPackLine.JL_ActualVolume", 0.2m, d2dPackLine.JL_ActualVolume);
				AssertEquals("d2dPackLine.JL_ActualVolumeUQ", Volume.CubicMetres, d2dPackLine.JL_ActualVolumeUQ);
				AssertEquals("d2dPackLine.JL_F3_NKPackType", PkgUnit.Bundle, d2dPackLine.JL_F3_NKPackType);

				AssertLoadListConsolidated(loadList1, consol);
				AssertLoadListConsolidated(loadList2, consol);
				AssertLoadListConsolidated(loadList3, consol);
			});
		}

		void AssertLoadListConsolidated(HVLVOriginLoadList loadList, ForwardingConsol consol)
		{
			AssertEquals("loadList.HVL_Status", ELoadListStatuses.Consolidated, loadList.HVL_Status);
			AssertEquals("ELC log count", 3, consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.ELoadListConsolidatedCode).Count());
		}

		public void TestTryAttachToConsol_WillNotDuplicateContainer()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "12456543";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_ContainerNumber = "12456543";

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			Assert("TryAttachToConsol should be successful", success);
			AssertEquals(1, consol.Containers.Count);
		}

		public void TestTryAttachToConsol_WillFindLoadListReferenceLogs()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "12456543";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_ContainerNumber = "12456543";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";
			var header = CreateBookingHeader(shipper, "STD");
			CreateItem("ITEMFORAIR", header, loadList, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", PkgUnit.Box);

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			Factory.Save();
			Assert("TryAttachToConsol should be successful", success);

			var logs = consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.ELoadListConsolidatedCode);
			AssertEquals("ELC should have only one log", 1, logs.Count());

			StmALog.GetParametersFromReference(logs.First().SL_Reference).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);
			AssertEquals(loadList.HVL_UniqueReference, referenceNumber);
		}

		public void TestTryAttachToConsol_DefaultContainerDeliveryModeWhenTransportModeIsNotAir()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.OH_RL_NKClosestPort = "AUSYD";
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";

			#region Consol transport air

			var airConsol = Factory.New<ForwardingConsol>();
			var containerTypeForAir = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-1")).PK;
			var loadListForAir = CreateLoadList(TransportModes.Air, "AIRCONTAINER", containerTypeForAir, destinationDepot);
			var headerForAir = CreateBookingHeader(shipper, "STD");
			CreateItem("ITEMFORAIR", headerForAir, loadListForAir, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", PkgUnit.Box);
			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(airConsol, new[] { loadListForAir }, out _);
			Factory.Save();

			var airContainer = airConsol.Containers.Cast<ForwardingContainer>().Single(x => x.JC_ContainerNum == "AIRCONTAINER");

			Assert("TryAttachToConsol should be successful for airContainer", success);
			AssertEquals("airContainer.JC_RC", containerTypeForAir, airContainer.JC_RC);
			AssertEquals("airContainer.JC_DeliveryMode", ZString.Empty, airContainer.JC_DeliveryMode);

			#endregion

			#region Consol transport not air

			var notairConsol = Factory.New<ForwardingConsol>();
			var containerTypeForNotAir = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-2")).PK;
			var loadListForNotAir = CreateLoadList(TransportModes.Sea, "NOTAIRCONTAINER", containerTypeForNotAir, destinationDepot);
			var headerForNotAir = CreateBookingHeader(shipper, "D2D");
			CreateItem("ITEMFORNOTAIR", headerForNotAir, loadListForNotAir, 20, Weight.Kilograms, 0.2, Volume.CubicMetres, 2, "AUD", PkgUnit.Bundle);
			Factory.Save();

			var success2 = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(notairConsol, new[] { loadListForNotAir }, out _);
			Factory.Save();

			var notairContainer = notairConsol.Containers.Cast<ForwardingContainer>().ToArray().Single(x => x.JC_ContainerNum == "NOTAIRCONTAINER");

			Assert("TryAttachToConsol should be successful for notAirContainer", success2);
			AssertEquals("notairContainer.JC_RC", containerTypeForNotAir, notairContainer.JC_RC);
			AssertEquals("notairContainer.JC_DeliveryMode", DeliveryModes.Codes.CFS_CFS, notairContainer.JC_DeliveryMode);

			#endregion
		}

		public void TestTryAttachToConsol_SetHVH_IsProcessedAtOriginDepot()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.OH_RL_NKClosestPort = "AUSYD";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var containerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-1")).PK;

			var loadList1 = CreateLoadList(TransportModes.Air, "CONT123", containerType, destinationDepot);
			var loadList2 = CreateLoadList(TransportModes.Air, "CONT456", containerType, destinationDepot);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";

			var headerWithItemOnLoadList1 = CreateBookingHeader(shipper, "STD");
			var headerWithItemOnLoadList2 = CreateBookingHeader(shipper, "D2D");
			var headerWithNoItemOnLoadList = CreateBookingHeader(shipper, "STD");

			CreateItem("ITEM1", headerWithItemOnLoadList1, loadList1, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", PkgUnit.Box);
			CreateItem("ITEM2", headerWithItemOnLoadList2, loadList2, 20, Weight.Kilograms, 0.2, Volume.CubicMetres, 2, "AUD", PkgUnit.Bundle);
			CreateItem("ITEM3", headerWithNoItemOnLoadList, loadList: null, 30, Weight.Pounds, 0.3, Volume.CubicFeet, 3, "USD", PkgUnit.Drum);

			Factory.Save();

			CombineAssertions("pre-condition", () =>
			{
				AssertEquals("Header with item on loadlist 1", false, headerWithItemOnLoadList1.HVH_IsProcessedAtOriginDepot);
				AssertEquals("Header with item on loadlist 2", false, headerWithItemOnLoadList2.HVH_IsProcessedAtOriginDepot);
				AssertEquals("Header with no item on loadlist", false, headerWithNoItemOnLoadList.HVH_IsProcessedAtOriginDepot);
			});

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList1, loadList2 }, out _);

			CombineAssertions("HVH_IsProcessedAtOriginDepot should be set to true if booking header is processed", () =>
			{
				Assert("TryAttachToConsol should be successful", success);
				AssertEquals("Header with item on loadlist 1", true, headerWithItemOnLoadList1.HVH_IsProcessedAtOriginDepot);
				AssertEquals("Header with item on loadlist 2", true, headerWithItemOnLoadList2.HVH_IsProcessedAtOriginDepot);
				AssertEquals("Header with no item on loadlist", false, headerWithNoItemOnLoadList.HVH_IsProcessedAtOriginDepot);
			});
		}

		#endregion

		#region AttachConsolToLoadLists_ContainerMode

		public void TestTryAttachToConsol_ContainerMode()
		{
			AssertShipmentPackingModeFromTransportMode("ITEM1", TransportModes.Sea, ContainerModes.LCL);
			AssertShipmentPackingModeFromTransportMode("ITEM2", TransportModes.Air, ContainerModes.Loose);
			AssertShipmentPackingModeFromTransportMode("ITEM3", TransportModes.Road, ContainerModes.LTL);
			AssertShipmentPackingModeFromTransportMode("ITEM4", TransportModes.Rail, ContainerModes.LCL);
		}

		void AssertShipmentPackingModeFromTransportMode(string id, string transportMode, string expectedPackingMode)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = id;
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			var shipment = consol.Shipments.Cast<ForwardingShipment>().Single();
			Assert("TryAttachToConsol should be successful", success);
			AssertEquals(transportMode, shipment.JS_TransportMode);
			AssertEquals(expectedPackingMode, shipment.JS_PackingMode);
		}

		public void TestTryAttachToConsol_WhenConsignmentManifestedOnShipmentIsEmpty_ThenManifestedOnShipmentPopulatedByShipmentPK()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			var consignment2 = header.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_HVL_LoadList = loadList.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_HVL_LoadList = loadList.PK;

			CombineAssertions("Precondition: The item's consignments are expected to have empty HVC_JS_ManifestedOnShipment", () =>
			{
				AssertEquals("Item1.HVC_JS_ManifestedOnShipment: ", ZGuid.Empty, item1.Consignment.HVC_JS_ManifestedOnShipment);
				AssertEquals("Item2.HVC_JS_ManifestedOnShipment: ", ZGuid.Empty, item2.Consignment.HVC_JS_ManifestedOnShipment);
			});

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			var consolShipmentPK = consol.Shipments.Single().PK;

			CombineAssertions("The consignments should have ManifestedOnShipment equal to its attached consol's shipment", () =>
			{
				Assert("TryAttachToConsol should be successful", success);
				AssertEquals("Consignment1.HVC_JS_ManifestedOnShipment: ", consolShipmentPK, consignment1.HVC_JS_ManifestedOnShipment);
				AssertEquals("Consignment2.HVC_JS_ManifestedOnShipment: ", consolShipmentPK, consignment2.HVC_JS_ManifestedOnShipment);
			});
		}

		public void TestTryAttachToConsol_WhenConsignmentManifestedOnShipmentIsNotEmpty_ThenManifestedOnShipmentRetainsValue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignment1 = Factory.New<HVLVConsignment>();
			var consignment2 = Factory.New<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_HVL_LoadList = loadList.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_HVL_LoadList = loadList.PK;

			CombineAssertions("Precondition: The item's consignment's ManifestedOnShipment should have this PK", () =>
			{
				AssertEquals("Item1.HVC_JS_ManifestedOnShipment: ", shipment.PK, item1.Consignment.HVC_JS_ManifestedOnShipment);
				AssertEquals("Item2.HVC_JS_ManifestedOnShipment: ", shipment.PK, item2.Consignment.HVC_JS_ManifestedOnShipment);
			});

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			CombineAssertions("The consignments should not have had their ManifestedOnShipment changed", () =>
			{
				Assert("TryAttachToConsol should be successful", success);
				AssertEquals("Consignment1.HVC_JS_ManifestedOnShipment: ", shipment.PK, consignment1.HVC_JS_ManifestedOnShipment);
				AssertEquals("Consignment2.HVC_JS_ManifestedOnShipment: ", shipment.PK, consignment2.HVC_JS_ManifestedOnShipment);
			});
		}

		#endregion

		public void TestTryAttachToConsol_ShipmentCreatedPerBillToParty()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var etailer1 = Factory.NewWithValidTestData<OrgAddress>();
			var etailer2 = Factory.NewWithValidTestData<OrgAddress>();
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();

			originDepot.OA_RL_NKRelatedPortCode = "USCHI";
			destinationDepot.OA_RL_NKRelatedPortCode = "AUSYD";

			bookingHeader1.HVH_OA_BillToParty = etailer1.PK;
			bookingHeader2.HVH_OA_BillToParty = etailer2.PK;

			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item1.HVI_ActualVolume = 123;
			item2.HVI_ActualVolume = 321;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_OA_OriginDepot = originDepot.PK;

			item1.HVI_HVL_LoadList = loadList.PK;
			item2.HVI_HVL_LoadList = loadList.PK;

			loadList.HVL_RS_NKServiceLevel = "STD";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var helper = new HVLVOriginLoadListHelper(new DummyLogger());
			var success = helper.TryAttachToConsol(consol, new[] { loadList }, out _);
			Assert("TryAttachToConsol should be successful", success);

			var shipments = consol.Shipments.OfType<ForwardingShipment>().ToList();
			AssertEquals("Should create 2 shipments", 2, shipments.Count);
			var shipment1 = shipments.Single(s => s.Consignor.PK == etailer1.Header.PK);
			var shipment2 = shipments.Single(s => s.Consignor.PK == etailer2.Header.PK);
			var packLine1 = shipment1.OuterPackLines.OfType<ForwardingPackLine>().Single();
			var packLine2 = shipment2.OuterPackLines.OfType<ForwardingPackLine>().Single();
			AssertEquals(123M, packLine1.JL_ActualVolume);
			AssertEquals(321M, packLine2.JL_ActualVolume);
		}

		public void TestTryAttachToConsol_SetsLoadedOnConsolForOuterPackages()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var header = CreateBookingHeader(shipper, "STD");
			CreateItem("ITEM1", header, loadList, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", "");

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(forwardingConsol, loadList.ToList<HVLVOriginLoadList>(), out _);

			Assert("TryAttachToConsol should be successful", success);
			AssertEquals(forwardingConsol.PK, outerPackage.HVO_JK_LoadedOnConsol);
		}

		public void TestTryAttachToConsol_SetsHVOStatusConsolidatedFromLodgedForOuterPacakges()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var header = CreateBookingHeader(shipper, "STD");
			CreateItem("ITEM1", header, loadList, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", "");

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_Status = HVLVOuterPackageStatus.Codes.Lodged;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(forwardingConsol, loadList.ToList<HVLVOriginLoadList>(), out _);

			Assert("TryAttachToConsol should be successful", success);
			AssertEquals(HVLVOuterPackageStatus.Codes.Consolidated, outerPackage.HVO_Status);
		}

		public void TestTryAttachToConsol_PopulatePackingDetails_PackTypeIsPKGWhenHVLVItemPackTypeAreAllEmpty()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_MasterBillNumber = "1234567";

			var header = CreateBookingHeader(shipper, "STD");

			CreateItem("ITEM1", header, loadList, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", "");
			CreateItem("ITEM2", header, loadList, 20, Weight.Kilograms, 0.2, Volume.CubicMetres, 2, "AUD", "");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var shipment = consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault();
			var packline = shipment.OuterPackLines.Cast<PackLine>().Single();

			Assert("TryAttachToConsol should be successful", success);
			AssertEquals("Expected packline.JL_F3_NKPackType to be package when majority HVLVItem's pack type is an empty string", PkgUnit.Package, packline.JL_F3_NKPackType);
		}

		public void TestTryAttachToConsol_WhenHVLVItemsActualVolumeAndWeightIsZero_ThenUseHVLVItemsManifestedVolumeAndWeight()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_MasterBillNumber = "1234567";

			var header = CreateBookingHeader(shipper, "STD");
			var item1 = CreateItem("ITEM1", header, loadList, 0, Weight.Kilograms, 0, Volume.CubicMetres, 1, "AUD", "");
			var item2 = CreateItem("ITEM2", header, loadList, 0, Weight.Kilograms, 0, Volume.CubicMetres, 2, "AUD", "");
			item1.HVI_ActualVolume = 0;
			item1.HVI_ActualWeight = 0;
			item2.HVI_ActualVolume = 0;
			item2.HVI_ActualWeight = 0;
			item1.HVI_ManifestedVolume = 10;
			item1.HVI_ManifestedWeight = 20;
			item2.HVI_ManifestedVolume = 30;
			item2.HVI_ManifestedWeight = 40;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var shipment = consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault();
			Assert("TryAttachToConsol should be successful", success);
			AssertEquals("Expected shipment to use manifested volume of items if actual volume is zero", (ZDecimal)40, shipment.JS_ActualVolume);
			AssertEquals("Expected shipment to use manifested weight of items if actual weight is zero", (ZDecimal)60, shipment.JS_ActualWeight);
		}

		public void TestTryAttachToConsol_AddELCLogForHVLVConsignmentHeader()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = header.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			var shipment = consol.Shipments.FirstOrDefault();
			var query = new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK);
			var consignmentHeader = Factory.Load<HVLVConsignmentHeader>(query).FirstOrDefault();

			Assert("TryAttachToConsol should be successful", success);
			Assert(consignmentHeader.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ELoadListConsolidatedCode).Any());
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_CreateHVLVMasterShipmentPerConsignmentDestinationCountry()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";

			InitRefZoneHeader(destinationDepot);

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_IsMasterHouse = true;

			var shipper = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var header = CreateBookingHeader(shipper, "STD");
			CreateItem("Item1", header, loadList, 1, Weight.Kilograms, 1, Volume.CubicMetres, 1, "AUD", "PKG", "US");
			CreateItem("Item2", header, loadList, 1, Weight.Kilograms, 1, Volume.CubicMetres, 1, "AUD", "PKG", "AU");

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			Assert("TryAttachToConsol should be successful", success);

			CombineAssertions("Expected HVM to HVM shipment mappings", () =>
			{
				var shipments = Factory.Load<ForwardingShipment>(new ZQuery());

				var masterShipments = shipments.Where(shipment => shipment.JS_ShipmentType == "HVM");
				AssertEquals("Should create 1 HVLV master shipment", 1, masterShipments.Count());

				var hvlvShipments = shipments.Where(shipment => shipment.JS_ShipmentType == "HVL");
				AssertEquals("Should create 1 HVLV shipment", 2, hvlvShipments.Count());
				AssertEquals("HVLV shipment should link to HVLV master shipment", masterShipments.First().PK, hvlvShipments.First().JS_JS_ColoadMasterShipment);

				var usShipment = hvlvShipments.FirstOrDefault(shipment => shipment.JS_ShipmentType == "HVL" && shipment.JS_RL_NKDestination == "USLAX");
				AssertNotNull("Create shipment with destination port USLAX", usShipment);
				var auShipment = hvlvShipments.FirstOrDefault(shipment => shipment.JS_ShipmentType == "HVL" && shipment.JS_RL_NKDestination == "AUSYD");
				AssertNotNull("Create shipment with destination port AUSYD", auShipment);
			});
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_ShowWarningWhenNoDestinationPortMatched()
		{
			var auDepot = Factory.NewWithValidTestData<OrgAddress>();
			auDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var usDepot = Factory.NewWithValidTestData<OrgAddress>();
			usDepot.OA_RL_NKRelatedPortCode = "USLAX";

			ProcessLoadListAndAssertWarningMessage("Show warning when no HVLV Gateway", hasWarningMessage: true);

			var zoneHeader = InitRefZoneHeader(usDepot);
			Factory.Save();
			ProcessLoadListAndAssertWarningMessage("Show warning when HVLV Gateway exists, but no UNLOCO matched", hasWarningMessage: true, "NZ");

			var wollert = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AU2TO");
			zoneHeader.UNLOCOs.Add(wollert);
			Factory.Save();
			ProcessLoadListAndAssertWarningMessage("Show warning when more than one UNLOCO matched", hasWarningMessage: true);

			ProcessLoadListAndAssertWarningMessage("No warning message when only one UNLOCO matched", hasWarningMessage: false, "US");

			void ProcessLoadListAndAssertWarningMessage(string message, bool hasWarningMessage, string destinationCountry = "AU")
			{
				var shipper = Factory.NewWithValidTestData<OrgHeader>();
				var header = CreateBookingHeader(shipper, "STD");
				Factory.Save();

				var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
				loadList.HVL_TransportMode = TransportModes.Air;
				loadList.HVL_OA_OriginDepot = auDepot.PK;
				loadList.HVL_OA_DestinationDepot = usDepot.PK;
				loadList.HVL_IsMasterHouse = true;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "123";

				CreateItem("Item1", header, loadList, 1, Weight.Kilograms, 1, Volume.CubicMetres, 1, "AUD", "PKG", destinationCountry);

				Factory.Save();

				var logger = new SimpleLogger();
				new HVLVOriginLoadListHelper(logger).TryAttachToConsol(consol, new[] { loadList }, out _);

				Factory.Save();

				Assert("shipment created", consol.Shipments.Any());
				if (hasWarningMessage)
				{
					AssertContains(message,
						string.Format("Warning: Cannot determine suitable Destination Port for HVL Shipment(s) on Consol '123'. There are zero or multiple UNLOCOs for country(s) '{0}'.", destinationCountry),
						logger.ToString().Trim());
				}
				else
				{
					AssertNotContains(message, "error", logger.ToString());
				}
			}
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_ShowWarningWhenNoDestinationPortMatched_MultipleShipments()
		{
			var auDepot = Factory.NewWithValidTestData<OrgAddress>();
			auDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var usDepot = Factory.NewWithValidTestData<OrgAddress>();
			usDepot.OA_RL_NKRelatedPortCode = "USLAX";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = auDepot.PK;
			loadList.HVL_OA_DestinationDepot = usDepot.PK;
			loadList.HVL_IsMasterHouse = true;

			var shipper = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "TestConsol";

			var header = CreateBookingHeader(shipper, "STD");
			CreateItem("Item1", header, loadList, 1, Weight.Kilograms, 1, Volume.CubicMetres, 1, "AUD", "PKG", "NZ");
			CreateItem("Item2", header, loadList, 1, Weight.Kilograms, 1, Volume.CubicMetres, 1, "AUD", "PKG", "IT");

			Factory.Save();

			var logger = new SimpleLogger();
			new HVLVOriginLoadListHelper(logger).TryAttachToConsol(consol, new[] { loadList }, out _);

			Assert("shipment created", consol.Shipments.Any());
			AssertContains(string.Format("Warning: Cannot determine suitable Destination Port for HVL Shipment(s) on Consol 'TestConsol'. There are zero or multiple UNLOCOs for country(s) 'NZ,IT'."),
				logger.ToString().Trim());
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_CreateJobPackLineForOuterPackage()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_IsMasterHouse = true;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = loadList.PK;

			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage2.HVO_HVL_LoadList = loadList.PK;

			var shipper = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var header = CreateBookingHeader(shipper, "STD");
			CreateItem("Item", header, loadList, 1, Weight.Kilograms, 1, Volume.CubicMetres, 1, "AUD", "PKG");

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			Assert("TryAttachToConsol should be successful", success);
			AssertEquals("Should create 2 pack lines on HVLV master shipment", 2, masterShipment.OuterPackLines.Count);
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_ThenPopulateHVMShipmentFields()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = originDepotAddress.PK;
			var consignment = header.Consignments.AddNew();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepotAddress.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepotAddress.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_RS_NKServiceLevel = "DIR";
			loadList.HVL_HouseBillNumber = "HB1";
			loadList.HVL_INCO = IncoTerms.FreeOnBoard;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage3 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = loadList.PK;
			outerPackage2.HVO_HVL_LoadList = loadList.PK;
			outerPackage3.HVO_HVL_LoadList = loadList.PK;
			var item1 = outerPackage1.Items.AddNew();
			var item2 = outerPackage1.Items.AddNew();
			var item3 = outerPackage1.Items.AddNew();
			var item4 = outerPackage2.Items.AddNew();
			var item5 = outerPackage2.Items.AddNew();
			var item6 = outerPackage2.Items.AddNew();
			var item7 = outerPackage3.Items.AddNew();
			var item8 = outerPackage3.Items.AddNew();
			var item9 = outerPackage3.Items.AddNew();
			item1.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_HVC_Consignment = consignment.PK;
			item4.HVI_HVC_Consignment = consignment.PK;
			item5.HVI_HVC_Consignment = consignment.PK;
			item6.HVI_HVC_Consignment = consignment.PK;
			item7.HVI_HVC_Consignment = consignment.PK;
			item8.HVI_HVC_Consignment = consignment.PK;
			item9.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_HVL_LoadList = loadList.PK;
			item2.HVI_HVL_LoadList = loadList.PK;
			item3.HVI_HVL_LoadList = loadList.PK;
			item4.HVI_HVL_LoadList = loadList.PK;
			item5.HVI_HVL_LoadList = loadList.PK;
			item6.HVI_HVL_LoadList = loadList.PK;
			item7.HVI_HVL_LoadList = loadList.PK;
			item8.HVI_HVL_LoadList = loadList.PK;
			item9.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			Assert("TryAttachToConsol should be successful", success);
			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			AssertNotNull("Expected a HVM shipment to be created", masterShipment);

			CombineAssertions("Expected HVM to HVM shipment mappings", () =>
			{
				AssertEquals("Expected AUSYD UNLOCO as HVM's origin depot", "AUSYD", masterShipment.Origin.Code);
				AssertEquals("Expected USLAX UNLOCO as HVM's destination depot", "USLAX", masterShipment.Destination.Code);
				AssertEquals("Expected originDepotOrg as HVM's shipper", originDepotAddress.Header.PK, masterShipment.Consignor.PK);
				AssertEquals("Expected destinationDepotOrg as HVM's consignee", destinationDepotAddress.Header.PK, masterShipment.Consignee.PK);
				AssertEquals("Expected XXX as HVM's service level", "DIR", masterShipment.ServiceLevel.RS_Code);
				AssertEquals("Expected 'Various Goods' as the default goods description", "Various Cargo", masterShipment.JS_GoodsDescription);
				AssertEquals("Expected 'HB1' to be HVM's house bill number", "HB1", masterShipment.JS_HouseBill);
				AssertEquals("Expected 'FOB' to be HVM's INCO term", "FOB", masterShipment.JS_INCO);
				AssertEquals("Expected only three outer package lines in HVM shipment", 3, masterShipment.OuterPackLines.Count);
				AssertEquals("Expected total outer packages to be three", 3, masterShipment.JS_OuterPacks);
				AssertEquals("Expected total packages to be nine", 9, masterShipment.JS_TotalPackageCount);
			});
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_ThenPopulateHVMShipmentPacklinesWithOuterPackages()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var outerPackageDestinationDepotAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			outerPackageDestinationDepotAddress1.OA_RL_NKRelatedPortCode = "USCHI";

			var outerPackageDestinationDepotAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			outerPackageDestinationDepotAddress2.OA_RL_NKRelatedPortCode = "USNYC";

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = originDepotAddress.PK;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepotAddress.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepotAddress.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_RS_NKServiceLevel = "XXX";
			loadList.HVL_HouseBillNumber = "HB1";
			loadList.HVL_INCO = IncoTerms.FreeOnBoard;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = loadList.PK;
			outerPackage1.HVO_RH_NKCommodityCode = "KFC";
			outerPackage1.HVO_PackageBarcode = "PROTEINBAR";
			outerPackage1.HVO_OA_DestinationDepot = outerPackageDestinationDepotAddress1.PK;
			outerPackage2.HVO_HVL_LoadList = loadList.PK;
			outerPackage2.HVO_RH_NKCommodityCode = "MAC";
			outerPackage2.HVO_PackageBarcode = "CHOCOBAR";
			outerPackage2.HVO_OA_DestinationDepot = outerPackageDestinationDepotAddress2.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			Assert("TryAttachToConsol should be successful", success);
			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			AssertNotNull("Expected a HVM shipment to be created", masterShipment);

			var masterShipmentPackLine1 = masterShipment.OuterPackLines.Cast<PackLine>().ToList()[0];
			var masterShipmentPackLine2 = masterShipment.OuterPackLines.Cast<PackLine>().ToList()[1];

			CombineAssertions("Mappings for PackLines", () =>
			{
				AssertEquals("Expected KFC for masterShipmentPackLine1 commodity code", "KFC", masterShipmentPackLine1.JL_RH_NKCommodityCode);
				AssertEquals("Expected MAC for masterShipmentPackLine2 commodity code", "MAC", masterShipmentPackLine2.JL_RH_NKCommodityCode);
				AssertEquals("Expected PROTEINBAR for masterShipmentPackLine1 ref number", "PROTEINBAR", masterShipmentPackLine1.JL_RefNumber);
				AssertEquals("Expected CHOCOBAR for masterShipmentPackLine2 ref number", "CHOCOBAR", masterShipmentPackLine2.JL_RefNumber);
				AssertEquals("Expected outerPackageDestinationDepotAddress1 for masterShipmentPackLine1 ref number", outerPackageDestinationDepotAddress1.PK, masterShipmentPackLine1.JL_OA_LastKnownTransitWarehouseAddress);
				AssertEquals("Expected outerPackageDestinationDepotAddress2 for masterShipmentPackLine2 ref number", outerPackageDestinationDepotAddress2.PK, masterShipmentPackLine2.JL_OA_LastKnownTransitWarehouseAddress);
			});
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouse_ThenPopulateHVMShipmentInnerPackageDetailWithOuterPackageItems()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = originDepotAddress.PK;
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment1.HVC_VolumeUQ = Volume.CubicMetres;
			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_WeightUQ = Weight.Kilograms;
			consignment2.HVC_VolumeUQ = Volume.CubicMetres;
			var consignment3 = header.Consignments.AddNew();
			consignment3.HVC_WeightUQ = Weight.Grams;
			consignment3.HVC_VolumeUQ = Volume.CubicDecimetres;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_OA_OriginDepot = originDepotAddress.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_RS_NKServiceLevel = "XXX";
			loadList.HVL_HouseBillNumber = "HB1";
			loadList.HVL_INCO = IncoTerms.FreeOnBoard;

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ActualWeight = 45m;
			item1.HVI_ActualVolume = 0.4m;
			item1.HVI_HVL_LoadList = loadList.PK;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_ActualWeight = 40m;
			item2.HVI_ActualVolume = 0.3m;
			item2.HVI_HVL_LoadList = loadList.PK;

			var item3 = consignment3.Items.AddNew();
			item3.HVI_ActualWeight = 9000m;
			item3.HVI_ActualVolume = 80m;
			item3.HVI_HVL_LoadList = loadList.PK;

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			Assert("TryAttachToConsol should be successful", success);

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			var masterShipmentInnerPackLine = masterShipment.InnerPackLines.Cast<PackLine>().Single();

			CombineAssertions("Populate inner packline", () =>
			{
				AssertEquals("Count", 3, masterShipmentInnerPackLine.JL_PackageCount);
				AssertEquals("Weight", 94m, masterShipmentInnerPackLine.JL_ActualWeight);
				AssertEquals("Volume", 0.78m, masterShipmentInnerPackLine.JL_ActualVolume);
			});
		}

		public void TestTryAttachToConsol_WhenLoadListHouseBillIsTooLong_ThenMapFirst25CharsToShipmentHouseBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_HouseBillNumber = "123456789012345678901234567890";

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			Assert("TryAttachToConsol should be successful", success);
			AssertEquals("Expected 'HB1' to be HVM's house bill number", "12345678901234567890", masterShipment.JS_HouseBill);
		}

		public void TestTryAttachToConsol_WhenLoadListIsMasterHouseWithNoHouseBill_ShipmentAutoGeneratesHouseBill()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUSYD";

			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_HouseBillNumber = string.Empty;
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();

			CombineAssertions(() =>
			{
				Assert("TryAttachToConsol should be successful", success);
				AssertEquals("HVM Shipment HBL will be auto-generated if load list HBL is empty", "S00001000", masterShipment.JS_HouseBill);
				AssertEquals("HVM Shipment Unique ID is the same as HBL", "S00001000", masterShipment.JS_UniqueConsignRef);
			});
		}

		public void TestTryAttachToConsol_WhenOuterPackageBarcodeIsTooLong_ThenMapFirst46CharsToPackLineRefNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;
			outerPackage.HVO_PackageBarcode = "12345678901234567890123456789012345678901234567890";

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			Assert("TryAttachToConsol should be successful", success);

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			AssertNotNull("Expected a HVM shipment to be created", masterShipment);

			var masterShipmentPackLine1 = masterShipment.OuterPackLines.Cast<PackLine>().FirstOrDefault();

			AssertEquals("Expected JL_RefNumber to be populated by the truncated value of HVO_PackageBarcode", "1234567890123456789012345678901234567890123456", masterShipmentPackLine1.JL_RefNumber);
		}

		public void TestTryAttachToConsol_ItemsInSameConsignmentAllocateToDifferentLoadList_CreateShipmentForEachLoadlist()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			item1.HVI_HVL_LoadList = loadList1.PK;
			item2.HVI_HVL_LoadList = loadList2.PK;

			Factory.Save();

			new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol1, new[] { loadList1 }, out _);
			Factory.Save();

			var shipments = Factory.Load<ForwardingShipment>(new ZQuery());
			AssertEquals("shipment has been created for the first load list", 1, shipments.Length);
			Assert("item1 has been allocated to shipment", shipments.Any(shipment => shipment.PK == item1.HVI_JS_LoadedOnShipment));

			new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol2, new[] { loadList2 }, out _);
			Factory.Save();

			shipments = Factory.Load<ForwardingShipment>(new ZQuery());
			AssertEquals("shipment has been created for the second load list", 2, shipments.Length);
			Assert("item2 has been allocated to shipment", shipments.Any(shipment => shipment.PK == item2.HVI_JS_LoadedOnShipment));
		}

		#region Waybill Number Tests

		public void TestTryAttachToConsol_WaybillNumbersUniqueInSameContainerItemGroup_ReturnTrue()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

			var consignment = CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, waybillNumber: "Waybill1");
			CreateConsignmentWithItem(bookingHeader2, originLoadList.PK, waybillNumber: "Waybill2");

			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);

			Assert("AttachToConsol should succeed", success);
			AssertNotContains("Not contain any error message", "Error", logger.ToString());
		}

		public void TestTryAttachToConsol_WaybillNumbersNotUniqueInDifferentContainerItemGroup_ReturnTrue()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "STD";

			CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, waybillNumber: "Waybill1");
			CreateConsignmentWithItem(bookingHeader2, originLoadList.PK, waybillNumber: "Waybill1");

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);

			Assert("AttachToConsol should succeed", success);
			AssertNotContains("Not contain any error message", "Error", logger.ToString());
		}

		public void TestTryAttachToConsol_WhenCombiningHVLWaybillNumbersNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages()
		{
			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader2";
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_BookingReference = "TestHeader1";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = bookingHeader1.HVH_RS_NKBookingServiceLevel;
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			Factory.Save();

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var masterBillNumber = "MBN20230202";
			var houseBillNumber = "S00001001";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RN_NKCountryCode = CountryCodes.UnitedStates;
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";
			InitRefZoneHeader(destinationDepot);

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;
			originLoadList.HVL_IsMasterHouse = true;
			originLoadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			originLoadList.HVL_HouseBillNumber = houseBillNumber;
			originLoadList.HVL_RS_NKServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, waybillNumber: "Waybill1");

			Factory.Save();
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);
			Assert("AttachToConsol should succeed", success);

			var newLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			newLoadList.HVL_Status = "LDG";
			newLoadList.HVL_MasterBillNumber = masterBillNumber;
			newLoadList.HVL_HouseBillNumber = houseBillNumber;
			newLoadList.HVL_IsMasterHouse = true;
			newLoadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			newLoadList.HVL_RS_NKServiceLevel = originLoadList.HVL_RS_NKServiceLevel;

			CreateConsignmentWithItem(bookingHeader2, newLoadList.PK, waybillNumber: "Waybill1");

			Factory.Save();
			var logger = new SimpleLogger();
			success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { newLoadList }, out _);
			var expectedErrorMessage =
				$@"Error: Error processing the HVLV Origin Load List '{newLoadList.HVL_UniqueReference}': Consignment Waybill number 'Waybill1' is repeated on booking headers: TestHeader1, TestHeader2";

			Assert("AttachToConsol should fail", !success);
			AssertContains("Error message contains", expectedErrorMessage, logger.ToString().Trim());
		}

		public void TestTryAttachToConsol_EmptyWaybillNumbersInSameContainerItemGroup_ReturnTrue()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

			var consignment1 = CreateConsignmentWithItem(bookingHeader1, originLoadList.PK);
			var consignment2 = CreateConsignmentWithItem(bookingHeader2, originLoadList.PK);

			Factory.Save();

			consignment1.HVC_WaybillNumber = string.Empty;
			consignment2.HVC_WaybillNumber = string.Empty;

			Factory.Save();

			CombineAssertions("Save existing consignment should allow empty waybill number", () =>
			{
				AssertEquals(string.Empty, consignment1.HVC_WaybillNumber);
				AssertEquals(string.Empty, consignment2.HVC_WaybillNumber);
			});

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);

			Assert("AttachToConsol should succeed", success);
			AssertNotContains("Not contain any error message", "Error", logger.ToString());
		}

		public void TestTryAttachToConsol_WaybillNumbersNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages()
		{
			AttachToConsol_WaybillNumbersNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages(false);
		}

		public void TestTryAttachToConsol_WaybillNumbersNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessagesForMaster()
		{
			AttachToConsol_WaybillNumbersNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages(true);
		}

		void AttachToConsol_WaybillNumbersNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages(bool isMasterLoadlist)
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";
			InitRefZoneHeader(destinationDepot);

			// Error messages only from first booking group with error.
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();

			// first load list -- no errors
			var masterBillNumber0 = "MBN20230202";

			var originLoadList0 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList0.HVL_Status = "LDG";
			originLoadList0.HVL_IsMasterHouse = isMasterLoadlist;
			originLoadList0.HVL_MasterBillNumber = masterBillNumber0;
			originLoadList0.HVL_OA_DestinationDepot = destinationDepot.PK;

			var bookingHeader11 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader12 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader11.HVH_BookingReference = "TestHeader11";
			bookingHeader12.HVH_BookingReference = "TestHeader12";
			bookingHeader12.HVH_OA_BillToParty = bookingHeader11.HVH_OA_BillToParty;
			bookingHeader11.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader12.HVH_RS_NKBookingServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader11, originLoadList0.PK, waybillNumber: "Waybill1");
			CreateConsignmentWithItem(bookingHeader11, originLoadList0.PK, waybillNumber: "Waybill2");

			// second load list -- containes duplicate waybill errors
			var masterBillNumber1 = "MBN20181227";

			var originLoadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList1.HVL_Status = "LDG";
			originLoadList1.HVL_IsMasterHouse = isMasterLoadlist;
			originLoadList1.HVL_MasterBillNumber = masterBillNumber1;
			originLoadList1.HVL_OA_DestinationDepot = destinationDepot.PK;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader1";
			bookingHeader2.HVH_BookingReference = "TestHeader2";
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader1, originLoadList1.PK, waybillNumber: "Waybill1");
			CreateConsignmentWithItem(bookingHeader1, originLoadList1.PK, waybillNumber: "Waybill2");

			// different bookingHeaderGroup because of different service level
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader4 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader5 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_BookingReference = "TestHeader3";
			bookingHeader4.HVH_BookingReference = "TestHeader4";
			bookingHeader5.HVH_BookingReference = "TestHeader5";
			bookingHeader4.HVH_OA_BillToParty = bookingHeader3.HVH_OA_BillToParty;
			bookingHeader5.HVH_OA_BillToParty = bookingHeader3.HVH_OA_BillToParty;
			bookingHeader3.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader4.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader5.HVH_RS_NKBookingServiceLevel = "STD";

			CreateConsignmentWithItem(bookingHeader3, originLoadList1.PK, waybillNumber: "Waybill1");
			CreateConsignmentWithItem(bookingHeader3, originLoadList1.PK, waybillNumber: "Waybill2-repeated");
			CreateConsignmentWithItem(bookingHeader3, originLoadList1.PK, waybillNumber: "Waybill3-repeated");
			CreateConsignmentWithItem(bookingHeader4, originLoadList1.PK, waybillNumber: "Waybill2-repeated");
			CreateConsignmentWithItem(bookingHeader4, originLoadList1.PK, waybillNumber: "Waybill3-repeated");
			CreateConsignmentWithItem(bookingHeader5, originLoadList1.PK, waybillNumber: "Waybill3-repeated");

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList0, originLoadList1 }, out _);

			var expectedErrorMessageContents1 =
				$@"Error processing the HVLV Origin Load List '{originLoadList1.HVL_UniqueReference}': Consignment Waybill number 'Waybill2-repeated' is repeated on booking headers: TestHeader3, TestHeader4";

			var expectedErrorMessageContents2 =
				$@"Error processing the HVLV Origin Load List '{originLoadList1.HVL_UniqueReference}': Consignment Waybill number 'Waybill3-repeated' is repeated on booking headers: TestHeader3, TestHeader4, TestHeader5";

			Assert("AttachToConsol should fail", !success);
			AssertContains("Contain excepted error message", expectedErrorMessageContents1, logger.ToString());
			AssertContains("Contain excepted error message", expectedErrorMessageContents2, logger.ToString());
		}

		#endregion

		#region Shipper Reference Tests

		public void TestTryAttachToConsol_ShipperReferencesUniqueInSameContainerItemGroup_ReturnTrue()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

			var consignment = CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, shipperReference: "ShipperRef1");
			CreateConsignmentWithItem(bookingHeader2, originLoadList.PK, shipperReference: "ShipperRef2");

			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);

			Assert("AttachToConsol should succeed", success);
			AssertNotContains("Not contain any error message", "Error", logger.ToString());
		}

		public void TestTryAttachToConsol_ShipperReferencesNotUniqueInDifferentContainerItemGroup_ReturnTrue()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "STD";

			CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, shipperReference: "ShipperRef1");
			CreateConsignmentWithItem(bookingHeader2, originLoadList.PK, shipperReference: "ShipperRef1");

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);

			Assert("AttachToConsol should succeed", success);
			AssertNotContains("Not contain any error message", "Error", logger.ToString());
		}

		public void TestTryAttachToConsol_ShipperReferencesNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages()
		{
			AttachToConsol_ShipperReferencesNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages(false);
		}

		public void TestTryAttachToConsol_ShipperReferencesNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessagesForMaster()
		{
			AttachToConsol_ShipperReferencesNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages(true);
		}

		public void AttachToConsol_ShipperReferencesNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages(bool isMasterLoadlist)
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "USLAX";
			InitRefZoneHeader(destinationDepot);

			// Error messages only from first booking group with error.
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();

			// first load list -- no errors
			var masterBillNumber0 = "MBN20230202";

			var originLoadList0 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList0.HVL_Status = "LDG";
			originLoadList0.HVL_IsMasterHouse = isMasterLoadlist;
			originLoadList0.HVL_MasterBillNumber = masterBillNumber0;
			originLoadList0.HVL_OA_DestinationDepot = destinationDepot.PK;

			var bookingHeader11 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader12 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader11.HVH_BookingReference = "TestHeader11";
			bookingHeader12.HVH_BookingReference = "TestHeader12";
			bookingHeader12.HVH_OA_BillToParty = bookingHeader11.HVH_OA_BillToParty;
			bookingHeader11.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader12.HVH_RS_NKBookingServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader11, originLoadList0.PK, shipperReference: "ShipperRef1");
			CreateConsignmentWithItem(bookingHeader11, originLoadList0.PK, shipperReference: "ShipperRef2");

			// second load list -- containes duplicate shipper reference errors
			var masterBillNumber1 = "MBN20181227";

			var originLoadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList1.HVL_Status = "LDG";
			originLoadList1.HVL_IsMasterHouse = isMasterLoadlist;
			originLoadList1.HVL_MasterBillNumber = masterBillNumber1;
			originLoadList1.HVL_OA_DestinationDepot = destinationDepot.PK;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader1";
			bookingHeader2.HVH_BookingReference = "TestHeader2";
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader1, originLoadList1.PK, shipperReference: "ShipperRef1");
			CreateConsignmentWithItem(bookingHeader1, originLoadList1.PK, shipperReference: "ShipperRef2");

			// different bookingHeaderGroup because of different service level
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader4 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader5 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_BookingReference = "TestHeader3";
			bookingHeader4.HVH_BookingReference = "TestHeader4";
			bookingHeader5.HVH_BookingReference = "TestHeader5";
			bookingHeader4.HVH_OA_BillToParty = bookingHeader3.HVH_OA_BillToParty;
			bookingHeader5.HVH_OA_BillToParty = bookingHeader3.HVH_OA_BillToParty;
			bookingHeader3.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader4.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader5.HVH_RS_NKBookingServiceLevel = "STD";

			CreateConsignmentWithItem(bookingHeader3, originLoadList1.PK, shipperReference: "ShipperRef1");
			CreateConsignmentWithItem(bookingHeader3, originLoadList1.PK, shipperReference: "ShipperRef2-repeated");
			CreateConsignmentWithItem(bookingHeader3, originLoadList1.PK, shipperReference: "ShipperRef3-repeated");
			CreateConsignmentWithItem(bookingHeader4, originLoadList1.PK, shipperReference: "ShipperRef2-repeated");
			CreateConsignmentWithItem(bookingHeader4, originLoadList1.PK, shipperReference: "ShipperRef3-repeated");
			CreateConsignmentWithItem(bookingHeader5, originLoadList1.PK, shipperReference: "ShipperRef3-repeated");

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList0, originLoadList1 }, out _);

			var expectedErrorMessage1 =
				$@"Error processing the HVLV Origin Load List '{originLoadList1.HVL_UniqueReference}': Consignment Shipper Reference 'ShipperRef2-repeated' is repeated on booking headers: TestHeader3, TestHeader4";

			var expectedErrorMessage2 =
				$@"Error processing the HVLV Origin Load List '{originLoadList1.HVL_UniqueReference}': Consignment Shipper Reference 'ShipperRef3-repeated' is repeated on booking headers: TestHeader3, TestHeader4, TestHeader5";

			Assert("AttachToConsol should fail", !success);
			AssertContains("Not contain any error message", expectedErrorMessage1, logger.ToString());
			AssertContains("Not contain any error message", expectedErrorMessage2, logger.ToString());
		}

		public void TestTryAttachToConsol_WaybillNumberAndShipperReferencesNotUniqueInSameContainerItemGroup_ReturnFalseAndErrorMessages()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = "MBN20181227";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader1";
			bookingHeader2.HVH_BookingReference = "TestHeader2";
			bookingHeader3.HVH_BookingReference = "TestHeader3";
			bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader3.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader3.HVH_RS_NKBookingServiceLevel = "STD";

			CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, shipperReference: "ShipperRef2-repeated");
			CreateConsignmentWithItem(bookingHeader1, originLoadList.PK, waybillNumber: "WaybillNumber3-repeated");
			CreateConsignmentWithItem(bookingHeader2, originLoadList.PK, shipperReference: "ShipperRef2-repeated");
			CreateConsignmentWithItem(bookingHeader2, originLoadList.PK, waybillNumber: "WaybillNumber3-repeated");
			CreateConsignmentWithItem(bookingHeader3, originLoadList.PK, waybillNumber: "WaybillNumber3-repeated");

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(forwardingConsol, new[] { originLoadList }, out _);

			var expectedErrorMessage =
				$@"Error processing the HVLV Origin Load List '{originLoadList.HVL_UniqueReference}': Consignment Waybill number 'WaybillNumber3-repeated' is repeated on booking headers: TestHeader1, TestHeader2, TestHeader3
Error processing the HVLV Origin Load List '{originLoadList.HVL_UniqueReference}': Consignment Shipper Reference 'ShipperRef2-repeated' is repeated on booking headers: TestHeader1, TestHeader2";

			Assert("AttachToConsol should fail", !success);
			AssertContains("Contain error message", expectedErrorMessage, logger.ToString());
		}

		#endregion

		public void TestTryAttachToConsol_LoadListsTRFLogHasRFNSetToConsolUniqueRefNumber()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			var containerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-1")).PK;
			var loadList = CreateLoadList(TransportModes.Air, "CONT123", containerType, destinationDepot);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000054515";

			AssertEquals("Expected 0 Reference for C000054515", 0, loadList.Logs.Find(x => x.SL_Reference == "|RFN=C000054515").Count());
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			Factory.Save();
			Assert("TryAttachToConsol should be successful", success);
			AssertEquals("Expected 1 Reference for C000054515", 1, loadList.Logs.Find(x => x.SL_Reference == "|RFN=C000054515").Count());
		}

		public void TestTryAttachToConsol_GivenMasterHouseLoadList_ThenPackLinePackageCountIsAlwaysOne()
		{
			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = originDepotAddress.PK;
			var consignment = header.Consignments.AddNew();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;
			var loadListDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			loadList.HVL_OA_DestinationDepot = loadListDestinationDepot.PK;

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;

			var item1 = outerPackage.Items.AddNew();
			var item2 = outerPackage.Items.AddNew();
			var item3 = outerPackage.Items.AddNew();
			item1.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_HVL_LoadList = loadList.PK;
			item2.HVI_HVL_LoadList = loadList.PK;
			item3.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();
			var masterShipmentPackLine = masterShipment.OuterPackLines.Cast<PackLine>().FirstOrDefault();

			Assert("TryAttachToConsol should be successful", success);
			AssertEquals("Expected JL_PackageCount to be one always because we map one HVO to one ForwardingPackLine", 1, masterShipmentPackLine.JL_PackageCount);
		}

		public void TestTryAttachToConsol_NoExceptionWhenLoadListDestinationDepotIsNull()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var loadList = Factory.New<HVLVOriginLoadList>();

			AssertNull("Precondition: loadlist destination depot is null", loadList.DestinationDepot);
			AssertNoExceptionThrown("No exception when loadList destination depot is null", () =>
			{
				HVLVOriginLoadListHelper.GetPortUNLOCOFromConsignment(consignment, loadList);
			});
		}

		public void TestTryAttachToConsol_NoExceptionWhenMasterHouseLoadListDestinationDepotIsNull()
		{
			var consol = Factory.New<ForwardingConsol>();
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;

			AssertNull("Precondition: loadlist destination depot is null", loadList.DestinationDepot);
			AssertNoExceptionThrown("No exception when loadList destination depot is null", () =>
			{
				new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			});
		}

		public void TestTryAttachToConsol_WhenMergingMasterShipmentIncrementsShipmentPacksAndInnersCorrectly()
		{
			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = originDepotAddress.PK;
			var consignment = header.Consignments.AddNew();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;
			var loadListDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			loadList.HVL_OA_DestinationDepot = loadListDestinationDepot.PK;

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;

			var item = outerPackage.Items.AddNew();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);

			Factory.Save();

			var masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();

			CombineAssertions("Preconditions:", () =>
			{
				Assert("TryAttachToConsol is successful", success);
				AssertEquals("JS_OuterPacks is 1 (1 outer package)", 1, masterShipment.JS_OuterPacks);
				AssertEquals("JS_TotalPackageCount is 1 (1 item)", 1, masterShipment.JS_TotalPackageCount);
			});

			var loadListToMerge = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListToMerge.HVL_IsMasterHouse = true;
			loadListToMerge.HVL_TransportMode = TransportModes.Air;
			loadListToMerge.HVL_OA_DestinationDepot = loadListDestinationDepot.PK;

			var outerPackageToMerge = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackageToMerge.HVO_HVL_LoadList = loadListToMerge.PK;

			var newConsignment = header.Consignments.AddNew();

			var firstItemToMerge = newConsignment.Items.AddNew();
			firstItemToMerge.HVI_HVO_OuterPackage = outerPackageToMerge.PK;
			firstItemToMerge.HVI_HVL_LoadList = loadListToMerge.PK;

			var secondItemToMerge = newConsignment.Items.AddNew();
			secondItemToMerge.HVI_HVO_OuterPackage = outerPackageToMerge.PK;
			secondItemToMerge.HVI_HVL_LoadList = loadListToMerge.PK;

			var thirdItemToMerge = newConsignment.Items.AddNew();
			thirdItemToMerge.HVI_HVO_OuterPackage = outerPackageToMerge.PK;
			thirdItemToMerge.HVI_HVL_LoadList = loadListToMerge.PK;

			Factory.Save();

			success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadListToMerge }, out _);

			Factory.Save();

			masterShipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM")).Single();

			CombineAssertions("Counts are updated", () =>
			{
				Assert("TryAttachToConsol is successful", success);
				AssertEquals("JS_OuterPacks is 2 (2 outer packages)", 2, masterShipment.JS_OuterPacks);
				AssertEquals("JS_TotalPackageCount is 4 (4 items)", 4, masterShipment.JS_TotalPackageCount);
			});
		}

		public void TestTryAttachToConsol_HVH_IsProcessedAtOriginDepotSetTrueBeforeProcessingItems_MasterHouse()
		{
			TryAttachToConsolAndAssert_HVH_IsProcessedAtOriginDepotSetTrueBeforeProcessingItems(isMasterHouse: true);
		}

		public void TestTryAttachToConsol_HVH_IsProcessedAtOriginDepotSetTrueBeforeProcessingItems()
		{
			TryAttachToConsolAndAssert_HVH_IsProcessedAtOriginDepotSetTrueBeforeProcessingItems(isMasterHouse: false);
		}

		void TryAttachToConsolAndAssert_HVH_IsProcessedAtOriginDepotSetTrueBeforeProcessingItems(bool isMasterHouse)
		{
			var errorReporter = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object))
			using (new DisposableAction(() => Globals.IsTest_ForTest.ResetValue()))
			{
				var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();

				var etailer = Factory.NewWithValidTestData<OrgAddress>();
				var originDepot = Factory.NewWithValidTestData<OrgAddress>();
				var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();

				originDepot.OA_RL_NKRelatedPortCode = "USCHI";
				destinationDepot.OA_RL_NKRelatedPortCode = "AUSYD";

				bookingHeader1.HVH_OA_BillToParty = etailer.PK;
				bookingHeader1.HVH_BookingReference = "A";
				bookingHeader2.HVH_OA_BillToParty = etailer.PK;
				bookingHeader2.HVH_BookingReference = "B";

				var consignment1 = bookingHeader1.Consignments.AddNew();
				var item1 = consignment1.Items.AddNew();
				var consignment2 = bookingHeader2.Consignments.AddNew();
				var item2 = consignment2.Items.AddNew();

				var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
				loadList.HVL_IsMasterHouse = isMasterHouse;
				loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
				loadList.HVL_OA_OriginDepot = originDepot.PK;
				loadList.HVL_RS_NKServiceLevel = "STD";

				item1.HVI_HVL_LoadList = loadList.PK;
				item2.HVI_HVL_LoadList = loadList.PK;

				Factory.Save();

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var helper = new HVLVOriginLoadListHelper(new DummyLogger());
				Globals.IsTest_ForTest.Value = false;

				var success = helper.TryAttachToConsol(consol, new[] { loadList }, out _);
				Assert(success);
			}

			errorReporter.Verify(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		public void TestTryAttachToConsol_ShouldOnlyProcessItemsNotLoadedOnShipment()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.OH_RL_NKClosestPort = "AUSYD";

			var existingShipment = Factory.New<ForwardingShipment>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var loadList = CreateLoadList(TransportModes.Air, "CONT123", ZGuid.Empty, destinationDepot);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_RL_NKClosestPort = "USLAX";

			var header = CreateBookingHeader(shipper, "STD");

			var itemAlreadyLoadedOnShipment = CreateItem("ITEM1", header, loadList, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", PkgUnit.Box);
			itemAlreadyLoadedOnShipment.HVI_JS_LoadedOnShipment = existingShipment.PK;
			var itemToLoad = CreateItem("ITEM2", header, loadList, 10, Weight.Kilograms, 0.1, Volume.CubicMetres, 1, "AUD", PkgUnit.Box);

			Factory.Save();

			var logger = new SimpleLogger();
			var success = new HVLVOriginLoadListHelper(logger).TryAttachToConsol(consol, new[] { loadList }, out _);
			Assert(success);

			var newShipment = consol.Shipments.Cast<ForwardingShipment>().SingleOrDefault();
			AssertNotNull("New Shipment should be created", newShipment);
			var newShipmentItems = consol.Shipments.Cast<ForwardingShipment>().SingleOrDefault().HVLVItems;

			CombineAssertions(() =>
			{
				AssertEquals("One item processed into consol shipment", 1, newShipmentItems.Count());
				AssertEquals("item2 processed into consol shipment", itemToLoad.PK, newShipmentItems.SingleOrDefault().PK);
				AssertContains($@"Warning: HVLV Item with ID {itemAlreadyLoadedOnShipment.HVI_ItemId} has been skipped as it is already loaded on Shipment with ID {existingShipment.JS_UniqueConsignRef}", logger.ToString().Trim());
				AssertNotContains($@"Warning: HVLV Item with ID {itemToLoad.HVI_ItemId} has been skipped as it is already loaded on Shipment with ID ", logger.ToString().Trim());
			});
		}

		public void TestTryAttachToConsol_AllShipmentHaveOwnExportBroker_MasterHouse()
		{
			TryAttachToConsolAndAssert_AllShipmentHaveOwnExportBroker(true);
		}

		public void TestTryAttachToConsol_AllShipmentHaveOwnExportBroker()
		{
			TryAttachToConsolAndAssert_AllShipmentHaveOwnExportBroker(false);
		}

		void TryAttachToConsolAndAssert_AllShipmentHaveOwnExportBroker(bool isMasterHouse)
		{
			var relatedOrgHeader = Factory.New<OrgHeader>();
			relatedOrgHeader.OH_Code = "ACCGPREPER";
			relatedOrgHeader.OH_IsBroker = true;
			relatedOrgHeader.OH_IsConsignee = true;
			relatedOrgHeader.OH_IsConsignor = true;
			relatedOrgHeader.OH_IsActive = true;
			relatedOrgHeader.OH_RL_NKClosestPort = "AUSYD";

			var orgHeaderOrigin = Factory.New<OrgHeader>();
			orgHeaderOrigin.OH_Code = "WTGNN";
			orgHeaderOrigin.OH_IsBroker = true;
			orgHeaderOrigin.OH_IsConsignee = true;
			orgHeaderOrigin.OH_IsConsignor = true;
			orgHeaderOrigin.OH_IsActive = true;
			orgHeaderOrigin.OH_RL_NKClosestPort = "AUSYD";
			orgHeaderOrigin.AddRelatedParty(relatedOrgHeader.PK, "CAB", "PIC", "AIR", "", GlbCompany.CurrentCompany);

			var orgOriginAddress = orgHeaderOrigin.Addresses.AddNew();
			orgOriginAddress.OA_RN_NKCountryCode = "AU";
			orgOriginAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgOriginAddress.OA_Address1 = "Sydney Under the seashore";

			var orgHeaderDestination1 = Factory.New<OrgHeader>();
			orgHeaderDestination1.OH_Code = "YOUCHISHA";
			orgHeaderDestination1.OH_IsBroker = true;
			orgHeaderDestination1.OH_IsConsignee = true;
			orgHeaderDestination1.OH_IsConsignor = true;
			orgHeaderDestination1.OH_IsActive = true;
			orgHeaderDestination1.OH_RL_NKClosestPort = "CNSHA";

			var orgDestination1Address = orgHeaderDestination1.Addresses.AddNew();
			orgDestination1Address.OA_RN_NKCountryCode = "CN";
			orgDestination1Address.OA_RL_NKRelatedPortCode = "CNSHA";
			orgDestination1Address.OA_Address1 = "Shanghai, No.62 Linping Road";

			var orgHeaderDestination2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderDestination2.OH_Code = "ACCGREHKG";
			orgHeaderDestination2.OH_IsBroker = true;
			orgHeaderDestination2.OH_IsConsignee = true;
			orgHeaderDestination2.OH_IsConsignor = true;
			orgHeaderDestination2.OH_IsActive = true;
			orgHeaderDestination2.OH_RL_NKClosestPort = "HKHKG";

			var orgDestination2Address = orgHeaderDestination2.Addresses.AddNew();
			orgDestination2Address.OA_RN_NKCountryCode = "HK";
			orgDestination2Address.OA_RL_NKRelatedPortCode = "HKHKG";
			orgDestination2Address.OA_Address1 = "Hongkong, Marry Street";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "CNSHA";
			loadList.HVL_Status = "LDG";
			loadList.HVL_OA_DestinationDepot = orgDestination1Address.PK;
			loadList.HVL_OA_OriginDepot = orgOriginAddress.PK;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = orgOriginAddress.PK;
			bookingHeader.HVH_OA_DispatchAddress = orgOriginAddress.PK;
			bookingHeader.HVH_OA_OriginDepot = orgDestination1Address.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_OA_ConsigneeAddress = orgDestination1Address.PK;
			consignment1.HVC_OA_ShipperAddress = orgOriginAddress.PK;

			var item1 = consignment1.Items.AddNew();
			item1.HVI_HVL_LoadList = loadList.PK;

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_OA_ConsigneeAddress = orgDestination2Address.PK;
			consignment2.HVC_OA_ShipperAddress = orgOriginAddress.PK;

			var item2 = consignment2.Items.AddNew();
			item2.HVI_HVL_LoadList = loadList.PK;

			var internationalZone = Factory.NewWithValidTestData<RefZoneHeader>();
			internationalZone.FZ_Code = "HVLV";
			internationalZone.FZ_IsActive = true;
			internationalZone.FZ_OH_RelatedParty = orgHeaderDestination1.PK;
			internationalZone.FZ_ZoneType = "HVL";
			internationalZone.FZ_Description = "HVLV GATEWAY";

			var query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.RL_Code, "CNSHA");
			var cn_UNLOCO = new BusinessObjectFactory().LoadTop1<RefUNLOCO>(query);

			query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.RL_Code, "HKHKG");
			var hk_UNLOCO = new BusinessObjectFactory().LoadTop1<RefUNLOCO>(query);

			var refZone1 = Factory.New<RefZonePivot>();
			refZone1.F2_FZ = internationalZone.PK;
			refZone1.F2_ParentID = cn_UNLOCO.PK;
			refZone1.F2_ParentTableCode = "RL";

			var refZone2 = Factory.New<RefZonePivot>();
			refZone2.F2_FZ = internationalZone.PK;
			refZone2.F2_ParentID = hk_UNLOCO.PK;
			refZone2.F2_ParentTableCode = "RL";

			Factory.Save();

			consol = (ForwardingConsol)new HVLVOriginLoadListHelper(new DummyLogger()).CreateConsol(loadList);
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(consol, new[] { loadList }, out _);
			Assert(success);

			Factory.Save();

			query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, "CNSHA");
			query.AddToFilter(JobShipmentSchema.JS_ShipmentType, "HVL");
			var shipment = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(query);
			AssertEquals("Export broker of the HVL shipment who has the same destination with the consolidation has a default value.", relatedOrgHeader.PK, shipment.JS_OH_ExportBroker);

			if (isMasterHouse)
			{
				query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, "HKHKG");
				shipment = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(query);
				AssertEquals("Export broker of the HVL shipment who has a different destination with the consolidation has a default value.", relatedOrgHeader.PK, shipment.JS_OH_ExportBroker);

				query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, "CNSHA");
				query.AddToFilter(JobShipmentSchema.JS_ShipmentType, "HVM");
				shipment = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(query);
				AssertEquals("Export broker of the HVM shipment who has the same destination with the consolidation has a default value.", relatedOrgHeader.PK, shipment.JS_OH_ExportBroker);
			}
		}

		public void TestGetConsolContainerMode()
		{
			var loadlist = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			loadlist.HVL_TransportMode = "SEA";
			AssertEquals("Consol container mode should return GRP", "GRP", HVLVOriginLoadListHelper.GetConsolContainerMode(loadlist));

			loadlist.HVL_TransportMode = "AIR";
			AssertEquals("Consol container mode should return LSE", "LSE", HVLVOriginLoadListHelper.GetConsolContainerMode(loadlist));

			loadlist.HVL_TransportMode = "AIR";
			loadlist.HVL_ContainerNumber = "test";
			loadlist.HVL_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-1")).PK;
			AssertEquals("Consol container mode should return ULD", "ULD", HVLVOriginLoadListHelper.GetConsolContainerMode(loadlist));

			loadlist.HVL_TransportMode = "ROA";
			AssertEquals("Consol container mode should return LTL", "LTL", HVLVOriginLoadListHelper.GetConsolContainerMode(loadlist));

			loadlist.HVL_TransportMode = "RAI";
			AssertEquals("Consol container mode should return LCL", "LCL", HVLVOriginLoadListHelper.GetConsolContainerMode(loadlist));
		}

		#region Implementation

		ForwardingConsol consol;
		ForwardingShipment hvlShipment;
		ForwardingShipment hvmShipment;
		OrgAddress shipperAddress;
		HVLVBookingHeader bookingHeader;
		HVLVOriginLoadList matchingMasterLoadList;

		HVLVOriginLoadList CreateLoadList(ZString transportMode, ZString containerNumber, ZGuid containerType, OrgHeader destinationDepot)
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = transportMode;
			loadList.HVL_ContainerNumber = containerNumber;
			loadList.HVL_RC_ContainerType = containerType;
			loadList.HVL_OA_DestinationDepot = destinationDepot.MainAddress.PK;

			return loadList;
		}

		protected HVLVBookingHeader CreateBookingHeader(OrgHeader shipper, ZString serviceLevel)
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = shipper.MainAddress.PK;
			header.HVH_RS_NKBookingServiceLevel = serviceLevel;

			return header;
		}

		HVLVConsignment CreateConsignmentWithItem(HVLVBookingHeader header, ZGuid loadListPK, string waybillNumber = "", string shipperReference = "", string destinationCountryCode = "US")
		{
			var consignment = header.Consignments.AddNew();
			consignment.HVC_RN_NKConsigneeCountryCode = destinationCountryCode;

			if (!string.IsNullOrEmpty(waybillNumber))
			{
				consignment.HVC_WaybillNumber = waybillNumber;
			}

			if (!string.IsNullOrEmpty(shipperReference))
			{
				consignment.HVC_ShipperReference = shipperReference;
			}

			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadListPK;

			return consignment;
		}

		protected HVLVItem CreateItem(ZString itemId, HVLVBookingHeader header, HVLVOriginLoadList loadList, ZDecimal weight, ZString weightUnit, ZDecimal volume, ZString volumeUnit,
			ZDecimal goodsValue, ZString goodsValueCurrency, ZString packType, string consignmentCountryCode = "US")
		{
			var consignment = header.Consignments.AddNew();
			consignment.HVC_GoodsValue = goodsValue;
			consignment.HVC_RX_NKGoodsValueCurrency = goodsValueCurrency;
			consignment.HVC_WeightUQ = weightUnit;
			consignment.HVC_VolumeUQ = volumeUnit;
			consignment.HVC_RN_NKConsigneeCountryCode = consignmentCountryCode;

			var item = consignment.Items.AddNew();
			item.HVI_ItemId = itemId;
			item.HVI_HVL_LoadList = loadList?.PK ?? ZGuid.Empty;
			item.HVI_ActualWeight = weight;
			item.HVI_ActualVolume = volume;
			item.HVI_F3_NKPackType = packType;

			return item;
		}

		RefZoneHeader InitRefZoneHeader(OrgAddress depot)
		{
			var zoneHeader = Factory.New<RefZoneHeader>();
			zoneHeader.FZ_Code = "code";
			zoneHeader.FZ_Description = "Sumink to copy like";
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway;

			var losAngeles = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			zoneHeader.UNLOCOs.Add(losAngeles);
			zoneHeader.UNLOCOs.Add(sydney);
			zoneHeader.FZ_OH_RelatedParty = depot.Header.PK;

			return zoneHeader;
		}

		#endregion
	}
}
