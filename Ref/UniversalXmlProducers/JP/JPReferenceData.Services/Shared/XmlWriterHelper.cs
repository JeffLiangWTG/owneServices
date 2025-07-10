using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class XmlWriterHelper
	{
		readonly string filePath;
		readonly XmlWriter xmlWriter;

		public XmlWriterHelper(XmlWriterConfiguration config, string dataSource, DateTime publicationDate, UpdateType updateType, string fileNameWithoutExtension, IEnumerable<Dependency> dependencies = null)
		{
			xmlWriter = new XmlWriter(config);
			xmlWriter.SetDataSource(dataSource);
			xmlWriter.SetPublicationTime(publicationDate);
			xmlWriter.SetUpdateType(updateType);

			if (dependencies != null)
			{
				foreach (var dependency in dependencies)
				{
					xmlWriter.SetDependency(dependency);
				}
			}

			filePath = Path.Combine(AppConfig.Shared.OutputDirectory, $"{fileNameWithoutExtension}.xml");
		}

		void Populate<T>(IEnumerable<T> records) => records.ToList().ForEach(record => xmlWriter.PopulateData(record));

		void Save() => xmlWriter.SaveXml(filePath, true);

		public void PopulateAndSave<T>(IEnumerable<T> records)
		{
			Populate(records);
			Save();
		}
	}
}
