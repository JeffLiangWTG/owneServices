using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class WCONomenclatureExcelParser : BaseEntityConfigurationParser
	{
		public WCONomenclatureExcelParser(string configFilePath, string dataFilePath, IEnumerable<RefCusNomenclatureGroup> wcoNomenclatures)
			: base(configFilePath, dataFilePath)
		{
			this.wcoNomenclatures = wcoNomenclatures;
		}
		IEnumerable<RefCusNomenclatureGroup> wcoNomenclatures;

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			new ExcelEntityUpdater<RefCusNomenclatureGroup>(configuration, new WCONomenclatureAdditionalDataUpdater(), new WCONomenclatureTopEntityLookupManager(wcoNomenclatures)).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.WCOCopiedNomenclature, outputFilePath, writerConfiguration, publicationDate, wcoNomenclatures);
		}
	}
}
