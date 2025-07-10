using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IXmlProducer<T> where T : class
	{
		void ExportToXml(IEnumerable<T> collection, string filePath = null);
		void ExportToXmlInBatch(IEnumerable<T> collection, DateTime publishTime);
		void InitializeWriter(DateTime publishTime, string dataSource = null, UpdateType updateType = UpdateType.Full);
		void ReportDataSourceError(string message);
		void ReportDataProcessingError(string processingErrors);
	}
}
