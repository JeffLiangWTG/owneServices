using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class RefCusPreferenceParser : BaseEntityConfigurationParser
	{
		public RefCusPreferenceParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var refCusPreferences = new ExcelEntityUpdater<RefCusPreference>(configuration, null).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.RefCusPreference, outputFilePath, writerConfiguration, publicationDate, refCusPreferences);
		}
	}
}
