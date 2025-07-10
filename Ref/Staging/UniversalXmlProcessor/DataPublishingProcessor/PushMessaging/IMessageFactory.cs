using System.Collections.Generic;
using System.Data;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	public interface IMessageFactory
	{
		Dictionary<string, List<short>> GetSubscribers(IDbCommand cmd);
		void SendMessage(IDbCommand cmd, int messageBatchSize);
		void AddBatchToMessage(IDbCommand cmd, IEnumerable<short> dataSetIds);
	}
}
