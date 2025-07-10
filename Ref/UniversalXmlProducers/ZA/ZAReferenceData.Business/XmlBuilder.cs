using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business
{
	public abstract class XmlBuilder<TSource, TOutput>
		where TSource : ISourceData
		where TOutput : RefDataRepoModelEntityType
	{
		protected XmlBuilder(TSource sourceData, ILogger logger)
		{
			this.sourceData = sourceData;
			this.logger = logger;
			PublishDate = sourceData?.PublicationDate ?? DateTime.UtcNow;
		}

		protected TSource sourceData { get; private set; }
		protected ILogger logger { get; private set; }

		public void CreateXmlFile(string outputFolder, DateTime msgCreatedDate)
		{
			var models = ConvertToRefModels();

			if (models.Any())
			{
				CreatedDate = msgCreatedDate;

				var filename = Path.Combine(outputFolder, OutputFilename);

				ExportToXMLFile(DataSource, filename, GetXmlWriterConfiguration(), CreatedDate, UpdateType, models);

				logger.LogInfo($"Created {filename}");
			}
		}

		protected DateTime PublishDate { get; }
		protected DateTime CreatedDate { get; private set; }

		protected string OutputFilename => $"{FilePrefix}_{PublishDate:yyyyMMdd}_{CreatedDate:yyyyMMddHHmmssfff}.xml";
		protected string SubstringSafe(string input, int maxLen) => input.Length > maxLen ? input.Substring(0, maxLen) : input;

		void ExportToXMLFile(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, UpdateType updateType, IEnumerable<TOutput> refModels)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			foreach (var dependency in GetDependencies(publicationDateTime))
			{
				writer.SetDependency(dependency);
			}

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var model in refModels)
			{
				writer.PopulateData(model);
			}
			writer.SaveXml(outputFile);
		}

		protected virtual IEnumerable<Dependency> GetDependencies(DateTime publicationDateTime) => Enumerable.Empty<Dependency>();

		protected abstract string DataSource { get; }
		protected abstract string FilePrefix { get; }
		protected abstract UpdateType UpdateType { get; }
		protected abstract List<TOutput> ConvertToRefModels();
		protected abstract XmlWriterConfiguration GetXmlWriterConfiguration();
	}
}
