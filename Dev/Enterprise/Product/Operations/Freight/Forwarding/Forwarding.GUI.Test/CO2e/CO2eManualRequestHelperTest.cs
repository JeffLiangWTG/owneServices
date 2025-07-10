using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class CO2eManualRequestHelperTest : BaseFreightTest
	{
		[TestDate(2023, 03, 03)]
		public void TestSendRequestManually_Shipment()
		{
			var shipment = CreateShipment();
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var result = CO2eManualRequestHelper.SendRequestManually((ICO2eCalculationSupporter)shipment, new NotificationBuffer());
				AssertEquals(CO2eResultType.EhubSuccess, result.Type);
				AssertEquals("Pre-condition", false, shipment.HasChanges);
				var newFactory = new BusinessObjectFactory();
				var messages = newFactory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);

				var message = messages[0];
				Assert(message.IsInDatabase);
				CombineAssertions("EDI Message sent", () =>
				{
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertContains("message.EM_MessageText", "UniversalShipment", message.EM_MessageText);
					AssertUniversalShipment(message.EM_MessageText, shipment);
				});

				var interchange = newFactory.Load<EDIInterchange>(message.EM_EI);
				Assert(interchange.IsInDatabase);
				CombineAssertions("EDI Interchange", () =>
				{
					AssertNotNull(interchange);
					AssertEquals("EMISSION_CALCULATOR", interchange.EI_To);
				});
				CombineAssertions("GHG Events", () =>
				{
					AssertGHGEvent(shipment.Logs);
					AssertGHGEvent(shipment.Transports[0].Logs);
					AssertGHGEvent(shipment.Transports[1].Logs);
				});

				var shipmentInNewFac = newFactory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(shipment.PK);
				AssertEquals(CO2eStatusList.Codes.Pending, ((ICO2eProvider)shipmentInNewFac).GetCO2eStatus());
				foreach (var transport in ((CommonShipment)shipmentInNewFac).TransportsIncludingRelated.Cast<Transport>())
				{
					AssertEquals(CO2eStatusList.Codes.Pending, transport.GetCO2eStatus());
					AssertSailing(transport);
				}
			}
		}

		public void TestSendRequestManually_ExceptionIsHandled()
		{
			var shipment = CreateShipment();
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
				{
					if (factory.NameForDebugging == "CO2e Calculation Request Sender")
					{
						throw new Exception("Hello world");
					}
				});

				var notifications = new NotificationBuffer();
				CO2eProcessResult result = CO2eProcessResult.Empty;
				AssertNoExceptionThrown(() => result = CO2eManualRequestHelper.SendRequestManually((ICO2eCalculationSupporter)shipment, notifications));
				AssertEquals(CO2eResultType.EhubFail, result.Type);
				AssertContains("Sending greenhouse gas emissions calculation request failed due to an error. Please try again.", notifications.AsString);
			}
		}

		[TestDate(2023, 03, 03)]
		public void TestSendRequestManually_WithAddressValidation()
		{
			// Arrange
			var shipment = CreateShipment();
			shipment.JS_OA_ImportReleaseDepot = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();

			var addressValidationManagerMock = new Mock<IAddressesValidationManager>();
			addressValidationManagerMock
				.Setup(manager => manager.Validate());

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					var result = CO2eManualRequestHelper.SendRequestManually((ICO2eCalculationSupporter)shipment, new NotificationBuffer(), addressValidationManagerMock.Object);

					// Assert
					AssertEquals(CO2eResultType.EhubSuccess, result.Type);
					addressValidationManagerMock
						.Verify(manager => manager.Validate(), Times.Once);
				}

				using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					// Act
					addressValidationManagerMock.Reset();
					var result = CO2eManualRequestHelper.SendRequestManually((ICO2eCalculationSupporter)shipment, new NotificationBuffer(), addressValidationManagerMock.Object);

					// Assert
					AssertEquals(CO2eResultType.EhubSuccess, result.Type);
					addressValidationManagerMock
						.Verify(manager => manager.Validate(), Times.Never);
				}
			}
		}

		[TestDate(2023, 03, 03)]
		public void TestSendRequestManually_NotRequired()
		{
			var shipment = CreateShipment();
			((ICO2eProvider)shipment).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			var notifications = new NotificationBuffer();

			var recalculationCheckerMock = new Mock<ICO2eRecalculationChecker>();
			recalculationCheckerMock
				.Setup(checker => checker.ShouldRecalculate(It.IsAny<ICO2eCalculationSupporter>()))
				.Returns(false);

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var result = CO2eManualRequestHelper.SendRequestManually(
					(ICO2eCalculationSupporter)shipment,
					notifications,
					recalculationChecker: recalculationCheckerMock.Object);

				AssertEquals(CO2eResultType.NotRequired, result.Type);

				var newFactory = new BusinessObjectFactory();
				var messages = newFactory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(0, messages.Length);

				CombineAssertions("GHG Events", () =>
				{
					AssertNoGHGEvent(shipment.Logs);
				});
			}
		}

		void AssertNoGHGEvent(Logs logs)
		{
			var ghgEventCount = logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count();
			AssertEquals("No GHG event should be created", 0, ghgEventCount);
		}

		void AssertGHGEvent(Logs logs)
		{
			AssertEquals("New transport GHG event created", 1, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", "|TYP=Requested", transportGHGEvent.SL_Reference);
		}

		void AssertSailing(Transport transport)
		{
			var sailing = transport.Sailing;
			if (transport.JW_IsLinked)
			{
				AssertNotNull(sailing);
				AssertGHGEvent(sailing.Logs);
				AssertEquals(CO2eStatusList.Codes.Pending, sailing.GetCO2eStatus());
			}
			else
			{
				AssertNull(sailing);
			}
		}

		void AssertUniversalShipment(ZString message, CommonShipment shipmentBO)
		{
			var universalShipment = XDocument.Parse(message).Root;
			AssertNotNull(universalShipment);
			var ns = universalShipment.GetDefaultNamespace();
			var dataContext = universalShipment.Elements(ns + "Shipment").Elements(ns + "DataContext").Single();
			var shipmentDataSource = dataContext?.Elements(ns + "DataSource").Single();
			AssertNotNull(shipmentDataSource);
			AssertEquals(shipmentBO.JS_UniqueConsignRef, shipmentDataSource?.Elements(ns + "Key").Single().Value);
			AssertEquals("ForwardingShipment", shipmentDataSource?.Elements(ns + "Type").Single().Value);

			var shipment = universalShipment.Element(ns + "Shipment");
			AssertEquals("UniversalShipment.Shipment.PortOfLoading", shipmentBO.JS_RL_NKOrigin, shipment.Element(ns + "PortOfLoading").Value);
			AssertEquals("UniversalShipment.Shipment.PortOfDischarge", shipmentBO.JS_RL_NKDestination, shipment.Element(ns + "PortOfDischarge").Value);
			AssertEquals("UniversalShipment.Shipment.TotalWeight", shipmentBO.JS_ActualWeight.ToString(), shipment.Element(ns + "TotalWeight").Value);
			AssertEquals("UniversalShipment.Shipment.TotalWeightUnit", shipmentBO.ShipmentWeightUnit, shipment.Element(ns + "TotalWeightUnit").Value);
			AssertEquals("UniversalShipment.Shipment.TransportMode", shipmentBO.JS_TransportMode, shipment.Element(ns + "TransportMode").Value);

			var transportLegs = shipment.Element(ns + "TransportLegCollection").Elements().ToArray();
			AssertEquals(shipmentBO.TransportsIncludingRelated.Count, transportLegs.Length);
		}

		CommonShipment CreateShipment()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			shipment.JS_UniqueConsignRef = "S0123";
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "VNVNH";
			AddTransportLeg(shipment, "SEA", "AUSYD", "SGSIN", "VOY001", "CARRIER1", true);
			AddTransportLeg(shipment, "SEA", "SGSIN", "VNVNH", "VOY002", "CARRIER2", false);

			Factory.Save();
			return shipment;
		}

		void AddTransportLeg(CommonShipment shipmentBO, string mode, string loadPort, string discPort, string voyageFlightNum, string carrierCode, bool isLinked)
		{
			var leg = shipmentBO.Transports.AddNew();
			leg.JW_TransportMode = mode;
			leg.JW_RL_NKLoadPort = loadPort;
			leg.JW_RL_NKDiscPort = discPort;
			leg.JW_VoyageFlight = voyageFlightNum;
			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = carrierCode;
			leg.JW_OA_CarrierAddress = carrier1.MainAddress.PK;
			leg.JW_IsLinked = isLinked;
			leg.JW_JX = AddSailing(mode, loadPort, discPort, voyageFlightNum, carrier1.PK).PK;
		}

		JobSailing AddSailing(string mode, string loadPort, string discPort, string voyageFlightNum, ZGuid carrier)
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			jobVoyage.JV_VoyageFlight = voyageFlightNum;
			jobVoyage.JV_OH_Line = carrier;
			jobVoyage.JV_AirSeaRoad = mode;

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = discPort;
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			return sailing;
		}
	}
}
