using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList DeclarationTypeList()
		{
			return Factory.GetCachedValue("DeclarationTypeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(TWRefCusCodeListLoader.GetImportDeclarationType(Factory, ZDateTime.Today));
				result.AddRange(TWRefCusCodeListLoader.GetExportDeclarationType(Factory, ZDateTime.Today));
				result.Sort();
				return result;
			});
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<JobDeclarationMessageStatusList>();

		public CodeDescriptionPairList ClearanceStatusList() => Factory.GetCachedValue<ClearanceStatusCodeList>();

		public CodeDescriptionPairList AgencyResponseCodeList() => Factory.GetCachedValue<CPT_016_RejectionReasons>();

		public CodeDescriptionPairList RequiredFormalitiesCodeList() => Factory.GetCachedValue<CTP_017_ErrorDocumentsOrRequiredFormalities>();

		public CodeDescriptionPairList ClearanceCodeList() => Factory.GetCachedValue<CPT_025_ExtraCondition>();
	}
}
