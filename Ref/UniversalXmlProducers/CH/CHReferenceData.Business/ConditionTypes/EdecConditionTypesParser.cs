using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.DomainSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	public abstract class EdecConditionTypesParser : BaseConditionTypesParser<domains>
	{
		protected abstract string DomainName { get; }

		protected EdecConditionTypesParser(DownloadResult download) : base(download)
		{
		}

		protected override DateTime CreationTime(domains input) => input.created;

		protected override IEnumerable<RefCusConditionType> ConvertConditions(domains input, DateTime actualDate)
		{
			IEnumerable<domainsDomainEntry> inputEntries = (from d in input.domain
															where d.name == DomainName
															select d.entry
					 ).FirstOrDefault();
			if (inputEntries == null)
			{
				throw new InvalidOperationException($@"ConditionTypes ""{DomainName}"" not found");
			}

			foreach (var inputEntryGroup in from entry in inputEntries
											group entry by entry.value.ToUpperInvariant() into entryGroup
											select entryGroup)
			{
				var inputEntry = (from entry in inputEntryGroup
								  where entry.validFrom <= actualDate && entry.validTo >= actualDate
								  orderby entry.validTo descending
								  select entry).FirstOrDefault();
				if (inputEntry == null)
				{
					inputEntry = (from entry in inputEntryGroup
								  orderby entry.validTo descending
								  select entry).First();
				}

				var oldestInputEntry = (from entry in inputEntryGroup
										orderby entry.validFrom ascending
										select entry).First();

				yield return ConvertCondition(inputEntry);
			}
		}

		protected abstract RefCusConditionType ConvertCondition(domainsDomainEntry inputEntry);
	}
}
