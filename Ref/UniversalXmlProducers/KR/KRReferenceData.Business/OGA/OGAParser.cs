using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business.OGA;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class OGAParser : BaseEntityConfigurationParser
	{
		public OGAParser(string configFile, string dataFile)
			: base(configFile, dataFile)
		{
		}
		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, new OGAAdditionalDataUpdater(publicationDate), topEntityLookupManager: new OGATopEntityLookupManager()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.OGA + suffix, outputFilePath, writerConfiguration, publicationDate, tariffs);
		}
	}
}
