using System;
using System.Linq;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class IgnitionPayloadRecordProcessorTest : PayloadRecordProcessorTest<IgnitionPayloadRecordProcessor, IgnitionPayloadRecord>
	{
		public override void TestAddsRecord()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), true);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), false);
			});

			void Test(DateTimeOffset dateTimeOffset, bool state)
			{
				// Arrange
				foreach (var record in device.Ignitions)
				{
					record.Delete();
				}

				var payloadRecord = new IgnitionPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					IgnitionState = state,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.Ignitions.Single();
				AssertEquals(expectedTime, result.GDI_MeasurementTimeUtc);
				AssertEquals(state, result.GDI_State);
				result.Delete();
			}
		}

		GlbDevice generateNewDevice(string deviceId, string humanReadableId)
		{
			var newDevice = Factory.New<GlbDevice>();
			newDevice.V3_HumanReadableIdentifier = humanReadableId;
			newDevice.V3_IsActive = true;
			newDevice.V3_MobileServicesIdentifier = Array.Empty<byte>();
			newDevice.V3_HardwareIdentifier = deviceId;
			newDevice.V3_Model = string.Empty;
			return newDevice;
		}

		public void TestNewRecordsAtLatestTime()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), false, new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), false, false, "TestNewRecordsAtLatestTimeDevice_1", "TestNewRecordsAtLatestTimeDevice_Human_1");
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), true, new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), false, true, "TestNewRecordsAtLatestTimeDevice_2", "TestNewRecordsAtLatestTimeDevice_Human_2");
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), true, new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), true, false, "TestNewRecordsAtLatestTimeDevice_3", "TestNewRecordsAtLatestTimeDevice_Human_3");
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), false, new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), true, true, "TestNewRecordsAtLatestTimeDevice_4", "TestNewRecordsAtLatestTimeDevice_Human_4");
			});

			void Test(DateTimeOffset preDateTimeOffset, bool preState, DateTimeOffset newDateTimeOffset, bool newState, bool stateChange, string deviceId, string humanReadableId)
			{
				var testDevice = generateNewDevice(deviceId, humanReadableId);

				var prePayloadRecord = new IgnitionPayloadRecord
				{
					DateTimeOffset = preDateTimeOffset,
					IgnitionState = preState,
				};

				var newPayloadRecord = new IgnitionPayloadRecord
				{
					DateTimeOffset = newDateTimeOffset,
					IgnitionState = newState,
				};

				// Act
				processor.Process(Factory, testDevice, prePayloadRecord);
				processor.Process(Factory, testDevice, newPayloadRecord);

				// Assert
				var expectedResult = testDevice.Ignitions.OrderByDescending(ign => ign.GDI_MeasurementTimeUtc)
														 .First();
				AssertEquals(stateChange, expectedResult.GDI_StateChange);
			}
		}

		public void TestInsertedRecordModifiesRelevantStateChanges()
		{
			CombineAssertions(() =>
			{
				var insertedDateTimeOffset = new DateTimeOffset(2012, 5, 3, 12, 6, 26, TimeSpan.Zero);

				//case 1
				var stateArray = new bool[] { false, false, false, false, false, false };
				var stateChangeArray = new bool[] { true, false, false, false, false, false };
				var testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_1", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_1");
				CargoWise.Types.ZBool[] expectedResult = new CargoWise.Types.ZBool[] { true, false, false, false, false, false, false };
				Test(insertedDateTimeOffset, false, expectedResult, testDevice);

				//case 2
				stateArray = new bool[] { false, false, true, false, false, false };
				stateChangeArray = new bool[] { true, false, true, true, false, false };
				testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_2", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_2");
				expectedResult = new CargoWise.Types.ZBool[] { true, false, true, true, false, false, false };
				Test(insertedDateTimeOffset, false, expectedResult, testDevice);

				//case 3
				stateArray = new bool[] { false, true, true, false, false, false };
				stateChangeArray = new bool[] { true, true, false, true, false, false };
				testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_3", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_3");
				expectedResult = new CargoWise.Types.ZBool[] { true, true, false, true, false, false, false };
				Test(insertedDateTimeOffset, false, expectedResult, testDevice);

				//case 4
				stateArray = new bool[] { false, false, false, true, true, true };
				stateChangeArray = new bool[] { true, false, false, true, false, false };
				testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_4", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_4");
				expectedResult = new CargoWise.Types.ZBool[] { true, false, false, true, false, false, false };
				Test(insertedDateTimeOffset, true, expectedResult, testDevice);

				//case 5
				stateArray = new bool[] { false, false, false, false, true, true };
				stateChangeArray = new bool[] { true, false, false, false, true, false };
				testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_5", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_5");
				expectedResult = new CargoWise.Types.ZBool[] { true, false, false, true, true, true, false };
				Test(insertedDateTimeOffset, true, expectedResult, testDevice);

				//case 6
				stateArray = new bool[] { false, true, false, true, false, true };
				stateChangeArray = new bool[] { true, true, true, true, true, true };
				testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_6", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_6");
				expectedResult = new CargoWise.Types.ZBool[] { true, true, true, false, true, true, true };
				Test(insertedDateTimeOffset, false, expectedResult, testDevice);

				//case 7
				stateArray = new bool[] { false, true, false, true, false, true };
				stateChangeArray = new bool[] { true, true, true, true, true, true };
				testDevice = GenerateTestIgnitionRecords(stateArray, stateChangeArray, "TestInsertedRecordModifiesRelevantStateChangesDevice_7", "TestInsertedRecordModifiesRelevantStateChangesDevice_Human_7");
				expectedResult = new CargoWise.Types.ZBool[] { true, true, true, true, false, true, true };
				Test(insertedDateTimeOffset, true, expectedResult, testDevice);
			});

			GlbDevice GenerateTestIgnitionRecords(bool[] stateArray, bool[] stateChangeArray, string deviceId, string humanReadableId)
			{
				var newDevice = generateNewDevice(deviceId, humanReadableId);

				for (int d = 0; d < 6; d++)
				{
					var ignition = Factory.New<GlbDeviceIgnition>();
					ignition.GDI_V3_Device = newDevice.PK;
					ignition.GDI_MeasurementTimeUtc = new DateTimeOffset(2012, 5, d + 1, 10, 5, 23, TimeSpan.Zero).UtcDateTime;
					ignition.GDI_State = stateArray[d];
					ignition.GDI_StateChange = stateChangeArray[d];
				}

				return newDevice;
			}

			void Test(DateTimeOffset dateTimeOffset, bool state, CargoWise.Types.ZBool[] expectedResult, GlbDevice testDevice)
			{
				var payloadRecord = new IgnitionPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					IgnitionState = state,
				};

				// Act
				processor.Process(Factory, testDevice, payloadRecord);

				// Assert
				var result = testDevice.Ignitions
					.OrderBy(ign => ign.GDI_MeasurementTimeUtc)
					.Select(ign => ign.GDI_StateChange)
					.ToArray();

				AssertArrayEqualsByElements(result, expectedResult);
			}
		}

		public new void TestReturnsCount()
		{
			var payloadRecord = new IgnitionPayloadRecord
			{
				DateTimeOffset = new DateTimeOffset(2012, 5, 6, 12, 6, 26, TimeSpan.Zero),
				IgnitionState = false,
			};
			var result = processor.Process(Factory, device, payloadRecord);
			AssertEquals(1, result);
		}
	}
}
