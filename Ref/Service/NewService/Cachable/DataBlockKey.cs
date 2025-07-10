using System;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService
{
	public class DataBlockKey<T> : IDataBlockKey<T>
	{
		public DataBlockKey(DateTime? lowerTimestamp, DateTime upperTimestamp, string checkpoint, string dataset)
		{
			LowerTimestamp = lowerTimestamp;
			UpperTimestamp = upperTimestamp;
			Checkpoint = checkpoint;
			DataSet = dataset;
		}

		public string GetKey()
		{
			var result = JsonConvert.SerializeObject(this);
			result = result.Replace(ForbiddenCharacterInMutex, '_');
			return result;
		}

		public IDataBlockKey<T> CaculateNextKey(string checkpoint)
		{
			IDataBlockKey<T> result = null;
			if (!string.IsNullOrEmpty(checkpoint))
			{
				result = new DataBlockKey<T>(LowerTimestamp, UpperTimestamp, checkpoint, DataSet);
			}

			return result;
		}

		public DateTime? LowerTimestamp { get; private set; }
		public DateTime UpperTimestamp { get; private set; }
		public string Checkpoint { get; private set; }
		public string DataSet { get; private set; }
		public int Version => 2;

		const char ForbiddenCharacterInMutex = '\\';
	}
}
