using System;
using System.IO;
using CargoWise.RefDbRepo.CNReferenceData.Business;

namespace CargoWise.RefDbRepo.CNReferenceData.CmdLine
{
	class CIQOfficeCodeProgram
	{
		public static void Run(string[] args)
		{
			var outputFolder = GlobalOption.Instance.Setting.OutputFileFolderPath;
			var inputFile = GlobalOption.Instance.Setting.CIQOfficeCodeInputFile;

			if (string.IsNullOrEmpty(outputFolder))
			{
				GlobalOption.Instance.Log.Error("AppSetting OutputFolder need to be set in config file");
				return;
			}

			if (string.IsNullOrEmpty(inputFile))
			{
				GlobalOption.Instance.Log.Error("AppSetting CIQOfficeCodeInputFile need to be set in config file");
				return;
			}

			var outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFolder);
			var inputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFile);

			if (args.Length < 2)
			{
				GlobalOption.Instance.Log.Info("Parameter 1 - Code Type");
				GlobalOption.Instance.Log.Info("Parameter 2 - Publication Time");
				GlobalOption.Instance.Log.Info("Parameter 3 - Input File Path (Optional)");
				GlobalOption.Instance.Log.Info("Parameter 4 - Output File Path (optional)");
			}
			else
			{
				string codeType = args[0];

				string publicationTimeString = args[1];
				if (!DateTime.TryParse(publicationTimeString, out var publicationTime))
				{
					GlobalOption.Instance.Log.Info("Parameter 2 - Publication Time: Please enter the correct time format. Ex: yyyy-MM-ddTHH:mm:ss");
					return;
				}

				string inputFileName = args.Length > 2 ? args[2] : inputFilePath;

				string outputFileName = args.Length > 3 ? args[3] : Path.Combine(outputFolderPath, $"RefCusCodeList_CN_{codeType}.xml");
				var dataSource = GlobalOption.Instance.Setting.GetDataSource(codeType);
				if (string.IsNullOrEmpty(dataSource))
				{
					dataSource = Path.GetFileNameWithoutExtension(inputFileName);
				}

				CIQOfficeCodeParser.ExportToXMLFile(inputFileName, outputFileName, dataSource, codeType, publicationTime);
				GlobalOption.Instance.Log.Info($"RefCusCodeList records generated to {outputFileName}");
			}
		}
	}
}
