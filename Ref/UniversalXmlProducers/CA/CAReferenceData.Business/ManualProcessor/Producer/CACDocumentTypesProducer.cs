using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.ManualProcessor
{
	public class CACDocumentTypesProducer : IProducer
	{
		public CACDocumentTypesProducer(StringBuilder errorBuilder)
		{
			_errorBuilder = errorBuilder;
		}

		readonly StringBuilder _errorBuilder;

		public string FunctionCode => "CACDocumentTypes";

		public string OutPutFilePath => outPutFilePath ?? (outPutFilePath = Path.Combine(ApplicationConfig.OutputPath, PublicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + FunctionCode + ".xml"));
		string outPutFilePath;

		public DateTime PublicationTime => DateTime.TryParse(ApplicationConfig.CACDocumentTypesPublicationTime, out publicationTime) ? publicationTime : DateTime.Today;
		DateTime publicationTime;

		public string WorkingFileOrDirectory => workingFolder ?? (workingFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.CACDocumentTypesFileName));
		string workingFolder;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start processing CA Document Types data");

			new CACDocumentTypesParser(OutPutFilePath, PublicationTime, WorkingFileOrDirectory, _errorBuilder).ParseCusCodeListIntoXML();

			Console.WriteLine("End processing CA CDocument Types data");
		}
	}
}
