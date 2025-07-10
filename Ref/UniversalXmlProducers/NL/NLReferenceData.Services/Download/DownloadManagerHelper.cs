using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public static class DownloadManagerHelper
	{
		public static void PrepareEnvironment(string workingFolder)
		{
			try
			{
				if (!Directory.Exists(workingFolder))
				{
					Directory.CreateDirectory(workingFolder);
				}
			}
			catch
			{
				throw new ProcessingException($"Could not create working folder '{workingFolder}'");
			}
		}

		public static void RemoveUnexpectedFiles(string expectedExtensions, List<string> contentFiles)
		{
			foreach (var file in contentFiles.Where(x => !expectedExtensions.Contains(Path.GetExtension(Path.GetFileName(x)))))
			{
				File.Delete(file);
			}

			List<string> deletedItems = contentFiles.Where(x => !expectedExtensions.Contains(Path.GetExtension(Path.GetFileName(x)))).ToList();
			foreach (var deletedFile in deletedItems)
			{
				contentFiles.Remove(deletedFile);
			}
		}

		public static void RemoveUnexpectedFiles(List<string> expectedFiles, List<string> contentFiles)
		{
			foreach (var file in contentFiles.Where(x => !expectedFiles.Contains(Path.GetFileName(x))))
			{
				File.Delete(file);
			}
		}

		public static void RemoveExpectedFiles(List<string> contentFiles)
		{
			List<string> files = contentFiles.ToList();

			foreach (var file in files)
			{
				File.Delete(file);
				if (!File.Exists(file))
				{
					contentFiles.Remove(file);
				}
			}
		}
	}
}
