using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class SpecialUseCodeDutyRateParser : BaseEntityConfigurationParser
	{
		public SpecialUseCodeDutyRateParser(string configFilePath, string dataFilePath) : base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var commonOutputFileName = Path.GetFileNameWithoutExtension(outputFilePath);
			var outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, AdditionalDataUpdater, new DutyRateTopEntityLookupManager()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.SpecialUseCodeDutyRates, outputFilePath, writerConfiguration, publicationDate, tariffs);
		}

		protected virtual SpecialUseCodeDutyRateAdditionalDataUpdater AdditionalDataUpdater => new SpecialUseCodeDutyRateAdditionalDataUpdater();
	}
}
