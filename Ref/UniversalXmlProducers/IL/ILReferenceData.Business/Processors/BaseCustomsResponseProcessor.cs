using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public abstract class BaseCustomsResponseProcessor
	{
		protected BaseCustomsResponseProcessor(ILogger logger)
		{
			this.logger = logger;
		}

		public void GenerateFiles(DateTime publicationDate, string tableName, string dataSet)
		{
			var dataSetDecoded = WebUtility.HtmlDecode(dataSet);
			logger.Log(LogType.Information, $"Processing {tableName} from IL Customs...");
			StoreDataSet(publicationDate, tableName, dataSetDecoded);
		}

		protected virtual void StoreDataSet(DateTime publicationDate, string tableName, string dataSet)
		{
			using StringReader sr = new StringReader(dataSet);

			XDocument xdoc = XDocument.Load(sr);

			using XmlWriter writer = XmlWriter.Create(GetSafeOutputFilePath(GetDataSourceName(tableName)));

			xdoc.WriteTo(writer);
		}

		protected virtual string OutputFolderPath => ApplicationConfig.Instance.OutputDirectory;

		protected string GetSafeOutputFilePath(string dataSourceName)
		{
			if (!Directory.Exists(OutputFolderPath))
			{
				Directory.CreateDirectory(OutputFolderPath);
			}

			return Path.Combine(OutputFolderPath, $"{GetOutputFileName(dataSourceName)}.xml");
		}

		protected virtual string GetDataSourceName(string tableName) => tableName;

		protected virtual string GetOutputFileName(string dataSourceName) => $"IL_{dataSourceName}";

		readonly ILogger logger;
	}
}
