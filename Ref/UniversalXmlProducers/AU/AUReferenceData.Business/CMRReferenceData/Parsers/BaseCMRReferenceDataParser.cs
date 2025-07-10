using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public interface ICMRDataParser
	{
		string FileNamePrefix { get; }

		string IndexUri { get; }

		void Parse(string content, DateTime publishedDate, string outputDirectory);
	}

	public abstract class BaseCMRReferenceDataParser : CommonCMRDataParser, ICMRDataParser
	{
		public void Parse(string content, DateTime publishedDate, string outputDirectory)
		{
			OutputDirectory = outputDirectory;
			PublishedDate = publishedDate;

			var writer = CreateWriter();
			ParseCore(writer, content);
			if (SaveWriter)
			{
				writer?.SaveXml(Path.Combine(OutputDirectory, OutputXMLName));
			}
			Console.WriteLine($"End of parsing {DataSource}");
		}

		protected virtual XmlWriter CreateWriter()
		{
			var writer = new XmlWriter(BuildXMLConfiguration(PublishedDate));
			writer.SetDataSource(DataSource);
			writer.SetUpdateType(UpdateType);
			writer.SetPublicationTime(UpdateType == UpdateType.Full ? PublishedDate : PublishedDate.Date);
			return writer;
		}

		protected abstract void ParseCore(IXmlWriter xmlWriter, string content);

		protected virtual bool SaveWriter => true;
		protected string OutputDirectory { get; private set; }
		protected DateTime PublishedDate { get; private set; }

		protected virtual string IndexUri => ApplicationConfig.AUReferenceFilesDirectory;
		protected virtual UpdateType UpdateType => UpdateType.Full;

		string ICMRDataParser.FileNamePrefix => FileNamePrefix;
		string ICMRDataParser.IndexUri => IndexUri;
	}
}
