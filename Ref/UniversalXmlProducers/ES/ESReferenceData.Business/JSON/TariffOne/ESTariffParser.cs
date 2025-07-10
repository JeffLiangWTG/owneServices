using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;

namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class ESTariffParser(IDateTimeProvider dateTimeProvider) : CommonParser(dateTimeProvider)
{
	public (string errors, string logs) ConvertToXMLFile(string nomenclaturesJson, string sectionsJson, string outPutFileWithPathTariffs, string outPutFileWithPathNomenclatures)
	{
		var nomenclaturesList = JsonHelper.GetListItemsFromJsonl<NomenclaturesSchema>(nomenclaturesJson, ErrorBuilder, handleDates: false, x => string.Equals(x.scope, Constants.CountryCode, StringComparison.Ordinal));

		if (nomenclaturesList.Count > 0)
		{
			var orderedNomenclatures = nomenclaturesList.OrderBy(x => x.goods_id).ToList();
			var result = GetNomenclatureRecords(orderedNomenclatures);
			var nomenclatureRecords = result.Item1;
			var includedGroups = result.Item2;
			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(new ChapterToSectionMapper(sectionsJson, ErrorBuilder));
			var rootNode = compositeKeyTreeGenerator.GenerateTree(nomenclatureRecords);

			var compositeKeyGenerator = new CompositeKeyGenerator();
			var keys = compositeKeyGenerator.GenerateCompositeKeys(rootNode);
			var tariffs = keys.Tariffs;
			
			var exportTariffs = new List<RefCusTariff>();

			foreach (var tariff in tariffs)
			{
				exportTariffs.Add(new RefCusTariff()
				{
					ZZ1_TariffCode = EUNUtils.NormalizeExportTariffCode(tariff.ZZ1_TariffCode),
					ZZ1_ZZI_NKTariffType = Constants.TariffType.Export,
					ZZ1_CompositeKeyOnZZ5 = tariff.ZZ1_CompositeKeyOnZZ5,
					ZZ1_Description = tariff.ZZ1_Description,
					ZZ1_StartDate = tariff.ZZ1_StartDate,
					ZZ1_EndDate = tariff.ZZ1_EndDate,
				});
			}

			var importTariffs = tariffs.ToArray();

			Array.ForEach(importTariffs, x =>
			{
				x.ZZ1_TariffCode = EUNUtils.NormalizeTariffCode(x.ZZ1_TariffCode);
				x.ZZ1_ZZI_NKTariffType = Constants.TariffType.Import;
			});

			var records = exportTariffs.Concat(importTariffs);
			var groups = SanitizeNomenclatureGroups(keys.NomenclatureGroups, includedGroups);

			Helper.ExportToXMLFile(Constants.DataSources.ES_Tariffs, outPutFileWithPathTariffs, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, records, UpdateType.Full);
			Helper.ExportToXMLFile(Constants.DataSources.ES_Nomenclatures, outPutFileWithPathNomenclatures, XMLWriterConfigurationNomenclatures, DateTimeProvider.CurrentLocalDate, groups, UpdateType.Full);
		}
		else
		{
			ErrorBuilder.AppendLine("Nomenclatures load failed. Details:");
			ErrorBuilder.AppendLine("Number of records: " + nomenclaturesList.Count);
			ErrorBuilder.AppendLine("Json Content: " + nomenclaturesJson);
		}

		return (ErrorBuilder.ToString(), LogBuilder.ToString());
	}

	static RefCusNomenclatureGroup[] SanitizeNomenclatureGroups(ICollection<RefCusNomenclatureGroup> groups, HashSet<string> includedGroups)
	{
		var sanitizedGroups = new List<RefCusNomenclatureGroup>();
		foreach (var group in groups)
		{
			var chapter = group.ZZ5_Value.Substring(0, 2);
			if (!string.Equals(group.ZZ5_Description, DescriptionForParent + chapter, StringComparison.Ordinal) &&
				includedGroups.Contains(chapter))
			{
				sanitizedGroups.Add(group);
			}
		}
		return sanitizedGroups.ToArray();
	}

	const string DescriptionForParent = "Parent ";

	(NomenclatureRecord[], HashSet<string>) GetNomenclatureRecords(List<NomenclaturesSchema> nomenclaturesList)
	{
		var result = new List<NomenclatureRecord>();
		var parentNomenclatures = new HashSet<string>();
		var languages = new List<(string, string)>() { ("EN", "English description") };
		foreach (var nomenclature in nomenclaturesList)
		{
			var goodsId = nomenclature.goods_id;
			var chapter = !string.IsNullOrEmpty(goodsId) && goodsId.Length > 2 ? goodsId.Substring(0, 2) : null;
			var startDate = nomenclature.start_date ?? Constants.MinimumDateTime;
			var endDate = nomenclature.end_date ?? Constants.MaximumDateTime;
			if (!string.IsNullOrEmpty(chapter) && !parentNomenclatures.Contains(chapter))
			{
				parentNomenclatures.Add(chapter);
				result.Add(new NomenclatureRecord(chapter + "00000000 80", startDate, endDate, 2, 0, DescriptionForParent + chapter, languages, isTariff: false));
			}
			var tariffHeader = goodsId + " " +nomenclature.suffix;
			var description = GetDescription(nomenclature.descriptions);

			if (string.IsNullOrEmpty(goodsId) ||
				string.IsNullOrEmpty(nomenclature.suffix) ||
				string.IsNullOrEmpty(description) ||
				!int.TryParse(nomenclature.hierarchy_position, out var hierarchy) ||
				!int.TryParse(nomenclature.indent, out var level))
			{
				ErrorBuilder.AppendLine("Tariff processing failed. Details:");
				ErrorBuilder.AppendLine("startDate: " + startDate.ToString("d/M/yyyy H:mm:ss", CultureInfo.InvariantCulture));
				ErrorBuilder.AppendLine("endDate: " + endDate.ToString("d/M/yyyy H:mm:ss", CultureInfo.InvariantCulture));
				ErrorBuilder.AppendLine("goods_id: " + goodsId);
				ErrorBuilder.AppendLine("suffix: " + nomenclature.suffix);
				ErrorBuilder.AppendLine("hierarchy: " + nomenclature.hierarchy_position);
				ErrorBuilder.AppendLine("level: " + nomenclature.indent);
				ErrorBuilder.AppendLine("description: " + description);
				continue;
			}
			
			result.Add(new NomenclatureRecord(tariffHeader, startDate, endDate, hierarchy, level, description, languages, true));
		}
		return (result.ToArray(), parentNomenclatures);
	}

	static string GetDescription(IEnumerable<NomenclaturesSchema.Description> descriptions)
		=> descriptions.FirstOrDefault(EnSearch)?.text ?? descriptions.FirstOrDefault(EsSearch)?.text ?? string.Empty;

	static Func<NomenclaturesSchema.Description, bool> EnSearch => enSearch ?? (enSearch = new Func<NomenclaturesSchema.Description, bool>(x => string.Equals(x.lang, "en", StringComparison.Ordinal)));
	static Func<NomenclaturesSchema.Description, bool> enSearch;
	static Func<NomenclaturesSchema.Description, bool> EsSearch => esSearch ?? (esSearch = new Func<NomenclaturesSchema.Description, bool>(x => string.Equals(x.lang, "es", StringComparison.Ordinal)));
	static Func<NomenclaturesSchema.Description, bool> esSearch;

	protected override XmlWriterConfiguration XMLWriterConfiguration
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeListConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.EuropeanUnion);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_EndDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			return writerConfiguration;
		}
	}

	static XmlWriterConfiguration XMLWriterConfigurationNomenclatures
	{
		get
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codeListConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, "CN");
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.CountryCode);
			codeListConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			codeListConfiguration.IncludeColumn(x => x.ZZ5_Value, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ5_Description, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ5_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZ5_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			return writerConfiguration;
		}
	}
}
