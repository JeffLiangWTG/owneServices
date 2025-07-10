using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefTimeZoneSetService : ReferenceDataServiceBase<Models.RefTimeZoneSet>
	{
		protected override string TableCode => "R3";

		static RefTimeZoneSetService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefTimeZoneSet, Models.RefTimeZoneSet>();
				cfg.CreateMap<RefTimeZoneRule, Models.RefTimeZoneRule>();
				cfg.CreateMap<RefTimeZone, Models.RefTimeZone>();
			});
		}

		public RefTimeZoneSetService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null) throw new InvalidProgramException("Mapper configuration should not be set second time");
				else config = value;
			}
		}

		public override IEnumerable<Models.RefTimeZoneSet> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}

		IEnumerable<Models.RefTimeZoneSet> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();

			var versionsChunk = GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint).OrderBy(x => x.RVC_ParentPK).ToArray();
			var timeZoneRuleChunk = RefDbRepo.Get<RefTimeZoneRule>().GetChunk(GetVersionControls(datasetId), x => x.R4_DataSetPK, lowerTimestamp, upperTimestamp, checkpoint);
			var timeZoneChunk = RefDbRepo.Get<RefTimeZone>().GetChunk(GetVersionControls(datasetId), x => x.R2_R3_TimeZoneSet, lowerTimestamp, upperTimestamp, checkpoint);
			// always retrieve timezoneset the last
			var timeZoneSetChunk = RefDbRepo.Get<RefTimeZoneSet>().GetChunk(GetVersionControls(datasetId), x => x.R3_PK, lowerTimestamp, upperTimestamp, checkpoint);

			var timeZoneSetIdx = 0;
			var timeZoneRuleIdx = 0;
			var versionIdx = 0;
			var timeZoneIdx = 0;

			while (timeZoneSetIdx < timeZoneSetChunk.Length)
			{
				var timeZoneSet = timeZoneSetChunk[timeZoneSetIdx];
				var dataSetPK = timeZoneSet.R3_PK;

				var result = mapper.Map<Models.RefTimeZoneSet>(timeZoneSet);
				var versions = versionsChunk.GetSetData(x => x.RVC_ParentPK, dataSetPK, ref versionIdx, RefDbRepo.IsDbProvider);
				result.Deleted = versions.FirstOrDefault().RVC_Deleted;

				var timeZone = timeZoneChunk.GetSetData(x => x.R2_R3_TimeZoneSet, dataSetPK, ref timeZoneIdx);
				result.RefTimeZoneStandardZone = mapper.Map<Models.RefTimeZone>(timeZone.FirstOrDefault(x => x.R2_R3_TimeZoneSet == timeZoneSet.R3_PK && x.R2_Type == "STD"));
				result.RefTimeZoneDaylightSavingZone = mapper.Map<Models.RefTimeZone>(timeZone.FirstOrDefault(x => x.R2_R3_TimeZoneSet == timeZoneSet.R3_PK && x.R2_Type == "DLS"));

				if (!result.Deleted)
				{
					var timeZoneRule = timeZoneRuleChunk.GetSetData(x => x.R4_DataSetPK, dataSetPK, ref timeZoneRuleIdx);

					if (result.RefTimeZoneDaylightSavingZone != null)
					{
						result.RefTimeZoneDaylightSavingZone.RefTimeZoneRules = ConvertFromEnumerable(timeZoneRule.Where(x => timeZone.First(y => y.R2_Type == "DLS").R2_PK == x.R4_R2));
					}
					if (result.RefTimeZoneStandardZone != null)
					{
						result.RefTimeZoneStandardZone.RefTimeZoneRules = ConvertFromEnumerable(timeZoneRule.Where(x => timeZone.First(y => y.R2_Type == "STD").R2_PK == x.R4_R2));
					}
				}
				timeZoneSetIdx++;
				if (timeZoneSetIdx % chunkSize == 0)
				{
					result.Checkpoint = CheckpointHelper.Create(dataSetPK).ToString();
				}
				yield return result;
			}
		}

		static Models.RefTimeZoneRule[] ConvertFromEnumerable(IEnumerable<RefTimeZoneRule> array)
		{
			var mapper = Config.CreateMapper();
			var result = new List<Models.RefTimeZoneRule>();
			foreach (var timeZoneRule in array)
			{
				result.Add(mapper.Map<Models.RefTimeZoneRule>(timeZoneRule));
			}

			return result.ToArray();
		}
	}
}
