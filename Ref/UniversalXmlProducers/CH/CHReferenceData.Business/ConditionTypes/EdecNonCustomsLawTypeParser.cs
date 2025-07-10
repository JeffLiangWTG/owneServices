using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.DomainSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	public class EdecNonCustomsLawTypeParser : EdecConditionTypesParser
	{
		public EdecNonCustomsLawTypeParser(DownloadResult download) : base(download)
		{
		}

		protected override RefCusConditionType ConvertCondition(domainsDomainEntry inputEntry)
		{
			return new RefCusConditionType
			{
				ZX2_ConditionType = "N" + inputEntry.value.ToUpperInvariant(),
				ZX2_Description = inputEntry.meaningDe.Truncate(500),
				RefCusConditionTypeLanguages = ConvertConditionTypeLanguages(inputEntry)
			};
		}

		protected override string DataSourceName => "CH NonCustomsLaw Condition Type";

		protected override string OutPutFileName => "RefCusConditionTypeZZ_CH_NCLT.xml";

		protected override string LogFileSuffix => "NonCustomsLawTypeCondition";

		protected override string DomainName => "nonCustomsLawType";
	}
}
