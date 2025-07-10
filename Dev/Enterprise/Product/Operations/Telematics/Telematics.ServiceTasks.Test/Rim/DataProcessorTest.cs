using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.Rim;
using Moq;
using NUnit.Framework;
using WTG.Telematics.Common.Conversion;

namespace Enterprise.Telematics.ServiceTasks.Test.Rim
{
	class DataProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			dataRecordNumberStrategyMock = new Mock<IDataRecordNumberStrategy>();
			dataProcessor = new DataProcessor(dataRecordNumberStrategyMock.Object);

			devicesWithLocations = Enumerable
				.Range(10, 3)
				.Select(i =>
				{
					var device = Factory.New<GlbDevice>();
					device.V3_HumanReadableIdentifier = $"Device{i:D5}";
					device.V3_IsActive = true;
					device.V3_MobileServicesIdentifier = BitConverter.GetBytes(i);
					device.V3_HardwareIdentifier = BinaryDataConverter.ByteArrayToHexString(device.V3_MobileServicesIdentifier);
					device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
					device.V3_Model = "Model";
					return device;
				})
				.Select(device =>
				{
					var locations = Enumerable
						.Range(1, 5)
						.Select(i =>
						{
							var location = Factory.New<GlbDeviceLocation>();
							location.V2_V3_Device = device.PK;
							location.V2_RimReported = false;
							location.V2_MeasurementTimeUtc = ZDateTime.Now;
							return location;
						});
					return (device, locations.ToArray());
				})
				.ToArray();
			Factory.Save();
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = dataProcessor.Process(Factory, null, 100, CancellationToken.None));
				AssertEquals("data", result.ParamName);
			});
		}

		public void TestMarksLocationsAsProcessed()
		{
			// Arrange
			var devicesData = devicesWithLocations
				.Select(tuple =>
				{
					var dataMock = new Mock<IDeviceData>();
					dataMock
						.SetupGet(data => data.DeviceId)
						.Returns(tuple.device.V3_HardwareIdentifier);
					dataMock
						.SetupGet(data => data.Locations)
						.Returns(tuple.locations);
					return dataMock;
				})
				.Select(mock => mock.Object);

			var expected = Enumerable.Repeat(true, devicesWithLocations.SelectMany(tuple => tuple.locations).Count()).ToArray();

			// Act
			_ = dataProcessor.Process(Factory, devicesData, 100, CancellationToken.None).ToArray();

			// Assert
			Factory.Save();
			var result = devicesWithLocations.SelectMany(tuple => tuple.locations).Select(location => (bool)location.V2_RimReported).ToArray();
			AssertArrayEqualsByElements(expected, result);
		}

		public void TestMarksLocationsAsProcessedOnlyBeforeCancellation()
		{
			// Arrange
			var cancellationTokenSource = new CancellationTokenSource();
			var devicesData = devicesWithLocations
				.Select(tuple =>
				{
					var dataMock = new Mock<IDeviceData>();
					dataMock
						.SetupGet(data => data.DeviceId)
						.Returns(tuple.device.V3_HardwareIdentifier);
					dataMock
						.SetupGet(data => data.Locations)
						.Returns(tuple.locations);
					return dataMock;
				})
				.Select(mock => mock.Object)
				.ToArray();

			var expected = Enumerable.Repeat(true, devicesWithLocations[0].locations.Length)
				.Concat(Enumerable.Repeat(false, devicesWithLocations.Skip(1).SelectMany(tuple => tuple.locations).Count()))
				.ToArray();

			// Act
			_ = dataProcessor.Process(Factory, GetPortionedData(), 10000, cancellationTokenSource.Token).ToArray();

			// Assert
			Factory.Save();
			var result = devicesWithLocations.SelectMany(tuple => tuple.locations).Select(location => (bool)location.V2_RimReported).ToArray();
			AssertArrayEqualsByElements(expected, result);

			IEnumerable<IDeviceData> GetPortionedData()
			{
				yield return devicesData[0];
				cancellationTokenSource.Cancel();
				yield return devicesData[1];
				throw new InvalidOperationException();
			}
		}

		public void TestExpectedResults()
		{
			var testRun = 0;
			CombineAssertions(
				() =>
				{
					Test(
						1,
						1,
						new[] { "WTG-EDI-0000000001" },
						new[]
						{
							@"{""batchId"":""WTG-EDI-0000000001"",""tdeVersion"":""2.0"",""deviceRecords"":" +
							@"[{""device"":{""id"":""000010""},""records"":[" +
							@"{""dateTime"":""2020-03-20T01:53:13Z"",""type"":""POSITION"",""receiptDateTime"":""2020-03-20T01:53:13Z"",""position"":{""latitude"":-10.0,""longitude"":10.0}}]}]}"
						});
					Test(
						2,
						2,
						new[] { "WTG-EDI-0000000002", "WTG-EDI-0000000003" },
						new[]
						{
							@"{""batchId"":""WTG-EDI-0000000002"",""tdeVersion"":""2.0"",""deviceRecords"":" +
							@"[{""device"":{""id"":""000020""},""records"":[" +
							@"{""dateTime"":""2020-03-20T02:53:13Z"",""type"":""POSITION"",""receiptDateTime"":""2020-03-20T02:53:13Z"",""position"":{""latitude"":-20.0,""longitude"":20.0}}," +
							@"{""dateTime"":""2020-03-20T03:53:13Z"",""type"":""POSITION"",""receiptDateTime"":""2020-03-20T03:53:13Z"",""position"":{""latitude"":-30.0,""longitude"":30.0}}]}]}",
							@"{""batchId"":""WTG-EDI-0000000003"",""tdeVersion"":""2.0"",""deviceRecords"":" +
							@"[{""device"":{""id"":""000021""},""records"":" +
							@"[{""dateTime"":""2020-03-20T02:53:13Z"",""type"":""POSITION"",""receiptDateTime"":""2020-03-20T02:53:13Z"",""position"":{""latitude"":-20.0,""longitude"":20.0}}," +
							@"{""dateTime"":""2020-03-20T03:53:13Z"",""type"":""POSITION"",""receiptDateTime"":""2020-03-20T03:53:13Z"",""position"":{""latitude"":-30.0,""longitude"":30.0}}]}]}"
						});
				});

			void Test(int numberOfDevices, int numberOfRecordsPerDevice, string[] expectedBatchIds, string[] expectedMessages)
			{
				// Arrange
				dataRecordNumberStrategyMock.Reset();
				var cancellationTokenSource = new CancellationTokenSource();
				testRun++;
				var deviceData = Enumerable.Range(0, numberOfDevices).Select(
					i =>
					{
						var device = Factory.New<GlbDevice>();
						device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
						device.V3_HardwareIdentifier = $"0000{testRun}{i}";
						device.V3_HumanReadableIdentifier = $"WTG-{testRun}{i}";
						device.V3_Model = "IVU";
						return device;
					}).Select(
					device =>
					{
						var locations = Enumerable.Range(0, numberOfRecordsPerDevice).Select(
							i =>
							{
								var location = Factory.New<GlbDeviceLocation>();
								location.V2_V3_Device = device.PK;
								location.V2_Location = ZGeography.CreatePoint((testRun + i) * 10, (testRun + i) * -10);
								location.V2_MeasurementTimeUtc = new ZDateTime(new DateTime(2020, 3, 20, testRun + i, 53, 13));
								return location;
							});

						var dataMock = new Mock<IDeviceData>();
						dataMock.SetupGet(data => data.DeviceId).Returns(device.V3_HardwareIdentifier);
						dataMock.SetupGet(data => data.Locations).Returns(locations);
						return dataMock.Object;
					});
				Factory.Save();

				var regNumber = 0;
				dataRecordNumberStrategyMock.Setup(strategy => strategy.GetNextFormatted(Factory)).Returns(() =>
				{
					return $"WTG-EDI-{(regNumber++ + testRun):D10}";
				});

				// Act
				var result = dataProcessor.Process(Factory, deviceData, 10000, cancellationTokenSource.Token).ToList();

				// Assert
				var batchIds = result
					.Select(portionedData => portionedData.BatchId)
					.ToArray();

				var messages = result
					.Select(portionedData => portionedData.Message)
					.ToArray();

				AssertArrayEqualsByElements(expectedBatchIds, batchIds);
				AssertArrayEqualsByElements(expectedMessages, messages);
				dataRecordNumberStrategyMock.Verify(strategy => strategy.GetNextFormatted(Factory), Times.Exactly(numberOfDevices));
			}
		}

		[ExpectNoExceptions]
		public void TestSplitsBatchByBatchNumber()
		{
			var testRun = 0;
			CombineAssertions(
				() =>
				{
					Test(1, 10, 3);
					Test(2, 50, 2);
					Test(3, 5, 1);
				});

			void Test(int numberOfDevices, int recordsPerBatch, int numberOfBatches)
			{
				// Arrange
				testRun++;
				var numberOfRecordsPerDevice = recordsPerBatch * numberOfBatches;
				var deviceData = Enumerable.Range(0, numberOfDevices).Select(
					i =>
					{
						var device = Factory.New<GlbDevice>();
						device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
						device.V3_HardwareIdentifier = $"0000{testRun:00}{i:0000}";
						device.V3_HumanReadableIdentifier = $"WTG-{testRun}-{i:00000000}";
						device.V3_MobileServicesIdentifier = ZBlob.FromAscii($"0000{testRun:00}{i:0000}");
						device.V3_Model = "IVU";
						return device;
					}).Select(
					device =>
					{
						var locations = Enumerable.Range(0, numberOfRecordsPerDevice).Select(
							i =>
							{
								var location = Factory.New<GlbDeviceLocation>();
								location.V2_V3_Device = device.PK;
								location.V2_Location = ZGeography.CreatePoint(143.00043, -38.61181);
								location.V2_MeasurementTimeUtc = new ZDateTime(new DateTime(2020, 3, 20, 1, 53, 13));
								return location;
							});

						var dataMock = new Mock<IDeviceData>();
						dataMock.SetupGet(data => data.DeviceId).Returns(device.V3_HardwareIdentifier);
						dataMock.SetupGet(data => data.Locations).Returns(locations);
						return dataMock.Object;
					});
				Factory.Save();

				// Act
				var portionedData = dataProcessor.Process(Factory, deviceData, recordsPerBatch, CancellationToken.None).ToArray();

				// Assert
				AssertEquals(numberOfDevices * numberOfBatches, portionedData.Length);
			}
		}

		[ExpectNoExceptions]
		public void TestDoesNotEnumerateTwice()
		{
			// Arrange
			var deviceDataMocks = devicesWithLocations
				.Select(tuple =>
				{
					var dataMock = new Mock<IDeviceData>();
					dataMock
						.SetupGet(data => data.DeviceId)
						.Returns(tuple.device.V3_HardwareIdentifier);
					var locationsMock = new Mock<IEnumerable<GlbDeviceLocation>>();
					locationsMock
						.Setup(enumerable => enumerable.GetEnumerator())
						.Returns(tuple.locations.AsEnumerable().GetEnumerator);
					dataMock
						.SetupGet(data => data.Locations)
						.Returns(locationsMock.Object);
					return (dataMock, locationsMock);
				})
				.ToArray();

			// Act
			_ = dataProcessor.Process(Factory, deviceDataMocks.Select(tuple => tuple.dataMock.Object), 100, CancellationToken.None).ToArray();

			// Assert
			foreach (var (dataMock, locationsMock) in deviceDataMocks)
			{
				dataMock.VerifyGet(data => data.Locations, Times.Exactly(1));
				locationsMock.Verify(enumerable => enumerable.GetEnumerator(), Times.Exactly(1));
			}
		}

		[ExpectNoExceptions]
		public void TestDoesNotProcessWithoutForcedEnumerating()
		{
			// Arrange
			var devicesData = devicesWithLocations
				.Select(tuple =>
				{
					var dataMock = new Mock<IDeviceData>();
					dataMock
						.SetupGet(data => data.DeviceId)
						.Returns(tuple.device.V3_HardwareIdentifier);
					dataMock
						.SetupGet(data => data.Locations)
						.Returns(tuple.locations);
					return dataMock;
				})
				.Select(mock => mock.Object)
				.ToArray();
			var devicesDataMock = new Mock<IEnumerable<IDeviceData>>();
			devicesDataMock
				.Setup(enumerable => enumerable.GetEnumerator())
				.Returns(devicesData.AsEnumerable().GetEnumerator);

			// Act
			_ = dataProcessor.Process(Factory, devicesDataMock.Object, 100, CancellationToken.None);

			// Assert
			devicesDataMock.Verify(enumerable => enumerable.GetEnumerator(), Times.Never);
		}

		DataProcessor dataProcessor;
		(GlbDevice device, GlbDeviceLocation[] locations)[] devicesWithLocations;
		Mock<IDataRecordNumberStrategy> dataRecordNumberStrategyMock;
	}
}
