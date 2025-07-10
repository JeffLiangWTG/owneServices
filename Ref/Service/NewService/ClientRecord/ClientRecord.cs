using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public class ClientRecord : IClientRecord
	{
		public ClientRecord(IReferenceDataRepository repo, IDateTimeProvider dateTimeProvider)
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.repo = repo;
			this.dateTimeProvider = dateTimeProvider;
		}

		readonly IReferenceDataRepository repo;
		readonly IDateTimeProvider dateTimeProvider;

		public async Task RecordAsync(string dataSet, string clientId, string systemType, DateTime? clientTimestamp, string checkpoint, bool isInUse)
		{
			Argument.NotNullOrEmpty(dataSet, nameof(dataSet));
			Argument.NotNullOrEmpty(clientId, nameof(clientId));
			var record = repo.Get<ClientRefDbVersionControl>().Where(x => x.CVC_DataSet == dataSet && x.CVC_ClientId == clientId && x.CVC_IsInUse == isInUse).FirstOrDefault();
			var now = dateTimeProvider.GetUTCNow();
			if (record == null)
			{
				record = new ClientRefDbVersionControl
				{
					CVC_PK = Guid.NewGuid(),
					CVC_DataSet = dataSet,
					CVC_ClientId = clientId,
					CVC_DataSetTimestamp = clientTimestamp,
					CVC_DataSetCheckpoint = checkpoint,
					CVC_LastUpdatedTimeUTC = now,
					CVC_SystemType = systemType,
					CVC_IsInUse = isInUse
				};
				repo.Add(record);
				await repo.SaveChangesAsync();
			}
			else if (record.CVC_IsInUse == isInUse && (!record.CVC_LastUpdatedTimeUTC.HasValue
				|| record.CVC_LastUpdatedTimeUTC.Value.AddMinutes(LastUpdatedTimeUTCRefreshRateInMinutes) < now ||
				!Nullable.Equals(record.CVC_DataSetTimestamp, clientTimestamp) ||
				((record.CVC_DataSetCheckpoint ?? string.Empty) != (checkpoint ?? string.Empty))))
			{
				record.CVC_DataSetTimestamp = clientTimestamp;
				record.CVC_DataSetCheckpoint = checkpoint;
				record.CVC_SystemType = systemType;
				record.CVC_LastUpdatedTimeUTC = now;
				repo.Update(record);
				await repo.SaveChangesAsync();
			}
		}

		public const int LastUpdatedTimeUTCRefreshRateInMinutes = 60;
	}
}
