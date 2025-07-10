using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	class ObsoleteDataCleanerTests : TestCaseWithFactory
	{
		public void TestDataOlderThan2YearsDeleted()
		{
			EnsureOnlyDataOlderThanRegistryYearThresholdIsDeleted(2);
		}

		public void TestDataOlderThan3YearsDeleted()
		{
			EnsureOnlyDataOlderThanRegistryYearThresholdIsDeleted(3);
		}

		public void TestDataOlderThan5YearsDeleted()
		{
			EnsureOnlyDataOlderThanRegistryYearThresholdIsDeleted(5);
		}

		void EnsureOnlyDataOlderThanRegistryYearThresholdIsDeleted(int numYearsToKeepData)
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.NumberOfBatchesDeletedPerRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			TelematicsConfigurationRegistry.Instance.BatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10000);
			TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, numYearsToKeepData);
			CreateObsoleteData(numYearsToKeepData, 1);
			var recentDeviceDataObjects = CreateRecentData(numYearsToKeepData, 1);
			Factory.Save();

			AssertEquals("Precondition", 2, Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true }).Length);

			var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object, ("DummyBizo", AutoDummyBizo.Schema.Z0_Date));

			// Act
			obsoleteDataCleaner.Run(CancellationToken.None);

			// Assert
			var result = Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true });
			AssertContainsExactElementsInAnyOrder(recentDeviceDataObjects, result);
		}

		[SnailTest]
		public void TestBatchSizeGreaterThanNumberOfRecordsInDatabase()
		{
			EnsureTaskDeletesNoMoreThanBatchSize(200, 150);
		}

		[SnailTest]
		public void TestBatchSizeSmallerThanNumberOfRecordsInDatabase()
		{
			EnsureTaskDeletesNoMoreThanBatchSize(150, 200);
		}

		[SnailTest]
		public void TestBatchSizeEqualToNumberOfRecordsInDatabase()
		{
			EnsureTaskDeletesNoMoreThanBatchSize(100, 100);
		}

		[SnailTest]
		public void TestNoRecordsInDatabase()
		{
			EnsureTaskDeletesNoMoreThanBatchSize(100, 0);
		}

		void EnsureTaskDeletesNoMoreThanBatchSize(int batchSize, int totalRecordsToAdd)
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			TelematicsConfigurationRegistry.Instance.NumberOfBatchesDeletedPerRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			TelematicsConfigurationRegistry.Instance.BatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			CreateObsoleteData(1, totalRecordsToAdd);
			Factory.Save();

			var expectedRemainder = (totalRecordsToAdd - batchSize) > 0 ? totalRecordsToAdd - batchSize : 0;
			var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object, ("DummyBizo", AutoDummyBizo.Schema.Z0_Date));

			// Act
			obsoleteDataCleaner.Run(CancellationToken.None);

			// Assert
			var result = Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true }).Length;
			AssertEquals($"Expected no more than {batchSize} rows to be deleted from table.", expectedRemainder, result);
		}

		[SnailTest]
		public void Test2BatchesDeletedPerRun()
		{
			EnsureCorrectNumberOfBatchesDeletedPerRun(2);
		}

		[SnailTest]
		public void Test4BatchesDeletedPerRun()
		{
			EnsureCorrectNumberOfBatchesDeletedPerRun(4);
		}

		void EnsureCorrectNumberOfBatchesDeletedPerRun(int numBatches)
		{
			// Arrange
			const int expectedRemainder = 20;
			const int batchSize = 100;
			TelematicsConfigurationRegistry.Instance.BatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			TelematicsConfigurationRegistry.Instance.NumberOfBatchesDeletedPerRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, numBatches);
			var numRowsToAddPerTable = batchSize * numBatches + expectedRemainder;
			CreateObsoleteData(1, numRowsToAddPerTable);
			Factory.Save();

			var recordsInDbBeforeClean = Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true });
			AssertEquals(numRowsToAddPerTable, recordsInDbBeforeClean.Length);

			var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object, ("DummyBizo", AutoDummyBizo.Schema.Z0_Date));

			// Act
			obsoleteDataCleaner.Run(CancellationToken.None);

			// Assert
			var deviceDataObjectList = Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true });
			AssertEquals($"Expected exactly {batchSize * numBatches} rows to be deleted from table.", expectedRemainder, deviceDataObjectList.Length);
		}

		[SnailTest]
		public void TestObsoleteDataDeletedFromOldestToNewest()
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			TelematicsConfigurationRegistry.Instance.BatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			TelematicsConfigurationRegistry.Instance.NumberOfBatchesDeletedPerRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			CreateObsoleteData(5, 100);
			var newestObsoleteData = CreateObsoleteData(1, 100);
			Factory.Save();

			var recordsInDbBeforeClean = Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true });
			AssertEquals(200, recordsInDbBeforeClean.Length);

			var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object, ("DummyBizo", AutoDummyBizo.Schema.Z0_Date));

			// Act
			obsoleteDataCleaner.Run(CancellationToken.None);

			// Assert
			var result = Factory.Load<DummyBusinessObject>(new ZDBOnlyQuery(typeof(DummyBusinessObject)) { ReLoadExistingRows = true });
			AssertContainsExactElementsInAnyOrder(newestObsoleteData, result);
		}

		public void TestObsoleteDataCleanerTableList()
		{
			// Arrange
			var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object);
			var expectedTables = new (string DataType, string MeasurementTimeSchemaColumnName)[]
			{
				(nameof(GlbDeviceBattery), GlbDeviceBatterySchema.GDB_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceExternalVoltage), GlbDeviceExternalVoltageSchema.GDV_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceIgnition), GlbDeviceIgnitionSchema.GDI_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceLocation), GlbDeviceLocationSchema.V2_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceLog), GlbDeviceLogSchema.GDL_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceOdometer), GlbDeviceOdometerSchema.GDO_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceOnboardMass), GlbDeviceOnboardMassSchema.GDM_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceTemperature), GlbDeviceTemperatureSchema.GDT_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceTyreAlert), GlbDeviceTyreAlertSchema.GDA_MeasurementTimeUtc.Name),
				(nameof(GlbDeviceTyreReport), GlbDeviceTyreReportSchema.GDR_MeasurementTimeUtc.Name)
			};

			// Assert
			AssertArrayEqualsByElements(expectedTables, obsoleteDataCleaner.ObsoleteDataTypes);
		}

		public void TestWrongConstructorParams()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new ObsoleteDataCleaner(null));
			AssertEquals("serviceLogger", result.ParamName);
		}

		public void TestLogsProgress()
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			TelematicsConfigurationRegistry.Instance.BatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			TelematicsConfigurationRegistry.Instance.NumberOfBatchesDeletedPerRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			CreateObsoleteData(1, 150);
			Factory.Save();

			var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object, ("DummyBizo", AutoDummyBizo.Schema.Z0_Date));

			// Act
			obsoleteDataCleaner.Run(CancellationToken.None);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(LogType.Information, "Cleanup operation started."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "DummyBizo: Cleanup started."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "DummyBizo: Batch 1 out of 2."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "DummyBizo: Deleted 100 record(s)."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "DummyBizo: Batch 2 out of 2."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "DummyBizo: Deleted 50 record(s)."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "DummyBizo: Cleanup finished."), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, "Cleanup operation completed."), Times.Once);
			});
		}

		public class EndToEndTest : TestCaseWithFactory
		{
			[SnailTest]
			public void TestCleanerRemovesDataFromAllTelematicsTables_EndToEnd()
			{
				// Arrange
				TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				CreateObsoleteTelematicsData(new ZDateTime(ZDateTime.Now.Year - 2, 1, 1));
				Factory.Save();
				LoadAllDataFromTelematicsTables();
				var obsoleteDataCleaner = new ObsoleteDataCleaner(loggerMock.Object);

				// Act
				obsoleteDataCleaner.Run(CancellationToken.None);

				// Assert
				var result = LoadAllDataFromTelematicsTables().Count();
				AssertEquals(0, result);
			}

			void CreateObsoleteTelematicsData(ZDateTime obsoleteDate)
			{
				var device = Factory.NewWithValidTestData<GlbDevice>();
				var subEquipment = Factory.NewWithValidTestData<TelSubEquipment>();
				subEquipment.TSE_Type = "O";

				var battery = Factory.New<GlbDeviceBattery>();
				battery.GDB_MeasurementTimeUtc = obsoleteDate;
				battery.GDB_V3_Device = device.PK;
				battery.GDB_Voltage = 3.0;

				var externalVoltage = Factory.New<GlbDeviceExternalVoltage>();
				externalVoltage.GDV_MeasurementTimeUtc = obsoleteDate;
				externalVoltage.GDV_V3_Device = device.PK;

				var ignition = Factory.New<GlbDeviceIgnition>();
				ignition.GDI_MeasurementTimeUtc = obsoleteDate;
				ignition.GDI_V3_Device = device.PK;

				var location = Factory.New<GlbDeviceLocation>();
				location.V2_MeasurementTimeUtc = obsoleteDate;
				location.V2_V3_Device = device.PK;

				var log = Factory.New<GlbDeviceLog>();
				log.GDL_MeasurementTimeUtc = obsoleteDate;
				log.GDL_V3_Device = device.PK;
				log.GDL_MessageString = "Test";

				var odometer = Factory.New<GlbDeviceOdometer>();
				odometer.GDO_MeasurementTimeUtc = obsoleteDate;
				odometer.GDO_V3_Device = device.PK;

				var onboardMass = Factory.New<GlbDeviceOnboardMass>();
				onboardMass.GDM_MeasurementTimeUtc = obsoleteDate;
				onboardMass.GDM_V3_Device = device.PK;
				onboardMass.GDM_TSE_SubEquipment = subEquipment.PK;

				var temperature = Factory.New<GlbDeviceTemperature>();
				temperature.GDT_MeasurementTimeUtc = obsoleteDate;
				temperature.GDT_V3_Device = device.PK;

				var tyreAlert = Factory.New<GlbDeviceTyreAlert>();
				tyreAlert.GDA_MeasurementTimeUtc = obsoleteDate;
				tyreAlert.GDA_V3_Device = device.PK;

				var tyreReport = Factory.New<GlbDeviceTyreReport>();
				tyreReport.GDR_MeasurementTimeUtc = obsoleteDate;
				tyreReport.GDR_V3_Device = device.PK;
			}

			IEnumerable<BusinessObject> LoadAllDataFromTelematicsTables()
			{
				var data = new List<BusinessObject>();
				var telematicsTypes = new Type[]
				{
					typeof(GlbDeviceBattery),
					typeof(GlbDeviceExternalVoltage),
					typeof(GlbDeviceIgnition),
					typeof(GlbDeviceLocation),
					typeof(GlbDeviceLog),
					typeof(GlbDeviceOdometer),
					typeof(GlbDeviceOnboardMass),
					typeof(GlbDeviceTemperature),
					typeof(GlbDeviceTyreAlert),
					typeof(GlbDeviceTyreReport)
				};

				foreach (var type in telematicsTypes)
				{
					data.AddRange(Factory.Load(type, new ZDBOnlyQuery(type) { ReLoadExistingRows = true }));
				}

				return data;
			}

			protected override void SetUp()
			{
				base.SetUp();
				loggerMock = new Mock<ILogger>();
			}

			Mock<ILogger> loggerMock;
		}

		IEnumerable<BusinessObject> CreateObsoleteData(int numYearsToRetain, int numEntriesToAddToDb)
		{
			return Enumerable.Range(0, numEntriesToAddToDb)
				.Select(i =>
				{
					var dummyObject = Factory.New<DummyBusinessObject>();
					dummyObject.Z0_Date = new ZDateTime(ZDateTime.Now.Year - (numYearsToRetain + 1), 1, 1);
					return dummyObject;
				})
				.Cast<BusinessObject>()
				.ToList();
		}

		IEnumerable<BusinessObject> CreateRecentData(int numYearsToRetain, int numRecordsToAdd)
		{
			return Enumerable.Range(0, numRecordsToAdd)
				.Select(i =>
				{
					var dummyObject = Factory.New<DummyBusinessObject>();
					dummyObject.Z0_Date = new ZDateTime(ZDateTime.Now.Year - numYearsToRetain, 1, 1);
					return dummyObject;
				})
				.Cast<BusinessObject>()
				.ToList();
		}

		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
		}

		Mock<ILogger> loggerMock;
	}
}
