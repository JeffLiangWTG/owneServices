using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	class AdditionalCodeList
	{
		internal const string CodeType = "ADDCD";

		readonly Dictionary<string, RefCusCodeList> list = new Dictionary<string, RefCusCodeList>();

		readonly Dictionary<string, string> hashCodes = new Dictionary<string, string>();

		internal string AddOrGetDescriptionHashCode(string descriptionDE, string descriptionEN, string descriptionFR, string descriptionIT, DateTime validFrom, DateTime validTo)
		{
			if (!list.TryGetValue(descriptionDE, out var refCusCodeList))
			{
				var languageList = new List<RefCusCodeListLanguage>();

				if (!string.IsNullOrEmpty(descriptionEN))
				{
					languageList.Add(new RefCusCodeListLanguage() { ZXA_ZX6_NKLanguage = "EN", ZXA_Description = descriptionEN });
				}

				if (!string.IsNullOrEmpty(descriptionFR))
				{
					languageList.Add(new RefCusCodeListLanguage() { ZXA_ZX6_NKLanguage = "FR", ZXA_Description = descriptionFR });
				}

				if (!string.IsNullOrEmpty(descriptionIT))
				{
					languageList.Add(new RefCusCodeListLanguage() { ZXA_ZX6_NKLanguage = "IT", ZXA_Description = descriptionIT });
				}

				refCusCodeList = new RefCusCodeList()
				{
					ZZD_Code = GetDescriptionHashCode(descriptionDE),
					ZZD_Description = descriptionDE,
					ZZD_StartDate = validFrom.Truncate(),
					ZZD_EndDate = validTo.Truncate().EndOfDay(),
					RefCusCodeListLanguages = languageList.ToArray(),
				};

				list.Add(descriptionDE, refCusCodeList);
			}
			else
			{
				refCusCodeList.ZZD_StartDate = Helper.Min(refCusCodeList.ZZD_StartDate, validFrom.Truncate());
				refCusCodeList.ZZD_EndDate = Helper.Max(refCusCodeList.ZZD_EndDate, validTo.Truncate().EndOfDay());
			}

			return refCusCodeList.ZZD_Code;
		}

		internal RefCusCodeList GetCusCodeListByDescription(string descriptionDE)
		{
			return list.TryGetValue(descriptionDE, out var refCusCodeList) ? refCusCodeList : null;
		}

		internal RefCusCodeList GetCusCodeListByHashCode(string hashCode)
		{
			return list.Values.Where(x => x.ZZD_Code == hashCode).SingleOrDefault();
		}

		internal string GetDescriptionHashCode(string description)
		{
			string hashCode;

			using (var hasher = CreateHashAlgorithm())
			{
				var descriptionHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(description));
				hashCode = Convert.ToBase64String(descriptionHash).Substring(0, 15).ToUpperInvariant();
			}

			if (hashCodes.TryGetValue(hashCode, out string knownDescription))
			{
				if (!description.Equals(knownDescription, StringComparison.Ordinal))
				{
					throw new InvalidOperationException($@"Ambiguous hashcode: {hashCode} ""{knownDescription}"" ""{description}""");
				}
			}
			else
			{
				hashCodes.Add(hashCode, description);
			}

			return hashCode;
		}

#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
		internal virtual HashAlgorithm CreateHashAlgorithm() => MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms

		internal int Count() => list.Count;

		internal void WriteXml(string outputFile, DateTime published)
		{
			var writerConfiguration = GetRefCusCodeListConfiguration();
			var dataSource = $"CH {CodeType} Code List";
			Helper.ExportToXMLFile(dataSource, outputFile, writerConfiguration, published, list.Values.OrderBy(x => x.ZZD_Description));
		}

		static XmlWriterConfiguration GetRefCusCodeListConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, CodeType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CH");
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_EndDate, false);
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			var codeListLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
