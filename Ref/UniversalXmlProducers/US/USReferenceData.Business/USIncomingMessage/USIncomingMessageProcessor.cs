using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public abstract class USIncomingMessageProcessor
	{
		readonly string outputPath;

		protected USIncomingMessageProcessor(string outputPath)
		{
			this.outputPath = outputPath;
		}

		public abstract string MetaDataPattern { get; }

		public abstract string OutputFileName { get; }

		public abstract string XMLWriterDataSource { get; }

		public abstract UpdateType UpdateType { get; }

		public void Process(string messageText)
		{
			var matchMetaDatas = Regex.Matches(messageText, MetaDataPattern);
			if (matchMetaDatas.Count > 0)
			{
				ProcessCore(matchMetaDatas);
			}
		}

		protected abstract void ProcessCore(MatchCollection matchMetaDatas);

		protected void SaveXML<T>(List<T> publishDataCollection, DateTime publicationTime, Func<XmlWriterConfiguration> xmlWriterConfigurationSupplier)
		{
			if (publishDataCollection.Any())
			{
				var outputFilePath = Path.Combine(outputPath, publicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "_" + OutputFileName);
				XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, outputFilePath, xmlWriterConfigurationSupplier(), publicationTime, publishDataCollection, UpdateType);
			}
		}
	}
}
