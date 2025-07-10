using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public partial class RefCusCodeTypeService
	{
		static MapperConfiguration configUpToV26;
		static void SetupMapperConfiguration()
		{
			configUpToV26 = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusCodeType, Models.RefCusCodeType>();
				cfg.CreateMap<RefCusCodeTypeLanguage, Models.RefCusCodeTypeLanguage>();
				cfg.CreateMap<RefCusCodeListAttributeName, Models.RefCusCodeListAttributeName>();
				cfg.CreateMap<RefCusCodeListAttributeNameLanguage, Models.RefCusCodeListAttributeNameLanguage>();
			});
		}

		public IEnumerable<Models.RefCusCodeType> GetDataUpToV26(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			SetupMapperConfiguration();
			return GetDataCoreUpToV26(lowerTimestamp, upperTimestamp, checkpointPK, chunkSize, dataSetId);
		}

		IEnumerable<Models.RefCusCodeType> GetDataCoreUpToV26(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			var mapper = configUpToV26.CreateMapper();
			var codeTypeQuery = from type in RefDbRepo.Get<RefCusCodeType>()
								join version in GetVersionControls(dataSetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK) on type.ZZK_PK equals version.RVC_ParentPK
								group new { type, version.RVC_Deleted }
								by type.ZZK_CodeType;
			var codeTypes = codeTypeQuery.Select(x => x.FirstOrDefault().type).ToArray();
			var codeTypeDeletionFlags = codeTypeQuery.ToDictionary(x => x.FirstOrDefault().type.ZZK_CodeType, x => x.All(y => y.RVC_Deleted));

			var dataSets = from type in codeTypes

						   join attributeName in RefDbRepo.Get<RefCusCodeListAttributeName>() on type.ZZK_CodeType equals attributeName.ZXE_ZZK_NKCodeType into atrriG
						   from attributeName in atrriG.DefaultIfEmpty()

						   join attrVersion in RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_ParentCode == "ZXE" && x.RVC_IsPublished) on attributeName?.ZXE_PK equals attrVersion.RVC_ParentPK into versionG
						   from attrVersion in versionG.DefaultIfEmpty()

						   join typeLanguage in RefDbRepo.Get<RefCusCodeTypeLanguage>() on type.ZZK_PK equals typeLanguage.ZXI_ZZK_CodeType into typeLanguageDb
						   from typeLanguage in typeLanguageDb.DefaultIfEmpty()

						   group new { type, attributeName, typeLanguage, attrVersion?.RVC_Deleted }
						   by type.ZZK_PK;

			dataSets = dataSets.OrderBy(x => x.Key);
			var attributeNameLanguages = RefDbRepo.Get<RefCusCodeListAttributeNameLanguage>().ToArray();

			int idx = 0;
			foreach (var dataSetG in dataSets)
			{
				var dataSet = dataSetG.ToArray();
				var result = mapper.Map<Models.RefCusCodeType>(dataSet[0].type);
				result.Deleted = codeTypeDeletionFlags[result.ZZK_CodeType];
				if (!result.Deleted)
				{
					var attributeNames = dataSet.Where(x => x.RVC_Deleted == false).Select(x => x.attributeName).Where(x => x != null).DistinctByKey(x => x.ZXE_PK);
					result.RefCusCodeListAttributeNames = GetAttributeNames(mapper, attributeNames, attributeNameLanguages);
					result.RefCusCodeTypeLanguages = dataSet.Select(x => x.typeLanguage).Where(x => x != null).DistinctByKey(x => x.ZXI_PK).Select(x => mapper.Map<Models.RefCusCodeTypeLanguage>(x)).ToArray();
				}

				result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
				yield return result;
			}
		}

		public static Models.RefCusCodeListAttributeName[] GetAttributeNames(IMapper mapper, IEnumerable<RefCusCodeListAttributeName> attributeNames, IEnumerable<RefCusCodeListAttributeNameLanguage> attriNameLanguages)
		{
			Argument.NotNull(mapper, nameof(mapper));
			Argument.NotNull(attributeNames, nameof(attributeNames));
			var attributeNameList = new List<Models.RefCusCodeListAttributeName>();
			foreach (var attributeName in attributeNames)
			{
				var pk = attributeName.ZXE_PK;
				var attriName = mapper.Map<Models.RefCusCodeListAttributeName>(attributeName);
				if (attriNameLanguages != null)
				{
					attriName.RefCusCodeListAttributeNameLanguages = attriNameLanguages.Where(x => x.ZXH_ZXE_CodeListAttributeName == pk).Select(x => mapper.Map<Models.RefCusCodeListAttributeNameLanguage>(x)).ToArray();
				}
				attributeNameList.Add(attriName);
			}

			return attributeNameList.ToArray();
		}
	}
}
