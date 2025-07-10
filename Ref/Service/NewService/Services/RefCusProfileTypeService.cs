using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefCusProfileTypeService : ReferenceDataServiceBase<Models.RefCusProfileType>
	{
		protected override string TableCode => "XXX";

		static RefCusProfileTypeService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusProfileType, Models.RefCusProfileType>();
			});
		}

		public RefCusProfileTypeService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null)
					throw new InvalidProgramException("Mapper configuration should not be set second time");
				else
					config = value;
			}
		}

		public override IEnumerable<Models.RefCusProfileType> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}

		IEnumerable<Models.RefCusProfileType> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();
			var dataSets = from profileType in RefDbRepo.Get<RefCusProfileType>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint) on profileType.XXX_PK equals version.RVC_ParentPK
						   join tariffType in RefDbRepo.Get<RefCusTariffType>().WhereNotDeleted(RefDbRepo) on profileType.XXX_ZZI_TariffType equals tariffType.ZZI_PK
						   select new { profileType, version.RVC_Deleted, tariffType };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.OrderBy(x => x.profileType.XXX_PK).GroupBy(x => x.profileType.XXX_PK);

			int idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{
				var dataSet = dataSetG.FirstOrDefault().profileType;

				var result = mapper.Map<Models.RefCusProfileType>(dataSet);
				result.Deleted = dataSetG.FirstOrDefault().RVC_Deleted;
				result.RefCusTariffType = mapper.Map<Models.RefCusTariffType>(dataSetG.FirstOrDefault().tariffType);
				result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
				yield return result;
			}
		}
	}
}
