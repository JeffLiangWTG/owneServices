using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public interface IXmlProducer<T> where T : class
	{
		void ExportToXml(IEnumerable<T> collection, string filePath = null);
		void ExportToXmlInBatch(IEnumerable<T> collection, DateTime publishTime, string filePath = null);
		void InitializeWriter(DateTime publishTime, string dataSource = null);
		void ReportDataSourceError(string message);
	}
}
