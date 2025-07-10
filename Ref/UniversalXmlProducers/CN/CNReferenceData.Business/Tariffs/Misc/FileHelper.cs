using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class FileHelper
	{
		public static string GetFullOutputFileName(this ISetting setting, string fileName)
		{
			var outpath = setting.OutputFileFolderPath;
			return GetFullFileName(outpath, fileName, true);
		}

		public static string GetFullExcelSourcePath(this ISetting setting)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var result = Path.Combine(binPath, setting.ExcelSourceFileFolder);
			return result;
		}

		public static string GetFullLogFileName(this ISetting setting)
		{
			return GetFullFileName(setting.OutputFolderForLog, $"CNTariffProducer_{GlobalOption.Instance.Now:yyyyMMddHHmmss}.log");
		}

		public static string GetFullResponseFileName(this ISetting setting, string fileName)
		{
			return GetFullFileName(setting.OutputFolderForResponse, fileName, true);
		}

		static string GetFullFileName(string relatedFolderPath, string fileName, bool createDirectoryIfNoExists = true)
		{
			var result = string.Empty;

			if (!string.IsNullOrEmpty(relatedFolderPath))
			{
				var folderPath = Path.Combine(ExecutingPath, relatedFolderPath);
				if (!Directory.Exists(folderPath) && createDirectoryIfNoExists)
				{
					Directory.CreateDirectory(folderPath);
				}

				result = Path.Combine(new DirectoryInfo(folderPath).FullName, fileName);
			}
			return result;
		}

		public static string ExecutingPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
