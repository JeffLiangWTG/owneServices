using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefCusProfileQuestionService : ReferenceDataServiceBase<Models.RefCusProfileQuestion>
	{
		protected override string TableCode => "XQ2";

		static RefCusProfileQuestionService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusProfileType, Models.RefCusProfileType>();
				cfg.CreateMap<RefCusProfileQuestion, Models.RefCusProfileQuestion>();
				cfg.CreateMap<RefCusProfileQuestionAnswerList, Models.RefCusProfileQuestionAnswerList>();
				cfg.CreateMap<RefCusProfileQuestionAnswerListLanguage, Models.RefCusProfileQuestionAnswerListLanguage>();
				cfg.CreateMap<RefCusProfileQuestionAttribute, Models.RefCusProfileQuestionAttribute>();
				cfg.CreateMap<RefCusProfileQuestionLanguage, Models.RefCusProfileQuestionLanguage>();
			});
		}

		public RefCusProfileQuestionService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
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

		IEnumerable<Models.RefCusProfileQuestion> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();
			var tariffTypes = RefDbRepo.Get<RefCusTariffType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZI_PK, x => mapper.Map<Models.RefCusTariffType>(x));
			var profileTypes = RefDbRepo.Get<RefCusProfileType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.XXX_PK, x => x);

			var questionIdx = 0;
			var answerListIdx = 0;
			var answerListLanguageIdx = 0;
			var attributeIdx = 0;
			var languageIdx = 0;
			var versionIdx = 0;

			var answerListChunk = RefDbRepo.Get<RefCusProfileQuestionAnswerList>().GetChunk(GetVersionControls(datasetId), x => x.XQ4_XQ2_Question, lowerTimestamp, upperTimestamp, checkpoint);
			var answerListLanguageChunk = RefDbRepo.Get<RefCusProfileQuestionAnswerListLanguage>().GetChunk(GetVersionControls(datasetId), x => x.XAL_DataSetPK, lowerTimestamp, upperTimestamp, checkpoint);
			var attributeChunk = RefDbRepo.Get<RefCusProfileQuestionAttribute>().GetChunk(GetVersionControls(datasetId), x => x.XQ3_XQ2_Question, lowerTimestamp, upperTimestamp, checkpoint);
			var languageChunk = RefDbRepo.Get<RefCusProfileQuestionLanguage>().GetChunk(GetVersionControls(datasetId), x => x.XQL_XQ2_Question, lowerTimestamp, upperTimestamp, checkpoint);
			var versionsChunk = GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint).OrderBy(x => x.RVC_ParentPK).ToArray();
			var questionChunk = RefDbRepo.Get<RefCusProfileQuestion>().GetChunk(GetVersionControls(datasetId), x => x.XQ2_PK, lowerTimestamp, upperTimestamp, checkpoint);

			while (questionIdx < questionChunk.Length)
			{
				var question = questionChunk[questionIdx];
				var dataSetPK = question.XQ2_PK;
				if (profileTypes.TryGetValue(question.XQ2_XXX_ProfileType, out var safeProfileType) && tariffTypes.TryGetValue(safeProfileType.XXX_ZZI_TariffType, out var tariffType))
				{
					var profileType = mapper.Map<Models.RefCusProfileType>(safeProfileType);
					profileType.RefCusTariffType = tariffType;

					var result = mapper.Map<Models.RefCusProfileQuestion>(question);
					var versions = versionsChunk.GetSetData(x => x.RVC_ParentPK, dataSetPK, ref versionIdx, RefDbRepo.IsDbProvider);
					result.Deleted = versions.FirstOrDefault().RVC_Deleted;
					result.RefCusProfileType = profileType;
					if (!result.Deleted)
					{
						var answerListLanguages = answerListLanguageChunk.GetSetData(x => x.XAL_DataSetPK, dataSetPK, ref answerListLanguageIdx);
						result.RefCusProfileQuestionAnswerLists = answerListChunk.GetSetData(x => x.XQ4_XQ2_Question, dataSetPK, ref answerListIdx).Select(x =>
						{
							var answerList = mapper.Map<Models.RefCusProfileQuestionAnswerList>(x);
							answerList.RefCusProfileQuestionAnswerListLanguages = answerListLanguages.Where(a => a.XAL_XQ4_QuestionAnswer == x.XQ4_PK).Select(a => mapper.Map<Models.RefCusProfileQuestionAnswerListLanguage>(a)).ToArray();
							return answerList;
						}).ToArray();
						result.RefCusProfileQuestionAttributes = attributeChunk.GetSetData(x => x.XQ3_XQ2_Question, dataSetPK, ref attributeIdx).Select(x => mapper.Map<Models.RefCusProfileQuestionAttribute>(x)).ToArray();
						result.RefCusProfileQuestionLanguages = languageChunk.GetSetData(x => x.XQL_XQ2_Question, dataSetPK, ref languageIdx).Select(x => mapper.Map<Models.RefCusProfileQuestionLanguage>(x)).ToArray();
					}
					questionIdx++;
					if (questionIdx % chunkSize == 0)
					{
						result.Checkpoint = CheckpointHelper.Create(dataSetPK).ToString();
					}
					yield return result;
				}
			}
		}

		public override IEnumerable<Models.RefCusProfileQuestion> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}
