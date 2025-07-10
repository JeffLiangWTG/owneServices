using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService
{
	class CheckpointV2 : ICheckpoint
	{
		public CheckpointV2(Guid pk)
		{
			DataSetPK = pk;
		}

		public Guid DataSetPK { get; set; }

		public IQueryable<RefDbVersionControl> Filter(IQueryable<RefDbVersionControl> source)
		{
			Argument.NotNull(source, nameof(source));
			return source.Where(x => x.RVC_ParentPK.CompareTo(DataSetPK) > 0);
		}

		public IQueryable<T> FilterView<T>(IQueryable<T> source) where T : class, IDataSetView
		{
			Argument.NotNull(source, nameof(source));
			return source.Where(x => x.RVC_ParentPK.CompareTo(DataSetPK) > 0);
		}

		public override string ToString()
		{
			var result = JsonConvert.SerializeObject(this);
			return result;
		}
	}
}
