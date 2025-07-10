using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RatesServiceClientTests : TestCaseWithFactory
	{
		#region Registry

		public void TestSearchRates_WithRegistryRateSubscription_CargoSphere_Disabled()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(false))
			{
				var request = new RatesSearchRequest
				{
					RatesQuery = new RatesQuery
					{
						ContainerMode = new[] { WRConstants.ContainerModes.FCL },
						TransportMode = new[] { WRConstants.TransportModes.SEA }
					},
				};

				var response = ratesServiceClient.SearchAsync(request, null).Result;
				AssertEquals(0, response.Rates.Length);
				AssertCollectionContains(
					"Request will not be sent to Rates Service because CGSP integration is disabled in the registry",
					response.Warnings
				);

				clientMock.Verify(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default), Times.Never);
				clientMock.VerifyAll();
			}
		}

		public void TestSearchRates_WithRegistryRateSubscription_CargoSphere_Enabled()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			{
				clientMock.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default)).Returns(Task.FromResult(new RatesSearchResponse()
				{
					Rates = new[] { new Rate() }
				}));

				var request = new RatesSearchRequest
				{
					RatesQuery = new RatesQuery
					{
						ContainerMode = new[] { WRConstants.ContainerModes.FCL },
						TransportMode = new[] { WRConstants.TransportModes.SEA }
					},
				};

				var response = ratesServiceClient.SearchAsync(request, null).Result;
				AssertGreaterThan(response.Rates.Length, 0);

				clientMock.Verify(x => x.SearchAsync(It.Is<RatesSearchRequest>(y => y.RatesQuery.AcceptedProviders.SequenceEqual(new[] { WRConstants.RateProviders.CargoSphere })), null, default), Times.Once);
				clientMock.VerifyAll();
			}
		}

		public void TestSearchRates_WithRegistryRateSubscription_Cargoguide_Disabled()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(false))
			{
				var request = new RatesSearchRequest
				{
					RatesQuery = new RatesQuery
					{
						ContainerMode = new[] { Core.Constants.ContainerModes.Loose },
						TransportMode = new[] { WRConstants.TransportModes.AIR }
					},
				};

				var response = ratesServiceClient.SearchAsync(request, null).Result;

				AssertEquals(0, response.Rates.Length);
				AssertCollectionContains(
					"Request will not be sent to Rates Service because CGGD integration is disabled in the registry",
					response.Warnings
				);

				clientMock.Verify(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default), Times.Never);
				clientMock.VerifyAll();
			}
		}

		public void TestSearchRates_WithRegistryRateSubscription_Cargoguide_Enabled()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			{
				clientMock.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default)).Returns(Task.FromResult(new RatesSearchResponse()
				{
					Rates = new[] { new Rate() }
				}));

				var request = new RatesSearchRequest
				{
					RatesQuery = new RatesQuery
					{
						ContainerMode = new[] { Core.Constants.ContainerModes.Loose },
						TransportMode = new[] { WRConstants.TransportModes.AIR }
					},
				};

				var response = ratesServiceClient.SearchAsync(request, null).Result;

				AssertGreaterThan(response.Rates.Length, 0);

				AssertCollectionNotContains(
					"Request will not be sent to Rates Service because CGGP integration is disabled in the registry",
					response.Warnings
				);

				clientMock.Verify(x => x.SearchAsync(It.Is<RatesSearchRequest>(y => y.RatesQuery.AcceptedProviders.SequenceEqual(new[] { WRConstants.RateProviders.CargoGuide })), null, default), Times.Once);
				clientMock.VerifyAll();
			}
		}

		public void TestRateService_WhenNoRateProviderEnabled_DoNotCallRateService()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(false))
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(false))
			{
				var request = new RatesSearchRequest
				{
					RatesQuery = new RatesQuery
					{
						ContainerMode = new[] { Core.Constants.ContainerModes.Loose },
						TransportMode = new[] { WRConstants.TransportModes.AIR, WRConstants.TransportModes.SEA }
					},
				};

				var response = ratesServiceClient.SearchAsync(request, null).Result;

				AssertCollectionContains(
					"Request will not be sent to Rates Service because CGGD integration is disabled in the registry",
					response.Warnings
				);

				AssertCollectionContains(
					"Request will not be sent to Rates Service because CGSP integration is disabled in the registry",
					response.Warnings
				);

				clientMock.Verify(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default), Times.Never);
			}
		}

		public void TestSearchRates_RatesQuery_MultipleTransportMode_BecomesOneBecauseRegistrySettings()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = Core.Constants.TransportModes.Sea,
					ContainerMode = Core.Constants.ContainerModes.FCL,
					IsSubscriptionEnabled = false
				},
				new RatesServiceRegistrySettings
				{
					TransportMode = Core.Constants.TransportModes.Air,
					ContainerMode = Core.Constants.ContainerModes.Loose,
					IsSubscriptionEnabled = true
				}
			};

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(false))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var request = new RatesSearchRequest
				{
					RatesQuery = new RatesQuery
					{
						ContainerMode = new[] { Core.Constants.ContainerModes.Loose },
						TransportMode = new[] { WRConstants.TransportModes.SEA, WRConstants.TransportModes.AIR }
					},
				};

				clientMock.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default)).Returns(Task.FromResult(new RatesSearchResponse()
				{
					Rates = new[] { new Rate() }
				}));

				var response = ratesServiceClient.SearchAsync(request, null).Result;
				AssertCollectionContains(
					"Request will not be sent to Rates Service because CGSP integration is disabled in the registry",
					response.Warnings
				);
				AssertGreaterThan(response.Rates.Length, 0);

				clientMock.Verify(x => x.SearchAsync(It.Is<RatesSearchRequest>(y => y.RatesQuery.TransportMode.SequenceEqual(new[] { WRConstants.TransportModes.AIR })), null, default), Times.Once);
				clientMock.Verify(x => x.SearchAsync(It.Is<RatesSearchRequest>(y => y.RatesQuery.TransportMode.Contains(WRConstants.TransportModes.SEA)), null, default), Times.Never);
				clientMock.VerifyAll();
			}
		}

		public void TestSearchRates_RatesQuery_EmptyTransportMode_ThrowsException()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			var request = new RatesSearchRequest
			{
				RatesQuery = new RatesQuery
				{
					ContainerMode = new[] { Core.Constants.ContainerModes.Loose, Core.Constants.ContainerModes.FCL },
					TransportMode = Array.Empty<string>()
				},
			};

			AssertExceptionThrown<ArgumentException>("Expect Exception when one list is empty", () =>
			{
				ratesServiceClient.SearchAsync(request, null).GetAwaiter().GetResult();
			});

			AssertExceptionThrown<ArgumentException>("Expect Exception when one list is null", () =>
			{
				request.RatesQuery.TransportMode = null;
				ratesServiceClient.SearchAsync(request, null).GetAwaiter().GetResult();
			});

			clientMock.Verify(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default), Times.Never);
			clientMock.VerifyAll();

			Assert(true); // Test uses Moq validation and fluent assertions
		}

		public void TestSearchRates_RatesQuery_EmptyContainerMode_ThrowsException()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			var ratesServiceClient = new RatesServiceClient(clientMock.Object);

			var request = new RatesSearchRequest
			{
				RatesQuery = new RatesQuery
				{
					ContainerMode = Array.Empty<string>(),
					TransportMode = new[] { WRConstants.TransportModes.AIR }
				}
			};

			AssertExceptionThrown<ArgumentException>("Expect Exception when one list is empty", () =>
			{
				ratesServiceClient.SearchAsync(request, null).GetAwaiter().GetResult();
			});

			AssertExceptionThrown<ArgumentException>("Expect Exception when one list is null", () =>
			{
				request.RatesQuery.ContainerMode = null;
				ratesServiceClient.SearchAsync(request, null).GetAwaiter().GetResult();
			});

			clientMock.Verify(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), null, default), Times.Never);
			clientMock.VerifyAll();

			Assert(true); // Test uses Moq validation and fluent assertions
		}

		#endregion

		#region Non critical exception safety tests

		public void TestGetChargeCodes_WhenNonCriticalExceptionThrows_ErrorWillBeReportedWithMessageContainingCorrelationIDAndServiceURL()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodes(It.IsAny<string>())).Throws(new Exception("ExceptionMessage"));
			clientMock.Setup(c => c.ServiceURL).Returns("SampleServiceURL");
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var chargeCodes = client.GetChargeCodes("SampleCorrelationID");

			var expectedErrorMessage = $@"TraceID: SampleCorrelationID
ServiceURL: SampleServiceURL
ExceptionMessage: ExceptionMessage";

			AssertEquals(0, chargeCodes.Length);
			AssertEquals("GetChargeCodes|Error", ErrorReporter.LastKeyReported);
			AssertEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetChargeCodes_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodes(It.IsAny<string>())).Throws(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var chargeCodes = client.GetChargeCodes();

			AssertEquals(0, chargeCodes.Length);
			AssertEquals("GetChargeCodes|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetChargeCodes_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodes(It.IsAny<string>())).Throws(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetChargeCodes());
		}

		public void TestGetChargeCodesAsync_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodesAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var chargeCodes = client.GetChargeCodesAsync().GetAwaiter().GetResult();

			AssertEquals(0, chargeCodes.Length);
			AssertEquals("GetChargeCodesAsync|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetChargeCodesAsync_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodesAsync(It.IsAny<string>())).ThrowsAsync(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetChargeCodesAsync().GetAwaiter().GetResult());
		}

		public void TestGetCommodityGroups_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetCommodityGroups(It.IsAny<string>())).Throws(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var commodityGroups = client.GetCommodityGroups();

			AssertEquals(0, commodityGroups.Length);
			AssertEquals("GetCommodityGroups|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetCommodityGroups_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetCommodityGroups(It.IsAny<string>())).Throws(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetCommodityGroups());
		}

		public void TestGetCommodityGroupsAsync_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetCommodityGroupsAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var commodityGroups = client.GetCommodityGroupsAsync().GetAwaiter().GetResult();

			AssertEquals(0, commodityGroups.Length);
			AssertEquals("GetCommodityGroupsAsync|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetCommodityGroupsAsync_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetCommodityGroupsAsync(It.IsAny<string>())).ThrowsAsync(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetCommodityGroupsAsync().GetAwaiter().GetResult());
		}

		public void TestGetServiceLevels_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetServiceLevels(It.IsAny<string>())).Throws(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var serviceLevels = client.GetServiceLevels();

			AssertEquals(0, serviceLevels.Length);
			AssertEquals("GetServiceLevels|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetServiceLevels_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetServiceLevels(It.IsAny<string>())).Throws(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetServiceLevels());
		}

		public void TestGetServiceLevelsAsync_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetServiceLevelsAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var serviceLevels = client.GetServiceLevelsAsync().GetAwaiter().GetResult();

			AssertEquals(0, serviceLevels.Length);
			AssertEquals("GetServiceLevelsAsync|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetServiceLevelsAsync_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetServiceLevelsAsync(It.IsAny<string>())).ThrowsAsync(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetServiceLevelsAsync().GetAwaiter().GetResult());
		}

		public void TestGetCodeMapping_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodesWithMappingInfo(It.IsAny<string>())).Throws(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var codeMapping = client.GetChargeCodesWithMappingInfo();

			AssertEquals(0, codeMapping.Length);
			AssertEquals("GetChargeCodesWithMappingInfo|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetCodeMapping_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodesWithMappingInfo(It.IsAny<string>())).Throws(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetChargeCodesWithMappingInfo());
		}

		public void TestGetCodeMappingAsync_WhenNonCriticalExceptionThrows_ReturnsAnEmptyArray()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodesWithMappingInfoAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
			var client = new RatesServiceClient(clientMock.Object);

			ObjectFactory.Get<Enterprise.Integration.Licensing.IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var codeMapping = client.GetChargeCodesWithMappingInfoAsync().GetAwaiter().GetResult();

			AssertEquals(0, codeMapping.Length);
			AssertEquals("GetChargeCodesWithMappingInfoAsync|Error", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetCodeMappingAsync_WhenCriticalExceptionThrows_ExceptionReturns()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock.Setup(c => c.GetChargeCodesWithMappingInfoAsync(It.IsAny<string>())).ThrowsAsync(new OutOfMemoryException());

			var client = new RatesServiceClient(clientMock.Object);

			AssertExceptionThrown(typeof(OutOfMemoryException), () => client.GetChargeCodesWithMappingInfoAsync().GetAwaiter().GetResult());
		}
		#endregion

		#region Security check and access tests

		public void TestRatesServiceClient_WhenSecurityCheckpointNotGranted_Disallowed()
		{
			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var securityTestCases = new[] {
					(false, true, RatesSearchRequest.Operation.Autorating),
					(true, false, RatesSearchRequest.Operation.Autorating),
					(false, true, RatesSearchRequest.Operation.WiseRateSearch),
					(true, false, RatesSearchRequest.Operation.WiseRateSearch)
				};

				foreach (var (cargoguideRateSearchAllowed, cargoSphereRateSearchAllowed, operation) in securityTestCases)
				{
					var logger = new TestLogger();

					Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = cargoguideRateSearchAllowed;
					Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = cargoSphereRateSearchAllowed;

					var isAllowedCargoguide = RatesServiceClient.IsRateSearchAllowed(operation, WRConstants.RateProviders.CargoGuide, logger);
					var isAllowedCargoSphere = RatesServiceClient.IsRateSearchAllowed(operation, WRConstants.RateProviders.CargoSphere, logger);

					// Security checks only for wiserates search. For the autorating operation the security checks are ignored.
					var expectedCargoSphereAllowed = cargoSphereRateSearchAllowed || operation == RatesSearchRequest.Operation.Autorating;
					var expectedCargoguideAllowed = cargoguideRateSearchAllowed || operation == RatesSearchRequest.Operation.Autorating;

					AssertEquals("Expected access for CargoSphere did not match.", expectedCargoSphereAllowed, isAllowedCargoSphere);
					AssertEquals("Expected access for CargoGuide did not match.", expectedCargoguideAllowed, isAllowedCargoguide);

					if (!isAllowedCargoguide)
					{
						AssertCollectionContains(
							"Warning:CGGD won't be accessed as access for current user is denied",
							logger.Warnings
						);
					}
					if (!isAllowedCargoSphere)
					{
						AssertCollectionContains(
							"Warning:CGSP won't be accessed as access for current user is denied",
							logger.Warnings
						);
					}
				}
			}
		}

		public void TestRatesServiceClient_WhenNoEmail_Disallowed()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = string.Empty;

			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var logger = new TestLogger();

				Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = true;
				Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = true;

				var isAllowedCargoguide = RatesServiceClient.IsRateSearchAllowed(RatesSearchRequest.Operation.Autorating, WRConstants.RateProviders.CargoGuide, logger);
				var isAllowedCargoSphere = RatesServiceClient.IsRateSearchAllowed(RatesSearchRequest.Operation.Autorating, WRConstants.RateProviders.CargoSphere, logger);

				AssertEquals(false, isAllowedCargoguide);
				AssertEquals(false, isAllowedCargoSphere);

				var expectedWarnings = new[]
				{
					"Warning:Email address of current user in staff details is mandatory for accessing CGGD",
					"Warning:Email address of current user in staff details is mandatory for accessing CGSP"
				};

				AssertContainsExactElementsInAnyOrder(
					"Log warnings should match the expected warning messages",
					expectedWarnings,
					logger.Warnings
				);
			}
		}

		public void TestRatesServiceClient_ProviderAccountsMapping_WithInvalidUrl()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => !p.ProviderAccounts.Any()),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var client = new RatesServiceClient(clientMock.Object);
			DataRegistryRating.Instance.CGSPApiURLOverride.DataType = new StringRegistryDataType();

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CGSPApiURLOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testserver1"))
			using (DataRegistryRating.Instance.CGGDApiSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoGuideApiSettingsForTest { ApiURL = "testserver2", ApiVersion = "1.2" }))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.SEA },
							ContainerMode = new[] { WRConstants.ContainerModes.FCL }
						}
					}).GetAwaiter().GetResult();

				var warningsCount = result.Warnings.Count(w => w.Contains("CargoSphere API URL is invalid.  Please send eRequest to WTG."));
				AssertEquals("Expected one warning for invalid CargoSphere API URL.", 1, warningsCount);
			}

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CGSPApiURLOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testserver1"))
			using (DataRegistryRating.Instance.CGGDApiSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoGuideApiSettingsForTest { ApiURL = "testserver2", ApiVersion = "1.2" }))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.AIR },
							ContainerMode = new[] { Core.Constants.ContainerModes.Loose }
						}
					}).GetAwaiter().GetResult();

				var warningsCount = result.Warnings.Count(w => w.Contains("Cargoguide API URL is invalid.  Please send eRequest to WTG."));
				AssertEquals("Expected one warning for invalid Cargoguide API URL.", 1, warningsCount);
			}

			clientMock.VerifyAll();
		}

		class CargoGuideApiSettingsForTest : CargoGuideApiSettings
		{
			public override ZString ApiURL
			{
				get { return fApiURL; }
				set { fApiURL = value; }
			}

			ZString fApiURL = "";
		}

		public void TestRatesServiceClient_ProviderAccountsMapping_NoOverridenURLs()
		{
			var cargoSphereAccount = @"testlogin
testpass
testcode
";

			var cargoGuideAccount = @"testlogin
testpass

";

			var cargoSphereAccountEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cargoSphereAccount));
			var cargoguideAccountEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cargoGuideAccount));

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.ProviderAccounts[WiseRatesProvider.CargoSphereProviderCode] == cargoSphereAccountEncoded),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var client = new RatesServiceClient(clientMock.Object);
			DataRegistryRating.Instance.CGSPApiURLOverride.DataType = new StringRegistryDataType();

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoSphereCredentials { Login = "testlogin", Password = "testpass", SystemCode = "testcode" }))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.SEA },
							ContainerMode = new[] { WRConstants.ContainerModes.FCL }
						}
					}).GetAwaiter().GetResult();

				AssertEquals(0, result.Warnings.Length);
			}

			clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.ProviderAccounts[WiseRatesProvider.CargoGuideProviderCode] == cargoguideAccountEncoded),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			client = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoGuideCredentials { Login = "testlogin", Password = "testpass" }))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.AIR },
							ContainerMode = new[] { Core.Constants.ContainerModes.Loose }
						}
					}).GetAwaiter().GetResult();

				AssertEquals(0, result.Warnings.Length);
			}

			clientMock.VerifyAll();
		}

		public void TestRatesServiceClient_ProviderAccountsMapping_WithOverridenURLs()
		{
			var cargoSphereAccount = @"testlogin
testpass
testcode
https://newcargosphere.biz";

			var cargoGuideAccount = @"testlogin
testpass
https://newcargoguide.com
1.2";

			var cargoSphereAccountEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cargoSphereAccount));
			var cargoguideAccountEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cargoGuideAccount));

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.ProviderAccounts[WiseRatesProvider.CargoSphereProviderCode] == cargoSphereAccountEncoded),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var client = new RatesServiceClient(clientMock.Object);
			DataRegistryRating.Instance.CGSPApiURLOverride.DataType = new StringRegistryDataType();

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoSphereCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoSphereCredentials { Login = "testlogin", Password = "testpass", SystemCode = "testcode" }))
			using (DataRegistryRating.Instance.CGSPApiURLOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://newcargosphere.biz"))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.SEA },
							ContainerMode = new[] { WRConstants.ContainerModes.FCL }
						}
					}).GetAwaiter().GetResult();

				AssertEquals(0, result.Warnings.Length);
			}

			clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.ProviderAccounts[WiseRatesProvider.CargoGuideProviderCode] == cargoguideAccountEncoded),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			client = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoGuideCredentials { Login = "testlogin", Password = "testpass" }))
			using (DataRegistryRating.Instance.CGGDApiSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoGuideApiSettings { ApiURL = "https://newcargoguide.com", ApiVersion = "1.2" }))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.AIR },
							ContainerMode = new[] { Core.Constants.ContainerModes.Loose }
						}
					}).GetAwaiter().GetResult();

				AssertEquals(0, result.Warnings.Length);
			}

			clientMock.VerifyAll();
		}

		public void TestRatesServiceClient_SSOWithOverridenURLs()
		{
			var cargoSphereAccount = @"


https://newcargosphere.biz";

			var cargoGuideAccount = @"

https://newcargoguide.com
1.2";
			var cargoSphereAccountEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cargoSphereAccount));
			var cargoguideAccountEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cargoGuideAccount));

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.ProviderAccounts[WiseRatesProvider.CargoSphereProviderCode] == cargoSphereAccountEncoded),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var client = new RatesServiceClient(clientMock.Object);
			DataRegistryRating.Instance.CGSPApiURLOverride.DataType = new StringRegistryDataType();

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoSphere(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CGSPApiURLOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://newcargosphere.biz"))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.SEA },
							ContainerMode = new[] { WRConstants.ContainerModes.FCL }
						}
					}).GetAwaiter().GetResult();

				AssertEquals(0, result.Warnings.Length);
			}

			clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.Is<RatesSearchRequest>(p => p.ProviderAccounts[WiseRatesProvider.CargoGuideProviderCode] == cargoguideAccountEncoded),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			client = new RatesServiceClient(clientMock.Object);

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (RatesServiceClientTestHelper.TemporarilySetRegistryValueForCargoguide(true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CGGDApiSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CargoGuideApiSettings { ApiURL = "https://newcargoguide.com", ApiVersion = "1.2" }))
			{
				var result = client.SearchAsync(
					new RatesSearchRequest
					{
						RatesQuery = new RatesQuery
						{
							TransportMode = new[] { WRConstants.TransportModes.AIR },
							ContainerMode = new[] { Core.Constants.ContainerModes.Loose }
						}
					}).GetAwaiter().GetResult();

				AssertEquals(0, result.Warnings.Length);
			}

			clientMock.VerifyAll();
		}

		#endregion
	}

	public static class RatesServiceClientTestHelper
	{
		public static IDisposable TemporarilyEnsureCurrentUserHasEmail()
		{
			var initialEmail = GlbStaff.CurrentUser.GS_EmailAddress;
			GlbStaff.CurrentUser.GS_EmailAddress = "blah@example.com";

			return new TemporaryDisposable(() => GlbStaff.CurrentUser.GS_EmailAddress = initialEmail);
		}

		public static IDisposable TemporarilySetRegistryValueForCargoSphere(bool value)
		{
			return TemporarilySetCargoSphereEnabled(value);
		}

		public static IDisposable TemporarilySetRegistryValueForCargoguide(bool value)
		{
			return TemporarilySetCargoguideEnabled(value);
		}

		static IDisposable TemporarilySetCargoSphereEnabled(bool enabled)
			=>
			DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);

		static IDisposable TemporarilySetCargoguideEnabled(bool enabled)
			=>
			DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);

		class TemporaryDisposable : IDisposable
		{
			readonly Action action;

			public TemporaryDisposable(Action action)
			{
				this.action = action;
			}

			public void Dispose()
			{
				action();
			}
		}
	}
}
