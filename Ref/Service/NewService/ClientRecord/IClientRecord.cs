using System;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NewService
{
	public interface IClientRecord
	{
		Task RecordAsync(string dataSet, string clientId, string systemType, DateTime? clientTimestamp, string checkpoint, bool isInUse);
	}
}
