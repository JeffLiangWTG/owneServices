using System;
using System.Diagnostics;
using CargoWise.RefDbRepo.Tools.Common;

namespace CargoWise.RefDbRepo.EdmxGen
{
	class ScaffoldRunner
	{
		public static void GenerateDbContextAndEntities(string targetDirectory, string connectionString, string dbContextName, string namespaceName)
		{
			Console.WriteLine($"Start to generate DbContext and Entities for {targetDirectory}");

			var sqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";
			ProcessStartInfo startInfo = new ProcessStartInfo()
			{
				FileName = "dotnet",
				Arguments = $"ef dbcontext scaffold \"{connectionString}\" {sqlServerProvider} --use-database-names --data-annotations --context {dbContextName} --no-onconfiguring --context-dir DbContext --output-dir Models --namespace {namespaceName} --force",
				WorkingDirectory = targetDirectory,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			WinProcessor.RunProcess(startInfo);

			Console.WriteLine($"DbContext and Entities has been generated for {targetDirectory}");
		}
	}
}
