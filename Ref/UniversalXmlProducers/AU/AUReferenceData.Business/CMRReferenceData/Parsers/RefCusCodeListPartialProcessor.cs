using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class RefCusCodeListPartialProcessor : RefDataEntityPartialProcessor<RefCusCodeList>
	{
		IRefDataLoader RefDataLoader { get; }

		string CodeType { get;  }

		public RefCusCodeListPartialProcessor(IRefDataLoader refDataLoader, string codeType)
		{
			RefDataLoader = refDataLoader;
			CodeType = codeType;
		}

		public override Func<RefCusCodeList, string> GetEntityKeySelector() => entity => entity.ZZD_Code;

		public override Dictionary<string, RefCusCodeList> LoadExistingEntities(string[] keys)
		{
			if (keys.Length == 0)
			{
				return new Dictionary<string, RefCusCodeList>();
			}

			var query = $"RefCusCodeListUpdate?$filter=ZZD_ZZZ_NKDataGrouping eq '{Constants.DataGrouping}' and ZZD_ZZK_NKCodeType eq '{CodeType}' and ZZD_Code in ('{string.Join("','", keys)}')&$expand=RefCusCodeListAttributes";
			var entities = RefDataLoader.LoadData<RefCusCodeList>(query).Result;
			return entities.ToDictionary(e => e.ZZD_Code, e => e);
		}

		public override bool ShouldSkipDeleteEntity(RefCusCodeList entity, RefCusCodeList existing, DateTime publishedDate) => existing.ZZD_EndDate <= publishedDate;

		public override void DeleteEntity(RefCusCodeList entity, DateTime publishedDate) => entity.ZZD_EndDate = publishedDate;

		public override bool ShouldSkipUpdateEntity(RefCusCodeList entity, RefCusCodeList existing, DateTime publishedDate)
		{
			var changeAttribute = entity.RefCusCodeListAttributes[0];
			return existing.RefCusCodeListAttributes.Any(x => x.ZZE_Value == changeAttribute.ZZE_Value && x.ZZE_EndDate > publishedDate);
		}

		public override void UpdateEntity(RefCusCodeList entity, RefCusCodeList existing, DateTime publishedDate)
		{
			var expiredAttributes = existing.RefCusCodeListAttributes.Where(x => x.ZZE_EndDate > publishedDate);
			foreach (var expiredAttribute in expiredAttributes)
			{
				expiredAttribute.ZZE_EndDate = publishedDate;
				entity.RefCusCodeListAttributes = entity.RefCusCodeListAttributes.Prepend(expiredAttribute).ToArray();
			}
		}
	}
}
