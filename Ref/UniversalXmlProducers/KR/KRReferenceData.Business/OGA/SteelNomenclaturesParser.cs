using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business.OGA;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class SteelNomenclaturesParser : BaseEntityConfigurationParser
	{
		public SteelNomenclaturesParser(string configFile, string dataFile)
			: base(configFile, dataFile)
		{
		}
		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var nomenclatureGroup = new ExcelEntityUpdater<RefCusNomenclatureGroup>(configuration, new SteelNomenclaturesAdditionalDataUpdater()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.SteelNomenclature, outputFilePath, writerConfiguration, publicationDate, nomenclatureGroup);
		}
	}
}
