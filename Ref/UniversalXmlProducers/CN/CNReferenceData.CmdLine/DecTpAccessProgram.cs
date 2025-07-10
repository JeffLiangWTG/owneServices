using System;
using System.IO;
using CargoWise.RefDbRepo.CNReferenceData.Business;

namespace CargoWise.RefDbRepo.CNReferenceData.CmdLine
{
	internal class DecTpAccessProgram
	{
		internal static void Run(string[] args)
		{
			using (GlobalOption.Instance.CreateDisposableLog("CN DecTpAccess Program"))
			{
				string inputFileName = args.Length > 1 ? args[1] : GetInputFilePath();
				if (string.IsNullOrEmpty(inputFileName))
				{
					GlobalOption.Instance.Log.Error("Input the first parameter or set DecTpAccessInputFile in App.config.");
					return;
				}

				// {\"StartRow\":4,\"ColumnIndexForCustomsCode\":9,\"ColumnIndexForType\":0,\"ColumnIndexForCustomsDistrict\":2,\"ColumnIndexForName\":4,\"FixedType\":\"DSSGN\"}
				string jsonForLoadSetting = args.Length > 2 ? args[2] : null;

				string outputFileName = args.Length > 3 ? args[3] : GetOutputFilePath();
				if (string.IsNullOrEmpty(outputFileName))
				{
					GlobalOption.Instance.Log.Error("Input the first parameter or set OutputFolder in App.config.");
					return;
				}

				var dataSource = GlobalOption.Instance.Setting.GetDataSource(Constants.ProgramFunctions.DecTpAccess);
				if (string.IsNullOrEmpty(dataSource))
				{
					dataSource = Path.GetFileNameWithoutExtension(inputFileName);
				}

				DateTime publicationTime = DateTime.Now;

				using (var fileStream = new FileStream(inputFileName, FileMode.Open))
				{
					new DecTpAccessParser(jsonForLoadSetting).ExportToXMLFile(fileStream, outputFileName, dataSource, publicationTime);
					GlobalOption.Instance.Log.Info($"RefCusCodeList records generated to {outputFileName}");
				}
			}
		}

		static string GetOutputFilePath()
		{
			var result = string.Empty;
			var outputFolder = GlobalOption.Instance.Setting.OutputFileFolderPath;
			if (!string.IsNullOrEmpty(outputFolder))
			{
				var outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFolder);
				result = Path.Combine(outputFolderPath, $"RefCusCodeList_CN_{Constants.ProgramFunctions.DecTpAccess}.xml");
			}
			return result;
		}

		static string GetInputFilePath()
		{
			var result = string.Empty;
			var inputFile = GlobalOption.Instance.Setting.DecTpAccessInputFile;
			if (!string.IsNullOrEmpty(inputFile))
			{
				result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFile);
			}
			return result;
		}
	}
}
