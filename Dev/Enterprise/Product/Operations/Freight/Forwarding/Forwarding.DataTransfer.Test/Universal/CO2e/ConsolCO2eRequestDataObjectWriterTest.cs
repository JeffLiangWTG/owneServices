using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolCO2eRequestDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateBusinessObject()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_RL_NKLoadPort = "AUSYD";
			consolBO.JK_RL_NKDischargePort = "CNSHA";
			consolBO.JK_TotalShipmentActWeightCheck = 10m;
			consolBO.JK_TotalShipmentActOtherUnit = "KG";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			consolBO.Transports.RemoveAll();
			var leg1 = consolBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "SGSIN";
			leg1.JW_TransportMode = "SEA";
			leg1.JW_VoyageFlight = "VOY123";
			leg1.JW_ETD = new ZDateTime(2022, 1, 1);
			leg1.JW_ETA = new ZDateTime(2022, 1, 10);
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER 1";
			var cusCode1 = carrier1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = "US";
			cusCode1.OK_CustomsRegNo = "1111";
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;

			var leg2 = consolBO.Transports.AddNew();
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
			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			AssertNotNull("consolData", consolData);
			AssertEquals("consolData.TransportLegCollection.Count", 2, consolData.TransportLegCollection.Count);
			AssertEquals("consolData.PortOfLoading", "AUSYD", consolData.PortOfLoading.Code);
			AssertEquals("consolData.PortOfDischarge", "CNSHA", consolData.PortOfDischarge.Code);
			AssertEquals("consolData.TotalWeight", 10m, consolData.TotalWeight);
			AssertEquals("consolData.TotalWeightUnit", "KG", consolData.TotalWeightUnit?.Code);
			AssertEquals("consolData.TransportMode", "SEA", consolData.TransportMode.Code);
			AssertEquals("consolData.ContainerMode", "FCL", consolData.ContainerMode.Code);
			AssertEquals("consolData.TransportLegCollection.Count", 2, consolData.TransportLegCollection.Count);

			AssertEquals("consolData.TransportLegCollection[0].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Sea), consolData.TransportLegCollection[0].TransportMode);
			AssertEquals("consolData.TransportLegCollection[0].VoyageFlightNo", "VOY123", consolData.TransportLegCollection[0].VoyageFlightNo);
			AssertEquals("consolData.TransportLegCollection[0].AircraftType.Code", ZString.Empty, consolData.TransportLegCollection[0].AircraftType.Code);
			AssertEquals("consolData.TransportLegCollection[0].PortOfLoading.Code", "AUSYD", consolData.TransportLegCollection[0].PortOfLoading.Code);
			AssertEquals("consolData.TransportLegCollection[0].PortOfDischarge.Code", "SGSIN", consolData.TransportLegCollection[0].PortOfDischarge.Code);
			AssertEquals("consolData.TransportLegCollection[0].Carrier.RegistrationNumber", "1111", consolData.TransportLegCollection[0].Carrier.RegistrationNumberCollection[0].Value);
			AssertEquals("consolData.TransportLegCollection[0].EstimatedDeparture", new ZDateTime(2022, 1, 1), consolData.TransportLegCollection[0].EstimatedDeparture);
			AssertEquals("consolData.TransportLegCollection[0].EstimatedArrival", new ZDateTime(2022, 1, 10), consolData.TransportLegCollection[0].EstimatedArrival);

			AssertEquals("consolData.TransportLegCollection[1].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Air), consolData.TransportLegCollection[1].TransportMode);
			AssertEquals("consolData.TransportLegCollection[1].VoyageFlightNo", "FL123", consolData.TransportLegCollection[1].VoyageFlightNo);
			AssertEquals("consolData.TransportLegCollection[1].AircraftType", "E90", consolData.TransportLegCollection[1].AircraftType.Code);
			AssertEquals("consolData.TransportLegCollection[1].PortOfLoading.Code", "SIN", consolData.TransportLegCollection[1].PortOfLoading.Code);
			AssertEquals("consolData.TransportLegCollection[1].PortOfDischarge.Code", "SHA", consolData.TransportLegCollection[1].PortOfDischarge.Code);
			AssertEquals("consolData.TransportLegCollection[1].Carrier.RegistrationNumber", "2222", consolData.TransportLegCollection[1].Carrier.RegistrationNumberCollection[0].Value);
		}

		public void TestPopulateVirtualLegs()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "AIR";
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "CNSHA";

			consolBO.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consolBO.Transports[0].JW_RL_NKDiscPort = "SGSIN";

			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var transportModeConverter = new TransportModeConverter();
			var consolData = writer.GetDataObject(consolBO);
			AssertNotNull("consolData", consolData);
			AssertEquals("consolData.TransportLegCollection.Count", 3, consolData.TransportLegCollection.Count);

			AssertEquals("consolData.TransportLegCollection[0].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Air), consolData.TransportLegCollection[0].TransportMode);
			AssertEquals("consolData.TransportLegCollection[0].PortOfLoading.Code", "MEL", consolData.TransportLegCollection[0].PortOfLoading.Code);
			AssertEquals("consolData.TransportLegCollection[0].PortOfDischarge.Code", "SYD", consolData.TransportLegCollection[0].PortOfDischarge.Code);

			AssertEquals("consolData.TransportLegCollection[1].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Air), consolData.TransportLegCollection[1].TransportMode);
			AssertEquals("consolData.TransportLegCollection[1].PortOfLoading.Code", "SYD", consolData.TransportLegCollection[1].PortOfLoading.Code);
			AssertEquals("consolData.TransportLegCollection[1].PortOfDischarge.Code", "SIN", consolData.TransportLegCollection[1].PortOfDischarge.Code);

			AssertEquals("consolData.TransportLegCollection[2].TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Air), consolData.TransportLegCollection[2].TransportMode);
			AssertEquals("consolData.TransportLegCollection[2].PortOfLoading.Code", "SIN", consolData.TransportLegCollection[2].PortOfLoading.Code);
			AssertEquals("consolData.TransportLegCollection[2].PortOfDischarge.Code", "SHA", consolData.TransportLegCollection[2].PortOfDischarge.Code);
		}

		public void TestPopulateIATAForAir()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "AIR";
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "CNSHA";

			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);
			AssertNotNull("consolData", consolData);
			AssertEquals("consolData.PortOfLoading", "MEL", consolData.PortOfLoading.Code);
			AssertEquals("consolData.PortOfDischarge", "SHA", consolData.PortOfDischarge.Code);
			AssertEquals("consolData.TransportLegCollection.Count", 1, consolData.TransportLegCollection.Count);
			AssertEquals("consolData.TransportLegCollection[0].PortOfLoading.Code", "MEL", consolData.TransportLegCollection[0].PortOfLoading.Code);
			AssertEquals("consolData.TransportLegCollection[0].PortOfDischarge.Code", "SHA", consolData.TransportLegCollection[0].PortOfDischarge.Code);

			var query = new ZQuery(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.Equal, ZString.Empty);
			query.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU");
			var unlocoWithoutIATA = Factory.LoadTop1<RefUNLOCO>(query);
			consolBO.JK_RL_NKLoadPort = unlocoWithoutIATA.RL_Code;

			writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			consolData = writer.GetDataObject(consolBO);
			AssertEquals("consolData.PortOfLoading fallback to UNLOCO when IATA is not configured", unlocoWithoutIATA.RL_Code, consolData.PortOfLoading.Code);
		}

		public void TestPopulateVoyageFlightNoFromCarrierIfEmpty()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "AIR";
			consolBO.JK_RL_NKLoadPort = "AUSYD";
			consolBO.JK_RL_NKDischargePort = "CNSHA";

			consolBO.Transports.RemoveAll();
			var leg1 = consolBO.Transports.AddNew();
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

			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			AssertNotNull("consolData", consolData);
			AssertEquals("consolData.TransportLegCollection.Count", 1, consolData.TransportLegCollection.Count);
			AssertEquals("consolData.TransportLegCollection[0].VoyageFlightNo", "AZ", consolData.TransportLegCollection[0].VoyageFlightNo);
		}

		public void TestPopulateTEU()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBO.JK_RL_NKLoadPort = "AUSYD";
			consolBO.JK_RL_NKDischargePort = "CNSHA";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			consolBO.Transports.RemoveAll();
			var leg1 = consolBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "CNSHA";
			leg1.JW_TransportMode = Core.Constants.TransportModes.Sea;

			var shipment1 = consolBO.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "CNSHA";
			shipment1.JS_ActualWeight = 2000;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			Shipment consolData;

			#region Empty Containers

			AssertEmptyTEU();

			#endregion

			#region With Containers

			var container1 = consolBO.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_DunnageWeight = 100m;

			var container2 = consolBO.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;
			container2.JC_DunnageWeight = 100m;

			AssertTEU();
			AssertEquals("consolData.TEU.ContainerEmptyWeightPerTEU", 2134.883721m, consolData.TEU.ContainerEmptyWeightPerTEU);
			AssertEquals("consolData.TEU.ContainerEmptyWeightPerTEUUnit.Code", Core.Constants.Weight.Kilograms, consolData.TEU.ContainerEmptyWeightPerTEUUnit.Code);

			#endregion

			#region With Rail Transport

			// Act
			leg1.JW_TransportMode = Core.Constants.TransportModes.Rail;
			consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertEquals("consolData.TEU.ContainerEmptyWeightPerTEU", 2134.883721m, consolData.TEU.ContainerEmptyWeightPerTEU);

			#endregion

			#region TransportMode & ContainerMode

			consolBO.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEmptyTEU();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEmptyTEU();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEmptyTEU();

			consolBO.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			AssertEmptyTEU();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Road;
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertTEU();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Rail;
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertTEU();

			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEmptyTEU();
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.ShippersConsol;
			AssertTEU();

			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			AssertTEU();
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertTEU();

			#endregion

			void AssertEmptyTEU()
			{
				consolData = writer.GetDataObject(consolBO);

				// Assert
				AssertNotNull("consolData", consolData);
				AssertNull("consolData.TEU", consolData.TEU);
			}

			void AssertTEU()
			{
				consolData = writer.GetDataObject(consolBO);

				// Assert
				AssertNotNull("consolData", consolData);
				AssertNotNull("consolData.TEU", consolData.TEU);
				AssertEquals("consolData.TEU.NumberOfTEU", 1 * 2 + 2.3m * 1m, consolData.TEU.NumberOfTEU);
				AssertEquals("consolData.TEU.TonnesPerTEU", Utilities.Round(2 / (1m * 2m + 2.3m * 1m), 6), consolData.TEU.TonnesPerTEU);
			}
		}

		public void TestConsolCO2eRequestDataObjectWriter_ConsolContainerModeTagIsFCL_WhenConsolContainerModeIsGRP()
		{
			// Arrange
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBO.JK_RL_NKLoadPort = "AUSYD";
			consolBO.JK_RL_NKDischargePort = "CNSHA";
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			// Act
			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			// Assert
			AssertNotNull("consolData", consolData);
			AssertEquals("consolData.ContainerMode", "FCL", consolData.ContainerMode.Code);
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
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "CNSHA";
			consolBO.JK_RequiresTemperatureControl = false;

			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);
			AssertEquals("consolData.RequiresTemperatureControl is null because Is Temperature Controled is not checked", null, consolData.RequiresTemperatureControl);

			consolBO.JK_RequiresTemperatureControl = true;
			consolData = writer.GetDataObject(consolBO);
			AssertEquals("consolData.RequiresTemperatureControl", true, consolData.RequiresTemperatureControl);
		}

		public void TestRemoveUnusedFields()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Air;
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKDischargePort = "CNSHA";
			consolBO.JK_RequiresTemperatureControl = false;

			consolBO.Transports.RemoveAll();
			var leg1 = consolBO.Transports.AddNew();
			leg1.JW_RL_NKLoadPort = "AUMEL";
			leg1.JW_RL_NKDiscPort = "CNSHA";
			leg1.JW_TransportMode = "SEA";
			leg1.JW_VoyageFlight = "VOY123";
			leg1.JW_ETD = new ZDateTime(2022, 1, 1);
			leg1.JW_ETA = new ZDateTime(2022, 1, 10);
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "CARRIER 1";
			var cusCode1 = carrier1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = "US";
			cusCode1.OK_CustomsRegNo = "1111";
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;

			var writer = new ConsolCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			AssertNull(consolData.CustomizedFieldCollection);
			AssertNull(consolData.MilestoneCollection);
			AssertNull(consolData.ExceptionCollection);
			AssertNull(consolData.JobCosting);
			AssertNull(consolData.ConsolCosts);
			var legData = consolData.TransportLegCollection[0];
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
	}
}
