using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class DomesticTaxExemptionParser : BaseEntityConfigurationParser
	{
		public DomesticTaxExemptionParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, new DomesticTaxExemptionDataUpdater(), new DomesticTaxExemptionTopEntityLookupManager()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.DomesticTaxExemption, outputFilePath, writerConfiguration, publicationDate, tariffs);
		}
	}
}
