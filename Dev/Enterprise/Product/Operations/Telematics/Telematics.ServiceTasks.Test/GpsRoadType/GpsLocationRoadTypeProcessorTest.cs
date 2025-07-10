using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
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
	class GpsLocationRoadTypeProcessorTests
	{
		class GpsLocationRoadTypeProcessorUnitTests : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				gpsLocationAccessorMock = new Mock<IGpsLocationAccessor>();
				roadTypeCalculatorMock = new Mock<IRoadTypeCalculator>();
				loggerMock = new Mock<ILogger>();
				gpsLocationRoadTypeProcessor = new GpsLocationRoadTypeProcessor(loggerMock.Object, Factory, gpsLocationAccessorMock.Object, roadTypeCalculatorMock.Object);
			}

			[ExpectNoExceptions]
			public void TestRunSequence()
			{
				// Arrange
				var locations = Array.Empty<GlbDeviceLocation>();
				var token = new CancellationToken();

				var sequence = new MockSequence();
				loggerMock
					.InSequence(sequence)
					.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
				gpsLocationAccessorMock
					.InSequence(sequence)
					.Setup(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()))
					.Returns(locations);
				loggerMock
					.InSequence(sequence)
					.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
				roadTypeCalculatorMock
					.InSequence(sequence)
					.Setup(calculator => calculator.MarkLocationRoadType(It.IsAny<IEnumerable<GlbDeviceLocation>>(), It.IsAny<CancellationToken>()));
				loggerMock
					.InSequence(sequence)
					.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

				// Act
				gpsLocationRoadTypeProcessor.Run(token);

				// Assert
				gpsLocationAccessorMock.Verify(accessor => accessor.GetLocations(Factory), Times.Once);
				roadTypeCalculatorMock.Verify(calc => calc.MarkLocationRoadType(locations, token), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "Start processing Location records"), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "Processing 0 Location records"), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "Processed 0 Location records"), Times.Once);
				gpsLocationAccessorMock.VerifyNoOtherCalls();
				roadTypeCalculatorMock.VerifyNoOtherCalls();
				loggerMock.VerifyNoOtherCalls();
			}

			[ExpectNoExceptions]
			public void TestGpsLocationAccessorExceptionsAreHandled()
			{
				CombineAssertions(() =>
				{
					Test(new JsonReaderException("Some message"));
					Test(new JsonSerializationException("some message"));
					Test(new GpsLocationRoadTypeDecodeException(ZGeography.CreatePoint(1, 1), new Exception("Some inner message")));
				});

				void Test(Exception exception)
				{
					// Arrange
					loggerMock.Reset();
					gpsLocationAccessorMock.Reset();
					roadTypeCalculatorMock.Reset();
					var locations = Array.Empty<GlbDeviceLocation>();

					loggerMock
						.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
					gpsLocationAccessorMock
						.Setup(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()))
						.Throws(exception);

					// Act
					gpsLocationRoadTypeProcessor.Run(CancellationToken.None);

					// Assert
					gpsLocationAccessorMock.Verify(accessor => accessor.GetLocations(Factory), Times.Once);
					roadTypeCalculatorMock.Verify(calc => calc.MarkLocationRoadType(locations, CancellationToken.None), Times.Never);
					loggerMock.Verify(logger => logger.Log(LogType.Error, exception.Message), Times.Once);
				}
			}

			[ExpectNoExceptions]
			public void TestRoadTypeCalculatorExceptionsAreHandled()
			{
				CombineAssertions(() =>
				{
					Test(new JsonReaderException("Some message"));
					Test(new JsonSerializationException("some message"));
					Test(new GpsLocationRoadTypeDecodeException(ZGeography.CreatePoint(1, 1), new Exception("Some inner message")));
					Test(new HttpRequestException("Some Message"));
				});

				void Test(Exception exception)
				{
					// Arrange
					loggerMock.Reset();
					gpsLocationAccessorMock.Reset();
					roadTypeCalculatorMock.Reset();
					var locations = Array.Empty<GlbDeviceLocation>();

					loggerMock
						.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
					gpsLocationAccessorMock
						.Setup(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()))
						.Returns(locations);
					roadTypeCalculatorMock
						.Setup(calculator => calculator.MarkLocationRoadType(It.IsAny<IEnumerable<GlbDeviceLocation>>(), It.IsAny<CancellationToken>()))
						.Throws(exception);

					// Act
					gpsLocationRoadTypeProcessor.Run(CancellationToken.None);

					// Assert
					gpsLocationAccessorMock.Verify(accessor => accessor.GetLocations(Factory), Times.Once);
					roadTypeCalculatorMock.Verify(calc => calc.MarkLocationRoadType(locations, CancellationToken.None), Times.Once);
					loggerMock.Verify(logger => logger.Log(LogType.Error, exception.Message), Times.Once);
				}
			}

			[ExpectNoExceptions]
			public void TestServiceTaskExitsWhenBacklogIsClearedAndRetrievedBatchSizeFallsUnderMinimum()
			{
				CombineAssertions(() =>
				{
					Test(2, new[] { 2, 2, 1 }, 3);
					Test(5, new[] { 5, 2, 5 }, 2);
					Test(10, new[] { 9, 2, 1 }, 1);
				});

				void Test(int batchSize, int[] numberOfMessagesReturned, int expectedAmountOfCalls)
				{
					// Arrange
					TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
					gpsLocationAccessorMock.Reset();
					roadTypeCalculatorMock.Reset();

					var sequence = gpsLocationAccessorMock
						.SetupSequence(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()));
					foreach (var number in numberOfMessagesReturned)
					{
						var locations = Enumerable.Range(0, number)
							.Select(j => Factory.NewWithValidTestData<GlbDeviceLocation>())
							.ToList();
						sequence.Returns(locations);
					}

					// Act
					gpsLocationRoadTypeProcessor.Run(CancellationToken.None);

					// Assert
					gpsLocationAccessorMock.Verify(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()), Times.Exactly(expectedAmountOfCalls));
					roadTypeCalculatorMock.Verify(calc => calc.MarkLocationRoadType(It.IsAny<IEnumerable<GlbDeviceLocation>>(), CancellationToken.None), Times.Exactly(expectedAmountOfCalls));
					gpsLocationAccessorMock.VerifyNoOtherCalls();
					roadTypeCalculatorMock.VerifyNoOtherCalls();
				}
			}

			[ExpectNoExceptions]
			public void TestFactoryProviderReclaimsMemory()
			{
				// Arrange
				var batchSize = 2;
				var numberOfMessagesReturned = new[] { 2, 2, 1 };
				TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);

				var sequence = gpsLocationAccessorMock
					.SetupSequence(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()));
				foreach (var number in numberOfMessagesReturned)
				{
					var locations = Enumerable.Range(0, number)
						.Select(j => Factory.NewWithValidTestData<GlbDeviceLocation>())
						.ToList();
					sequence.Returns(locations);
				}

				// Act
				gpsLocationRoadTypeProcessor.Run(CancellationToken.None);

				// Assert
				AssertEquals(1, Factory.SaveCount);
			}

			[ExpectNoExceptions]
			public void TestNoCallsIfCancellationIsRequested()
			{
				// Arrange
				// Act
				gpsLocationRoadTypeProcessor.Run(new CancellationToken(true));

				// Assert
				gpsLocationAccessorMock.VerifyNoOtherCalls();
				roadTypeCalculatorMock.VerifyNoOtherCalls();
				loggerMock.VerifyNoOtherCalls();
			}

			public void TestWrongParamsCall()
			{
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GpsLocationRoadTypeProcessor(null, Factory, gpsLocationAccessorMock.Object, roadTypeCalculatorMock.Object));
					AssertEquals("logger", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GpsLocationRoadTypeProcessor(loggerMock.Object, null, gpsLocationAccessorMock.Object, roadTypeCalculatorMock.Object));
					AssertEquals("factory", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GpsLocationRoadTypeProcessor(loggerMock.Object, Factory, null, roadTypeCalculatorMock.Object));
					AssertEquals("gpsLocationAccessor", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GpsLocationRoadTypeProcessor(loggerMock.Object, Factory, gpsLocationAccessorMock.Object, null));
					AssertEquals("roadTypeCalculator", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GpsLocationRoadTypeProcessor(null));
					AssertEquals("logger", result.ParamName);
				});
			}

			Mock<IGpsLocationAccessor> gpsLocationAccessorMock;
			Mock<IRoadTypeCalculator> roadTypeCalculatorMock;
			Mock<ILogger> loggerMock;
			GpsLocationRoadTypeProcessor gpsLocationRoadTypeProcessor;
		}

		class GpsLocationRoadTypeProcessorIntegrationTests : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				httpMessageHandlerMock = new Mock<HttpMessageHandler>();
				httpClientFactoryMock = new Mock<IHttpClientFactory>();
				loggerMock = new Mock<ILogger>();
				var httpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock.Setup(factory => factory.Create(It.IsAny<TimeSpan>())).Returns(httpClient);
				roadTypeCalculator = new RoadTypeCalculator(httpClientFactoryMock.Object, loggerMock.Object);
				gpsLocationAccessorMock = new Mock<IGpsLocationAccessor>();
				gpsLocationRoadTypeProcessor = new GpsLocationRoadTypeProcessor(loggerMock.Object, Factory, gpsLocationAccessorMock.Object, roadTypeCalculator);
				TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}

			[ExpectNoExceptions]
			public void TestRoadTypeCalculatorAggregateException()
			{
				// Arrange
				var exception = new HttpRequestException("SomeMessage");
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.ThrowsAsync(exception);

				gpsLocationAccessorMock
					.Setup(accessor => accessor.GetLocations(It.IsAny<BusinessObjectFactory>()))
					.Returns(Array.Empty<GlbDeviceLocation>());

				// Act
				gpsLocationRoadTypeProcessor.Run(CancellationToken.None);

				// Assert
				loggerMock.Verify(logger => logger.Log(LogType.Error, exception.Message), Times.Once);
			}

			Mock<IGpsLocationAccessor> gpsLocationAccessorMock;
			Mock<IHttpClientFactory> httpClientFactoryMock;
			Mock<ILogger> loggerMock;
			Mock<HttpMessageHandler> httpMessageHandlerMock;
			RoadTypeCalculator roadTypeCalculator;
			GpsLocationRoadTypeProcessor gpsLocationRoadTypeProcessor;
		}
	}
}
