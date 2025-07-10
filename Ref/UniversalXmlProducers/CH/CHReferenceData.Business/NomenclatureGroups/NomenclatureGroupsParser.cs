using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.NomenclatureGroups
{
	public class NomenclatureGroupsParser
	{
		readonly DownloadResult download;

		public NomenclatureGroupsParser(DownloadResult download)
		{
			this.download = download;
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public void ConvertToRefXML(string outputFile, DateTime published)
		{
			var writerConfiguration = GetRefExchangeRateWriterConfiguration();

			var groups = new List<RefCusNomenclatureGroup>();

			TariffStructureLoader.Load(download, AddGroup, true);
			void AddGroup(string numm, Description description)
			{
				groups.Add(new RefCusNomenclatureGroup
				{
					ZZ5_Value = numm.Replace(".", ""),
					ZZ5_Description = description.TextD,
					ZZ5_CompositeKey = description.SortKey,
					RefCusNomenclatureLanguages = new RefCusNomenclatureLanguage[]
					{
							new RefCusNomenclatureLanguage
							{
								ZX8_ZX6_NKLanguage = "FR",
								ZX8_Description = description.TextF,
							},
							new RefCusNomenclatureLanguage
							{
								ZX8_ZX6_NKLanguage = "IT",
								ZX8_Description = description.TextI,
							},
							new RefCusNomenclatureLanguage
							{
								ZX8_ZX6_NKLanguage = "EN",
								ZX8_Description = description.TextE,
							}
					}
				});
			}

			Helper.ExportToXMLFile(DataSource, outputFile, writerConfiguration, published, groups);
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var nomenclatureConfiguration = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);
			nomenclatureConfiguration.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, "CH");
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, "CH");
			nomenclatureConfiguration.IncludeColumn(x => x.ZZ5_Value, false);
			nomenclatureConfiguration.IncludeColumn(x => x.ZZ5_Description, false);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_StartDate, false, Helper.MinimumDateTime);
			nomenclatureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ5_EndDate, false, Helper.EndOfDay(Helper.MaximumDateTime));
			nomenclatureConfiguration.IncludeColumn(x => x.RefCusNomenclatureLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(nomenclatureConfiguration);

			var languagesConfiguration = new EntityTypeConfiguration<RefCusNomenclatureLanguage>(true);
			languagesConfiguration.IncludeColumn(x => x.ZX8_ZX6_NKLanguage, true);
			languagesConfiguration.IncludeColumn(x => x.ZX8_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(languagesConfiguration);

			return writerConfiguration;
		}

		const string DataSource = "CH Nomenclature";
	}
}
