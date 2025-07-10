using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business.DutyReductionExemption;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class OGARegulationCategoryParser : BaseEntityConfigurationParser
	{
		public OGARegulationCategoryParser(string configFilePath, string dataFilePath) : base(configFilePath, dataFilePath)
		{
		}

			protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var refCusCodeList = new ExcelEntityUpdater<RefCusCodeList>(configuration, new OGARegulationCategoryDataUpdater()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.OGARegulationCategory, outputFilePath, writerConfiguration, publicationDate, refCusCodeList);
		}
	}
}
