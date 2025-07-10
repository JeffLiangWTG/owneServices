using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.MXRefLocoMap.Properties;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml();
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml()
		{
			var outputFilePath = GetOutputFilePath();
			ServicesProvider.Logger.Info($"The out put file path is {outputFilePath}");

			Run(outputFilePath);

			ServicesProvider.Logger.Succeed($"Job completed! The file has been saved to {outputFilePath}");
		}

		static void Run(string outputFilePath)
		{
			ServicesProvider.Logger.Info("Start read excel file...");

			var excelFile = "\\CMR.xlsx";
			excelFile = excelFile.Replace("?", "");
			excelFile = System.Environment.CurrentDirectory + excelFile;

			ServicesProvider.Logger.Info("Start parsing the data in excel to RefLocoMap.");
			var reflocomap = ServicesProvider.ExcelParser.Parse<RefLocoMap>(excelFile, backupMapper: RefLocoMap.BackupMapper);
			ServicesProvider.Logger.Info("Successfully parsed the data to RefLocoMap.");

			ServicesProvider.Logger.Info("Start serializing the RefLocoMap to xml file.");
			var serializer = new XmlSerializer(typeof(UniversalReferenceData));
			var writer = new StreamWriter(outputFilePath);

			var data = new UniversalReferenceData
			{
				DataSource = Settings.Default.DataSource,
				PublicationTime = DateTime.Today,
				UpdateType = Settings.Default.UpdateType,
				Schemas = Settings.Default.Schema
			};

			reflocomap.ForEach(x => data.RefCusCodeLists.AddRange(x.ToCodeListLocoMap()));

			serializer.Serialize(writer, data);
			writer.Close();
			ServicesProvider.Logger.Info("Successfully serialized the RefLocoMap to xml file.");
		}
		static string GetOutputFilePath()
		{
			var outputPath = Settings.Default.OutputPath;

			if (string.IsNullOrEmpty(outputPath))
			{
				Console.WriteLine("AppSetting OutputFolder needs to be set in config file");
				outputPath = @"..\UxmlFiles";
			}
			if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}
			var filePath = Path.Combine(outputPath, "RefLocoMap.xml");

			return filePath;
		}
	}
}
