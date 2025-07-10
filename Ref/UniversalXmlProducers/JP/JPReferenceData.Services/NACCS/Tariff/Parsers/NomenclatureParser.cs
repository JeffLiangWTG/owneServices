using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class NomenclatureParser : Parser
	{
		public NomenclatureParser(DateTime publishDate, bool isImport) : base(publishDate, isImport)
		{
		}

		const string ImportOutputXMLFileName = "JPImportNomenclature.xml";
		const string ExportOutputXMLFileName = "JPExportNomenclature.xml";
		const string ImportDataSource = "Japan NACCS Import Nomenclature";
		const string ExportDataSource = "Japan NACCS Export Nomenclature";
		const string GroupTypeImport = "JP";
		const string GroupTypeExport = "JPE";

		protected override string DataSource { get => IsImport ? ImportDataSource : ExportDataSource; }
		protected override string OutputXMLFileName { get => IsImport ? ImportOutputXMLFileName : ExportOutputXMLFileName; }
		protected override string EntityName { get => "Nomenclature"; }

		protected override IEnumerable<RefDataRepoModelEntityType> CreateEntities(string filePath)
		{
			if (CsvReaderHelper.TryRead(filePath, out var records, "UTF-8"))
			{
				foreach (var row in records)
				{
					var columns = ParserHelper.SplitComma(row);

					if (columns[0].Trim().Equals(EntityName, StringComparison.OrdinalIgnoreCase))
					{
						yield return new RefCusNomenclatureGroup
						{
							ZZ5_Value = columns[1].Trim(),
							ZZ5_Description = columns[3].Trim().TrimStart('-').TrimStart('－'),
							ZZ5_CompositeKey = columns[2].Trim(),

							RefCusNomenclatureLanguages = new[]
							{
								new RefCusNomenclatureLanguage()
								{
									ZX8_Description = columns[4].Trim().TrimStart('-').TrimStart('－'),
								}
							}
						};
					}
				}
			}
		}

		protected override XmlWriterConfiguration GetXMLConfiguration()
		{
			var configuration = new XmlWriterConfiguration();

			var refCusNomenclatureGroupConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Value, true);
			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.ZZ5_Description);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, IsImport ? GroupTypeImport : GroupTypeExport);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
			refCusNomenclatureGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ5_StartDate, false, IsImport ? StaticResources.DefaultZZD_StartDate : new DateTime(2019, 09, 01, 00, 00, 00));
			refCusNomenclatureGroupConfiguration.IncludeColumnWithDefaultValue(x => x.ZZ5_EndDate, false, StaticResources.DefaultZZD_EndDate);
			configuration.IncludeEntityTypeConfiguration(refCusNomenclatureGroupConfiguration);

			refCusNomenclatureGroupConfiguration.IncludeColumn(x => x.RefCusNomenclatureLanguages, false);
			var languageConfig = new EntityTypeConfiguration<RefCusNomenclatureLanguage>(true);
			languageConfig.IncludeColumnWithConstantValue(x => x.ZX8_ZX6_NKLanguage, true, Constants.DataGrouping.JP);
			languageConfig.IncludeColumn(x => x.ZX8_Description);
			configuration.IncludeEntityTypeConfiguration(languageConfig);

			return configuration;
		}
	}
}
