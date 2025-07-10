using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentVsConsolMessageHelperTest : TestCaseWithFactory
	{
		#region IsAllowedToAddNew

		public void TestSamplesAreValid()
		{
			AssertEquals(true, Helper.IsAllowedToAddNewShipment(out message, consol1));
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB }).RestrictedMessage.IsEmpty);

			AssertEquals(true, Helper.IsAllowedToAddNewConsol(out message, shipmentA));
			AssertEquals(true, Helper.IsAllowedToAttachConsol(shipmentA, standAloneConsol).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2 }).RestrictedMessage.IsEmpty);
		}

		public void TestIsAllowedToAddNewShipment_CheckPastCutOffDate()
		{
			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			AssertEquals(true, Helper.IsAllowedToAddNewShipment(out message, consol1));

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
			AssertEquals(false, Helper.IsAllowedToAddNewShipment(out message, consol1));
			AssertContains("Cannot add a new shipment to the consol CONSOL1 as the consol's Cut Off Date has passed.", message);

			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(+1);
			AssertEquals(true, Helper.IsAllowedToAddNewShipment(out message, consol1));

			var consolNotInDatabase = Factory.New<CommonConsol>();
			consolNotInDatabase.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(true, Helper.IsAllowedToAddNewShipment(out message, consolNotInDatabase));
		}

		public void TestIsAllowedToAddNewShipment_CheckIsDirectConsol()
		{
			consol1.Shipments.RemoveAll();
			consol1.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, Helper.IsAllowedToAddNewShipment(out message, consol1));

			consol1.Shipments.Add(shipmentA);

			AssertEquals(false, Helper.IsAllowedToAddNewShipment(out message, consol1));
			AssertContains("Cannot add a new shipment to the Direct consol CONSOL1 because it already has another shipment attached to it.", message);
		}

		public void TestIsAllowedToAddNewShipment_CheckIsMultiAWBMasterConsol()
		{
			consol1.JK_AgentType = Constants.AgentType.AWBCoload;
			AssertEquals(true, Helper.IsAllowedToAddNewShipment(out message, consol1));

			consol1.JK_AgentType = Constants.AgentType.AWBMaster;

			AssertEquals(false, Helper.IsAllowedToAddNewShipment(out message, consol1));
			AssertContains("Cannot add a new shipment to the Multi AWB Master consol CONSOL1.", message);
		}

		#endregion

		#region IsAllowedToAttach

		public void TestIsAllowedToAttach_CheckPastCutOffDate()
		{
			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			var attachRequest = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertContains("Cannot attach shipment ALONESHIPMENT to the consol CONSOL1 as the consol's Cut Off Date has passed.", attachRequest.Errors);

			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, null).Errors.IsEmpty);
			attachRequest = Helper.IsAllowedToAttachShipment(consol1, null);
			AssertContains("Cannot attach any shipment to the consol CONSOL1 as the consol's Cut Off Date has passed.", attachRequest.Errors);

			AssertEquals(false, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
			attachRequest = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertContains("Cannot attach the consol CONSOL1 to the shipment ALONESHIPMENT as the consol Cut Off Date has passed.", attachRequest.Errors);
		}

		public void TestIsAllowedToAttachShipmentConsignorConsigneeAgentsMatchSendingRecievingAgents()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			shipmentA.ConsigneePK = consignee.PK;
			shipmentA.ConsignorPK = consignor.PK;

			shipmentA.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery);
			var message = Helper.IsAllowedToAttachShipment(consol1, shipmentA);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, shipmentA).Warnings.IsEmpty);
			AssertContains("The Consignee/Consignor Related Sending Agent on shipment SHIPMENTA does not match the Sending Forwarder on consol CONSOL1.", message.Warnings);

			consol1.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			shipmentA.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipmentA.Consignee.SetRelatedParty(org3, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery);
			message = Helper.IsAllowedToAttachShipment(consol1, shipmentA);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, shipmentA).Warnings.IsEmpty);
			AssertContains("The Consignee/Consignor Related Sending Agent on shipment SHIPMENTA does not match the Sending Forwarder on consol CONSOL1.", message.Warnings);

			shipmentA.Consignee.SetRelatedParty(org, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipmentA.Consignor.SetRelatedParty(org2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup);

			message = Helper.IsAllowedToAttachShipment(consol1, shipmentA);
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, shipmentA).Warnings.IsEmpty);
			AssertNotContains("The Consignee/Consignor Related Receiving Agent on shipment SHIPMENTA does not match the Receiving Forwarder on consol CONSOL1.", message.Warnings);
		}

		public void TestIsAllowedToAttachConsolConsignorConsigneeAgentsMatchSendingRecievingAgents()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			shipmentA.ConsigneePK = consignee.PK;
			shipmentA.ConsignorPK = consignor.PK;

			shipmentA.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			var message = Helper.IsAllowedToAttachConsol(shipmentA, consol1);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(shipmentA, consol1).Warnings.IsEmpty);
			AssertContains("The Consignee/Consignor Related Receiving Agent on shipment SHIPMENTA does not match the Receiving Forwarder on consol CONSOL1.", message.Warnings);

			consol1.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			shipmentA.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipmentA.Consignee.SetRelatedParty(org3, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery);
			message = Helper.IsAllowedToAttachConsol(shipmentA, consol1);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(shipmentA, consol1).Warnings.IsEmpty);
			AssertContains("The Consignee/Consignor Related Sending Agent on shipment SHIPMENTA does not match the Sending Forwarder on consol CONSOL1.", message.Warnings);

			shipmentA.Consignee.SetRelatedParty(org, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery);
			shipmentA.Consignor.SetRelatedParty(org2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup);

			message = Helper.IsAllowedToAttachConsol(shipmentA, consol1);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(shipmentA, consol1).Warnings.IsEmpty);
			AssertNotContains("The Consignee/Consignor Related Receiving Agent on shipment SHIPMENTA does not match the Receiving Forwarder on consol CONSOL1.", message.Warnings);
		}

		public void TestIsAllowedToAttach_CheckInactiveConsol()
		{
			consol1.IsCancelled = true;

			var attachRequest = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertContains("Cannot attach the shipment ALONESHIPMENT to the consol CONSOL1 because the consol is inactive.", attachRequest.Errors);

			attachRequest = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
			AssertContains("Cannot attach the consol CONSOL1 to the shipment ALONESHIPMENT because the consol is inactive.", attachRequest.Errors);
		}

		public void TestIsAllowedToAttach_CheckIsDirectShipment()
		{
			var directConsol = standAloneShipment.Consols.AddNew();
			directConsol.JK_UniqueConsignRef = "DIRECTCONSOL";
			directConsol.JK_AgentType = Constants.AgentType.Direct;

			AssertEquals(true, standAloneShipment.IsDirectShipment);
			AssertEquals(false, consol1.IsDirect);

			var attachRequest = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertContains("The shipment ALONESHIPMENT is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).", attachRequest.Errors);

			attachRequest = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
			AssertContains("The shipment ALONESHIPMENT is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).", attachRequest.Errors);
		}

		public void TestIsAllowedToAttach_CheckIsDirectShipment_AgentConsol()
		{
			var directConsol = standAloneShipment.Consols.AddNew();
			directConsol.JK_UniqueConsignRef = "DIRECTCONSOL";
			directConsol.JK_AgentType = Constants.AgentType.Direct;
			directConsol.JK_RL_NKLoadPort = "USLAX";
			directConsol.JK_RL_NKDischargePort = "AUSYD";
			directConsol.Transports.DepartureTransport.JW_ETD = ZDateTime.Today;
			directConsol.Transports.ArrivalTransport.JW_ETA = ZDateTime.Today.AddDays(2);

			AssertEquals(true, standAloneShipment.IsDirectShipment);

			var agentConsol1 = FreightTestHelper.GetConsol<CommonConsol>("CON01", Constants.TransportModes.Road, Constants.AgentType.Agent, "USCHI", "USLAX", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2), Factory);
			var agentConsol2 = FreightTestHelper.GetConsol<CommonConsol>("CON02", Constants.TransportModes.Sea, Constants.AgentType.Agent, "AUSYD", "AUMEL", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(6), Factory);
			var agentConsol3 = FreightTestHelper.GetConsol<CommonConsol>("CON03", Constants.TransportModes.Sea, Constants.AgentType.Agent, "AUSYD", "NZAKL", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(6), Factory);
			var agentConsol4 = FreightTestHelper.GetConsol<CommonConsol>("CON04", Constants.TransportModes.Road, Constants.AgentType.Agent, "USCHI", "USLAX", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(1), Factory);
			var agentConsol5 = FreightTestHelper.GetConsol<CommonConsol>("CON05", Constants.TransportModes.Sea, Constants.AgentType.Agent, "AUSYD", "AUMEL", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(6), Factory);
			var agentConsol6 = FreightTestHelper.GetConsol<CommonConsol>("CON06", Constants.TransportModes.Road, Constants.AgentType.Agent, "USCHI", "USNYC", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2), Factory);
			var agentConsol7 = FreightTestHelper.GetConsol<CommonConsol>("CON07", Constants.TransportModes.Sea, Constants.AgentType.Agent, "AUBNE", "AUMEL", ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(6), Factory);

			AssertAttachDirectShipmentToAgentConsol_ShouldAllowWithWarning(agentConsol1, standAloneShipment);
			AssertAttachDirectShipmentToAgentConsol_ShouldAllowWithWarning(agentConsol2, standAloneShipment);
			AssertAttachDirectShipmentToAgentConsol_ShouldNotAllowWithError("AGT SEA consol is not domestic", agentConsol3, standAloneShipment);
			AssertAttachDirectShipmentToAgentConsol_ShouldNotAllowWithError("AGT consol arrival date is after departure DRT consol", agentConsol4, standAloneShipment);
			AssertAttachDirectShipmentToAgentConsol_ShouldNotAllowWithError("AGT consol departure date is before arrival DRT consol", agentConsol5, standAloneShipment);
			AssertAttachDirectShipmentToAgentConsol_ShouldNotAllowWithError("AGT consol discharge port does not match departure DRT consol's load port", agentConsol6, standAloneShipment);
			AssertAttachDirectShipmentToAgentConsol_ShouldNotAllowWithError("AGT consol load port does not match arrival DRT consol's discharge port", agentConsol7, standAloneShipment);
		}

		void AssertAttachDirectShipmentToAgentConsol_ShouldAllowWithWarning(CommonConsol consol, CommonShipment shipment)
		{
			var attachShipmentRequest = Helper.IsAllowedToAttachShipment(consol, shipment);
			AssertEquals(true, attachShipmentRequest.Errors.IsEmpty);
			AssertEquals(false, attachShipmentRequest.Warnings.IsEmpty);
			AssertContains($"The shipment {shipment.JS_UniqueConsignRef} is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s))", attachShipmentRequest.Warnings);

			var attachConsolRequest = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertEquals(true, attachConsolRequest.Errors.IsEmpty);
			AssertEquals(false, attachConsolRequest.Warnings.IsEmpty);
			AssertContains($"The shipment {shipment.JS_UniqueConsignRef} is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s))", attachConsolRequest.Warnings);
		}

		void AssertAttachDirectShipmentToAgentConsol_ShouldNotAllowWithError(string message, CommonConsol consol, CommonShipment shipment)
		{
			var attachShipmentRequest = Helper.IsAllowedToAttachShipment(consol, shipment);
			AssertEquals(message, false, attachShipmentRequest.Errors.IsEmpty);
			AssertContains($"The shipment {shipment.JS_UniqueConsignRef} is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s))", attachShipmentRequest.Errors);

			var attachConsolRequest = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertEquals(message, false, attachConsolRequest.Errors.IsEmpty);
			AssertContains($"The shipment {shipment.JS_UniqueConsignRef} is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s))", attachConsolRequest.Errors);
		}

		public void TestIsAlloedToAttach_CheckIsDirectConsol_AttachTwoSTDShipments_CauseNullCurrentMaster()
		{
			consol1.Shipments.RemoveAll();
			consol1.JK_AgentType = Constants.AgentType.Direct;

			consol1.Shipments.Add(shipmentA);
			consol1.Shipments.Add(shipmentB);

			AssertNoExceptionThrown("Expect no NullReference in checking IsAllowedToAttachShipment", () => { var error = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors; });
		}

		public void TestIsAllowedToAttach_CheckIsDirectConsol()
		{
			consol1.Shipments.RemoveAll();
			consol1.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			consol1.Shipments.Add(shipmentA);

			var attachRequest = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertContains("Cannot attach the shipment ALONESHIPMENT to the Direct consol CONSOL1 because it already has another shipment attached to it.", attachRequest.Errors);

			attachRequest = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertContains("Cannot attach the Direct consol CONSOL1 to the shipment ALONESHIPMENT because the consol already has another shipment attached.", attachRequest.Errors);
		}

		public void TestIsAllowedToAttach_CheckIsMultiAWBMasterConsol()
		{
			consol1.JK_AgentType = Constants.AgentType.AWBCoload;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			consol1.JK_AgentType = Constants.AgentType.AWBMaster;

			var attachRequest = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertContains("Cannot attach the shipment ALONESHIPMENT to the Multi AWB Master consol CONSOL1.", attachRequest.Errors);

			attachRequest = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertContains("Cannot attach the Multi AWB Master consol CONSOL1 to the shipment ALONESHIPMENT.", attachRequest.Errors);
		}

		public void TestIsAllowedToAttach_DirectConsol_CheckCanHaveShipment()
		{
			consol1.Shipments.RemoveAll();
			consol1.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			standAloneShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(false, standAloneShipment.CanBeDirect());

			var message = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertContains("Cannot attach the shipment ALONESHIPMENT to the Direct consol CONSOL1, because the shipment is not a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no master shipment.", message.Errors);

			message = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
			AssertContains("Cannot attach the Direct consol CONSOL1 to the shipment ALONESHIPMENT because the shipment is not a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no master shipment.", message.Errors);

			standAloneShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, standAloneShipment.CanBeDirect());

			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			//NOT ALLOWED: Shipment can be direct, but has only non-direct consol(s) attached
			standAloneShipment.Consols.Add(standAloneConsol);

			message = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertContains("Cannot attach the shipment ALONESHIPMENT to the Direct consol CONSOL1, because the shipment is already attached to at least one other Non-Direct Consol.", message.Errors);

			message = Helper.IsAllowedToAttachConsol(standAloneShipment, consol1);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
			AssertContains("Cannot attach the Direct consol CONSOL1 to the shipment ALONESHIPMENT because the shipment is already attached to at least one other Non-Direct Consol.", message.Errors);

			//ALLOWED: Shipment can be direct and has direct consol attached
			standAloneConsol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			//ALLOWED: Shipment can be direct and has at least one direct consol attached
			standAloneShipment.Consols.AddNew();
			standAloneShipment.Consols.AddNew();
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
		}

		public void TestIsAllowedToAttach_DirectConsol_CheckCanHaveHVLShipment()
		{
			var directConsol = Factory.New<CommonConsol>();
			directConsol.JK_AgentType = Constants.AgentType.Direct;

			standAloneShipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			AssertEquals(true, standAloneShipment.CanBeDirect());

			AssertEquals(true, Helper.IsAllowedToAttachShipment(directConsol, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, directConsol).Errors.IsEmpty);
		}

		public void TestIsAllowedToAttach_DirectConsol_CheckCanHaveASMShipmentAndSubShipments()
		{
			var directConsol = Factory.New<CommonConsol>();
			directConsol.JK_AgentType = Constants.AgentType.Direct;

			standAloneShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(true, standAloneShipment.CanBeDirect());

			AssertEquals(true, Helper.IsAllowedToAttachShipment(directConsol, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, directConsol).Errors.IsEmpty);

			var subShipment = Factory.New<CommonShipment>();
			subShipment.Consols.Add(directConsol);
			AssertEquals(true, subShipment.CanBeDirect());

			var attachRequest = Helper.IsAllowedToAttachShipment(directConsol, standAloneShipment);
			AssertEquals(false, Helper.IsAllowedToAttachShipment(directConsol, standAloneShipment).Errors.IsEmpty);
			AssertContains(FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster, attachRequest.Warnings);

			attachRequest = Helper.IsAllowedToAttachConsol(standAloneShipment, directConsol);
			AssertEquals(false, Helper.IsAllowedToAttachConsol(standAloneShipment, directConsol).Errors.IsEmpty);
			AssertContains(FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster, attachRequest.Warnings);

			standAloneShipment.CoLoadShipments.Add(subShipment);
			AssertEquals(true, subShipment.CanBeDirect());

			AssertEquals(true, Helper.IsAllowedToAttachShipment(directConsol, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, directConsol).Errors.IsEmpty);
			standAloneShipment.CoLoadShipments.Remove(subShipment);
		}

		public void TestIsAllowedToAttach_CheckIsAllowedToAttachShipmentWithApprovedForCargoOnlyInspectionType()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USNYC";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "MYKUL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYKUL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var transport1 = consol1.Transports[0];
			transport1.JW_IsLinked = true;
			transport1.JW_TransportType = "FL1";
			transport1.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("USNYC", "MYKUL").PK;
			transport1.JW_IsCargoOnly = false;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_TransportType = "FL2";
			transport2.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("MYKUL", "SGSIN").PK;
			transport2.JW_IsCargoOnly = false;

			AssertEquals("Precondition", true, consol1.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

			standAloneShipment.JS_InspectionTypeCode = "XXX";
			AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				consol1.JK_RL_NKLoadPort = "USNYC";

				AssertEquals(true, FreightUtilities.SupplyChainSecurityConfiguration.IsEnabled);

				standAloneShipment.JS_InspectionTypeCode = "APP";
				AssertEquals(true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
				AssertEquals(true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

				standAloneShipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK is allowed to attach shipment", true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
				AssertEquals("UNK is allowed to attach consol", true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);

				transport1.JW_IsCargoOnly = true;
				transport2.JW_IsCargoOnly = false;
				AssertEquals("Is allowed to attach as Cargo Only for sailing departing US", true, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
				AssertEquals("Is allowed to attach as Cargo Only for sailing departing US", true, Helper.IsAllowedToAttachConsol(standAloneShipment, consol1).Errors.IsEmpty);
			}
		}

		public void TestIsAllowedToAttach_CheckOrganisationsAreApprovedForShippingOnPassengerFlights()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "HKHKG";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYKUL";

				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_UniqueConsignRef = "CONSOL1";
				consol.JK_RL_NKLoadPort = "HKHKG";

				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
				shipment.JS_UniqueConsignRef = "SHIPMENT1";

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("HKHKG", "MYKUL").PK;
				transport.JW_IsCargoOnly = false;

				shipment.JS_InspectionTypeCode = "TRN";

				AssertEquals(true, FreightUtilities.SupplyChainSecurityConfiguration.IsEnabled);
				AssertEquals(true, Helper.IsAllowedToAttachShipment(consol, shipment).Errors.IsEmpty);

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_InspectionTypeCode = "APP";

				var message = Helper.IsAllowedToAttachConsol(shipment, consol);
				AssertEquals(false, Helper.IsAllowedToAttachConsol(shipment, consol).Errors.IsEmpty);
				AssertContains("Cannot attach the consol CONSOL1 to the shipment SHIPMENT1 because the shipment or one of its sub-shipments has been received from an Account Consignor so can only be sent on 'Is Cargo Only' aircraft even though tendered as known cargo.", message.Errors);

				message = Helper.IsAllowedToAttachShipment(consol, shipment);
				AssertEquals(false, Helper.IsAllowedToAttachShipment(consol, shipment).Errors.IsEmpty);
				AssertContains("The Shipment has been received from an Account Consignor so can only be sent on 'Is Cargo Only' aircraft even though tendered as known cargo.", message.Errors);

				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;

				AssertEquals(true, Helper.IsAllowedToAttachShipment(consol, shipment).Errors.IsEmpty);
				AssertEquals(true, Helper.IsAllowedToAttachConsol(shipment, consol).Errors.IsEmpty);
			}
		}

		public void TestIsAllowedToAttach_CheckConsolTypeAccessRights()
		{
			var coLoadConsol = Factory.NewWithValidTestData<CommonConsol>();
			coLoadConsol.JK_AgentType = Constants.AgentType.CoLoad;
			coLoadConsol.JK_UniqueConsignRef = "C0000000";

			var agentConsol = Factory.NewWithValidTestData<CommonConsol>();
			agentConsol.JK_AgentType = Constants.AgentType.Agent;
			agentConsol.JK_UniqueConsignRef = "C0000001";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			Env.Security.MaintainConsolTypeCoLoad.IsAllowed = true;
			Env.Security.MaintainConsolTypeAgent.IsAllowed = false;

			var expectedError = @"Cannot Attach
Consol C0000001
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Consol Type Access -> Agent";

			Assert(Helper.IsAllowedToAttachConsol(shipment, coLoadConsol).Errors.IsEmpty);
			AssertEquals(expectedError, Helper.IsAllowedToAttachConsol(shipment, agentConsol).Errors.ToString());
		}

		public void TestIsAllowedToAttach_CheckPorts_Allowed_WhenBothDischargePortAndLoadPortsAreDifferent()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Sea;

			shipment.Consols.Add(anotherConsol);
			AssertNoErrors(consol);
			AssertNoErrors(anotherConsol);
			shipment.Consols.Remove(anotherConsol);
		}

		public void TestIsAllowedToAttach_CheckPorts_IsNotAllowed_WhenDischargeOrLoadPortsAreTheSameAndBothTransportModesAreNotRailOrRoad()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);
			AssertContains("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);
			AssertContains("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			shipment.Consols.Add(anotherConsol);
			AssertNoErrors(consol);
			AssertNoErrors(anotherConsol);
			shipment.Consols.Remove(anotherConsol);

			var factoryAnother = new BusinessObjectFactory();
			var consolCopy = factoryAnother.Load<CommonConsol>(consol.PK);
			consolCopy.JK_RL_NKLoadPort = "AUMEL";
			factoryAnother.Save();

			anotherConsol.JK_RL_NKLoadPort = "AUMEL";
			anotherConsol.JK_RL_NKDischargePort = "DEFRA";
			anotherConsol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.ToString());
			AssertEquals("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors.ToString());

			shipment.Consols.Add(anotherConsol);
			AssertEquals(true, consol.HasRowErrors);
			AssertEquals(true, anotherConsol.HasRowErrors);
		}

		public void TestIsAllowedToAttach_CheckPorts_IsNotAllowed_WhenOneOfDischargeOrLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsNotDomestic()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FTL;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.IsDomesticFreight = false;

			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);
			AssertContains("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);
			AssertContains("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			shipment.Consols.Add(anotherConsol);
			AssertNoErrors(consol);
			AssertNoErrors(anotherConsol);
			shipment.Consols.Remove(anotherConsol);

			var factoryAnother = new BusinessObjectFactory();
			var consolCopy = factoryAnother.Load<CommonConsol>(consol.PK);
			consolCopy.JK_RL_NKLoadPort = "AUMEL";
			factoryAnother.Save();

			anotherConsol.JK_RL_NKLoadPort = "AUMEL";
			anotherConsol.JK_RL_NKDischargePort = "DEFRA";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.IsDomesticFreight = false;

			AssertEquals("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.ToString());
			AssertEquals("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors.ToString());

			shipment.Consols.Add(anotherConsol);
			AssertEquals(true, consol.HasRowErrors);
			AssertEquals(true, anotherConsol.HasRowErrors);
		}

		public void TestIsAllowedToAttach_CheckPorts_Allowed_WhenOnlyOneOfDischargeOrLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsDomestic()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUMEL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals("This shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Warnings.ToString());
			AssertEquals("This shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Warnings.ToString());
			shipment.Consols.Add(anotherConsol);
			AssertNoErrors(consol);
			AssertNoErrors(anotherConsol);
			shipment.Consols.Remove(anotherConsol);

			anotherConsol.JK_RL_NKLoadPort = "CNNGB";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertEquals("This shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Warnings.ToString());
			AssertEquals("This shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Warnings.ToString());
			shipment.Consols.Add(anotherConsol);
			AssertNoErrors(consol);
			AssertNoErrors(anotherConsol);
			shipment.Consols.Remove(anotherConsol);
		}

		public void TestIsAllowedToAttach_CheckPorts_IsNotAllowed_WhenBothOfDischargeAndLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsDomesticAndTheyHaveOverlapETDETA()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now;
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(2);
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUSYD";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(3);
			anotherConsol.IsDomesticFreight = true;

			AssertEquals("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same Load and Discharge port with ETD/ATD or ETA/ATA date overlap",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.ToString());
			AssertEquals("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same Load and Discharge port with ETD/ATD or ETA/ATA date overlap",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors.ToString());
		}

		public void TestIsAllowedToAttach_CheckPorts_IsNotAllowed_WhenBothOfDischargeAndLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsDomesticAndTheyHaveOverlapATDATA()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
			consol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now;
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUSYD";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same Load and Discharge port with ETD/ATD or ETA/ATA date overlap",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.ToString());
			AssertEquals("Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same Load and Discharge port with ETD/ATD or ETA/ATA date overlap",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors.ToString());
		}

		public void TestIsAllowedToAttach_CheckPorts_Allowed_WhenBothOfDischargeAndLoadPortsAreTheSameAndTransportModeIsRailOrRoadAndIsDomesticAndTheyDontHaveOverlap()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_UniqueConsignRef = "C0000000";
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-4);
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(-3);
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-4);
			consol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now.AddDays(-2);
			consol.IsDomesticFreight = false;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "AUSYD";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Rail;
			anotherConsol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			anotherConsol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
			anotherConsol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			anotherConsol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
			anotherConsol.IsDomesticFreight = true;

			AssertEquals("This shipment is already attached to a consol with same load port, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Warnings.ToString());
			AssertEquals("This shipment is already attached to a consol with same load port, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Warnings.ToString());
			shipment.Consols.Add(anotherConsol);
			AssertNoErrors(consol);
			AssertNoErrors(anotherConsol);
			shipment.Consols.Remove(anotherConsol);
		}

		public void TestIsAllowedToAttachShipment_CheckShipmentHasAnotherConsolWithSameLoadOrDischarge_WhenShipmentTypeIsBuyersConsolLead()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);
		}

		public void TestIsAllowedToAttachConsol_CheckShipmentHasAnotherConsolWithSameLoadOrDischarge_WhenShipmentTypeIsBuyersConsolLead()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var sameLoadPortMessage = "Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same load port";
			var sameDischargePortMessage = "Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same discharge port";
			var sameLoadAndDischargePortsMessage = "Cannot attach consol C0000001 to shipment S000001, this shipment is already attached to a consol with same load port, this shipment is already attached to a consol with same discharge port";

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";
			AssertContains(sameLoadPortMessage, Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertContains(sameLoadPortMessage, Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains(sameDischargePortMessage, Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertContains(sameDischargePortMessage, Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains(sameLoadAndDischargePortsMessage, Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertContains(sameLoadAndDischargePortsMessage, Helper.IsAllowedToAttachConsol(shipment, anotherConsol).Errors);
		}

		public void TestIsAllowedToAttachShipment_CheckShipmentHasAnotherConsolWithSameLoadOrDischarge_WhenAddingConsolFromBuyersConsolLeadShipmentToSubShipment()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C0000000";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.Consols.Add(consol);

			Factory.Save();

			var anotherConsol = Factory.NewWithValidTestData<CommonConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "NZAKL";
			anotherConsol.JK_UniqueConsignRef = "C0000001";

			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			masterShipment.JS_UniqueConsignRef = "S000002";
			masterShipment.Consols.Add(anotherConsol);

			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			anotherConsol.JK_RL_NKLoadPort = "SGSIN";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_RL_NKDischargePort = "CNSHA";
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);

			var masterShipment2 = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			masterShipment2.JS_UniqueConsignRef = "S000003";
			masterShipment2.Consols.Add(anotherConsol);
			AssertContains("Cannot attach shipment S000001 to consol C0000001, this shipment is already attached to a consol with same load port, this shipment is already attached to a consol with same discharge port",
				Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors);

			masterShipment2.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Helper.IsAllowedToAttachShipment(anotherConsol, shipment).Errors.IsEmpty);
		}

		public void TestIsAllowedToAttach_CheckShipmentPacklinesMatchConsolTemperatureControl_RequiresTempControl()
		{
			var nonTempControlConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			nonTempControlConsol.JK_UniqueConsignRef = "CONSOL1";
			nonTempControlConsol.JK_RequiresTemperatureControl = false;

			var tempControlConsol = (CommonConsol)Factory.New<IForwardingConsol>();
			tempControlConsol.JK_UniqueConsignRef = "CONSOL1";
			tempControlConsol.JK_RequiresTemperatureControl = true;

			var nonTempControlShipment = (CommonShipment)Factory.New<IForwardingShipment>();
			nonTempControlShipment.JS_UniqueConsignRef = "SHIPMENTA";
			nonTempControlShipment.OuterPackLines.AddNew().JL_RequiresTemperatureControl = false;

			var tempControlShipment = (CommonShipment)Factory.New<IForwardingShipment>();
			tempControlShipment.JS_UniqueConsignRef = "SHIPMENTB";
			tempControlShipment.OuterPackLines.AddNew().JL_RequiresTemperatureControl = true;

			CombineAssertions("Adding a shipment to a consol (and vice versa) with both not being temp controlled should succeed", () =>
			{
				Assert(Helper.IsAllowedToAttachShipment(nonTempControlConsol, nonTempControlShipment).Errors.IsEmpty);
				Assert(Helper.IsAllowedToAttachConsol(nonTempControlShipment, nonTempControlConsol).Errors.IsEmpty);
			});

			CombineAssertions("Adding a shipment to a consol (and vice versa) with only consol being temp controlled should succeed", () =>
			{
				Assert(Helper.IsAllowedToAttachShipment(tempControlConsol, nonTempControlShipment).Errors.IsEmpty);
				Assert(Helper.IsAllowedToAttachConsol(nonTempControlShipment, tempControlConsol).Errors.IsEmpty);
			});

			CombineAssertions("Adding a shipment to a consol (and vice versa) with only shipment being temp controlled should fail", () =>
			{
				AssertContains("Cannot attach shipment SHIPMENTB as it has a temperature range not supported by the consol CONSOL1.",
					Helper.IsAllowedToAttachShipment(nonTempControlConsol, tempControlShipment).Errors);
				AssertContains("Cannot attach shipment SHIPMENTB as it has a temperature range not supported by the consol CONSOL1.",
					Helper.IsAllowedToAttachConsol(tempControlShipment, nonTempControlConsol).Errors);
			});
		}

		public void TestIsAllowedToAttach_CheckShipmentPacklinesMatchConsolTemperatureControl_TempControlRange()
		{
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL1";
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureMinimum = 5;
			consol.JK_RequiredTemperatureMaximum = 15;
			consol.JK_RequiredTemperatureUnit = Constants.Temperature.Centigrade;

			var matchingShipment = (CommonShipment)Factory.New<IForwardingShipment>();
			matchingShipment.JS_UniqueConsignRef = "SHIPMENTA";
			var matchingPackline = matchingShipment.OuterPackLines.AddNew();
			matchingPackline.JL_RequiresTemperatureControl = true;
			matchingPackline.JL_RequiredTemperatureMinimum = 4;
			matchingPackline.JL_RequiredTemperatureMaximum = 16;
			matchingPackline.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;

			var nonMatchingShipment = (CommonShipment)Factory.New<IForwardingShipment>();
			nonMatchingShipment.JS_UniqueConsignRef = "SHIPMENTB";
			var nonMatchingPackline = nonMatchingShipment.OuterPackLines.AddNew();
			nonMatchingPackline.JL_RequiresTemperatureControl = true;
			nonMatchingPackline.JL_RequiredTemperatureMinimum = 6;
			nonMatchingPackline.JL_RequiredTemperatureMaximum = 15;
			nonMatchingPackline.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;

			var nonMatchingShipment2 = (CommonShipment)Factory.New<IForwardingShipment>();
			nonMatchingShipment2.JS_UniqueConsignRef = "SHIPMENTC";
			var nonMatchingPackline2 = nonMatchingShipment2.OuterPackLines.AddNew();
			nonMatchingPackline2.JL_RequiresTemperatureControl = true;
			nonMatchingPackline2.JL_RequiredTemperatureMinimum = 5;
			nonMatchingPackline2.JL_RequiredTemperatureMaximum = 14;
			nonMatchingPackline2.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;

			CombineAssertions("Adding a shipment to a consol (and vice versa) with matching temp ranges should succeed", () =>
			{
				Assert("Attaching should result in no errors", Helper.IsAllowedToAttachShipment(consol, matchingShipment).Errors.IsEmpty);
				Assert("Attaching should result in no errors", Helper.IsAllowedToAttachConsol(matchingShipment, consol).Errors.IsEmpty);
			});

			CombineAssertions("Adding a shipment to a consol (and vice versa) with non matching temp ranges should fail", () =>
			{
				AssertContains("Cannot attach shipment SHIPMENTB as it has a temperature range not supported by the consol CONSOL1.",
					Helper.IsAllowedToAttachShipment(consol, nonMatchingShipment).Errors);
				AssertContains("Cannot attach shipment SHIPMENTB as it has a temperature range not supported by the consol CONSOL1.",
					Helper.IsAllowedToAttachConsol(nonMatchingShipment, consol).Errors);
				AssertContains("Cannot attach shipment SHIPMENTC as it has a temperature range not supported by the consol CONSOL1.",
					Helper.IsAllowedToAttachShipment(consol, nonMatchingShipment2).Errors);
				AssertContains("Cannot attach shipment SHIPMENTC as it has a temperature range not supported by the consol CONSOL1.",
					Helper.IsAllowedToAttachConsol(nonMatchingShipment2, consol).Errors);
			});
		}

		public void TestIsllowedToAttachShipment_CheckConsolAllowsAttachingOfShipmentsDangerousGoods()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_IsHazardous = false;

				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
				var packline = shipment.OuterPackLines.AddNew();
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_IMOClass = "CLS1";

				Factory.Save();

				AssertContains(ZString.Format("Cannot attach shipment {0} as it contains dangerous cargo that is not accepted by the consol {1}", shipment.JS_UniqueConsignRef, consol.JK_UniqueConsignRef), Helper.IsAllowedToAttachConsol(shipment, consol).Errors);

				consol.JK_IsHazardous = true;
				Factory.Save();

				AssertNotContains(ZString.Format("Cannot attach shipment {0} as it contains dangerous cargo that is not accepted by the consol {1}", shipment.JS_UniqueConsignRef, consol.JK_UniqueConsignRef), Helper.IsAllowedToAttachConsol(shipment, consol).Errors);
			}
		}

		public void TestIsllowedToAttachShipment_CheckConsolAllowsAttachingOfShipmentsDangerousGoods_RegistryIsDisabled()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = (CommonConsol)Factory.New<IForwardingConsol>();
				consol.JK_IsHazardous = false;

				var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
				var packline = shipment.OuterPackLines.AddNew();
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_IMOClass = "CLS1";

				Factory.Save();

				AssertNotContains("Cannot attach shipment S00001000 as it contains dangerous cargo that is not accepted by the consol C00001000", Helper.IsAllowedToAttachConsol(shipment, consol).Errors);

				consol.JK_IsHazardous = true;
				Factory.Save();

				AssertNotContains("Cannot attach shipment S00001000 as it contains dangerous cargo that is not accepted by the consol C00001000", Helper.IsAllowedToAttachConsol(shipment, consol).Errors);
			}
		}

		public void TestIsAllowedToAttach_GatewayServiceLevel_CheckUserRightsToAttachShipmentWithDifferentValue()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var agentPorts = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			agentPorts.O5_PortOrCountry = "AUSYD";
			agentPorts.O5_AgentDirection = "BTH";
			agentPorts.O5_SeaAgentStatus = "GTA";
			agentPorts.O5_AirAgentStatus = "GTA";
			agentPorts.O5_RailAgentStatus = "GTA";
			agentPorts.O5_RoadAgentStatus = "GTA";

			var gatewayService = agentPorts.ExclusiveGatewayServices.AddNew();
			gatewayService.O7_RS_NKGatewayService = "DIR";
			gatewayService.O7_RS_NKShipmentServiceLevel = "STD";

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", "STD", Factory);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			shipment.RequestPermissionByImpersonation = null;

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONSOL1", "SEA", "AGT", "USLAX", "AUSYD", ZDateTime.Today, ZDateTime.Today.AddDays(5), Factory);
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			consol.RequestPermissionByImpersonation = null;

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;

			Assert(Helper.IsAllowedToAttachShipment(parentConsol: consol, shipment: shipment).Errors.IsEmpty);
			Assert(Helper.IsAllowedToAttachConsol(parentShipment: shipment, consol: consol).Errors.IsEmpty);

			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			var expectedErrorMessage =
				"You don't have permission to attach a shipment with Gateway Service Level 'DEF' to a consol with Gateway Service Level 'DIR'." +
				$" Please contact the administrator to either set up this relation for the organization '{receivingAgent.OH_Code}' or give you permission for this operation.";

			AssertEquals(expectedErrorMessage, Helper.IsAllowedToAttachShipment(parentConsol: consol, shipment: shipment).Errors);
			AssertEquals(expectedErrorMessage, Helper.IsAllowedToAttachConsol(parentShipment: shipment, consol: consol).Errors);
		}

		public void TestIsAllowedToAttach_GatewayServiceLevel_BlankValueNotAllowedInShipmentWhenItsSpecifiedInConsol()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var agentPorts = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			agentPorts.O5_PortOrCountry = "AUSYD";
			agentPorts.O5_AgentDirection = "BTH";
			agentPorts.O5_SeaAgentStatus = "GTA";
			agentPorts.O5_AirAgentStatus = "GTA";
			agentPorts.O5_RailAgentStatus = "GTA";
			agentPorts.O5_RoadAgentStatus = "GTA";

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", "STD", Factory);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			shipment.RequestPermissionByImpersonation = null;

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONSOL1", "SEA", "AGT", "USLAX", "AUSYD", ZDateTime.Today, ZDateTime.Today.AddDays(5), Factory);
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			consol.RequestPermissionByImpersonation = null;

			Assert(Helper.IsAllowedToAttachShipment(parentConsol: consol, shipment: shipment).Errors.IsEmpty);
			Assert(Helper.IsAllowedToAttachConsol(parentShipment: shipment, consol: consol).Errors.IsEmpty);

			shipment.JS_RS_NKGatewayServiceLevel = "";
			var expectedErrorMessage =
				$"Gateway Service Level for {shipment.HumanReadableName} cannot be blank. Because it's attached to {consol.HumanReadableName} with Gateway Service Level 'DIR'.";

			AssertEquals(expectedErrorMessage, Helper.IsAllowedToAttachShipment(parentConsol: consol, shipment: shipment).Errors);
			AssertEquals(expectedErrorMessage, Helper.IsAllowedToAttachConsol(parentShipment: shipment, consol: consol).Errors);
		}

		#endregion

		#region IsAllowedToDetach

		public void TestIsAllowedToDetach_CheckHasConsolApportionmentOnShipmentsAttachedToTwoDifferentConsol()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_UniqueConsignRef = "C1";
			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_UniqueConsignRef = "C2";

			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			var shipment2 = Factory.NewWithValidTestData<CommonShipment>();
			shipment2.JS_UniqueConsignRef = "S2";

			consol1.Shipments.AddRange(shipment1, shipment2);
			consol2.Shipments.AddRange(shipment1, shipment2);

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol1.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.E6_OSCostAmount] = 500m;

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipment1.PK;
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_JobNum = "Job2";
			jobHeader2.JH_ParentTableCode = "JS";
			jobHeader2.JH_ParentID = shipment2.PK;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_JH = jobHeader1.PK;
			charge1.JR_E6 = consolCost.PK;
			charge1.JR_OSCostAmt = 100m;
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_JH = jobHeader2.PK;
			charge2.JR_E6 = consolCost.PK;
			charge2.JR_OSCostAmt = 150m;

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipment1, shipment2 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol2, new[] { shipment1, shipment2 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);
		}

		public void TestIsAllowedToDetach_CheckHasConsolApportionmentOnUnsavedShipment()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "C1";

			var savedShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			savedShipment1.JS_UniqueConsignRef = "S1";
			var savedShipment2 = Factory.NewWithValidTestData<CommonShipment>();
			savedShipment2.JS_UniqueConsignRef = "S2";
			var savedShipment3 = Factory.NewWithValidTestData<CommonShipment>();
			savedShipment3.JS_UniqueConsignRef = "S3";
			consol.Shipments.AddRange(savedShipment1, savedShipment2, savedShipment3);

			var savedConsolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			savedConsolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				savedConsolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				savedConsolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				savedConsolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			savedConsolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			savedConsolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			savedConsolCost[JobConsolCostSchema.E6_OSCostAmount] = 500m;
			savedConsolCost[JobConsolCostSchema.E6_LocalCostAmount] = 500m;

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = savedShipment1.PK;
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_JobNum = "Job2";
			jobHeader2.JH_ParentTableCode = "JS";
			jobHeader2.JH_ParentID = savedShipment2.PK;
			var jobHeader3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader3.JH_JobNum = "Job3";
			jobHeader3.JH_ParentTableCode = "JS";
			jobHeader3.JH_ParentID = savedShipment3.PK;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_JH = jobHeader1.PK;
			charge1.JR_E6 = savedConsolCost.PK;
			charge1.JR_LocalCostAmt = charge1.JR_OSCostAmt = 100m;
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_JH = jobHeader2.PK;
			charge2.JR_E6 = savedConsolCost.PK;
			charge2.JR_LocalCostAmt = charge2.JR_OSCostAmt = 150m;
			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_AC = Env.Registry.FreightChargeCode;
			charge3.JR_JH = jobHeader3.PK;
			charge3.JR_E6 = savedConsolCost.PK;
			charge3.JR_LocalCostAmt = charge3.JR_OSCostAmt = 250m;
			Factory.Save();

			var unsavedShipment1 = Factory.NewWithValidTestData<CommonShipment>();
			unsavedShipment1.JS_UniqueConsignRef = "";
			var unsavedShipment2 = Factory.NewWithValidTestData<CommonShipment>();
			unsavedShipment2.JS_UniqueConsignRef = "";
			var unsavedShipment3 = Factory.NewWithValidTestData<CommonShipment>();
			unsavedShipment3.JS_UniqueConsignRef = "";
			consol.Shipments.AddRange(unsavedShipment1, unsavedShipment2, unsavedShipment3);

			var unSavedConsolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			unSavedConsolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				unSavedConsolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				unSavedConsolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				unSavedConsolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			unSavedConsolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			unSavedConsolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			savedConsolCost[JobConsolCostSchema.E6_OSCostAmount] = 600m;
			savedConsolCost[JobConsolCostSchema.E6_LocalCostAmount] = 600m;

			var jobHeader4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader4.JH_JobNum = "Job4";
			jobHeader4.JH_ParentTableCode = "JS";
			jobHeader4.JH_ParentID = unsavedShipment1.PK;
			var jobHeader5 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader5.JH_JobNum = "Job5";
			jobHeader5.JH_ParentTableCode = "JS";
			jobHeader5.JH_ParentID = unsavedShipment2.PK;
			var jobHeader6 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader6.JH_JobNum = "Job6";
			jobHeader6.JH_ParentTableCode = "JS";
			jobHeader6.JH_ParentID = unsavedShipment3.PK;

			var charge4 = Factory.NewWithValidTestData<JobCharge>();
			charge4.JR_AC = Env.Registry.FreightChargeCode;
			charge4.JR_JH = jobHeader4.PK;
			charge4.JR_E6 = unSavedConsolCost.PK;
			charge4.JR_LocalCostAmt = charge4.JR_OSCostAmt = 100m;
			var charge5 = Factory.NewWithValidTestData<JobCharge>();
			charge5.JR_AC = Env.Registry.FreightChargeCode;
			charge5.JR_JH = jobHeader5.PK;
			charge5.JR_E6 = unSavedConsolCost.PK;
			charge5.JR_LocalCostAmt = charge5.JR_OSCostAmt = 200m;
			var charge6 = Factory.NewWithValidTestData<JobCharge>();
			charge6.JR_AC = Env.Registry.FreightChargeCode;
			charge6.JR_JH = jobHeader6.PK;
			charge6.JR_E6 = unSavedConsolCost.PK;
			charge6.JR_LocalCostAmt = charge6.JR_OSCostAmt = 300m;

			var detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { savedShipment1 });
			AssertContains("Cannot detach the shipment S1 from the consol C1 as posted/unposted apportionments exist on the consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { unsavedShipment1 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { savedShipment1, unsavedShipment1 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { savedShipment1, unsavedShipment1, unsavedShipment2 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { savedShipment1, unsavedShipment1, unsavedShipment2, unsavedShipment3 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { savedShipment1, unsavedShipment1, unsavedShipment2, savedShipment2 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { savedShipment1, savedShipment2, savedShipment3, unsavedShipment1 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { unsavedShipment1, unsavedShipment2, unsavedShipment3, savedShipment1 });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);
		}

		public void TestIsAllowedToDetach_CheckPastCutOffDate()
		{
			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			AssertEquals(true, Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB }).RestrictedMessage.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2 }).RestrictedMessage.IsEmpty);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertContains("Cannot detach the shipments (SHIPMENTA, SHIPMENTB) from the consol CONSOL1 as the consol's Cut Off Date has passed.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2 });
			AssertContains("Cannot detach the consol CONSOL1 from the shipment SHIPMENTA as the consol's Cut Off Date has passed.", detachRequest.RestrictedMessage);
		}

		public void TestIsAllowedToDetach_CheckSTDShipmentHasJobAndChanges()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipmentA.PK;
			Factory.Save();

			shipmentA.JS_TransportMode = "Sea";
			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			Factory.Save();
			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
		}

		public void TestIsAllowedToDetach_CheckASMShipmentHasJobAndChanges()
		{
			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipmentA.PK;
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader2.JH_JobNum = "Job2";
			jobHeader2.JH_ParentTableCode = "JS";
			jobHeader2.JH_ParentID = shipmentB.PK;
			var jobHeader3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader3.JH_JobNum = "Job3";
			jobHeader3.JH_ParentTableCode = "JS";
			jobHeader3.JH_ParentID = shipmentC.PK;

			shipmentA.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipmentB.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			// Nested shipments: A <- B <- C
			shipmentA.CoLoadShipments.Add(shipmentB);
			shipmentB.CoLoadShipments.Add(shipmentC);
			Factory.Save();

			shipmentC.JS_TransportMode = "Sea";
			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA });
			AssertContains("Shipment contains important changes and should be saved before it can be detached from consol.", detachRequest.RestrictedMessage);

			Factory.Save();
			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
		}

		public void TestIsAllowedToDetach_CheckHasConsolApportionmentOnShipment()
		{
			BusinessObject consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol1.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_JobNum = "hello";
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_ParentID = shipmentB.PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = jobHeader.PK;

			Factory.Save();
			AssertEquals(true, Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB }).RestrictedMessage.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 }).RestrictedMessage.IsEmpty);

			charge.JR_E6 = consolCost.PK;
			Factory.Save();

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertContains("Cannot detach the shipment SHIPMENTB from the consol CONSOL1 as posted/unposted apportionments exist on the consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 });
			AssertContains("Cannot detach the consol CONSOL1 from the shipment SHIPMENTB as posted/unposted apportionments exist on the consol.", detachRequest.RestrictedMessage);
		}

		public void TestIsAllowedToDetach_CheckHasConsolApportionmentInOtherCompany()
		{
			var otherCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
			AssertNotNull("otherCompanyBranch", otherCompanyBranch);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				BusinessObject consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
				consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
				try
				{
					consolCost[JobConsolCostSchema.E6_ParentID] = consol1.PK;
					consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
				}
				finally
				{
					consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
				}
				consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = Env.Registry.FreightChargeCode;
				consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_JobNum = "hello";
				jobHeader.JH_ParentTableCode = "JS";
				jobHeader.JH_ParentID = shipmentB.PK;

				JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_JH = jobHeader.PK;

				Factory.Save();
				AssertEquals(true, Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB }).RestrictedMessage.IsEmpty);
				AssertEquals(true, Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 }).RestrictedMessage.IsEmpty);

				charge.JR_E6 = consolCost.PK;
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			consol1 = newFactory.Load<CommonConsol>(consol1.PK);
			shipmentA = newFactory.Load<CommonShipment>(shipmentA.PK);
			shipmentB = newFactory.Load<CommonShipment>(shipmentB.PK);
			AssertNull(shipmentA.Job);
			AssertNull(shipmentB.Job);

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertContains("Cannot detach the shipment SHIPMENTB from the consol CONSOL1 as posted/unposted apportionments exist on the consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 });
			AssertContains("Cannot detach the consol CONSOL1 from the shipment SHIPMENTB as posted/unposted apportionments exist on the consol.", detachRequest.RestrictedMessage);
		}

		public void TestIsAllowedToDetach_CheckHasShipmentChargesOnCollectInvoiceWhenConsolJK_UniqueConsignRefIsEmpty()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENTA";
			consol.Shipments.AddRange(shipment);

			AssertEquals(string.Empty, consol.JK_UniqueConsignRef);
			var detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { shipment });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
			detachRequest = Helper.IsAllowedToDetachConsols(shipment, new[] { consol });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
		}

		public void TestCheckHasShipmentChargesOnCollectInvoiceWhenAH_ConsolidatedInvoiceRefIsLiteralize()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.Name;

				AssertCollectionContains("AH_ConsolidatedInvoiceRef", ParameterSettingsCache.FieldsToLiteralize);

				var consol = Factory.New<CommonConsol>();
				var shipment = Factory.New<CommonShipment>();
				shipment.JS_UniqueConsignRef = "SHIPMENTA";
				consol.Shipments.AddRange(shipment);
				consol.JK_UniqueConsignRef = "CONSOL1";

				var detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { shipment });
				AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
			}
		}

		public void TestIsAllowedToDetach_CheckHasShipmentChargesOnCollectInvoice()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_JobNum = "hello";
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_ParentID = shipmentB.PK;

			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_ConsolidatedInvoiceRef = "hello!";
			arInvoice.AH_JH = ZGuid.Empty;

			var revenueLine = Factory.NewWithValidTestData<AccTransactionLines>();
			revenueLine.AL_LineType = TransactionLineTypes.Revenue;
			revenueLine.AL_JH = jobHeader.PK;
			revenueLine.AL_AH = arInvoice.PK;
			revenueLine.AL_AC = Env.Registry.FreightChargeCode;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_JH = jobHeader.PK;
			charge.JR_AL_ARLine = revenueLine.PK;

			Factory.Save();
			AssertEquals(true, Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB }).RestrictedMessage.IsEmpty);
			AssertEquals(true, Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 }).RestrictedMessage.IsEmpty);

			arInvoice.AH_ConsolidatedInvoiceRef = "CONSOL1/A";
			Factory.Save();

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertContains("Cannot detach the shipment SHIPMENTB from the consol CONSOL1 as there are collect charges posted.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 });
			AssertContains("Cannot detach the consol CONSOL1 from the shipment SHIPMENTB as there are collect charges posted.", detachRequest.RestrictedMessage);
		}

		public void TestIsAllowedToDetach_CheckShipmentHasMasterOnSameConsol()
		{
			shipmentA.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			// Nested shipments: A <- B <- C
			shipmentA.CoLoadShipments.Add(shipmentB);
			shipmentB.CoLoadShipments.Add(shipmentC);

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentC });
			AssertContains("Cannot detach the shipment SHIPMENTC from the consol CONSOL1 as the shipment is a sub-shipment of the master-shipment SHIPMENTB that also belongs to the consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentC, new[] { consol1 });
			AssertContains("Cannot detach the consol CONSOL1 from the shipment SHIPMENTC as the shipment is a sub-shipment of the master-shipment SHIPMENTB that also belongs to the consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentB, shipmentC });
			AssertContains("Cannot detach the shipment SHIPMENTB from the consol CONSOL1 as the shipment is a sub-shipment of the master-shipment SHIPMENTA that also belongs to the consol.", detachRequest.RestrictedMessage);

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2, consol3 });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
		}

		public void TestIsAllowedToDetach_AdditionalMessage_CutOffDatePassed()
		{
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertEquals(string.Empty, detachRequest.AdditionalMessage_CutOffDatePassed);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2 });
			AssertEquals(string.Empty, detachRequest.AdditionalMessage_CutOffDatePassed);

			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);
			consol2.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertEquals("The Cut Off Date has passed for the consol CONSOL1. Are you sure that you want to detach the selected shipments (SHIPMENTA, SHIPMENTB)?", detachRequest.AdditionalMessage_CutOffDatePassed);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2, consol3 });
			AssertEquals("The Cut Off Date has passed for the consols (CONSOL1, CONSOL2). Are you sure that you want to detach the consols from the shipment SHIPMENTA?", detachRequest.AdditionalMessage_CutOffDatePassed);
		}

		public void TestIsAllowedToDetach_AdditionalMessage_DetachSubShipments()
		{
			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertEquals(string.Empty, detachRequest.AdditionalMessage_DetachSubShipments);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2 });
			AssertEquals(string.Empty, detachRequest.AdditionalMessage_DetachSubShipments);

			var subShipmentA1 = shipmentA.CoLoadShipments.AddNew();
			subShipmentA1.JS_UniqueConsignRef = "SUBA1";

			var subSubShipmentA11 = subShipmentA1.CoLoadShipments.AddNew();
			subSubShipmentA11.JS_UniqueConsignRef = "SUBA11";

			string expectedMessage =
@"The shipment SHIPMENTA is a master / lead shipment of (SUBA1, SUBA11).

Do you also want to detach the sub-shipments from the consol CONSOL1?

Press [Yes] to detach the master / lead shipment and sub-shipments from the consol.
Press [No] to detach the master / lead shipment from the consol, but keep the sub-shipments attached to the consol.
Press [Cancel] to cancel the operation.";

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB });
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_DetachSubShipments);

			expectedMessage =
@"The shipments (SHIPMENTA, SUBA1) are master / lead shipments of SUBA11.

Do you also want to detach the sub-shipment from the consol CONSOL1?

Press [Yes] to detach the master / lead shipments and sub-shipment from the consol.
Press [No] to detach the master / lead shipments from the consol, but keep the sub-shipment attached to the consol.
Press [Cancel] to cancel the operation.";

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, subShipmentA1 });
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_DetachSubShipments);

			expectedMessage =
@"The shipment SHIPMENTA is a master / lead shipment of (SUBA1, SUBA11).

Do you also want to detach the consols (CONSOL1, CONSOL2, CONSOL3) from the shipments (SUBA1, SUBA11)?

Press [Yes] to detach the consols from the master / lead shipment & sub-shipments.
Press [No] to detach the consols from the master / lead shipment, but keep the sub-shipments attached to the consols.
Press [Cancel] to cancel the operation.";

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2, consol3 });
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_DetachSubShipments);
		}

		public void TestIsAllowedToDetach_CheckPivotCouldBeDeleted()
		{
			var deleteChecker = new Mock<DeleteChecker>(MockBehavior.Strict);

			Hashtable mockBizoStrategies = new Hashtable();
			mockBizoStrategies.Add("JobConShipLink", new TestObjectHandle(new ArrayList() { deleteChecker.Object }));
			using (ObjectFactory.Substitute("BusinessObjectStrategies", mockBizoStrategies))
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				CommonConsol consol = factory.Load<CommonConsol>(consol1.PK);
				CommonShipment shipment = factory.Load<CommonShipment>(shipmentA.PK);

				factory.Save();

				deleteChecker.Setup(m => m.DeleteDetails(It.IsAny<BusinessObject>())).Returns(new DeleteDetails.Disallow("Problem, officer?.."));

				var detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { shipment });
				AssertEquals("Cannot detach the shipment SHIPMENTA from the consol CONSOL1: Problem, officer?..\r\n", detachRequest.RestrictedMessage);

				deleteChecker.VerifyAll();
			}
		}

		public void TestIsAllowedToDetach_CheckPivotCouldBeDeletedWithSubShipments()
		{
			var deleteChecker = new Mock<DeleteChecker>();

			var mockBizoStrategies = new Hashtable { { "JobConShipLink", new TestObjectHandle(new ArrayList() { deleteChecker.Object }) } };

			using (ObjectFactory.Substitute("BusinessObjectStrategies", mockBizoStrategies))
			{
				var factory = new BusinessObjectFactory();

				var consol = factory.NewWithValidTestData<CommonConsol>();
				consol.JK_UniqueConsignRef = "CONSOL";
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				shipment.JS_UniqueConsignRef = "MASTER";

				var subShipment1 = shipment.CoLoadShipments.AddNew();
				subShipment1.JS_UniqueConsignRef = "SUB1";
				subShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				var subShipment2 = shipment.CoLoadShipments.AddNew();
				subShipment2.JS_UniqueConsignRef = "SUB2";
				subShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var masterPivot = shipment.Consols.GetRelationshipBusinessObject(consol);

				factory.Save();

				deleteChecker.Setup(m => m.DeleteDetails(It.IsAny<BusinessObject>())).Returns(new Func<BusinessObject, DeleteDetails>(pivot => (pivot == masterPivot)
					? new DeleteDetails.Allow()
					: new DeleteDetails.Disallow("Sub-shipments cannot be deleted.")));

				var detachRequest = Helper.IsAllowedToDetachShipments(consol, new[] { shipment });
				AssertEquals("Cannot detach the shipments (SUB1, SUB2) from the consol CONSOL: Sub-shipments cannot be deleted.\r\n", detachRequest.RestrictedMessage);

				deleteChecker.VerifyAll();

				deleteChecker.Verify(m => m.DeleteDetails(It.IsAny<BusinessObject>()), Times.Exactly(3));
			}
		}

		public void TestIsAllowedToDetach_CheckConsolTypeAccessRights()
		{
			var agentConsol = Factory.NewWithValidTestData<CommonConsol>();
			agentConsol.JK_AgentType = Constants.AgentType.Agent;
			agentConsol.JK_UniqueConsignRef = "C0000000";

			var directConsol = Factory.NewWithValidTestData<CommonConsol>();
			directConsol.JK_AgentType = Constants.AgentType.Direct;
			directConsol.JK_UniqueConsignRef = "C0000001";

			var coLoadConsol1 = Factory.NewWithValidTestData<CommonConsol>();
			coLoadConsol1.JK_AgentType = Constants.AgentType.CoLoad;
			coLoadConsol1.JK_UniqueConsignRef = "C0000002";

			var coLoadConsol2 = Factory.NewWithValidTestData<CommonConsol>();
			coLoadConsol2.JK_AgentType = Constants.AgentType.CoLoad;
			coLoadConsol2.JK_UniqueConsignRef = "C0000003";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			Env.Security.MaintainConsolTypeAgent.IsAllowed = true;
			Env.Security.MaintainConsolTypeDirect.IsAllowed = false;
			Env.Security.MaintainConsolTypeCoLoad.IsAllowed = false;

			var expectedError = @"Cannot Detach
Consol C0000001
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Consol Type Access -> Direct
Consol C0000002, Consol C0000003
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Consol Type Access -> Co-Load";

			var detachRequest = Helper.IsAllowedToDetachConsols(shipment, new[] { agentConsol });
			Assert(detachRequest.RestrictedMessage.IsEmpty);

			detachRequest = Helper.IsAllowedToDetachConsols(shipment, new[] { agentConsol, directConsol, coLoadConsol1, coLoadConsol2 });
			AssertEquals(expectedError, detachRequest.RestrictedMessage.ToString());
		}

		public void TestIsAllowedToDetach_CheckStandAloneShipmentCannotDetachConsol()
		{
			standAloneShipment.JS_IsForwardRegistered = ZBool.True;
			standAloneShipment.JS_IsBooking = ZBool.True;
			Factory.Save();

			AssertEquals(true, standAloneShipment.IsStandAloneShipmentFromBooking);
			standAloneConsol.IsAttachedToStandAloneShipment = true;
			standAloneShipment.Consols.Add(standAloneConsol);
			AssertEquals(false, standAloneShipment.IsStandAloneShipmentFromBooking);

			var detachRequest = Helper.IsAllowedToDetachConsols(standAloneShipment, new[] { standAloneConsol });
			AssertContains("Cannot detach consol ALONECONSOL as the shipment is a standalone shipment. Please save the form first or cancel the form.", detachRequest.RestrictedMessage);

			standAloneConsol.IsAttachedToStandAloneShipment = false;
			detachRequest = Helper.IsAllowedToDetachConsols(standAloneShipment, new[] { standAloneConsol });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
		}

		public void TestIsAllowedToDetach_CheckStandAloneShipmentCannotDetachConsol_MasterShipment()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_UniqueConsignRef = "Master001";
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			standAloneShipment.JS_IsForwardRegistered = ZBool.True;
			standAloneShipment.JS_IsBooking = ZBool.True;
			masterShipment.Consols.Add(standAloneConsol);

			Factory.Save();

			AssertEquals(true, standAloneShipment.IsStandAloneShipmentFromBooking);

			standAloneConsol.IsAttachedToStandAloneShipment = true;
			masterShipment.CoLoadShipments.Add(standAloneShipment);

			var detachRequest = Helper.IsAllowedToDetachConsols(masterShipment, new[] { standAloneConsol });
			AssertContains("Cannot detach consol ALONECONSOL as the shipment is a standalone shipment. Please save the form first or cancel the form.", detachRequest.RestrictedMessage);

			standAloneConsol.IsAttachedToStandAloneShipment = false;
			detachRequest = Helper.IsAllowedToDetachConsols(masterShipment, new[] { standAloneConsol });
			AssertEquals(true, detachRequest.RestrictedMessage.IsEmpty);
		}

		public void TestIsAllowedToDetach_AdditionalMessage_ExportNotification755Message()
		{
			var receivedFFMAndDepartureMessage = "This shipment has been linked to an Export Notification (755) at Cargo Information Network (CIN) France. To de-link any MRN after FFM and departure message received, please manually correct this with Customs Authorities if needed.";
			var awaitingResponseMessageShipmentMessage = "A response has not yet been received from Cargo Information Network (CIN) France. Click \"Yes\" if you want to wait for a response. Click \"No\" if you want to continue detaching the Shipment from the Consol.";

			var documentName = (NoResString)"Export Notification (755)";

			var detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			shipmentA.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentA.JS_RL_NKOrigin = "FRMAR";
			var shipmentADocumentData = CreateDocumentData(shipmentA) as IStmALogProvider;

			shipmentB.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentB.JS_RL_NKOrigin = "FRMAR";
			var shipmentBDocumentData = CreateDocumentData(shipmentB) as IStmALogProvider;

			shipmentC.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentC.JS_RL_NKOrigin = "FRMAR";
			var shipmentCDocumentData = CreateDocumentData(shipmentC) as IStmALogProvider;

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			CreateLog(shipmentADocumentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(shipmentADocumentData,
				Events.InterchangeSent,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals(awaitingResponseMessageShipmentMessage, detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2, consol3 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals(awaitingResponseMessageShipmentMessage, detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2, consol3 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			CreateLog(shipmentBDocumentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(shipmentBDocumentData,
				Events.InterchangeSent,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			CreateLog(shipmentBDocumentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			var expectedMessage = @"This shipment's MRN Number should be de-linked from the Air Waybill registered at Cargo Information Network (CIN) France. Before detaching the Shipment, you should cancel the Export Notification (755) at Cargo Information Network (CIN) France. To do so, go to Shipment SHIPMENTB > Electronic Messaging > Port Messaging > Export > CIN Export Notification 755 (FR) and run Cancel/Withdraw Message.
Click ""Yes"" if you first want to de-link the MRN Number and stop detaching the shipment from the Consol. Click ""No"" if you want to continue detaching the Shipment from the Consol.";

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals(awaitingResponseMessageShipmentMessage, detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2, consol3 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			CreateLog(shipmentADocumentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			expectedMessage = @"This shipment's MRN Number should be de-linked from the Air Waybill registered at Cargo Information Network (CIN) France. Before detaching the Shipment, you should cancel the Export Notification (755) at Cargo Information Network (CIN) France. To do so, go to Shipment (SHIPMENTA, SHIPMENTB) > Electronic Messaging > Port Messaging > Export > CIN Export Notification 755 (FR) and run Cancel/Withdraw Message.
Click ""Yes"" if you first want to de-link the MRN Number and stop detaching the shipment from the Consol. Click ""No"" if you want to continue detaching the Shipment from the Consol.";

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			CreateLog(shipmentCDocumentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(shipmentCDocumentData,
				Events.InterchangeSent,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals(awaitingResponseMessageShipmentMessage, detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			CreateLog(shipmentBDocumentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 23),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			expectedMessage = @"This shipment's MRN Number should be de-linked from the Air Waybill registered at Cargo Information Network (CIN) France. Before detaching the Shipment, you should cancel the Export Notification (755) at Cargo Information Network (CIN) France. To do so, go to Shipment SHIPMENTA > Electronic Messaging > Port Messaging > Export > CIN Export Notification 755 (FR) and run Cancel/Withdraw Message.
Click ""Yes"" if you first want to de-link the MRN Number and stop detaching the shipment from the Consol. Click ""No"" if you want to continue detaching the Shipment from the Consol.";

			detachRequest = Helper.IsAllowedToDetachShipments(consol1, new[] { shipmentA, shipmentB, shipmentC });
			AssertEquals(receivedFFMAndDepartureMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals(awaitingResponseMessageShipmentMessage, detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentA, new[] { consol1, consol2, consol3 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentB, new[] { consol1, consol2, consol3 });
			AssertEquals(receivedFFMAndDepartureMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			detachRequest = Helper.IsAllowedToDetachConsols(shipmentC, new[] { consol1, consol2, consol3 });
			AssertNullOrEmpty(detachRequest.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertNullOrEmpty(expectedMessage, detachRequest.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals(awaitingResponseMessageShipmentMessage, detachRequest.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);

			IVisualizerDocumentData CreateDocumentData(CommonShipment shipment)
			{
				var documentData = Factory.New<VisualizerDocumentData>();
				documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				documentData.JDD_ParentID = shipment.PK;
				documentData.JDD_Name = "CINExportNotification";

				return documentData;
			}
		}

		#endregion

		#region SubShipments

		public void TestIsAllowedToAddNewSubShipment()
		{
			AssertEquals(true, Helper.IsAllowedToAddNewSubShipment(out message, shipmentA));

			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			AssertEquals("Precondition", false, Helper.IsAllowedToAddNewShipment(out message, consol1));
			string expectedMessage = "New sub-shipment cannot be added to the master-shipment SHIPMENTA as the master has a consol that cannot be attached to a new sub-shipment.\r\n" + message;

			AssertEquals(false, Helper.IsAllowedToAddNewSubShipment(out message, shipmentA));
			AssertContains(expectedMessage, message);
		}

		public void TestIsAllowedToAttachSubShipment()
		{
			AssertEquals(true, Helper.IsAllowedToAttachSubShipment(out message, shipmentA, standAloneShipment));

			consol1.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			AssertEquals("Precondition", false, Helper.IsAllowedToAttachShipment(consol1, standAloneShipment).Errors.IsEmpty);
			var attachRequest = Helper.IsAllowedToAttachShipment(consol1, standAloneShipment);
			string expectedMessage = "The sub-shipment ALONESHIPMENT cannot be attached to the master-shipment SHIPMENTA as the master has a consol that cannot be attached to the sub-shipment.\r\n" + attachRequest.Errors;

			AssertEquals(false, Helper.IsAllowedToAttachSubShipment(out message, shipmentA, standAloneShipment));
			AssertContains(expectedMessage, message);

			standAloneShipment.Consols.Add(consol1);
			AssertEquals("Not checking consols that are already on both master and sub", true, Helper.IsAllowedToAttachSubShipment(out message, shipmentA, standAloneShipment));
		}

		public void TestIsAllowedToAttachSubShipment_MasterAndSubAreAlreadyOnTheSameConsol_ButLoadedInDifferentFactories()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			Factory.Save();

			var shipment2Reloaded = new BusinessObjectFactory().Load<CommonShipment>(shipment2.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Master and sub are already attached to the same consol",
					shipment1.Consols.Single().PK,
					shipment2Reloaded.Consols.Single().PK);

				AssertNotEquals("Master and sub have different factories",
					shipment2.Factory._Instance,
					shipment2Reloaded.Factory._Instance);

				AssertEquals("Not checking consols that are already on both master and sub even when they are loaded in different factories",
					true,
					Helper.IsAllowedToAttachSubShipment(out message, shipment1, shipment2Reloaded));
			});
		}

		public void TestGetConsolsToDetachFromSubShipments()
		{
			var oldMaster = Factory.New<CommonShipment>();
			oldMaster.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var subShipment1 = oldMaster.CoLoadShipments.AddNew();
			subShipment1.JS_UniqueConsignRef = "SUB1";

			var subSubShipment11 = subShipment1.CoLoadShipments.AddNew();
			subSubShipment11.JS_UniqueConsignRef = "SUB11";

			var subShipment2 = oldMaster.CoLoadShipments.AddNew();
			subShipment2.JS_UniqueConsignRef = "SUB2";

			var newMaster = Factory.New<CommonShipment>();

			oldMaster.Consols.AddRange(new[] { consol1, consol2, consol3 });
			newMaster.Consols.AddRange(new[] { consol1 });

			var consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, null, oldMaster, newMaster, new[] { subShipment1, subShipment2 });
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, consolsToDetach);
			AssertEquals("Do you also want to detach the consols (CONSOL2, CONSOL3) from the shipments (SUB1, SUB11, SUB2)?", message);

			consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, consol2, oldMaster, newMaster, new[] { subShipment1, subShipment2 });
			AssertContainsExactElementsInAnyOrder(new[] { consol3 }, consolsToDetach);

			consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, null, oldMaster, null, new[] { subShipment1, subShipment2 });
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol3 }, consolsToDetach);

			consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, null, null, null, new[] { subShipment1, subShipment2 });
			AssertContainsExactElementsInAnyOrder(Array.Empty<CommonConsol>(), consolsToDetach);
			AssertEquals(string.Empty, message);
		}

		public void TestGetConsolsToDetachFromSubShipments_WhenMasterShipmentTypeIsBuyersConsolLead()
		{
			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_UniqueConsignRef = "CONSOL4";

			var oldMaster = Factory.New<CommonShipment>();
			oldMaster.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var subShipment1 = oldMaster.CoLoadShipments.AddNew();
			var subShipment2 = oldMaster.CoLoadShipments.AddNew();

			var newMaster = Factory.New<CommonShipment>();

			oldMaster.Consols.AddRange(new[] { consol1, consol2, consol3, consol4 });
			newMaster.Consols.AddRange(new[] { consol1 });

			subShipment1.Consols.Remove(consol4);
			subShipment2.Consols.Remove(consol4);
			subShipment1.JS_UniqueConsignRef = "SUB1";
			subShipment2.JS_UniqueConsignRef = "SUB2";

			var consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, null, oldMaster, newMaster, new[] { subShipment1, subShipment2 });
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, consolsToDetach);
			AssertEquals("Do you also want to detach the consols (CONSOL2, CONSOL3) from the shipments (SUB1, SUB2)?", message);

			consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, null, oldMaster, null, new[] { subShipment1, subShipment2 });
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol3 }, consolsToDetach);
			AssertEquals("Do you also want to detach the consols (CONSOL1, CONSOL2, CONSOL3) from the shipments (SUB1, SUB2)?", message);
		}

		public void TestMessageForUnsaved()
		{
			var oldMaster = Factory.New<CommonShipment>();
			oldMaster.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var newMaster = Factory.New<CommonShipment>();

			var subShipment1 = oldMaster.CoLoadShipments.AddNew();

			var subSubShipment11 = subShipment1.CoLoadShipments.AddNew();
			subSubShipment11.JS_UniqueConsignRef = "SUB11";

			var subShipment2 = oldMaster.CoLoadShipments.AddNew();
			subShipment2.JS_HouseBill = "HBILL-SUB2";

			var consol1 = Factory.New<CommonConsol>();
			var consol2 = Factory.New<CommonConsol>();
			var consol3 = Factory.New<CommonConsol>();

			consol3.JK_UniqueConsignRef = "CONSOL3";

			oldMaster.Consols.AddRange(new[] { consol1, consol2, consol3 });
			newMaster.Consols.AddRange(new[] { consol1 });

			var consolsToDetach = Helper.GetConsolsToDetachFromSubShipments(out message, null, oldMaster, newMaster, new[] { subShipment1, subShipment2 });
			AssertEquals("Do you also want to detach the consols ([new consol], CONSOL3) from the shipments ([new shipment], SUB11, HBILL-SUB2)?", message);
		}

		#endregion

		#region DetachShipmentsFromConsols

		public void TestDetachShipmentsFromConsols()
		{
			var subShipmentA1 = shipmentA.CoLoadShipments.AddNew();
			subShipmentA1.JS_UniqueConsignRef = "SUBA1";

			var subSubShipmentA11 = subShipmentA1.CoLoadShipments.AddNew();
			subSubShipmentA11.JS_UniqueConsignRef = "SUBA11";

			var subSubShipmentA12 = subShipmentA1.CoLoadShipments.AddNew();
			subSubShipmentA12.JS_UniqueConsignRef = "SUBA12";

			Factory.Save();

			consol2.Shipments.Remove(subShipmentA1);
			consol2.Shipments.Remove(subSubShipmentA11);
			consol3.Shipments.Remove(subShipmentA1);

			AssertContainsExactElementsInAnyOrder(new[] { shipmentA, shipmentB, shipmentC, subShipmentA1, subSubShipmentA11, subSubShipmentA12 }, consol1.Shipments);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentA, shipmentB, shipmentC, subSubShipmentA12 }, consol2.Shipments);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentA, shipmentB, shipmentC, subSubShipmentA11, subSubShipmentA12 }, consol3.Shipments);

			Helper.DetachShipmentsFromConsols(new[] { shipmentA, shipmentB, null, standAloneShipment }, new[] { consol1, consol2, null, standAloneConsol });

			AssertContainsExactElementsInAnyOrder(new[] { shipmentC }, consol1.Shipments);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentC, subSubShipmentA12 }, consol2.Shipments);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentA, shipmentB, shipmentC, subSubShipmentA11, subSubShipmentA12 }, consol3.Shipments);
		}

		public void TestHowManyTimesEventsGetFiredWhileDetachingGroupOfShipments()
		{
			shipmentA.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			// Nested shipments: A <- B <- C
			shipmentA.CoLoadShipments.Add(shipmentB);
			shipmentB.CoLoadShipments.Add(shipmentC);

			int numberOfShipmentsCountChanged = 0;
			int numberOfShipmentsForTotalingCountChanged = 0;

			consol1.Shipments.CountChanged += (s, e) => { numberOfShipmentsCountChanged++; };

			consol1.ShipmentsForTotalling.CountChanged += (s, e) => { numberOfShipmentsForTotalingCountChanged++; };

			Helper.DetachShipmentsFromConsols(new[] { shipmentA }, new[] { consol1 });

			AssertEquals(0, numberOfShipmentsCountChanged);
			AssertEquals(0, numberOfShipmentsForTotalingCountChanged);
		}

		public void TestDetachNotGetTrapedWhenWeHaveCrossReferencesSubshipments()
		{
			shipmentA.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			// Nested shipments: A <- B <- C <- A
			shipmentA.CoLoadShipments.Add(shipmentB);
			shipmentB.CoLoadShipments.Add(shipmentC);
			shipmentC.CoLoadShipments.Add(shipmentA);

			Helper.DetachShipmentsFromConsols(new[] { shipmentA }, new[] { consol1 });

			Assert(!consol1.Shipments.Contains(shipmentA) && !consol1.Shipments.Contains(shipmentB) && !consol1.Shipments.Contains(shipmentC));
		}

		public void TestDetachMultipleShipmentsIncludingMasterAndSub()
		{
			shipmentA.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			shipmentA.CoLoadShipments.Add(shipmentB);
			shipmentA.CoLoadShipments.Add(shipmentC);

			Helper.DetachShipmentsFromConsols(new[] { shipmentA, shipmentB }, new[] { consol1 });

			AssertEquals("Consol shouldn't have any shipments attached", 0, consol1.Shipments.Count);
		}

		#endregion

		#region CheckRelatedAgents

		public void TestCheckRelatedReceivingAgents()
		{
			consol1.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			shipmentA.ConsigneePK = consignee.PK;

			shipmentA.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			var message = Helper.CheckRelatedReceivingAgents(Enumerable.Cast<CommonShipment>(shipmentA), Enumerable.Cast<CommonConsol>(consol1));
			AssertContains("The Consignee/Consignor Related Receiving Agent on shipment SHIPMENTA does not match the Receiving Forwarder on consol CONSOL1.", message);

			consol1.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			shipmentA.Consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);
			message = Helper.CheckRelatedReceivingAgents(Enumerable.Cast<CommonShipment>(shipmentA), Enumerable.Cast<CommonConsol>(consol1));
			AssertNotContains("The Consignee/Consignor Related Receiving Agent on shipment SHIPMENTA does not match the Receiving Forwarder on consol CONSOL1.", message);
		}

		public void TestCheckRelatedSendingAgents()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			shipmentA.ConsignorPK = consignor.PK;

			shipmentA.Consignor.SetRelatedParty(org1, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup);
			var message = Helper.CheckRelatedSendingAgents(Enumerable.Cast<CommonShipment>(shipmentA), Enumerable.Cast<CommonConsol>(consol1));
			AssertContains("The Consignee/Consignor Related Sending Agent on shipment SHIPMENTA does not match the Sending Forwarder on consol CONSOL1.", message);

			consol1.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			shipmentA.Consignor.SetRelatedParty(org1, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup);
			message = Helper.CheckRelatedSendingAgents(Enumerable.Cast<CommonShipment>(shipmentA), Enumerable.Cast<CommonConsol>(consol1));
			AssertNotContains("The Consignee/Consignor Related Sending Agent on shipment SHIPMENTA does not match the Sending Forwarder on consol CONSOL1.", message);
		}

		#endregion

		#region LithiumBatteriesIsCargoOnly

		public void TestLithiumBatteriesSeaTransportDoesntMatch()
		{
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.Transports[0].JW_IsCargoOnly = true;
			consol.Transports[0].JW_IsCargoOnly = true;
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Sea;

			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dangerousGood1 = packline.UNDGs.AddNew();
			dangerousGood1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3480", "", "IMO").First().PK;
			var dangerousGood2 = packline.UNDGs.AddNew();
			dangerousGood2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3490", "", "IMO").First().PK;

			Factory.Save();
			AssertEquals("should not match because it is a SEA transport", string.Empty, Helper.IsAllowedToAttachConsol(shipment, consol).Errors);
		}

		public void TestLithiumBatteriesAirFlightsMatch()
		{
			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.Transports[0].JW_IsCargoOnly = false;
			consol.Transports[0].JW_IsCargoOnly = false;
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;

			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var packline = shipment.OuterPackLines.AddNew();
			var dangerousGood1 = packline.UNDGs.AddNew();
			dangerousGood1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3480", "", "IMO").First().PK;

			Factory.Save();
			var message = Helper.IsAllowedToAttachConsol(shipment, consol);

			AssertContains(ZString.Format("Cannot attach shipment {0} as it contains lithium substances that are not accepted by the consol {1}.", shipment.JS_UniqueConsignRef, consol.JK_UniqueConsignRef), message.Errors);
		}

		#endregion

		#region CheckForbiddenDGShipmentCannotAttachToNotCargoOnlyConsol

		public void TestCheckForbiddenDGShipmentCannotAttachToNotCargoOnlyConsol()
		{
			var undgSubstanceFOB = Factory.New<UNDGSubstance>();
			undgSubstanceFOB.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstanceFOB.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USNYC";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "MYKUL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "MYKUL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var transport1 = consol1.Transports[0];
			transport1.JW_IsLinked = true;
			transport1.JW_TransportType = "FL1";
			transport1.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("USNYC", "MYKUL").PK;
			transport1.JW_IsCargoOnly = true;

			var packline = shipmentA.OuterPackLines.AddNew();
			var dangerousGood = packline.UNDGs.AddNew();
			dangerousGood.DI_IMOClass = "CLS1";
			dangerousGood.DI_DG = undgSubstanceFOB.PK;

			var shipmentAttachErrorMesssage = "Cannot attach shipment to consol as shipment has dangerous goods substances that are forbidden for a passenger flight. Please ensure 'Is Cargo Only' flag is checked on Consol.";

			var message = Helper.IsAllowedToAttachConsol(shipmentA, consol1);
			AssertNotContains(shipmentAttachErrorMesssage, message.Errors);

			transport1.JW_IsCargoOnly = false;
			message = Helper.IsAllowedToAttachConsol(shipmentA, consol1);
			AssertContains(shipmentAttachErrorMesssage, message.Errors);

			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			message = Helper.IsAllowedToAttachConsol(shipmentA, consol1);
			AssertNotContains(shipmentAttachErrorMesssage, message.Errors);
		}

		#endregion

		public void TestCheckDatesWithinRange()
		{
			var messages = Helper.CheckDatesWithinRange(null, null);
			AssertEquals(false, messages.Any());

			messages = Helper.CheckDatesWithinRange(new[] { standAloneShipment }, new[] { standAloneConsol });
			AssertEquals(false, messages.Any());

			var now = ZDateTime.Now;
			standAloneShipment.JS_E_ARV = now;

			var standAloneShipment2 = Factory.New<CommonShipment>();
			standAloneShipment2.JS_UniqueConsignRef = "ALONESHIPMENT2";
			standAloneShipment2.JS_E_DEP = now;

			standAloneConsol.Transports.MostInterestingTransport.JW_ETA = now.AddMinutes(1);
			standAloneConsol.Transports.MostInterestingTransport.JW_ETD = now.AddMinutes(-1);

			var standAloneConsol2 = Factory.New<CommonConsol>();
			standAloneConsol2.JK_UniqueConsignRef = "ALONECONSOL2";
			standAloneConsol2.Transports.MostInterestingTransport.JW_ETD = now.AddMinutes(-1);

			messages = Helper.CheckDatesWithinRange(new[] { standAloneShipment, standAloneShipment2 }, new[] { standAloneConsol, standAloneConsol2 });
			AssertEquals(3, messages.Count);
			AssertEquals("Shipment ALONESHIPMENT has estimated arrival date before Consol ALONECONSOL ETA.", messages.ElementAt(0));
			AssertEquals("Shipment ALONESHIPMENT2 has estimated departure date after Consol ALONECONSOL ETD.", messages.ElementAt(1));
			AssertEquals("Shipment ALONESHIPMENT2 has estimated departure date after Consol ALONECONSOL2 ETD.", messages.ElementAt(2));
		}

		public void TestCheckEstimatedDeliveryDateWithinRange()
		{
			var messages = Helper.CheckShipmentEstimatedDeliveryIsAfterConsolArrival(null, null);
			AssertEquals(false, messages.Any());

			messages = Helper.CheckShipmentEstimatedDeliveryIsAfterConsolArrival(new[] { shipmentA }, new[] { consol1 });
			AssertEquals(false, messages.Any());

			var now = ZDateTime.Now;
			shipmentA.DocsAndCartage.JP_EstimatedDelivery = now.AddMinutes(1);
			shipmentB.DocsAndCartage.JP_EstimatedDelivery = now.AddMinutes(-1);
			consol1.Transports.MostInterestingTransport.JW_ETA = now;

			messages = Helper.CheckShipmentEstimatedDeliveryIsAfterConsolArrival(new[] { shipmentA, shipmentB }, new[] { consol1 });
			AssertEquals(1, messages.Count);
			AssertEquals("Shipment SHIPMENTB", messages.ElementAt(0));
		}

		public void TestAttachingShipmentToConsolPackingInstruction()
		{
			var lithiumSubstanceCode = "3090";
			var packingInstruction = "970";
			var paxLimit = 5;
			var caoLimit = 35;

			var sectionCode = PackingInstructionSectionTypeList.Codes.SectionI;
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = lithiumSubstanceCode;
			substance.DG_Variant = "a";
			substance.DG_CargoPackIns = packingInstruction;
			substance.DG_PaxPackIns = packingInstruction;
			substance.DG_LQ2OrPaxMaxAmt = paxLimit;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			substance.DG_CargoMaxAmt = caoLimit;
			substance.DG_CargoMaxAmtUQ = "KG";

			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();

			var packline = shipment.OuterPackLines.AddNew();
			var dangerousGood = packline.UNDGs.AddNew();
			dangerousGood.DI_DG = substance.PK;
			dangerousGood.DI_PackageCount = 1;
			dangerousGood.DI_F3_NKPackType = "BOX";
			dangerousGood.DI_DGWeight = caoLimit;
			dangerousGood.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			dangerousGood.DI_PackingInstructionSection = sectionCode;

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			shipment.Transports.AddNew();
			shipment.MostInterestingTransport.JW_IsCargoOnly = true;

			var shipmentAttachErrorMesssage = "Cannot attach shipment [new shipment] as it contains lithium substances which would cause the total allowable quantity for consol [new consol] to be exceeded.";

			var message = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertNotContains(shipmentAttachErrorMesssage, message.Errors);

			shipment.MostInterestingTransport.JW_IsCargoOnly = false;
			message = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertContains(shipmentAttachErrorMesssage, message.Errors);
		}

		public void TestCheckShipmentAndConsolBothHaveSentBRMessages()
		{
			var expectMessage = @"A Booking Request was sent from this Consol and Shipment. To attach this Shipment to the Consol, either:
    1.	Withdraw/Cancel the Booking Request from either the Shipment or Consol or
    2.	Reset either the Shipment or Consol's Booking Request to Original and advise the NVOCC accordingly.";

			var consol = (CommonConsol)Factory.New<IForwardingConsol>();
			consol.Logs.AddNew(Events.BookingRequested);
			var shipment = consol.Shipments.AddNew();
			shipment.Logs.AddNew(Events.BookingRequested);

			var message = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertNotContains(expectMessage, message.Errors);

			CreateEvent(shipment, Events.MessageSent);
			CreateEvent(consol, Events.MessageSent);

			message = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertContains("Both shipment and consol had sent the BR message", expectMessage, message.Errors);

			CreateEvent(shipment, Events.MessageRejected);

			message = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertNotContains("Shipment's BR had been rejected", expectMessage, message.Errors);

			CreateEvent(shipment, Events.MessageSent);
			CreateEvent(shipment, Events.MessageAccepted);
			CreateEvent(shipment, Events.MessageWithdrawCancelRequest);
			CreateEvent(shipment, Events.MessageWithdrawCancelAccepted);

			message = Helper.IsAllowedToAttachConsol(shipment, consol);
			AssertNotContains("Shipment's BR had been canceled", expectMessage, message.Errors);
		}

		void CreateEvent(IStmALogParent logParent, Event @event, string documentName = "Booking Request")
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		public void TestIsAllowedToAttachSubShipmentWhenBothSubAndMasterHaveSentCCT()
		{
			var expectMessage = "Advanced Air Cargo Reporting has been sent from this shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from this shipment prior to attaching it to Assembly master.";

			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			var subShipment = (CommonShipment)Factory.New<IForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			Assert(Helper.IsAllowedToAttachSubShipment(out message, shipment, subShipment));
			AssertNotContains(expectMessage, message);

			CreateEvent(subShipment, Events.MessageSent, "Advanced Cargo Report");

			Helper.IsAllowedToAttachSubShipment(out message, shipment, subShipment);
			AssertNotContains(expectMessage, message);

			CreateEvent(shipment, Events.MessageSent, "Advanced Cargo Report");
			Helper.IsAllowedToAttachSubShipment(out message, shipment, subShipment);
			AssertContains("Both shipment and consol had sent the BR message", expectMessage, message);
		}

		#region Implementation

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		readonly IShipmentVsConsolMessageHelper Helper = FreightShipmentVsConsolMessageHelper.Instance;

		string message;

		CommonConsol consol1;
		CommonConsol consol2;
		CommonConsol consol3;
		CommonConsol standAloneConsol;

		CommonShipment shipmentA;
		CommonShipment shipmentB;
		CommonShipment shipmentC;
		CommonShipment standAloneShipment;

		protected override void SetUp()
		{
			base.SetUp();

			consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "CONSOL1";

			consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "CONSOL2";

			consol3 = Factory.New<CommonConsol>();
			consol3.JK_UniqueConsignRef = "CONSOL3";

			standAloneConsol = Factory.New<CommonConsol>();
			standAloneConsol.JK_UniqueConsignRef = "ALONECONSOL";

			shipmentA = Factory.New<CommonShipment>();
			shipmentA.JS_UniqueConsignRef = "SHIPMENTA";

			shipmentB = Factory.New<CommonShipment>();
			shipmentB.JS_UniqueConsignRef = "SHIPMENTB";

			shipmentC = Factory.New<CommonShipment>();
			shipmentC.JS_UniqueConsignRef = "SHIPMENTC";

			standAloneShipment = Factory.New<CommonShipment>();
			standAloneShipment.JS_UniqueConsignRef = "ALONESHIPMENT";

			consol1.Shipments.AddRange(shipmentA, shipmentB, shipmentC);
			consol2.Shipments.AddRange(shipmentA, shipmentB, shipmentC);
			consol3.Shipments.AddRange(shipmentA, shipmentB, shipmentC);

			Factory.Save();
		}

		#endregion
	}
}
