using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class IMOParser
	{
		readonly string _zipFilePath;
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;

		public IMOParser(IFileDownloaderWrapper fileDownloaderWrapper, string zipFilePath)
		{
			_fileDownloaderWrapper = fileDownloaderWrapper;
			_zipFilePath = zipFilePath;
		}

		public void ParseZipAndSaveXml()
		{
			var extractedFiles = _fileDownloaderWrapper.ExtractLocalZipFile(_zipFilePath);
			if (extractedFiles.Any())
			{
				var specProvFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.SpecProv, StringComparison.OrdinalIgnoreCase));
				var stowSegFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.StowSeg, StringComparison.OrdinalIgnoreCase));
				var propertyFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.Property, StringComparison.OrdinalIgnoreCase));
				var qualifyingDescriptiveTextFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.QualifyingDescriptiveText, StringComparison.OrdinalIgnoreCase));
				var seaRecordsFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.List, StringComparison.OrdinalIgnoreCase));

				if (string.IsNullOrEmpty(specProvFile) || string.IsNullOrEmpty(stowSegFile) || string.IsNullOrEmpty(propertyFile) || string.IsNullOrEmpty(qualifyingDescriptiveTextFile) || string.IsNullOrEmpty(seaRecordsFile))
				{
					Console.Error.WriteLine("Incorrect file mapping within the zip file");
				}

				var specProvParser = new SpecProvParser();
				var specProvRecords = specProvParser.Parse(specProvFile);

				var stowSegParser = new StowSegParser();
				var stowSegRecords = stowSegParser.Parse(stowSegFile);

				var propertyRecordParser = new PropertyRecordParser();
				var propertyRecords = propertyRecordParser.Parse(propertyFile);

				var qualifyingDescriptiveTextRecordParser = new QualifyingDescriptiveTextRecordParser();
				var qualifyingDescriptiveTextRecords = qualifyingDescriptiveTextRecordParser.Parse(qualifyingDescriptiveTextFile);

				var seaRecordsParser = new IMORecordParser(stowSegRecords, specProvRecords, propertyRecords, qualifyingDescriptiveTextRecords);
				var seaRecords = seaRecordsParser.Parse(seaRecordsFile);

				XmlWriterHelper.ExportToXml(IMOXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), seaRecords, ConfigurationProvider.FilePathIMO);

				var commonDataParser = new CommonDataParser(stowSegRecords, specProvRecords);
				var commonDataRecords = commonDataParser.Parse();

				XmlWriterHelper.ExportToXml(CommonDataXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), commonDataRecords, ConfigurationProvider.FilePathCommonData);
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

		public IEnumerable<UNDGSubstance> ParseZipAndExtractUNDGSbustances()
		{
			var extractedFiles = _fileDownloaderWrapper.ExtractLocalZipFile(_zipFilePath);
			if (extractedFiles.Any())
			{
				var iMORecordsFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.List, StringComparison.OrdinalIgnoreCase));

				if (!string.IsNullOrEmpty(iMORecordsFile))
				{
					var iMORecordsParser = new IMORecordParser(null, null, null, null);
					var results = iMORecordsParser.Parse(iMORecordsFile);
					foreach (var file in extractedFiles)
					{
						File.Delete(file);
					}
					return results;
				}
			}
			return null;
		}
	}
}
