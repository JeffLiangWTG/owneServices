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
	public partial class RefCusProfileQuestionPathwayService : ReferenceDataServiceBase<Models.RefCusProfileQuestionPathway>
	{
		protected override string TableCode => "XQP";

		static RefCusProfileQuestionPathwayService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusTariffType, Models.RefCusTariffType>();
				cfg.CreateMap<RefCusProfileType, Models.RefCusProfileType>();
				cfg.CreateMap<RefCusProfileQuestion, Models.RefCusProfileQuestion>();
				cfg.CreateMap<RefCusProfileQuestionPathway, Models.RefCusProfileQuestionPathway>();
			});
		}

		public RefCusProfileQuestionPathwayService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
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

		public override IEnumerable<Models.RefCusProfileQuestionPathway> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}

		IEnumerable<Models.RefCusProfileQuestionPathway> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var mapper = Config.CreateMapper();
			var tariffTypes = RefDbRepo.Get<RefCusTariffType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.ZZI_PK, x => mapper.Map<Models.RefCusTariffType>(x));
			var profileTypes = RefDbRepo.Get<RefCusProfileType>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.XXX_PK, x => x);
			var profileQuestions = RefDbRepo.Get<RefCusProfileQuestion>().WhereNotDeleted(RefDbRepo).ToDictionary(x => x.XQ2_PK, x => x);
			var dataSets = RefDbRepo.Get<RefCusProfileQuestionPathway>().Join(GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint),
				x => x.XQP_PK, y => y.RVC_ParentPK, (tbl, version) => new { tbl, version });

			dataSets = dataSets.OrderBy(x => x.version.RVC_ParentPK);

			var idx = 0;
			foreach (var dataSet in dataSets)
			{
				var questionParentPK = dataSet.tbl.XQP_XQ2_QuestionParent;
				var questionChildPK = dataSet.tbl.XQP_XQ2_QuestionChild;
				if (profileQuestions.ContainsKey(questionParentPK) && profileQuestions.ContainsKey(questionChildPK))
				{
					var questionParent = profileQuestions[questionParentPK];
					var questionChild = profileQuestions[questionChildPK];
					if (profileTypes.TryGetValue(questionParent.XQ2_XXX_ProfileType, out var profileTypeParent) && profileTypes.TryGetValue(questionChild.XQ2_XXX_ProfileType, out var profileTypeChild) && tariffTypes.TryGetValue(profileTypeParent.XXX_ZZI_TariffType, out var tariffTypeParent) && tariffTypes.TryGetValue(profileTypeChild.XXX_ZZI_TariffType, out var tariffTypeChild))
					{
						var result = mapper.Map<Models.RefCusProfileQuestionPathway>(dataSet.tbl);
						result.Deleted = dataSet.version.RVC_Deleted;

						var mappedQuestionParent = mapper.Map<Models.RefCusProfileQuestion>(questionParent);
						var mappedProfileTypeParent = mapper.Map<Models.RefCusProfileType>(profileTypeParent);
						mappedProfileTypeParent.RefCusTariffType = tariffTypeParent;
						mappedQuestionParent.RefCusProfileType = mappedProfileTypeParent;
						result.RefCusProfileQuestionParent = mappedQuestionParent;

						var mappedQuestionChild = mapper.Map<Models.RefCusProfileQuestion>(questionChild);
						var mappedProfileTypeChild = mapper.Map<Models.RefCusProfileType>(profileTypeChild);
						mappedProfileTypeChild.RefCusTariffType = tariffTypeChild;
						mappedQuestionChild.RefCusProfileType = mappedProfileTypeChild;
						result.RefCusProfileQuestionChild = mappedQuestionChild;

						result.WriteCheckpoint(dataSet.tbl.XQP_PK, ref idx, chunkSize);
						yield return result;
					}
				}
			}
		}
	}
}
