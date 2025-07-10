using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class SimpleDrawbackParser : BaseEntityConfigurationParser
	{
		public SimpleDrawbackParser(string configFilePath, string dataFilePath) : base(configFilePath, dataFilePath)
		{
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, new SimpleDrawbackAdditionalDataUpdater(publicationDate)).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			Helper.ExportToXMLFile(Constants.DataSources.SimpleDrawback, outputFilePath, writerConfiguration, publicationDate, tariffs);
		}
	}
}
