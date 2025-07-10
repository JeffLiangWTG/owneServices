using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business.DutyReductionExemption;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class DutyReductionExemptionParser : BaseEntityConfigurationParser
	{
		public DutyReductionExemptionParser(string configFile, string dataFile)
			: base(configFile, dataFile)
		{
		}
		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var tariff = new ExcelEntityUpdater<RefCusTariff>(configuration, new DutyReductionExemptionAdditionalDataUpdater()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.DutyReductionExemption, outputFilePath, writerConfiguration, publicationDate, tariff);
		}
	}
}
