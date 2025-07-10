using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class TariffParser : BaseEntityConfigurationParser
	{
		public TariffParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var commonOutputFileName = Path.GetFileNameWithoutExtension(outputFilePath);
			var outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, AdditionalDataUpdater).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			for (int i = 0; i < 10; i++)
			{
				var outputFile = $"{outputDirectoryPath}\\{commonOutputFileName}_{publicationDate.Year}_{i}.xml";
				var entities = tariffs.Where(item => item.ZZ1_TariffCode.StartsWith(i.ToString(CultureInfo.CurrentCulture), StringComparison.Ordinal))?.ToArray();
				Helper.ExportToXMLFile($"{Constants.DataSources.Tariffs}_{i}", outputFile, writerConfiguration, publicationDate, entities);
			}
		}
		protected virtual TariffAdditionalDataUpdater AdditionalDataUpdater => new TariffAdditionalDataUpdater();
	}
}
