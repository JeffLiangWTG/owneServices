using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.DomainSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists
{
	public class CodeListsParser : BaseCodeListsParser<domains>
	{
		public CodeListsParser(DownloadResult download) : base(download)
		{
		}

		const string EComplaintHeaderFields = "eComplaintHeaderFields";
		const string EComplaintPositionFields = "eComplaintPositionFields";

		public override IEnumerable<MappingConfig> DefaultListTypeMappings => new List<MappingConfig>
		{
			new MappingConfig() { DomainNames = new[] { "methodOfPayment" }, ListType = "MOP", WithLanguagesList = true },
			new MappingConfig() { DomainNames = new[] { "permitAuthority" }, ListType = "PRMAU", WithLanguagesList = true },
			new MappingConfig() { DomainNames = new[] { "previousDocumentType" }, OnlyForImport = true, ListType = "DC40I", WithLanguagesList = true },
			new MappingConfig() { DomainNames = new[] { "vehicleModelCode" }, OnlyForExport = true, ListType = "VEHMC", WithLanguagesList = false },
			new MappingConfig() { DomainNames = new[] { "documentType" }, OnlyForImport = true, ListType = "DC44I", WithLanguagesList = true },
			new MappingConfig() { DomainNames = new[] { "nonCustomsLawType" }, ListType = "NCLT", WithLanguagesList = true },
			new MappingConfig() { DomainNames = new[] { EComplaintHeaderFields, EComplaintPositionFields }, ListType = "ECFLD", WithLanguagesList = true, WithAttributeList = true , MixedCasedCodes = true },
			new MappingConfig() { DomainNames = new[] { "goodsItemDetailName" }, ListType = "GIDNM", OnlyForImport = true, WithLanguagesList = true, YearsToImport = 10 },
		};

		protected override string LogFileSuffix => "CodeLists";

		protected override IEnumerable<RefCusCodeList> ConvertDomain(domains inputDoc, DateTime actualDate, MappingConfig mapping)
		{
			foreach (var domain in from d in inputDoc.domain
								   where mapping.DomainNames.Contains(d.name)
								   select d)
			{
				var inputEntries = domain.entry;

				IEnumerable<domainsDomainEntry> filteredInputEntries = inputEntries;
				if (mapping.OnlyForImport)
				{
					filteredInputEntries = from e in inputEntries where e.validForImport select e;
				}
				if (mapping.OnlyForExport)
				{
					filteredInputEntries = from e in inputEntries where e.validForExport select e;
				}

				if (mapping.YearsToImport > 0)
				{
					var pastDate = actualDate.AddYears(-mapping.YearsToImport);
					filteredInputEntries = from entry in filteredInputEntries
										   where entry.validFrom <= actualDate && entry.validTo >= pastDate
										   select entry;
				}

				foreach (var inputEntryGroup in from entry in filteredInputEntries
												group entry by ConvertCodeCase(mapping, entry.value) into entryGroup
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

					yield return new RefCusCodeList
					{
						ZZD_Code = ConvertCodeCase(mapping, inputEntry.value),
						ZZD_Description = inputEntry.meaningDe,
						ZZD_StartDate = oldestInputEntry.validFrom.Truncate(),
						ZZD_EndDate = inputEntry.validTo.Truncate().EndOfDay(),
						RefCusCodeListLanguages = mapping.WithLanguagesList ? ConvertLanguages(inputEntry) : null,
						RefCusCodeListAttributes = mapping.WithAttributeList ? ConvertAttributes(domain) : null,
					};
				}
			}
		}

		static string ConvertCodeCase(MappingConfig mappingConfig, string code)
		{
			return mappingConfig.MixedCasedCodes ? code : code.ToUpperInvariant();
		}

		static RefCusCodeListAttribute[] ConvertAttributes(domainsDomain domain)
		{
			switch (domain.name)
			{
				case EComplaintHeaderFields:
					return new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.IsHeader, ZZE_Value = AttributeValues.Y}
					};
				case EComplaintPositionFields:
					return new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute() { ZZE_ZXE_NKName = AttributeNames.IsHeader, ZZE_Value = AttributeValues.N}
					};
			}
			return null;
		}

		class AttributeNames
		{
			internal const string IsHeader = "IsHeader";
		}

		class AttributeValues
		{
			internal const string N = "N";
			internal const string Y = "Y";
		}
	}
}
