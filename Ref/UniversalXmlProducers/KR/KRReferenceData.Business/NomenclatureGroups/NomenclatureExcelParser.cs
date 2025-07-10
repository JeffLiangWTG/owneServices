using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class NomenclatureExcelParser : BaseEntityConfigurationParser
	{
		public NomenclatureExcelParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var nomenclatures = new ExcelEntityUpdater<RefCusNomenclatureGroup>(configuration, AdditionalDataUpdater).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.KRNomenclature, outputFilePath, writerConfiguration, publicationDate, nomenclatures);
		}
		protected virtual NomenclatureAdditionalDataUpdater AdditionalDataUpdater => new NomenclatureAdditionalDataUpdater();
	}
}
