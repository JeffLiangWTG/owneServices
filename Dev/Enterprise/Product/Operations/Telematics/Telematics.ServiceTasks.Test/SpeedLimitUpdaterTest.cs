using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	public class SpeedLimitUpdaterTest : TestCaseWithFactory
	{
		public void TestWrongConstructorParamsCall()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SpeedLimitUpdater(null, Factory, httpClientAdapterMock.Object, requestPackerMock.Object, responseUnpackerMock.Object, progressLoggerMock.Object));
			AssertExceptionThrown<ArgumentNullException>(() => new SpeedLimitUpdater(loggerMock.Object, null, httpClientAdapterMock.Object, requestPackerMock.Object, responseUnpackerMock.Object, progressLoggerMock.Object));
			AssertExceptionThrown<ArgumentNullException>(() => new SpeedLimitUpdater(loggerMock.Object, Factory, null, requestPackerMock.Object, responseUnpackerMock.Object, progressLoggerMock.Object));
			AssertExceptionThrown<ArgumentNullException>(() => new SpeedLimitUpdater(loggerMock.Object, Factory, httpClientAdapterMock.Object, null, responseUnpackerMock.Object, progressLoggerMock.Object));
			AssertExceptionThrown<ArgumentNullException>(() => new SpeedLimitUpdater(loggerMock.Object, Factory, httpClientAdapterMock.Object, requestPackerMock.Object, null, progressLoggerMock.Object));
			AssertExceptionThrown<ArgumentNullException>(() => new SpeedLimitUpdater(loggerMock.Object, Factory, httpClientAdapterMock.Object, requestPackerMock.Object, responseUnpackerMock.Object, null));
		}

		public void TestRequestsNothingForUndefinedSpeedLimits()
		{
			TestRequestsNothing("U");
		}

		public void TestRequestsNothingForLoadedSpeedLimits()
		{
			TestRequestsNothing("S");
		}

		void TestRequestsNothing(string speedLimitState)
		{
			// Arrange
			var deviceLocations = Enumerable.Range(0, 5).Select(i => CreateLocationForDevice(device.PK, speedLimitState, i)).ToArray();
			Factory.Save();

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			Assert(true); // empty test check.
		}

		public void TestUnpacksReceivedDataAndSaveItToTheDatabaseWithSpeedLimit()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = Enumerable.Range(0, count).Select(i => new { value = (decimal)(10 + i + i / 10.0), index = i }).ToDictionary(arg => arg.index, arg => arg.value);

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			for (var i = 0; i < deviceLocations.Length; i++)
			{
				var deviceLocation = deviceLocations[i];
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(speedLimits[i], deviceLocation.V2_SpeedLimitKmh);
			}
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestUnpacksReceivedDataAndSaveItToTheDatabaseWithoutSpeedLimit()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = new Dictionary<int, decimal>();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceLocation in deviceLocations)
			{
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithoutLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(0m, deviceLocation.V2_SpeedLimitKmh);
			}
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestUnpacksReceivedDataAndSaveItToTheDatabaseWithZeroSpeedLimit()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = Enumerable.Range(0, count).Select(i => new { value = 0m, index = i }).ToDictionary(arg => arg.index, arg => arg.value);

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceLocation in deviceLocations)
			{
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithoutLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(0m, deviceLocation.V2_SpeedLimitKmh);
			}
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestProcessesAllLocationsNotOnlyOneHundredLimit()
		{
			// Arrange
			const int count = 300;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(unpacker => unpacker.Unpack(It.IsAny<string>())).Returns(new Dictionary<int, decimal>());

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceLocation in deviceLocations)
			{
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithoutLimit, deviceLocation.V2_SpeedLimitState);
			}
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestPassesRightParamsFromRequestPackerToHttpAndToResponseUnpacker()
		{
			// Arrange
			var exception = new InvalidOleVariantTypeException("So sad...");
			var deviceLocations = Enumerable.Range(0, 3).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var uri = new Uri("http://localhost");
			const string response = "string.Empty";
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(uri);
			httpClientAdapterMock.Setup(m => m.GetStringAsync(uri, It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
			responseUnpackerMock.Setup(m => m.Unpack(response)).Throws(exception);

			// Act
			// Assert
			AssertExceptionThrown<InvalidOleVariantTypeException>(() => speedLimitUpdater.Run(CancellationToken.None));
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestAmountOfUpdatedLimitsIsLogged()
		{
			var deviceLocations = Enumerable.Range(0, 3).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(unpacker => unpacker.Unpack(It.IsAny<string>())).Returns(new Dictionary<int, decimal>());

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			loggerMock.Verify(logger => logger.Log(LogType.Information, It.Is<string>(s => s.Contains("3"))));
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestAmountOfUpdatedLimitsIsNotLoggedIfNothingToLog()
		{
			Factory.Save();

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never());
			Assert(true); // empty test check.
		}

		public void TestOneHundredPerRequestLimit()
		{
			// Arrange
			var exception = new InvalidOleVariantTypeException("So sad...");
			var deviceLocations = Enumerable.Range(0, 200).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Throws(exception);

			// Act
			AssertExceptionThrown<InvalidOleVariantTypeException>(() => speedLimitUpdater.Run(CancellationToken.None));

			// Assert
			requestPackerMock.Verify(packer => packer.CreateUri(It.Is<IEnumerable<ZGeography>>(tuples => tuples.Count() == 100)));
		}

		public void TestMixedRepeatingValuesAreRequested()
		{
			// Arrange
			var exception = new InvalidOleVariantTypeException("So sad...");
			var deviceLocations = Enumerable.Range(0, 10).Select(i => CreateLocationForDevice(device.PK, "M", 0))
				.Concat(Enumerable.Range(1, 10).Select(i => CreateLocationForDevice(device.PK, "M", i)))
				.ToArray();
			Factory.Save();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Throws(exception);

			// Act
			AssertExceptionThrown<InvalidOleVariantTypeException>(() => speedLimitUpdater.Run(CancellationToken.None));

			// Assert
			requestPackerMock.Setup(packer => packer.CreateUri(It.Is<IEnumerable<ZGeography>>(tuples => tuples.Count() == 11)));
		}

		public void TestRepeatingValuesAreRequestedOnce()
		{
			// Arrange
			var exception = new InvalidOleVariantTypeException("So sad...");
			var deviceLocations = Enumerable.Range(0, 50).Select(i => CreateLocationForDevice(device.PK, "M", 0)).ToArray();
			Factory.Save();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Throws(exception);

			// Act
			AssertExceptionThrown<InvalidOleVariantTypeException>(() => speedLimitUpdater.Run(CancellationToken.None));

			// Assert
			requestPackerMock.Setup(packer => packer.CreateUri(It.Is<IEnumerable<ZGeography>>(tuples => tuples.Count() == 1)));
		}

		public void TestRepeatingValuesAreUnpackedAndSavedToTheDatabaseWithSpeedLimit()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", 0)).ToArray();
			Factory.Save();

			const decimal speedLimit = 123.5m;
			var speedLimits = new Dictionary<int, decimal> { { 0, speedLimit } };
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceLocation in deviceLocations)
			{
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(speedLimit, deviceLocation.V2_SpeedLimitKmh);
			}

			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestRepeatingValuesAreUnpackedAndSavedToTheDatabaseWithoutSpeedLimit()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", 0)).ToArray();
			Factory.Save();

			var speedLimits = new Dictionary<int, decimal>();
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceLocation in deviceLocations)
			{
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithoutLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(0m, deviceLocation.V2_SpeedLimitKmh);
			}

			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestRepeatingValuesAreUnpackedAndSavedToTheDatabaseWithZeroSpeedLimit()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", 0)).ToArray();
			Factory.Save();

			const decimal speedLimit = 0m;
			var speedLimits = new Dictionary<int, decimal> { { 0, speedLimit } };
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceLocation in deviceLocations)
			{
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithoutLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(speedLimit, deviceLocation.V2_SpeedLimitKmh);
			}

			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestMixedValuesAreUnpackedAndSavedToTheDatabase()
		{
			// Arrange
			const int count = 10;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", 0))
				.Concat(Enumerable.Range(1, count).Select(i => CreateLocationForDevice(device.PK, "M", i)))
				.ToArray();
			Factory.Save();

			var speedLimits = Enumerable.Range(1, count).Select(i => new { value = (decimal)(10 + i + i / 10.0), index = i }).ToDictionary(arg => arg.index, arg => arg.value);
			speedLimits.Add(0, 21m);
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			AssertEquals(2 * count, deviceLocations.Length);
			for (var i = 0; i < count; i++)
			{
				var deviceLocation = deviceLocations[i];
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(speedLimits[count], deviceLocation.V2_SpeedLimitKmh);
			}

			for (var i = count; i < 2 * count; i++)
			{
				var deviceLocation = deviceLocations[i];
				AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithLimit, deviceLocation.V2_SpeedLimitState);
				AssertEquals(speedLimits[i - count + 1], deviceLocation.V2_SpeedLimitKmh);
			}

			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestValuesOfTheStates()
		{
			// Arrange
			var deviceLocationLookups = new GlbDeviceLocationLookups(Factory.NewWithValidTestData<GlbDeviceLocation>());

			// Act
			var speedLimitStates = deviceLocationLookups.SpeedLimitStateList;

			// Assert
			AssertEquals(3, speedLimitStates.Count);
			Assert(speedLimitStates.ContainsCode(SpeedLimitUpdater.SpeedLimitStateToLoad));
			Assert(speedLimitStates.ContainsCode(SpeedLimitUpdater.SpeedLimitStateWithLimit));
			Assert(speedLimitStates.ContainsCode(SpeedLimitUpdater.SpeedLimitStateWithoutLimit));
		}

		[ExpectNoExceptions]
		public void TestErrorsAreLogged()
		{
			// Arrange
			var exception = new HttpRequestException("So sad...");
			var deviceLocations = Enumerable.Range(0, 5).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Throws(exception);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			loggerMock.Verify(logger => logger.Log(LogType.Error, It.Is<string>(s => s.Contains(exception.Message))));
		}

		public void TestAllDevicesAreUpdated()
		{
			// Arrange
			var devicesWithLocations = Enumerable.Range(1, 50).Select(i =>
				{
					var glbDevice = Factory.NewWithValidTestData<GlbDevice>();
					glbDevice.V3_MobileServicesIdentifier = BitConverter.GetBytes(i);
					glbDevice.V3_Model = $"GLaDOS v3.{i}";
					return glbDevice;
				})
				.Select(glbDevice => new
				{
					glbDevice,
					Locations = Enumerable.Range(0, 5).Select(i => CreateLocationForDevice(glbDevice.PK, "M", i)).ToList()
				})
				.ToList();
			Factory.Save();

			var speedLimits = new Dictionary<int, decimal>();

			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			foreach (var deviceWithLocation in devicesWithLocations)
			{
				foreach (var location in deviceWithLocation.Locations)
				{
					AssertEquals(SpeedLimitUpdater.SpeedLimitStateWithoutLimit, location.V2_SpeedLimitState);
					AssertEquals(0m, location.V2_SpeedLimitKmh);
				}
			}

			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestReportsResults()
		{
			// Arrange
			const int count = 5;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = Enumerable.Range(0, count).Select(i => new { value = (decimal)(10 + i + i / 10.0), index = i }).ToDictionary(arg => arg.index, arg => arg.value);
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			loggerMock.Setup(logger => logger.Log(LogType.Information, "Total speed limits updated: 5 row(s)."));

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			loggerMock.VerifyAll();
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestReportsResultsInCaseOfError()
		{
			// Arrange
			const int count = 105;
			var deviceLocations = Enumerable.Range(0, count).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = Enumerable.Range(0, count).Select(i => new { value = (decimal)(10 + i + i / 10.0), index = i }).ToDictionary(arg => arg.index, arg => arg.value);

			var exception = new AbandonedMutexException("Ha!");
			requestPackerMock.Setup(packer => packer.CreateUri(It.Is<IEnumerable<ZGeography>>(tuples => tuples.Count() == 100))).Returns(new Uri("http://localhost"));
			requestPackerMock.Setup(packer => packer.CreateUri(It.Is<IEnumerable<ZGeography>>(tuples => tuples.Count() != 100))).Throws(exception);
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			loggerMock.Setup(logger => logger.Log(LogType.Information, "Total speed limits updated: 100 row(s)."));

			// Act
			// Assert
			AssertExceptionThrown<AbandonedMutexException>(() => speedLimitUpdater.Run(CancellationToken.None));
			loggerMock.VerifyAll();
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestProgressLoggerIsInitializedInTheBeginning()
		{
			// Arrange
			progressLoggerMock.Setup(m => m.Initialize());

			// Act
			AssertExceptionThrown<OperationCanceledException>(() => speedLimitUpdater.Run(new CancellationToken(true)));

			// Assert
			progressLoggerMock.Verify(logger => logger.Initialize());
		}

		public void TestProgressLoggerTimeToLog()
		{
			// Arrange
			var deviceLocations = Enumerable.Range(0, 10).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = new Dictionary<int, decimal>();
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			var expectedMessage = $"Speed limits updated: {deviceLocations.Length} row(s).";
			progressLoggerMock.Setup(m => m.ShouldLog()).Returns(true);
			loggerMock.Setup(logger => logger.Log(LogType.Information, expectedMessage));

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				progressLoggerMock.Verify(logger => logger.ShouldLog());
				loggerMock.Verify(logger => logger.Log(LogType.Information, expectedMessage));
			});

			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		public void TestProgressLoggerNotTimeToLog()
		{
			// Arrange
			var deviceLocations = Enumerable.Range(0, 10).Select(i => CreateLocationForDevice(device.PK, "M", i)).ToArray();
			Factory.Save();

			var speedLimits = new Dictionary<int, decimal>();
			requestPackerMock.Setup(m => m.CreateUri(It.IsAny<IEnumerable<ZGeography>>())).Returns(new Uri("http://localhost"));
			httpClientAdapterMock.Setup(m => m.GetStringAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(string.Empty));
			responseUnpackerMock.Setup(m => m.Unpack(It.IsAny<string>())).Returns(speedLimits);

			progressLoggerMock.Setup(m => m.ShouldLog()).Returns(false);

			// Act
			speedLimitUpdater.Run(CancellationToken.None);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				progressLoggerMock.Verify(logger => logger.ShouldLog());
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.Is<string>(s => s.StartsWith("Speed limits updated: "))), Times.Never());
			});
			requestPackerMock.VerifyAll();
			httpClientAdapterMock.VerifyAll();
			responseUnpackerMock.VerifyAll();
		}

		GlbDeviceLocation CreateLocationForDevice(ZGuid devicePk, string speedLimitState, int offset)
		{
			var glbDeviceLocation = Factory.New<GlbDeviceLocation>();
			glbDeviceLocation.V2_V3_Device = devicePk;
			glbDeviceLocation.V2_SpeedLimitState = speedLimitState;
			glbDeviceLocation.V2_MeasurementTimeUtc = new ZDateTime(2015, 1, 1, 0, 0, 0).AddMinutes(offset);
			glbDeviceLocation.V2_Location = ZGeography.CreatePoint(100 + offset / 10.0, 10 + offset / 10.0);
			return glbDeviceLocation;
		}

		protected override void SetUp()
		{
			base.SetUp();

			loggerMock = new Mock<ILogger>();
			httpClientAdapterMock = new Mock<IHttpClientAdapter>();
			requestPackerMock = new Mock<IRequestPacker>();
			responseUnpackerMock = new Mock<IResponseUnpacker>();
			progressLoggerMock = new Mock<IProgressLogger>();
			speedLimitUpdater = new SpeedLimitUpdater(loggerMock.Object, Factory, httpClientAdapterMock.Object, requestPackerMock.Object, responseUnpackerMock.Object, progressLoggerMock.Object);

			device = Factory.NewWithValidTestData<GlbDevice>();
			device.V3_MobileServicesIdentifier = new byte[] { 0x07, 0x07, 0xAA, 0xED };
			device.V3_Model = "GLaDOS v3.1";
		}

		GlbDevice device;
		Mock<IHttpClientAdapter> httpClientAdapterMock;
		Mock<ILogger> loggerMock;
		Mock<IProgressLogger> progressLoggerMock;
		Mock<IRequestPacker> requestPackerMock;
		Mock<IResponseUnpacker> responseUnpackerMock;
		SpeedLimitUpdater speedLimitUpdater;
	}
}
