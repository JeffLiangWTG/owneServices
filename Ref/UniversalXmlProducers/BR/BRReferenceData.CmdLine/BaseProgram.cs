using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public interface IProgram
	{
		void Run();
	}

	public abstract class BaseProgram : IProgram
	{
		public void Run()
		{
			if (!Directory.Exists(OutputFolderPath))
			{
				Directory.CreateDirectory(OutputFolderPath);
			}

			RunCore();

			ParserErrorCollector.Instance.ReportErrors();
		}

		string OutputFolderPath => outputFolderPath ?? (outputFolderPath = GetOutputFolderPath());
		string outputFolderPath;

		static string GetOutputFolderPath()
		{
			var outputFolder = ConfigurationProvider.Configuration.GetSection("OutputFolder").Value;
			if (string.IsNullOrEmpty(outputFolder))
			{
				throw new InvalidOperationException("AppSetting OutputFolder need to be set in config file");
			}

			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFolder);
		}

		protected abstract void RunCore();

		protected string GetOutputFilePath(string outputFileName)
		{
			var filePath = Path.Combine(GetOutputFolderPath(), outputFileName);
			if (!outputFilePaths.Contains(filePath))
			{
				outputFilePaths.Add(filePath);
			}
			return filePath;
		}

		readonly List<string> outputFilePaths = new List<string>();

		public void DeleteOutputFiles()
		{
			foreach (var filePath in outputFilePaths)
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}
	}
}
