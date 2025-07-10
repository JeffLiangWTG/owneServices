using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class InstalmentCodesParser : BaseEntityConfigurationParser
	{
		public InstalmentCodesParser(string configFilePath, string dataFilePath) : base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var refCodeList = new ExcelEntityUpdater<RefCusCodeList>(configuration, new InstalmentCodesAdditionalDataUpdater()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.InstalmentCodes, outputFilePath, writerConfiguration, publicationDate, refCodeList);
		}
	}
}
