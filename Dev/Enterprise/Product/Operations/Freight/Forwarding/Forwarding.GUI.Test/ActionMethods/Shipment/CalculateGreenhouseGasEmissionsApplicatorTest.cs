using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.ApiClient;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Moq;
using NUnit.Framework;
using CO2eBusinessTestHelper = Enterprise.Freight.CarbonEmissions.Business.Testing.CO2eTestHelper;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CalculateGreenhouseGasEmissionsApplicator))]
	public class CalculateGreenhouseGasEmissionsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestGHGCalculationOfValidNumberOfShipments_Ehub()
		{
			using (CO2eBusinessTestHelper.MockCO2eFeatureControl())
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			{
				var shipments = new ForwardingShipment[7];
				for (var i = 0; i < 7; i++)
				{
					var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
					ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

					shipment.JS_UniqueConsignRef = $"S0000000{i + 1}";
					shipments[i] = shipment;
				}

				Factory.Save();

				ApplyApplicator(shipments, @"INFO: S00000001: The greenhouse gas emissions calculation has been requested.
INFO: S00000001: Processing
SUCCESS: S00000001: Updated
INFO: S00000002: The greenhouse gas emissions calculation has been requested.
INFO: S00000002: Processing
SUCCESS: S00000002: Updated
INFO: S00000003: The greenhouse gas emissions calculation has been requested.
INFO: S00000003: Processing
SUCCESS: S00000003: Updated
INFO: S00000004: The greenhouse gas emissions calculation has been requested.
INFO: S00000004: Processing
SUCCESS: S00000004: Updated
INFO: S00000005: The greenhouse gas emissions calculation has been requested.
INFO: S00000005: Processing
SUCCESS: S00000005: Updated
INFO: S00000006: The greenhouse gas emissions calculation has been requested.
INFO: S00000006: Processing
SUCCESS: S00000006: Updated
INFO: S00000007: The greenhouse gas emissions calculation has been requested.
INFO: S00000007: Processing
SUCCESS: S00000007: Updated
INFO: Greenhouse gas emissions calculation request is completed, please check above log for details.");

				foreach (var shipment in shipments)
				{
					AssertEquals("Shipment should have CO2e status pending.", CO2eStatusList.Codes.Pending, shipment.GetCO2eStatus());
				}
			}
		}

		public void TestGHGCalculationOfValidNumberOfShipments_API()
		{
			using (CO2eBusinessTestHelper.MockCO2eFeatureControl())
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			{
				var client = new Mock<IApiClient>();
				client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));

				ObjectFactory.Substitute("HttpClient", client.Object);

				var shipments = new ForwardingShipment[7];
				for (var i = 0; i < 7; i++)
				{
					var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
					ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

					shipment.JS_UniqueConsignRef = $"S0000000{i + 1}";
					shipments[i] = shipment;
				}

				Factory.Save();

				ApplyApplicator(shipments, @"INFO: S00000001: The greenhouse gas emissions calculation has been requested.
INFO: S00000001: Processing
SUCCESS: S00000001: Updated
INFO: S00000002: The greenhouse gas emissions calculation has been requested.
INFO: S00000002: Processing
SUCCESS: S00000002: Updated
INFO: S00000003: The greenhouse gas emissions calculation has been requested.
INFO: S00000003: Processing
SUCCESS: S00000003: Updated
INFO: S00000004: The greenhouse gas emissions calculation has been requested.
INFO: S00000004: Processing
SUCCESS: S00000004: Updated
INFO: S00000005: The greenhouse gas emissions calculation has been requested.
INFO: S00000005: Processing
SUCCESS: S00000005: Updated
INFO: S00000006: The greenhouse gas emissions calculation has been requested.
INFO: S00000006: Processing
SUCCESS: S00000006: Updated
INFO: S00000007: The greenhouse gas emissions calculation has been requested.
INFO: S00000007: Processing
SUCCESS: S00000007: Updated
INFO: Greenhouse gas emissions calculation request is completed, please check above log for details.");

				foreach (var shipment in shipments)
				{
					AssertEquals("Shipment should have CO2e status current.", CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
					AssertEquals("Shipment should have TotalCO2e populated.", 10000m, shipment.GetTotalCO2e());
				}
			}

			ObjectFactory.DisposeSubstitutions();
		}

		public void TestGHGCalculationOfInvalidNumberOfShipments()
		{
			var shipments = new ForwardingShipment[26];
			for (var i = 0; i < 26; i++)
			{
				var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				shipment.JS_UniqueConsignRef = $"S0000000{i + 1}";
				shipments[i] = shipment;
			}

			Factory.Save();

			ApplyApplicator(shipments, @"ERROR: Please reduce the number of selected jobs, the maximum allowed for this action is 25.");

			foreach (var shipment in shipments)
			{
				AssertNotEquals("Shipment should not have CO2e status pending.", CO2eStatusList.Codes.Pending, shipment.GetCO2eStatus());
			}
		}

		public void TestGHGCalculationDoesNotStopOnFailure()
		{
			using (CO2eBusinessTestHelper.MockCO2eFeatureControl())
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			{
				var shipment1 = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment1.JS_UniqueConsignRef = "S00000001";

				var invalidShipment2 = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				invalidShipment2.JS_ActualWeight = 0;
				invalidShipment2.JS_UniqueConsignRef = "S00000002";

				var shipment3 = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment3.JS_UniqueConsignRef = "S00000003";

				ChildEditableService.SetState(shipment1.Factory, ChildEditableServiceStates.Shipment);
				ChildEditableService.SetState(invalidShipment2.Factory, ChildEditableServiceStates.Shipment);
				ChildEditableService.SetState(shipment3.Factory, ChildEditableServiceStates.Shipment);

				var shipments = new[] { shipment1, invalidShipment2, shipment3 };

				Factory.Save();

				ApplyApplicator(shipments, @"INFO: S00000001: The greenhouse gas emissions calculation has been requested.
INFO: S00000001: Processing
SUCCESS: S00000001: Updated
ERROR: S00000002: Error: The greenhouse gas emissions calculation cannot be requested because the following mandatory input is missing or invalid:
Shipment > Basic Registration > Weight
INFO: S00000003: The greenhouse gas emissions calculation has been requested.
INFO: S00000003: Processing
SUCCESS: S00000003: Updated
INFO: Greenhouse gas emissions calculation request is completed, please check above log for details.");

				AssertEquals("Shipment with valid data should have CO2e status pending.", CO2eStatusList.Codes.Pending, shipments[0].GetCO2eStatus());
				AssertNotEquals("Shipment with invalid data should not have CO2e status pending.", CO2eStatusList.Codes.Pending, shipments[1].GetCO2eStatus());
				AssertEquals("Shipment with valid data should have CO2e status Pending.", CO2eStatusList.Codes.Pending, shipments[2].GetCO2eStatus());
			}
		}

		public void TestGHGCalculation_StatusIsCUR_RequestNotRequired()
		{
			using (CO2eBusinessTestHelper.MockCO2eFeatureControl())
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			{
				var client = new Mock<IApiClient>();
				ObjectFactory.Substitute("HttpClient", client.Object);

				var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment.JS_UniqueConsignRef = "S00000001";
				shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				Factory.Save();

				ApplyApplicator(new[] { shipment }, @"INFO: S00000001: The greenhouse gas emissions calculation has been requested.
INFO: S00000001: Processing
INFO: S00000001: Greenhouse Gas calculation not required because the CO2e value is current.
INFO: Greenhouse gas emissions calculation request is completed, please check above log for details.");

				AssertEquals("Shipment should have CO2e status current.", CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
				client.Verify(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
			}

			ObjectFactory.DisposeSubstitutions();
		}

		public void TestGHGCalculation_ServiceUnavailable_API()
		{
			using (CO2eBusinessTestHelper.MockCO2eFeatureControl())
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			{
				var client = new Mock<IApiClient>();
				var svcUnavailable = new ApiResponse<EmissionResult>(
					new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.ServiceUnavailable,
						Content = new StringContent("Service Unavailable"),
						RequestMessage = new HttpRequestMessage(
											 HttpMethod.Post,
											 "https://api.co2e.wtg.zone/shipment/")
					},
					null,
					client.Object);

				client.Setup(x => x.PostAsync<EmissionResult>(
								It.IsAny<string>(),
								It.IsAny<string>(),
								It.IsAny<CancellationToken>()))
					  .ReturnsAsync(svcUnavailable);

				ObjectFactory.Substitute("HttpClient", client.Object);

				var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment.JS_UniqueConsignRef = "S00000001";
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				Factory.Save();

				ApplyApplicator(new[] { shipment },
					@"INFO: S00000001: The greenhouse gas emissions calculation has been requested.
INFO: S00000001: Processing
ERROR: S00000001: Issue communicating with server. Please try again later.
INFO: Greenhouse gas emissions calculation request is completed, please check above log for details.");
			}

			ObjectFactory.DisposeSubstitutions();
		}

		[ExpectNoExceptions]
		public void TestGHGCalculation_ShipmentWithTransportBookings_ValidationFails()
		{
			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));

			ObjectFactory.Substitute("HttpClient", client.Object);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl())
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			{
				var shipment1 = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment1.JS_UniqueConsignRef = "S00000001";
				CO2eTestHelper.CreateTransportBooking(shipment1, "PIC", Factory);

				var shipment2 = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment2.JS_UniqueConsignRef = "S00000002";
				var shipment2DtbBooking = CO2eTestHelper.CreateTransportBooking(shipment2, "DLV", Factory);
				shipment2DtbBooking.Instructions[0].Address.E2_OA_Address = ZGuid.Empty;

				var shipment3 = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
				shipment3.JS_UniqueConsignRef = "S00000003";
				CO2eTestHelper.CreateTransportBooking(shipment3, "PIC", Factory);

				ChildEditableService.SetState(shipment1.Factory, ChildEditableServiceStates.Shipment);
				ChildEditableService.SetState(shipment2.Factory, ChildEditableServiceStates.Shipment);
				ChildEditableService.SetState(shipment3.Factory, ChildEditableServiceStates.Shipment);

				var shipments = new[] { shipment1, shipment2, shipment3 };

				Factory.Save();

				ApplyApplicator(shipments, @"INFO: S00000001: The greenhouse gas emissions calculation has been requested.
INFO: S00000001: Processing
SUCCESS: S00000001: Updated
ERROR: S00000002: Error: The greenhouse gas emissions calculation cannot be requested because the following mandatory input is missing or invalid:
Transport Booking TB00000002: Transport Booking > Details > Instruction '0' > Address is empty.
INFO: S00000003: The greenhouse gas emissions calculation has been requested.
INFO: S00000003: Processing
SUCCESS: S00000003: Updated
INFO: Greenhouse gas emissions calculation request is completed, please check above log for details.");

				AssertEquals("Shipment with valid data should have CO2e status current.", CO2eStatusList.Codes.Current, shipments[0].GetCO2eStatus());
				AssertEquals("Shipment with failed validation should have CO2e status not calculated.", CO2eStatusList.Codes.NotCalculated, shipments[1].GetCO2eStatus());
				AssertEquals("Shipment with valid data should have CO2e status current.", CO2eStatusList.Codes.Current, shipments[2].GetCO2eStatus());
			}

			ObjectFactory.DisposeSubstitutions();
		}
	}
}
