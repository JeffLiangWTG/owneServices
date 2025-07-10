using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public class ItalyExchangeRateDataExporter : IExchangeRateDataExporter
	{
		const string FileExtension = ".xml";

		readonly ExchangeRateMetaData _metaData;
		readonly ExchangeRateData _data;

		public ItalyExchangeRateDataExporter(ExchangeRateMetaData metaData, ExchangeRateData data)
		{
			_metaData = metaData;
			_data = data;
		}

		public void Export()
		{
			var xmlWriter = new XmlWriter(XmlWriterHelper.GetXmlWriterConfiguration());
			xmlWriter.SetDataSource(_metaData.DataSourceName);
			xmlWriter.SetPublicationTime(_metaData.PublicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);

			foreach (var exchangeRate in _data.ProcessedData)
			{
				xmlWriter.PopulateData(exchangeRate);
			}

			xmlWriter.SaveXml(Path.Combine(_metaData.OutputPath, _metaData.OutputFilePrefix + _metaData.Version + FileExtension));
		}
	}
}
