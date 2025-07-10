using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	abstract public class ReferenceDataServiceBase<T> : IReferenceDataService<T>
	{
		protected ReferenceDataServiceBase(IReadOnlyReferenceDataRepository refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));

			this.refDbRepo = refDbRepo;
		}

		readonly IReadOnlyReferenceDataRepository refDbRepo;
		protected IReadOnlyReferenceDataRepository RefDbRepo => refDbRepo;

		protected abstract string TableCode { get; }

		public abstract IEnumerable<T> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId);

		static ConcurrentDictionary<string, Lazy<RefDataSetInformation>> CachedDataSetInformation
		{
			get
			{
				if (_cachedDataSetInformation == null)
				{
					_cachedDataSetInformation = new ConcurrentDictionary<string, Lazy<RefDataSetInformation>>();
				}
				return _cachedDataSetInformation;
			}
		}
		static ConcurrentDictionary<string, Lazy<RefDataSetInformation>> _cachedDataSetInformation;

		public short GetDataSetId(string dataSetName)
		{
			if (dataSetName.Contains("0_"))
			{
				dataSetName = dataSetName.Substring(0, dataSetName.IndexOf("0_", StringComparison.OrdinalIgnoreCase));
			}
			return CachedDataSetInformation.GetOrAdd(dataSetName, key => new Lazy<RefDataSetInformation>(() =>
				 RefDbRepo.Get<RefDataSetInformation>().FirstOrDefault(x => x.RDS_DataSetName == key)
			 )).Value?.RDS_DataSetId ?? 0;
		}

		public virtual DateTime GetServerTimestamp()
		{
			return GetDataSetInformation().GetMaxLastUpdatedUTC();
		}

		public IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps(IEnumerable<Tuple<string, IEnumerable<DataSetVersion>>> dataSetVersions)
		{
			return dataSetVersions.FirstOrDefault(x => x.Item1 == TableCode)?.Item2;
		}

		protected IQueryable<RefDbVersionControl> GetVersionControls(short dataSetId)
		{
			return RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished);
		}

		IQueryable<RefDataSetInformation> GetDataSetInformation()
		{
			return RefDbRepo.Get<RefDataSetInformation>().Where(x => x.RDS_DataSetTableCode == TableCode);
		}

		public IEnumerable<Tuple<string, IEnumerable<DataSetVersion>>> GetAllDataSetTimestamps()
		{
			foreach (var g in RefDbRepo.Get<RefDataSetInformation>().GroupBy(x => x.RDS_DataSetTableCode, x => x))
			{
				yield return Tuple.Create(g.Key, g.Select(x => new DataSetVersion(x.RDS_DataSetName, x.GetLastUpdatedUTC())));
			}
		}
	}
}
