using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.Schema.PermitItemDetails;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists
{
	public class PermitItemDetailsParser : BaseCodeListsParser<permitItemDetails>
	{
		public PermitItemDetailsParser(DownloadResult download) : base(download)
		{
		}

		public override IEnumerable<MappingConfig> DefaultListTypeMappings => new List<MappingConfig>
		{
			new MappingConfig { DomainNames = new[] { "WarenartBVET" }, ListType = "CITCT", WithLanguagesList = true },
			new MappingConfig { DomainNames = new[] { "TierartBVET" }, ListType = "CITSN", WithLanguagesList = true }
		};

		protected override string LogFileSuffix => "PermitItemDetails";

		protected override IEnumerable<RefCusCodeList> ConvertDomain(permitItemDetails inputDoc, DateTime actualDate, MappingConfig mapping)
		{
			var outputCodeList = new List<RefCusCodeList>();
			foreach (var inputEntry in from detail in inputDoc.permitItemDetail
									   where mapping.DomainNames.Contains(detail.name)
									   from entry in detail.entry
									   where entry.validFrom <= actualDate && entry.validTo >= actualDate
									   select entry)
			{
				yield return new RefCusCodeList
				{
					ZZD_Code = inputEntry.value,
					ZZD_Description = inputEntry.meaningDe.Truncate(2000),
					ZZD_StartDate = inputEntry.validFrom.Truncate(),
					ZZD_EndDate = inputEntry.validTo.Truncate().EndOfDay(),
					RefCusCodeListLanguages = ConvertLanguages(inputEntry)
				};
			}
		}
	}
}
