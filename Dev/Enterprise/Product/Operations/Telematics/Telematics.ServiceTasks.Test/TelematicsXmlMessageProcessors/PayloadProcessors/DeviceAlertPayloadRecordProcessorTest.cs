using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class DeviceAlertPayloadRecordProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			processor = new DeviceAlertPayloadRecordProcessor();
		}

		public void TestNewAlertsCreateNewBusinessObjects()
		{
			CombineAssertions(() =>
			{
				Test("Some Notes", "ASD", new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(0)));
				Test("Some other notes", "DSA", new DateTimeOffset(2021, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(0)));
			});

			void Test(string notes, string type, DateTimeOffset dateTimeOffset)
			{
				// Arrange
				var record = new DeviceAlertPayloadRecord
				{
					AlertNotes = notes,
					AlertType = type,
					DateTimeOffset = dateTimeOffset,
				};
				var glbDevice = Factory.NewWithValidTestData<GlbDevice>();
				var query = new ZQuery(TelDeviceAlertSchema.TDA_LastOccurrenceTimeUtc, record.DateTimeOffset.UtcDateTime);

				// Act
				processor.Process(Factory, glbDevice, record);

				// Assert
				var result = Factory
					.Load<TelDeviceAlert>(query)
					.Single();
				AssertEquals(1, result.TDA_Occurrences);
				AssertEquals(false, result.TDA_IsAcknowledged);
				AssertEquals(type, result.TDA_Type);
				AssertEquals(notes, result.TDA_Notes);
				AssertEquals(dateTimeOffset.UtcDateTime, result.TDA_FirstOccurrenceTimeUtc);
				AssertEquals(dateTimeOffset.UtcDateTime, result.TDA_LastOccurrenceTimeUtc);
				AssertEquals(glbDevice.PK, result.TDA_V3_Device);
			}
		}

		public void TestAlertsOnOpenedInstancesUpdateRequiredFields()
		{
			CombineAssertions(() =>
			{
				Test("Some Notes", "ASD", new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(0)), 10);
				Test("Some other notes", "DSA", new DateTimeOffset(2021, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(0)), 50);
			});

			void Test(string notes, string type, DateTimeOffset dateTimeOffset, int occurrences)
			{
				// Arrange
				var record = new DeviceAlertPayloadRecord
				{
					AlertNotes = notes,
					AlertType = type,
					DateTimeOffset = dateTimeOffset,
				};
				var glbDevice = Factory.NewWithValidTestData<GlbDevice>();
				var query = new ZQuery(TelDeviceAlertSchema.TDA_V3_Device, glbDevice.PK);

				// Act
				for (var i = 0; i < occurrences; i++)
				{
					record.DateTimeOffset = dateTimeOffset.AddHours(i);
					processor.Process(Factory, glbDevice, record);
				}

				// Assert
				var result = Factory
					.Load<TelDeviceAlert>(query)
					.Single();
				AssertEquals(occurrences, result.TDA_Occurrences);
				AssertEquals(false, result.TDA_IsAcknowledged);
				AssertEquals(type, result.TDA_Type);
				AssertEquals(notes, result.TDA_Notes);
				AssertEquals(dateTimeOffset.UtcDateTime, result.TDA_FirstOccurrenceTimeUtc);
				AssertEquals(dateTimeOffset.AddHours(occurrences - 1).UtcDateTime, result.TDA_LastOccurrenceTimeUtc);
				AssertEquals(glbDevice.PK, result.TDA_V3_Device);
			}
		}

		public void TestAlertsOnClosedInstancesCreatesNewRecordInTable()
		{
			CombineAssertions(() =>
			{
				Test("Some Notes", "ASD", new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(0)), 10);
				Test("Some other notes", "DSA", new DateTimeOffset(2021, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(0)), 50);
			});

			void Test(string notes, string type, DateTimeOffset dateTimeOffset, int originalOccurrences)
			{
				// Arrange
				var record = new DeviceAlertPayloadRecord
				{
					AlertNotes = notes,
					AlertType = type,
					DateTimeOffset = dateTimeOffset,
				};
				var glbDevice = Factory.NewWithValidTestData<GlbDevice>();
				var originalTime = record.DateTimeOffset.AddYears(-1);

				var originalRecord = Factory.New<TelDeviceAlert>();
				originalRecord.TDA_V3_Device = glbDevice.PK;
				originalRecord.TDA_FirstOccurrenceTimeUtc = new ZDateTime(originalTime.UtcDateTime);
				originalRecord.TDA_LastOccurrenceTimeUtc = new ZDateTime(originalTime.UtcDateTime);
				originalRecord.TDA_IsAcknowledged = true;
				originalRecord.TDA_Occurrences = originalOccurrences;
				originalRecord.TDA_Notes = record.AlertNotes;
				originalRecord.TDA_Type = record.AlertType;

				var query = new ZQuery(TelDeviceAlertSchema.TDA_V3_Device, glbDevice.PK);

				// Act
				processor.Process(Factory, glbDevice, record);

				// Assert
				var storedRecords = Factory
					.Load<TelDeviceAlert>(query);
				var result = storedRecords.Single(r => !r.TDA_IsAcknowledged);
				AssertEquals(2, storedRecords.Length);
				AssertEquals(1, result.TDA_Occurrences);
				AssertEquals(false, result.TDA_IsAcknowledged);
				AssertEquals(type, result.TDA_Type);
				AssertEquals(notes, result.TDA_Notes);
				AssertEquals(dateTimeOffset.UtcDateTime, result.TDA_FirstOccurrenceTimeUtc);
				AssertEquals(dateTimeOffset.UtcDateTime, result.TDA_LastOccurrenceTimeUtc);
				AssertEquals(glbDevice.PK, result.TDA_V3_Device);
			}
		}

		DeviceAlertPayloadRecordProcessor processor;
	}
}
