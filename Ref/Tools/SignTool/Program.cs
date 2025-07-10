using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using WTG.DevTools.Common;

namespace CargoWise.RefDbRepo.SignTool
{
	class Program
	{
		static void Main(string[] args)
		{
			var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), args[0]);
			foreach (var filePath in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
			{
				var file = new FileInfo(filePath);
				if (file.Name.Contains("CargoWise.RefDbRepo", StringComparison.OrdinalIgnoreCase)
					&& (file.Extension.ToUpper(CultureInfo.InvariantCulture) == ".DLL" || file.Extension.ToUpper(CultureInfo.InvariantCulture) == ".EXE")
					&& !file.Name.Contains("CargoWise.RefDbRepo.SignTool", StringComparison.OrdinalIgnoreCase)
					&& !file.Name.Contains("CargoWise.RefDbRepo.TestingApplication", StringComparison.OrdinalIgnoreCase))
				{
					var result = new AuthenticodeSigning(CertificateName).RunWithRetryAsync(filePath).GetAwaiter().GetResult();
					Console.WriteLine($"Sign {filePath} : {result.Result} and {result.ErrorMessage}");
					if (result.Result is SignToolResultType.Failure)
					{
						throw new InvalidOperationException("The signing process failed!");
					}
				}
			}
		}

		const string CertificateName = "WTG Internal Code Signing";
	}
}
