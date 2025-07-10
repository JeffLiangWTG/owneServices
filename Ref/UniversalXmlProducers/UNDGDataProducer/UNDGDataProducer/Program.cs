using System;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	class Program
	{
		static int Main(string[] args)
		{
			return (int)ProduceXml(args);
		}

		static ProducerStatus ProduceXml(string[] args)
		{
			if (args.Length > 0)
			{
				var firstArg = args[0];
				var fileDownloaderWrapper = new FileDownloaderWrapper();
				switch (firstArg)
				{
					case Constants.ProgramArgs.IATA:
						var resultAir = new IATARecordParser().Parse(ConfigurationProvider.RefIATACsvFilePath);
						XmlWriterHelper.ExportToXml(IATAXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), resultAir, ConfigurationProvider.FilePathIATA);
						return ProducerStatus.Success;
					case Constants.ProgramArgs.IMO:
						var seaParser = new IMOParser(fileDownloaderWrapper, ConfigurationProvider.RefIMOZipFilePath);
						seaParser.ParseZipAndSaveXml();
						return ProducerStatus.Success;
					case Constants.ProgramArgs.PSA:
						var psaParser = new PSAGroupParser(fileDownloaderWrapper, ConfigurationProvider.RefIMOZipFilePath);
						psaParser.ParseAndSaveXmlAsync(ConfigurationProvider.FilePathPSA).GetAwaiter().GetResult();
						return ProducerStatus.Success;
					case Constants.ProgramArgs.ADR:
						var resultADR = new ADRRecordParser().Parse(ConfigurationProvider.RefADRCsvFilePath);
						XmlWriterHelper.ExportToXml(ADRXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), resultADR, ConfigurationProvider.FilePathADR);
						return ProducerStatus.Success;
					case Constants.ProgramArgs.RID:
						var resultRID = new RIDRecordParser().Parse(ConfigurationProvider.RefRIDCsvFilePath);
						XmlWriterHelper.ExportToXml(RIDXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), resultRID, ConfigurationProvider.FilePathRID);
						return ProducerStatus.Success;
					case Constants.ProgramArgs.ADN:
						var resultADN = new ADNRecordParser().Parse(ConfigurationProvider.RefADNCsvFilePath);
						XmlWriterHelper.ExportToXml(ADNXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), resultADN, ConfigurationProvider.FilePathADN, false); //TODO: set validation to true on WI00433229
						return ProducerStatus.Success;
					case Constants.ProgramArgs.COUNTRYREFERENCE:
						var resultCountryReference = new CountryReferenceParser().Parse(ConfigurationProvider.RefUNDGCountryReferenceCSVFilePath);
						var resultPivots = new SingaporePSAPivotParser(fileDownloaderWrapper, ConfigurationProvider.RefIMOZipFilePath).ParseAndAddPivotsToReferences(ConfigurationProvider.RefUNDGCountryReferencePivotCSVFilePath, resultCountryReference);
						XmlWriterHelper.ExportToXml(CountryReferenceXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), resultPivots, ConfigurationProvider.FilePathCountryReference);
						return ProducerStatus.Success;
					case Constants.ProgramArgs.COUNTRYREFERENCEPSA:
						var resultCountryReferenceNonPSA = new CountryReferenceParser().Parse(ConfigurationProvider.RefUNDGCountryReferenceNonPSACSVFilePath);
						var crpsaParser = new CountryReferencePSAParser(fileDownloaderWrapper, ConfigurationProvider.RefIMOZipFilePath, resultCountryReferenceNonPSA);
						crpsaParser.ParseAndSaveXmlAsync(ConfigurationProvider.FilePathCountryReferencePSA).GetAwaiter().GetResult();
						return ProducerStatus.Success;
					case Constants.ProgramArgs.CFR:
						var cfrParser = new CFRParser(fileDownloaderWrapper, ConfigurationProvider.RefCFRZipFilePath);
						cfrParser.ParseZipAndSaveXml();
						return ProducerStatus.Success;
					case Constants.ProgramArgs.JTT:
						var resultJTT = new JTTRecordParser().Parse(ConfigurationProvider.RefJTTCsvFilePath);
						XmlWriterHelper.ExportToXml(JTTXmlWriterConfiguration.GetXmlWriter(DateTime.UtcNow), resultJTT, ConfigurationProvider.FilePathJTT);
						return ProducerStatus.Success;
					default:
						Console.Error.WriteLine("Incorrect argument specified.");
						return ProducerStatus.Failure;
				}
			}
			else
			{
				Console.Error.WriteLine("No arguments specified");
				return ProducerStatus.Failure;
			}
		}
	}
}
