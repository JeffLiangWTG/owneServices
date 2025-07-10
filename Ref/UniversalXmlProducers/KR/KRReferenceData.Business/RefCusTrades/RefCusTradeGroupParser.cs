using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class RefCusTradeGroupParser : BaseEntityConfigurationParser
	{
		public RefCusTradeGroupParser(string configFilePath, string dataFilePath)
			: base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var refCusTradeGroup = new ExcelEntityUpdater<RefCusTradeGroup>(configuration, null, new RefCusTradeGroupLookupManager()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.TradeGroup, outputFilePath, writerConfiguration, publicationDate, refCusTradeGroup);
		}
	}
}
