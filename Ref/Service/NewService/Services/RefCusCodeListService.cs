using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusCodeListService : ReferenceDataServiceBase<Models.RefCusCodeList>
	{
		protected override string TableCode => "ZZD";

		static RefCusCodeListService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusCodeList, Models.RefCusCodeList>();
				cfg.CreateMap<RefCusCodeListAttribute, Models.RefCusCodeListAttribute>();
				cfg.CreateMap<RefCusCodeListLanguage, Models.RefCusCodeListLanguage>();
				cfg.CreateMap<RefCusCodeOrAttributeTransportMode, Models.RefCusCodeOrAttributeTransportMode>();
			});
		}

		public RefCusCodeListService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
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

		IEnumerable<Models.RefCusCodeList> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();

			var codelistAttributeChunk = RefDbRepo.Get<RefCusCodeListAttribute>().GetChunk(GetVersionControls(datasetId), x => x.ZZE_ZZD_CodeList,
				lowerTimestamp, upperTimestamp, checkpointPK);
			var languageChunk = RefDbRepo.Get<RefCusCodeListLanguage>().GetChunk(GetVersionControls(datasetId), x => x.ZXA_ZZD_CodeList,
				lowerTimestamp, upperTimestamp, checkpointPK);
			var transportChunk = RefDbRepo.Get<RefCusCodeOrAttributeTransportMode>().GetChunk(GetVersionControls(datasetId), x => x.ZZU_DataSetPK,
				lowerTimestamp, upperTimestamp, checkpointPK);
			var versionsChunk = GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK).OrderBy(x => x.RVC_ParentPK).ToArray();
			// always retrieve codelist the last
			var codelists = RefDbRepo.Get<RefCusCodeList>()
				.GetChunk(GetVersionControls(datasetId), x => x.ZZD_PK, lowerTimestamp, upperTimestamp, checkpointPK);

			var codelistIdx = 0;
			var codelistAttributeIdx = 0;
			var languageIdx = 0;
			var versionIdx = 0;
			var transportIdx = 0;

			while (codelistIdx < codelists.Length)
			{
				var codelist = codelists[codelistIdx];
				var dataSetPK = codelist.ZZD_PK;
				var result = mapper.Map<Models.RefCusCodeList>(codelist);
				var versions = versionsChunk.GetSetData(x => x.RVC_ParentPK, dataSetPK, ref versionIdx, RefDbRepo.IsDbProvider);
				result.Deleted = versions.FirstOrDefault().RVC_Deleted;
				if (!result.Deleted)
				{
					result.RefCusCodeListLanguages = languageChunk.GetSetData(x => x.ZXA_ZZD_CodeList, dataSetPK, ref languageIdx)
						.Select(x => mapper.Map<Models.RefCusCodeListLanguage>(x)).ToArray();

					var transport = transportChunk.GetSetData(x => x.ZZU_DataSetPK, dataSetPK, ref transportIdx).GroupBy(x => x.ZZU_ZZD_CodeList ?? x.ZZU_ZZE_Attribute, x =>
					{
						Models.RefCusCodeOrAttributeTransportMode transportModel = null;
						transportModel = mapper.Map<Models.RefCusCodeOrAttributeTransportMode>(x);
						return transportModel;
					});
					result.RefCusCodeOrAttributeTransportModes = transport.Where(x => x.Key == codelist.ZZD_PK).SelectMany(x => x).NotNull().ToArray();

					result.RefCusCodeListAttributes = codelistAttributeChunk.GetSetData(x => x.ZZE_ZZD_CodeList, dataSetPK, ref codelistAttributeIdx).Select(x =>
					{
						var attributeModel = mapper.Map<Models.RefCusCodeListAttribute>(x);
						attributeModel.RefCusCodeOrAttributeTransportModes = transport.Where(r => r.Key == x.ZZE_PK).SelectMany(r => r).NotNull().ToArray();
						return attributeModel;
					}).ToArray();
				}
				codelistIdx++;
				if (codelistIdx % chunkSize == 0)
				{
					result.Checkpoint = CheckpointHelper.Create(dataSetPK).ToString();
				}
				yield return result;
			}
		}

		public override IEnumerable<Models.RefCusCodeList> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}
