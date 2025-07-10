using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class NonGAReasonParser : BaseEntityConfigurationParser
	{
		public NonGAReasonParser(string configFilePath, string dataFilePath, string dataSource)
			: base(configFilePath, dataFilePath)
		{
			this.DataSource = dataSource;
		}
		protected string DataSource { get; }

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var nonGAReasonTypes = new ExcelEntityUpdater<RefCusCodeList>(configuration, new NonGAReasonAdditionalDataUpdater()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(DataSource, outputFilePath, writerConfiguration, publicationDate, nonGAReasonTypes);
		}
	}
}
