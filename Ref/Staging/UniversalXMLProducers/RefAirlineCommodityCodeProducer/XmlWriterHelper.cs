using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer
{
	public static class XmlWriterHelper
	{
		public static void ExportToXml<T>(IXmlWriter xmlWriter, IEnumerable<T> collection, string filePath, bool validate = true) where T : RefDataRepoModelEntityType
		{
			foreach (var data in collection)
			{
				xmlWriter.PopulateData(data);
			}
			xmlWriter.SaveXml(filePath, validate);
		}
	}
}
