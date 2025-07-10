using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CFRParser
	{
		readonly string _zipFilePath;
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;

		public CFRParser(IFileDownloaderWrapper fileDownloaderWrapper, string zipFilePath)
		{
			_fileDownloaderWrapper = fileDownloaderWrapper;
			_zipFilePath = zipFilePath;
		}

		public void ParseZipAndSaveXml()
		{
			var extractedFiles = _fileDownloaderWrapper.ExtractLocalZipFile(_zipFilePath);
			if (extractedFiles.Any())
			{

				var cfrRecordsFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.CFRFiles.List, StringComparison.OrdinalIgnoreCase));
				var qualifyingDescriptiveTextFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.CFRFiles.QualifyingDescriptiveText, StringComparison.OrdinalIgnoreCase));

				if (string.IsNullOrEmpty(cfrRecordsFile) || string.IsNullOrEmpty(qualifyingDescriptiveTextFile))
				{
					Console.Error.WriteLine("Incorrect file mapping within the zip file");
				}	

				var qualifyingDescriptiveTextRecordParser = new CFRQualifyingDescriptiveTextRecordParser();
				var qualifyingDescriptiveTextRecords = qualifyingDescriptiveTextRecordParser.Parse(qualifyingDescriptiveTextFile);

				var cfrRecordParser = new CFRRecordParser(qualifyingDescriptiveTextRecords);
				var cfrRecords = cfrRecordParser.Parse(cfrRecordsFile);

				XmlWriterHelper.ExportToXml(CFRXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), cfrRecords, ConfigurationProvider.FilePathCFR);

			}
			else
			{
				Console.Error.WriteLine("No files were found on the zip file");
			}

			foreach (var file in extractedFiles)
			{
				File.Delete(file);
			}
		}
	}
}
