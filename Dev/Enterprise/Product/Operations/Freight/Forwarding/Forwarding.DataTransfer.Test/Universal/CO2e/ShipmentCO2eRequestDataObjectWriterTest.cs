using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public sealed class ShipmentCO2eRequestDataObjectWriterTest : BaseFreightTest
	{
		public void TestPopulateBusinessObject()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_RL_NKOrigin = "AUSYD";
			shipmentBO.JS_RL_NKDestination = "CNSHA";
			shipmentBO.JS_ActualWeight = 10m;
			shipmentBO.JS_UnitOfWeight = "KG";
			shipmentBO.JS_E_DEP = new ZDateTime(2022, 1, 1);
			shipmentBO.JS_E_ARV = new ZDateTime(2022, 1, 10);
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("shipmentData.TransportLegCollection.Count", 0, shipmentData.TransportLegCollection.Count);
			AssertEquals("shipmentData.PortOfLoading", "AUSYD", shipmentData.PortOfLoading.Code);
			AssertEquals("shipmentData.PortOfDischarge", "CNSHA", shipmentData.PortOfDischarge.Code);
			AssertEquals("shipmentData.TotalWeight", 10m, shipmentData.TotalWeight);
			AssertEquals("shipmentData.TotalWeightUnit", "KG", shipmentData.TotalWeightUnit?.Code);
			AssertContents(shipmentData.DateCollection[0], DateType.Arrival, new ZDateTime(2022, 1, 10), ZBool.True);
			AssertContents(shipmentData.DateCollection[1], DateType.Departure, new ZDateTime(2022, 1, 1), ZBool.True);
			AssertEquals("shipmentData.TransportMode", "SEA", shipmentData.TransportMode.Code);
			AssertEquals("shipmentData.ContainerMode", "FCL", shipmentData.ContainerMode.Code);

			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "SGSIN";
			leg1.JW_TransportMode = "SEA";
			leg1.JW_VoyageFlight = "VOY123";
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER 1";
			var cusCode1 = carrier1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = "US";
			cusCode1.OK_CustomsRegNo = "1111";
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;

			var leg2 = shipmentBO.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "SGSIN";
			leg2.JW_RL_NKDiscPort = "CNSHA";
			leg2.JW_TransportMode = "AIR";
			leg2.JW_VoyageFlight = "FL123";
			leg2.JW_AircraftType = "E90";
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "CARRIER 2";
			var cusCode2 = carrier2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_RN_NKCodeCountry = "US";
			cusCode2.OK_CustomsRegNo = "2222";
			leg2.JW_OA_CarrierAddress = carrier2.MainAddress.PK;

			var transportModeConverter = new TransportModeConverter();
			var shipmentDataWithLegs = writer.GetDataObject(shipmentBO);
			AssertNotNull("shipmentDataWithLegs", shipmentDataWithLegs);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection.Count", 2, shipmentDataWithLegs.TransportLegCollection.Count);

			AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Sea), shipmentDataWithLegs.TransportLegCollection[0].TransportMode);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].VoyageFlightNo", "VOY123", shipmentDataWithLegs.TransportLegCollection[0].VoyageFlightNo);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].AircraftType.Code", ZString.Empty, shipmentDataWithLegs.TransportLegCollection[0].AircraftType.Code);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].PortOfLoading.Code", "AUSYD", shipmentDataWithLegs.TransportLegCollection[0].PortOfLoading.Code);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].PortOfDischarge.Code", "SGSIN", shipmentDataWithLegs.TransportLegCollection[0].PortOfDischarge.Code);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].Carrier.RegistrationNumber", "1111", shipmentDataWithLegs.TransportLegCollection[0].Carrier.RegistrationNumberCollection[0].Value);

			AssertEquals("shipmentDataWithLegs.TransportLegCollection[1].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Air), shipmentDataWithLegs.TransportLegCollection[1].TransportMode);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[1].VoyageFlightNo", "FL123", shipmentDataWithLegs.TransportLegCollection[1].VoyageFlightNo);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[1].AircraftType", "E90", shipmentDataWithLegs.TransportLegCollection[1].AircraftType.Code);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[1].PortOfLoading.Code", "SIN", shipmentDataWithLegs.TransportLegCollection[1].PortOfLoading.Code);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[1].PortOfDischarge.Code", "SHA", shipmentDataWithLegs.TransportLegCollection[1].PortOfDischarge.Code);
			AssertEquals("shipmentDataWithLegs.TransportLegCollection[1].Carrier.RegistrationNumber", "2222", shipmentDataWithLegs.TransportLegCollection[1].Carrier.RegistrationNumberCollection[0].Value);
		}

		public void TestPopulateShipmentTransportMode()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Air);
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Sea);
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Road);
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.Rail, Core.Constants.TransportModes.Rail);
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.SeaAir, Core.Constants.TransportModes.Sea);
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.AirSea, Core.Constants.TransportModes.Air);
				SetupAndAssertTransportMode(HomePort, OverseasPort, AlternateHomePort, OverseasPort2, OverseasPort3, OverseasPort4, Core.Constants.TransportModes.Courier, Core.Constants.TransportModes.Air);
				SetupAndAssertTransportMode("AUMEL", "AUBNE", "AUSYD", "AUPER", "CNSHA", "FRPAR", Core.Constants.TransportModes.Courier, Core.Constants.TransportModes.Road);
			}
		}

		void SetupAndAssertTransportMode(string origin, string destination, string consol1LoadPort, string consol1DiscPort, string consol2LoadPort, string consol2DiscPort,  string transportMode, string expectedTransportMode)
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = transportMode;
			shipmentBO.JS_RL_NKOrigin = origin;
			shipmentBO.JS_RL_NKDestination = destination;

			var consolBO1 = shipmentBO.Consols.AddNew();
			consolBO1.JK_RL_NKLoadPort = consol1LoadPort;
			consolBO1.JK_RL_NKDischargePort = consol1DiscPort;

			var consolBO2 = shipmentBO.Consols.AddNew();
			consolBO2.JK_RL_NKLoadPort = consol2LoadPort;
			consolBO2.JK_RL_NKDischargePort = consol2DiscPort;

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			var transportModeConverter = new TransportModeConverter();
			var shipmentDataWithLegs = writer.GetDataObject(shipmentBO);
			AssertNotNull("shipmentDataWithLegs", shipmentDataWithLegs);
			AssertEquals("Shipment TransportMode", expectedTransportMode, shipmentDataWithLegs.TransportMode.Code);
			AssertEquals("Virtual leg TransportMode", transportModeConverter.ToEnumValue(expectedTransportMode), shipmentDataWithLegs.TransportLegCollection[1].TransportMode);
		}

		public void TestPopulateVirtualLegs()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipmentBO = Factory.New<ForwardingShipment>();
				shipmentBO.JS_TransportMode = "AIR";
				shipmentBO.JS_RL_NKOrigin = "AUMEL";
				shipmentBO.JS_RL_NKDestination = "FRPAR";

				var consolBO = shipmentBO.Consols.AddNew();
				consolBO.JK_RL_NKLoadPort = "AUSYD";
				consolBO.JK_RL_NKDischargePort = "SGSIN";

				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				var transportModeConverter = new TransportModeConverter();
				var shipmentDataWithLegs = writer.GetDataObject(shipmentBO);
				AssertNotNull("shipmentDataWithLegs", shipmentDataWithLegs);
				AssertEquals("shipmentDataWithLegs.TransportLegCollection.Count", 1,shipmentDataWithLegs.TransportLegCollection.Count);

				AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Air), shipmentDataWithLegs.TransportLegCollection[0].TransportMode);
				AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].PortOfLoading.Code", "SYD", shipmentDataWithLegs.TransportLegCollection[0].PortOfLoading.Code);
				AssertEquals("shipmentDataWithLegs.TransportLegCollection[0].PortOfDischarge.Code", "SIN", shipmentDataWithLegs.TransportLegCollection[0].PortOfDischarge.Code);
			}
		}

		public void TestPopulateIATAForAir()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipmentBO = Factory.New<ForwardingShipment>();
				shipmentBO.JS_TransportMode = "AIR";
				shipmentBO.JS_RL_NKOrigin = "AUMEL";
				shipmentBO.JS_RL_NKDestination = "FRPAR";

				var consolBO = shipmentBO.Consols.AddNew();
				consolBO.JK_RL_NKLoadPort = "AUMEL";
				consolBO.JK_RL_NKDischargePort = "FRPAR";

				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				var shipmentData = writer.GetDataObject(shipmentBO);

				AssertEquals("shipmentData.PortOfLoading", "MEL", shipmentData.PortOfLoading.Code);
				AssertEquals("shipmentData.PortOfDischarge", "PAR", shipmentData.PortOfDischarge.Code);
				AssertEquals("shipmentData.TransportLegCollection.Count", 1, shipmentData.TransportLegCollection.Count);
				AssertEquals("shipmentData.TransportLegCollection[0].PortOfLoading.Code", "MEL", shipmentData.TransportLegCollection[0].PortOfLoading.Code);
				AssertEquals("shipmentData.TransportLegCollection[0].PortOfDischarge.Code", "PAR", shipmentData.TransportLegCollection[0].PortOfDischarge.Code);

				var query = new ZQuery(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.Equal, ZString.Empty);
				query.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU");
				var unlocoWithoutIATA = Factory.LoadTop1<RefUNLOCO>(query);
				shipmentBO.JS_RL_NKOrigin = unlocoWithoutIATA.RL_Code;
				consolBO.JK_RL_NKLoadPort = unlocoWithoutIATA.RL_Code;

				writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				shipmentData = writer.GetDataObject(shipmentBO);
				AssertEquals("shipmentData.PortOfLoading fallback to UNLOCO when IATA is not configured", unlocoWithoutIATA.RL_Code, shipmentData.PortOfLoading.Code);
			}
		}

		public void TestPopulateVoyageFlightNoFromCarrierIfEmpty()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_RL_NKOrigin = "AUSYD";
			shipmentBO.JS_RL_NKDestination = "CNSHA";

			shipmentBO.Transports.RemoveAll();
			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "CNSHA";
			leg1.JW_TransportMode = "AIR";
			leg1.JW_VoyageFlight = ZString.Empty;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER 1";

			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "AZ";
			var miscServ = carrier.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;
			leg1.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("shipmentData.TransportLegCollection.Count", 1, shipmentData.TransportLegCollection.Count);
			AssertEquals("shipmentData.TransportLegCollection[0].VoyageFlightNo", "AZ", shipmentData.TransportLegCollection[0].VoyageFlightNo);
		}

		void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		public void TestPopulateTEU()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			consol1.Transports.RemoveAll();
			var leg1 = consol1.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "CNSHA";
			leg1.JW_TransportMode = Core.Constants.TransportModes.Sea;

			var shipmentBO = consol1.Shipments.AddNew();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBO.JS_RL_NKOrigin = "AUSYD";
			shipmentBO.JS_RL_NKDestination = "CNSHA";
			shipmentBO.JS_ActualWeight = 2000;
			shipmentBO.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			Shipment shipmentData;

			#region Empty Containers

			AssertEmptyTEU();

			#endregion

			#region With Containers

			consol1.Containers.RemoveAll();
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			shipmentBO.OuterPackLines.RemoveAll();
			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 1;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			container2.AddPackLine(packline1);

			var packline2 = shipmentBO.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 1000;
			packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			container1.AddPackLine(packline2);

			AssertEquals(2, shipmentBO.OuterPackLines.Count);
			AssertEquals(container2, packline1.GetContainer(consol1));
			AssertEquals(container1, packline2.GetContainer(consol1));

			AssertTEU();
			AssertEquals("shipmentData.TEU.ContainerEmptyWeightPerTEU", 2088.372093m, shipmentData.TEU.ContainerEmptyWeightPerTEU);
			AssertEquals("shipmentData.TEU.ContainerEmptyWeightPerTEUUnit.Code", Core.Constants.Weight.Kilograms, shipmentData.TEU.ContainerEmptyWeightPerTEUUnit.Code);

			#endregion

			#region With Rail Transport

			// Act
			leg1.JW_TransportMode = Core.Constants.TransportModes.Rail;
			shipmentData = writer.GetDataObject(shipmentBO);

			// Assert
			AssertEquals("shipmentData.TEU.ContainerEmptyWeightPerTEU", 2088.372093m, shipmentData.TEU.ContainerEmptyWeightPerTEU);

			#endregion

			#region TransportMode & ContainerMode

			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEmptyTEU();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertTEU();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertTEU();

			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			AssertEmptyTEU();
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.Groupage;
			AssertEmptyTEU();

			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.ShippersConsol;
			AssertTEU();
			shipmentBO.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertTEU();

			#endregion

			void AssertEmptyTEU()
			{
				shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				AssertNotNull("shipmentData", shipmentData);
				AssertNull("shipmentData.TEU", shipmentData.TEU);
			}

			void AssertTEU()
			{
				shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				AssertNotNull("shipmentData", shipmentData);
				AssertNotNull("shipmentData.TEU", shipmentData.TEU);
				AssertEquals("shipmentData.TEU.NumberOfTEU", 1 * 2 + 2.3m * 1m, shipmentData.TEU.NumberOfTEU);
				AssertEquals("shipmentData.TEU.TonnesPerTEU", Utilities.Round(2 / (1m * 2m + 2.3m * 1m), 6), shipmentData.TEU.TonnesPerTEU);
			}
		}

		RefContainer NewRefContainer(ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		public void TestPopulateRequiresTemperatureControl()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "FRPAR";

			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_RequiresTemperatureControl = false;
			var packline2 = shipmentBO.OuterPackLines.AddNew();
			packline2.JL_RequiresTemperatureControl = false;

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertEquals("shipmentData.RequiresTemperatureControl is null because there is no Temperature Controlled option checked for any packlines", null, shipmentData.RequiresTemperatureControl);

			packline1.JL_RequiresTemperatureControl = true;
			shipmentData = writer.GetDataObject(shipmentBO);
			AssertEquals("shipmentData.RequiresTemperatureControl", true, shipmentData.RequiresTemperatureControl);
		}

		public void TestRemoveUnusedFields()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "FRPAR";

			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUMEL";
			leg1.JW_RL_NKDiscPort = "FRPAR";
			leg1.JW_TransportMode = "SEA";
			leg1.JW_VoyageFlight = "VOY123";
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER 1";
			var cusCode1 = carrier1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = "US";
			cusCode1.OK_CustomsRegNo = "1111";
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertEquals("CustomizedFieldCollection should be null for CO2e Calculation", null, shipmentData.CustomizedFieldCollection);
			AssertEquals("MilestoneCollection should be null for CO2e Calculation", null, shipmentData.MilestoneCollection);
			AssertEquals("ExceptionCollection should be null for CO2e Calculation", null, shipmentData.ExceptionCollection);
			AssertEquals("JobCosting should be null for CO2e Calculation", null, shipmentData.JobCosting);
			AssertEquals("ConsolCosts should be null for CO2e Calculation", null, shipmentData.ConsolCosts);
			AssertNull(shipmentData.CustomizedFieldCollection);
			AssertNull(shipmentData.MilestoneCollection);
			AssertNull(shipmentData.ExceptionCollection);
			AssertNull(shipmentData.JobCosting);
			AssertNull(shipmentData.ConsolCosts);
			var legData = shipmentData.TransportLegCollection[0];
			AssertNull(legData.ArrivalAt);
			AssertNull(legData.DepartureFrom);
			var carrierData = legData.Carrier;
			AssertNull(carrierData.OrganizationCode);
			AssertNull(carrierData.City);
			AssertNull(carrierData.Country);
			AssertNull(carrierData.CompanyName);
			AssertNull(carrierData.Phone);
			AssertNull(carrierData.Postcode);
		}

		#region Pre/Post Carriage

		public void TestPopulateEmptyContainerCollection()
		{
			// Arrange
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Singapore", "SG", "SGSIN");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Singapore", "SG", "SGSIN");

			var address5 = CreateAddress(ZGeography.CreatePoint(50, 50), "4001", "Sydney", "AU", "AUSYD");
			var address6 = CreateAddress(ZGeography.CreatePoint(60, 60), "5051", "South Melbourne", "AU", "AUMEL");
			var address7 = CreateAddress(ZGeography.CreatePoint(70, 70), "6000", "Singapore", "SG", "SGSIN");
			var address8 = CreateAddress(ZGeography.CreatePoint(80, 80), "7000", "Singapore", "SG", "SGSIN");

			var address9 = CreateAddress(ZGeography.CreatePoint(90, 90), "8001", "Sydney", "AU", "AUSYD");
			var address10 = CreateAddress(ZGeography.CreatePoint(100, 100), "9001", "Singapore", "AU", "AUSYD");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";
			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "SGSIN";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBO.JK_AgentType = Core.Constants.AgentType.Direct;
			consolBO.JK_OA_ContainerYardEmptyPickupAddress = address6.PK;
			consolBO.JK_OA_ContainerYardEmptyReturnAddress = address8.PK;
			consolBO.JK_OA_PackDepotAddress = address9.PK;
			consolBO.JK_OA_UnpackDepotAddress = address10.PK;

			consolBO.Containers.RemoveAll();
			var container1 = consolBO.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("99GP", "22G0", 1.5m, 2300m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_OA_DepartureContainerYardAddress = address5.PK;
			container1.JC_OA_ArrivalContainerYardAddress = address7.PK;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var container2 = consolBO.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("99REHC", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			Factory.Save();

			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Act
				var shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				CombineAssertions("Doesn't populate SubShipmentCollection when registry is disabled.", () =>
				{
					AssertNull(shipmentData.SubShipmentCollection);
				});
			}

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Act
				var shipmentData = writer.GetDataObject(shipmentBO);
				// Assert
				CombineAssertions("Doesn't populate EmptyContainerCollection when containers are not packed.", () =>
				{
					AssertNotNull(shipmentData.SubShipmentCollection);
					AssertNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
				});

				// Act
				shipmentBO.OuterPackLines.RemoveAndDeleteAll();
				var packline1 = shipmentBO.OuterPackLines.AddNew();
				packline1.JL_ActualWeight = 0.5;
				packline1.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
				container1.AddPackLine(packline1);
				var packline2 = shipmentBO.OuterPackLines.AddNew();
				packline2.JL_ActualWeight = 500;
				packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				container2.AddPackLine(packline2);
				shipmentData = writer.GetDataObject(shipmentBO);
				// Assert
				CombineAssertions("Populate SubShipmentCollection and EmptyContainerCollection when registry is enabled.", () =>
				{
					AssertEquals(shipmentData.SubShipmentCollection.Count, 1);
					AssertNotNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
					AssertEquals(shipmentData.SubShipmentCollection[0].EmptyContainerCollection.Count, 2);
					var emptyContainer1 = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEquals(emptyContainer1.TotalWeight, 2 * 2300m);
					AssertEquals(emptyContainer1.TotalWeightUnit.Code, Core.Constants.Weight.Kilograms);
					AssertEquals(emptyContainer1.TEU.NumberOfTEU, 2 * 1.5m);
					AssertEquals(emptyContainer1.TEU.TonnesPerTEU, 0m);
					AssertEquals(emptyContainer1.TEU.ContainerEmptyWeightPerTEU, 1533.333333m);
					AssertEquals(emptyContainer1.TEU.ContainerEmptyWeightPerTEUUnit.Code, Core.Constants.Weight.Kilograms);
					AssertEmptyContainerAddress(emptyContainer1.EmptyPickup, address5, address2);
					AssertEmptyContainerAddress(emptyContainer1.EmptyReturn, address4, address7);

					var emptyContainer2 = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[1];
					AssertEmptyContainerAddress(emptyContainer2.EmptyPickup, address6, address2);
					AssertEmptyContainerAddress(emptyContainer2.EmptyReturn, address4, address8);
				});

				// Act
				shipmentBO.JS_OA_ExportReceivingDepot = ZGuid.Empty;
				shipmentBO.JS_OA_ImportReleaseDepot = ZGuid.Empty;
				shipmentData = writer.GetDataObject(shipmentBO);
				// Assert
				CombineAssertions("Fallback to Shipment Pickup/Delivery Address", () =>
				{
					var emptyContainer1 = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEmptyContainerAddress(emptyContainer1.EmptyPickup, address5, address1);
					AssertEmptyContainerAddress(emptyContainer1.EmptyReturn, address3, address7);
				});

				// Act
				container1.JC_OA_DepartureContainerYardAddress = ZGuid.Empty;
				container1.JC_OA_ArrivalContainerYardAddress = ZGuid.Empty;
				shipmentData = writer.GetDataObject(shipmentBO);
				// Assert
				CombineAssertions("Fallback to Consol > Departure/Arrival > Container Yard", () =>
				{
					var emptyContainer1 = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEmptyContainerAddress(emptyContainer1.EmptyPickup, address6, address1);
					AssertEmptyContainerAddress(emptyContainer1.EmptyReturn, address3, address8);
				});

				// Act
				consolBO.JK_AgentType = Core.Constants.AgentType.Agent;
				shipmentData = writer.GetDataObject(shipmentBO);
				// Assert
				CombineAssertions("Fallback to Consol > Details > Departure/Arrival > CFS Address", () =>
				{
					var emptyContainer1 = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEmptyContainerAddress(emptyContainer1.EmptyPickup, address6, address9);
					AssertEmptyContainerAddress(emptyContainer1.EmptyReturn, address10, address8);
				});
			}
		}

		public void TestPopulateEmptyContainerCollection_TransportMode()
		{
			// Arrange
			var address1 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Singapore", "SG", "SGSIN");
			var address3 = CreateAddress(ZGeography.CreatePoint(50, 50), "4001", "Sydney", "AU", "AUSYD");
			var address4 = CreateAddress(ZGeography.CreatePoint(70, 70), "6000", "Singapore", "SG", "SGSIN");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";
			shipmentBO.JS_OA_ExportReceivingDepot = address1.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address2.PK;

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "SGSIN";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBO.JK_AgentType = Core.Constants.AgentType.Direct;

			consolBO.Containers.RemoveAll();
			var container1 = consolBO.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("99GP", "22G0", 1.5m, 2280m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_OA_DepartureContainerYardAddress = address3.PK;
			container1.JC_OA_ArrivalContainerYardAddress = address4.PK;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container1.EmptyPickupByTransportMode = Core.Constants.TransportModes.Rail;

			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 0.5;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			container1.AddPackLine(packline1);

			Factory.Save();

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Act
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				var shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				AssertEquals(1, shipmentData.SubShipmentCollection[0].EmptyContainerCollection.Count);
				var emptyContainer = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
				AssertEquals(Core.Constants.TransportModes.Rail, emptyContainer.EmptyPickup.TransportMode.Code);
				AssertEquals(Core.Constants.TransportModes.Road, emptyContainer.EmptyReturn.TransportMode.Code);
			}
		}

		public void TestPopulateEmptyContainerCollection_WithCurrentStatus()
		{
			// Arrange
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Singapore", "SG", "SGSIN");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Singapore", "SG", "SGSIN");

			var address5 = CreateAddress(ZGeography.CreatePoint(50, 50), "4001", "Sydney", "AU", "AUSYD");
			var address6 = CreateAddress(ZGeography.CreatePoint(60, 60), "5051", "South Melbourne", "AU", "AUMEL");
			var address7 = CreateAddress(ZGeography.CreatePoint(70, 70), "6000", "Singapore", "SG", "SGSIN");
			var address8 = CreateAddress(ZGeography.CreatePoint(80, 80), "7000", "Singapore", "SG", "SGSIN");

			var address9 = CreateAddress(ZGeography.CreatePoint(90, 90), "8001", "Sydney", "AU", "AUSYD");
			var address10 = CreateAddress(ZGeography.CreatePoint(100, 100), "9001", "Singapore", "AU", "AUSYD");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";
			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "SGSIN";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBO.JK_AgentType = Core.Constants.AgentType.Direct;
			consolBO.JK_OA_ContainerYardEmptyPickupAddress = address6.PK;
			consolBO.JK_OA_ContainerYardEmptyReturnAddress = address8.PK;
			consolBO.JK_OA_PackDepotAddress = address9.PK;
			consolBO.JK_OA_UnpackDepotAddress = address10.PK;

			consolBO.Containers.RemoveAll();
			var container1 = consolBO.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("99GP", "22G0", 1.5m, 2280m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_OA_DepartureContainerYardAddress = address5.PK;
			container1.JC_OA_ArrivalContainerYardAddress = address7.PK;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container1.SetCO2ePerTEUInKg(10, CO2eTypes.EmptyPickup);

			var container2 = consolBO.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("99REHC", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;
			container2.SetCO2ePerTEUInKg(10, CO2eTypes.EmptyPickup);
			container2.SetCO2ePerTEUInKg(10, CO2eTypes.EmptyReturn);

			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 0.5;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			container1.AddPackLine(packline1);
			var packline2 = shipmentBO.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 500;
			packline2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			container2.AddPackLine(packline2);

			Factory.Save();
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Act
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				var shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				AssertEquals(1, shipmentData.SubShipmentCollection[0].EmptyContainerCollection.Count);
				var emptyContainer1 = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
				AssertEmptyContainerAddress(emptyContainer1.EmptyReturn, address4, address7);
				AssertNull(emptyContainer1.EmptyPickup);
			}
		}

		public void TestPopulateEmptyContainerCollection_ExcludeWhenTEUAndWeightAreZero()
		{
			// Arrange
			var address1 = CreateAddress(ZGeography.CreatePoint(50, 50), "4001", "Sydney", "AU", "AUSYD");
			var address2 = CreateAddress(ZGeography.CreatePoint(70, 70), "6000", "Singapore", "SG", "SGSIN");
			var address3 = CreateAddress(ZGeography.CreatePoint(90, 90), "8001", "Sydney", "AU", "AUSYD");
			var address4 = CreateAddress(ZGeography.CreatePoint(100, 100), "9001", "Singapore", "AU", "AUSYD");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "SGSIN";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBO.JK_AgentType = Core.Constants.AgentType.Agent;
			consolBO.JK_OA_PackDepotAddress = address3.PK;
			consolBO.JK_OA_UnpackDepotAddress = address4.PK;

			consolBO.Containers.RemoveAll();
			var container1 = consolBO.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("99GP", "22G0", 0m, 0m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_OA_DepartureContainerYardAddress = address1.PK;
			container1.JC_OA_ArrivalContainerYardAddress = address2.PK;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();
			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 0.5;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			container1.AddPackLine(packline1);
			Factory.Save();

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Act
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				var shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				AssertEquals(1, shipmentData.SubShipmentCollection.Count);
				AssertNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
			}
		}

		public void TestPopulateEmptyContainerCollection_UseHBLDeliveryMode()
		{
			// Arrange
			var address1 = CreateAddress(ZGeography.CreatePoint(50, 50), "4001", "Sydney", "AU", "AUSYD");
			var address2 = CreateAddress(ZGeography.CreatePoint(70, 70), "6000", "Singapore", "SG", "SGSIN");
			var address3 = CreateAddress(ZGeography.CreatePoint(90, 90), "8001", "Sydney", "AU", "AUSYD");
			var address4 = CreateAddress(ZGeography.CreatePoint(100, 100), "9001", "Singapore", "AU", "AUSYD");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "SGSIN";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consolBO.JK_AgentType = Core.Constants.AgentType.Agent;
			consolBO.JK_OA_PackDepotAddress = address3.PK;
			consolBO.JK_OA_UnpackDepotAddress = address4.PK;

			consolBO.Containers.RemoveAll();
			var container1 = consolBO.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("99GP", "22G0", 1.5m, 2280m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_OA_DepartureContainerYardAddress = address1.PK;
			container1.JC_OA_ArrivalContainerYardAddress = address2.PK;
			container1.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();
			var packline1 = shipmentBO.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 0.5;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			container1.AddPackLine(packline1);
			Factory.Save();

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Act
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
				var shipmentData = writer.GetDataObject(shipmentBO);

				// Assert
				CombineAssertions("Empty HBL Delivery Mode", () =>
				{
					AssertEquals(1, shipmentData.SubShipmentCollection.Count);
					AssertNotNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
					AssertEquals(1, shipmentData.SubShipmentCollection[0].EmptyContainerCollection.Count);
					var emptyContainer = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEquals(2 * 2280m, emptyContainer.TotalWeight);
					AssertEquals(Core.Constants.Weight.Kilograms, emptyContainer.TotalWeightUnit.Code);
					AssertEquals(2 * 1.5m, emptyContainer.TEU.NumberOfTEU);
					AssertEquals(0m, emptyContainer.TEU.TonnesPerTEU);
					AssertEquals(2280m / 1.5m, emptyContainer.TEU.ContainerEmptyWeightPerTEU);
					AssertEquals(Core.Constants.Weight.Kilograms, emptyContainer.TEU.ContainerEmptyWeightPerTEUUnit.Code);
					AssertEmptyContainerAddress(emptyContainer.EmptyPickup, address1, address3);
					AssertEmptyContainerAddress(emptyContainer.EmptyReturn, address4, address2);
				});

				shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
				shipmentData = writer.GetDataObject(shipmentBO);
				CombineAssertions("CFS_CY HBL Delivery Mode", () =>
				{
					AssertEquals(1, shipmentData.SubShipmentCollection.Count);
					AssertNotNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
					AssertEquals(1, shipmentData.SubShipmentCollection[0].EmptyContainerCollection.Count);
					var emptyContainer = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEmptyContainerAddress(emptyContainer.EmptyPickup, address1, address3);
					AssertNull(emptyContainer.EmptyReturn);
				});

				shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CFS;
				shipmentData = writer.GetDataObject(shipmentBO);
				CombineAssertions("CY_CFS HBL Delivery Mode", () =>
				{
					AssertEquals(1, shipmentData.SubShipmentCollection.Count);
					AssertNotNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
					AssertEquals(1, shipmentData.SubShipmentCollection[0].EmptyContainerCollection.Count);
					var emptyContainer = shipmentData.SubShipmentCollection[0].EmptyContainerCollection[0];
					AssertEmptyContainerAddress(emptyContainer.EmptyReturn, address4, address2);
					AssertNull(emptyContainer.EmptyPickup);
				});

				shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT;
				shipmentData = writer.GetDataObject(shipmentBO);
				CombineAssertions("ARPT_ARPT HBL Delivery Mode", () =>
				{
					AssertEquals(1, shipmentData.SubShipmentCollection.Count);
					AssertNull(shipmentData.SubShipmentCollection[0].EmptyContainerCollection);
				});
			}
		}

		void AssertEmptyContainerAddress(EmptyContainerAddress dataObject, OrgAddress from, OrgAddress to)
		{
			AssertEquals(from.Postcode, dataObject.From.Postcode);
			AssertEquals(from.Latitude, dataObject.From.GeoLocation.Latitude);
			AssertEquals(from.Longitude, dataObject.From.GeoLocation.Longitude);
			AssertEquals(to.Postcode, dataObject.To.Postcode);
			AssertEquals(to.Latitude, dataObject.To.GeoLocation.Latitude);
			AssertEquals(to.Longitude, dataObject.To.GeoLocation.Longitude);
		}

		public void TestPopulatePrePostCarriageLegs()
		{
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Singapore", "SG", "SGSIN");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Singapore", "SG", "SGSIN");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";
			shipmentBO.JS_ActualWeight = 100m;
			var transport = shipmentBO.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "SGSIN";

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.PickupByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.CFSDepartureByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.DeliveryByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolNoPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPopulatePrePostCarriageLegs_HBLDeliveryModes()
		{
			// Arrange
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Singapore", "SG", "SGSIN");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Singapore", "SG", "SGSIN");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "SGSIN";
			shipmentBO.JS_ActualWeight = 100m;
			var transport = shipmentBO.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "SGSIN";

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;

			// Act & Assert
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.CFS_CFS_ShipmentWithoutConsolPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);

					shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
					shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.CY_CY_ShipmentWithoutConsolPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPopulatePrePostCarriageLegs_WithConsols()
		{
			// Arrange
			// C1: AUMEL -> AUSYD, AUSYD -> NZAKL
			// S: NZAKL -> SGSIN, SGSIN > VNSGN
			// C2: VNSGN -> VNHAN, VNHAN -> CNSHA
			// C3: CNSHA -> CNTAO
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Qingdao", "CN", "CNTAO");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Qingdao", "CN", "CNTAO");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_UniqueConsignRef = "S0001";
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "CNTAO";
			shipmentBO.JS_ActualWeight = 100m;
			var shipmentTransport1 = shipmentBO.Transports.AddNew();
			shipmentTransport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTransport1.JW_RL_NKLoadPort = "NZAKL";
			shipmentTransport1.JW_RL_NKDiscPort = "SGSIN";
			var shipmentTransport2 = shipmentBO.Transports.AddNew();
			shipmentTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTransport2.JW_RL_NKLoadPort = "SGSIN";
			shipmentTransport2.JW_RL_NKDiscPort = "VNSGN";

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.PickupByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.CFSDepartureByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;
			shipmentBO.CFSArrivalByTransportMode = Core.Constants.TransportModes.Rail;

			var address5 = CreateAddress(ZGeography.CreatePoint(50, 50), "3066", "Collingwood", "AU", "AUMEL");
			var address6 = CreateAddress(ZGeography.CreatePoint(60, 60), "9999", "Auckland", "NZ", "NZAKL");
			var address7 = CreateAddress(ZGeography.CreatePoint(70, 70), "11", "District 1", "VN", "VNSGN");
			var address8 = CreateAddress(ZGeography.CreatePoint(80, 80), "22", "Shanghai", "CN", "CNSHA");
			var address9 = CreateAddress(ZGeography.CreatePoint(90, 90), "33", "Shanghai", "CN", "CNSHA");
			var address10 = CreateAddress(ZGeography.CreatePoint(0, 0), "44", "Qingdao", "CN", "CNTAO");

			var consol1 = shipmentBO.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			consol1.JK_TransportMode = "SEA";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			var consol1Transport2 = consol1.Transports.AddNew();
			consol1Transport2.JW_TransportMode = "SEA";
			consol1Transport2.JW_RL_NKLoadPort = "AUSYD";
			consol1Transport2.JW_RL_NKDiscPort = "NZAKL";
			consol1.JK_OA_PackDepotAddress = address5.PK;
			consol1.CFSDepartureByTransportMode = Core.Constants.TransportModes.Rail;
			consol1.JK_OA_UnpackDepotAddress = address6.PK;
			consol1.CFSArrivalByTransportMode = Core.Constants.TransportModes.Road;

			var consol2 = shipmentBO.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "VNSGN";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.Transports[0].JW_RL_NKDiscPort = "VNHAN";
			var consol2Transport2 = consol2.Transports.AddNew();
			consol2Transport2.JW_TransportMode = "SEA";
			consol2Transport2.JW_RL_NKLoadPort = "VNHAN";
			consol2Transport2.JW_RL_NKDiscPort = "CNSHA";
			consol2.JK_OA_PackDepotAddress = address7.PK;
			consol2.CFSDepartureByTransportMode = Core.Constants.TransportModes.Road;
			consol2.JK_OA_UnpackDepotAddress = address8.PK;
			consol2.CFSArrivalByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

			var consol3 = shipmentBO.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";
			consol3.JK_TransportMode = "AIR";
			consol3.JK_RL_NKLoadPort = "CNSHA";
			consol3.JK_RL_NKDischargePort = "CNTAO";
			consol3.JK_OA_PackDepotAddress = address9.PK;
			consol3.JK_OA_UnpackDepotAddress = address10.PK;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				// Act & Assert
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithConsolsPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPopulatePrePostCarriageLegs_WithConsols_HBLDeliveryModes()
		{
			// Arrange
			// C1: LoadPort - AUMEL, AUSYD -> NZAKL
			// C2: VNSGN -> VNHAN, VNHAN -> CNSHA
			// C3: CNSHA -> CNTAO, DiscPort - HKHKG
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Qingdao", "CN", "CNTAO");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Qingdao", "CN", "CNTAO");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_UniqueConsignRef = "S0001";
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "HKHKG";
			shipmentBO.JS_ActualWeight = 100m;
			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;

			var address5 = CreateAddress(ZGeography.CreatePoint(50, 50), "3066", "Collingwood", "AU", "AUMEL");
			var address6 = CreateAddress(ZGeography.CreatePoint(60, 60), "9999", "Auckland", "NZ", "NZAKL");
			var address7 = CreateAddress(ZGeography.CreatePoint(70, 70), "11", "District 1", "VN", "VNSGN");
			var address8 = CreateAddress(ZGeography.CreatePoint(80, 80), "22", "Shanghai", "CN", "CNSHA");
			var address9 = CreateAddress(ZGeography.CreatePoint(90, 90), "33", "Shanghai", "CN", "CNSHA");
			var address10 = CreateAddress(ZGeography.CreatePoint(0, 0), "44", "Hong Kong", "HK", "HKHKG");

			var consol1 = shipmentBO.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			consol1.JK_TransportMode = "SEA";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol1.JK_OA_PackDepotAddress = address5.PK;
			consol1.JK_OA_UnpackDepotAddress = address6.PK;

			var consol2 = shipmentBO.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "VNSGN";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.Transports[0].JW_RL_NKDiscPort = "VNHAN";
			var consol2Transport2 = consol2.Transports.AddNew();
			consol2Transport2.JW_TransportMode = "SEA";
			consol2Transport2.JW_RL_NKLoadPort = "VNHAN";
			consol2Transport2.JW_RL_NKDiscPort = "CNSHA";
			consol2.JK_OA_PackDepotAddress = address7.PK;
			consol2.JK_OA_UnpackDepotAddress = address8.PK;

			var consol3 = shipmentBO.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";
			consol3.JK_TransportMode = "AIR";
			consol3.JK_RL_NKLoadPort = "CNSHA";
			consol3.JK_RL_NKDischargePort = "HKHKG";
			consol3.Transports[0].JW_RL_NKDiscPort = "CNTAO";
			consol3.JK_OA_PackDepotAddress = address9.PK;
			consol3.JK_OA_UnpackDepotAddress = address10.PK;

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				// Act & Assert
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.UseHBLDeliveryModeForGHGCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.CFS_CFS_ShipmentWithConsolsPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);

					shipmentBO.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
					shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.CY_CY_ShipmentWithConsolsPrePostCarriageLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		ForwardingShipment CreateShipmentBOWithPrePostCarriageLocationsAndNoRoutingLegs()
		{
			// Arrange shipment
			// Has No Routing legs
			// PickupFrom: AUMEL
			// DepartureCFS: AUDRW
			// ArrivalCFS: AUBWU
			// DeliverTo: AUSYD

			var pickupFromAddress = CreateAddress(ZGeography.CreatePoint(10, 10), "3001", "CBD 1", "AU", "AUMEL");
			var departureCFS = CreateAddress(ZGeography.CreatePoint(20, 20), "3002", "Darwin 2", "AU", "AUDRW");
			var arrivalCFS = CreateAddress(ZGeography.CreatePoint(30, 30), "3003", "Bankstown 3", "AU", "AUBWU");
			var deliverToAddress = CreateAddress(ZGeography.CreatePoint(40, 40), "3004", "Sydney 4", "AU", "AUSYD");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipmentBO.JS_RL_NKOrigin = "AUADL";
			shipmentBO.JS_RL_NKDestination = "AUANB";
			shipmentBO.JS_ActualWeight = 100m;

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = pickupFromAddress.PK;
			shipmentBO.PickupByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ExportReceivingDepot = departureCFS.PK;
			shipmentBO.CFSDepartureByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = deliverToAddress.PK;
			shipmentBO.DeliveryByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ImportReleaseDepot = arrivalCFS.PK;
			return shipmentBO;
		}

		public void TestPreMainPostCarriageFallbacks_NoRoutingLegs()
		{
			// Arrange shipment
			// Has No Routing legs
			// PickupFrom: AUMEL
			// DepartureCFS: AUDRW
			// ArrivalCFS: AUBWU
			// DeliverTo: AUSYD
			var shipmentBO = CreateShipmentBOWithPrePostCarriageLocationsAndNoRoutingLegs();

			// Assert
			// PreCarriageCollection: AUMEL -> AUDRW
			// TransportLegCollection: AUDRW -> AUBWU
			// PostCarriageCollection: AUBWU -> AUSYD
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolPrePostCarriageLegs_NoRoutingLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPreMainPostCarriageFallbacks_RoutingLegsWithEmptyRoutingLoadAndEmptyRoutingDischarge()
		{
			// Arrange shipment with Routing legs but leg load and discharge are empty
			// PickupFrom: AUMEL
			// DepartureCFS: AUDRW
			// ArrivalCFS: AUBWU
			// DeliverTo: AUSYD
			var shipmentBO = CreateShipmentBOWithPrePostCarriageLocationsAndNoRoutingLegs();
			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = ZString.Empty;
			leg1.JW_RL_NKDiscPort = ZString.Empty;

			// Assert
			// PreCarriageCollection: AUMEL -> AUDRW
			// TransportLegCollection: AUDRW -> AUBWU
			// PostCarriageCollection: AUBWU -> AUSYD
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolPrePostCarriageLegs_NoRoutingLegs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPreMainPostCarriageFallbacks_RoutingLegsWithPopulatedRoutingLoadAndEmptyRoutingDischarge()
		{
			// Arrange with Routing legs where Routing Load is populated but Routing Discharge is empty
			// PickupFrom: AUMEL
			// DepartureCFS: AUDRW
			// ArrivalCFS: AUBWU
			// DeliverTo: AUSYD
			// Routing Load Port: NZAKL
			// Routing Discharge Port: None
			var shipmentBO = CreateShipmentBOWithPrePostCarriageLocationsAndNoRoutingLegs();
			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NZAKL";
			leg1.JW_RL_NKDiscPort = ZString.Empty;

			// Assert
			// PreCarriageCollection: AUMEL -> AUDRW -> NZAKL
			// TransportLegCollection: NZAKL -> AUBWU, 
			// PostCarriageCollection: AUBWU -> AUSYD
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolPrePostCarriageLegs_OneRoutingLegEmptyRoutingDischarge.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPreMainPostCarriageFallbacks_RoutingLegsWithEmptyRoutingLoadAndPopulatedRoutingDischarge()
		{
			// Arrange with Routing legs where Routing Load is empty but Routing Discharge is populated
			// PickupFrom: AUMEL
			// DepartureCFS: AUDRW
			// ArrivalCFS: AUBWU
			// DeliverTo: AUSYD
			// Routing Load Port: None
			// Routing Discharge Port: NZCHC
			var shipmentBO = CreateShipmentBOWithPrePostCarriageLocationsAndNoRoutingLegs();
			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = ZString.Empty;
			leg1.JW_RL_NKDiscPort = "NZCHC";

			// Assert
			// PreCarriageCollection: AUMEL -> AUDRW
			// TransportLegCollection: AUDRW -> NZCHC, 
			// PostCarriageCollection: NZCHC -> AUBWU -> AUSYD

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolPrePostCarriageLegs_OneRoutingLegEmptyRoutingLoad.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPreMainPostCarriageFallbacks_RoutingLegsBothRoutingLoadAndRoutingDischargeArePopulated()
		{
			// Arrange with Routing legs where both Routing Load and Discharge are populated
			// PickupFrom: AUMEL
			// DepartureCFS: AUDRW
			// ArrivalCFS: AUBWU
			// DeliverTo: AUSYD
			// Routing Load Port: NZAKL
			// Routing Discharge Port: NZCHC
			var shipmentBO = CreateShipmentBOWithPrePostCarriageLocationsAndNoRoutingLegs();
			var leg1 = shipmentBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "NZAKL";
			leg1.JW_RL_NKDiscPort = "NZCHC";

			// Assert
			// PreCarriageCollection: AUMEL -> AUDRW -> NZAKL
			// TransportLegCollection: NZAKL -> NZCHC, 
			// PostCarriageCollection: NZCHC -> AUBWU -> AUSYD

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithoutConsolPrePostCarriageLegs_OneRoutingLeg.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestShipmentCO2eRequestDataObjectWriter_SubShipmentContainerModeTagIsFCL_WhenConsolContainerModeIsGRP()
		{
			// Arrange
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consolBO = shipmentBO.Consols.AddNew();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			// Act
			var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));
			var shipmentData = writer.GetDataObject(shipmentBO);

			// Assert
			AssertNotNull("shipmentData", shipmentData);
			AssertEquals("shipmentData.SubShipmentCollection.Count", 1, shipmentData.SubShipmentCollection.Count);
			AssertEquals("shipmentData.SubShipmentCollection[0].ContainerMode", "FCL", shipmentData.SubShipmentCollection[0].ContainerMode.Code);
		}

		public void TestPopulatePrePostCarriageLegs_WithConsols_AndTransportBookings()
		{
			// Arrange
			// Departure consol has Pickup TB
			// Arrival consol has Delivery TB
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Qingdao", "CN", "CNTAO");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Qingdao", "CN", "CNTAO");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_UniqueConsignRef = "S0001";
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "CNTAO";
			shipmentBO.JS_ActualWeight = 100m;
			var shipmentTransport1 = shipmentBO.Transports.AddNew();
			shipmentTransport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTransport1.JW_RL_NKLoadPort = "NZAKL";
			shipmentTransport1.JW_RL_NKDiscPort = "SGSIN";
			var shipmentTransport2 = shipmentBO.Transports.AddNew();
			shipmentTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTransport2.JW_RL_NKLoadPort = "SGSIN";
			shipmentTransport2.JW_RL_NKDiscPort = "VNSGN";

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.PickupByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.CFSDepartureByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;
			shipmentBO.CFSArrivalByTransportMode = Core.Constants.TransportModes.Rail;

			var address5 = CreateAddress(ZGeography.CreatePoint(50, 50), "3066", "Collingwood", "AU", "AUMEL");
			var address6 = CreateAddress(ZGeography.CreatePoint(60, 60), "9999", "Auckland", "NZ", "NZAKL");
			var address7 = CreateAddress(ZGeography.CreatePoint(70, 70), "11", "District 1", "VN", "VNSGN");
			var address8 = CreateAddress(ZGeography.CreatePoint(80, 80), "22", "Shanghai", "CN", "CNSHA");
			var address9 = CreateAddress(ZGeography.CreatePoint(90, 90), "33", "Shanghai", "CN", "CNSHA");
			var address10 = CreateAddress(ZGeography.CreatePoint(15, 15), "44", "Qingdao", "CN", "CNTAO");

			var consol1 = shipmentBO.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			consol1.JK_TransportMode = "SEA";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			var consol1Transport2 = consol1.Transports.AddNew();
			consol1Transport2.JW_TransportMode = "SEA";
			consol1Transport2.JW_RL_NKLoadPort = "AUSYD";
			consol1Transport2.JW_RL_NKDiscPort = "NZAKL";
			consol1.JK_OA_PackDepotAddress = address5.PK;
			consol1.CFSDepartureByTransportMode = Core.Constants.TransportModes.Rail;
			consol1.JK_OA_UnpackDepotAddress = address6.PK;
			consol1.CFSArrivalByTransportMode = Core.Constants.TransportModes.Road;
			CO2eTestHelper.CreateTransportBooking(consol1, nameof(DtbBookingDirection.PIC), Factory);

			var consol2 = shipmentBO.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "VNSGN";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.Transports[0].JW_RL_NKDiscPort = "VNHAN";
			var consol2Transport2 = consol2.Transports.AddNew();
			consol2Transport2.JW_TransportMode = "SEA";
			consol2Transport2.JW_RL_NKLoadPort = "VNHAN";
			consol2Transport2.JW_RL_NKDiscPort = "CNSHA";
			consol2.JK_OA_PackDepotAddress = address7.PK;
			consol2.CFSDepartureByTransportMode = Core.Constants.TransportModes.Road;
			consol2.JK_OA_UnpackDepotAddress = address8.PK;
			consol2.CFSArrivalByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

			var consol3 = shipmentBO.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";
			consol3.JK_TransportMode = "AIR";
			consol3.JK_RL_NKLoadPort = "CNSHA";
			consol3.JK_RL_NKDischargePort = "CNTAO";
			consol3.JK_OA_PackDepotAddress = address9.PK;
			consol3.JK_OA_UnpackDepotAddress = address10.PK;
			CO2eTestHelper.CreateTransportBooking(consol3, nameof(DtbBookingDirection.DLV), Factory);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				// Act & Assert
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithConsolsPrePostCarriageLegs_ConsolsHaveTBs.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		public void TestPopulatePrePostCarriageLegs_WithConsols_AndTransportBookings_WithoutConsolCFS()
		{
			// Arrange
			// Departure consol has Pickup TB, missing CFS Pack
			// Arrival consol has Delivery TB, mising CFS Unpack
			var address1 = CreateAddress(ZGeography.CreatePoint(10, 10), "3000", "CBD", "AU", "AUMEL");
			var address2 = CreateAddress(ZGeography.CreatePoint(20, 20), "3051", "North Melbourne", "AU", "AUMEL");
			var address3 = CreateAddress(ZGeography.CreatePoint(30, 30), "1000", "Qingdao", "CN", "CNTAO");
			var address4 = CreateAddress(ZGeography.CreatePoint(40, 40), "2000", "Qingdao", "CN", "CNTAO");

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_UniqueConsignRef = "S0001";
			shipmentBO.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentBO.JS_RL_NKOrigin = "AUMEL";
			shipmentBO.JS_RL_NKDestination = "CNTAO";
			shipmentBO.JS_ActualWeight = 100m;
			var shipmentTransport1 = shipmentBO.Transports.AddNew();
			shipmentTransport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTransport1.JW_RL_NKLoadPort = "NZAKL";
			shipmentTransport1.JW_RL_NKDiscPort = "SGSIN";
			var shipmentTransport2 = shipmentBO.Transports.AddNew();
			shipmentTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentTransport2.JW_RL_NKLoadPort = "SGSIN";
			shipmentTransport2.JW_RL_NKDiscPort = "VNSGN";

			shipmentBO.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			shipmentBO.PickupByTransportMode = Core.Constants.TransportModes.Rail;
			shipmentBO.JS_OA_ExportReceivingDepot = address2.PK;
			shipmentBO.CFSDepartureByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = address3.PK;
			shipmentBO.JS_OA_ImportReleaseDepot = address4.PK;
			shipmentBO.CFSArrivalByTransportMode = Core.Constants.TransportModes.Rail;

			var address5 = CreateAddress(ZGeography.CreatePoint(60, 60), "9999", "Auckland", "NZ", "NZAKL");
			var address6 = CreateAddress(ZGeography.CreatePoint(70, 70), "11", "District 1", "VN", "VNSGN");
			var address7 = CreateAddress(ZGeography.CreatePoint(80, 80), "22", "Shanghai", "CN", "CNSHA");
			var address8 = CreateAddress(ZGeography.CreatePoint(90, 90), "33", "Shanghai", "CN", "CNSHA");

			var consol1 = shipmentBO.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			consol1.JK_TransportMode = "SEA";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			var consol1Transport2 = consol1.Transports.AddNew();
			consol1Transport2.JW_TransportMode = "SEA";
			consol1Transport2.JW_RL_NKLoadPort = "AUSYD";
			consol1Transport2.JW_RL_NKDiscPort = "NZAKL";
			consol1.JK_OA_UnpackDepotAddress = address5.PK;
			consol1.CFSArrivalByTransportMode = Core.Constants.TransportModes.Road;
			CO2eTestHelper.CreateTransportBooking(consol1, nameof(DtbBookingDirection.PIC), Factory);

			var consol2 = shipmentBO.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "VNSGN";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.Transports[0].JW_RL_NKDiscPort = "VNHAN";
			var consol2Transport2 = consol2.Transports.AddNew();
			consol2Transport2.JW_TransportMode = "SEA";
			consol2Transport2.JW_RL_NKLoadPort = "VNHAN";
			consol2Transport2.JW_RL_NKDiscPort = "CNSHA";
			consol2.JK_OA_PackDepotAddress = address6.PK;
			consol2.CFSDepartureByTransportMode = Core.Constants.TransportModes.Road;
			consol2.JK_OA_UnpackDepotAddress = address7.PK;
			consol2.CFSArrivalByTransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

			var consol3 = shipmentBO.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";
			consol3.JK_TransportMode = "AIR";
			consol3.JK_RL_NKLoadPort = "CNSHA";
			consol3.JK_RL_NKDischargePort = "CNTAO";
			consol3.JK_OA_PackDepotAddress = address8.PK;
			CO2eTestHelper.CreateTransportBooking(consol3, nameof(DtbBookingDirection.DLV), Factory);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				// Act & Assert
				var writer = new ShipmentCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)));

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataXMLString = UniversalTestHelper.GetXml(writer.GetDataObject(shipmentBO)).Trim();
					var expectedXMLString = resourceRetriever.GetString("Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.CO2e.TestFiles.ShipmentWithConsolsPrePostCarriageLegs_ConsolsHaveTBs_NoConsolCFS.xml");
					AssertMultilineASCIIEquals(expectedXMLString, shipmentDataXMLString);
				}
			}
		}

		OrgAddress CreateAddress(ZGeography geoLocation, string postcode, string city, string countryCode, string port)
		{
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address.OA_GeoLocation = geoLocation;
			address.OA_City = city;
			address.OA_PostCode = postcode;
			address.OA_RN_NKCountryCode = countryCode;
			address.OA_RL_NKRelatedPortCode = port;
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			return address;
		}

		#endregion
	}
}
