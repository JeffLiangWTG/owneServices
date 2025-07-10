using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists
{
	public abstract class BaseCodeListsParser<T>
		where T : IInputDoc
	{
		protected DownloadResult Download { get; }

		protected BaseCodeListsParser(DownloadResult download)
		{
			Download = download;
			LogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + $"-{LogFileSuffix}.log");
		}

		public abstract IEnumerable<MappingConfig> DefaultListTypeMappings { get; }

		public void ConvertToRefXML(string outputFileTemplate, DateTime actualDate, MappingConfig convertionConfigOverride = null)
		{
			var inputDoc = Helper.DeserializeXML<T>(Download.Content);

			var logFileText = inputDoc.Created.ToString("r");
			if (!File.Exists(LogFilePath) || !logFileText.Equals(File.ReadAllText(LogFilePath), StringComparison.Ordinal))
			{
				var mappings = convertionConfigOverride == null ? DefaultListTypeMappings : new[] { convertionConfigOverride }.AsEnumerable();

				foreach (var mapping in mappings)
				{
					var writerConfiguration = GetRefCusCodeListConfiguration(mapping.ListType, mapping.WithLanguagesList, mapping.WithAttributeList);
					var outputList = new List<RefCusCodeList>();
					outputList.AddRange(ConvertDomain(inputDoc, actualDate, mapping));
					if (mapping.AdditionalCodes != null)
					{
						outputList.AddRange(mapping.AdditionalCodes());
					}

					var dataSource = $"CH {mapping.ListType} Code List";
					var published = inputDoc.Created;

					var outputFile = outputFileTemplate.Insert(outputFileTemplate.LastIndexOf(".", StringComparison.InvariantCulture), mapping.ListType);
					Helper.ExportToXMLFile(dataSource, outputFile, writerConfiguration, published, outputList);
				}

				File.WriteAllText(LogFilePath, logFileText);
			}
		}

		protected abstract IEnumerable<RefCusCodeList> ConvertDomain(T inputDoc, DateTime actualDate, MappingConfig mapping);

		protected RefCusCodeListLanguage[] ConvertLanguages(IInputEntry inputEntry)
		{
			var outputGroupLanguages = new List<RefCusCodeListLanguage>();
			AddLanguage(outputGroupLanguages, "FR", inputEntry.MeaningFr);
			AddLanguage(outputGroupLanguages, "IT", inputEntry.MeaningIt);
			AddLanguage(outputGroupLanguages, "EN", inputEntry.MeaningEn);
			return outputGroupLanguages.ToArray();
		}

		static void AddLanguage(List<RefCusCodeListLanguage> languages, string languageCode, string description)
		{
			if (!Helper.IsUselessDescription(description))
			{
				languages.Add(new RefCusCodeListLanguage
				{
					ZXA_ZX6_NKLanguage = languageCode,
					ZXA_Description = description.Truncate(2000)
				});
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfiguration(string listType, bool withLanguagesList, bool withAttributes)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, listType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CH");
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_EndDate, false);
			if (withLanguagesList)
			{
				codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages);
			}
			if (withAttributes)
			{
				codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes);
			}
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			if (withLanguagesList)
			{
				var codeListLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
				codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
				codeListLanguageConfiguration.IncludeColumn(x => x.ZXA_Description, false);
				writerConfiguration.IncludeEntityTypeConfiguration(codeListLanguageConfiguration);
			}

			if (withAttributes)
			{
				var attributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				attributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				attributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
				writerConfiguration.IncludeEntityTypeConfiguration(attributeConfiguration);
			}

			return writerConfiguration;
		}

		protected abstract string LogFileSuffix { get; }

		public string LogFilePath { get; }
	}
}
