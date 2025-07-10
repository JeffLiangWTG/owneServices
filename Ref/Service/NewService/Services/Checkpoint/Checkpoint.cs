using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService
{
	[Obsolete]
	class Checkpoint : ICheckpoint
	{
		public Checkpoint(DateTime timestamp, Guid pk)
		{
			this.DataSetPK = pk;
			this.DataSetTimestamp = timestamp;
		}

		public Guid DataSetPK { get; set; }
		public DateTime DataSetTimestamp { get; set; }

		public override string ToString()
		{
			var result = JsonConvert.SerializeObject(this);
			return result;
		}

		public IQueryable<RefDbVersionControl> Filter(IQueryable<RefDbVersionControl> source)
		{
			Argument.NotNull(source, nameof(source));
			return source.Where(x => x.RVC_LastUpdatedUTC < DataSetTimestamp ||
				(x.RVC_LastUpdatedUTC == DataSetTimestamp && x.RVC_ParentPK.CompareTo(DataSetPK) > 0));
		}

		public IQueryable<T> FilterView<T>(IQueryable<T> source) where T : class, IDataSetView
		{
			Argument.NotNull(source, nameof(source));
			return source.Where(x => x.RVC_LastUpdatedUTC < DataSetTimestamp ||
				(x.RVC_LastUpdatedUTC == DataSetTimestamp && x.RVC_ParentPK.CompareTo(DataSetPK) > 0));
		}
	}
}
