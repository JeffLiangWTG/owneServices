using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public abstract class ConfigurationBase
	{
		protected ConfigurationBase(string airlineId)
		{
			AirlineId = airlineId;
		}

		protected abstract string DataSource { get; }
		string AirlineId { get; set; }
		public string DataSourceName => $"{DataSource} {AirlineId}";

		public IXmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var configuration = GetWriterConfiguration();
			var xmlWriter = new XmlWriter(configuration);
			xmlWriter.SetDataSource(DataSourceName);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		protected abstract IXmlWriterConfiguration GetWriterConfiguration();
	}
}
