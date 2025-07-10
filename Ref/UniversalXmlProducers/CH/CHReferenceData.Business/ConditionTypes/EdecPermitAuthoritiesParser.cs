using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.DomainSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	public class EdecPermitAuthoritiesParser : EdecConditionTypesParser
	{
		public EdecPermitAuthoritiesParser(DownloadResult download) : base(download)
		{
		}

		protected override RefCusConditionType ConvertCondition(domainsDomainEntry inputEntry)
		{
			return new RefCusConditionType
			{
				ZX2_ConditionType = "PA" + inputEntry.value,
				ZX2_Description = inputEntry.meaningDe.Truncate(500),
				RefCusConditionTypeLanguages = ConvertConditionTypeLanguages(inputEntry),
			};
		}

		protected override string DomainName => "permitAuthority";

		protected override string DataSourceName => "CH PermitAuthority Condition Type";

		protected override string OutPutFileName => "RefCusConditionTypeZZ_CH_PA.xml";

		protected override string LogFileSuffix => "PermitAuthorityCondition";
	}
}
