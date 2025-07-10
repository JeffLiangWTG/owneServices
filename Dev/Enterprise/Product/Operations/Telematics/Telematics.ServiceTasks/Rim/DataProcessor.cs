using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Newtonsoft.Json;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	class DataProcessor : IDataProcessor
	{
		public DataProcessor(IDataRecordNumberStrategy dataRecordNumberStrategy)
		{
			this.dataRecordNumberStrategy = dataRecordNumberStrategy ?? throw new ArgumentNullException(nameof(dataRecordNumberStrategy));
		}

		public IEnumerable<IPortionedData> Process(BusinessObjectFactory factory, IEnumerable<IDeviceData> data, int maximumRecordsInBatch, CancellationToken cancellationToken)
		{
			_ = data ?? throw new ArgumentNullException(nameof(data));

			return PortionData();

			IEnumerable<IPortionedData> PortionData()
			{
				foreach (var deviceData in data)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						yield break;
					}

					var locationLists = deviceData.Locations
						.Select((x, i) => new { Index = i, Value = x })
						.GroupBy(tuple => (tuple.Index / maximumRecordsInBatch))
						.Select(group => group
							.Select(tuple => tuple.Value));

					foreach (var locations in locationLists)
					{
						yield return SerializeToRimJsonLocations(factory, locations);
					}
				}
			}
		}

		PortionedData SerializeToRimJsonLocations(BusinessObjectFactory factory, IEnumerable<GlbDeviceLocation> records)
		{
			#region SuppressResourceStringsCheckRegion
			var batchId = dataRecordNumberStrategy.GetNextFormatted(factory);
			return new PortionedData
			{
				BatchId = batchId,
				Message = JsonConvert.SerializeObject(new
				{
					batchId = batchId,
					tdeVersion = "2.0",
					deviceRecords = records.GroupBy(record => record.Device.V3_HardwareIdentifier)
						.Select(tuple => new Dictionary<string, object>
						{
							{ "device", new  { id = tuple.Key } },
							{ "records", tuple
								.Select(record =>
								{
									record.V2_RimReported = true;
									return new Dictionary<string, object>
									{
										{ "dateTime", $"{record.V2_MeasurementTimeUtc.ToISO8601String()}Z" } ,
										{ "type", "POSITION" },
										{ "receiptDateTime", $"{record.V2_MeasurementTimeUtc.ToISO8601String()}Z" },
										{ "position", new { latitude = record.Location.Latitude, longitude = record.Location.Longitude } },
									};
								})
							},
						})
				})
			};
			#endregion
		}

		class PortionedData : IPortionedData
		{
			public string BatchId { get; set; }
			public string Message { get; set; }
		}

		readonly IDataRecordNumberStrategy dataRecordNumberStrategy;
	}
}
