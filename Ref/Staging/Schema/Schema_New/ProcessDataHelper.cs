using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public class ProcessDataHelper : IProcessDataHelper
	{
		public ProcessDataHelper(IStagingRepository staging, string processName, string dataType)
		{
			Argument.NotNull(staging, nameof(staging));
			Argument.NotNullOrEmpty(processName, nameof(processName));
			Argument.NotNullOrEmpty(dataType, nameof(dataType));

			this.staging = staging;
			this.processName = processName;
			this.dataType = dataType;
		}

		public bool TryToGet<T>(out T value)
		{
			var result = false;
			value = default(T);
			if (CachedProcessData != null)
			{
				result = true;
				value = JsonConvert.DeserializeObject<T>(CachedProcessData.Data);
			}
			return result;
		}

		public void Update<T>(T value)
		{
			if (CachedProcessData == null)
			{
				CachedProcessData = new ProcessData
				{
					ID = Guid.NewGuid(),
					Name = processName,
					Type = dataType,
				};
				staging.Add(CachedProcessData);
			}
			CachedProcessData.Data = JsonConvert.ToString(value);
		}

		public void Reset()
		{
			if (CachedProcessData != null)
			{
				staging.Remove(CachedProcessData);
				CachedProcessData = null;
			}
		}

		ProcessData CachedProcessData
		{
			get { return fCachedProcessData ?? (fCachedProcessData = staging.Get<ProcessData>().Where(x => x.Name == processName && x.Type == dataType).FirstOrDefault()); }
			set { fCachedProcessData = value; }
		}
		ProcessData fCachedProcessData;

		readonly IStagingRepository staging;
		readonly string processName;
		readonly string dataType;
	}
}
