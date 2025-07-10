using System;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public static class XMLCreator
	{
		public static void CreateXMLsFromCSVFile(string csvFilePath, string outputFolderPath, string outputCommodityCodeFileName, string outputProductCodeFileName, DateTime publicationDate)
		{
			var (productCodeEntities, commodityCodeEntities, airlineId) = new CommodityCodeProductCodePivotRecordParser().Parse(csvFilePath);

			var commodityCodeConfig = new CommodityCodeConfiguration(airlineId);
			var commodityCodeXmlWriter = commodityCodeConfig.GetXmlWriter(publicationDate);

			var productCodeConfig = new ProductCodeConfiguration(airlineId);
			var productCodeXmlWriter = productCodeConfig.GetXmlWriter(publicationDate);

			productCodeXmlWriter.SetDependency(new Dependency(commodityCodeConfig.DataSourceName, publicationDate, DependencyType.Required));

			XmlWriterHelper.ExportToXml(productCodeXmlWriter, productCodeEntities, Path.Combine(outputFolderPath, outputProductCodeFileName));
			XmlWriterHelper.ExportToXml(commodityCodeXmlWriter, commodityCodeEntities, Path.Combine(outputFolderPath, outputCommodityCodeFileName));
		}
	}
}
