using System.Diagnostics;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Tools.Common;

namespace CargoWise.RefDbRepo.T4Runner
{
	public class TTFileConfig : ITTFileConfig
	{
		const string TextTransformPath = @"Common7\IDE\TextTransform.exe";

		public string DirectoryPath { get; set; }
		public string NamespaceName { get; set; }
		public string FileName { get; set; }

		public void CallProcessor(string namespaceName, string ttFile)
		{
			Argument.NotNullOrEmpty(namespaceName, nameof(namespaceName));
			Argument.NotNullOrEmpty(ttFile, nameof(ttFile));
			var vsPath = Application.VisualStudioPath;
			var textTransformPath = Path.Combine(vsPath, TextTransformPath);

			var procStartInfo = new ProcessStartInfo(textTransformPath, $@"-a !!NamespaceHint!{namespaceName} -I ""{Application.T4Include}"" -I ""{Application.DatasetTTIncludePath}"" -a xmlwriter!ttgen!inputFile!""{Application.DefaultPathEDMXStaging}"" -a contracts!ttgen!inputFile!""{Application.DefaultPathEDMXSafe}"" {ttFile}")
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden
			};

			WinProcessor.RunProcess(procStartInfo);
		}
	}
}
