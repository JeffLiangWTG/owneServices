using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	public abstract class BaseConditionTypesParser<TInput>
	{
		DownloadResult Download { get; }

		protected abstract string DataSourceName { get; }

		protected abstract string OutPutFileName { get; }

		protected abstract string LogFileSuffix { get; }

		string LogFilePath { get; }

		protected BaseConditionTypesParser(DownloadResult download)
		{
			Download = download;
			LogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + $"-{LogFileSuffix}.log");
		}

		public void ConvertToRefXML(string outputFilePath, DateTime actualDate)
		{
			var inputDoc = Helper.DeserializeXML<TInput>(Download.Content);

			var created = CreationTime(inputDoc);
			var logFileText = created.ToString("r");
			if (!File.Exists(LogFilePath) || !logFileText.Equals(File.ReadAllText(LogFilePath), StringComparison.Ordinal))
			{
				var writerConfiguration = GetWriterConfiguration();
				var outputConditionTypeList = ConvertConditions(inputDoc, actualDate);
				Helper.ExportToXMLFile(DataSourceName, Path.Combine(outputFilePath, OutPutFileName), writerConfiguration, created, outputConditionTypeList, UpdateType.Partial);
				File.WriteAllText(LogFilePath, logFileText);
			}
		}

		protected abstract DateTime CreationTime(TInput input);

		protected abstract IEnumerable<RefCusConditionType> ConvertConditions(TInput input, DateTime actualDate);

		protected static RefCusConditionTypeLanguage[] ConvertConditionTypeLanguages(IInputEntry inputEntry)
		{
			var outputGroupLanguages = new List<RefCusConditionTypeLanguage>();
			AddConditionTypeLanguage(outputGroupLanguages, "FR", inputEntry.MeaningFr);
			AddConditionTypeLanguage(outputGroupLanguages, "IT", inputEntry.MeaningIt);
			AddConditionTypeLanguage(outputGroupLanguages, "EN", inputEntry.MeaningEn);
			return outputGroupLanguages.ToArray();
		}

		static void AddConditionTypeLanguage(List<RefCusConditionTypeLanguage> languages, string languageCode, string description)
		{
			if (!Helper.IsUselessDescription(description))
			{
				languages.Add(new RefCusConditionTypeLanguage
				{
					ZXW_ZX6_NKLanguage = languageCode,
					ZXW_Description = description.Truncate(500)
				});
			}
		}

		static protected XmlWriterConfiguration GetWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var conditionTypeConfiguration = new EntityTypeConfiguration<RefCusConditionType>(true);
			conditionTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZX2_ConditionClass, true, "CTRL");
			conditionTypeConfiguration.IncludeColumn(x => x.ZX2_ConditionType, true);
			conditionTypeConfiguration.IncludeColumn(x => x.ZX2_Description, false);
			conditionTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZX2_ZZZ_NKDataGrouping, true, "CH");
			conditionTypeConfiguration.IncludeColumn(x => x.RefCusConditionTypeLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionTypeConfiguration);

			var conditionTypeLanguageConfiguration = new EntityTypeConfiguration<RefCusConditionTypeLanguage>(true);
			conditionTypeLanguageConfiguration.IncludeColumn(x => x.ZXW_Description, false);
			conditionTypeLanguageConfiguration.IncludeColumn(x => x.ZXW_ZX6_NKLanguage, true);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionTypeLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
