using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.PassarCodeListsSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	public class PassarConditionTypesParser : BaseConditionTypesParser<codeLists>
	{
		protected override string DataSourceName => "CH Passar Condition Type";

		protected override string OutPutFileName => "RefCusConditionTypeZZ_CH_Passar.xml";

		protected override string LogFileSuffix => "Passar";

		public PassarConditionTypesParser(DownloadResult download) : base(download)
		{
		}

		protected override DateTime CreationTime(codeLists input) => input.created;

		protected override IEnumerable<RefCusConditionType> ConvertConditions(codeLists input, DateTime actualDate)
		{
			IEnumerable<codeListsCodeListCode> inputEntries = (from d in input.codeList
															   where d.id == CodeListId
															   select d.code
															   ).FirstOrDefault();
			if (inputEntries != null)
			{
				foreach (var inputEntryGroup in from entry in inputEntries
												group entry by entry.codeNr into entryGroup
												select entryGroup)
				{
					var inputEntry = (from entry in inputEntryGroup
									  orderby entry.validTo descending
									  select entry).First();

					yield return ConvertCondition(inputEntry, PermitAuthorityPrefix);
					yield return ConvertCondition(inputEntry, NonCustomsLawPrefix);
				}
			}
		}

		static RefCusConditionType ConvertCondition(codeListsCodeListCode inputEntry, string prefix)
		{
			return new RefCusConditionType
			{
				ZX2_ConditionType = prefix + inputEntry.codeNr,
				ZX2_Description = inputEntry.MeaningDe.Truncate(500),
				RefCusConditionTypeLanguages = ConvertConditionTypeLanguages(inputEntry)
			};
		}

		const string CodeListId = "NCL1115";

		const string PermitAuthorityPrefix = "PP";
		const string NonCustomsLawPrefix = "PN";
	}
}
