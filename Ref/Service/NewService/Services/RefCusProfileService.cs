using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefCusProfileService : ReferenceDataServiceBase<Models.RefCusProfile>
	{
		protected override string TableCode => "XX0";

		static RefCusProfileService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusProfileType, Models.RefCusProfileType>();
				cfg.CreateMap<RefCusProfile, Models.RefCusProfile>();
				cfg.CreateMap<RefCusProfileAttribute, Models.RefCusProfileAttribute>();
			});
		}

		public RefCusProfileService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
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

		IEnumerable<Models.RefCusProfile> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();
			var tariffTypes = RefDbRepo.Get<RefCusTariffType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZI_PK, x => mapper.Map<Models.RefCusTariffType>(x));
			var profileTypes = RefDbRepo.Get<RefCusProfileType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.XXX_PK, x => x);

			var dataSets = from profile in RefDbRepo.Get<RefCusProfile>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint) on profile.XX0_PK equals version.RVC_ParentPK

						   join profileAttribute in RefDbRepo.Get<RefCusProfileAttribute>() on profile.XX0_PK equals profileAttribute.XXY_XX0_Profile into profileAttributeG
						   from profileAttribute in profileAttributeG.DefaultIfEmpty()

						   select new { profile, version.RVC_Deleted, profileAttribute };

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.OrderBy(x => x.profile.XX0_PK).GroupBy(x => x.profile.XX0_PK);

			int idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{
				var profile = dataSetG.FirstOrDefault().profile;
				Models.RefCusProfileType profileType = null;
				if (profileTypes.TryGetValue(profile.XX0_XXX_ProfileType, out var safeProfileType) && tariffTypes.TryGetValue(safeProfileType.XXX_ZZI_TariffType, out var tariffType))
				{
					profileType = mapper.Map<Models.RefCusProfileType>(safeProfileType);
					profileType.RefCusTariffType = tariffType;

					var result = mapper.Map<Models.RefCusProfile>(profile);
					result.Deleted = dataSetG.FirstOrDefault().RVC_Deleted;
					result.RefCusProfileType = profileType;
					if (!result.Deleted)
					{
						result.RefCusProfileAttributes = dataSetG.Select(x => x.profileAttribute).NotNull().DistinctByKey(x => x.XXY_PK).Select(x => mapper.Map<Models.RefCusProfileAttribute>(x)).ToArray();
					}
					result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
					yield return result;
				}
			}
		}

		public override IEnumerable<Models.RefCusProfile> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}
