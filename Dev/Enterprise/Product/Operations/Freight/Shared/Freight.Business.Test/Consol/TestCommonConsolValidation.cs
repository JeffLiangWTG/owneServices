using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TestCommonConsolValidation : BaseFreightTest
	{
		public void TestValidateJK_RL_NKLoad()
		{
			Consol.JK_RL_NKLoadPort = "";
			Consol.Validation.ValidateJK_RL_NKLoadPort();
			AssertHasErrors("Is mandatory.", Consol.JK_RL_NKLoadPortInfo);

			Consol.JK_RL_NKLoadPort = "AUSYD";
			AssertNoErrors("Is valid port", Consol.JK_RL_NKLoadPortInfo);

			Consol.JK_RL_NKLoadPort = "ZZZZZ";
			AssertHasErrors("Must be a valid port", Consol.JK_RL_NKLoadPortInfo);
		}

		public void TestValidateJK_RL_NKDischarge()
		{
			Consol.JK_RL_NKDischargePort = "";
			Consol.Validation.ValidateJK_RL_NKDischargePort();
			AssertHasErrors("Is mandatory.", Consol.JK_RL_NKDischargePortInfo);

			Consol.JK_RL_NKDischargePort = "AUSYD";
			AssertNoErrors("Is valid port", Consol.JK_RL_NKDischargePortInfo);

			Consol.JK_RL_NKDischargePort = "ZZZZZ";
			AssertHasErrors("Must be a valid port", Consol.JK_RL_NKDischargePortInfo);
		}

		public void TestValidateJK_TransportMode()
		{
			Consol.JK_TransportMode = "ABC";
			AssertHasErrors("Invalid TransportMode. Error expected.", Consol.JK_TransportModeInfo);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertNoErrors("Valid TransportMode. No error expected.", Consol.JK_TransportModeInfo);

			Consol.JK_TransportMode = "";
			AssertHasErrors("Empty TransportMode. Error expected.", Consol.JK_TransportModeInfo);
		}

		public void TestValidateJK_AgentType()
		{
			Consol.JK_AgentType = "";
			Assert("Empty AgentType. Error expected.", Consol.JK_AgentTypeInfo.HasErrors());

			Consol.JK_AgentType = "ABC";
			Assert("Invalid AgentType. Error expected.", Consol.JK_AgentTypeInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			Assert("Valid AgentType. No error expected.", !Consol.JK_AgentTypeInfo.HasErrors());

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S12345";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master.");

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			consol.Validation.ValidateJK_AgentType();
			AssertNoError(consol.JK_AgentTypeInfo, "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master.");

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			consol.Validation.ValidateJK_AgentType();
			AssertNoError(consol.JK_AgentTypeInfo, "Direct Consol can only contain a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no Coload Master.");

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S12346";
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments.");

			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertNoError(consol.JK_AgentTypeInfo, "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments.");

			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "The following shipment(s) is/are already attached to at least one other Direct Consol: S12346. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).");

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertNoError(consol.JK_AgentTypeInfo, "The following shipment(s) is/are already attached to at least one other Direct Consol: S12346. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).");

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_JS_ColoadMasterShipment = shipment.PK;
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Consol cannot be marked as Direct, as these shipment(s) is/are already attached to at least one other non-Direct Consol: S12346.");

			CreateShipmentAttachedNonDirectConsol(consol, shipment, "S12347");
			CreateShipmentAttachedNonDirectConsol(consol, shipment, "S12348");

			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Consol cannot be marked as Direct, as these shipment(s) is/are already attached to at least one other non-Direct Consol: S12346, S12347, S12348.");

			CreateShipmentAttachedNonDirectConsol(consol, shipment, "S12349");
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Consol cannot be marked as Direct, as these and other shipment(s) are already attached to at least one other non-Direct Consol: S12346, S12347, S12348.");
		}

		public void TestValidateJK_AgentType_AttachDirectShipmentToAgentConsol()
		{
			Consol.JK_AgentType = Constants.AgentType.Direct;
			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			Consol.Transports.DepartureTransport.JW_ETD = ZDateTime.Today;
			Consol.Transports.ArrivalTransport.JW_ETA = ZDateTime.Today.AddDays(2);

			var shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_UniqueConsignRef = "S12345";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			Factory.Save();

			AssertAttachDirectShipmentToAgentConsol_HasWarning(shipment, Constants.TransportModes.Road, "USCHI", "USLAX", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2));
			AssertAttachDirectShipmentToAgentConsol_HasWarning(shipment, Constants.TransportModes.Sea, "AUSYD", "AUMEL", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(6));
			AssertAttachDirectShipmentToAgentConsol_HasError("AGT SEA consol is not domestic", shipment, Constants.TransportModes.Sea, "AUSYD", "NZAKL", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(6));
			AssertAttachDirectShipmentToAgentConsol_HasError("AGT consol arrival date is after departure DRT consol", shipment, Constants.TransportModes.Road, "USCHI", "USLAX", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(1));
			AssertAttachDirectShipmentToAgentConsol_HasError("AGT consol departure date is before arrival DRT consol", shipment, Constants.TransportModes.Sea, "AUSYD", "AUMEL", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(6));
			AssertAttachDirectShipmentToAgentConsol_HasError("AGT consol discharge port does not match departure DRT consol's load port", shipment, Constants.TransportModes.Road, "USCHI", "USNYC", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2));
			AssertAttachDirectShipmentToAgentConsol_HasError("AGT consol load port does not match arrival DRT consol's discharge port", shipment, Constants.TransportModes.Sea, "AUBNE", "AUMEL", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(6));
		}

		void AssertAttachDirectShipmentToAgentConsol_HasWarning(CommonShipment shipment, string transportMode, string loadPort, string dischargePort, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONXXX01", transportMode, Constants.AgentType.Agent, loadPort, dischargePort, departureDate, arrivalDate, Factory, shipment);
			consol.Validation.ValidateJK_AgentType();
			AssertNoErrors(consol.JK_AgentTypeInfo);
			AssertHasWarning(consol.JK_AgentTypeInfo, $"The following shipment(s) is/are already attached to at least one other Direct Consol: {shipment.JS_UniqueConsignRef}. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).");

			var directConsol = shipment.Consols.Cast<CommonConsol>().First(c => c.IsDirect);
			directConsol.Validation.ValidateJK_AgentType();
			AssertNoErrors(directConsol.JK_AgentTypeInfo);

			shipment.Consols.Remove(consol);
		}

		void AssertAttachDirectShipmentToAgentConsol_HasError(string message, CommonShipment shipment, string transportMode, string loadPort, string dischargePort, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONXXX01", transportMode, Constants.AgentType.Agent, loadPort, dischargePort, departureDate, arrivalDate, Factory, shipment);
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(message, consol.JK_AgentTypeInfo, $"The following shipment(s) is/are already attached to at least one other Direct Consol: {shipment.JS_UniqueConsignRef}. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).");
		}

		void CreateShipmentAttachedNonDirectConsol(CommonConsol consol, CommonShipment masterShipment, string uniqueConsignRef)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = uniqueConsignRef;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			var newConsol = shipment.Consols.AddNew();
			newConsol.JK_AgentType = Constants.AgentType.Agent;
		}

		public void TestAreAllShipmentsValidForDirectConsol()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			consol.Shipments.Add(shipment1);
			consol.Validation.ValidateJK_AgentType();
			AssertNoError(consol.JK_AgentTypeInfo, "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments.");

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			consol.Shipments.Add(shipment2);
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments.");

			shipment1.CoLoadShipments.Add(shipment2);
			AssertEquals(true, consol.Shipments.Contains(shipment2.PK));
			consol.Validation.ValidateJK_AgentType();
			AssertHasError(consol.JK_AgentTypeInfo, "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments.");

			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			consol.Validation.ValidateJK_AgentType();
			AssertNoError(consol.JK_AgentTypeInfo, "Direct Consol can only have 1 Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with Standard House sub-shipments.");
		}

		public void TestValidateJK_PrepaidCollect()
		{
			Consol.JK_PrepaidCollect = "";
			Assert("Empty PrepaidCollect. No error expected.", !Consol.JK_PrepaidCollectInfo.HasErrors());

			Consol.JK_PrepaidCollect = "ABC";
			Assert("Invalid PrepaidCollect. Error expected.", Consol.JK_PrepaidCollectInfo.HasErrors());

			Consol.JK_PrepaidCollect = "CCX";
			Assert("Valid PrepaidCollect. No error expected.", !Consol.JK_PrepaidCollectInfo.HasErrors());
		}

		public void TestValidateJK_ConsolMode()
		{
			Consol.JK_ConsolMode = "ABC";
			Assert("ABC is an invalid ConsolMode. Error expected.", Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			Assert("Loose is valid for Air. No error expected.", !Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Assert("LCL is invalid for Air. Error expected.", Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Assert("FCL is valid for Sea. No error expected.", !Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.Other;
			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Assert("LCL is invalid for Sea, Agent=Other. Error expected.", Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			Assert("GRP is valid for Sea, Agent=Agent. No error expected.", !Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.Direct;
			Consol.Validation.ValidateJK_ConsolMode();
			Assert("GRP is invalid for Sea, Agent=Direct. Error expected.", Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Other;
			Consol.JK_ConsolMode = Constants.ContainerModes.Other;
			Assert("OTH is valid for Other. No error expected.", !Consol.JK_ConsolModeInfo.HasErrors());

			Consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			Assert("GRP is invalid for Other. Error expected.", Consol.JK_ConsolModeInfo.HasErrors());
		}

		public void TestValidateJK_RL_NKLastForeignPort()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_RL_NKDischargePort = HomePort;
			Consol.JK_RL_NKLastForeignPort = "";
			Assert("JK_RL_NKLastForeignPort - No error expected", !Consol.JK_RL_NKLastForeignPortInfo.HasErrors());

			Consol.JK_RL_NKDischargePort = USPort;
			Consol.Validation.ValidateJK_RL_NKLastForeignPort();
			Assert("JK_RL_NKLastForeignPort - Error expected", Consol.JK_RL_NKLastForeignPortInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.Validation.ValidateJK_RL_NKLastForeignPort();
			Assert("JK_RL_NKLastForeignPort - No error expected", !Consol.JK_RL_NKLastForeignPortInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_RL_NKDischargePort = HomePort;
			Consol.JK_RL_NKLoadPort = OverseasPort2;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = OverseasPort2;
			transport.JW_RL_NKDiscPort = USPort;
			Consol.Validation.ValidateJK_RL_NKLastForeignPort();
			Assert("JK_RL_NKLastForeignPort - Error expected", Consol.JK_RL_NKLastForeignPortInfo.HasErrors());
			transport.JW_RL_NKLoadPort = USPortAlt;
			Consol.Validation.ValidateJK_RL_NKLastForeignPort();
			Assert("JK_RL_NKLastForeignPort - No error expected", !Consol.JK_RL_NKLastForeignPortInfo.HasErrors());

			Consol.JK_RL_NKDischargePort = "PRSJU";
			Consol.Validation.ValidateJK_RL_NKLastForeignPort();
			Assert("JK_RL_NKLastForeignPort - Error expected", Consol.JK_RL_NKLastForeignPortInfo.HasErrors());
			AssertHasError("should be entered", Consol.JK_RL_NKLastForeignPortInfo, "Please enter a Last Foreign Port.");

			Consol.JK_RL_NKDischargePort = "ASABC";
			Consol.Validation.ValidateJK_RL_NKLastForeignPort();
			Assert("JK_RL_NKLastForeignPort - Error expected", Consol.JK_RL_NKLastForeignPortInfo.HasErrors());
			AssertHasError("should be entered", Consol.JK_RL_NKLastForeignPortInfo, "Please enter a Last Foreign Port.");
		}

		public void TestValidateJK_ConsolChargeable()
		{
			Consol.JK_OverrideConsolChargeable = true;

			Consol.JK_ConsolChargeable = 0;
			AssertNoErrors("Zero ConsolChargeable. No error expected.", Consol.JK_ConsolChargeableInfo);

			Consol.JK_ConsolChargeable = -200;
			AssertHasError("Negative ConsolChargeable. Error expected.", Consol.JK_ConsolChargeableInfo,
				"Consol Chargeable quantity should not be less than zero.");

			Consol.JK_ConsolChargeable = 200;
			AssertNoErrors("Valid ConsolChargeable. No error expected.", Consol.JK_ConsolChargeableInfo);
		}

		public void TestJK_CorrectedConsolWeight()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ActualWeight = 2000;
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;

			CommonConsol consol = shipment.Consols.AddNew();
			consol.Validation.ValidateJK_CorrectedConsolWeight();
			AssertEquals(2000000m, consol.JK_TotalShipmentWeight);
			AssertEquals(Constants.Weight.Kilograms, consol.JK_TotalShipmentWeightUnit);
			AssertEquals(false, consol.JK_OverrideConsolChargeable);
			AssertNoErrors("Shouldn't have error", consol.JK_CorrectedConsolWeightInfo);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 1000m;
			consol.Validation.ValidateJK_CorrectedConsolWeight();
			AssertNoErrors("Shouldn't have error", consol.JK_CorrectedConsolWeightInfo);

			consol.JK_CorrectedConsolWeight = 1000000m;
			consol.Validation.ValidateJK_CorrectedConsolWeight();
			AssertHasErrors("Should have error: Weight is overridden and too large to store in database", consol.JK_CorrectedConsolWeightInfo);
		}

		public void TestJK_CorrectedConsolWeightUnit()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ActualWeight = 2000m;
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;

			var consol = shipment.Consols.AddNew();
			consol.Validation.ValidateJK_CorrectedConsolWeightUnit();
			AssertEquals(Constants.Weight.Kilograms, consol.JK_CorrectedConsolWeightUnit);
			AssertNoErrors("Shouldn't have error", consol.JK_CorrectedConsolWeightUnitInfo);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 1000m;
			consol.JK_CorrectedConsolWeightUnit = "P";
			consol.Validation.ValidateJK_CorrectedConsolWeightUnit();
			AssertHasErrors("Should have error", consol.JK_CorrectedConsolWeightUnitInfo);
		}

		public void TestJK_CorrectedConsolVolumeUnit()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ActualVolume = 20m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			var consol = shipment.Consols.AddNew();
			consol.Validation.ValidateJK_CorrectedConsolVolumeUnit();
			AssertEquals(Constants.Volume.CubicFeet, consol.JK_CorrectedConsolVolumeUnit);
			AssertNoErrors("Shouldn't have error", consol.JK_CorrectedConsolVolumeUnitInfo);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolVolume = 10m;
			consol.JK_CorrectedConsolVolumeUnit = "G3";
			consol.Validation.ValidateJK_CorrectedConsolVolumeUnit();
			AssertHasErrors("Should have error", consol.JK_CorrectedConsolVolumeUnitInfo);
		}

		public void TestCheckJKOAShippingLineAddress_AddressDoesntMatchAnyCarrierOnTransports_GenerateWarning()
		{
			var warningMessage = "The carrier does not match a carrier on any of the routing legs.";

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_FullName = "McLaren";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_FullName = "Button";
			var carrier2Address2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier2Address2.OA_OH = carrier2.PK;

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingLine = true;
			carrier3.OH_FullName = "Perez";
			var carrier3Address2 = Factory.NewWithValidTestData<OrgAddress>();
			carrier3Address2.OA_OH = carrier3.PK;

			var invalidAddress = Factory.New<OrgAddress>();
			invalidAddress.OA_OH = ZGuid.Empty;

			var consol = Factory.New<CommonConsol>();

			var leg1 = consol.Transports.AddNew();
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;
			var leg2 = consol.Transports.AddNew();
			leg2.JW_OA_CarrierAddress = carrier2.MainAddress.PK;

			consol.JK_OA_ShippingLineAddress = carrier3.MainAddress.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertHasWarning("Expected a warning as carrier3 is unrelated to the carriers on both transport legs", consol.JK_OA_ShippingLineAddressInfo, warningMessage);

			consol.JK_OA_ShippingLineAddress = carrier3Address2.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertHasWarning("Expected to still have the warning as the carrier organization is the same as before", consol.JK_OA_ShippingLineAddressInfo, warningMessage);

			consol.JK_OA_ShippingLineAddress = carrier2.MainAddress.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertNoWarning("Expected no warning as the carrier matches a carrier from a rounting leg", consol.JK_OA_ShippingLineAddressInfo, warningMessage);

			consol.JK_OA_ShippingLineAddress = carrier2Address2.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertNoWarning("Expected no warning as it carrier is still the same carrier as the second routing leg, just with a different address", consol.JK_OA_ShippingLineAddressInfo, warningMessage);

			consol.JK_OA_ShippingLineAddress = invalidAddress.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertNoWarning("Expected no warning as the carrier address is invalid, not being linked to a carrier organisation", consol.JK_OA_ShippingLineAddressInfo, warningMessage);
		}

		public void TestCheckJKOAShippingLineAddress_AddressMatchesCarrierOnTransports_ShouldNotHaveWarnings()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_FullName = "McLaren";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_FullName = "Button";

			var consol = Factory.New<CommonConsol>();

			var leg1 = consol.Transports[0];
			leg1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;

			var leg2 = consol.Transports.AddNew();
			leg2.JW_OA_CarrierAddress = carrier2.MainAddress.PK;

			consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertNoWarnings(consol.JK_OA_ShippingLineAddressInfo);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertNoWarnings(consol.JK_OA_ShippingLineAddressInfo);
		}

		#region CheckJK_ScreeningStatus

		public void TestCheckJK_ScreeningStatus()
		{
			var consol = Factory.New<CommonConsol>();
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = string.Empty;
			AssertHasError(consol.JK_ScreeningStatusInfo, "Please enter a Screening Status.");

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(consol.JK_ScreeningStatusInfo);

			consol.JK_ScreeningStatus = "ZZZ";
			AssertHasError(consol.JK_ScreeningStatusInfo, "Enter a valid Screening Status.");
		}

		#endregion

		#region Validating Sending and Receiving Forwarder

		public void TestValidateSendingAndReceivingForwarder()
		{
			//Setting up Sending Forwarder Org with an appointed agent port
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsForwarder = true;
			var seaAppointedAgentPortGB = sendingForwarder.AppointedAgentPorts.AddNew();
			seaAppointedAgentPortGB.O5_OA_AgentOfficeAddress = sendingForwarder.MainAddress.PK;
			seaAppointedAgentPortGB.O5_AgentDirection = AgentDirectionList.Codes.Both;
			seaAppointedAgentPortGB.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			seaAppointedAgentPortGB.O5_PortOrCountry = "GB";

			//Setting up Receiving Forwarder Org with appointed agent port
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsForwarder = true;
			var seaHandlingAgentPortAU = receivingForwarder.AppointedAgentPorts.AddNew();
			seaHandlingAgentPortAU.O5_OA_AgentOfficeAddress = receivingForwarder.MainAddress.PK;
			seaHandlingAgentPortAU.O5_AgentDirection = AgentDirectionList.Codes.Both;
			seaHandlingAgentPortAU.O5_IsHandlesSeaAgent = true;
			seaHandlingAgentPortAU.O5_PortOrCountry = "AUSYD";

			//Setting up a new consol
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_RL_NKLoadPort = "GBLON";
			consol.JK_RL_NKDischargePort = "AUSYD";

			Factory.Save();

			AssertNoErrors("Sending Forwarder empty. Should be no errors.", consol.JK_OA_SendingForwarderAddressInfo);
			AssertNoErrors("Receiving Forwarder empty. Should be no errors.", consol.JK_OA_ReceivingForwarderAddressInfo);

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			AssertNoErrors("Sending Forwarder is a valid, should be no errors.", consol.JK_OA_SendingForwarderAddressInfo);
			AssertNoErrors("Receiving Forwarder empty. Should be no errors.", consol.JK_OA_ReceivingForwarderAddressInfo);

			consol.JK_OA_ReceivingForwarderAddress = sendingForwarder.MainAddress.PK;

			AssertHasError("Receiving Forwarder is the same as the sending forwarder. Should have an error.", consol.JK_OA_ReceivingForwarderAddressInfo, "The Sending and Receiving Forwarder organizations cannot be the same.");

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertNoErrors("Sending Forwarder is different to receiving agent", consol.JK_OA_SendingForwarderAddressInfo);
			AssertNoErrors("Receiving Forwarder is different to sending agent", consol.JK_OA_ReceivingForwarderAddressInfo);

			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertHasErrors("Receiving Forwarder is no long valid as it has no appointed port for AUMEL, only AUSYD.", consol.JK_OA_ReceivingForwarderAddressInfo);

			seaHandlingAgentPortAU.O5_PortOrCountry = "AU";
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertNoErrors("Receiving Forwarder should now be fine as the org has been made a generic appointed agent for Australia", consol.JK_OA_ReceivingForwarderAddressInfo);
		}

		public void TestSendingAndReceivingForwardersForGatewayAgent()
		{
			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				var sendingForwarderAppointedPort = sendingForwarder.AppointedAgentPorts.AddNew();
				sendingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Export;
				sendingForwarderAppointedPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				sendingForwarderAppointedPort.O5_PortOrCountry = "GBLON";

				var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				var receivingForwarderAppointedPort = receivingForwarder.AppointedAgentPorts.AddNew();
				receivingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Import;
				receivingForwarderAppointedPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				receivingForwarderAppointedPort.O5_PortOrCountry = "AUMEL";

				var consol = Factory.NewWithValidTestData<CommonConsol>();
				consol.JK_RL_NKLoadPort = "GBLON";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_TransportMode = Constants.TransportModes.Air;

				consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				AssertNoErrors(consol.JK_OA_ReceivingForwarderAddressInfo);

				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				AssertNoErrors(consol.JK_OA_SendingForwarderAddressInfo);
			}
		}

		public void TestSendingAndReceivingForwardersWhenValidationOfAgentsIsSuppressed()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			var sendingForwarderAppointedPort = sendingForwarder.AppointedAgentPorts.AddNew();

			sendingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Export;
			sendingForwarderAppointedPort.O5_IsHandlesSeaAgent = true;
			sendingForwarderAppointedPort.O5_PortOrCountry = "GBAAA";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			var receivingForwarderAppointedPort = receivingForwarder.AppointedAgentPorts.AddNew();

			receivingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Import;
			receivingForwarderAppointedPort.O5_IsHandlesSeaAgent = true;
			receivingForwarderAppointedPort.O5_PortOrCountry = "AUSYD";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "GBLON";
			consol.JK_RL_NKDischargePort = "AUMEL";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertHasError(consol.JK_OA_ReceivingForwarderAddressInfo, "This organization is not valid for handling  consols in AUMEL. Please choose a new organization or use F3 to amend this organization’s Freight Handling Details on the Fwd/Agent tab.");

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertHasError(consol.JK_OA_SendingForwarderAddressInfo, "This organization is not valid for handling  consols in GBLON. Please choose a new organization or use F3 to amend this organization’s Freight Handling Details on the Fwd/Agent tab.");

			//test validation supress registry item
			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				//reset a value to clear notification
				consol.JK_OA_ReceivingForwarderAddress = Guid.Empty;

				consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				AssertNoErrors(consol.JK_OA_ReceivingForwarderAddressInfo);

				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				AssertNoErrors(consol.JK_OA_SendingForwarderAddressInfo);
			}
		}

		public void TestReceivingForwarderHasBeenChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			var address1 = org.MainAddress;
			address1.OA_Address1 = "ASDF lane 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "SDFG lane 2";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			Factory.Save();

			AssertEquals("Nothing changes", false, consol.Validation.ReceivingForwarderHasBeenChanged());

			consol.JK_OA_ReceivingForwarderAddress = address2.PK;
			Factory.Save();

			AssertEquals("Address is changed", false, consol.Validation.ReceivingForwarderHasBeenChanged());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			AssertEquals("Organization is changed", true, consol.Validation.ReceivingForwarderHasBeenChanged());
		}

		public void TestSendingForwarderHasBeenChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			var address1 = org.MainAddress;
			address1.OA_Address1 = "ASDF lane 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "SDFG lane 2";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			Factory.Save();

			AssertEquals("Nothing changes", false, consol.Validation.SendingForwarderHasBeenChanged());

			consol.JK_OA_SendingForwarderAddress = address2.PK;
			Factory.Save();

			AssertEquals("Address is changed", false, consol.Validation.SendingForwarderHasBeenChanged());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			AssertEquals("Organization is changed", true, consol.Validation.SendingForwarderHasBeenChanged());
		}

		#endregion

		public void TestValidateShipmentRelatedPartiesMatchReceivingAgent()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var shipment = Factory.New<CommonShipment>();

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper
					.SetupSequence(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns("message")
					.Returns(string.Empty)
					.CallBase();

				consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertHasWarning(consol.JK_OA_ReceivingForwarderAddressInfo, "message");
				consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertNoWarnings(consol.JK_OA_ReceivingForwarderAddressInfo);
			}
		}

		public void TestValidateShipmentRelatedPartiesMatchSendingAgent()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper
					.SetupSequence(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns("message")
					.Returns(string.Empty)
					.CallBase();

				consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertHasWarning(consol.JK_OA_SendingForwarderAddressInfo, "message");
				consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				AssertNoWarnings(consol.JK_OA_SendingForwarderAddressInfo);
			}
		}

		#region Implementation

		CommonConsol Consol;

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<CommonConsol>();
		}

		#endregion
	}
}
