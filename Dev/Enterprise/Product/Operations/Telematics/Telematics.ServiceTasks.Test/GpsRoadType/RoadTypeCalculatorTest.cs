using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks.GpsRoadType;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.Telematics.ServiceTasks.Test.GpsRoadType
{
	class RoadTypeCalculatorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpClientFactoryMock = new Mock<IHttpClientFactory>();
			loggerMock = new Mock<ILogger>();
			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			httpClientFactoryMock.Setup(factory => factory.Create(It.IsAny<TimeSpan>())).Returns(httpClient);
			roadTypeCalculator = new RoadTypeCalculator(httpClientFactoryMock.Object, loggerMock.Object);
			TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TestUseUriFromAvsWebServicesRegistry()
		{
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Primary.ServiceUri = "https://fortest.domain/fakeapi", Factory))
			{
				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				var factoryMock = new Mock<IHttpClientFactory>();
				factoryMock.Setup(factory => factory.Create(It.IsAny<TimeSpan>())).Returns(httpClient);

				var calculator = new RoadTypeCalculator(factoryMock.Object, loggerMock.Object);

				AssertEquals("https://fortest.domain/fakeapi/", httpClient.BaseAddress);
			}
		}

		public void TestCorrectLocationsAreMarkedAsPublicOrPrivate()
		{
			CombineAssertions(() =>
			{
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Private),
						(1.12345, 1.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Public),
					},
					$@"[{{""Coordinate"":{{""Latitude"":10.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}},{{""Coordinate"":{{""Latitude"":1.12345,""Longitude"":1.543210}},""IsLandParcel"":false,""ErrorMessage"":null}}]");
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Private),
						(10.12345, 51.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Public),
					},
					$@"[{{""Coordinate"":{{""Latitude"":80.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}},{{""Coordinate"":{{""Latitude"":10.12345,""Longitude"":51.543210}},""IsLandParcel"":false,""ErrorMessage"":null}}]");
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.54321, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Private),
					},
					$@"[{{""Coordinate"":{{""Latitude"":80.123451,""Longitude"":12.5432101}},""IsLandParcel"":true,""ErrorMessage"":null}}]");
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.543211, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Private),
					},
					$@"[{{""Coordinate"":{{""Latitude"":80.123459,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}}]");
				Test(
					// Floating point math means we can hit 12.54322-12.54321 == 0.999... In this rare edge case i think we can accept the 1.11m inaccuracy for the road type
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.54321, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Private),
					},
					$@"[{{""Coordinate"":{{""Latitude"":80.12345,""Longitude"":12.54322}},""IsLandParcel"":true,""ErrorMessage"":null}}]");
			});

			void Test(List<(double lat, double lon, string roadType, string endResult)> locations, string endpointResponse)
			{
				// Arrange
				var glbLocations = locations
					.Select(tuple =>
					{
						var glbLoc = Factory.New<GlbDeviceLocation>();
						glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
						glbLoc.V2_RoadType = tuple.roadType;
						return glbLoc;
					})
					.ToList();

				httpMessageHandlerMock.Reset();
				var returnCode = HttpStatusCode.Accepted;

				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent(endpointResponse),
					});

				// Act
				roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken());

				// Assert
				for (var i = 0; i < locations.Count; i++)
				{
					AssertEquals(locations[i].endResult, glbLocations[i].V2_RoadType);
				}
				loggerMock.VerifyNoOtherCalls();
			}
		}

		public void TestRoadTypeSkipsRecordsIfRegistryMarkedNotToProcess()
		{
			// Arrange
			var locations = new List<(double lat, double lon, string roadType, string endResult)>
			{
				(80.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Public),
				(10.12345, 51.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Public),
			};
			TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			loggerMock.Reset();
			var glbLocations = locations
				.Select(tuple =>
				{
					var glbLoc = Factory.New<GlbDeviceLocation>();
					glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
					glbLoc.V2_RoadType = tuple.roadType;
					return glbLoc;
				})
				.ToList();

			// Act
			roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken());

			// Assert
			httpMessageHandlerMock.VerifyNoOtherCalls();
			foreach (var location in locations)
			{
				var glbLocation = glbLocations.Single(loc => loc.Location.Latitude.Value == location.lat && loc.Location.Longitude.Value == location.lon);
				AssertEquals(location.endResult, glbLocation.V2_RoadType);
			}
		}

		[ExpectNoExceptions]
		public void TestExceptionRaisedForMismatchedLatLongReturn()
		{
			CombineAssertions(() =>
			{
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(10.12345, 12.54321, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
						(1.12345, 1.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Public),
					},
					$@"[{{""Coordinate"":{{""Latitude"":100.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}},{{""Coordinate"":{{""Latitude"":1.12345,""Longitude"":1.543210}},""IsLandParcel"":false,""ErrorMessage"":null}}]",
					new[] { $"Returned location does not match given location, expected: (10.12345, 12.54321) received: (100.12345, 12.54321)" });
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(10.12345, 12.54321, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
						(1.12345, 1.54321, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"[{{""Coordinate"":{{""Latitude"":100.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}},{{""Coordinate"":{{""Latitude"":50.12345,""Longitude"":1.543210}},""IsLandParcel"":false,""ErrorMessage"":null}}]",
					new[]
					{
						"Returned location does not match given location, expected: (10.12345, 12.54321) received: (100.12345, 12.54321)",
						"Returned location does not match given location, expected: (1.12345, 1.54321) received: (50.12345, 1.54321)"
					});
				Test(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(10.12345, 12.5432098, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"[{{""Coordinate"":{{""Latitude"":10.1234599,""Longitude"":12.5432199}},""IsLandParcel"":true,""ErrorMessage"":null}},{{""Coordinate"":{{""Latitude"":50.12345,""Longitude"":1.543210}},""IsLandParcel"":false,""ErrorMessage"":null}}]",
					new[]
					{
						"Returned location does not match given location, expected: (10.12345, 12.5432098) received: (10.1234599, 12.5432199)",
					});
			});

			void Test(List<(double lat, double lon, string roadType, string endResult)> locations, string endpointResponse, string[] expectedLogs)
			{
				// Arrange
				loggerMock.Reset();
				var glbLocations = locations
					.Select(tuple =>
					{
						var glbLoc = Factory.New<GlbDeviceLocation>();
						glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
						glbLoc.V2_RoadType = tuple.roadType;
						return glbLoc;
					})
					.ToList();

				httpMessageHandlerMock.Reset();
				var returnCode = HttpStatusCode.Accepted;

				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent(endpointResponse),
					});

				// Act
				roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken());

				// Assert
				foreach (var location in locations)
				{
					var glbLocation = glbLocations.Single(loc => loc.Location.Latitude.Value == location.lat && loc.Location.Longitude.Value == location.lon);
					AssertEquals(location.endResult, glbLocation.V2_RoadType);
				}
				foreach (var log in expectedLogs)
				{
					loggerMock.Verify(logger => logger.Log(LogType.Error, log), Times.Once);
				}
				loggerMock.VerifyNoOtherCalls();
			}
		}

		[ExpectNoExceptions]
		public void TestServiceTaskErrorLoggedOnServerErrors()
		{
			CombineAssertions(() =>
			{
				Test(HttpStatusCode.HttpVersionNotSupported);
				Test(HttpStatusCode.BadGateway);
				Test(HttpStatusCode.GatewayTimeout);
			});

			void Test(HttpStatusCode endpointResponse)
			{
				// Arrange
				var locations = new List<(double lat, double lon, string roadType)>
				{
					(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
					(1.12345, 1.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
				};

				loggerMock.Reset();
				var glbLocations = locations
					.Select(tuple =>
					{
						var glbLoc = Factory.New<GlbDeviceLocation>();
						glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
						glbLoc.V2_RoadType = tuple.roadType;
						return glbLoc;
					})
					.ToList();

				httpMessageHandlerMock.Reset();

				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = endpointResponse,
						Content = new StringContent("Oh NO!"),
					});

				// Act
				roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken());
				// Assert
				loggerMock.Verify(logger => logger.Log(LogType.Error, $"Encountered Server error trying to post, status code is: ({(int)endpointResponse})"), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestExceptionRaisedOnClientErrors()
		{
			CombineAssertions(() =>
			{
				Test(HttpStatusCode.BadRequest);
				Test(HttpStatusCode.Forbidden);
				Test(HttpStatusCode.MethodNotAllowed);
			});

			void Test(HttpStatusCode endpointResponse)
			{
				// Arrange
				var expectedError = $"Response status code does not indicate success: {(int)endpointResponse}";
				var locations = new List<(double lat, double lon, string roadType)>
				{
					(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
					(1.12345, 1.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
				};

				loggerMock.Reset();
				var glbLocations = locations
					.Select(tuple =>
					{
						var glbLoc = Factory.New<GlbDeviceLocation>();
						glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
						glbLoc.V2_RoadType = tuple.roadType;
						return glbLoc;
					})
					.ToList();

				httpMessageHandlerMock.Reset();

				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = endpointResponse,
						Content = new StringContent("Oh NO!"),
					});

				// Act
				var exception = AssertExceptionThrown<HttpRequestException>(() =>
				{
					roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken());
				});

				// Assert
				Assert(exception.Message.Contains(expectedError));
			}
		}

		public void TestLocationMarkedAsFailedOnInvalidResponse()
		{
			CombineAssertions(() =>
			{
				Test<JsonSerializationException>(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"{{""Coordinate"":{{""Latitude"":10.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}}",
					"");
				Test<JsonReaderException>(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"This is a completely invalid json response!",
					"");
				Test<GpsLocationRoadTypeDecodeException>(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"[{{""NotACoordinate!"":{{""Latitude"":80.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}}]",
					"");
				Test<GpsLocationRoadTypeDecodeException>(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"[{{""Coordinate"":{{""Whatitude"":80.12345,""NotTheRightThing"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}}]",
					"");
				Test<GpsLocationRoadTypeDecodeException>(
					new List<(double lat, double lon, string roadType, string endResult)>
					{
						(80.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown, GlbDeviceLocationRoadTypes.Codes.Failed),
					},
					$@"[{{""Coordinate"":{{""Latitude"":80.12345,""Longitude"":12.54321}},""IsLandParcel"":""Not a bool!"",""ErrorMessage"":null}}]",
					"");
			});

			void Test<T>(List<(double lat, double lon, string roadType, string endResult)> locations, string endpointResponse, string expectedMessage)
				where T : Exception
			{
				// Arrange
				var glbLocations = locations
					.Select(tuple =>
					{
						var glbLoc = Factory.New<GlbDeviceLocation>();
						glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
						glbLoc.V2_RoadType = tuple.roadType;
						return glbLoc;
					})
					.ToList();

				httpMessageHandlerMock.Reset();
				var returnCode = HttpStatusCode.Accepted;

				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent(endpointResponse),
					});

				loggerMock.Reset();
				loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

				// Act
				var exception = AssertExceptionThrown<T>(() => roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken()));

				// Assert
				for (var i = 0; i < locations.Count; i++)
				{
					AssertEquals(locations[i].endResult, glbLocations[i].V2_RoadType);
				}
				AssertContains(expectedMessage, exception.Message, true);
				loggerMock.VerifyNoOtherCalls();
			}
		}

		public void TestHttpRequestException()
		{
			CombineAssertions(() =>
			{
				Test(new HttpRequestException("oh NO!"));
				Test(new HttpRequestException("oh NO!", new WebException("oh no again!")));
				Test(new HttpRequestException("oh NO!", new WebException("oh no again!", new SocketException(1))));
			});

			void Test(Exception exception)
			{
				// Arrange
				var locations = new List<(double lat, double lon, string roadType)>
				{
					(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
					(1.12345, 1.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
				};

				loggerMock.Reset();
				var glbLocations = locations
					.Select(tuple =>
					{
						var glbLoc = Factory.New<GlbDeviceLocation>();
						glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
						glbLoc.V2_RoadType = tuple.roadType;
						return glbLoc;
					})
					.ToList();

				httpMessageHandlerMock.Reset();

				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ThrowsAsync(exception);

				// Act
				var result = AssertExceptionThrown<HttpRequestException>(() =>
				{
					roadTypeCalculator.MarkLocationRoadType(glbLocations, new CancellationToken());
				});

				// Assert
				AssertEquals(exception, result);
			}
		}

		public void TestCancellationRequest()
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var cancellationTokenSource = new CancellationTokenSource();

			// Arrange
			var locations = new List<(double lat, double lon, string roadType)>
			{
				(10.12345, 12.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
				(1.12345, 1.543210, GlbDeviceLocationRoadTypes.Codes.Unknown),
			};

			var glbLocations = locations
				.Select(tuple =>
				{
					var glbLoc = Factory.New<GlbDeviceLocation>();
					glbLoc.V2_Location = ZGeography.CreatePoint(tuple.lon, tuple.lat);
					glbLoc.V2_RoadType = tuple.roadType;
					return glbLoc;
				})
				.ToList();

			httpMessageHandlerMock.Reset();
			var returnCode = HttpStatusCode.Accepted;

			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = returnCode,
					Content = new StringContent(GetResponse()),
				});

			// Act
			roadTypeCalculator.MarkLocationRoadType(glbLocations, cancellationTokenSource.Token);

			// Assert
			foreach (var glbLoc in glbLocations)
			{
				AssertEquals(GlbDeviceLocationRoadTypes.Codes.Unknown, glbLoc.V2_RoadType);
			}

			string GetResponse()
			{
				cancellationTokenSource.Cancel();
				return $@"[{{""Coordinate"":{{""Latitude"":10.12345,""Longitude"":12.54321}},""IsLandParcel"":true,""ErrorMessage"":null}},{{""Coordinate"":{{""Latitude"":1.12345,""Longitude"":1.543210}},""IsLandParcel"":false,""ErrorMessage"":null}}]";
			}
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new RoadTypeCalculator(null, loggerMock.Object));
				AssertEquals("httpClientFactory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new RoadTypeCalculator(httpClientFactoryMock.Object, null));
				AssertEquals("logger", result.ParamName);
			});
		}

		Mock<HttpMessageHandler> httpMessageHandlerMock;
		Mock<IHttpClientFactory> httpClientFactoryMock;
		Mock<ILogger> loggerMock;
		RoadTypeCalculator roadTypeCalculator;
	}
}
