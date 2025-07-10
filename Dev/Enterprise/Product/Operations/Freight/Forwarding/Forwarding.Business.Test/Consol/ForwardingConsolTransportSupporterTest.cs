using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolTransportSupporterTest : TransportSupporterTestCase<ForwardingConsolTransportSupporter<ForwardingConsol>>
	{
		public void TestUpdateLoadAndDishargePort()
		{
			var bridge = new FreightConsolTest.AirCargoBridgeTestHelper();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.SetAirCargoSynchroniserRetriever(delegate
			{ return bridge; });

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "TEST1";
			AssertEquals("TEST1", bridge.LoadPortData);
			AssertEquals("", bridge.DischargePortData);

			transport.JW_RL_NKDiscPort = "TEST2";
			AssertEquals("TEST1", bridge.LoadPortData);
			AssertEquals("TEST2", bridge.DischargePortData);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			AssertEquals("", bridge.LoadPortData);
			AssertEquals("", bridge.DischargePortData);
		}

		public void TestDefaultMAWBOnConsolWhenFlightNumberChanges()
		{
			JobMawb mawb1 = CreateMAWB(AirNZCarrier, "55555625");
			JobMawb mawb2 = CreateMAWB(SingaporeCarrier, "55555626");
			JobMawb mawb3 = CreateMAWB(QantasCarrier, "55555627");

			Factory.Save();

			Transport transport1 = Consol.Transports[0];
			transport1.JW_VoyageFlight = "NZ01";
			Factory.Save();

			AssertEquals("Consol MAWB defaulted", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport1.JW_TransportType = Constants.TransportPlanningType.Flight2;
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport2.JW_VoyageFlight = "QF01";

			AssertEquals("Consol MAWB not changed, as it corresponds to another flight leg", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport2.JW_TransportType = Constants.TransportPlanningType.Flight2;
			transport1.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport1.JW_VoyageFlight = "SQ01";
			Factory.Save();

			AssertEquals("Consol MAWB not changed, as although it no longer corresponds to any other flight leg, the previous value is not empty", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			Consol.MasterBillMAWB = ZString.Empty;
			transport1.JW_VoyageFlight = "SQ02";
			Factory.Save();

			AssertEquals("Consol MAWB changed, as it no longer corresponds to any other flight leg, the previous value is empty", mawb2.JM_Airline3DigitPrefix + mawb2.JM_MAWB, Consol.JK_MasterBillNum);
		}

		public void TestDefaultMAWBOnConsolWhenFlightTypeChanges()
		{
			JobMawb mawb1 = CreateMAWB(AirNZCarrier, "55555625");
			JobMawb mawb2 = CreateMAWB(SingaporeCarrier, "55555626");
			JobMawb mawb3 = CreateMAWB(QantasCarrier, "55555627");

			Factory.Save();

			Transport transport1 = Consol.Transports[0];
			transport1.JW_VoyageFlight = "NZ01";
			Factory.Save();

			AssertEquals("Consol MAWB defaulted", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport1.JW_TransportType = Constants.TransportPlanningType.Flight2;
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport2.JW_VoyageFlight = "QF01";
			transport1.JW_VoyageFlight = "SQ01";

			AssertEquals("Consol MAWB not changed, as it corresponds to another flight leg and flight changed is not Flight1 type", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport2.JW_TransportType = Constants.TransportPlanningType.Flight2;
			transport1.JW_TransportType = Constants.TransportPlanningType.Flight1;
			Factory.Save();

			AssertEquals("Consol MAWB changed, as it no longer corresponds to any other flight leg", mawb2.JM_Airline3DigitPrefix + mawb2.JM_MAWB, Consol.JK_MasterBillNum);
		}

		public void TestDefaultMAWBOnConsolWhenFlightLoadChanges()
		{
			JobMawb mawb1 = CreateMAWB(AirNZCarrier, "55555625");
			JobMawb mawb2 = CreateMAWB(SingaporeCarrier, "55555626");
			JobMawb mawb3 = CreateMAWB(QantasCarrier, "55555627");

			Factory.Save();

			Transport transport1 = Consol.Transports[0];
			transport1.JW_VoyageFlight = "NZ01";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			Factory.Save();

			AssertEquals("Consol MAWB defaulted", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport1.JW_TransportType = Constants.TransportPlanningType.Flight2;
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport2.JW_VoyageFlight = "QF01";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			AssertEquals("Consol MAWB not changed, as it corresponds to another flight leg", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport2.JW_RL_NKLoadPort = "AUMEL";

			AssertEquals("Consol MAWB still not changed, as it corresponds to another flight leg", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight2;
			transport1.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport1.JW_VoyageFlight = "SQ01";
			Factory.Save();

			AssertEquals("Consol MAWB not changed, as although it no longer corresponds to any other flight leg, the previous value is not empty", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			Consol.MasterBillMAWB = ZString.Empty;
			transport1.JW_VoyageFlight = "SQ02";
			Factory.Save();

			AssertEquals("Consol MAWB changed, as it no longer corresponds to any other flight leg, the previous value is empty", mawb2.JM_Airline3DigitPrefix + mawb2.JM_MAWB, Consol.JK_MasterBillNum);
		}

		public void TestDefaultMAWBOnConsolWhenSailingChanges()
		{
			JobMawb mawb1 = CreateMAWB(AirNZCarrier, "55555625");
			JobMawb mawb2 = CreateMAWB(SingaporeCarrier, "55555626");
			JobMawb mawb3 = CreateMAWB(QantasCarrier, "55555627");

			Factory.Save();

			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "NZ01";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "QF01";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NZAKL";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			JobVoyage voyage3 = Factory.New<JobVoyage>();
			voyage3.JV_VoyageFlight = "SQ01";
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_JX = voyage1.Sailings[0].PK;
			Factory.Save();

			AssertEquals("Consol MAWB defaulted", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport1.JW_TransportType = Constants.TransportPlanningType.Flight2;
			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport2.JW_JX = voyage2.Sailings[0].PK;

			AssertEquals("Consol MAWB not changed, as it corresponds to another flight leg", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			transport2.JW_TransportType = Constants.TransportPlanningType.Flight2;
			transport1.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport1.JW_JX = voyage3.Sailings[0].PK;
			Factory.Save();

			AssertEquals("Consol MAWB not changed, as although it no longer corresponds to any other flight leg, the previous value is not empty", mawb1.JM_Airline3DigitPrefix + mawb1.JM_MAWB, Consol.JK_MasterBillNum);

			Consol.MasterBillMAWB = ZString.Empty;
			transport1.JW_VoyageFlight = "SQ02";
			Factory.Save();

			AssertEquals("Consol MAWB changed, as it no longer corresponds to any other flight leg, the previous value is empty", mawb2.JM_Airline3DigitPrefix + mawb2.JM_MAWB, Consol.JK_MasterBillNum);
		}

		public void TestIsAnyFlightMatchesMAWB()
		{
			var airlineWithEmptyCode = Factory.NewWithValidTestData<RefAirline>();
			airlineWithEmptyCode.RM_TwoCharacterCode = "";

			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			Consol.MasterBillAirlinePrefix = ZString.Empty;

			var transport1 = Consol.Transports.AddNew();
			transport1.JW_VoyageFlight = "";

			var transport2 = Consol.Transports[0];
			transport2.JW_VoyageFlight = "QF001";
			transport2.JW_JX = voyage1.Sailings[0].PK;

			AssertEquals("Correct airline has been defaulted", "081", Consol.MasterBillAirlinePrefix);
		}

		public void TestDefaultMasterBillAirlinePrefix_MultiAWBMasterIsIgnored()
		{
			Consol.MasterBillAirlinePrefix = ZString.Empty;

			Consol.JK_AgentType = Constants.AgentType.AWBMaster;
			Consol.Transports[0].JW_VoyageFlight = "QF001";

			AssertEquals("MasterBillAirlinePrefix was not defaulted", ZString.Empty, Consol.MasterBillAirlinePrefix);

			Consol.JK_AgentType = Constants.AgentType.AWBCoload;
			Consol.Transports[0].JW_VoyageFlight = "QF002";

			AssertEquals("MasterBillAirlinePrefix was defaulted", "081", Consol.MasterBillAirlinePrefix);
		}

		public void TestShipmentInspectionTypeIsRecalculatedOnLoadOrModeChange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				Consol.JK_TransportMode = "AIR";
				var shipment = Consol.Shipments.AddNew();

				var frAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "FR"));
				var approval = frAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = frAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "AC";

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = frAccountConsignor.MainAddress.PK;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "FRPAR";
				transport.JW_RL_NKDiscPort = "SGSIN";

				AssertEquals("Shipment is approved for export from France", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_RL_NKLoadPort = "BEBRU";
				AssertEquals("Shipment is not approved for uplift in Belgium", "UNK", shipment.JS_InspectionTypeCode);

				transport.JW_TransportMode = "ROA";
				AssertEquals("Shipment is approved for road freight in Belgium", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestShipmentInspectionTypeIsRecalculatedOnETDChange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				Consol.JK_TransportMode = "AIR";
				var shipment = Consol.Shipments.AddNew();

				var frAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "FR"));
				var approval = frAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = frAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = frAccountConsignor.MainAddress.PK;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "FRPAR";
				transport.JW_RL_NKDiscPort = "SGSIN";

				AssertEquals("Shipment is approved for export from France", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_ETD = ZDate.Today.AddDays(10);
				AssertEquals("Approval expiry date expires before ETD", "UNK", shipment.JS_InspectionTypeCode);

				transport.JW_ETD = ZDate.Today.AddDays(2);
				AssertEquals("Approval expiry date is valid at ETD", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestShipmentInspectionTypeIsNotRecalculatedOnLoadChangeForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				Consol.JK_TransportMode = "AIR";
				var shipment = Consol.Shipments.AddNew();

				var ukAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "GB"));
				var approval = ukAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = ukAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_InspectionTypeCode = "APP";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ukAccountConsignor.MainAddress.PK;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "GBLON";

				AssertEquals("Shipment is approved for export from United Kingdom", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_RL_NKLoadPort = "FRPAR";
				AssertEquals("Shipment inspection type remains APP", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestETDChangeValidatesAllocationLine()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.New<IRatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = (IRatingContractAllocationLine)carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;

			const string etdDateRangeMessage = "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period";

			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_ETD = ZDateTime.Today.AddDays(6);
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_ETD = ZDateTime.Today;
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_RL_NKDiscPort = "NZAKL";
			var precedingTransport = consol.Transports.AddNew();
			precedingTransport.JW_TransportMode = Constants.TransportModes.Sea;
			precedingTransport.JW_ETD = transport.JW_ETD.AddDays(-1);
			precedingTransport.JW_RL_NKLoadPort = "AUPER";
			precedingTransport.JW_RL_NKDiscPort = "NZWEL";
			AssertNoErrorContaining("Consol has valid disc leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);
		}

		public void TestShipmentInspectionTypeIsNotRecalculatedOnModeChangeForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				Consol.JK_TransportMode = "AIR";
				var shipment = Consol.Shipments.AddNew();

				var ukAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "GB"));
				var approval = ukAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = ukAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_InspectionTypeCode = "APP";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ukAccountConsignor.MainAddress.PK;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "GBLON";

				AssertEquals("Shipment is approved for export from United Kingdom", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_TransportMode = "ROA";
				AssertEquals("Shipment inspection type remains APP", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestShipmentInspectionTypeIsNotRecalculatedOnETDChangeForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				Consol.JK_TransportMode = "AIR";
				var shipment = Consol.Shipments.AddNew();

				var ukAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "GB"));
				var approval = ukAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = ukAccountConsignor.PK;
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_InspectionTypeCode = "APP";
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ukAccountConsignor.MainAddress.PK;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "GBLON";

				AssertEquals("Shipment is approved for export from United Kingdom", "APP", shipment.JS_InspectionTypeCode);

				transport.JW_ETD = ZDate.Today.AddDays(10);
				AssertEquals("Shipment inspection type remains APP", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		#region Implementation

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<CommonConsol>();
			return parent.TransportSupporter;
		}

		OrgHeader AirNZCarrier
		{
			get { return airNZCarrier ?? (airNZCarrier = CreateCarrier("NZ")); }
		}
		OrgHeader airNZCarrier;

		OrgHeader SingaporeCarrier
		{
			get { return singaporeCarrier ?? (singaporeCarrier = CreateCarrier("SQ")); }
		}
		OrgHeader singaporeCarrier;

		OrgHeader QantasCarrier
		{
			get { return qantasCarrier ?? (qantasCarrier = CreateCarrier("QF")); }
		}
		OrgHeader qantasCarrier;

		OrgHeader CreateCarrier(string airlineCode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_IsShippingLine = true;
			result.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, airlineCode).PK;

			return result;
		}

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
					consol.JK_AgentType = Constants.AgentType.Agent;
					consol.JK_TransportMode = Constants.TransportModes.Air;
					consol.JK_AWBServiceLevel = "STD";
					consol.JK_RL_NKLoadPort = "AUSYD";
					consol.JK_RL_NKDischargePort = "USLAX";
				}

				return consol;
			}
		}
		ForwardingConsol consol;

		JobMawb CreateMAWB(OrgHeader carrier, ZString number)
		{
			ZString carrierCode = carrier.MiscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = carrierCode;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = number;
			mawb.JM_ServiceLevel = "STD";

			return mawb;
		}

		#endregion

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}
	}
}
