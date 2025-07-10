using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class PreferenceRefCusMapParser : BaseEntityConfigurationParser
	{
		public PreferenceRefCusMapParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var refCusMap = new ExcelEntityUpdater<RefCusMap>(configuration, new PreferenceRefCusMapDataUpdater()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.PreferenceRefCusMap, outputFilePath, writerConfiguration, publicationDate, refCusMap);
		}
	}
}
