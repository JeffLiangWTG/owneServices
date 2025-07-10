using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class OtherGovernmentParser : BaseEntityConfigurationParser
	{
		public OtherGovernmentParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var refCusCodeList = new ExcelEntityUpdater<RefCusCodeList>(configuration, null).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.OtherGovernment, outputFilePath, writerConfiguration, publicationDate, refCusCodeList);
		}
	}
}
