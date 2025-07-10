using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.TimeZoneParser
{
	public abstract class XmlProducer<T> : IXmlProducer<T> where T : class
	{
		public abstract string FilePath { get; }
		public abstract string DataSource { get; }

		protected IXmlWriter XmlWriter { get; set; }

		public void ExportToXml(IEnumerable<T> collection, string filePath = null)
		{
			foreach (var data in collection)
			{
				XmlWriter.PopulateData(data);
			}

			if (string.IsNullOrEmpty(filePath))
			{
				filePath = FilePath;
			}
			XmlWriter.SaveXml(filePath);
		}

		public virtual void ExportToXmlInBatch(IEnumerable<T> collection, DateTime publishTime, string filePath = null)
		{
		}

		public void InitializeWriter(DateTime publishTime, string dataSource = null)
		{
			if (string.IsNullOrEmpty(dataSource))
			{
				dataSource = DataSource;
			}

			XmlWriter.SetDataSource(dataSource);
			XmlWriter.SetPublicationTime(publishTime);
			XmlWriter.SetUpdateType(UpdateType.Full);
		}

		public void ReportDataSourceError(string message)
		{
			throw new NotSupportedException(message);
		}
	}
}
