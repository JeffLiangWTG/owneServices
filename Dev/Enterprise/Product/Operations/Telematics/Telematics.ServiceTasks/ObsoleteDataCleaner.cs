using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks
{
	public class ObsoleteDataCleaner
	{
		public ObsoleteDataCleaner(ILogger serviceLogger) : this(serviceLogger,
			(nameof(GlbDeviceBattery), GlbDeviceBatterySchema.GDB_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceExternalVoltage), GlbDeviceExternalVoltageSchema.GDV_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceIgnition), GlbDeviceIgnitionSchema.GDI_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceLocation), GlbDeviceLocationSchema.V2_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceLog), GlbDeviceLogSchema.GDL_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceOdometer), GlbDeviceOdometerSchema.GDO_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceOnboardMass), GlbDeviceOnboardMassSchema.GDM_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceTemperature), GlbDeviceTemperatureSchema.GDT_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceTyreAlert), GlbDeviceTyreAlertSchema.GDA_MeasurementTimeUtc.Name),
			(nameof(GlbDeviceTyreReport), GlbDeviceTyreReportSchema.GDR_MeasurementTimeUtc.Name))
		{
		}

		internal ObsoleteDataCleaner(ILogger serviceLogger, params (string DataType, string MeasurementTimeSchemaColumnName)[] obsoleteDataTypes)
		{
			this.serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
			ObsoleteDataTypes = obsoleteDataTypes;
			batchesPerRun = TelematicsConfigurationRegistry.Instance.NumberOfBatchesDeletedPerRun.Value;
			batchSize = TelematicsConfigurationRegistry.Instance.BatchSize.Value;
			numYearsToKeepData = TelematicsConfigurationRegistry.Instance.NumberOfYearsToKeepData.Value;
		}

		public void Run(CancellationToken token)
		{
			serviceLogger.Log(LogType.Information, "Cleanup operation started.");
			foreach (var (dataType, measurementTimeSchemaColumnName) in ObsoleteDataTypes)
			{
				serviceLogger.Log(LogType.Information, FormattableString.Invariant($"{dataType}: Cleanup started."));
				for (var curBatch = 1; curBatch <= batchesPerRun; curBatch++)
				{
					token.ThrowIfCancellationRequested();
					serviceLogger.Log(LogType.Information, FormattableString.Invariant($"{dataType}: Batch {curBatch} out of {batchesPerRun}."));

					var obsoleteDate = new ZDateTime(ZDateTime.Now.Year - numYearsToKeepData, 1, 1);
					var sqlDelete = FormattableString.Invariant($@"
;WITH
	data AS
	(
		SELECT TOP {batchSize}
			*
		FROM
			{dataType} WITH (READPAST, READCOMMITTEDLOCK)
		WHERE
			{measurementTimeSchemaColumnName} < @ObsoleteDate
		ORDER BY {measurementTimeSchemaColumnName} ASC
	)
DELETE FROM data
");

					var numDeleted = Db.Connection.ExecuteNonQuery(sqlDelete, command =>
					{
						command.AddParameter("@ObsoleteDate", System.Data.SqlDbType.DateTime, obsoleteDate.ToDateTime());
					});

					serviceLogger.Log(LogType.Information, FormattableString.Invariant($"{dataType}: Deleted {numDeleted} record(s)."));

					if (numDeleted < batchSize)
					{
						break;
					}
				}
				serviceLogger.Log(LogType.Information, FormattableString.Invariant($"{dataType}: Cleanup finished."));
			}
			serviceLogger.Log(LogType.Information, "Cleanup operation completed.");
		}

		internal (string DataType, string MeasurementTimeSchemaColumn)[] ObsoleteDataTypes { get; }
		readonly ILogger serviceLogger;
		readonly int batchesPerRun;
		readonly int batchSize;
		readonly int numYearsToKeepData;
	}
}
