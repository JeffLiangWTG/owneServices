using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonConsolTest1 : BaseFreightTest
	{
		#region SetDefaultLastForeignPortFirstArrivalPortBasedOnSailingSchedule

		public void TestSeaConsolLastForeignPort_HasError_WhenUSBasedAndSailingScheduleLastForeignIsEmpty()
		{
			// Arrange
			const string lastForeignPort = "CNTAO";
			CreateVoyageForConsol("CNSHA", "USNYC", lastForeignPort, "USNYC", "QF8332");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USNYC";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "USNYC";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_ETA = new DateTime(2023, 05, 05);
			transport.JW_VoyageFlight = "QF8332";
			transport.JW_IsLinked = true;

			//Act & Assert
			Assert("sailing schedule linked and have last foreign port", consol.SeaImportLinkedTransportLegHasLastForeignPort);
			consol.Validation.ValidateJK_RL_NKLastForeignPort();
			AssertNoErrors("Last foreign port should not have error", consol.JK_RL_NKLastForeignPortInfo);

			consol.JK_DateLastForeignPort = new DateTime(2023, 05, 05);
			consol.Validation.ValidateJK_RL_NKLastForeignPort();
			AssertHasError("Last foreign port should not be empty", consol.JK_RL_NKLastForeignPortInfo, "Please enter a Last Foreign Port.");

			consol.JK_DateLastForeignPort = ZDateTime.Empty;
			transport.JW_RL_NKDiscPort = "AUSYD";
			Assert("sailing schedule is not linked", !consol.SeaImportLinkedTransportLegHasLastForeignPort);
			consol.Validation.ValidateJK_RL_NKLastForeignPort();
			AssertHasError("Last foreign port should not be empty", consol.JK_RL_NKLastForeignPortInfo, "Please enter a Last Foreign Port.");
		}

		(CommonConsol, Transport) CreateConsolWithLinkedTransport()
		{
			CreateVoyageForConsol("AUSYD", "CNSHA", "AUBNE", "CNTAO", "QF8332");
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "CNSHA";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_ETA = new DateTime(2023, 05, 05);
			transport.JW_VoyageFlight = "QF8332";
			transport.JW_IsLinked = true;
			return (consol, transport);
		}

		public void TestOnSaving_SetDefaultLastForeignPortFirstArrivalPortBasedOnSailingSchedule_NoUpdateWhenCountryDoesNotMatch()
		{
			// Arrange
			var (consol, transport) = CreateConsolWithLinkedTransport();

			// Act & Assert
			consol.JK_RL_NKDischargePort = "USLAX";
			transport.JW_RL_NKDiscPort = "CNSHA";
			Factory.Save();
			CombineAssertions("Should not update when country of discharge port does not match", () =>
			{
				AssertEquals(string.Empty, consol.JK_RL_NKLastForeignPort);
				AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);
				AssertEquals(string.Empty, consol.JK_RL_NKPortOfFirstArrival);
				AssertEquals(ZDateTime.Empty, consol.JK_DatePortOfFirstArrival);
			});
		}

		public void TestOnSaving_SetDefaultLastForeignPortFirstArrivalPortBasedOnSailingSchedule()
		{
			var (consol, transport) = CreateConsolWithLinkedTransport();

			consol.JK_RL_NKDischargePort = "CNSHA";
			Factory.Save();
			CombineAssertions("Should update when country of discharge port match", () =>
			{
				AssertEquals("AUBNE", consol.JK_RL_NKLastForeignPort);
				AssertEquals(new DateTime(2023, 5, 1), consol.JK_DateLastForeignPort);
				AssertEquals("CNTAO", consol.JK_RL_NKPortOfFirstArrival);
				AssertEquals(new DateTime(2023, 5, 5), consol.JK_DatePortOfFirstArrival);
			});

			consol.JK_RL_NKLastForeignPort = string.Empty;
			consol.JK_DateLastForeignPort = ZDateTime.Empty;
			Factory.Save();
			CombineAssertions("Should not update when transports has no changes", () =>
			{
				AssertEquals(string.Empty, consol.JK_RL_NKLastForeignPort);
				AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);
				AssertEquals("CNTAO", consol.JK_RL_NKPortOfFirstArrival);
				AssertEquals(new DateTime(2023, 5, 5), consol.JK_DatePortOfFirstArrival);
			});
		}

		void CreateVoyageForConsol(ZString loading, ZString discharge, ZString lastForeignPort, ZString firstDischargePort, ZString flight)
		{
			var today = new DateTime(2023, 5, 5);
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportModes.Sea;
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = flight;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loading;
			origin.JA_E_DEP = today.AddDays(1);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = today.AddDays(4);
			destination.JB_RL_NKFirstDischargePort = firstDischargePort;
			destination.JB_FirstDischargePortETA = new DateTime(2023, 5, 5);
			destination.JB_RL_NKLastForeignPort = lastForeignPort;
			destination.JB_LastForeignPortETD = new DateTime(2023, 5, 1);
			voyage.GenerateSailings();
			Factory.Save();
		}

		#endregion

		#region IOriginDestinationForDocumentDeliveryRestriction

		public void TestDocumentDeliveryRestriction()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USPHL";
			var documentDeliveryRestriction = consol as IOriginDestinationForDocumentDeliveryRestriction;

			AssertNotNull(documentDeliveryRestriction);
			AssertEquals("OriginCountryCode should be AU", "AU", documentDeliveryRestriction.OriginCountryCode);
			AssertEquals("DestinationCountryCode should be US", "US", documentDeliveryRestriction.DestinationCountryCode);
		}

		#endregion

		#region Freight Load/Unload Events Special Cascading

		public void TestOnSaving_CascadeFreshFreightLoadedUnloadedEvents()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShippedOnBoardDate = ZDateTime.Now;

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container2.JC_FCLOnBoardVessel = ZDateTime.Now.AddDays(-1);
			container2.JC_FCLUnloadFromVessel = ZDateTime.Now;

			consol.Logs.AddNew(Events.FreightLoaded, "Consol event");
			consol.Logs.AddNew(Events.FreightUnloaded, "Consol event");

			Factory.Save();

			Action<BusinessObject, Event, bool> assertHasCascadedEvent = (target, freightEvent, shouldHaveEvent) =>
			{
				var eventLog = target.GetLogs().MostRecentLogByEventTime(freightEvent, "Consol event");
				AssertEquals(shouldHaveEvent, eventLog != null);
			};

			assertHasCascadedEvent(shipment1, Events.FreightLoaded, true);
			assertHasCascadedEvent(shipment1, Events.FreightUnloaded, false);

			assertHasCascadedEvent(shipment2, Events.FreightLoaded, false);
			assertHasCascadedEvent(shipment2, Events.FreightUnloaded, false);

			assertHasCascadedEvent(container1, Events.FreightLoaded, true);
			assertHasCascadedEvent(container1, Events.FreightUnloaded, true);

			assertHasCascadedEvent(container2, Events.FreightLoaded, false);
			assertHasCascadedEvent(container2, Events.FreightUnloaded, true);
		}

		#endregion

		#region CreditorIsNVOCC

		public void TestCreditorIsNVOCC()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;

			Assert(consol.CreditorIsNVOCC);

			shippingLine.RSL_IsNVO = false;
			Assert(!consol.CreditorIsNVOCC);
		}

		#endregion

		#region CreditorIsCW1User

		public void TestCreditorIsCW1User()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_IsCW1User = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;

			Assert(consol.CreditorIsCW1User);

			shippingLine.RSL_IsCW1User = false;
			Assert(!consol.CreditorIsCW1User);

			org.OH_RSL_ShippingLine = Guid.Empty;
			Assert(!consol.CreditorIsCW1User);

			consol.JK_OA_CreditorAddress = Guid.Empty;
			Assert(!consol.CreditorIsCW1User);
		}

		#endregion

		#region CreditorHasBookingRequestIntegration

		public void TestCreditorHasBookingRequestIntegration()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_BookingRequestAvailable = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_CreditorAddress = org.MainAddress.PK;

			Assert(consol.CreditorHasBookingRequestIntegration);

			shippingLine.RSL_BookingRequestAvailable = false;
			Assert(!consol.CreditorHasBookingRequestIntegration);

			org.OH_RSL_ShippingLine = Guid.Empty;
			Assert(!consol.CreditorHasBookingRequestIntegration);

			consol.JK_OA_CreditorAddress = Guid.Empty;
			Assert(!consol.CreditorHasBookingRequestIntegration);
		}

		#endregion

		public void TestShippingLineIsNVOCC()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = false;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			AssertNotNull(org.ShippingLine);
			Assert(consol.ShippingLineIsNVOCC);

			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = true;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			Assert(consol.ShippingLineIsNVOCC);

			org.OH_RSL_ShippingLine = ZGuid.Empty;

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			org.OH_IsSeaWholesaler = true;
			org.OH_IsShippingLine = false;

			AssertNull(org.ShippingLine);
			Assert(consol.ShippingLineIsNVOCC);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			org.OH_IsSeaWholesaler = true;
			org.OH_IsShippingLine = true;

			AssertNull(org.ShippingLine);
			Assert(consol.ShippingLineIsNVOCC);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Assert(!consol.ShippingLineIsNVOCC);
		}

		public void TestShippingLineIsShippingLine()
		{
			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_IsNVO = false;
			shippingLine.RSL_IsShippingLine = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			AssertNotNull(org.ShippingLine);
			Assert(consol.ShippingLineIsShippingLine);

			shippingLine.RSL_IsNVO = true;
			shippingLine.RSL_IsShippingLine = true;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			Assert(consol.ShippingLineIsShippingLine);

			org.OH_RSL_ShippingLine = ZGuid.Empty;

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			org.OH_IsSeaWholesaler = false;
			org.OH_IsShippingLine = true;

			AssertNull(org.ShippingLine);
			Assert(consol.ShippingLineIsShippingLine);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			org.OH_IsSeaWholesaler = true;
			org.OH_IsShippingLine = true;

			AssertNull(org.ShippingLine);
			Assert(consol.ShippingLineIsShippingLine);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Assert(!consol.ShippingLineIsShippingLine);
		}

		public void TestIsAgentOrDirect()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			Assert(consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.Direct;
			Assert(consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.AWBCoload;
			Assert(!consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			Assert(!consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.Charter;
			Assert(!consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			Assert(!consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.OnBoardCourier;
			Assert(!consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.Other;
			Assert(!consol.IsAgentOrDirect);

			consol.JK_AgentType = Constants.AgentType.Courier;
			Assert(consol.IsAgentOrDirect);
		}

		public void TestIsSendingOrReceivingForwarderGateway()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			Assert(!consol.IsSendingOrReceivingForwarderGateway);

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert(consol.IsSendingOrReceivingForwarderGateway);

			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Assert(consol.IsSendingOrReceivingForwarderGateway);

			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			Assert(consol.IsSendingOrReceivingForwarderGateway);
		}

		public void TestReceivingForwarderDoesntReset()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertNotNull("Receiving Forwarder Address should not be null", consol.ReceivingForwarderAddress);

			consol.JK_AgentType = Constants.AgentType.Direct;
			Assert(consol.IsAgentOrDirect);

			AssertNotNull("Receiving Forwarder Address should still not be null", consol.ReceivingForwarderAddress);
		}

		public void TestDefaultConsolModeIsOtherForCourierConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Courier;
			Assert(consol.IsCourier);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Constants.ContainerModes.Other, consol.JK_ConsolMode);
		}

		public void TestCourierConsolTransportIsNotLinkedByDefault()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			AssertEquals(1, consol.Transports.Count);
			Assert(consol.Transports[0].JW_IsLinked);

			consol.JK_AgentType = Constants.AgentType.Courier;
			Assert(consol.IsCourier);
			AssertEquals(1, consol.Transports.Count);
			Assert(!consol.Transports[0].JW_IsLinked);
		}

		public void TestTransportModeBooleanProperties()
		{
			var consol = Factory.New<CommonConsol>();

			Action<string, string> assertBooleanProperties = (transportMode, expectedResults) =>
			{
				consol.JK_TransportMode = transportMode;

				var actualResults = new ZStringBuilder();
				actualResults.Append(consol.IsAir ? "Y" : "N");
				actualResults.Append(consol.IsSea ? "Y" : "N");
				actualResults.Append(consol.IsRail ? "Y" : "N");
				actualResults.Append(consol.IsRoad ? "Y" : "N");

				AssertEquals(expectedResults, actualResults.ToString());
			};

			assertBooleanProperties("XXX", "NNNN");
			assertBooleanProperties(Constants.TransportModes.Air, "YNNN");
			assertBooleanProperties(Constants.TransportModes.Sea, "NYNN");
			assertBooleanProperties(Constants.TransportModes.Rail, "NNYN");
			assertBooleanProperties(Constants.TransportModes.Road, "NNNY");
		}

		public void TestAgentTypeBooleanProperties()
		{
			var consol = Factory.New<CommonConsol>();

			Action<string, string> assertBooleanProperties = (agentType, expectedResults) =>
			{
				consol.JK_AgentType = agentType;

				var actualResults = new ZStringBuilder();
				actualResults.Append(consol.IsDirect ? "Y" : "N");
				actualResults.Append(consol.IsCoLoad ? "Y" : "N");
				actualResults.Append(consol.IsAgent ? "Y" : "N");
				actualResults.Append(consol.IsCharter ? "Y" : "N");
				actualResults.Append(consol.IsAWBCoload ? "Y" : "N");
				actualResults.Append(consol.IsMultiAWBMaster ? "Y" : "N");
				actualResults.Append(consol.IsCourier ? "Y" : "N");
				actualResults.Append(consol.CouldBeAttachedToMultiAWBMaster ? "Y" : "N");

				AssertEquals(expectedResults, actualResults.ToString());
			};

			assertBooleanProperties("XXX", "NNNNNNNN");
			assertBooleanProperties(Constants.AgentType.Direct, "YNNNNNNY");
			assertBooleanProperties(Constants.AgentType.CoLoad, "NYNNNNNN");
			assertBooleanProperties(Constants.AgentType.Agent, "NNYNNNNN");
			assertBooleanProperties(Constants.AgentType.Charter, "NNNYNNNN");
			assertBooleanProperties(Constants.AgentType.AWBCoload, "NNNNYNNY");
			assertBooleanProperties(Constants.AgentType.AWBMaster, "NNNNNYNN");
			assertBooleanProperties(Constants.AgentType.Courier, "NNNNNNYN");
		}

		public void TestIsGatewayConsol()
		{
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			void AssertIsGatewayConsol(bool expected, ZString agentType)
			{
				Consol.JK_AgentType = agentType;
				AssertEquals("Agent type: " + agentType, expected, Consol.IsGatewayConsol);
			}

			AssertIsGatewayConsol(true, Constants.AgentType.Direct);
			AssertIsGatewayConsol(true, Constants.AgentType.CoLoad);
			AssertIsGatewayConsol(true, Constants.AgentType.Agent);
			AssertIsGatewayConsol(false, Constants.AgentType.Charter);
			AssertIsGatewayConsol(true, Constants.AgentType.AWBCoload);
			AssertIsGatewayConsol(false, Constants.AgentType.AWBMaster);
			AssertIsGatewayConsol(false, Constants.AgentType.Courier);
		}

		public void TestConsolModeBooleanProperties()
		{
			var consol = Factory.New<CommonConsol>();

			CombineAssertions("Properties should be ZBool to be correctly evaluated in document filters", () =>
			{
				AssertEquals(typeof(ZBool), consol.IsBuyersConsol.GetType());
				AssertEquals(typeof(ZBool), consol.IsFCL.GetType());
				AssertEquals(typeof(ZBool), consol.IsGroupage.GetType());
			});

			Action<string, string> assertBooleanProperties = (consolMode, expectedResults) =>
			{
				consol.JK_ConsolMode = consolMode;

				var actualResults = new ZStringBuilder();
				actualResults.Append(consol.IsBuyersConsol ? "Y" : "N");
				actualResults.Append(consol.IsFCL ? "Y" : "N");
				actualResults.Append(consol.IsGroupage ? "Y" : "N");

				AssertEquals(expectedResults, actualResults.ToString());
			};

			assertBooleanProperties("XXX", "NNN");
			assertBooleanProperties(Constants.ContainerModes.BuyersConsol, "YNN");
			assertBooleanProperties(Constants.ContainerModes.FCL, "NYN");
			assertBooleanProperties(Constants.ContainerModes.Groupage, "NNY");
			assertBooleanProperties(Constants.ContainerModes.LCL, "NNN");
		}

		public void TestCFSRelatedParties()
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			var receivingForwarder = Factory.New<OrgHeader>();
			var departureCFS = Factory.New<OrgHeader>();
			var arrivalCFS = Factory.New<OrgHeader>();

			var pickupAddress = departureCFS.Addresses.AddNew();
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var deliveryAddress = arrivalCFS.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			sendingForwarder.AddRelatedParty(departureCFS.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			receivingForwarder.AddRelatedParty(arrivalCFS.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);

			sendingForwarder.AllRelatedParties[0].PR_Location = "AUSYD";
			receivingForwarder.AllRelatedParties[0].PR_Location = "NZAKL";

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertEquals(departureCFS.PK, consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
			AssertEquals(arrivalCFS.PK, consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertEquals(departureCFS.PK, consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
			AssertEquals(arrivalCFS.PK, consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			AssertEquals(pickupAddress.PK, consol.JK_OA_PackDepotAddress);
			AssertEquals(deliveryAddress.PK, consol.JK_OA_UnpackDepotAddress);
		}

		public void TestCFSRelatedParties_FEADepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Export = true;
			GlbDepartment.CurrentDepartment.GE_Import = false;
			GlbDepartment.CurrentDepartment.GE_Air = true;

			var sendingForwarder = Factory.New<OrgHeader>();
			var airDepartureCFS = Factory.New<OrgHeader>();
			var seaDepartureCFS = Factory.New<OrgHeader>();

			var pickupAddress1 = airDepartureCFS.Addresses.AddNew();
			pickupAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var pickupAddress2 = seaDepartureCFS.Addresses.AddNew();
			pickupAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			sendingForwarder.AddRelatedParty(airDepartureCFS.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty, GlbCompany.CurrentCompany);
			sendingForwarder.AddRelatedParty(seaDepartureCFS.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty, GlbCompany.CurrentCompany);

			sendingForwarder.AllRelatedParties[0].PR_Location = "AUSYD";

			var consol1 = Factory.New<CommonConsol>();
			AssertEquals("GlbDepartment.CurrentDepartment.TransportMode is Air", TransportModes.Air, consol1.JK_TransportMode);
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals(airDepartureCFS.PK, consol1.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";

			var consol2 = shipment.Consols.AddNew();
			AssertEquals("Updated TransportMode for Consol2 based on shipment TransportMode", TransportModes.Sea, consol2.JK_TransportMode);
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertEquals(seaDepartureCFS.PK, consol2.JK_OA_PackDepotAddress_ZAddress.OrgPK);
		}

		public void TestLocalTransportRelatedParties()
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			var receivingForwarder = Factory.New<OrgHeader>();
			var departureTransport = Factory.New<OrgHeader>();
			var arrivalTransport = Factory.New<OrgHeader>();

			sendingForwarder.AddRelatedParty(departureTransport.PK, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			receivingForwarder.AddRelatedParty(arrivalTransport.PK, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertEquals(departureTransport.MainAddress.PK, consol.JK_OA_DeparturePackCFSTransportAddress);
			AssertEquals(arrivalTransport.MainAddress.PK, consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			AssertEquals(departureTransport.MainAddress.PK, consol.JK_OA_DeparturePackCFSTransportAddress);
			AssertEquals(arrivalTransport.MainAddress.PK, consol.JK_OA_ArrivalUnpackCFSTransportAddress);
		}

		public void TestTypeDecider()
		{
			AssertEquals(typeof(ConsolTypeDecider), CommonConsol.TypeDecider.GetType());
		}

		#region IMovementLeg

		public void TestArrivalDateWithEmptyTransports()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.Transports.ParentDeleting();

			Assert("Pre-condition", consol is IMovementLeg);
			AssertEquals(0, consol.Transports.Count);

			var convertedConsol = (IMovementLeg)consol;
			AssertEquals(ZDateTime.Empty, convertedConsol.ArrivalDate);
		}

		public void TestDepartureDateWithEmptyTransports()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.Transports.ParentDeleting();

			Assert("Pre-condition", consol is IMovementLeg);
			AssertEquals(0, consol.Transports.Count);

			var convertedConsol = (IMovementLeg)consol;
			AssertEquals(ZDateTime.Empty, convertedConsol.DepartureDate);
		}

		#endregion

		#region IDocumentSupportable

		public void TestIDocumentSupportable()
		{
			AssertEquals("Expected DocumentSupporter", typeof(CommonConsolDocumentSupporter), Consol.DocumentSupporter.GetType());
		}

		#endregion

		#region Universal Copy

		public void TestUniversalCopyIgnoreElement()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var componentType = consol.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true).OfType<UniversalCopyIgnoreElementAttribute>();

			AssertEquals(2, ignoreElementAttributes.Count());

			AssertEquals(1, ignoreElementAttributes.Count(e => e.ElementNames[0] == "JobConsolSummaries"));
			AssertEquals(1, ignoreElementAttributes.Count(e => e.ElementNames[0] == "JK_UniqueConsignRef"));
		}

		#endregion

		public void TestOrganisationsForCreditChecks()
		{
			var consol = Factory.New<CommonConsol>();
			var creditControlled = (ICreditControlledDocumentDelivery)consol;

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var consignor = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor));
			var consignee = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK));
			consol.JK_OA_SendingForwarderAddress = consignor.Addresses[0].PK;
			consol.JK_OA_ReceivingForwarderAddress = consignee.Addresses[0].PK;

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryCreate();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var consignor2 = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor, consignor.PK, consignee.PK));
			var consignee2 = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK, consignee.PK, consignor2.PK));

			shipment.ConsignorPK = consignor2.PK;
			shipment.ConsigneePK = consignee2.PK;
			job.LocalChargesPK = localClient.PK;

			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(5, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor2));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee2));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestDescriptionOfOrganisationBeingCheckedForCredit()
		{
			var consol = Factory.New<CommonConsol>();
			var creditOnHoldDocument = (ICreditControlledDocumentDelivery)consol;
			AssertContains("Sending Agent, Receiving Agent or Consignee, Consignor, Local Client or any Debtors in any associated Shipment", creditOnHoldDocument.DescriptionOfOrganisationBeingCheckedForCredit);
		}

		public void TestOrganisationsForCreditChecks_WithManyShipments_AndDefaultRegistry()
		{
			var consol = Factory.New<CommonConsol>();
			var creditControlled = (ICreditControlledDocumentDelivery)consol;
			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor));
			var consignee = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK));
			consol.JK_OA_SendingForwarderAddress = consignor.Addresses[0].PK;
			consol.JK_OA_ReceivingForwarderAddress = consignee.Addresses[0].PK;

			for (int i = 0; i < 20; i++)
			{
				var shipment = consol.Shipments.AddNew();
				var job = new JobHeader.Loader(shipment).TryCreate();

				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;
				job.LocalChargesPK = localClient.PK;
			}

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.Inner.DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.Inner.DeleteValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;

			AssertEquals((consol.Shipments.Count * 3) + 2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		#region IBillGenerationSupport members

		public void TestIBillGenerationSupport_Default()
		{
			CommonConsol consol = GetConsol();
			IBillGenerationSupport support = consol;

			AssertEquals("Transhipment Indicator is not used at this level.", "", support.TranshipmentIndicator);
			AssertNull("Default carrier principal", support.CarrierPrincipal);
			AssertNull("Default Destination", support.Destination);
			AssertNull("Default Origin", support.Origin);
			Assert("Default TransportMode", support.TransportMode.IsEmpty);
			AssertEquals("Default ServiceLevel is STD", "STD", support.ServiceLevel);
			AssertNull("Default Load", support.Load);
			AssertNull("Default Discharge", support.Discharge);
		}

		public void TestIBillGenerationSupport_Populated()
		{
			CommonConsol consol = GetConsol();
			IBillGenerationSupport support = consol;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals(support.CarrierPrincipal, org);
			AssertEquals("Load", "AUSYD", support.Load.Code);
			AssertEquals("Discharge", "NZAKL", support.Discharge.Code);
			AssertEquals("TransportMode", "AIR", support.TransportMode);
		}

		#endregion

		#region IRoutingSupport members

		public void TestConsolType()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("JK", Consol.ConsolType);
		}

		public void TestTransportsIncludingRelatedReadOnlyDueToPhase()
		{
			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			AssertEquals("Not read-only by default", false, ((IRoutingSupport)consol).TransportsIncludingRelated.ReadOnly);

			consol = Factory.New<CommonConsolForTest>();
			consol.PropertiesForcedToReadOnlyDueToPhase.Add("hello");
			AssertEquals("Not read-only", false, ((IRoutingSupport)consol).TransportsIncludingRelated.ReadOnly);

			consol = Factory.New<CommonConsolForTest>();
			consol.PropertiesForcedToReadOnlyDueToPhase.Add("Routing");
			AssertEquals("Read-only due to phase", true, ((IRoutingSupport)consol).TransportsIncludingRelated.ReadOnly);
		}

		public void TestIRoutingSupportMembersAir()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();
			AssertEquals(2, ((IRoutingSupport)consol).TransportsIncludingRelated.Count);
			AssertEquals(2, ((IRoutingSupport)consol).Transports.Count);
			AssertEquals("AIR", ((IRoutingSupport)consol).TransportMode);
		}

		public void TestIRoutingSupportMembersSea()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUMEL";
			Transport transport1 = consol.Transports[0];
			transport1.JW_Vessel = "Vessel1";
			transport1.JW_VoyageFlight = "23";
			transport1.JW_ETD = new ZDateTime(2008, 1, 1);
			transport1.JW_ETA = new ZDateTime(2008, 1, 15);
			transport1.JW_RL_NKDiscPort = "AUSYD";
			RefVessel testVessel2 = RefVessel.New(Factory);
			testVessel2.RV_Name = "TESTVESSEL2";
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "24";
			transport2.JW_Vessel = testVessel2.RV_FK;
			transport2.JW_ETD = transport1.JW_ETA.AddDays(1);
			transport2.JW_ETA = transport1.JW_ETA.AddDays(2);
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_RL_NKLoadPort = transport1.JW_RL_NKDiscPort;
			AssertEquals(2, ((IRoutingSupport)consol).TransportsIncludingRelated.Count);
			AssertEquals(2, ((IRoutingSupport)consol).Transports.Count);
			AssertEquals("SEA", ((IRoutingSupport)consol).TransportMode);
		}

		#endregion

		#region ConsolNumberFountain

		public void TestConsolNumberGeneration()
		{
			//this is to make sure 'C00001000' is assigned
			Factory.Save();

			var consol1 = Factory.New<CommonConsol>();
			Factory.Save();
			AssertEquals("C00001001", consol1.JK_UniqueConsignRef);

			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_Code = "MEL";
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "XYZ";
			registrationKey.ServerCodeForTest = "123";
			branch.GB_GC = company.PK;
			Factory.Save();

			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Consol;
			customisation.RemoveFountainPrefix = true;

			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, 1, "Q");
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, 2, "Z");
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.ClientCoded3, 3, "X");
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "5");
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var consol2 = Factory.New<CommonConsol>();
			Factory.Save();
			AssertEquals("QZX01002", consol2.JK_UniqueConsignRef);

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.CompanyCode, 1);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, 2);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol3 = Factory.New<CommonConsol>();
			using (branch.SetAsTemporaryContext())
			{
				Factory.Save();
				AssertEquals("ABCXYZ00001003", consol3.JK_UniqueConsignRef);
			}

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 1);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.YearAsLetter, 3);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol4 = Factory.New<CommonConsol>();
			Factory.Save();
			ZString consol4UniqueConsignRef = string.Format("{0}{1}{2}00001004",
				"ABCDEFGHIJKL"[ZDateTime.Now.Month - 1].ToString(),
				new ZString(ZDateTime.Now.Year.ToString()).Right(new ZInt(1)),
				"BCDEFGHIJKLMNOPQRSTUVWXYZA"[ZDateTime.Now.Year % 26].ToString());
			AssertEquals(consol4UniqueConsignRef, consol4.JK_UniqueConsignRef);

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.Direction, 1);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.FirstLoadIATA, 2);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.LastDischargeIATA, 3);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol5 = Factory.New<CommonConsol>();
			consol5.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol5.JK_RL_NKDischargePort = "NLAMS";
			Factory.Save();
			ZString consol5UniqueConsignRef = string.Format("E{0}AMS00001005", GlbBranch.CurrentBranch.HomePort.RL_IATA);
			AssertEquals(consol5UniqueConsignRef, consol5.JK_UniqueConsignRef);

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.FirstLoadUNLOCO, 1);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol6 = Factory.New<CommonConsol>();
			consol6.JK_RL_NKLoadPort = "GBLON";
			Factory.Save();
			AssertEquals("GBLON00001006", consol6.JK_UniqueConsignRef);

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.LastDischargeUNLOCO, 1);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol7 = Factory.New<CommonConsol>();
			consol7.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertEquals("USLAX00001007", consol7.JK_UniqueConsignRef);

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.BranchCode, 1);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.ServerCode, 2);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol8 = Factory.New<CommonConsol>();
			using (branch.SetAsTemporaryContext())
			{
				Factory.Save();
				AssertEquals("MEL12300001008", consol8.JK_UniqueConsignRef);
			}

			ResetElements(customisation);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.TransportMode, 1);
			SetElement(customisation, BillOfLadingNumberCustomisationElement.Keys.ServiceLevel, 2);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			var consol9 = Factory.New<CommonConsol>();
			consol9.JK_TransportMode = Constants.TransportModes.Air;
			consol9.JK_AWBServiceLevel = "EXP";
			Factory.Save();
			AssertEquals("AEXP00001009", consol9.JK_UniqueConsignRef);

			foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
			{
				element.Include = true;
			}
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var consol10 = Factory.New<CommonConsol>();
			try
			{
				Factory.Save();
				throw new Exception("Factory.Save() was successful, but should not have been.");
			}
			catch (GeneratedOverLengthCodeException e)
			{
				AssertContains("Generated a consol number that is too big to fit in the available space.", e.Message);
			}
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateConsolNumberIfNeeded_FixFountain()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				Factory.Save();
				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				newCustomisation.ServiceLevel = "STD";
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
				FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);

				CommonConsol consol = Factory.New<CommonConsol>();
				Factory.Save();
				AssertEquals("JK_UniqueConsignRef", "C7B001", consol.JK_UniqueConsignRef);
				consol.JK_UniqueConsignRef = "C7B002";
				Factory.Save();

				consol = Factory.New<CommonConsol>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JK_UniqueConsignRef", "C7B003", consol.JK_UniqueConsignRef);

				newCustomisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
				FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				consol.JK_UniqueConsignRef = "C7B004S";
				Factory.Save();

				consol = Factory.New<CommonConsol>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JK_UniqueConsignRef", "C7B0052", consol.JK_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateConsolNumberIfNeeded_FixFountainSameLength()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "C9997";
				consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "C10007";
				Factory.Save();

				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "3");
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2).Fountain = true;
				FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				var fountain = Env.NumberFountains.GetForwardingConsolGeneratorFountain("7");
				((IDbConnected)Factory).Connection.BeginTransaction();
				fountain.SetNext(Factory, 1000);
				((IDbConnected)Factory).Connection.CommitTransaction();
				consol = Factory.New<CommonConsol>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JK_UniqueConsignRef", "C10017", consol.JK_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateConsolNumberIfNeeded_FixFountainDifferentSuffixes()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				Factory.Save();
				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "3");
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2).Fountain = true;
				FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				var fountain = Env.NumberFountains.GetForwardingConsolGeneratorFountain("7");
				((IDbConnected)Factory).Connection.BeginTransaction();
				fountain.SetNext(Factory, 1002);
				((IDbConnected)Factory).Connection.CommitTransaction();
				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "C10027";
				consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "C10037";
				consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "C10048";
				Factory.Save();

				consol = Factory.New<CommonConsol>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JK_UniqueConsignRef", "C10047", consol.JK_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		[TestDate(2007, 02, 05)]
		public void TestPopulateConsolNumberIfNeeded_FixFountainNoPrefix()
		{
			Db.Connection.RollbackTransaction();
			using (var adminConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection))
			{
				Factory.Save();
				BillOfLadingNumberCustomisation newCustomisation = new BillOfLadingNumberCustomisation();
				newCustomisation.RemoveFountainPrefix = true;
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 1, "3");
				SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 2).Fountain = true;
				FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
				var fountain = Env.NumberFountains.GetForwardingConsolGeneratorFountain("7");
				((IDbConnected)Factory).Connection.BeginTransaction();
				fountain.SetNext(Factory, 1005);
				((IDbConnected)Factory).Connection.CommitTransaction();
				CommonConsol consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "10057";
				consol = Factory.New<CommonConsol>();
				consol.JK_UniqueConsignRef = "10067";
				Factory.Save();

				consol = Factory.New<CommonConsol>();
				try
				{
					Factory.Save();
					Fail("Index exception should happen");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
					ErrorReporter.Clear();
				}

				Factory.Save();
				AssertEquals("JK_UniqueConsignRef", "10077", consol.JK_UniqueConsignRef);
			}
			Db.Connection.BeginTransaction();
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			return element;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			return element;
		}

		void ResetElements(BillOfLadingNumberCustomisation customisation)
		{
			foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
			{
				element.Include = false;
			}
		}

		#endregion

		public void TestValidationForLoadAndDischargePorts()
		{
			var consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_IsLinked = false;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "MYBAG";
			transport.JW_ETD = ZDateTime.Now;
			consol.JK_RL_NKLoadPort = "GBLON";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.RunPreSaveValidation();
			Factory.Save();
			Assert(!transport.JW_RL_NKDiscPortInfo.HasErrors());
			Assert(!transport.JW_RL_NKLoadPortInfo.HasErrors());

			consol.JK_RL_NKLoadPort = "MYBAG";
			consol.RunPreSaveValidation();
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "A routing leg cannot discharge at the consol's first load.");
			consol.JK_RL_NKLoadPort = "GBLON";
			Factory.Save();
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.RunPreSaveValidation();
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "A routing leg cannot load at the consol's final discharge.");
		}

		public void TestSetConsolExportCreditorAddress_CoLoad()
		{
			var coLoad = Factory.New<OrgHeader>();
			coLoad.OH_RL_NKClosestPort = "NZAKL";
			coLoad.OH_IsShippingLine = true;
			coLoad.OH_IsGlobalAccount = true;
			coLoad.OH_Code = "COLOAD";

			var addressMel = coLoad.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Export: Co-Load Address will fallback to Main Office Address",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Export: Co-Load Address will default to the one which UNLOCO matches the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, addressMel.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Export: Co-Load Address will default to the one which country code matches the country code of Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, addressMel.PK);

				var addressSyd = coLoad.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Export: Co-Load Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);

				var addressSyd2 = coLoad.Addresses.AddNew();
				addressSyd2.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd2.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Export: Co-Load Address will fallback to Main Office Address if multiple addresses match the port code of the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);
			}
		}

		public void TestSetConsolImportCreditorAddress_CoLoad()
		{
			var coLoad = Factory.New<OrgHeader>();
			coLoad.OH_RL_NKClosestPort = "NZAKL";
			coLoad.OH_IsShippingLine = true;
			coLoad.OH_IsGlobalAccount = true;
			coLoad.OH_Code = "COLOAD";

			var addressMel = coLoad.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Import: Co-Load Address will fallback to Main Office Address ",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Import: Co-Load Address will default to the one which UNLOCO matches the Consol > Details > Last Discharge",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, addressMel.PK);

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Import: Co-Load Address will default to the one which country code matches the country code of Consol > Details > Last Discharge",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, addressMel.PK);

				var addressSyd = coLoad.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Import: Co-Load Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > Last Discharge",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);

				var addressSyd2 = coLoad.Addresses.AddNew();
				addressSyd2.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd2.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Import: Co-Load Address will fallback to Main Office Address if multiple addresses match the port code of the Consol > Details > Last Discharge",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);
			}
		}

		public void TestSetConsolDomesticCreditorAddress_CoLoad()
		{
			var coLoad = Factory.New<OrgHeader>();
			coLoad.OH_RL_NKClosestPort = "NZAKL";
			coLoad.OH_IsShippingLine = true;
			coLoad.OH_IsGlobalAccount = true;
			coLoad.OH_Code = "COLOAD";

			var addressMel = coLoad.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will fallback to Main Office Address",
					consol.JK_OA_CreditorAddress, coLoad.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will default to the one UNLOCO matches the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress, addressMel.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "AUROS";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will default to the one which country code matches the country code of either Consol > 1st Load/Last Discharge",
					consol.JK_OA_CreditorAddress, addressMel.PK);

				var addressSyd = coLoad.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will fallback to Last Disc if UNLOCO does not matches the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress, addressSyd.PK);

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will default to the one UNLOCO matches the Consol > Details > 1st Load if UNLOCO matches both 1st Load and Last Discharge",
					consol.JK_OA_CreditorAddress, addressSyd.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "AUROS";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load/Last Discharge",
					consol.JK_OA_CreditorAddress, coLoad.MainAddress.PK);

				var addressSyd2 = coLoad.Addresses.AddNew();
				addressSyd2.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd2.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "AUROS";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Domestic: Co-Load Address will fallback to Main Office Address if multiple addresses match the port code of the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress, coLoad.MainAddress.PK);
			}
		}

		public void TestSetConsolUnknownDirectionCreditorAddress_CoLoad()
		{
			var coLoad = Factory.New<OrgHeader>();
			coLoad.OH_RL_NKClosestPort = "NZAKL";
			coLoad.OH_IsShippingLine = true;
			coLoad.OH_IsGlobalAccount = true;
			coLoad.OH_Code = "COLOAD";

			var addressMel = coLoad.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Unknown Direction: Co-Load Address will fallback to Main Office Address",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Unknown Direction: Co-Load Address will default to the one which UNLOCO matches the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, addressMel.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Unknown Direction: Co-Load Address will default to the one which country code matches the country code of Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, addressMel.PK);

				var addressSyd = coLoad.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoad.PK;

				AssertEquals("Unknown Direction: Co-Load Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load",
					consol.JK_OA_CreditorAddress_ZAddress.AddressFK, coLoad.MainAddress.PK);
			}
		}

		public void TestSetConsolExportShippingLineAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "NZAKL";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsGlobalAccount = true;
			carrier.OH_Code = "CARRIER";

			var addressMel = carrier.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Export: Carrier Address will fallback to Main Office Address",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Export: Carrier Address will default to the one which UNLOCO matches the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Export: Carrier Address will default to the one which country code matches the country code of the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				var addressSyd = carrier.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Export: Carrier Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);
			}
		}

		public void TestSetConsolImportShippingLineAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "NZAKL";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsGlobalAccount = true;
			carrier.OH_Code = "CARRIER";

			var addressMel = carrier.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Import: Carrier Address will fallback to Main Office Address",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Import: Carrier Address will default to the one UNLOCO matches the Consol > Details > Last Disc",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Import: Carrier Address will default to the one which country code matches the country code of the Consol > Details > Last Disc",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				var addressSyd = carrier.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Import: Carrier Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);
			}
		}

		public void TestSetConsolDomesticShippingLineAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "NZAKL";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsGlobalAccount = true;
			carrier.OH_Code = "CARRIER";

			var addressMel = carrier.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Domestic: Carrier Address will fallback to Main Office Address",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Domestic: Carrier Address will default to the one UNLOCO matches the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "AUROS";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Domestic: Carrier Address will default to the one which country code matches the country code of either Consol > 1st Load/Last Discharge",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				var addressSyd = carrier.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Domestic: Carrier Address will fallback to Last Disc if UNLOCO does not matches the Consol > Details > Last Disc",
					consol.JK_OA_ShippingLineAddress, addressSyd.PK);

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Domestic: Carrier Address will default to the one UNLOCO matches the Consol > Details > 1st Load if UNLOCO matches both 1st Load and Last Discharge",
					consol.JK_OA_ShippingLineAddress, addressSyd.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "AUROS";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Domestic: Carrier Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load/Last Discharge",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);
			}
		}

		public void TestSetConsolUnknownDirectionShippingLineAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "NZAKL";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsGlobalAccount = true;
			carrier.OH_Code = "CARRIER";

			var addressMel = carrier.Addresses.AddNew();
			addressMel.OA_RL_NKRelatedPortCode = "AUMEL";
			addressMel.OA_IsActive = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			{
				var consol = Factory.New<CommonConsol>();

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Unknown Direction: Carrier Address will fallback to Main Office Address",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);

				addressMel.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUMEL";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Unknown Direction: Carrier Address will default to the one which UNLOCO matches the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Unknown Direction: Carrier Address will default to the one which country code matches the country code of the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, addressMel.PK);

				var addressSyd = carrier.Addresses.AddNew();
				addressSyd.OA_RL_NKRelatedPortCode = "AUSYD";
				addressSyd.OA_IsActive = true;

				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = ZGuid.Empty;
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;

				AssertEquals("Unknown Direction: Carrier Address will fallback to Main Office Address if multiple addresses match the country code of the Consol > Details > 1st Load",
					consol.JK_OA_ShippingLineAddress, carrier.MainAddress.PK);
			}
		}

		public void TestCarrierZAddressBoundToMainAddress()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "AUSYD";
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsGlobalAccount = true;
			carrier.OH_Code = "CARRIER";

			var ausMain = carrier.MainAddress;
			ausMain.OA_RL_NKRelatedPortCode = "AUSYD";
			ausMain.OA_RN_NKCountryCode = Constants.CountryCodes.Australia;
			ausMain.AddAddressType(OrgAddressType.Office);
			ausMain.OA_IsActive = true;
			ausMain.OA_Address1 = "AUS1";
			ausMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			var chinaMain = carrier.Addresses.AddNew();
			chinaMain.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			chinaMain.AddAddressType(OrgAddressType.Office);
			chinaMain.OA_IsActive = true;
			chinaMain.OA_Address1 = "CHINA1";
			chinaMain.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			AssertEquals("Precond: Address is unique main for country", true, ausMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));
			AssertEquals("Precond: Address is unique main for country", true, chinaMain.IsUniqueMainAddressForGlobalOrgOfType(OrgAddressType.Office.Code));

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;
				AssertEquals("Should default to Australia Main when login is AUS", consol.JK_OA_ShippingLineAddress_ZAddress.AddressFK, ausMain.PK);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.China))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrier.PK;
				AssertEquals("Should default to China Main when login is CN", consol.JK_OA_ShippingLineAddress_ZAddress.AddressFK, chinaMain.PK);
			}
		}

		public void TestCusEntryNumbers()
		{
			var consol = Factory.New<CommonConsol>();
			CusEntryNumber num1 = consol.CusEntryNums.AddNew();
			CusEntryNumber num2 = consol.CusEntryNums.AddNew();
			num1.CE_EntryType = "111";
			num1.CE_EntryNum = "222";
			num1.CE_ParentID = consol.PK;
			num1.CE_ParentTable = consol.TableName;
			num2.CE_EntryType = "333";
			num2.CE_EntryNum = "444";
			num2.CE_ParentID = consol.PK;
			num2.CE_ParentTable = consol.TableName;
			num2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Afghanistan;
			Factory.Save();

			consol = new BusinessObjectFactory().Load<CommonConsol>(consol.PK);
			AssertEquals(1, consol.CusEntryNums.Count);
			AssertEquals(2, consol.CusEntryNumsForAllCountries.Count);
		}

		public void TestNumbersAsString()
		{
			var consol = Factory.New<CommonConsol>();
			Factory.Save();

			var cusNumber1 = consol.Numbers.AddNew();
			cusNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "XXXABILL1";
			AssertEquals("AMS: XXXABILL1", consol.NumbersAsString);

			cusNumber1.CE_RN_NKCountryCode = "AU";
			AssertEquals("AMS: XXXABILL1/AU", consol.NumbersAsString);

			var cusNumber2 = consol.Numbers.AddNew();
			cusNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusNumber2.CE_EntryNum = "CV00001";
			cusNumber2.CE_RN_NKCountryCode = "CN";

			AssertEquals("AMS: XXXABILL1/AU, BKG: CV00001/CN", consol.NumbersAsString);
		}

		public void TestFlightNumberSetsCarrierOnTransports()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_IsShippingLine = true;
			org.MiscServ.OM_RM_Airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "QF")).PK;

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_IsShippingLine = true;
			org2.MiscServ.OM_RM_Airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "BA")).PK;

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			AssertEquals(ZGuid.Empty, consol.JK_OA_ShippingLineAddress);
			AssertEquals(ZGuid.Empty, transport1.CarrierPK);
			AssertEquals(ZGuid.Empty, transport2.CarrierPK);

			transport2.JW_VoyageFlight = "QF123";
			AssertEquals(ZGuid.Empty, transport1.CarrierPK);
			AssertEquals(org.PK, transport2.CarrierPK);

			transport1.JW_VoyageFlight = "BA987";
			AssertEquals(org2.PK, transport1.CarrierPK);
			AssertEquals(org.PK, transport2.CarrierPK);
		}

		public void TestDomesticLegDifferentVessel()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUMEL";
			Transport transport1 = consol.Transports[0];
			transport1.JW_Vessel = "Vessel1";
			transport1.JW_VoyageFlight = "23";
			transport1.JW_ETD = new ZDateTime(2008, 1, 1);
			transport1.JW_ETA = new ZDateTime(2008, 1, 15);
			transport1.JW_RL_NKDiscPort = "AUSYD";

			AssertEquals("Vessel with 1 leg", "Vessel1", consol.JK_VesselOfLastImportTransport);
			AssertEquals("Voyage with 1 leg", "23", consol.JK_VoyageOfLastImportTransport);
			AssertEquals("PortOfDischarge with 1 leg", "AUSYD", consol.JK_RL_NKDiscForLastImportTransport);

			RefVessel testVessel2 = RefVessel.New(Factory);
			testVessel2.RV_Name = "TESTVESSEL2";

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "24";
			transport2.JW_Vessel = testVessel2.RV_FK;
			transport2.JW_ETD = transport1.JW_ETA.AddDays(1);
			transport2.JW_ETA = transport1.JW_ETA.AddDays(2);
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_RL_NKLoadPort = transport1.JW_RL_NKDiscPort;

			AssertEquals("Vessel with 1 leg", "TESTVESSEL2", consol.JK_VesselOfLastImportTransport);
			AssertEquals("Voyage with 1 leg", "24", consol.JK_VoyageOfLastImportTransport);
			AssertEquals("PortOfDischarge with 1 leg", "AUMEL", consol.JK_RL_NKDiscForLastImportTransport);
		}

		public void TestImportExportTransports()
		{
			var consol = Factory.New<CommonConsol>();
			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "HKHKG";
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "HKHKG";
			transport2.JW_RL_NKDiscPort = "USCHI";

			AssertNull("No import transport", consol.GetImportTransport(Core.Constants.CountryCodes.Australia));
			AssertNull("No export transport", consol.GetExportTransport(Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("Import Transport", transport1, consol.GetImportTransport(Core.Constants.CountryCodes.HongKong));
			AssertEquals("Export Transport", transport2, consol.GetExportTransport(Core.Constants.CountryCodes.HongKong));
		}

		public void TestAttchToConsol_FallbackATCEventReferenceToPKWhenSaveFailed()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";

			consol.Shipments.Add(shipment);

			var attachLog = shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single();
			using (attachLog.LockForUpdatingKeyFieldsForTesting())
			{
				attachLog.SL_Reference = "ConsolRef|TYP=Consol";

				consol.OnSaved(false);
				shipment.OnSaved(false);

				AssertEquals(string.Format("{0}|TYP=Consol", consol.PK), shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.Attached.Code).Single().SL_Reference);
			}
		}

		public void TestDoNeedRecalculateJK_IsCFS()
		{
			var branch = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			orgHeader.Addresses[0].OA_Address1 = "Test Org header Address";
			orgHeader.Addresses[0].AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			branch.GB_OH_OrgProxy = branch.GB_OH_OrgProxy.IsEmpty ? orgHeader.PK : branch.GB_OH_OrgProxy;

			OrgAddress orgAddress = branch.OrgProxy.Addresses.MainAddress;
			orgAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.OFC));
			orgAddress.OA_Address1 = "Test Address1";

			var commonConsolTest = Factory.NewWithValidTestData<CommonConsolTest>();
			commonConsolTest.JK_IsCFS = false;
			commonConsolTest.JK_RL_NKLoadPort = HomePort;
			commonConsolTest.JK_RL_NKDischargePort = AlternateHomePort;
			commonConsolTest.JK_OA_PackDepotAddress = orgAddress.PK;
			Factory.Save();
			Assert("should recalculate JK_IsCFS to true.", commonConsolTest.JK_IsCFS);
			AssertEquals("should call SetTypeFlag method.", 1, commonConsolTest.SetTypeFlagsMethodCallTimes);

			commonConsolTest.JK_ConsolMode = "SEA";
			Factory.Save();
			AssertEquals("no need to call SetTypeFlag again.", 1, commonConsolTest.SetTypeFlagsMethodCallTimes);

			var commonCFSConsolTest = Factory.NewWithValidTestData<CommonConsolTest>();
			commonCFSConsolTest.JK_IsCFS = true;
			commonCFSConsolTest.JK_RL_NKLoadPort = HomePort;
			commonCFSConsolTest.JK_RL_NKDischargePort = AlternateHomePort;
			commonCFSConsolTest.JK_OA_PackDepotAddress = orgAddress.PK;
			Factory.Save();
			AssertEquals("should not call SetTypeFlag method.", 0, commonCFSConsolTest.SetTypeFlagsMethodCallTimes);
		}

		public void TestTemplateRecordProviderConsol()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";

			var consolType = ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol));
			var consol = (CommonConsol)Factory.NewWithValidTestData(consolType);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";

			AssertNull(consol.TemplateRecordProviderConsol);

			var provider = consol as ITemplateRecordProvider;
			provider.TemplateRecord = templateRecord;
			provider.IsTemplateRecord = false;

			AssertNotNull(consol.TemplateRecordProviderConsol);
		}

		public void TestDefaultCreditorForExportConsolShouldNotUpdateFromCarrierRelatedPartyWhenConsolIsCoLoad()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = OverseasPort2;
			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKLoadPort);

			Factory.Save();

			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("Co-Load should not be updated from carrier", coloadOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForExportConsolUpdatedFromCarrierRelatedParty()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKA";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "NZABY";
			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKLoadPort);

			Factory.Save();

			var parties = carrier.AllRelatedParties;
			var partyRecord = parties.First() as OrgRelatedParty;

			AssertEquals("Preconditions: partyRecord is ENT", "ENT", partyRecord.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Related party is not payable", false, relatedParty.OH_IsCreditor);
			AssertEquals("carrier is not payable", false, carrier.OH_IsCreditor);

			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // Clear carrier
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // Clear carrier
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty1.OH_IsCreditor = true;

			var partyRecord1 = parties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord1.PR_OH_RelatedParty = relatedParty1.PK;
			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = transport.JW_RL_NKLoadPort;

			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Pickup direction overtakes Pickup And Delivery when Transport/Container/Location/Company are the same", relatedParty1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord1.PR_FreightTransportMode = Constants.TransportModes.All;

			AssertEquals("Preconditions: partyRecord is SEA", "SEA", partyRecord.PR_FreightTransportMode);
			AssertEquals("Preconditions: partyRecord1 is ALL", "ALL", partyRecord1.PR_FreightTransportMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Transport SEA  overtakes ALL", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;

			partyRecord.PR_FreightContainerMode = "";
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;

			AssertEquals("Preconditions: partyRecord Container mode is balnk", "", partyRecord.PR_FreightContainerMode);
			AssertEquals("Preconditions: partyRecord1 Container mode is FCL", "FCL", partyRecord1.PR_FreightContainerMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Container mode FCL overtakes blank", relatedParty1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is NZABY", "NZABY", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is blank", "", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Location Port overtakes blank", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord1.PR_Location = "NZ";

			AssertEquals("Preconditions: partyRecord Location is NZABY", "NZABY", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is NZ", "NZ", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Location Port overtakes Country", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is blank", "", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is NZ", "NZ", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Location Country overtakes blank", relatedParty1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.CompanyLevel = "COM";
			partyRecord1.CompanyLevel = "ENT";

			AssertEquals("Preconditions: partyRecord Company Level is COM", "COM", partyRecord.CompanyLevel);
			AssertEquals("Preconditions: partyRecord1 Company Level is ENT", "ENT", partyRecord1.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Company level overtakes ENT", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			carrier.OH_IsCreditor = false; // No fall back to carrier

			Factory.Save();

			AssertEquals("Preconditions: partyRecord Direction is Delivery", "DLV", partyRecord.PR_FreightDirection);
			AssertEquals("Preconditions: partyRecord1 Direction is Delivery", "DLV", partyRecord1.PR_FreightDirection);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("For Export consol, should igonre Delivery parties ", ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForExportConsolUpdatedFromPayableCarrier_WhenReceivingAgentIsPayableAndConsolIsCollect()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsCreditor = false; // Not Payable
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("Receiving and creditor are not payable ", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Creditor is payable, we set creditor to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			receivingForwarder.OH_IsCreditor = true; // Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Even receiving is payable, we set creditor to to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForExportConsolUpdatedFromOrganizationCarrierIfItIsPayableAndConsolIsPrepiad()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false; // Not Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Carrier is not payable", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true; // Payable
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("We set creditor to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForImportConsolUpdatedFromCarrierRelatedParty()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKDiscPort);

			Factory.Save();

			var parties = carrier.AllRelatedParties;
			var partyRecord = parties.First() as OrgRelatedParty;

			AssertEquals("Preconditions: partyRecord is ENT", "ENT", partyRecord.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Related party is not payable", false, relatedParty.OH_IsCreditor);
			AssertEquals("Carrier is not payable", false, carrier.OH_IsCreditor);
			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // Clear carrier
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // Clear carrier
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty1.OH_IsCreditor = true;

			var partyRecord1 = parties.AddNew();
			partyRecord1.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord1.PR_OH_RelatedParty = relatedParty1.PK;
			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = transport.JW_RL_NKDiscPort;

			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Delivery direction overtakes Pickup And Delivery when Transport/Container/Location/Company are the same", relatedParty1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord1.PR_FreightTransportMode = Constants.TransportModes.All;

			AssertEquals("Preconditions: partyRecord is SEA", "SEA", partyRecord.PR_FreightTransportMode);
			AssertEquals("Preconditions: partyRecord1 is ALL", "ALL", partyRecord1.PR_FreightTransportMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Transport SEA  overtakes ALL", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord1.PR_FreightTransportMode = consol.JK_TransportMode;

			partyRecord.PR_FreightContainerMode = "";
			partyRecord1.PR_FreightContainerMode = consol.JK_ConsolMode;

			AssertEquals("Preconditions: partyRecord Container mode is balnk", "", partyRecord.PR_FreightContainerMode);
			AssertEquals("Preconditions: partyRecord1 Container mode is FCL", "FCL", partyRecord1.PR_FreightContainerMode);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Container mode FCL overtakes blank", relatedParty1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord1.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is AUMEL", "AUMEL", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is blank", "", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Location Port overtakes blank", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord1.PR_Location = "AU";

			AssertEquals("Preconditions: partyRecord Location is AUMEL", "AUMEL", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is AU", "AU", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Location Port overtakes Country", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.PR_Location = "";

			AssertEquals("Preconditions: partyRecord Location is blank", "", partyRecord.PR_Location);
			AssertEquals("Preconditions: partyRecord1 Location is AU", "AU", partyRecord1.PR_Location);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Location Country overtakes blank", relatedParty1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.CompanyLevel = "COM";
			partyRecord1.CompanyLevel = "ENT";

			AssertEquals("Preconditions: partyRecord Company Level is COM", "COM", partyRecord.CompanyLevel);
			AssertEquals("Preconditions: partyRecord1 Company Level is ENT", "ENT", partyRecord1.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Company level overtakes ENT", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			partyRecord1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			carrier.OH_IsCreditor = false; // No fall back to carrier

			Factory.Save();

			AssertEquals("Preconditions: partyRecord Direction is Pickup", "PIC", partyRecord.PR_FreightDirection);
			AssertEquals("Preconditions: partyRecord1 Direction is Pickup", "PIC", partyRecord1.PR_FreightDirection);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("For Import consol, should igonre Pickup parties ", ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForImportConsolUpdatedFromPayableCarreirWhenSendingAgentIsPayableAndConsolIsPrepaid()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = false; // Not Payable
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("Sending and creditor are not payable ", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Creditor is payable, we set creditor to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			sendingForwarder.OH_IsCreditor = true; // Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Even sending is payable,  we set creditor to to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForImportConsolUpdatedFromOrganizationCarrierIfItIsPayableAndConsolIsCollect()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false; // Not Payable

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Carrier is not payable", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true; // Payable
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("We set creditor to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorDoesNotChangeWhenChangingPaymentMode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, consol.JK_RL_NKDischargePort);

			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Related party is not payable", false, relatedParty.OH_IsCreditor);
			AssertEquals("Carrier is not payable", false, carrier.OH_IsCreditor);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		#region Default Creditor For Domestic Consol

		public void TestDefaultCreditorForDomesticConsolNonCoload_UpdatedFromCarrierRelatedParty()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = AlternateHomePort2;
			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Non-CLD mode.", false, consol.IsCoLoad);

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKLoadPort);

			Factory.Save();

			var parties = carrier.AllRelatedParties;
			var partyRecord = parties.First() as OrgRelatedParty;

			AssertEquals("Preconditions: partyRecord is ENT", "ENT", partyRecord.CompanyLevel);

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Related party is not payable", false, relatedParty.OH_IsCreditor);
			AssertEquals("carrier is not payable", false, carrier.OH_IsCreditor);

			AssertEquals("Carrier and its related party are not payable, cannot set creditor", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // Clear carrier
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Set Creditor from Carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty; // Clear carrier
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Set Creditor from Carrier's related party", relatedParty.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorForDomesticConsolNonCoload_WhenNoMatchingCarrierRelatedParty()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Non-CLD mode.", false, consol.IsCoLoad);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = false;

			AssertEquals("Preconditions: Carrier doesn't have related party.", true, carrier.AllRelatedParties.IsNullOrEmpty());

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("carrier is not payable, creditor should be empty ", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Carrier is payable, we set creditor to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		#endregion

		#region Default Creditor For Cross Trade Consol

		CommonConsol GetCrossTradeConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_RL_NKLoadPort = OverseasPort2;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			AssertEquals("Preconditions: Cross Trade Consol expected.", true, consol.IsCrossTrade());
			AssertEquals("Preconditions: Non-CoLoad mode.", false, consol.IsCoLoad);

			return consol;
		}

		public void TestNonCoLoadCrossTrade_WhenCarrierChange_WithMatchingRelatedParty_UpdateCreditor()
		{
			var consol = GetCrossTradeConsol();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = AlternateHomePort2;

			carrier.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.Delivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode
				, transport.JW_RL_NKDiscPort);

			Factory.Save();

			var parties = carrier.AllRelatedParties;
			var partyRecord = parties.First() as OrgRelatedParty;

			AssertEquals("Preconditions: partyRecord is ENT", "ENT", partyRecord.CompanyLevel);
			TestNonCoLoadCrossTradeCarrier(consol, carrier, false);

			AssertEquals("Related party is not payable", false, relatedParty.OH_IsCreditor);
			TestNonCoLoadCrossTradeCarrier(consol, carrier, true);

			relatedParty.OH_IsCreditor = true;
			TestNonCoLoadCrossTradeCarrier(consol, carrier, true);
		}

		public void TestNonCoLoadCrossTrade_WhenCarrierChange_WithNoMatchingRelatedParty_UpdateCreditor()
		{
			var consol = GetCrossTradeConsol();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Preconditions: Carrier doesn't have related party.", true, carrier.AllRelatedParties.IsNullOrEmpty());

			TestNonCoLoadCrossTradeCarrier(consol, carrier, false);
			TestNonCoLoadCrossTradeCarrier(consol, carrier, true);
		}

		void TestNonCoLoadCrossTradeCarrier(CommonConsol consol, OrgHeader carrier, bool isCreditor)
		{
			carrier.OH_IsCreditor = isCreditor;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			Factory.Save();

			if (isCreditor)
			{
				AssertEquals("Carrier is payable, we set creditor to payable carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
			}
			else
			{
				AssertEquals("carrier is not payable, creditor should be empty ", ZGuid.Empty, consol.JK_OA_CreditorAddress);
			}
		}

		#endregion

		class CommonConsolTest : CommonConsol
		{
			public CommonConsolTest(BusinessObjectFactory factory, DataRow dataRow)
				: base(factory, dataRow)
			{
			}

			public int SetTypeFlagsMethodCallTimes { get; set; }

			protected override void SetTypeFlagsCore()
			{
				base.SetTypeFlagsCore();
				SetTypeFlagsMethodCallTimes++;
			}
		}

		#region Domestic Freight

		public void TestDomesticSetFromDepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Domestic = true;
			var consol = Factory.New<CommonConsol>();
			AssertEquals(true, consol.IsDomesticFreight);

			GlbDepartment.CurrentDepartment.GE_Domestic = false;
			consol = Factory.New<CommonConsol>();
			Assert(!consol.IsDomesticFreight);
		}

		public void TestDomesticFreightSetsPorts()
		{
			var consol = Factory.New<CommonConsol>();
			consol.IsDomesticFreight = true;
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, consol.JK_RL_NKLoadPort);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, consol.JK_RL_NKDischargePort);
		}

		public void TestDomesticSetAutomaticallyWhenSettingPorts()
		{
			var consol = Factory.New<CommonConsol>();
			consol.IsDomesticFreight = false;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			Assert(!consol.IsDomesticFreight);

			consol.JK_RL_NKDischargePort = "AUBNE";
			Assert(consol.IsDomesticFreight);

			consol.JK_RL_NKDischargePort = "USLAX";
			Assert(!consol.IsDomesticFreight);

			consol.JK_RL_NKDischargePort = "AUMEL";
			Assert(consol.IsDomesticFreight);

			consol.JK_RL_NKLoadPort = "USLAX";
			Assert(!consol.IsDomesticFreight);
		}

		public void TestIsDomesticFreightDoesNotHaveChangesAfterFirstLoad()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";

			Factory.Save();

			Assert(consol.IsDomesticFreight);
			Assert(!consol.HasChanges);

			consol = new BusinessObjectFactory().Load<CommonConsol>(consol.PK);
			Assert(consol.IsDomesticFreight);
			Assert(!consol.HasChanges);
		}

		#endregion

		#region TestJK_RL_NKLoadForFirstImportTransport
		public void TestPortsOnFirstOrLastImportTransport()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 0;
			transport1.JW_RL_NKLoadPort = OverseasPort;
			AssertEquals(OverseasPort, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForLastImportTransport);

			transport1.JW_RL_NKDiscPort = HomePort;
			AssertEquals(OverseasPort, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForLastImportTransport);

			transport1.JW_RL_NKDiscPort = AlternateHomePort;
			AssertEquals(OverseasPort, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(AlternateHomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(AlternateHomePort, consol.JK_RL_NKDiscForLastImportTransport);

			transport1.JW_RL_NKDiscPort = OverseasPort2;
			AssertEquals(OverseasPort, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(OverseasPort2, consol.JK_RL_NKDiscForLastImportTransport);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_RL_NKLoadPort = OverseasPort2;
			transport2.JW_RL_NKDiscPort = HomePort;
			AssertEquals(OverseasPort2, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForLastImportTransport);

			transport1.JW_RL_NKDiscPort = "";
			AssertEquals(OverseasPort2, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForLastImportTransport);

			transport2.JW_RL_NKLoadPort = AlternateHomePort;
			AssertEquals(OverseasPort, consol.JK_RL_NKLoadForFirstImportTransport);
			AssertEquals(AlternateHomePort, consol.JK_RL_NKDiscForFirstImportTransport);
			AssertEquals(HomePort, consol.JK_RL_NKDiscForLastImportTransport);
		}
		#endregion

		#region TestJK_CTOReceivalCommences

		public void TestProxyLCLFCLDatesFromCorrectTransport()
		{
			DateTime day = DateTime.Today;

			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport1 = consol.Transports[0];
			transport1.JW_IsLinked = true;
			transport1.JW_LegOrder = 0;
			transport1.JW_DocumentaryCutOff = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_DepotReceivalCommences = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_TerminalReceivalCommences = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_DepotCutOff = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_TerminalCutOff = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_ETD = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_ETA = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_TerminalAvailabilityDate = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_DepotAvailabilityDate = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_TerminalStorageDate = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_DepotStorageDate = day += new TimeSpan(1, 0, 0, 0);
			transport1.JW_VGMCutOff = day += new TimeSpan(1, 0, 0, 0);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_LegOrder = 1;
			transport2.JW_DocumentaryCutOff = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_DepotReceivalCommences = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_TerminalReceivalCommences = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_DepotCutOff = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_TerminalCutOff = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_ETD = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_ETA = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_TerminalAvailabilityDate = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_DepotAvailabilityDate = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_TerminalStorageDate = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_DepotStorageDate = day += new TimeSpan(1, 0, 0, 0);
			transport2.JW_VGMCutOff = day += new TimeSpan(2, 0, 0, 0);

			AssertEquals("Docs Cut Off", transport1.JW_DocumentaryCutOff, consol.JK_DocsCutOff);
			AssertEquals("CFS Receive Start", transport1.JW_DepotReceivalCommences, consol.JK_DepotReceivalCommences);
			AssertEquals("CFS Receive", transport1.JW_TerminalReceivalCommences, consol.JK_CTOReceivalCommences);
			AssertEquals("CFS Cut Off", transport1.JW_DepotCutOff, consol.JK_DepotCutOff);
			AssertEquals("CFS Cut Off", transport1.JW_TerminalCutOff, consol.JK_CTOCutOff);
			AssertEquals("VGM Cut Off", transport1.JW_VGMCutOff, consol.JK_VGMCutOff);

			AssertEquals("CTO Available", transport2.JW_TerminalAvailabilityDate, consol.JK_CTOAvailabilityDate);
			AssertEquals("CFS Available", transport2.JW_DepotAvailabilityDate, consol.JK_DepotAvailabilityDate);
			AssertEquals("CTO Storage Start", transport2.JW_TerminalStorageDate, consol.JK_CTOStorageDate);
			AssertEquals("CFS Storage Start", transport2.JW_DepotStorageDate, consol.JK_DepotStorageDate);
		}

		#endregion

		#region TestJK_ConsolCutOffDateLocal

		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestJK_ConsolCutOffDateLocal()
		{
			ZDateTime testTimeUTC = ZDateTime.UtcNow;
			ZDateTime testTimeBNE = Env.Time.GetLocalTimeFromUtc(testTimeUTC.ToDateTime());

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_ConsolCutOffDate = testTimeUTC;

			AssertEquals("UTC converted to local (BNE)", testTimeBNE, consol.JK_ConsolCutOffDateLocal);

			consol = Factory.New<CommonConsol>();
			consol.JK_ConsolCutOffDateLocal = testTimeBNE;

			AssertEquals("Local (BNE) converted to UTC", testTimeUTC, consol.JK_ConsolCutOffDate);

			TestTimeZoneUNLOCOAttribute.UNLOCO = "USCHI";

			ZDateTime safeTestTimeUTC = new ZDateTime(testTimeUTC.Year, 7, 15, 18, 45, 31);
			ZDateTime safeTestTimeCHI = Env.Time.GetLocalTimeFromUtc(safeTestTimeUTC.ToDateTime());

			consol.JK_ConsolCutOffDate = safeTestTimeUTC;

			AssertEquals("UTC converted to local (CHI)", safeTestTimeCHI, consol.JK_ConsolCutOffDateLocal);

			consol = Factory.New<CommonConsol>();
			consol.JK_ConsolCutOffDateLocal = safeTestTimeCHI;

			AssertEquals("Local (CHI) " + safeTestTimeCHI + " converted to UTC", safeTestTimeUTC, consol.JK_ConsolCutOffDate);
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestConsolCutOffDateLocalSetter_AnyValue_GeneratePOFEvent()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var localCutOffDate = ZDateTime.Today;
			consol.JK_ConsolCutOffDateLocal = localCutOffDate;

			var workflowProvider = (IWorkflowProvider)consol;
			var milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.PreAllocationCutOffDate.Code;

			AssertEquals("Milestones EstimatedDate should be defaulted to current CutOffDate", localCutOffDate, milestone.P9_ScheduledDate.ToZDateTime());

			consol.JK_ConsolCutOffDateLocal = localCutOffDate.AddDays(4);
			AssertEquals("Milestones EstimatedDate is updated", localCutOffDate.AddDays(4), milestone.P9_ScheduledDate.ToZDateTime());

			consol.JK_ConsolCutOffDateLocal = ZDateTime.Empty;
			AssertEquals("Milestones EstimatedDate should be cleared", ZDateTime.Empty, milestone.P9_ScheduledDate.ToZDateTime());

			var utcCutOffDate = ZDateTime.Today;
			localCutOffDate = Env.Time.GetLocalTimeFromUtc(utcCutOffDate.ToDateTime());
			consol.JK_ConsolCutOffDate = utcCutOffDate;

			AssertEquals("Local Consol Cut Off Date has been updated to the local offset time", localCutOffDate, consol.JK_ConsolCutOffDateLocal);
			AssertEquals("Milestones EstimatedDate is updated to match offset time", localCutOffDate, milestone.P9_ScheduledDate.ToZDateTime());

			consol.JK_ConsolCutOffDate = ZDateTime.Empty;
			AssertEquals("Local Consol Cut Off Date should be cleared as well", ZDateTime.Empty, consol.JK_ConsolCutOffDateLocal);
			AssertEquals("Milestones EstimatedDate should be cleared", ZDateTime.Empty, milestone.P9_ScheduledDate.ToZDateTime());
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestConsolCutOffDateSetterShouldRefreshOldEventInDb()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			Factory.Save();

			AssertEquals("No event", 0, FindLogs((IStmALogParent)consol.Logs.Parent).Length);

			var createdLog = consol.Logs.AddNew(AutoEvents.PreAllocationCutOffDate, new ZDateTimeOffset(2014, 4, 30), true);

			AssertEquals("SL_SE_NKEvent of new log", AutoEvents.PreAllocationCutOffDate.Code, createdLog.SL_SE_NKEvent);
			AssertEquals("SL_EventTime of new log", new ZDateTime(2014, 4, 30), createdLog.SL_EventTime);
			AssertEquals("SL_IsEstimate of new log", true, createdLog.SL_IsEstimate);

			Factory.Save();

			AssertEquals("Add a new event", 1, FindLogs((IStmALogParent)consol.Logs.Parent).Length);

			consol.JK_ConsolCutOffDateLocal = new ZDateTime(2014, 4, 29);
			Factory.Save();

			AssertEquals("Should refresh old event, NOT add new one", 1, FindLogs((IStmALogParent)consol.Logs.Parent).Length);

			consol.JK_ConsolCutOffDate = new ZDateTime(2014, 4, 28);
			Factory.Save();

			AssertEquals("Should refresh old event, NOT add new one", 1, FindLogs((IStmALogParent)consol.Logs.Parent).Length);
		}

		StmALog[] FindLogs(IStmALogParent parent)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, parent.LogsParentPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.PreAllocationCutOffDate.Code);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, true);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);

			return Factory.Load<StmALog>(query);
		}

		#endregion

		#region TestNewConsolsHaveNoChanges

		public void TestNewConsolsHaveNoChanges()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals(false, consol.HasChanges);
		}

		#endregion

		#region TestDontRemoveSpacesFromMasterBillsAsThisCanBreakCustomsMessaging

		public void TestDontRemoveSpacesFromMasterBillsAsThisCanBreakCustomsMessaging()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "Blah Blah-Blah";
			AssertEquals("Blah Blah-Blah", consol.JK_MasterBillNum);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "Blah Blah-Blah";
			AssertEquals("BlahBlahBlah", consol.JK_MasterBillNum);
		}

		#endregion

		#region TestSameVoyageValidatesAndSavesCorrectlyWhenCreatedOnDifferentFactories

		public void TestSameVoyageValidatesAndSavesCorrectlyWhenCreatedOnDifferentFactories()
		{
			var creationFactory = new BusinessObjectFactory();
			var carrier = creationFactory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			creationFactory.Save();

			BusinessObjectFactory factoryX = new BusinessObjectFactory();
			BusinessObjectFactory factoryY = new BusinessObjectFactory();

			CommonConsol consolOnX = SetupConsolWithSameNewSailing(factoryX, carrier);
			CommonConsol consolOnY = SetupConsolWithSameNewSailing(factoryY, carrier);

			AssertCorrectValidation(consolOnY);

			factoryY.Save();
			AssertCorrectValidation(consolOnX);
		}

		void AssertCorrectValidation(CommonConsol consol)
		{
			consol.RunPreSaveValidation();
			AssertNoWarnings("There should be no warnings on this save", consol);
			AssertNoErrors("There should be no errors on this save", consol);
		}

		CommonConsol SetupConsolWithSameNewSailing(BusinessObjectFactory factory, OrgHeader carrier)
		{
			ZDateTime departure = ZDateTime.Today;
			ZDateTime arrival = ZDateTime.Today.AddDays(5);

			CommonConsol consol = factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "TEST1";
			transport.CarrierPK = carrier.PK;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_ETD = departure;
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_ETA = arrival;
			transport.CreditorPK = ZGuid.Empty;

			return consol;
		}

		#endregion

		#region TestFlightScheduleSelectsCorrectFlight

		public void TestFlightScheduleSelectsCorrectFlight()
		{
			ZDateTime today = ZDateTime.Today;

			JobVoyage v1 = Factory.New<JobVoyage>();
			v1.JV_AirSeaRoad = Constants.TransportModes.Air;
			v1.JV_VoyageFlight = "QF8332";
			VoyageOrigin o1 = v1.Origins.AddNew();
			o1.JA_RL_NKPortOfLoading = OverseasPort;
			o1.JA_E_DEP = today.AddDays(-1);
			VoyageDestination d1 = v1.Destinations.AddNew();
			d1.JB_RL_NKPortOfDischarge = HomePort;
			d1.JB_E_ARV = today;
			v1.GenerateSailings();

			JobSailing flight1 = v1.Sailings[0];

			JobVoyage v2 = Factory.New<JobVoyage>();
			v2.JV_AirSeaRoad = Constants.TransportModes.Air;
			v2.JV_VoyageFlight = "QF8332";
			VoyageOrigin o2 = v2.Origins.AddNew();
			o2.JA_RL_NKPortOfLoading = OverseasPort;
			o2.JA_E_DEP = today;
			VoyageDestination d2 = v2.Destinations.AddNew();
			d2.JB_RL_NKPortOfDischarge = HomePort;
			d2.JB_E_ARV = today.AddDays(1);
			v2.GenerateSailings();

			JobSailing flight2 = v2.Sailings[0];

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = OverseasPort;
			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_VoyageFlight = "QF8332";
			transport.JW_ETA = today;

			AssertEquals("Consol's Schedule should be flight 1", flight1.PK, transport.JW_JX);
		}

		#endregion

		#region TestEnteringAnInvalidArivalDateWhenTheFlightDateIsSetDontGoBoom

		[ExpectNoExceptions]
		public void TestEnteringAnInvalidArivalDateWhenTheFlightDateIsSetDontGoBoom()
		{
			ExportSailing1.Voyage.JV_FlightDate = ZDateTime.Today.AddDays(2);
			CommonConsol consol = GetConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ExportSailing1.PK;
			transport.JW_ETA = ZDateTime.Invalid;
		}

		#endregion

		#region TestEnteringAnInvalidDeparturelDateWhenTheFlightDateIsSetDontGoBoom

		[ExpectNoExceptions]
		public void TestEnteringAnInvalidDeparturelDateWhenTheFlightDateIsSetDontGoBoom()
		{
			ExportSailing1.Voyage.JV_FlightDate = ZDateTime.Today.AddDays(2);
			CommonConsol consol = GetConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ExportSailing1.PK;
			transport.JW_ETD = ZDateTime.Invalid;
		}

		#endregion

		#region IncludeLogsFromTransports

		public void IncludeLogsFromTransports()
		{
			CommonConsol consol = GetConsol();
			Transport transport = consol.Transports[0];

			int transportFound = 0;

			foreach (BusinessObject bO in consol.BusinessObjectsWithRelatedEvents)
			{
				if (bO.PK == transport.PK)
				{
					AssertEquals(transport.GetType(), bO.GetType());
					transportFound++;
				}
			}

			AssertEquals("Should have found the transport exactly once", 1, transportFound);
		}

		#endregion

		#region TestScheduleWhenConsolDeleted

		public void TestScheduleWhenConsolDeleted()
		{
			TestBaseConsol consol = Factory.New<TestBaseConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort3;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "1111";
			Assert("Schedule should not be null", transport.Sailing != null);
			Factory.Save();

			TestBaseConsol consol2 = Factory.New<TestBaseConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = HomePort;
			transport2.JW_RL_NKDiscPort = OverseasPort3;
			transport2.JW_Vessel = TestVessel1.RV_FK;
			transport2.JW_VoyageFlight = "1111";
			AssertEquals("Consol & Consol2 should refer to the same Schedule", consol.Schedule.PK, consol2.Schedule.PK);

			consol2.Delete();
			var schedule = Factory.Load<JobSailing>(consol.Schedule.PK);
			Assert("Schedule should not be deleted", schedule != null);

			ZGuid schedulePK = consol.Schedule.PK;
			consol.Delete();
			schedule = Factory.Load<JobSailing>(schedulePK);
			Assert("Schedule should not be deleted", schedule != null);
		}

		#endregion

		#region TestOtherInstancesOfConsolGetRefreshedWhenScheduleFieldsChange

		public void TestOtherInstancesOfConsolGetRefreshedWhenScheduleFieldsChange()
		{
			TestBaseConsol consol = Factory.New<TestBaseConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_VoyageFlight = "QF111";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol consol2 = newFactory.Load<TestBaseConsol>(consol.PK);
			Transport transport2 = consol2.Transports[0];

			AssertEquals("Flight No", transport.JW_VoyageFlight, transport2.JW_VoyageFlight);

			transport.JW_VoyageFlight = "QF222";
			Factory.Save();

			AssertEquals("Flight No", transport.JW_VoyageFlight, transport.JW_VoyageFlight);
		}

		#endregion

		#region TestChangingFlightNoAfterFlightHasBeenSetUp

		public void TestChangingFlightNoAfterFlightHasBeenSetUp()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = AlternateHomePort;
			CommonConsol airConsol = Factory.New<CommonConsol>();
			airConsol.JK_TransportMode = Constants.TransportModes.Air;

			Transport airTransport = airConsol.Transports[0];
			airTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
			airTransport.JW_RL_NKLoadPort = AlternateHomePort;
			airTransport.JW_RL_NKDiscPort = OverseasPort;
			airTransport.JW_ETD = ZDateTime.Now;
			airTransport.JW_VoyageFlight = "QF342";

			AssertNotNull(airConsol.Schedule);
			Assert(!airTransport.JW_JX.IsEmpty);

			ZGuid firstSchedule = airConsol.Schedule.PK;

			airTransport.JW_VoyageFlight = "";
			AssertNull(airConsol.Schedule);
			Assert(airTransport.JW_JX.IsEmpty);

			airTransport.JW_VoyageFlight = "PS322";

			AssertNotNull(airConsol.Schedule);
			Assert(!airTransport.JW_JX.IsEmpty);

			ZGuid secondSchedule = airConsol.Schedule.PK;
			Assert(firstSchedule != secondSchedule);
		}

		#endregion

		#region TestMultipleSailingsPerVoyage

		public void TestMultipleSailingsPerVoyage()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = HomePort;

			var transport = consol.Transports[0];
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "38234920";
			transport.JW_RL_NKLoadPort = OverseasPort;
			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_ETA = ZDateTime.Today;

			Factory.Save();

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = HomePort;

			var transport2 = consol2.Transports[0];
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_Vessel = TestVessel1.RV_FK;
			transport2.JW_VoyageFlight = "38234920";
			transport2.JW_RL_NKLoadPort = HomePort;
			transport2.JW_RL_NKDiscPort = OverseasPort3;
			transport2.JW_ETD = ZDateTime.Today;

			AssertNoErrors("Not expecting Consol2 to have errors.", consol2);
		}

		#endregion

		#region TestLoadAndDischargePortsAreAutoCreatedWhenCreatedFromConsol

		public void TestLoadAndDischargePortsAreAutoCreatedWhenCreatedFromConsol()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "22022005";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_ETD = ZDateTime.Today;

			Factory.Save();

			Assert("Expecting VoyageOrigin on consol to set to auto created.", consol.Schedule.Origin.JA_AutoCreated);
			Assert("Expecting VoyageDestination on consol to set to auto created.", consol.Schedule.Destination.JB_AutoCreated);
		}

		#endregion

		#region TestPrintColoadsOnManifest

		public void TestPrintColoadsOnManifest()
		{
			CommonConsol consol = GetConsol();

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(false, consol.PrintColoadsOnManifest);

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(true, consol.PrintColoadsOnManifest);

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(true, consol.PrintColoadsOnManifest);
		}

		#endregion

		#region TestPrintColoadsOnOtherDocs

		public void TestPrintColoadsOnOtherDocs()
		{
			CommonConsol consol = GetConsol();

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(false, consol.PrintColoadsOnOtherDocs);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(true, consol.PrintColoadsOnOtherDocs);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(true, consol.PrintColoadsOnOtherDocs);
		}

		#endregion

		#region TestPrintMastersOnManifest

		public void TestPrintMastersOnManifest()
		{
			CommonConsol consol = GetConsol();

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(false, consol.PrintMastersOnManifest);

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(true, consol.PrintMastersOnManifest);

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(true, consol.PrintMastersOnManifest);
		}

		#endregion

		#region TestPrintMastersOnOtherDocs

		public void TestPrintMastersOnOtherDocs()
		{
			CommonConsol consol = GetConsol();

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(false, consol.PrintMastersOnOtherDocs);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			AssertEquals(true, consol.PrintMastersOnOtherDocs);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(true, consol.PrintMastersOnOtherDocs);
		}

		#endregion

		#region ISupportDataImporting

		public void TestSettingJK_TransportModeDoesntSetAgentType()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = "";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Shouldnt change agent type", "", consol.JK_AgentType);
		}

		#endregion

		#region Validate Charters

		public void TestValidateAirCharters()
		{
			CommonConsol consol = GetConsol();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Charter;

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsCharter = true;
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort2;
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now.AddHours(20);
			transport.JW_VoyageFlight = "QF9";
			transport.JW_JX_JV_RegistrationNo = "AIRREG";

			AssertNoErrors("Not expecting aircraftregistration to have errors", transport.JW_JX_JV_RegistrationNoInfo);
			AssertNotNull("Expecting consol to have a schedule", transport.Sailing);
			AssertEquals("Expecting charter to be saved on the schedule.", "QF9", transport.Sailing.Voyage.JV_VoyageFlight);

			transport.JW_VoyageFlight = "3842342";
			AssertEquals("Expecting charter to be saved on the schedule.", "3842342", transport.Sailing.Voyage.JV_VoyageFlight);
			AssertEquals("Expecting voyage transport mode to be air", Constants.TransportModes.Air, transport.Sailing.Voyage.JV_AirSeaRoad);
			AssertEquals("Expecting voyage transport to be chartered", true, transport.Sailing.Voyage.IsCharter);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_AgentType = Constants.AgentType.Charter;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_IsCharter = true;
			transport2.JW_RL_NKLoadPort = HomePort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;
			transport2.JW_ETD = ZDateTime.Now;
			transport2.JW_ETA = ZDateTime.Now.AddHours(20);
			transport2.JW_VoyageFlight = "1235222";
			transport2.JW_JX_JV_RegistrationNo = "999AI88";

			AssertNoErrors("Not expecting aircraftregistration to have errors", transport.JW_JX_JV_RegistrationNoInfo);

			AssertNotNull("Expecting consol to have a schedule", transport2.Sailing);
			AssertEquals("Expecting charter to be saved on the schedule.", "1235222", transport2.Sailing.Voyage.JV_VoyageFlight);

			transport2.JW_VoyageFlight = "CN042";
			AssertEquals("Expecting charter to be saved on the schedule.", "CN042", transport2.Sailing.Voyage.JV_VoyageFlight);
			AssertEquals("Expecting voyage transport mode to be chartered air", Constants.TransportModes.Air, transport2.Sailing.Voyage.JV_AirSeaRoad);
			AssertEquals("Expecting voyage transport to be chartered", true, transport2.Sailing.Voyage.IsCharter);
		}

		public void TestValidateSeaCharters()
		{
			CommonConsol consol = GetConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Charter;

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsCharter = true;
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort2;
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now.AddHours(20);
			transport.JW_VoyageFlight = "482";
			transport.JW_Vessel = "APL IVORY";

			AssertNotNull("Expecting consol to have a schedule", consol.Schedule);
			AssertEquals("Expecting voyage transport mode to be chartered sea", Constants.TransportModes.Sea, consol.Schedule.Voyage.JV_AirSeaRoad);
			AssertEquals("Expecting voyage to be chartered", true, transport.Sailing.Voyage.IsCharter);
		}

		#endregion

		#region TestIsGoingVia
		public void TestIsGoingVia()
		{
			CommonConsol consol = GetConsol();
			RefCountry uSCountry = null;
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(""));
			uSCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
			consol.JK_RL_NKDischargePort = "";
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));

			consol.JK_RL_NKFirstForeignPort = "USLAX";
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
			consol.JK_RL_NKFirstForeignPort = "";
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));

			consol.JK_RL_NKLastForeignPort = "USLAX";
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
			consol.JK_RL_NKLastForeignPort = "";
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));

			consol.JK_RL_NKPortOfFirstArrival = "USLAX";
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
			consol.JK_RL_NKPortOfFirstArrival = "";
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));

			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", true, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "";
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));

			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "USCHI";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(uSCountry));
			AssertEquals("IsGoingVia US", false, consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates));
		}
		#endregion

		#region TestConsolPassesThroughCountry

		public void TestConsolPassesThroughCountry()
		{
			CommonConsol consol = GetConsol();

			Transport originalTransport = consol.Transports[0];
			Transport transport1 = consol.Transports.AddNew();
			Transport transport2 = consol.Transports.AddNew();
			originalTransport.JW_RL_NKLoadPort = "AUSYD";
			originalTransport.JW_RL_NKDiscPort = "NZAKL";
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "CACAD";

			AssertEquals("Consol passes through AU", true, consol.ConsolPassesThroughCountry("AU"));
			AssertEquals("Consol passes through NZ", true, consol.ConsolPassesThroughCountry("NZ"));
			AssertEquals("Consol passes through US", true, consol.ConsolPassesThroughCountry("US"));
			AssertEquals("Consol passes through CA", true, consol.ConsolPassesThroughCountry("CA"));
			AssertEquals("Consol passes through ZA", false, consol.ConsolPassesThroughCountry("ZA"));

			consol.JK_RL_NKDischargePort = "ZAAAM";
			AssertEquals("Consol passes through ZA", true, consol.ConsolPassesThroughCountry("ZA"));

			consol.JK_RL_NKDischargePort = "CACAD";
			consol.JK_RL_NKLoadPort = "ZAAAM";
			AssertEquals("Consol passes through ZA", true, consol.ConsolPassesThroughCountry("ZA"));
		}

		#endregion

		#region TestDatesKind_Unspecified

		public void TestDates_DateTimeKind_Unspecified()
		{
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Consol.JK_MasterBillIssueDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Consol.JK_DateFirstForeignPort.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Consol.JK_DateLastForeignPort.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Consol.JK_DatePortOfFirstArrival.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Consol.JK_CustomDate1.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Consol.JK_CustomDate2.Kind);
		}

		#endregion

		#region Clone

		public void TestCloneExcludeProperties()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_CarrierContractNumber = "CONTRACTNUMBER";
			consol.JK_RCA_AllocationLine = ZGuid.NewZGuid();
			consol.JK_ElectronicBillOfLadingType = "XXX";
			consol.JK_ElectronicBillOfLadingTerms = "XXX";
			consol.JK_ElectronicBillOfLadingReference = "REF_XXX";

			var result = (CommonConsol)consol.Clone();
			AssertEquals(string.Empty, result.JK_CarrierContractNumber);
			AssertEquals(ZGuid.Empty, result.JK_RCA_AllocationLine);
			AssertEquals(string.Empty, result.JK_ElectronicBillOfLadingType);
			AssertEquals(string.Empty, result.JK_ElectronicBillOfLadingTerms);
			AssertEquals(string.Empty, result.JK_ElectronicBillOfLadingReference);
		}

		#endregion

		#region Template Copy

		public void TestTemplateCopyHandlesMasterBill()
		{
			CommonConsol consol = GetConsol();
			consol.JK_MasterBillNum = "30040000";
			CommonConsol copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("300", copiedConsol.JK_MasterBillNum);

			consol = GetConsol();
			consol.JK_MasterBillNum = "";
			copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("", copiedConsol.JK_MasterBillNum);

			consol = Factory.New<ConsolWithCloneMasterBillNum>();
			consol.JK_MasterBillNum = "30040000";
			copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("3001111", copiedConsol.JK_MasterBillNum);
		}

		public void TestTemplateCopyUsesAdditionalFactoryCorrectly()
		{
			var consol = GetConsol();
			consol.JK_MasterBillNum = "30040000";
			consol.Transports.AddNew();

			var consolCusEntryNum = consol.Numbers.AddNew();
			consolCusEntryNum.CE_RN_NKCountryCode = Constants.CountryCodes.Australia;
			consolCusEntryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			consolCusEntryNum.CE_EntryNum = "42069";

			var consolNote = consol.Notes.AddNew();
			consolNote.ST_Description = "Ye olde test";

			var alternativeFactory = new BusinessObjectFactory();
			var copiedConsol = consol.TemplateCopy(true, true, alternativeFactory: alternativeFactory);
			AssertEquals("300", copiedConsol.JK_MasterBillNum);
			AssertEquals("Copied consol should be in alternative factory.", alternativeFactory, copiedConsol.Factory);
			Assert("All tranports should be in alternative factory.", copiedConsol.Transports.All(transport => transport.Factory == alternativeFactory));

			AssertEquals("Numbers collection should have been copied from template into alternative factory.", 1, copiedConsol.Numbers.Count);
			AssertEquals("Number should be identical to template.", consolCusEntryNum.CE_EntryNum, copiedConsol.Numbers[0].CE_EntryNum);

			AssertEquals("Notes collection should have been copied from template into alternative factory.", 1, copiedConsol.Notes.VisibleNotes.Count);
			AssertEquals("Note should be identical to template.", consolNote.ST_Description, copiedConsol.Notes.VisibleNotes[0].ST_Description);
		}

		public void TestTemplateCopyHandlesAdditionalReferences()
		{
			var consol = GetConsol();
			var consolCusEntryNum = consol.Numbers.AddNew();
			consolCusEntryNum.CE_RN_NKCountryCode = Constants.CountryCodes.Australia;
			consolCusEntryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			consolCusEntryNum.CE_EntryNum = "42069";

			var hirNumber = consol.Numbers.AddNew();
			hirNumber.CE_EntryType = "HIR";
			hirNumber.CE_RN_NKCountryCode = "AU";
			hirNumber.CE_EntryNum = "HIR123";

			var szbNumber = consol.Numbers.AddNew();
			szbNumber.CE_EntryType = "SZB";
			szbNumber.CE_RN_NKCountryCode = "DE";
			szbNumber.CE_EntryNum = "SZB123";

			var copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("Numbers collection should have been copied from template.", 1, copiedConsol.Numbers.Count);
			AssertEquals("Number should be identical to template.", consolCusEntryNum.CE_EntryNum, copiedConsol.Numbers.GetFirstReferenceNumberByType("BKG")?.CE_EntryNum);

			AssertEquals("HIR should not be copied", null, copiedConsol.Numbers.GetFirstReferenceNumberByType("HIR"));
			AssertEquals("SZB should not be copied", null, copiedConsol.Numbers.GetFirstReferenceNumberByType("SZB"));
		}

		public void TestTemplateCopyHandlesPorts()
		{
			var consol = GetConsol();
			consol.JK_RL_NKFirstForeignPort = "AUSYD";
			consol.JK_RL_NKPortOfFirstArrival = "NZAKL";

			var copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("First foreign port should be identical to template.", consol.JK_RL_NKFirstForeignPort, copiedConsol.JK_RL_NKFirstForeignPort);
			AssertEquals("Port of first arrival should be identical to template.", consol.JK_RL_NKPortOfFirstArrival, copiedConsol.JK_RL_NKPortOfFirstArrival);
		}

		public void TestTemplateCopyHandlesNotes()
		{
			var consol = GetConsol();
			var consolNote = consol.Notes.AddNew();
			consolNote.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			consolNote = consol.Notes.AddNew();
			consolNote.ST_Description = "Ye olde test";

			var copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();
			AssertEquals("Notes collection should have been copied from template.", 1, copiedConsol.Notes.VisibleNotes.Count);
			AssertEquals("Note should be identical to template.", consolNote.ST_Description, copiedConsol.Notes.VisibleNotes[0].ST_Description);
		}

		public void TestTemplateCopyHandlesNumbersWithCSR()
		{
			var consol1 = GetConsol();

			var csrNumber1 = consol1.Numbers.AddNew();
			csrNumber1.CE_EntryType = "CSR";
			csrNumber1.CE_RN_NKCountryCode = "AU";
			csrNumber1.CE_EntryNum = "TEST_CONSOL1_CSR";

			var copiedConsol1 = (CommonConsol)((ITemplateCopyable)consol1).TemplateCopy();

			AssertEquals(0, copiedConsol1.Numbers.Count);

			var consol2 = GetConsol();

			var csrNumber2 = consol2.Numbers.AddNew();
			csrNumber2.CE_EntryType = "AMS";
			csrNumber2.CE_RN_NKCountryCode = "FR";
			csrNumber2.CE_EntryNum = "TEST_CONSOL2_AMS";

			var csrNumber3 = consol2.Numbers.AddNew();
			csrNumber3.CE_EntryType = "CSR";
			csrNumber3.CE_RN_NKCountryCode = "CN";
			csrNumber3.CE_EntryNum = "TEST_CONSOL2_CSR";

			var copiedConsol2 = (CommonConsol)((ITemplateCopyable)consol2).TemplateCopy();

			AssertEquals(1, copiedConsol2.Numbers.Count);
			AssertEquals("AMS", copiedConsol2.Numbers[0].CE_EntryType);
			AssertEquals("FR", copiedConsol2.Numbers[0].CE_RN_NKCountryCode);
			AssertEquals("TEST_CONSOL2_AMS", copiedConsol2.Numbers[0].CE_EntryNum);
		}

		public void TestTemplateCopyHandlesNumbersWithCMR()
		{
			var consol1 = GetConsol();

			var csrNumber1 = consol1.Numbers.AddNew();
			csrNumber1.CE_EntryType = "CMR";
			csrNumber1.CE_RN_NKCountryCode = "AU";
			csrNumber1.CE_EntryNum = "TEST_CONSOL1_CMR";

			var copiedConsol1 = (CommonConsol)((ITemplateCopyable)consol1).TemplateCopy();

			AssertEquals(0, copiedConsol1.Numbers.Count);
		}

		public void TestTemplateCopyDatesAndPorts()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolCutOffDate = ZDateTime.Today;
			consol.JK_MasterBillIssueDate = ZDateTime.Today.AddDays(-2);
			consol.JK_RL_NKLastForeignPort = "NZAKL";

			var copiedConsol = consol.TemplateCopy(false, false, includeDatesAndPorts: true);
			AssertEquals(ZDateTime.Today, copiedConsol.JK_ConsolCutOffDate);
			AssertEquals(ZDateTime.Today.AddDays(-2), copiedConsol.JK_MasterBillIssueDate);
			AssertEquals("NZAKL", copiedConsol.JK_RL_NKLastForeignPort);
		}

		class ConsolWithCloneMasterBillNum : CommonConsol
		{
			public ConsolWithCloneMasterBillNum(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			{
				CommonConsol consol = (CommonConsol)base.CloneInternal(args);
				consol.JK_MasterBillNum = "3001111";
				return consol;
			}
		}

		#endregion

		#region Defaulting from Carrier

		public void TestDefaultLinkedSeaTransportCarrierFromConsolCarrier()
		{
			var carrier = Factory.New<OrgHeader>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Prerequisite", 1, consol.Transports.Count);
			AssertEquals("Prerequisite", true, consol.Transports[0].IsSea);
			AssertEquals("Prerequisite", true, consol.Transports[0].CarrierPK.IsEmpty);

			consol.SetDefaultShippingLineAddress(carrier);
			AssertEquals(carrier.PK, consol.Transports[0].CarrierPK);

			var anotherCarrier = Factory.New<OrgHeader>();
			consol.SetDefaultShippingLineAddress(anotherCarrier);
			AssertEquals("No re-defaulting when transport carrier is non-empty", carrier.PK, consol.Transports[0].CarrierPK);

			consol.Transports.AddNew();
			consol.Transports[0].CarrierPK = ZGuid.Empty;

			consol.SetDefaultShippingLineAddress(anotherCarrier);
			AssertEquals("No re-defaulting when more than one transport", ZGuid.Empty, consol.Transports[0].CarrierPK);
		}

		public void TestDefaultShippingYardFromCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var sY1 = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "AUBNE");
			var sY2 = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "USSEA");
			var sY3 = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "USLAX");
			var sY4 = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "AUSYD");

			CommonConsol consol = GetConsol();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USSEA";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals(sY1.PK, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals(sY2.PK, consol.JK_OA_ContainerYardEmptyReturnAddress);

			consol.JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
			consol.JK_OA_ContainerYardEmptyReturnAddress = ZGuid.Empty;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals(sY3.PK, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals(sY4.PK, consol.JK_OA_ContainerYardEmptyReturnAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ContainerYardEmptyReturnAddress);
		}

		public void TestDefaultCTOFromCarrier()
		{
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			var cTO1 = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_Stevedore, "AUBNE");
			var cTO2 = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_Stevedore, "USSEA");

			CommonConsol consol = GetConsol();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USSEA";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals(cTO1.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(cTO2.PK, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress);

			var cTO3Org = Factory.NewWithValidTestData<OrgHeader>();
			cTO3Org.OH_RL_NKClosestPort = "JPOSA";
			var cTO3 = cTO3Org.MainAddress;

			consol.JK_OA_DepartureCTOAddress = cTO3.PK;
			consol.JK_OA_ArrivalCTOAddress = cTO3.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals(cTO3.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(cTO3.PK, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals(cTO3.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(cTO3.PK, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_OA_DepartureCTOAddress = ZGuid.Empty;
			consol.JK_OA_ArrivalCTOAddress = ZGuid.Empty;
			consol.JK_RL_NKLoadPort = "ADALV";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals(ZGuid.Empty, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USSEA";
			AssertEquals(cTO1.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(cTO2.PK, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_DepartureCTOAddress = ZGuid.Empty;
			consol.JK_OA_ArrivalCTOAddress = ZGuid.Empty;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			voyage.JV_VoyageFlight = "1111";

			VoyageOrigin voyOrigin = voyage.Origins.AddNew();
			voyOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			voyOrigin.JA_OA_DepartureCTOAddress = cTO3.PK;

			VoyageDestination voyDestination = voyage.Destinations.AddNew();
			voyDestination.JB_RL_NKPortOfDischarge = "USSEA";
			voyDestination.JB_OA_ArrivalCTOAddress = cTO3.PK;

			Factory.Save();

			Transport transport = consol.Transports[0];
			transport.JW_OA_CarrierAddress = ZGuid.Empty;
			transport.JW_IsLinked = true;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = voyage.JV_RV_NKVessel;
			transport.JW_VoyageFlight = "1111";

			AssertEquals("Transport was linked to the matched voyage", voyage.PK, transport.Sailing.Voyage.PK);
			AssertEquals("CTO defaulted from sailing", cTO3.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals("CTO defaulted from sailing", cTO3.PK, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("No re-defaultin as CTO was defaulted from sailing", cTO3.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals("No re-defaultin as CTO was defaulted from sailing", cTO3.PK, consol.JK_OA_ArrivalCTOAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals("No re-defaultin as CTO was defaulted from sailing", cTO3.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals("No re-defaultin as CTO was defaulted from sailing", cTO3.PK, consol.JK_OA_ArrivalCTOAddress);
		}

		public void TestDefaultCTOFromCarrierWithDirection()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var sydDepartureCTO = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_Stevedore, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Departure);
			var sydArrivalCTO = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_Stevedore, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Arrival);
			var aklDepartureCTO = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_Stevedore, "NZAKL", OrgConstants.CarrierAgentDirections.Code.Departure);
			var aklArrivalCTO = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_Stevedore, "NZAKL", OrgConstants.CarrierAgentDirections.Code.Arrival);

			var consol = GetConsol();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals(sydDepartureCTO.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(aklArrivalCTO.PK, consol.JK_OA_ArrivalCTOAddress);
		}

		public void TestDefaultCYFromCarrierWithDirection()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var sydDepartureCY = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Departure);
			var sydArrivalCY = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Arrival);
			var aklDepartureCY = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "NZAKL", OrgConstants.CarrierAgentDirections.Code.Departure);
			var aklArrivalCY = AddAppointedAgentToCarrier(carrier.CarrierAppointedAgentPorts_ContainerYardPark, "NZAKL", OrgConstants.CarrierAgentDirections.Code.Arrival);

			var consol = GetConsol();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals(sydDepartureCY.PK, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals(aklArrivalCY.PK, consol.JK_OA_ContainerYardEmptyReturnAddress);
		}

		OrgAddress AddAppointedAgentToCarrier(OrgCarrierAppointedAgentPortsDependentCollection collection, string port, string direction = "BTH", string terminalType = "")
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_RL_NKClosestPort = port;

			OrgAppointedAgentPorts map = collection.AddNew();
			map.O5_PortOrCountry = port;
			map.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;
			map.O5_TerminalType = terminalType;
			map.O5_AgentDirection = direction;

			return agent.MainAddress;
		}

		#endregion

		#region TestHasChangesAfterLoading

		public void TestHasChangesAfterLoading()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "GBLON";
			origin.JA_JV = voyage.PK;
			VoyageDestination dest = Factory.New<VoyageDestination>();
			dest.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.HomePort.RL_Code;
			dest.JB_JV = voyage.PK;
			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = dest.PK;

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Transport transport = Consol.Transports[0];
			transport.JW_JX = sailing.PK;
			transport.JW_RL_NKLoadPort = "GBLON";
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			Factory.Save();

			var copyConsol = Factory.Load<CommonConsol>(Consol.PK);
			AssertEquals("Loaded Consol HasChanges should be false.", false, copyConsol.HasChanges);
		}

		#endregion

		#region TestSetConsolType

		public void TestSetConsolType()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Default Consol Type (Transport=Air)", Constants.ContainerModes.Loose, Consol.JK_ConsolMode);
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Default Consol Type (Transport=Sea)", Constants.ContainerModes.FCL, Consol.JK_ConsolMode);
			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Default Consol Type (Transport=Road)", "", Consol.JK_ConsolMode);
			Consol.JK_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Default Consol Type (Transport=Courier)", "", Consol.JK_ConsolMode);
			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Default Consol Type (Transport=Rail)", "", Consol.JK_ConsolMode);
			Consol.JK_TransportMode = Constants.TransportModes.Other;
			AssertEquals("Default Consol Type (Transport=Other)", Constants.ContainerModes.Other, Consol.JK_ConsolMode);
		}

		#endregion

		#region TestCalcAgentCodes

		public void TestCalcAgentCodes()
		{
			OrgHeader newSendingAgent = Factory.New<OrgHeader>();
			newSendingAgent.OH_IsForwarder = true;
			newSendingAgent.OH_FullName = "Sending Agent";
			newSendingAgent.OH_Code = "SENDAGENT";

			OrgHeader newReceivingAgent = Factory.New<OrgHeader>();
			newReceivingAgent.OH_IsForwarder = true;
			newReceivingAgent.OH_FullName = "Receiving Agent";
			newReceivingAgent.OH_Code = "RECVAGENT";

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";
			Consol.SetDefaultSendingForwarderAddress(newSendingAgent.PK);
			Consol.SetDefaultReceivingForwarderAddress(newReceivingAgent.PK);
			AssertEquals("Sending Agent Code.", "SENDAGENT", Consol.JK_Calc_SendingAgentCode);
			AssertEquals("Receiving Agent Code.", "RECVAGENT", Consol.JK_Calc_ReceivingAgentCode);
		}

		#endregion

		#region JK_Calc_Container*

		public void TestJK_Calc_ContainerCount()
		{
			Action<CommonConsol, ZGuid, short> addContainer = (consolidation, refContainerPK, jC_ContainerCount) =>
			{
				CommonContainer container = consolidation.Containers.AddNew();
				container.JC_ContainerCount = jC_ContainerCount;
				container.JC_RC = refContainerPK;
			};

			CommonConsol consol = Factory.New<CommonConsol>();
			addContainer(consol, RC_20GP_PK, 1);
			addContainer(consol, RC_20GP_PK, 2);

			AssertEquals(3m, consol.JK_Calc_TEUCount);

			addContainer(consol, RC_40GP_PK, 4);
			addContainer(consol, RC_40GP_PK, 8);

			AssertEquals(27m, consol.JK_Calc_TEUCount);

			addContainer(consol, RC_20RE_PK, 16);
			addContainer(consol, RC_20RE_PK, 32);

			addContainer(consol, RC_40RE_PK, 20);
			addContainer(consol, RC_40RE_PK, 30);

			RefContainer[] refContainers = Factory.Load<RefContainer>(new ZQuery());
			ZGuid otherContainerTypePK = refContainers.First(container => container.IsOtherContainerType).PK;

			addContainer(consol, otherContainerTypePK, 13);
			addContainer(consol, otherContainerTypePK, 23);

			AssertEquals(3, consol.JK_Calc_20GPCount);
			AssertEquals(12, consol.JK_Calc_40GPCount);
			AssertEquals(48, consol.JK_Calc_20RECount);
			AssertEquals(50, consol.JK_Calc_40RECount);
			AssertEquals(36, consol.JK_Calc_OtherContainerCount);
			AssertEquals(3 + 12 + 48 + 50 + 36, consol.JK_Calc_ContainerCount);
		}

		#endregion

		#region TestJK_UniqueConsignRefClearedIfSaveIsUnsuccessful

		[ExpectException(typeof(Exception))]
		public void TestJK_UniqueConsignRefClearedIfSaveIsUnsuccessful()
		{
			CommonConsol consol = Factory.New<TestHelperBaseConsolThatThrowsExecptionWhilstSaving>();
			try
			{
				Factory.Save();
			}
			finally
			{
				AssertEquals("JK_UniqueConsignRef", "", consol.JK_UniqueConsignRef);
			}
		}

		#endregion

		#region Test Defaults

		public void TestDefaultJK_TransportMode()
		{
			AssertEquals("Default Transport Mode", GlbDepartment.CurrentDepartment.TransportMode, Consol.JK_TransportMode);
		}

		public void TestDefaultPaymentTerm()
		{
			Env.Registry.SetConsolPaymentTerm("");
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("No default", "", consol.JK_PrepaidCollect);

			Env.Registry.SetConsolPaymentTerm(Constants.PaymentType.Prepaid);
			consol = Factory.New<CommonConsol>();
			AssertEquals("Default to Prepaid", Constants.PaymentType.Prepaid, consol.JK_PrepaidCollect);
		}

		public void TestDefaultOfficeAddressTypes()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("No default address", AddressType.NoDefault, Consol.JK_OA_CreditorAddress_ZAddress.DefaultAddressType);
			AssertEquals("It will always default to Office Address", AddressType.OFC, Consol.JK_OA_ArrivalUnpackCFSTransportAddress_ZAddress.DefaultAddressType);
			AssertEquals("It will always default to Office Address", AddressType.OFC, Consol.JK_OA_DeparturePackCFSTransportAddress_ZAddress.DefaultAddressType);
			AssertEquals("No default address", AddressType.NoDefault, Consol.JK_OA_ShippingLineAddress_ZAddress.DefaultAddressType);
		}

		public void TestDefaultCreditorAddressToAPAddress()
		{
			var consol = Factory.New<CommonConsol>();

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR";
			creditor.OH_IsCreditor = true;

			OrgAddress officeAddress = creditor.Addresses.AddNew(OrgAddressType.Office, true);
			officeAddress.OA_Address1 = "Office Address1";

			consol.CreditorPK = ZGuid.Empty;
			consol.CreditorPK = creditor.PK;

			AssertEquals(officeAddress.PK, consol.JK_OA_CreditorAddress);
			AssertEquals(officeAddress.OA_Address1, consol.JK_OA_CreditorAddress_ZAddress.AddressFull);

			OrgAddress postalAddress = creditor.Addresses.AddNew(OrgAddressType.Payables, true);
			postalAddress.OA_Address1 = "Postal Adress1";

			Factory.Save();

			consol.CreditorPK = ZGuid.Empty;
			consol.CreditorPK = creditor.PK;

			AssertEquals(postalAddress.PK, consol.JK_OA_CreditorAddress);
			AssertEquals(postalAddress.OA_Address1, consol.JK_OA_CreditorAddress_ZAddress.AddressFull);

			OrgAddress payablesAddress = creditor.Addresses.AddNew(OrgAddressType.Payables, true);
			payablesAddress.OA_Address1 = "Payables Adress1";

			Factory.Save();

			consol.CreditorPK = ZGuid.Empty;
			consol.CreditorPK = creditor.PK;

			AssertEquals(payablesAddress.PK, consol.JK_OA_CreditorAddress);
			AssertEquals(payablesAddress.OA_Address1, consol.JK_OA_CreditorAddress_ZAddress.AddressFull);
		}

		public void TestSetDefaultDepartureAddressType()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var transport = Consol.Transports[0];
			Consol.JK_RL_NKLoadPort = "";

			AssertEquals(AddressType.PIC, Consol.JK_OA_DepartureCTOAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.PIC, Consol.JK_OA_PackDepotAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.PIC, Consol.JK_OA_ContainerYardEmptyPickupAddress_ZAddress.DefaultAddressType);

			Consol.JK_RL_NKLoadPort = HomePort;
			AssertEquals(AddressType.PIC, Consol.JK_OA_DepartureCTOAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.PIC, Consol.JK_OA_PackDepotAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.PIC, Consol.JK_OA_ContainerYardEmptyPickupAddress_ZAddress.DefaultAddressType);
		}

		#region SetDefaultArrivalAddressType

		public void TestSetDefaultArrivalAddressType()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var transport = Consol.Transports[0];
			Consol.JK_RL_NKDischargePort = "";

			AssertEquals(AddressType.DLV, Consol.JK_OA_ArrivalCTOAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.DLV, Consol.JK_OA_UnpackDepotAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.DLV, Consol.JK_OA_ContainerYardEmptyReturnAddress_ZAddress.DefaultAddressType);

			Consol.JK_RL_NKDischargePort = HomePort;
			AssertEquals(AddressType.DLV, Consol.JK_OA_ArrivalCTOAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.DLV, Consol.JK_OA_UnpackDepotAddress_ZAddress.DefaultAddressType);
			AssertEquals(AddressType.DLV, Consol.JK_OA_ContainerYardEmptyReturnAddress_ZAddress.DefaultAddressType);
		}

		#endregion

		#region Test Default Air Transport Status

		public void TestDefaultAirTransportStatus()
		{
			GlbDepartment.CurrentDepartment.GE_Air = true;
			var consolAir = Factory.New<CommonConsol>();
			var transport1 = consolAir.Transports[0];
			AssertEquals(Constants.TransportStatus.Planned, transport1.JW_Status);

			var transport2 = consolAir.Transports.AddNew();
			AssertEquals(Constants.TransportStatus.Planned, transport2.JW_Status);
		}

		#endregion

		#region Test Default Port

		public void TestDefaultJK_RL_NKLoadPort() => TestConsolDefaultPort(GlbBranchDefaultToList.Codes.ConsolFirstLoad);

		public void TestDefaultJK_RL_NKDischargePort() => TestConsolDefaultPort(GlbBranchDefaultToList.Codes.ConsolLastDischarge);

		void TestConsolDefaultPort(string defaultTo)
		{
			SetDepartment(defaultTo == GlbBranchDefaultToList.Codes.ConsolLastDischarge, defaultTo == GlbBranchDefaultToList.Codes.ConsolFirstLoad, false);
			var branch = GlbBranch.CurrentBranch;
			branch.GB_RL_NKHomePort = "AUSYD";
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, defaultTo, "SEA", "FCL", "AUMEL");
			branch.Factory.Save();

			var defaultContainerModes = new DefaultContainerModesCollection
			{
				new DefaultContainerModes
				{
					TransportMode = Constants.TransportModes.Sea,
					ContainerMode = Constants.ContainerModes.FCL
				},
				new DefaultContainerModes
				{
					TransportMode = Constants.TransportModes.Air,
					ContainerMode = Constants.ContainerModes.Loose
				}
			};
			using (FreightConfigurationRegistry.Instance.DefaultContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultContainerModes))
			{
				SetDepartmentTransportMode("SEA");
				var consol = Factory.New<CommonConsol>();
				AssertEquals("AUMEL", defaultTo == GlbBranchDefaultToList.Codes.ConsolFirstLoad ? consol.JK_RL_NKLoadPort : consol.JK_RL_NKDischargePort);

				SetDepartmentTransportMode("AIR");
				consol = Factory.New<CommonConsol>();
				AssertEquals("AUSYD", defaultTo == GlbBranchDefaultToList.Codes.ConsolFirstLoad ? consol.JK_RL_NKLoadPort : consol.JK_RL_NKDischargePort);
			}
		}

		#endregion

		#endregion

		#region Schedule

		#region TestDefaultCTOAddressFromSchedule

		public void TestDefaultCTOAddressFromSchedule()
		{
			OrgHeader cTO1 = Factory.New<OrgHeader>();
			cTO1.OH_FullName = "CTO ONE COMPANY";
			cTO1.OH_IsSeaCTO = true;
			cTO1.MainAddress.OA_Address1 = "Street";
			cTO1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.HomePort.RL_Code;

			OrgHeader cTO2 = Factory.New<OrgHeader>();
			cTO2.OH_FullName = "CTO TWO COMPANY";
			cTO2.MainAddress.OA_Address1 = "Street";
			cTO2.OH_IsSeaCTO = true;
			cTO2.OH_RL_NKClosestPort = "USSEA";

			OrgHeader cTO3 = Factory.New<OrgHeader>();
			cTO3.OH_FullName = "CTO THREE COMPANY";
			cTO3.MainAddress.OA_Address1 = "Street";
			cTO3.OH_IsSeaCTO = true;
			cTO3.OH_RL_NKClosestPort = "JPOSA";

			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage1.JV_RV_NKVessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			voyage1.JV_VoyageFlight = "1111";
			VoyageOrigin voyOrigin1 = voyage1.Origins.AddNew();
			voyOrigin1.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.HomePort.RL_Code;
			voyOrigin1.JA_OA_DepartureCTOAddress = cTO1.Addresses[0].PK;
			VoyageDestination voyDestination1 = voyage1.Destinations.AddNew();
			voyDestination1.JB_RL_NKPortOfDischarge = "USSEA";
			voyDestination1.JB_OA_ArrivalCTOAddress = cTO2.Addresses[0].PK;

			//Setup Import Sea Voyage
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage2.JV_RV_NKVessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			voyage2.JV_VoyageFlight = "2222";
			VoyageOrigin voyOrigin2 = voyage2.Origins.AddNew();
			voyOrigin2.JA_RL_NKPortOfLoading = "JPOSA";
			voyOrigin2.JA_OA_DepartureCTOAddress = cTO3.Addresses[0].PK;
			VoyageDestination voyDestination2 = voyage2.Destinations.AddNew();
			voyDestination2.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.HomePort.RL_Code;
			voyDestination2.JB_OA_ArrivalCTOAddress = cTO1.Addresses[0].PK;
			Factory.Save();

			//Setup Export Sea consol
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			Consol.JK_RL_NKDischargePort = "USSEA";

			Transport transport = Consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = voyage1.JV_RV_NKVessel;
			transport.JW_VoyageFlight = "1111";
			AssertEquals("Schedule should not be null", voyage1.Sailings[0], Consol.Schedule);
			AssertEquals("Consol Departure CTO should be defaulted from Schedule", voyOrigin1.JA_OA_DepartureCTOAddress, Consol.JK_OA_DepartureCTOAddress);
			AssertEquals("Consol Arrival CTO should be defaulted from Schedule", voyDestination1.JB_OA_ArrivalCTOAddress, Consol.JK_OA_ArrivalCTOAddress);

			transport.JW_VoyageFlight = "";
			Assert("Schedule should be null", Consol.Schedule == null);
			AssertEquals("Consol Departure CTO should be cleared", ZGuid.Empty, Consol.JK_OA_DepartureCTOAddress);
			AssertEquals("Consol Arrival CTO should be cleared", ZGuid.Empty, Consol.JK_OA_ArrivalCTOAddress);

			Consol.JK_OA_DepartureCTOAddress = ZGuid.NewZGuid();
			transport.JW_VoyageFlight = "1111";
			AssertEquals("Schedule should not be null", voyage1.Sailings[0], Consol.Schedule);
			Assert("Consol Departure CTO should not be defaulted from Schedule", Consol.JK_OA_DepartureCTOAddress != voyOrigin1.JA_OA_DepartureCTOAddress);
			Assert("Consol Arrival CTO should be defaulted from Schedule", Consol.JK_OA_ArrivalCTOAddress == voyDestination1.JB_OA_ArrivalCTOAddress);

			transport.JW_VoyageFlight = "";
			Assert("Schedule should be null", Consol.Schedule == null);
			Assert("Consol Departure CTO should not be cleared", Consol.JK_OA_DepartureCTOAddress != ZGuid.Empty);
			Assert("Consol Arrival CTO should be cleared", Consol.JK_OA_ArrivalCTOAddress == ZGuid.Empty);

			Consol.JK_RL_NKLoadPort = "JPOSA";
			Consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			transport.JW_Vessel = voyage2.JV_RV_NKVessel;
			transport.JW_VoyageFlight = "2222";
			AssertEquals("Schedule should not be null", voyage2.Sailings[0], Consol.Schedule);
			Assert("Consol Departure CTO should not be defaulted from Schedule", voyOrigin2.JA_OA_DepartureCTOAddress != Consol.JK_OA_DepartureCTOAddress);
			AssertEquals("Consol Arrival CTO should be defaulted from Schedule", voyDestination2.JB_OA_ArrivalCTOAddress, Consol.JK_OA_ArrivalCTOAddress);

			transport.JW_VoyageFlight = "";
			Assert("Schedule should be null", Consol.Schedule == null);
			AssertEquals("Consol Arrival CTO should be cleared", ZGuid.Empty, Consol.JK_OA_ArrivalCTOAddress);
			Assert("Consol Departure CTO should not be cleared", !Consol.JK_OA_DepartureCTOAddress.IsEmpty);

			Consol.JK_OA_DepartureCTOAddress = ZGuid.Empty;
			Consol.JK_OA_ArrivalCTOAddress = ZGuid.NewZGuid();
			transport.JW_VoyageFlight = "2222";
			AssertEquals("Schedule should not be null", voyage2.Sailings[0], Consol.Schedule);
			Assert("Consol Departure CTO should be defaulted from Schedule", Consol.JK_OA_DepartureCTOAddress == voyOrigin2.JA_OA_DepartureCTOAddress);
			Assert("Consol Arrival CTO should not be defaulted from Schedule", Consol.JK_OA_ArrivalCTOAddress != voyDestination2.JB_OA_ArrivalCTOAddress);

			transport.JW_VoyageFlight = "";
			Assert("Schedule should be null", Consol.Schedule == null);
			Assert("Consol Departure CTO should be cleared", Consol.JK_OA_DepartureCTOAddress == ZGuid.Empty);
			Assert("Consol Arrival CTO should not be cleared", Consol.JK_OA_ArrivalCTOAddress != ZGuid.Empty);
		}

		#endregion

		#region TestScheduleForRoad

		public void TestScheduleForRoad()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Road;
			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Transport transport = Consol.Transports[0];

			transport.JW_RL_NKLoadPort = "ZWBAT";
			transport.JW_RL_NKDiscPort = "ZWWKI";
			transport.JW_ETD = Env.Time.CurrentLocalDate;
			transport.JW_ETA = Env.Time.CurrentLocalDate;
			transport.JW_VoyageFlight = "CONSOLTEST";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobSailing schedule = factory2.Load<JobSailing>(transport.JW_JX);
			AssertNotNull("Road Schedule not saved", schedule);
		}

		#endregion

		#region TestSailingScheduleClearsETA

		public void TestSailingScheduleClearsETA()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ANRO ASIA", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "192";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			origin.JA_E_DEP = ZDateTime.Today;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "CHRRC";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(20);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];

			transport.JW_JX = voyage.Sailings[0].PK;
			AssertEquals("Consol ETD should be set", destination.JB_E_ARV, consol.JK_JX_JB_E_ARV);
			Factory.Save();
			AssertEquals("Consol ETD should be set", destination.JB_E_ARV, consol.JK_JX_JB_E_ARV);
			transport.JW_ETA = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Consol ETD should be empty", ZDateTime.Empty, consol.JK_JX_JB_E_ARV);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol loadedConsol = newFactory.Load<CommonConsol>(consol.PK);
			AssertEquals("Consol ETD should be empty after saving and re-loading", ZDateTime.Empty, loadedConsol.JK_JX_JB_E_ARV);
		}

		#endregion

		#region Test Sailing Schedule Load Port Arrival Dates

		[TestDate(2013, 8, 1)]
		public void TestJK_JX_JA_E_ARV()
		{
			ZDateTime today = ZDateTime.Today;
			CommonConsol consol = GetConsolWithTransports(today);
			AssertEquals("Pre-condition: expected consol to have two transports", 2, consol.Transports.Count);

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports[1];

			AssertEquals("Expected transport1's load port ETA to be three days before today", today.AddDays(-3), transport1.JW_JX_Load_ETA);
			AssertEquals("Expected transport2's load port ETA to be empty", ZDateTime.Empty, transport2.JW_JX_Load_ETA);
			AssertEquals("Expected load port ETA to match most interesting transport's load port ETA", consol.JK_JX_JA_E_ARV, transport1.JW_JX_Load_ETA);

			consol.Transports.RemoveAndDelete(transport1);

			AssertEquals("Expected load port ETA to fall back to remaining transport load port ETA", consol.JK_JX_JA_E_ARV, transport2.JW_JX_Load_ETA);

			transport2.Sailing.Origin.JA_A_ARV = today.AddDays(25);

			AssertEquals("Expected load port ETA to fall back to remaining transport load port ETA", consol.JK_JX_JA_E_ARV, transport2.JW_JX_Load_ETA);

			consol.Transports.RemoveAndDelete(transport2);

			AssertEquals("Expected load port ETA to be an empty date", ZDateTime.Empty, consol.JK_JX_JA_E_ARV);
		}

		[TestDate(2013, 8, 1)]
		public void TestJK_JX_JA_A_ARV()
		{
			ZDateTime today = ZDateTime.Today;
			CommonConsol consol = GetConsolWithTransports(today);
			AssertEquals("Pre-condition: expected consol to have two transports", 2, consol.Transports.Count);

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports[1];

			AssertEquals("Expected transport1's load port ATA to be two days before today", today.AddDays(-2), transport1.JW_JX_Load_ATA);
			AssertEquals("Expected transport2's load port ATA to be empty", ZDateTime.Empty, transport2.JW_JX_Load_ATA);
			AssertEquals("Expected load port ATA to match most interesting transport's load port ATA", consol.JK_JX_JA_A_ARV, transport1.JW_JX_Load_ATA);

			consol.Transports.RemoveAndDelete(transport1);

			AssertEquals("Expected load port ATA to fall back to remaining transport load port ATA", consol.JK_JX_JA_A_ARV, transport2.JW_JX_Load_ATA);

			transport2.Sailing.Origin.JA_A_ARV = today.AddDays(25);

			AssertEquals("Expected load port ATA to fall back to remaining transport load port ATA", consol.JK_JX_JA_A_ARV, transport2.JW_JX_Load_ATA);

			consol.Transports.RemoveAndDelete(transport2);

			AssertEquals("Expected load port ATA to be an empty date", ZDateTime.Empty, consol.JK_JX_JA_A_ARV);
		}

		CommonConsol GetConsolWithTransports(ZDateTime today)
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			JobVoyage mainVoyage = Factory.NewWithValidTestData<JobVoyage>();
			mainVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ANRO ASIA", Factory).First().RV_FK;
			mainVoyage.JV_VoyageFlight = "11111";
			VoyageOrigin origin1 = mainVoyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			origin1.JA_E_ARV = today.AddDays(-3);
			origin1.JA_A_ARV = today.AddDays(-2);
			origin1.JA_E_DEP = today;
			VoyageDestination destination1 = mainVoyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = OverseasPort;
			destination1.JB_E_ARV = today.AddDays(10);

			Transport mainTransport = consol.Transports[0];
			mainTransport.JW_IsLinked = true;
			mainTransport.JW_JX = mainVoyage.Sailings[0].PK;

			JobVoyage anotherVoyage = Factory.NewWithValidTestData<JobVoyage>();
			anotherVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ANRO ASIA", Factory).First().RV_FK;
			anotherVoyage.JV_VoyageFlight = "22222";
			VoyageOrigin origin2 = anotherVoyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = OverseasPort;
			origin2.JA_E_DEP = today.AddDays(12);
			VoyageDestination destination2 = anotherVoyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = OverseasPort2;
			destination2.JB_E_ARV = today.AddDays(20);

			Transport anotherTransport = consol.Transports.AddNew();
			anotherTransport.JW_IsLinked = true;
			anotherTransport.JW_JX = anotherVoyage.Sailings[0].PK;

			Factory.Save();

			return consol;
		}

		#endregion

		#endregion

		#region Test Lists

		public void TestJK_PrepaidCollect_List()
		{
			Assert(Consol.JK_PrepaidCollect_List.Count > 0);
		}

		public void TestJK_TransportMode_List()
		{
			Assert(Consol.JK_TransportMode_List.Count > 0);
		}

		public void TestJK_CarrierBookingStatus_List()
		{
			Assert(Consol.JK_CarrierBookingStatus_List.Count > 0);
		}

		public void TestJK_ElectronicBillOfLadingType_List()
		{
			var billOfLadingBillTypeList = Consol.JK_ElectronicBillOfLadingType_List;

			AssertEquals("STR, TOR, BLE", billOfLadingBillTypeList.CodesAsString);
			AssertEquals("Straight", billOfLadingBillTypeList[0].Description);
			AssertEquals("To Order", billOfLadingBillTypeList[1].Description);
			AssertEquals("Blank Endorse", billOfLadingBillTypeList[2].Description);
		}

		public void TestJK_ElectronicBillOfLadingTerms_List()
		{
			var billOfLadingBillTermsList = Consol.JK_ElectronicBillOfLadingTerms_List;

			AssertEquals("TRA, NTR", billOfLadingBillTermsList.CodesAsString);
			AssertEquals("Transferable", billOfLadingBillTermsList[0].Description);
			AssertEquals("Non-Transferable", billOfLadingBillTermsList[1].Description);
		}

		public void TestJK_PackageGrouping_List()
		{
			AssertEquals("DNG, SHP, PKL", Consol.JK_PackageGrouping_List.CodesAsString);
		}

		public void TestJK_ConsolType_List()
		{
			Consol.JK_AgentType = "Any";
			Consol.JK_TransportMode = "Jnk";
			AssertEquals("Codes in list", 0, Consol.JK_ConsolMode_List.Count);
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Codes in list (Transport=Air)", 5, Consol.JK_ConsolMode_List.Count);
			Consol.JK_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Codes in list (Transport=Courier)", 3, Consol.JK_ConsolMode_List.Count);
			Consol.JK_TransportMode = Constants.TransportModes.Other;
			AssertEquals("Codes in list (Transport=Other)", 1, Consol.JK_ConsolMode_List.Count);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Codes in list (Agent=Agent, Transport=Sea)", 10, Consol.JK_ConsolMode_List.Count);
			Consol.JK_AgentType = Constants.AgentType.Other;
			AssertEquals("Codes in list (Agent=Charter, Transport=Sea)", 1, Consol.JK_ConsolMode_List.Count);
			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals("Codes in list (Agent=Direct, Transport=Sea)", 9, Consol.JK_ConsolMode_List.Count);
		}

		public void TestJK_AgentType_List()
		{
			var genericAgentTypes = new CodeDescriptionPairList(OLookUpEditType.AgentType).ToArray().Select(x => x.Code);

			Action<IEnumerable<string>> assertAgentTypeList = (expectedCodes) =>
			{
				AssertContainsExactElementsInAnyOrder(expectedCodes, Consol.JK_AgentType_List.ToArray().Select(y => y.Code));
			};

			Consol.JK_TransportMode = "XXX";
			assertAgentTypeList(genericAgentTypes);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			assertAgentTypeList(genericAgentTypes);

			var airSuperManifestAgentTypes = new List<string>(genericAgentTypes);
			airSuperManifestAgentTypes.Add(Constants.AgentType.AWBCoload);
			airSuperManifestAgentTypes.Add(Constants.AgentType.AWBMaster);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			assertAgentTypeList(airSuperManifestAgentTypes);

			Consol.JK_TransportMode = Constants.TransportModes.Other;
			assertAgentTypeList(genericAgentTypes);
		}

		#endregion

		#region PackLines

		public void TestShipmentRemoved_PackLineIsSplittedAndPackedFromAnotherFactory_UnpackPackLine()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var consolInAnotherFactory = anotherFactory.Load<CommonConsol>(consol.PK);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);

			consolInAnotherFactory.Shipments.RemoveAll();
			anotherFactory.Save();

			AssertEquals("Packed PackLines count", 0, container.PackLines.Count);
		}

		#endregion

		#region DepartureFlightOriginLOCO

		public void TestDepartureFlightOriginLOCO_NoFirstFlight()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainLeg = consol.Transports[0];
			mainLeg.JW_RL_NKLoadPort = "AUMEL";
			mainLeg.JW_RL_NKDiscPort = "CNSHA";
			mainLeg.JW_TransportMode = Core.Constants.TransportModes.Road;

			AssertEquals("Origin code should be consol's load port Sydney.", "AUSYD", consol.DepartureFlightOriginLoco.Code);
		}

		public void TestDepartureFlightOriginLOCO_FirstFlightOnly()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var transportAfterFlight = consol.Transports.AddNew();
			transportAfterFlight.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			transportAfterFlight.JW_RL_NKLoadPort = "CNSHA";
			transportAfterFlight.JW_RL_NKDiscPort = "CNPEK";
			transportAfterFlight.JW_TransportMode = Core.Constants.TransportModes.Road;

			AssertEquals("Origin code should be consol's load port Sydney. Should ignore transports after first flight", "AUSYD", consol.DepartureFlightOriginLoco.Code);
		}

		public void TestDepartureFlightOriginLOCO_PreCarriageLeg_DifferentCarrier()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var transportAfterFlight = consol.Transports.AddNew();
			transportAfterFlight.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			transportAfterFlight.JW_RL_NKLoadPort = "CNSHA";
			transportAfterFlight.JW_RL_NKDiscPort = "CNPEK";
			transportAfterFlight.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport1 = consol.Transports.AddNew();
			preCarriageTransport1.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			preCarriageTransport1.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport1.JW_RL_NKDiscPort = "AUMEL";
			preCarriageTransport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			AssertEquals("Pre-carriage with a different carrier comes before the first flight, origin code comes from the carrier's first transport", "AUMEL", consol.DepartureFlightOriginLoco.Code);
		}

		public void TestDepartureFlightOriginLOCO_TwoPreCarriageLegs_SecondWithSameCarrier()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var transportAfterFlight = consol.Transports.AddNew();
			transportAfterFlight.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			transportAfterFlight.JW_RL_NKLoadPort = "CNSHA";
			transportAfterFlight.JW_RL_NKDiscPort = "CNPEK";
			transportAfterFlight.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport1 = consol.Transports.AddNew();
			preCarriageTransport1.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			preCarriageTransport1.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport1.JW_RL_NKDiscPort = "AUCBR";
			preCarriageTransport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport2 = consol.Transports.AddNew();
			preCarriageTransport2.JW_OA_CarrierAddress = mainCarrier.MainAddress.PK;
			preCarriageTransport2.JW_RL_NKLoadPort = "AUCBR";
			preCarriageTransport2.JW_RL_NKDiscPort = "AUMEL";
			preCarriageTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Should pick up from Canberra as it is now the carrier's first transport leg", "AUCBR", consol.DepartureFlightOriginLoco.Code);
		}

		public void TestDepartureFlightOriginLOCO_TwoPreCarriageLegs_SameOrEmptyCarrier()
		{
			var mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNPEK";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			var mainFlight = consol.Transports[0];
			mainFlight.JW_RL_NKLoadPort = "AUMEL";
			mainFlight.JW_RL_NKDiscPort = "CNSHA";
			mainFlight.JW_TransportMode = Core.Constants.TransportModes.Air;
			mainFlight.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;

			var preCarriageTransport1 = consol.Transports.AddNew();
			preCarriageTransport1.JW_OA_CarrierAddress = mainCarrier.Addresses.AddNew().PK;
			preCarriageTransport1.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport1.JW_RL_NKDiscPort = "AUCBR";
			preCarriageTransport1.JW_TransportMode = Core.Constants.TransportModes.Road;

			var preCarriageTransport2 = consol.Transports.AddNew();
			preCarriageTransport2.JW_RL_NKLoadPort = "AUCBR";
			preCarriageTransport2.JW_RL_NKDiscPort = "AUMEL";
			preCarriageTransport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Should use consol load port as all precarriage legs are empty or the same carrier", "AUSYD", consol.DepartureFlightOriginLoco.Code);
		}

		public void TestDepartureFlightOriginLOCOForUNLOCOWithoutIATA()
		{
			var locationWithoutIATA = Factory.New<RefUNLOCO>();
			locationWithoutIATA.RL_Code = "NOIAT";
			locationWithoutIATA.RL_PortName = "Royksopp";
			locationWithoutIATA.RL_NameWithDiacriticals = "Röyksopp";
			locationWithoutIATA.RL_HasAirport = true;

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "NOIAT";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";

			AssertEquals("Consol load port has no IATA code, origin code comes from the first transport", "AUSYD", consol.DepartureFlightOriginLoco.Code);
		}

		#endregion

		#region ISupplyChainSecurityImportExportSupporter

		public void TestISupplyChainSecurityImportExportSupporter()
		{
			Consol.JK_RL_NKLoadPort = "AUBNE";
			Consol.JK_RL_NKDischargePort = "JMKIN";
			AssertEquals("AU", ((ISupplyChainSecurityImportExportSupporter)Consol).LoadCountryForSupplyChainSecurity);

			var transport1 = Consol.Transports[0];
			transport1.JW_TransportMode = "ROA";
			transport1.JW_RL_NKLoadPort = "CHAGO";
			transport1.JW_RL_NKDiscPort = "ITMIL";

			var transport2 = Consol.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_RL_NKLoadPort = "ITMIL";
			transport2.JW_RL_NKDiscPort = "FRCDG";

			var transport3 = Consol.Transports.AddNew();
			transport3.JW_TransportMode = "AIR";
			transport3.JW_RL_NKLoadPort = "FRCDG";
			transport3.JW_RL_NKDiscPort = "USJFK";

			Consol.JK_RL_NKLoadPort = "CHAGO";

			AssertEquals("Load country of first air leg", "IT", ((ISupplyChainSecurityImportExportSupporter)Consol).LoadCountryForSupplyChainSecurity);
		}

		#endregion

		#region IContainerParent tests

		public void TestIContainerParent_DocsAndCartage()
		{
			var shipment1 = Consol.Shipments.AddNew();
			var shipment2 = Consol.Shipments.AddNew();
			var shipment3 = Consol.Shipments.AddNew();

			shipment1.DocsAndCartage.JP_FCLStorageCommences = ZDateTime.Today;
			shipment2.DocsAndCartage.JP_FCLAvailable = ZDateTime.Today.AddDays(1);
			shipment3.DocsAndCartage.JP_LCLAvailable = ZDateTime.Today.AddDays(2);

			var container1 = Consol.Containers.AddNew();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.Containers.Add(container1);

			IContainerParent containerParent = Consol;

			AssertCollectionContains("Shipment 1 has a pack line in the container", shipment1.DocsAndCartage, containerParent.DocsAndCartage(container1));
			AssertCollectionContains("Shipment 2 has no packlines, but there is only one container on the Consol", shipment2.DocsAndCartage, containerParent.DocsAndCartage(container1));
			AssertCollectionContains("Shipment 3 has no packlines, but there is only one container on the Consol", shipment3.DocsAndCartage, containerParent.DocsAndCartage(container1));
			AssertEquals(3, containerParent.DocsAndCartage(container1).Count());

			var container2 = Consol.Containers.AddNew();

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.Containers.Add(container2);

			AssertCollectionContains("Shipment 1 has a pack line in the container", shipment1.DocsAndCartage, containerParent.DocsAndCartage(container1));
			AssertEquals("Shipment 2 and 3 are no longer included as they have no packline in the container and there is more than one container on the consol", 1, containerParent.DocsAndCartage(container1).Count());

			AssertCollectionContains("Shipment 2 is included for container 2", shipment2.DocsAndCartage, containerParent.DocsAndCartage(container2));
			AssertEquals(1, containerParent.DocsAndCartage(container2).Count());

			var packLine3 = shipment3.OuterPackLines.AddNew();
			packLine3.Containers.Add(container2);

			AssertCollectionContains("Shipment 2 is included for container 2", shipment2.DocsAndCartage, containerParent.DocsAndCartage(container2));
			AssertCollectionContains("Shipment 3 is included for container 3", shipment3.DocsAndCartage, containerParent.DocsAndCartage(container2));
			AssertEquals(2, containerParent.DocsAndCartage(container2).Count());
		}

		public void TestIContainerParent_LoadPort()
		{
			Consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals("DEHAM", ((IContainerParent)Consol).LoadPort.RL_Code);
		}

		public void TestIContainerParent_DischargePort()
		{
			Consol.JK_RL_NKDischargePort = "USCHI";
			AssertEquals("USCHI", ((IContainerParent)Consol).DischargePort.RL_Code);
		}

		#endregion

		#region Default ColoadWithAddress

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ImportCountryMatched()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.All, ZString.Empty,
				"AUSYD", "HK");

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.All, ZString.Empty,
				"AUSYD", "NZ");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;

			AssertEquals(relatedPartyOrg2.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address2.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ImportCountryEmpty()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, address1) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");

			var forwarderParty = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", ZString.Empty);

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;

			AssertEquals(relatedPartyOrg1.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address1.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_FallbackToRelatedPartyDirectionAndPorts()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Sales Address 1", "32 Pitt St",
				OrgConstants.AddressType.Sales);

			var forwarderParty = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "NZ");

			var address2 = relatedPartyOrg1.Addresses.AddNew();
			address2.OA_Code = "Wells Address";
			address2.OA_Address1 = "20 Wells St";
			address2.OA_RL_NKRelatedPortCode = "AUMEL";
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var address3 = relatedPartyOrg1.Addresses.AddNew();
			address3.OA_Code = "Pitt Address";
			address3.OA_Address1 = "360 Pitt St";
			address3.OA_RL_NKRelatedPortCode = "AUSYD";
			address3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;

			AssertEquals(relatedPartyOrg1.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address3.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ChangeAgentType()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, address1) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");

			var forwarderParty = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", ZString.Empty);

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals(relatedPartyOrg1.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address1.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ChangeTransportMode()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, address1) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");

			var forwarderParty = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", ZString.Empty);

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg1.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address1.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ChangeContainerMode()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.Sea, Constants.ContainerModes.FCL,
				"AUSYD", "HK");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Other;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "NZ");

			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			AssertEquals(relatedPartyOrg2.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address2.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ChangeJK_RL_NKLoadPort()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, address1) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUMEL", "NZ");

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "NZ");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg1.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address1.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg2.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address2.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ChangeJK_RL_NKDischargePort()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, address1) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "HK");

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "NZ");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg1.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address1.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals(relatedPartyOrg2.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address2.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_OnlyWhenHavingEnoughInformation()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.Sea, Constants.ContainerModes.FCL,
				"AUSYD", "HK");

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.All, ZString.Empty,
				"AUSYD", "NZ");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = ZString.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;

			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_SpecificTransportMode()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");
			var (relatedPartyOrg3, address3) = CreateOrgHeaderWithPickupAddress("Test Org 3",
				"TESTORG3", "Pickup Address 3", "10 Albert Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.All, ZString.Empty,
				"AUSYD", "HK");

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.All, ZString.Empty,
				"AUSYD", "NZ");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg2.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address2.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			var forwarderParty3 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty3, relatedPartyOrg3,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "NZ");
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg3.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address3.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_FallbackToContainerModeAndLocation()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, address2) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");
			var (relatedPartyOrg3, address3) = CreateOrgHeaderWithPickupAddress("Test Org 3",
				"TESTORG3", "Pickup Address 3", "10 Albert Rd");
			var (relatedPartyOrg4, address4) = CreateOrgHeaderWithPickupAddress("Test Org 4",
				"TESTORG4", "Pickup Address 4", "20 Orange Rd");
			var (relatedPartyOrg5, address5) = CreateOrgHeaderWithPickupAddress("Test Org 5",
				"TESTORG5", "Pickup Address 5", "30 Star Rd");
			var (relatedPartyOrg6, address6) = CreateOrgHeaderWithPickupAddress("Test Org 6",
				"TESTORG6", "Pickup Address 6", "40 Park St");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.Sea, Constants.ContainerModes.FCL,
				"AUMEL", ZString.Empty);

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.Air, ZString.Empty,
				ZString.Empty, ZString.Empty);

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg2.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address2.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			var forwarderParty3 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty3, relatedPartyOrg3,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				ZString.Empty, ZString.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg3.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address3.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			var forwarderParty4 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty4, relatedPartyOrg4,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AU", ZString.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg4.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address4.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			var forwarderParty5 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty5, relatedPartyOrg5,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", ZString.Empty);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg5.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address5.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);

			var forwarderParty6 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty6, relatedPartyOrg6,
				Constants.TransportModes.Air, Constants.ContainerModes.Loose,
				"AUSYD", "NZ");
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(relatedPartyOrg6.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
			AssertEquals(address6.PK, consol.JK_OA_CreditorAddress_ZAddress.AddressFK);
		}

		public void TestDefaultColoadWithAddressBasedOnRelatedParty_ParamertersMismatch()
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var (relatedPartyOrg1, _) = CreateOrgHeaderWithPickupAddress("Test Org 1",
				"TESTORG1", "Pickup Address 1", "32 Pitt St");
			var (relatedPartyOrg2, _) = CreateOrgHeaderWithPickupAddress("Test Org 2",
				"TESTORG2", "Pickup Address 2", "200 Hall Rd");
			var (relatedPartyOrg3, _) = CreateOrgHeaderWithPickupAddress("Test Org 3",
				"TESTORG3", "Pickup Address 3", "10 Albert Rd");

			var forwarderParty1 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty1, relatedPartyOrg1,
				Constants.TransportModes.Air, Constants.ContainerModes.Other,
				ZString.Empty, ZString.Empty);

			var forwarderParty2 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty2, relatedPartyOrg2,
				Constants.TransportModes.Air, ZString.Empty,
				"AUBNE", ZString.Empty);

			var forwarderParty3 = sendingAgent.ForwarderRelatedParties.AddNew();
			SetupRelatedParty(forwarderParty3, relatedPartyOrg3,
				Constants.TransportModes.Air, ZString.Empty,
				ZString.Empty, "SG");

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingAgent.PK;
			AssertEquals(ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		void SetupRelatedParty(OrgRelatedParty org, OrgHeader relatedParty,
			ZString transportMode, ZString containerMode,
			ZString location, ZString importCountry)
		{
			org.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			org.PR_OH_RelatedParty = relatedParty.PK;
			org.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			org.PR_FreightTransportMode = transportMode;
			org.PR_FreightContainerMode = containerMode;
			org.PR_Location = location;
			org.PR_RN_NKImporterCountry = importCountry;
		}

		(OrgHeader, OrgAddress) CreateOrgHeaderWithPickupAddress(ZString orgFullName, ZString orgCode,
			ZString addressCode, ZString address1, ZString capabilityCode = default)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = orgFullName;
			org.OH_Code = orgCode;

			var address = org.Addresses.AddNew();
			address.OA_Code = addressCode;
			address.OA_Address1 = address1;

			address.AddressCapability.SetCapabilityEnabled(capabilityCode.IsEmpty ? (ZString)OrgConstants.AddressType.Pickup : capabilityCode);

			return (org, address);
		}

		#endregion

		#region IPortMatchingSupport

		void AssertSetFirstArrivalPort(CommonConsol consol, string unlocoMsg, ZString consolUnloco, ZString unloco, ZString expectedUnloco, string dateMsg, ZDateTime consolDate, ZDateTimeOffset date, ZDateTime expectedDate)
		{
			IPortMatchingSupport portMatchingSupport = consol;
			consol.JK_RL_NKPortOfFirstArrival = consolUnloco;
			consol.JK_DatePortOfFirstArrival = consolDate;
			portMatchingSupport.SetFirstArrivalPort(unloco, date);
			CombineAssertions(() =>
			{
				AssertEquals(unlocoMsg, expectedUnloco, consol.JK_RL_NKPortOfFirstArrival);
				AssertEquals(dateMsg, expectedDate, consol.JK_DatePortOfFirstArrival);
			});
		}

		void AssertSetLastForeignPort(CommonConsol consol, string unlocoMsg, ZString consolUnloco, ZString unloco, ZString expectedUnloco, string dateMsg, ZDateTime consolDate, ZDateTimeOffset date, ZDateTime expectedDate)
		{
			IPortMatchingSupport portMatchingSupport = consol;
			consol.JK_RL_NKLastForeignPort = consolUnloco;
			consol.JK_DateLastForeignPort = consolDate;
			portMatchingSupport.SetLastForeignPort(unloco, date);
			CombineAssertions(() =>
			{
				AssertEquals(unlocoMsg, expectedUnloco, consol.JK_RL_NKLastForeignPort);
				AssertEquals(dateMsg, expectedDate, consol.JK_DateLastForeignPort);
			});
		}

		[TestDate(2022, 2, 9)]
		public void TestPortMatchingSupport_SetFirstArrivalPort()
		{
			// Arrange
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUMEL";

			// Act & Assert
			AssertSetFirstArrivalPort(consol
				, "First Arrival Port must be set if empty.", ZString.Empty, new ZString("AUSYD"), new ZString("AUSYD")
				, "First Arrival Date must be set if empty.", ZDateTime.Empty, ZDateTimeOffset.Today, ZDateTime.Today);

			AssertSetFirstArrivalPort(consol
				, "First Arrival Port must not be set if First Arrival Date is not empty.", ZString.Empty, new ZString("AUSYD"), ZString.Empty
				, "First Arrival Date must not be set if popuated.", ZDateTime.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTime.Today.AddDays(1));

			AssertSetFirstArrivalPort(consol
				, "First Arrival Port must not be set if populated.", new ZString("AUMEL"), new ZString("AUSYD"), new ZString("AUMEL")
				, "First Arrival Date must not be set if First Arrival Port is not empty.", ZDateTime.Empty, ZDateTimeOffset.Today, ZDateTime.Empty);

			AssertSetFirstArrivalPort(consol
				, "First Arrival Port must not be set if populated.", new ZString("AUMEL"), new ZString("AUSYD"), new ZString("AUMEL")
				, "Date of Arrival to First Arrival Port must not be set if popuated.", ZDateTime.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTime.Today.AddDays(1));
		}

		[TestDate(2022, 2, 9)]
		public void TestPortMatchingSupport_SetLastForeignPort()
		{
			// Arrange
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUMEL";

			// Act & Assert
			AssertSetLastForeignPort(consol
				, "Last Foreign Port must be set if empty.", ZString.Empty, new ZString("TWTPE"), new ZString("TWTPE")
				, "Last Foreign Date must be set if empty.", ZDateTime.Empty, ZDateTimeOffset.Today, ZDateTime.Today);

			AssertSetLastForeignPort(consol
				, "Last Foreign Port must not be set if Last Foreign Date is not empty.", ZString.Empty, new ZString("TWTPE"), ZString.Empty
				, "Last Foreign Date must not be set if popuated.", ZDateTime.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTime.Today.AddDays(1));

			AssertSetLastForeignPort(consol
				, "Last Foreign Port must not be set if populated.", new ZString("SGSIN"), new ZString("TWTPE"), new ZString("SGSIN")
				, "Last Foreign Date must not be set if Last Foreign Port is not empty.", ZDateTime.Empty, ZDateTimeOffset.Today, ZDateTime.Empty);

			AssertSetLastForeignPort(consol
				, "Last Foreign Port must not be set if populated.", new ZString("SGSIN"), new ZString("TWTPE"), new ZString("SGSIN")
				, "Last Foreign Date must not be set if popuated.", ZDateTime.Today.AddDays(1), ZDateTimeOffset.Today, ZDateTime.Today.AddDays(1));
		}

		[TestDate(2022, 2, 9)]
		public void TestPortMatchingSupportDoesNotSetLastForeignPort_IfReadOnly()
		{
			// Arrange
			var forwardingConsol = Factory.New<CommonConsol>();
			forwardingConsol.JK_TransportMode = TransportModes.Air;
			forwardingConsol.JK_RL_NKLoadPort = "CNSHA";
			forwardingConsol.JK_RL_NKDischargePort = "AUMEL";
			IPortMatchingSupport portMatchingSupport = forwardingConsol;

			// Act
			portMatchingSupport.SetLastForeignPort(new ZString("TWTPE"), ZDateTimeOffset.Today);

			// Assert
			CombineAssertions(() =>
			{
				Assert("JK_RL_NKLastForeignPort_ReadOnly is readonly for air consol", forwardingConsol.JK_RL_NKLastForeignPortInfo.ReadOnly);
				Assert("JK_DateLastForeignPort_ReadOnly is readonly for air consol", forwardingConsol.JK_DateLastForeignPortInfo.ReadOnly);
				AssertEquals("Last Foreign Port must not be set if readonly.", string.Empty, forwardingConsol.JK_RL_NKLastForeignPort);
				AssertEquals("Date of Departure from Last Foreign Port must not be set if readonly.", ZDateTime.Empty, forwardingConsol.JK_DateLastForeignPort);
			});
		}

		#endregion

		#region Implementation

		#region TestHelperBaseConsolThatThrowsExecptionWhilstSaving

		protected class TestHelperBaseConsolThatThrowsExecptionWhilstSaving : CommonConsol
		{
			public TestHelperBaseConsolThatThrowsExecptionWhilstSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new Exception();
			}
		}

		#endregion

		CommonConsol Consol;

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<CommonConsol>();
		}

		CommonConsol GetConsol()
		{
			return Factory.New<CommonConsol>();
		}

		void SetDepartment(bool isImport, bool isExport, bool isDomestic)
		{
			GlbDepartment.CurrentDepartment.GE_Export = isExport;
			GlbDepartment.CurrentDepartment.GE_Import = isImport;
			GlbDepartment.CurrentDepartment.GE_Domestic = isDomestic;
		}

		void SetDepartmentTransportMode(string transportMode)
		{
			GlbDepartment.CurrentDepartment.GE_Sea = transportMode == Core.Constants.TransportModes.Sea;
			GlbDepartment.CurrentDepartment.GE_Air = transportMode == Core.Constants.TransportModes.Air;
			GlbDepartment.CurrentDepartment.GE_Road = transportMode == Core.Constants.TransportModes.Road;
			GlbDepartment.CurrentDepartment.GE_Rail = transportMode == Core.Constants.TransportModes.Rail;
		}

		protected class TestBaseConsol : CommonConsol
		{
			public TestBaseConsol(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void CopyValuesFrom(CommonConsol sourceObject)
			{
				base.CopyValuesFrom(sourceObject);
			}
		}

		#endregion

		#region AddressAdditionalInfo

		public void TestAddressAdditionalInfo_Items_Is_Binded_To_DependentAdresses()
		{
			var commonConsol = Factory.New<CommonConsol>();
			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = Guid.NewGuid();
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = Guid.NewGuid();

			commonConsol.CFSDepartureByTransportMode = "ROA";
			commonConsol.CFSArrivalByTransportMode = "IWT";

			Factory.Save();

			Assert(!commonConsol.CFSDepartureByTransportMode_ReadOnly);
			Assert(!commonConsol.CFSArrivalByTransportMode_ReadOnly);

			AssertEquals("ROA", commonConsol.CFSDepartureByTransportMode);
			AssertEquals("IWT", commonConsol.CFSArrivalByTransportMode);
		}

		public void TestAddressAdditionalInfo_Is_Deleted_When_DependentAdress_Is_Cleared()
		{
			var commonConsol = Factory.New<CommonConsol>();
			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = Guid.NewGuid();
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = Guid.NewGuid();

			commonConsol.CFSDepartureByTransportMode = "ROA";
			commonConsol.CFSArrivalByTransportMode = "IWT";

			Factory.Save();

			var rowsInDB = Factory.Load<JobAddressAdditionalInfo>(new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, commonConsol.PK));

			AssertEquals(2, rowsInDB.Length);

			AssertEquals("ROA", commonConsol.CFSDepartureByTransportMode);
			AssertEquals("IWT", commonConsol.CFSArrivalByTransportMode);

			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = Guid.Empty;
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = Guid.Empty;

			Factory.Save();

			rowsInDB = Factory.Load<JobAddressAdditionalInfo>(new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, commonConsol.PK));

			AssertEquals(0, rowsInDB.Length);

			Assert(commonConsol.CFSDepartureByTransportMode_ReadOnly);
			Assert(commonConsol.CFSArrivalByTransportMode_ReadOnly);

			AssertEquals(string.Empty, commonConsol.CFSDepartureByTransportMode);
			AssertEquals(string.Empty, commonConsol.CFSArrivalByTransportMode);
		}

		public void TestAddressAdditionalInfo_Has_Default_Value_When_DependentAdress_Is_Reset()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.AddAddressType(OrgAddressType.Office);
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.AddAddressType(OrgAddressType.Office);
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_RL_NKRelatedPortCode = "AUML";

			var commonConsol = Factory.New<CommonConsol>();
			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = org1.PK;
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = org1.PK;
			commonConsol.CFSDepartureByTransportMode = "RAI";
			commonConsol.CFSArrivalByTransportMode = "RAI";

			Factory.Save();

			AssertEquals("RAI", commonConsol.CFSDepartureByTransportMode);
			AssertEquals("RAI", commonConsol.CFSArrivalByTransportMode);

			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = org2.PK;
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = org2.PK;
			commonConsol.CFSDepartureByTransportMode = "IWT";
			commonConsol.CFSArrivalByTransportMode = "IWT";

			Factory.Save();

			AssertEquals("IWT", commonConsol.CFSDepartureByTransportMode);
			AssertEquals("IWT", commonConsol.CFSArrivalByTransportMode);

			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = org1.PK;
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = org1.PK;

			AssertEquals(string.Empty, commonConsol.CFSDepartureByTransportMode);
			AssertEquals(string.Empty, commonConsol.CFSArrivalByTransportMode);

			commonConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK = org2.PK;
			commonConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = org2.PK;

			AssertEquals(string.Empty, commonConsol.CFSDepartureByTransportMode);
			AssertEquals(string.Empty, commonConsol.CFSArrivalByTransportMode);
		}

		#endregion
	}
}
